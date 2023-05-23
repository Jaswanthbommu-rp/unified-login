CREATE TABLE [Auth].[ClientGrantTypes](
	[Id]        INT            IDENTITY (1, 1) NOT NULL,
    [GrantType] NVARCHAR (250) NOT NULL,
    [ClientId]  INT            NOT NULL,
    CONSTRAINT [PK_ClientGrantTypes] PRIMARY KEY CLUSTERED ([Id] ASC),
	CONSTRAINT [FK_dbo.ClientGrantTypes_dbo.Clients_Id] FOREIGN KEY ([ClientId]) REFERENCES [Auth].[Clients] ([Id]) ON DELETE CASCADE
)
GO

CREATE UNIQUE NONCLUSTERED INDEX [IX_ClientGrantTypes_ClientId_GrantType]
    ON [Auth].[ClientGrantTypes]([ClientId] ASC, [GrantType] ASC);


