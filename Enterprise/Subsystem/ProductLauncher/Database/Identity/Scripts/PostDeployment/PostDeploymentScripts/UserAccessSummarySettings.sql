Go
-- UserAccessSummary and Report settings
if NOT EXISTS ( SELECT TOP 1 1 FROM Enterprise.ProductSettingType WHERE name = 'UserReport_MaxDOP' )
BEGIN
	INSERT INTO enterprise.ProductSettingType ( name, Description, SensitiveData ) VALUES ( 'UserReport_MaxDOP', 'The MaxDegreeOfParallelism to use when getting the user product roles and properties for the user report.', 0)
END
ELSE
BEGIN
	UPDATE Enterprise.ProductSettingType SET Description = 'The MaxDegreeOfParallelism to use when getting the user product roles and properties for the user report.' WHERE Name = 'UserReport_MaxDOP'
END

IF NOT EXISTS ( SELECT TOP 1 1 FROM Enterprise.ProductSettingType WHERE name = 'UserReport_ProductApiCacheDuration' )
BEGIN
	INSERT INTO enterprise.ProductSettingType ( name, Description, SensitiveData ) VALUES ( 'UserReport_ProductApiCacheDuration', 'The number of minutes to cache a users product information for the user report. Should be greater than 0.', 0)
END
ELSE
BEGIN
	UPDATE Enterprise.ProductSettingType SET Description = 'The number of minutes to cache a users product information for the user report. Should be greater than 0.' WHERE Name = 'UserReport_ProductApiCacheDuration'
END

IF NOT EXISTS ( SELECT TOP 1 1 FROM Enterprise.ProductSettingType WHERE name = 'UserReport_RetryUserReportCount' )
BEGIN
	INSERT INTO enterprise.ProductSettingType ( name, Description, SensitiveData ) VALUES ( 'UserReport_RetryUserReportCount', 'The number of times to retry the product api when getting a users properties or roles for the user report.', 0)
END
ELSE
BEGIN
	UPDATE Enterprise.ProductSettingType SET Description = 'The number of times to retry the product api when getting a users properties or roles for the user report.' WHERE Name = 'UserReport_RetryUserReportCount'
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
	INSERT INTO enterprise.ProductSettingType ( name, Description, SensitiveData ) VALUES ( 'UserReport_ExcludeProductIdList', 'The list of product ids to exclude when retrieving a users list of products when running a user report.', 0)
END
ELSE
BEGIN
	UPDATE Enterprise.ProductSettingType SET Description = 'The list of product ids to exclude when retrieving a users list of products when running a user report.' WHERE Name = 'UserReport_ExcludeProductIdList'
END

------------------------------------------------------------
if NOT EXISTS ( SELECT TOP 1 1 FROM Enterprise.ProductSettingType WHERE name = 'UserAccessDetails_MaxDOP' )
BEGIN
	INSERT INTO enterprise.ProductSettingType ( name, Description, SensitiveData ) VALUES ( 'UserAccessDetails_MaxDOP', 'The MaxDegreeOfParallelism to use when getting the user product roles and properties for the user access details.', 0)
END
ELSE
BEGIN
	UPDATE Enterprise.ProductSettingType SET Description = 'The MaxDegreeOfParallelism to use when getting the user product roles and properties for the user access details.' WHERE Name = 'UserAccessDetails_MaxDOP'
END

IF NOT EXISTS ( SELECT TOP 1 1 FROM Enterprise.ProductSettingType WHERE name = 'UserAccessDetails_ProductApiCacheDuration' )
BEGIN
	INSERT INTO enterprise.ProductSettingType ( name, Description, SensitiveData ) VALUES ( 'UserAccessDetails_ProductApiCacheDuration', 'The number of minutes to cache a users product information for the user access details. Should be greater than 0.', 0)
END
ELSE
BEGIN
	UPDATE Enterprise.ProductSettingType SET Description = 'The number of minutes to cache a users product information for the user access details. Should be greater than 0.' WHERE Name = 'UserAccessDetails_ProductApiCacheDuration'
END

IF NOT EXISTS ( SELECT TOP 1 1 FROM Enterprise.ProductSettingType WHERE name = 'UserAccessDetails_RetryUserReportCount' )
BEGIN
	INSERT INTO enterprise.ProductSettingType ( name, Description, SensitiveData ) VALUES ( 'UserAccessDetails_RetryUserReportCount', 'The number of times to retry the product api when getting a users properties or roles for the user access details.', 0)
END
ELSE
BEGIN
	UPDATE Enterprise.ProductSettingType SET Description = 'The number of times to retry the product api when getting a users properties or roles for the user access details.' WHERE Name = 'UserAccessDetails_RetryUserReportCount'
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
	INSERT INTO enterprise.ProductSettingType ( name, Description, SensitiveData ) VALUES ( 'UserAccessDetails_ExcludeProductIdList', 'The list of product ids to exclude when retrieving a users list of products when running a user access details.', 0)
END
ELSE
BEGIN
	UPDATE Enterprise.ProductSettingType SET Description = 'The list of product ids to exclude when retrieving a users list of products when running a user access details.' WHERE Name = 'UserAccessDetails_ExcludeProductIdList'
END

-- UserAccessSummary and Report settings
GO
