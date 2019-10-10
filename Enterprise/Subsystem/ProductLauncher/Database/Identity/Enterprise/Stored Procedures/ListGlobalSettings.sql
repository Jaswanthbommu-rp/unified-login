CREATE PROCEDURE [Enterprise].[ListGlobalSettings]
AS
         BEGIN
             SELECT MST.Name AS 'SettingName',
                    MS.Value AS 'Value',
                    MCS.MasterConfigurationSettingId AS 'MasterConfigurationSettingId'
             FROM Enterprise.MasterConfigurationSetting MCS
                  INNER JOIN Enterprise.MasterConfiguration MC ON MC.MasterConfigurationId = MCS.MasterConfigurationId
                  INNER JOIN Enterprise.MasterSetting MS ON MCS.MasterSettingId = MS.MasterSettingId
                  INNER JOIN Enterprise.MasterSettingType MST ON MST.MasterSettingTypeId = MS.MasterSettingTypeId
                  INNER JOIN Enterprise.MasterConfigurationType MCT ON MCT.MasterConfigurationTypeId = MST.MasterConfigurationTypeId
             WHERE MCT.Name = 'Global';
         END;