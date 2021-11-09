CREATE   PROCEDURE [Security].[GetRightsAndProductsForADGroup] @adGroupId int  
AS  
BEGIN  
 SELECT r.RightId, r.RightName, p.Name as ProductName, p1.Name AS TargetProductName
 FROM security.ADGroupRight adr
 INNER JOIN security.[Right] r ON r.RightId = adr.RightId
 INNER JOIN enterprise.Product p ON p.ProductId = r.ProductId
 INNER JOIN enterprise.product p1 ON p1.ProductId = r.TargetProductId
 WHERE adr.ADGroupId = @adGroupId      
 ORDER BY p.name, p1.name 
END  