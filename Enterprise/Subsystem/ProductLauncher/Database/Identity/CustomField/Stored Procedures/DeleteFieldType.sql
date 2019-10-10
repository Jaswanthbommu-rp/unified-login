
-- =============================================
-- Author:		Monte Jennings
-- Create date: 
-- Description: Deletes the record with the indicated ID from the FieldType table.
-- =============================================
CREATE PROCEDURE [CustomField].DeleteFieldType (
	 @FieldTypeId TINYINT = NULL 
	,@Name VARCHAR(100) = NULL 
	,@Description VARCHAR(500) = NULL 
	,@CreatedDate DATETIME = NULL 
	,@CreatedBy NVARCHAR(650) = NULL 
)

AS
BEGIN


	UPDATE
		[CustomField].[FieldType]--Captures the current record for auditing purposes
		SET
		 CreatedBy = @CreatedBy
		,CreatedDate = @CreatedDate
	WHERE
		(@FieldTypeId IS NULL  OR  [FieldTypeId] = @FieldTypeId)
	AND
		(@Name IS NULL  OR  [Name] = @Name)
	AND
		(@Description IS NULL  OR  [Description] = @Description)

	DELETE 
		[CustomField].[FieldType]

	WHERE
		(@FieldTypeId IS NULL  OR  [FieldTypeId] = @FieldTypeId)
	AND
		(@Name IS NULL  OR  [Name] = @Name)
	AND
		(@Description IS NULL  OR  [Description] = @Description)

	RETURN 1 --for success

END

