
-- =============================================
-- Author:		Monte Jennings
-- Create date: 
-- Description: Gets the record with the indicated ID from the ControlDependency table.
-- =============================================
CREATE PROCEDURE [UserManagement].[GetControlDependencyByComparatorId] (
	 @ComparatorId TINYINT) 

 AS 

	SELECT
		 [ControlDependencyId]
		,[MasterControlId]
		,[SlaveControlID]
		,[MasterControlValue]
		,[ComparatorId]
		,[CreatedBy]
		,[CreatedDate]
	FROM
		[UserManagement].[ControlDependency]
	WHERE
		[ComparatorId] = @ComparatorId