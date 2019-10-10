CREATE TABLE [Auth].[ClientSecrets] (
    [ClientSecretId]          INT                IDENTITY (1, 1) NOT NULL,
	   [ClientId]   INT                NOT NULL,
    [Value]       NVARCHAR (250)     NOT NULL,
    [Type]        NVARCHAR (250)     NULL,
    [Description] NVARCHAR (2000)    NULL,
    [Expiration]  DATETIMEOFFSET (7) NULL, 
    CONSTRAINT [PK_dbo.ClientSecrets] PRIMARY KEY CLUSTERED ([ClientSecretId] ASC),
    CONSTRAINT [FK_dbo.ClientSecrets_dbo.Clients_Client_Id] FOREIGN KEY ([ClientId]) REFERENCES [Auth].[Clients] ([ClientId]) ON DELETE CASCADE
);

