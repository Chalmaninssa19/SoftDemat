using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using SoftDemat.Application.DTOs;
using SoftDemat.Application.Validators;
using SoftDemat.Domain.Entities;
using SoftDemat.Domain.Exceptions;
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
        var request = Request(host: string.Empty);

        // Act
        var result = validator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public async Task Update_BlankPassword_KeepsStoredSecret()
    {
        // Arrange
        var stored = new SmtpSetting { Id = 3, Name = "SMTP historique", Host = "old", Password = "secret" };
        var service = Service(stored);

        // Act
        var result = await service.UpdateAsync(3, Request(), CancellationToken.None);

        // Assert
        stored.Password.Should().Be("secret");
        stored.Host.Should().Be("smtp.softwell.mg");
        result.HasPassword.Should().BeTrue();
        result.FromAddress.Should().Be("paie@softwell.mg");
        result.User.Should().Be("paie");
    }

    [Fact]
    public async Task Create_FirstSetting_MakesItActive()
    {
        // Arrange
        var store = new SmtpStore();
        var service = Service(store);

        // Act
        var result = await service.CreateAsync(Request(), CancellationToken.None);

        // Assert
        result.IsActive.Should().BeTrue();
        (await store.GetActiveAsync()).Should().NotBeNull();
    }

    [Fact]
    public async Task Activate_Setting_DeactivatesOtherSettings()
    {
        // Arrange
        var first = new SmtpSetting { Id = 1, Name = "Premier", IsActive = true };
        var second = new SmtpSetting { Id = 2, Name = "Second" };
        var store = new SmtpStore(first, second);
        var service = Service(store);

        // Act
        await service.ActivateAsync(second.Id, CancellationToken.None);

        // Assert
        first.IsActive.Should().BeFalse();
        second.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task Delete_ActiveSetting_ActivatesRemainingSetting()
    {
        // Arrange
        var active = new SmtpSetting { Id = 1, Name = "Active", IsActive = true };
        var replacement = new SmtpSetting { Id = 2, Name = "Autre" };
        var store = new SmtpStore(active, replacement);
        var service = Service(store);

        // Act
        await service.DeleteAsync(active.Id, CancellationToken.None);

        // Assert
        store.Settings.Should().ContainSingle().Which.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task Update_UnknownSetting_ThrowsNotFound()
    {
        // Arrange
        var service = Service(new SmtpStore());

        // Act
        var act = () => service.UpdateAsync(9, Request(), CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    private static UpdateSmtpSettingRequest Request(string host = "smtp.softwell.mg")
        => new("Paie", host, 587, true, "paie", null, "paie@softwell.mg");

    private static SmtpSettingService Service(params SmtpSetting[] settings)
        => Service(new SmtpStore(settings));

    private static SmtpSettingService Service(SmtpStore store)
    {
        var unitOfWork = new Mock<IUnitOfWork>();
        unitOfWork.Setup(work => work.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
        return new SmtpSettingService(store, unitOfWork.Object, NullLogger<SmtpSettingService>.Instance);
    }

    private sealed class SmtpStore : ISmtpSettingRepository
    {
        public SmtpStore(params SmtpSetting[] settings)
        {
            Settings = settings.ToList();
        }

        public List<SmtpSetting> Settings { get; }

        public Task<IReadOnlyList<SmtpSetting>> GetAllAsync(CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<SmtpSetting>>(Settings);

        public Task<IReadOnlyList<SmtpSetting>> GetAllForUpdateAsync(CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<SmtpSetting>>(Settings);

        public Task<SmtpSetting?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
            => Task.FromResult(Settings.FirstOrDefault(setting => setting.Id == id));

        public Task<SmtpSetting?> GetActiveAsync(CancellationToken cancellationToken = default)
            => Task.FromResult(Settings.FirstOrDefault(setting => setting.IsActive));

        public Task AddAsync(SmtpSetting setting, CancellationToken cancellationToken = default)
        {
            setting.Id = Settings.Count == 0 ? 1 : Settings.Max(current => current.Id) + 1;
            Settings.Add(setting);
            return Task.CompletedTask;
        }

        public void Remove(SmtpSetting setting)
            => Settings.Remove(setting);
    }
}
