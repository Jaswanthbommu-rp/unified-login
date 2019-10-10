CREATE PROCEDURE [Enterprise].[ListOrganizationStatusByUserId](
	@UserId INT
)
AS
BEGIN
	DECLARE @NOW  DATETIME = GETUTCDATE();

	SELECT	
			ep.PartyId,
			ep.RealPageId,
			o.Name,
			ulp.StatusTypeId,
			ulp.FromDate,
			ulp.ThruDate,
			ulp.StatusThruDate,
			ulp.PrimaryOrganization
	FROM 
		Ident.UserLogin ul 
		INNER JOIN Ident.UserLoginPersona ulp on ul.userid = ulp.UserLoginId
		INNER JOIN Enterprise.Organization o on ulp.OrganizationPartyId = o.PartyId
		INNER JOIN Enterprise.Party ep on o.PartyId = ep.PartyId
	WHERE	
		ul.userId = @UserId
END;