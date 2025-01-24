CREATE TABLE [UserAudit].[UserProductError] (
    [UserProductErrorId] BIGINT        IDENTITY (1, 1) NOT NULL,
    [UserProductId]      BIGINT        NOT NULL,
    [ErrorText]          VARCHAR (MAX) NULL,
    CONSTRAINT [PK_UserProductError_UserProductErrorId] PRIMARY KEY CLUSTERED ([UserProductErrorId] ASC),
    CONSTRAINT [FK_UserProduct_UserProductId] FOREIGN KEY ([UserProductId]) REFERENCES [UserAudit].[UserProduct] ([UserProductId]) ON DELETE CASCADE
);

GO
CREATE NONCLUSTERED INDEX IDX_UserProductError_UserProductId 
ON [UserAudit].[UserProductError] ([UserProductId])
GO
CREATE NONCLUSTERED INDEX [IX_UserProductError_UserProductId]
ON [UserAudit].[UserProductError] ([UserProductId])
GO