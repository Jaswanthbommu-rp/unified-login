--User Story 2625610: Update Create User logic for Maintenance Contact Center/Answer Automation


DECLARE @ProductsettingTypeid int;

IF NOT EXISTS (SELECT * FROM Enterprise.ProductSettingType WHERE [Name] = 'CallUpdateWhenCreateReturnsUserExists')
BEGIN
	INSERT INTO Enterprise.ProductSettingType ([Name], [Description], SensitiveData)
	VALUES ('CallUpdateWhenCreateReturnsUserExists', 'Flag to call the update API if the create API returns a user already exists', 0);
	SELECT @ProductsettingTypeid = ProductSettingTypeId FROM Enterprise.ProductSettingType WHERE [Name] = 'CallUpdateWhenCreateReturnsUserExists'
    exec [Enterprise].[SetProductSetting] 0,78,@ProductsettingTypeid,'1'
END
GO