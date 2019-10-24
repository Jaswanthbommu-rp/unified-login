GO

DECLARE @MasterSettingTypeName nvarchar(200) = 'CustomFields',
	@MasterConfigurationTypeName nvarchar(100) = 'UserLogin'

DECLARE	@MasterConfiguration TABLE (
	MasterConfigurationId bigint
)

DECLARE @MasterSetting TABLE (
	MasterSettingId bigint
)

INSERT INTO @MasterSetting (
	MasterSettingId
)
SELECT	DISTINCT ems.MasterSettingId
FROM	Enterprise.MasterConfigurationType emct
			INNER JOIN Enterprise.MasterSettingType emst ON emct.MasterConfigurationTypeId = emst.MasterConfigurationTypeId
			INNER JOIN Enterprise.MasterSetting ems ON emst.MasterSettingTypeId = ems.MasterSettingTypeId
			LEFT OUTER JOIN Enterprise.MasterConfigurationSetting emcs ON emcs.MasterSettingId = ems.MasterSettingId
			LEFT OUTER JOIN Enterprise.MasterConfiguration emc ON emc.MasterConfigurationId = emcs.MasterConfigurationId
WHERE	emct.Name = @MasterConfigurationTypeName
 AND		emst.Name = @MasterSettingTypeName
 
INSERT INTO @MasterConfiguration (
	MasterConfigurationId
)
SELECT	DISTINCT emc.MasterConfigurationId
FROM	Enterprise.MasterConfiguration emc
			INNER JOIN Enterprise.MasterConfigurationType emct ON (emc.MasterConfigurationTypeId = emct.MasterConfigurationTypeId)
			INNER JOIN Enterprise.MasterConfigurationSetting emcs ON emc.MasterConfigurationId = emcs.MasterConfigurationId
			INNER JOIN Enterprise.MasterSetting ems ON emcs.MasterSettingId = ems.MasterSettingId
			INNER JOIN Enterprise.MasterSettingType emst ON ems.MasterSettingTypeId = emst.MasterSettingTypeId
WHERE	emct.Name = @MasterConfigurationTypeName
 AND		emst.Name = @MasterSettingTypeName

DELETE emcs
FROM	Enterprise.MasterConfigurationSetting emcs
			INNER JOIN @MasterSetting ms ON (emcs.MasterSettingId = ms.MasterSettingId)

DELETE	emc
FROM	Enterprise.MasterConfiguration emc
			INNER JOIN @MasterConfiguration mc ON (emc.MasterConfigurationId = mc.MasterConfigurationId)
			INNER JOIN Enterprise.MasterConfigurationSetting emcs ON emc.MasterConfigurationId = emcs.MasterConfigurationId
			INNER JOIN Enterprise.MasterSetting ems ON emcs.MasterSettingId = ems.MasterSettingId
			INNER JOIN Enterprise.MasterSettingType emst ON ems.MasterSettingTypeId = emst.MasterSettingTypeId
WHERE	emst.Name = @MasterSettingTypeName

DELETE ems
FROM	Enterprise.MasterSetting ems
			INNER JOIN @MasterSetting ms ON (ems.MasterSettingId = ms.MasterSettingId)

DELETE emst
FROM	Enterprise.MasterSettingType emst
WHERE	emst.Name = @MasterSettingTypeName
GO