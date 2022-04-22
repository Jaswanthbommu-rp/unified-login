CREATE PROCEDURE [Security].[GetRolesForADGroup] @adGroupId int  
AS  
BEGIN  
	SELECT  agr.ADGroupRoleId, agr.ADGroupId, sr.RoleId,sr.RoleName, agr.ProductId
	FROM [Security].[ADGroupRole] agr 
	JOIN [Security].[Role] sr on agr.RoleId= sr.RoleId
	WHERE agr.ADGroupId = @adGroupId  
	ORDER BY sr.RoleName
END
GO