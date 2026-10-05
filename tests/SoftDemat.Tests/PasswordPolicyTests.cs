using FluentAssertions;
using SoftDemat.Domain.Rules;

namespace SoftDemat.Tests;

public class PasswordPolicyTests
{
    [Fact]
    public void IsSatisfied_TwelveCharsThreeFamilies_ReturnsTrue()
    {
        // Arrange
        var password = "Bulletin-Paie1";

        // Act
        var satisfied = PasswordPolicy.IsSatisfied(password);

        // Assert
        satisfied.Should().BeTrue();
    }

    [Fact]
    public void IsSatisfied_TooShort_ReturnsFalse()
    {
        // Arrange
        var password = "Abcdef1!";

        // Act
        var satisfied = PasswordPolicy.IsSatisfied(password);

        // Assert
        satisfied.Should().BeFalse();
    }

    [Fact]
    public void IsSatisfied_OnlyTwoFamilies_ReturnsFalse()
    {
        // Arrange
        var password = "bulletindepaie";

        // Act
        var satisfied = PasswordPolicy.IsSatisfied(password);

        // Assert
        satisfied.Should().BeFalse();
    }
}
