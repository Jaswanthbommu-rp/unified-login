CREATE TABLE [Auth].[SamlAttribute]
(
[SamlAttributeId] [int] NOT NULL IDENTITY(1, 1),
[Name] [nvarchar] (50) NOT NULL,
[Type] [nvarchar] (100) NOT NULL
)
GO
ALTER TABLE [Auth].[SamlAttribute] ADD CONSTRAINT [PK_SamlAttribute] PRIMARY KEY CLUSTERED  ([SamlAttributeId])
GO
