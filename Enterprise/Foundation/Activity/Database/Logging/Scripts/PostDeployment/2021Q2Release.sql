
--START New log types for SETTINGS

--UserStory: 775005

DECLARE @LogCategoryTypeId INT

--Company or Property
BEGIN

	IF NOT EXISTS (
			SELECT TOP 1 1
			FROM logging.logcategorytype
			WHERE [name] = 'Company or Property'
			)
	BEGIN

		INSERT INTO [Logging].[LogCategoryType]	VALUES ('Company or Property',NULL)  
		SET @LogCategoryTypeId = SCOPE_IDENTITY();
	
		INSERT INTO [Logging].[LogType]	VALUES (@LogCategoryTypeId,'Modified Setting',NULL)

		INSERT INTO [Logging].[LogType]	VALUES (@LogCategoryTypeId,'Selected Button',NULL)

		INSERT INTO [Logging].[LogType]	VALUES (@LogCategoryTypeId,'Added Table Row',NULL)

		INSERT INTO [Logging].[LogType]	VALUES (@LogCategoryTypeId,'Deleted Table Row',NULL)

		INSERT INTO [Logging].[LogType]	VALUES (@LogCategoryTypeId,'Modified Table Row',NULL)

		INSERT INTO [Logging].[LogType]	VALUES (@LogCategoryTypeId,'Selected Table Actions',NULL)

		INSERT INTO [Logging].[LogType]	VALUES (@LogCategoryTypeId,'Modified Table',NULL)

	END

END

--------------------------------------------------------------------
--Global Update
BEGIN

	IF NOT EXISTS (
			SELECT TOP 1 1
			FROM logging.logcategorytype
			WHERE [name] = 'Global Update'
			)
	BEGIN

		INSERT INTO [Logging].[LogCategoryType]	VALUES ('Global Update',NULL)  
		SET @LogCategoryTypeId = SCOPE_IDENTITY();
	
		INSERT INTO [Logging].[LogType]	VALUES (@LogCategoryTypeId,'Modified Setting',NULL)

	END
	

END

--------------------------------------------------------------------------

--Templates
BEGIN

IF NOT EXISTS (
		SELECT TOP 1 1
		FROM logging.logcategorytype
		WHERE [name] = 'Templates'
		)
BEGIN

	INSERT INTO [Logging].[LogCategoryType]	VALUES ('Templates',NULL)  
	SET @LogCategoryTypeId = SCOPE_IDENTITY();
	
	INSERT INTO [Logging].[LogType]	VALUES (@LogCategoryTypeId,'Added Template',NULL)

	INSERT INTO [Logging].[LogType]	VALUES (@LogCategoryTypeId,'Applied Template',NULL)

	INSERT INTO [Logging].[LogType]	VALUES (@LogCategoryTypeId,'Deleted Template',NULL)

	INSERT INTO [Logging].[LogType]	VALUES (@LogCategoryTypeId,'Modified Template',NULL)

	INSERT INTO [Logging].[LogType]	VALUES (@LogCategoryTypeId,'Modified Table',NULL)

	INSERT INTO [Logging].[LogType]	VALUES (@LogCategoryTypeId,'Added Table Row',NULL)

	INSERT INTO [Logging].[LogType]	VALUES (@LogCategoryTypeId,'Deleted Table Row',NULL)

	INSERT INTO [Logging].[LogType]	VALUES (@LogCategoryTypeId,'Modified Table Row',NULL)

	INSERT INTO [Logging].[LogType]	VALUES (@LogCategoryTypeId,'Selected Table Actions',NULL)

END

END

-------------------------------------------------------

--UserStory: 772341
--ADMIN Console

BEGIN

	IF NOT EXISTS (
			SELECT TOP 1 1
			FROM logging.logcategorytype
			WHERE [name] = 'Admin Console'
			)
	BEGIN

		INSERT INTO [Logging].[LogCategoryType]	VALUES ('Admin Console',NULL)  
		SET @LogCategoryTypeId = SCOPE_IDENTITY();
	
		INSERT INTO [Logging].[LogType]	VALUES (@LogCategoryTypeId,'Added Column',NULL)

		INSERT INTO [Logging].[LogType]	VALUES (@LogCategoryTypeId,'Added Page',NULL)

		INSERT INTO [Logging].[LogType]	VALUES (@LogCategoryTypeId,'Added Section',NULL)

		INSERT INTO [Logging].[LogType]	VALUES (@LogCategoryTypeId,'Added Setting',NULL)

		INSERT INTO [Logging].[LogType]	VALUES (@LogCategoryTypeId,'Added Tile',NULL)

		INSERT INTO [Logging].[LogType]	VALUES (@LogCategoryTypeId,'Deleted Column',NULL)

		INSERT INTO [Logging].[LogType]	VALUES (@LogCategoryTypeId,'Deleted Page',NULL)

		INSERT INTO [Logging].[LogType]	VALUES (@LogCategoryTypeId,'Deleted Section',NULL)

		INSERT INTO [Logging].[LogType]	VALUES (@LogCategoryTypeId,'Deleted Setting',NULL)

		INSERT INTO [Logging].[LogType]	VALUES (@LogCategoryTypeId,'Deleted Tile',NULL)
	
		INSERT INTO [Logging].[LogType]	VALUES (@LogCategoryTypeId,'Modified Column',NULL)

		INSERT INTO [Logging].[LogType]	VALUES (@LogCategoryTypeId,'Modified Page',NULL)

		INSERT INTO [Logging].[LogType]	VALUES (@LogCategoryTypeId,'Modified Section',NULL)

		INSERT INTO [Logging].[LogType]	VALUES (@LogCategoryTypeId,'Modified Setting',NULL)

		INSERT INTO [Logging].[LogType]	VALUES (@LogCategoryTypeId,'Modified Tile',NULL)


	END

END


--END New log types for SETTINGS