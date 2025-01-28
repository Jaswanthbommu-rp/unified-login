--User Story 2070448: Add new Product Setting called "AvailableOnlyForThisOrgType"


DECLARE @ProductsettingTypeid int;

IF NOT EXISTS (SELECT * FROM Enterprise.ProductSettingType WHERE [Name] = 'AvailableOnlyForThisOrgType')
BEGIN
	INSERT INTO Enterprise.ProductSettingType ([Name], [Description], SensitiveData)
	VALUES ('AvailableOnlyForThisOrgType', 'Product available for this organization types', 0);
	SELECT @ProductsettingTypeid = ProductSettingTypeId FROM Enterprise.ProductSettingType WHERE [Name] = 'AvailableOnlyForThisOrgType'
    exec [Enterprise].[SetProductSetting] 0,3,@ProductsettingTypeid,''
END
GO