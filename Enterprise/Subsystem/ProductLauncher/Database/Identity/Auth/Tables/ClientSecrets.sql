CREATE TABLE [Auth].[ClientSecrets] (
    [Id]          INT             IDENTITY (1, 1) NOT NULL,
    [ClientId]    INT             NOT NULL,
    [Description] NVARCHAR (2000) NULL,
    [Value]       NVARCHAR (4000) NOT NULL,
    [Expiration]  DATETIME2 (7)   NULL,
    [Type]        NVARCHAR (250)  NOT NULL,
    [Created]     DATETIME2 (7)   NOT NULL,
    CONSTRAINT [PK_ClientSecrets] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_dbo.ClientSecrets_dbo.Clients_Id] FOREIGN KEY ([ClientId]) REFERENCES [Auth].[Clients] ([Id]) ON DELETE CASCADE
);

