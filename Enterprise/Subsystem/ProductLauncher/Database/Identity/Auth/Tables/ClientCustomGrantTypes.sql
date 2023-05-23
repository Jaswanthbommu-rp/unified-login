--DROP
CREATE TABLE [Auth].[ClientCustomGrantTypes] (
    [ClientCustomGrantTypeId]        INT            IDENTITY (1, 1) NOT NULL,
	[ClientId] INT            NOT NULL,
    [GrantType] NVARCHAR (250) NOT NULL,   
    CONSTRAINT [PK_dbo.ClientCustomGrantTypes] PRIMARY KEY CLUSTERED ([ClientCustomGrantTypeId] ASC),
    CONSTRAINT [FK_dbo.ClientCustomGrantTypes_dbo.Clients_Id] FOREIGN KEY ([ClientId]) REFERENCES [Auth].[Clients] ([Id]) ON DELETE CASCADE
);

