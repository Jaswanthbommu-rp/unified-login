CREATE TABLE [Auth].[PortfolioProductUserClaims]
(
[PortfolioProductUserClaimsId] [int] NOT NULL IDENTITY(1, 1),
[PortfolioProductUserId] [int] NOT NULL,
[Type] [nvarchar] (250) NOT NULL,
[Value] [nvarchar] (250) NOT NULL
)
GO
ALTER TABLE [Auth].[PortfolioProductUserClaims] ADD CONSTRAINT [PK_PortfolioProductUserClaims] PRIMARY KEY CLUSTERED  ([PortfolioProductUserClaimsId])
GO
CREATE NONCLUSTERED INDEX [IX_PortfolioProductUserClaims_PortfolioIDProductIDUserID] ON [Auth].[PortfolioProductUserClaims] ([PortfolioProductUserId])
GO
ALTER TABLE [Auth].[PortfolioProductUserClaims] ADD CONSTRAINT [FK_PortfolioProductUserClaims_PortfolioProductUserId] FOREIGN KEY ([PortfolioProductUserId]) REFERENCES [Auth].[PortfolioProductUser] ([PortfolioProductUserId])
GO
