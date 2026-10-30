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

-- Keep CompanyId 1 as the default company for all existing employees.
IF NOT EXISTS (SELECT 1 FROM dbo.Companies WHERE CompanyId = 1)
BEGIN
    IF EXISTS (SELECT 1 FROM dbo.Companies WHERE CompanyName = N'Company')
    BEGIN
        THROW 50001, 'A company named Company already exists with an ID other than 1.', 1;
    END;

    SET IDENTITY_INSERT dbo.Companies ON;
    INSERT INTO dbo.Companies (CompanyId, CompanyName)
    VALUES (1, N'Company');
    SET IDENTITY_INSERT dbo.Companies OFF;
END;
ELSE IF NOT EXISTS
(
    SELECT 1
    FROM dbo.Companies
    WHERE CompanyName = N'Company'
      AND CompanyId <> 1
)
BEGIN
    UPDATE dbo.Companies
    SET CompanyName = N'Company'
    WHERE CompanyId = 1;
END;
GO

IF COL_LENGTH(N'dbo.Users', N'CompanyId') IS NULL
BEGIN
    ALTER TABLE dbo.Users ADD CompanyId INT NULL;
END;
GO

-- Assign the default company to every existing employee without a company.
UPDATE dbo.Users
SET CompanyId = 1
WHERE CompanyId IS NULL;
GO

IF NOT EXISTS
(
    SELECT 1
    FROM sys.indexes
    WHERE object_id = OBJECT_ID(N'dbo.Users')
      AND name = N'IX_Users_CompanyId'
)
BEGIN
    CREATE INDEX IX_Users_CompanyId
        ON dbo.Users (CompanyId);
END;
GO

IF NOT EXISTS
(
    SELECT 1
    FROM sys.foreign_keys
    WHERE parent_object_id = OBJECT_ID(N'dbo.Users')
      AND name = N'FK_Users_Companies_CompanyId'
)
BEGIN
    ALTER TABLE dbo.Users
        ADD CONSTRAINT FK_Users_Companies_CompanyId
        FOREIGN KEY (CompanyId) REFERENCES dbo.Companies (CompanyId);
END;
GO
