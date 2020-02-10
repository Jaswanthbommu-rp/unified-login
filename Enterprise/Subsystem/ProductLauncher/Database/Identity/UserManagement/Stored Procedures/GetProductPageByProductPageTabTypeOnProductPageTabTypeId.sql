
-- =============================================
-- Author:		Monte Jennings
-- Create date: 
-- Description: Searches the ProductPage table for the record with the indicated criteria.
-- =============================================
CREATE PROCEDURE [UserManagement].GetProductPageByProductPageTabTypeOnProductPageTabTypeId (
@ProductPageTabTypeId INT
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
	INNER Join
		[UserManagement].[ProductPageTabType]
	On
		[UserManagement].[ProductPageTabType].[ProductPageTabTypeId] = [UserManagement].[ProductPage].[ProductPageId]
	WHERE
		[UserManagement].[ProductPageTabType].[ProductPageTabTypeId] = @ProductPageTabTypeId

END