
-- =============================================
-- Author:		Monte Jennings
-- Create date: 
-- Description: Searches the Control table for the record with the indicated criteria.
-- =============================================
CREATE PROCEDURE [UserManagement].[GetControlAncestors](
	@ChildId INT = NULL
)
AS

BEGIN
WITH  SelfJoinCTE AS ( --Recursive call to get all [UserManagement].[Control]s
	SELECT
	 [ControlId]
	,[ParentControlId]
	,[ControlTypeId]
	,[UIId]
	,[DisplayName]
	,[DataSource]
	,[Sequence]
	,[CreatedBy]
	,[CreatedDate]
	,0 AS Level
	FROM	[UserManagement].[Control] 
	WHERE ControlId = @ChildId OR @ChildId IS NULL
	UNION ALL
	SELECT
	 
		 [Control].[ControlId]
		,[Control].[ParentControlId]
		,[Control].[ControlTypeId]
		,[Control].[UIId]
		,[Control].[DisplayName]
		,[Control].[DataSource]
		,[Control].[Sequence]
		,[Control].[CreatedBy]
		,[Control].[CreatedDate]
	, Level + 1
	FROM	[UserManagement].[Control]
	INNER JOIN SelfJoinCTE 
	ON ([UserManagement].[Control].ControlId = SelfJoinCTE.ParentControlId)			
)

SELECT
	 [ControlId]
	,[ParentControlId]
	,[ControlTypeId]
	,[UIId]
	,[DisplayName]
	,[DataSource]
	,[Sequence]
	,[CreatedBy]
	,[CreatedDate]
	,Level
FROM SelfJoinCTE
ORDER BY Level DESC

END