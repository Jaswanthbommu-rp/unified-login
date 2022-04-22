CREATE PROCEDURE [Security].[GetADGroupsByProductId] (    
 @ProductId int)    
AS    
BEGIN    
 SELECT     
  AP.ADGroupId,     
  G.DisplayName [ADGroupName],    
  g.ActiveDirectoryId [ActiveDirectoryId],    
  AP.AssignmentOrder
 FROM [Security].ADGroupProduct AP    
  JOIN [Security].ADGroup G on G.ADGroupId = AP.ADGroupId    
 WHERE AP.ProductId = @ProductId AND G.IsActive = 1    
 ORDER BY G.DisplayName    
END
