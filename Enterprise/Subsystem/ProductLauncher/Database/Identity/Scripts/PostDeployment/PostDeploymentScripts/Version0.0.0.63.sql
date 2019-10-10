SET @ProductId = 35

IF NOT EXISTS(SELECT 1 FROM Enterprise.Product WHERE Name = 'Support Tool')
BEGIN
	EXEC Enterprise.CreateProduct @ProductId, NULL, 'Support Tool', 'Support Tool', NULL
END

EXEC Enterprise.CreateProductConfiguration @ConfigurationId = @ConfigurationId OUTPUT 

SET @ClientName = 'UnifiedLogin Support Tool'
SET @ClientCode = 'UnifiedLoginSupportTool'

--Research Application
SET @ClientId = NULL
SET @FromDate = GETUTCDATE();

-- If this is a new Global Product Configuration then create a Product Configuration if you do not have one to use.
SELECT	@ProductId = ProductId
FROM		Enterprise.Product
WHERE	Name = 'Support Tool'

DELETE
FROM	@AuthClients;

IF NOT EXISTS(SELECT TOP 1 1 FROM Auth.Scopes WHERE Name = @ClientCode)
BEGIN
	INSERT INTO Auth.Scopes (
		Name,
		DisplayName,
		Description,
		ClaimsRule,
		Enabled,
		Required,
		Emphasize,
		Type,
		IncludeAllClaimsForUser,
		ShowInDiscoveryDocument,
		AllowUnrestrictedIntrospection
	)
	VALUES (
		@ClientCode, --Name,
		@ClientName, --DisplayName,
		NULL, --Description,
		NULL, --ClaimsRule,
		1, --Enabled,
		0, --Required,
		0, --Emphasize,
		1, --Type,
		1, --IncludeAllClaimsForUser,
		1, --ShowInDiscoveryDocument,
		0 --AllowUnrestrictedIntrospection
	)
END

--ClientCredentials Flow
IF NOT EXISTS(SELECT TOP 1 1 FROM Auth.Clients WHERE ClientCode = @ClientCode AND Flow = 3)
BEGIN
	INSERT INTO Auth.Clients (
					ClientCode,
					ClientName,
					ClientUri,
					LogoUri,
					Flow,
					LogoutUri,
					IdentityTokenLifetime,
					AccessTokenLifetime,
					AuthorizationCodeLifetime,
					AbsoluteRefreshTokenLifetime,
					SlidingRefreshTokenLifetime,
					RefreshTokenUsage,
					RefreshTokenExpiration,
					AccessTokenType,
					UpdateAccessTokenOnRefresh,
					Enabled,
					LogoutSessionRequired,
					RequireSignOutPrompt,
					AllowAccessToAllScopes,
					AllowClientCredentialsOnly,
					RequireConsent,
					AllowRememberConsent,
					EnableLocalLogin,
					IncludeJwtId,
					AlwaysSendClientClaims,
					PrefixClientClaims,
					AllowAccessToAllGrantTypes
	)
	OUTPUT INSERTED.ClientId INTO @AuthClients (ClientId)
	VALUES (
					@ClientCode, --ClientCode
					@ClientName, --ClientName
					NULL, --ClientUri
					NULL, --LogoUri
					3, --ClientCredentials
					NULL, --LogoutUri
					36000, --IdentityTokenLifetime
					36000, --AccessTokenLifetime
					36000, --AuthorizationCodeLifetime
					86400, --AbsoluteRefreshTokenLifetime
					36000, --SlidingRefreshTokenLifetime
					0, --RefreshTokenUsage
					0, --RefreshTokenExpiration
					0, --AccessTokenType
					1, --UpdateAccessTokenOnRefresh
					1, --Enabled
					0, --LogoutSessionRequired
					0, --RequireSignOutPrompt
					1, --AllowAccessToAllScopes
					0, --AllowClientCredentialsOnly
					0, --RequireConsent
					1, --AllowRememberConsent
					1, --EnableLocalLogin
					1, --IncludeJwtId
					1, --AlwaysSendClientClaims
					1, --PrefixClientClaims
					1 --AllowAccessToAllGrantTypes
	)

	SELECT	@ClientId = ClientId
	FROM	@AuthClients

	INSERT INTO Auth.ClientScopes (
		ClientId,
		Scope
	)
	SELECT	@ClientId, --ClientId
					ScopeId --Scope
	FROM		Auth.Scopes
	WHERE Name = @ClientCode

	INSERT INTO Auth.ClientSecrets (
		ClientId,
		Value,
		Type,
		Description,
		Expiration
	)
	VALUES (
		@ClientId,
		'RnLaeOYVnJjS7Qzy+l/93sveiwaoc7G26bHTYgSXiVg=',
		NULL,
		@ClientName,
		'2099-12-31 00:00:00.0000000 -06:00'
	)
END

------------------------------------------------------------------------------------------------------------------------------------------------------
IF (@ProductId = 35)
BEGIN
	SELECT	@ConfigurationId = pc.ConfigurationId
	FROM		Enterprise.GlobalProductConfiguration gpc  
					JOIN Enterprise.ProductConfiguration pc ON pc.ConfigurationId = gpc.ConfigurationId  
					JOIN Enterprise.ProductSetting ps ON ps.ProductSettingId = pc.ProductSettingId  
					JOIN Enterprise.ProductSettingType pst ON pst.ProductSettingTypeId = ps.ProductSettingTypeId  
	WHERE	gpc.ProductId = @ProductId 
	AND			pst.Name = 'ClassName'
	AND			((@FromDate BETWEEN gpc.FromDate AND gpc.ThruDate) OR (@FromDate >= gpc.FromDate AND gpc.ThruDate IS NULL))  
	AND			((@FromDate BETWEEN pc.FromDate AND pc.ThruDate) OR (@FromDate >= pc.FromDate AND pc.ThruDate IS NULL))  
	AND			((@FromDate BETWEEN ps.FromDate AND ps.ThruDate) OR (@FromDate >= ps.FromDate AND ps.ThruDate IS NULL)) 
	------------------------------------------------------------------------------------------------------------------------------------------------------
	-- Get the ProductSettingTypeId for the attribute you need to select, the API should have all these cached though.
	-- Commented out per Vinay - this does not need to be set for this product.
	--SET @ProductSettingId = NULL
	--SET @ProductSettingTypeId = NULL

	--EXEC Enterprise.GetProductSettingType
	--	@Name = 'ApiEndPoint',                                         -- varchar(50)
	--	@ProductSettingTypeId = @ProductSettingTypeId OUTPUT -- int

	---- Create the Value and assign it to the Product and ProductSettingType
	--EXEC Enterprise.CreateProductSetting
	--	@ProductId = @ProductId,                             -- int
	--	@ProductSettingTypeId = @ProductSettingTypeId,                  -- int
	--	@Value = N'https://mylocal.corp.realpage.com/#/employee-access',                               -- nvarchar(1000)
	--	@FromDate = @FromDate,          -- datetime
	--	@ThruDate = NULL,          -- datetime
	--	@ProductSettingId = @ProductSettingId OUTPUT -- int

	---- Link the Product Setting to an actual configuration
	--EXEC Enterprise.LinkProductSettingToConfiguration
	--	@ConfigurationId = @ConfigurationId,              -- int
	--	@ProductSettingId = @ProductSettingId,             -- int
	--	@FromDate = @FromDate, -- datetime
	--	@ThruDate = NULL   -- datetime

	--EXEC Enterprise.ListGlobalSettingsForProduct @productid = @ProductId
END

---- Step 3 - Add all settings

SET @FromDate = GETUTCDATE()

IF NOT EXISTS(SELECT 1 FROM Ident.SamlProductSettings WHERE ProductId = @ProductId)
BEGIN
	EXEC Ident.CreateSamlProductSetting @ProductId, 'https://mylocal.corp.realpage.com/#/employee-access', 'NA', 'productUsername', @SamlProductSettingId OUTPUT
END

IF NOT EXISTS(SELECT 1 FROM Enterprise.GlobalProductConfiguration WHERE ProductId = @ProductId)
BEGIN
	--EXEC Enterprise.CreateProductConfiguration @ConfigurationId OUTPUT

	SELECT @ClientId = ClientId FROM Auth.Clients WHERE ClientCode = 'UnifiedLoginSupportTool'

	--***** Start a Product setting loop for each attribute / value that needs set. *****
	------------------------------------------------------------------------------------------------------------------------------------------------------
	-- Get the ProductSettingTypeId for the attribute you need to select, the API should have all these cached though.
	SET @ProductSettingId = NULL
	SET @ProductSettingTypeId = NULL

	EXEC Enterprise.GetProductSettingType
		@Name = 'ClientId',                                         -- varchar(50)
		@ProductSettingTypeId = @ProductSettingTypeId OUTPUT -- int

	-- Create the Value and assign it to the Product and ProductSettingType
	EXEC Enterprise.CreateProductSetting
		@ProductId = @ProductId,                             -- int
		@ProductSettingTypeId = @ProductSettingTypeId,                  -- int
		@Value = @clientID,                               -- nvarchar(1000)
		@FromDate = @FromDate,          -- datetime
		@ThruDate = NULL,          -- datetime
		@ProductSettingId = @ProductSettingId OUTPUT -- int

	-- Link the Product Setting to an actual configuration
	EXEC Enterprise.LinkProductSettingToConfiguration
		@ConfigurationId = @ConfigurationId,              -- int
		@ProductSettingId = @ProductSettingId,             -- int
		@FromDate = @FromDate, -- datetime
		@ThruDate = NULL   -- datetime

		------------------------------------------------------------------------------------------------------------------------------------------------------
	-- Get the ProductSettingTypeId for the attribute you need to select, the API should have all these cached though.
	SET @ProductSettingId = NULL
	SET @ProductSettingTypeId = NULL

	EXEC Enterprise.GetProductSettingType
		@Name = 'ClassName',                                         -- varchar(50)
		@ProductSettingTypeId = @ProductSettingTypeId OUTPUT -- int

	-- Create the Value and assign it to the Product and ProductSettingType
	EXEC Enterprise.CreateProductSetting
		@ProductId = @ProductId,                             -- int
		@ProductSettingTypeId = @ProductSettingTypeId,                  -- int
		@Value = N'supporttool',                               -- nvarchar(1000)
		@FromDate = @FromDate,          -- datetime
		@ThruDate = NULL,          -- datetime
		@ProductSettingId = @ProductSettingId OUTPUT -- int

	-- Link the Product Setting to an actual configuration
	EXEC Enterprise.LinkProductSettingToConfiguration
		@ConfigurationId = @ConfigurationId,              -- int
		@ProductSettingId = @ProductSettingId,             -- int
		@FromDate = @FromDate, -- datetime
		@ThruDate = NULL   -- datetime

	------------------------------------------------------------------------------------------------------------------------------------------------------
	-- Get the ProductSettingTypeId for the attribute you need to select, the API should have all these cached though.
	SET @ProductSettingId = NULL
	SET @ProductSettingTypeId = NULL

	EXEC Enterprise.GetProductSettingType
		@Name = 'TitleId',                                         -- varchar(50)
		@ProductSettingTypeId = @ProductSettingTypeId OUTPUT -- int

	-- Create the Value and assign it to the Product and ProductSettingType
	EXEC Enterprise.CreateProductSetting
		@ProductId = @ProductId,                             -- int
		@ProductSettingTypeId = @ProductSettingTypeId,                  -- int
		@Value = N'Support Tool',                               -- nvarchar(1000)
		@FromDate = @FromDate,          -- datetime
		@ThruDate = NULL,          -- datetime
		@ProductSettingId = @ProductSettingId OUTPUT -- int

	-- Link the Product Setting to an actual configuration
	EXEC Enterprise.LinkProductSettingToConfiguration
		@ConfigurationId = @ConfigurationId,              -- int
		@ProductSettingId = @ProductSettingId,             -- int
		@FromDate = @FromDate, -- datetime
		@ThruDate = NULL   -- datetime

	------------------------------------------------------------------------------------------------------------------------------------------------------
	-- Get the ProductSettingTypeId for the attribute you need to select, the API should have all these cached though.
	SET @ProductSettingId = NULL
	SET @ProductSettingTypeId = NULL

	SET @TitleUniqueId = NEWID()

	EXEC Enterprise.GetProductSettingType
		@Name = 'TitleUniqueId',                                         -- varchar(50)
		@ProductSettingTypeId = @ProductSettingTypeId OUTPUT -- int

	-- Create the Value and assign it to the Product and ProductSettingType
	EXEC Enterprise.CreateProductSetting
		@ProductId = @ProductId,                             -- int
		@ProductSettingTypeId = @ProductSettingTypeId,                  -- int
		@Value = @TitleUniqueId,                               -- nvarchar(1000)
		@FromDate = @FromDate,          -- datetime
		@ThruDate = NULL,          -- datetime
		@ProductSettingId = @ProductSettingId OUTPUT -- int

	-- Link the Product Setting to an actual configuration
	EXEC Enterprise.LinkProductSettingToConfiguration
		@ConfigurationId = @ConfigurationId,              -- int
		@ProductSettingId = @ProductSettingId,             -- int
		@FromDate = @FromDate, -- datetime
		@ThruDate = NULL   -- datetime

	------------------------------------------------------------------------------------------------------------------------------------------------------
	-- Get the ProductSettingTypeId for the attribute you need to select, the API should have all these cached though.
	SET @ProductSettingId = NULL
	SET @ProductSettingTypeId = NULL

	EXEC Enterprise.GetProductSettingType
		@Name = 'IsNewTab',                                         -- varchar(50)
		@ProductSettingTypeId = @ProductSettingTypeId OUTPUT -- int

	-- Create the Value and assign it to the Product and ProductSettingType
	EXEC Enterprise.CreateProductSetting
		@ProductId = @ProductId,                             -- int
		@ProductSettingTypeId = @ProductSettingTypeId,                  -- int
		@Value = N'0',                               -- nvarchar(1000)
		@FromDate = @FromDate,          -- datetime
		@ThruDate = NULL,          -- datetime
		@ProductSettingId = @ProductSettingId OUTPUT -- int

	-- Link the Product Setting to an actual configuration
	EXEC Enterprise.LinkProductSettingToConfiguration
		@ConfigurationId = @ConfigurationId,              -- int
		@ProductSettingId = @ProductSettingId,             -- int
		@FromDate = @FromDate, -- datetime
		@ThruDate = NULL   -- datetime

	------------------------------------------------------------------------------------------------------------------------------------------------------
	-- Get the ProductSettingTypeId for the attribute you need to select, the API should have all these cached though.
	SET @ProductSettingId = NULL
	SET @ProductSettingTypeId = NULL

	EXEC Enterprise.GetProductSettingType
		@Name = 'MetatagUniqueId',                                         -- varchar(50)
		@ProductSettingTypeId = @ProductSettingTypeId OUTPUT -- int

	-- Create the Value and assign it to the Product and ProductSettingType
	EXEC Enterprise.CreateProductSetting
		@ProductId = @ProductId,                             -- int
		@ProductSettingTypeId = @ProductSettingTypeId,                  -- int
		@Value = N'SupportTool',                               -- nvarchar(1000)
		@FromDate = @FromDate,          -- datetime
		@ThruDate = NULL,          -- datetime
		@ProductSettingId = @ProductSettingId OUTPUT -- int

	-- Link the Product Setting to an actual configuration
	EXEC Enterprise.LinkProductSettingToConfiguration
		@ConfigurationId = @ConfigurationId,              -- int
		@ProductSettingId = @ProductSettingId,             -- int
		@FromDate = @FromDate, -- datetime
		@ThruDate = NULL   -- datetime

	------------------------------------------------------------------------------------------------------------------------------------------------------
	-- Get the ProductSettingTypeId for the attribute you need to select, the API should have all these cached though.
	SET @ProductSettingId = NULL
	SET @ProductSettingTypeId = NULL

	EXEC Enterprise.GetProductSettingType
		@Name = 'IsFavorite',                                         -- varchar(50)
		@ProductSettingTypeId = @ProductSettingTypeId OUTPUT -- int

	-- Create the Value and assign it to the Product and ProductSettingType
	EXEC Enterprise.CreateProductSetting
		@ProductId = @ProductId,                             -- int
		@ProductSettingTypeId = @ProductSettingTypeId,                  -- int
		@Value = N'0',                               -- nvarchar(1000)
		@FromDate = @FromDate,          -- datetime
		@ThruDate = NULL,          -- datetime
		@ProductSettingId = @ProductSettingId OUTPUT -- int

	-- Link the Product Setting to an actual configuration
	EXEC Enterprise.LinkProductSettingToConfiguration
		@ConfigurationId = @ConfigurationId,              -- int
		@ProductSettingId = @ProductSettingId,             -- int
		@FromDate = @FromDate, -- datetime
		@ThruDate = NULL   -- datetime

	------------------------------------------------------------------------------------------------------------------------------------------------------
	-- Get the ProductSettingTypeId for the attribute you need to select, the API should have all these cached though.
	SET @ProductSettingId = NULL
	SET @ProductSettingTypeId = NULL

	EXEC Enterprise.GetProductSettingType
		@Name = 'IsResource',                                         -- varchar(50)
		@ProductSettingTypeId = @ProductSettingTypeId OUTPUT -- int

	-- Create the Value and assign it to the Product and ProductSettingType
	EXEC Enterprise.CreateProductSetting
		@ProductId = @ProductId,                             -- int
		@ProductSettingTypeId = @ProductSettingTypeId,                  -- int
		@Value = N'1',                               -- nvarchar(1000)
		@FromDate = @FromDate,          -- datetime
		@ThruDate = NULL,          -- datetime
		@ProductSettingId = @ProductSettingId OUTPUT -- int

	-- Link the Product Setting to an actual configuration
	EXEC Enterprise.LinkProductSettingToConfiguration
		@ConfigurationId = @ConfigurationId,              -- int
		@ProductSettingId = @ProductSettingId,             -- int
		@FromDate = @FromDate, -- datetime
		@ThruDate = NULL   -- datetime

	------------------------------------------------------------------------------------------------------------------------------------------------------
	-- Get the ProductSettingTypeId for the attribute you need to select, the API should have all these cached though.
	SET @ProductSettingId = NULL
	SET @ProductSettingTypeId = NULL

	EXEC Enterprise.GetProductSettingType
		@Name = 'LearnMore',                                         -- varchar(50)
		@ProductSettingTypeId = @ProductSettingTypeId OUTPUT -- int

	-- Create the Value and assign it to the Product and ProductSettingType
	EXEC Enterprise.CreateProductSetting
		@ProductId = @ProductId,                             -- int
		@ProductSettingTypeId = @ProductSettingTypeId,                  -- int
		@Value = N'https://www.realpage.com/',                               -- nvarchar(1000)
		@FromDate = @FromDate,          -- datetime
		@ThruDate = NULL,          -- datetime
		@ProductSettingId = @ProductSettingId OUTPUT -- int

	-- Link the Product Setting to an actual configuration
	EXEC Enterprise.LinkProductSettingToConfiguration
		@ConfigurationId = @ConfigurationId,              -- int
		@ProductSettingId = @ProductSettingId,             -- int
		@FromDate = @FromDate, -- datetime
		@ThruDate = NULL   -- datetime

	------------------------------------------------------------------------------------------------------------------------------------------------------
	-- Get the ProductSettingTypeId for the attribute you need to select, the API should have all these cached though.
	SET @ProductSettingId = NULL
	SET @ProductSettingTypeId = NULL

	EXEC Enterprise.GetProductSettingType
		@Name = 'ProductStatus',                                         -- varchar(50)
		@ProductSettingTypeId = @ProductSettingTypeId OUTPUT -- int

	-- Create the Value and assign it to the Product and ProductSettingType
	EXEC Enterprise.CreateProductSetting
		@ProductId = @ProductId,                             -- int
		@ProductSettingTypeId = @ProductSettingTypeId,                  -- int
		@Value = N'8',                               -- nvarchar(1000)
		@FromDate = @FromDate,          -- datetime
		@ThruDate = NULL,          -- datetime
		@ProductSettingId = @ProductSettingId OUTPUT -- int

	-- Link the Product Setting to an actual configuration
	EXEC Enterprise.LinkProductSettingToConfiguration
		@ConfigurationId = @ConfigurationId,              -- int
		@ProductSettingId = @ProductSettingId,             -- int
		@FromDate = @FromDate, -- datetime
		@ThruDate = NULL   -- datetime

	------------------------------------------------------------------------------------------------------------------------------------------------------
	-- Get the ProductSettingTypeId for the attribute you need to select, the API should have all these cached though.
	SET @ProductSettingId = NULL
	SET @ProductSettingTypeId = NULL

	EXEC Enterprise.GetProductSettingType
		@Name = 'ProductUrl',                                         -- varchar(50)
		@ProductSettingTypeId = @ProductSettingTypeId OUTPUT -- int

	-- Create the Value and assign it to the Product and ProductSettingType
	EXEC Enterprise.CreateProductSetting
		@ProductId = @ProductId,                             -- int
		@ProductSettingTypeId = @ProductSettingTypeId,                  -- int
		@Value = N'/#/employee-access',                               -- nvarchar(1000)
		@FromDate = @FromDate,          -- datetime
		@ThruDate = NULL,          -- datetime
		@ProductSettingId = @ProductSettingId OUTPUT -- int

	-- Link the Product Setting to an actual configuration
	EXEC Enterprise.LinkProductSettingToConfiguration
		@ConfigurationId = @ConfigurationId,              -- int
		@ProductSettingId = @ProductSettingId,             -- int
		@FromDate = @FromDate, -- datetime
		@ThruDate = NULL   -- datetime


	------------------------------------------------------------------------------------------------------------------------------------------------------
	EXEC Enterprise.LinkGlobalConfigurationToProduct
		@ConfigurationId = @ConfigurationId,              -- int
		@ProductId = @ProductId,                    -- int
		@FromDate = @FromDate, -- datetime
		@ThruDate = NULL   -- datetime

	EXEC Enterprise.ListGlobalSettingsForProduct @productid = @ProductId

END

-- Step 7 - Assign product to all RealPage Employee users
SET @ProductId = 35
DECLARE PersonaConfiguration CURSOR
FOR SELECT P.PersonaId FROM Enterprise.PartyRelationship PR
    INNER JOIN Person.Persona P
	   ON PR.PartyIdFrom = P.PersonPartyId
    WHERE RoleTypeIdFrom = 403
SET @PersonaId = NULL;
OPEN PersonaConfiguration;
FETCH NEXT FROM PersonaConfiguration INTO @PersonaId;
WHILE @@FETCH_STATUS = 0
    BEGIN
        EXEC Enterprise.CreatePersonaConfiguration
             @PersonaId,
             @ProductId,
             @FromDate,
             NULL;
        FETCH NEXT FROM PersonaConfiguration INTO @PersonaId;
    END;
CLOSE PersonaConfiguration;
DEALLOCATE PersonaConfiguration;

EXEC sys.sp_updateextendedproperty @name=N'Build', @value='64'
