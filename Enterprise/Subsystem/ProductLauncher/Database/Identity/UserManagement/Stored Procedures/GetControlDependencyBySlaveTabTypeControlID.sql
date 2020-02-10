
-- =============================================
-- Author:		Monte Jennings
-- Create date: 
-- Description: Gets the record with the indicated ID from the ControlDependency table.
-- =============================================
CREATE PROCEDURE [UserManagement].GetControlDependencyBySlaveTabTypeControlID (
	 @SlaveTabTypeControlID INT) 

 AS 

	SELECT
		 [ControlDependencyId]
		,[MasterTabTypeControlId]
		,[SlaveTabTypeControlID]
		,[MasterControlValue]
		,[ComparatorID]
		,[CreatedBy]
		,[CreatedDate]
	FROM
		[UserManagement].[ControlDependency]
	WHERE
		[SlaveTabTypeControlID] = @SlaveTabTypeControlID