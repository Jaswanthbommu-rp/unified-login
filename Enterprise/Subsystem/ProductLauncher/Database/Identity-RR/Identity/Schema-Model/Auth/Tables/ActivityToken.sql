CREATE TABLE [Auth].[ActivityToken]
(
[ActivityTokenId] [int] NOT NULL IDENTITY(1, 1),
[ActivityId] [int] NOT NULL,
[UserId] [bigint] NOT NULL,
[ActivityToken] [nvarchar] (100) NOT NULL,
[IsActive] [bit] NOT NULL CONSTRAINT [DF_ActivityToken_IsActive] DEFAULT ((0)),
[CreateDateTime] [smalldatetime] NOT NULL CONSTRAINT [DF_ActivityToken_CreateDateTime] DEFAULT (getdate()),
[ExpireDateTime] [smalldatetime] NOT NULL
)
GO
ALTER TABLE [Auth].[ActivityToken] ADD CONSTRAINT [PK_ActivityToken] PRIMARY KEY CLUSTERED  ([ActivityTokenId])
GO
ALTER TABLE [Auth].[ActivityToken] ADD CONSTRAINT [FK_ActivityToken_Activity] FOREIGN KEY ([ActivityId]) REFERENCES [Auth].[Activity] ([ActivityId])
GO
ALTER TABLE [Auth].[ActivityToken] ADD CONSTRAINT [FK_ActivityToken_Users] FOREIGN KEY ([UserId]) REFERENCES [Auth].[Users] ([UserId])
GO
