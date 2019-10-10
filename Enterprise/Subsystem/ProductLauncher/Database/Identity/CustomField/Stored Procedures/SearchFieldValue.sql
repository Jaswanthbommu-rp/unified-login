
-- =============================================
-- Author:		Monte Jennings
-- Create date: 
-- Description: Searches the FieldValue table for the record with the indicated criteria.
-- =============================================
CREATE PROCEDURE [CustomField].[SearchFieldValue] (
	 @FieldValueId BIGINT = NULL 
	,@UserLoginPersonaId BIGINT = NULL 
	,@FieldID BIGINT = NULL 
	,@Value NVARCHAR(MAX) = NULL 
	,@CreatedDate DATETIME = NULL 
	,@CreatedBy BIGINT = NULL 
)
AS
BEGIN
	SELECT
		 [FieldValueId]
		,[UserLoginPersonaID]
		,[FieldID]
		,[Value]
		,[CreatedDate]
		,[CreatedBy]
	FROM
		[CustomField].[FieldValue]
	WHERE 
		(@FieldValueId IS NULL  OR  [FieldValueId] = @FieldValueId)
	AND
		(@UserLoginPersonaId IS NULL  OR  [UserLoginPersonaID] = @UserLoginPersonaID)
	AND
		(@FieldID IS NULL  OR  [FieldID] = @FieldID)
	AND
		(@Value IS NULL OR [Value] = @Value OR CHARINDEX(@Value,[Value]) > 0)
	AND
		(@CreatedDate IS NULL  OR  [CreatedDate] = @CreatedDate)
	AND
		(@CreatedBy IS NULL OR [CreatedBy] = @CreatedBy)
END