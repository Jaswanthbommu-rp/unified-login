CREATE PROCEDURE [Security].[GetRoleTemplateProductRolesMappings]
(
	@RoleTemplateId int, 
	@PartyId int
)
AS
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
FROM Security.RoleTemplate rt
	LEFT OUTER  JOIN Security.RoleTemplateProduct rtp ON rt.RoleTemplateId = rtp.RoleTemplateId
	LEFT OUTER JOIN Security.RoleTemplateProductRoleMapping rprm ON rprm.RoleTemplateProductId = rtp.RoleTemplateProductId
	LEFT OUTER JOIN Security.RoleTemplateAdditionalProductRoleMapping rtprm ON rtprm.RoleTemplateProductId = rtp.RoleTemplateProductId

WHERE rt.RoleTemplateId = @RoleTemplateId 
	AND rt.PartyID = @PartyId
ORDER BY rtp.ProductId