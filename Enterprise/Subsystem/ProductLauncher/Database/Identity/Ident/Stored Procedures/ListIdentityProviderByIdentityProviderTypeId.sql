CREATE PROCEDURE [Ident].[ListIdentityProviderByIdentityProviderTypeId](@IdentityProviderTypeId INT)
AS
     BEGIN
         SELECT pvt.ContactMechanismId AS ProviderPortfolioId,
                pvt.PartyId AS PortfolioIdId,
                pvt.AuthenticationMode,
                CONVERT(BIT, pvt.ValidateIssuer) AS ValidateIssuer,
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
         FROM
         (
             SELECT p.PartyId,
                    p.RealPageId,
                    ipt.ContactMechanismId,
                    ipt.IdentityProviderTypeId,
                    ipt.Name AS IdentityTypeName,
                    ipt.Description,
                    ipst.Name AS SettingTypeName,
                    ips.Value
             FROM Enterprise.Party p
                  INNER JOIN Enterprise.Organization o ON(p.PartyId = o.PartyId)
                  INNER JOIN Enterprise.PartyContactMechanism pcm ON(p.PartyId = pcm.PartyId)
                  INNER JOIN Ident.IdentityProviderType ipt ON ipt.ContactMechanismId = pcm.ContactMechanismId
                                                               AND ipt.IdentityProviderTypeId = o.[IdentityProviderTypeId]
                  JOIN Ident.IdentityProviderSettingType ipst ON ipt.IdentityProviderTypeId = ipt.IdentityProviderTypeId
                  INNER JOIN Ident.IdentityProviderSetting ips ON ipst.IdentityProviderSettingTypeId = ips.IdentityProviderSettingTypeId
             WHERE ipt.IdentityProviderTypeId = @IdentityProviderTypeId
         ) p PIVOT(MAX(Value) FOR SettingTypeName IN([AuthenticationType],
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
                                                     [ClientSecret])) AS pvt;
     END;

	GRANT EXECUTE ON  [Ident].[ListIdentityProviderByIdentityProviderTypeId] TO [identityserver]     