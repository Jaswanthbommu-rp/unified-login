CREATE TABLE [Auth].[DeveloperClients]
(
[DeveloperClientId] [bigint] NOT NULL IDENTITY(1, 1),
[DeveloperId] [bigint] NOT NULL,
[ClientId] [int] NOT NULL
)
GO
ALTER TABLE [Auth].[DeveloperClients] ADD CONSTRAINT [PK_dbo.DeveloperClients] PRIMARY KEY CLUSTERED  ([DeveloperClientId])
GO
ALTER TABLE [Auth].[DeveloperClients] ADD CONSTRAINT [FK_DeveloperClients_Clients] FOREIGN KEY ([ClientId]) REFERENCES [Auth].[Clients] ([ClientId])
GO
ALTER TABLE [Auth].[DeveloperClients] ADD CONSTRAINT [FK_DeveloperClients_Developer] FOREIGN KEY ([DeveloperId]) REFERENCES [Auth].[Developer] ([DeveloperId])
GO
