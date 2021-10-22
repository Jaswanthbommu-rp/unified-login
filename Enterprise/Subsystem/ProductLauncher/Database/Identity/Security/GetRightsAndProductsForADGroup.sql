CREATE   PROCEDURE [Security].[GetRightsAndProductsForADGroup] @adGroupId int  
AS  
BEGIN  
 SELECT r.RightId, r.RightName, p.Name as ProductName  
 FROM Security.[ADGroupRight] ar  
 JOIN Security.[Right] r on r.RightId = ar.RightId  
 JOIN  Security.ADGroupProduct ap on ar.ADGroupId = ap.ADGroupId  
 JOIN Enterprise.Product p on p.ProductId = ap.ProductId  
 WHERE ar.ADGroupId = @adGroupId  
 ORDER BY p.Name  
END  