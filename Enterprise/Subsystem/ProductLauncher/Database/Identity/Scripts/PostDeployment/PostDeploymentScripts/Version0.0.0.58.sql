--DECLARE @ProductSettingTypeId INT
--DECLARE @ProductId INT
--DECLARE @ProductSettingId INT
--DECLARE @ConfigurationId INT

SELECT @ProductId = ProductId 
FROM Enterprise.Product 
WHERE Name = 'OneSite';

SELECT @ConfigurationId = ConfigurationId 
FROM Enterprise.GlobalProductConfiguration 
WHERE ProductId = @ProductId 
  AND (GetDate() BETWEEN FromDate AND ThruDate
       OR ThruDate IS NULL);

SET @ProductSettingTypeId = NULL;
SET @ProductSettingId = NULL;

IF (SELECT 1 FROM Enterprise.ProductSettingType WHERE Name = 'MTApiEndPoint') IS NULL
BEGIN
EXEC Enterprise.CreateProductSettingType 'MTApiEndPoint', 'MTApiEndPoint', @ProductSettingTypeId OUTPUT
EXEC Enterprise.CreateProductSetting @ProductId, @ProductSettingTypeId, 'api/core/common/ulmigration', NULL, NULL, @ProductSettingId OUTPUT
EXEC Enterprise.LinkProductSettingToConfiguration @ConfigurationId, @ProductSettingId, NULL, NULL
END;

SET @ProductSettingTypeId = NULL;
SET @ProductSettingId = NULL;

IF (SELECT 1 FROM Enterprise.ProductSettingType WHERE Name = 'MTTokenEndPoint') IS NULL
BEGIN
EXEC Enterprise.CreateProductSettingType 'MTTokenEndPoint', 'MTTokenEndPoint', @ProductSettingTypeId OUTPUT
EXEC Enterprise.CreateProductSetting @ProductId, @ProductSettingTypeId, 'api/core/authentication/login', NULL, NULL, @ProductSettingId OUTPUT
EXEC Enterprise.LinkProductSettingToConfiguration @ConfigurationId, @ProductSettingId, NULL, NULL
END;

SET @ProductSettingTypeId = NULL;
SET @ProductSettingId = NULL;

IF (SELECT 1 FROM Enterprise.ProductSettingType WHERE Name = 'MTClientId') IS NULL
BEGIN
EXEC Enterprise.CreateProductSettingType 'MTClientId', 'MTClientId', @ProductSettingTypeId OUTPUT
EXEC Enterprise.CreateProductSetting @ProductId, @ProductSettingTypeId, 'Unified_Login', NULL, NULL, @ProductSettingId OUTPUT
EXEC Enterprise.LinkProductSettingToConfiguration @ConfigurationId, @ProductSettingId, NULL, NULL
END;

SET @ProductSettingTypeId = NULL;
SET @ProductSettingId = NULL;

IF (SELECT 1 FROM Enterprise.ProductSettingType WHERE Name = 'MTClientSecret') IS NULL
BEGIN
EXEC Enterprise.CreateProductSettingType 'MTClientSecret', 'MTClientSecret', @ProductSettingTypeId OUTPUT
EXEC Enterprise.CreateProductSetting @ProductId, @ProductSettingTypeId, '7C858876-B6D4-47AD-86DB-6B8FC95C4420', NULL, NULL, @ProductSettingId OUTPUT
EXEC Enterprise.LinkProductSettingToConfiguration @ConfigurationId, @ProductSettingId, NULL, NULL
END;

EXEC sys.sp_updateextendedproperty @name = N'Build', @value = '59';