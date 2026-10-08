using FluentAssertions;
using SoftDemat.Application.DTOs;
using SoftDemat.Application.Validators;

namespace SoftDemat.Tests;

public class PasswordResetValidatorTests
{
    [Fact]
    public void PasswordReset_ValidEmail_IsValid()
    {
        // Arrange
        var validator = new PasswordResetRequestValidator();
        var request = new PasswordResetRequest("utilisateur@softwell.mg");

        // Act
        var result = validator.Validate(request);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData("adresse-invalide")]
    public void PasswordReset_BadEmail_IsInvalid(string email)
    {
        // Arrange
        var validator = new PasswordResetRequestValidator();
        var request = new PasswordResetRequest(email);

        // Act
        var result = validator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void PasswordReset_TooLongEmail_IsInvalid()
    {
        // Arrange
        var validator = new PasswordResetRequestValidator();
        var request = new PasswordResetRequest(new string('a', 250) + "@mg.mg");

        // Act
        var result = validator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void Reset_ValidRequest_IsValid()
    {
        // Arrange
        var validator = new ResetPasswordRequestValidator();
        var request = new ResetPasswordRequest("token", "MotDePasse123!", "MotDePasse123!");

        // Act
        var result = validator.Validate(request);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Reset_EmptyToken_IsInvalid()
    {
        // Arrange
        var validator = new ResetPasswordRequestValidator();
        var request = new ResetPasswordRequest("", "MotDePasse123!", "MotDePasse123!");

        // Act
        var result = validator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void Reset_ShortPassword_IsInvalid()
    {
        // Arrange
        var validator = new ResetPasswordRequestValidator();
        var request = new ResetPasswordRequest("token", "Court1!", "Court1!");

        // Act
        var result = validator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void Reset_MismatchedConfirmation_IsInvalid()
    {
        // Arrange
        var validator = new ResetPasswordRequestValidator();
        var request = new ResetPasswordRequest("token", "MotDePasse123!", "AutreMotDePasse!");

        // Act
        var result = validator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
    }
}
