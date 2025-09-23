--User Story 2541436: Add new Product Setting called "PlatformAdminRole"


DECLARE @ProductsettingTypeid int;

IF NOT EXISTS (SELECT * FROM Enterprise.ProductSettingType WHERE [Name] = 'PlatformAdminRole')
BEGIN
	INSERT INTO Enterprise.ProductSettingType ([Name], [Description], SensitiveData)
	VALUES ('PlatformAdminRole', 'Default role for system admin', 0);
	SELECT @ProductsettingTypeid = ProductSettingTypeId FROM Enterprise.ProductSettingType WHERE [Name] = 'PlatformAdminRole'
    exec [Enterprise].[SetProductSetting] 0,3,@ProductsettingTypeid,'Platform Administrator'
END
GO