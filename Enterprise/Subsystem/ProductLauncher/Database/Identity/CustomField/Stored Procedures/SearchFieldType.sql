
-- =============================================
-- Author:		Monte Jennings
-- Create date: 
-- Description: Searches the FieldType table for the record with the indicated criteria.
-- =============================================
CREATE PROCEDURE [CustomField].[SearchFieldType] (
	 @FieldTypeId TINYINT = NULL 
	,@Name NVARCHAR(100) = NULL 
	,@Description NVARCHAR(500) = NULL 
	,@CreatedDate DATETIME = NULL 
	,@CreatedBy BIGINT = NULL 
)

AS

BEGIN

	SELECT
		 [FieldTypeId]
		,[Name]
		,[Description]
		,[CreatedDate]
		,[CreatedBy]
	FROM
		[CustomField].[FieldType]
	WHERE 
		(@FieldTypeId IS NULL  OR  [FieldTypeId] = @FieldTypeId)
	AND
		(@Name IS NULL OR [Name] = @Name OR CHARINDEX(@Name,[Name]) > 0)
	AND
		(@Description IS NULL OR [Description] = @Description OR CHARINDEX(@Description,[Description]) > 0)
	AND
		(@CreatedDate IS NULL  OR  [CreatedDate] = @CreatedDate)
	AND
		(@CreatedBy IS NULL OR [CreatedBy] = @CreatedBy)

END