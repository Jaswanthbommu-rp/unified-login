CREATE PROCEDURE [Ident].[GetIdentityProviderTypeByLoginName]
    (
        @LoginName VARCHAR(255) ,
        @SettingTypeName NVARCHAR(50) = 'AuthenticationType'
    )
AS
    BEGIN

        DECLARE @NOW DATETIME = GETUTCDATE();

        SELECT DISTINCT ips.Value AS 'AuthenticationType' ,
               ipt.ContactMechanismId 'ContactMechanismId'
        FROM Ident.UserLogin UL
			 INNER JOIN Ident.IdentityProviderType ipt ON UL.[IdentityProviderTypeId]  = ipt.IdentityProviderTypeId
			 INNER JOIN Ident.IdentityProviderSettingType ipst ON ipt.IdentityproviderTypeId = ipst.IdentityProviderTypeId
			 INNER JOIN Ident.IdentityproviderSetting ips ON ips.IdentityProviderSettingTypeId = ipst.IdentityProviderSettingTypeId
			 WHERE UL.Loginname = @LoginName 
			 AND ipst.Name = @SettingTypeName
    END;