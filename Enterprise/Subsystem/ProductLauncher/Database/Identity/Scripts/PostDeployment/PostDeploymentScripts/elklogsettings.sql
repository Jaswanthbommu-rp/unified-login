-- elklogsettings
if NOT EXISTS ( SELECT TOP 1 1 FROM Enterprise.ProductSettingType WHERE name = 'Elk_LogManageBlueBook' )
BEGIN
	INSERT INTO enterprise.ProductSettingType ( name, Description, SensitiveData ) VALUES ( 'Elk_LogManageBlueBook', 'Log ManageBlueBook calls in Elk', 0)
END

IF NOT EXISTS ( SELECT TOP 1 1 FROM Enterprise.ProductSettingType WHERE name = 'Elk_LogManageUnifiedSettings' )
BEGIN
	INSERT INTO enterprise.ProductSettingType ( name, Description, SensitiveData ) VALUES ( 'Elk_LogManageUnifiedSettings', 'Log ManageUnifiedSettings calls in Elk', 0)
END

IF NOT EXISTS ( SELECT TOP 1 1 FROM Enterprise.ProductSettingType WHERE name = 'Elk_LogManageUserPropertiesSync' )
BEGIN
	INSERT INTO enterprise.ProductSettingType ( name, Description, SensitiveData ) VALUES ( 'Elk_LogManageUserPropertiesSync', 'Log ManageUserPropertiesSync calls in Elk', 1)
END

IF NOT EXISTS ( SELECT TOP 1 1 FROM Enterprise.ProductSettingType WHERE name = 'AdditionalValidationCertsThumbprint' )
BEGIN
	INSERT INTO enterprise.ProductSettingType ( name, Description, SensitiveData ) VALUES ( 'Elk_LogManageProductBase', 'Log ManageProductBase calls in Elk', 0)
END

GO

-- elklogsettings
