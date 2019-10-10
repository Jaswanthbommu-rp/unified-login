
-- =============================================
-- Author:		Monte Jennings
-- Create date: 
-- Description: Deletes the record with the indicated ID from the FieldValue table.
-- =============================================
CREATE PROCEDURE [CustomField].[DeleteFieldValue] (
	 @FieldValueId BIGINT = NULL 
	,@UserLoginPersonaId BIGINT = NULL 
	,@FieldID BIGINT = NULL 
	,@Value NVARCHAR(MAX) = NULL 
	,@CreatedDate DATETIME = NULL 
	,@CreatedBy NVARCHAR(650) = NULL 
)

AS
BEGIN


	UPDATE
		[CustomField].[FieldValue]--Captures the current record for auditing purposes
		SET
		 CreatedBy = @CreatedBy
		,CreatedDate = @CreatedDate
	WHERE
		(@FieldValueId IS NULL  OR  [FieldValueId] = @FieldValueId)
	AND
		(@UserLoginPersonaId IS NULL  OR  [UserLoginPersonaId] = @UserLoginPersonaId)
	AND
		(@FieldID IS NULL  OR  [FieldID] = @FieldID)
	AND
		(@Value IS NULL  OR  [Value] = @Value)

	DELETE 
		[CustomField].[FieldValue]

	WHERE
		(@FieldValueId IS NULL  OR  [FieldValueId] = @FieldValueId)
	AND
		(@UserLoginPersonaId IS NULL  OR  [UserLoginPersonaId] = @UserLoginPersonaId)
	AND
		(@FieldID IS NULL  OR  [FieldID] = @FieldID)
	AND
		(@Value IS NULL  OR  [Value] = @Value)

	RETURN 1 --for success

END

