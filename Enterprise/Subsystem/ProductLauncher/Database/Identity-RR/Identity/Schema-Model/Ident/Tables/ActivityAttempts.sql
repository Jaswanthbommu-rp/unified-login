CREATE TABLE [Ident].[ActivityAttempts]
(
[ActivityAttemptsId] [int] NOT NULL IDENTITY(1, 1),
[ActivityId] [int] NOT NULL,
[EnterpriseUserName] [nvarchar] (50) NOT NULL,
[AuthenticationServiceId] [nvarchar] (50) NULL,
[AttemptCount] [tinyint] NOT NULL CONSTRAINT [DF_ActivityAttempts_AttemptCount] DEFAULT ((0)),
[IpAddress] [nvarchar] (50) NULL,
[BrowserType] [nvarchar] (20) NULL,
[BrowserName] [nvarchar] (20) NULL,
[Version] [nvarchar] (10) NULL,
[Platform] [nvarchar] (20) NULL,
[IsMobile] [bit] NOT NULL CONSTRAINT [DF_ActivityAttempts_IsMobile] DEFAULT ((0)),
[DeviceType] [nvarchar] (20) NULL,
[LastAttemptDateTime] [smalldatetime] NOT NULL CONSTRAINT [DF_ActivityAttempts_LastAttemptDateTime] DEFAULT (getutcdate()),
[Timezone] [nvarchar] (100) NULL
)
GO
ALTER TABLE [Ident].[ActivityAttempts] ADD CONSTRAINT [PK_ActivityAttempts] PRIMARY KEY CLUSTERED  ([ActivityAttemptsId])
GO
ALTER TABLE [Ident].[ActivityAttempts] ADD CONSTRAINT [FK_ActivityAttempts_Activity] FOREIGN KEY ([ActivityId]) REFERENCES [Ident].[Activity] ([ActivityId])
GO
