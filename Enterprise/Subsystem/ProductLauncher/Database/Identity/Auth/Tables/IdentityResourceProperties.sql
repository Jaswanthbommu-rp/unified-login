CREATE TABLE [Auth].[IdentityResourceProperties] (
    [Id]                 INT             IDENTITY (1, 1) NOT NULL,
    [IdentityResourceId] INT             NOT NULL,
    [Key]                NVARCHAR (250)  NOT NULL,
    [Value]              NVARCHAR (2000) NOT NULL,
    CONSTRAINT [PK_IdentityResourceProperties] PRIMARY KEY CLUSTERED ([Id] ASC)
);

GO

CREATE UNIQUE NONCLUSTERED INDEX [IX_IdentityResourceProperties_IdentityResourceId_Key]
    ON [Auth].[IdentityResourceProperties]([IdentityResourceId] ASC, [Key] ASC);

GO
    
ALTER TABLE [Auth].[IdentityResourceProperties] WITH NOCHECK
    ADD CONSTRAINT [FK_IdentityResourceProperties_IdentityResources_IdentityResourceId] FOREIGN KEY ([IdentityResourceId]) REFERENCES [Auth].[IdentityResources] ([Id]) ON DELETE CASCADE;

GO
