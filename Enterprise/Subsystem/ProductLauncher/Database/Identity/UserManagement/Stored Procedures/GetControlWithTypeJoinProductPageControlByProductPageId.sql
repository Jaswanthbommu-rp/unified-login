  
-- =============================================  
-- Author:  Monte Jennings  
-- Create date:   
-- Description: Searches the Control table for the record with the indicated criteria.  
-- =============================================  
CREATE PROCEDURE [UserManagement].[GetControlWithTypeJoinProductPageControlByProductPageId] (  
@ProductPageId INT  
)  
AS  
  
BEGIN  
 SELECT DISTINCT   
  [UserManagement].[Control].[ControlId]  
 ,[UserManagement].[Control].[ParentControlId]  
 ,[UserManagement].[Control].[ControlTypeId]  
 ,[UserManagement].[Control].[UIId]  
 ,[UserManagement].[Control].[DisplayName]  
 ,[UserManagement].[Control].[DataSource]  
 ,[UserManagement].[Control].[Sequence]  
 ,[UserManagement].[ControlType].[Name] AS 'ControlType' 
 ,[UserManagement].[ControlType].[ControlTypeId] as 'ControlTypeId'
 ,[UserManagement].[Control].[CreatedBy]  
 ,[UserManagement].[Control].[CreatedDate]  
 ,CASE WHEN Children.ControlId IS NULL THEN 'False' ELSE 'True' END AS Children   
 ,CASE WHEN ControlDependency.SlaveControlId IS NULL THEN 'False' Else 'True' END as Dependency   
 ,CASE WHEN ControlAttribute.ControlId IS NULL THEN 'False' Else 'True' END as Attribute 
FROM [UserManagement].[Control]  
INNER JOIN [UserManagement].[ControlType] ON [Control].[ControlTypeId] = [ControlType].[ControlTypeId]  
INNER JOIN [UserManagement].[ProductPageControl] A ON [A].[ControlId] = [UserManagement].[Control].[ControlId]  
INNER JOIN [UserManagement].[ProductPage] B ON [A].[ProductPageId] = [B].[ProductPageId]  
LEFT OUTER JOIN [UserManagement].[Control] Children ON Children.[ParentControlId] = [Control].[ControlId]    
LEFT OUTER JOIN [UserManagement].[ControlDependency] ON ControlDependency.SlaveControlId = [Control].ControlId  
LEFT OUTER JOIN [UserManagement].[ControlAttribute] ON ControlAttribute.ControlId = [Control].ControlId  
WHERE [B].[ProductPageId] = @ProductPageId  
  
  
END