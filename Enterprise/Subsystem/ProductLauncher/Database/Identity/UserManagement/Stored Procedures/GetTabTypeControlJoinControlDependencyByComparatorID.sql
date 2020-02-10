
-- =============================================
-- Author:		Monte Jennings
-- Create date: 
-- Description: Searches the TabTypeControl table for the record with the indicated criteria.
-- =============================================
CREATE PROCEDURE [UserManagement].GetTabTypeControlJoinControlDependencyByComparatorID (
@ComparatorId INT
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
		[UserManagement].[ControlDependency] A
	On
		[A].[SlaveTabTypeControlID] = [UserManagement].[TabTypeControl].[TabTypeControlId]
	INNER Join
		[UserManagement].[Comparator] B
	On
		[A].[ComparatorID] = [B].[ComparatorId]
	WHERE
		[B].[ComparatorId] = @ComparatorId

END