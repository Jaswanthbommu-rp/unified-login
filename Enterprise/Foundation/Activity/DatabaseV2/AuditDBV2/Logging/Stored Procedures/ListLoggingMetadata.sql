
CREATE PROCEDURE [Logging].[ListLoggingMetadata] 	
(
	 @LogCategoryTypeIds NVARCHAR(500) = NULL 
)
AS
BEGIN 
SET NOCOUNT ON;

	SELECT 
		LogCategoryTypeId
		,[Name]
		,[Description]
	FROM   
		Logging.LogCategoryType
	WHERE
		(@LogCategoryTypeIds IS NULL 
		OR 
		LogCategoryTypeId IN (SELECT [Value] FROM STRING_SPLIT(@LogCategoryTypeIds,',')))

	SELECT 
		LogTypeId
		,LogcategoryTypeId
		,[Name]
		,[Description]
	FROM 
		Logging.LogType
	WHERE 
		(@LogCategoryTypeIds IS NULL 
		OR 
		LogCategoryTypeId IN (SELECT [Value] FROM STRING_SPLIT(@LogCategoryTypeIds,',')))
	
END
