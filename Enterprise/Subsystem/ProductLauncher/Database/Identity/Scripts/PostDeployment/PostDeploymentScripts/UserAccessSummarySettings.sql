Go
-- UserAccessSummary and Report settings
if NOT EXISTS ( SELECT TOP 1 1 FROM Enterprise.ProductSettingType WHERE name = 'UserReport_MaxDOP' )
BEGIN
	INSERT INTO enterprise.ProductSettingType ( name, Description, SensitiveData ) VALUES ( 'UserReport_MaxDOP', 'The MaxDegreeOfParallelism to use when getting the user product roles and properties.', 0)
END

IF NOT EXISTS ( SELECT TOP 1 1 FROM Enterprise.ProductSettingType WHERE name = 'UserReport_ProductApiCacheDuration' )
BEGIN
	INSERT INTO enterprise.ProductSettingType ( name, Description, SensitiveData ) VALUES ( 'UserReport_ProductApiCacheDuration', 'The number of minutes to cache a users product information. Should be greater than 0.', 0)
END

IF NOT EXISTS ( SELECT TOP 1 1 FROM Enterprise.ProductSettingType WHERE name = 'UserReport_RetryUserReportCount' )
BEGIN
	INSERT INTO enterprise.ProductSettingType ( name, Description, SensitiveData ) VALUES ( 'UserReport_RetryUserReportCount', 'The number of times to retry the product api when getting a users properties or roles.', 0)
END

IF NOT EXISTS ( SELECT TOP 1 1 FROM Enterprise.ProductSettingType WHERE name = 'UserReport_MaxRunningTime' )
BEGIN
	INSERT INTO enterprise.ProductSettingType ( name, Description, SensitiveData ) VALUES ( 'UserReport_MaxRunningTime', 'The maximum amount of time to wait when generating the user report.', 0)
END

IF NOT EXISTS ( SELECT TOP 1 1 FROM Enterprise.ProductSettingType WHERE name = 'UserSummaryReport_ExcludeProductIdList' )
BEGIN
	INSERT INTO enterprise.ProductSettingType ( name, Description, SensitiveData ) VALUES ( 'UserSummaryReport_ExcludeProductIdList', 'The list of product ids to exclude when retrieving a users list of products.', 0)
END

-- UserAccessSummary and Report settings
GO
