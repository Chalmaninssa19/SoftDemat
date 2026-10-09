using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using SoftDemat.Domain.Exceptions;
using SoftDemat.Domain.Interfaces;
using SoftDemat.Infrastructure.Services;

namespace SoftDemat.Tests;

public class PasswordResetNotifierTests
{
    [Fact]
    public async Task SendResetLink_ValidSmtp_SendsLinkToAccountEmail()
    {
        // Arrange
        OutgoingMail? sent = null;
        var mailSender = new Mock<IMailSender>();
        mailSender.Setup(sender => sender.SendAsync(It.IsAny<OutgoingMail>(), It.IsAny<CancellationToken>()))
            .Callback<OutgoingMail, CancellationToken>((mail, _) => sent = mail)
            .ReturnsAsync("softappli@softwell.mg");
        var sut = Notifier(mailSender);

        // Act
        await sut.SendResetLinkAsync("salarie@gmail.com", "jeton", CancellationToken.None);

        // Assert
        sent.Should().NotBeNull();
        sent!.To.Should().Be("salarie@gmail.com");
        sent.HtmlBody.Should().Contain("http://localhost:4200/auth/reinitialiser?token=jeton");
    }

    [Fact]
    public async Task SendResetLink_SmtpRefusesSender_HidesTechnicalReasonFromRequester()
    {
        // Arrange
        var mailSender = new Mock<IMailSender>();
        mailSender.Setup(sender => sender.SendAsync(It.IsAny<OutgoingMail>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new DomainException("Le serveur SMTP smtpauth.moov.mg refuse l'expéditeur rh@softwell.mg."));
        var sut = Notifier(mailSender);

        // Act
        var act = () => sut.SendResetLinkAsync("salarie@gmail.com", "jeton", CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("Impossible d'envoyer le lien de réinitialisation pour le moment.");
    }

    private static PasswordResetNotifier Notifier(Mock<IMailSender> mailSender)
    {
        var configuration = new Mock<IConfiguration>();
        configuration.Setup(settings => settings["PasswordReset:FrontendBaseUrl"]).Returns("http://localhost:4200");
        return new PasswordResetNotifier(
            mailSender.Object,
            configuration.Object,
            NullLogger<PasswordResetNotifier>.Instance);
    }
}
