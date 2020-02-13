
-- =============================================
-- Author:		Monte Jennings
-- Create date: 
-- Description: Gets the record with the indicated ID from the ProductPage table.
-- =============================================
CREATE PROCEDURE [UserManagement].[GetProductPageByProductIdOrGuid] (
	 @ProductId INT = NULL
	 ,@ProductGuid uniqueidentifier = NULL
) 

 AS 

	SELECT
		 [ProductPage].[ProductPageId]
		,[ProductPage].[ProductId]
		,[ProductPage].[DisplayName]
		,[ProductPage].[CreatedBy]
		,[ProductPage].[CreatedDate]
	FROM
		[UserManagement].[ProductPage]
	INNER JOIN Enterprise.Product ON ProductPage.ProductID = Product.ProductId

	WHERE
		(Product.ProductId = @ProductId OR @ProductId IS NULL)
		AND
		(Product.ProductGUID = @ProductGuid OR @ProductGUID IS NULL)