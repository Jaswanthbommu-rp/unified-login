IF NOT EXISTS (SELECT TOP 1 1 FROM Enterprise.ProductSettingType WHERE [Name] = 'SuperUserPropertiesId')
BEGIN
    DECLARE @ProductsettingTypeid int;
	INSERT INTO Enterprise.ProductSettingType ([Name], [Description], SensitiveData)
	VALUES ('SuperUserPropertiesId', 'The Property Id to create admin user in product', 0);
    SELECT @ProductsettingTypeid = ProductSettingTypeId FROM Enterprise.ProductSettingType WHERE [Name] = 'SuperUserPropertiesId'
    exec [Enterprise].[SetProductSetting] 0,78,@ProductsettingTypeid,'all'
END