CREATE PROCEDURE [Security].[GetUserRoleTemplate] (
 @PersonaId		 BIGINT
)  
AS  
BEGIN    
 
	DECLARE @RoleTemplateId INT
	SELECT @RoleTemplateId =  ISNULL(RoleTemplateId,0) 
	FROM Security.RoleTemplateUserMapping
	WHERE PersonaId = @PersonaId
	
	SELECT	@RoleTemplateId 
 
END;