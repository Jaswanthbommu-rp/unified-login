CREATE PROCEDURE [Ident].[GetUserLoginOnly]
(
	@EnterpriseUserName VARCHAR(255) = NULL,
	@UserId				INT          = NULL,
	@RealPageId			UNIQUEIDENTIFIER = NULL,
	@PersonaId BIGINT = NULL
)
AS
BEGIN
	IF @RealPageId IS NOT NULL
	BEGIN
		SELECT 
			ul.UserId,
			ul.PersonPartyId [PartyId],
			P1.RealPageId,
			ul.[LoginName],
			ul.PasswordModifiedDate,
			ul.PasswordHash,
			ul.PasswordSalt,
			ulp.LastLoginDate [LastLogin],
			case when ipt.name = 'ID3' then 0 else 1 end as [Is3rdPartyIDP],
			ul.TwoFactorEnabled [TwoFactorEnabled]
		FROM Ident.UserLogin ul
			INNER JOIN Ident.UserLoginPersona ulp on ul.userid = ulp.userloginid
			INNER JOIN Person.Persona P on P.UserLoginPersonaId = ulp.UserLoginPersonaId
			INNER JOIN Enterprise.Party p1 on ul.PersonPartyId = p1.PartyId
			INNER JOIN Ident.IdentityProviderType ipt ON ul.IdentityProviderTypeId = ipt.IdentityProviderTypeId
		WHERE
			( 
				P1.RealPageId = @RealPageId
				AND P.PersonaId = @PersonaId
			)
	END
	ELSE IF @UserId IS NOT NULL
	BEGIN
		SELECT 
			ul.UserId,
			ul.PersonPartyId,
			P1.RealPageId,
			ul.[LoginName],
			ul.PasswordModifiedDate,
			ul.PasswordHash,
			ul.PasswordSalt,
			ulp.LastLoginDate [LastLogin],
			case when ipt.name = 'ID3' then 0 else 1 end as [Is3rdPartyIDP],
			ul.TwoFactorEnabled [TwoFactorEnabled]
		FROM Ident.UserLogin ul
			INNER JOIN Ident.UserLoginPersona ulp on ul.userid = ulp.userloginid
			INNER JOIN Person.Persona P on P.UserLoginPersonaId = ulp.UserLoginPersonaId
			INNER JOIN Enterprise.Party p1 on ul.PersonPartyId = p1.PartyId
			INNER JOIN Ident.IdentityProviderType ipt ON ul.IdentityProviderTypeId = ipt.IdentityProviderTypeId
		WHERE
			ul.UserId = @UserId
			AND P.PersonaId = @PersonaId
	END

	ELSE IF @EnterpriseUserName is not null
	BEGIN
		SELECT 
			ul.UserId,
			ul.PersonPartyId,
			P1.RealPageId,
			ul.[LoginName],
			ul.PasswordModifiedDate,
			ul.PasswordHash,
			ul.PasswordSalt,
			ulp.LastLoginDate [LastLogin],
			case when ipt.name = 'ID3' then 0 else 1 end as [Is3rdPartyIDP],
			ul.TwoFactorEnabled [TwoFactorEnabled]
		FROM Ident.UserLogin ul
			INNER JOIN Ident.UserLoginPersona ulp on ul.userid = ulp.userloginid
			INNER JOIN Person.Persona P on P.UserLoginPersonaId = ulp.UserLoginPersonaId
			INNER JOIN Enterprise.Party p1 on ul.PersonPartyId = p1.PartyId
			INNER JOIN Ident.IdentityProviderType ipt ON ul.IdentityProviderTypeId = ipt.IdentityProviderTypeId
		WHERE
			ul.LoginName = @EnterpriseUserName
			AND P.PersonaId = @PersonaId
	END
END;
