Go
DECLARE @ProductsettingTypeid int;
IF NOT EXISTS (SELECT TOP 1 1 FROM Enterprise.ProductSettingType WHERE [Name] = 'ClearProductDataList')
BEGIN
	INSERT INTO Enterprise.ProductSettingType ([Name], [Description], SensitiveData)
	VALUES ('ClearProductDataList', 'List of products to delete SAML', 0);
	SELECT @ProductsettingTypeid = ProductSettingTypeId FROM Enterprise.ProductSettingType WHERE [Name] = 'ClearProductDataList'
    exec [Enterprise].[SetProductSetting] 0,3,@ProductsettingTypeid,'36'
END
GO