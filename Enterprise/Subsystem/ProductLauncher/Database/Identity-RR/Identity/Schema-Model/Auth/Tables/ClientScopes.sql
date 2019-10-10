CREATE TABLE [Auth].[ClientScopes]
(
[ClientScopeId] [int] NOT NULL IDENTITY(1, 1),
[ClientId] [int] NOT NULL,
[Scope] [nvarchar] (200) NOT NULL
)
GO
ALTER TABLE [Auth].[ClientScopes] ADD CONSTRAINT [PK_dbo.ClientScopes] PRIMARY KEY CLUSTERED  ([ClientScopeId])
GO
ALTER TABLE [Auth].[ClientScopes] ADD CONSTRAINT [FK_dbo.ClientScopes_dbo.Clients_Client_Id] FOREIGN KEY ([ClientId]) REFERENCES [Auth].[Clients] ([ClientId]) ON DELETE CASCADE
GO
