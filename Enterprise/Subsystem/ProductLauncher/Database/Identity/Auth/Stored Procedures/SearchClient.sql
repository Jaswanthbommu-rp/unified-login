CREATE PROCEDURE [Auth].SearchClient (  
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
		  ,[UserSsoLifetime]
		  ,[UserCodeType]
		  ,[DeviceCodeLifetime]
		  ,[CibaLifetime]
		  ,[PollingInterval]
		  ,[CoordinateLifetimeWithUserSession]
		  ,[Created]
		  ,[Updated]
		  ,[LastAccessed]
		  ,[NonEditable]
	  FROM [Auth].[Clients]
	  WHERE
  
	  (@ClientId IS NULL OR [ClientId] = @ClientId )  
 
END