GO
IF NOT EXISTS (SELECT TOP 1 1 FROM Enterprise.ProductSettingType WHERE [Name] = 'BatchProcessorRetryThread')
BEGIN
	INSERT INTO Enterprise.ProductSettingType ([Name], [Description], SensitiveData)
	VALUES ('BatchProcessorRetryThread', 'Batch Processor Retry thread count. Integer', 0);
END

GO
IF NOT EXISTS (SELECT TOP 1 1 FROM Enterprise.ProductSettingType WHERE [Name] = 'BatchProcessorPendingThread')
BEGIN
	INSERT INTO Enterprise.ProductSettingType ([Name], [Description], SensitiveData)
	VALUES ('BatchProcessorPendingThread', 'Batch Processor Pending thread count. Integer', 0);
END

GO
IF NOT EXISTS (SELECT TOP 1 1 FROM Enterprise.ProductSettingType WHERE [Name] = 'BatchProcessorEnterpriseRoleThread')
BEGIN
	INSERT INTO Enterprise.ProductSettingType ([Name], [Description], SensitiveData)
	VALUES ('BatchProcessorEnterpriseRoleThread', 'Batch Processor Enterprise Role thread count. Integer', 0);
END

GO
IF NOT EXISTS (SELECT TOP 1 1 FROM Enterprise.ProductSettingType WHERE [Name] = 'BatchProcessorPrimaryPropertiesThread')
BEGIN
	INSERT INTO Enterprise.ProductSettingType ([Name], [Description], SensitiveData)
	VALUES ('BatchProcessorPrimaryPropertiesThread', 'Batch Processor Primary Properties thread count. Integer', 0);
END

GO

DECLARE @ProductId INT = 3, @Now DATETIME = GETUTCDATE(), @ProductsettingTypeid int;
IF NOT EXISTS (SELECT TOP 1 (1) FROM ENTERPRISE.ProductSetting PS INNER JOIN enterprise.ProductSettingType pst ON pst.ProductSettingTypeId = PS.ProductSettingTypeId 
	WHERE productid = @ProductId AND pst.Name = 'BatchProcessorRetryThread' )
BEGIN
	SELECT @ProductsettingTypeid = ProductSettingTypeId FROM Enterprise.ProductSettingType WHERE [Name] = 'BatchProcessorRetryThread'
	exec [Enterprise].[SetProductSetting] 0,@Productid,@ProductsettingTypeid,10
END

IF NOT EXISTS (SELECT TOP 1 (1) FROM ENTERPRISE.ProductSetting PS INNER JOIN enterprise.ProductSettingType pst ON pst.ProductSettingTypeId = PS.ProductSettingTypeId 
	WHERE productid = @ProductId AND pst.Name = 'BatchProcessorPendingThread' )
BEGIN
	SELECT @ProductsettingTypeid = ProductSettingTypeId FROM Enterprise.ProductSettingType WHERE [Name] = 'BatchProcessorPendingThread'
	exec [Enterprise].[SetProductSetting] 0,@Productid,@ProductsettingTypeid,10
END

IF NOT EXISTS (SELECT TOP 1 (1) FROM ENTERPRISE.ProductSetting PS INNER JOIN enterprise.ProductSettingType pst ON pst.ProductSettingTypeId = PS.ProductSettingTypeId 
	WHERE productid = @ProductId AND pst.Name = 'BatchProcessorEnterpriseRoleThread' )
BEGIN
	SELECT @ProductsettingTypeid = ProductSettingTypeId FROM Enterprise.ProductSettingType WHERE [Name] = 'BatchProcessorEnterpriseRoleThread'
	exec [Enterprise].[SetProductSetting] 0,@Productid,@ProductsettingTypeid,5
END

IF NOT EXISTS (SELECT TOP 1 (1) FROM ENTERPRISE.ProductSetting PS INNER JOIN enterprise.ProductSettingType pst ON pst.ProductSettingTypeId = PS.ProductSettingTypeId 
	WHERE productid = @ProductId AND pst.Name = 'BatchProcessorPrimaryPropertiesThread' )
BEGIN
	SELECT @ProductsettingTypeid = ProductSettingTypeId FROM Enterprise.ProductSettingType WHERE [Name] = 'BatchProcessorPrimaryPropertiesThread'
	exec [Enterprise].[SetProductSetting] 0,@Productid,@ProductsettingTypeid,5
END
