-- August 2020 changes
GO

update enterprise.ProductSettingType set sensitivedata = 1 where name in (
'ApiSecret'
,'MTClientSecret'
,'UnifiedLoginResearchApplicationClientSecret'
,'TokenClientSecret'
,'TiboWebHookSigningSecret'
,'ApiPassword'
,'IntactPassword'

)
GO
