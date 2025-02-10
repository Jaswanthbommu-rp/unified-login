CREATE TYPE [UserAudit].[UserNotificationInfo] AS TABLE (
    [AuditUserId]					BIGINT           NULL,
    [PersonaId]						BIGINT           NULL,
    [ProductName]					NVARCHAR (255)   NULL,
    [CategoryName]					VARCHAR (100)    NULL,
    [Feed]							VARCHAR (50)	 NULL,
    [Banner]						VARCHAR (50)     NULL,
    [Email]							VARCHAR (50)     NULL,
    [EmailSummary]					VARCHAR (50)     NULL,
    [SMS]							VARCHAR (50)     NULL);
