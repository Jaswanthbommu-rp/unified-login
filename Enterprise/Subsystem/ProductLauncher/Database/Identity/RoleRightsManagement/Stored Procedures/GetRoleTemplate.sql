--EXEC [Security].[GetRoleTemplate] 350
CREATE PROCEDURE [Security].[GetRoleTemplate]
(
 @PartyId BIGINT
)
AS
BEGIN

	with RoleTemplateProduct
	AS
	(
		SELECT 
			RT.RoleTemplateId as RoleTemplateId 
			,count(ProductID) as ProductCount
		FROM Security.RoleTemplate RT
			LEFT OUTER JOIN Security.RoleTemplateProduct RTP on RTP.RoleTemplateId = RT.roleTemplateId
		GROUP BY RT.RoleTemplateId
	)

	SELECT 
			RT.RoleTemplateId
			,RT.RoleTemplateName
			,RT.RoleTemplateDescription
			,RT.RoleType
			,RTP.productCount as Products
			,0 as Users --TODO
		FROM Security.RoleTemplate RT
			INNER JOIN RoleTemplateProduct RTP on RTP.RoleTemplateId = RT.RoleTemplateId
		wHERE RT.partyId = @PartyId
END

