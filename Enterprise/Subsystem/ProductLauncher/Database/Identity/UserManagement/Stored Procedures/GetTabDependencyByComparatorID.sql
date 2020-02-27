
-- =============================================
-- Author:		Monte Jennings
-- Create date: 
-- Description: Gets the record with the indicated ID from the TabDependency table.
-- =============================================
CREATE PROCEDURE [UserManagement].GetTabDependencyByComparatorID (
	 @ComparatorID INT) 

 AS 

	SELECT
		 [TabDependencyId]
		,[ControlId]
		,[TabTypeId]
		,[ControlValue]
		,[ComparatorID]
		,[CreatedBy]
		,[CreatedDate]
	FROM
		[UserManagement].[TabDependency]
	WHERE
		[ComparatorID] = @ComparatorID