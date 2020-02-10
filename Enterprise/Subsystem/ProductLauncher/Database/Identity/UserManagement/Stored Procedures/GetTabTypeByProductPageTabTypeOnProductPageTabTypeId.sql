
-- =============================================
-- Author:		Monte Jennings
-- Create date: 
-- Description: Searches the TabType table for the record with the indicated criteria.
-- =============================================
CREATE PROCEDURE [UserManagement].GetTabTypeByProductPageTabTypeOnProductPageTabTypeId (
@ProductPageTabTypeId INT
)
AS

BEGIN
	SELECT
		 [UserManagement].[TabType].[TabTypeId]
		,[UserManagement].[TabType].[UIId]
		,[UserManagement].[TabType].[DisplayName]
		,[UserManagement].[TabType].[Sequence]
		,[UserManagement].[TabType].[CreatedBy]
		,[UserManagement].[TabType].[CreatedDate]
	FROM
		[UserManagement].[TabType]
	INNER Join
		[UserManagement].[ProductPageTabType]
	On
		[UserManagement].[ProductPageTabType].[ProductPageTabTypeId] = [UserManagement].[TabType].[TabTypeId]
	WHERE
		[UserManagement].[ProductPageTabType].[ProductPageTabTypeId] = @ProductPageTabTypeId

END