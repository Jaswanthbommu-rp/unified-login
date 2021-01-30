CREATE PROCEDURE Enterprise.GetProductSettings (@productId INT)
AS
BEGIN
SELECT p.ProductSettingId as SettingId, p.Value as SettingValue,
	   pt.ProductSettingTypeId as SettingTypeId, pt.Name as SettingTypeName, 
	   pt.Description as SettingTypeDescription, pt.SensitiveData
FROM Enterprise.ProductSetting p
	JOIN Enterprise.ProductSettingType pt on pt.ProductSettingTypeId = p.ProductSettingTypeId
WHERE ProductId = @productId
END