
-- =============================================
-- Author:		Monte Jennings
-- Create date: 
-- Description: Gets the record with the indicated ID from the ProductPage table.
-- =============================================
CREATE PROCEDURE [UserManagement].[GetProductPage] (
	 @ProductPageId INT) 

 AS 

	SELECT
		 [ProductPageId]
		,[ProductId]
		,[DisplayName]
		,[CreatedBy]
		,[CreatedDate]
	FROM
		[UserManagement].[ProductPage]
	WHERE
		[ProductPageId] = @ProductPageId