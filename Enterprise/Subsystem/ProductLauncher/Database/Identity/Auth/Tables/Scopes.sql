CREATE TABLE [Auth].[Scopes] (
    [ScopeId]                             INT             IDENTITY (1, 1) NOT NULL,
    [Name]                           NVARCHAR (200)  NOT NULL,
    [DisplayName]                    NVARCHAR (200)  NULL,
    [Description]                    NVARCHAR (1000) NULL,
	[ClaimsRule]                     NVARCHAR (200)  NULL,
	[Enabled]                        BIT             NOT NULL,
    [Required]                       BIT             NOT NULL,
    [Emphasize]                      BIT             NOT NULL,
    [Type]                           INT             NOT NULL,
    [IncludeAllClaimsForUser]        BIT             NOT NULL,  
    [ShowInDiscoveryDocument]        BIT             NOT NULL,
    [AllowUnrestrictedIntrospection] BIT             NOT NULL,
    CONSTRAINT [PK_dbo.Scopes] PRIMARY KEY CLUSTERED ([ScopeId] ASC)
);

