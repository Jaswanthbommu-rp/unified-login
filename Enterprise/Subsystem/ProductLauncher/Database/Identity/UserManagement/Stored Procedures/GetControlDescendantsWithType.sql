    
-- =============================================    
-- Author:  Monte Jennings    
-- Create date:     
-- Description: Searches the Control table for the record with the indicated criteria.    
-- =============================================    
    
CREATE PROCEDURE [UserManagement].[GetControlDescendantsWithType](    
 @ParentId INT = NULL    
)    
AS    
    
BEGIN    
WITH  SelfJoinCTE AS ( --Recursive call to get all [UserManagement].[Control]s    
 SELECT    
  [Control].[ControlId]    
 ,[Control].[ParentControlId]    
 ,[Control].[ControlTypeId]    
 ,[Control].[UIId]    
 ,[Control].[DisplayName]    
 ,[Control].[DataSource]    
 ,[Control].[Sequence]    
 ,[ControlType].[Name]    
 ,[Control].[CreatedBy]    
 ,[Control].[CreatedDate]    
 ,0 AS Level    
 FROM [UserManagement].[Control]    
 INNER JOIN [UserManagement].[ControlType] ON [Control].[ControlTypeId] = [ControlType].[ControlTypeId]     
 WHERE [ControlId] = @ParentId OR @ParentId IS NULL    
 UNION ALL    
 SELECT    
   [Control].[ControlId]    
  ,[Control].[ParentControlId]    
  ,[Control].[ControlTypeId]    
  ,[Control].[UIId]    
  ,[Control].[DisplayName]    
  ,[Control].[DataSource]    
  ,[Control].[Sequence]    
  ,[ControlType].[Name]    
  ,[Control].[CreatedBy]    
  ,[Control].[CreatedDate]    
 , Level + 1    
 FROM [UserManagement].[Control]    
 INNER JOIN [UserManagement].[ControlType] ON [Control].[ControlTypeId] = [ControlType].[ControlTypeId]     
 INNER JOIN SelfJoinCTE     
 ON ([UserManagement].[Control].[ParentControlId] = SelfJoinCTE.[ControlId])
)    
    
SELECT DISTINCT    
  CTE.[ControlId]    
 ,CTE.[ParentControlId]    
 ,CTE.[ControlTypeId]    
 ,CTE.[UIId]    
 ,CTE.[DisplayName]    
 ,CTE.[DataSource]    
 ,CTE.[Sequence]    
 ,CTE.[Name] AS 'ControlType'  
 ,CTE.[CreatedBy]    
 ,CTE.[CreatedDate]    
 ,CTE.Level    
 ,CASE WHEN [Control].ControlId IS NULL THEN 'False' Else 'True' END as Children    
 ,CASE WHEN ControlDependency.SlaveControlId IS NULL THEN 'False' Else 'True' END as Dependency     
 ,CASE WHEN ControlAttribute.ControlId IS NULL THEN 'False' Else 'True' END as Attribute  
FROM SelfJoinCTE CTE    
LEFT OUTER JOIN [UserManagement].[Control] ON [Control].[ParentControlId] = CTE.[ControlId]    
LEFT OUTER JOIN [UserManagement].[ControlDependency] ON ControlDependency.SlaveControlId = CTE.ControlId    
LEFT OUTER JOIN [UserManagement].[ControlAttribute] ON ControlAttribute.ControlId = CTE.ControlId    
ORDER BY Level, ParentControlId, Sequence   
    
END