CREATE TABLE [Auth].[ClientCorsOrigins] (
    [Id]       INT            IDENTITY (1, 1) NOT NULL,
    [Origin]   NVARCHAR (150) NOT NULL,
    [ClientId] INT            NOT NULL, 
    CONSTRAINT [PK_ClientCorsOrigins] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_dbo.ClientCorsOrigins_dbo.Clients_Id] FOREIGN KEY ([ClientId]) REFERENCES [Auth].[Clients] ([Id]) ON DELETE CASCADE
);

