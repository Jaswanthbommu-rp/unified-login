IF NOT EXISTS
(
    SELECT 1
    FROM Auth.Scopes
    WHERE Name = 'blackbook'
)
BEGIN
	INSERT INTO Auth.Scopes(Name, DisplayName, Enabled, Required, Emphasize, Type, IncludeAllClaimsForUser, ShowInDiscoveryDocument, AllowUnrestrictedIntrospection) 
	VALUES('blackbook', 'BlackBook', 1, 0, 0, 1, 1, 1, 0)
END;
-----------------------------------------------------------------------------------
SET @ClientId = NULL
SELECT @ClientId = ClientId FROM Auth.Clients WHERE ClientCode = 'blackbook' AND Flow = 0
	IF @ClientId IS NULL
		BEGIN
			INSERT INTO Auth.Clients(ClientCode, ClientName, Flow, IdentityTokenLifetime, AccessTokenLifetime, AuthorizationCodeLifetime, AbsoluteRefreshTokenLifetime, SlidingRefreshTokenLifetime, RefreshTokenUsage, RefreshTokenExpiration, AccessTokenType, UpdateAccessTokenOnRefresh, Enabled, LogoutSessionRequired, RequireSignOutPrompt, AllowAccessToAllScopes, AllowClientCredentialsOnly, RequireConsent, AllowRememberConsent, EnableLocalLogin, IncludeJwtId, AlwaysSendClientClaims, PrefixClientClaims, AllowAccessToAllGrantTypes) 
			VALUES('blackbook', 'Blackbook Application', 0, 360, 3600, 360, 86400, 3600, 0, 0, 0, 'True', 'True', 'False', 'False', 'True', 'False', 'False', 'True', 'True', 'True', 'True', 'True', 'True')

			SELECT @ClientId=SCOPE_IDENTITY()

			INSERT INTO Auth.ClientScopes(ClientId, Scope)
			VALUES(@ClientId, 'blackbook')

			INSERT INTO Auth.ClientScopes(ClientId, Scope)
			VALUES(@ClientId, 'bluebookapi')

			INSERT INTO Auth.ClientScopes(ClientId, Scope)
			VALUES(@ClientId, 'offline_access')

			INSERT INTO Auth.ClientRedirectUris(ClientId, Uri)
			VALUES(@ClientId, 'https://blackbookresearch-dev.realpage.com')

			INSERT INTO Auth.ClientRedirectUris(ClientId, Uri)
			VALUES(@ClientId, 'https://blackbookresearch-qa.realpage.com')

			INSERT INTO Auth.ClientRedirectUris(ClientId, Uri)
			VALUES(@ClientId, 'https://blackbook-staging.realpage.com')

			INSERT INTO Auth.ClientRedirectUris(ClientId, Uri)
			VALUES(@ClientId, 'https://blackbookresearch.realpage.com')

			INSERT INTO Auth.ClientSecrets(ClientId, Value, Description, Expiration)
			VALUES(@ClientId, 'QwscCGLe7efLZ959e11yxJCZ93bTubOJqwq96NfP6U8=', 'Blackbook Application', '2099-12-31 00:00:00.0000000 -06:00')
		END;

-----------------------------------------------------------------------------------

EXEC sys.sp_updateextendedproperty @name=N'Build', @value='33'