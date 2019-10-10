CREATE TABLE [Auth].[ClientRedirectUris]
(
[ClientRedirectUriId] [int] NOT NULL IDENTITY(1, 1),
[ClientId] [int] NOT NULL,
[Uri] [nvarchar] (2000) NOT NULL
)
GO
ALTER TABLE [Auth].[ClientRedirectUris] ADD CONSTRAINT [PK_dbo.ClientRedirectUris] PRIMARY KEY CLUSTERED  ([ClientRedirectUriId])
GO
ALTER TABLE [Auth].[ClientRedirectUris] ADD CONSTRAINT [FK_dbo.ClientRedirectUris_dbo.Clients_Client_Id] FOREIGN KEY ([ClientId]) REFERENCES [Auth].[Clients] ([ClientId]) ON DELETE CASCADE
GO
