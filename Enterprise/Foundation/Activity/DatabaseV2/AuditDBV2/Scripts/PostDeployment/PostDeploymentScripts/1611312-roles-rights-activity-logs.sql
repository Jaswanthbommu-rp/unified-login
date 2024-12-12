
DECLARE @LogCategoryTypeId int;

IF NOT EXISTS
(
    SELECT 1
    FROM [Logging].[LogCategoryType] WHERE Name = 'RolesRights'
)
BEGIN
    INSERT INTO [Logging].[LogCategoryType]( Name )
    VALUES( 'RolesRights' );
    SELECT @LogcategoryTypeId = SCOPE_IDENTITY();
END;
ELSE
BEGIN
    SELECT @LogCategoryTypeId=  LogcategoryTypeId
    FROM [Logging].[LogCategoryType]
    WHERE Name = 'RolesRights';
END;

IF NOT EXISTS
(
    SELECT 1
    FROM [Logging].[LogType]
    WHERE LogCategoryTypeId = @LogCategoryTypeId AND 
          Name = 'Roles and Rights'
)
BEGIN
    DECLARE @logId INT;
	SELECT @logId = MAX(LogTypeId) + 1
	FROM Logging.LogType

	INSERT INTO Logging.LogType (LogTypeId, LogcategoryTypeId, Name, Description)
	VALUES (@logId, @LogCategoryTypeId, 'Roles and Rights', 'Roles and Rights activities')
END;

GO