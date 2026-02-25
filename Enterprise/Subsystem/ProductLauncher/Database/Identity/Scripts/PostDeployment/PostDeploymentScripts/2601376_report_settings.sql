--User Story 2601376: Unified Platform reports failed to generate to change status to Error 
DECLARE @ProductsettingTypeid int;
 
IF NOT EXISTS (SELECT * FROM Enterprise.ProductSettingType WHERE [Name] = 'UserReport_Batchsize')
BEGIN
    INSERT INTO Enterprise.ProductSettingType ([Name], [Description], SensitiveData)
    VALUES ('UserReport_Batchsize', 'Specifies the number records to be processed on each batch.', 0);
    SELECT @ProductsettingTypeid = ProductSettingTypeId FROM Enterprise.ProductSettingType WHERE [Name] = 'UserReport_Batchsize'
    exec [Enterprise].[SetProductSetting] 0,3,@ProductsettingTypeid,'300'
END

IF NOT EXISTS (SELECT * FROM Enterprise.ProductSettingType WHERE [Name] = 'UserReport_PerBatchDelaySeconds')
BEGIN
    INSERT INTO Enterprise.ProductSettingType ([Name], [Description], SensitiveData)
    VALUES ('UserReport_PerBatchDelaySeconds', 'Specifies the seconds the next batch has to hold.', 0);
    SELECT @ProductsettingTypeid = ProductSettingTypeId FROM Enterprise.ProductSettingType WHERE [Name] = 'UserReport_Batchsize'
    exec [Enterprise].[SetProductSetting] 0,3,@ProductsettingTypeid,'30'
END
GO