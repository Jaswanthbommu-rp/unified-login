Create PROCEDURE [Hots].[ListHotsBaseOrganizationUsers] (
	@OrganizationId int
)
AS
BEGIN
	DECLARE @NOW DATETIME = GETUTCDATE();
	Declare @AdminPersonaID BIGINT;

	SELECT	@AdminPersonaID = ULP.UserLoginPersonaId	
	FROM	Enterprise.MasterConfigurationType mct
			INNER JOIN Enterprise.MasterSettingType mst ON mst.MasterConfigurationTypeId = mct.MasterConfigurationTypeId
			INNER JOIN Enterprise.MasterSetting ms ON MS.MasterSettingTypeId = mst.MasterSettingTYpeId
			INNER JOIN Enterprise.Party p ON p.RealPageId = ms.Value
			INNER JOIN Ident.UserLogin UL ON UL.PersonPartyId = P.PartyId
			INNER JOIN Ident.UserLoginPersona ULP ON UL.UserId = ULP.UserLoginId
	WHERE mct.Name = 'Organization'	
	AND mst.Name = 'RealPageEmployeeAccessID'
	AND ULP.OrganizationPartyId = @OrganizationId

	SELECT	ul.UserId AS 'UserId',
			p.PersonaId AS 'PersonaId',
			pa.RealPageId AS 'UserRealPageId',
			@AdminPersonaID AS 'AdminUserPersonaId'
	FROM	Ident.UserLogin ul
			INNER JOIN Enterprise.Party pa ON pa.PartyId = ul.PersonPartyId
			INNER JOIN Ident.UserLoginPersona ULP ON ULP.UserLoginId = ul.UserId
			INNER JOIN Person.Persona p ON p.UserLoginPersonaId = ULP.UserLoginPersonaId
			INNER JOIN Enterprise.Organization o ON o.PartyId = ULP.OrganizationPartyId
			INNER JOIN Person.Person pe ON pe.PartyId = ul.PersonPartyId
			INNER JOIN Enterprise.StatusType st ON st.StatusTYpeId = ULP.StatusTypeId 
	WHERE	o.PartyId = @OrganizationId 		
	AND		ULP.UserLoginPersonaId NOT IN (@AdminPersonaID)
	And st.Name = 'Active'
	
END