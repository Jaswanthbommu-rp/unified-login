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
		  Name = 'Change security questions failure'
)
BEGIN
	INSERT INTO [Logging].[LogType]( LogCategoryTypeId, Name )
	VALUES( @LogCategoryTypeId, 'Change security questions failure' );
END;
