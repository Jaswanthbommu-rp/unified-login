--declare @fromdate datetime
--declare @ProductId int
--declare @ConfigurationId int
--declare @ProductSettingId int
--declare @ProductSettingTypeId int

-- Step 1 - In case product type is missing, add one

IF NOT EXISTS(SELECT 1 FROM Enterprise.ProductType WHERE Name = 'Internal Tools' AND ProductTypeId = 600 )
BEGIN
	INSERT INTO Enterprise.ProductType ( ProductTypeId, ParentProductTypeId, Name, Description)
		values
			(600, null, 'Internal Tools', 'Internal Tools') 
END

IF NOT EXISTS(SELECT 1 FROM Enterprise.ProductType WHERE Name = 'Research Tool' AND ProductTypeId = 601 )
BEGIN
	INSERT INTO Enterprise.ProductType ( ProductTypeId, ParentProductTypeId, Name, Description)
		values
			(601, 600, 'Research Tool', 'Research Tool' ) 
END

--step 2 - add product
IF NOT EXISTS(SELECT 1 FROM Enterprise.Product WHERE productid = 24)
BEGIN
	INSERT INTO Enterprise.Product (ProductId, ProductGUID, Name, Description, ProductTypeId)
	VALUES (24, '2C017D7E-0E70-47AF-BB0F-82C9DF818EC5',  'Research Application', 'Research Application', 601)
END

SET @clientID = 0

--Step 3 -- Add all settings

SET @FromDate = GETUTCDATE();

-- If this is a new Global Product Configuration then create a Product Configuration if you do not have one to use.
SELECT	@ProductId = ProductId
FROM	Enterprise.Product
WHERE	ProductId = 24

-- select * from [identitydevelopment].Ident.SamlProductSettings where productid = 24

IF (not exists ( select top 1 * from Ident.SamlProductSettings where productid = 24))
BEGIN
	INSERT INTO	Ident.SamlProductSettings ( ProductId, LoginUri, SigningCertificateThumbprint, SubjectIdSamlAttribute )
	values (24, 'https://blackbookresearch-dev.realpage.com', 'NA', 'productUsername' )
END


IF (not exists ( select top 1 * from Enterprise.GlobalProductConfiguration where productid = 24))
BEGIN
	EXEC Enterprise.CreateProductConfiguration @ConfigurationId = @ConfigurationId OUTPUT -- int
	
	SELECT @clientID = ClientID from Auth.Clients where clientname = 'BlackBook'

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
		@Value = N'researchapplication',                               -- nvarchar(1000)
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
		@Value = N'ResearchApplication',                               -- nvarchar(1000)
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
		@Value = N'3293CD7A-55AC-4715-8E52-AADA560C4947',                               -- nvarchar(1000)
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
		@Name = 'MetatagUniqueId',                                         -- varchar(50)
		@ProductSettingTypeId = @ProductSettingTypeId OUTPUT -- int

	-- Create the Value and assign it to the Product and ProductSettingType
	EXEC Enterprise.CreateProductSetting
		@ProductId = @ProductId,                             -- int
		@ProductSettingTypeId = @ProductSettingTypeId,                  -- int
		@Value = N'ResearchApplication',                               -- nvarchar(1000)
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
		@Value = N'/product/researchapplication',                               -- nvarchar(1000)
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

IF NOT EXISTS
(
    SELECT 1
    FROM Auth.Scopes
    WHERE Name = 'greenbooknwpapi'
)
BEGIN
	INSERT INTO Auth.Scopes(Name, DisplayName, Enabled, Required, Emphasize, Type, IncludeAllClaimsForUser, ShowInDiscoveryDocument, AllowUnrestrictedIntrospection) 
	VALUES('greenbooknwpapi', 'GreenBookNwpApi', 1, 0, 0, 1, 1, 1, 0)
END;
-----------------------------------------------------------------------------------
--DECLARE @ClientID INT
SET @ClientId = NULL;
SELECT @ClientId = ClientId FROM Auth.Clients WHERE ClientCode = 'greenbooknwpapi' AND Flow = 3
	IF @ClientId IS NULL
		BEGIN
			INSERT INTO Auth.Clients(ClientCode, ClientName, Flow, IdentityTokenLifetime, AccessTokenLifetime, AuthorizationCodeLifetime, AbsoluteRefreshTokenLifetime, SlidingRefreshTokenLifetime, RefreshTokenUsage, RefreshTokenExpiration, AccessTokenType, UpdateAccessTokenOnRefresh, Enabled, LogoutSessionRequired, RequireSignOutPrompt, AllowAccessToAllScopes, AllowClientCredentialsOnly, RequireConsent, AllowRememberConsent, EnableLocalLogin, IncludeJwtId, AlwaysSendClientClaims, PrefixClientClaims, AllowAccessToAllGrantTypes) 
			VALUES('greenbooknwpapi', 'Resident Utility Management', 3, 360, 3600, 360, 86400, 3600, 0, 0, 0, 'True', 'True', 'False', 'False', 'True', 'False', 'False', 'True', 'True', 'True', 'True', 'True', 'True')

			SELECT @ClientId=SCOPE_IDENTITY()

			INSERT INTO Auth.ClientScopes(ClientId, Scope)
			VALUES(@ClientId, 'greenbooknwpapi')

			INSERT INTO Auth.ClientSecrets(ClientId, Value, Description, Expiration)
			VALUES(@ClientId, '5jMIFx8mOxhZD+YCMNiBKlYAn7Q4RiycqhZtt5+DhcU=', 'Resident Utility Management', '2099-12-31 00:00:00.0000000 -06:00')
		END

DECLARE @PSID INT = NULL
DECLARE @ConfigID INT = (select ConfigurationId from enterprise.GlobalProductConfiguration where productid = 18 )

IF NOT EXISTS (SELECT 1 FROM Enterprise.ProductSetting WHERE ProductId = 18 AND ProductSettingTypeId = 1) 
BEGIN
	INSERT INTO Enterprise.ProductSetting (ProductId, ProductSettingTypeId, Value)
	VALUES(18, 1, 'greenbooknwpapi')

	SELECT @PSID = SCOPE_IDENTITY()

	INSERT INTO Enterprise.ProductConfiguration (ConfigurationId, ProductSettingId, FromDate)
	VALUES(@ConfigID, @PSID, GETUTCDATE())
END

--INSERT INTO Enterprise.ProductSetting (ProductId, ProductSettingTypeId, Value)
--VALUES(18, 1005, 'https://apisat.nwp.com/Identity')

IF NOT EXISTS (SELECT 1 FROM Enterprise.ProductSetting WHERE ProductId = 18 AND ProductSettingTypeId = 1021) 
BEGIN
	INSERT INTO Enterprise.ProductSetting (ProductId, ProductSettingTypeId, Value)
	VALUES(18, 1021, 'api-secret1')

	SELECT @PSID = SCOPE_IDENTITY()

	INSERT INTO Enterprise.ProductConfiguration (ConfigurationId, ProductSettingId, FromDate)
	VALUES(@ConfigID, @PSID, GETUTCDATE())
END

IF NOT EXISTS (SELECT 1 FROM Enterprise.ProductSetting WHERE ProductId = 18 AND ProductSettingTypeId = 1024) 
BEGIN
	Insert into [Enterprise].[ProductSetting](ProductId,ProductSettingTypeId,Value,FromDate,ThruDate)
	VALUES(18,1024,'https://mylDev.corp.realpage.com/identity',GETUTCDATE(),NULL)

	SELECT @PSID = SCOPE_IDENTITY()

	INSERT INTO Enterprise.ProductConfiguration (ConfigurationId, ProductSettingId, FromDate)
	VALUES(@ConfigID, @PSID, GETUTCDATE())
END

IF NOT EXISTS(SELECT 1 FROM Ident.SamlAttribute WHERE Name = 'NWPUserType')
     Insert into [Ident].[SamlAttribute](Name,SamlAttributeTypeId)
     VALUES('NWPUserType',1)
	 
-----------------------------------------------------------------------------------
IF NOT EXISTS
(
    SELECT 1
    FROM Auth.Scopes
    WHERE Name = 'activityreader'
)
BEGIN
	INSERT INTO Auth.Scopes(Name, DisplayName, Enabled, Required, Emphasize, Type, IncludeAllClaimsForUser, ShowInDiscoveryDocument, AllowUnrestrictedIntrospection) 
	VALUES('activityreader', 'Activity Reader - Logging', 1, 0, 0, 1, 1, 1, 0)
END;
-----------------------------------------------------------------------------------
SET @ClientId = NULL;
SELECT @ClientId = ClientId FROM Auth.Clients WHERE ClientCode = 'activityreader' AND Flow = 1
	IF @ClientId IS NULL
		BEGIN
			INSERT INTO Auth.Clients(ClientCode, ClientName, Flow, IdentityTokenLifetime, AccessTokenLifetime, AuthorizationCodeLifetime, AbsoluteRefreshTokenLifetime, SlidingRefreshTokenLifetime, RefreshTokenUsage, RefreshTokenExpiration, AccessTokenType, UpdateAccessTokenOnRefresh, Enabled, LogoutSessionRequired, RequireSignOutPrompt, AllowAccessToAllScopes, AllowClientCredentialsOnly, RequireConsent, AllowRememberConsent, EnableLocalLogin, IncludeJwtId, AlwaysSendClientClaims, PrefixClientClaims, AllowAccessToAllGrantTypes) 
			VALUES('activityreader', 'Activity Reader', 1, 360, 3600, 360, 86400, 3600, 0, 0, 0, 'True', 'True', 'False', 'False', 'True', 'False', 'False', 'True', 'True', 'True', 'True', 'True', 'True')

			SELECT @ClientId=SCOPE_IDENTITY()

			INSERT INTO Auth.ClientScopes(ClientId, Scope)
			VALUES(@ClientId, 'activityreader')

			INSERT INTO Auth.ClientSecrets(ClientId, Value, Description, Expiration)
			VALUES(@ClientId, 'gKkyux328RADLpXIJrmh7TW1wOvGVATZAPipNjVUYPY=', 'Activity Reader - Logging', '2099-12-31 00:00:00.0000000 -06:00')

			INSERT INTO Auth.ClientRedirectUris (ClientId, Uri)
			VALUES (@ClientId, 'https://activitylogging/swagger/ui/o2c-html')
		END;
-----------------------------------------------------------------------------------

EXEC sys.sp_updateextendedproperty @name=N'Build', @value='46'