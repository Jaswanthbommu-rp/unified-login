CREATE TABLE [Ident].[SamlAttributeType]
(
	[SamlAttributeTypeId] INT NOT NULL IDENTITY, 
    [Name] NVARCHAR(100) NOT NULL, 
    CONSTRAINT [PK_SamlAttributeType] PRIMARY KEY ([SamlAttributeTypeId])
)
