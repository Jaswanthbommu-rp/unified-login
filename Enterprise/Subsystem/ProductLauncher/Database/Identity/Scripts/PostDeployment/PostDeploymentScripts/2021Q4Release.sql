-- Add GetAccessTypesEndpoint product setting

IF NOT EXISTS (SELECT TOP 1 1 FROM Enterprise.ProductSettingType WHERE [Name] = 'GetAccessTypesEndpoint')
BEGIN
	INSERT INTO Enterprise.ProductSettingType ([Name], [Description], SensitiveData)
	VALUES ('GetAccessTypesEndpoint', 'Access Type endpoint for product API', 0);
END

GO
