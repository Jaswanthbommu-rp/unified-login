CREATE PROCEDURE [Hots].[ListHotsBaseOrganizationUsers] (
	@OrganizationId bigint
)
AS
BEGIN
	Declare @AdminPersonaID BIGINT;
	
	SELECT	@AdminPersonaID = P.PersonaId	
	FROM	Enterprise.OrganizationAdminUser OAU
			INNER JOIN Ident.UserLoginPersona ULP ON OAU.UserLoginPersonaId = ULP.UserLoginPersonaId
			INNER JOIN Person.Persona P ON P.UserLoginPersonaId = ULP.UserLoginPersonaId
	WHERE OAU.OrganizationPartyId = @OrganizationId

	SELECT	ul.UserId AS 'UserId',
			p.PersonaId AS 'PersonaId',
			pa.RealPageId AS 'UserRealPageId',
			@AdminPersonaID AS 'AdminUserPersonaId'
	FROM	Ident.UserLogin ul
			INNER JOIN Enterprise.Party pa ON pa.PartyId = ul.PersonPartyId
			INNER JOIN Ident.UserLoginPersona ULP ON ULP.UserLoginId = ul.UserId
			INNER JOIN Person.Persona p ON p.UserLoginPersonaId = ULP.UserLoginPersonaId
			INNER JOIN Enterprise.Organization o ON o.PartyId = ULP.OrganizationPartyId
			INNER JOIN Person.Person pe ON pe.PartyId = ul.PersonPartyId
			INNER JOIN Enterprise.StatusType st ON st.StatusTYpeId = ULP.StatusTypeId 
	WHERE	o.PartyId = @OrganizationId 		
	AND		P.PersonaId NOT IN (@AdminPersonaID)
	And st.Name = 'Active'
	
END