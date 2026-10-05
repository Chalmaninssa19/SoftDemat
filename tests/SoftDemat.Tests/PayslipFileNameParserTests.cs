using FluentAssertions;
using SoftDemat.Domain.Rules;

namespace SoftDemat.Tests;

public class PayslipFileNameParserTests
{
    [Fact]
    public void TryParse_FourDigitMatricule_KeepsMatricule()
    {
        // Arrange
        var fileName = "0042_20260331.pdf";

        // Act
        var parsed = PayslipFileNameParser.TryParse(fileName, out var result);

        // Assert
        parsed.Should().BeTrue();
        result.Matricule.Should().Be("0042");
        result.PayDate.Should().Be(new DateTime(2026, 3, 31));
    }

    [Fact]
    public void TryParse_ShortMatricule_PadsWithSpaces()
    {
        // Arrange
        var fileName = "42_20260331.pdf";

        // Act
        var parsed = PayslipFileNameParser.TryParse(fileName, out var result);

        // Assert
        parsed.Should().BeTrue();
        result.Matricule.Should().Be("  42");
    }

    [Fact]
    public void TryParse_NotAPdf_ReturnsFalse()
    {
        // Arrange
        var fileName = "0042_20260331.txt";

        // Act
        var parsed = PayslipFileNameParser.TryParse(fileName, out _);

        // Assert
        parsed.Should().BeFalse();
    }

    [Fact]
    public void TryParse_InvalidDate_ReturnsFalse()
    {
        // Arrange
        var fileName = "0042_20261340.pdf";

        // Act
        var parsed = PayslipFileNameParser.TryParse(fileName, out _);

        // Assert
        parsed.Should().BeFalse();
    }
}
