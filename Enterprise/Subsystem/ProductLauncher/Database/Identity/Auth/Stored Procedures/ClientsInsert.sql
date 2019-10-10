CREATE PROCEDURE [Auth].[ClientsInsert]
(
	@ClientCode nvarchar(200),
	@ClientName nvarchar(200),
	@ClientUri nvarchar(2000),
	@LogoUri nvarchar(MAX),
	@Flow int,
	@LogoutUri nvarchar(MAX),
	@IdentityTokenLifetime int,
	@AccessTokenLifetime int,
	@AuthorizationCodeLifetime int,
	@AbsoluteRefreshTokenLifetime int,
	@SlidingRefreshTokenLifetime int,
	@RefreshTokenUsage int,
	@RefreshTokenExpiration int,
	@AccessTokenType int,
	@UpdateAccessTokenOnRefresh bit,
	@Enabled bit,
	@LogoutSessionRequired bit,
	@RequireSignOutPrompt bit,
	@AllowAccessToAllScopes bit,
	@AllowClientCredentialsOnly bit,
	@RequireConsent bit,
	@AllowRememberConsent bit,
	@EnableLocalLogin bit,
	@IncludeJwtId bit,
	@AlwaysSendClientClaims bit,
	@PrefixClientClaims bit,
	@AllowAccessToAllGrantTypes bit
)
AS
BEGIN
	SET NOCOUNT OFF;
	INSERT INTO [auth].[clients] 
	(
		[ClientCode]
		, [ClientName]
		, [ClientUri]
		, [LogoUri]
		, [Flow]
		, [LogoutUri]
		, [IdentityTokenLifetime]
		, [AccessTokenLifetime]
		, [AuthorizationCodeLifetime]
		, [AbsoluteRefreshTokenLifetime]
		, [SlidingRefreshTokenLifetime]
		, [RefreshTokenUsage]
		, [RefreshTokenExpiration]
		, [AccessTokenType]
		, [UpdateAccessTokenOnRefresh]
		, [Enabled]
		, [LogoutSessionRequired]
		, [RequireSignOutPrompt]
		, [AllowAccessToAllScopes]
		, [AllowClientCredentialsOnly]
		, [RequireConsent]
		, [AllowRememberConsent]
		, [EnableLocalLogin]
		, [IncludeJwtId]
		, [AlwaysSendClientClaims]
		, [PrefixClientClaims]
		, [AllowAccessToAllGrantTypes]
	) 
	VALUES 
	(
		@ClientCode
		, @ClientName
		, @ClientUri
		, @LogoUri
		, @Flow
		, @LogoutUri
		, @IdentityTokenLifetime
		, @AccessTokenLifetime
		, @AuthorizationCodeLifetime
		, @AbsoluteRefreshTokenLifetime
		, @SlidingRefreshTokenLifetime
		, @RefreshTokenUsage
		, @RefreshTokenExpiration
		, @AccessTokenType
		, @UpdateAccessTokenOnRefresh
		, @Enabled
		, @LogoutSessionRequired
		, @RequireSignOutPrompt
		, @AllowAccessToAllScopes
		, @AllowClientCredentialsOnly
		, @RequireConsent
		, @AllowRememberConsent
		, @EnableLocalLogin
		, @IncludeJwtId
		, @AlwaysSendClientClaims
		, @PrefixClientClaims
		, @AllowAccessToAllGrantTypes
	)
	
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
			(ClientId = SCOPE_IDENTITY())
END
