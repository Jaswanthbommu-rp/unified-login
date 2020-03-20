
-- =============================================
-- Author:		Monte Jennings
-- Create date: 
-- Description: Gets the record with the indicated ID from the Comparator table.
-- =============================================
CREATE PROCEDURE [Enterprise].GetComparator (
	 @ComparatorId INT) 

 AS 

	SELECT
		 [ComparatorId]
		,[Name]
		,[CreatedDate]
		,[CreatedBy]
	FROM
		[Enterprise].[Comparator]
	WHERE
		[ComparatorId] = @ComparatorId