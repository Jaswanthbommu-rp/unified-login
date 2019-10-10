CREATE TABLE [Auth].[PortfolioProduct]
(
[PortfolioProductId] [int] NOT NULL IDENTITY(1, 1),
[PortfolioId] [int] NOT NULL,
[ProductId] [int] NOT NULL
)
GO
ALTER TABLE [Auth].[PortfolioProduct] ADD CONSTRAINT [PK_PortfolioProduct] PRIMARY KEY CLUSTERED  ([PortfolioProductId])
GO
CREATE NONCLUSTERED INDEX [IX_PortfolioProduct_PortfolioId_ProductId] ON [Auth].[PortfolioProduct] ([PortfolioId], [ProductId])
GO
ALTER TABLE [Auth].[PortfolioProduct] ADD CONSTRAINT [FK_PortfolioProduct_PortfolioId] FOREIGN KEY ([PortfolioId]) REFERENCES [Auth].[Portfolio] ([PortfolioId])
GO
ALTER TABLE [Auth].[PortfolioProduct] ADD CONSTRAINT [FK_PortfolioProduct_ProductId] FOREIGN KEY ([ProductId]) REFERENCES [Auth].[Product] ([ProductId])
GO
