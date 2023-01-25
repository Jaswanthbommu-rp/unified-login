Go
-- UserAccessSummary and Report settings
if NOT EXISTS ( SELECT TOP 1 1 FROM Enterprise.ProductSettingType WHERE name = 'UserReport_MaxDOP' )
BEGIN
	INSERT INTO enterprise.ProductSettingType ( name, Description, SensitiveData ) VALUES ( 'UserReport_MaxDOP', 'The max threads to use when calling apis for the user report.', 0)
END
ELSE
BEGIN
	UPDATE Enterprise.ProductSettingType SET Description = 'The max threads to use when calling apis for the user report.' WHERE Name = 'UserReport_MaxDOP'
END

IF NOT EXISTS ( SELECT TOP 1 1 FROM Enterprise.ProductSettingType WHERE name = 'UserReport_ProductApiCacheDuration' )
BEGIN
	INSERT INTO enterprise.ProductSettingType ( name, Description, SensitiveData ) VALUES ( 'UserReport_ProductApiCacheDuration', 'The number of minutes to cache product information for the user report. Should be greater than 0.', 0)
END
ELSE
BEGIN
	UPDATE Enterprise.ProductSettingType SET Description = 'The number of minutes to cache product information for the user report. Should be greater than 0.' WHERE Name = 'UserReport_ProductApiCacheDuration'
END

IF NOT EXISTS ( SELECT TOP 1 1 FROM Enterprise.ProductSettingType WHERE name = 'UserReport_RetryUserReportCount' )
BEGIN
	INSERT INTO enterprise.ProductSettingType ( name, Description, SensitiveData ) VALUES ( 'UserReport_RetryUserReportCount', 'The number of times to retry the product api for the user report.', 0)
END
ELSE
BEGIN
	UPDATE Enterprise.ProductSettingType SET Description = 'The number of times to retry the product api for the user report.' WHERE Name = 'UserReport_RetryUserReportCount'
END

IF NOT EXISTS ( SELECT TOP 1 1 FROM Enterprise.ProductSettingType WHERE name = 'UserReport_MaxRunningTime' )
BEGIN
	INSERT INTO enterprise.ProductSettingType ( name, Description, SensitiveData ) VALUES ( 'UserReport_MaxRunningTime', 'The maximum amount of time to wait when generating the user report.', 0)
END
ELSE
BEGIN
	UPDATE Enterprise.ProductSettingType SET Description = 'The maximum amount of time to wait when generating the user report.' WHERE Name = 'UserReport_MaxRunningTime'
END

IF NOT EXISTS ( SELECT TOP 1 1 FROM Enterprise.ProductSettingType WHERE name = 'UserReport_ExcludeProductIdList' )
BEGIN
	INSERT INTO enterprise.ProductSettingType ( name, Description, SensitiveData ) VALUES ( 'UserReport_ExcludeProductIdList', 'Product ids to exclude when running a user report.', 0)
END
ELSE
BEGIN
	UPDATE Enterprise.ProductSettingType SET Description = 'Product ids to exclude when running a user report.' WHERE Name = 'UserReport_ExcludeProductIdList'
END

------------------------------------------------------------
if NOT EXISTS ( SELECT TOP 1 1 FROM Enterprise.ProductSettingType WHERE name = 'UserAccessDetails_MaxDOP' )
BEGIN
	INSERT INTO enterprise.ProductSettingType ( name, Description, SensitiveData ) VALUES ( 'UserAccessDetails_MaxDOP', 'The max threads to use when calling apis for the user access details.', 0)
END
ELSE
BEGIN
	UPDATE Enterprise.ProductSettingType SET Description = 'The max threads to use when calling apis for the user access details.' WHERE Name = 'UserAccessDetails_MaxDOP'
END

IF NOT EXISTS ( SELECT TOP 1 1 FROM Enterprise.ProductSettingType WHERE name = 'UserAccessDetails_ProductApiCacheDuration' )
BEGIN
	INSERT INTO enterprise.ProductSettingType ( name, Description, SensitiveData ) VALUES ( 'UserAccessDetails_ProductApiCacheDuration', 'The number of minutes to cache product info for the user access details. Should be greater than 0.', 0)
END
ELSE
BEGIN
	UPDATE Enterprise.ProductSettingType SET Description = 'The number of minutes to cache product info for the user access details. Should be greater than 0.' WHERE Name = 'UserAccessDetails_ProductApiCacheDuration'
END

IF NOT EXISTS ( SELECT TOP 1 1 FROM Enterprise.ProductSettingType WHERE name = 'UserAccessDetails_RetryUserReportCount' )
BEGIN
	INSERT INTO enterprise.ProductSettingType ( name, Description, SensitiveData ) VALUES ( 'UserAccessDetails_RetryUserReportCount', 'The number of times to retry the product api for the user access details.', 0)
END
ELSE
BEGIN
	UPDATE Enterprise.ProductSettingType SET Description = 'The number of times to retry the product api for the user access details.' WHERE Name = 'UserAccessDetails_RetryUserReportCount'
END

IF NOT EXISTS ( SELECT TOP 1 1 FROM Enterprise.ProductSettingType WHERE name = 'UserAccessDetails_MaxRunningTime' )
BEGIN
	INSERT INTO enterprise.ProductSettingType ( name, Description, SensitiveData ) VALUES ( 'UserAccessDetails_MaxRunningTime', 'The maximum amount of time to wait when generating the user access details.', 0)
END
ELSE
BEGIN
	UPDATE Enterprise.ProductSettingType SET Description = 'The maximum amount of time to wait when generating the user access details.' WHERE Name = 'UserAccessDetails_MaxRunningTime'
END

IF NOT EXISTS ( SELECT TOP 1 1 FROM Enterprise.ProductSettingType WHERE name = 'UserAccessDetails_ExcludeProductIdList' )
BEGIN
	INSERT INTO enterprise.ProductSettingType ( name, Description, SensitiveData ) VALUES ( 'UserAccessDetails_ExcludeProductIdList', 'Product ids to exclude when running user access details.', 0)
END
ELSE
BEGIN
	UPDATE Enterprise.ProductSettingType SET Description = 'Product ids to exclude when running user access details.' WHERE Name = 'UserAccessDetails_ExcludeProductIdList'
END

-- UserAccessSummary and Report settings
GO
