
-- =============================================
-- Author:		Monte Jennings
-- Create date: 
-- Description: Searches the Comparator table for the record with the indicated criteria.
-- =============================================
CREATE PROCEDURE [Enterprise].GetComparatorByControlDependencyOnControlDependencyId (
@ControlDependencyId INT
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
		[UserManagement].[ControlDependency]
	On
		[UserManagement].[ControlDependency].[ControlDependencyId] = [Enterprise].[Comparator].[ComparatorId]
	WHERE
		[UserManagement].[ControlDependency].[ControlDependencyId] = @ControlDependencyId

END