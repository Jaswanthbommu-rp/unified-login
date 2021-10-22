CREATE TABLE [Ident].[SamlProductAttribute]
(
	[SamlProductAttributeId] INT NOT NULL IDENTITY, 
    [ProductId] INT NOT NULL,
	[SamlAttributeId] INT NOT NULL
    CONSTRAINT [PK_SamlProductAttribute] PRIMARY KEY ([SamlProductAttributeId]), 
    CONSTRAINT [FK_SamlProductAttribute_ProductId] FOREIGN KEY ([ProductId]) REFERENCES ENTERPRISE.Product([ProductId]) ON DELETE CASCADE ON UPDATE CASCADE,	
    CONSTRAINT [FK_SamlProductAttribute_SamlAttributeId] FOREIGN KEY ([SamlAttributeId]) REFERENCES Ident.SamlAttribute([SamlAttributeId]) ON DELETE CASCADE ON UPDATE CASCADE
)
