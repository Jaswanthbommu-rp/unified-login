CREATE TABLE [Auth].[Portfolio]
(
[PortfolioId] [int] NOT NULL IDENTITY(1, 1),
[PortfolioName] [nvarchar] (100) NOT NULL
)
GO
ALTER TABLE [Auth].[Portfolio] ADD CONSTRAINT [PK_Property] PRIMARY KEY CLUSTERED  ([PortfolioId])
GO
