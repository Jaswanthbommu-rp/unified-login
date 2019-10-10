CREATE TABLE [Ident].[SamlAttribute]
(
[SamlAttributeId] [int] NOT NULL IDENTITY(1, 1),
[Name] [nvarchar] (50) NOT NULL,
[SamlAttributeTypeId] [int] NOT NULL
)
GO
ALTER TABLE [Ident].[SamlAttribute] ADD CONSTRAINT [PK_SamlAttribute] PRIMARY KEY CLUSTERED  ([SamlAttributeId])
GO
ALTER TABLE [Ident].[SamlAttribute] ADD CONSTRAINT [FK_SamlAttribute_SamlAttributeType] FOREIGN KEY ([SamlAttributeTypeId]) REFERENCES [Ident].[SamlAttributeType] ([SamlAttributeTypeId]) ON DELETE CASCADE ON UPDATE CASCADE
GO
