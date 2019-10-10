CREATE TABLE [Auth].[ClientCustomGrantTypes]
(
[ClientCustomGrantTypeId] [int] NOT NULL IDENTITY(1, 1),
[ClientId] [int] NOT NULL,
[GrantType] [nvarchar] (250) NOT NULL
)
GO
ALTER TABLE [Auth].[ClientCustomGrantTypes] ADD CONSTRAINT [PK_dbo.ClientCustomGrantTypes] PRIMARY KEY CLUSTERED  ([ClientCustomGrantTypeId])
GO
ALTER TABLE [Auth].[ClientCustomGrantTypes] ADD CONSTRAINT [FK_dbo.ClientCustomGrantTypes_dbo.Clients_Client_Id] FOREIGN KEY ([ClientId]) REFERENCES [Auth].[Clients] ([ClientId]) ON DELETE CASCADE
GO
