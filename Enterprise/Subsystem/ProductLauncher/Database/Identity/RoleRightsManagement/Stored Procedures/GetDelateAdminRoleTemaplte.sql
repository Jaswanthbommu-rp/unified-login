
CREATE   PROCEDURE [Security].[GetDelateAdminRoleTemaplte]  
(    
 @UserLoginPersonaId BIGINT    
)    
AS    
BEGIN    
    SELECT     
  EUR.UserLoginPersonaId,    
  EUR.RoleTemplateId  
      
 FROM  [Security].[DelegatedAdminRoleTemplate] EUR     
  INNER JOIN [Security].[RoleTemplate] TPR ON EUR.RoleTemplateId = TPR.RoleTemplateId    
 WHERE    
  EUR.UserLoginPersonaId = @UserLoginPersonaId    
END