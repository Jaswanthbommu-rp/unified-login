---DEAD
CREATE PROCEDURE [Auth].[ClientsSelect]
(
	@ClientId INT = NULL
)
AS
BEGIN
	SET NOCOUNT OFF;
	
	SELECT 
		ClientId
		, ClientCode
		, ClientName
		, ClientUri
		, LogoUri
		, Flow
		, LogoutUri
		, IdentityTokenLifetime
		, AccessTokenLifetime
		, AuthorizationCodeLifetime
		, AbsoluteRefreshTokenLifetime
		, SlidingRefreshTokenLifetime
		, RefreshTokenUsage
		, RefreshTokenExpiration
		, AccessTokenType
		, UpdateAccessTokenOnRefresh
		, Enabled
		, LogoutSessionRequired
		, RequireSignOutPrompt
		, AllowAccessToAllScopes
		, AllowClientCredentialsOnly
		, RequireConsent
		, AllowRememberConsent
		, EnableLocalLogin
		, IncludeJwtId
		, AlwaysSendClientClaims
		, PrefixClientClaims
		, AllowAccessToAllGrantTypes 
	FROM Auth.Clients 
		WHERE 
			@ClientId IS NULL OR ClientId = @ClientId
END