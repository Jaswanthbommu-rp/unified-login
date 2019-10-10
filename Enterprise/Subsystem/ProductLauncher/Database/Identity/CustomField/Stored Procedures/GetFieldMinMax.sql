
-- =============================================
-- Author:		Monte Jennings
-- Create date: 
-- Description: Gets the record with the indicated ID from the FieldMinMax table.
-- =============================================
CREATE PROCEDURE [CustomField].GetFieldMinMax (
	 @FieldId BIGINT) 

 AS 

	SELECT
		 [FieldId]
		,[MinMaxTypeId]
		,[Minimum]
		,[Maximum]
		,[CreatedBy]
		,[CreatedDate]
	FROM
		[CustomField].[FieldMinMax]
	WHERE
		[FieldId] = @FieldId