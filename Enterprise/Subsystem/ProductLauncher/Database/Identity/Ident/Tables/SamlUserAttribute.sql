CREATE TABLE [Ident].[SamlUserAttribute]
(
	[SamlUserAttributeId] INT NOT NULL IDENTITY, 
    [PersonaId] BIGINT NOT NULL, 
	[ProductId] INT NOT NULL,
    [SamlAttributeId] INT NOT NULL, 
    [Value] NVARCHAR(500) NULL, 
    [FromDate] DATETIME NOT NULL DEFAULT GETUTCDATE(), 
    [ThruDate] DATETIME NULL, 
    CONSTRAINT [PK_SamlUserAttribute] PRIMARY KEY ([SamlUserAttributeId]), 
    CONSTRAINT [FK_SamlUserAttribute_Persona] FOREIGN KEY ([PersonaId]) REFERENCES [Person].[Persona]([PersonaId]) ON DELETE CASCADE ON UPDATE CASCADE, 
    CONSTRAINT [FK_SamlUserAttribute_SamlAttribute] FOREIGN KEY ([SamlAttributeId]) REFERENCES Ident.SamlAttribute([SamlAttributeId]) ON DELETE CASCADE ON UPDATE CASCADE, 
    CONSTRAINT [FK_SamlUserAttribute_Product] FOREIGN KEY ([ProductId]) REFERENCES Enterprise.[Product]([ProductId]) ON DELETE CASCADE ON UPDATE CASCADE, 
    CONSTRAINT [AK_SamlUserAttribute_PersonaId_ProductId_SamlAttributeId] UNIQUE ([PersonaId], [ProductId], [SamlAttributeId])
)
GO
CREATE INDEX [IX_SamlUserAttribute_ProductId_SamlAttributeId]
ON [Ident].[SamlUserAttribute]
( [ProductId], [SamlAttributeId]
) 
	   INCLUDE( [SamlUserAttributeId], [PersonaId], [Value], [FromDate], [ThruDate] );
GO
