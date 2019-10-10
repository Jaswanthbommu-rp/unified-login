CREATE TABLE [Auth].[UserSamlAttribute]
(
[UserSamlAttributeId] [int] NOT NULL IDENTITY(1, 1),
[PortfolioProductUserId] [int] NOT NULL,
[SamlAttributeId] [int] NOT NULL,
[Value] [nvarchar] (500) NOT NULL
)
GO
ALTER TABLE [Auth].[UserSamlAttribute] ADD CONSTRAINT [PK_UserSamlAttribute] PRIMARY KEY CLUSTERED  ([UserSamlAttributeId])
GO
CREATE NONCLUSTERED INDEX [IX_UserSamlAttribute_PortfolioProductUserId] ON [Auth].[UserSamlAttribute] ([PortfolioProductUserId])
GO
ALTER TABLE [Auth].[UserSamlAttribute] ADD CONSTRAINT [FK_UserSamlAttribute_PortfolioProductUser] FOREIGN KEY ([PortfolioProductUserId]) REFERENCES [Auth].[PortfolioProductUser] ([PortfolioProductUserId])
GO
ALTER TABLE [Auth].[UserSamlAttribute] ADD CONSTRAINT [FK_UserSamlAttribute_SamlAttribute] FOREIGN KEY ([SamlAttributeId]) REFERENCES [Auth].[SamlAttribute] ([SamlAttributeId])
GO
