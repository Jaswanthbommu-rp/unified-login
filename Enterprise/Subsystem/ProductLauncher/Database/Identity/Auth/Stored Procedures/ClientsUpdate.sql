CREATE PROCEDURE [Auth].[ClientsUpdate]
(
	@Id INT,
    @Enabled BIT,
    @ClientId NVARCHAR(200),
    @ProtocolType NVARCHAR(200),
    @RequireClientSecret BIT,
    @ClientName NVARCHAR(200),
    @Description NVARCHAR(1000),
    @ClientUri NVARCHAR(2000),
    @LogoUri NVARCHAR(2000),
    @RequireConsent BIT,
    @AllowRememberConsent BIT,
    @AlwaysIncludeUserClaimsInIdToken BIT,
    @RequirePkce BIT,
    @AllowPlainTextPkce BIT,
    @RequireRequestObject BIT,
    @AllowAccessTokensViaBrowser BIT,
    @RequireDPoP BIT,
    @DPoPValidationMode INT,
    @DPoPClockSkew INT,
    @FrontChannelLogoutUri NVARCHAR(2000),
    @FrontChannelLogoutSessionRequired BIT,
    @BackChannelLogoutUri NVARCHAR(2000),
	@BackChannelLogoutSessionRequired BIT,
    @AllowOfflineAccess BIT,
    @IdentityTokenLifetime INT,
    @AllowedIdentityTokenSigningAlgorithms NVARCHAR(100),
    @AccessTokenLifetime INT,
    @AuthorizationCodeLifetime INT,
    @ConsentLifetime INT,
    @AbsoluteRefreshTokenLifetime INT,
    @SlidingRefreshTokenLifetime INT,
    @RefreshTokenUsage INT,
    @UpdateAccessTokenClaimsOnRefresh BIT,
    @RefreshTokenExpiration INT,
    @AccessTokenType INT,
    @EnableLocalLogin BIT,
    @IncludeJwtId BIT,
    @AlwaysSendClientClaims BIT,
    @ClientClaimsPrefix NVARCHAR(200),
    @PairWiseSubjectSalt NVARCHAR(200),
	@InitiateLoginUri NVARCHAR(2000),
	@UserSsoLifetime INT,
    @UserCodeType NVARCHAR(100),
	@DeviceCodeLifetime INT,
    @CibaLifetime INT,
    @PollingInterval INT,
    @CoordinateLifetimeWithUserSession BIT,
    @PushedAuthorizationLifetime INT,
	@RequirePushedAuthorization BIT
)
AS
BEGIN
	SET NOCOUNT OFF;
	UPDATE [Auth].[Clients] 
		SET 
        Enabled = @Enabled,
        ClientId = @ClientId,
        ProtocolType = CASE WHEN @ProtocolType IS NULL THEN 'oidc' ELSE @ProtocolType END,
        RequireClientSecret = @RequireClientSecret,
        ClientName = @ClientName,
        Description = @Description,
        ClientUri = CASE WHEN @ClientUri = '' THEN NULL ELSE @ClientUri END,
        LogoUri = CASE WHEN @LogoUri = '' THEN NULL ELSE @LogoUri END,
        RequireConsent = @RequireConsent,
        AllowRememberConsent = @AllowRememberConsent,
        AlwaysIncludeUserClaimsInIdToken = @AlwaysIncludeUserClaimsInIdToken,
        RequirePkce = @RequirePkce,
        AllowPlainTextPkce = @AllowPlainTextPkce,
        RequireRequestObject = @RequireRequestObject,
        AllowAccessTokensViaBrowser = @AllowAccessTokensViaBrowser,
        RequireDPoP = @RequireDPoP,
        DPoPValidationMode = @DPoPValidationMode,
        DPoPClockSkew = CAST(DATEADD(ms, @DPoPClockSkew * 1000, 0) AS TIME),
        FrontChannelLogoutUri = CASE WHEN @FrontChannelLogoutUri = '' THEN NULL ELSE @FrontChannelLogoutUri END,
        FrontChannelLogoutSessionRequired = @FrontChannelLogoutSessionRequired,
        BackChannelLogoutUri = CASE WHEN @BackChannelLogoutUri = '' THEN NULL ELSE @BackChannelLogoutUri END,
        BackChannelLogoutSessionRequired = @BackChannelLogoutSessionRequired,
        AllowOfflineAccess = @AllowOfflineAccess,
        IdentityTokenLifetime = @IdentityTokenLifetime,
        AllowedIdentityTokenSigningAlgorithms = CASE WHEN @AllowedIdentityTokenSigningAlgorithms = '' THEN NULL ELSE @AllowedIdentityTokenSigningAlgorithms END,
        AccessTokenLifetime = @AccessTokenLifetime,
        AuthorizationCodeLifetime = @AuthorizationCodeLifetime,
        ConsentLifetime = CASE WHEN @ConsentLifetime = '' THEN NULL ELSE @ConsentLifetime END,
        AbsoluteRefreshTokenLifetime = @AbsoluteRefreshTokenLifetime,
        SlidingRefreshTokenLifetime = @SlidingRefreshTokenLifetime,
        RefreshTokenUsage = @RefreshTokenUsage,
        UpdateAccessTokenClaimsOnRefresh = @UpdateAccessTokenClaimsOnRefresh,
        RefreshTokenExpiration = @RefreshTokenExpiration,
        AccessTokenType = @AccessTokenType,
        EnableLocalLogin = @EnableLocalLogin,
        IncludeJwtId = @IncludeJwtId,
        AlwaysSendClientClaims = @AlwaysSendClientClaims,
        ClientClaimsPrefix = CASE WHEN @ClientClaimsPrefix = '' THEN NULL ELSE @ClientClaimsPrefix END,
        PairWiseSubjectSalt = CASE WHEN @PairWiseSubjectSalt = '' THEN NULL ELSE @PairWiseSubjectSalt END,
        InitiateLoginUri = CASE WHEN @InitiateLoginUri = '' THEN NULL ELSE @InitiateLoginUri END,
        UserSsoLifetime = CASE WHEN @UserSsoLifetime = '' THEN NULL ELSE @UserSsoLifetime END,
        UserCodeType = CASE  WHEN @UserCodeType = '' THEN NULL ELSE @UserCodeType END,
        DeviceCodeLifetime = @DeviceCodeLifetime,
        CibaLifetime = CASE WHEN @CibaLifetime = '' THEN NULL ELSE @CibaLifetime END,
        PollingInterval = CASE WHEN @PollingInterval = '' THEN NULL ELSE @PollingInterval END,
        CoordinateLifetimeWithUserSession = @CoordinateLifetimeWithUserSession,
        PushedAuthorizationLifetime = CASE WHEN @PushedAuthorizationLifetime = '' THEN NULL ELSE @PushedAuthorizationLifetime END,
        RequirePushedAuthorization = @RequirePushedAuthorization,
		Updated = GETUTCDATE()
	WHERE 
		Id = @Id;
	
	SELECT [Id]
		  ,[Enabled]
		  ,[ClientId]
		  ,[ProtocolType]
		  ,[RequireClientSecret]
		  ,[ClientName]
		  ,[Description]
		  ,[ClientUri]
		  ,[LogoUri]
		  ,[RequireConsent]
		  ,[AllowRememberConsent]
		  ,[AlwaysIncludeUserClaimsInIdToken]
		  ,[RequirePkce]
		  ,[AllowPlainTextPkce]
		  ,[RequireRequestObject]
		  ,[AllowAccessTokensViaBrowser]
		  ,[RequireDPoP]
		  ,[DPoPValidationMode]
		  ,DATEDIFF(second,0,DPoPClockSkew) [DPoPClockSkew]
		  ,[FrontChannelLogoutUri]
		  ,[FrontChannelLogoutSessionRequired]
		  ,[BackChannelLogoutUri]
		  ,[BackChannelLogoutSessionRequired]
		  ,[AllowOfflineAccess]
		  ,[IdentityTokenLifetime]
		  ,[AllowedIdentityTokenSigningAlgorithms]
		  ,[AccessTokenLifetime]
		  ,[AuthorizationCodeLifetime]
		  ,[ConsentLifetime]
		  ,[AbsoluteRefreshTokenLifetime]
		  ,[SlidingRefreshTokenLifetime]
		  ,[RefreshTokenUsage]
		  ,[UpdateAccessTokenClaimsOnRefresh]
		  ,[RefreshTokenExpiration]
		  ,[AccessTokenType]
		  ,[EnableLocalLogin]
		  ,[IncludeJwtId]
		  ,[AlwaysSendClientClaims]
		  ,[ClientClaimsPrefix]
		  ,[PairWiseSubjectSalt]
		  ,[InitiateLoginUri]
		  ,[UserSsoLifetime]
		  ,[UserCodeType]
		  ,[DeviceCodeLifetime]
		  ,[CibaLifetime]
		  ,[PollingInterval]
		  ,[CoordinateLifetimeWithUserSession]
		  ,[PushedAuthorizationLifetime]
		  ,[RequirePushedAuthorization]
		  ,[Created]
		  ,[Updated]
		  ,[LastAccessed]
		  ,[NonEditable]
	  FROM [Auth].[Clients]
		WHERE 
			(ClientId = @ClientId)
END