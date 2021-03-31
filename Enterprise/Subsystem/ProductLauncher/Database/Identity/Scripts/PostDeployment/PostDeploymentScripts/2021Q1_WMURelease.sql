--Updating GetUserEndpoint for ILMLA and ILMLA products
DECLARE @ProductSettingTypeId INT
SELECT @ProductSettingTypeId = ProductSettingTypeId FROM Enterprise.ProductSettingType WHERE Name='GetUserEndpoint'
IF EXISTS(SELECT 1 FROM Enterprise.ProductSetting WHERE ProductId IN (40, 41) AND ProductSettingTypeId = @ProductSettingTypeId)
BEGIN
UPDATE Enterprise.ProductSetting SET value = '/{0}/users?loginName={1}'
WHERE ProductId IN (40, 41) AND ProductSettingTypeId = @ProductSettingTypeId
END
GO