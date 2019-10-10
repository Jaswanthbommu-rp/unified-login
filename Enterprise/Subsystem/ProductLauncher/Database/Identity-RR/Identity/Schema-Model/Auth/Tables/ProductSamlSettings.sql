CREATE TABLE [Auth].[ProductSamlSettings]
(
[ProductSamlSettingsId] [int] NOT NULL IDENTITY(1, 1),
[ProductId] [int] NOT NULL,
[LoginUri] [nvarchar] (100) NOT NULL,
[SigningCertificateThumbprint] [nvarchar] (50) NOT NULL,
[SubjectIdSamlAttribute] [nvarchar] (20) NOT NULL
)
GO
ALTER TABLE [Auth].[ProductSamlSettings] ADD CONSTRAINT [PK__ProductS__E0A6F172E44DA88A] PRIMARY KEY CLUSTERED  ([ProductSamlSettingsId])
GO
CREATE NONCLUSTERED INDEX [IX_ProductSamlSettings_ProductId] ON [Auth].[ProductSamlSettings] ([ProductId])
GO
ALTER TABLE [Auth].[ProductSamlSettings] ADD CONSTRAINT [FK_Product_ProductId] FOREIGN KEY ([ProductId]) REFERENCES [Auth].[Product] ([ProductId])
GO
