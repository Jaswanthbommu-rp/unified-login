 

DECLARE @ProductsettingTypeid int;
 
IF NOT EXISTS (SELECT * FROM Enterprise.ProductSettingType WHERE [Name] = 'MFASetupReminderDays')
BEGIN
	INSERT INTO Enterprise.ProductSettingType ([Name], [Description], SensitiveData)
	VALUES ('MFASetupReminderDays', 'Number of days to remind users to set up MFA.', 0);
	SELECT @ProductsettingTypeid = ProductSettingTypeId FROM Enterprise.ProductSettingType WHERE [Name] = 'MFASetupReminderDays'
    exec [Enterprise].[SetProductSetting] 0,3,@ProductsettingTypeid,'1'
END
GO