CREATE PROCEDURE [Security].[GetRoleTemplateByRoleTemplateName] (  
 @RoleTemplateName varchar(100)  , @PartyId bigint
)      
AS      
BEGIN   
 SELECT   
  RoleTemplateId,  
  RoleTemplateName  
 FROM [Security].RoleTemplate WHERE RoleTemplateName = @RoleTemplateName and PartyID = @PartyId
END;