Create PROCEDURE [Enterprise].[GetOrganizationSettingValue] (
	@PartyId bigint ,
	@SettingName Nvarchar(50),
	@SettingValue nvarchar(4000) OUTPUT
)
AS
BEGIN
	
	SELECT @SettingValue = MS.Value            
        FROM Enterprise.MasterConfigurationSetting mcs
        INNER JOIN Enterprise.MasterConfiguration mc ON mc.MasterConfigurationId = mcs.MasterConfigurationId
        INNER JOIN Enterprise.MasterSetting ms ON mcs.MasterSettingId = ms.MasterSettingId
        INNER JOIN Enterprise.MasterSettingType mst ON mst.MasterSettingTypeId = ms.MasterSettingTypeId
        INNER JOIN Enterprise.MasterConfigurationType mct ON mct.MasterConfigurationTypeId = mst.MasterConfigurationTypeId
	WHERE MST.Name = @SettingName
	AND MCT.Name = 'Organization'
	AND  MC.AttributeId = @PartyId;
END;