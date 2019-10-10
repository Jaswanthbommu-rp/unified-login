---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

/*
select * from enterprise.GlobalProductConfiguration where productid = 1
select * from enterprise.OrganizationProduct where productid = 1 and partyid = 17403

select * from enterprise.Configuration
select * from enterprise.ProductConfiguration where ConfigurationId = 16255
select * from enterprise.productsetting where ProductSettingId = 1427
select * from enterprise.productsettingtype 

select *
		FROM Enterprise.ProductSetting PS 
		INNER JOIN Enterprise.ProductConfiguration PC on PS.ProductSettingId = PC.ProductSettingId 
		INNER JOIN Enterprise.ProductSettingType PST on PS.ProductSettingTypeId = PST.ProductSettingTypeId AND PST.Name = 'OverridePMCID'

	set @OrgPartyId = 350
	SET @OverridePMCID = '7079607'
	select * from ident.userlogin ul inner join person.persona p on ul.userid = p.UserId where p.OrganizationPartyId = 17403
	BEGIN TRAN
		select * from enterprise.OrganizationProduct where productid = 1 and partyid = 17403
		SELECT 350, COUNT(1) FROM IDENT.SamlUserAttribute SUA INNER JOIN PERSON.PERSONA PA ON SUA.PersonaId = PA.PersonaId WHERE ProductId = 1 AND PA.OrganizationPartyId = 350
		SELECT 353, COUNT(1) FROM IDENT.SamlUserAttribute SUA INNER JOIN PERSON.PERSONA PA ON SUA.PersonaId = PA.PersonaId WHERE ProductId = 1 AND PA.OrganizationPartyId = 353
		SELECT 17403, COUNT(1) FROM IDENT.SamlUserAttribute SUA INNER JOIN PERSON.PERSONA PA ON SUA.PersonaId = PA.PersonaId WHERE ProductId = 1 AND PA.OrganizationPartyId = 17403
		SELECT 16120, COUNT(1) FROM IDENT.SamlUserAttribute SUA INNER JOIN PERSON.PERSONA PA ON SUA.PersonaId = PA.PersonaId WHERE ProductId = 1 AND PA.OrganizationPartyId = 16120

	--	exec Enterprise.OverrideOneSitePMCIDByPartyId @OrgPartyId = 350, @OverridePMCID = '7654321', @REMOVEALLUSERS = 1
		--exec Enterprise.OverrideOneSitePMCIDByPartyId @OrgPartyId = 353, @OverridePMCID = '333', @REMOVEALLUSERS = 1
		exec Enterprise.OverrideOneSitePMCIDByPartyId @OrgPartyId = 174043, @OverridePMCID = '123213'
	--	exec Enterprise.OverrideOneSitePMCIDByPartyId @OrgPartyId = 16120, @OverridePMCID = '9999'
		select * from enterprise.OrganizationProduct where productid = 1 and partyid = 17403
		select OP.PARTYID,*
		FROM Enterprise.ProductSetting PS 
		INNER JOIN Enterprise.ProductConfiguration PC on PS.ProductSettingId = PC.ProductSettingId 
		INNER JOIN enterprise.OrganizationProduct OP on OP.ConfigurationId = PC.ConfigurationId
		INNER JOIN Enterprise.ProductSettingType PST on PS.ProductSettingTypeId = PST.ProductSettingTypeId AND PST.Name = 'OverridePMCID'
				SELECT 350, COUNT(1) FROM IDENT.SamlUserAttribute SUA INNER JOIN PERSON.PERSONA PA ON SUA.PersonaId = PA.PersonaId WHERE ProductId = 1 AND PA.OrganizationPartyId = 350
		SELECT 353, COUNT(1) FROM IDENT.SamlUserAttribute SUA INNER JOIN PERSON.PERSONA PA ON SUA.PersonaId = PA.PersonaId WHERE ProductId = 1 AND PA.OrganizationPartyId = 353
		SELECT 17403, COUNT(1) FROM IDENT.SamlUserAttribute SUA INNER JOIN PERSON.PERSONA PA ON SUA.PersonaId = PA.PersonaId WHERE ProductId = 1 AND PA.OrganizationPartyId = 17403
		SELECT 16120, COUNT(1) FROM IDENT.SamlUserAttribute SUA INNER JOIN PERSON.PERSONA PA ON SUA.PersonaId = PA.PersonaId WHERE ProductId = 1 AND PA.OrganizationPartyId = 16120

	ROLLBACK TRAN

	SP_HELPTEXT 'Enterprise.CreateProductConfiguration'
	
*/ 
CREATE PROCEDURE Enterprise.OverrideOneSitePMCIDByPartyId (
	@OrgPartyId BIGINT,
	@OverridePMCID VARCHAR(7),
	@RemoveAllUsers BIT = 0
)
AS
BEGIN
	SET NOCOUNT ON

	DECLARE @OverriddenPMCList TABLE (ConfigurationId INT, ProductSettingId INT )
	DECLARE @OverridePMCIDTypeId INT
	DECLARE @ConfigurationId INT
	DECLARE @ProductId INT

	SET @ProductId = 1

	IF NOT EXISTS ( SELECT TOP 1 1 FROM Enterprise.Organization WHERE PartyId = @OrgPartyId )
	BEGIN
		PRINT 'Company not found'
		Return
	END

	SELECT @OverridePMCIDTypeId = ProductSettingTypeId FROM Enterprise.ProductSettingType WHERE Name = 'OverridePMCID'

	IF @OverridePMCIDTypeId IS NULL
	BEGIN
		PRINT 'OverridePMCID setting not found'
		RETURN
	END

	INSERT INTO @OverriddenPMCList ( ConfigurationId, ProductSettingId ) 
		SELECT PC.ConfigurationId, PC.ProductSettingId
			FROM Enterprise.ProductSetting PS 
			INNER JOIN Enterprise.ProductConfiguration PC on PS.ProductSettingId = PC.ProductSettingId
			INNER JOIN Enterprise.OrganizationProduct OP on OP.ConfigurationId = PC.ConfigurationId AND OP.PartyId = @OrgPartyId
			INNER JOIN Enterprise.ProductSettingType PST on PS.ProductSettingTypeId = PST.ProductSettingTypeId AND PST.Name = 'OverridePMCID'
	
	IF EXISTS (
		SELECT TOP 1 1 FROM Enterprise.OrganizationProduct OP INNER JOIN @OverriddenPMCList OPL ON OP.ConfigurationId = OPL.ConfigurationId 
			WHERE PartyId = @OrgPartyID 
			AND ProductId = @ProductId 
			AND ThruDate IS NULL
		)
	BEGIN
		--PRINT 'HAS OVERRIDE'
		SELECT 'PRIOR PMC ID', VALUE FROM Enterprise.ProductSetting PS INNER JOIN @OverriddenPMCList OPL ON PS.ProductSettingId = OPL.ProductSettingId AND PS.ProductId = @ProductId
		UPDATE PS 
		SET Value = @OverridePMCID 
			FROM Enterprise.ProductSetting PS INNER JOIN @OverriddenPMCList OPL ON PS.ProductSettingId = OPL.ProductSettingId AND PS.ProductId = @ProductId
	END
	ELSE
	BEGIN
		--PRINT 'NO OVERRIDE'
		DECLARE @ProductSettingIDTable TABLE (ID INT)
		DECLARE @ConfigurationIDTable TABLE (ID INT)

		BEGIN TRY
			BEGIN TRANSACTION
			INSERT INTO Enterprise.ProductSetting (ProductId, ProductSettingTypeId, Value, FromDate )
				OUTPUT inserted.ProductSettingId into @ProductSettingIDTable
			VALUES ( @ProductId, @OverridePMCIDTypeId, @OverridePMCID, GETUTCDATE() )
		
			INSERT INTO Enterprise.Configuration( CreateDate )
				OUTPUT inserted.ConfigurationId into @ConfigurationIDTable
			VALUES ( GETUTCDATE() )
			
			-- GET A NEW CONFIGURATION ID
			SELECT @ConfigurationId = ID FROM @ConfigurationIDTable
			
			INSERT INTO Enterprise.ProductConfiguration ( ConfigurationId, ProductSettingId, FromDate ) 
			SELECT @ConfigurationId, ID, GETUTCDATE() FROM @ProductSettingIDTable

			INSERT INTO Enterprise.OrganizationProduct ( PartyId, ConfigurationId, ProductId, FromDate )
			SELECT @OrgPartyID, @ConfigurationId, @ProductId, GETUTCDATE() FROM @ProductSettingIDTable

			COMMIT
		END TRY
		BEGIN CATCH
			PRINT 'THERE WAS A PROBLEM UPDATING THE CLONE SETTINGS'
			ROLLBACK
		END CATCH
	END

	IF @RemoveAllUsers = 1
	BEGIN
		DECLARE @PersonaToKeep TABLE ( PersonaId INT )
		INSERT INTO @PersonaToKeep ( PersonaId )
		SELECT DISTINCT PersonaId FROM Ident.SamlUserAttribute SUA 
			INNER JOIN Ident.SamlAttribute SA ON SUA.SamlAttributeId = SA.SamlAttributeId where SA.Name = 'PersistLogin' AND SUA.ProductId = @ProductId

		DELETE FROM Enterprise.ProductConfiguration where ConfigurationId in (select ConfigurationId from enterprise.PersonaConfiguration 
			WHERE productid = @ProductId and personaid in ( select personaid from person.persona 
				INNER JOIN Ident.UserLoginPersona ULP ON Persona.UserLoginPersonaId = ULP.UserLoginPersonaId where ULP.OrganizationPartyId = @OrgPartyId and personaid not in ( select personaid from @PersonaToKeep  ) ))
		DELETE FROM Enterprise.PersonaConfiguration 
			WHERE productid = @ProductId and personaid in ( select personaid from person.persona 
				INNER JOIN Ident.UserLoginPersona ULP ON Persona.UserLoginPersonaId = ULP.UserLoginPersonaId where ULP.OrganizationPartyId = @OrgPartyId and personaid not in ( select personaid from @PersonaToKeep ) )
		DELETE FROM Ident.SamlUserAttribute where productid = @productid and personaid in ( select personaid from person.persona  
				INNER JOIN Ident.UserLoginPersona ULP ON Persona.UserLoginPersonaId = ULP.UserLoginPersonaId where ULP.OrganizationPartyId = @OrgPartyId and personaid not in ( select personaid from @PersonaToKeep ))
		PRINT 'REMOVED EXISTING USERS'
	END
END
