using FluentAssertions;
using SoftDemat.Domain.Rules;

namespace SoftDemat.Tests;

public class ArchivePathBuilderTests
{
    [Fact]
    public void BuildDirectory_MarchAntananarivo_UsesFrenchMonth()
    {
        // Arrange
        var payDate = new DateTime(2026, 3, 31);

        // Act
        var path = ArchivePathBuilder.BuildDirectory(@"D:\Archives", payDate, "Antananarivo");

        // Assert
        path.Should().Be(Path.Combine(@"D:\Archives", "2026", "mars", "Antananarivo"));
    }

    [Fact]
    public void BuildDirectory_SlashInName_IsReplaced()
    {
        // Arrange
        var payDate = new DateTime(2026, 3, 31);

        // Act
        var path = ArchivePathBuilder.BuildDirectory(@"D:\Archives", payDate, "Nord/Sud");

        // Assert
        path.Should().Be(Path.Combine(@"D:\Archives", "2026", "mars", "Nord-Sud"));
    }

    [Fact]
    public void BuildDirectory_EmptyEstablishment_UsesSansEtablissement()
    {
        // Arrange
        var payDate = new DateTime(2026, 3, 31);

        // Act
        var path = ArchivePathBuilder.BuildDirectory(@"D:\Archives", payDate, " ");

        // Assert
        path.Should().Be(Path.Combine(@"D:\Archives", "2026", "mars", "Sans Etablissement"));
    }

    [Fact]
    public void BuildFileName_UsesMailCodeMonthAndMatricule()
    {
        // Arrange
        var payDate = new DateTime(2026, 3, 31);

        // Act
        var name = ArchivePathBuilder.BuildFileName("BP", payDate, "0042", "Aina");

        // Assert
        name.Should().Be("BP_03_2026_0042_Aina.pdf");
    }
}
