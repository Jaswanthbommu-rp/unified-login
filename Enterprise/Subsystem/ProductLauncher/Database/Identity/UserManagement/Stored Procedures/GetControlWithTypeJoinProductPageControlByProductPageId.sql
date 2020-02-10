CREATE PROCEDURE [UserManagement].[GetControlWithTypeJoinProductPageControlByProductPageId] (
	@ProductPageId INT
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
				INNER Join [UserManagement].[ProductPageControl] umppc ON umppc.[ControlId] =umc.[ControlId]
				INNER Join [UserManagement].[ProductPage] umpp ON umpp.[ProductPageId] = umppc.[ProductPageId]
				INNER JOIN [UserManagement].[ControlType] umct ON umct.ControlTypeId = umc.ControlTypeId
				LEFT OUTER JOIN [UserManagement].[Control] PUMC ON PUMC.ControlId = UMC.ParentControlId
	WHERE umpp.[ProductPageId] = @ProductPageId
END