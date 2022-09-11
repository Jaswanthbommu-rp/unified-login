-- For User Story User Story 1184649: UL: Product Integration - G5/LL Marketing Products
IF NOT EXISTS (SELECT TOP 1 1 FROM Enterprise.ProductSettingType WHERE [Name] = 'IterateUserNameRequiredForUserCreation')
BEGIN
	INSERT INTO Enterprise.ProductSettingType ([Name], [Description], SensitiveData)
	VALUES ('IterateUserNameRequiredForUserCreation', 'it will iterate username when it finds same in product.', 0);
END
-- Enabling 
GO
DECLARE @ProductId INT = 86, @Now DATETIME = GETUTCDATE(), @ProductsettingTypeid int;
IF NOT EXISTS (SELECT TOP 1 (1) FROM ENTERPRISE.ProductSetting PS INNER JOIN enterprise.ProductSettingType pst ON pst.ProductSettingTypeId = PS.ProductSettingTypeId 
    WHERE productid = @ProductId AND pst.Name = 'IterateUserNameRequiredForUserCreation' )
BEGIN
    SELECT @ProductsettingTypeid = ProductSettingTypeId FROM Enterprise.ProductSettingType WHERE [Name] = 'IterateUserNameRequiredForUserCreation'
    exec [Enterprise].[SetProductSetting] 0,@Productid,@ProductsettingTypeid,1
END
GO