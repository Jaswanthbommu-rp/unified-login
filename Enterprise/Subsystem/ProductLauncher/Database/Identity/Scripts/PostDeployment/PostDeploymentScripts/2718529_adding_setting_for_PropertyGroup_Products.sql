 
IF NOT EXISTS (SELECT * FROM Enterprise.ProductSettingType WHERE [Name] = 'PropertyGroupAvailableProducts')
BEGIN
	INSERT INTO Enterprise.ProductSettingType ([Name], [Description], SensitiveData)
	VALUES ('PropertyGroupAvailableProducts', 'Products that have property groups and primary properties available.', 0);
	DECLARE @ProductsettingTypeid int;
	SELECT @ProductsettingTypeid = ProductSettingTypeId FROM Enterprise.ProductSettingType WHERE [Name] = 'PropertyGroupAvailableProducts'
    exec [Enterprise].[SetProductSetting] 0,3,@ProductsettingTypeid,'8,18,16,51,52,53,54,30,32,66,29,33,95,31,34,103'
END
GO
