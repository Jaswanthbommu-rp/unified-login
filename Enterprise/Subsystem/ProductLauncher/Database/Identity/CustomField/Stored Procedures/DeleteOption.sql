
-- =============================================
-- Author:		Monte Jennings
-- Create date: 
-- Description: Deletes the record with the indicated ID from the Option table.
-- =============================================
CREATE PROCEDURE [CustomField].DeleteOption (
	 @OptionId BIGINT = NULL 
	,@FieldId BIGINT = NULL 
	,@Name NVARCHAR(2048) = NULL 
	,@CreatedDate DATETIME = NULL 
	,@CreatedBy NVARCHAR(650) = NULL 
)

AS
BEGIN


	UPDATE
		[CustomField].[Option]--Captures the current record for auditing purposes
		SET
		 CreatedBy = @CreatedBy
		,CreatedDate = @CreatedDate
	WHERE
		(@OptionId IS NULL  OR  [OptionId] = @OptionId)
	AND
		(@FieldId IS NULL  OR  [FieldId] = @FieldId)
	AND
		(@Name IS NULL  OR  [Name] = @Name)

	DELETE 
		[CustomField].[Option]

	WHERE
		(@OptionId IS NULL  OR  [OptionId] = @OptionId)
	AND
		(@FieldId IS NULL  OR  [FieldId] = @FieldId)
	AND
		(@Name IS NULL  OR  [Name] = @Name)

	RETURN 1 --for success

END

