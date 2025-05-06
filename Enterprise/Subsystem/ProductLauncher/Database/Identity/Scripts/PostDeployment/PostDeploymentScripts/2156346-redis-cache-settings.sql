IF NOT EXISTS (SELECT 1 FROM enterprise.ProductSettingType WHERE Name = N'FusionCacheEnabledDuende')
BEGIN
    INSERT INTO enterprise.ProductSettingType(Name, Description, SensitiveData)
    VALUES (N'FusionCacheEnabledDuende', 'Use FusionCache with IdentityServer', 0)
END

IF NOT EXISTS (SELECT 1 FROM enterprise.ProductSettingType WHERE Name = N'FusionCacheWithRedisEnabledDuende')
BEGIN
    INSERT INTO enterprise.ProductSettingType(Name, Description, SensitiveData)
    VALUES (N'FusionCacheWithRedisEnabledDuende', 'Use FusionCache with Redis with IdentityServer', 0)
END
IF NOT EXISTS (SELECT 1 FROM enterprise.ProductSettingType WHERE Name = N'CacheDisabledDuende')
BEGIN
    INSERT INTO enterprise.ProductSettingType(Name, Description, SensitiveData)
    VALUES (N'CacheDisabledDuende', 'Do not cache anything in memory with IdentityServer', 0)
END

IF NOT EXISTS (SELECT 1 FROM enterprise.ProductSettingType WHERE Name = N'FusionCacheDurationDuende')
BEGIN
    INSERT INTO enterprise.ProductSettingType(Name, Description, SensitiveData)
    VALUES (N'FusionCacheDurationDuende', 'How long the redis cache should persist in minutes with IdentityServer', 0)
END

IF NOT EXISTS (SELECT 1 FROM enterprise.ProductSettingType WHERE Name = N'CacheMaxMemoryMBDuende')
BEGIN
    INSERT INTO enterprise.ProductSettingType(Name, Description, SensitiveData)
    VALUES (N'CacheMaxMemoryMBDuende', 'The maximum amount of memory to use for caching with IdentityServer', 0)
END
