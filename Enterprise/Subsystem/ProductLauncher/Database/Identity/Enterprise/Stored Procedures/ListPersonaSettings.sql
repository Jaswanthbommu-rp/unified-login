CREATE PROCEDURE [Enterprise].[ListPersonaSettings]
(@PartyId     BIGINT,
 @SettingName NVARCHAR(100) = NULL
)
AS
         BEGIN
             SELECT MST.Name,
                    MS.Value,
				MCS.MasterConfigurationSettingId as 'MasterConfigurationSettingId'
             FROM Enterprise.MasterConfigurationSetting MCS
                  INNER JOIN Enterprise.MasterConfiguration MC ON MC.MasterConfigurationId = MCS.MasterConfigurationId
                  INNER JOIN Enterprise.MasterSetting MS ON MCS.MasterSettingId = MS.MasterSettingId
                  INNER JOIN Enterprise.MasterSettingType MST ON MST.MasterSettingTypeId = MS.MasterSettingTypeId
                  INNER JOIN Enterprise.MasterConfigurationType MCT ON MCT.MasterConfigurationTypeId = MST.MasterConfigurationTypeId
             WHERE MCT.Name = 'Persona'
                   AND AttributeId = @PartyId
                   AND (MST.Name = @SettingName
                        OR @SettingName IS NULL);
         END;
