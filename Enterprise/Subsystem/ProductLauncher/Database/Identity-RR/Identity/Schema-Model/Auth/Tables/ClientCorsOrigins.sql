CREATE TABLE [Auth].[ClientCorsOrigins]
(
[ClientCorsOriginId] [int] NOT NULL IDENTITY(1, 1),
[ClientId] [int] NOT NULL,
[Origin] [nvarchar] (150) NOT NULL
)
GO
ALTER TABLE [Auth].[ClientCorsOrigins] ADD CONSTRAINT [PK_dbo.ClientCorsOrigins] PRIMARY KEY CLUSTERED  ([ClientCorsOriginId])
GO
ALTER TABLE [Auth].[ClientCorsOrigins] ADD CONSTRAINT [FK_dbo.ClientCorsOrigins_dbo.Clients_Client_Id] FOREIGN KEY ([ClientId]) REFERENCES [Auth].[Clients] ([ClientId]) ON DELETE CASCADE
GO
