Create Procedure [UserManagement].[GetProductPageById] @pageId int
As
Begin
	SELECT pg.ProductPageId, pg.DisplayName, 
	pg.ProductPageTypeId, pt.Value as PageTypeName,
	p.ProductId, p.Name as ProductName,
	pg.IsActive as IsActive
	FROM UserManagement.ProductPage pg
	JOIN Enterprise.Product p on pg.ProductId = p.ProductId
	JOIN UserManagement.ProductPageType pt on pt.ProductPageTypeId = pg.ProductPageTypeId
	WHERE ProductPageId = @pageId
End
