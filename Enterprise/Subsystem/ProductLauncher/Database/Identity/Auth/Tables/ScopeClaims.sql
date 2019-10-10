CREATE TABLE [Auth].[ScopeClaims] (
    [ScopeClaimId]                     INT             IDENTITY (1, 1) NOT NULL,
	[ScopeId]               INT             NOT NULL,
    [Name]                   NVARCHAR (200)  NOT NULL,
    [Description]            NVARCHAR (1000) NULL,
    [AlwaysIncludeInIdToken] BIT             NOT NULL,
    CONSTRAINT [PK_dbo.ScopeClaims] PRIMARY KEY CLUSTERED ([ScopeClaimId] ASC),
    CONSTRAINT [FK_dbo.ScopeClaims_dbo.Scopes_Scope_Id] FOREIGN KEY ([ScopeId]) REFERENCES [Auth].[Scopes] ([ScopeId]) ON DELETE CASCADE
);

