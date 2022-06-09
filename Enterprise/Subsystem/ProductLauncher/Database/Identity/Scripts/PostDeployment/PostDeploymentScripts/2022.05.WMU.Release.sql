--User Story 1072599: DB/API: AD Group Implementation for products with a UPFM integration type
IF NOT EXISTS(SELECT * FROM Enterprise.ProductSettingType WHERE Name = 'UPFMProductsHasProperties')
BEGIN
	INSERT INTO Enterprise.ProductSettingType([Name], [Description],SensitiveData)
	VALUES('UPFMProductsHasProperties','Can UPFM Products can have properties. Eg: For HOTS values is 0',0)
END

GO

IF NOT EXISTS (SELECT TOP 1 1 FROM [Enterprise].ProductSettingType WHERE [NAME] = 'GetUserGroupEndpoint')
BEGIN
INSERT INTO [Enterprise].ProductSettingType ([Name], [Description],SensitiveData) 
VALUES ('GetUserGroupEndpoint','GET user groups end point for product API',0)
END

GO

IF NOT EXISTS (SELECT TOP 1 1 FROM Enterprise.ProductSettingType WHERE [Name] = 'AOSpecialEditorUser')
BEGIN
	DECLARE @settingTypeId INT = 0;
	
	INSERT INTO Enterprise.ProductSettingType ([Name], [Description], SensitiveData)
	VALUES ('AOSpecialEditorUser', 'Automate RealPage Access user creation', 0);

	SELECT @settingTypeId = ProductSettingTypeId FROM Enterprise.ProductSettingType WHERE NAME = 'AOSpecialEditorUser'
	EXEC Enterprise.SetProductSetting 0, 4, @settingTypeId, N'ulserviceuser@realpage.com'
END

GO 

IF NOT EXISTS (SELECT TOP 1 1 FROM Enterprise.ProductSettingType WHERE [Name] = 'IsAnswerAutomation')
BEGIN
	DECLARE @settingTypeId INT = 0;
	
	INSERT INTO Enterprise.ProductSettingType ([Name], [Description], SensitiveData)
	VALUES ('IsAnswerAutomation', 'Answer Automation check flag is required to verify whether product is Answer automation', 0);

	SELECT @settingTypeId = ProductSettingTypeId FROM Enterprise.ProductSettingType WHERE NAME = 'IsAnswerAutomation'
	EXEC Enterprise.SetProductSetting 0, 78, @settingTypeId, N'0'
END

GO