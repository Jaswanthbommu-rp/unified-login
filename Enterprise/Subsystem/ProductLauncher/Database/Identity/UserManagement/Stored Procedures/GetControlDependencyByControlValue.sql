
-- =============================================  
-- Author:  Monte Jennings  
-- Create date:   
-- Description: Searches the TabDependency table for the record with the indicated criteria.  
-- =============================================  
CREATE PROCEDURE [UserManagement].[GetControlDependencyByControlValue] (
	@ControlId int,
	@ControlValue nvarchar(510)  
)  
AS  
BEGIN  
	SELECT	
		 [ControlDependency].[ControlDependencyId]
		,[ControlDependency].[MasterControlId]
		,[ControlDependency].[SlaveControlId]
		,[Control].[DisplayName]
		,[ControlDependency].[MasterControlValue]
		,[ControlDependency].[ComparatorID]
		,[Comparator].[Name]
		,[ControlDependency].[CreatedBy]
		,[ControlDependency].[CreatedDate]
	FROM	
	[UserManagement].[ControlDependency] 
	INNER JOIN [UserManagement].[Control] ON [Control].[ControlId] = [ControlDependency].[MasterControlId]  
	INNER JOIN [Enterprise].[Comparator] ON [Comparator].[ComparatorId] = [ControlDependency].[ComparatorId]  
	WHERE	[ControlDependency].[MasterControlId]  = @ControlId
	AND		[ControlDependency].[MasterControlValue] = @ControlValue  
END