using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SoftDemat.Infrastructure.Context;

namespace SoftDemat.Infrastructure.Persistence;

public static class SdtSchemaInitializer
{
    public static async Task EnsureAuthTablesAsync(SdtDbContext context, ILogger logger, CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(context.Database.GetConnectionString()))
            {
                logger.LogWarning("Chaîne SDT absente. Tables d'authentification non créées.");
                return;
            }

            if (!await context.Database.CanConnectAsync(cancellationToken))
            {
                logger.LogWarning("Base SDT inaccessible. Tables d'authentification non créées.");
                return;
            }

            await context.Database.ExecuteSqlRawAsync(CreateAuthSessionSql, cancellationToken);
            await context.Database.ExecuteSqlRawAsync(CreateUserSecuritySql, cancellationToken);
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Initialisation des tables d'authentification impossible.");
        }
    }

    private const string CreateAuthSessionSql =
        """
        IF OBJECT_ID(N'dbo.G_AUTH_SESSION', N'U') IS NULL
        BEGIN
            CREATE TABLE dbo.G_AUTH_SESSION (
                Id uniqueidentifier NOT NULL CONSTRAINT PK_G_AUTH_SESSION PRIMARY KEY,
                UserId int NOT NULL,
                RefreshTokenHash nvarchar(128) NOT NULL,
                ExpiresAt datetime2 NOT NULL,
                RevokedAt datetime2 NULL,
                CreatedAt datetime2 NOT NULL
            );
        END
        """;

    private const string CreateUserSecuritySql =
        """
        IF OBJECT_ID(N'dbo.G_USER_SECURITY', N'U') IS NULL
        BEGIN
            CREATE TABLE dbo.G_USER_SECURITY (
                UserId int NOT NULL CONSTRAINT PK_G_USER_SECURITY PRIMARY KEY,
                MustChangePassword bit NOT NULL
            );
        END
        """;
}
