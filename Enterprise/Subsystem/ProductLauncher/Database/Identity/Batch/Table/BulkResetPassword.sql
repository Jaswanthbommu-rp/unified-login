CREATE TABLE [Batch].[BulkResetPassword]
(
    [Id]                BIGINT             NOT NULL PRIMARY KEY IDENTITY(1,1),
    [RealPageId]        UNIQUEIDENTIFIER   NOT NULL,
    [Status]            BIT                NOT NULL CONSTRAINT [DF_BulkResetPassword_Status]          DEFAULT (0),
    [CreatedDateTime]   DATETIME           NOT NULL CONSTRAINT [DF_BulkResetPassword_CreatedDateTime] DEFAULT (GETUTCDATE())
)
GO

CREATE NONCLUSTERED INDEX [IDX_BulkResetPassword_Status]
    ON [Batch].[BulkResetPassword] ([Status])
GO