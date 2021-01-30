CREATE PROCEDURE [Enterprise].[GetAllProducts] 
AS
BEGIN
SELECT pt.ProductTypeId as TypeId, 
	   pt.Name as TypeName, p.ProductId, p.ProductGUID, p.Name, 
	   p.Description, p.ProductTypeId,p.BooksProductCode,
	   p.UDMSourceCode, p.AssignToAllUsers
FROM Enterprise.Product p
	LEFT JOIN Enterprise.ProductType pt on pt.ProductTypeId = p.ProductTypeId
ORDER BY pt.Name
END