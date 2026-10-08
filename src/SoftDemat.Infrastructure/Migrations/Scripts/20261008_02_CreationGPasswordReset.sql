IF OBJECT_ID(N'dbo.G_PASSWORD_RESET', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.G_PASSWORD_RESET (
        Id uniqueidentifier NOT NULL CONSTRAINT PK_G_PASSWORD_RESET PRIMARY KEY,
        UserId int NOT NULL,
        TokenHash nvarchar(64) NOT NULL,
        ExpiresAt datetime2 NOT NULL,
        UsedAt datetime2 NULL,
        CreatedAt datetime2 NOT NULL
    );
    CREATE UNIQUE INDEX UX_G_PASSWORD_RESET_TokenHash
        ON dbo.G_PASSWORD_RESET (TokenHash);
    CREATE INDEX IX_G_PASSWORD_RESET_UserId_ExpiresAt
        ON dbo.G_PASSWORD_RESET (UserId, ExpiresAt);
END
