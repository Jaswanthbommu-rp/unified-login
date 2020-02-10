
-- =============================================
-- Author:		Monte Jennings
-- Create date: 
-- Description: Searches the Comparator table for the record with the indicated criteria.
-- =============================================
CREATE PROCEDURE [UserManagement].GetComparatorJoinTabDependencyByTabTypeId (
@TabTypeId INT
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
		[UserManagement].[TabDependency] A
	On
		[A].[ComparatorID] = [UserManagement].[Comparator].[ComparatorId]
	INNER Join
		[UserManagement].[TabType] B
	On
		[A].[TabTypeId] = [B].[TabTypeId]
	WHERE
		[B].[TabTypeId] = @TabTypeId

END