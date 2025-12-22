CREATE PROCEDURE [Ident].[GetUserProfileByUserIds]
(
	@OrganizationPartyId BIGINT
	,@UserIds [Enterprise].[BigIntListType] READONLY
)
AS
BEGIN

	SELECT DISTINCT PP.FirstName,
			PP.LastName,
			UL.LoginName,
			CUST.TypeName AS CreateUserSourceType,
			UL.UserId,
			EP.RealPageId
	FROM Ident.UserLogin UL
	JOIN Person.Person PP ON PP.PartyId = ul.PersonPartyId
	JOIN ident.UserLoginPersona ULP ON ULP.UserLoginId = UL.UserId
	JOIN Enterprise.CreateUserSourceType CUST ON CUST.TypeId = ul.CreateUserSourceId
	JOIN Enterprise.Party EP ON EP.PartyId = UL.PersonPartyId
	JOIN @UserIds UIDS ON ul.UserId = UIDS.Id
	WHERE ULP.OrganizationPartyId = @OrganizationPartyId
	AND ULP.PrimaryOrganization = 1 
END
GO