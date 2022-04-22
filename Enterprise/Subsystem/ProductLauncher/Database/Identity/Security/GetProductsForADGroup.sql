CREATE PROCEDURE [Security].[GetProductsForADGroup] @adGroupId int  
AS  
BEGIN  
	SELECT p.ProductId as Id, p.Name, ap.AssignmentOrder
	FROM Security.[ADGroupProduct] ap  
	JOIN Enterprise.[Product] p on p.ProductId = ap.ProductId  
	WHERE AP.ADGroupId = @adGroupId  
	ORDER BY p.Name  
END