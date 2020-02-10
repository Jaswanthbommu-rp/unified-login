CREATE PROCEDURE [UserManagement].[GetControlWithTypeJoinTabTypeControlByProductPageId] (
	@TabTypeId INT
)
AS
BEGIN
  SELECT umc.[ControlId]
			  ,umc.[ParentControlId]
			  ,umc.[ControlTypeId]
			  ,umc.[UIId]
			  ,umc.[DisplayName]
			  ,umc.[DataSource]
			  ,umc.[Sequence]
			  ,umc.[CreatedBy]
			  ,umc.[CreatedDate]
			  ,umct.[Name] AS 'ControlType'
			  ,CASE WHEN PUMC.ControlId IS NULL THEN 'False' ELSE 'True' END AS HasChildren
  FROM [UserManagement].[Control] umc
			  INNER Join [UserManagement].[TabTypeControl] umppc ON umppc.[ControlId] =umc.[ControlId]
			  INNER Join [UserManagement].[TabType] umpp ON umpp.[TabTypeId] = umppc.[TabTypeId]
			  INNER JOIN [UserManagement].[ControlType] umct ON umct.ControlTypeId = umc.ControlTypeId
			  LEFT OUTER JOIN [UserManagement].[Control] PUMC ON PUMC.ControlId = UMC.ParentControlId
  WHERE umpp.[TabTypeId] = @TabTypeId
END