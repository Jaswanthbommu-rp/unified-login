--User Story 2563145: Product Integration - Admin & Support Portal (Transition to Standard Integration) - DEV Only

DECLARE @ProductsettingTypeid int;
 
IF NOT EXISTS (SELECT * FROM Enterprise.ProductSettingType WHERE [Name] = 'SuperUserRoleType')
BEGIN
    INSERT INTO Enterprise.ProductSettingType ([Name], [Description], SensitiveData)
    VALUES ('SuperUserRoleType', 'Super User Role Type', 0);
END
GO