CREATE PROCEDURE [Security].[GetRolesAndProductForADGroup] @adGroupId int    
AS    
BEGIN    
 SELECT DISTINCT r.RoleId, r.RoleName, p.Name as ProductName  
 FROM SECURITY.[Role] r    
 JOIN SECURITY.RoleRight rr ON rr.RoleId = r.RoleId    
 JOIN ENTERPRISE.Product p ON p.ProductId = r.ProductId    
 JOIN SECURITY.ADGroupRight adr ON adr.RightId = rr.RightId    
 WHERE adr.ADGroupId = @adGroupId    
END 