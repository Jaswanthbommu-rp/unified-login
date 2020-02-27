CREATE PROCEDURE [UserManagement].[GetControlWithTypeByTabTypeId] (  
	@TabTypeId INT  
)  
AS  
BEGIN  


	SELECT DISTINCT
		 umc.[ControlId]  
		,umc.[ParentControlId]  
		,umc.[ControlTypeId]  
		,umc.[UIId]  
		,umc.[DisplayName]  
		,umc.[DataSource]  
		,umc.[Sequence]  
		,umc.[CreatedBy]  
		,umc.[CreatedDate]  
		,umct.[Name] AS 'ControlType'
		,CASE WHEN pumc.ControlId IS NULL THEN 'False' ELSE 'True' END AS Children
		,CASE WHEN TD.ControlId IS NULL THEN 'False' ELSE 'True' END AS Dependency
	FROM	[UserManagement].[Control] umc  
	INNER Join [UserManagement].[TabTypeControl] umttc ON umttc.[ControlId] =umc.[ControlId]  
	INNER JOIN [UserManagement].[ControlType] umct ON umct.ControlTypeId = umc.ControlTypeId 
	LEFT OUTER JOIN [UserManagement].[Control] pumc ON umc.ControlId = pumc.ParentControlId
	LEFT OUTER JOIN [UserManagement].[TabDependency] TD ON UMC.ControlId = TD.ControlId
	WHERE umttc.[TabTypeId] = @TabTypeId  
END