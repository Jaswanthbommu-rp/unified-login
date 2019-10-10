
-- =============================================
-- Author:		Monte Jennings
-- Create date: 
-- Description: Gets the record with the indicated ID from the FieldValue table.
-- =============================================
CREATE PROCEDURE [CustomField].[GetFieldValue] (
	 @FieldValueId BIGINT) 

 AS 

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
		[FieldValueId] = @FieldValueId