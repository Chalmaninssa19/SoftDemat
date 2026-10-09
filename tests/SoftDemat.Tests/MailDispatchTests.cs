using FluentAssertions;
using Moq;
using SoftDemat.Application.DTOs;
using SoftDemat.Application.Validators;
using SoftDemat.Domain.Entities;
using SoftDemat.Domain.Exceptions;
using SoftDemat.Domain.Interfaces;
using SoftDemat.Domain.Rules;
using SoftDemat.Infrastructure.Mail;

namespace SoftDemat.Tests;

public class MailDispatchTests
{
    [Theory]
    [InlineData(MailSenderTools.Outlook)]
    [InlineData(MailSenderTools.MailKit)]
    public void Update_KnownMode_IsValid(string tool)
    {
        // Arrange
        var validator = new UpdateGeneralParameterRequestValidator();
        var request = new UpdateGeneralParameterRequest(@"C:\archives", null, tool, null);

        // Act
        var result = validator.Validate(request);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData(MailSenderTools.LegacyAddress)]
    [InlineData("Fax")]
    [InlineData("")]
    public void Update_UnknownMode_IsInvalid(string tool)
    {
        // Arrange
        var validator = new UpdateGeneralParameterRequestValidator();
        var request = new UpdateGeneralParameterRequest(@"C:\archives", null, tool, null);

        // Act
        var result = validator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(error => error.ErrorMessage.Contains("MailKit"));
    }

    [Fact]
    public async Task Send_Outlook_UsesMailboxAndReturnsItsAddress()
    {
        // Arrange
        var outlook = new Mock<ILocalMailbox>();
        var smtp = new Mock<IMailSender>();
        outlook.Setup(mailbox => mailbox.Send(It.IsAny<OutgoingMail>())).Returns("paie@softwell.mg");
        var mailer = Mailer(MailSenderTools.Outlook, smtp, outlook);

        // Act
        var from = await mailer.SendAsync(Sample(), true, CancellationToken.None);

        // Assert
        from.Should().Be("paie@softwell.mg");
        outlook.Verify(mailbox => mailbox.Send(It.Is<OutgoingMail>(mail => SameMessage(mail))), Times.Once);
        smtp.Verify(sender => sender.SendAsync(It.IsAny<OutgoingMail>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Send_OutlookOnAnotherMachine_DoesNotOpenMailbox()
    {
        // Arrange
        var outlook = new Mock<ILocalMailbox>();
        var smtp = new Mock<IMailSender>();
        var mailer = Mailer(MailSenderTools.Outlook, smtp, outlook);

        // Act
        var act = async () => await mailer.SendAsync(Sample(), false, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<DomainException>().WithMessage("*MailKit*");
        outlook.Verify(mailbox => mailbox.Send(It.IsAny<OutgoingMail>()), Times.Never);
        smtp.Verify(sender => sender.SendAsync(It.IsAny<OutgoingMail>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Theory]
    [InlineData(MailSenderTools.MailKit)]
    [InlineData(MailSenderTools.LegacyAddress)]
    public async Task Send_MailKitOrLegacyAddress_UsesSmtp(string tool)
    {
        // Arrange
        OutgoingMail? sent = null;
        var outlook = new Mock<ILocalMailbox>();
        var smtp = new Mock<IMailSender>();
        smtp.Setup(sender => sender.SendAsync(It.IsAny<OutgoingMail>(), It.IsAny<CancellationToken>()))
            .Callback<OutgoingMail, CancellationToken>((mail, _) => sent = mail)
            .ReturnsAsync("smtp@softwell.mg");
        var mailer = Mailer(tool, smtp, outlook);

        // Act
        var from = await mailer.SendAsync(Sample(), false, CancellationToken.None);

        // Assert
        from.Should().Be("smtp@softwell.mg");
        sent.Should().NotBeNull();
        SameMessage(sent!).Should().BeTrue();
        outlook.Verify(mailbox => mailbox.Send(It.IsAny<OutgoingMail>()), Times.Never);
    }

    [Fact]
    public async Task Send_MailKitFailure_DoesNotOpenOutlook()
    {
        // Arrange
        var outlook = new Mock<ILocalMailbox>();
        var smtp = new Mock<IMailSender>();
        smtp.Setup(sender => sender.SendAsync(It.IsAny<OutgoingMail>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new DomainException("Le serveur SMTP n'est pas configuré."));
        var mailer = Mailer(MailSenderTools.MailKit, smtp, outlook);

        // Act
        var act = async () => await mailer.SendAsync(Sample(), true, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<DomainException>().WithMessage("*SMTP*");
        outlook.Verify(mailbox => mailbox.Send(It.IsAny<OutgoingMail>()), Times.Never);
    }

    [Fact]
    public async Task Send_UnknownMode_ThrowsWithoutSending()
    {
        // Arrange
        var outlook = new Mock<ILocalMailbox>();
        var smtp = new Mock<IMailSender>();
        var mailer = Mailer("Fax", smtp, outlook);

        // Act
        var act = async () => await mailer.SendAsync(Sample(), true, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<DomainException>().WithMessage("*pas reconnu*");
        outlook.Verify(mailbox => mailbox.Send(It.IsAny<OutgoingMail>()), Times.Never);
        smtp.Verify(sender => sender.SendAsync(It.IsAny<OutgoingMail>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task MailKit_MissingSmtp_ThrowsBeforeConnect()
    {
        // Arrange
        var sender = new SmtpMailSender(new SmtpStore());

        // Act
        var act = async () => await sender.SendAsync(Sample(), CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<DomainException>().WithMessage("*SMTP*");
    }

    private static PayslipMailer Mailer(string tool, Mock<IMailSender> smtp, Mock<ILocalMailbox> outlook)
        => new(new SenderStore(tool), smtp.Object, outlook.Object, Microsoft.Extensions.Logging.Abstractions.NullLogger<PayslipMailer>.Instance);

    private static bool SameMessage(OutgoingMail mail)
        => mail.To == "salarie@softwell.mg"
            && mail.Bcc == "copie@softwell.mg"
            && mail.Subject == "Bulletin"
            && mail.HtmlBody == "<p>Bulletin</p>"
            && mail.AttachmentName == "0042.pdf";

    private static OutgoingMail Sample()
        => new("salarie@softwell.mg", "copie@softwell.mg", "Bulletin", "<p>Bulletin</p>", @"C:\bulletins\0042_20260331.pdf", "0042.pdf");

    private sealed class SenderStore : IMailSenderSettingRepository
    {
        private readonly MailSenderSetting _setting;

        public SenderStore(string tool)
        {
            _setting = new MailSenderSetting { Id = MailSenderSetting.SingletonId, SenderTool = tool };
        }

        public Task<MailSenderSetting?> GetAsync(CancellationToken cancellationToken = default)
            => Task.FromResult<MailSenderSetting?>(_setting);

        public Task AddAsync(MailSenderSetting setting, CancellationToken cancellationToken = default)
            => Task.CompletedTask;
    }

    private sealed class SmtpStore : ISmtpSettingRepository
    {
        public Task<IReadOnlyList<SmtpSetting>> GetAllAsync(CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<SmtpSetting>>([]);

        public Task<IReadOnlyList<SmtpSetting>> GetAllForUpdateAsync(CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<SmtpSetting>>([]);

        public Task<SmtpSetting?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
            => Task.FromResult<SmtpSetting?>(null);

        public Task<SmtpSetting?> GetActiveAsync(CancellationToken cancellationToken = default)
            => Task.FromResult<SmtpSetting?>(null);

        public Task AddAsync(SmtpSetting setting, CancellationToken cancellationToken = default)
            => Task.CompletedTask;

        public void Remove(SmtpSetting setting)
        {
        }
    }
}
