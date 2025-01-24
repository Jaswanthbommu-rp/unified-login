CREATE TABLE [UserAudit].[UserProduct] (
    [UserProductId]         BIGINT        IDENTITY (1, 1) NOT NULL,
    [AuditUserId]           BIGINT        NOT NULL,
    [PersonaId]             BIGINT        NOT NULL,
    [ProductId]             INT           NOT NULL,
    [UserName]              VARCHAR (250) NULL,
    [CreatedDate]           DATETIME2 (7) DEFAULT (getutcdate()) NULL,
    [CompletedDate]         DATETIME2 (7) NULL,
    [Status]                TINYINT       DEFAULT ((0)) NULL
    CONSTRAINT [PK_UserProduct_UserProductId] PRIMARY KEY CLUSTERED ([UserProductId] ASC),
    CONSTRAINT [FK_UserProduct_AuditUserId] FOREIGN KEY ([AuditUserId]) REFERENCES [UserAudit].[User] ([AuditUserId]) ON DELETE CASCADE
);

GO

CREATE NONCLUSTERED INDEX [IDX_AuditUserId_PersonaId_ProductId_UserProduct]
ON [UserAudit].[UserProduct] ([AuditUserId],[PersonaId],[ProductId])
GO
CREATE NONCLUSTERED INDEX [IX_UserProduct_AuditUserId_PersonaId_ProductId]
ON [UserAudit].[UserProduct] ([AuditUserId], [PersonaId], [ProductId])
GO
