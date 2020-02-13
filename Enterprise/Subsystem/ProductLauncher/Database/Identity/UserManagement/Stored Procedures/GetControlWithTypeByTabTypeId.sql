-- =============================================
-- Author:		Monte Jennings
-- Create date: 
-- Description: Returns controls with the TabTypeId indicated.
-- =============================================
CREATE PROCEDURE [UserManagement].[GetControlWithTypeByTabTypeId] (
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
				INNER JOIN [UserManagement].[ControlType] umct ON umct.ControlTypeId = umc.ControlTypeId
				LEFT OUTER JOIN [UserManagement].[Control] PUMC ON PUMC.ControlId = UMC.ParentControlId
	WHERE umppc.[TabTypeId] = @TabTypeId
END