CREATE TABLE [Ident].[Activity]
(
[ActivityId] [int] NOT NULL,
[ActivityCode] [varchar] (50) NOT NULL,
[Description] [varchar] (100) NULL,
[MaxActivityAttemptCount] [tinyint] NOT NULL,
[ActivityTokenExpirationMinutes] [int] NOT NULL
)
GO
ALTER TABLE [Ident].[Activity] ADD CONSTRAINT [PK_Activity_1] PRIMARY KEY CLUSTERED  ([ActivityId])
GO
