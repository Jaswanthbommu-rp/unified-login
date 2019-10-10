SET @ClientName = 'UnifiedLogin Research Application'
SET @ClientCode = 'UnifiedLoginResearchApp'

--Research Application
SET @ClientId = NULL
SET @FromDate = GETUTCDATE();

-- If this is a new Global Product Configuration then create a Product Configuration if you do not have one to use.
SELECT	@ProductId = ProductId
FROM		Enterprise.Product
WHERE	Name = 'Research Application'

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
IF (@ProductId = 24)
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
	SET @ProductSettingId = NULL
	SET @ProductSettingTypeId = NULL

	EXEC Enterprise.GetProductSettingType
		@Name = 'ApiEndPoint',                                         -- varchar(50)
		@ProductSettingTypeId = @ProductSettingTypeId OUTPUT -- int

	-- Create the Value and assign it to the Product and ProductSettingType
	EXEC Enterprise.CreateProductSetting
		@ProductId = @ProductId,                             -- int
		@ProductSettingTypeId = @ProductSettingTypeId,                  -- int
		@Value = N'https://blackbookresearch-dev.realpage.com',                               -- nvarchar(1000)
		@FromDate = @FromDate,          -- datetime
		@ThruDate = NULL,          -- datetime
		@ProductSettingId = @ProductSettingId OUTPUT -- int

	-- Link the Product Setting to an actual configuration
	EXEC Enterprise.LinkProductSettingToConfiguration
		@ConfigurationId = @ConfigurationId,              -- int
		@ProductSettingId = @ProductSettingId,             -- int
		@FromDate = @FromDate, -- datetime
		@ThruDate = NULL   -- datetime

	EXEC Enterprise.ListGlobalSettingsForProduct @productid = @ProductId
END

SELECT	@ProductId = ProductId
FROM		Enterprise.Product
WHERE	Name = 'Unified Login'

IF (@ProductId = 3)
BEGIN
	SELECT	@ConfigurationId = pc.ConfigurationId
	FROM		Enterprise.GlobalProductConfiguration gpc  
					JOIN Enterprise.ProductConfiguration pc ON pc.ConfigurationId = gpc.ConfigurationId  
					JOIN Enterprise.ProductSetting ps ON ps.ProductSettingId = pc.ProductSettingId  
					JOIN Enterprise.ProductSettingType pst ON pst.ProductSettingTypeId = ps.ProductSettingTypeId  
	WHERE	gpc.ProductId = @ProductId 
	AND			pst.Name = 'ClientId'
	AND			((@FromDate BETWEEN gpc.FromDate AND gpc.ThruDate) OR (@FromDate >= gpc.FromDate AND gpc.ThruDate IS NULL))  
	AND			((@FromDate BETWEEN pc.FromDate AND pc.ThruDate) OR (@FromDate >= pc.FromDate AND pc.ThruDate IS NULL))  
	AND			((@FromDate BETWEEN ps.FromDate AND ps.ThruDate) OR (@FromDate >= ps.FromDate AND ps.ThruDate IS NULL)) 

	------------------------------------------------------------------------------------------------------------------------------------------------------
	-- Get the ProductSettingTypeId for the attribute you need to select, the API should have all these cached though.
	SET @ProductSettingId = NULL
	SET @ProductSettingTypeId = NULL

	EXEC Enterprise.GetProductSettingType
		@Name = 'UnifiedLoginResearchApplicationClientSecret',                                         -- varchar(50)
		@ProductSettingTypeId = @ProductSettingTypeId OUTPUT -- int

	IF (@ProductSettingTypeId IS NULL)
	BEGIN
		EXEC Enterprise.CreateProductSettingType
			@ProductSettingTypeName = 'UnifiedLoginResearchApplicationClientSecret',
			@ProductSettingTypeDescription = 'UnifiedLogin Research Application Client Secret',
			@ProductSettingTypeId = @ProductSettingTypeId  OUTPUT
	END

	-- Create the Value and assign it to the Product and ProductSettingType
	EXEC Enterprise.CreateProductSetting
		@ProductId = @ProductId,                             -- int
		@ProductSettingTypeId = @ProductSettingTypeId,                  -- int
		@Value = N'MzNCNUY3OTgtQkU1NS00MkJDLThBQTgtMDAyNUI5MDNEQzNC',                               -- nvarchar(1000)
		@FromDate = @FromDate,          -- datetime
		@ThruDate = NULL,          -- datetime
		@ProductSettingId = @ProductSettingId OUTPUT -- int

	-- Link the Product Setting to an actual configuration
	EXEC Enterprise.LinkProductSettingToConfiguration
		@ConfigurationId = @ConfigurationId,              -- int
		@ProductSettingId = @ProductSettingId,             -- int
		@FromDate = @FromDate, -- datetime
		@ThruDate = NULL   -- datetime

	EXEC Enterprise.ListGlobalSettingsForProduct @productid = @ProductId
END

EXEC sys.sp_updateextendedproperty @name=N'Build', @value='63'