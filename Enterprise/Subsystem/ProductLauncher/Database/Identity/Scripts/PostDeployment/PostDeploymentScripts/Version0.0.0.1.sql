
Print 'Adding new values to ProductSettingType'

SET IDENTITY_INSERT Enterprise.ProductSettingType ON
MERGE INTO Enterprise.ProductSettingType AS Target  
USING (VALUES (1021, 'ApiSecret', 'API Secret used by Identity Server to get token'))  
       AS Source ([NewProductSettingTypeId],[NewName] , [NewDescription])  
ON Target.[ProductSettingTypeId] = Source.[NewProductSettingTypeId]  
WHEN MATCHED THEN  
UPDATE SET [Name] = Source.NewName, Description = NewDescription 
WHEN NOT MATCHED BY TARGET THEN  
INSERT ([ProductSettingTypeId],[Name] , [Description]) VALUES (1021, 'ApiSecret', 'API Secret used by Identity Server to get token');

MERGE INTO Enterprise.ProductSettingType AS Target  
USING (VALUES (1022, 'SecurityToken', 'Used with Salesforce Password'))  
       AS Source ([NewProductSettingTypeId],[NewName] , [NewDescription])  
ON Target.[ProductSettingTypeId] = Source.[NewProductSettingTypeId]  
WHEN MATCHED THEN  
UPDATE SET [Name] = Source.NewName, Description = NewDescription 
WHEN NOT MATCHED BY TARGET THEN  
INSERT ([ProductSettingTypeId],[Name] , [Description]) VALUES (1022, 'SecurityToken', 'Used with Salesforce Password');

MERGE INTO Enterprise.ProductSettingType AS Target  
USING (VALUES (1023, 'ApiRoute', 'Salesforce partial API Route with version'))  
       AS Source ([NewProductSettingTypeId],[NewName] , [NewDescription])  
ON Target.[ProductSettingTypeId] = Source.[NewProductSettingTypeId]  
WHEN MATCHED THEN  
UPDATE SET [Name] = Source.NewName, Description = NewDescription 
WHEN NOT MATCHED BY TARGET THEN  
INSERT ([ProductSettingTypeId],[Name] , [Description]) VALUES (1023, 'ApiRoute', 'Salesforce partial API Route with version');

MERGE INTO Enterprise.ProductSettingType AS Target  
USING (VALUES (1024, 'TokenURL', 'Token URL for saleforce'))  
       AS Source ([NewProductSettingTypeId],[NewName] , [NewDescription])  
ON Target.[ProductSettingTypeId] = Source.[NewProductSettingTypeId]  
WHEN MATCHED THEN  
UPDATE SET [Name] = Source.NewName, Description = NewDescription 
WHEN NOT MATCHED BY TARGET THEN  
INSERT ([ProductSettingTypeId],[Name] , [Description]) VALUES (1024, 'TokenURL', 'Token URL for saleforce');

MERGE INTO Enterprise.ProductSettingType AS Target  
USING (VALUES (1025, 'PortalId', 'Required in SAML to post saleforce'))  
       AS Source ([NewProductSettingTypeId],[NewName] , [NewDescription])  
ON Target.[ProductSettingTypeId] = Source.[NewProductSettingTypeId]  
WHEN MATCHED THEN  
UPDATE SET [Name] = Source.NewName, Description = NewDescription 
WHEN NOT MATCHED BY TARGET THEN  
INSERT ([ProductSettingTypeId],[Name] , [Description]) VALUES (1025, 'PortalId', 'Required in SAML to post saleforce');

MERGE INTO Enterprise.ProductSettingType AS Target  
USING (VALUES (1026, 'OrganizationId', 'Required in SAML to post saleforce'))  
       AS Source ([NewProductSettingTypeId],[NewName] , [NewDescription])  
ON Target.[ProductSettingTypeId] = Source.[NewProductSettingTypeId]  
WHEN MATCHED THEN  
UPDATE SET [Name] = Source.NewName, Description = NewDescription 
WHEN NOT MATCHED BY TARGET THEN  
INSERT ([ProductSettingTypeId],[Name] , [Description]) VALUES (1026, 'OrganizationId', 'Required in SAML to post saleforce');

SET IDENTITY_INSERT Enterprise.ProductSettingType OFF


Print 'Adding new values to ProductSetting'

SET IDENTITY_INSERT Enterprise.ProductSetting ON

MERGE INTO Enterprise.ProductSetting AS Target  
USING (VALUES (1286, 16, 1005, 'http://web2012.compliancedepot.com/vcapi', GETUTCDATE(),NULL))  
       AS Source ([NewProductSettingId],[NewProductId],[ProductSettingTypeId], [NewValue],[NewFromDate],[NewThruDate])  
ON Target.[ProductSettingId] = Source.[NewProductSettingId]  
WHEN MATCHED THEN  
UPDATE SET [ProductId]=Source.[NewProductId],[Value]=Source.[NewValue],[FromDate] = Source.[NewFromDate],[ThruDate] = NULL 
WHEN NOT MATCHED BY TARGET THEN  
INSERT ([ProductSettingId],[ProductId],[ProductSettingTypeId], [Value],[FromDate],[ThruDate]) 
VALUES (1286, 16, 1005, 'http://web2012.compliancedepot.com/vcapi', GETUTCDATE(),NULL);

MERGE INTO Enterprise.ProductSetting AS Target  
USING (VALUES (1287, 16, 1021, 'AF6977FB-8BCE-43BD-B715-2DDC1E5A6009',GETUTCDATE(), NULL))  
       AS Source ([NewProductSettingId],[NewProductId],[ProductSettingTypeId], [NewValue],[NewFromDate],[NewThruDate])  
ON Target.[ProductSettingId] = Source.[NewProductSettingId]  
WHEN MATCHED THEN  
UPDATE SET [ProductId]=Source.[NewProductId],[Value]=Source.[NewValue],[FromDate] = Source.[NewFromDate],[ThruDate] = NULL 
WHEN NOT MATCHED BY TARGET THEN  
INSERT ([ProductSettingId],[ProductId],[ProductSettingTypeId], [Value],[FromDate],[ThruDate]) 
VALUES (1287, 16, 1021, 'AF6977FB-8BCE-43BD-B715-2DDC1E5A6009',GETUTCDATE(), NULL);

MERGE INTO Enterprise.ProductSetting AS Target  
USING (VALUES (1288,16,1,'vendorcompliance', GETUTCDATE(), NULL))  
       AS Source ([NewProductSettingId],[NewProductId],[ProductSettingTypeId], [NewValue],[NewFromDate],[NewThruDate])  
ON Target.[ProductSettingId] = Source.[NewProductSettingId]  
WHEN MATCHED THEN  
UPDATE SET [ProductId]=Source.[NewProductId],[Value]=Source.[NewValue],[FromDate] = Source.[NewFromDate],[ThruDate] = NULL 
WHEN NOT MATCHED BY TARGET THEN  
INSERT ([ProductSettingId],[ProductId],[ProductSettingTypeId], [Value],[FromDate],[ThruDate]) 
VALUES (1288,16,1,'vendorcompliance', GETUTCDATE(), NULL);

SET IDENTITY_INSERT Enterprise.ProductSetting OFF


Print 'Adding new values to SamlProductSettings'

DECLARE @NewSamlProductSettingsId int = NULL

IF (SELECT COUNT(*) FROM Ident.SamlProductSettings WHERE ProductId = 16 and SubjectIdSamlAttribute = 'productUsername') > 1
BEGIN
	Print 'Dropping Duplicates From SamlProductSettings'
	DELETE Ident.SamlProductSettings
	WHERE ProductId = 16 and SubjectIdSamlAttribute = 'productUsername'
END

SELECT @NewSamlProductSettingsId = [SamlProductSettingsId] 
	FROM Ident.SamlProductSettings 
	WHERE ProductId = 16 and SubjectIdSamlAttribute = 'productUsername'

IF @NewSamlProductSettingsId IS NULL
BEGIN
	Print 'Adding New SamlProductSetting'
	EXEC [Ident].[CreateSamlProductSetting]
		 @ProductId = 16
		,@LoginUri = 'http://rcerdsappcdp01.corp.realpage.com/webapp/acs.aspx'
		,@SigningCertificateThumbprint = 'EF26FEC08C554976572E8A9767DDA437AC452CF6'
		,@SubjectIdSamlAttribute = 'productUsername'
		,@SamlProductSettingsId = @NewSamlProductSettingsId	OUTPUT
	END
ELSE
BEGIN
	
	Print 'Updating SamlProductSetting'
	UPDATE Ident.SamlProductSettings SET 
		 [LoginUri] = 'http://rcerdsappcdp01.corp.realpage.com/webapp/acs.aspx'
		,[SigningCertificateThumbprint] = 'EF26FEC08C554976572E8A9767DDA437AC452CF6'
	WHERE ProductId = 16 AND SubjectIdSamlAttribute = 'productUsername'	
END

Print 'Updating ProductConfigutration'

MERGE INTO Enterprise.ProductConfiguration AS Target  
USING (VALUES (94, 1286, GETUTCDATE(), NULL))  
       AS Source (NewConfigurationId, NewProductSettingId, NewFromDate, NewThruDate)  
ON Target.[ConfigurationId] = Source.[NewConfigurationId] AND Target.[ProductSettingId] = Source.[NewProductSettingId]
WHEN MATCHED THEN  
UPDATE SET ConfigurationId = Source.NewConfigurationId, ProductSettingId = NewProductSettingId 
WHEN NOT MATCHED BY TARGET THEN  
INSERT (ConfigurationId,ProductSettingId,FromDate,ThruDate) VALUES (94, 1286, GETUTCDATE(), NULL);

MERGE INTO Enterprise.ProductConfiguration AS Target  
USING (VALUES (94, 1287, GETUTCDATE(), NULL))  
       AS Source (NewConfigurationId, NewProductSettingId, NewFromDate, NewThruDate)  
ON Target.[ConfigurationId] = Source.[NewConfigurationId] AND Target.[ProductSettingId] = Source.[NewProductSettingId]
WHEN MATCHED THEN  
UPDATE SET ConfigurationId = Source.NewConfigurationId, ProductSettingId = NewProductSettingId 
WHEN NOT MATCHED BY TARGET THEN  
INSERT (ConfigurationId,ProductSettingId,FromDate,ThruDate) VALUES (94, 1287, GETUTCDATE(), NULL);

MERGE INTO Enterprise.ProductConfiguration AS Target  
USING (VALUES (94, 1288, GETUTCDATE(), NULL))  
       AS Source (NewConfigurationId, NewProductSettingId, NewFromDate, NewThruDate)  
ON Target.[ConfigurationId] = Source.[NewConfigurationId] AND Target.[ProductSettingId] = Source.[NewProductSettingId]
WHEN MATCHED THEN  
UPDATE SET ConfigurationId = Source.NewConfigurationId, ProductSettingId = NewProductSettingId 
WHEN NOT MATCHED BY TARGET THEN  
INSERT (ConfigurationId,ProductSettingId,FromDate,ThruDate) VALUES (94, 1288, GETUTCDATE(), NULL);

	
EXEC sys.sp_updateextendedproperty @name=N'Build', @value='2'

