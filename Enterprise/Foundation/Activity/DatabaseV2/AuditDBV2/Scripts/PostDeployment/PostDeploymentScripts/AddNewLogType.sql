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

IF NOT EXISTS (SELECT TOP 1 1 FROM Logging.LogCategoryType WHERE Name = 'Internal Product Settings')
BEGIN
	Declare @catId int;

	SELECT @catId = MAX(LogCategoryTypeId) + 1
	FROM Logging.LogCategoryType

	Insert into Logging.LogCategoryType (LogCategoryTypeId, Name, Description)
	values (@catId, 'Internal Product Settings','Internal Product Settings.')
END

GO

IF NOT EXISTS(SELECT TOP 1 1 FROM Logging.LogType WHERE NAME = 'Product Settings Update')
BEGIN
	DECLARE @catId INT;
	DECLARE @logId INT;
	
	SELECT @catId = LogCategoryTypeId
	FROM logging.LogCategoryType 
	WHERE NAME = 'Internal Product Settings'
	
	SELECT @logId = MAX(LogTypeId) + 1
	FROM Logging.LogType
	
	SELECT @catId, @logId
	
	INSERT INTO Logging.LogType (LogTypeId, LogcategoryTypeId, Name, Description)
	VALUES (@logId, @catId, 'Product Settings Update', 'Product Settings Update')
	
End

GO

IF NOT EXISTS(SELECT TOP 1 1 FROM Logging.LogType WHERE NAME = 'Import Product')
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
	VALUES (@logId, @catId, 'Import Product', 'Import Product')
	
End

GO

IF NOT EXISTS(SELECT TOP 1 1 FROM Logging.LogType WHERE NAME = 'Product Pages Update')
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
	VALUES (@logId, @catId, 'Product Pages Update', 'Product Pages Update')
	
End

GO

IF NOT EXISTS(SELECT TOP 1 1 FROM Logging.LogType WHERE NAME = 'Product Details Update')
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
	VALUES (@logId, @catId, 'Product Details Update', 'Product Details Update')
	
End

GO


-- 1011098

IF NOT EXISTS ( SELECT TOP (1) 1 FROM Logging.LogType lt 
						INNER JOIN logging.LogCategoryType lct ON lct.LogCategoryTypeId = lt.LogcategoryTypeId 
						WHERE lct.Name = 'InternalProductSettings' AND lt.Name = 'Client Settings Update' )
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
	VALUES (@logId, @catId, 'Client Settings Update', 'Client Settings actvities, such as add/update clients, client urls, etc.')						
END

-- 1011098
GO
