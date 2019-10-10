CREATE TABLE [Auth].[ClientPostLogoutRedirectUris] (
    [ClientPostLogoutRedirectUriId]        INT             IDENTITY (1, 1) NOT NULL,
    [ClientId] INT             NOT NULL,
    [Uri]       NVARCHAR (2000) NOT NULL,   
    CONSTRAINT [PK_dbo.ClientPostLogoutRedirectUris] PRIMARY KEY CLUSTERED ([ClientPostLogoutRedirectUriId] ASC),
    CONSTRAINT [FK_dbo.ClientPostLogoutRedirectUris_dbo.Clients_Client_Id] FOREIGN KEY ([ClientId]) REFERENCES [Auth].[Clients] ([ClientId]) ON DELETE CASCADE
);

