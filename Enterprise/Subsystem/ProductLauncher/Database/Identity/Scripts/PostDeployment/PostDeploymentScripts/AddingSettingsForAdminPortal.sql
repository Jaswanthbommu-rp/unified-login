-- User Story 1276811: UL: Admin & Support Portal - Dynamic change of tile name (dev only)
GO
DECLARE @ProductsettingTypeid int;
IF NOT EXISTS (SELECT TOP 1 1 FROM Enterprise.ProductSettingType WHERE [Name] = 'BatchUserProductStatusSleepTimeout')
BEGIN
	INSERT INTO Enterprise.ProductSettingType ([Name], [Description], SensitiveData)
	VALUES ('BatchUserProductStatusSleepTimeout', 'The amount of time in ms to wait checking a product batch process', 0);
	SELECT @ProductsettingTypeid = ProductSettingTypeId FROM Enterprise.ProductSettingType WHERE [Name] = 'BatchUserProductStatusSleepTimeout'
    exec [Enterprise].[SetProductSetting] 0,89,@ProductsettingTypeid,'5000'
END
IF NOT EXISTS (SELECT TOP 1 1 FROM Enterprise.ProductSettingType WHERE [Name] = 'BatchUserProductStatusRetryCount')
BEGIN
	INSERT INTO Enterprise.ProductSettingType ([Name], [Description], SensitiveData)
	VALUES ('BatchUserProductStatusRetryCount', 'The number of times to retry when checking if a product batch process is complete', 0);
	SELECT @ProductsettingTypeid = ProductSettingTypeId FROM Enterprise.ProductSettingType WHERE [Name] = 'BatchUserProductStatusRetryCount'
    exec [Enterprise].[SetProductSetting] 0,89,@ProductsettingTypeid,'5'
END
GO