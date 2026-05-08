GO
IF NOT EXISTS (SELECT * FROM Enterprise.ProductSettingType WHERE [Name] = 'IsRequireSamlCreateOnUpdateUser')
BEGIN
	INSERT INTO Enterprise.ProductSettingType ([Name], [Description], SensitiveData)
	VALUES ('IsRequireSamlCreateOnUpdateUser', 'Products requires saml add on updating user.', 0);
	DECLARE @ProductsettingTypeid int;
	SELECT @ProductsettingTypeid = ProductSettingTypeId FROM Enterprise.ProductSettingType WHERE [Name] = 'IsRequireSamlCreateOnUpdateUser'
    exec [Enterprise].[SetProductSetting] 0,105,@ProductsettingTypeid,'1'
END
GO
