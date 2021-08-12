CREATE PROCEDURE [Security].[GetRoleTemplateByRoleTemplateName] (
 @RoleTemplateName varchar(100)
)    
AS    
BEGIN 
	SELECT 
		RoleTemplateId,
		RoleTemplateName
	FROM Security.RoleTemplate WHERE RoleTemplateName = @RoleTemplateName
END;