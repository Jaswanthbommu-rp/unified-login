CREATE PROCEDURE [Enterprise].[CheckOrgAdmin]
(
	@UserRealPageId UNIQUEIDENTIFIER,
	@OrgPartyId BIGINT
)
AS
BEGIN
	--EXEC Enterprise.CheckOrgAdmin @UserRealPageId='b4e4b63b-2087-f011-aa3c-005056b0e429',@OrgPartyId = 12980
	--EXEC Enterprise.CheckOrgAdmin @UserRealPageId='f634e176-12cf-e911-a9b4-005056b0e429',@OrgPartyId = 12980
	SELECT COUNT(ep.PartyId) AS AdminCount FROM Enterprise.Party EP
	JOIN Ident.UserLogin UL ON UL.PersonPartyId = EP.PartyId
	JOIN ident.UserLoginPersona ULP ON ULP.UserLoginId = UL.UserId
	JOIN Enterprise.OrganizationAdminUser OAU ON OAU.UserLoginPersonaId = ULP.UserLoginPersonaId
	WHERE EP.RealPageId = @UserRealPageId
	AND ulp.OrganizationPartyId = @OrgPartyId
END
GO