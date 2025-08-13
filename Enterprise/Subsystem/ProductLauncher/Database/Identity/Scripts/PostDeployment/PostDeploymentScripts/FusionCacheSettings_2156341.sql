

-- FusionCacheEnabledCoreApiEnterprise
IF NOT EXISTS (SELECT 1 FROM enterprise.ProductSettingType WHERE Name = N'FusionCacheEnabledCoreApiEnterprise')
BEGIN
    INSERT INTO enterprise.ProductSettingType(Name, Description, SensitiveData)
    VALUES (N'FusionCacheEnabledCoreApiEnterprise', 'Enable FusionCache for Core API in Enterprise context', 0)
END
 
-- FusionCacheDurationCoreApiEnterprise
IF NOT EXISTS (SELECT 1 FROM enterprise.ProductSettingType WHERE Name = N'FusionCacheDurationCoreApiEnterprise')
BEGIN
    INSERT INTO enterprise.ProductSettingType(Name, Description, SensitiveData)
    VALUES (N'FusionCacheDurationCoreApiEnterprise', 'Duration for FusionCache in Core API (Enterprise)', 0)
END
 
-- FusionCacheWithRedisEnabledCoreApiEnterprise
IF NOT EXISTS (SELECT 1 FROM enterprise.ProductSettingType WHERE Name = N'FusionCacheWithRedisEnabledCoreApiEnterprise')
BEGIN
    INSERT INTO enterprise.ProductSettingType(Name, Description, SensitiveData)
    VALUES (N'FusionCacheWithRedisEnabledCoreApiEnterprise', 'Enable Redis integration for FusionCache in Core API (Enterprise)', 0)
END
 
-- FusionCacheEnabledCoreApi
IF NOT EXISTS (SELECT 1 FROM enterprise.ProductSettingType WHERE Name = N'FusionCacheEnabledCoreApi')
BEGIN
    INSERT INTO enterprise.ProductSettingType(Name, Description, SensitiveData)
    VALUES (N'FusionCacheEnabledCoreApi', 'Enable FusionCache for Core API', 0)
END
 
-- FusionCacheDurationCoreApi
IF NOT EXISTS (SELECT 1 FROM enterprise.ProductSettingType WHERE Name = N'FusionCacheDurationCoreApi')
BEGIN
    INSERT INTO enterprise.ProductSettingType(Name, Description, SensitiveData)
    VALUES (N'FusionCacheDurationCoreApi', 'Duration for FusionCache in Core API', 0)
END
 
-- FusionCacheWithRedisEnabledCoreApi
IF NOT EXISTS (SELECT 1 FROM enterprise.ProductSettingType WHERE Name = N'FusionCacheWithRedisEnabledCoreApi')
BEGIN
    INSERT INTO enterprise.ProductSettingType(Name, Description, SensitiveData)
    VALUES (N'FusionCacheWithRedisEnabledCoreApi', 'Enable Redis integration for FusionCache in Core API', 0)
END