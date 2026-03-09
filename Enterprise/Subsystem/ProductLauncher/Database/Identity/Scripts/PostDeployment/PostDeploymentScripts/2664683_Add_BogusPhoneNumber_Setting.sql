--User Story 2664683: Update User Deactivation Logic for Knock CRM Users Without Phone Numbers
GO
DECLARE @ProductsettingTypeid int;

IF NOT EXISTS (SELECT TOP 1 1 FROM Enterprise.ProductSettingType WHERE [Name] = 'BogusPhoneNumber')
BEGIN
	INSERT INTO Enterprise.ProductSettingType ([Name], [Description], SensitiveData)
	VALUES ('BogusPhoneNumber', 'Bogus Phone Number to products for mandatory products on deactivate', 0);
END

SELECT @ProductsettingTypeid = ProductSettingTypeId FROM Enterprise.ProductSettingType WHERE [Name] = 'BogusPhoneNumber'

IF NOT EXISTS (SELECT TOP 1 1 FROM Enterprise.ProductSetting WHERE ProductSettingTypeId = @ProductsettingTypeid AND ProductId = 91)
BEGIN
    EXEC [Enterprise].[SetProductSetting] 0, 91, @ProductsettingTypeid, '9999999999'
END
GO