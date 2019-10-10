
SET @ClientId = NULL
SET @ProductId = 26
SET @FromDate = GETUTCDATE()

DECLARE @AuthClients050 TABLE (
	ClientId int
);

IF NOT EXISTS(SELECT TOP 1 1 FROM Auth.Scopes WHERE Name = 'unifiedamenities')
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
		'unifiedamenities', --Name,
		'Unified Amenities', --DisplayName,
		NULL, --Description,
		NULL, --ClaimsRule,
		1, --Enabled,
		0, --Required,
		1, --Emphasize,
		0, --Type,
		0, --IncludeAllClaimsForUser,
		1, --ShowInDiscoveryDocument,
		1 --AllowUnrestrictedIntrospection
	)
END

IF NOT EXISTS(SELECT TOP 1 1 FROM Auth.Scopes WHERE Name = 'unifiedamenitiesapi')
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
		'unifiedamenitiesapi', --Name,
		'Unified Amenities API', --DisplayName,
		NULL, --Description,
		NULL, --ClaimsRule,
		1, --Enabled,
		0, --Required,
		1, --Emphasize,
		1, --Type,
		0, --IncludeAllClaimsForUser,
		1, --ShowInDiscoveryDocument,
		1 --AllowUnrestrictedIntrospection
	)
END

--Implicit Flow
IF NOT EXISTS(SELECT TOP 1 1 FROM Auth.Clients WHERE ClientCode = 'unifiedamenities' AND Flow = 1)
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
	OUTPUT INSERTED.ClientId INTO @AuthClients050 (ClientId)
	VALUES (
					'unifiedamenities', --ClientCode
					'Unified Amenities', --ClientName
					'http://localhost:5101/', --ClientUri
					'https://somesite.com/logo/logo.jpg', --LogoUri
					1, --Implicit Flow
					'http://localhost/logout', --LogoutUri
					360, --IdentityTokenLifetime
					3600, --AccessTokenLifetime
					360, --AuthorizationCodeLifetime
					86400, --AbsoluteRefreshTokenLifetime
					3600, --SlidingRefreshTokenLifetime
					0, --RefreshTokenUsage
					0, --RefreshTokenExpiration
					1, --AccessTokenType
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
	FROM	@AuthClients050

	INSERT INTO Auth.ClientScopes (
		ClientId,
		Scope
	)
	SELECT	@ClientId, --ClientId
					ScopeId --Scope
	FROM		Auth.Scopes
	WHERE Name = 'unifiedamenities'

	INSERT INTO Auth.ClientRedirectUris (
		ClientId,
		Uri
	)
	SELECT	@ClientId, --ClientId
		'http://localhost:5101/signin-oidc' --Uri
END

DELETE FROM @AuthClients050

IF NOT EXISTS(SELECT TOP 1 1 FROM Auth.Clients WHERE ClientCode = 'unifiedamenitiesapi' AND Flow = 1)
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
	OUTPUT INSERTED.ClientId INTO @AuthClients050 (ClientId)
	VALUES (
					'unifiedamenitiesapi', --ClientCode
					'Unified Amenities API', --ClientName
					'http://localhost:5101/', --ClientUri
					'https://somesite.com/logo/logo.jpg', --LogoUri
					1, --Implicit Flow
					'http://localhost:5101/logout', --LogoutUri
					360, --IdentityTokenLifetime
					3600, --AccessTokenLifetime
					360, --AuthorizationCodeLifetime
					86400, --AbsoluteRefreshTokenLifetime
					3600, --SlidingRefreshTokenLifetime
					0, --RefreshTokenUsage
					0, --RefreshTokenExpiration
					1, --AccessTokenType
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
	FROM	@AuthClients050

	INSERT INTO Auth.ClientScopes (
		ClientId,
		Scope
	)
	SELECT	@ClientId, --ClientId
					ScopeId --Scope
	FROM		Auth.Scopes
	WHERE Name = 'unifiedamenitiesapi'

	INSERT INTO Auth.ClientRedirectUris (
		ClientId,
		Uri
	)
	SELECT	@ClientId, --ClientId
		'http://localhost:5101/signin-oidc' --Uri
END



IF NOT EXISTS(SELECT TOP 1 1 FROM Enterprise.ProductType WHERE Name = 'Unified Amenities')
BEGIN
	INSERT INTO Enterprise.ProductType (
		ProductTypeId,
		ParentProductTypeId,
		Name,
		Description,
		ProductTypeGuid
	)
	VALUES (
		505, --ProductTypeId,
		500, --ParentProductTypeId: Administration,
		'Unified Amenities', --Name,
		'Unified Amenities', --Description,
		'BE52EE9B-1386-413D-ADDA-0EFF55E8F232' --ProductTypeGuid
	)
END

IF NOT EXISTS(SELECT TOP 1 1 FROM Enterprise.Product WHERE ProductId = @ProductId AND Name = 'Unified Amenities')
BEGIN
	INSERT INTO Enterprise.Product (
		ProductId,
		ProductGUID,
		Name,
		Description,
		ProductTypeId
	)
	SELECT	@ProductId, --ProductId
					'72DE0260-F3ED-4573-AED0-061352175464', --ProductGUID
					'Unified Amenities', --Name
					'Unified Amenities', --Description
					ProductTypeID
	FROM		Enterprise.ProductType
	WHERE	Name = 'Unified Amenities'

	EXEC Enterprise.CreateProductConfiguration @ConfigurationId = @ConfigurationId OUTPUT -- int

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
		@Value = N'unifiedamenities',                               -- nvarchar(1000)
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
		@Value = 'unifiedamenities',                               -- nvarchar(1000)
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
		@Value = 'Unified Amenities',                               -- nvarchar(1000)
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
		@Name = 'TitleUniqueId',                                         -- varchar(50)
		@ProductSettingTypeId = @ProductSettingTypeId OUTPUT -- int

	-- Create the Value and assign it to the Product and ProductSettingType
	EXEC Enterprise.CreateProductSetting
		@ProductId = @ProductId,                             -- int
		@ProductSettingTypeId = @ProductSettingTypeId,                  -- int
		@Value = N'5A12C9DB-FF5B-48A9-B381-75742A524C78',                               -- nvarchar(1000)
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
		@Name = 'ProductUrl',                                         -- varchar(50)
		@ProductSettingTypeId = @ProductSettingTypeId OUTPUT -- int

	-- Create the Value and assign it to the Product and ProductSettingType
	EXEC Enterprise.CreateProductSetting
		@ProductId = @ProductId,                             -- int
		@ProductSettingTypeId = @ProductSettingTypeId,                  -- int
		@Value = N'/product/unifiedamenities',                               -- nvarchar(1000)
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
		@Value = 'Unified Amenities',                               -- nvarchar(1000)
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

IF NOT EXISTS(SELECT TOP 1 1 FROM Ident.SamlProductSettings WHERE ProductId = @ProductId)
BEGIN
	INSERT INTO Ident.SamlProductSettings (
		ProductId,
		LoginUri,
		SigningCertificateThumbprint,
		SubjectIdSamlAttribute
	)
	VALUES (
		@ProductId, --ProductId
		'http://localhost:4544/signin-oidc', --LoginUri
		'NA', --SigningCertificateThumbprint
		'productUsername' --SubjectIdSamlAttribute
	)
END

EXEC sys.sp_updateextendedproperty @name=N'Build', @value='51'