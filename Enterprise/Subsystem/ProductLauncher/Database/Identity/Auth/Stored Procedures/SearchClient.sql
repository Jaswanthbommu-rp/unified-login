CREATE PROCEDURE [Auth].[SearchClient] (  
  @Id INT = NULL,
 @ClientId NVARCHAR(400) = NULL   
)  
  
AS  
  
BEGIN  
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
      (@Id IS NULL OR [Id] = @Id)
	  and
	  (@ClientId IS NULL OR [ClientId] = @ClientId )  
 
END