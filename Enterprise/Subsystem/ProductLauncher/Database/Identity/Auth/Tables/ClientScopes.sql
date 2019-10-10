CREATE TABLE [Auth].[ClientScopes] (
    [ClientScopeId]        INT            IDENTITY (1, 1) NOT NULL,
	[ClientId] INT            NOT NULL,
    [Scope]     NVARCHAR (200) NOT NULL,    
    CONSTRAINT [PK_dbo.ClientScopes] PRIMARY KEY CLUSTERED ([ClientScopeId] ASC),
    CONSTRAINT [FK_dbo.ClientScopes_dbo.Clients_Client_Id] FOREIGN KEY ([ClientId]) REFERENCES [Auth].[Clients] ([ClientId]) ON DELETE CASCADE
);

