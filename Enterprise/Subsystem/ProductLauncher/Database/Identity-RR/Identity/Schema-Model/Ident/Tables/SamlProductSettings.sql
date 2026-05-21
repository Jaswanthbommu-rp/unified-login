CREATE TABLE [Ident].[SamlProductSettings]
(
[SamlProductSettingsId] [int] NOT NULL IDENTITY(1, 1),
[ProductId] [int] NOT NULL,
[LoginUri] [nvarchar] (500) NOT NULL,
[SigningCertificateThumbprint] [nvarchar] (50) NOT NULL,
[SubjectIdSamlAttribute] [nvarchar] (20) NOT NULL
)
GO
ALTER TABLE [Ident].[SamlProductSettings] ADD CONSTRAINT [PK_SamlProductSettings] PRIMARY KEY CLUSTERED  ([SamlProductSettingsId])
GO
ALTER TABLE [Ident].[SamlProductSettings] ADD CONSTRAINT [FK_SamlProductSettings_Product] FOREIGN KEY ([ProductId]) REFERENCES [Enterprise].[Product] ([ProductId]) ON DELETE CASCADE ON UPDATE CASCADE
GO
