-- =============================================
-- Author:		Monte Jennings
-- Create date: 
-- Description: Searches the ProductPage table for the record with the indicated criteria.
-- =============================================
CREATE PROCEDURE [UserManagement].[GetProductPageByTabTypeControlOnControlId] (
@ControlId INT
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
		[UserManagement].[TabTypeControl].[ControlId] = [UserManagement].[ProductPage].[ProductPageId]
	WHERE
		[UserManagement].[TabTypeControl].[ControlId] = @ControlId

END