CREATE TABLE [Auth].[Activity]
(
[ActivityId] [int] NOT NULL,
[ActivityCode] [nvarchar] (50) NOT NULL,
[Description] [nvarchar] (100) NULL,
[MaxActivityAttemptCount] [tinyint] NOT NULL CONSTRAINT [DF_Activity_MaxActivityAttemptCount] DEFAULT ((0)),
[ActivityTokenExpirationMinutes] [int] NOT NULL CONSTRAINT [DF_Activity_ActivityTokenExpirationMinutes] DEFAULT ((0))
)
GO
ALTER TABLE [Auth].[Activity] ADD CONSTRAINT [PK_Activity] PRIMARY KEY CLUSTERED  ([ActivityId])
GO
