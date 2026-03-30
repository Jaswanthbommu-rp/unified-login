

DECLARE @ProductsettingTypeid int;
 
IF NOT EXISTS (SELECT * FROM Enterprise.ProductSettingType WHERE [Name] = 'PhoneNumberRequiredProducts')
BEGIN
	INSERT INTO Enterprise.ProductSettingType ([Name], [Description], SensitiveData)
	VALUES ('PhoneNumberRequiredProducts', 'Products that require a phone number to assign.', 0);
	SELECT @ProductsettingTypeid = ProductSettingTypeId FROM Enterprise.ProductSettingType WHERE [Name] = 'PhoneNumberRequiredProducts'
    exec [Enterprise].[SetProductSetting] 0,3,@ProductsettingTypeid,'78,91,105'
END
GO