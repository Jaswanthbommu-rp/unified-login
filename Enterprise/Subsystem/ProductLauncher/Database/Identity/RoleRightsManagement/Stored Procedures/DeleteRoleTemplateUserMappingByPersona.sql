CREATE PROCEDURE [Security].[DeleteRoleTemplateUserMappingByPersona] (
 @PersonaId   BIGINT  
)    
AS
BEGIN  
 DELETE 
	FROM Security.RoleTemplateUserMapping 
	WHERE PersonaId = @PersonaId 
END;