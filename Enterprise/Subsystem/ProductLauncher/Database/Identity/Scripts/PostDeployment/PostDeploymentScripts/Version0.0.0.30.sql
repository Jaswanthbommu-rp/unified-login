IF NOT EXISTS
(
    SELECT 1
    FROM Auth.Scopes
    WHERE Name = 'bluebookapi'
)
BEGIN
	INSERT INTO Auth.Scopes(Name, DisplayName, Enabled, Required, Emphasize, Type, IncludeAllClaimsForUser, ShowInDiscoveryDocument, AllowUnrestrictedIntrospection) 
	VALUES('bluebookapi', 'BlueBookAPI', 1, 0, 0, 1, 1, 1, 0)
END;
-----------------------------------------------------------------------------------
SET @ClientId = NULL;
SELECT @ClientId = ClientId FROM Auth.Clients WHERE ClientCode = 'BI' AND Flow = 3
	IF @ClientId IS NULL
		BEGIN
			INSERT INTO Auth.Clients(ClientCode, ClientName, Flow, IdentityTokenLifetime, AccessTokenLifetime, AuthorizationCodeLifetime, AbsoluteRefreshTokenLifetime, SlidingRefreshTokenLifetime, RefreshTokenUsage, RefreshTokenExpiration, AccessTokenType, UpdateAccessTokenOnRefresh, Enabled, LogoutSessionRequired, RequireSignOutPrompt, AllowAccessToAllScopes, AllowClientCredentialsOnly, RequireConsent, AllowRememberConsent, EnableLocalLogin, IncludeJwtId, AlwaysSendClientClaims, PrefixClientClaims, AllowAccessToAllGrantTypes) 
			VALUES('BI', 'Business Intelligence', 3, 360, 3600, 360, 86400, 3600, 0, 0, 0, 'True', 'True', 'False', 'False', 'True', 'False', 'False', 'True', 'True', 'True', 'True', 'True', 'True')

			SELECT @ClientId=SCOPE_IDENTITY()

			INSERT INTO Auth.ClientScopes(ClientId, Scope)
			VALUES(@ClientId, 'bluebookapi')

			INSERT INTO Auth.ClientScopes(ClientId, Scope)
			VALUES(@ClientId, 'offline_access')

			INSERT INTO Auth.ClientSecrets(ClientId, Value, Description, Expiration)
			VALUES(@ClientId, 'NtEvETu7sML25d2oyCGb3VdkI5bfU59ZT9233WbguMU=', 'Business Intelligence', '2099-12-31 00:00:00.0000000 -06:00')
		END;
-----------------------------------------------------------------------------------
SET @ClientId = NULL
SELECT @ClientId = ClientId FROM Auth.Clients WHERE ClientCode = 'MPF' AND Flow = 3
	IF @ClientId IS NULL
		BEGIN
			INSERT INTO Auth.Clients(ClientCode, ClientName, Flow, IdentityTokenLifetime, AccessTokenLifetime, AuthorizationCodeLifetime, AbsoluteRefreshTokenLifetime, SlidingRefreshTokenLifetime, RefreshTokenUsage, RefreshTokenExpiration, AccessTokenType, UpdateAccessTokenOnRefresh, Enabled, LogoutSessionRequired, RequireSignOutPrompt, AllowAccessToAllScopes, AllowClientCredentialsOnly, RequireConsent, AllowRememberConsent, EnableLocalLogin, IncludeJwtId, AlwaysSendClientClaims, PrefixClientClaims, AllowAccessToAllGrantTypes) 
			VALUES('MPF', 'MPF Research', 3, 360, 3600, 360, 86400, 3600, 0, 0, 0, 'True', 'True', 'False', 'False', 'True', 'False', 'False', 'True', 'True', 'True', 'True', 'True', 'True')

			SELECT @ClientId=SCOPE_IDENTITY()

			INSERT INTO Auth.ClientScopes(ClientId, Scope)
			VALUES(@ClientId, 'bluebookapi')

			INSERT INTO Auth.ClientScopes(ClientId, Scope)
			VALUES(@ClientId, 'offline_access')

			INSERT INTO Auth.ClientSecrets(ClientId, Value, Description, Expiration)
			VALUES(@ClientId, 'CDgd74qCQ76ejjxQWtef4nKVkBJrM/dh/A5xQwOx1mE=', 'MPF Research', '2099-12-31 00:00:00.0000000 -06:00')
		END;
-----------------------------------------------------------------------------------
SET @ClientId = NULL
SELECT @ClientId = ClientId FROM Auth.Clients WHERE ClientCode = 'AXIO' AND Flow = 3
	IF @ClientId IS NULL
		BEGIN
			INSERT INTO Auth.Clients(ClientCode, ClientName, Flow, IdentityTokenLifetime, AccessTokenLifetime, AuthorizationCodeLifetime, AbsoluteRefreshTokenLifetime, SlidingRefreshTokenLifetime, RefreshTokenUsage, RefreshTokenExpiration, AccessTokenType, UpdateAccessTokenOnRefresh, Enabled, LogoutSessionRequired, RequireSignOutPrompt, AllowAccessToAllScopes, AllowClientCredentialsOnly, RequireConsent, AllowRememberConsent, EnableLocalLogin, IncludeJwtId, AlwaysSendClientClaims, PrefixClientClaims, AllowAccessToAllGrantTypes) 
			VALUES('AXIO', 'Axiometrics', 3, 360, 3600, 360, 86400, 3600, 0, 0, 0, 'True', 'True', 'False', 'False', 'True', 'False', 'False', 'True', 'True', 'True', 'True', 'True', 'True')

			SELECT @ClientId=SCOPE_IDENTITY()

			INSERT INTO Auth.ClientScopes(ClientId, Scope)
			VALUES(@ClientId, 'bluebookapi')

			INSERT INTO Auth.ClientScopes(ClientId, Scope)
			VALUES(@ClientId, 'offline_access')

			INSERT INTO Auth.ClientSecrets(ClientId, Value, Description, Expiration)
			VALUES(@ClientId, 'qGMKp32/G2QIzQYlOgKQNpjSDH0j2GVZb5GDPj3U510=', 'Axiometrics', '2099-12-31 00:00:00.0000000 -06:00')
		END;
-----------------------------------------------------------------------------------
SET @ClientId = NULL
SELECT @ClientId = ClientId FROM Auth.Clients WHERE ClientCode = 'RCA' AND Flow = 3
	IF @ClientId IS NULL
		BEGIN
			INSERT INTO Auth.Clients(ClientCode, ClientName, Flow, IdentityTokenLifetime, AccessTokenLifetime, AuthorizationCodeLifetime, AbsoluteRefreshTokenLifetime, SlidingRefreshTokenLifetime, RefreshTokenUsage, RefreshTokenExpiration, AccessTokenType, UpdateAccessTokenOnRefresh, Enabled, LogoutSessionRequired, RequireSignOutPrompt, AllowAccessToAllScopes, AllowClientCredentialsOnly, RequireConsent, AllowRememberConsent, EnableLocalLogin, IncludeJwtId, AlwaysSendClientClaims, PrefixClientClaims, AllowAccessToAllGrantTypes) 
			VALUES('RCA', 'Real Capital Analytics', 3, 360, 3600, 360, 86400, 3600, 0, 0, 0, 'True', 'True', 'False', 'False', 'True', 'False', 'False', 'True', 'True', 'True', 'True', 'True', 'True')

			SELECT @ClientId=SCOPE_IDENTITY()

			INSERT INTO Auth.ClientScopes(ClientId, Scope)
			VALUES(@ClientId, 'bluebookapi')

			INSERT INTO Auth.ClientScopes(ClientId, Scope)
			VALUES(@ClientId, 'offline_access')

			INSERT INTO Auth.ClientSecrets(ClientId, Value, Description, Expiration)
			VALUES(@ClientId, 'uTWDSDVh2eT1GqKr2e4il1MDCrusu4dAMbzNlPPZZmM=', 'Real Capital Analytics', '2099-12-31 00:00:00.0000000 -06:00')
		END;
-----------------------------------------------------------------------------------
SET @ClientId = NULL
SELECT @ClientId = ClientId FROM Auth.Clients WHERE ClientCode = 'HUD' AND Flow = 3
	IF @ClientId IS NULL
		BEGIN
			INSERT INTO Auth.Clients(ClientCode, ClientName, Flow, IdentityTokenLifetime, AccessTokenLifetime, AuthorizationCodeLifetime, AbsoluteRefreshTokenLifetime, SlidingRefreshTokenLifetime, RefreshTokenUsage, RefreshTokenExpiration, AccessTokenType, UpdateAccessTokenOnRefresh, Enabled, LogoutSessionRequired, RequireSignOutPrompt, AllowAccessToAllScopes, AllowClientCredentialsOnly, RequireConsent, AllowRememberConsent, EnableLocalLogin, IncludeJwtId, AlwaysSendClientClaims, PrefixClientClaims, AllowAccessToAllGrantTypes) 
			VALUES('HUD', 'US Dept of HUD', 3, 360, 3600, 360, 86400, 3600, 0, 0, 0, 'True', 'True', 'False', 'False', 'True', 'False', 'False', 'True', 'True', 'True', 'True', 'True', 'True')

			SELECT @ClientId=SCOPE_IDENTITY()

			INSERT INTO Auth.ClientScopes(ClientId, Scope)
			VALUES(@ClientId, 'bluebookapi')

			INSERT INTO Auth.ClientScopes(ClientId, Scope)
			VALUES(@ClientId, 'offline_access')

			INSERT INTO Auth.ClientSecrets(ClientId, Value, Description, Expiration)
			VALUES(@ClientId, 'CvpSXM1K9O0p/quL0dJl6ToUyokKGwdNSMkIe9ozA8s=', 'US Dept of HUD', '2099-12-31 00:00:00.0000000 -06:00')
		END;
-----------------------------------------------------------------------------------
SET @ClientId = NULL
SELECT @ClientId = ClientId FROM Auth.Clients WHERE ClientCode = 'OMS' AND Flow = 3
	IF @ClientId IS NULL
		BEGIN
			INSERT INTO Auth.Clients(ClientCode, ClientName, Flow, IdentityTokenLifetime, AccessTokenLifetime, AuthorizationCodeLifetime, AbsoluteRefreshTokenLifetime, SlidingRefreshTokenLifetime, RefreshTokenUsage, RefreshTokenExpiration, AccessTokenType, UpdateAccessTokenOnRefresh, Enabled, LogoutSessionRequired, RequireSignOutPrompt, AllowAccessToAllScopes, AllowClientCredentialsOnly, RequireConsent, AllowRememberConsent, EnableLocalLogin, IncludeJwtId, AlwaysSendClientClaims, PrefixClientClaims, AllowAccessToAllGrantTypes) 
			VALUES('OMS', 'OMS', 3, 360, 3600, 360, 86400, 3600, 0, 0, 0, 'True', 'True', 'False', 'False', 'True', 'False', 'False', 'True', 'True', 'True', 'True', 'True', 'True')

			SELECT @ClientId=SCOPE_IDENTITY()

			INSERT INTO Auth.ClientScopes(ClientId, Scope)
			VALUES(@ClientId, 'bluebookapi')

			INSERT INTO Auth.ClientScopes(ClientId, Scope)
			VALUES(@ClientId, 'offline_access')

			INSERT INTO Auth.ClientSecrets(ClientId, Value, Description, Expiration)
			VALUES(@ClientId, 'YNbMW4qRsLDXBSjHWLPH6Rf26pK839B+FT5vj71viyo=', 'OMS', '2099-12-31 00:00:00.0000000 -06:00')
		END;
-----------------------------------------------------------------------------------
SET @ClientId = NULL
SELECT @ClientId = ClientId FROM Auth.Clients WHERE ClientCode = 'OS' AND Flow = 3
	IF @ClientId IS NULL
		BEGIN
			INSERT INTO Auth.Clients(ClientCode, ClientName, Flow, IdentityTokenLifetime, AccessTokenLifetime, AuthorizationCodeLifetime, AbsoluteRefreshTokenLifetime, SlidingRefreshTokenLifetime, RefreshTokenUsage, RefreshTokenExpiration, AccessTokenType, UpdateAccessTokenOnRefresh, Enabled, LogoutSessionRequired, RequireSignOutPrompt, AllowAccessToAllScopes, AllowClientCredentialsOnly, RequireConsent, AllowRememberConsent, EnableLocalLogin, IncludeJwtId, AlwaysSendClientClaims, PrefixClientClaims, AllowAccessToAllGrantTypes) 
			VALUES('OS', 'OneSite', 3, 360, 3600, 360, 86400, 3600, 0, 0, 0, 'True', 'True', 'False', 'False', 'True', 'False', 'False', 'True', 'True', 'True', 'True', 'True', 'True')

			SELECT @ClientId=SCOPE_IDENTITY()

			INSERT INTO Auth.ClientScopes(ClientId, Scope)
			VALUES(@ClientId, 'bluebookapi')

			INSERT INTO Auth.ClientScopes(ClientId, Scope)
			VALUES(@ClientId, 'offline_access')

			INSERT INTO Auth.ClientSecrets(ClientId, Value, Description, Expiration)
			VALUES(@ClientId, 'bUgQ6VTybM3mTo47C1St28LO47zHhpPU8yfKiYA3ads=', 'OneSite', '2099-12-31 00:00:00.0000000 -06:00')
		END;
-----------------------------------------------------------------------------------
SET @ClientId = NULL
SELECT @ClientId = ClientId FROM Auth.Clients WHERE ClientCode = 'LVL1' AND Flow = 3
	IF @ClientId IS NULL
		BEGIN
			INSERT INTO Auth.Clients(ClientCode, ClientName, Flow, IdentityTokenLifetime, AccessTokenLifetime, AuthorizationCodeLifetime, AbsoluteRefreshTokenLifetime, SlidingRefreshTokenLifetime, RefreshTokenUsage, RefreshTokenExpiration, AccessTokenType, UpdateAccessTokenOnRefresh, Enabled, LogoutSessionRequired, RequireSignOutPrompt, AllowAccessToAllScopes, AllowClientCredentialsOnly, RequireConsent, AllowRememberConsent, EnableLocalLogin, IncludeJwtId, AlwaysSendClientClaims, PrefixClientClaims, AllowAccessToAllGrantTypes) 
			VALUES('LVL1', 'Contact Center', 3, 360, 3600, 360, 86400, 3600, 0, 0, 0, 'True', 'True', 'False', 'False', 'True', 'False', 'False', 'True', 'True', 'True', 'True', 'True', 'True')

			SELECT @ClientId=SCOPE_IDENTITY()

			INSERT INTO Auth.ClientScopes(ClientId, Scope)
			VALUES(@ClientId, 'bluebookapi')

			INSERT INTO Auth.ClientScopes(ClientId, Scope)
			VALUES(@ClientId, 'offline_access')

			INSERT INTO Auth.ClientSecrets(ClientId, Value, Description, Expiration)
			VALUES(@ClientId, 'AwPw0agJpUC3R3cO2uGp9jLVOeVuGKp5dBKNnO7qTd4=', 'Contact Center', '2099-12-31 00:00:00.0000000 -06:00')
		END;
-----------------------------------------------------------------------------------
SET @ClientId = NULL
SELECT @ClientId = ClientId FROM Auth.Clients WHERE ClientCode = 'LS' AND Flow = 3
	IF @ClientId IS NULL
		BEGIN
			INSERT INTO Auth.Clients(ClientCode, ClientName, Flow, IdentityTokenLifetime, AccessTokenLifetime, AuthorizationCodeLifetime, AbsoluteRefreshTokenLifetime, SlidingRefreshTokenLifetime, RefreshTokenUsage, RefreshTokenExpiration, AccessTokenType, UpdateAccessTokenOnRefresh, Enabled, LogoutSessionRequired, RequireSignOutPrompt, AllowAccessToAllScopes, AllowClientCredentialsOnly, RequireConsent, AllowRememberConsent, EnableLocalLogin, IncludeJwtId, AlwaysSendClientClaims, PrefixClientClaims, AllowAccessToAllGrantTypes) 
			VALUES('LS', 'Marketing Center', 3, 360, 3600, 360, 86400, 3600, 0, 0, 0, 'True', 'True', 'False', 'False', 'True', 'False', 'False', 'True', 'True', 'True', 'True', 'True', 'True')

			SELECT @ClientId=SCOPE_IDENTITY()

			INSERT INTO Auth.ClientScopes(ClientId, Scope)
			VALUES(@ClientId, 'bluebookapi')

			INSERT INTO Auth.ClientScopes(ClientId, Scope)
			VALUES(@ClientId, 'offline_access')

			INSERT INTO Auth.ClientSecrets(ClientId, Value, Description, Expiration)
			VALUES(@ClientId, '75D19HciJbgPAJDJGcmU76VuoeVdrbVyMOeAl7SFqhI=', 'Marketing Center', '2099-12-31 00:00:00.0000000 -06:00')
		END;
-----------------------------------------------------------------------------------
SET @ClientId = NULL
SELECT @ClientId = ClientId FROM Auth.Clients WHERE ClientCode = 'AO' AND Flow = 3
	IF @ClientId IS NULL
		BEGIN
			INSERT INTO Auth.Clients(ClientCode, ClientName, Flow, IdentityTokenLifetime, AccessTokenLifetime, AuthorizationCodeLifetime, AbsoluteRefreshTokenLifetime, SlidingRefreshTokenLifetime, RefreshTokenUsage, RefreshTokenExpiration, AccessTokenType, UpdateAccessTokenOnRefresh, Enabled, LogoutSessionRequired, RequireSignOutPrompt, AllowAccessToAllScopes, AllowClientCredentialsOnly, RequireConsent, AllowRememberConsent, EnableLocalLogin, IncludeJwtId, AlwaysSendClientClaims, PrefixClientClaims, AllowAccessToAllGrantTypes) 
			VALUES('AO', 'Asset Optimization', 3, 360, 3600, 360, 86400, 3600, 0, 0, 0, 'True', 'True', 'False', 'False', 'True', 'False', 'False', 'True', 'True', 'True', 'True', 'True', 'True')

			SELECT @ClientId=SCOPE_IDENTITY()

			INSERT INTO Auth.ClientScopes(ClientId, Scope)
			VALUES(@ClientId, 'bluebookapi')

			INSERT INTO Auth.ClientScopes(ClientId, Scope)
			VALUES(@ClientId, 'offline_access')

			INSERT INTO Auth.ClientSecrets(ClientId, Value, Description, Expiration)
			VALUES(@ClientId, 'n4g+xAoezJrJQ63pkkyRoRTfp/JmqcIZRwJuNDqjJOE=', 'Asset Optimization', '2099-12-31 00:00:00.0000000 -06:00')
		END;
-----------------------------------------------------------------------------------
SET @ClientId = NULL
SELECT @ClientId = ClientId FROM Auth.Clients WHERE ClientCode = 'L2L' AND Flow = 3
	IF @ClientId IS NULL
		BEGIN
			INSERT INTO Auth.Clients(ClientCode, ClientName, Flow, IdentityTokenLifetime, AccessTokenLifetime, AuthorizationCodeLifetime, AbsoluteRefreshTokenLifetime, SlidingRefreshTokenLifetime, RefreshTokenUsage, RefreshTokenExpiration, AccessTokenType, UpdateAccessTokenOnRefresh, Enabled, LogoutSessionRequired, RequireSignOutPrompt, AllowAccessToAllScopes, AllowClientCredentialsOnly, RequireConsent, AllowRememberConsent, EnableLocalLogin, IncludeJwtId, AlwaysSendClientClaims, PrefixClientClaims, AllowAccessToAllGrantTypes) 
			VALUES('L2L', 'Lead2Lease', 3, 360, 3600, 360, 86400, 3600, 0, 0, 0, 'True', 'True', 'False', 'False', 'True', 'False', 'False', 'True', 'True', 'True', 'True', 'True', 'True')

			SELECT @ClientId=SCOPE_IDENTITY()

			INSERT INTO Auth.ClientScopes(ClientId, Scope)
			VALUES(@ClientId, 'bluebookapi')

			INSERT INTO Auth.ClientScopes(ClientId, Scope)
			VALUES(@ClientId, 'offline_access')

			INSERT INTO Auth.ClientSecrets(ClientId, Value, Description, Expiration)
			VALUES(@ClientId, 'DNRX6q7v8RSbCgo/p909ubiEOEk4oZ7DEa1BvZCzfIg=', 'Lead2Lease', '2099-12-31 00:00:00.0000000 -06:00')
		END;
-----------------------------------------------------------------------------------
SET @ClientId = NULL
SELECT @ClientId = ClientId FROM Auth.Clients WHERE ClientCode = 'LD' AND Flow = 3
	IF @ClientId IS NULL
		BEGIN
			INSERT INTO Auth.Clients(ClientCode, ClientName, Flow, IdentityTokenLifetime, AccessTokenLifetime, AuthorizationCodeLifetime, AbsoluteRefreshTokenLifetime, SlidingRefreshTokenLifetime, RefreshTokenUsage, RefreshTokenExpiration, AccessTokenType, UpdateAccessTokenOnRefresh, Enabled, LogoutSessionRequired, RequireSignOutPrompt, AllowAccessToAllScopes, AllowClientCredentialsOnly, RequireConsent, AllowRememberConsent, EnableLocalLogin, IncludeJwtId, AlwaysSendClientClaims, PrefixClientClaims, AllowAccessToAllGrantTypes) 
			VALUES('LD', 'Renters Insurance', 3, 360, 3600, 360, 86400, 3600, 0, 0, 0, 'True', 'True', 'False', 'False', 'True', 'False', 'False', 'True', 'True', 'True', 'True', 'True', 'True')

			SELECT @ClientId=SCOPE_IDENTITY()

			INSERT INTO Auth.ClientScopes(ClientId, Scope)
			VALUES(@ClientId, 'bluebookapi')

			INSERT INTO Auth.ClientScopes(ClientId, Scope)
			VALUES(@ClientId, 'offline_access')

			INSERT INTO Auth.ClientSecrets(ClientId, Value, Description, Expiration)
			VALUES(@ClientId, '7GGeqn4k18+NzK+FwGofHu5XR1E30V9RTetjhrhr5OY=', 'Renters Insurance', '2099-12-31 00:00:00.0000000 -06:00')
		END;
-----------------------------------------------------------------------------------
SET @ClientId = NULL
SELECT @ClientId = ClientId FROM Auth.Clients WHERE ClientCode = 'CD' AND Flow = 3
	IF @ClientId IS NULL
		BEGIN
			INSERT INTO Auth.Clients(ClientCode, ClientName, Flow, IdentityTokenLifetime, AccessTokenLifetime, AuthorizationCodeLifetime, AbsoluteRefreshTokenLifetime, SlidingRefreshTokenLifetime, RefreshTokenUsage, RefreshTokenExpiration, AccessTokenType, UpdateAccessTokenOnRefresh, Enabled, LogoutSessionRequired, RequireSignOutPrompt, AllowAccessToAllScopes, AllowClientCredentialsOnly, RequireConsent, AllowRememberConsent, EnableLocalLogin, IncludeJwtId, AlwaysSendClientClaims, PrefixClientClaims, AllowAccessToAllGrantTypes) 
			VALUES('CD', 'Compliance Depot', 3, 360, 3600, 360, 86400, 3600, 0, 0, 0, 'True', 'True', 'False', 'False', 'True', 'False', 'False', 'True', 'True', 'True', 'True', 'True', 'True')

			SELECT @ClientId=SCOPE_IDENTITY()

			INSERT INTO Auth.ClientScopes(ClientId, Scope)
			VALUES(@ClientId, 'bluebookapi')

			INSERT INTO Auth.ClientScopes(ClientId, Scope)
			VALUES(@ClientId, 'offline_access')

			INSERT INTO Auth.ClientSecrets(ClientId, Value, Description, Expiration)
			VALUES(@ClientId, 'ueu7ttoPAe7ouNp8Xtz9BGupOgCBFkGEW2pe5UUvET4=', 'Compliance Depot', '2099-12-31 00:00:00.0000000 -06:00')
		END;
-----------------------------------------------------------------------------------
SET @ClientId = NULL
SELECT @ClientId = ClientId FROM Auth.Clients WHERE ClientCode = 'NWP' AND Flow = 3
	IF @ClientId IS NULL
		BEGIN
			INSERT INTO Auth.Clients(ClientCode, ClientName, Flow, IdentityTokenLifetime, AccessTokenLifetime, AuthorizationCodeLifetime, AbsoluteRefreshTokenLifetime, SlidingRefreshTokenLifetime, RefreshTokenUsage, RefreshTokenExpiration, AccessTokenType, UpdateAccessTokenOnRefresh, Enabled, LogoutSessionRequired, RequireSignOutPrompt, AllowAccessToAllScopes, AllowClientCredentialsOnly, RequireConsent, AllowRememberConsent, EnableLocalLogin, IncludeJwtId, AlwaysSendClientClaims, PrefixClientClaims, AllowAccessToAllGrantTypes) 
			VALUES('NWP', 'NWP', 3, 360, 3600, 360, 86400, 3600, 0, 0, 0, 'True', 'True', 'False', 'False', 'True', 'False', 'False', 'True', 'True', 'True', 'True', 'True', 'True')

			SELECT @ClientId=SCOPE_IDENTITY()

			INSERT INTO Auth.ClientScopes(ClientId, Scope)
			VALUES(@ClientId, 'bluebookapi')

			INSERT INTO Auth.ClientScopes(ClientId, Scope)
			VALUES(@ClientId, 'offline_access')

			INSERT INTO Auth.ClientSecrets(ClientId, Value, Description, Expiration)
			VALUES(@ClientId, 'uqcr1dyhAdgHrRqmnI+Ihga+SToJQVor2cmb9Yjspzo=', 'NWP', '2099-12-31 00:00:00.0000000 -06:00')
		END;
-----------------------------------------------------------------------------------
SET @ClientId = NULL
SELECT @ClientId = ClientId FROM Auth.Clients WHERE ClientCode = 'VES' AND Flow = 3
	IF @ClientId IS NULL
		BEGIN
			INSERT INTO Auth.Clients(ClientCode, ClientName, Flow, IdentityTokenLifetime, AccessTokenLifetime, AuthorizationCodeLifetime, AbsoluteRefreshTokenLifetime, SlidingRefreshTokenLifetime, RefreshTokenUsage, RefreshTokenExpiration, AccessTokenType, UpdateAccessTokenOnRefresh, Enabled, LogoutSessionRequired, RequireSignOutPrompt, AllowAccessToAllScopes, AllowClientCredentialsOnly, RequireConsent, AllowRememberConsent, EnableLocalLogin, IncludeJwtId, AlwaysSendClientClaims, PrefixClientClaims, AllowAccessToAllGrantTypes) 
			VALUES('VES', 'Velocity', 3, 360, 3600, 360, 86400, 3600, 0, 0, 0, 'True', 'True', 'False', 'False', 'True', 'False', 'False', 'True', 'True', 'True', 'True', 'True', 'True')

			SELECT @ClientId=SCOPE_IDENTITY()

			INSERT INTO Auth.ClientScopes(ClientId, Scope)
			VALUES(@ClientId, 'bluebookapi')

			INSERT INTO Auth.ClientScopes(ClientId, Scope)
			VALUES(@ClientId, 'offline_access')

			INSERT INTO Auth.ClientSecrets(ClientId, Value, Description, Expiration)
			VALUES(@ClientId, 'VQAXawUsVRd2VLWy9nLTc2k743p5JT4f7gH0T00Xcd4=', 'Velocity', '2099-12-31 00:00:00.0000000 -06:00')
		END;
-----------------------------------------------------------------------------------
SET @ClientId = NULL
SELECT @ClientId = ClientId FROM Auth.Clients WHERE ClientCode = 'OPS' AND Flow = 3
	IF @ClientId IS NULL
		BEGIN
			INSERT INTO Auth.Clients(ClientCode, ClientName, Flow, IdentityTokenLifetime, AccessTokenLifetime, AuthorizationCodeLifetime, AbsoluteRefreshTokenLifetime, SlidingRefreshTokenLifetime, RefreshTokenUsage, RefreshTokenExpiration, AccessTokenType, UpdateAccessTokenOnRefresh, Enabled, LogoutSessionRequired, RequireSignOutPrompt, AllowAccessToAllScopes, AllowClientCredentialsOnly, RequireConsent, AllowRememberConsent, EnableLocalLogin, IncludeJwtId, AlwaysSendClientClaims, PrefixClientClaims, AllowAccessToAllGrantTypes) 
			VALUES('OPS', 'OpsTechnology', 3, 360, 3600, 360, 86400, 3600, 0, 0, 0, 'True', 'True', 'False', 'False', 'True', 'False', 'False', 'True', 'True', 'True', 'True', 'True', 'True')

			SELECT @ClientId=SCOPE_IDENTITY()

			INSERT INTO Auth.ClientScopes(ClientId, Scope)
			VALUES(@ClientId, 'bluebookapi')

			INSERT INTO Auth.ClientScopes(ClientId, Scope)
			VALUES(@ClientId, 'offline_access')

			INSERT INTO Auth.ClientSecrets(ClientId, Value, Description, Expiration)
			VALUES(@ClientId, '8zJWui1PexYiNI7XD7cdplk+uPmyS8LFFfesHgNxIAE=', 'OpsTechnology', '2099-12-31 00:00:00.0000000 -06:00')
		END;
-----------------------------------------------------------------------------------
SET @ClientId = NULL
SELECT @ClientId = ClientId FROM Auth.Clients WHERE ClientCode = 'SF' AND Flow = 3
	IF @ClientId IS NULL
		BEGIN
			INSERT INTO Auth.Clients(ClientCode, ClientName, Flow, IdentityTokenLifetime, AccessTokenLifetime, AuthorizationCodeLifetime, AbsoluteRefreshTokenLifetime, SlidingRefreshTokenLifetime, RefreshTokenUsage, RefreshTokenExpiration, AccessTokenType, UpdateAccessTokenOnRefresh, Enabled, LogoutSessionRequired, RequireSignOutPrompt, AllowAccessToAllScopes, AllowClientCredentialsOnly, RequireConsent, AllowRememberConsent, EnableLocalLogin, IncludeJwtId, AlwaysSendClientClaims, PrefixClientClaims, AllowAccessToAllGrantTypes) 
			VALUES('SF', 'Salesforce', 3, 360, 3600, 360, 86400, 3600, 0, 0, 0, 'True', 'True', 'False', 'False', 'True', 'False', 'False', 'True', 'True', 'True', 'True', 'True', 'True')

			SELECT @ClientId=SCOPE_IDENTITY()

			INSERT INTO Auth.ClientScopes(ClientId, Scope)
			VALUES(@ClientId, 'bluebookapi')

			INSERT INTO Auth.ClientScopes(ClientId, Scope)
			VALUES(@ClientId, 'offline_access')

			INSERT INTO Auth.ClientSecrets(ClientId, Value, Description, Expiration)
			VALUES(@ClientId, 'AJAkaN4re83qzc5yne5gUuGq6gd2zI4QfNRpI8yTVRA=', 'Salesforce', '2099-12-31 00:00:00.0000000 -06:00')
		END;
-----------------------------------------------------------------------------------
SET @ClientId = NULL
SELECT @ClientId = ClientId FROM Auth.Clients WHERE ClientCode = 'FR' AND Flow = 3
	IF @ClientId IS NULL
		BEGIN
			INSERT INTO Auth.Clients(ClientCode, ClientName, Flow, IdentityTokenLifetime, AccessTokenLifetime, AuthorizationCodeLifetime, AbsoluteRefreshTokenLifetime, SlidingRefreshTokenLifetime, RefreshTokenUsage, RefreshTokenExpiration, AccessTokenType, UpdateAccessTokenOnRefresh, Enabled, LogoutSessionRequired, RequireSignOutPrompt, AllowAccessToAllScopes, AllowClientCredentialsOnly, RequireConsent, AllowRememberConsent, EnableLocalLogin, IncludeJwtId, AlwaysSendClientClaims, PrefixClientClaims, AllowAccessToAllGrantTypes) 
			VALUES('FR', 'For Rent', 3, 360, 3600, 360, 86400, 3600, 0, 0, 0, 'True', 'True', 'False', 'False', 'True', 'False', 'False', 'True', 'True', 'True', 'True', 'True', 'True')

			SELECT @ClientId=SCOPE_IDENTITY()

			INSERT INTO Auth.ClientScopes(ClientId, Scope)
			VALUES(@ClientId, 'bluebookapi')

			INSERT INTO Auth.ClientScopes(ClientId, Scope)
			VALUES(@ClientId, 'offline_access')

			INSERT INTO Auth.ClientSecrets(ClientId, Value, Description, Expiration)
			VALUES(@ClientId, 'L+nIYgOzOxtHQmfK79IJrFF3LgwQl1UCLoChq/5vULw=', 'For Rent', '2099-12-31 00:00:00.0000000 -06:00')
		END;
-----------------------------------------------------------------------------------
SET @ClientId = NULL
SELECT @ClientId = ClientId FROM Auth.Clients WHERE ClientCode = 'AB' AND Flow = 3
	IF @ClientId IS NULL
		BEGIN
			INSERT INTO Auth.Clients(ClientCode, ClientName, Flow, IdentityTokenLifetime, AccessTokenLifetime, AuthorizationCodeLifetime, AbsoluteRefreshTokenLifetime, SlidingRefreshTokenLifetime, RefreshTokenUsage, RefreshTokenExpiration, AccessTokenType, UpdateAccessTokenOnRefresh, Enabled, LogoutSessionRequired, RequireSignOutPrompt, AllowAccessToAllScopes, AllowClientCredentialsOnly, RequireConsent, AllowRememberConsent, EnableLocalLogin, IncludeJwtId, AlwaysSendClientClaims, PrefixClientClaims, AllowAccessToAllGrantTypes) 
			VALUES('AB', 'Active Building', 3, 360, 3600, 360, 86400, 3600, 0, 0, 0, 'True', 'True', 'False', 'False', 'True', 'False', 'False', 'True', 'True', 'True', 'True', 'True', 'True')

			SELECT @ClientId=SCOPE_IDENTITY()

			INSERT INTO Auth.ClientScopes(ClientId, Scope)
			VALUES(@ClientId, 'bluebookapi')

			INSERT INTO Auth.ClientScopes(ClientId, Scope)
			VALUES(@ClientId, 'offline_access')

			INSERT INTO Auth.ClientSecrets(ClientId, Value, Description, Expiration)
			VALUES(@ClientId, 'cxpQasT7r238jRgy+3tgCXGpNghcYyBFjpwvEDiqij4=', 'Active Building', '2099-12-31 00:00:00.0000000 -06:00')
		END;
-----------------------------------------------------------------------------------
SET @ClientId = NULL
SELECT @ClientId = ClientId FROM Auth.Clients WHERE ClientCode = 'ONST' AND Flow = 3
	IF @ClientId IS NULL
		BEGIN
			INSERT INTO Auth.Clients(ClientCode, ClientName, Flow, IdentityTokenLifetime, AccessTokenLifetime, AuthorizationCodeLifetime, AbsoluteRefreshTokenLifetime, SlidingRefreshTokenLifetime, RefreshTokenUsage, RefreshTokenExpiration, AccessTokenType, UpdateAccessTokenOnRefresh, Enabled, LogoutSessionRequired, RequireSignOutPrompt, AllowAccessToAllScopes, AllowClientCredentialsOnly, RequireConsent, AllowRememberConsent, EnableLocalLogin, IncludeJwtId, AlwaysSendClientClaims, PrefixClientClaims, AllowAccessToAllGrantTypes) 
			VALUES('ONST', 'On-Site', 3, 360, 3600, 360, 86400, 3600, 0, 0, 0, 'True', 'True', 'False', 'False', 'True', 'False', 'False', 'True', 'True', 'True', 'True', 'True', 'True')

			SELECT @ClientId=SCOPE_IDENTITY()

			INSERT INTO Auth.ClientScopes(ClientId, Scope)
			VALUES(@ClientId, 'bluebookapi')

			INSERT INTO Auth.ClientScopes(ClientId, Scope)
			VALUES(@ClientId, 'offline_access')

			INSERT INTO Auth.ClientSecrets(ClientId, Value, Description, Expiration)
			VALUES(@ClientId, 'Q5oli1vJl5IyBLUHP+pRJlxLk4b3XETUU4wV9G4AHhA=', 'On-Site', '2099-12-31 00:00:00.0000000 -06:00')
		END;
-----------------------------------------------------------------------------------
SET @ClientId = NULL
SELECT @ClientId = ClientId FROM Auth.Clients WHERE ClientCode = 'GREYSTAR' AND Flow = 3
	IF @ClientId IS NULL
		BEGIN
			INSERT INTO Auth.Clients(ClientCode, ClientName, Flow, IdentityTokenLifetime, AccessTokenLifetime, AuthorizationCodeLifetime, AbsoluteRefreshTokenLifetime, SlidingRefreshTokenLifetime, RefreshTokenUsage, RefreshTokenExpiration, AccessTokenType, UpdateAccessTokenOnRefresh, Enabled, LogoutSessionRequired, RequireSignOutPrompt, AllowAccessToAllScopes, AllowClientCredentialsOnly, RequireConsent, AllowRememberConsent, EnableLocalLogin, IncludeJwtId, AlwaysSendClientClaims, PrefixClientClaims, AllowAccessToAllGrantTypes) 
			VALUES('GREYSTAR', 'Greystar', 3, 360, 3600, 360, 86400, 3600, 0, 0, 0, 'True', 'True', 'False', 'False', 'True', 'False', 'False', 'True', 'True', 'True', 'True', 'True', 'True')

			SELECT @ClientId=SCOPE_IDENTITY()

			INSERT INTO Auth.ClientScopes(ClientId, Scope)
			VALUES(@ClientId, 'bluebookapi')

			INSERT INTO Auth.ClientScopes(ClientId, Scope)
			VALUES(@ClientId, 'offline_access')

			INSERT INTO Auth.ClientSecrets(ClientId, Value, Description, Expiration)
			VALUES(@ClientId, 'nMYvp42x9mcTuuN0/BMZru/Rl4t+SPGnmE/rvVzBBkA=', 'Greystar', '2099-12-31 00:00:00.0000000 -06:00')
		END;
-----------------------------------------------------------------------------------
SET @ClientId = NULL
SELECT @ClientId = ClientId FROM Auth.Clients WHERE ClientCode = 'AUM' AND Flow = 3
	IF @ClientId IS NULL
		BEGIN
			INSERT INTO Auth.Clients(ClientCode, ClientName, Flow, IdentityTokenLifetime, AccessTokenLifetime, AuthorizationCodeLifetime, AbsoluteRefreshTokenLifetime, SlidingRefreshTokenLifetime, RefreshTokenUsage, RefreshTokenExpiration, AccessTokenType, UpdateAccessTokenOnRefresh, Enabled, LogoutSessionRequired, RequireSignOutPrompt, AllowAccessToAllScopes, AllowClientCredentialsOnly, RequireConsent, AllowRememberConsent, EnableLocalLogin, IncludeJwtId, AlwaysSendClientClaims, PrefixClientClaims, AllowAccessToAllGrantTypes) 
			VALUES('AUM', 'American Utility Management', 3, 360, 3600, 360, 86400, 3600, 0, 0, 0, 'True', 'True', 'False', 'False', 'True', 'False', 'False', 'True', 'True', 'True', 'True', 'True', 'True')

			SELECT @ClientId=SCOPE_IDENTITY()

			INSERT INTO Auth.ClientScopes(ClientId, Scope)
			VALUES(@ClientId, 'bluebookapi')

			INSERT INTO Auth.ClientScopes(ClientId, Scope)
			VALUES(@ClientId, 'offline_access')

			INSERT INTO Auth.ClientSecrets(ClientId, Value, Description, Expiration)
			VALUES(@ClientId, 'sulzXTL+HAA16rIAY+2GIqvF+SPnh4beHvEJBDS0sOw=', 'American Utility Management', '2099-12-31 00:00:00.0000000 -06:00')
		END;
-----------------------------------------------------------------------------------
SET @ClientId = NULL
SELECT @ClientId = ClientId FROM Auth.Clients WHERE ClientCode = 'INDATUS' AND Flow = 3
	IF @ClientId IS NULL
		BEGIN
			INSERT INTO Auth.Clients(ClientCode, ClientName, Flow, IdentityTokenLifetime, AccessTokenLifetime, AuthorizationCodeLifetime, AbsoluteRefreshTokenLifetime, SlidingRefreshTokenLifetime, RefreshTokenUsage, RefreshTokenExpiration, AccessTokenType, UpdateAccessTokenOnRefresh, Enabled, LogoutSessionRequired, RequireSignOutPrompt, AllowAccessToAllScopes, AllowClientCredentialsOnly, RequireConsent, AllowRememberConsent, EnableLocalLogin, IncludeJwtId, AlwaysSendClientClaims, PrefixClientClaims, AllowAccessToAllGrantTypes) 
			VALUES('INDATUS', 'Indatus', 3, 360, 3600, 360, 86400, 3600, 0, 0, 0, 'True', 'True', 'False', 'False', 'True', 'False', 'False', 'True', 'True', 'True', 'True', 'True', 'True')

			SELECT @ClientId=SCOPE_IDENTITY()

			INSERT INTO Auth.ClientScopes(ClientId, Scope)
			VALUES(@ClientId, 'bluebookapi')

			INSERT INTO Auth.ClientScopes(ClientId, Scope)
			VALUES(@ClientId, 'offline_access')

			INSERT INTO Auth.ClientSecrets(ClientId, Value, Description, Expiration)
			VALUES(@ClientId, '/9kLm1HspY9As7x2BhGiHa/yiv8v89WAF2dIsRsRilk=', 'Indatus', '2099-12-31 00:00:00.0000000 -06:00')
		END;
-----------------------------------------------------------------------------------
SET @ClientId = NULL
SELECT @ClientId = ClientId FROM Auth.Clients WHERE ClientCode = 'NTVS' AND Flow = 3
	IF @ClientId IS NULL
		BEGIN
			INSERT INTO Auth.Clients(ClientCode, ClientName, Flow, IdentityTokenLifetime, AccessTokenLifetime, AuthorizationCodeLifetime, AbsoluteRefreshTokenLifetime, SlidingRefreshTokenLifetime, RefreshTokenUsage, RefreshTokenExpiration, AccessTokenType, UpdateAccessTokenOnRefresh, Enabled, LogoutSessionRequired, RequireSignOutPrompt, AllowAccessToAllScopes, AllowClientCredentialsOnly, RequireConsent, AllowRememberConsent, EnableLocalLogin, IncludeJwtId, AlwaysSendClientClaims, PrefixClientClaims, AllowAccessToAllGrantTypes) 
			VALUES('NTVS', 'Notivus', 3, 360, 3600, 360, 86400, 3600, 0, 0, 0, 'True', 'True', 'False', 'False', 'True', 'False', 'False', 'True', 'True', 'True', 'True', 'True', 'True')

			SELECT @ClientId=SCOPE_IDENTITY()

			INSERT INTO Auth.ClientScopes(ClientId, Scope)
			VALUES(@ClientId, 'bluebookapi')

			INSERT INTO Auth.ClientScopes(ClientId, Scope)
			VALUES(@ClientId, 'offline_access')

			INSERT INTO Auth.ClientSecrets(ClientId, Value, Description, Expiration)
			VALUES(@ClientId, '/lJXByY4MMFzyMljyg6YVk6MoxQBfUhULKk2JHf207c=', 'Notivus', '2099-12-31 00:00:00.0000000 -06:00')
		END;
-----------------------------------------------------------------------------------
SET @ClientId = NULL
SELECT @ClientId = ClientId FROM Auth.Clients WHERE ClientCode = 'ACCT' AND Flow = 3
	IF @ClientId IS NULL
		BEGIN
			INSERT INTO Auth.Clients(ClientCode, ClientName, Flow, IdentityTokenLifetime, AccessTokenLifetime, AuthorizationCodeLifetime, AbsoluteRefreshTokenLifetime, SlidingRefreshTokenLifetime, RefreshTokenUsage, RefreshTokenExpiration, AccessTokenType, UpdateAccessTokenOnRefresh, Enabled, LogoutSessionRequired, RequireSignOutPrompt, AllowAccessToAllScopes, AllowClientCredentialsOnly, RequireConsent, AllowRememberConsent, EnableLocalLogin, IncludeJwtId, AlwaysSendClientClaims, PrefixClientClaims, AllowAccessToAllGrantTypes) 
			VALUES('ACCT', 'Accounting', 3, 360, 3600, 360, 86400, 3600, 0, 0, 0, 'True', 'True', 'False', 'False', 'True', 'False', 'False', 'True', 'True', 'True', 'True', 'True', 'True')

			SELECT @ClientId=SCOPE_IDENTITY()

			INSERT INTO Auth.ClientScopes(ClientId, Scope)
			VALUES(@ClientId, 'bluebookapi')

			INSERT INTO Auth.ClientScopes(ClientId, Scope)
			VALUES(@ClientId, 'offline_access')

			INSERT INTO Auth.ClientSecrets(ClientId, Value, Description, Expiration)
			VALUES(@ClientId, '/WLzgqIMqKpSuy6Sx21gZ5U084wDS+/gZFKt+GZmwlA=', 'Accounting', '2099-12-31 00:00:00.0000000 -06:00')
		END;
-----------------------------------------------------------------------------------
SET @ClientId = NULL
SELECT @ClientId = ClientId FROM Auth.Clients WHERE ClientCode = 'SLN' AND Flow = 3
	IF @ClientId IS NULL
		BEGIN
			INSERT INTO Auth.Clients(ClientCode, ClientName, Flow, IdentityTokenLifetime, AccessTokenLifetime, AuthorizationCodeLifetime, AbsoluteRefreshTokenLifetime, SlidingRefreshTokenLifetime, RefreshTokenUsage, RefreshTokenExpiration, AccessTokenType, UpdateAccessTokenOnRefresh, Enabled, LogoutSessionRequired, RequireSignOutPrompt, AllowAccessToAllScopes, AllowClientCredentialsOnly, RequireConsent, AllowRememberConsent, EnableLocalLogin, IncludeJwtId, AlwaysSendClientClaims, PrefixClientClaims, AllowAccessToAllGrantTypes) 
			VALUES('SLN', 'Our Parents', 3, 360, 3600, 360, 86400, 3600, 0, 0, 0, 'True', 'True', 'False', 'False', 'True', 'False', 'False', 'True', 'True', 'True', 'True', 'True', 'True')

			SELECT @ClientId=SCOPE_IDENTITY()

			INSERT INTO Auth.ClientScopes(ClientId, Scope)
			VALUES(@ClientId, 'bluebookapi')

			INSERT INTO Auth.ClientScopes(ClientId, Scope)
			VALUES(@ClientId, 'offline_access')

			INSERT INTO Auth.ClientSecrets(ClientId, Value, Description, Expiration)
			VALUES(@ClientId, '0aGvwXNcJ3wZPc7qWDDzlz6RT7yvufhfIRYzIli5BCo=', 'Our Parents', '2099-12-31 00:00:00.0000000 -06:00')
		END;
-----------------------------------------------------------------------------------
SET @ClientId = NULL
SELECT @ClientId = ClientId FROM Auth.Clients WHERE ClientCode = 'PSH' AND Flow = 3
	IF @ClientId IS NULL
		BEGIN
			INSERT INTO Auth.Clients(ClientCode, ClientName, Flow, IdentityTokenLifetime, AccessTokenLifetime, AuthorizationCodeLifetime, AbsoluteRefreshTokenLifetime, SlidingRefreshTokenLifetime, RefreshTokenUsage, RefreshTokenExpiration, AccessTokenType, UpdateAccessTokenOnRefresh, Enabled, LogoutSessionRequired, RequireSignOutPrompt, AllowAccessToAllScopes, AllowClientCredentialsOnly, RequireConsent, AllowRememberConsent, EnableLocalLogin, IncludeJwtId, AlwaysSendClientClaims, PrefixClientClaims, AllowAccessToAllGrantTypes) 
			VALUES('PSH', 'Preferred Senior Housing (temporary)', 3, 360, 3600, 360, 86400, 3600, 0, 0, 0, 'True', 'True', 'False', 'False', 'True', 'False', 'False', 'True', 'True', 'True', 'True', 'True', 'True')

			SELECT @ClientId=SCOPE_IDENTITY()

			INSERT INTO Auth.ClientScopes(ClientId, Scope)
			VALUES(@ClientId, 'bluebookapi')

			INSERT INTO Auth.ClientScopes(ClientId, Scope)
			VALUES(@ClientId, 'offline_access')

			INSERT INTO Auth.ClientSecrets(ClientId, Value, Description, Expiration)
			VALUES(@ClientId, 'ZatDnMRyiT0XYUCRiWFPfJvZ3EgneeCATL8wlyg1zsQ=', 'Preferred Senior Housing (temporary)', '2099-12-31 00:00:00.0000000 -06:00')
		END;
-----------------------------------------------------------------------------------
SET @ClientId = NULL
SELECT @ClientId = ClientId FROM Auth.Clients WHERE ClientCode = 'NOVA' AND Flow = 3
	IF @ClientId IS NULL
		BEGIN
			INSERT INTO Auth.Clients(ClientCode, ClientName, Flow, IdentityTokenLifetime, AccessTokenLifetime, AuthorizationCodeLifetime, AbsoluteRefreshTokenLifetime, SlidingRefreshTokenLifetime, RefreshTokenUsage, RefreshTokenExpiration, AccessTokenType, UpdateAccessTokenOnRefresh, Enabled, LogoutSessionRequired, RequireSignOutPrompt, AllowAccessToAllScopes, AllowClientCredentialsOnly, RequireConsent, AllowRememberConsent, EnableLocalLogin, IncludeJwtId, AlwaysSendClientClaims, PrefixClientClaims, AllowAccessToAllGrantTypes) 
			VALUES('NOVA', 'Supernova', 3, 360, 3600, 360, 86400, 3600, 0, 0, 0, 'True', 'True', 'False', 'False', 'True', 'False', 'False', 'True', 'True', 'True', 'True', 'True', 'True')

			SELECT @ClientId=SCOPE_IDENTITY()

			INSERT INTO Auth.ClientScopes(ClientId, Scope)
			VALUES(@ClientId, 'bluebookapi')

			INSERT INTO Auth.ClientScopes(ClientId, Scope)
			VALUES(@ClientId, 'offline_access')

			INSERT INTO Auth.ClientSecrets(ClientId, Value, Description, Expiration)
			VALUES(@ClientId, '7VaNweh0yF2WnIVv7pxKiBc2kiAA2KPJtxliKJG0qUg=', 'Supernova', '2099-12-31 00:00:00.0000000 -06:00')
		END;
-----------------------------------------------------------------------------------
SET @ClientId = NULL
SELECT @ClientId = ClientId FROM Auth.Clients WHERE ClientCode = 'WH' AND Flow = 3
	IF @ClientId IS NULL
		BEGIN
			INSERT INTO Auth.Clients(ClientCode, ClientName, Flow, IdentityTokenLifetime, AccessTokenLifetime, AuthorizationCodeLifetime, AbsoluteRefreshTokenLifetime, SlidingRefreshTokenLifetime, RefreshTokenUsage, RefreshTokenExpiration, AccessTokenType, UpdateAccessTokenOnRefresh, Enabled, LogoutSessionRequired, RequireSignOutPrompt, AllowAccessToAllScopes, AllowClientCredentialsOnly, RequireConsent, AllowRememberConsent, EnableLocalLogin, IncludeJwtId, AlwaysSendClientClaims, PrefixClientClaims, AllowAccessToAllGrantTypes) 
			VALUES('WH', 'Welcome Home', 3, 360, 3600, 360, 86400, 3600, 0, 0, 0, 'True', 'True', 'False', 'False', 'True', 'False', 'False', 'True', 'True', 'True', 'True', 'True', 'True')

			SELECT @ClientId=SCOPE_IDENTITY()

			INSERT INTO Auth.ClientScopes(ClientId, Scope)
			VALUES(@ClientId, 'bluebookapi')

			INSERT INTO Auth.ClientScopes(ClientId, Scope)
			VALUES(@ClientId, 'offline_access')

			INSERT INTO Auth.ClientSecrets(ClientId, Value, Description, Expiration)
			VALUES(@ClientId, 'ue+OblCu+MOjAKirIWtbUzHwPAqNr0zwR0uvtiQxyvA=', 'Welcome Home', '2099-12-31 00:00:00.0000000 -06:00')
		END;
-----------------------------------------------------------------------------------
SET @ClientId = NULL
SELECT @ClientId = ClientId FROM Auth.Clients WHERE ClientCode = 'SS IG' AND Flow = 3
	IF @ClientId IS NULL
		BEGIN
			INSERT INTO Auth.Clients(ClientCode, ClientName, Flow, IdentityTokenLifetime, AccessTokenLifetime, AuthorizationCodeLifetime, AbsoluteRefreshTokenLifetime, SlidingRefreshTokenLifetime, RefreshTokenUsage, RefreshTokenExpiration, AccessTokenType, UpdateAccessTokenOnRefresh, Enabled, LogoutSessionRequired, RequireSignOutPrompt, AllowAccessToAllScopes, AllowClientCredentialsOnly, RequireConsent, AllowRememberConsent, EnableLocalLogin, IncludeJwtId, AlwaysSendClientClaims, PrefixClientClaims, AllowAccessToAllGrantTypes) 
			VALUES('SS IG', 'Dallas Self Storage (Infogroup)', 3, 360, 3600, 360, 86400, 3600, 0, 0, 0, 'True', 'True', 'False', 'False', 'True', 'False', 'False', 'True', 'True', 'True', 'True', 'True', 'True')

			SELECT @ClientId=SCOPE_IDENTITY()

			INSERT INTO Auth.ClientScopes(ClientId, Scope)
			VALUES(@ClientId, 'bluebookapi')

			INSERT INTO Auth.ClientScopes(ClientId, Scope)
			VALUES(@ClientId, 'offline_access')

			INSERT INTO Auth.ClientSecrets(ClientId, Value, Description, Expiration)
			VALUES(@ClientId, 'r0YLyUOgB3PpGxaMTQkotHMUdbjltKCtaowvpYwBNyw=', 'Dallas Self Storage (Infogroup)', '2099-12-31 00:00:00.0000000 -06:00')
		END;
-----------------------------------------------------------------------------------
SET @ClientId = NULL
SELECT @ClientId = ClientId FROM Auth.Clients WHERE ClientCode = 'SS DBUSA' AND Flow = 3
	IF @ClientId IS NULL
		BEGIN
			INSERT INTO Auth.Clients(ClientCode, ClientName, Flow, IdentityTokenLifetime, AccessTokenLifetime, AuthorizationCodeLifetime, AbsoluteRefreshTokenLifetime, SlidingRefreshTokenLifetime, RefreshTokenUsage, RefreshTokenExpiration, AccessTokenType, UpdateAccessTokenOnRefresh, Enabled, LogoutSessionRequired, RequireSignOutPrompt, AllowAccessToAllScopes, AllowClientCredentialsOnly, RequireConsent, AllowRememberConsent, EnableLocalLogin, IncludeJwtId, AlwaysSendClientClaims, PrefixClientClaims, AllowAccessToAllGrantTypes) 
			VALUES('SS DBUSA', 'Dallas Self Storage (DatabaseUSA)', 3, 360, 3600, 360, 86400, 3600, 0, 0, 0, 'True', 'True', 'False', 'False', 'True', 'False', 'False', 'True', 'True', 'True', 'True', 'True', 'True')

			SELECT @ClientId=SCOPE_IDENTITY()

			INSERT INTO Auth.ClientScopes(ClientId, Scope)
			VALUES(@ClientId, 'bluebookapi')

			INSERT INTO Auth.ClientScopes(ClientId, Scope)
			VALUES(@ClientId, 'offline_access')

			INSERT INTO Auth.ClientSecrets(ClientId, Value, Description, Expiration)
			VALUES(@ClientId, '3UaMVbcb0kdybGEtvOH5veShQ+hlhfnZZLWWm2hYd0Q=', 'Dallas Self Storage (DatabaseUSA)', '2099-12-31 00:00:00.0000000 -06:00')
		END;
-----------------------------------------------------------------------------------
SET @ClientId = NULL
SELECT @ClientId = ClientId FROM Auth.Clients WHERE ClientCode = 'BK' AND Flow = 3
	IF @ClientId IS NULL
		BEGIN
			INSERT INTO Auth.Clients(ClientCode, ClientName, Flow, IdentityTokenLifetime, AccessTokenLifetime, AuthorizationCodeLifetime, AbsoluteRefreshTokenLifetime, SlidingRefreshTokenLifetime, RefreshTokenUsage, RefreshTokenExpiration, AccessTokenType, UpdateAccessTokenOnRefresh, Enabled, LogoutSessionRequired, RequireSignOutPrompt, AllowAccessToAllScopes, AllowClientCredentialsOnly, RequireConsent, AllowRememberConsent, EnableLocalLogin, IncludeJwtId, AlwaysSendClientClaims, PrefixClientClaims, AllowAccessToAllGrantTypes) 
			VALUES('BK', 'BlackKnight Data', 3, 360, 3600, 360, 86400, 3600, 0, 0, 0, 'True', 'True', 'False', 'False', 'True', 'False', 'False', 'True', 'True', 'True', 'True', 'True', 'True')

			SELECT @ClientId=SCOPE_IDENTITY()

			INSERT INTO Auth.ClientScopes(ClientId, Scope)
			VALUES(@ClientId, 'bluebookapi')

			INSERT INTO Auth.ClientScopes(ClientId, Scope)
			VALUES(@ClientId, 'offline_access')

			INSERT INTO Auth.ClientSecrets(ClientId, Value, Description, Expiration)
			VALUES(@ClientId, '6up19UjpOVRSsp8bPrCbvH1yxJvrST9nxObRD4IX/D4=', 'BlackKnight Data', '2099-12-31 00:00:00.0000000 -06:00')
		END;
-----------------------------------------------------------------------------------
SET @ClientId = NULL
SELECT @ClientId = ClientId FROM Auth.Clients WHERE ClientCode = 'PW' AND Flow = 3
	IF @ClientId IS NULL
		BEGIN
			INSERT INTO Auth.Clients(ClientCode, ClientName, Flow, IdentityTokenLifetime, AccessTokenLifetime, AuthorizationCodeLifetime, AbsoluteRefreshTokenLifetime, SlidingRefreshTokenLifetime, RefreshTokenUsage, RefreshTokenExpiration, AccessTokenType, UpdateAccessTokenOnRefresh, Enabled, LogoutSessionRequired, RequireSignOutPrompt, AllowAccessToAllScopes, AllowClientCredentialsOnly, RequireConsent, AllowRememberConsent, EnableLocalLogin, IncludeJwtId, AlwaysSendClientClaims, PrefixClientClaims, AllowAccessToAllGrantTypes) 
			VALUES('PW', 'Propertyware', 3, 360, 3600, 360, 86400, 3600, 0, 0, 0, 'True', 'True', 'False', 'False', 'True', 'False', 'False', 'True', 'True', 'True', 'True', 'True', 'True')

			SELECT @ClientId=SCOPE_IDENTITY()

			INSERT INTO Auth.ClientScopes(ClientId, Scope)
			VALUES(@ClientId, 'bluebookapi')

			INSERT INTO Auth.ClientScopes(ClientId, Scope)
			VALUES(@ClientId, 'offline_access')

			INSERT INTO Auth.ClientSecrets(ClientId, Value, Description, Expiration)
			VALUES(@ClientId, 'cylqNzWcFvz/3XY2uL1N9C4Wu5aujvHSBWJApP4OaKY=', 'Propertyware', '2099-12-31 00:00:00.0000000 -06:00')
		END;
-----------------------------------------------------------------------------------
SET @ClientId = NULL
SELECT @ClientId = ClientId FROM Auth.Clients WHERE ClientCode = 'BBA' AND Flow = 3
	IF @ClientId IS NULL
		BEGIN
			INSERT INTO Auth.Clients(ClientCode, ClientName, Flow, IdentityTokenLifetime, AccessTokenLifetime, AuthorizationCodeLifetime, AbsoluteRefreshTokenLifetime, SlidingRefreshTokenLifetime, RefreshTokenUsage, RefreshTokenExpiration, AccessTokenType, UpdateAccessTokenOnRefresh, Enabled, LogoutSessionRequired, RequireSignOutPrompt, AllowAccessToAllScopes, AllowClientCredentialsOnly, RequireConsent, AllowRememberConsent, EnableLocalLogin, IncludeJwtId, AlwaysSendClientClaims, PrefixClientClaims, AllowAccessToAllGrantTypes) 
			VALUES('BBA', 'Blackbook Application', 3, 360, 3600, 360, 86400, 3600, 0, 0, 0, 'True', 'True', 'False', 'False', 'True', 'False', 'False', 'True', 'True', 'True', 'True', 'True', 'True')

			SELECT @ClientId=SCOPE_IDENTITY()

			INSERT INTO Auth.ClientScopes(ClientId, Scope)
			VALUES(@ClientId, 'bluebookapi')

			INSERT INTO Auth.ClientScopes(ClientId, Scope)
			VALUES(@ClientId, 'offline_access')

			INSERT INTO Auth.ClientSecrets(ClientId, Value, Description, Expiration)
			VALUES(@ClientId, 'mmj5NLagcxLRWWp6l275Tob3kbtrLk/IpwxzpicW6c8=', 'Blackbook Application', '2099-12-31 00:00:00.0000000 -06:00')
		END;
-----------------------------------------------------------------------------------
SET @ClientId = NULL
SELECT @ClientId = ClientId FROM Auth.Clients WHERE ClientCode = 'BBQA' AND Flow = 3
	IF @ClientId IS NULL
		BEGIN
			INSERT INTO Auth.Clients(ClientCode, ClientName, Flow, IdentityTokenLifetime, AccessTokenLifetime, AuthorizationCodeLifetime, AbsoluteRefreshTokenLifetime, SlidingRefreshTokenLifetime, RefreshTokenUsage, RefreshTokenExpiration, AccessTokenType, UpdateAccessTokenOnRefresh, Enabled, LogoutSessionRequired, RequireSignOutPrompt, AllowAccessToAllScopes, AllowClientCredentialsOnly, RequireConsent, AllowRememberConsent, EnableLocalLogin, IncludeJwtId, AlwaysSendClientClaims, PrefixClientClaims, AllowAccessToAllGrantTypes) 
			VALUES('BBQA', 'BlackBook QA', 3, 360, 3600, 360, 86400, 3600, 0, 0, 0, 'True', 'True', 'False', 'False', 'True', 'False', 'False', 'True', 'True', 'True', 'True', 'True', 'True')

			SELECT @ClientId=SCOPE_IDENTITY()

			INSERT INTO Auth.ClientScopes(ClientId, Scope)
			VALUES(@ClientId, 'bluebookapi')

			INSERT INTO Auth.ClientScopes(ClientId, Scope)
			VALUES(@ClientId, 'offline_access')

			INSERT INTO Auth.ClientSecrets(ClientId, Value, Description, Expiration)
			VALUES(@ClientId, 'U75vw9swkUuPLNmD6b9m0pN5iVux7h13AbHQLOUZpb4=', 'BlackBook QA', '2099-12-31 00:00:00.0000000 -06:00')
		END;
-----------------------------------------------------------------------------------
SET @ClientId = NULL
SELECT @ClientId = ClientId FROM Auth.Clients WHERE ClientCode = 'DAM' AND Flow = 3
	IF @ClientId IS NULL
		BEGIN
			INSERT INTO Auth.Clients(ClientCode, ClientName, Flow, IdentityTokenLifetime, AccessTokenLifetime, AuthorizationCodeLifetime, AbsoluteRefreshTokenLifetime, SlidingRefreshTokenLifetime, RefreshTokenUsage, RefreshTokenExpiration, AccessTokenType, UpdateAccessTokenOnRefresh, Enabled, LogoutSessionRequired, RequireSignOutPrompt, AllowAccessToAllScopes, AllowClientCredentialsOnly, RequireConsent, AllowRememberConsent, EnableLocalLogin, IncludeJwtId, AlwaysSendClientClaims, PrefixClientClaims, AllowAccessToAllGrantTypes) 
			VALUES('DAM', 'Digital Asset Management', 3, 360, 3600, 360, 86400, 3600, 0, 0, 0, 'True', 'True', 'False', 'False', 'True', 'False', 'False', 'True', 'True', 'True', 'True', 'True', 'True')

			SELECT @ClientId=SCOPE_IDENTITY()

			INSERT INTO Auth.ClientScopes(ClientId, Scope)
			VALUES(@ClientId, 'bluebookapi')

			INSERT INTO Auth.ClientScopes(ClientId, Scope)
			VALUES(@ClientId, 'offline_access')

			INSERT INTO Auth.ClientSecrets(ClientId, Value, Description, Expiration)
			VALUES(@ClientId, 'PGwrr0z9xojjhpZfEPvKka5yhqVpFs8l5SF0Yhp6JRI=', 'Digital Asset Management', '2099-12-31 00:00:00.0000000 -06:00')
		END;
-----------------------------------------------------------------------------------
SET @ClientId = NULL
SELECT @ClientId = ClientId FROM Auth.Clients WHERE ClientCode = 'PVPC' AND Flow = 3
	IF @ClientId IS NULL
		BEGIN
			INSERT INTO Auth.Clients(ClientCode, ClientName, Flow, IdentityTokenLifetime, AccessTokenLifetime, AuthorizationCodeLifetime, AbsoluteRefreshTokenLifetime, SlidingRefreshTokenLifetime, RefreshTokenUsage, RefreshTokenExpiration, AccessTokenType, UpdateAccessTokenOnRefresh, Enabled, LogoutSessionRequired, RequireSignOutPrompt, AllowAccessToAllScopes, AllowClientCredentialsOnly, RequireConsent, AllowRememberConsent, EnableLocalLogin, IncludeJwtId, AlwaysSendClientClaims, PrefixClientClaims, AllowAccessToAllGrantTypes) 
			VALUES('PVPC', 'PVPC Application', 3, 360, 3600, 360, 86400, 3600, 0, 0, 0, 'True', 'True', 'False', 'False', 'True', 'False', 'False', 'True', 'True', 'True', 'True', 'True', 'True')

			SELECT @ClientId=SCOPE_IDENTITY()

			INSERT INTO Auth.ClientScopes(ClientId, Scope)
			VALUES(@ClientId, 'bluebookapi')

			INSERT INTO Auth.ClientScopes(ClientId, Scope)
			VALUES(@ClientId, 'offline_access')

			INSERT INTO Auth.ClientSecrets(ClientId, Value, Description, Expiration)
			VALUES(@ClientId, 'dfJVYAIebuioMpgVNQ6yxb1ZHDdgi2UiHsVLxllvSGI=', 'PVPC Application', '2099-12-31 00:00:00.0000000 -06:00')
		END;
-----------------------------------------------------------------------------------
SET @ClientId = NULL
SELECT @ClientId = ClientId FROM Auth.Clients WHERE ClientCode = 'STARS' AND Flow = 3
	IF @ClientId IS NULL
		BEGIN
			INSERT INTO Auth.Clients(ClientCode, ClientName, Flow, IdentityTokenLifetime, AccessTokenLifetime, AuthorizationCodeLifetime, AbsoluteRefreshTokenLifetime, SlidingRefreshTokenLifetime, RefreshTokenUsage, RefreshTokenExpiration, AccessTokenType, UpdateAccessTokenOnRefresh, Enabled, LogoutSessionRequired, RequireSignOutPrompt, AllowAccessToAllScopes, AllowClientCredentialsOnly, RequireConsent, AllowRememberConsent, EnableLocalLogin, IncludeJwtId, AlwaysSendClientClaims, PrefixClientClaims, AllowAccessToAllGrantTypes) 
			VALUES('STARS', 'Stars', 3, 360, 3600, 360, 86400, 3600, 0, 0, 0, 'True', 'True', 'False', 'False', 'True', 'False', 'False', 'True', 'True', 'True', 'True', 'True', 'True')

			SELECT @ClientId=SCOPE_IDENTITY()

			INSERT INTO Auth.ClientScopes(ClientId, Scope)
			VALUES(@ClientId, 'bluebookapi')

			INSERT INTO Auth.ClientScopes(ClientId, Scope)
			VALUES(@ClientId, 'offline_access')

			INSERT INTO Auth.ClientSecrets(ClientId, Value, Description, Expiration)
			VALUES(@ClientId, 'FtzizddG7gPlTH9m3RgUf7hUjD6pcgznLUnTemfZRCA=', 'Stars', '2099-12-31 00:00:00.0000000 -06:00')
		END;
-----------------------------------------------------------------------------------
SET @ClientId = NULL
SELECT @ClientId = ClientId FROM Auth.Clients WHERE ClientCode = 'UC' AND Flow = 3
	IF @ClientId IS NULL
		BEGIN
			INSERT INTO Auth.Clients(ClientCode, ClientName, Flow, IdentityTokenLifetime, AccessTokenLifetime, AuthorizationCodeLifetime, AbsoluteRefreshTokenLifetime, SlidingRefreshTokenLifetime, RefreshTokenUsage, RefreshTokenExpiration, AccessTokenType, UpdateAccessTokenOnRefresh, Enabled, LogoutSessionRequired, RequireSignOutPrompt, AllowAccessToAllScopes, AllowClientCredentialsOnly, RequireConsent, AllowRememberConsent, EnableLocalLogin, IncludeJwtId, AlwaysSendClientClaims, PrefixClientClaims, AllowAccessToAllGrantTypes) 
			VALUES('UC', 'Unified Communications', 3, 360, 3600, 360, 86400, 3600, 0, 0, 0, 'True', 'True', 'False', 'False', 'True', 'False', 'False', 'True', 'True', 'True', 'True', 'True', 'True')

			SELECT @ClientId=SCOPE_IDENTITY()

			INSERT INTO Auth.ClientScopes(ClientId, Scope)
			VALUES(@ClientId, 'bluebookapi')

			INSERT INTO Auth.ClientScopes(ClientId, Scope)
			VALUES(@ClientId, 'offline_access')

			INSERT INTO Auth.ClientSecrets(ClientId, Value, Description, Expiration)
			VALUES(@ClientId, '4zg8fp3DfO3mG+zN4RZXRO5hvBZJcZtePw9z8BdjOvo=', 'Unified Communications', '2099-12-31 00:00:00.0000000 -06:00')
		END;
-----------------------------------------------------------------------------------
SET @ClientId = NULL
SELECT @ClientId = ClientId FROM Auth.Clients WHERE ClientCode = 'LL' AND Flow = 3
	IF @ClientId IS NULL
		BEGIN
			INSERT INTO Auth.Clients(ClientCode, ClientName, Flow, IdentityTokenLifetime, AccessTokenLifetime, AuthorizationCodeLifetime, AbsoluteRefreshTokenLifetime, SlidingRefreshTokenLifetime, RefreshTokenUsage, RefreshTokenExpiration, AccessTokenType, UpdateAccessTokenOnRefresh, Enabled, LogoutSessionRequired, RequireSignOutPrompt, AllowAccessToAllScopes, AllowClientCredentialsOnly, RequireConsent, AllowRememberConsent, EnableLocalLogin, IncludeJwtId, AlwaysSendClientClaims, PrefixClientClaims, AllowAccessToAllGrantTypes) 
			VALUES('LL', 'Legal Log', 3, 360, 3600, 360, 86400, 3600, 0, 0, 0, 'True', 'True', 'False', 'False', 'True', 'False', 'False', 'True', 'True', 'True', 'True', 'True', 'True')

			SELECT @ClientId=SCOPE_IDENTITY()

			INSERT INTO Auth.ClientScopes(ClientId, Scope)
			VALUES(@ClientId, 'bluebookapi')

			INSERT INTO Auth.ClientScopes(ClientId, Scope)
			VALUES(@ClientId, 'offline_access')

			INSERT INTO Auth.ClientSecrets(ClientId, Value, Description, Expiration)
			VALUES(@ClientId, '71/YhBXhv084yJMevErFkJf2Dh6TYqNbrELyb/KIE8A=', 'Legal Log', '2099-12-31 00:00:00.0000000 -06:00')
		END;
-----------------------------------------------------------------------------------
SET @ClientId = NULL
SELECT @ClientId = ClientId FROM Auth.Clients WHERE ClientCode = 'WEB' AND Flow = 3
	IF @ClientId IS NULL
		BEGIN
			INSERT INTO Auth.Clients(ClientCode, ClientName, Flow, IdentityTokenLifetime, AccessTokenLifetime, AuthorizationCodeLifetime, AbsoluteRefreshTokenLifetime, SlidingRefreshTokenLifetime, RefreshTokenUsage, RefreshTokenExpiration, AccessTokenType, UpdateAccessTokenOnRefresh, Enabled, LogoutSessionRequired, RequireSignOutPrompt, AllowAccessToAllScopes, AllowClientCredentialsOnly, RequireConsent, AllowRememberConsent, EnableLocalLogin, IncludeJwtId, AlwaysSendClientClaims, PrefixClientClaims, AllowAccessToAllGrantTypes) 
			VALUES('WEB', 'Andrews thing', 3, 360, 3600, 360, 86400, 3600, 0, 0, 0, 'True', 'True', 'False', 'False', 'True', 'False', 'False', 'True', 'True', 'True', 'True', 'True', 'True')

			SELECT @ClientId=SCOPE_IDENTITY()

			INSERT INTO Auth.ClientScopes(ClientId, Scope)
			VALUES(@ClientId, 'bluebookapi')

			INSERT INTO Auth.ClientScopes(ClientId, Scope)
			VALUES(@ClientId, 'offline_access')

			INSERT INTO Auth.ClientSecrets(ClientId, Value, Description, Expiration)
			VALUES(@ClientId, 'IUiKUw3AStbSvX8fQHnALNMqXXVFopfE5D59wy9XQj4=', 'Andrews thing', '2099-12-31 00:00:00.0000000 -06:00')
		END;
-----------------------------------------------------------------------------------
SET @ClientId = NULL
SELECT @ClientId = ClientId FROM Auth.Clients WHERE ClientCode = 'RPISF' AND Flow = 3
	IF @ClientId IS NULL
		BEGIN
			INSERT INTO Auth.Clients(ClientCode, ClientName, Flow, IdentityTokenLifetime, AccessTokenLifetime, AuthorizationCodeLifetime, AbsoluteRefreshTokenLifetime, SlidingRefreshTokenLifetime, RefreshTokenUsage, RefreshTokenExpiration, AccessTokenType, UpdateAccessTokenOnRefresh, Enabled, LogoutSessionRequired, RequireSignOutPrompt, AllowAccessToAllScopes, AllowClientCredentialsOnly, RequireConsent, AllowRememberConsent, EnableLocalLogin, IncludeJwtId, AlwaysSendClientClaims, PrefixClientClaims, AllowAccessToAllGrantTypes) 
			VALUES('RPISF', 'RP Salesforce', 3, 360, 3600, 360, 86400, 3600, 0, 0, 0, 'True', 'True', 'False', 'False', 'True', 'False', 'False', 'True', 'True', 'True', 'True', 'True', 'True')

			SELECT @ClientId=SCOPE_IDENTITY()

			INSERT INTO Auth.ClientScopes(ClientId, Scope)
			VALUES(@ClientId, 'bluebookapi')

			INSERT INTO Auth.ClientScopes(ClientId, Scope)
			VALUES(@ClientId, 'offline_access')

			INSERT INTO Auth.ClientSecrets(ClientId, Value, Description, Expiration)
			VALUES(@ClientId, 'rAuqnv2CISmKnGv7HV/1Gq5P4GMfasi+RrYy3P5Fiy4=', 'RP Salesforce', '2099-12-31 00:00:00.0000000 -06:00')
		END;
-----------------------------------------------------------------------------------
SET @ClientId = NULL
SELECT @ClientId = ClientId FROM Auth.Clients WHERE ClientCode = 'RPAPP' AND Flow = 3
	IF @ClientId IS NULL
		BEGIN
			INSERT INTO Auth.Clients(ClientCode, ClientName, Flow, IdentityTokenLifetime, AccessTokenLifetime, AuthorizationCodeLifetime, AbsoluteRefreshTokenLifetime, SlidingRefreshTokenLifetime, RefreshTokenUsage, RefreshTokenExpiration, AccessTokenType, UpdateAccessTokenOnRefresh, Enabled, LogoutSessionRequired, RequireSignOutPrompt, AllowAccessToAllScopes, AllowClientCredentialsOnly, RequireConsent, AllowRememberConsent, EnableLocalLogin, IncludeJwtId, AlwaysSendClientClaims, PrefixClientClaims, AllowAccessToAllGrantTypes) 
			VALUES('RPAPP', 'RealPage Application', 3, 360, 3600, 360, 86400, 3600, 0, 0, 0, 'True', 'True', 'False', 'False', 'True', 'False', 'False', 'True', 'True', 'True', 'True', 'True', 'True')

			SELECT @ClientId=SCOPE_IDENTITY()

			INSERT INTO Auth.ClientScopes(ClientId, Scope)
			VALUES(@ClientId, 'bluebookapi')

			INSERT INTO Auth.ClientScopes(ClientId, Scope)
			VALUES(@ClientId, 'offline_access')

			INSERT INTO Auth.ClientSecrets(ClientId, Value, Description, Expiration)
			VALUES(@ClientId, 'hSZiVlqCT0vEXPW/AiFRNFelXblr/N0fF7iR5gaOCJg=', 'RealPage Application', '2099-12-31 00:00:00.0000000 -06:00')
		END;
-----------------------------------------------------------------------------------

EXEC sys.sp_updateextendedproperty @name=N'Build', @value='31'