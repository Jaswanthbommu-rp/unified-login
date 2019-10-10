CREATE PROCEDURE Ident.GetUserLoginPersona (
	@UserLoginPersonaId bigint = NULL,
	@UserLoginId bigint = NULL,
	@OrganizationPartyId bigint = NULL
)
AS
BEGIN
	SELECT	UserLoginPersonaId,
					UserLoginId,
					StatusTypeId,
					OrganizationPartyId,
					PrimaryOrganization,
					FromDate,
					ThruDate,
					StatusThruDate
	FROM		Ident.UserLoginPersona
	WHERE	((@UserLoginPersonaId IS NULL) OR (UserLoginPersonaId = @UserLoginPersonaId))
	AND			((@UserLoginId IS NULL) OR (UserLoginId = @UserLoginId))
	AND			((@OrganizationPartyId IS NULL) OR (OrganizationPartyId = @OrganizationPartyId))
END