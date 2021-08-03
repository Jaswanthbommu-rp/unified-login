

----New log types and categories for SETTINGS

DECLARE @LogCategoryTypeId INT

--Internal Settings
BEGIN

	IF NOT EXISTS (
			SELECT TOP 1 1
			FROM logging.logcategorytype
			WHERE [name] = 'Internal Settings'
			)
	BEGIN

		INSERT INTO [Logging].[LogCategoryType]	VALUES ('Internal Settings',NULL)  
		SET @LogCategoryTypeId = SCOPE_IDENTITY();
	
		INSERT INTO [Logging].[LogType]	VALUES (@LogCategoryTypeId,'Internal Enabled Setting',NULL)

		INSERT INTO [Logging].[LogType]	VALUES (@LogCategoryTypeId,'Internal Disabled Setting',NULL)

		INSERT INTO [Logging].[LogType]	VALUES (@LogCategoryTypeId,'Internal Update Settings',NULL)

	END
	

END


--Internal Templates
BEGIN

	IF NOT EXISTS (
			SELECT TOP 1 1
			FROM logging.logcategorytype
			WHERE [name] = 'Internal Templates'
			)
	BEGIN

		INSERT INTO [Logging].[LogCategoryType]	VALUES ('Internal Templates',NULL)  
		SET @LogCategoryTypeId = SCOPE_IDENTITY();
	
		INSERT INTO [Logging].[LogType]	VALUES (@LogCategoryTypeId,'Internal Added Template',NULL)

		INSERT INTO [Logging].[LogType]	VALUES (@LogCategoryTypeId,'Internal Applied Template',NULL)

		INSERT INTO [Logging].[LogType]	VALUES (@LogCategoryTypeId,'Internal Deleted Template',NULL)

		INSERT INTO [Logging].[LogType]	VALUES (@LogCategoryTypeId,'Internal Modified Template',NULL)

		INSERT INTO [Logging].[LogType]	VALUES (@LogCategoryTypeId,'Internal Modified Table',NULL)

		INSERT INTO [Logging].[LogType]	VALUES (@LogCategoryTypeId,'Internal Added Table Row',NULL)

		INSERT INTO [Logging].[LogType]	VALUES (@LogCategoryTypeId,'Internal Deleted Table Row',NULL)

		INSERT INTO [Logging].[LogType]	VALUES (@LogCategoryTypeId,'Internal Modified Table Row',NULL)

		INSERT INTO [Logging].[LogType]	VALUES (@LogCategoryTypeId,'Internal Selected Table Actions',NULL)

		INSERT INTO [Logging].[LogType]	VALUES (@LogCategoryTypeId,'Internal Template Error',NULL)

	END
	

END


--Templates
BEGIN

	IF NOT EXISTS (
			SELECT TOP 1 1
			FROM [Logging].[LogType]
			WHERE [name] = 'Template Error'
			)
	BEGIN

		SELECT @LogCategoryTypeId = LogcategoryTypeId 
		FROM logging.logcategorytype
		WHERE [Name] = 'Templates'

		INSERT INTO [Logging].[LogType]	VALUES (@LogCategoryTypeId,'Template Error',NULL)

	END

END


