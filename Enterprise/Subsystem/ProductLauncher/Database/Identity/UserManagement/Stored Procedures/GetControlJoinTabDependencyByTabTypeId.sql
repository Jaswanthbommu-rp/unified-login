
-- =============================================
-- Author:		Monte Jennings
-- Create date: 
-- Description: Searches the Control table for the record with the indicated criteria.
-- =============================================
CREATE PROCEDURE [UserManagement].GetControlJoinTabDependencyByTabTypeId (
@TabTypeId INT
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
		[UserManagement].[TabDependency] A
	On
		[A].[ControlId] = [UserManagement].[Control].[ControlId]
	INNER Join
		[UserManagement].[TabType] B
	On
		[A].[TabTypeId] = [B].[TabTypeId]
	WHERE
		[B].[TabTypeId] = @TabTypeId

END