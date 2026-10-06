using System.Text;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using SoftDemat.Application.DTOs;
using SoftDemat.Domain.Exceptions;
using SoftDemat.Domain.Rules;
using SoftDemat.Infrastructure.Options;
using SoftDemat.Infrastructure.Services;
using SoftDemat.Infrastructure.Storage;

namespace SoftDemat.Tests;

public sealed class PayslipUploadFlowTests : IDisposable
{
    private readonly string _root = Path.Combine(Path.GetTempPath(), "softdemat-payslips", Guid.NewGuid().ToString("N"));
    private readonly PayslipUploadService _service;
    private readonly PayslipDirectory _directory;
    private readonly PayslipUploadStore _store;

    public PayslipUploadFlowTests()
    {
        var location = new PayslipStorageLocation(Options.Create(new PayslipStorageOptions { RootPath = _root }));
        _store = new PayslipUploadStore(location);
        _directory = new PayslipDirectory(location, _store);
        _service = new PayslipUploadService(_store, NullLogger<PayslipUploadService>.Instance);
    }

    [Fact]
    public async Task Import_ValidPdf_IsListedAndResolvedInsideBatch()
    {
        // Arrange
        var content = Encoding.ASCII.GetBytes("%PDF-1.4 bulletin");
        var files = new[] { Item("Paie Mars/0042_20260331.pdf", content) };

        // Act
        var result = await _service.ImportAsync(7, files, CancellationToken.None);
        var listed = _directory.ListPdfFileNames(result.RelativeFolder);
        var resolved = _directory.ResolveFile(result.RelativeFolder, listed[0]);

        // Assert
        result.FolderName.Should().Be("Paie Mars");
        result.RelativeFolder.Should().StartWith("batches/7/");
        result.RelativeFolder.Should().NotContain("Paie Mars");
        listed.Should().Equal("0042_20260331.pdf");
        resolved.Should().StartWith(_root);
        File.ReadAllText(resolved).Should().Be("%PDF-1.4 bulletin");
        PayslipUploadPolicy.IsOwned(result.RelativeFolder, 7).Should().BeTrue();
    }

    [Fact]
    public async Task Import_InvalidOversizedAndMalicious_RejectsWithoutLeavingFiles()
    {
        // Arrange
        var pdf = Encoding.ASCII.GetBytes("%PDF-1.4");
        var files = new List<PayslipUploadItem>
        {
            Item("Bulletins/0042_20260331.pdf", pdf),
            Item("Bulletins/0042_20260331.PDF", pdf),
            Item("Bulletins/mars/0010_20260331.pdf", pdf),
            Item("Bulletins/../../secret.pdf", pdf),
            Item("Bulletins/note.txt", pdf),
            Item("Bulletins/bulletin.pdf", pdf),
            new("Bulletins/0099_20260331.pdf", new MemoryStream(Encoding.ASCII.GetBytes("hello")), 5),
            new("Bulletins/0088_20260331.pdf", new MemoryStream(pdf), PayslipUploadPolicy.MaxFileBytes + 1),
        };

        // Act
        var result = await _service.ImportAsync(4, files, CancellationToken.None);

        // Assert
        result.Accepted.Should().Equal("0042_20260331.pdf");
        result.Rejected.Select(item => item.Reason).Should().Contain(
            ["Un bulletin porte déjà ce nom.", "Chemin de fichier refusé.", "Le fichier n'est pas un PDF."]);
        result.Rejected.Should().Contain(item => item.Reason.Contains("10 Mo"));
        OutsideFiles("secret.pdf").Should().BeEmpty();
    }

    [Fact]
    public async Task Import_BatchTooLarge_ThrowsBeforeWriting()
    {
        // Arrange
        var pdf = Encoding.ASCII.GetBytes("%PDF-1.4");
        var files = new[]
        {
            new PayslipUploadItem("Bulletins/0042_20260331.pdf", new MemoryStream(pdf), PayslipUploadPolicy.MaxBatchBytes),
            new PayslipUploadItem("Bulletins/0043_20260331.pdf", new MemoryStream(pdf), 1),
        };

        // Act
        var act = async () => await _service.ImportAsync(4, files, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<DomainException>().WithMessage("*200 Mo*");
        Directory.Exists(Path.Combine(_root, "batches")).Should().BeFalse();
    }

    [Fact]
    public async Task Save_MaliciousName_DoesNotEscapeBatch()
    {
        // Arrange
        var header = Encoding.ASCII.GetBytes("%PDF-");
        var write = new PayslipUploadWrite(@"..\..\evil.pdf", header, Stream.Null);

        // Act
        var act = async () => await _store.SaveAsync(4, [write], CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<DomainException>();
        OutsideFiles("evil.pdf").Should().BeEmpty();
    }

    [Fact]
    public void ResolveFile_ParentSegments_StayInsideBatch()
    {
        // Arrange
        Directory.CreateDirectory(Path.Combine(_root, "batches", "4", "0123456789abcdef0123456789abcdef"));
        var relative = "batches/4/0123456789abcdef0123456789abcdef";

        // Act
        var resolved = _directory.ResolveFile(relative, @"..\..\outside.pdf");

        // Assert
        resolved.Should().StartWith(Path.Combine(_root, "batches", "4"));
        Path.GetFileName(resolved).Should().Be("outside.pdf");
    }

    [Fact]
    public void List_ExpiredBatch_IsPurged()
    {
        // Arrange
        var batch = Path.Combine(_root, "batches", "4", "0123456789abcdef0123456789abcdef");
        Directory.CreateDirectory(batch);
        File.WriteAllText(Path.Combine(batch, "0042_20260331.pdf"), "%PDF-");
        Directory.SetCreationTimeUtc(batch, DateTime.UtcNow.AddHours(-25));

        // Act
        var act = () => _directory.ListPdfFileNames("batches/4/0123456789abcdef0123456789abcdef");

        // Assert
        act.Should().Throw<DomainException>().WithMessage("*expiré*");
        Directory.Exists(batch).Should().BeFalse();
    }

    public void Dispose()
    {
        if (Directory.Exists(_root))
            Directory.Delete(_root, true);
    }

    private static PayslipUploadItem Item(string browserPath, byte[] content)
        => new(browserPath, new MemoryStream(content), content.Length);

    private string[] OutsideFiles(string name)
        => Directory.Exists(_root) ? Directory.GetFiles(_root, name, SearchOption.AllDirectories) : [];
}
