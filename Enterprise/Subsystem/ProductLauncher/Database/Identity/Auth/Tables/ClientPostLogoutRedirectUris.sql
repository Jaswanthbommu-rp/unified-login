CREATE TABLE [Auth].[ClientPostLogoutRedirectUris] (
    [Id]                    INT            IDENTITY (1, 1) NOT NULL,
    [PostLogoutRedirectUri] NVARCHAR (400) NOT NULL,
    [ClientId]              INT            NOT NULL,   
    CONSTRAINT [PK_ClientPostLogoutRedirectUris] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_ClientPostLogoutRedirectUris_Clients_ClientId] FOREIGN KEY ([ClientId]) REFERENCES [Auth].[Clients] ([Id]) ON DELETE CASCADE
);

GO

CREATE UNIQUE NONCLUSTERED INDEX [IX_ClientPostLogoutRedirectUris_ClientId_PostLogoutRedirectUri] ON [Auth].[ClientPostLogoutRedirectUris]
(
	[ClientId] ASC,
	[PostLogoutRedirectUri] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
