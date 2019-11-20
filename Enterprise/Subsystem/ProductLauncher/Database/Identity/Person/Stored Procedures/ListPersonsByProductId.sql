CREATE PROCEDURE [Person].[ListPersonsByProductId] (
	@ProductId int,
	@RealPageId uniqueidentifier = NULL,
	@PersonaId bigint = NULL
)
AS
BEGIN
	DECLARE @PartyId bigint,
		@ProductSettingTypeId int,
		@NOW DATETIME = GETUTCDATE();

	DECLARE @ProductList TABLE (
		UserLoginPersonaId bigint
	)

	SELECT	@PartyId = PartyId
	FROM		Enterprise.Party
	WHERE	RealPageId = @RealPageId

	SELECT	@ProductSettingTypeId = ProductSettingTypeId
	FROM		Enterprise.ProductSettingType
	WHERE	Name = 'ProductStatus'

	IF @RealPageId IS NOT NULL
	BEGIN
		INSERT INTO @ProductList (
			UserLoginPersonaId
		)
		SELECT DISTINCT iulp.UserLoginPersonaId
		FROM Ident.UserLoginPersona iulp
			INNER JOIN Person.Persona pe ON (pe.UserLoginPersonaId = iulp.UserLoginPersonaId)
			INNER JOIN Enterprise.PersonaConfiguration pec ON (pe.PersonaId = pec.PersonaId AND pec.ProductId = @ProductId)
			INNER JOIN Enterprise.ProductConfiguration prc ON (pec.ConfigurationId = prc.ConfigurationId)
			INNER JOIN Enterprise.ProductSetting ps ON (prc.ProductSettingId = ps.ProductSettingId AND ps.ProductSettingTypeId = @ProductSettingTypeId AND ps.Value IN ('6','7','8'))
		WHERE	
			((@NOW BETWEEN pec.FromDate AND pec.ThruDate) OR (@NOW >= pec.FromDate AND pec.ThruDate IS NULL))
			AND ((@NOW BETWEEN prc.FromDate AND prc.ThruDate) OR (@NOW >= prc.FromDate AND prc.ThruDate IS NULL))
			AND ((@NOW BETWEEN ps.FromDate AND ps.ThruDate) OR (@NOW >= ps.FromDate AND ps.ThruDate IS NULL))
			AND ((@PartyId IS NULL) OR (iulp.OrganizationPartyId = @PartyId))
			AND ((@PersonaId IS NULL) OR (pe.PersonaId = @PersonaId))
		option(force order)
	END
	ELSE
	BEGIN
		INSERT INTO @ProductList (
			UserLoginPersonaId
		)
		SELECT DISTINCT iulp.UserLoginPersonaId
		FROM Ident.UserLoginPersona iulp
			INNER JOIN Person.Persona pe ON (pe.UserLoginPersonaId = iulp.UserLoginPersonaId)
			INNER JOIN Enterprise.PersonaConfiguration pec ON (pe.PersonaId = pec.PersonaId AND pec.ProductId = @ProductId)
			INNER JOIN Enterprise.ProductConfiguration prc ON (pec.ConfigurationId = prc.ConfigurationId)
			INNER JOIN Enterprise.ProductSetting ps ON (prc.ProductSettingId = ps.ProductSettingId AND ps.ProductSettingTypeId = @ProductSettingTypeId AND ps.Value IN ('6', '7', '8'))
		WHERE	
			((@NOW BETWEEN pec.FromDate AND pec.ThruDate) OR (@NOW >= pec.FromDate AND pec.ThruDate IS NULL))
			AND ((@NOW BETWEEN prc.FromDate AND prc.ThruDate) OR (@NOW >= prc.FromDate AND prc.ThruDate IS NULL))
			AND ((@NOW BETWEEN ps.FromDate AND ps.ThruDate) OR (@NOW >= ps.FromDate AND ps.ThruDate IS NULL))
			AND ((@PersonaId IS NULL) OR (pe.PersonaId = @PersonaId))
		option(force order)
	END

	SELECT pa.RealpageId AS RealPageID,
		p.PartyId,
		p.FirstName,
		p.MiddleName,
		p.LastName,
		p.Title,
		p.Suffix,
		ul.UserId,
		ul.LoginName
	FROM	@ProductList pl
		INNER JOIN Ident.UserLoginPersona iulp  ON (iulp.UserLoginPersonaId = pl.UserLoginPersonaId)
		INNER JOIN Enterprise.Organization eo ON iulp.OrganizationPartyId = eo.PartyId
		INNER JOIN Ident.UserLogin UL ON iulp.UserLoginId = UL.UserId
		INNER JOIN Person.Person p ON p.PartyId = UL.PersonPartyId
		INNER JOIN Enterprise.Party pa ON pa.PartyId = UL.PersonPartyId
	ORDER BY pa.PartyId
	OPTION (RECOMPILE)
END