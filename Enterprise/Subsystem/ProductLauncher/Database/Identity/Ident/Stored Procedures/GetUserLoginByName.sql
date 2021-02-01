CREATE PROCEDURE [Ident].[GetUserLoginByName](
	 @EnterpriseUserName VARCHAR(255)
)
AS
BEGIN
	-- THIS PROC MAY NO LONGER BE USED!! DELETE IN THE FUTURE
    SELECT ul.UserId,
        ul.PersonPartyId,
        ul.[LoginName],
        ul.PasswordModifiedDate,
        p.RealPageId,
		ULP.StatusTypeId AS StatusId,
        ul.PasswordHash,
        ul.PasswordSalt,
        ULP.FromDate,
        ULP.ThruDate,
		ULP.StatusThruDate AS StatusThruDate,
		ul.LastLoginDate 'LastLogin',
        '' AS [TimeZoneOffset],--MS.Value [TimeZoneOffset],
		ul.TwoFactorEnabled [TwoFactorEnabled]
    FROM Ident.UserLogin ul
        JOIN Enterprise.Party p ON p.PartyId = UL.PersonPartyId
		INNER JOIN Ident.UserLoginPersona ULP ON ULP.UserLoginId = UL.UserId AND ULP.PrimaryOrganization = 1 -- ONLY JOIN TO THE PRIMARY ORG FOR THIS PROC
        --INNER JOIN Person.Persona PE ON ULP.UserLoginPersonaId = PE.UserLoginPersonaId
        --INNER JOIN Enterprise.MasterConfiguration MC ON MC.AttributeId = UL.UserId
        --INNER JOIN Enterprise.MasterConfigurationSetting MCS ON MC.MasterConfigurationId = MCS.MasterConfigurationId
        --INNER JOIN Enterprise.MasterSetting MS ON MCS.MasterSettingId = MS.MasterSettingId
        --INNER JOIN Enterprise.MasterSettingType MST ON MST.MasterSettingTypeId = MS.MasterSettingTypeId
        --INNER JOIN Enterprise.MasterConfigurationType MCT ON MCT.MasterConfigurationTypeId = MST.MasterConfigurationTypeId
    WHERE ul.[LoginName] = @EnterpriseUserName
        --AND MCT.Name = 'UserLogin'
        --AND MST.Name = 'TimeZone';
END;

