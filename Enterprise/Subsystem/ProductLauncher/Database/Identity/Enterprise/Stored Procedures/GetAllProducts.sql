CREATE PROCEDURE [Enterprise].[GetAllProducts] 
AS
BEGIN
SELECT pt.ProductTypeId as TypeId, 
	   pt.Name as TypeName, p.ProductId, p.ProductGUID, p.Name, 
	   p.Description, p.ProductTypeId,p.BooksProductCode,
	   p.UDMSourceCode, p.AssignToAllUsers
FROM Enterprise.ProductType pt
	JOIN Enterprise.ProductType pt2 on pt.ProductTypeId = pt2.ParentProductTypeId
	RIGHT JOIN Enterprise.Product p on p.ProductTypeId = pt2.ProductTypeId
ORDER BY pt.Name
END