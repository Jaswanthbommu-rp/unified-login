CREATE TABLE [UserAudit].[UserRole] (
    [UserProductId] BIGINT        NOT NULL,
    [RoleName]      VARCHAR (250) NULL,
    [CreatedDate]   DATETIME2 (7) DEFAULT (getutcdate()) NULL,
    CONSTRAINT [FK_UserRole_UserProductId] FOREIGN KEY ([UserProductId]) REFERENCES [UserAudit].[UserProduct] ([UserProductId]) ON DELETE CASCADE
);

