CREATE TABLE [Auth].[ScopeSecrets] (
    [ScopeSecretId]          INT                IDENTITY (1, 1) NOT NULL,
	[ScopeId]    INT                NOT NULL,
    [Description] NVARCHAR (1000)    NULL,   
    [Type]        NVARCHAR (250)     NULL,
    [Value]       NVARCHAR (250)     NOT NULL,
	[Expiration]  DATETIMEOFFSET (7) NULL,
    CONSTRAINT [PK_dbo.ScopeSecrets] PRIMARY KEY CLUSTERED ([ScopeSecretId] ASC),
    CONSTRAINT [FK_dbo.ScopeSecrets_dbo.Scopes_Scope_Id] FOREIGN KEY ([ScopeId]) REFERENCES [Auth].[Scopes] ([ScopeId]) ON DELETE CASCADE
);

