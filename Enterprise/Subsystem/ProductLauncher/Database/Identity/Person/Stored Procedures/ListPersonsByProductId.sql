CREATE PROCEDURE [Person].[ListPersonsByProductId](
	@ProductId int,
	@RealPageId uniqueidentifier = NULL,
	@PersonaId bigint = NULL
)
AS
BEGIN
	DECLARE @PartyId bigint;

	DECLARE @NOW  datetime = GETUTCDATE();

	SELECT @PartyId = PartyId
	FROM Enterprise.Party
	WHERE RealPageId = @RealPageId

	DECLARE @timezoneresult TABLE (
		userid bigint,
		timezone nvarchar(4000) )
	if @RealPageId IS NOT NULL
	BEGIN
		insert into @timezoneresult
			(userid, timezone)
		SELECT mc.AttributeId,
			ms.Value
		FROM Enterprise.MasterConfigurationSetting mcs
			INNER JOIN Enterprise.MasterConfiguration mc ON (mc.MasterConfigurationId = mcs.MasterConfigurationId)
			INNER JOIN Enterprise.MasterSetting ms ON (mcs.MasterSettingId = ms.MasterSettingId)
			INNER JOIN Enterprise.MasterSettingType mst ON (mst.MasterSettingTypeId = ms.MasterSettingTypeId)
			INNER JOIN Enterprise.MasterConfigurationType mct ON (mct.MasterConfigurationTypeId = mst.MasterConfigurationTypeId)
			INNER JOIN Ident.UserLoginPersona ULP on mc.AttributeId = ULP.UserLoginId AND ULP.OrganizationPartyId = @PartyId
		WHERE	mct.Name = 'UserLogin'
			AND mst.Name = 'TimeZone'
		option(force
		order)
	END

	declare @productlist table (
		userloginpersonaid bigint )
	IF @RealPageId IS NOT NULL
	BEGIN
		insert into @productlist
			(UserLoginPersonaId)
		SELECT ULP.UserLoginPersonaId
		FROM Person.Persona pe
			INNER JOIN Ident.UserLoginPersona ULP ON pe.UserLoginPersonaId = ULP.UserLoginPersonaId
			INNER JOIN Enterprise.PersonaConfiguration pec ON (pe.PersonaId = pec.PersonaId) AND pec.ProductId = @ProductId
			INNER JOIN Enterprise.ProductConfiguration prc ON (pec.ConfigurationId = prc.ConfigurationId)
			INNER JOIN Enterprise.ProductSetting ps ON (prc.ProductSettingId = ps.ProductSettingId AND ps.Value IN ('6','7','8'))
			INNER JOIN Enterprise.ProductSettingType pst ON (ps.ProductSettingTypeId = pst.ProductSettingTypeId AND pst.Name = 'ProductStatus')
		WHERE	
			ULP.OrganizationPartyId = @PartyId
			AND ((@NOW BETWEEN pec.FromDate AND pec.ThruDate) OR (@NOW >= pec.FromDate AND pec.ThruDate IS NULL))
			AND ((@NOW BETWEEN prc.FromDate AND prc.ThruDate) OR (@NOW >= prc.FromDate AND prc.ThruDate IS NULL))
			AND ((@NOW BETWEEN ps.FromDate AND ps.ThruDate) OR (@NOW >= ps.FromDate AND ps.ThruDate IS NULL))
			AND pec.ProductId = @ProductId
			AND ((@PersonaId IS NULL) OR (pe.PersonaId = @PersonaId))
		option(force order)
	END
	ELSE
	BEGIN
		insert into @productlist
			(UserLoginPersonaId)
		SELECT ULP.UserLoginPersonaId
		FROM Person.Persona pe
			INNER JOIN Ident.UserLoginPersona ULP ON pe.UserLoginPersonaId = ULP.UserLoginPersonaId
			INNER JOIN Enterprise.PersonaConfiguration pec ON (pe.PersonaId = pec.PersonaId) AND pec.ProductId = @ProductId
			INNER JOIN Enterprise.ProductConfiguration prc ON (pec.ConfigurationId = prc.ConfigurationId)
			INNER JOIN Enterprise.ProductSetting ps ON (prc.ProductSettingId = ps.ProductSettingId AND ps.Value IN ('6','7','8'))
			INNER JOIN Enterprise.ProductSettingType pst ON (ps.ProductSettingTypeId = pst.ProductSettingTypeId AND pst.Name = 'ProductStatus')
		WHERE	
			((@NOW BETWEEN pec.FromDate AND pec.ThruDate) OR (@NOW >= pec.FromDate AND pec.ThruDate IS NULL))
			AND ((@NOW BETWEEN prc.FromDate AND prc.ThruDate) OR (@NOW >= prc.FromDate AND prc.ThruDate IS NULL))
			AND ((@NOW BETWEEN ps.FromDate AND ps.ThruDate) OR (@NOW >= ps.FromDate AND ps.ThruDate IS NULL))
			AND pec.ProductId = @ProductId
			AND ((@PersonaId IS NULL) OR (pe.PersonaId = @PersonaId))
		option(force order)
	END
	IF @RealPageId IS NULL
	BEGIN
		SELECT pa.RealpageId AS RealPageID,
			p.PartyId,
			p.FirstName,
			p.MiddleName,
			p.LastName,
			p.Title,
			p.Suffix,
			ul.UserId,
			ul.LoginName,
			'' As 'TimeZoneOffset'
		FROM
			Ident.UserLoginPersona ULP
			INNER JOIN Enterprise.Organization eo ON ULP.OrganizationPartyId = eo.PartyId
			INNER JOIN Ident.UserLogin UL ON ULP.UserLoginId = UL.UserId
			JOIN Person.Person p ON p.PartyId = UL.PersonPartyId
			JOIN Enterprise.Party pa ON pa.PartyId = UL.PersonPartyId
		WHERE
		ULP.UserLoginPersonaId IN ( SELECT UserLoginPersonaId FROM @productlist )
	END
	ELSE
	BEGIN
		SELECT pa.RealpageId AS RealPageID,
			p.PartyId,
			p.FirstName,
			p.MiddleName,
			p.LastName,
			p.Title,
			p.Suffix,
			ul.UserId,
			ul.LoginName,
			s.timezone As 'TimeZoneOffset'
		FROM
			Ident.UserLoginPersona ULP
			INNER JOIN Enterprise.Organization eo ON ULP.OrganizationPartyId = eo.PartyId
			INNER JOIN Ident.UserLogin UL ON ULP.UserLoginId = UL.UserId
			JOIN Person.Person p ON p.PartyId = UL.PersonPartyId
			JOIN Enterprise.Party pa ON pa.PartyId = UL.PersonPartyId
			LEFT JOIN @timezoneresult AS s ON s.userid = ul.UserId
		WHERE	
			ULP.OrganizationPartyId = @PartyId
			AND ULP.UserLoginPersonaId IN ( SELECT UserLoginPersonaId FROM @productlist )
	END
END

