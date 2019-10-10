CREATE TABLE [Auth].[PortfolioProductUser]
(
[PortfolioProductUserId] [int] NOT NULL IDENTITY(1, 1),
[PortfolioId] [int] NOT NULL,
[ProductId] [int] NOT NULL,
[UserId] [bigint] NOT NULL,
[Title] [nvarchar] (50) NULL
)
GO
ALTER TABLE [Auth].[PortfolioProductUser] ADD CONSTRAINT [PK_PortfolioProductUser] PRIMARY KEY CLUSTERED  ([PortfolioProductUserId])
GO
CREATE NONCLUSTERED INDEX [IX_PortfolioProductUser_PortfolioIdProductIdUserId] ON [Auth].[PortfolioProductUser] ([PortfolioId], [ProductId], [UserId])
GO
CREATE NONCLUSTERED INDEX [IX_PortfolioProductUser_PortfolioIdUserId] ON [Auth].[PortfolioProductUser] ([PortfolioId], [UserId])
GO
ALTER TABLE [Auth].[PortfolioProductUser] ADD CONSTRAINT [FK_PortfolioProductUser_PortfolioID] FOREIGN KEY ([PortfolioId]) REFERENCES [Auth].[Portfolio] ([PortfolioId])
GO
ALTER TABLE [Auth].[PortfolioProductUser] ADD CONSTRAINT [FK_PortfolioProductUser_ProductID] FOREIGN KEY ([ProductId]) REFERENCES [Auth].[Product] ([ProductId])
GO
ALTER TABLE [Auth].[PortfolioProductUser] ADD CONSTRAINT [FK_PortfolioProductUser_UserID] FOREIGN KEY ([UserId]) REFERENCES [Auth].[Users] ([UserId])
GO
