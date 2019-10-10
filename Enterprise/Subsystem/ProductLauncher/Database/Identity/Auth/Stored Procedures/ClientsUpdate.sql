CREATE PROCEDURE [Auth].[ClientsUpdate]
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
	@AllowAccessToAllGrantTypes bit,
	@Original_ClientId int,
	@Original_ClientCode nvarchar(200),
	@Original_ClientName nvarchar(200),
	@IsNull_ClientUri Int,
	@Original_ClientUri nvarchar(2000),
	@Original_Flow int,
	@Original_IdentityTokenLifetime int,
	@Original_AccessTokenLifetime int,
	@Original_AuthorizationCodeLifetime int,
	@Original_AbsoluteRefreshTokenLifetime int,
	@Original_SlidingRefreshTokenLifetime int,
	@Original_RefreshTokenUsage int,
	@Original_RefreshTokenExpiration int,
	@Original_AccessTokenType int,
	@Original_UpdateAccessTokenOnRefresh bit,
	@Original_Enabled bit,
	@Original_LogoutSessionRequired bit,
	@Original_RequireSignOutPrompt bit,
	@Original_AllowAccessToAllScopes bit,
	@Original_AllowClientCredentialsOnly bit,
	@Original_RequireConsent bit,
	@Original_AllowRememberConsent bit,
	@Original_EnableLocalLogin bit,
	@Original_IncludeJwtId bit,
	@Original_AlwaysSendClientClaims bit,
	@Original_PrefixClientClaims bit,
	@Original_AllowAccessToAllGrantTypes bit,
	@ClientId int
)
AS
BEGIN
	SET NOCOUNT OFF;
	UPDATE [Auth].[Clients] 
		SET 
		  [ClientCode] = @ClientCode
		, [ClientName] = @ClientName
		, [ClientUri] = @ClientUri
		, [LogoUri] = @LogoUri
		, [Flow] = @Flow
		, [LogoutUri] = @LogoutUri
		, [IdentityTokenLifetime] = @IdentityTokenLifetime
		, [AccessTokenLifetime] = @AccessTokenLifetime
		, [AuthorizationCodeLifetime] = @AuthorizationCodeLifetime
		, [AbsoluteRefreshTokenLifetime] = @AbsoluteRefreshTokenLifetime
		, [SlidingRefreshTokenLifetime] = @SlidingRefreshTokenLifetime
		, [RefreshTokenUsage] = @RefreshTokenUsage
		, [RefreshTokenExpiration] = @RefreshTokenExpiration
		, [AccessTokenType] = @AccessTokenType
		, [UpdateAccessTokenOnRefresh] = @UpdateAccessTokenOnRefresh
		, [Enabled] = @Enabled
		, [LogoutSessionRequired] = @LogoutSessionRequired
		, [RequireSignOutPrompt] = @RequireSignOutPrompt
		, [AllowAccessToAllScopes] = @AllowAccessToAllScopes
		, [AllowClientCredentialsOnly] = @AllowClientCredentialsOnly
		, [RequireConsent] = @RequireConsent
		, [AllowRememberConsent] = @AllowRememberConsent
		, [EnableLocalLogin] = @EnableLocalLogin
		, [IncludeJwtId] = @IncludeJwtId
		, [AlwaysSendClientClaims] = @AlwaysSendClientClaims
		, [PrefixClientClaims] = @PrefixClientClaims
		, [AllowAccessToAllGrantTypes] = @AllowAccessToAllGrantTypes 
		WHERE 
		(
			([ClientId] = @Original_ClientId) 
			AND ([ClientCode] = @Original_ClientCode) 
			AND ([ClientName] = @Original_ClientName) 
			AND ((@IsNull_ClientUri = 1 AND [ClientUri] IS NULL) OR ([ClientUri] = @Original_ClientUri)) 
			AND ([Flow] = @Original_Flow) 
			AND ([IdentityTokenLifetime] = @Original_IdentityTokenLifetime) 
			AND ([AccessTokenLifetime] = @Original_AccessTokenLifetime) 
			AND ([AuthorizationCodeLifetime] = @Original_AuthorizationCodeLifetime) 
			AND ([AbsoluteRefreshTokenLifetime] = @Original_AbsoluteRefreshTokenLifetime) 
			AND ([SlidingRefreshTokenLifetime] = @Original_SlidingRefreshTokenLifetime) 
			AND ([RefreshTokenUsage] = @Original_RefreshTokenUsage) 
			AND ([RefreshTokenExpiration] = @Original_RefreshTokenExpiration) 
			AND ([AccessTokenType] = @Original_AccessTokenType) 
			AND ([UpdateAccessTokenOnRefresh] = @Original_UpdateAccessTokenOnRefresh) 
			AND ([Enabled] = @Original_Enabled) 
			AND ([LogoutSessionRequired] = @Original_LogoutSessionRequired) 
			AND ([RequireSignOutPrompt] = @Original_RequireSignOutPrompt) 
			AND ([AllowAccessToAllScopes] = @Original_AllowAccessToAllScopes) 
			AND ([AllowClientCredentialsOnly] = @Original_AllowClientCredentialsOnly) 
			AND ([RequireConsent] = @Original_RequireConsent) 
			AND ([AllowRememberConsent] = @Original_AllowRememberConsent) 
			AND ([EnableLocalLogin] = @Original_EnableLocalLogin) 
			AND ([IncludeJwtId] = @Original_IncludeJwtId) 
			AND ([AlwaysSendClientClaims] = @Original_AlwaysSendClientClaims) 
			AND ([PrefixClientClaims] = @Original_PrefixClientClaims) 
			AND ([AllowAccessToAllGrantTypes] = @Original_AllowAccessToAllGrantTypes)
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
			(ClientId = @ClientId)
END