GO
IF NOT EXISTS(SELECT 1 FROM Enterprise.ProductSettingType WHERE Name = 'MigrationUri')
BEGIN
	INSERT INTO Enterprise.ProductSettingType(Name,Description,SensitiveData)
	VALUES(N'MigrationUri',N'Migration Domain Name',DEFAULT)
END

IF NOT EXISTS(SELECT 1 FROM Enterprise.ProductSettingType WHERE Name = 'MigrationUri')
BEGIN
	INSERT INTO Enterprise.ProductSettingType(Name,Description,SensitiveData)
	VALUES(N'IgnoreProductsForMigrationTool',N'List of product id that do not support migration',DEFAULT)
END

GO