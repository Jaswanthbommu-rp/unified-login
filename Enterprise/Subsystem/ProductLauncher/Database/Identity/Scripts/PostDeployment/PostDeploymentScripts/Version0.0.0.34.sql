-- Add Client On-Site

--select ClientCode,ClientName,ClientUri,LogoUri,Flow,LogoutUri,IdentityTokenLifetime,AccessTokenLifetime,AuthorizationCodeLifetime,AbsoluteRefreshTokenLifetime,SlidingRefreshTokenLifetime,RefreshTokenUsage,RefreshTokenExpiration,AccessTokenType,UpdateAccessTokenOnRefresh,Enabled,LogoutSessionRequired,RequireSignOutPrompt,AllowAccessToAllScopes,AllowClientCredentialsOnly,RequireConsent,AllowRememberConsent,EnableLocalLogin,IncludeJwtId,AlwaysSendClientClaims,PrefixClientClaims,AllowAccessToAllGrantTypes from Auth.Clients where clientcode = 'onsite'
--declare @ClientId int

-----------------------------------------------------------------------------------

SET @ClientId = NULL;

SELECT @ClientId = ClientId
FROM Auth.Clients
WHERE ClientCode = 'onsite' AND 
	  Flow = 1;

IF @ClientId IS NULL
BEGIN
	INSERT INTO Auth.Clients( ClientCode, ClientName, ClientUri, LogoUri, Flow, LogoutUri, IdentityTokenLifetime, AccessTokenLifetime, AuthorizationCodeLifetime, AbsoluteRefreshTokenLifetime, SlidingRefreshTokenLifetime, RefreshTokenUsage, RefreshTokenExpiration, AccessTokenType, UpdateAccessTokenOnRefresh, Enabled, LogoutSessionRequired, RequireSignOutPrompt, AllowAccessToAllScopes, AllowClientCredentialsOnly, RequireConsent, AllowRememberConsent, EnableLocalLogin, IncludeJwtId, AlwaysSendClientClaims, PrefixClientClaims, AllowAccessToAllGrantTypes )
	VALUES( 'onsite', 'On-Site', 'http://onsite', 'https://onesite', 1, 'https://staging2.on-site.com/web/session/destroy', 360, 3600, 360, 86400, 3600, 0, 0, 0, 1, 1, 1, 0, 1, 0, 0, 1, 1, 1, 1, 1, 1 );
	SELECT @ClientId = SCOPE_IDENTITY();
	INSERT INTO Auth.ClientScopes( ClientId, Scope )
	VALUES( @ClientId, 'openid' );
	INSERT INTO Auth.ClientScopes( ClientId, Scope )
	VALUES( @ClientId, 'profile' );
	INSERT INTO Auth.ClientRedirectUris( ClientId, Uri )
	VALUES( @ClientId, 'https://staging9.on-site.com/web/session' );
END;

-----------------------------------------------------------------------------------

EXEC sys.sp_updateextendedproperty @name = N'Build', @value = '35';
	
