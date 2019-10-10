CREATE TABLE [Ident].[SamlAttributeStatement]
(
	[SamlAttributeStatementId] INT NOT NULL IDENTITY, 
    [ProductId] INT NOT NULL, 
    [SamlAttributeId] INT NOT NULL, 
    CONSTRAINT [PK_SamlAttributeStatement] PRIMARY KEY ([SamlAttributeStatementId]), 
    CONSTRAINT [FK_SamlAttributeStatement_Product] FOREIGN KEY ([ProductId]) REFERENCES [Enterprise].[Product]([ProductId]) ON DELETE CASCADE ON UPDATE CASCADE, 
    CONSTRAINT [FK_SamlAttributeStatement_SamlAttribute] FOREIGN KEY ([SamlAttributeId]) REFERENCES [Ident].[SamlAttribute]([SamlAttributeId]) ON DELETE CASCADE ON UPDATE CASCADE
)
