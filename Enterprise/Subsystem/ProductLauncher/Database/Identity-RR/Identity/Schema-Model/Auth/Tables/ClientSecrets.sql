CREATE TABLE [Auth].[ClientSecrets]
(
[ClientSecretId] [int] NOT NULL IDENTITY(1, 1),
[ClientId] [int] NOT NULL,
[Value] [nvarchar] (250) NOT NULL,
[Type] [nvarchar] (250) NULL,
[Description] [nvarchar] (2000) NULL,
[Expiration] [datetimeoffset] NULL
)
GO
ALTER TABLE [Auth].[ClientSecrets] ADD CONSTRAINT [PK_dbo.ClientSecrets] PRIMARY KEY CLUSTERED  ([ClientSecretId])
GO
ALTER TABLE [Auth].[ClientSecrets] ADD CONSTRAINT [FK_dbo.ClientSecrets_dbo.Clients_Client_Id] FOREIGN KEY ([ClientId]) REFERENCES [Auth].[Clients] ([ClientId]) ON DELETE CASCADE
GO
