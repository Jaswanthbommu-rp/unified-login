CREATE TABLE [Ident].[ContactMechanismIdentity]
(
[ContactMechanismIdentityId] [int] NOT NULL IDENTITY(1, 1),
[ContactMechanismId] [int] NOT NULL,
[IdentityProviderSettingId] [int] NOT NULL,
[FromDate] [datetime] NOT NULL CONSTRAINT [DF__ContactMe__FromD__11158940] DEFAULT (getutcdate()),
[ThruDate] [datetime] NULL
)
GO
ALTER TABLE [Ident].[ContactMechanismIdentity] ADD CONSTRAINT [PK_ContactMechanismIdentity] PRIMARY KEY CLUSTERED  ([ContactMechanismIdentityId])
GO
ALTER TABLE [Ident].[ContactMechanismIdentity] ADD CONSTRAINT [FK_ContactMechanismIdentity_ContactMechanism] FOREIGN KEY ([ContactMechanismId]) REFERENCES [Enterprise].[ContactMechanism] ([ContactMechanismID]) ON DELETE CASCADE ON UPDATE CASCADE
GO
