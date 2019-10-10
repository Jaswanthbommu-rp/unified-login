----  REQUESTED BY IM FROM DJ 4/20/18
--IF EXISTS
--(
--    SELECT 1
--    FROM enterprise.rightvaluetype
--    WHERE value = 'Ability to create and manage users for Resident Portals'
--)
--    BEGIN
--        UPDATE [Identity].[Enterprise].[RightValueType]
--          SET
--              ShortName = 'AddEditResidentPortalUser'
--        WHERE value = 'Ability to create and manage users for Resident Portals';
--    END;

----GB-854
--IF NOT EXISTS
--(
--SELECT 1
--FROM Auth.Scopes
--WHERE Name = 'settings-management-tool'
--)
--BEGIN
--INSERT INTO Auth.Scopes(Name, DisplayName, Enabled, Required, Emphasize, Type, IncludeAllClaimsForUser, ShowInDiscoveryDocument, AllowUnrestrictedIntrospection) 
--VALUES( 'settings-management-tool', 'Settings Management Tool', 1, 0, 1, 1, 1, 1, 0 );
--END;

--IF NOT EXISTS
--(
--SELECT 1
--FROM Auth.Scopes
--WHERE Name = 'unifiedsettingsapi'
--)
--BEGIN
--INSERT INTO Auth.Scopes(Name, DisplayName, Enabled, Required, Emphasize, Type, IncludeAllClaimsForUser, ShowInDiscoveryDocument, AllowUnrestrictedIntrospection) 
--VALUES('unifiedsettingsapi', 'Unified Settings API', 1, 0, 1, 1, 1, 1, 0)
--END;

--SELECT @ClientId = ClientId FROM Auth.Clients WHERE ClientCode = 'settings-management' AND Flow = 2
--IF @ClientId IS NULL
--BEGIN
--INSERT INTO Auth.Clients(ClientCode, ClientName, Flow, IdentityTokenLifetime, AccessTokenLifetime, AuthorizationCodeLifetime, AbsoluteRefreshTokenLifetime, SlidingRefreshTokenLifetime, RefreshTokenUsage, RefreshTokenExpiration, AccessTokenType, UpdateAccessTokenOnRefresh, Enabled, LogoutSessionRequired, RequireSignOutPrompt, AllowAccessToAllScopes, AllowClientCredentialsOnly, RequireConsent, AllowRememberConsent, EnableLocalLogin, IncludeJwtId, AlwaysSendClientClaims, PrefixClientClaims, AllowAccessToAllGrantTypes) 
--VALUES('settings-management', 'Settings Management', 2, 360, 3600, 360, 86400, 3600, 0, 0, 1, 'True', 'True', 'False', 'False', 'True', 'False', 'False', 'True', 'True', 'True', 'True', 'True', 'True')

--SELECT @ClientId=SCOPE_IDENTITY()

--INSERT INTO Auth.ClientScopes(ClientId, Scope)
--VALUES(@ClientId, 'settings-management-tool')

--INSERT INTO Auth.ClientScopes(ClientId, Scope)
--VALUES(@ClientId, 'unifiedsettingsapi')

--INSERT INTO Auth.ClientScopes(ClientId, Scope)
--VALUES(@ClientId, 'offline_access')

--INSERT INTO Auth.ClientSecrets(ClientId, Value, Description, Expiration)
--VALUES(@ClientId, 'VzsmfPiRyUunha36V6LY0EOFIHkqlB6w0coPJDfmHbY=', 'Settings Management Console Application', '2099-12-31 00:00:00.0000000 -06:00')

--IF @@SERVERNAME = 'RCDUSODBSQL001'
--BEGIN
--	IF DB_NAME() = 'IdentityDevelopment'
--	INSERT INTO Auth.ClientRedirectUris(ClientId, Uri)
--	VALUES(@ClientId, 'https://settings-management-dev.corp.realpage.com/signin-oidc')

--	IF DB_NAME() = 'Identity'
--	INSERT INTO Auth.ClientRedirectUris(ClientId, Uri)
--	VALUES(@ClientId, 'https://settings-management-qa.realpage.com/signin-oidc')
--END

--IF @@SERVERNAME = 'RCQUSODBSQL001'
--BEGIN
--	INSERT INTO Auth.ClientRedirectUris(ClientId, Uri)
--	VALUES(@ClientId, 'https://settings-management-sat.realpage.com/signin-oidc')
--END

--IF @@SERVERNAME IN ('RCPGBKDBSQL005A', 'RCPGBKDBSQL005B')
--BEGIN
--	INSERT INTO Auth.ClientRedirectUris(ClientId, Uri)
--	VALUES(@ClientId, 'https://settings-management.realpage.com/signin-oidc')
--END

--END;

----GB-853
--IF NOT EXISTS (SELECT 1 FROM Enterprise.ProductSettingType WHERE Name = 'TokenEndPoint')
--EXEC Enterprise.CreateProductSettingType 'TokenEndPoint', 'TokenEndPoint', @ProductSettingTypeId OUTPUT

--SELECT @ProductId = ProductId FROM Enterprise.Product WHERE Name = 'Vendor Services'

--SET @FromDate = GETUTCDATE()

--IF @@SERVERNAME = 'RCDUSODBSQL001'
--BEGIN
--	EXEC Enterprise.CreateProductSetting @ProductId, @ProductSettingTypeId, 'https://mylsat.realpage.com/identity/connect/token', @FromDate, NULL, @ProductSettingId OUTPUT
--	SELECT @ConfigurationId = ConfigurationId FROM Enterprise.GlobalProductConfiguration WHERE ProductId = @ProductId AND ThruDate IS NULL
--	EXEC Enterprise.LinkProductSettingToConfiguration @ConfigurationId, @ProductSettingID, NULL, NULL
--END

--IF @@SERVERNAME = 'RCTUSODBSQL001'
--BEGIN
--	EXEC Enterprise.CreateProductSetting @ProductId, @ProductSettingTypeId, 'https://mylqa.realpage.com/identity/connect/token', @FromDate, NULL, @ProductSettingId OUTPUT
--	SELECT @ConfigurationId = ConfigurationId FROM Enterprise.GlobalProductConfiguration WHERE ProductId = @ProductId AND ThruDate IS NULL
--	EXEC Enterprise.LinkProductSettingToConfiguration @ConfigurationId, @ProductSettingID, NULL, NULL
--END

--IF @@SERVERNAME = 'RCQUSODBSQL001'
--BEGIN
--	EXEC Enterprise.CreateProductSetting @ProductId, @ProductSettingTypeId, 'https://mylsat.realpage.com/identity/connect/token', @FromDate, NULL, @ProductSettingId OUTPUT
--	SELECT @ConfigurationId = ConfigurationId FROM Enterprise.GlobalProductConfiguration WHERE ProductId = @ProductId AND ThruDate IS NULL
--	EXEC Enterprise.LinkProductSettingToConfiguration @ConfigurationId, @ProductSettingID, NULL, NULL
--END

--IF @@SERVERNAME IN ('RCPGBKDBSQL005A', 'RCPGBKDBSQL005B')
--BEGIN
--	EXEC Enterprise.CreateProductSetting @ProductId, @ProductSettingTypeId, 'https://myl.realpage.com/identity/connect/token', @FromDate, NULL, @ProductSettingId OUTPUT
--	SELECT @ConfigurationId = ConfigurationId FROM Enterprise.GlobalProductConfiguration WHERE ProductId = @ProductId AND ThruDate IS NULL
--	EXEC Enterprise.LinkProductSettingToConfiguration @ConfigurationId, @ProductSettingID, NULL, NULL
--END

----GB-757/GB-817
--IF EXISTS (SELECT 1 FROM Enterprise.Product WHERE Name = 'Unified Login')
--BEGIN
--UPDATE Enterprise.Product SET Name = 'Unified Platform' WHERE Name = 'Unified Login'
--END

--IF EXISTS (SELECT 1 FROM Enterprise.Product WHERE Name = 'Websites & Syndication')
--BEGIN
--UPDATE Enterprise.Product SET Name = 'Marketing Center' WHERE Name = 'Websites & Syndication'
--END

--EXEC sys.sp_updateextendedproperty @name=N'Build', @value='65'