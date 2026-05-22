CREATE TABLE [Ident].[SamlProductSettings]
(
	[SamlProductSettingsId] INT NOT NULL IDENTITY, 
    [ProductId] INT NOT NULL, 
    [LoginUri] NVARCHAR(500) NOT NULL,
    [SigningCertificateThumbprint] NVARCHAR(50) NOT NULL, 
    [SubjectIdSamlAttribute] NVARCHAR(20) NOT NULL, 
    CONSTRAINT [PK_SamlProductSettings] PRIMARY KEY ([SamlProductSettingsId]), 
    CONSTRAINT [FK_SamlProductSettings_Product] FOREIGN KEY ([ProductId]) REFERENCES Enterprise.[Product]([ProductId]) ON DELETE CASCADE ON UPDATE CASCADE
)
