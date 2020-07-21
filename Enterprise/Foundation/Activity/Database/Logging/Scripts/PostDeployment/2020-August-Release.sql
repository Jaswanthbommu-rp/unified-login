--START New log types for CIMPL
DECLARE @LogCategoryTypeId INT

IF NOT EXISTS (SELECT TOP 1 1 FROM logging.logcategorytype WHERE [name] = 'CIMPL')
BEGIN
	INSERT INTO [Logging].[LogCategoryType]	VALUES ('CIMPL',NULL)
	SET @LogCategoryTypeId = SCOPE_IDENTITY();
	
	INSERT INTO [Logging].[LogType]	VALUES (@LogCategoryTypeId,'Answered Question(s)',NULL)

	INSERT INTO [Logging].[LogType]	VALUES (@LogCategoryTypeId,'Submitted Questionnaire',NULL)

	INSERT INTO [Logging].[LogType]	VALUES (@LogCategoryTypeId,'Created Template',NULL)

	INSERT INTO [Logging].[LogType]	VALUES (@LogCategoryTypeId,'Modified Template',NULL)

	INSERT INTO [Logging].[LogType]	VALUES (@LogCategoryTypeId,'Applied Template',NULL)

	INSERT INTO [Logging].[LogType]	VALUES (@LogCategoryTypeId,'Removed Template',NULL)
END
ELSE
BEGIN
	SELECT @LogCategoryTypeId = LogCategoryTypeId FROM logging.logcategorytype	WHERE [name] = 'CIMPL'

	IF NOT EXISTS (SELECT TOP 1 1 FROM logging.logtype lt INNER JOIN logging.logcategorytype lct ON lt.LogCategoryTypeId = lct.LogCategoryTypeId AND lt.[name] = 'Answered Question(s)')
	BEGIN
		INSERT INTO [Logging].[LogType] VALUES (@LogCategoryTypeId,'Answered Question(s)',NULL)
	END

	IF NOT EXISTS (SELECT TOP 1 1 FROM logging.logtype lt INNER JOIN logging.logcategorytype lct ON lt.LogCategoryTypeId = lct.LogCategoryTypeId AND lt.[name] = 'Submitted Questionnaire')
	BEGIN
		INSERT INTO [Logging].[LogType] VALUES (@LogCategoryTypeId,'Submitted Questionnaire',NULL)
	END

	IF NOT EXISTS (SELECT TOP 1 1 FROM logging.logtype lt INNER JOIN logging.logcategorytype lct ON lt.LogCategoryTypeId = lct.LogCategoryTypeId AND lt.[name] = 'Created Template')
	BEGIN
		INSERT INTO [Logging].[LogType] VALUES (@LogCategoryTypeId,'Created Template',NULL)
	END

	IF NOT EXISTS (SELECT TOP 1 1 FROM logging.logtype lt INNER JOIN logging.logcategorytype lct ON lt.LogCategoryTypeId = lct.LogCategoryTypeId AND lt.[name] = 'Modified Template')
	BEGIN
		INSERT INTO [Logging].[LogType] VALUES (@LogCategoryTypeId,'Modified Template',NULL)
	END

	IF NOT EXISTS (SELECT TOP 1 1 FROM logging.logtype lt INNER JOIN logging.logcategorytype lct ON lt.LogCategoryTypeId = lct.LogCategoryTypeId AND lt.[name] = 'Applied Template')
	BEGIN
		INSERT INTO [Logging].[LogType] VALUES (@LogCategoryTypeId,'Applied Template',NULL)
	END

	IF NOT EXISTS (SELECT TOP 1 1 FROM logging.logtype lt INNER JOIN logging.logcategorytype lct ON lt.LogCategoryTypeId = lct.LogCategoryTypeId AND lt.[name] = 'Removed Template')
	BEGIN
		INSERT INTO [Logging].[LogType] VALUES (@LogCategoryTypeId,'Removed Template',NULL)
	END
END
--END New log types for CIMPL