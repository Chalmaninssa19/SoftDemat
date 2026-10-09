using System.Net.Sockets;
using FluentAssertions;
using MailKit.Net.Smtp;
using MimeKit;
using SoftDemat.Domain.Entities;
using SoftDemat.Infrastructure.Mail;

namespace SoftDemat.Tests;

public class SmtpDeliveryFailureTests
{
    private const string SenderNotOwned =
        "5.7.1 <rh@softwell.mg>: Sender address rejected: not owned by user softappli@softwell.mg";

    private static readonly SmtpSetting Setting = new()
    {
        Name = "SMTP", Host = "smtpauth.moov.mg", Port = 587, UserName = "softappli@softwell.mg"
    };

    [Fact]
    public void ToDomainException_SenderNotOwnedByAccount_ExplainsWhichAddressToUse()
    {
        // Arrange
        var refusal = new SmtpCommandException(
            SmtpErrorCode.RecipientNotAccepted,
            SmtpStatusCode.MailboxNameNotAllowed,
            MailboxAddress.Parse("salarie@gmail.com"),
            SenderNotOwned);

        // Act
        var result = SmtpDeliveryFailure.ToDomainException(refusal, Setting, "rh@softwell.mg");

        // Assert
        result.Message.Should().Contain("refuse l'expéditeur rh@softwell.mg pour le compte softappli@softwell.mg")
            .And.Contain("doit appartenir à ce compte")
            .And.Contain(SenderNotOwned);
        result.InnerException.Should().BeSameAs(refusal);
    }

    [Fact]
    public void ToDomainException_WrongCredentials_NamesTheAccount()
    {
        // Arrange
        var refusal = new MailKit.Security.AuthenticationException("535 5.7.8 Authentication failed");

        // Act
        var result = SmtpDeliveryFailure.ToDomainException(refusal, Setting, "softappli@softwell.mg");

        // Assert
        result.Message.Should().Contain("mot de passe du compte softappli@softwell.mg");
    }

    [Fact]
    public void IsDeliveryFailure_SmtpAndNetworkErrors_AreTranslated()
    {
        // Arrange
        var failures = new Exception[]
        {
            new SmtpCommandException(SmtpErrorCode.MessageNotAccepted, SmtpStatusCode.TransactionFailed, "refus"),
            new MailKit.Security.AuthenticationException("535 5.7.8 Authentication failed"),
            new SocketException(),
            new IOException()
        };

        // Act
        var results = failures.Select(SmtpDeliveryFailure.IsDeliveryFailure);

        // Assert
        results.Should().OnlyContain(isDeliveryFailure => isDeliveryFailure);
    }

    [Fact]
    public void IsDeliveryFailure_ProgrammingError_IsNotTranslated()
    {
        // Arrange
        var bug = new InvalidOperationException();

        // Act
        var result = SmtpDeliveryFailure.IsDeliveryFailure(bug);

        // Assert
        result.Should().BeFalse();
    }
}
