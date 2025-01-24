CREATE TABLE [UserAudit].[User] (
    [AuditUserId]            BIGINT           IDENTITY (1, 1) NOT NULL,
    [RequestId]              BIGINT           NOT NULL,
    [PersonaId]              BIGINT           NOT NULL,
    [FirstName]              [dbo].[Name]     NULL,
    [LastName]               [dbo].[Name]     NULL,
    [UserName]               VARCHAR (255)    NULL,
    [UserType]               VARCHAR (100)    NULL,
    [LastLoginDate]          DATETIME2 (7)    NULL,
    [LastUpdateDate]         DATETIME2 (7)    NULL,
    [Status]                 VARCHAR (50)     NULL,
    [CreatedDate]            DATETIME2 (7)    DEFAULT (getutcdate()) NULL,
    [CompletedDate]          DATETIME2 (7)    NULL,
    [Complete]               TINYINT          DEFAULT ((0)) NULL,
    [OrganizationRealPageId] UNIQUEIDENTIFIER NULL,
    [OrganizationPartyId]    BIGINT           NULL,
    CONSTRAINT [PK_User_AuditUserId] PRIMARY KEY CLUSTERED ([AuditUserId] ASC),
    CONSTRAINT [FK_User_RequestId] FOREIGN KEY ([RequestId]) REFERENCES [UserAudit].[Request] ([RequestId]) ON DELETE CASCADE
);

GO
CREATE NONCLUSTERED INDEX [IX_User_RequestId]
ON [UserAudit].[User] ([RequestId])
INCLUDE ([PersonaId], [UserName], [Complete], [OrganizationRealPageId], [OrganizationPartyId])
GO

