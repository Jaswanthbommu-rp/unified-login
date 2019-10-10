CREATE TABLE [Auth].[Scopes]
(
[ScopeId] [int] NOT NULL IDENTITY(1, 1),
[Name] [nvarchar] (200) NOT NULL,
[DisplayName] [nvarchar] (200) NULL,
[Description] [nvarchar] (1000) NULL,
[ClaimsRule] [nvarchar] (200) NULL,
[Enabled] [bit] NOT NULL,
[Required] [bit] NOT NULL,
[Emphasize] [bit] NOT NULL,
[Type] [int] NOT NULL,
[IncludeAllClaimsForUser] [bit] NOT NULL,
[ShowInDiscoveryDocument] [bit] NOT NULL,
[AllowUnrestrictedIntrospection] [bit] NOT NULL
)
GO
ALTER TABLE [Auth].[Scopes] ADD CONSTRAINT [PK_dbo.Scopes] PRIMARY KEY CLUSTERED  ([ScopeId])
GO
