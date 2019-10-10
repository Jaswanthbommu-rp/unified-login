CREATE TABLE [Auth].[ScopeClaims]
(
[ScopeClaimId] [int] NOT NULL IDENTITY(1, 1),
[ScopeId] [int] NOT NULL,
[Name] [nvarchar] (200) NOT NULL,
[Description] [nvarchar] (1000) NULL,
[AlwaysIncludeInIdToken] [bit] NOT NULL
)
GO
ALTER TABLE [Auth].[ScopeClaims] ADD CONSTRAINT [PK_dbo.ScopeClaims] PRIMARY KEY CLUSTERED  ([ScopeClaimId])
GO
ALTER TABLE [Auth].[ScopeClaims] ADD CONSTRAINT [FK_dbo.ScopeClaims_dbo.Scopes_Scope_Id] FOREIGN KEY ([ScopeId]) REFERENCES [Auth].[Scopes] ([ScopeId]) ON DELETE CASCADE
GO
