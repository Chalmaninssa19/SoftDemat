using System.Runtime.CompilerServices;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SoftDemat.Infrastructure.Context;

[assembly: InternalsVisibleTo("SoftDemat.Tests")]

namespace SoftDemat.Infrastructure.Migrations;

internal sealed record SqlScript(string Id, string Sql);

public sealed class SqlScriptMigrator
{
    private const string ResourcePrefix = "SoftDemat.Infrastructure.Migrations.Scripts.";
    private const string CreateHistoryTableSql =
        """
        IF OBJECT_ID(N'dbo.G_MIGRATION_HISTORY', N'U') IS NULL
        BEGIN
            CREATE TABLE dbo.G_MIGRATION_HISTORY (
                MigrationId nvarchar(200) NOT NULL CONSTRAINT PK_G_MIGRATION_HISTORY PRIMARY KEY,
                AppliedAt datetime2 NOT NULL
            );
        END
        """;

    private readonly SdtDbContext _context;
    private readonly ILogger<SqlScriptMigrator> _logger;

    public SqlScriptMigrator(SdtDbContext context, ILogger<SqlScriptMigrator> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task MigrateAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            if (!await CanConnectAsync(cancellationToken))
                return;

            await _context.Database.ExecuteSqlRawAsync(CreateHistoryTableSql, cancellationToken);
            var applied = await AppliedIdsAsync(cancellationToken);
            foreach (var script in LoadScripts())
            {
                if (applied.Contains(script.Id))
                    continue;
                if (string.IsNullOrWhiteSpace(script.Sql))
                {
                    _logger.LogWarning("Script de migration {MigrationId} vide, ignoré.", script.Id);
                    continue;
                }

                await ApplyAsync(script, cancellationToken);
            }
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Application des scripts de migration impossible.");
        }
    }

    internal static IReadOnlyList<SqlScript> LoadScripts()
    {
        var assembly = typeof(SqlScriptMigrator).Assembly;
        return assembly.GetManifestResourceNames()
            .Where(name => name.StartsWith(ResourcePrefix, StringComparison.Ordinal)
                && name.EndsWith(".sql", StringComparison.Ordinal))
            .OrderBy(name => name, StringComparer.Ordinal)
            .Select(name => new SqlScript(ScriptId(name), ReadResource(assembly, name)))
            .ToList();
    }

    private async Task<bool> CanConnectAsync(CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(_context.Database.GetConnectionString()))
        {
            _logger.LogWarning("Chaîne SDT absente. Scripts de migration non appliqués.");
            return false;
        }

        if (!await _context.Database.CanConnectAsync(cancellationToken))
        {
            _logger.LogWarning("Base SDT inaccessible. Scripts de migration non appliqués.");
            return false;
        }

        return true;
    }

    private async Task<HashSet<string>> AppliedIdsAsync(CancellationToken cancellationToken)
    {
        var ids = await _context.Database
            .SqlQueryRaw<string>("SELECT MigrationId FROM dbo.G_MIGRATION_HISTORY")
            .ToListAsync(cancellationToken);
        return new HashSet<string>(ids, StringComparer.Ordinal);
    }

    private async Task ApplyAsync(SqlScript script, CancellationToken cancellationToken)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
        await _context.Database.ExecuteSqlRawAsync(script.Sql, cancellationToken);
        await _context.Database.ExecuteSqlAsync(
            $"INSERT INTO dbo.G_MIGRATION_HISTORY (MigrationId, AppliedAt) VALUES ({script.Id}, {DateTime.UtcNow})",
            cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        _logger.LogInformation("Script de migration {MigrationId} appliqué.", script.Id);
    }

    private static string ScriptId(string resourceName)
    {
        var file = resourceName.Substring(ResourcePrefix.Length);
        return file.Substring(0, file.Length - ".sql".Length);
    }

    private static string ReadResource(System.Reflection.Assembly assembly, string resourceName)
    {
        using var stream = assembly.GetManifestResourceStream(resourceName)
            ?? throw new InvalidOperationException($"Ressource introuvable : {resourceName}.");
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }
}
