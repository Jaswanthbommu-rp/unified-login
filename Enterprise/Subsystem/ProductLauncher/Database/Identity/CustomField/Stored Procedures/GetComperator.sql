
-- =============================================
-- Author:		Monte Jennings
-- Create date: 
-- Description: Gets the record with the indicated ID from the Comperator table.
-- =============================================
CREATE PROCEDURE [CustomField].GetComperator (
	 @ComperatorId TINYINT) 

 AS 

	SELECT
		 [ComperatorId]
		,[Name]
		,[CreatedDate]
		,[CreatedBy]
	FROM
		[CustomField].[Comperator]
	WHERE
		[ComperatorId] = @ComperatorId