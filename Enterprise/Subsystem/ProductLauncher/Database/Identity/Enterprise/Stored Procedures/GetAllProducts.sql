CREATE PROCEDURE [Enterprise].[GetAllProducts] @ProductId INT = NULL
AS  
BEGIN  
SELECT pt.ProductTypeId as TypeId, pt.Name as TypeName, pt.ProductTypeGuid, 
       pt2.ProductTypeId as ParentTypeId, pt2.Name as ParentTypeName,  
       p.ProductId,  p.ProductGUID, p.Name,  
       p.Description, p.ProductTypeId,p.BooksProductCode,  
       p.UDMSourceCode, p.AssignToAllUsers,
       s.LoginUri, s.SigningCertificateThumbprint
FROM Enterprise.Product p  
    left join Enterprise.ProductType pt on p.ProductTypeId = pt.ProductTypeId  
    left JOIN Enterprise.ProductType pt2 on pt2.ProductTypeId = pt.ParentProductTypeId
    left join Ident.SamlProductSettings s on s.ProductId = p.ProductId
WHERE p.ProductId = isnull(@productId, p.productId)
ORDER BY pt.Name  
END