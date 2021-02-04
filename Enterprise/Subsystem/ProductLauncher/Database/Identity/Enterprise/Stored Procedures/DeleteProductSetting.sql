CREATE PROCEDURE [Enterprise].[DeleteProductSetting] @productSettingId INT
As
BEGIN
	DELETE FROM Enterprise.ProductSetting
	WHERE ProductSettingId = @productSettingId
END