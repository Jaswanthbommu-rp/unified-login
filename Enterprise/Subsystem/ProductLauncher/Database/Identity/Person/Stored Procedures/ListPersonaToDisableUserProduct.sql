CREATE PROCEDURE [Person].[ListPersonaToDisableUserProduct] (
	@OrganizationPartyId bigint,
	@PersonRealPageId uniqueidentifier
)
AS
BEGIN
	DECLARE @NOW datetime = GETUTCDATE(),
		@PersonPartyId bigint,
		@PrimaryOrganization bit,
		@PartyRoleTypeId int

	SELECT	@PersonPartyId = PartyId
	FROM		Enterprise.Party
	WHERE	RealPageId = @PersonRealPageId

	SELECT		@PrimaryOrganization = iulp.PrimaryOrganization
	FROM		Ident.UserLogin iul
					INNER JOIN Ident.UserLoginPersona iulp ON (iul.UserId = iulp.UserLoginId)
	WHERE	iul.PersonPartyId = @PersonPartyId
	AND			iulp.OrganizationPartyId = @OrganizationPartyId
	AND			iulp.PrimaryOrganization = 'true'

	SELECT	@PartyRoleTypeId = PartyRoleTypeId
	FROM		Enterprise.RoleType
	WHERE	Name = 'User Type';

	WITH cteUserOrganization (
		OrganizationPartyId
	)
	AS
	(
		SELECT	OrganizationPartyId
		FROM		Ident.UserLogin iul
						INNER JOIN Ident.UserLoginPersona iulp ON (iul.UserId = iulp.UserLoginId)
		WHERE		iul.PersonPartyId = @PersonPartyId
	),
	cteEmployeeAccess
	(
		PersonaId,
		RealPageId,
		OrganizationPartyId
	)
	AS
	(
		SELECT	pe.PersonaId,
					p.RealPageId,
					ulp.OrganizationPartyId
		FROM	Enterprise.MasterConfigurationType mct
					INNER JOIN Enterprise.MasterSettingType mst ON (mst.MasterConfigurationTypeId = mct.MasterConfigurationTypeId)
					INNER JOIN Enterprise.MasterSetting ms ON (ms.MasterSettingTypeId = mst.MasterSettingTYpeId)
					INNER JOIN Enterprise.Party p ON p.RealPageId = ms.Value
					INNER JOIN Ident.UserLogin ul ON ul.PersonPartyId = p.PartyId
					INNER JOIN Ident.UserLoginPersona ulp ON ulp.UserLoginId = ul.UserId
					INNER JOIN Person.Persona pe ON pe.UserLoginPersonaId = ulp.UserLoginPersonaId
					INNER JOIN cteUserOrganization uo ON (ulp.OrganizationPartyId = uo.OrganizationPartyId)
		WHERE	mct.Name = 'Organization'
		AND			mst.Name = 'RealPageEmployeeAccessID'
		AND			TRY_CAST(ms.Value AS UNIQUEIDENTIFIER) IS NOT NULL
	)

	SELECT		pe.PersonaId,
					iulp.UserLoginPersonaId,
					iulp.OrganizationPartyId,
					iulp.PrimaryOrganization,
					a.PersonaId AS 'EditorPersonaId',
					a.RealPageId AS 'EditorRealPageId',
					epr.RoleTypeIdFrom AS 'UserTypeId'
	FROM		Ident.UserLogin iul
					INNER JOIN Ident.UserLoginPersona iulp ON (iul.UserId = iulp.UserLoginId)
					INNER JOIN Person.Persona pe ON (iulp.UserLoginPersonaId = pe.UserLoginPersonaId)
					INNER JOIN Enterprise.PartyRelationship epr ON (epr.PartyIdFrom = iul.PersonPartyId AND epr.PartyIdTo = iulp.OrganizationPartyId AND epr.RoleTypeIdTo = @PartyRoleTypeId)
					INNER JOIN cteEmployeeAccess a ON (a.OrganizationPartyId = iulp.OrganizationPartyId)
	WHERE		iul.PersonPartyId = @PersonPartyId
	AND			(
						@PrimaryOrganization = 'true'
						OR
						((@PrimaryOrganization IS NULL) AND (iulp.OrganizationPartyId = @OrganizationPartyId))
					)
	AND			((@NOW BETWEEN pe.FromDate AND pe.ThruDate) OR (@NOW >= pe.FromDate AND pe.ThruDate IS NULL))
	AND			((@NOW BETWEEN epr.FromDate AND epr.ThruDate) OR (@NOW >= epr.FromDate AND epr.ThruDate IS NULL))
END