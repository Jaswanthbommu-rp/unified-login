CREATE TABLE [Ident].[IdentityProviderSetting]
(
[IdentityProviderSettingId] [int] NOT NULL IDENTITY(1, 1),
[IdentityProviderSettingTypeId] [int] NOT NULL,
[Value] [nvarchar] (255) NOT NULL
)
GO
ALTER TABLE [Ident].[IdentityProviderSetting] ADD CONSTRAINT [PK_IdentityProviderSetting] PRIMARY KEY CLUSTERED  ([IdentityProviderSettingId])
GO
ALTER TABLE [Ident].[IdentityProviderSetting] ADD CONSTRAINT [FK_IdentityProviderSetting_IdentityProviderSettingType] FOREIGN KEY ([IdentityProviderSettingTypeId]) REFERENCES [Ident].[IdentityProviderSettingType] ([IdentityProviderSettingTypeId]) ON DELETE CASCADE ON UPDATE CASCADE
GO
