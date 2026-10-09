-- Migration : creation de G_SMTP_SETTING (liste des serveurs SMTP) + reprise de la ligne historique G_SMTP.
-- Idempotent : rejouable sans doublon. Applique via SqlScriptMigrator (G_MIGRATION_HISTORY).
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
END;

IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = N'IX_G_SMTP_SETTING_IsActive'
      AND object_id = OBJECT_ID(N'dbo.G_SMTP_SETTING'))
    CREATE INDEX IX_G_SMTP_SETTING_IsActive ON dbo.G_SMTP_SETTING (IsActive);

-- Reprise historique : G_SMTP (table Desktop, sans Name/IsActive) vers G_SMTP_SETTING.
-- Sans filtre Id = 1 : reprend la premiere ligne quel que soit son Id. Ne fait rien si la cible contient deja des lignes.
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
END;
