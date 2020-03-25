GO

DECLARE @ProductId INT = 3,
		 @CurrentProductConfigurationID INT,
		 @ProductSettingTypeId INT,
		 @ProductSettingId INT,
		 @Now DATETIME = GETUTCDATE()

SELECT TOP 1 @CurrentProductConfigurationID = ConfigurationId
FROM Enterprise.GlobalProductConfiguration AS gpc
WHERE gpc.ProductId = @ProductId AND 
		( ( @NOW BETWEEN gpc.FromDate AND gpc.ThruDate
		) OR 
		( @NOW >= gpc.FromDate AND 
			gpc.ThruDate IS NULL
		)
		)
ORDER BY GlobalProductConfigurationId DESC;

IF
(
	SELECT 1
	FROM Enterprise.ProductSettingType
	WHERE Name = 'IDS_EnableWebApiDiagnostics'
) IS NULL
BEGIN
	EXEC Enterprise.CreateProductSettingType 'IDS_EnableWebApiDiagnostics', 'Used to enable webapi diagnostics for IdentityServer', @ProductSettingTypeId OUTPUT;
END;

IF @ProductSettingTypeId IS NOT NULL AND 
	   NOT EXISTS
	(
		SELECT TOP 1 1
		FROM Enterprise.ProductSetting
		WHERE ProductID = @productId AND 
			  ProductSettingTypeId = @ProductSettingTypeId AND 
			  ThruDate IS NULL
	)
	BEGIN
	
		-- Create the Value and assign it to the Product and ProductSettingType
		EXEC Enterprise.CreateProductSetting @ProductId = @ProductId, -- int
		@ProductSettingTypeId = @ProductSettingTypeId, -- int
		@Value = '0', 
		@FromDate = @NOW, -- datetime
		@ThruDate = NULL, -- datetime
		@ProductSettingId = @ProductSettingId OUTPUT; -- int

		-- Link the Product Setting to an actual configuration
		EXEC Enterprise.LinkProductSettingToConfiguration @ConfigurationId = @CurrentProductConfigurationID, -- int
		@ProductSettingId = @ProductSettingId, -- int
		@FromDate = @NOW, -- datetime
		@ThruDate = NULL;   -- datetime
	END;


IF
(
	SELECT 1
	FROM Enterprise.ProductSettingType
	WHERE Name = 'IDS_EnableKatanaLogging'
) IS NULL
BEGIN
	EXEC Enterprise.CreateProductSettingType 'IDS_EnableKatanaLogging', 'Used to enable Katana logging for IdentityServer', @ProductSettingTypeId OUTPUT;
END;

IF @ProductSettingTypeId IS NOT NULL AND 
	   NOT EXISTS
	(
		SELECT TOP 1 1
		FROM Enterprise.ProductSetting
		WHERE ProductID = @productId AND 
			  ProductSettingTypeId = @ProductSettingTypeId AND 
			  ThruDate IS NULL
	)
	BEGIN
	
		-- Create the Value and assign it to the Product and ProductSettingType
		EXEC Enterprise.CreateProductSetting @ProductId = @ProductId, -- int
		@ProductSettingTypeId = @ProductSettingTypeId, -- int
		@Value = '0', 
		@FromDate = @NOW, -- datetime
		@ThruDate = NULL, -- datetime
		@ProductSettingId = @ProductSettingId OUTPUT; -- int

		-- Link the Product Setting to an actual configuration
		EXEC Enterprise.LinkProductSettingToConfiguration @ConfigurationId = @CurrentProductConfigurationID, -- int
		@ProductSettingId = @ProductSettingId, -- int
		@FromDate = @NOW, -- datetime
		@ThruDate = NULL;   -- datetime
	END;

IF
(
	SELECT 1
	FROM Enterprise.ProductSettingType
	WHERE Name = 'IDS_EnableHttpLogging'
) IS NULL
BEGIN
	EXEC Enterprise.CreateProductSettingType 'IDS_EnableHttpLogging', 'Used to enable HTTP logging for IdentityServer', @ProductSettingTypeId OUTPUT;
END;

IF @ProductSettingTypeId IS NOT NULL AND 
	   NOT EXISTS
	(
		SELECT TOP 1 1
		FROM Enterprise.ProductSetting
		WHERE ProductID = @productId AND 
			  ProductSettingTypeId = @ProductSettingTypeId AND 
			  ThruDate IS NULL
	)
	BEGIN
	
		-- Create the Value and assign it to the Product and ProductSettingType
		EXEC Enterprise.CreateProductSetting @ProductId = @ProductId, -- int
		@ProductSettingTypeId = @ProductSettingTypeId, -- int
		@Value = '0', 
		@FromDate = @NOW, -- datetime
		@ThruDate = NULL, -- datetime
		@ProductSettingId = @ProductSettingId OUTPUT; -- int

		-- Link the Product Setting to an actual configuration
		EXEC Enterprise.LinkProductSettingToConfiguration @ConfigurationId = @CurrentProductConfigurationID, -- int
		@ProductSettingId = @ProductSettingId, -- int
		@FromDate = @NOW, -- datetime
		@ThruDate = NULL;   -- datetime
	END;

IF
(
	SELECT 1
	FROM Enterprise.ProductSettingType
	WHERE Name = 'IDS_WebApiDiagnosticsIsVerbose'
) IS NULL
BEGIN
	EXEC Enterprise.CreateProductSettingType 'IDS_WebApiDiagnosticsIsVerbose', 'Used to enable verbose webapi diagnostics for IdentityServer', @ProductSettingTypeId OUTPUT;
END;


IF @ProductSettingTypeId IS NOT NULL AND 
	   NOT EXISTS
	(
		SELECT TOP 1 1
		FROM Enterprise.ProductSetting
		WHERE ProductID = @productId AND 
			  ProductSettingTypeId = @ProductSettingTypeId AND 
			  ThruDate IS NULL
	)
	BEGIN
	
		-- Create the Value and assign it to the Product and ProductSettingType
		EXEC Enterprise.CreateProductSetting @ProductId = @ProductId, -- int
		@ProductSettingTypeId = @ProductSettingTypeId, -- int
		@Value = '0', 
		@FromDate = @NOW, -- datetime
		@ThruDate = NULL, -- datetime
		@ProductSettingId = @ProductSettingId OUTPUT; -- int

		-- Link the Product Setting to an actual configuration
		EXEC Enterprise.LinkProductSettingToConfiguration @ConfigurationId = @CurrentProductConfigurationID, -- int
		@ProductSettingId = @ProductSettingId, -- int
		@FromDate = @NOW, -- datetime
		@ThruDate = NULL;   -- datetime
	END;

IF
(
	SELECT 1
	FROM Enterprise.ProductSettingType
	WHERE Name = 'IdentityServerLogEventLevel'
) IS NULL
BEGIN
	EXEC Enterprise.CreateProductSettingType 'IdentityServerLogEventLevel', 'Used to control the level of logging for IdentityServer', @ProductSettingTypeId OUTPUT;
END;


IF @ProductSettingTypeId IS NOT NULL AND 
	   NOT EXISTS
	(
		SELECT TOP 1 1
		FROM Enterprise.ProductSetting
		WHERE ProductID = @productId AND 
			  ProductSettingTypeId = @ProductSettingTypeId AND 
			  ThruDate IS NULL
	)
	BEGIN
	
		-- Create the Value and assign it to the Product and ProductSettingType
		EXEC Enterprise.CreateProductSetting @ProductId = @ProductId, -- int
		@ProductSettingTypeId = @ProductSettingTypeId, -- int
		@Value = '2', 
		@FromDate = @NOW, -- datetime
		@ThruDate = NULL, -- datetime
		@ProductSettingId = @ProductSettingId OUTPUT; -- int

		-- Link the Product Setting to an actual configuration
		EXEC Enterprise.LinkProductSettingToConfiguration @ConfigurationId = @CurrentProductConfigurationID, -- int
		@ProductSettingId = @ProductSettingId, -- int
		@FromDate = @NOW, -- datetime
		@ThruDate = NULL;   -- datetime
	END;

GO
