CREATE TABLE [Auth].[ClientClaims] (
    [ClientClaimsId]        INT            IDENTITY (1, 1) NOT NULL,
	[ClientId] INT            NOT NULL,
    [Type]      NVARCHAR (100) NOT NULL,
    [Value]     NVARCHAR (100) NOT NULL,    
    CONSTRAINT [PK_dbo.ClientClaims] PRIMARY KEY CLUSTERED ([ClientClaimsId] ASC),
    CONSTRAINT [FK_dbo.ClientClaims_dbo.Clients_Client_Id] FOREIGN KEY ([ClientId]) REFERENCES [Auth].[Clients] ([ClientId]) ON DELETE CASCADE
);

