
-- =============================================
-- Author:		Monte Jennings
-- Create date: 
-- Description: Searches the TabType table for the record with the indicated criteria.
-- =============================================
CREATE PROCEDURE [UserManagement].GetTabTypeJoinTabTypeControlDependencyByTabTypeControlId (
@TabTypeControlId INT
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
		[UserManagement].[TabTypeControlDependency] A
	On
		[A].[TabTypeId] = [UserManagement].[TabType].[TabTypeId]
	INNER Join
		[UserManagement].[TabTypeControl] B
	On
		[A].[TabTypeControlId] = [B].[TabTypeControlId]
	WHERE
		[B].[TabTypeControlId] = @TabTypeControlId

END