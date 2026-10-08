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
            await context.Database.ExecuteSqlRawAsync(CreateSmtpSql, cancellationToken);
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

    private const string CreateSmtpSql =
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
        """;
}
