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
		LEFT OUTER JOIN Security.RoleTemplateProduct RTP on RTP.RoleTemplateId = RT.roleTemplateId and rtp.ProductId <> 3
		GROUP BY RT.RoleTemplateId
	),
	RoleTemplateUser 
	AS  
	(  
		SELECT   
			RT.RoleTemplateId as RoleTemplateId   
			,count(RTP.PersonaId) as UserCount  
		FROM Security.RoleTemplate RT  
		LEFT OUTER JOIN Security.RoleTemplateUserMapping RTP on RTP.RoleTemplateId = RT.roleTemplateId 
		LEFT OUTER JOIN Person.Persona P ON P.PersonaId = RTP.PersonaId
	    INNER JOIN ident.UserLoginPersona ulp ON ulp.UserLoginPersonaId = p.UserLoginPersonaId where ulp.statustypeid <>24
		GROUP BY RT.RoleTemplateId  
	) 

	SELECT 
			RT.RoleTemplateId
			,RT.RoleTemplateName
			,RT.RoleTemplateDescription
			,RT.RoleType
			,RTP.productCount as Products
			,RTUM.UserCount as Users
		FROM Security.RoleTemplate RT
			INNER JOIN RoleTemplateProduct RTP on RTP.RoleTemplateId = RT.RoleTemplateId
			INNER JOIN RoleTemplateUser RTUM ON RTUM.RoleTemplateId = RT.RoleTemplateId
		wHERE RT.partyId = @PartyId
END

