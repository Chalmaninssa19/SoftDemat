using FluentAssertions;
using SoftDemat.Domain.Rules;

namespace SoftDemat.Tests;

public class PayslipFilterTests
{
    [Fact]
    public void Matches_AllCriteria_RequiresEveryFilter()
    {
        // Arrange
        var matricule = "0042";
        var fullName = "Rabe Aina";

        // Act
        var matches = PayslipFilter.Matches(matricule, fullName, "TANA", "TANA", "0042", "42", "aina");
        var rejected = PayslipFilter.Matches(matricule, fullName, "TANA", "DIEGO", "0042", "42", "aina");

        // Assert
        matches.Should().BeTrue();
        rejected.Should().BeFalse();
    }
}
