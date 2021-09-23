CREATE PROCEDURE [Security].[GetAdGroupForProduct] 
(
	@ProductId int
)
AS
BEGIN
	SELECT ADGroupProductId, ADGroupId, ProductId
	FROM [Security].ADGroupProduct
	WHERE ProductId = @ProductId
END
