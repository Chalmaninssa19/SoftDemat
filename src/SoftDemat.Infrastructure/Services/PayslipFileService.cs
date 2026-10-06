using SoftDemat.Application.Common;
using SoftDemat.Application.DTOs;
using SoftDemat.Application.Services.Interfaces;
using SoftDemat.Domain.Entities;
using SoftDemat.Domain.Interfaces;
using SoftDemat.Domain.Rules;

namespace SoftDemat.Infrastructure.Services;

public sealed class PayslipFileService : IPayslipFileService
{
    private readonly IPayslipDirectory _directory;
    private readonly IEmployeeRepository _employees;
    private readonly IDispatchRepository _dispatches;

    public PayslipFileService(IPayslipDirectory directory, IEmployeeRepository employees, IDispatchRepository dispatches)
    {
        _directory = directory;
        _employees = employees;
        _dispatches = dispatches;
    }

    public Task<PayslipFolderResponse> ListFoldersAsync(string? relativeFolder, int userId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(relativeFolder))
            return Task.FromResult(new PayslipFolderResponse(string.Empty, []));

        PayslipUploadPolicy.EnsureOwned(relativeFolder, userId);
        var children = _directory.ListChildDirectories(relativeFolder);
        return Task.FromResult(new PayslipFolderResponse(relativeFolder, children));
    }

    public async Task<PaginatedResult<PayslipFileResponse>> ListAsync(
        PayslipFileQuery query,
        int userId,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(query.RelativeFolder))
            return Empty(query);

        PayslipUploadPolicy.EnsureOwned(query.RelativeFolder, userId);
        var parsed = _directory.ListPdfFileNames(query.RelativeFolder)
            .Select(name => PayslipFileNameParser.TryParse(name, out var parsedFile) ? parsedFile : (ParsedPayslipFile?)null)
            .Where(file => file is not null)
            .Select(file => file!.Value)
            .ToList();
        if (parsed.Count == 0)
            return Empty(query);

        var employees = await _employees.GetCurrentAsync(cancellationToken);
        var lookup = employees.ToDictionary(employee => employee.Matricule.Trim(), StringComparer.OrdinalIgnoreCase);
        var sent = await LoadSentFlagsAsync(parsed, cancellationToken);
        var rows = parsed
            .Select(file => ToRow(file, lookup, sent))
            .Where(row => PayslipFilter.Matches(row.Matricule, row.FullName, row.EstablishmentCode, query.EstablishmentCode, query.EmployeeMatricule, query.Matricule, query.Name))
            .Where(row => MatchesSearch(row, query.Search))
            .ToList();
        rows.Sort((left, right) => Compare(left, right, query.SortBy, PageRequest.IsDescending(query.SortDirection)));
        return Page(rows, query);
    }

    private async Task<Dictionary<string, bool>> LoadSentFlagsAsync(
        IReadOnlyList<ParsedPayslipFile> files,
        CancellationToken cancellationToken)
    {
        var from = files.Min(file => file.PayDate).Date;
        var to = files.Max(file => file.PayDate).Date.AddDays(1).AddTicks(-1);
        var history = await _dispatches.GetByPayPeriodAsync(files.Select(file => file.Matricule).Distinct().ToList(), from, to, cancellationToken);
        return history
            .GroupBy(dispatch => Key(dispatch.EmployeeNumber, dispatch.PayDate))
            .ToDictionary(group => group.Key, group => group.OrderByDescending(dispatch => dispatch.SentAt).First().Sent);
    }

    private static PayslipFileResponse ToRow(
        ParsedPayslipFile file,
        IReadOnlyDictionary<string, CurrentEmployee> lookup,
        IReadOnlyDictionary<string, bool> sent)
    {
        lookup.TryGetValue(file.Matricule.Trim(), out var employee);
        var alreadySent = sent.TryGetValue(Key(file.Matricule, file.PayDate), out var value) && value;
        return new PayslipFileResponse(
            file.FileName,
            file.Matricule,
            employee?.LastName ?? string.Empty,
            employee?.FirstName ?? string.Empty,
            employee?.FullName ?? string.Empty,
            employee?.Email ?? string.Empty,
            employee?.EstablishmentCode ?? string.Empty,
            employee?.EstablishmentName ?? string.Empty,
            file.PayDate,
            alreadySent,
            SelectedByDefault: !alreadySent);
    }

    private static bool MatchesSearch(PayslipFileResponse row, string? search)
    {
        if (string.IsNullOrWhiteSpace(search))
            return true;

        var term = search.Trim();
        return row.Matricule.Contains(term, StringComparison.OrdinalIgnoreCase)
            || row.FullName.Contains(term, StringComparison.OrdinalIgnoreCase)
            || row.FileName.Contains(term, StringComparison.OrdinalIgnoreCase);
    }

    private static int Compare(PayslipFileResponse left, PayslipFileResponse right, string? sortBy, bool descending)
    {
        var result = sortBy?.ToLowerInvariant() switch
        {
            "name" => string.Compare(left.FullName, right.FullName, StringComparison.OrdinalIgnoreCase),
            "paydate" => left.PayDate.CompareTo(right.PayDate),
            "email" => string.Compare(left.Email, right.Email, StringComparison.OrdinalIgnoreCase),
            _ => string.Compare(left.Matricule, right.Matricule, StringComparison.OrdinalIgnoreCase)
        };
        return descending ? -result : result;
    }

    private static PaginatedResult<PayslipFileResponse> Page(IReadOnlyList<PayslipFileResponse> rows, PayslipFileQuery query)
    {
        var page = PageRequest.Normalize(query.Page, query.Size);
        var items = rows.Skip(page.Skip).Take(page.Size).ToList();
        return new PaginatedResult<PayslipFileResponse>(items, rows.Count, page.Page, page.Size, PageRequest.PageCount(rows.Count, page.Size));
    }

    private static PaginatedResult<PayslipFileResponse> Empty(PayslipFileQuery query)
    {
        var page = PageRequest.Normalize(query.Page, query.Size);
        return new PaginatedResult<PayslipFileResponse>([], 0, page.Page, page.Size, 0);
    }

    private static string Key(string matricule, DateTime payDate) => $"{matricule.Trim()}|{payDate:yyyyMMdd}";
}
