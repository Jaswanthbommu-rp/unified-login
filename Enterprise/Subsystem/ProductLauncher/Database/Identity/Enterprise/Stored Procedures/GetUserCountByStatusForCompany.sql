CREATE PROCEDURE [Enterprise].[GetUserCountByStatusForCompany]
(
	@companyId UNIQUEIDENTIFIER
)
AS
BEGIN
	SELECT 
		COUNT(p.personaId) AS [ActiveUsers]
		FROM ident.userlogin ul
		INNER JOIN ident.UserLoginPersona ulp ON ulp.UserLoginId = ul.UserId
		INNER JOIN person.persona p ON p.UserLoginPersonaId = ulp.UserLoginPersonaId
		INNER JOIN Enterprise.Organization o ON o.PartyId = ulp.OrganizationPartyId
		INNER JOIN Enterprise.Party P2 ON P2.PartyId=o.PartyId
		WHERE P2.RealPageId=@CompanyId
		AND ulp.StatusTypeId IN (1,2,3,12)
END
