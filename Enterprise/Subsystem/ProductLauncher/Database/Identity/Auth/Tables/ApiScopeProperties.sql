CREATE TABLE [Auth].[ApiScopeProperties](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[ScopeId] [int] NOT NULL,
	[Key] [nvarchar](250) NOT NULL,
	[Value] [nvarchar](2000) NOT NULL,
 CONSTRAINT [PK_ApiScopeProperties] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [Auth].[ApiScopeProperties]  WITH CHECK ADD  CONSTRAINT [FK_ApiScopeProperties_ApiScopes_ScopeId] FOREIGN KEY([ScopeId])
REFERENCES [Auth].[ApiScopes] ([Id])
ON DELETE CASCADE
GO

CREATE UNIQUE NONCLUSTERED INDEX [IX_ApiScopeProperties_ScopeId_Key] ON [Auth].[ApiScopeProperties]([ScopeId] ASC, [Key] ASC);
GO

