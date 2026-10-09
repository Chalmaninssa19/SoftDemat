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
            await context.Database.ExecuteSqlRawAsync(CreateMailSenderSql, cancellationToken);
            await context.Database.ExecuteSqlRawAsync(CreateSmtpTablesSql, cancellationToken);
            await context.Database.ExecuteSqlRawAsync(MigrateSmtpSql, cancellationToken);
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

    private const string CreateMailSenderSql =
        """
        IF OBJECT_ID(N'dbo.G_MAIL_SENDER', N'U') IS NULL
        BEGIN
            CREATE TABLE dbo.G_MAIL_SENDER (
                Id int NOT NULL CONSTRAINT PK_G_MAIL_SENDER PRIMARY KEY,
                SenderTool nvarchar(20) NOT NULL,
                SenderAddress nvarchar(200) NOT NULL CONSTRAINT DF_G_MAIL_SENDER_Address DEFAULT ''
            );
        END
        IF NOT EXISTS (SELECT 1 FROM dbo.G_MAIL_SENDER WHERE Id = 1)
            INSERT INTO dbo.G_MAIL_SENDER (Id, SenderTool, SenderAddress) VALUES (1, 'Outlook', '');
        UPDATE dbo.G_MAIL_SENDER SET SenderTool = 'MailKit' WHERE SenderTool = 'Address';
        """;

    private const string CreateSmtpTablesSql =
        """
        IF OBJECT_ID(N'dbo.G_SMTP', N'U') IS NULL
        BEGIN
            CREATE TABLE dbo.G_SMTP (
                Id int NOT NULL CONSTRAINT PK_G_SMTP PRIMARY KEY,
                Host nvarchar(200) NOT NULL,
                Port int NOT NULL,
                UseSsl bit NOT NULL,
                UserName nvarchar(200) NOT NULL,
                Password nvarchar(500) NOT NULL,
                FromAddress nvarchar(200) NOT NULL
            );
        END

        IF OBJECT_ID(N'dbo.G_SMTP_SETTING', N'U') IS NULL
        BEGIN
            CREATE TABLE dbo.G_SMTP_SETTING (
                Id int IDENTITY(1,1) NOT NULL CONSTRAINT PK_G_SMTP_SETTING PRIMARY KEY,
                Name nvarchar(100) NOT NULL,
                IsActive bit NOT NULL,
                Host nvarchar(200) NOT NULL,
                Port int NOT NULL,
                UseSsl bit NOT NULL,
                UserName nvarchar(200) NOT NULL,
                Password nvarchar(500) NOT NULL,
                FromAddress nvarchar(200) NOT NULL
            );
        END

        IF OBJECT_ID(N'dbo.G_SMTP_MIGRATION', N'U') IS NULL
        BEGIN
            CREATE TABLE dbo.G_SMTP_MIGRATION (
                MigrationKey nvarchar(100) NOT NULL CONSTRAINT PK_G_SMTP_MIGRATION PRIMARY KEY,
                AppliedAt datetime2 NOT NULL
            );
        END

        IF NOT EXISTS (
            SELECT 1 FROM sys.indexes
            WHERE name = N'IX_G_SMTP_SETTING_IsActive'
              AND object_id = OBJECT_ID(N'dbo.G_SMTP_SETTING'))
            CREATE INDEX IX_G_SMTP_SETTING_IsActive ON dbo.G_SMTP_SETTING (IsActive);
        """;

    private const string MigrateSmtpSql =
        """
        IF OBJECT_ID(N'dbo.G_SMTP', N'U') IS NOT NULL
        AND OBJECT_ID(N'dbo.G_SMTP_SETTING', N'U') IS NOT NULL
        AND COL_LENGTH(N'dbo.G_SMTP', N'Host') IS NOT NULL
        AND COL_LENGTH(N'dbo.G_SMTP', N'Port') IS NOT NULL
        AND COL_LENGTH(N'dbo.G_SMTP', N'UseSsl') IS NOT NULL
        AND COL_LENGTH(N'dbo.G_SMTP', N'UserName') IS NOT NULL
        AND COL_LENGTH(N'dbo.G_SMTP', N'Password') IS NOT NULL
        AND COL_LENGTH(N'dbo.G_SMTP', N'FromAddress') IS NOT NULL
        AND NOT EXISTS (SELECT 1 FROM dbo.G_SMTP_SETTING)
        AND EXISTS (SELECT 1 FROM dbo.G_SMTP)
        BEGIN
            INSERT INTO dbo.G_SMTP_SETTING
                (Name, IsActive, Host, Port, UseSsl, UserName, Password, FromAddress)
            SELECT TOP (1) N'Configuration existante', 1, Host, Port, UseSsl, UserName, Password, FromAddress
            FROM dbo.G_SMTP
            ORDER BY Id;
        END

        IF OBJECT_ID(N'dbo.G_SMTP_MIGRATION', N'U') IS NOT NULL
        AND NOT EXISTS (SELECT 1 FROM dbo.G_SMTP_MIGRATION WHERE MigrationKey = N'G_SMTP_TO_G_SMTP_SETTING')
        BEGIN
            INSERT INTO dbo.G_SMTP_MIGRATION (MigrationKey, AppliedAt)
            VALUES (N'G_SMTP_TO_G_SMTP_SETTING', SYSUTCDATETIME());
        END
        """;
}
