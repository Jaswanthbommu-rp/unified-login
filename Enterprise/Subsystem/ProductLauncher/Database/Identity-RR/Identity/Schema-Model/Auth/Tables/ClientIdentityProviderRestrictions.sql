CREATE TABLE [Auth].[ClientIdentityProviderRestrictions]
(
[ClientIdentityProviderRestrictionId] [int] NOT NULL IDENTITY(1, 1),
[ClientId] [int] NOT NULL,
[Provider] [nvarchar] (200) NOT NULL
)
GO
ALTER TABLE [Auth].[ClientIdentityProviderRestrictions] ADD CONSTRAINT [PK_dbo.ClientIdPRestrictions] PRIMARY KEY CLUSTERED  ([ClientIdentityProviderRestrictionId])
GO
ALTER TABLE [Auth].[ClientIdentityProviderRestrictions] ADD CONSTRAINT [FK_dbo.ClientIdPRestrictions_dbo.Clients_Client_Id] FOREIGN KEY ([ClientId]) REFERENCES [Auth].[Clients] ([ClientId]) ON DELETE CASCADE
GO
