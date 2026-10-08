using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using SoftDemat.Application.DTOs;
using SoftDemat.Application.Validators;
using SoftDemat.Domain.Entities;
using SoftDemat.Domain.Interfaces;
using SoftDemat.Infrastructure.Services;

namespace SoftDemat.Tests;

public class SmtpSettingTests
{
    [Fact]
    public void Update_MissingHost_IsInvalid()
    {
        // Arrange
        var validator = new UpdateSmtpSettingRequestValidator();
        var request = new UpdateSmtpSettingRequest("", 587, true, null, null, "paie@softwell.mg");

        // Act
        var result = validator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public async Task Update_BlankPassword_KeepsStoredSecret()
    {
        // Arrange
        var stored = new SmtpSetting { Id = SmtpSetting.SingletonId, Host = "old", Password = "secret" };
        var service = Service(stored);

        // Act
        var result = await service.UpdateAsync(
            new UpdateSmtpSettingRequest("smtp.softwell.mg", 587, true, "paie", null, "paie@softwell.mg"),
            CancellationToken.None);

        // Assert
        stored.Password.Should().Be("secret");
        stored.Host.Should().Be("smtp.softwell.mg");
        result.HasPassword.Should().BeTrue();
        result.FromAddress.Should().Be("paie@softwell.mg");
        result.User.Should().Be("paie");
    }

    [Fact]
    public async Task Get_MissingRow_ReturnsEmptyDefaults()
    {
        // Arrange
        var service = Service(null);

        // Act
        var result = await service.GetAsync(CancellationToken.None);

        // Assert
        result.Host.Should().BeEmpty();
        result.Port.Should().Be(587);
        result.UseSsl.Should().BeTrue();
        result.HasPassword.Should().BeFalse();
    }

    private static SmtpSettingService Service(SmtpSetting? stored)
    {
        var unitOfWork = new Mock<IUnitOfWork>();
        unitOfWork.Setup(work => work.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
        return new SmtpSettingService(new SmtpStore(stored), unitOfWork.Object, NullLogger<SmtpSettingService>.Instance);
    }

    private sealed class SmtpStore : ISmtpSettingRepository
    {
        private SmtpSetting? _setting;

        public SmtpStore(SmtpSetting? setting)
        {
            _setting = setting;
        }

        public Task<SmtpSetting?> GetAsync(CancellationToken cancellationToken = default)
            => Task.FromResult(_setting);

        public Task AddAsync(SmtpSetting setting, CancellationToken cancellationToken = default)
        {
            _setting = setting;
            return Task.CompletedTask;
        }
    }
}
