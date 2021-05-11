--exec [Security].[GetRoleTemplateProductRolesMappings] 11,350
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
	,rtp.RoleTemplateProductId
	,rtp.ProductId
	,ISNULL(rprm.RoleTemplateProductRoleMappingId,0) AS RoleTemplateProductRoleMappingId
	,rprm.ProductRoleId
	,rprm.ProductRoleName
	,ISNULL(rtprm.RoleTemplateAdditionalProductRoleMappingId,0) AS RoleTemplateAdditionalProductRoleMappingId
	,rtprm.AttributeName
	,rtprm.AttributeValue
FROM Security.RoleTemplate rt
	INNER JOIN Security.RoleTemplateProduct rtp ON rt.RoleTemplateId = rtp.RoleTemplateId
	LEFT OUTER JOIN Security.RoleTemplateProductRoleMapping rprm ON rprm.RoleTemplateProductId = rtp.RoleTemplateProductId
	LEFT OUTER JOIN Security.RoleTemplateAdditionalProductRoleMapping rtprm ON rtprm.RoleTemplateProductId = rtp.RoleTemplateProductId

WHERE rt.RoleTemplateId = @RoleTemplateId 
	AND rt.PartyID = @PartyId
ORDER BY rtp.ProductId