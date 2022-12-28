

DECLARE @ProductId INT = 3, @Now DATETIME = GETUTCDATE(), @ProductsettingTypeid int;
IF NOT EXISTS (SELECT TOP 1 1 FROM Enterprise.ProductSettingType WHERE [Name] = 'ImpersonationRightsToBeExcluded')
BEGIN
	INSERT INTO Enterprise.ProductSettingType ([Name], [Description], SensitiveData)
	VALUES ('ImpersonationRightsToBeExcluded', 'Rights to be ignored when impersonating a user', 0);
END
-- Enabling 

IF NOT EXISTS (SELECT TOP 1 (1) FROM ENTERPRISE.ProductSetting PS INNER JOIN enterprise.ProductSettingType pst ON pst.ProductSettingTypeId = PS.ProductSettingTypeId 
    WHERE productid = @ProductId AND pst.Name = 'ImpersonationRightsToBeExcluded' )
BEGIN
    SELECT @ProductsettingTypeid = ProductSettingTypeId FROM Enterprise.ProductSettingType WHERE [Name] = 'ImpersonationRightsToBeExcluded'
    exec [Enterprise].[SetProductSetting] 0,@Productid,@ProductsettingTypeid,'InternalAdminaccessToUnifiedSettings,EmployeeAccesstoManageSettingsTemplates,AccessSettingsAdmin,EmployeeAccessUnifiedReportingAdminConsole'
END
GO