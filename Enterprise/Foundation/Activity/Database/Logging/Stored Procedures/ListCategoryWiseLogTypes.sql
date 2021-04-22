
CREATE PROCEDURE [Logging].[ListCategoryWiseLogTypes]
(
	 @LogCategoryTypeId INT 
)
AS
BEGIN
SET NOCOUNT ON 
 
	SELECT 
		LogTypeId
		,LogcategoryTypeId
		,[Name]
		,[Description]
	FROM 
		[Logging].[LogType] 
	WHERE
		LogCategoryTypeId  = @LogCategoryTypeId
	 
END