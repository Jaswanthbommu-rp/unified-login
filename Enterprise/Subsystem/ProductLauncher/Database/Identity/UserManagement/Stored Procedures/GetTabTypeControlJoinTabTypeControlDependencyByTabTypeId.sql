
-- =============================================
-- Author:		Monte Jennings
-- Create date: 
-- Description: Searches the TabTypeControl table for the record with the indicated criteria.
-- =============================================
CREATE PROCEDURE [UserManagement].GetTabTypeControlJoinTabTypeControlDependencyByTabTypeId (
@TabTypeId INT
)
AS

BEGIN
	SELECT
		 [UserManagement].[TabTypeControl].[TabTypeControlId]
		,[UserManagement].[TabTypeControl].[TabTypeId]
		,[UserManagement].[TabTypeControl].[ProductPageId]
		,[UserManagement].[TabTypeControl].[ControlId]
		,[UserManagement].[TabTypeControl].[CreatedBy]
		,[UserManagement].[TabTypeControl].[CreatedDate]
	FROM
		[UserManagement].[TabTypeControl]
	INNER Join
		[UserManagement].[TabTypeControlDependency] A
	On
		[A].[TabTypeControlId] = [UserManagement].[TabTypeControl].[TabTypeControlId]
	INNER Join
		[UserManagement].[TabType] B
	On
		[A].[TabTypeId] = [B].[TabTypeId]
	WHERE
		[B].[TabTypeId] = @TabTypeId

END