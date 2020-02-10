
-- =============================================
-- Author:		Monte Jennings
-- Create date: 
-- Description: Searches the Comparator table for the record with the indicated criteria.
-- =============================================
CREATE PROCEDURE [UserManagement].GetComparatorByTabDependencyOnTabDependencyId (
@TabDependencyId INT
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
		[UserManagement].[TabDependency]
	On
		[UserManagement].[TabDependency].[TabDependencyId] = [UserManagement].[Comparator].[ComparatorId]
	WHERE
		[UserManagement].[TabDependency].[TabDependencyId] = @TabDependencyId

END