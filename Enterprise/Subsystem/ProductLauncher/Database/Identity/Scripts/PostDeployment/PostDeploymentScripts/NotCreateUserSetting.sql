-- For User Story 1097140: Product Integration: L&R Conversion Tile for Employee Company
IF NOT EXISTS (SELECT TOP 1 1 FROM Enterprise.ProductSettingType WHERE [Name] = 'IsCreateUserNotRequiredForProduct')
BEGIN
	INSERT INTO Enterprise.ProductSettingType ([Name], [Description], SensitiveData)
	VALUES ('IsCreateUserNotRequiredForProduct', 'it will check whether creation of user required or not for product.', 0);
END
-- Enabling 
GO
DECLARE @ProductId INT = 85, @Now DATETIME = GETUTCDATE(), @ProductsettingTypeid int;
IF NOT EXISTS (SELECT TOP 1 (1) FROM ENTERPRISE.ProductSetting PS INNER JOIN enterprise.ProductSettingType pst ON pst.ProductSettingTypeId = PS.ProductSettingTypeId 
    WHERE productid = @ProductId AND pst.Name = 'IsCreateUserNotRequiredForProduct' )
BEGIN
    SELECT @ProductsettingTypeid = ProductSettingTypeId FROM Enterprise.ProductSettingType WHERE [Name] = 'IsCreateUserNotRequiredForProduct'
    exec [Enterprise].[SetProductSetting] 0,@Productid,@ProductsettingTypeid,1
END
GO