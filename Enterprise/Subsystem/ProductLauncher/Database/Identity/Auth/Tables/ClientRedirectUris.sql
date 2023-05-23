CREATE TABLE [Auth].[ClientRedirectUris] (
    [Id]          INT            IDENTITY (1, 1) NOT NULL,
    [RedirectUri] NVARCHAR (400) NOT NULL,
    [ClientId]    INT            NOT NULL,
    CONSTRAINT [PK_dbo.ClientRedirectUris] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_dbo.ClientRedirectUris_dbo.Clients_Id] FOREIGN KEY ([ClientId]) REFERENCES [Auth].[Clients] ([Id]) ON DELETE CASCADE
);

