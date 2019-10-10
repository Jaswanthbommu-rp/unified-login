CREATE TABLE [Ident].[SamlAttributeType]
(
[SamlAttributeTypeId] [int] NOT NULL IDENTITY(1, 1),
[Name] [nvarchar] (100) NOT NULL
)
GO
ALTER TABLE [Ident].[SamlAttributeType] ADD CONSTRAINT [PK_SamlAttributeType] PRIMARY KEY CLUSTERED  ([SamlAttributeTypeId])
GO
