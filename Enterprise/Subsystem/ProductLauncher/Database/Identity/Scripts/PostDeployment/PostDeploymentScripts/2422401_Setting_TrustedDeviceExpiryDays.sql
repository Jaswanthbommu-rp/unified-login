--User Story 2422401: Add new Product Setting called "TrustedDeviceExpiryDays"
 
 
DECLARE @ProductsettingTypeid int;
 
IF NOT EXISTS (SELECT * FROM Enterprise.ProductSettingType WHERE [Name] = 'TrustedDeviceExpiryDays')
BEGIN
    INSERT INTO Enterprise.ProductSettingType ([Name], [Description], SensitiveData)
    VALUES ('TrustedDeviceExpiryDays', 'Specifies the number of days a device is remembered after successful MFA', 0);
    SELECT @ProductsettingTypeid = ProductSettingTypeId FROM Enterprise.ProductSettingType WHERE [Name] = 'TrustedDeviceExpiryDays'
    exec [Enterprise].[SetProductSetting] 0,3,@ProductsettingTypeid,''
END
GO