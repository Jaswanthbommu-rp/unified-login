CREATE TABLE [Ident].[SamlUserAttribute]
(
[SamlUserAttributeId] [int] NOT NULL IDENTITY(1, 1),
[PersonaId] [bigint] NOT NULL,
[ProductId] [int] NOT NULL,
[SamlAttributeId] [int] NOT NULL,
[Value] [nvarchar] (500) NULL,
[FromDate] [datetime] NOT NULL CONSTRAINT [DF__SamlUserA__FromD__269AB60B] DEFAULT (getutcdate()),
[ThruDate] [datetime] NULL
)
GO
ALTER TABLE [Ident].[SamlUserAttribute] ADD CONSTRAINT [PK_SamlUserAttribute] PRIMARY KEY CLUSTERED  ([SamlUserAttributeId])
GO
ALTER TABLE [Ident].[SamlUserAttribute] ADD CONSTRAINT [AK_SamlUserAttribute_PersonaId_ProductId_SamlAttributeId] UNIQUE NONCLUSTERED  ([PersonaId], [ProductId], [SamlAttributeId])
GO
ALTER TABLE [Ident].[SamlUserAttribute] ADD CONSTRAINT [FK_SamlUserAttribute_Persona] FOREIGN KEY ([PersonaId]) REFERENCES [Person].[Persona] ([PersonaId]) ON DELETE CASCADE ON UPDATE CASCADE
GO
ALTER TABLE [Ident].[SamlUserAttribute] ADD CONSTRAINT [FK_SamlUserAttribute_Product] FOREIGN KEY ([ProductId]) REFERENCES [Enterprise].[Product] ([ProductId]) ON DELETE CASCADE ON UPDATE CASCADE
GO
ALTER TABLE [Ident].[SamlUserAttribute] ADD CONSTRAINT [FK_SamlUserAttribute_SamlAttribute] FOREIGN KEY ([SamlAttributeId]) REFERENCES [Ident].[SamlAttribute] ([SamlAttributeId]) ON DELETE CASCADE ON UPDATE CASCADE
GO
