CREATE TABLE [Auth].[ClientScopes] (
    [Id]       INT            IDENTITY (1, 1) NOT NULL,
    [Scope]    NVARCHAR (200) NOT NULL,
    [ClientId] INT            NOT NULL,
    CONSTRAINT [PK_ClientScopes] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_dbo.ClientScopes_dbo.Clients_Id] FOREIGN KEY ([ClientId]) REFERENCES [Auth].[Clients] ([Id]) ON DELETE CASCADE
);

