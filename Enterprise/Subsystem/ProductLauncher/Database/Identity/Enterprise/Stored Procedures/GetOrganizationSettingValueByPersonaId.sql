Create PROCEDURE [Enterprise].[GetOrganizationSettingValueByPersonaId] (
	@PersonaId bigint ,
	@SettingName Nvarchar(50),
	@SettingValue nvarchar(4000) OUTPUT
)
AS
BEGIN
	Declare @PartyId bigint

	  SELECT @PartyId = O.PartyId 
	  From Person.Persona PE   
	  INNER JOIN Ident.UserLoginPersona ULP ON PE.UserLoginPersonaId = ULP.UserLoginPersonaId
	  INNER JOIN Enterprise.Organization O ON O.PartyId = ulp.OrganizationPartyId	  
	  WHERE  pe.PersonaId = @PersonaId

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