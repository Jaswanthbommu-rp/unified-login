
-- =============================================
-- Author:		Monte Jennings
-- Create date: 
-- Description: Searches the Comparator table for the record with the indicated criteria.
-- =============================================
CREATE PROCEDURE [UserManagement].GetComparatorByControlDependencyOnControlDependencyId (
@ControlDependencyId INT
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
		[UserManagement].[ControlDependency]
	On
		[UserManagement].[ControlDependency].[ControlDependencyId] = [UserManagement].[Comparator].[ComparatorId]
	WHERE
		[UserManagement].[ControlDependency].[ControlDependencyId] = @ControlDependencyId

END