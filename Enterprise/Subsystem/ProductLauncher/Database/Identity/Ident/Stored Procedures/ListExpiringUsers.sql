CREATE PROCEDURE [Ident].[ListExpiringUsers]
AS
BEGIN
	DECLARE @Now datetime = GETUTCDATE()

	SELECT	UL.UserId,
					UL.LoginName,
					P1.RealPageId as UserRealPageId,
					P2.RealPageId as OrganizationRealPageId
	FROM		Ident.UserLogin UL
					INNER JOIN Ident.UserLoginPersona ULP ON ULP.UserLoginId = UL.UserId
					INNER JOIN Enterprise.Party P1 ON UL.PersonPartyId = P1.PartyId
					INNER JOIN Enterprise.Party P2 ON ULP.OrganizationPartyId = P2.PartyId
	WHERE	CAST(ULP.ThruDate AS DATE) = CAST(DATEADD(dd, -1, @Now) AS DATE)
	ORDER BY
		P2.PartyId, 
		P1.PartyId
END;