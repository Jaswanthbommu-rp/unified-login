
-- =============================================
-- Author:		Monte Jennings
-- Create date: 
-- Description: Deletes the record with the indicated ID from the ProductPage table.
-- =============================================
CREATE PROCEDURE [UserManagement].[DeleteProductPage] (
	 @ProductPageId INT = NULL 
	,@ProductId INT = NULL 
	,@DisplayName NVARCHAR(510) = NULL 
	,@CreatedBy BIGINT = NULL 
	,@CreatedDate DATETIME = NULL 
)

AS
BEGIN



	DELETE 
		[UserManagement].[ProductPage]

	WHERE
		(@ProductPageId IS NULL  OR  [ProductPageId] = @ProductPageId)
	AND
		(@ProductId IS NULL  OR  [ProductId] = @ProductId)
	AND
		(@DisplayName IS NULL  OR  [DisplayName] = @DisplayName)

	RETURN 1 --for success

END