using Microsoft.Extensions.Logging;
using SoftDemat.Application.Common;
using SoftDemat.Application.DTOs;
using SoftDemat.Application.Services.Interfaces;
using SoftDemat.Domain.Entities;
using SoftDemat.Domain.Exceptions;
using SoftDemat.Domain.Interfaces;
using SoftDemat.Domain.Rules;

namespace SoftDemat.Infrastructure.Services;

public sealed class DispatchService : IDispatchService
{
    private readonly IMailTemplateRepository _templates;
    private readonly IGeneralParameterRepository _parameters;
    private readonly IEmployeeRepository _employees;
    private readonly IDispatchRepository _dispatches;
    private readonly IPayslipDirectory _directory;
    private readonly IPayslipArchiver _archiver;
    private readonly IPayslipMailer _mailer;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<DispatchService> _logger;

    public DispatchService(
        IMailTemplateRepository templates,
        IGeneralParameterRepository parameters,
        IEmployeeRepository employees,
        IDispatchRepository dispatches,
        IPayslipDirectory directory,
        IPayslipArchiver archiver,
        IPayslipMailer mailer,
        IUnitOfWork unitOfWork,
        ILogger<DispatchService> logger)
    {
        _templates = templates;
        _parameters = parameters;
        _employees = employees;
        _dispatches = dispatches;
        _directory = directory;
        _archiver = archiver;
        _mailer = mailer;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<DispatchResultResponse> SendAsync(
        DispatchRequest request,
        int userId,
        bool onSenderMachine,
        CancellationToken cancellationToken = default)
    {
        PayslipUploadPolicy.EnsureOwned(request.RelativeFolder, userId);
        var template = await _templates.GetByIdAsync(request.MailTemplateId, cancellationToken)
            ?? throw new NotFoundException("Modèle de mail introuvable.");
        var parameter = await _parameters.GetAsync(cancellationToken)
            ?? throw new DomainException("Paramètres généraux introuvables.");
        if (string.IsNullOrWhiteSpace(parameter.ArchiveFolder))
            throw new DomainException("Le dossier d'archivage n'est pas configuré.");

        var selected = request.FileNames.ToHashSet(StringComparer.OrdinalIgnoreCase);
        var files = _directory.ListPdfFileNames(request.RelativeFolder)
            .Where(name => selected.Contains(name))
            .Select(name => PayslipFileNameParser.TryParse(name, out var parsed) ? parsed : (ParsedPayslipFile?)null)
            .Where(file => file is not null)
            .Select(file => file!.Value)
            .ToList();
        var employees = await _employees.GetCurrentAsync(cancellationToken);
        var lookup = employees.ToDictionary(employee => employee.Matricule.Trim(), StringComparer.OrdinalIgnoreCase);
        var results = new List<DispatchItemResponse>();
        foreach (var file in files)
        {
            cancellationToken.ThrowIfCancellationRequested();
            results.Add(await SendOneAsync(request.RelativeFolder, file, lookup, template, parameter, onSenderMachine, cancellationToken));
        }

        var missing = selected.Count - files.Count;
        return new DispatchResultResponse(results.Count(item => item.Sent), results.Count(item => !item.Sent) + missing, results);
    }

    public async Task<PaginatedResult<DispatchHistoryResponse>> SearchAsync(
        DispatchHistoryQuery query,
        CancellationToken cancellationToken = default)
    {
        var from = query.From <= query.To ? query.From : query.To;
        var to = query.From <= query.To ? query.To : query.From;
        from = from.Date;
        to = to.Date.AddDays(1).AddTicks(-1);
        var employees = await _employees.GetCurrentAsync(cancellationToken);
        var numbers = FilterNumbers(employees, query.EstablishmentCode, query.EmployeeMatricule);
        var page = PageRequest.Normalize(query.Page, query.Size);
        var result = await _dispatches.SearchAsync(
            from,
            to,
            query.Sent,
            numbers,
            null,
            query.Search,
            PageRequest.IsDescending(query.SortDirection) || string.IsNullOrWhiteSpace(query.SortBy),
            page.Skip,
            page.Size,
            cancellationToken);
        var emails = employees.GroupBy(employee => employee.Matricule.Trim(), StringComparer.OrdinalIgnoreCase)
            .ToDictionary(group => group.Key, group => group.First().Email, StringComparer.OrdinalIgnoreCase);
        var items = result.Items.Select(dispatch => ToHistory(dispatch, emails)).ToList();
        return new PaginatedResult<DispatchHistoryResponse>(items, result.TotalCount, page.Page, page.Size, PageRequest.PageCount(result.TotalCount, page.Size));
    }

    private async Task<DispatchItemResponse> SendOneAsync(
        string relativeFolder,
        ParsedPayslipFile file,
        IReadOnlyDictionary<string, CurrentEmployee> lookup,
        MailTemplate template,
        GeneralParameter parameter,
        bool onSenderMachine,
        CancellationToken cancellationToken)
    {
        lookup.TryGetValue(file.Matricule.Trim(), out var employee);
        var fullName = (employee?.FullName ?? string.Empty).Replace("'", " ");
        if (string.IsNullOrWhiteSpace(employee?.Email))
            return await RecordAsync(file, fullName, false, "E-mail du salarié absent pour ce matricule.", cancellationToken);

        try
        {
            var from = await DeliverAsync(relativeFolder, file, employee!, template, parameter, fullName, onSenderMachine, cancellationToken);
            _logger.LogInformation("Bulletin {Matricule} envoyé", file.Matricule.Trim());
            return await RecordAsync(file, fullName, true, $"Envoyé de {from} vers {employee!.Email.Trim()}.", cancellationToken);
        }
        catch (DomainException exception)
        {
            _logger.LogWarning("Bulletin {Matricule} non envoyé : {Reason}", file.Matricule.Trim(), exception.Message);
            return await RecordAsync(file, fullName, false, exception.Message, cancellationToken);
        }
        catch (Exception exception)
        {
            _logger.LogWarning(exception, "Bulletin {Matricule} non envoyé", file.Matricule.Trim());
            return await RecordAsync(file, fullName, false, "Non envoyé.", cancellationToken);
        }
    }

    private async Task<string> DeliverAsync(
        string relativeFolder,
        ParsedPayslipFile file,
        CurrentEmployee employee,
        MailTemplate template,
        GeneralParameter parameter,
        string fullName,
        bool onSenderMachine,
        CancellationToken cancellationToken)
    {
        var source = _directory.ResolveFile(relativeFolder, file.FileName);
        var subject = MailContentComposer.Apply(template.MailObject, file.PayDate, employee.FirstName, fullName, file.Matricule);
        var body = MailContentComposer.Apply(template.MailContent, file.PayDate, employee.FirstName, fullName, file.Matricule);
        var attachmentName = ArchivePathBuilder.BuildFileName(template.MailCode, file.PayDate, file.Matricule, employee.FirstName);
        var from = await _mailer.SendAsync(
            new OutgoingMail(employee.Email.Trim(), parameter.Cc, subject, body, source, attachmentName),
            onSenderMachine,
            cancellationToken);
        _archiver.Archive(source, parameter.ArchiveFolder, file.PayDate, employee.EstablishmentName, template.MailCode, file.Matricule, employee.FirstName);
        return from;
    }

    private async Task<DispatchItemResponse> RecordAsync(
        ParsedPayslipFile file,
        string fullName,
        bool sent,
        string detail,
        CancellationToken cancellationToken)
    {
        await _dispatches.AddAsync(new PayslipDispatch
        {
            EmployeeNumber = file.Matricule,
            Name = fullName,
            SentAt = DateTime.Now,
            FilePath = file.FileName,
            FileName = file.FileName,
            PayDate = file.PayDate,
            Sent = sent
        }, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return new DispatchItemResponse(file.Matricule.Trim(), file.FileName, sent, detail);
    }

    private static IReadOnlyCollection<string>? FilterNumbers(
        IReadOnlyList<CurrentEmployee> employees,
        string? establishmentCode,
        string? employeeMatricule)
    {
        if (string.IsNullOrWhiteSpace(establishmentCode) && string.IsNullOrWhiteSpace(employeeMatricule))
            return null;

        var filtered = employees.AsEnumerable();
        if (!string.IsNullOrWhiteSpace(establishmentCode))
            filtered = filtered.Where(employee => employee.EstablishmentCode.Trim().Equals(establishmentCode.Trim(), StringComparison.OrdinalIgnoreCase));
        if (!string.IsNullOrWhiteSpace(employeeMatricule))
            filtered = filtered.Where(employee => employee.Matricule.Trim().Equals(employeeMatricule.Trim(), StringComparison.OrdinalIgnoreCase));

        return filtered.SelectMany(employee => new[] { employee.Matricule.Trim(), employee.Matricule.Trim().PadLeft(4) }).Distinct().ToList();
    }

    private static DispatchHistoryResponse ToHistory(PayslipDispatch dispatch, IReadOnlyDictionary<string, string> emails)
    {
        emails.TryGetValue(dispatch.EmployeeNumber.Trim(), out var email);
        return new DispatchHistoryResponse(
            dispatch.Id,
            dispatch.EmployeeNumber.Trim(),
            dispatch.Name,
            email ?? string.Empty,
            dispatch.SentAt,
            dispatch.PayDate,
            dispatch.FileName,
            dispatch.Sent);
    }
}
