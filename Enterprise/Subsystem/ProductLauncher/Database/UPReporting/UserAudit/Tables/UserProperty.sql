CREATE TABLE [UserAudit].[UserProperty] (
    [UserProductId] BIGINT        NOT NULL,
    [PropertyName]  VARCHAR (250) NULL,
    [PropertyId]    VARCHAR (250)  NULL,
    [CreatedDate]   DATETIME2 (7) DEFAULT (getutcdate()) NULL,
    CONSTRAINT [FK_UserProperty_UserProductId] FOREIGN KEY ([UserProductId]) REFERENCES [UserAudit].[UserProduct] ([UserProductId]) ON DELETE CASCADE
);

GO
CREATE NONCLUSTERED INDEX [IX_UserProperty_UserProductId]
ON [UserAudit].[UserProperty] ([UserProductId])
INCLUDE ([PropertyName])
GO