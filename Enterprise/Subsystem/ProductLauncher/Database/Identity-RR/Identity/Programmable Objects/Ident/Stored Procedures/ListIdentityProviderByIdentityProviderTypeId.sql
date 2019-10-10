IF OBJECT_ID('[Ident].[ListIdentityProviderByIdentityProviderTypeId]') IS NOT NULL
	DROP PROCEDURE [Ident].[ListIdentityProviderByIdentityProviderTypeId];

GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_NULLS ON
GO
CREATE PROCEDURE [Ident].[ListIdentityProviderByIdentityProviderTypeId] (
	@IdentityProviderTypeId int
)
AS
BEGIN
	SELECT	pvt.ContactMechanismId AS ProviderPortfolioId,
			pvt.PartyId AS PortfolioIdId,
			pvt.AuthenticationMode,
			CONVERT(bit, pvt.ValidateIssuer) AS ValidateIssuer,
			pvt.IdentityTypeName AS ProviderName,
			pvt.Description,
			pvt.AuthenticationType,
			pvt.Caption,
			pvt.ProviderClientId,
			pvt.AuthorityUri,
			pvt.PostLogoutRedirectUri,
			pvt.RedirectUri,
			pvt.TokenValidationAuthenticationType,
			pvt.Scope,
			pvt.OktaEntityId,
			pvt.OktaMetadataLocation,
			pvt.ClientSecret
	FROM	(
			SELECT	p.PartyId,
					p.RealPageId,
					cmi.ContactMechanismId,
					ipt.IdentityProviderTypeId,
					ipt.Name AS IdentityTypeName,
					ipt.Description,
					ipst.Name AS SettingTypeName,
					ips.Value
			FROM	Enterprise.Party p
					INNER JOIN Enterprise.Organization o ON (p.PartyId = o.PartyId)
					INNER JOIN Enterprise.PartyContactMechanism pcm ON (p.PartyId = pcm.PartyId)
					INNER JOIN Ident.ContactMechanismIdentity cmi ON (pcm.ContactMechanismId = cmi.ContactMechanismId)
					INNER JOIN [Ident].[IdentityProviderSetting] ips ON (cmi.IdentityProviderSettingId = ips.IdentityProviderSettingId)
					INNER JOIN [Ident].[IdentityProviderSettingType] ipst ON (ips.IdentityProviderSettingTypeId = ipst.IdentityProviderSettingTypeId)
					INNER JOIN [Ident].[IdentityProviderType] ipt ON ipst.IdentityProviderTypeId = ipt.IdentityProviderTypeId
			WHERE	ipt.IdentityProviderTypeId = @IdentityProviderTypeId
			) p
	PIVOT	(
			MAX(Value) FOR SettingTypeName IN (
				[AuthenticationType],
				[Caption],
				[ProviderClientId],
				[AuthorityUri],
				[PostLogoutRedirectUri],
				[RedirectUri],
				[AuthenticationMode],
				[ValidateIssuer],
				[TokenValidationAuthenticationType],
				[Scope],
				[OktaEntityId],
				[OktaMetadataLocation],
				[ClientSecret]
				)
			) AS pvt;
END
GO
