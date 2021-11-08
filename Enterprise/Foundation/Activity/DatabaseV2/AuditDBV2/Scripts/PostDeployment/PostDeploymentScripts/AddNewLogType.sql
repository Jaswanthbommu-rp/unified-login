USE AuditDBV2
GO

IF NOT EXISTS(SELECT TOP 1 1 FROM Logging.LogType WHERE NAME = 'User Update - Internal')
BEGIN
	DECLARE @catId INT;
	DECLARE @logId INT;
	
	SELECT @catId = LogCategoryTypeId
	FROM logging.LogCategoryType 
	WHERE NAME = 'user'
	
	SELECT @logId = MAX(LogTypeId) + 1
	FROM Logging.LogType
	
	SELECT @catId, @logId
	
	INSERT INTO Logging.LogType (LogTypeId, LogcategoryTypeId, Name, Description)
	VALUES (@logId, @catId, 'User Update - Internal', 'User Update - Internal')
	
End


GO

IF NOT EXISTS (SELECT TOP 1 1 FROM Logging.LogCategoryType WHERE Name = 'InternalProductSettings')
BEGIN
	Declare @catId int;

	SELECT @catId = MAX(LogCategoryTypeId) + 1
	FROM Logging.LogCategoryType

	Insert into Logging.LogCategoryType (LogCategoryTypeId, Name, Description)
	values (@catId, 'InternalProductSettings','Internal Product Settings.')
END

GO

IF NOT EXISTS(SELECT TOP 1 1 FROM Logging.LogType WHERE NAME = 'Product Settings Update')
BEGIN
	DECLARE @catId INT;
	DECLARE @logId INT;
	
	SELECT @catId = LogCategoryTypeId
	FROM logging.LogCategoryType 
	WHERE NAME = 'InternalProductSettings'
	
	SELECT @logId = MAX(LogTypeId) + 1
	FROM Logging.LogType
	
	SELECT @catId, @logId
	
	INSERT INTO Logging.LogType (LogTypeId, LogcategoryTypeId, Name, Description)
	VALUES (@logId, @catId, 'Product Settings Update', 'Product Settings Update')
	
End