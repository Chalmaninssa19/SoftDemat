using System.Data;
using System.Data.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SoftDemat.Domain.Entities;
using SoftDemat.Domain.Interfaces;
using SoftDemat.Infrastructure.Context;

namespace SoftDemat.Infrastructure.Repositories;

public sealed class SmtpSettingRepository : ISmtpSettingRepository
{
    private const string LegacyListSql =
        "SELECT TOP (50) Id, Host, Port, UseSsl, UserName, Password, FromAddress FROM dbo.G_SMTP ORDER BY Id";

    private readonly SdtDbContext _context;
    private readonly ILogger<SmtpSettingRepository> _logger;

    public SmtpSettingRepository(SdtDbContext context, ILogger<SmtpSettingRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<IReadOnlyList<SmtpSetting>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var settings = await _context.SmtpSettings.AsNoTracking().OrderBy(s => s.Id).ToListAsync(cancellationToken);
            if (settings.Count > 0)
                return settings;
        }
        catch (Exception exception) when (IsMissingStorage(exception))
        {
            _logger.LogWarning(exception, "Table G_SMTP_SETTING absente. Repli vers G_SMTP.");
        }

        return await GetLegacyAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<SmtpSetting>> GetAllForUpdateAsync(CancellationToken cancellationToken = default)
        => await _context.SmtpSettings.OrderBy(setting => setting.Id).ToListAsync(cancellationToken);

    public Task<SmtpSetting?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        => _context.SmtpSettings.FirstOrDefaultAsync(setting => setting.Id == id, cancellationToken);

    public async Task<SmtpSetting?> GetActiveAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var active = await _context.SmtpSettings
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.IsActive, cancellationToken);
            if (active is not null)
                return active;
        }
        catch (Exception exception) when (IsMissingStorage(exception))
        {
            _logger.LogWarning(exception, "Table G_SMTP_SETTING absente. Repli vers G_SMTP pour l'actif.");
        }

        var legacy = await GetLegacyAsync(cancellationToken);
        return legacy.FirstOrDefault();
    }

    public Task AddAsync(SmtpSetting setting, CancellationToken cancellationToken = default)
    {
        _context.SmtpSettings.Add(setting);
        return Task.CompletedTask;
    }

    public void Remove(SmtpSetting setting)
        => _context.SmtpSettings.Remove(setting);

    private async Task<IReadOnlyList<SmtpSetting>> GetLegacyAsync(CancellationToken cancellationToken)
    {
        try
        {
            return await ReadLegacyAsync(cancellationToken);
        }
        catch (Exception exception)
        {
            _logger.LogWarning(exception, "Lecture G_SMTP impossible.");
            return [];
        }
    }

    private async Task<IReadOnlyList<SmtpSetting>> ReadLegacyAsync(CancellationToken cancellationToken)
    {
        var connection = _context.Database.GetDbConnection();
        var shouldClose = connection.State != ConnectionState.Open;
        if (shouldClose)
            await connection.OpenAsync(cancellationToken);

        try
        {
            using var command = connection.CreateCommand();
            command.CommandText = LegacyListSql;
            using var reader = await ((DbCommand)command).ExecuteReaderAsync(cancellationToken);
            return MapLegacy(reader);
        }
        finally
        {
            if (shouldClose)
                connection.Close();
        }
    }

    private static IReadOnlyList<SmtpSetting> MapLegacy(DbDataReader reader)
    {
        var settings = new List<SmtpSetting>();
        var ordinals = Ordinals(reader);
        while (reader.Read())
            settings.Add(MapRow(reader, ordinals));

        return settings;
    }

    private static Dictionary<string, int> Ordinals(DbDataReader reader)
    {
        var ordinals = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        for (var index = 0; index < reader.FieldCount; index++)
            ordinals.TryAdd(reader.GetName(index), index);

        return ordinals;
    }

    private static SmtpSetting MapRow(DbDataReader reader, Dictionary<string, int> ordinals)
        => new()
        {
            Id = GetInt(reader, ordinals, "Id"),
            Name = "Configuration existante",
            IsActive = true,
            Host = GetText(reader, ordinals, "Host"),
            Port = GetInt(reader, ordinals, "Port", 587),
            UseSsl = GetBool(reader, ordinals, "UseSsl", true),
            UserName = GetText(reader, ordinals, "UserName"),
            Password = GetText(reader, ordinals, "Password"),
            FromAddress = GetText(reader, ordinals, "FromAddress"),
        };

    private static int GetInt(DbDataReader reader, Dictionary<string, int> ordinals, string name, int fallback = 0)
    {
        if (ordinals.TryGetValue(name, out var index) && !reader.IsDBNull(index))
            return Convert.ToInt32(reader.GetValue(index));

        return fallback;
    }

    private static string GetText(DbDataReader reader, Dictionary<string, int> ordinals, string name)
    {
        if (ordinals.TryGetValue(name, out var index) && !reader.IsDBNull(index))
            return Convert.ToString(reader.GetValue(index)) ?? string.Empty;

        return string.Empty;
    }

    private static bool GetBool(DbDataReader reader, Dictionary<string, int> ordinals, string name, bool fallback)
    {
        if (!ordinals.TryGetValue(name, out var index) || reader.IsDBNull(index))
            return fallback;

        return Convert.ToBoolean(reader.GetValue(index));
    }

    private static bool IsMissingStorage(Exception exception)
    {
        var current = exception;
        while (current is not null)
        {
            if (current is DbException db && (db.ErrorCode == -2146232060 || HasMissingObjectMessage(db.Message)))
                return true;

            if (HasMissingObjectMessage(current.Message))
                return true;

            current = current.InnerException;
        }

        return false;
    }

    private static bool HasMissingObjectMessage(string message)
        => message.Contains("Invalid object name", StringComparison.OrdinalIgnoreCase)
            || message.Contains("Nom d'objet non valide", StringComparison.OrdinalIgnoreCase)
            || message.Contains("G_SMTP_SETTING", StringComparison.OrdinalIgnoreCase);
}
