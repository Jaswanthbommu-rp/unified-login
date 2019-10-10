--Renters Insurance
SET @FromDate = GETUTCDATE();

-- If this is a new Global Product Configuration then create a Product Configuration if you do not have one to use.
SELECT	@ProductId = ProductId
FROM	Enterprise.Product
WHERE	Name = 'Renters Insurance'

IF (@ProductId = 15)
BEGIN
	UPDATE	Ident.SamlProductSettings
	SET		LoginUri = 'http://sat.clientaccess.leasingdesk.realpage.com/greenbook.aspx', --Production 'https://www.clientaccess.leasingdesk.realpage.com/acs.aspx',
			SigningCertificateThumbprint = '0BE4C7B686D1918A4B2B571E8BF098B994990CAB'
	WHERE	ProductId = @ProductId

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
		@Value = N'7',                               -- nvarchar(1000)
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
		@Value = N'renters-insurance',                               -- nvarchar(1000)
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
		@Value = N'Renters Insurance',                               -- nvarchar(1000)
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
		@Value = N'EED3BAF4-46B3-48AC-A576-43659B233BA1',                               -- nvarchar(1000)
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
		@Value = N'Renters Insurance',                               -- nvarchar(1000)
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
		@Name = 'ProductUrl',                                         -- varchar(50)
		@ProductSettingTypeId = @ProductSettingTypeId OUTPUT -- int

	-- Create the Value and assign it to the Product and ProductSettingType
	EXEC Enterprise.CreateProductSetting
		@ProductId = @ProductId,                             -- int
		@ProductSettingTypeId = @ProductSettingTypeId,                  -- int
		@Value = N'/product/insurance',                               -- nvarchar(1000)
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
		@Name = 'ApiEndPoint',                                         -- varchar(50)
		@ProductSettingTypeId = @ProductSettingTypeId OUTPUT -- int

	-- Create the Value and assign it to the Product and ProductSettingType
	EXEC Enterprise.CreateProductSetting
		@ProductId = @ProductId,                             -- int
		@ProductSettingTypeId = @ProductSettingTypeId,                  -- int
		@Value = N'http://sat.ldweb.realpage.com/LDClientAccessService.asmx',                               -- nvarchar(1000)
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
	SET @ProductSettingId = NULL
	SET @ProductSettingTypeId = NULL

	EXEC Enterprise.GetProductSettingType
		@Name = 'ApiUserName',                                         -- varchar(50)
		@ProductSettingTypeId = @ProductSettingTypeId OUTPUT -- int

	-- Create the Value and assign it to the Product and ProductSettingType
	EXEC Enterprise.CreateProductSetting
		@ProductId = @ProductId,                             -- int
		@ProductSettingTypeId = @ProductSettingTypeId,                  -- int
		@Value = N'webservice@realpage.com',                               -- nvarchar(1000)
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
	SET @ProductSettingId = NULL
	SET @ProductSettingTypeId = NULL

	EXEC Enterprise.GetProductSettingType
		@Name = 'ApiPassword',                                         -- varchar(50)
		@ProductSettingTypeId = @ProductSettingTypeId OUTPUT -- int

	-- Create the Value and assign it to the Product and ProductSettingType
	EXEC Enterprise.CreateProductSetting
		@ProductId = @ProductId,                             -- int
		@ProductSettingTypeId = @ProductSettingTypeId,                  -- int
		@Value = N'k00BneerG',                               -- nvarchar(1000)
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
	SET @ProductSettingId = NULL
	SET @ProductSettingTypeId = NULL

	EXEC Enterprise.GetProductSettingType
		@Name = 'RequestedBy',                                         -- varchar(50)
		@ProductSettingTypeId = @ProductSettingTypeId OUTPUT -- int

	IF (@ProductSettingTypeId IS NULL)
	BEGIN
		EXEC Enterprise.CreateProductSettingType
			@ProductSettingTypeName = 'RequestedBy',
			@ProductSettingTypeDescription = 'Renters Insurance requested by Id',
			@ProductSettingTypeId = @ProductSettingTypeId  OUTPUT
	END

	-- Create the Value and assign it to the Product and ProductSettingType
	EXEC Enterprise.CreateProductSetting
		@ProductId = @ProductId,                             -- int
		@ProductSettingTypeId = @ProductSettingTypeId,                  -- int
		@Value = N'999',                               -- nvarchar(1000)
		@FromDate = @FromDate,          -- datetime
		@ThruDate = NULL,          -- datetime
		@ProductSettingId = @ProductSettingId OUTPUT -- int

	-- Link the Product Setting to an actual configuration
	EXEC Enterprise.LinkProductSettingToConfiguration
		@ConfigurationId = @ConfigurationId,              -- int
		@ProductSettingId = @ProductSettingId,             -- int
		@FromDate = @FromDate, -- datetime
		@ThruDate = NULL   -- datetime-- Link the Config Globally

	------------------------------------------------------------------------------------------------------------------------------------------------------
	EXEC Enterprise.LinkGlobalConfigurationToProduct
		@ConfigurationId = @ConfigurationId,              -- int
		@ProductId = @ProductId,                    -- int
		@FromDate = @FromDate, -- datetime
		@ThruDate = NULL   -- datetime

	EXEC Enterprise.ListGlobalSettingsForProduct @productid = @ProductId
END

EXEC sys.sp_updateextendedproperty @name=N'Build', @value='20'