CREATE TABLE [Auth].[IdentityResourceClaims] (
    [Id]                 INT            IDENTITY (1, 1) NOT NULL,
    [IdentityResourceId] INT            NOT NULL,
    [Type]               NVARCHAR (200) NOT NULL,
    CONSTRAINT [PK_IdentityResourceClaims] PRIMARY KEY CLUSTERED ([Id] ASC)
);

GO

CREATE UNIQUE NONCLUSTERED INDEX [IX_IdentityResourceClaims_IdentityResourceId_Type]
    ON [Auth].[IdentityResourceClaims]([IdentityResourceId] ASC, [Type] ASC);
GO

ALTER TABLE [Auth].[IdentityResourceClaims] WITH NOCHECK
    ADD CONSTRAINT [FK_IdentityResourceClaims_IdentityResources_IdentityResourceId] FOREIGN KEY ([IdentityResourceId]) REFERENCES [Auth].[IdentityResources] ([Id]) ON DELETE CASCADE;

GO