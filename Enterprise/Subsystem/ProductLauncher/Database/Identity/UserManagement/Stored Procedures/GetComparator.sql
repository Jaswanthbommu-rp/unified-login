
-- =============================================
-- Author:		Monte Jennings
-- Create date: 
-- Description: Gets the record with the indicated ID from the Comparator table.
-- =============================================
CREATE PROCEDURE [UserManagement].GetComparator (
	 @ComparatorId INT) 

 AS 

	SELECT
		 [ComparatorId]
		,[Name]
		,[CreatedDate]
		,[CreatedBy]
	FROM
		[UserManagement].[Comparator]
	WHERE
		[ComparatorId] = @ComparatorId