CREATE PROCEDURE [Security].[GetRolesForADGroupByPersona] @PersonaId BIGINT  
AS  
BEGIN  
	SELECT  agr.ADGroupRoleId, agr.ADGroupId, sr.RoleId,sr.RoleName, agr.ProductId
	FROM [Security].[ADGroupRole] agr 
	JOIN [Security].[Role] sr on agr.RoleId= sr.RoleId
	join [security].[ADGroupUser] sagu on sagu.ADGroupId = agr.ADGroupId
	WHERE sagu.PersonaId = @PersonaId
	ORDER BY sr.RoleName
END
GO
