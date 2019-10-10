CREATE TABLE [Ident].[IdentityProviderSettingType]
(
	[IdentityProviderSettingTypeId] INT NOT NULL IDENTITY, 
	[IdentityProviderTypeId] INT NOT NULL,
    [Name] NVARCHAR(50) NOT NULL, 
    CONSTRAINT [PK_IdentityProviderSettingType] PRIMARY KEY ([IdentityProviderSettingTypeId]), 
    CONSTRAINT [FK_IdentityProviderSettingType_IdentityProviderTypeId] FOREIGN KEY ([IdentityProviderTypeId]) REFERENCES Ident.IdentityProviderType([IdentityProviderTypeId]) ON DELETE CASCADE ON UPDATE CASCADE,
)
