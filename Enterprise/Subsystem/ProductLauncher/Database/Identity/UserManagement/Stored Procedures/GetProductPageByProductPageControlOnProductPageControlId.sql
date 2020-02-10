
-- =============================================
-- Author:		Monte Jennings
-- Create date: 
-- Description: Searches the ProductPage table for the record with the indicated criteria.
-- =============================================
CREATE PROCEDURE [UserManagement].GetProductPageByProductPageControlOnProductPageControlId (
@ProductPageControlId INT
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
		[UserManagement].[ProductPageControl]
	On
		[UserManagement].[ProductPageControl].[ProductPageControlId] = [UserManagement].[ProductPage].[ProductPageId]
	WHERE
		[UserManagement].[ProductPageControl].[ProductPageControlId] = @ProductPageControlId

END