
-- =============================================
-- Author:		Monte Jennings
-- Create date: 
-- Description: Searches the Control table for the record with the indicated criteria.
-- =============================================
CREATE PROCEDURE [UserManagement].GetControlByTabDependencyOnTabDependencyId (
@TabDependencyId INT
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
		[UserManagement].[TabDependency]
	On
		[UserManagement].[TabDependency].[TabDependencyId] = [UserManagement].[Control].[ControlId]
	WHERE
		[UserManagement].[TabDependency].[TabDependencyId] = @TabDependencyId

END