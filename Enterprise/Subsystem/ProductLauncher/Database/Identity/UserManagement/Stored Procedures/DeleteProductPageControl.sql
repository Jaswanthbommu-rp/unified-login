
-- =============================================
-- Author:		Monte Jennings
-- Create date: 
-- Description: Deletes the record with the indicated ID from the ProductPageControl table.
-- =============================================
CREATE PROCEDURE [UserManagement].DeleteProductPageControl (
	 @ProductPageControlId INT = NULL 
	,@ProductPageId INT = NULL 
	,@ControlId INT = NULL 
	,@CreatedBy BIGINT = NULL 
	,@CreatedDate DATETIME = NULL 
)

AS
BEGIN



	DELETE 
		[UserManagement].[ProductPageControl]

	WHERE
		(@ProductPageControlId IS NULL  OR  [ProductPageControlId] = @ProductPageControlId)
	AND
		(@ProductPageId IS NULL  OR  [ProductPageId] = @ProductPageId)
	AND
		(@ControlId IS NULL  OR  [ControlId] = @ControlId)

	RETURN 1 --for success

END