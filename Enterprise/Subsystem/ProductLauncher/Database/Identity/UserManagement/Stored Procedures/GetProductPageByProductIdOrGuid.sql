
-- =============================================
-- Author:		Monte Jennings
-- Create date: 
-- Description: Searches the ProductPage table for the record with the indicated criteria.
-- =============================================
CREATE PROCEDURE [UserManagement].[GetProductPageByProductIdOrGuid] (
	 @ProductId INT = NULL
	,@ProductGUID uniqueidentifier
)
AS

BEGIN
	SELECT
		 [UserManagement].[ProductPage].[ProductPageId]
		,[UserManagement].[ProductPage].[ProductId]
		,[UserManagement].[ProductPage].[DisplayName]
		,[UserManagement].[ProductPage].[CreatedBy]
		,[UserManagement].[ProductPage].[CreatedDate]
	FROM
		[UserManagement].[ProductPage]
	INNER JOIN
		[Enterprise].[Product]
	On
		[Enterprise].[Product].[ProductId] = [UserManagement].[ProductPage].[ProductId]
	WHERE
		([Product].[ProductId] IS NULL OR [Product].[ProductId] = @ProductId)
		OR
		([Product].[ProductGUID] IS NULL OR [Product].[ProductGUID] = @ProductGUID)


END