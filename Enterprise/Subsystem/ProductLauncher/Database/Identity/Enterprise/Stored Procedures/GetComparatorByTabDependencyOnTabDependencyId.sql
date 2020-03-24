
-- =============================================
-- Author:		Monte Jennings
-- Create date: 
-- Description: Searches the Comparator table for the record with the indicated criteria.
-- =============================================
CREATE PROCEDURE [Enterprise].GetComparatorByTabDependencyOnTabDependencyId (
@TabDependencyId INT
)
AS

BEGIN
	SELECT
		 [Enterprise].[Comparator].[ComparatorId]
		,[Enterprise].[Comparator].[Name]
		,[Enterprise].[Comparator].[CreatedDate]
		,[Enterprise].[Comparator].[CreatedBy]
	FROM
		[Enterprise].[Comparator]
	INNER Join
		[UserManagement].[TabDependency]
	On
		[UserManagement].[TabDependency].[TabDependencyId] = [Enterprise].[Comparator].[ComparatorId]
	WHERE
		[UserManagement].[TabDependency].[TabDependencyId] = @TabDependencyId

END