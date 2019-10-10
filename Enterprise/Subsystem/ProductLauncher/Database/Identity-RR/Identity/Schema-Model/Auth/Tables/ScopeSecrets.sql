CREATE TABLE [Auth].[ScopeSecrets]
(
[ScopeSecretId] [int] NOT NULL IDENTITY(1, 1),
[ScopeId] [int] NOT NULL,
[Description] [nvarchar] (1000) NULL,
[Type] [nvarchar] (250) NULL,
[Value] [nvarchar] (250) NOT NULL,
[Expiration] [datetimeoffset] NULL
)
GO
ALTER TABLE [Auth].[ScopeSecrets] ADD CONSTRAINT [PK_dbo.ScopeSecrets] PRIMARY KEY CLUSTERED  ([ScopeSecretId])
GO
ALTER TABLE [Auth].[ScopeSecrets] ADD CONSTRAINT [FK_dbo.ScopeSecrets_dbo.Scopes_Scope_Id] FOREIGN KEY ([ScopeId]) REFERENCES [Auth].[Scopes] ([ScopeId]) ON DELETE CASCADE
GO
