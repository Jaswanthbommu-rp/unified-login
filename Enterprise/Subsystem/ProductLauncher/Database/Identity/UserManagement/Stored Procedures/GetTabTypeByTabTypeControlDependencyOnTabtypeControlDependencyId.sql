
-- =============================================
-- Author:		Monte Jennings
-- Create date: 
-- Description: Searches the TabType table for the record with the indicated criteria.
-- =============================================
CREATE PROCEDURE [UserManagement].GetTabTypeByTabTypeControlDependencyOnTabtypeControlDependencyId (
@TabtypeControlDependencyId INT
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
		[UserManagement].[TabTypeControlDependency]
	On
		[UserManagement].[TabTypeControlDependency].[TabtypeControlDependencyId] = [UserManagement].[TabType].[TabTypeId]
	WHERE
		[UserManagement].[TabTypeControlDependency].[TabtypeControlDependencyId] = @TabtypeControlDependencyId

END