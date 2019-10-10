CREATE TABLE [Auth].[ClientRedirectUris] (
    [ClientRedirectUriId]        INT             IDENTITY (1, 1) NOT NULL,
	 [ClientId] INT             NOT NULL,
    [Uri]       NVARCHAR (2000) NOT NULL,   
    CONSTRAINT [PK_dbo.ClientRedirectUris] PRIMARY KEY CLUSTERED ([ClientRedirectUriId] ASC),
    CONSTRAINT [FK_dbo.ClientRedirectUris_dbo.Clients_Client_Id] FOREIGN KEY ([ClientId]) REFERENCES [Auth].[Clients] ([ClientId]) ON DELETE CASCADE
);

