--IF NOT EXISTS
--(
--    SELECT 1
--    FROM Auth.Scopes
--    WHERE Name = 'greenbooknwpapi'
--)
--BEGIN
--	INSERT INTO Auth.Scopes(Name, DisplayName, Enabled, Required, Emphasize, Type, IncludeAllClaimsForUser, ShowInDiscoveryDocument, AllowUnrestrictedIntrospection) 
--	VALUES('greenbooknwpapi', 'GreenBookNwpApi', 1, 0, 0, 1, 1, 1, 0)
--END;
-------------------------------------------------------------------------------------
--SET @ClientId = NULL;
--SELECT @ClientId = ClientId FROM Auth.Clients WHERE ClientCode = 'RUM' AND Flow = 3
--	IF @ClientId IS NULL
--		BEGIN
--			INSERT INTO Auth.Clients(ClientCode, ClientName, Flow, IdentityTokenLifetime, AccessTokenLifetime, AuthorizationCodeLifetime, AbsoluteRefreshTokenLifetime, SlidingRefreshTokenLifetime, RefreshTokenUsage, RefreshTokenExpiration, AccessTokenType, UpdateAccessTokenOnRefresh, Enabled, LogoutSessionRequired, RequireSignOutPrompt, AllowAccessToAllScopes, AllowClientCredentialsOnly, RequireConsent, AllowRememberConsent, EnableLocalLogin, IncludeJwtId, AlwaysSendClientClaims, PrefixClientClaims, AllowAccessToAllGrantTypes) 
--			VALUES('RUM', 'Resident Utility Management', 3, 360, 3600, 360, 86400, 3600, 0, 0, 0, 'True', 'True', 'False', 'False', 'True', 'False', 'False', 'True', 'True', 'True', 'True', 'True', 'True')

--			SELECT @ClientId=SCOPE_IDENTITY()

--			INSERT INTO Auth.ClientScopes(ClientId, Scope)
--			VALUES(@ClientId, 'bluebookapi')

--			INSERT INTO Auth.ClientScopes(ClientId, Scope)
--			VALUES(@ClientId, 'offline_access')

--			INSERT INTO Auth.ClientScopes(ClientId, Scope)
--			VALUES(@ClientId, 'greenbooknwpapi')

--			INSERT INTO Auth.ClientSecrets(ClientId, Value, Description, Expiration)
--			VALUES(@ClientId, '5jMIFx8mOxhZD+YCMNiBKlYAn7Q4RiycqhZtt5+DhcU=', 'Resident Utility Management', '2099-12-31 00:00:00.0000000 -06:00')

--			INSERT INTO Enterprise.ProductSetting (ProductId, ProductSettingTypeId, Value)
--			VALUES(18, 1, 'greenbooknwpapi')

--			INSERT INTO Enterprise.ProductSetting (ProductId, ProductSettingTypeId, Value)
--			VALUES(18, 1005, 'https://apisat.nwp.com/Identity')

--			INSERT INTO Enterprise.ProductSetting (ProductId, ProductSettingTypeId, Value)
--			VALUES(18, 1021, 'api-secret1')
--		END;
-------------------------------------------------------------------------------------
--IF NOT EXISTS
--(
--    SELECT 1
--    FROM Auth.Scopes
--    WHERE Name = 'activityreader'
--)
--BEGIN
--	INSERT INTO Auth.Scopes(Name, DisplayName, Enabled, Required, Emphasize, Type, IncludeAllClaimsForUser, ShowInDiscoveryDocument, AllowUnrestrictedIntrospection) 
--	VALUES('activityreader', 'Activity Reader', 1, 0, 0, 1, 1, 1, 0)
--END;
-------------------------------------------------------------------------------------
--SET @ClientId = NULL;
--SELECT @ClientId = ClientId FROM Auth.Clients WHERE ClientCode = 'activityreader' AND Flow = 3
--	IF @ClientId IS NULL
--		BEGIN
--			INSERT INTO Auth.Clients(ClientCode, ClientName, Flow, IdentityTokenLifetime, AccessTokenLifetime, AuthorizationCodeLifetime, AbsoluteRefreshTokenLifetime, SlidingRefreshTokenLifetime, RefreshTokenUsage, RefreshTokenExpiration, AccessTokenType, UpdateAccessTokenOnRefresh, Enabled, LogoutSessionRequired, RequireSignOutPrompt, AllowAccessToAllScopes, AllowClientCredentialsOnly, RequireConsent, AllowRememberConsent, EnableLocalLogin, IncludeJwtId, AlwaysSendClientClaims, PrefixClientClaims, AllowAccessToAllGrantTypes) 
--			VALUES('activityreader', 'Activity Reader', 3, 360, 3600, 360, 86400, 3600, 0, 0, 0, 'True', 'True', 'False', 'False', 'True', 'False', 'False', 'True', 'True', 'True', 'True', 'True', 'True')

--			SELECT @ClientId=SCOPE_IDENTITY()

--			INSERT INTO Auth.ClientScopes(ClientId, Scope)
--			VALUES(@ClientId, 'bluebookapi')

--			INSERT INTO Auth.ClientScopes(ClientId, Scope)
--			VALUES(@ClientId, 'offline_access')

--			INSERT INTO Auth.ClientScopes(ClientId, Scope)
--			VALUES(@ClientId, 'greenbooknwpapi')

--			INSERT INTO Auth.ClientSecrets(ClientId, Value, Description, Expiration)
--			VALUES(@ClientId, '5jMIFx8mOxhZD+YCMNiBKlYAn7Q4RiycqhZtt5+DhcU=', 'Activity Reader', '2099-12-31 00:00:00.0000000 -06:00')

--			INSERT INTO Auth.ClientRedirectUris (ClientId, Uri)
--			VALUES (@ClientId, 'https://activitylogging/swagger/ui/o2c-html')
--		END;
-----------------------------------------------------------------------------------
EXEC sys.sp_updateextendedproperty @name=N'Build', @value='44'