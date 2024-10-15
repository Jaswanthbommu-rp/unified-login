

IF NOT EXISTS (SELECT TOP (1) 1 FROM Enterprise.ProductSettingType WHERE [Name] = 'ShowInPrimaryPropertiesReport')
BEGIN
	INSERT INTO Enterprise.ProductSettingType ([Name], [Description], SensitiveData) 
	VALUES('ShowInPrimaryPropertiesReport', 'It will display in primary property audit report dropdown list', 0)
END


IF NOT EXISTS (SELECT TOP (1) 1 FROM Enterprise.ProductSettingType WHERE [Name] = 'CacheInMinutesPrimaryPropertyReportData')
BEGIN
	INSERT INTO Enterprise.ProductSettingType ([Name], [Description], SensitiveData) 
	VALUES('CacheInMinutesPrimaryPropertyReportData', 'Caching the data from UDM translated primary property API call for specified time in minutes', 0)
END

GO

