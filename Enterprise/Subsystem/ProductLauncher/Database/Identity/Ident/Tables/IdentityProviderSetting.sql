CREATE TABLE [Ident].[IdentityProviderSetting]
(
	[IdentityProviderSettingId] INT NOT NULL IDENTITY,
	[IdentityProviderSettingTypeId] INT NOT NULL,
	[Value] NVARCHAR(MAX) NOT NULL,
    CONSTRAINT [PK_IdentityProviderSetting] PRIMARY KEY ([IdentityProviderSettingId]), 
    CONSTRAINT [FK_IdentityProviderSetting_IdentityProviderSettingType] FOREIGN KEY ([IdentityProviderSettingTypeId]) REFERENCES Ident.[IdentityProviderSettingType]([IdentityProviderSettingTypeId]) ON DELETE CASCADE ON UPDATE CASCADE
)
GO

CREATE NONCLUSTERED INDEX [IDX_IdentityProviderSetting_IdentityProviderSettingTypeId]
ON [Ident].[IdentityProviderSetting] ([IdentityProviderSettingTypeId])
INCLUDE ([Value])