
-- =============================================
-- Author:		Monte Jennings
-- Create date: 
-- Description: Gets the record with the indicated ID from the MinMaxType table.
-- =============================================
CREATE PROCEDURE [CustomField].GetMinMaxType (
	 @MinMaxTypeId TINYINT) 

 AS 

	SELECT
		 [MinMaxTypeId]
		,[MinMaxTypeName]
		,[CreatedDate]
		,[CreatedBy]
	FROM
		[CustomField].[MinMaxType]
	WHERE
		[MinMaxTypeId] = @MinMaxTypeId