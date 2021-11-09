CREATE   PROCEDURE [Security].[GetRightsAndProductsForADGroup] @adGroupId int  
AS  
BEGIN  
 SELECT r.RightId, r.RightName, p.Name as ProductName      
 FROM [Security].ADGroupRight ar
 JOIN Security.[Right] r on r.RightId = ar.RightId
 JOIN Enterprise.Product p on p.ProductId = r.TargetProductId
 WHERE ar.ADGroupId = @adGroupId      
 ORDER BY p.Name   
END  