--Bug 2251226: Internal - Employee Users unable to Login as Myself
DECLARE @ProductsettingTypeid int;

IF NOT EXISTS (SELECT * FROM Enterprise.ProductSettingType WHERE [Name] = 'IsActivityCheckNotRequired')
BEGIN
	INSERT INTO Enterprise.ProductSettingType ([Name], [Description], SensitiveData)
	VALUES ('IsActivityCheckNotRequired', 'Produtct activity logic check is required', 0);
	SELECT @ProductsettingTypeid = ProductSettingTypeId FROM Enterprise.ProductSettingType WHERE [Name] = 'IsActivityCheckNotRequired'
    exec [Enterprise].[SetProductSetting] 0,1,@ProductsettingTypeid,''
END
GO