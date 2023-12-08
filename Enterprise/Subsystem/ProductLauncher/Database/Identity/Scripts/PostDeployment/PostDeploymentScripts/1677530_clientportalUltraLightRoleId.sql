--User Story 1677530: PME-353909: Add new Client Portal Ultra Light role to the Admin& Support Portal Product Access panel in all environments
IF NOT EXISTS (SELECT TOP 1 1 FROM Enterprise.ProductSettingType WHERE [Name] = 'ClientPortalUltraLightRoleId')
BEGIN
	INSERT INTO Enterprise.ProductSettingType ([Name], [Description], SensitiveData)
	VALUES ('ClientPortalUltraLightRoleId', 'Admin and Supportal Client Portal Ultra Light Role Id', 0) 
END
