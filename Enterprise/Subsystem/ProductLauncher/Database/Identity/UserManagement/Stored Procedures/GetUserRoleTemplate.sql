CREATE PROCEDURE [Security].[GetUserRoleTemplate] (
 @PersonaId		 BIGINT
)  
AS  
BEGIN
	SELECT  
		ISNULL(RT.RoleTemplateId,0) AS RoleTemplateId
		,RT.RoleTemplateName
		,userMapping.PersonaId
 
	FROM Security.RoleTemplateUserMapping userMapping 
		INNER JOIN Security.RoleTemplate RT ON RT.RoleTemplateId = userMapping.RoleTemplateId AND userMapping.PersonaId = @PersonaId
	WHERE PersonaId = @PersonaId
END;