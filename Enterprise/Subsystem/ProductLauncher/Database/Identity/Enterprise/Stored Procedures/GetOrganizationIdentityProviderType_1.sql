CREATE PROCEDURE [Enterprise].[GetOrganizationIdentityProviderType](@RealPageId UNIQUEIDENTIFIER)
AS
         BEGIN
             SELECT DISTINCT
                    ips.Value AS 'AuthenticationType',
                    ipt.ContactMechanismId AS 'ContactMechanismId',
					ipt.description As Name  
             FROM Enterprise.PartyContactMechanism PCM
                  INNER JOIN Ident.IdentityProviderType IPT ON IPT.ContactMechanismId = PCM.ContactMechanismId
                  INNER JOIN Enterprise.Organization O ON PCM.PartyId = O.PartyId
                  INNER JOIN Enterprise.Party P ON P.PartyId = O.PartyId
                  INNER JOIN Ident.IdentityProviderSettingType ipst ON ipt.IdentityproviderTypeId = ipst.IdentityProviderTypeId
                  INNER JOIN Ident.IdentityproviderSetting ips ON ips.IdentityProviderSettingTypeId = ipst.IdentityProviderSettingTypeId
             WHERE P.RealPageId = @RealPageId
                   AND ipst.Name = 'AuthenticationType';
         END;