--DROP
CREATE TABLE [Auth].[ClientIdentityProviderRestrictions] (
    [ClientIdentityProviderRestrictionId]        INT            IDENTITY (1, 1) NOT NULL,
	[ClientId] INT            NOT NULL,
	[Provider]  NVARCHAR (200) NOT NULL,
    CONSTRAINT [PK_dbo.ClientIdPRestrictions] PRIMARY KEY CLUSTERED ([ClientIdentityProviderRestrictionId] ASC),
    CONSTRAINT [FK_dbo.ClientIdPRestrictions_dbo.Clients_Client_Id] FOREIGN KEY ([ClientId]) REFERENCES [Auth].[Clients] ([Id]) ON DELETE CASCADE
);

