--User Story 1688264: Enhance HOTS process to create a cloned company if the product have Facilities product enabled.

Go

DECLARE @ProductsettingTypeid int;

IF NOT EXISTS (SELECT TOP 1 1 FROM Enterprise.ProductSettingType WHERE [Name] = 'HOTSCheckUserExcludeProductIds')
BEGIN
	INSERT INTO Enterprise.ProductSettingType ([Name], [Description], SensitiveData)
	VALUES ('HOTSCheckUserExcludeProductIds', 'List of products to Exclude in HOTS user cloning.', 0);
	SELECT @ProductsettingTypeid = ProductSettingTypeId FROM Enterprise.ProductSettingType WHERE [Name] = 'HOTSCheckUserExcludeProductIds'
    exec [Enterprise].[SetProductSetting] 0,3,@ProductsettingTypeid,''
END
GO