CREATE PROCEDURE [UserManagement].[GetControlDescendantsWithType] (
	@ParentId INT = NULL
)
AS

BEGIN
	WITH  SelfJoinCTE AS ( --Recursive call to get all [UserManagement].[Control]s
		SELECT
		 umc.[ControlId]
		,umc.[ParentControlId]
		,umc.[ControlTypeId]
		,umc.[UIId]
		,umc.[DisplayName]
		,umc.[DataSource]
		,umc.[Sequence]
		,umc.[CreatedBy]
		,umc.[CreatedDate]
		,umct.Name AS 'ControlType'
		,0 AS Level
		FROM	[UserManagement].[Control] umc
					INNER JOIN [UserManagement].[ControlType] umct ON umct.ControlTypeId = umc.ControlTypeId
		WHERE [ControlId] = @ParentId OR @ParentId IS NULL
		UNION ALL
		SELECT
	 
			 umc.[ControlId]
			,umc.[ParentControlId]
			,umc.[ControlTypeId]
			,umc.[UIId]
			,umc.[DisplayName]
			,umc.[DataSource]
			,umc.[Sequence]
			,umc.[CreatedBy]
			,umc.[CreatedDate]
			,umct.Name AS 'ControlType'
		, Level + 1
		FROM	[UserManagement].[Control] umc
					INNER JOIN [UserManagement].[ControlType] umct ON umct.ControlTypeId = umc.ControlTypeId
					INNER JOIN SelfJoinCTE cte ON (umc.[ParentControlId] = cte.[ControlId])
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
		,ControlType
		,Level
	FROM SelfJoinCTE
END