
-- =============================================
-- Author:		Monte Jennings
-- Create date: 
-- Description: Deletes the record with the indicated ID from the ProductPageTabType table.
-- =============================================
CREATE PROCEDURE [UserManagement].DeleteProductPageTabType (
	 @ProductPageTabTypeId INT = NULL 
	,@ProductPageId INT = NULL 
	,@TabTypeId INT = NULL 
	,@Sequence TINYINT = NULL 
	,@CreatedBy BIGINT = NULL 
	,@CreatedDate DATETIME = NULL 
)

AS
BEGIN



	DELETE 
		[UserManagement].[ProductPageTabType]

	WHERE
		(@ProductPageTabTypeId IS NULL  OR  [ProductPageTabTypeId] = @ProductPageTabTypeId)
	AND
		(@ProductPageId IS NULL  OR  [ProductPageId] = @ProductPageId)
	AND
		(@TabTypeId IS NULL  OR  [TabTypeId] = @TabTypeId)
	AND
		(@Sequence IS NULL  OR  [Sequence] = @Sequence)

	RETURN 1 --for success

END