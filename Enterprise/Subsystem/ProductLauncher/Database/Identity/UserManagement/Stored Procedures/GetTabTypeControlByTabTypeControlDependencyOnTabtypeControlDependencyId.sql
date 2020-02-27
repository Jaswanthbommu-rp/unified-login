
-- =============================================
-- Author:		Monte Jennings
-- Create date: 
-- Description: Searches the TabTypeControl table for the record with the indicated criteria.
-- =============================================
CREATE PROCEDURE [UserManagement].GetTabTypeControlByTabTypeControlDependencyOnTabtypeControlDependencyId (
@TabtypeControlDependencyId INT
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
		[UserManagement].[TabTypeControlDependency]
	On
		[UserManagement].[TabTypeControlDependency].[TabtypeControlDependencyId] = [UserManagement].[TabTypeControl].[TabTypeControlId]
	WHERE
		[UserManagement].[TabTypeControlDependency].[TabtypeControlDependencyId] = @TabtypeControlDependencyId

END