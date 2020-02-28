CREATE PROCEDURE [UserManagement].[GetControlDescendantsWithType] (
	@ParentId INT = 50 --= NULL
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

	SELECT Distinct
		 TD.[ControlId]
		,[ParentControlId]
		,[ControlTypeId]
		,[UIId]
		,[DisplayName]
		,[DataSource]
		,[Sequence]
		,SelfJoinCTE.[CreatedBy]
		,SelfJoinCTE.[ControlType]
		,ControlType
		,Level
		,CASE WHEN TD.ControlId IS NULL THEN 'False' ELSE 'True' END AS Dependency
	FROM SelfJoinCTE
	LEFT OUTER JOIN [UserManagement].[TabDependency] TD ON SelfJoinCTE.ControlId = TD.ControlId
	ORDER BY Level, TD.ControlId, ParentControlId
END