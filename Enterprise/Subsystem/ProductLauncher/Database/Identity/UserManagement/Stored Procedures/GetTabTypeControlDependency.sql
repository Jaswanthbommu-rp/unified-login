
-- =============================================
-- Author:		Monte Jennings
-- Create date: 
-- Description: Gets the record with the indicated ID from the TabTypeControlDependency table.
-- =============================================
CREATE PROCEDURE [UserManagement].GetTabTypeControlDependency (
	 @TabtypeControlDependencyId INT) 

 AS 

	SELECT
		 [TabtypeControlDependencyId]
		,[TabTypeControlId]
		,[TabTypeId]
		,[ControlTypeValue]
		,[ComparatorID]
		,[CreatedBy]
		,[CreatedDate]
	FROM
		[UserManagement].[TabTypeControlDependency]
	WHERE
		[TabtypeControlDependencyId] = @TabtypeControlDependencyId