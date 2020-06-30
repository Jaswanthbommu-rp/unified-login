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

if not exists ( select top 1 1 from Enterprise.ProductSettingType where name = 'BooksUseDomains' )
begin
	INSERT INTO Enterprise.ProductSettingType ( name, Description, SensitiveData ) values ( 'BooksUseDomains', 'Use domains for books api calls', 0 )
end
GO

if not exists ( select top 1 1 from Enterprise.ProductSettingType where name = 'BooksUseUPFMId' )
begin
	INSERT INTO Enterprise.ProductSettingType ( name, Description, SensitiveData ) values ( 'BooksUseUPFMId', 'Use UPFM instance for books api calls', 0 )
end

GO
