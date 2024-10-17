
IF NOT EXISTS (SELECT TOP (1) 1 FROM Enterprise.ProductSettingType WHERE [Name] = 'ShowInPrimaryPropertiesReport')
BEGIN
	INSERT INTO Enterprise.ProductSettingType ([Name], [Description], SensitiveData) 
	VALUES('ShowInPrimaryPropertiesReport', 'Based on this setting value only items will be populated in audit report dropdown list', 0)
END
IF NOT EXISTS (SELECT TOP (1) 1 FROM Enterprise.ProductSettingType WHERE [Name] = 'CacheInMinutesPrimaryPropertyReportData')
BEGIN
	INSERT INTO Enterprise.ProductSettingType ([Name], [Description], SensitiveData) 
	VALUES('CacheInMinutesPrimaryPropertyReportData', 'Caching the data from UDM property API call for specified time in minutes', 0)
END
GO

