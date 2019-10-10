CREATE PROCEDURE [Ident].[ListPendingUsers]
AS
BEGIN
	SELECT 
		P.RealPageId as UserRealPageId,
		P1.RealPageId as OrganizationRealPageId,
		ULP.StatusThruDate
	FROM Ident.UserLogin U
		INNER JOIN Ident.UserLoginPersona ULP ON ULP.UserLoginId = U.UserId
		INNER JOIN Enterprise.Party P ON U.PersonPartyId = P.PartyId
		INNER JOIN Enterprise.Party P1 ON ULP.OrganizationPartyId = P1.PartyId
	WHERE 
		ULP.StatusTypeId in (2,12) --Pending or force reset password
		AND ULP.StatusThruDate IS NOT NULL
		AND ULP.StatusThruDate < GETUTCDATE()
	ORDER BY
		P1.PartyId, 
		P.PartyId
END 