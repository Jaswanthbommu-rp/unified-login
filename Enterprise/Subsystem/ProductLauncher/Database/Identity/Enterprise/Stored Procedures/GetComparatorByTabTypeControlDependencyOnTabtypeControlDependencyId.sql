
-- =============================================
-- Author:		Monte Jennings
-- Create date: 
-- Description: Searches the Comparator table for the record with the indicated criteria.
-- =============================================
CREATE PROCEDURE [Enterprise].GetComparatorByTabTypeControlDependencyOnTabtypeControlDependencyId (
@TabtypeControlDependencyId INT
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
		[UserManagement].[TabTypeControlDependency]
	On
		[UserManagement].[TabTypeControlDependency].[TabtypeControlDependencyId] = [Enterprise].[Comparator].[ComparatorId]
	WHERE
		[UserManagement].[TabTypeControlDependency].[TabtypeControlDependencyId] = @TabtypeControlDependencyId

END