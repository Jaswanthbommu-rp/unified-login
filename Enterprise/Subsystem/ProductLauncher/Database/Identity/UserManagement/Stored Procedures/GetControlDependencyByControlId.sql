
-- =============================================
-- Author:		Monte Jennings
-- Create date: 
-- Description: Gets the record with the indicated ID from the ControlDependency table.
-- =============================================
CREATE PROCEDURE [UserManagement].[GetControlDependencyByControlId] (
	 @ControlId INT
) 

 AS 

	SELECT
		 [ControlDependency].[ControlDependencyId]
		,[ControlDependency].[MasterControlId]
		,[Master].[UIId] AS MasterControlUIId
		,[ControlDependency].[SlaveControlId]
		,[Slave].[UIId] AS SlaveControlUIId
		,[ControlDependency].[MasterControlValue]
		,[Comparator].[Name]
		,[ControlDependency].[CreatedBy]
		,[ControlDependency].[CreatedDate]
	FROM
		[UserManagement].[ControlDependency]
	INNER JOIN [Enterprise].[Comparator] ON ControlDependency.ComparatorId = Comparator.ComparatorId
	INNER JOIN [UserManagement].[Control] [Slave] ON [Slave].[ControlId] = [ControlDependency].[SlaveControlId]
	INNER JOIN [UserManagement].[Control] [Master] ON [Master].[ControlId] = [ControlDependency].[MasterControlId]
	WHERE
		[MasterControlId] = @ControlId