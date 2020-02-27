-- =============================================  
-- Author:  Monte Jennings  
-- Create date:   
-- Description: Searches the TabDependency table for the record with the indicated criteria.  
-- =============================================  
CREATE PROCEDURE [UserManagement].[GetTabDependencyByControlValue] (
	@ControlId int,
	@ControlValue nvarchar(510)  
)  
AS  
BEGIN  
	SELECT	umtd.[TabDependencyId]
					,umtd.[ControlId]
					,umtd.[TabTypeId]
					,umtt.[DisplayName]
					,umtd.[ControlValue]
					,umtd.[ComparatorID]
					,ec.[Name]
					,umtd.[CreatedBy]
					,umtd.[CreatedDate]
	FROM	[UserManagement].[TabDependency] umtd
				INNER JOIN [UserManagement].[TabType] umtt ON umtt.[TabTypeId] = umtd.[TabTypeId]  
				INNER JOIN [Enterprise].[Comparator] ec ON ec.[ComparatorId] = umtd.[ComparatorId]  
	WHERE	umtd.[ControlId]  = @ControlId
	AND			umtd.[ControlValue] = @ControlValue  
END