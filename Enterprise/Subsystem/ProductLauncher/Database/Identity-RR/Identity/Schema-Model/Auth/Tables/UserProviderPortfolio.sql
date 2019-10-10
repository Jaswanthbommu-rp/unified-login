CREATE TABLE [Auth].[UserProviderPortfolio]
(
[UserProviderPortfolioId] [bigint] NOT NULL,
[UserId] [bigint] NOT NULL,
[ProviderPortfolioId] [int] NOT NULL
)
GO
ALTER TABLE [Auth].[UserProviderPortfolio] ADD CONSTRAINT [PK_UserProviderProperty] PRIMARY KEY CLUSTERED  ([UserProviderPortfolioId])
GO
ALTER TABLE [Auth].[UserProviderPortfolio] ADD CONSTRAINT [FK_UserProviderPortfolio_ProviderPortfolio] FOREIGN KEY ([ProviderPortfolioId]) REFERENCES [Auth].[ProviderPortfolio] ([ProviderPortfolioId])
GO
ALTER TABLE [Auth].[UserProviderPortfolio] ADD CONSTRAINT [FK_UserProviderPortfolio_Users] FOREIGN KEY ([UserId]) REFERENCES [Auth].[Users] ([UserId])
GO
