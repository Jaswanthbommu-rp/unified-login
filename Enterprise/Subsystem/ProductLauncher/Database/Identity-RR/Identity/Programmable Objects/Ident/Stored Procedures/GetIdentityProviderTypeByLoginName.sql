IF OBJECT_ID('[Ident].[GetIdentityProviderTypeByLoginName]') IS NOT NULL
	DROP PROCEDURE [Ident].[GetIdentityProviderTypeByLoginName];

GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_NULLS ON
GO
CREATE PROCEDURE [Ident].[GetIdentityProviderTypeByLoginName] (
	@LoginName varchar(255),
	@SettingTypeName NVARCHAR(50) = 'AuthenticationType'
)
AS
BEGIN
		
		DECLARE @NOW  DATETIME = GETUTCDATE();

        SELECT DISTINCT 
               ips.Value
        FROM   Ident.UserLogin ul
               JOIN Enterprise.PartyRelationship pr ON ul.PartyId = pr.PartyIdFrom
               JOIN Enterprise.RelationshipType rt ON rt.RelationshipTypeId = pr.PartyRelationshipTypeId
               JOIN Enterprise.PartyContactMechanism pcm ON pr.PartyIdTo = pcm.PartyId
               JOIN Ident.ContactMechanismIdentity cmi ON cmi.ContactMechanismId = pcm.ContactMechanismId
               JOIN Ident.IdentityProviderSetting ips ON ips.IdentityProviderSettingId = cmi.IdentityProviderSettingId
               JOIN Ident.IdentityProviderSettingType ipst ON ipst.IdentityProviderSettingTypeId = ips.IdentityProviderSettingTypeId
               JOIN Ident.IdentityProviderType ipt ON ipt.IdentityProviderTypeId = ipst.IdentityProviderTypeId
        WHERE  rt.Name = 'Employment'
               AND cmi.ContactMechanismIdentityId IS NOT NULL
			   AND ipst.Name = @SettingTypeName
               AND ul.LoginName = @LoginName
			   AND ((@NOW BETWEEN pcm.FromDate AND pcm.ThruDate) OR (@NOW >= pcm.FromDate AND pcm.ThruDate IS NULL)) 
			   AND ((@NOW BETWEEN pcm.FromDate AND pcm.ThruDate) OR (@NOW >= pcm.FromDate AND pcm.ThruDate IS NULL)) 
			   AND ((@NOW BETWEEN ul.FromDate AND ul.ThruDate) OR (@NOW >= ul.FromDate AND ul.ThruDate IS NULL));
END
GO
