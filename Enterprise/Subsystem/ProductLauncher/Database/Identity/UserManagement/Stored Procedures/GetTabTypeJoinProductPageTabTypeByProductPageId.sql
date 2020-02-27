
-- =============================================
-- Author:		Monte Jennings
-- Create date: 
-- Description: Searches the TabType table for the record with the indicated criteria.
-- =============================================
CREATE PROCEDURE [UserManagement].GetTabTypeJoinProductPageTabTypeByProductPageId (
@ProductPageId INT
)
AS

BEGIN
	SELECT
		 [UserManagement].[TabType].[TabTypeId]
		,[UserManagement].[TabType].[UIId]
		,[UserManagement].[TabType].[DisplayName]
		,[UserManagement].[TabType].[CreatedBy]
		,[UserManagement].[TabType].[CreatedDate]
	FROM
		[UserManagement].[TabType]
	INNER Join
		[UserManagement].[ProductPageTabType] A
	On
		[A].[TabTypeId] = [UserManagement].[TabType].[TabTypeId]
	INNER Join
		[UserManagement].[ProductPage] B
	On
		[A].[ProductPageId] = [B].[ProductPageId]
	WHERE
		[B].[ProductPageId] = @ProductPageId

END