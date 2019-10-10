--This data is already there in QA, SAT and PROD.. we dont need this as per James.

--Insert into [Auth].[ClientRedirectUris](ClientId,Uri)
--  Select 15,'https://umsat.realpage.com/pages/rplogin.aspx'

--Update [Ident].[SamlProductSettings] Set LoginUri = 'https://umsat.realpage.com/pages/rplogin.aspx'
--  Where ProductId = 18

EXEC sys.sp_updateextendedproperty @name=N'Build', @value='45'
