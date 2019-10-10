CREATE TABLE [Auth].[ProviderPortfolio]
(
[ProviderPortfolioId] [int] NOT NULL IDENTITY(1, 1),
[PortfolioIdId] [int] NOT NULL,
[ProviderName] [nvarchar] (50) NOT NULL,
[Description] [nvarchar] (100) NULL,
[AuthenticationType] [nvarchar] (20) NOT NULL,
[Caption] [nvarchar] (50) NOT NULL,
[ProviderClientId] [nvarchar] (1000) NOT NULL,
[AuthorityUri] [nvarchar] (100) NOT NULL,
[PostLogoutRedirectUri] [nvarchar] (100) NOT NULL,
[RedirectUri] [nvarchar] (100) NOT NULL,
[AuthenticationMode] [tinyint] NOT NULL CONSTRAINT [DF_ProviderProperty_AuthenticationMode] DEFAULT ((0)),
[ValidateIssuer] [bit] NOT NULL CONSTRAINT [DF_ProviderProperty_ValidateIssuer] DEFAULT ((0)),
[TokenValidationAuthenticationType] [nvarchar] (20) NOT NULL,
[Scope] [nvarchar] (500) NULL,
[OktaEntityId] [nvarchar] (100) NULL,
[OktaMetadataLocation] [nvarchar] (1000) NULL,
[ClientSecret] [nvarchar] (1000) NULL
)
GO
ALTER TABLE [Auth].[ProviderPortfolio] ADD CONSTRAINT [PK_ProviderProperty] PRIMARY KEY CLUSTERED  ([ProviderPortfolioId])
GO
ALTER TABLE [Auth].[ProviderPortfolio] ADD CONSTRAINT [FK_ProviderPortfolio_Portfolio] FOREIGN KEY ([PortfolioIdId]) REFERENCES [Auth].[Portfolio] ([PortfolioId])
GO
