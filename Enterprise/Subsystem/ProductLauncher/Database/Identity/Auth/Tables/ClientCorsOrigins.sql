CREATE TABLE [Auth].[ClientCorsOrigins] (
    [ClientCorsOriginId]        INT            IDENTITY (1, 1) NOT NULL,
	[ClientId] INT            NOT NULL,
    [Origin]    NVARCHAR (150) NOT NULL,    
    CONSTRAINT [PK_dbo.ClientCorsOrigins] PRIMARY KEY CLUSTERED ([ClientCorsOriginId] ASC),
    CONSTRAINT [FK_dbo.ClientCorsOrigins_dbo.Clients_Client_Id] FOREIGN KEY ([ClientId]) REFERENCES [Auth].[Clients] ([ClientId]) ON DELETE CASCADE
);

