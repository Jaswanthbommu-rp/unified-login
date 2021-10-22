CREATE PROCEDURE [Security].[GetExcludedCompaniesForRole](
    @RoleId int
)
AS
BEGIN
	SELECT OG.PartyId, OG.[Name]
	FROM [Security].OrganizationOverRideRole ORR
	INNER JOIN Enterprise.Organization OG ON OG.PartyId = ORR.OrgPartyId
	WHERE ORR.RoleId = @RoleId
END

