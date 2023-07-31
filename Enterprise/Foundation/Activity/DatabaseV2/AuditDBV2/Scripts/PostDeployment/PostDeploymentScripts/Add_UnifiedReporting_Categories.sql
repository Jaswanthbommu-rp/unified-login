USE AuditDBV2
GO

--Reporting Log Category Types
IF NOT EXISTS (SELECT TOP 1 1 FROM Logging.LogCategoryType WHERE Name = 'Reporting')
BEGIN
	Declare @catId int;

	SELECT @catId = MAX(LogCategoryTypeId) + 1
	FROM Logging.LogCategoryType

	Insert into Logging.LogCategoryType (LogCategoryTypeId, Name, Description)
	values (@catId, 'Reporting', 'Unified Reporting')
END

GO

--Reporting LogTypes
IF NOT EXISTS(SELECT TOP 1 1 FROM Logging.LogType WHERE NAME = 'Available/Unavailable')
BEGIN
	DECLARE @catId INT;
	DECLARE @logId INT;
	
	SELECT @catId = LogCategoryTypeId
	FROM logging.LogCategoryType 
	WHERE NAME = 'Reporting'
	
	SELECT @logId = MAX(LogTypeId) + 1
	FROM Logging.LogType
	
	SELECT @catId, @logId
	
	INSERT INTO Logging.LogType (LogTypeId, LogcategoryTypeId, Name, Description)
	VALUES (@logId, @catId, N'Available/Unavailable', N'Report Availbility Change')
	
End

GO


IF NOT EXISTS(SELECT TOP 1 1 FROM Logging.LogType WHERE NAME = 'Generated/Scheduled')
BEGIN
	DECLARE @catId INT;
	DECLARE @logId INT;
	
	SELECT @catId = LogCategoryTypeId
	FROM logging.LogCategoryType 
	WHERE NAME = 'Reporting'
	
	SELECT @logId = MAX(LogTypeId) + 1
	FROM Logging.LogType
	
	SELECT @catId, @logId
	
	INSERT INTO Logging.LogType (LogTypeId, LogcategoryTypeId, Name, Description)
	VALUES (@logId, @catId, N'Generated/Scheduled', N'Report Generate')
	
End

GO

IF NOT EXISTS(SELECT TOP 1 1 FROM Logging.LogType WHERE NAME = 'Modified Parameters')
BEGIN
	DECLARE @catId INT;
	DECLARE @logId INT;
	
	SELECT @catId = LogCategoryTypeId
	FROM logging.LogCategoryType 
	WHERE NAME = 'Reporting'
	
	SELECT @logId = MAX(LogTypeId) + 1
	FROM Logging.LogType
	
	SELECT @catId, @logId
	
	INSERT INTO Logging.LogType (LogTypeId, LogcategoryTypeId, Name, Description)
	VALUES (@logId, @catId, N'Modified Parameters', N'Modified Parameters')
	
End

GO

IF NOT EXISTS(SELECT TOP 1 1 FROM Logging.LogType WHERE NAME = 'Downloaded')
BEGIN
	DECLARE @catId INT;
	DECLARE @logId INT;
	
	SELECT @catId = LogCategoryTypeId
	FROM logging.LogCategoryType 
	WHERE NAME = 'Reporting'
	
	SELECT @logId = MAX(LogTypeId) + 1
	FROM Logging.LogType
	
	SELECT @catId, @logId
	
	INSERT INTO Logging.LogType (LogTypeId, LogcategoryTypeId, Name, Description)
	VALUES (@logId, @catId, N'Downloaded', N'Downloaded Report')
	
End

GO


IF NOT EXISTS(SELECT TOP 1 1 FROM Logging.LogType WHERE NAME = 'Filed')
BEGIN
	DECLARE @catId INT;
	DECLARE @logId INT;
	
	SELECT @catId = LogCategoryTypeId
	FROM logging.LogCategoryType 
	WHERE NAME = 'Reporting'
	
	SELECT @logId = MAX(LogTypeId) + 1
	FROM Logging.LogType
	
	SELECT @catId, @logId
	
	INSERT INTO Logging.LogType (LogTypeId, LogcategoryTypeId, Name, Description)
	VALUES (@logId, @catId, N'Filed', N'Report Filed')
	
End

GO

IF NOT EXISTS(SELECT TOP 1 1 FROM Logging.LogType WHERE NAME = 'Sent by Email')
BEGIN
	DECLARE @catId INT;
	DECLARE @logId INT;
	
	SELECT @catId = LogCategoryTypeId
	FROM logging.LogCategoryType 
	WHERE NAME = 'Reporting'
	
	SELECT @logId = MAX(LogTypeId) + 1
	FROM Logging.LogType
	
	SELECT @catId, @logId
	
	INSERT INTO Logging.LogType (LogTypeId, LogcategoryTypeId, Name, Description)
	VALUES (@logId, @catId, N'Sent by Email', N'Sent by Email')
	
End

GO

IF NOT EXISTS(SELECT TOP 1 1 FROM Logging.LogType WHERE NAME = 'Sent by Notification')
BEGIN
	DECLARE @catId INT;
	DECLARE @logId INT;
	
	SELECT @catId = LogCategoryTypeId
	FROM logging.LogCategoryType 
	WHERE NAME = 'Reporting'
	
	SELECT @logId = MAX(LogTypeId) + 1
	FROM Logging.LogType
	
	SELECT @catId, @logId
	
	INSERT INTO Logging.LogType (LogTypeId, LogcategoryTypeId, Name, Description)
	VALUES (@logId, @catId, N'Sent by Notification', N'Sent by Notification')
	
End

GO

IF NOT EXISTS(SELECT TOP 1 1 FROM Logging.LogType WHERE NAME = 'Created Report Group')
BEGIN
	DECLARE @catId INT;
	DECLARE @logId INT;
	
	SELECT @catId = LogCategoryTypeId
	FROM logging.LogCategoryType 
	WHERE NAME = 'Reporting'
	
	SELECT @logId = MAX(LogTypeId) + 1
	FROM Logging.LogType
	
	SELECT @catId, @logId
	
	INSERT INTO Logging.LogType (LogTypeId, LogcategoryTypeId, Name, Description)
	VALUES (@logId, @catId, N'Created Report Group', N'Created Report Group')
	
End

GO

IF NOT EXISTS(SELECT TOP 1 1 FROM Logging.LogType WHERE NAME = 'Modified Report Group')
BEGIN
	DECLARE @catId INT;
	DECLARE @logId INT;
	
	SELECT @catId = LogCategoryTypeId
	FROM logging.LogCategoryType 
	WHERE NAME = 'Reporting'
	
	SELECT @logId = MAX(LogTypeId) + 1
	FROM Logging.LogType
	
	SELECT @catId, @logId
	
	INSERT INTO Logging.LogType (LogTypeId, LogcategoryTypeId, Name, Description)
	VALUES (@logId, @catId, N'Modified Report Group', N'Modified Report Group')
	
End

GO

IF NOT EXISTS(SELECT TOP 1 1 FROM Logging.LogType WHERE NAME = 'Deleted Report Group')
BEGIN
	DECLARE @catId INT;
	DECLARE @logId INT;
	
	SELECT @catId = LogCategoryTypeId
	FROM logging.LogCategoryType 
	WHERE NAME = 'Reporting'
	
	SELECT @logId = MAX(LogTypeId) + 1
	FROM Logging.LogType
	
	SELECT @catId, @logId
	
	INSERT INTO Logging.LogType (LogTypeId, LogcategoryTypeId, Name, Description)
	VALUES (@logId, @catId, N'Deleted Report Group', N'Deleted Report Group')
	
End

GO

IF NOT EXISTS(SELECT TOP 1 1 FROM Logging.LogType WHERE NAME = 'Added Report to Report Group')
BEGIN
	DECLARE @catId INT;
	DECLARE @logId INT;
	
	SELECT @catId = LogCategoryTypeId
	FROM logging.LogCategoryType 
	WHERE NAME = 'Reporting'
	
	SELECT @logId = MAX(LogTypeId) + 1
	FROM Logging.LogType
	
	SELECT @catId, @logId
	
	INSERT INTO Logging.LogType (LogTypeId, LogcategoryTypeId, Name, Description)
	VALUES (@logId, @catId, N'Added Report to Report Group', N'Added Report to Report Group')
	
End

GO

IF NOT EXISTS(SELECT TOP 1 1 FROM Logging.LogType WHERE NAME = 'Deleted Report from Report Group')
BEGIN
	DECLARE @catId INT;
	DECLARE @logId INT;
	
	SELECT @catId = LogCategoryTypeId
	FROM logging.LogCategoryType 
	WHERE NAME = 'Reporting'
	
	SELECT @logId = MAX(LogTypeId) + 1
	FROM Logging.LogType
	
	SELECT @catId, @logId
	
	INSERT INTO Logging.LogType (LogTypeId, LogcategoryTypeId, Name, Description)
	VALUES (@logId, @catId, N'Deleted Report from Report Group', N'Deleted Report from Report Group')
	
End

GO

IF NOT EXISTS(SELECT TOP 1 1 FROM Logging.LogType WHERE NAME = 'Canceled')
BEGIN
	DECLARE @catId INT;
	DECLARE @logId INT;
	
	SELECT @catId = LogCategoryTypeId
	FROM logging.LogCategoryType 
	WHERE NAME = 'Reporting'
	
	SELECT @logId = MAX(LogTypeId) + 1
	FROM Logging.LogType
	
	SELECT @catId, @logId
	
	INSERT INTO Logging.LogType (LogTypeId, LogcategoryTypeId, Name, Description)
	VALUES (@logId, @catId,  N'Canceled', N'Cancelled scheduled report')
	
End

GO