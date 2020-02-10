
-- =============================================
-- Author:		Monte Jennings
-- Create date: 
-- Description: Searches the Comparator table for the record with the indicated criteria.
-- =============================================
CREATE PROCEDURE [UserManagement].GetComparatorJoinControlDependencyBySlaveTabTypeControlID (
@TabTypeControlId INT
)
AS

BEGIN
	SELECT
		 [UserManagement].[Comparator].[ComparatorId]
		,[UserManagement].[Comparator].[Name]
		,[UserManagement].[Comparator].[CreatedDate]
		,[UserManagement].[Comparator].[CreatedBy]
	FROM
		[UserManagement].[Comparator]
	INNER Join
		[UserManagement].[ControlDependency] A
	On
		[A].[ComparatorID] = [UserManagement].[Comparator].[ComparatorId]
	INNER Join
		[UserManagement].[TabTypeControl] B
	On
		[A].[SlaveTabTypeControlID] = [B].[TabTypeControlId]
	WHERE
		[B].[TabTypeControlId] = @TabTypeControlId

END