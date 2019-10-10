CREATE TABLE [Ident].[IdentityProviderType]
(
[IdentityProviderTypeId] [int] NOT NULL IDENTITY(1, 1),
[Name] [nvarchar] (50) NOT NULL,
[Description] [nvarchar] (50) NULL
)
GO
ALTER TABLE [Ident].[IdentityProviderType] ADD CONSTRAINT [PK_IdentityProvider] PRIMARY KEY CLUSTERED  ([IdentityProviderTypeId])
GO
