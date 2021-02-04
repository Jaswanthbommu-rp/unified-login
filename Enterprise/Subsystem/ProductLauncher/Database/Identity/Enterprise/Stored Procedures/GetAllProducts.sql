CREATE PROCEDURE [Enterprise].[GetAllProducts] @ProductId INT = NULL  
AS    
BEGIN    
SELECT pt.ProductTypeId as TypeId, pt.Name as TypeName,    
       pt2.ProductTypeId as ParentTypeId, pt2.Name as ParentTypeName,    
       p.ProductId,  p.ProductGUID, p.Name,    
       p.Description, p.ProductTypeId,p.BooksProductCode,    
       p.UDMSourceCode, p.AssignToAllUsers,
	   s.LoginUri, s.SigningCertificateThumbprint    
FROM Enterprise.Product p    
    LEFT JOIN Enterprise.ProductType pt on p.ProductTypeId = pt.ProductTypeId    
    LEFT JOIN Enterprise.ProductType pt2 on pt2.ProductTypeId = pt.ParentProductTypeId
	LEFT JOIN Ident.SamlProductSettings s on s.ProductId = p.ProductId    
WHERE p.ProductId = isnull(@productId, p.productId)  
ORDER BY pt.Name    
END  