--User Story 1550800: Propagate Access Rights that are assigned to the RP Employee User into the Customer Company when Logging in as RealPage Access User
Go
DECLARE @ProductsettingTypeid int;
IF NOT EXISTS (SELECT TOP 1 1 FROM Enterprise.ProductSettingType WHERE [Name] = 'IsUserManagementByADGroup')
BEGIN
	INSERT INTO Enterprise.ProductSettingType ([Name], [Description], SensitiveData)
	VALUES ('IsUserManagementByADGroup', 'ADGroups rights propagated in customer companies', 0);

	SELECT @ProductsettingTypeid = ProductSettingTypeId FROM Enterprise.ProductSettingType WHERE [Name] = 'IsUserManagementByADGroup'
    exec [Enterprise].[SetProductSetting] 0,3,@ProductsettingTypeid,'1'

END
Go