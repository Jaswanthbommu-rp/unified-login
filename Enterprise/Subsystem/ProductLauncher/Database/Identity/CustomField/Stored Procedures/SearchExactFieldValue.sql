
-- =============================================
-- Author:		Monte Jennings
-- Create date: 
-- Description: Searches the FieldValue table for the record with the indicated criteria.
-- =============================================
CREATE PROCEDURE [CustomField].[SearchExactFieldValue] (
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
		,[UserLoginPersonaId]
		,[FieldID]
		,[Value]
		,[CreatedDate]
		,[CreatedBy]
	FROM
		[CustomField].[FieldValue]
	WHERE 
		(@FieldValueId IS NULL  OR  [FieldValueId] = @FieldValueId)
	AND
		(@UserLoginPersonaId IS NULL  OR  [UserLoginPersonaId] = @UserLoginPersonaId)
	AND
		(@FieldID IS NULL  OR  [FieldID] = @FieldID)
	AND
		(@Value IS NULL  OR  [Value] = @Value)
	AND
		(@CreatedDate IS NULL  OR  [CreatedDate] = @CreatedDate)
	AND
		(@CreatedBy IS NULL  OR  [CreatedBy] = @CreatedBy)

END