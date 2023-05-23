CREATE TABLE [Auth].[ClientPostLogoutRedirectUris] (
    [Id]                    INT            IDENTITY (1, 1) NOT NULL,
    [PostLogoutRedirectUri] NVARCHAR (400) NOT NULL,
    [ClientId]              INT            NOT NULL,   
    CONSTRAINT [PK_ClientPostLogoutRedirectUris] PRIMARY KEY CLUSTERED ([Id] ASC),

    CONSTRAINT [FK_dbo.ClientPostLogoutRedirectUris_dbo.Clients_Id] FOREIGN KEY ([ClientId]) REFERENCES [Auth].[Clients] ([Id]) ON DELETE CASCADE
);

GO

CREATE UNIQUE NONCLUSTERED INDEX [IX_ClientClaims_ClientId_Type_Value] ON [Auth].[ClientClaims] ([ClientId], [Type], [Value]) ON [PRIMARY]
GO
