CREATE TABLE [Auth].[UserLockAcitvity]
(
[UserLockActivityId] [int] NOT NULL IDENTITY(1, 1),
[UserId] [bigint] NOT NULL,
[AcivityId] [int] NOT NULL,
[LockReason] [nvarchar] (100) NOT NULL,
[IsLockActive] [bit] NOT NULL CONSTRAINT [DF_UserLockAcitvity_IsLockActive] DEFAULT ((0)),
[LockDateTime] [smalldatetime] NOT NULL
)
GO
ALTER TABLE [Auth].[UserLockAcitvity] ADD CONSTRAINT [PK_UserLockAcitvity] PRIMARY KEY CLUSTERED  ([UserLockActivityId])
GO
ALTER TABLE [Auth].[UserLockAcitvity] ADD CONSTRAINT [FK_UserLockAcitvity_Activity] FOREIGN KEY ([AcivityId]) REFERENCES [Auth].[Activity] ([ActivityId])
GO
ALTER TABLE [Auth].[UserLockAcitvity] ADD CONSTRAINT [FK_UserLockAcitvity_Users] FOREIGN KEY ([UserId]) REFERENCES [Auth].[Users] ([UserId])
GO
