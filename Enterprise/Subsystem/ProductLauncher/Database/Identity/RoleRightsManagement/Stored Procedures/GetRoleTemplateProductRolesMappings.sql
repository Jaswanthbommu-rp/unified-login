CREATE PROCEDURE [Security].[GetRoleTemplateProductRolesMappings]
(
	@RoleTemplateId int, 
	@OrganizationRealPageId UNIQUEIDENTIFIER = NULL,
    @PartyId BIGINT = NULL
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
	INNER JOIN Enterprise.Party P ON
		P.PartyId = rt.PartyID
	LEFT OUTER  JOIN Security.RoleTemplateProduct rtp ON rt.RoleTemplateId = rtp.RoleTemplateId
	LEFT OUTER JOIN Security.RoleTemplateProductRoleMapping rprm ON rprm.RoleTemplateProductId = rtp.RoleTemplateProductId
	LEFT OUTER JOIN Security.RoleTemplateAdditionalProductRoleMapping rtprm ON rtprm.RoleTemplateProductId = rtp.RoleTemplateProductId

WHERE rt.RoleTemplateId = @RoleTemplateId 
	AND (P.RealPageId = @OrganizationRealPageId OR @OrganizationRealPageId IS NULL)
	AND (P.PartyId = @PartyId OR @PartyId IS NULL)
ORDER BY rtp.ProductId