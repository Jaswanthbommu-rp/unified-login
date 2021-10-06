CREATE TABLE [Ident].[IdentityProviderType]
(
	[IdentityProviderTypeId] INT NOT NULL IDENTITY, 
    [Name] NVARCHAR(50) NOT NULL, 
    [ContactMechanismId] INT NOT NULL DEFAULT (-1),
    [Description] NVARCHAR(200) NULL, 
    CONSTRAINT [PK_IdentityProvider] PRIMARY KEY (IdentityProviderTypeId)
)
