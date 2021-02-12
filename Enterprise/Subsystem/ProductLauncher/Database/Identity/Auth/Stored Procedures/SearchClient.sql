CREATE PROCEDURE [Auth].SearchClient (  
  @ClientId BIGINT = NULL   
 --,@ClientGroupId BIGINT = NULL   
 ,@ClientCode NVARCHAR(400) = NULL   
 ,@ClientName NVARCHAR(400) = NULL   
 ,@ClientUri NVARCHAR(4000) = NULL   
 ,@LogoUri NVARCHAR(MAX) = NULL   
 ,@Flow INT = NULL   
 ,@LogoutUri NVARCHAR(MAX) = NULL   
 ,@IdentityTokenLifetime INT = NULL   
 ,@AccessTokenLifetime INT = NULL   
 ,@AuthorizationCodeLifetime INT = NULL   
 ,@AbsoluteRefreshTokenLifetime INT = NULL   
 ,@SlidingRefreshTokenLifetime INT = NULL   
 ,@RefreshTokenUsage INT = NULL   
 ,@RefreshTokenExpiration INT = NULL   
 ,@AccessTokenType INT = NULL   
 ,@UpdateAccessTokenOnRefresh BIT = NULL   
 ,@Enabled BIT = NULL   
 ,@LogoutSessionRequired BIT = NULL   
 ,@RequireSignOutPrompt BIT = NULL   
 ,@AllowAccessToAllScopes BIT = NULL   
 ,@AllowClientCredentialsOnly BIT = NULL   
 ,@RequireConsent BIT = NULL   
 ,@AllowRememberConsent BIT = NULL   
 ,@EnableLocalLogin BIT = NULL   
 ,@IncludeJwtId BIT = NULL   
 ,@AlwaysSendClientClaims BIT = NULL   
 ,@PrefixClientClaims BIT = NULL   
 ,@AllowAccessToAllGrantTypes BIT = NULL   
)  
  
AS  
  
BEGIN  
  
 SELECT  
   [ClientId]  
  --,[ClientGroupId]  
  ,[ClientCode]  
  ,[ClientName]  
  ,[ClientUri]  
  ,[LogoUri]  
  ,[Flow]  
  ,[LogoutUri]  
  ,[IdentityTokenLifetime]  
  ,[AccessTokenLifetime]  
  ,[AuthorizationCodeLifetime]  
  ,[AbsoluteRefreshTokenLifetime]  
  ,[SlidingRefreshTokenLifetime]  
  ,[RefreshTokenUsage]  
  ,[RefreshTokenExpiration]  
  ,[AccessTokenType]  
  ,[UpdateAccessTokenOnRefresh]  
  ,[Enabled]  
  ,[LogoutSessionRequired]  
  ,[RequireSignOutPrompt]  
  ,[AllowAccessToAllScopes]  
  ,[AllowClientCredentialsOnly]  
  ,[RequireConsent]  
  ,[AllowRememberConsent]  
  ,[EnableLocalLogin]  
  ,[IncludeJwtId]  
  ,[AlwaysSendClientClaims]  
  ,[PrefixClientClaims]  
  ,[AllowAccessToAllGrantTypes]  
 FROM  
  [Auth].[Clients]  
 WHERE   
  (@ClientId IS NULL  OR  [ClientId] = @ClientId)  
 --AND  
 -- (@ClientGroupId IS NULL  OR  [ClientGroupId] = @ClientGroupId)  
 AND  
  (@ClientCode IS NULL OR [ClientCode] = @ClientCode )  
 AND  
  (@ClientName IS NULL OR [ClientName] = @ClientName OR CHARINDEX(@ClientName,[ClientName]) > 0)  
 AND  
  (@ClientUri IS NULL OR [ClientUri] = @ClientUri OR CHARINDEX(@ClientUri,[ClientUri]) > 0)  
 AND  
  (@LogoUri IS NULL OR [LogoUri] = @LogoUri OR CHARINDEX(@LogoUri,[LogoUri]) > 0)  
 AND  
  (@Flow IS NULL  OR  [Flow] = @Flow)  
 AND  
  (@LogoutUri IS NULL OR [LogoutUri] = @LogoutUri OR CHARINDEX(@LogoutUri,[LogoutUri]) > 0)  
 AND  
  (@IdentityTokenLifetime IS NULL  OR  [IdentityTokenLifetime] = @IdentityTokenLifetime)  
 AND  
  (@AccessTokenLifetime IS NULL  OR  [AccessTokenLifetime] = @AccessTokenLifetime)  
 AND  
  (@AuthorizationCodeLifetime IS NULL  OR  [AuthorizationCodeLifetime] = @AuthorizationCodeLifetime)  
 AND  
  (@AbsoluteRefreshTokenLifetime IS NULL  OR  [AbsoluteRefreshTokenLifetime] = @AbsoluteRefreshTokenLifetime)  
 AND  
  (@SlidingRefreshTokenLifetime IS NULL  OR  [SlidingRefreshTokenLifetime] = @SlidingRefreshTokenLifetime)  
 AND  
  (@RefreshTokenUsage IS NULL  OR  [RefreshTokenUsage] = @RefreshTokenUsage)  
 AND  
  (@RefreshTokenExpiration IS NULL  OR  [RefreshTokenExpiration] = @RefreshTokenExpiration)  
 AND  
  (@AccessTokenType IS NULL  OR  [AccessTokenType] = @AccessTokenType)  
 AND  
  (@UpdateAccessTokenOnRefresh IS NULL  OR  [UpdateAccessTokenOnRefresh] = @UpdateAccessTokenOnRefresh)  
 AND  
  (@Enabled IS NULL  OR  [Enabled] = @Enabled)  
 AND  
  (@LogoutSessionRequired IS NULL  OR  [LogoutSessionRequired] = @LogoutSessionRequired)  
 AND  
  (@RequireSignOutPrompt IS NULL  OR  [RequireSignOutPrompt] = @RequireSignOutPrompt)  
 AND  
  (@AllowAccessToAllScopes IS NULL  OR  [AllowAccessToAllScopes] = @AllowAccessToAllScopes)  
 AND  
  (@AllowClientCredentialsOnly IS NULL  OR  [AllowClientCredentialsOnly] = @AllowClientCredentialsOnly)  
 AND  
  (@RequireConsent IS NULL  OR  [RequireConsent] = @RequireConsent)  
 AND  
  (@AllowRememberConsent IS NULL  OR  [AllowRememberConsent] = @AllowRememberConsent)  
 AND  
  (@EnableLocalLogin IS NULL  OR  [EnableLocalLogin] = @EnableLocalLogin)  
 AND  
  (@IncludeJwtId IS NULL  OR  [IncludeJwtId] = @IncludeJwtId)  
 AND  
  (@AlwaysSendClientClaims IS NULL  OR  [AlwaysSendClientClaims] = @AlwaysSendClientClaims)  
 AND  
  (@PrefixClientClaims IS NULL  OR  [PrefixClientClaims] = @PrefixClientClaims)  
 AND  
  (@AllowAccessToAllGrantTypes IS NULL  OR  [AllowAccessToAllGrantTypes] = @AllowAccessToAllGrantTypes)  
  
END