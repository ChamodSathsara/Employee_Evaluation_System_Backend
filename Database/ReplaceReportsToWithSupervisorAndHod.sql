IF COL_LENGTH(N'dbo.Users', N'ReportedSupervisorId') IS NULL
BEGIN
    ALTER TABLE dbo.Users ADD ReportedSupervisorId INT NULL;
END;
GO

IF COL_LENGTH(N'dbo.Users', N'ReportedHodId') IS NULL
BEGIN
    ALTER TABLE dbo.Users ADD ReportedHodId INT NULL;
END;
GO

-- Migrate every existing ReportsTo value to ReportedHodId as requested.
IF COL_LENGTH(N'dbo.Users', N'ReportsTo') IS NOT NULL
BEGIN
    UPDATE dbo.Users
    SET ReportedHodId = ReportsTo
    WHERE ReportedHodId IS NULL
      AND ReportsTo IS NOT NULL;
END;
GO

IF NOT EXISTS
(
    SELECT 1 FROM sys.indexes
    WHERE object_id = OBJECT_ID(N'dbo.Users')
      AND name = N'IX_Users_ReportedSupervisorId'
)
BEGIN
    CREATE INDEX IX_Users_ReportedSupervisorId
        ON dbo.Users (ReportedSupervisorId);
END;
GO

IF NOT EXISTS
(
    SELECT 1 FROM sys.indexes
    WHERE object_id = OBJECT_ID(N'dbo.Users')
      AND name = N'IX_Users_ReportedHodId'
)
BEGIN
    CREATE INDEX IX_Users_ReportedHodId
        ON dbo.Users (ReportedHodId);
END;
GO

IF NOT EXISTS
(
    SELECT 1 FROM sys.foreign_keys
    WHERE parent_object_id = OBJECT_ID(N'dbo.Users')
      AND name = N'FK_Users_Users_ReportedSupervisorId'
)
BEGIN
    ALTER TABLE dbo.Users
        ADD CONSTRAINT FK_Users_Users_ReportedSupervisorId
        FOREIGN KEY (ReportedSupervisorId) REFERENCES dbo.Users (UserId);
END;
GO

IF NOT EXISTS
(
    SELECT 1 FROM sys.foreign_keys
    WHERE parent_object_id = OBJECT_ID(N'dbo.Users')
      AND name = N'FK_Users_Users_ReportedHodId'
)
BEGIN
    ALTER TABLE dbo.Users
        ADD CONSTRAINT FK_Users_Users_ReportedHodId
        FOREIGN KEY (ReportedHodId) REFERENCES dbo.Users (UserId);
END;
GO

-- Keep the old ReportsTo column temporarily for rollback safety. After the new
-- application has been verified, it can be removed in a separate migration.
