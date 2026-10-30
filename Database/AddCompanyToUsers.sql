IF OBJECT_ID(N'dbo.Companies', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Companies
    (
        CompanyId INT IDENTITY(1,1) NOT NULL,
        CompanyName NVARCHAR(150) NOT NULL,
        CONSTRAINT PK_Companies PRIMARY KEY (CompanyId)
    );

    CREATE UNIQUE INDEX IX_Companies_CompanyName
        ON dbo.Companies (CompanyName);
END;
GO

IF COL_LENGTH(N'dbo.Users', N'CompanyId') IS NULL
BEGIN
    ALTER TABLE dbo.Users ADD CompanyId INT NULL;

    CREATE INDEX IX_Users_CompanyId
        ON dbo.Users (CompanyId);

    ALTER TABLE dbo.Users
        ADD CONSTRAINT FK_Users_Companies_CompanyId
        FOREIGN KEY (CompanyId) REFERENCES dbo.Companies (CompanyId);
END;
GO
