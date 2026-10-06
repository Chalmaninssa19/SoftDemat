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
    [Fact]
    public void Update_OutlookWithoutAddress_IsValid()
    {
        // Arrange
        var validator = new UpdateGeneralParameterRequestValidator();
        var request = new UpdateGeneralParameterRequest(@"C:\archives", null, MailSenderTools.Outlook, null);

        // Act
        var result = validator.Validate(request);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Update_AddressWithoutEmail_IsInvalid()
    {
        // Arrange
        var validator = new UpdateGeneralParameterRequestValidator();
        var request = new UpdateGeneralParameterRequest(@"C:\archives", null, MailSenderTools.Address, "pas-un-email");

        // Act
        var result = validator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public async Task Send_OutlookOnAnotherMachine_DoesNotOpenMailbox()
    {
        // Arrange
        var outlook = new Mock<ILocalMailbox>();
        var smtp = new Mock<IMailSender>();
        var mailer = new PayslipMailer(new SenderStore(MailSenderTools.Outlook, string.Empty), smtp.Object, outlook.Object);

        // Act
        var act = async () => await mailer.SendAsync(Sample(), false, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<DomainException>();
        outlook.Verify(mailbox => mailbox.Send(It.IsAny<OutgoingMail>()), Times.Never);
    }

    [Fact]
    public async Task Send_ConfiguredAddress_UsesThatSender()
    {
        // Arrange
        OutgoingMail? sent = null;
        var smtp = new Mock<IMailSender>();
        smtp.Setup(sender => sender.SendAsync(It.IsAny<OutgoingMail>(), It.IsAny<CancellationToken>()))
            .Callback<OutgoingMail, CancellationToken>((mail, _) => sent = mail)
            .Returns(Task.CompletedTask);
        var mailer = new PayslipMailer(new SenderStore(MailSenderTools.Address, "paie@softwell.mg"), smtp.Object, Mock.Of<ILocalMailbox>());

        // Act
        var from = await mailer.SendAsync(Sample(), false, CancellationToken.None);

        // Assert
        from.Should().Be("paie@softwell.mg");
        sent!.From.Should().Be("paie@softwell.mg");
        sent.To.Should().Be("salarie@softwell.mg");
    }

    private static OutgoingMail Sample()
        => new("salarie@softwell.mg", null, "Bulletin", "<p>Bulletin</p>", @"C:\bulletins\0042_20260331.pdf", "0042.pdf");

    private sealed class SenderStore : IMailSenderSettingRepository
    {
        private readonly MailSenderSetting _setting;

        public SenderStore(string tool, string address)
        {
            _setting = new MailSenderSetting { Id = MailSenderSetting.SingletonId, SenderTool = tool, SenderAddress = address };
        }

        public Task<MailSenderSetting?> GetAsync(CancellationToken cancellationToken = default)
            => Task.FromResult<MailSenderSetting?>(_setting);

        public Task AddAsync(MailSenderSetting setting, CancellationToken cancellationToken = default)
            => Task.CompletedTask;
    }
}
