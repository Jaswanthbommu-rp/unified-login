
-- =============================================
-- Author:		Monte Jennings
-- Create date: 
-- Description: Gets the record with the indicated ID from the Option table.
-- =============================================
CREATE PROCEDURE [CustomField].GetOption (
	 @OptionId BIGINT) 

 AS 

	SELECT
		 [OptionId]
		,[FieldId]
		,[Name]
		,[CreatedDate]
		,[CreatedBy]
	FROM
		[CustomField].[Option]
	WHERE
		[OptionId] = @OptionId