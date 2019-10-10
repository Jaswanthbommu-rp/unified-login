
-- =============================================
-- Author:		Monte Jennings
-- Create date: 
-- Description: Searches the Option table for the record with the indicated criteria.
-- =============================================
CREATE PROCEDURE [CustomField].SearchExactOption (
	 @OptionId BIGINT = NULL 
	,@FieldId BIGINT = NULL 
	,@Name NVARCHAR(2048) = NULL 
	,@CreatedDate DATETIME = NULL 
	,@CreatedBy NVARCHAR(650) = NULL 
)

AS

BEGIN

	SELECT
		 [OptionId]
		,[FieldId]
		,[Name]
		,[CreatedDate]
		,[CreatedBy]
	FROM
		[CustomField].[Option]
	WHERE 
		(@OptionId IS NULL  OR  [OptionId] = @OptionId)
	AND
		(@FieldId IS NULL  OR  [FieldId] = @FieldId)
	AND
		(@Name IS NULL  OR  [Name] = @Name)
	AND
		(@CreatedDate IS NULL  OR  [CreatedDate] = @CreatedDate)
	AND
		(@CreatedBy IS NULL  OR  [CreatedBy] = @CreatedBy)

END