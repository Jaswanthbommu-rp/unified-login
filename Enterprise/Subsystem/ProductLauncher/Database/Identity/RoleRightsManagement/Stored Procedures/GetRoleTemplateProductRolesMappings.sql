CREATE PROCEDURE [Security].[GetRoleTemplateProductRolesMappings]    
(    
 @RoleTemplateId int,     
 @OrganizationRealPageId UNIQUEIDENTIFIER = NULL,    
    @PartyId BIGINT = NULL    
)    
AS    
BEGIN  


 create table #TempRTPersona(PersonaId bigint)
  insert into  #TempRTPersona
  select distinct RTP.PersonaId from [Security].RoleTemplate RT                
  LEFT OUTER JOIN [Security].RoleTemplateUserMapping RTP on RTP.RoleTemplateId = RT.roleTemplateId               
  LEFT OUTER JOIN Person.Persona P ON P.PersonaId = RTP.PersonaId  WHERE RT.RoleTemplateId = @RoleTemplateId

declare @userCount int = (select count(1) from #TempRTPersona where PersonaId is not null)
declare @personaIds nvarchar(max)= (select STRING_AGG(CONVERT(VARCHAR, PersonaId), ',')  FROM #TempRTPersona where PersonaId is not null)
  
SELECT             
 rt.RoleTemplateId            
 ,rt.PartyID            
 ,rt.RoleTemplateName            
 ,rt.RoleTemplateDescription            
 ,ISNULL(rtp.RoleTemplateProductId,0) AS RoleTemplateProductId            
 ,ISNULL(rtp.ProductId,0) as ProductId            
 ,ISNULL(rprm.RoleTemplateProductRoleMappingId,0) AS RoleTemplateProductRoleMappingId            
 ,ISNULL(rprm.ProductRoleId,0) as ProductRoleId            
 ,rprm.ProductRoleName            
 ,ISNULL(rtprm.RoleTemplateAdditionalProductRoleMappingId,0) AS RoleTemplateAdditionalProductRoleMappingId            
 ,rtprm.AttributeName            
 ,rtprm.AttributeValue            
 ,@userCount as Users           
 ,EP.[Name] AS ProductName          
 ,RTAT.TabJson as RoleTemplateProductBatch    
 ,rt.RoleTemplateNotification  
 ,@personaIds as PersonasAssociatedWithER  
FROM Security.RoleTemplate rt            
 INNER JOIN Enterprise.Party P ON P.PartyId = rt.PartyID            
 LEFT OUTER  JOIN [Security].RoleTemplateProduct rtp ON rt.RoleTemplateId = rtp.RoleTemplateId            
 LEFT OUTER JOIN [Security].RoleTemplateProductRoleMapping rprm ON rprm.RoleTemplateProductId = rtp.RoleTemplateProductId            
 LEFT OUTER JOIN [Security].RoleTemplateAdditionalProductRoleMapping rtprm ON rtprm.RoleTemplateProductId = rtp.RoleTemplateProductId            
        
 LEFT OUTER JOIN [Security].[RoleTemplateProductAdditionalTab] RTAT ON RTAT.RoleTemplateId = RT.RoleTemplateId        
 INNER JOIN Enterprise.Product EP ON EP.ProductId = rtp.ProductId          
WHERE rt.RoleTemplateId = @RoleTemplateId             
 AND (P.RealPageId = @OrganizationRealPageId OR @OrganizationRealPageId IS NULL)            
 AND (P.PartyId = @PartyId OR @PartyId IS NULL)            
ORDER BY rtp.ProductId            


 drop table if exists #TempRTPersona
END