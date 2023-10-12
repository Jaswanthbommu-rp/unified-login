--Feature Story 1534201
IF NOT EXISTS (SELECT TOP 1 1 FROM Enterprise.ProductSettingType WHERE [Name] = 'UserAccessReport_MaxDOP')
BEGIN
	INSERT INTO Enterprise.ProductSettingType ([Name], [Description], SensitiveData)
	VALUES ('UserAccessReport_MaxDOP', 'Max DOP Setting for Users Access Review Audit Reporting', 0);
END
DECLARE @ProductId INT = 3, @ProductsettingTypeid int;
IF NOT EXISTS (SELECT TOP (1) (1) FROM ENTERPRISE.ProductSetting PS INNER JOIN enterprise.ProductSettingType pst ON pst.ProductSettingTypeId = PS.ProductSettingTypeId 
    WHERE PS.ProductId = @ProductId AND pst.Name = 'UserAccessReport_MaxDOP' )
BEGIN
    SELECT @ProductsettingTypeid = ProductSettingTypeId FROM Enterprise.ProductSettingType WHERE [Name] = 'UserAccessReport_MaxDOP'
    exec [Enterprise].[SetProductSetting] 0,@Productid,@ProductsettingTypeid, 12
END

GO