go

DECLARE @LogCategoryTypeId int;

IF NOT EXISTS
(
    SELECT 1
    FROM [Logging].[LogCategoryType] WHERE Name = 'Security'
)
BEGIN
    INSERT INTO [Logging].[LogCategoryType]( Name )
    VALUES( 'Security' );
    SELECT @LogcategoryTypeId = SCOPE_IDENTITY();
END;
ELSE
BEGIN
    SELECT @LogCategoryTypeId=  LogcategoryTypeId
    FROM [Logging].[LogCategoryType]
    WHERE Name = 'Security';
END;

IF NOT EXISTS
(
    SELECT 1
    FROM [Logging].[LogType]
    WHERE LogCategoryTypeId = @LogCategoryTypeId AND 
          Name = 'Change security questions success'
)
BEGIN
    INSERT INTO [Logging].[LogType]( LogCategoryTypeId, Name )
    VALUES( @LogCategoryTypeId, 'Change security questions success' );
END;

IF NOT EXISTS
(
    SELECT 1
    FROM [Logging].[LogType]
    WHERE LogCategoryTypeId = @LogCategoryTypeId AND 
          Name = 'Change security questions failurer'
)
BEGIN
    INSERT INTO [Logging].[LogType]( LogCategoryTypeId, Name )
    VALUES( @LogCategoryTypeId, 'Change security questions failure' );
END;

GO

IF EXISTS (SELECT 1 FROM [Logging].[LogType] WHERE Name = 'Import User')
BEGIN
    UPDATE Logging.LogType SET NAME = 'Refresh User Data'
        WHERE Name = 'Import User'
END
GO
DECLARE @LogCategoryTypeId INT
SELECT @LogCategoryTypeId = LogCategoryTypeId FROM Logging.LogCategoryType
    WHERE Name = 'Migration'

SELECT * FROM Logging.Logtype WHERE LogCategoryTypeId = @LogCategoryTypeId
IF NOT EXISTS(SELECT 1 FROM Logging.Logtype WHERE Name = 'Hide Product User')
BEGIN
    INSERT INTO Logging.Logtype (LogCategoryTypeId, Name)
        VALUES (@LogCategoryTypeId, 'Hide Product User')
END
SELECT * FROM Logging.Logtype WHERE LogCategoryTypeId = @LogCategoryTypeId
IF NOT EXISTS(SELECT 1 FROM Logging.Logtype WHERE Name = 'Unhide Product User')
BEGIN
    INSERT INTO Logging.Logtype (LogCategoryTypeId, Name)
        VALUES (@LogCategoryTypeId, 'Unhide Product User')
END
SELECT * FROM Logging.Logtype WHERE LogCategoryTypeId = @LogCategoryTypeId
IF NOT EXISTS(SELECT 1 FROM Logging.Logtype WHERE Name = 'Deactivate User')
BEGIN
    INSERT INTO Logging.Logtype (LogCategoryTypeId, Name)
        VALUES (@LogCategoryTypeId, 'Deactivate User')
END
GO
DECLARE @LogCategoryTypeId INT
SELECT @LogCategoryTypeId = LogCategoryTypeId FROM Logging.LogCategoryType
    WHERE Name = 'Migration'


IF NOT EXISTS(SELECT 1 FROM Logging.Logtype WHERE Name = 'Refresh User Data Count')
BEGIN
    UPDATE Logging.Logtype SET Name =  'Refresh User Data' WHERE Name = 'Refresh User Data Count'
END


SELECT * FROM Logging.Logtype WHERE LogCategoryTypeId = @LogCategoryTypeId
GO


UPDATE L
SET L.Name = 'Updated Setting'
 FROM logging.logtype L
    Inner join logging.logcategorytype LC
    ON LC.LogcategoryTypeId = L.LogcategoryTypeId
WHERE L.Name = 'Settings' AND LC.Name = 'Settings'

GO

DECLARE @LogCategoryTypeId int;

IF NOT EXISTS
(
    SELECT 1
    FROM [Logging].[LogCategoryType] WHERE Name = 'Settings'
)
BEGIN
    INSERT INTO [Logging].[LogCategoryType]( Name )
    VALUES( 'Settings' );
    SELECT @LogcategoryTypeId = SCOPE_IDENTITY();
END;
ELSE
BEGIN
    SELECT @LogCategoryTypeId=  LogcategoryTypeId
    FROM [Logging].[LogCategoryType]
    WHERE Name = 'Settings';
END;

IF NOT EXISTS
(
    SELECT 1
    FROM [Logging].[LogType]
    WHERE LogCategoryTypeId = @LogCategoryTypeId AND 
          Name = 'Enabled setting'
)
BEGIN
    INSERT INTO [Logging].[LogType]( LogCategoryTypeId, Name )
    VALUES( @LogCategoryTypeId, 'Enabled setting' );
END;

IF NOT EXISTS
(
    SELECT 1
    FROM [Logging].[LogType]
    WHERE LogCategoryTypeId = @LogCategoryTypeId AND 
          Name = 'Disabled setting'
)
BEGIN
    INSERT INTO [Logging].[LogType]( LogCategoryTypeId, Name )
    VALUES( @LogCategoryTypeId, 'Disabled setting' );
END;

go
