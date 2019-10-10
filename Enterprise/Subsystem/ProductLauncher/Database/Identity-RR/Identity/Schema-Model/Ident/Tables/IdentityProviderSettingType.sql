CREATE TABLE [Ident].[IdentityProviderSettingType]
(
[IdentityProviderSettingTypeId] [int] NOT NULL IDENTITY(1, 1),
[IdentityProviderTypeId] [int] NOT NULL,
[Name] [nvarchar] (50) NOT NULL
)
GO
ALTER TABLE [Ident].[IdentityProviderSettingType] ADD CONSTRAINT [PK_IdentityProviderSettingType] PRIMARY KEY CLUSTERED  ([IdentityProviderSettingTypeId])
GO
ALTER TABLE [Ident].[IdentityProviderSettingType] ADD CONSTRAINT [FK_IdentityProviderSettingType_IdentityProviderTypeId] FOREIGN KEY ([IdentityProviderTypeId]) REFERENCES [Ident].[IdentityProviderType] ([IdentityProviderTypeId]) ON DELETE CASCADE ON UPDATE CASCADE
GO
