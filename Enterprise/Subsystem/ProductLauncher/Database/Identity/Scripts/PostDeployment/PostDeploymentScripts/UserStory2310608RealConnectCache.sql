GO
IF NOT EXISTS (SELECT TOP (1) 1 FROM Enterprise.ProductSettingType WHERE [Name] = 'LearningPathRedisCacheInMinutes')
BEGIN
	INSERT INTO Enterprise.ProductSettingType ([Name], [Description], SensitiveData)
	VALUES ('LearningPathRedisCacheInMinutes', 'Cachig Learning path API call result content in specified minutes', 0);
END
IF NOT EXISTS (SELECT TOP (1) 1 FROM Enterprise.ProductSettingType WHERE [Name] = 'LicenseDetailsRedisCacheInMinutes')
BEGIN
	INSERT INTO Enterprise.ProductSettingType ([Name], [Description], SensitiveData)
	VALUES ('LicenseDetailsRedisCacheInMinutes', 'Cachig License Details API call result content in specified minutes', 0);
END
IF NOT EXISTS (SELECT TOP (1) 1 FROM Enterprise.ProductSettingType WHERE [Name] = 'IsLearningPathAPICallsEnabled')
BEGIN
	INSERT INTO Enterprise.ProductSettingType ([Name], [Description], SensitiveData)
	VALUES ('IsLearningPathAPICallsEnabled', 'When this flag is enabled API calls will occur to get learning path content', 0);
END
GO