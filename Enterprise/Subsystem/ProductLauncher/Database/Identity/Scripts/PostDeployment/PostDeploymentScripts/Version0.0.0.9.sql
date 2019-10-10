
DECLARE @NOW DATETIME = GETUTCDATE()
DECLARE @ProductSettingID int = NULL
DECLARE @ProductConfigurationID int = NULL
DECLARE @GlobalProductConfigurationID int
DECLARE @NewSamlProductSettings int = NULL

IF @@SERVERNAME != 'RCDUSODBSQL001' --DEV
	AND @@SERVERNAME != 'RCTUSODBSQL001' --QA
	AND @@SERVERNAME != 'RCQUSODBSQL001' --SAT
	AND @@SERVERNAME != 'RCPGBKDBSQL005A' --PROD
BEGIN
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

	SELECT @GlobalProductConfigurationID = ConfigurationId FROM Enterprise.GlobalProductConfiguration WHERE ProductId = 14

	------------------------------------------
	SELECT @ProductSettingID = ProductSettingId FROM Enterprise.ProductSetting WHERE ProductId=14 AND ProductSettingTypeId=1010
		IF @productSettingID IS NULL
			BEGIN
				INSERT INTO Enterprise.ProductSetting ([ProductId],[ProductSettingTypeId], [Value],[FromDate],[ThruDate]) 
				VALUES (14, 1010, 'greenbookapi@realpage.com.rpidev', @NOW,NULL);

				SELECT @ProductSettingID=SCOPE_IDENTITY();
			END
		ELSE
			BEGIN
				UPDATE Enterprise.ProductSetting 
				SET ProductId = 14, ProductSettingTypeId=1010, Value='greenbookapi@realpage.com.rpidev',FromDate=@NOW,ThruDate = NULL
				WHERE ProductSettingId = @ProductSettingID
			END

	SELECT @ProductConfigurationID = ProductConfigurationId FROM Enterprise.ProductConfiguration WHERE ConfigurationId = @GlobalProductConfigurationID AND ProductSettingId = @ProductSettingID
		IF @ProductConfigurationID IS NULL
			BEGIN
				INSERT INTO Enterprise.ProductConfiguration(ConfigurationId,ProductSettingId,FromDate,ThruDate)
				VALUES(@GlobalProductConfigurationID,@ProductSettingID,@NOW,NULL)
				SELECT @ProductConfigurationID = SCOPE_IDENTITY();
			END
			ELSE
			BEGIN
				UPDATE Enterprise.ProductConfiguration SET
				ConfigurationId = @GlobalProductConfigurationID, ProductSettingId = @ProductSettingID,FromDate = @NOW,ThruDate = NULL
				WHERE ProductSettingId = @ProductSettingID
			END

	--------------------------------------------
	SET @ProductSettingID  = NULL
	SET @ProductConfigurationID  = NULL

	SELECT @ProductSettingID = ProductSettingId FROM Enterprise.ProductSetting WHERE ProductId=14 AND ProductSettingTypeId=1011
		IF @productSettingID IS NULL
			BEGIN
				INSERT INTO Enterprise.ProductSetting ([ProductId],[ProductSettingTypeId], [Value],[FromDate],[ThruDate]) 
				VALUES (14, 1011, 'GreenB0%kd3V', @NOW,NULL);

				SELECT @ProductSettingID=SCOPE_IDENTITY();
			END
		ELSE
			BEGIN
				UPDATE Enterprise.ProductSetting 
				SET ProductId = 14, ProductSettingTypeId=1011, Value='GreenB0%kd3V',FromDate=@NOW,ThruDate = NULL
				WHERE ProductSettingId = @ProductSettingID
			END

	SELECT @ProductConfigurationID = ProductConfigurationId FROM Enterprise.ProductConfiguration WHERE ConfigurationId = @GlobalProductConfigurationID AND ProductSettingId = @ProductSettingID
		IF @ProductConfigurationID IS NULL
			BEGIN
				INSERT INTO Enterprise.ProductConfiguration(ConfigurationId,ProductSettingId,FromDate,ThruDate)
				VALUES(@GlobalProductConfigurationID,@ProductSettingID,@NOW,NULL)
				SELECT @ProductConfigurationID = SCOPE_IDENTITY();
			END
			ELSE
			BEGIN
				UPDATE Enterprise.ProductConfiguration SET
				ConfigurationId = @GlobalProductConfigurationID, ProductSettingId = @ProductSettingID,FromDate = @NOW,ThruDate = NULL
				WHERE ProductSettingId = @ProductSettingID
			END

	--------------------------------------------
	SET @ProductSettingID  = NULL
	SET @ProductConfigurationID  = NULL

	SELECT @ProductSettingID = ProductSettingId FROM Enterprise.ProductSetting WHERE ProductId=14 AND ProductSettingTypeId=1012
		IF @productSettingID IS NULL
			BEGIN
				INSERT INTO Enterprise.ProductSetting ([ProductId],[ProductSettingTypeId], [Value],[FromDate],[ThruDate]) 
				VALUES (14, 1012, 'https://realpage--RPIDEV.cs19.my.salesforce.com?so=00D290000000XyT', @NOW,NULL);

				SELECT @ProductSettingID=SCOPE_IDENTITY();
			END
		ELSE
			BEGIN
				UPDATE Enterprise.ProductSetting 
				SET ProductId = 14, ProductSettingTypeId=1012, Value='https://realpage--RPIDEV.cs19.my.salesforce.com?so=00D290000000XyT',FromDate=@NOW,ThruDate = NULL
				WHERE ProductSettingId = @ProductSettingID
			END

	SELECT @ProductConfigurationID = ProductConfigurationId FROM Enterprise.ProductConfiguration WHERE ConfigurationId = @GlobalProductConfigurationID AND ProductSettingId = @ProductSettingID
		IF @ProductConfigurationID IS NULL
			BEGIN
				INSERT INTO Enterprise.ProductConfiguration(ConfigurationId,ProductSettingId,FromDate,ThruDate)
				VALUES(@GlobalProductConfigurationID,@ProductSettingID,@NOW,NULL)
				SELECT @ProductConfigurationID = SCOPE_IDENTITY();
			END
			ELSE
			BEGIN
				UPDATE Enterprise.ProductConfiguration SET
				ConfigurationId = @GlobalProductConfigurationID, ProductSettingId = @ProductSettingID,FromDate = @NOW,ThruDate = NULL
				WHERE ProductSettingId = @ProductSettingID
			END

	--------------------------------------------
	SET @ProductSettingID  = NULL
	SET @ProductConfigurationID  = NULL

	SELECT @ProductSettingID = ProductSettingId FROM Enterprise.ProductSetting WHERE ProductId=14 AND ProductSettingTypeId=1013
		IF @productSettingID IS NULL
			BEGIN
				INSERT INTO Enterprise.ProductSetting ([ProductId],[ProductSettingTypeId], [Value],[FromDate],[ThruDate]) 
				VALUES (14, 1013, '3MVG9Vik22TUgUpgLpQA6UEwAc4MBq_xXoTjysYx2vNJq2nmmo.LXp0vP24FUq9WFyWn5UT0jXEuZxuSqZ3WA', @NOW,NULL);

				SELECT @ProductSettingID=SCOPE_IDENTITY();
			END
		ELSE
			BEGIN
				UPDATE Enterprise.ProductSetting 
				SET ProductId = 14, ProductSettingTypeId=1013, Value='3MVG9Vik22TUgUpgLpQA6UEwAc4MBq_xXoTjysYx2vNJq2nmmo.LXp0vP24FUq9WFyWn5UT0jXEuZxuSqZ3WA',FromDate=@NOW,ThruDate = NULL
				WHERE ProductSettingId = @ProductSettingID
			END

	SELECT @ProductConfigurationID = ProductConfigurationId FROM Enterprise.ProductConfiguration WHERE ConfigurationId = @GlobalProductConfigurationID AND ProductSettingId = @ProductSettingID
		IF @ProductConfigurationID IS NULL
			BEGIN
				INSERT INTO Enterprise.ProductConfiguration(ConfigurationId,ProductSettingId,FromDate,ThruDate)
				VALUES(@GlobalProductConfigurationID,@ProductSettingID,@NOW,NULL)
				SELECT @ProductConfigurationID = SCOPE_IDENTITY();
			END
			ELSE
			BEGIN
				UPDATE Enterprise.ProductConfiguration SET
				ConfigurationId = @GlobalProductConfigurationID, ProductSettingId = @ProductSettingID,FromDate = @NOW,ThruDate = NULL
				WHERE ProductSettingId = @ProductSettingID
			END

	--------------------------------------------
	SET @ProductSettingID  = NULL
	SET @ProductConfigurationID  = NULL

	SELECT @ProductSettingID = ProductSettingId FROM Enterprise.ProductSetting WHERE ProductId=14 AND ProductSettingTypeId=1021
		IF @productSettingID IS NULL
			BEGIN
				INSERT INTO Enterprise.ProductSetting ([ProductId],[ProductSettingTypeId], [Value],[FromDate],[ThruDate]) 
				VALUES (14, 1021, '7116286042861318785', @NOW,NULL);

				SELECT @ProductSettingID=SCOPE_IDENTITY();
			END
		ELSE
			BEGIN
				UPDATE Enterprise.ProductSetting 
				SET ProductId = 14, ProductSettingTypeId=1021, Value='7116286042861318785',FromDate=@NOW,ThruDate = NULL
				WHERE ProductSettingId = @ProductSettingID
			END

	SELECT @ProductConfigurationID = ProductConfigurationId FROM Enterprise.ProductConfiguration WHERE ConfigurationId = @GlobalProductConfigurationID AND ProductSettingId = @ProductSettingID
		IF @ProductConfigurationID IS NULL
			BEGIN
				INSERT INTO Enterprise.ProductConfiguration(ConfigurationId,ProductSettingId,FromDate,ThruDate)
				VALUES(@GlobalProductConfigurationID,@ProductSettingID,@NOW,NULL)
				SELECT @ProductConfigurationID = SCOPE_IDENTITY();
			END
			ELSE
			BEGIN
				UPDATE Enterprise.ProductConfiguration SET
				ConfigurationId = @GlobalProductConfigurationID, ProductSettingId = @ProductSettingID,FromDate = @NOW,ThruDate = NULL
				WHERE ProductSettingId = @ProductSettingID
			END

	--------------------------------------------
	SET @ProductSettingID  = NULL
	SET @ProductConfigurationID  = NULL

	SELECT @ProductSettingID = ProductSettingId FROM Enterprise.ProductSetting WHERE ProductId=14 AND ProductSettingTypeId=1022
		IF @productSettingID IS NULL
			BEGIN
				INSERT INTO Enterprise.ProductSetting ([ProductId],[ProductSettingTypeId], [Value],[FromDate],[ThruDate]) 
				VALUES (14, 1022, 'WMgMrIlDLHhmSlRKZPt2iaBSb', @NOW,NULL);

				SELECT @ProductSettingID=SCOPE_IDENTITY();
			END
		ELSE
			BEGIN
				UPDATE Enterprise.ProductSetting 
				SET ProductId = 14, ProductSettingTypeId=1022, Value='WMgMrIlDLHhmSlRKZPt2iaBSb',FromDate=@NOW,ThruDate = NULL
				WHERE ProductSettingId = @ProductSettingID
			END

	SELECT @ProductConfigurationID = ProductConfigurationId FROM Enterprise.ProductConfiguration WHERE ConfigurationId = @GlobalProductConfigurationID AND ProductSettingId = @ProductSettingID
		IF @ProductConfigurationID IS NULL
			BEGIN
				INSERT INTO Enterprise.ProductConfiguration(ConfigurationId,ProductSettingId,FromDate,ThruDate)
				VALUES(@GlobalProductConfigurationID,@ProductSettingID,@NOW,NULL)
				SELECT @ProductConfigurationID = SCOPE_IDENTITY();
			END
			ELSE
			BEGIN
				UPDATE Enterprise.ProductConfiguration SET
				ConfigurationId = @GlobalProductConfigurationID, ProductSettingId = @ProductSettingID,FromDate = @NOW,ThruDate = NULL
				WHERE ProductSettingId = @ProductSettingID
			END

	--------------------------------------------
	SET @ProductSettingID  = NULL
	SET @ProductConfigurationID  = NULL

	SELECT @ProductSettingID = ProductSettingId FROM Enterprise.ProductSetting WHERE ProductId=14 AND ProductSettingTypeId=1023
		IF @productSettingID IS NULL
			BEGIN
				INSERT INTO Enterprise.ProductSetting ([ProductId],[ProductSettingTypeId], [Value],[FromDate],[ThruDate]) 
				VALUES (14, 1023, '/services/data/v39.0/', @NOW,NULL);

				SELECT @ProductSettingID=SCOPE_IDENTITY();
			END
		ELSE
			BEGIN
				UPDATE Enterprise.ProductSetting 
				SET ProductId = 14, ProductSettingTypeId=1023, Value='/services/data/v39.0/',FromDate=@NOW,ThruDate = NULL
				WHERE ProductSettingId = @ProductSettingID
			END

	SELECT @ProductConfigurationID = ProductConfigurationId FROM Enterprise.ProductConfiguration WHERE ConfigurationId = @GlobalProductConfigurationID AND ProductSettingId = @ProductSettingID
		IF @ProductConfigurationID IS NULL
			BEGIN
				INSERT INTO Enterprise.ProductConfiguration(ConfigurationId,ProductSettingId,FromDate,ThruDate)
				VALUES(@GlobalProductConfigurationID,@ProductSettingID,@NOW,NULL)
				SELECT @ProductConfigurationID = SCOPE_IDENTITY();
			END
			ELSE
			BEGIN
				UPDATE Enterprise.ProductConfiguration SET
				ConfigurationId = @GlobalProductConfigurationID, ProductSettingId = @ProductSettingID,FromDate = @NOW,ThruDate = NULL
				WHERE ProductSettingId = @ProductSettingID
			END

	--------------------------------------------
	SET @ProductSettingID  = NULL
	SET @ProductConfigurationID  = NULL

	SELECT @ProductSettingID = ProductSettingId FROM Enterprise.ProductSetting WHERE ProductId=14 AND ProductSettingTypeId=1024
		IF @productSettingID IS NULL
			BEGIN
				INSERT INTO Enterprise.ProductSetting ([ProductId],[ProductSettingTypeId], [Value],[FromDate],[ThruDate]) 
				VALUES (14, 1024, 'https://test.salesforce.com/services/oauth2/token', @NOW,NULL);

				SELECT @ProductSettingID=SCOPE_IDENTITY();
			END
		ELSE
			BEGIN
				UPDATE Enterprise.ProductSetting 
				SET ProductId = 14, ProductSettingTypeId=1024, Value='https://test.salesforce.com/services/oauth2/token',FromDate=@NOW,ThruDate = NULL			
				WHERE ProductSettingId = @ProductSettingID
			END

	SELECT @ProductConfigurationID = ProductConfigurationId FROM Enterprise.ProductConfiguration WHERE ConfigurationId = @GlobalProductConfigurationID AND ProductSettingId = @ProductSettingID
		IF @ProductConfigurationID IS NULL
			BEGIN
				INSERT INTO Enterprise.ProductConfiguration(ConfigurationId,ProductSettingId,FromDate,ThruDate)
				VALUES(@GlobalProductConfigurationID,@ProductSettingID,@NOW,NULL)
				SELECT @ProductConfigurationID = SCOPE_IDENTITY();
			END
			ELSE
			BEGIN
				UPDATE Enterprise.ProductConfiguration SET
				ConfigurationId = @GlobalProductConfigurationID, ProductSettingId = @ProductSettingID,FromDate = @NOW,ThruDate = NULL
				WHERE ProductSettingId = @ProductSettingID
			END

	--------------------------------------------
	SET @ProductSettingID  = NULL
	SET @ProductConfigurationID  = NULL

	SELECT @ProductSettingID = ProductSettingId FROM Enterprise.ProductSetting WHERE ProductId=14 AND ProductSettingTypeId=1025
		IF @productSettingID IS NULL
			BEGIN
				INSERT INTO Enterprise.ProductSetting ([ProductId],[ProductSettingTypeId], [Value],[FromDate],[ThruDate]) 
				VALUES (14, 1025, '060000000005YDy', @NOW,NULL);

				SELECT @ProductSettingID=SCOPE_IDENTITY();
			END
		ELSE
			BEGIN
				UPDATE Enterprise.ProductSetting 
				SET ProductId = 14, ProductSettingTypeId=1025, Value='060000000005YDy',FromDate=@NOW,ThruDate = NULL
				WHERE ProductSettingId = @ProductSettingID
			END

	SELECT @ProductConfigurationID = ProductConfigurationId FROM Enterprise.ProductConfiguration WHERE ConfigurationId = @GlobalProductConfigurationID AND ProductSettingId = @ProductSettingID
		IF @ProductConfigurationID IS NULL
			BEGIN
				INSERT INTO Enterprise.ProductConfiguration(ConfigurationId,ProductSettingId,FromDate,ThruDate)
				VALUES(@GlobalProductConfigurationID,@ProductSettingID,@NOW,NULL)
				SELECT @ProductConfigurationID = SCOPE_IDENTITY();
			END
			ELSE
			BEGIN
				UPDATE Enterprise.ProductConfiguration SET
				ConfigurationId = @GlobalProductConfigurationID, ProductSettingId = @ProductSettingID,FromDate = @NOW,ThruDate = NULL
				WHERE ProductSettingId = @ProductSettingID
			END

	----------------------------------------------
	SET @ProductSettingID  = NULL
	SET @ProductConfigurationID  = NULL

	SELECT @ProductSettingID = ProductSettingId FROM Enterprise.ProductSetting WHERE ProductId=14 AND ProductSettingTypeId=1026
		IF @productSettingID IS NULL
			BEGIN
				INSERT INTO Enterprise.ProductSetting ([ProductId],[ProductSettingTypeId], [Value],[FromDate],[ThruDate]) 
				VALUES (14, 1026, '00D290000000XyT', @NOW,NULL);

				SELECT @ProductSettingID=SCOPE_IDENTITY();
			END
		ELSE
			BEGIN
				UPDATE Enterprise.ProductSetting 
				SET ProductId = 14, ProductSettingTypeId=1026, Value='00D290000000XyT',FromDate=@NOW,ThruDate = NULL
				WHERE ProductSettingId = @ProductSettingID
			END

	SELECT @ProductConfigurationID = ProductConfigurationId FROM Enterprise.ProductConfiguration WHERE ConfigurationId = @GlobalProductConfigurationID AND ProductSettingId = @ProductSettingID
		IF @ProductConfigurationID IS NULL
			BEGIN
				INSERT INTO Enterprise.ProductConfiguration(ConfigurationId,ProductSettingId,FromDate,ThruDate)
				VALUES(@GlobalProductConfigurationID,@ProductSettingID,@NOW,NULL)
				SELECT @ProductConfigurationID = SCOPE_IDENTITY();
			END
			ELSE
			BEGIN
				UPDATE Enterprise.ProductConfiguration SET
				ConfigurationId = @GlobalProductConfigurationID, ProductSettingId = @ProductSettingID,FromDate = @NOW,ThruDate = NULL
				WHERE ProductSettingId = @ProductSettingID
			END


	Print 'Adding new values to SamlProductSettings'

	IF (SELECT COUNT(*) FROM Ident.SamlProductSettings WHERE ProductId = 14 and SubjectIdSamlAttribute = 'productUsername') > 1
	BEGIN
		Print 'Dropping Duplicates From SamlProductSettings'
		DELETE Ident.SamlProductSettings
		WHERE ProductId = 14 and SubjectIdSamlAttribute = 'productUsername'
	END

	SELECT @NewSamlProductSettings = [SamlProductSettingsId] 
		FROM Ident.SamlProductSettings 
		WHERE ProductId = 14 and SubjectIdSamlAttribute = 'productUsername'

	IF @NewSamlProductSettings IS NULL
	BEGIN
		Print 'Adding New SamlProductSetting'
		EXEC [Ident].[CreateSamlProductSetting]
			 @ProductId = 14
			,@LoginUri = 'https://realpage--RPIDEV.cs19.my.salesforce.com?so=00D290000000XyT'
			,@SigningCertificateThumbprint = 'EF26FEC08C554976572E8A9767DDA437AC452CF6'
			,@SubjectIdSamlAttribute = 'productUsername'
			,@SamlProductSettingsId = @NewSamlProductSettings	OUTPUT
		END
	ELSE
	BEGIN
	
		Print 'Updating SamlProductSetting'
		UPDATE Ident.SamlProductSettings SET 
			 [LoginUri] = 'https://realpage--RPIDEV.cs19.my.salesforce.com?so=00D290000000XyT'
			,[SigningCertificateThumbprint] = 'EF26FEC08C554976572E8A9767DDA437AC452CF6'
		WHERE ProductId = 14 AND SubjectIdSamlAttribute = 'productUsername'	
	END
END
--=================================================================================================================================================================================
--=================================================================================================================================================================================

IF @@SERVERNAME = 'RCDUSODBSQL001'
BEGIN
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

	SELECT @GlobalProductConfigurationID = ConfigurationId FROM Enterprise.GlobalProductConfiguration WHERE ProductId = 14

	------------------------------------------
	SELECT @ProductSettingID = ProductSettingId FROM Enterprise.ProductSetting WHERE ProductId=14 AND ProductSettingTypeId=1010
		IF @productSettingID IS NULL
			BEGIN
				INSERT INTO Enterprise.ProductSetting ([ProductId],[ProductSettingTypeId], [Value],[FromDate],[ThruDate]) 
				VALUES (14, 1010, 'greenbookapi@realpage.com.rpidev', @NOW,NULL);

				SELECT @ProductSettingID=SCOPE_IDENTITY();
			END
		ELSE
			BEGIN
				UPDATE Enterprise.ProductSetting 
				SET ProductId = 14, ProductSettingTypeId=1010, Value='greenbookapi@realpage.com.rpidev',FromDate=@NOW,ThruDate = NULL
				WHERE ProductSettingId = @ProductSettingID
			END

	SELECT @ProductConfigurationID = ProductConfigurationId FROM Enterprise.ProductConfiguration WHERE ConfigurationId = @GlobalProductConfigurationID AND ProductSettingId = @ProductSettingID
		IF @ProductConfigurationID IS NULL
			BEGIN
				INSERT INTO Enterprise.ProductConfiguration(ConfigurationId,ProductSettingId,FromDate,ThruDate)
				VALUES(@GlobalProductConfigurationID,@ProductSettingID,@NOW,NULL)
				SELECT @ProductConfigurationID = SCOPE_IDENTITY();
			END
			ELSE
			BEGIN
				UPDATE Enterprise.ProductConfiguration SET
				ConfigurationId = @GlobalProductConfigurationID, ProductSettingId = @ProductSettingID,FromDate = @NOW,ThruDate = NULL
				WHERE ProductSettingId = @ProductSettingID
			END

	--------------------------------------------
	SET @ProductSettingID  = NULL
	SET @ProductConfigurationID  = NULL

	SELECT @ProductSettingID = ProductSettingId FROM Enterprise.ProductSetting WHERE ProductId=14 AND ProductSettingTypeId=1011
		IF @productSettingID IS NULL
			BEGIN
				INSERT INTO Enterprise.ProductSetting ([ProductId],[ProductSettingTypeId], [Value],[FromDate],[ThruDate]) 
				VALUES (14, 1011, 'GreenB0%kd3V', @NOW,NULL);

				SELECT @ProductSettingID=SCOPE_IDENTITY();
			END
		ELSE
			BEGIN
				UPDATE Enterprise.ProductSetting 
				SET ProductId = 14, ProductSettingTypeId=1011, Value='GreenB0%kd3V',FromDate=@NOW,ThruDate = NULL
				WHERE ProductSettingId = @ProductSettingID
			END

	SELECT @ProductConfigurationID = ProductConfigurationId FROM Enterprise.ProductConfiguration WHERE ConfigurationId = @GlobalProductConfigurationID AND ProductSettingId = @ProductSettingID
		IF @ProductConfigurationID IS NULL
			BEGIN
				INSERT INTO Enterprise.ProductConfiguration(ConfigurationId,ProductSettingId,FromDate,ThruDate)
				VALUES(@GlobalProductConfigurationID,@ProductSettingID,@NOW,NULL)
				SELECT @ProductConfigurationID = SCOPE_IDENTITY();
			END
			ELSE
			BEGIN
				UPDATE Enterprise.ProductConfiguration SET
				ConfigurationId = @GlobalProductConfigurationID, ProductSettingId = @ProductSettingID,FromDate = @NOW,ThruDate = NULL
				WHERE ProductSettingId = @ProductSettingID
			END

	--------------------------------------------
	SET @ProductSettingID  = NULL
	SET @ProductConfigurationID  = NULL

	SELECT @ProductSettingID = ProductSettingId FROM Enterprise.ProductSetting WHERE ProductId=14 AND ProductSettingTypeId=1012
		IF @productSettingID IS NULL
			BEGIN
				INSERT INTO Enterprise.ProductSetting ([ProductId],[ProductSettingTypeId], [Value],[FromDate],[ThruDate]) 
				VALUES (14, 1012, 'https://realpage--RPIDEV.cs19.my.salesforce.com?so=00D290000000XyT', @NOW,NULL);

				SELECT @ProductSettingID=SCOPE_IDENTITY();
			END
		ELSE
			BEGIN
				UPDATE Enterprise.ProductSetting 
				SET ProductId = 14, ProductSettingTypeId=1012, Value='https://realpage--RPIDEV.cs19.my.salesforce.com?so=00D290000000XyT',FromDate=@NOW,ThruDate = NULL
				WHERE ProductSettingId = @ProductSettingID
			END

	SELECT @ProductConfigurationID = ProductConfigurationId FROM Enterprise.ProductConfiguration WHERE ConfigurationId = @GlobalProductConfigurationID AND ProductSettingId = @ProductSettingID
		IF @ProductConfigurationID IS NULL
			BEGIN
				INSERT INTO Enterprise.ProductConfiguration(ConfigurationId,ProductSettingId,FromDate,ThruDate)
				VALUES(@GlobalProductConfigurationID,@ProductSettingID,@NOW,NULL)
				SELECT @ProductConfigurationID = SCOPE_IDENTITY();
			END
			ELSE
			BEGIN
				UPDATE Enterprise.ProductConfiguration SET
				ConfigurationId = @GlobalProductConfigurationID, ProductSettingId = @ProductSettingID,FromDate = @NOW,ThruDate = NULL
				WHERE ProductSettingId = @ProductSettingID
			END

	--------------------------------------------
	SET @ProductSettingID  = NULL
	SET @ProductConfigurationID  = NULL

	SELECT @ProductSettingID = ProductSettingId FROM Enterprise.ProductSetting WHERE ProductId=14 AND ProductSettingTypeId=1013
		IF @productSettingID IS NULL
			BEGIN
				INSERT INTO Enterprise.ProductSetting ([ProductId],[ProductSettingTypeId], [Value],[FromDate],[ThruDate]) 
				VALUES (14, 1013, '3MVG9Vik22TUgUpgLpQA6UEwAc4MBq_xXoTjysYx2vNJq2nmmo.LXp0vP24FUq9WFyWn5UT0jXEuZxuSqZ3WA', @NOW,NULL);

				SELECT @ProductSettingID=SCOPE_IDENTITY();
			END
		ELSE
			BEGIN
				UPDATE Enterprise.ProductSetting 
				SET ProductId = 14, ProductSettingTypeId=1013, Value='3MVG9Vik22TUgUpgLpQA6UEwAc4MBq_xXoTjysYx2vNJq2nmmo.LXp0vP24FUq9WFyWn5UT0jXEuZxuSqZ3WA',FromDate=@NOW,ThruDate = NULL
				WHERE ProductSettingId = @ProductSettingID
			END

	SELECT @ProductConfigurationID = ProductConfigurationId FROM Enterprise.ProductConfiguration WHERE ConfigurationId = @GlobalProductConfigurationID AND ProductSettingId = @ProductSettingID
		IF @ProductConfigurationID IS NULL
			BEGIN
				INSERT INTO Enterprise.ProductConfiguration(ConfigurationId,ProductSettingId,FromDate,ThruDate)
				VALUES(@GlobalProductConfigurationID,@ProductSettingID,@NOW,NULL)
				SELECT @ProductConfigurationID = SCOPE_IDENTITY();
			END
			ELSE
			BEGIN
				UPDATE Enterprise.ProductConfiguration SET
				ConfigurationId = @GlobalProductConfigurationID, ProductSettingId = @ProductSettingID,FromDate = @NOW,ThruDate = NULL
				WHERE ProductSettingId = @ProductSettingID
			END

	--------------------------------------------
	SET @ProductSettingID  = NULL
	SET @ProductConfigurationID  = NULL

	SELECT @ProductSettingID = ProductSettingId FROM Enterprise.ProductSetting WHERE ProductId=14 AND ProductSettingTypeId=1021
		IF @productSettingID IS NULL
			BEGIN
				INSERT INTO Enterprise.ProductSetting ([ProductId],[ProductSettingTypeId], [Value],[FromDate],[ThruDate]) 
				VALUES (14, 1021, '7116286042861318785', @NOW,NULL);

				SELECT @ProductSettingID=SCOPE_IDENTITY();
			END
		ELSE
			BEGIN
				UPDATE Enterprise.ProductSetting 
				SET ProductId = 14, ProductSettingTypeId=1021, Value='7116286042861318785',FromDate=@NOW,ThruDate = NULL
				WHERE ProductSettingId = @ProductSettingID
			END

	SELECT @ProductConfigurationID = ProductConfigurationId FROM Enterprise.ProductConfiguration WHERE ConfigurationId = @GlobalProductConfigurationID AND ProductSettingId = @ProductSettingID
		IF @ProductConfigurationID IS NULL
			BEGIN
				INSERT INTO Enterprise.ProductConfiguration(ConfigurationId,ProductSettingId,FromDate,ThruDate)
				VALUES(@GlobalProductConfigurationID,@ProductSettingID,@NOW,NULL)
				SELECT @ProductConfigurationID = SCOPE_IDENTITY();
			END
			ELSE
			BEGIN
				UPDATE Enterprise.ProductConfiguration SET
				ConfigurationId = @GlobalProductConfigurationID, ProductSettingId = @ProductSettingID,FromDate = @NOW,ThruDate = NULL
				WHERE ProductSettingId = @ProductSettingID
			END

	--------------------------------------------
	SET @ProductSettingID  = NULL
	SET @ProductConfigurationID  = NULL

	SELECT @ProductSettingID = ProductSettingId FROM Enterprise.ProductSetting WHERE ProductId=14 AND ProductSettingTypeId=1022
		IF @productSettingID IS NULL
			BEGIN
				INSERT INTO Enterprise.ProductSetting ([ProductId],[ProductSettingTypeId], [Value],[FromDate],[ThruDate]) 
				VALUES (14, 1022, 'WMgMrIlDLHhmSlRKZPt2iaBSb', @NOW,NULL);

				SELECT @ProductSettingID=SCOPE_IDENTITY();
			END
		ELSE
			BEGIN
				UPDATE Enterprise.ProductSetting 
				SET ProductId = 14, ProductSettingTypeId=1022, Value='WMgMrIlDLHhmSlRKZPt2iaBSb',FromDate=@NOW,ThruDate = NULL
				WHERE ProductSettingId = @ProductSettingID
			END

	SELECT @ProductConfigurationID = ProductConfigurationId FROM Enterprise.ProductConfiguration WHERE ConfigurationId = @GlobalProductConfigurationID AND ProductSettingId = @ProductSettingID
		IF @ProductConfigurationID IS NULL
			BEGIN
				INSERT INTO Enterprise.ProductConfiguration(ConfigurationId,ProductSettingId,FromDate,ThruDate)
				VALUES(@GlobalProductConfigurationID,@ProductSettingID,@NOW,NULL)
				SELECT @ProductConfigurationID = SCOPE_IDENTITY();
			END
			ELSE
			BEGIN
				UPDATE Enterprise.ProductConfiguration SET
				ConfigurationId = @GlobalProductConfigurationID, ProductSettingId = @ProductSettingID,FromDate = @NOW,ThruDate = NULL
				WHERE ProductSettingId = @ProductSettingID
			END

	--------------------------------------------
	SET @ProductSettingID  = NULL
	SET @ProductConfigurationID  = NULL

	SELECT @ProductSettingID = ProductSettingId FROM Enterprise.ProductSetting WHERE ProductId=14 AND ProductSettingTypeId=1023
		IF @productSettingID IS NULL
			BEGIN
				INSERT INTO Enterprise.ProductSetting ([ProductId],[ProductSettingTypeId], [Value],[FromDate],[ThruDate]) 
				VALUES (14, 1023, '/services/data/v39.0/', @NOW,NULL);

				SELECT @ProductSettingID=SCOPE_IDENTITY();
			END
		ELSE
			BEGIN
				UPDATE Enterprise.ProductSetting 
				SET ProductId = 14, ProductSettingTypeId=1023, Value='/services/data/v39.0/',FromDate=@NOW,ThruDate = NULL
				WHERE ProductSettingId = @ProductSettingID
			END

	SELECT @ProductConfigurationID = ProductConfigurationId FROM Enterprise.ProductConfiguration WHERE ConfigurationId = @GlobalProductConfigurationID AND ProductSettingId = @ProductSettingID
		IF @ProductConfigurationID IS NULL
			BEGIN
				INSERT INTO Enterprise.ProductConfiguration(ConfigurationId,ProductSettingId,FromDate,ThruDate)
				VALUES(@GlobalProductConfigurationID,@ProductSettingID,@NOW,NULL)
				SELECT @ProductConfigurationID = SCOPE_IDENTITY();
			END
			ELSE
			BEGIN
				UPDATE Enterprise.ProductConfiguration SET
				ConfigurationId = @GlobalProductConfigurationID, ProductSettingId = @ProductSettingID,FromDate = @NOW,ThruDate = NULL
				WHERE ProductSettingId = @ProductSettingID
			END

	--------------------------------------------
	SET @ProductSettingID  = NULL
	SET @ProductConfigurationID  = NULL

	SELECT @ProductSettingID = ProductSettingId FROM Enterprise.ProductSetting WHERE ProductId=14 AND ProductSettingTypeId=1024
		IF @productSettingID IS NULL
			BEGIN
				INSERT INTO Enterprise.ProductSetting ([ProductId],[ProductSettingTypeId], [Value],[FromDate],[ThruDate]) 
				VALUES (14, 1024, 'https://test.salesforce.com/services/oauth2/token', @NOW,NULL);

				SELECT @ProductSettingID=SCOPE_IDENTITY();
			END
		ELSE
			BEGIN
				UPDATE Enterprise.ProductSetting 
				SET ProductId = 14, ProductSettingTypeId=1024, Value='https://test.salesforce.com/services/oauth2/token',FromDate=@NOW,ThruDate = NULL			
				WHERE ProductSettingId = @ProductSettingID
			END

	SELECT @ProductConfigurationID = ProductConfigurationId FROM Enterprise.ProductConfiguration WHERE ConfigurationId = @GlobalProductConfigurationID AND ProductSettingId = @ProductSettingID
		IF @ProductConfigurationID IS NULL
			BEGIN
				INSERT INTO Enterprise.ProductConfiguration(ConfigurationId,ProductSettingId,FromDate,ThruDate)
				VALUES(@GlobalProductConfigurationID,@ProductSettingID,@NOW,NULL)
				SELECT @ProductConfigurationID = SCOPE_IDENTITY();
			END
			ELSE
			BEGIN
				UPDATE Enterprise.ProductConfiguration SET
				ConfigurationId = @GlobalProductConfigurationID, ProductSettingId = @ProductSettingID,FromDate = @NOW,ThruDate = NULL
				WHERE ProductSettingId = @ProductSettingID
			END

	--------------------------------------------
	SET @ProductSettingID  = NULL
	SET @ProductConfigurationID  = NULL

	SELECT @ProductSettingID = ProductSettingId FROM Enterprise.ProductSetting WHERE ProductId=14 AND ProductSettingTypeId=1025
		IF @productSettingID IS NULL
			BEGIN
				INSERT INTO Enterprise.ProductSetting ([ProductId],[ProductSettingTypeId], [Value],[FromDate],[ThruDate]) 
				VALUES (14, 1025, '060000000005YDy', @NOW,NULL);

				SELECT @ProductSettingID=SCOPE_IDENTITY();
			END
		ELSE
			BEGIN
				UPDATE Enterprise.ProductSetting 
				SET ProductId = 14, ProductSettingTypeId=1025, Value='060000000005YDy',FromDate=@NOW,ThruDate = NULL
				WHERE ProductSettingId = @ProductSettingID
			END

	SELECT @ProductConfigurationID = ProductConfigurationId FROM Enterprise.ProductConfiguration WHERE ConfigurationId = @GlobalProductConfigurationID AND ProductSettingId = @ProductSettingID
		IF @ProductConfigurationID IS NULL
			BEGIN
				INSERT INTO Enterprise.ProductConfiguration(ConfigurationId,ProductSettingId,FromDate,ThruDate)
				VALUES(@GlobalProductConfigurationID,@ProductSettingID,@NOW,NULL)
				SELECT @ProductConfigurationID = SCOPE_IDENTITY();
			END
			ELSE
			BEGIN
				UPDATE Enterprise.ProductConfiguration SET
				ConfigurationId = @GlobalProductConfigurationID, ProductSettingId = @ProductSettingID,FromDate = @NOW,ThruDate = NULL
				WHERE ProductSettingId = @ProductSettingID
			END

	----------------------------------------------
	SET @ProductSettingID  = NULL
	SET @ProductConfigurationID  = NULL

	SELECT @ProductSettingID = ProductSettingId FROM Enterprise.ProductSetting WHERE ProductId=14 AND ProductSettingTypeId=1026
		IF @productSettingID IS NULL
			BEGIN
				INSERT INTO Enterprise.ProductSetting ([ProductId],[ProductSettingTypeId], [Value],[FromDate],[ThruDate]) 
				VALUES (14, 1026, '00D290000000XyT', @NOW,NULL);

				SELECT @ProductSettingID=SCOPE_IDENTITY();
			END
		ELSE
			BEGIN
				UPDATE Enterprise.ProductSetting 
				SET ProductId = 14, ProductSettingTypeId=1026, Value='00D290000000XyT',FromDate=@NOW,ThruDate = NULL
				WHERE ProductSettingId = @ProductSettingID
			END

	SELECT @ProductConfigurationID = ProductConfigurationId FROM Enterprise.ProductConfiguration WHERE ConfigurationId = @GlobalProductConfigurationID AND ProductSettingId = @ProductSettingID
		IF @ProductConfigurationID IS NULL
			BEGIN
				INSERT INTO Enterprise.ProductConfiguration(ConfigurationId,ProductSettingId,FromDate,ThruDate)
				VALUES(@GlobalProductConfigurationID,@ProductSettingID,@NOW,NULL)
				SELECT @ProductConfigurationID = SCOPE_IDENTITY();
			END
			ELSE
			BEGIN
				UPDATE Enterprise.ProductConfiguration SET
				ConfigurationId = @GlobalProductConfigurationID, ProductSettingId = @ProductSettingID,FromDate = @NOW,ThruDate = NULL
				WHERE ProductSettingId = @ProductSettingID
			END


	Print 'Adding new values to SamlProductSettings'

	IF (SELECT COUNT(*) FROM Ident.SamlProductSettings WHERE ProductId = 14 and SubjectIdSamlAttribute = 'productUsername') > 1
	BEGIN
		Print 'Dropping Duplicates From SamlProductSettings'
		DELETE Ident.SamlProductSettings
		WHERE ProductId = 14 and SubjectIdSamlAttribute = 'productUsername'
	END

	SELECT @NewSamlProductSettings = [SamlProductSettingsId] 
		FROM Ident.SamlProductSettings 
		WHERE ProductId = 14 and SubjectIdSamlAttribute = 'productUsername'

	IF @NewSamlProductSettings IS NULL
	BEGIN
		Print 'Adding New SamlProductSetting'
		EXEC [Ident].[CreateSamlProductSetting]
			 @ProductId = 14
			,@LoginUri = 'https://realpage--RPIDEV.cs19.my.salesforce.com?so=00D290000000XyT'
			,@SigningCertificateThumbprint = 'EF26FEC08C554976572E8A9767DDA437AC452CF6'
			,@SubjectIdSamlAttribute = 'productUsername'
			,@SamlProductSettingsId = @NewSamlProductSettings	OUTPUT
		END
	ELSE
	BEGIN
	
		Print 'Updating SamlProductSetting'
		UPDATE Ident.SamlProductSettings SET 
			 [LoginUri] = 'https://realpage--RPIDEV.cs19.my.salesforce.com?so=00D290000000XyT'
			,[SigningCertificateThumbprint] = 'EF26FEC08C554976572E8A9767DDA437AC452CF6'
		WHERE ProductId = 14 AND SubjectIdSamlAttribute = 'productUsername'	
	END
END
--=================================================================================================================================================================================
--=================================================================================================================================================================================
IF @@SERVERNAME = 'RCTUSODBSQL001'
BEGIN
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

	SELECT @GlobalProductConfigurationID = ConfigurationId FROM Enterprise.GlobalProductConfiguration WHERE ProductId = 14

	------------------------------------------
	SELECT @ProductSettingID = ProductSettingId FROM Enterprise.ProductSetting WHERE ProductId=14 AND ProductSettingTypeId=1010
		IF @productSettingID IS NULL
			BEGIN
				INSERT INTO Enterprise.ProductSetting ([ProductId],[ProductSettingTypeId], [Value],[FromDate],[ThruDate]) 
				VALUES (14, 1010, 'greenbookapi@realpage.com.rpiuat', @NOW,NULL);

				SELECT @ProductSettingID=SCOPE_IDENTITY();
			END
		ELSE
			BEGIN
				UPDATE Enterprise.ProductSetting 
				SET ProductId = 14, ProductSettingTypeId=1010, Value='greenbookapi@realpage.com.rpiuat',FromDate=@NOW,ThruDate = NULL
				WHERE ProductSettingId = @ProductSettingID
			END

	SELECT @ProductConfigurationID = ProductConfigurationId FROM Enterprise.ProductConfiguration WHERE ConfigurationId = @GlobalProductConfigurationID AND ProductSettingId = @ProductSettingID
		IF @ProductConfigurationID IS NULL
			BEGIN
				INSERT INTO Enterprise.ProductConfiguration(ConfigurationId,ProductSettingId,FromDate,ThruDate)
				VALUES(@GlobalProductConfigurationID,@ProductSettingID,@NOW,NULL)
				SELECT @ProductConfigurationID = SCOPE_IDENTITY();
			END
			ELSE
			BEGIN
				UPDATE Enterprise.ProductConfiguration SET
				ConfigurationId = @GlobalProductConfigurationID, ProductSettingId = @ProductSettingID,FromDate = @NOW,ThruDate = NULL
				WHERE ProductSettingId = @ProductSettingID
			END

	--------------------------------------------
	SET @ProductSettingID  = NULL
	SET @ProductConfigurationID  = NULL

	SELECT @ProductSettingID = ProductSettingId FROM Enterprise.ProductSetting WHERE ProductId=14 AND ProductSettingTypeId=1011
		IF @productSettingID IS NULL
			BEGIN
				INSERT INTO Enterprise.ProductSetting ([ProductId],[ProductSettingTypeId], [Value],[FromDate],[ThruDate]) 
				VALUES (14, 1011, 'GreenB0%kU@t', @NOW,NULL);

				SELECT @ProductSettingID=SCOPE_IDENTITY();
			END
		ELSE
			BEGIN
				UPDATE Enterprise.ProductSetting 
				SET ProductId = 14, ProductSettingTypeId=1011, Value='GreenB0%kU@t',FromDate=@NOW,ThruDate = NULL
				WHERE ProductSettingId = @ProductSettingID
			END

	SELECT @ProductConfigurationID = ProductConfigurationId FROM Enterprise.ProductConfiguration WHERE ConfigurationId = @GlobalProductConfigurationID AND ProductSettingId = @ProductSettingID
		IF @ProductConfigurationID IS NULL
			BEGIN
				INSERT INTO Enterprise.ProductConfiguration(ConfigurationId,ProductSettingId,FromDate,ThruDate)
				VALUES(@GlobalProductConfigurationID,@ProductSettingID,@NOW,NULL)
				SELECT @ProductConfigurationID = SCOPE_IDENTITY();
			END
			ELSE
			BEGIN
				UPDATE Enterprise.ProductConfiguration SET
				ConfigurationId = @GlobalProductConfigurationID, ProductSettingId = @ProductSettingID,FromDate = @NOW,ThruDate = NULL
				WHERE ProductSettingId = @ProductSettingID
			END

	--------------------------------------------
	SET @ProductSettingID  = NULL
	SET @ProductConfigurationID  = NULL

	SELECT @ProductSettingID = ProductSettingId FROM Enterprise.ProductSetting WHERE ProductId=14 AND ProductSettingTypeId=1012
		IF @productSettingID IS NULL
			BEGIN
				INSERT INTO Enterprise.ProductSetting ([ProductId],[ProductSettingTypeId], [Value],[FromDate],[ThruDate]) 
				VALUES (14, 1012, 'https://realpage--RPIUAT.cs12.my.salesforce.com?so=00DV0000007zGCk', @NOW,NULL);

				SELECT @ProductSettingID=SCOPE_IDENTITY();
			END
		ELSE
			BEGIN
				UPDATE Enterprise.ProductSetting 
				SET ProductId = 14, ProductSettingTypeId=1012, Value='https://realpage--RPIUAT.cs12.my.salesforce.com?so=00DV0000007zGCk',FromDate=@NOW,ThruDate = NULL
				WHERE ProductSettingId = @ProductSettingID
			END

	SELECT @ProductConfigurationID = ProductConfigurationId FROM Enterprise.ProductConfiguration WHERE ConfigurationId = @GlobalProductConfigurationID AND ProductSettingId = @ProductSettingID
		IF @ProductConfigurationID IS NULL
			BEGIN
				INSERT INTO Enterprise.ProductConfiguration(ConfigurationId,ProductSettingId,FromDate,ThruDate)
				VALUES(@GlobalProductConfigurationID,@ProductSettingID,@NOW,NULL)
				SELECT @ProductConfigurationID = SCOPE_IDENTITY();
			END
			ELSE
			BEGIN
				UPDATE Enterprise.ProductConfiguration SET
				ConfigurationId = @GlobalProductConfigurationID, ProductSettingId = @ProductSettingID,FromDate = @NOW,ThruDate = NULL
				WHERE ProductSettingId = @ProductSettingID
			END

	--------------------------------------------
	SET @ProductSettingID  = NULL
	SET @ProductConfigurationID  = NULL

	SELECT @ProductSettingID = ProductSettingId FROM Enterprise.ProductSetting WHERE ProductId=14 AND ProductSettingTypeId=1013
		IF @productSettingID IS NULL
			BEGIN
				INSERT INTO Enterprise.ProductSetting ([ProductId],[ProductSettingTypeId], [Value],[FromDate],[ThruDate]) 
				VALUES (14, 1013, '3MVG9Oe7T3Ol0ea4bdh_hA1xr.88xVfGpopoy_gDcApmL9MqAKSsylsBg5ksnQO90ugLc2Q74SRp4JsHmlLNR', @NOW,NULL);

				SELECT @ProductSettingID=SCOPE_IDENTITY();
			END
		ELSE
			BEGIN
				UPDATE Enterprise.ProductSetting 
				SET ProductId = 14, ProductSettingTypeId=1013, Value='3MVG9Oe7T3Ol0ea4bdh_hA1xr.88xVfGpopoy_gDcApmL9MqAKSsylsBg5ksnQO90ugLc2Q74SRp4JsHmlLNR',FromDate=@NOW,ThruDate = NULL
				WHERE ProductSettingId = @ProductSettingID
			END

	SELECT @ProductConfigurationID = ProductConfigurationId FROM Enterprise.ProductConfiguration WHERE ConfigurationId = @GlobalProductConfigurationID AND ProductSettingId = @ProductSettingID
		IF @ProductConfigurationID IS NULL
			BEGIN
				INSERT INTO Enterprise.ProductConfiguration(ConfigurationId,ProductSettingId,FromDate,ThruDate)
				VALUES(@GlobalProductConfigurationID,@ProductSettingID,@NOW,NULL)
				SELECT @ProductConfigurationID = SCOPE_IDENTITY();
			END
			ELSE
			BEGIN
				UPDATE Enterprise.ProductConfiguration SET
				ConfigurationId = @GlobalProductConfigurationID, ProductSettingId = @ProductSettingID,FromDate = @NOW,ThruDate = NULL
				WHERE ProductSettingId = @ProductSettingID
			END

	--------------------------------------------
	SET @ProductSettingID  = NULL
	SET @ProductConfigurationID  = NULL

	SELECT @ProductSettingID = ProductSettingId FROM Enterprise.ProductSetting WHERE ProductId=14 AND ProductSettingTypeId=1021
		IF @productSettingID IS NULL
			BEGIN
				INSERT INTO Enterprise.ProductSetting ([ProductId],[ProductSettingTypeId], [Value],[FromDate],[ThruDate]) 
				VALUES (14, 1021, '3406521381523977224', @NOW,NULL);

				SELECT @ProductSettingID=SCOPE_IDENTITY();
			END
		ELSE
			BEGIN
				UPDATE Enterprise.ProductSetting 
				SET ProductId = 14, ProductSettingTypeId=1021, Value='3406521381523977224',FromDate=@NOW,ThruDate = NULL
				WHERE ProductSettingId = @ProductSettingID
			END

	SELECT @ProductConfigurationID = ProductConfigurationId FROM Enterprise.ProductConfiguration WHERE ConfigurationId = @GlobalProductConfigurationID AND ProductSettingId = @ProductSettingID
		IF @ProductConfigurationID IS NULL
			BEGIN
				INSERT INTO Enterprise.ProductConfiguration(ConfigurationId,ProductSettingId,FromDate,ThruDate)
				VALUES(@GlobalProductConfigurationID,@ProductSettingID,@NOW,NULL)
				SELECT @ProductConfigurationID = SCOPE_IDENTITY();
			END
			ELSE
			BEGIN
				UPDATE Enterprise.ProductConfiguration SET
				ConfigurationId = @GlobalProductConfigurationID, ProductSettingId = @ProductSettingID,FromDate = @NOW,ThruDate = NULL
				WHERE ProductSettingId = @ProductSettingID
			END

	--------------------------------------------
	SET @ProductSettingID  = NULL
	SET @ProductConfigurationID  = NULL

	SELECT @ProductSettingID = ProductSettingId FROM Enterprise.ProductSetting WHERE ProductId=14 AND ProductSettingTypeId=1022
		IF @productSettingID IS NULL
			BEGIN
				INSERT INTO Enterprise.ProductSetting ([ProductId],[ProductSettingTypeId], [Value],[FromDate],[ThruDate]) 
				VALUES (14, 1022, 'WD8ww46ewe97a13krCx69wUCC', @NOW,NULL);

				SELECT @ProductSettingID=SCOPE_IDENTITY();
			END
		ELSE
			BEGIN
				UPDATE Enterprise.ProductSetting 
				SET ProductId = 14, ProductSettingTypeId=1022, Value='WD8ww46ewe97a13krCx69wUCC',FromDate=@NOW,ThruDate = NULL
				WHERE ProductSettingId = @ProductSettingID
			END

	SELECT @ProductConfigurationID = ProductConfigurationId FROM Enterprise.ProductConfiguration WHERE ConfigurationId = @GlobalProductConfigurationID AND ProductSettingId = @ProductSettingID
		IF @ProductConfigurationID IS NULL
			BEGIN
				INSERT INTO Enterprise.ProductConfiguration(ConfigurationId,ProductSettingId,FromDate,ThruDate)
				VALUES(@GlobalProductConfigurationID,@ProductSettingID,@NOW,NULL)
				SELECT @ProductConfigurationID = SCOPE_IDENTITY();
			END
			ELSE
			BEGIN
				UPDATE Enterprise.ProductConfiguration SET
				ConfigurationId = @GlobalProductConfigurationID, ProductSettingId = @ProductSettingID,FromDate = @NOW,ThruDate = NULL
				WHERE ProductSettingId = @ProductSettingID
			END

	--------------------------------------------
	SET @ProductSettingID  = NULL
	SET @ProductConfigurationID  = NULL

	SELECT @ProductSettingID = ProductSettingId FROM Enterprise.ProductSetting WHERE ProductId=14 AND ProductSettingTypeId=1023
		IF @productSettingID IS NULL
			BEGIN
				INSERT INTO Enterprise.ProductSetting ([ProductId],[ProductSettingTypeId], [Value],[FromDate],[ThruDate]) 
				VALUES (14, 1023, '/services/data/v39.0/', @NOW,NULL);

				SELECT @ProductSettingID=SCOPE_IDENTITY();
			END
		ELSE
			BEGIN
				UPDATE Enterprise.ProductSetting 
				SET ProductId = 14, ProductSettingTypeId=1023, Value='/services/data/v39.0/',FromDate=@NOW,ThruDate = NULL
				WHERE ProductSettingId = @ProductSettingID
			END

	SELECT @ProductConfigurationID = ProductConfigurationId FROM Enterprise.ProductConfiguration WHERE ConfigurationId = @GlobalProductConfigurationID AND ProductSettingId = @ProductSettingID
		IF @ProductConfigurationID IS NULL
			BEGIN
				INSERT INTO Enterprise.ProductConfiguration(ConfigurationId,ProductSettingId,FromDate,ThruDate)
				VALUES(@GlobalProductConfigurationID,@ProductSettingID,@NOW,NULL)
				SELECT @ProductConfigurationID = SCOPE_IDENTITY();
			END
			ELSE
			BEGIN
				UPDATE Enterprise.ProductConfiguration SET
				ConfigurationId = @GlobalProductConfigurationID, ProductSettingId = @ProductSettingID,FromDate = @NOW,ThruDate = NULL
				WHERE ProductSettingId = @ProductSettingID
			END

	--------------------------------------------
	SET @ProductSettingID  = NULL
	SET @ProductConfigurationID  = NULL

	SELECT @ProductSettingID = ProductSettingId FROM Enterprise.ProductSetting WHERE ProductId=14 AND ProductSettingTypeId=1024
		IF @productSettingID IS NULL
			BEGIN
				INSERT INTO Enterprise.ProductSetting ([ProductId],[ProductSettingTypeId], [Value],[FromDate],[ThruDate]) 
				VALUES (14, 1024, 'https://test.salesforce.com/services/oauth2/token', @NOW,NULL);

				SELECT @ProductSettingID=SCOPE_IDENTITY();
			END
		ELSE
			BEGIN
				UPDATE Enterprise.ProductSetting 
				SET ProductId = 14, ProductSettingTypeId=1024, Value='https://test.salesforce.com/services/oauth2/token',FromDate=@NOW,ThruDate = NULL			
				WHERE ProductSettingId = @ProductSettingID
			END

	SELECT @ProductConfigurationID = ProductConfigurationId FROM Enterprise.ProductConfiguration WHERE ConfigurationId = @GlobalProductConfigurationID AND ProductSettingId = @ProductSettingID
		IF @ProductConfigurationID IS NULL
			BEGIN
				INSERT INTO Enterprise.ProductConfiguration(ConfigurationId,ProductSettingId,FromDate,ThruDate)
				VALUES(@GlobalProductConfigurationID,@ProductSettingID,@NOW,NULL)
				SELECT @ProductConfigurationID = SCOPE_IDENTITY();
			END
			ELSE
			BEGIN
				UPDATE Enterprise.ProductConfiguration SET
				ConfigurationId = @GlobalProductConfigurationID, ProductSettingId = @ProductSettingID,FromDate = @NOW,ThruDate = NULL
				WHERE ProductSettingId = @ProductSettingID
			END

	--------------------------------------------
	SET @ProductSettingID  = NULL
	SET @ProductConfigurationID  = NULL

	SELECT @ProductSettingID = ProductSettingId FROM Enterprise.ProductSetting WHERE ProductId=14 AND ProductSettingTypeId=1025
		IF @productSettingID IS NULL
			BEGIN
				INSERT INTO Enterprise.ProductSetting ([ProductId],[ProductSettingTypeId], [Value],[FromDate],[ThruDate]) 
				VALUES (14, 1025, '060000000005YDy', @NOW,NULL);

				SELECT @ProductSettingID=SCOPE_IDENTITY();
			END
		ELSE
			BEGIN
				UPDATE Enterprise.ProductSetting 
				SET ProductId = 14, ProductSettingTypeId=1025, Value='060000000005YDy',FromDate=@NOW,ThruDate = NULL
				WHERE ProductSettingId = @ProductSettingID
			END

	SELECT @ProductConfigurationID = ProductConfigurationId FROM Enterprise.ProductConfiguration WHERE ConfigurationId = @GlobalProductConfigurationID AND ProductSettingId = @ProductSettingID
		IF @ProductConfigurationID IS NULL
			BEGIN
				INSERT INTO Enterprise.ProductConfiguration(ConfigurationId,ProductSettingId,FromDate,ThruDate)
				VALUES(@GlobalProductConfigurationID,@ProductSettingID,@NOW,NULL)
				SELECT @ProductConfigurationID = SCOPE_IDENTITY();
			END
			ELSE
			BEGIN
				UPDATE Enterprise.ProductConfiguration SET
				ConfigurationId = @GlobalProductConfigurationID, ProductSettingId = @ProductSettingID,FromDate = @NOW,ThruDate = NULL
				WHERE ProductSettingId = @ProductSettingID
			END

	----------------------------------------------
	SET @ProductSettingID  = NULL
	SET @ProductConfigurationID  = NULL

	SELECT @ProductSettingID = ProductSettingId FROM Enterprise.ProductSetting WHERE ProductId=14 AND ProductSettingTypeId=1026
		IF @productSettingID IS NULL
			BEGIN
				INSERT INTO Enterprise.ProductSetting ([ProductId],[ProductSettingTypeId], [Value],[FromDate],[ThruDate]) 
				VALUES (14, 1026, '00D290000000XyT', @NOW,NULL);

				SELECT @ProductSettingID=SCOPE_IDENTITY();
			END
		ELSE
			BEGIN
				UPDATE Enterprise.ProductSetting 
				SET ProductId = 14, ProductSettingTypeId=1026, Value='00D290000000XyT',FromDate=@NOW,ThruDate = NULL
				WHERE ProductSettingId = @ProductSettingID
			END

	SELECT @ProductConfigurationID = ProductConfigurationId FROM Enterprise.ProductConfiguration WHERE ConfigurationId = @GlobalProductConfigurationID AND ProductSettingId = @ProductSettingID
		IF @ProductConfigurationID IS NULL
			BEGIN
				INSERT INTO Enterprise.ProductConfiguration(ConfigurationId,ProductSettingId,FromDate,ThruDate)
				VALUES(@GlobalProductConfigurationID,@ProductSettingID,@NOW,NULL)
				SELECT @ProductConfigurationID = SCOPE_IDENTITY();
			END
			ELSE
			BEGIN
				UPDATE Enterprise.ProductConfiguration SET
				ConfigurationId = @GlobalProductConfigurationID, ProductSettingId = @ProductSettingID,FromDate = @NOW,ThruDate = NULL
				WHERE ProductSettingId = @ProductSettingID
			END


	Print 'Adding new values to SamlProductSettings'

	IF (SELECT COUNT(*) FROM Ident.SamlProductSettings WHERE ProductId = 14 and SubjectIdSamlAttribute = 'productUsername') > 1
	BEGIN
		Print 'Dropping Duplicates From SamlProductSettings'
		DELETE Ident.SamlProductSettings
		WHERE ProductId = 14 and SubjectIdSamlAttribute = 'productUsername'
	END

	SELECT @NewSamlProductSettings = [SamlProductSettingsId] 
		FROM Ident.SamlProductSettings 
		WHERE ProductId = 14 and SubjectIdSamlAttribute = 'productUsername'

	IF @NewSamlProductSettings IS NULL
	BEGIN
		Print 'Adding New SamlProductSetting'
		EXEC [Ident].[CreateSamlProductSetting]
			 @ProductId = 14
			,@LoginUri = 'https://realpage--RPIUAT.cs12.my.salesforce.com?so=00DV0000007zGCk'
			,@SigningCertificateThumbprint = '0BE4C7B686D1918A4B2B571E8BF098B994990CAB'
			,@SubjectIdSamlAttribute = 'productUsername'
			,@SamlProductSettingsId = @NewSamlProductSettings	OUTPUT
		END
	ELSE
	BEGIN
	
		Print 'Updating SamlProductSetting'
		UPDATE Ident.SamlProductSettings SET 
			 [LoginUri] = 'https://realpage--RPIUAT.cs12.my.salesforce.com?so=00DV0000007zGCk'
			,[SigningCertificateThumbprint] = '0BE4C7B686D1918A4B2B571E8BF098B994990CAB'
		WHERE ProductId = 14 AND SubjectIdSamlAttribute = 'productUsername'	
	END
END
--=================================================================================================================================================================================
--=================================================================================================================================================================================
IF @@SERVERNAME = 'RCQUSODBSQL001'
BEGIN
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

	SELECT @GlobalProductConfigurationID = ConfigurationId FROM Enterprise.GlobalProductConfiguration WHERE ProductId = 14

	------------------------------------------
	SELECT @ProductSettingID = ProductSettingId FROM Enterprise.ProductSetting WHERE ProductId=14 AND ProductSettingTypeId=1010
		IF @productSettingID IS NULL
			BEGIN
				INSERT INTO Enterprise.ProductSetting ([ProductId],[ProductSettingTypeId], [Value],[FromDate],[ThruDate]) 
				VALUES (14, 1010, 'greenbookapi@realpage.com.rpiuat', @NOW,NULL);

				SELECT @ProductSettingID=SCOPE_IDENTITY();
			END
		ELSE
			BEGIN
				UPDATE Enterprise.ProductSetting 
				SET ProductId = 14, ProductSettingTypeId=1010, Value='greenbookapi@realpage.com.rpiuat',FromDate=@NOW,ThruDate = NULL
				WHERE ProductSettingId = @ProductSettingID
			END

	SELECT @ProductConfigurationID = ProductConfigurationId FROM Enterprise.ProductConfiguration WHERE ConfigurationId = @GlobalProductConfigurationID AND ProductSettingId = @ProductSettingID
		IF @ProductConfigurationID IS NULL
			BEGIN
				INSERT INTO Enterprise.ProductConfiguration(ConfigurationId,ProductSettingId,FromDate,ThruDate)
				VALUES(@GlobalProductConfigurationID,@ProductSettingID,@NOW,NULL)
				SELECT @ProductConfigurationID = SCOPE_IDENTITY();
			END
			ELSE
			BEGIN
				UPDATE Enterprise.ProductConfiguration SET
				ConfigurationId = @GlobalProductConfigurationID, ProductSettingId = @ProductSettingID,FromDate = @NOW,ThruDate = NULL
				WHERE ProductSettingId = @ProductSettingID
			END

	--------------------------------------------
	SET @ProductSettingID  = NULL
	SET @ProductConfigurationID  = NULL

	SELECT @ProductSettingID = ProductSettingId FROM Enterprise.ProductSetting WHERE ProductId=14 AND ProductSettingTypeId=1011
		IF @productSettingID IS NULL
			BEGIN
				INSERT INTO Enterprise.ProductSetting ([ProductId],[ProductSettingTypeId], [Value],[FromDate],[ThruDate]) 
				VALUES (14, 1011, 'GreenB0%kU@t', @NOW,NULL);

				SELECT @ProductSettingID=SCOPE_IDENTITY();
			END
		ELSE
			BEGIN
				UPDATE Enterprise.ProductSetting 
				SET ProductId = 14, ProductSettingTypeId=1011, Value='GreenB0%kU@t',FromDate=@NOW,ThruDate = NULL
				WHERE ProductSettingId = @ProductSettingID
			END

	SELECT @ProductConfigurationID = ProductConfigurationId FROM Enterprise.ProductConfiguration WHERE ConfigurationId = @GlobalProductConfigurationID AND ProductSettingId = @ProductSettingID
		IF @ProductConfigurationID IS NULL
			BEGIN
				INSERT INTO Enterprise.ProductConfiguration(ConfigurationId,ProductSettingId,FromDate,ThruDate)
				VALUES(@GlobalProductConfigurationID,@ProductSettingID,@NOW,NULL)
				SELECT @ProductConfigurationID = SCOPE_IDENTITY();
			END
			ELSE
			BEGIN
				UPDATE Enterprise.ProductConfiguration SET
				ConfigurationId = @GlobalProductConfigurationID, ProductSettingId = @ProductSettingID,FromDate = @NOW,ThruDate = NULL
				WHERE ProductSettingId = @ProductSettingID
			END

	--------------------------------------------
	SET @ProductSettingID  = NULL
	SET @ProductConfigurationID  = NULL

	SELECT @ProductSettingID = ProductSettingId FROM Enterprise.ProductSetting WHERE ProductId=14 AND ProductSettingTypeId=1012
		IF @productSettingID IS NULL
			BEGIN
				INSERT INTO Enterprise.ProductSetting ([ProductId],[ProductSettingTypeId], [Value],[FromDate],[ThruDate]) 
				VALUES (14, 1012, 'https://realpage--RPIUAT.cs12.my.salesforce.com?so=00DV0000007zGCk', @NOW,NULL);

				SELECT @ProductSettingID=SCOPE_IDENTITY();
			END
		ELSE
			BEGIN
				UPDATE Enterprise.ProductSetting 
				SET ProductId = 14, ProductSettingTypeId=1012, Value='https://realpage--RPIUAT.cs12.my.salesforce.com?so=00DV0000007zGCk',FromDate=@NOW,ThruDate = NULL
				WHERE ProductSettingId = @ProductSettingID
			END

	SELECT @ProductConfigurationID = ProductConfigurationId FROM Enterprise.ProductConfiguration WHERE ConfigurationId = @GlobalProductConfigurationID AND ProductSettingId = @ProductSettingID
		IF @ProductConfigurationID IS NULL
			BEGIN
				INSERT INTO Enterprise.ProductConfiguration(ConfigurationId,ProductSettingId,FromDate,ThruDate)
				VALUES(@GlobalProductConfigurationID,@ProductSettingID,@NOW,NULL)
				SELECT @ProductConfigurationID = SCOPE_IDENTITY();
			END
			ELSE
			BEGIN
				UPDATE Enterprise.ProductConfiguration SET
				ConfigurationId = @GlobalProductConfigurationID, ProductSettingId = @ProductSettingID,FromDate = @NOW,ThruDate = NULL
				WHERE ProductSettingId = @ProductSettingID
			END

	--------------------------------------------
	SET @ProductSettingID  = NULL
	SET @ProductConfigurationID  = NULL

	SELECT @ProductSettingID = ProductSettingId FROM Enterprise.ProductSetting WHERE ProductId=14 AND ProductSettingTypeId=1013
		IF @productSettingID IS NULL
			BEGIN
				INSERT INTO Enterprise.ProductSetting ([ProductId],[ProductSettingTypeId], [Value],[FromDate],[ThruDate]) 
				VALUES (14, 1013, '3MVG9Oe7T3Ol0ea4bdh_hA1xr.88xVfGpopoy_gDcApmL9MqAKSsylsBg5ksnQO90ugLc2Q74SRp4JsHmlLNR', @NOW,NULL);

				SELECT @ProductSettingID=SCOPE_IDENTITY();
			END
		ELSE
			BEGIN
				UPDATE Enterprise.ProductSetting 
				SET ProductId = 14, ProductSettingTypeId=1013, Value='3MVG9Oe7T3Ol0ea4bdh_hA1xr.88xVfGpopoy_gDcApmL9MqAKSsylsBg5ksnQO90ugLc2Q74SRp4JsHmlLNR',FromDate=@NOW,ThruDate = NULL
				WHERE ProductSettingId = @ProductSettingID
			END

	SELECT @ProductConfigurationID = ProductConfigurationId FROM Enterprise.ProductConfiguration WHERE ConfigurationId = @GlobalProductConfigurationID AND ProductSettingId = @ProductSettingID
		IF @ProductConfigurationID IS NULL
			BEGIN
				INSERT INTO Enterprise.ProductConfiguration(ConfigurationId,ProductSettingId,FromDate,ThruDate)
				VALUES(@GlobalProductConfigurationID,@ProductSettingID,@NOW,NULL)
				SELECT @ProductConfigurationID = SCOPE_IDENTITY();
			END
			ELSE
			BEGIN
				UPDATE Enterprise.ProductConfiguration SET
				ConfigurationId = @GlobalProductConfigurationID, ProductSettingId = @ProductSettingID,FromDate = @NOW,ThruDate = NULL
				WHERE ProductSettingId = @ProductSettingID
			END

	--------------------------------------------
	SET @ProductSettingID  = NULL
	SET @ProductConfigurationID  = NULL

	SELECT @ProductSettingID = ProductSettingId FROM Enterprise.ProductSetting WHERE ProductId=14 AND ProductSettingTypeId=1021
		IF @productSettingID IS NULL
			BEGIN
				INSERT INTO Enterprise.ProductSetting ([ProductId],[ProductSettingTypeId], [Value],[FromDate],[ThruDate]) 
				VALUES (14, 1021, '3406521381523977224', @NOW,NULL);

				SELECT @ProductSettingID=SCOPE_IDENTITY();
			END
		ELSE
			BEGIN
				UPDATE Enterprise.ProductSetting 
				SET ProductId = 14, ProductSettingTypeId=1021, Value='3406521381523977224',FromDate=@NOW,ThruDate = NULL
				WHERE ProductSettingId = @ProductSettingID
			END

	SELECT @ProductConfigurationID = ProductConfigurationId FROM Enterprise.ProductConfiguration WHERE ConfigurationId = @GlobalProductConfigurationID AND ProductSettingId = @ProductSettingID
		IF @ProductConfigurationID IS NULL
			BEGIN
				INSERT INTO Enterprise.ProductConfiguration(ConfigurationId,ProductSettingId,FromDate,ThruDate)
				VALUES(@GlobalProductConfigurationID,@ProductSettingID,@NOW,NULL)
				SELECT @ProductConfigurationID = SCOPE_IDENTITY();
			END
			ELSE
			BEGIN
				UPDATE Enterprise.ProductConfiguration SET
				ConfigurationId = @GlobalProductConfigurationID, ProductSettingId = @ProductSettingID,FromDate = @NOW,ThruDate = NULL
				WHERE ProductSettingId = @ProductSettingID
			END

	--------------------------------------------
	SET @ProductSettingID  = NULL
	SET @ProductConfigurationID  = NULL

	SELECT @ProductSettingID = ProductSettingId FROM Enterprise.ProductSetting WHERE ProductId=14 AND ProductSettingTypeId=1022
		IF @productSettingID IS NULL
			BEGIN
				INSERT INTO Enterprise.ProductSetting ([ProductId],[ProductSettingTypeId], [Value],[FromDate],[ThruDate]) 
				VALUES (14, 1022, 'WD8ww46ewe97a13krCx69wUCC', @NOW,NULL);

				SELECT @ProductSettingID=SCOPE_IDENTITY();
			END
		ELSE
			BEGIN
				UPDATE Enterprise.ProductSetting 
				SET ProductId = 14, ProductSettingTypeId=1022, Value='WD8ww46ewe97a13krCx69wUCC',FromDate=@NOW,ThruDate = NULL
				WHERE ProductSettingId = @ProductSettingID
			END

	SELECT @ProductConfigurationID = ProductConfigurationId FROM Enterprise.ProductConfiguration WHERE ConfigurationId = @GlobalProductConfigurationID AND ProductSettingId = @ProductSettingID
		IF @ProductConfigurationID IS NULL
			BEGIN
				INSERT INTO Enterprise.ProductConfiguration(ConfigurationId,ProductSettingId,FromDate,ThruDate)
				VALUES(@GlobalProductConfigurationID,@ProductSettingID,@NOW,NULL)
				SELECT @ProductConfigurationID = SCOPE_IDENTITY();
			END
			ELSE
			BEGIN
				UPDATE Enterprise.ProductConfiguration SET
				ConfigurationId = @GlobalProductConfigurationID, ProductSettingId = @ProductSettingID,FromDate = @NOW,ThruDate = NULL
				WHERE ProductSettingId = @ProductSettingID
			END

	--------------------------------------------
	SET @ProductSettingID  = NULL
	SET @ProductConfigurationID  = NULL

	SELECT @ProductSettingID = ProductSettingId FROM Enterprise.ProductSetting WHERE ProductId=14 AND ProductSettingTypeId=1023
		IF @productSettingID IS NULL
			BEGIN
				INSERT INTO Enterprise.ProductSetting ([ProductId],[ProductSettingTypeId], [Value],[FromDate],[ThruDate]) 
				VALUES (14, 1023, '/services/data/v39.0/', @NOW,NULL);

				SELECT @ProductSettingID=SCOPE_IDENTITY();
			END
		ELSE
			BEGIN
				UPDATE Enterprise.ProductSetting 
				SET ProductId = 14, ProductSettingTypeId=1023, Value='/services/data/v39.0/',FromDate=@NOW,ThruDate = NULL
				WHERE ProductSettingId = @ProductSettingID
			END

	SELECT @ProductConfigurationID = ProductConfigurationId FROM Enterprise.ProductConfiguration WHERE ConfigurationId = @GlobalProductConfigurationID AND ProductSettingId = @ProductSettingID
		IF @ProductConfigurationID IS NULL
			BEGIN
				INSERT INTO Enterprise.ProductConfiguration(ConfigurationId,ProductSettingId,FromDate,ThruDate)
				VALUES(@GlobalProductConfigurationID,@ProductSettingID,@NOW,NULL)
				SELECT @ProductConfigurationID = SCOPE_IDENTITY();
			END
			ELSE
			BEGIN
				UPDATE Enterprise.ProductConfiguration SET
				ConfigurationId = @GlobalProductConfigurationID, ProductSettingId = @ProductSettingID,FromDate = @NOW,ThruDate = NULL
				WHERE ProductSettingId = @ProductSettingID
			END

	--------------------------------------------
	SET @ProductSettingID  = NULL
	SET @ProductConfigurationID  = NULL

	SELECT @ProductSettingID = ProductSettingId FROM Enterprise.ProductSetting WHERE ProductId=14 AND ProductSettingTypeId=1024
		IF @productSettingID IS NULL
			BEGIN
				INSERT INTO Enterprise.ProductSetting ([ProductId],[ProductSettingTypeId], [Value],[FromDate],[ThruDate]) 
				VALUES (14, 1024, 'https://test.salesforce.com/services/oauth2/token', @NOW,NULL);

				SELECT @ProductSettingID=SCOPE_IDENTITY();
			END
		ELSE
			BEGIN
				UPDATE Enterprise.ProductSetting 
				SET ProductId = 14, ProductSettingTypeId=1024, Value='https://test.salesforce.com/services/oauth2/token',FromDate=@NOW,ThruDate = NULL			
				WHERE ProductSettingId = @ProductSettingID
			END

	SELECT @ProductConfigurationID = ProductConfigurationId FROM Enterprise.ProductConfiguration WHERE ConfigurationId = @GlobalProductConfigurationID AND ProductSettingId = @ProductSettingID
		IF @ProductConfigurationID IS NULL
			BEGIN
				INSERT INTO Enterprise.ProductConfiguration(ConfigurationId,ProductSettingId,FromDate,ThruDate)
				VALUES(@GlobalProductConfigurationID,@ProductSettingID,@NOW,NULL)
				SELECT @ProductConfigurationID = SCOPE_IDENTITY();
			END
			ELSE
			BEGIN
				UPDATE Enterprise.ProductConfiguration SET
				ConfigurationId = @GlobalProductConfigurationID, ProductSettingId = @ProductSettingID,FromDate = @NOW,ThruDate = NULL
				WHERE ProductSettingId = @ProductSettingID
			END

	--------------------------------------------
	SET @ProductSettingID  = NULL
	SET @ProductConfigurationID  = NULL

	SELECT @ProductSettingID = ProductSettingId FROM Enterprise.ProductSetting WHERE ProductId=14 AND ProductSettingTypeId=1025
		IF @productSettingID IS NULL
			BEGIN
				INSERT INTO Enterprise.ProductSetting ([ProductId],[ProductSettingTypeId], [Value],[FromDate],[ThruDate]) 
				VALUES (14, 1025, '060000000005YDy', @NOW,NULL);

				SELECT @ProductSettingID=SCOPE_IDENTITY();
			END
		ELSE
			BEGIN
				UPDATE Enterprise.ProductSetting 
				SET ProductId = 14, ProductSettingTypeId=1025, Value='060000000005YDy',FromDate=@NOW,ThruDate = NULL
				WHERE ProductSettingId = @ProductSettingID
			END

	SELECT @ProductConfigurationID = ProductConfigurationId FROM Enterprise.ProductConfiguration WHERE ConfigurationId = @GlobalProductConfigurationID AND ProductSettingId = @ProductSettingID
		IF @ProductConfigurationID IS NULL
			BEGIN
				INSERT INTO Enterprise.ProductConfiguration(ConfigurationId,ProductSettingId,FromDate,ThruDate)
				VALUES(@GlobalProductConfigurationID,@ProductSettingID,@NOW,NULL)
				SELECT @ProductConfigurationID = SCOPE_IDENTITY();
			END
			ELSE
			BEGIN
				UPDATE Enterprise.ProductConfiguration SET
				ConfigurationId = @GlobalProductConfigurationID, ProductSettingId = @ProductSettingID,FromDate = @NOW,ThruDate = NULL
				WHERE ProductSettingId = @ProductSettingID
			END

	----------------------------------------------
	SET @ProductSettingID  = NULL
	SET @ProductConfigurationID  = NULL

	SELECT @ProductSettingID = ProductSettingId FROM Enterprise.ProductSetting WHERE ProductId=14 AND ProductSettingTypeId=1026
		IF @productSettingID IS NULL
			BEGIN
				INSERT INTO Enterprise.ProductSetting ([ProductId],[ProductSettingTypeId], [Value],[FromDate],[ThruDate]) 
				VALUES (14, 1026, '00D290000000XyT', @NOW,NULL);

				SELECT @ProductSettingID=SCOPE_IDENTITY();
			END
		ELSE
			BEGIN
				UPDATE Enterprise.ProductSetting 
				SET ProductId = 14, ProductSettingTypeId=1026, Value='00D290000000XyT',FromDate=@NOW,ThruDate = NULL
				WHERE ProductSettingId = @ProductSettingID
			END

	SELECT @ProductConfigurationID = ProductConfigurationId FROM Enterprise.ProductConfiguration WHERE ConfigurationId = @GlobalProductConfigurationID AND ProductSettingId = @ProductSettingID
		IF @ProductConfigurationID IS NULL
			BEGIN
				INSERT INTO Enterprise.ProductConfiguration(ConfigurationId,ProductSettingId,FromDate,ThruDate)
				VALUES(@GlobalProductConfigurationID,@ProductSettingID,@NOW,NULL)
				SELECT @ProductConfigurationID = SCOPE_IDENTITY();
			END
			ELSE
			BEGIN
				UPDATE Enterprise.ProductConfiguration SET
				ConfigurationId = @GlobalProductConfigurationID, ProductSettingId = @ProductSettingID,FromDate = @NOW,ThruDate = NULL
				WHERE ProductSettingId = @ProductSettingID
			END


	Print 'Adding new values to SamlProductSettings'

	IF (SELECT COUNT(*) FROM Ident.SamlProductSettings WHERE ProductId = 14 and SubjectIdSamlAttribute = 'productUsername') > 1
	BEGIN
		Print 'Dropping Duplicates From SamlProductSettings'
		DELETE Ident.SamlProductSettings
		WHERE ProductId = 14 and SubjectIdSamlAttribute = 'productUsername'
	END

	SELECT @NewSamlProductSettings = [SamlProductSettingsId] 
		FROM Ident.SamlProductSettings 
		WHERE ProductId = 14 and SubjectIdSamlAttribute = 'productUsername'

	IF @NewSamlProductSettings IS NULL
	BEGIN
		Print 'Adding New SamlProductSetting'
		EXEC [Ident].[CreateSamlProductSetting]
			 @ProductId = 14
			,@LoginUri = 'https://realpage--RPIUAT.cs12.my.salesforce.com?so=00DV0000007zGCk'
			,@SigningCertificateThumbprint = '0BE4C7B686D1918A4B2B571E8BF098B994990CAB'
			,@SubjectIdSamlAttribute = 'productUsername'
			,@SamlProductSettingsId = @NewSamlProductSettings	OUTPUT
		END
	ELSE
	BEGIN
	
		Print 'Updating SamlProductSetting'
		UPDATE Ident.SamlProductSettings SET 
			 [LoginUri] = 'https://realpage--RPIUAT.cs12.my.salesforce.com?so=00DV0000007zGCk'
			,[SigningCertificateThumbprint] = '0BE4C7B686D1918A4B2B571E8BF098B994990CAB'
		WHERE ProductId = 14 AND SubjectIdSamlAttribute = 'productUsername'	
	END
END
--=================================================================================================================================================================================
--=================================================================================================================================================================================
IF @@SERVERNAME = ' RCPGBKDBSQL005A'
BEGIN
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

	SELECT @GlobalProductConfigurationID = ConfigurationId FROM Enterprise.GlobalProductConfiguration WHERE ProductId = 14

	------------------------------------------
	SELECT @ProductSettingID = ProductSettingId FROM Enterprise.ProductSetting WHERE ProductId=14 AND ProductSettingTypeId=1010
		IF @productSettingID IS NULL
			BEGIN
				INSERT INTO Enterprise.ProductSetting ([ProductId],[ProductSettingTypeId], [Value],[FromDate],[ThruDate]) 
				VALUES (14, 1010, 'ajit.mungale@realpage.com.rpidev', @NOW,NULL);

				SELECT @ProductSettingID=SCOPE_IDENTITY();
			END
		ELSE
			BEGIN
				UPDATE Enterprise.ProductSetting 
				SET ProductId = 14, ProductSettingTypeId=1010, Value='ajit.mungale@realpage.com.rpidev',FromDate=@NOW,ThruDate = NULL
				WHERE ProductSettingId = @ProductSettingID
			END

	SELECT @ProductConfigurationID = ProductConfigurationId FROM Enterprise.ProductConfiguration WHERE ConfigurationId = @GlobalProductConfigurationID AND ProductSettingId = @ProductSettingID
		IF @ProductConfigurationID IS NULL
			BEGIN
				INSERT INTO Enterprise.ProductConfiguration(ConfigurationId,ProductSettingId,FromDate,ThruDate)
				VALUES(@GlobalProductConfigurationID,@ProductSettingID,@NOW,NULL)
				SELECT @ProductConfigurationID = SCOPE_IDENTITY();
			END
			ELSE
			BEGIN
				UPDATE Enterprise.ProductConfiguration SET
				ConfigurationId = @GlobalProductConfigurationID, ProductSettingId = @ProductSettingID,FromDate = @NOW,ThruDate = NULL
				WHERE ProductSettingId = @ProductSettingID
			END

	--------------------------------------------
	SET @ProductSettingID  = NULL
	SET @ProductConfigurationID  = NULL

	SELECT @ProductSettingID = ProductSettingId FROM Enterprise.ProductSetting WHERE ProductId=14 AND ProductSettingTypeId=1011
		IF @productSettingID IS NULL
			BEGIN
				INSERT INTO Enterprise.ProductSetting ([ProductId],[ProductSettingTypeId], [Value],[FromDate],[ThruDate]) 
				VALUES (14, 1011, 'P@ssw0rd1!', @NOW,NULL);

				SELECT @ProductSettingID=SCOPE_IDENTITY();
			END
		ELSE
			BEGIN
				UPDATE Enterprise.ProductSetting 
				SET ProductId = 14, ProductSettingTypeId=1011, Value='P@ssw0rd1!',FromDate=@NOW,ThruDate = NULL
				WHERE ProductSettingId = @ProductSettingID
			END

	SELECT @ProductConfigurationID = ProductConfigurationId FROM Enterprise.ProductConfiguration WHERE ConfigurationId = @GlobalProductConfigurationID AND ProductSettingId = @ProductSettingID
		IF @ProductConfigurationID IS NULL
			BEGIN
				INSERT INTO Enterprise.ProductConfiguration(ConfigurationId,ProductSettingId,FromDate,ThruDate)
				VALUES(@GlobalProductConfigurationID,@ProductSettingID,@NOW,NULL)
				SELECT @ProductConfigurationID = SCOPE_IDENTITY();
			END
			ELSE
			BEGIN
				UPDATE Enterprise.ProductConfiguration SET
				ConfigurationId = @GlobalProductConfigurationID, ProductSettingId = @ProductSettingID,FromDate = @NOW,ThruDate = NULL
				WHERE ProductSettingId = @ProductSettingID
			END

	--------------------------------------------
	SET @ProductSettingID  = NULL
	SET @ProductConfigurationID  = NULL

	SELECT @ProductSettingID = ProductSettingId FROM Enterprise.ProductSetting WHERE ProductId=14 AND ProductSettingTypeId=1012
		IF @productSettingID IS NULL
			BEGIN
				INSERT INTO Enterprise.ProductSetting ([ProductId],[ProductSettingTypeId], [Value],[FromDate],[ThruDate]) 
				VALUES (14, 1012, 'https://realpage.my.salesforce.com?so=00D00000000heZB', @NOW,NULL);

				SELECT @ProductSettingID=SCOPE_IDENTITY();
			END
		ELSE
			BEGIN
				UPDATE Enterprise.ProductSetting 
				SET ProductId = 14, ProductSettingTypeId=1012, Value='https://realpage.my.salesforce.com?so=00D00000000heZB',FromDate=@NOW,ThruDate = NULL
				WHERE ProductSettingId = @ProductSettingID
			END

	SELECT @ProductConfigurationID = ProductConfigurationId FROM Enterprise.ProductConfiguration WHERE ConfigurationId = @GlobalProductConfigurationID AND ProductSettingId = @ProductSettingID
		IF @ProductConfigurationID IS NULL
			BEGIN
				INSERT INTO Enterprise.ProductConfiguration(ConfigurationId,ProductSettingId,FromDate,ThruDate)
				VALUES(@GlobalProductConfigurationID,@ProductSettingID,@NOW,NULL)
				SELECT @ProductConfigurationID = SCOPE_IDENTITY();
			END
			ELSE
			BEGIN
				UPDATE Enterprise.ProductConfiguration SET
				ConfigurationId = @GlobalProductConfigurationID, ProductSettingId = @ProductSettingID,FromDate = @NOW,ThruDate = NULL
				WHERE ProductSettingId = @ProductSettingID
			END

	--------------------------------------------
	SET @ProductSettingID  = NULL
	SET @ProductConfigurationID  = NULL

	SELECT @ProductSettingID = ProductSettingId FROM Enterprise.ProductSetting WHERE ProductId=14 AND ProductSettingTypeId=1013
		IF @productSettingID IS NULL
			BEGIN
				INSERT INTO Enterprise.ProductSetting ([ProductId],[ProductSettingTypeId], [Value],[FromDate],[ThruDate]) 
				VALUES (14, 1013, '3MVG9Vik22TUgUpgLpQA6UEwAc4MBq_xXoTjysYx2vNJq2nmmo.LXp0vP24FUq9WFyWn5UT0jXEuZxuSqZ3WA', @NOW,NULL);

				SELECT @ProductSettingID=SCOPE_IDENTITY();
			END
		ELSE
			BEGIN
				UPDATE Enterprise.ProductSetting 
				SET ProductId = 14, ProductSettingTypeId=1013, Value='3MVG9Vik22TUgUpgLpQA6UEwAc4MBq_xXoTjysYx2vNJq2nmmo.LXp0vP24FUq9WFyWn5UT0jXEuZxuSqZ3WA',FromDate=@NOW,ThruDate = NULL
				WHERE ProductSettingId = @ProductSettingID
			END

	SELECT @ProductConfigurationID = ProductConfigurationId FROM Enterprise.ProductConfiguration WHERE ConfigurationId = @GlobalProductConfigurationID AND ProductSettingId = @ProductSettingID
		IF @ProductConfigurationID IS NULL
			BEGIN
				INSERT INTO Enterprise.ProductConfiguration(ConfigurationId,ProductSettingId,FromDate,ThruDate)
				VALUES(@GlobalProductConfigurationID,@ProductSettingID,@NOW,NULL)
				SELECT @ProductConfigurationID = SCOPE_IDENTITY();
			END
			ELSE
			BEGIN
				UPDATE Enterprise.ProductConfiguration SET
				ConfigurationId = @GlobalProductConfigurationID, ProductSettingId = @ProductSettingID,FromDate = @NOW,ThruDate = NULL
				WHERE ProductSettingId = @ProductSettingID
			END

	--------------------------------------------
	SET @ProductSettingID  = NULL
	SET @ProductConfigurationID  = NULL

	SELECT @ProductSettingID = ProductSettingId FROM Enterprise.ProductSetting WHERE ProductId=14 AND ProductSettingTypeId=1021
		IF @productSettingID IS NULL
			BEGIN
				INSERT INTO Enterprise.ProductSetting ([ProductId],[ProductSettingTypeId], [Value],[FromDate],[ThruDate]) 
				VALUES (14, 1021, '7116286042861318785', @NOW,NULL);

				SELECT @ProductSettingID=SCOPE_IDENTITY();
			END
		ELSE
			BEGIN
				UPDATE Enterprise.ProductSetting 
				SET ProductId = 14, ProductSettingTypeId=1021, Value='7116286042861318785',FromDate=@NOW,ThruDate = NULL
				WHERE ProductSettingId = @ProductSettingID
			END

	SELECT @ProductConfigurationID = ProductConfigurationId FROM Enterprise.ProductConfiguration WHERE ConfigurationId = @GlobalProductConfigurationID AND ProductSettingId = @ProductSettingID
		IF @ProductConfigurationID IS NULL
			BEGIN
				INSERT INTO Enterprise.ProductConfiguration(ConfigurationId,ProductSettingId,FromDate,ThruDate)
				VALUES(@GlobalProductConfigurationID,@ProductSettingID,@NOW,NULL)
				SELECT @ProductConfigurationID = SCOPE_IDENTITY();
			END
			ELSE
			BEGIN
				UPDATE Enterprise.ProductConfiguration SET
				ConfigurationId = @GlobalProductConfigurationID, ProductSettingId = @ProductSettingID,FromDate = @NOW,ThruDate = NULL
				WHERE ProductSettingId = @ProductSettingID
			END

	--------------------------------------------
	SET @ProductSettingID  = NULL
	SET @ProductConfigurationID  = NULL

	SELECT @ProductSettingID = ProductSettingId FROM Enterprise.ProductSetting WHERE ProductId=14 AND ProductSettingTypeId=1022
		IF @productSettingID IS NULL
			BEGIN
				INSERT INTO Enterprise.ProductSetting ([ProductId],[ProductSettingTypeId], [Value],[FromDate],[ThruDate]) 
				VALUES (14, 1022, 'KMSEJNjjru5RTsmIB7tPEZ2S', @NOW,NULL);

				SELECT @ProductSettingID=SCOPE_IDENTITY();
			END
		ELSE
			BEGIN
				UPDATE Enterprise.ProductSetting 
				SET ProductId = 14, ProductSettingTypeId=1022, Value='KMSEJNjjru5RTsmIB7tPEZ2S',FromDate=@NOW,ThruDate = NULL
				WHERE ProductSettingId = @ProductSettingID
			END

	SELECT @ProductConfigurationID = ProductConfigurationId FROM Enterprise.ProductConfiguration WHERE ConfigurationId = @GlobalProductConfigurationID AND ProductSettingId = @ProductSettingID
		IF @ProductConfigurationID IS NULL
			BEGIN
				INSERT INTO Enterprise.ProductConfiguration(ConfigurationId,ProductSettingId,FromDate,ThruDate)
				VALUES(@GlobalProductConfigurationID,@ProductSettingID,@NOW,NULL)
				SELECT @ProductConfigurationID = SCOPE_IDENTITY();
			END
			ELSE
			BEGIN
				UPDATE Enterprise.ProductConfiguration SET
				ConfigurationId = @GlobalProductConfigurationID, ProductSettingId = @ProductSettingID,FromDate = @NOW,ThruDate = NULL
				WHERE ProductSettingId = @ProductSettingID
			END

	--------------------------------------------
	SET @ProductSettingID  = NULL
	SET @ProductConfigurationID  = NULL

	SELECT @ProductSettingID = ProductSettingId FROM Enterprise.ProductSetting WHERE ProductId=14 AND ProductSettingTypeId=1023
		IF @productSettingID IS NULL
			BEGIN
				INSERT INTO Enterprise.ProductSetting ([ProductId],[ProductSettingTypeId], [Value],[FromDate],[ThruDate]) 
				VALUES (14, 1023, '/services/data/v39.0/', @NOW,NULL);

				SELECT @ProductSettingID=SCOPE_IDENTITY();
			END
		ELSE
			BEGIN
				UPDATE Enterprise.ProductSetting 
				SET ProductId = 14, ProductSettingTypeId=1023, Value='/services/data/v39.0/',FromDate=@NOW,ThruDate = NULL
				WHERE ProductSettingId = @ProductSettingID
			END

	SELECT @ProductConfigurationID = ProductConfigurationId FROM Enterprise.ProductConfiguration WHERE ConfigurationId = @GlobalProductConfigurationID AND ProductSettingId = @ProductSettingID
		IF @ProductConfigurationID IS NULL
			BEGIN
				INSERT INTO Enterprise.ProductConfiguration(ConfigurationId,ProductSettingId,FromDate,ThruDate)
				VALUES(@GlobalProductConfigurationID,@ProductSettingID,@NOW,NULL)
				SELECT @ProductConfigurationID = SCOPE_IDENTITY();
			END
			ELSE
			BEGIN
				UPDATE Enterprise.ProductConfiguration SET
				ConfigurationId = @GlobalProductConfigurationID, ProductSettingId = @ProductSettingID,FromDate = @NOW,ThruDate = NULL
				WHERE ProductSettingId = @ProductSettingID
			END

	--------------------------------------------
	SET @ProductSettingID  = NULL
	SET @ProductConfigurationID  = NULL

	SELECT @ProductSettingID = ProductSettingId FROM Enterprise.ProductSetting WHERE ProductId=14 AND ProductSettingTypeId=1024
		IF @productSettingID IS NULL
			BEGIN
				INSERT INTO Enterprise.ProductSetting ([ProductId],[ProductSettingTypeId], [Value],[FromDate],[ThruDate]) 
				VALUES (14, 1024, 'https://test.salesforce.com/services/oauth2/token', @NOW,NULL);

				SELECT @ProductSettingID=SCOPE_IDENTITY();
			END
		ELSE
			BEGIN
				UPDATE Enterprise.ProductSetting 
				SET ProductId = 14, ProductSettingTypeId=1024, Value='https://test.salesforce.com/services/oauth2/token',FromDate=@NOW,ThruDate = NULL			
				WHERE ProductSettingId = @ProductSettingID
			END

	SELECT @ProductConfigurationID = ProductConfigurationId FROM Enterprise.ProductConfiguration WHERE ConfigurationId = @GlobalProductConfigurationID AND ProductSettingId = @ProductSettingID
		IF @ProductConfigurationID IS NULL
			BEGIN
				INSERT INTO Enterprise.ProductConfiguration(ConfigurationId,ProductSettingId,FromDate,ThruDate)
				VALUES(@GlobalProductConfigurationID,@ProductSettingID,@NOW,NULL)
				SELECT @ProductConfigurationID = SCOPE_IDENTITY();
			END
			ELSE
			BEGIN
				UPDATE Enterprise.ProductConfiguration SET
				ConfigurationId = @GlobalProductConfigurationID, ProductSettingId = @ProductSettingID,FromDate = @NOW,ThruDate = NULL
				WHERE ProductSettingId = @ProductSettingID
			END

	--------------------------------------------
	SET @ProductSettingID  = NULL
	SET @ProductConfigurationID  = NULL

	SELECT @ProductSettingID = ProductSettingId FROM Enterprise.ProductSetting WHERE ProductId=14 AND ProductSettingTypeId=1025
		IF @productSettingID IS NULL
			BEGIN
				INSERT INTO Enterprise.ProductSetting ([ProductId],[ProductSettingTypeId], [Value],[FromDate],[ThruDate]) 
				VALUES (14, 1025, '060000000005YDy', @NOW,NULL);

				SELECT @ProductSettingID=SCOPE_IDENTITY();
			END
		ELSE
			BEGIN
				UPDATE Enterprise.ProductSetting 
				SET ProductId = 14, ProductSettingTypeId=1025, Value='060000000005YDy',FromDate=@NOW,ThruDate = NULL
				WHERE ProductSettingId = @ProductSettingID
			END

	SELECT @ProductConfigurationID = ProductConfigurationId FROM Enterprise.ProductConfiguration WHERE ConfigurationId = @GlobalProductConfigurationID AND ProductSettingId = @ProductSettingID
		IF @ProductConfigurationID IS NULL
			BEGIN
				INSERT INTO Enterprise.ProductConfiguration(ConfigurationId,ProductSettingId,FromDate,ThruDate)
				VALUES(@GlobalProductConfigurationID,@ProductSettingID,@NOW,NULL)
				SELECT @ProductConfigurationID = SCOPE_IDENTITY();
			END
			ELSE
			BEGIN
				UPDATE Enterprise.ProductConfiguration SET
				ConfigurationId = @GlobalProductConfigurationID, ProductSettingId = @ProductSettingID,FromDate = @NOW,ThruDate = NULL
				WHERE ProductSettingId = @ProductSettingID
			END

	----------------------------------------------
	SET @ProductSettingID  = NULL
	SET @ProductConfigurationID  = NULL

	SELECT @ProductSettingID = ProductSettingId FROM Enterprise.ProductSetting WHERE ProductId=14 AND ProductSettingTypeId=1026
		IF @productSettingID IS NULL
			BEGIN
				INSERT INTO Enterprise.ProductSetting ([ProductId],[ProductSettingTypeId], [Value],[FromDate],[ThruDate]) 
				VALUES (14, 1026, '00D290000000XyT', @NOW,NULL);

				SELECT @ProductSettingID=SCOPE_IDENTITY();
			END
		ELSE
			BEGIN
				UPDATE Enterprise.ProductSetting 
				SET ProductId = 14, ProductSettingTypeId=1026, Value='00D290000000XyT',FromDate=@NOW,ThruDate = NULL
				WHERE ProductSettingId = @ProductSettingID
			END

	SELECT @ProductConfigurationID = ProductConfigurationId FROM Enterprise.ProductConfiguration WHERE ConfigurationId = @GlobalProductConfigurationID AND ProductSettingId = @ProductSettingID
		IF @ProductConfigurationID IS NULL
			BEGIN
				INSERT INTO Enterprise.ProductConfiguration(ConfigurationId,ProductSettingId,FromDate,ThruDate)
				VALUES(@GlobalProductConfigurationID,@ProductSettingID,@NOW,NULL)
				SELECT @ProductConfigurationID = SCOPE_IDENTITY();
			END
			ELSE
			BEGIN
				UPDATE Enterprise.ProductConfiguration SET
				ConfigurationId = @GlobalProductConfigurationID, ProductSettingId = @ProductSettingID,FromDate = @NOW,ThruDate = NULL
				WHERE ProductSettingId = @ProductSettingID
			END


	Print 'Adding new values to SamlProductSettings'

	IF (SELECT COUNT(*) FROM Ident.SamlProductSettings WHERE ProductId = 14 and SubjectIdSamlAttribute = 'productUsername') > 1
	BEGIN
		Print 'Dropping Duplicates From SamlProductSettings'
		DELETE Ident.SamlProductSettings
		WHERE ProductId = 14 and SubjectIdSamlAttribute = 'productUsername'
	END

	SELECT @NewSamlProductSettings = [SamlProductSettingsId] 
		FROM Ident.SamlProductSettings 
		WHERE ProductId = 14 and SubjectIdSamlAttribute = 'productUsername'

	IF @NewSamlProductSettings IS NULL
	BEGIN
		Print 'Adding New SamlProductSetting'
		EXEC [Ident].[CreateSamlProductSetting]
			 @ProductId = 14
			,@LoginUri = 'https://realpage.my.salesforce.com?so=00D00000000heZB'
			,@SigningCertificateThumbprint = '5357F3B84E302A9871B71CD037819A42688C1C64'
			,@SubjectIdSamlAttribute = 'productUsername'
			,@SamlProductSettingsId = @NewSamlProductSettings	OUTPUT
		END
	ELSE
	BEGIN
	
		Print 'Updating SamlProductSetting'
		UPDATE Ident.SamlProductSettings SET 
			 [LoginUri] = 'https://realpage.my.salesforce.com?so=00D00000000heZB'
			,[SigningCertificateThumbprint] = '5357F3B84E302A9871B71CD037819A42688C1C64'
		WHERE ProductId = 14 AND SubjectIdSamlAttribute = 'productUsername'	
	END
END


EXEC sys.sp_updateextendedproperty @name = N'Build' , @value = '10';

