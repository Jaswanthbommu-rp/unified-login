CREATE PROCEDURE [UserManagement].[GetProductPageByProductId] (@ProductId INT)  
AS  
  
BEGIN  
 SELECT  
   pg.[ProductPageId]  
  ,pg.[ProductId]  
  ,pg.[DisplayName]  
  ,pg.IsActive  
  ,pg.[CreatedBy]  
  ,pg.[CreatedDate], pgt.Value as ProductPageType  
 FROM  
  [UserManagement].[ProductPage] pg 
  INNER JOIN UserManagement.ProductPageType pgt on pgt.ProductPageTypeId = pg.ProductPageTypeId
  Where pg.ProductId = @ProductId
END