
-- =============================================
-- Author:		Monte Jennings
-- Create date: 
-- Description: Gets the record with the indicated ID from the ControlDependency table.
-- =============================================
CREATE PROCEDURE [UserManagement].[GetControlDependencyBySlaveControlID] (
	 @SlaveControlID INT) 

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
		[SlaveControlID] = @SlaveControlID