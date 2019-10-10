CREATE TABLE [Auth].[ClientPostLogoutRedirectUris]
(
[ClientPostLogoutRedirectUriId] [int] NOT NULL IDENTITY(1, 1),
[ClientId] [int] NOT NULL,
[Uri] [nvarchar] (2000) NOT NULL
)
GO
ALTER TABLE [Auth].[ClientPostLogoutRedirectUris] ADD CONSTRAINT [PK_dbo.ClientPostLogoutRedirectUris] PRIMARY KEY CLUSTERED  ([ClientPostLogoutRedirectUriId])
GO
ALTER TABLE [Auth].[ClientPostLogoutRedirectUris] ADD CONSTRAINT [FK_dbo.ClientPostLogoutRedirectUris_dbo.Clients_Client_Id] FOREIGN KEY ([ClientId]) REFERENCES [Auth].[Clients] ([ClientId]) ON DELETE CASCADE
GO
