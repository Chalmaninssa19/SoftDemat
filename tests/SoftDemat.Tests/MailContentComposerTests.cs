using FluentAssertions;
using SoftDemat.Domain.Rules;

namespace SoftDemat.Tests;

public class MailContentComposerTests
{
    [Fact]
    public void Apply_March2026_WritesFrenchMonthAndYear()
    {
        // Arrange
        var template = "Bulletin MM AA";
        var payDate = new DateTime(2026, 3, 31);

        // Act
        var result = MailContentComposer.Apply(template, payDate, "Marie", "Rabe Marie", "0042");

        // Assert
        result.Should().Be("Bulletin mars 2026");
    }

    [Fact]
    public void Apply_FirstNameToken_IsNotReplacedByLastNameToken()
    {
        // Arrange
        var template = "Bonjour PNOM NOM, matricule MAT";
        var payDate = new DateTime(2026, 3, 31);

        // Act
        var result = MailContentComposer.Apply(template, payDate, "Aina", "Rabe Aina", "0042");

        // Assert
        result.Should().Be("Bonjour Aina Rabe Aina, matricule 0042");
    }

    [Fact]
    public void Apply_NewLine_BecomesBreak()
    {
        // Arrange
        var template = "Ligne 1\nLigne 2";

        // Act
        var result = MailContentComposer.Apply(template, new DateTime(2026, 1, 1), "", "", "");

        // Assert
        result.Should().Be("Ligne 1<br/>Ligne 2");
    }
}
