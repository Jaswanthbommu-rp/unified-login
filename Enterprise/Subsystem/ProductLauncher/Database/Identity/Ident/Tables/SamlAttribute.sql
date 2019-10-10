CREATE TABLE [Ident].[SamlAttribute]
(
	[SamlAttributeId] INT NOT NULL IDENTITY, 
    [Name] NVARCHAR(50) NOT NULL, 
    [SamlAttributeTypeId] INT NOT NULL, 
    CONSTRAINT [PK_SamlAttribute] PRIMARY KEY ([SamlAttributeId]), 
    CONSTRAINT [FK_SamlAttribute_SamlAttributeType] FOREIGN KEY ([SamlAttributeTypeId]) REFERENCES Ident.SamlAttributeType([SamlAttributeTypeId]) ON DELETE CASCADE ON UPDATE CASCADE
)
