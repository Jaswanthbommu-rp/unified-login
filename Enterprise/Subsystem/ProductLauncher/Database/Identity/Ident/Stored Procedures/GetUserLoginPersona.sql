CREATE PROCEDURE Ident.GetUserLoginPersona (
	@UserLoginPersonaId bigint = NULL,
	@UserLoginId bigint = NULL,
	@OrganizationPartyId bigint = NULL
)
AS
BEGIN
	IF @UserLoginPersonaId IS NOT NULL
	BEGIN
		SELECT	UserLoginPersonaId,
						UserLoginId,
						StatusTypeId,
						OrganizationPartyId,
						PrimaryOrganization,
						FromDate,
						ThruDate,
						StatusThruDate,
						IsDelegateAdmin
		FROM		Ident.UserLoginPersona
		WHERE	UserLoginPersonaId = @UserLoginPersonaId
	END
	ELSE
	BEGIN    
		SELECT	UserLoginPersonaId,
						UserLoginId,
						StatusTypeId,
						OrganizationPartyId,
						PrimaryOrganization,
						FromDate,
						ThruDate,
						StatusThruDate,
						IsDelegateAdmin
		FROM		Ident.UserLoginPersona
		WHERE
			UserLoginId = @UserLoginId
			AND OrganizationPartyId = @OrganizationPartyId
	END
END
