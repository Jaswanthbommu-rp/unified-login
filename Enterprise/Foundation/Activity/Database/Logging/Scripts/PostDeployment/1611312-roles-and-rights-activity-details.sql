
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
    INSERT INTO [Logging].[LogType]( LogCategoryTypeId, Name )
    VALUES( @LogCategoryTypeId, 'Roles and Rights' );
END;

GO