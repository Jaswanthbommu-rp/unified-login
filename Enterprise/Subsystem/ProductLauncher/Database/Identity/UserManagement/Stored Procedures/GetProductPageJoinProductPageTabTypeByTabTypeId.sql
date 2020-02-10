
-- =============================================
-- Author:		Monte Jennings
-- Create date: 
-- Description: Searches the ProductPage table for the record with the indicated criteria.
-- =============================================
CREATE PROCEDURE [UserManagement].GetProductPageJoinProductPageTabTypeByTabTypeId (
@TabTypeId INT
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
		[UserManagement].[ProductPageTabType] A
	On
		[A].[ProductPageId] = [UserManagement].[ProductPage].[ProductPageId]
	INNER Join
		[UserManagement].[TabType] B
	On
		[A].[TabTypeId] = [B].[TabTypeId]
	WHERE
		[B].[TabTypeId] = @TabTypeId

END