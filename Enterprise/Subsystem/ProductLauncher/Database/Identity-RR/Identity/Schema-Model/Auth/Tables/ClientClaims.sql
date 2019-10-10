CREATE TABLE [Auth].[ClientClaims]
(
[ClientClaimsId] [int] NOT NULL IDENTITY(1, 1),
[ClientId] [int] NOT NULL,
[Type] [nvarchar] (100) NOT NULL,
[Value] [nvarchar] (100) NOT NULL
)
GO
ALTER TABLE [Auth].[ClientClaims] ADD CONSTRAINT [PK_dbo.ClientClaims] PRIMARY KEY CLUSTERED  ([ClientClaimsId])
GO
ALTER TABLE [Auth].[ClientClaims] ADD CONSTRAINT [FK_dbo.ClientClaims_dbo.Clients_Client_Id] FOREIGN KEY ([ClientId]) REFERENCES [Auth].[Clients] ([ClientId]) ON DELETE CASCADE
GO
