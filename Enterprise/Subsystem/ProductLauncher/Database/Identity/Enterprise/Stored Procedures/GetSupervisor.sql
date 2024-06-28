CREATE PROCEDURE [Enterprise].[GetSupervisor]
	@UserId BIGINT, 
	@OrgPartyId BIGINT
AS
BEGIN
	SELECT UL.UserId, USV.SuperVisorUserId, P.FirstName, P.LastName, UL.LoginName 
	FROM Ident.UserLogin UL
	JOIN Enterprise.UserSuperVisor USV ON UL.UserId = USV.SuperVisorUserId
	JOIN Ident.UserLoginPersona ULP ON ULP.UserLoginId = UL.UserId
	JOIN Person.Person P ON Ul.PersonPartyId = P.PartyId
	WHERE ULP.OrganizationPartyId = @OrgPartyId AND USV.UserId = @UserId
END
GO

