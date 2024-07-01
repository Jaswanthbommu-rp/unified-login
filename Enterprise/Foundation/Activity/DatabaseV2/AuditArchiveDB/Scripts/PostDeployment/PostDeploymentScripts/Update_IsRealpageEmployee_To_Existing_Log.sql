use AuditArchiveDB
GO

IF EXISTS(SELECT TOP 1 1 FROM Logging.Activity WHERE IsRealPageEmployee IS NULL)
BEGIN

	UPDATE Logging.Activity SET IsRealPageEmployee = 0 WHERE IsRealPageEmployee IS NULL 
END