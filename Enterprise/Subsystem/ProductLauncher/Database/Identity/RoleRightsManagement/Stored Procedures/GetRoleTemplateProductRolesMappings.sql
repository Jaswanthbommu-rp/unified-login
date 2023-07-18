CREATE PROCEDURE [Security].[GetRoleTemplateProductRolesMappings]
(
	@RoleTemplateId int, 
	@OrganizationRealPageId UNIQUEIDENTIFIER = NULL,
    @PartyId BIGINT = NULL,
    @UserPersonaId int = NULL

)
AS
BEGIN

 select distinct rtp.productId  into #RoleTemplateProductIds from [Security].RoleTemplate RT      
  JOIN  [Security].RoleTemplateProduct rtp on rtp.RoleTemplateId = RT.roleTemplateId
  JOIN [Security].RoleTemplateUserMapping RTUP on RTUP.RoleTemplateId = RT.roleTemplateId  
  JOIN Person.Persona P ON P.PersonaId = RTUP.PersonaId  
  JOIN Enterprise.PersonaConfiguration PC on PC.PersonaId  = RTUP.PersonaId and rtp.productId = PC.productId
  where RT.RoleTemplateId =  @RoleTemplateId and P.PersonaId = @UserPersonaId and PC.ThruDate is null and PC.StatusTypeId = 8

  insert into #RoleTemplateProductIds
  select 3 


;WITH
RoleTemplateUser   
 AS    
 (    
  SELECT     
   RT.RoleTemplateId as RoleTemplateId     
   ,count(RTP.PersonaId) as UserCount    
  FROM Security.RoleTemplate RT    
  LEFT OUTER JOIN Security.RoleTemplateUserMapping RTP on RTP.RoleTemplateId = RT.roleTemplateId   
  LEFT OUTER JOIN Person.Persona P ON P.PersonaId = RTP.PersonaId  
  GROUP BY RT.RoleTemplateId    
 ) 

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
	,RTUM.UserCount as Users 
FROM Security.RoleTemplate rt
	INNER JOIN Enterprise.Party P ON
		P.PartyId = rt.PartyID
	LEFT OUTER  JOIN Security.RoleTemplateProduct rtp ON rt.RoleTemplateId = rtp.RoleTemplateId
	LEFT OUTER JOIN Security.RoleTemplateProductRoleMapping rprm ON rprm.RoleTemplateProductId = rtp.RoleTemplateProductId
	LEFT OUTER JOIN Security.RoleTemplateAdditionalProductRoleMapping rtprm ON rtprm.RoleTemplateProductId = rtp.RoleTemplateProductId
	LEFT OUTER JOIN RoleTemplateUser RTUM ON RTUM.RoleTemplateId = RT.RoleTemplateId
WHERE rt.RoleTemplateId = @RoleTemplateId 
	AND (P.RealPageId = @OrganizationRealPageId OR @OrganizationRealPageId IS NULL)
	AND (P.PartyId = @PartyId OR @PartyId IS NULL)
	AND rtp.ProductId in (select ProductId from #RoleTemplateProductIds)
ORDER BY rtp.ProductId
END