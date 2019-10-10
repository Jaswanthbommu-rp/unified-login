CREATE PROCEDURE [Enterprise].[GetIdentityProviderTypeByRealPageId]
(
	@RealPageId UNIQUEIDENTIFIER
)
AS
BEGIN
	SELECT DISTINCT ips.Value AS 'AuthenticationType'                
        FROM Enterprise.Organization O
			 INNER JOIN Enterprise.Party P on O.PartyId = P.PartyId
			 INNER JOIN Ident.IdentityProviderType ipt ON O.[IdentityProviderTypeId]  = ipt.IdentityProviderTypeId
			 INNER JOIN Enterprise.PartyContactMechanism pcm on o.partyid = pcm.partyid
				and ipt.ContactMechanismId = pcm.ContactMechanismId
			 INNER JOIN Ident.IdentityProviderSettingType ipst ON ipt.IdentityproviderTypeId = ipst.IdentityProviderTypeId
			 INNER JOIN Ident.IdentityproviderSetting ips ON ips.IdentityProviderSettingTypeId = ipst.IdentityProviderSettingTypeId
			 WHERE P.RealPageId = @RealPageId 
			 AND ipst.Name =  'AuthenticationType'
END
GO