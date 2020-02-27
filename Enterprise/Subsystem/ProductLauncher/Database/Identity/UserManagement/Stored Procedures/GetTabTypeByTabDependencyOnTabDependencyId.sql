
-- =============================================
-- Author:		Monte Jennings
-- Create date: 
-- Description: Searches the TabType table for the record with the indicated criteria.
-- =============================================
CREATE PROCEDURE [UserManagement].GetTabTypeByTabDependencyOnTabDependencyId (
@TabDependencyId INT
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
		[UserManagement].[TabDependency]
	On
		[UserManagement].[TabDependency].[TabDependencyId] = [UserManagement].[TabType].[TabTypeId]
	WHERE
		[UserManagement].[TabDependency].[TabDependencyId] = @TabDependencyId

END