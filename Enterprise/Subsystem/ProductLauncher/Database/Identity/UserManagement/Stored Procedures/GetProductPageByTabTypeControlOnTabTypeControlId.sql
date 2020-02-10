
-- =============================================
-- Author:		Monte Jennings
-- Create date: 
-- Description: Searches the ProductPage table for the record with the indicated criteria.
-- =============================================
CREATE PROCEDURE [UserManagement].GetProductPageByTabTypeControlOnTabTypeControlId (
@TabTypeControlId INT
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
		[UserManagement].[TabTypeControl]
	On
		[UserManagement].[TabTypeControl].[TabTypeControlId] = [UserManagement].[ProductPage].[ProductPageId]
	WHERE
		[UserManagement].[TabTypeControl].[TabTypeControlId] = @TabTypeControlId

END