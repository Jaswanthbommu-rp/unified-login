CREATE TABLE [Auth].[ClientIdPRestrictions] (
    [Id]       INT            IDENTITY (1, 1) NOT NULL,
    [Provider] NVARCHAR (200) NOT NULL,
    [ClientId] INT            NOT NULL,
    CONSTRAINT [PK_ClientIdPRestrictions] PRIMARY KEY CLUSTERED ([Id] ASC)
);

GO

CREATE UNIQUE NONCLUSTERED INDEX [IX_ClientIdPRestrictions_ClientId_Provider]
    ON [Auth].[ClientIdPRestrictions]([ClientId] ASC, [Provider] ASC);
GO

ALTER TABLE [Auth].[ClientIdentityProviderRestrictions] ADD CONSTRAINT [FK_dbo.ClientIdPRestrictions_dbo.Clients_Id] FOREIGN KEY ([ClientId]) REFERENCES [Auth].[Clients] ([Id]) ON DELETE CASCADE
GO