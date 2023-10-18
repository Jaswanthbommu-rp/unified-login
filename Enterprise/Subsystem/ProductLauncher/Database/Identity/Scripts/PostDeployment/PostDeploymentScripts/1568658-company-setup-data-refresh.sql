GO
IF NOT EXISTS(SELECT 1 FROM Enterprise.ProductSettingType WHERE Name = 'MigrationUri')
BEGIN
	INSERT INTO Enterprise.ProductSettingType(Name,Description,SensitiveData)
	VALUES(N'MigrationUri',N'Migration Domain Name',DEFAULT)
END

IF NOT EXISTS(SELECT 1 FROM Enterprise.ProductSettingType WHERE Name = 'IgnoreProductsForMigrationTool')
BEGIN
	INSERT INTO Enterprise.ProductSettingType(Name,Description,SensitiveData)
	VALUES(N'IgnoreProductsForMigrationTool',N'List of product id that do not support migration',DEFAULT)
END

DECLARE @ProductSettingTypeId INT, @productId INT = 3, @NOW DATETIME = GETUTCDATE(),@ProductSettingId INT, @CurrentProductConfigurationID int;
SELECT @ProductSettingTypeId = ProductSettingTypeId FROM Enterprise.ProductSettingType WHERE Name = 'MigrationUri'
SELECT @ProductSettingTypeId

SELECT TOP 1 @CurrentProductConfigurationID = ConfigurationId
FROM Enterprise.GlobalProductConfiguration AS gpc
WHERE gpc.ProductId = @ProductId AND ((@NOW BETWEEN gpc.FromDate AND gpc.ThruDate ) OR (@NOW >= gpc.FromDate AND gpc.ThruDate IS NULL))
ORDER BY GlobalProductConfigurationId DESC;

 IF @ProductSettingTypeId IS NOT NULL AND NOT EXISTS(
		SELECT TOP 1 1
		FROM Enterprise.ProductSetting
		WHERE ProductID = @productId AND 
			  ProductSettingTypeId = @ProductSettingTypeId AND 
			  ThruDate IS NULL
	)
	BEGIN

		Declare @ServerName SYSNAME = @@SERVERNAME, @apiendpoint varchar(256) = '';

		IF @ServerName IN ('RCDUSODBSQL001')  --DEV
		BEGIN
			SET @apiendpoint = 'https://ulmtapidev.realpage.com';
		END
		IF @ServerName IN ('rctusodbsql001') --QA
		BEGIN
			SET @apiendpoint = 'https://ulmtapiqa.realpage.com';
		END
		IF @ServerName IN ('rcausodbsql001') --SAT
		BEGIN
			SET @apiendpoint = 'https://ulmtapisat.realpage.com';
		END
		IF @ServerName IN ('RCTUSODBSQL001A','RCTUSODBSQL001B') --UAT
		BEGIN
			Set @apiendpoint = 'https://ulmtapiuat.realpage.com';
		END
		IF @ServerName IN ('RCIUSODBSQL002') --PREPROD
		BEGIN
			Set @apiendpoint = 'https://ulmtapipreprod.realpage.com';
		END

		IF @ServerName IN ('RCVGBKDBSQL001') --DEMO
		BEGIN
			Set @apiendpoint = 'https://ulmtapidemo.realpage.com';
		END

		IF @ServerName IN ('RCTUSODBTUL001') --TRAINING
		BEGIN
			Set @apiendpoint = 'https://ulmtapitraining.realpage.com';
		END

		IF @ServerName IN ('RCPGBKDBSQL005A', 'RCPGBKDBSQL005B') --PROD
		BEGIN
			Set @apiendpoint = 'https://ulmtapi.realpage.com';
		END

		IF @ServerName IN ('reagbkdbsql001') --EUSAT
		BEGIN
			SET @apiendpoint = 'https://ulmtapisat.realpage.co.uk';
		END

		IF @ServerName IN ('repgbkdbsql001a','repgbkdbsql001b') --EUPROD
		BEGIN
			SET @apiendpoint = 'https://ulmtapi.realpage.co.uk';
		END

		-- Create the Value and assign it to the Product and ProductSettingType
		EXEC Enterprise.CreateProductSetting @ProductId = @ProductId, -- int
											@ProductSettingTypeId = @ProductSettingTypeId, -- int
											@Value = @apiendpoint, 
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

DECLARE @ProductSettingTypeId INT, @productId INT = 3, @NOW DATETIME = GETUTCDATE(),@ProductSettingId INT, @CurrentProductConfigurationID int;
SELECT @ProductSettingTypeId = ProductSettingTypeId FROM Enterprise.ProductSettingType WHERE Name = 'IgnoreProductsForMigrationTool'
SELECT @ProductSettingTypeId

SELECT TOP 1 @CurrentProductConfigurationID = ConfigurationId
FROM Enterprise.GlobalProductConfiguration AS gpc
WHERE gpc.ProductId = @ProductId AND ((@NOW BETWEEN gpc.FromDate AND gpc.ThruDate ) OR (@NOW >= gpc.FromDate AND gpc.ThruDate IS NULL))
ORDER BY GlobalProductConfigurationId DESC;

 IF @ProductSettingTypeId IS NOT NULL AND NOT EXISTS(
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
											@Value = '2,3,5,7,11,12,19,21,22,24,25,26,27,28,29,30,31,32,33,34,35,36,37,38,39,43,45', 
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