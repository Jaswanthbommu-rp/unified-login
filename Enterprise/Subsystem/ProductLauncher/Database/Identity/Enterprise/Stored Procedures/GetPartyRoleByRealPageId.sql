CREATE PROCEDURE [Enterprise].[GetPartyRoleByRealPageId] (
	@RealPageId UNIQUEIDENTIFIER
)
AS
BEGIN
	SELECT	pr.PartyRoleId,
			pr.PartyId,
			pr.RoleTypeId,
			ert.Name
	FROM	Enterprise.Party pa
			INNER JOIN Person.Person p ON (pa.PartyId = p.PartyId)
			INNER JOIN Enterprise.PartyRole pr ON (p.PartyId = pr.PartyId)
			JOIN Enterprise.RoleType ert on ert.PartyRoleTypeId = pr.RoleTypeId
	WHERE	pa.RealPageId = @RealPageId
END