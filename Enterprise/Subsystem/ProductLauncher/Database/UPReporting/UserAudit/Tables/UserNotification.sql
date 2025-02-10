CREATE TABLE [UserAudit].[UserNotification] (
    [UserNotificationsId]           BIGINT			 IDENTITY (1, 1) NOT NULL,
	[AuditUserId]					BIGINT           NOT NULL,
    [PersonaId]						BIGINT           NOT NULL,
    [ProductName]					NVARCHAR (255)   NULL,
    [CategoryName]					VARCHAR (100)    NULL,
    [Feed]							VARCHAR (50)	 NULL,
    [Banner]						VARCHAR (50)     NULL,
    [Email]							VARCHAR (50)     NULL,
    [EmailSummary]					VARCHAR (50)     NULL,
    [SMS]							VARCHAR (50)     NULL,
    [CreatedDate]					DATETIME2 (7)    DEFAULT (getutcdate()) NULL,
    CONSTRAINT [PK_UserNotification_UserNotificationsId] PRIMARY KEY CLUSTERED ([UserNotificationsId] ASC),
    CONSTRAINT [FK_UserNotification_AuditUserId] FOREIGN KEY ([AuditUserId]) REFERENCES [UserAudit].[User] ([AuditUserId]) ON DELETE CASCADE
);
