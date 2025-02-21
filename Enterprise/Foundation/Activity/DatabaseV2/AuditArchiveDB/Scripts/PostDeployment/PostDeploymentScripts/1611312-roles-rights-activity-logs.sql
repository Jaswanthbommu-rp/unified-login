IF NOT EXISTS (SELECT TOP 1 1 FROM Logging.LogCategoryType WHERE Name = 'RolesRights')
BEGIN
	Declare @catId int;
 
	SELECT @catId = MAX(LogCategoryTypeId) + 1
	FROM Logging.LogCategoryType
 
	Insert into Logging.LogCategoryType (LogCategoryTypeId, Name, Description)
	values(@catId, 'InternalProductSettings','Internal Product Settings.')
END

IF NOT EXISTS
(
    SELECT 1
    FROM [Logging].[LogType]
    WHERE  Name = 'Roles and Rights'
)
BEGIN
    DECLARE @catId INT;
    DECLARE @logId INT;
	SELECT @catId = LogCategoryTypeId
	FROM logging.LogCategoryType 
	WHERE NAME = 'RolesRights'
	SELECT @logId = MAX(LogTypeId) + 1
	FROM Logging.LogType

	INSERT INTO Logging.LogType (LogTypeId, LogcategoryTypeId, Name, Description)
	VALUES (@logId, @catId, 'Roles and Rights', 'Roles and Rights activities')
END;

GO