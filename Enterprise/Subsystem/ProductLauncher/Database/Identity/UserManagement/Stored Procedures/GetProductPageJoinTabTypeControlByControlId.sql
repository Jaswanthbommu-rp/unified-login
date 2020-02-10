
-- =============================================
-- Author:		Monte Jennings
-- Create date: 
-- Description: Searches the ProductPage table for the record with the indicated criteria.
-- =============================================
CREATE PROCEDURE [UserManagement].GetProductPageJoinTabTypeControlByControlId (
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
		[UserManagement].[TabTypeControl] A
	On
		[A].[ProductPageId] = [UserManagement].[ProductPage].[ProductPageId]
	INNER Join
		[UserManagement].[Control] B
	On
		[A].[ControlId] = [B].[ControlId]
	WHERE
		[B].[ControlId] = @ControlId

END