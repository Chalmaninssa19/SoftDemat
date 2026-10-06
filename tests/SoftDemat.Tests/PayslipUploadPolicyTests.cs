using System.Text;
using FluentAssertions;
using SoftDemat.Domain.Exceptions;
using SoftDemat.Domain.Rules;

namespace SoftDemat.Tests;

public class PayslipUploadPolicyTests
{
    private static readonly byte[] PdfHeader = Encoding.ASCII.GetBytes("%PDF-");

    [Fact]
    public void TryAccept_DirectBulletin_AcceptsSafeName()
    {
        // Arrange
        var names = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        // Act
        var accepted = PayslipUploadPolicy.TryAccept("Bulletins/0042_20260331.pdf", 128, PdfHeader, names, out var safeName, out var error);

        // Assert
        accepted.Should().BeTrue();
        safeName.Should().Be("0042_20260331.pdf");
        error.Should().BeEmpty();
    }

    [Theory]
    [InlineData("0042_20260331.pdf", "Sélectionnez un dossier, pas un fichier isolé.")]
    [InlineData("Bulletins/mars/0042_20260331.pdf", "Seul le dossier sélectionné est lu, pas ses sous-dossiers.")]
    [InlineData("Bulletins/../../0042_20260331.pdf", "Chemin de fichier refusé.")]
    [InlineData("C:/Windows/0042_20260331.pdf", "Sélectionnez un dossier, pas un fichier isolé.")]
    [InlineData("Bulletins/note.txt", "Seuls les fichiers PDF du dossier sont acceptés.")]
    [InlineData("Bulletins/0042_20260331.pdf\0.exe", "Sélectionnez un dossier, pas un fichier isolé.")]
    public void TryAccept_UntrustedPath_Rejects(string browserPath, string expected)
    {
        // Arrange
        var names = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        // Act
        var accepted = PayslipUploadPolicy.TryAccept(browserPath, 128, PdfHeader, names, out _, out var error);

        // Assert
        accepted.Should().BeFalse();
        error.Should().Be(expected);
        names.Should().BeEmpty();
    }

    [Fact]
    public void TryAccept_OversizedFile_Rejects()
    {
        // Arrange
        var names = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        // Act
        var accepted = PayslipUploadPolicy.TryAccept(
            "Bulletins/0042_20260331.pdf",
            PayslipUploadPolicy.MaxFileBytes + 1,
            PdfHeader,
            names,
            out _,
            out var error);

        // Assert
        accepted.Should().BeFalse();
        error.Should().Contain("10 Mo");
    }

    [Fact]
    public void TryAccept_WrongHeaderOrName_Rejects()
    {
        // Arrange
        var names = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var text = Encoding.ASCII.GetBytes("hello");

        // Act
        var badHeader = PayslipUploadPolicy.TryAccept("Bulletins/0042_20260331.pdf", 5, text, names, out _, out var headerError);
        var badName = PayslipUploadPolicy.TryAccept("Bulletins/bulletin.pdf", 8, PdfHeader, names, out _, out var nameError);

        // Assert
        badHeader.Should().BeFalse();
        headerError.Should().Be("Le fichier n'est pas un PDF.");
        badName.Should().BeFalse();
        nameError.Should().Contain("matricule_aaaammjj.pdf");
    }

    [Fact]
    public void TryAccept_DuplicateName_RejectsSecondFile()
    {
        // Arrange
        var names = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        PayslipUploadPolicy.TryAccept("Bulletins/0042_20260331.pdf", 8, PdfHeader, names, out _, out _);

        // Act
        var accepted = PayslipUploadPolicy.TryAccept("Bulletins/0042_20260331.PDF", 8, PdfHeader, names, out _, out var error);

        // Assert
        accepted.Should().BeFalse();
        error.Should().Be("Un bulletin porte déjà ce nom.");
    }

    [Fact]
    public void EnsureOwned_OtherUserOrTraversal_Rejects()
    {
        // Arrange
        var owned = "batches/7/0123456789abcdef0123456789abcdef";

        // Act
        var own = () => PayslipUploadPolicy.EnsureOwned(owned, 7);
        var other = () => PayslipUploadPolicy.EnsureOwned(owned, 8);
        var empty = () => PayslipUploadPolicy.EnsureOwned("  ", 7);
        var escape = () => PayslipUploadPolicy.EnsureOwned(owned + "/../../windows", 7);

        // Assert
        own.Should().NotThrow();
        other.Should().Throw<ForbiddenException>();
        empty.Should().Throw<DomainException>();
        escape.Should().Throw<ForbiddenException>();
    }
}
