
-- =============================================
-- Author:		Monte Jennings
-- Create date: 
-- Description: Gets the record with the indicated ID from the TabDependency table.
-- =============================================
CREATE PROCEDURE [UserManagement].GetTabDependencyByTabTypeId (
	 @TabTypeId INT) 

 AS 

	SELECT
		 [TabDependencyId]
		,[ControlIdentifier]
		,[TabTypeId]
		,[ControlValue]
		,[ComparatorID]
		,[CreatedBy]
		,[CreatedDate]
	FROM
		[UserManagement].[TabDependency]
	WHERE
		[TabTypeId] = @TabTypeId