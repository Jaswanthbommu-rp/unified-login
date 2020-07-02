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
-- sync up the employee and external company guids
update enterprise.party set realpageid = '0D018E46-C20E-477D-ADED-4E5A35FB8F99' where partyid = (select top 1 partyid from enterprise.DataImportMapping where sourceid = '-1')
update enterprise.party set realpageid = 'EEFACE50-9F75-4DCE-B133-A97EE0E0D723' where partyid = (select top 1 partyid from enterprise.DataImportMapping where sourceid = '-2')

GO
