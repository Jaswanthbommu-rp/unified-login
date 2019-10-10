CREATE TABLE [Auth].[SamlAttributeStatement]
(
[SamlAttributeStatementId] [int] NOT NULL IDENTITY(1, 1),
[ProductId] [int] NOT NULL,
[SamlAttributeId] [int] NOT NULL
)
GO
ALTER TABLE [Auth].[SamlAttributeStatement] ADD CONSTRAINT [PK_SamlAttributeStatement] PRIMARY KEY CLUSTERED  ([SamlAttributeStatementId])
GO
CREATE NONCLUSTERED INDEX [IX_SamlAttributeStatement_ProductId] ON [Auth].[SamlAttributeStatement] ([ProductId])
GO
ALTER TABLE [Auth].[SamlAttributeStatement] ADD CONSTRAINT [FK_SamlAttributeStatement_Product] FOREIGN KEY ([ProductId]) REFERENCES [Auth].[Product] ([ProductId])
GO
ALTER TABLE [Auth].[SamlAttributeStatement] ADD CONSTRAINT [FK_SamlAttributeStatement_SamlAttribute] FOREIGN KEY ([SamlAttributeId]) REFERENCES [Auth].[SamlAttribute] ([SamlAttributeId])
GO
