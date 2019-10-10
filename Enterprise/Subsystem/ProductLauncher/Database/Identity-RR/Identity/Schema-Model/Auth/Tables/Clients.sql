CREATE TABLE [Auth].[Clients]
(
[ClientId] [int] NOT NULL IDENTITY(1, 1),
[ClientCode] [nvarchar] (200) NOT NULL,
[ClientName] [nvarchar] (200) NOT NULL,
[ClientUri] [nvarchar] (2000) NULL,
[LogoUri] [nvarchar] (max) NULL,
[Flow] [int] NOT NULL,
[LogoutUri] [nvarchar] (max) NULL,
[IdentityTokenLifetime] [int] NOT NULL,
[AccessTokenLifetime] [int] NOT NULL,
[AuthorizationCodeLifetime] [int] NOT NULL,
[AbsoluteRefreshTokenLifetime] [int] NOT NULL,
[SlidingRefreshTokenLifetime] [int] NOT NULL,
[RefreshTokenUsage] [int] NOT NULL,
[RefreshTokenExpiration] [int] NOT NULL,
[AccessTokenType] [int] NOT NULL,
[UpdateAccessTokenOnRefresh] [bit] NOT NULL,
[Enabled] [bit] NOT NULL,
[LogoutSessionRequired] [bit] NOT NULL,
[RequireSignOutPrompt] [bit] NOT NULL,
[AllowAccessToAllScopes] [bit] NOT NULL,
[AllowClientCredentialsOnly] [bit] NOT NULL,
[RequireConsent] [bit] NOT NULL,
[AllowRememberConsent] [bit] NOT NULL,
[EnableLocalLogin] [bit] NOT NULL,
[IncludeJwtId] [bit] NOT NULL,
[AlwaysSendClientClaims] [bit] NOT NULL,
[PrefixClientClaims] [bit] NOT NULL,
[AllowAccessToAllGrantTypes] [bit] NOT NULL
)
GO
ALTER TABLE [Auth].[Clients] ADD CONSTRAINT [PK_dbo.Clients] PRIMARY KEY CLUSTERED  ([ClientId])
GO
