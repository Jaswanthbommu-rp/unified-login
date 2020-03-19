
-- =============================================
-- Author:		Monte Jennings
-- Create date: 
-- Description: Searches the Control table for the record with the indicated criteria.
-- =============================================
CREATE PROCEDURE [UserManagement].[GetControlJoinProductPageControlByProductPageId] (
@ProductPageId INT
)
AS

BEGIN
	SELECT
		 [UserManagement].[Control].[ControlId]
		,[UserManagement].[Control].[ParentControlId]
		,[UserManagement].[Control].[ControlTypeId]
		,[UserManagement].[Control].[UIId]
		,[UserManagement].[Control].[DisplayName]
		,[UserManagement].[Control].[DataSource]
		,[UserManagement].[Control].[Sequence]
		,[UserManagement].[Control].[CreatedBy]
		,[UserManagement].[Control].[CreatedDate]
	FROM
		[UserManagement].[Control]
	INNER Join
		[UserManagement].[ProductPageControl] A
	On
		[A].[ControlId] = [UserManagement].[Control].[ControlId]
	INNER Join
		[UserManagement].[ProductPage] B
	On
		[A].[ProductPageId] = [B].[ProductPageId]
	WHERE
		[B].[ProductPageId] = @ProductPageId

END