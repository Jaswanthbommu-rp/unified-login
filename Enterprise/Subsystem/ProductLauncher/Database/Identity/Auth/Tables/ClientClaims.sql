CREATE TABLE [Auth].[ClientClaims] (
    [Id]       INT            IDENTITY (1, 1) NOT NULL,
    [Type]     NVARCHAR (250) NOT NULL,
    [Value]    NVARCHAR (250) NOT NULL,
    [ClientId] INT            NOT NULL, 
    CONSTRAINT [PK_ClientClaims] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_ClientClaims_Clients_Id] FOREIGN KEY ([ClientId]) REFERENCES [Auth].[Clients] ([Id]) ON DELETE CASCADE
);

