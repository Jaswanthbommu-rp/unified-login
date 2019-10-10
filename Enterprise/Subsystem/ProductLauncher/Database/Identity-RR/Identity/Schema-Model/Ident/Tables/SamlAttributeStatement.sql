CREATE TABLE [Ident].[SamlAttributeStatement]
(
[SamlAttributeStatementId] [int] NOT NULL IDENTITY(1, 1),
[ProductId] [int] NOT NULL,
[SamlAttributeId] [int] NOT NULL
)
GO
ALTER TABLE [Ident].[SamlAttributeStatement] ADD CONSTRAINT [PK_SamlAttributeStatement] PRIMARY KEY CLUSTERED  ([SamlAttributeStatementId])
GO
ALTER TABLE [Ident].[SamlAttributeStatement] ADD CONSTRAINT [FK_SamlAttributeStatement_Product] FOREIGN KEY ([ProductId]) REFERENCES [Enterprise].[Product] ([ProductId]) ON DELETE CASCADE ON UPDATE CASCADE
GO
ALTER TABLE [Ident].[SamlAttributeStatement] ADD CONSTRAINT [FK_SamlAttributeStatement_SamlAttribute] FOREIGN KEY ([SamlAttributeId]) REFERENCES [Ident].[SamlAttribute] ([SamlAttributeId]) ON DELETE CASCADE ON UPDATE CASCADE
GO
