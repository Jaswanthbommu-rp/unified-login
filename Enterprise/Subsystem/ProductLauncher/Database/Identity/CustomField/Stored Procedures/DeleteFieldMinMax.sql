
-- =============================================
-- Author:		Monte Jennings
-- Create date: 
-- Description: Deletes the record with the indicated ID from the FieldMinMax table.
-- =============================================
CREATE PROCEDURE [CustomField].DeleteFieldMinMax (
	 @FieldId BIGINT = NULL 
	,@MinMaxTypeId TINYINT = NULL 
	,@Minimum INT = NULL 
	,@Maximum INT = NULL 
	,@CreatedBy NVARCHAR(650) = NULL 
	,@CreatedDate DATETIME = NULL 
)

AS
BEGIN


	UPDATE
		[CustomField].[FieldMinMax]--Captures the current record for auditing purposes
		SET
		 CreatedBy = @CreatedBy
		,CreatedDate = @CreatedDate
	WHERE
		(@FieldId IS NULL  OR  [FieldId] = @FieldId)
	AND
		(@MinMaxTypeId IS NULL  OR  [MinMaxTypeId] = @MinMaxTypeId)
	AND
		(@Minimum IS NULL  OR  [Minimum] = @Minimum)
	AND
		(@Maximum IS NULL  OR  [Maximum] = @Maximum)

	DELETE 
		[CustomField].[FieldMinMax]

	WHERE
		(@FieldId IS NULL  OR  [FieldId] = @FieldId)
	AND
		(@MinMaxTypeId IS NULL  OR  [MinMaxTypeId] = @MinMaxTypeId)
	AND
		(@Minimum IS NULL  OR  [Minimum] = @Minimum)
	AND
		(@Maximum IS NULL  OR  [Maximum] = @Maximum)

	RETURN 1 --for success

END

