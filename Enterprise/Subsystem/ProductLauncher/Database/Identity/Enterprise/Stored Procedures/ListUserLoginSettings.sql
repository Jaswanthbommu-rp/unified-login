CREATE PROCEDURE [Enterprise].[ListUserLoginSettings]
(@PartyId     BIGINT        = NULL,
 @UserId      BIGINT        = NULL,
 @SettingName NVARCHAR(100) = NULL
)
AS
         BEGIN
             DECLARE @UserLoginId BIGINT;
             IF @PartyId IS NOT NULL
                OR @PartyId <> ''
                 BEGIN
                     SELECT @UserLoginId = U.UserId
                     FROM Person.Persona P
					 INNER JOIN Ident.UserLoginPersona ULP ON P.UserLoginPersonaId = ULP.UserLoginPersonaId
                     INNER JOIN Ident.UserLogin U ON U.UserId = ULP.UserLoginId--P.UserId = U.UserId
                     WHERE U.PersonPartyId = @PartyId;
                 END;
                 ELSE
                 BEGIN
                     SET @UserLoginId = @UserId;
                 END;
             SELECT MST.Name AS 'SettingName',
                    MS.Value AS 'Value',
                    MCS.MasterConfigurationSettingId AS 'MasterConfigurationSettingId'
             FROM Enterprise.MasterConfigurationSetting MCS
                  INNER JOIN Enterprise.MasterConfiguration MC ON MC.MasterConfigurationId = MCS.MasterConfigurationId
                  INNER JOIN Enterprise.MasterSetting MS ON MCS.MasterSettingId = MS.MasterSettingId
                  INNER JOIN Enterprise.MasterSettingType MST ON MST.MasterSettingTypeId = MS.MasterSettingTypeId
                  INNER JOIN Enterprise.MasterConfigurationType MCT ON MCT.MasterConfigurationTypeId = MST.MasterConfigurationTypeId
             WHERE MCT.Name = 'UserLogin'
                   AND MC.AttributeId = @UserLoginId
                   AND (MST.Name = @SettingName
                        OR @SettingName IS NULL);
         END;