CREATE TABLE [Auth].[PasswordHistory]
(
[PasswordHistoryId] [int] NOT NULL IDENTITY(1, 1),
[UserId] [bigint] NOT NULL,
[ActivityId] [int] NOT NULL,
[ChangedPasswordHash] [nvarchar] (1000) NOT NULL,
[ChangedPasswordSalt] [nvarchar] (255) NULL,
[ChangedPasswordDateTime] [smalldatetime] NOT NULL CONSTRAINT [DF_PasswordHistory_ChangedPasswordDateTime] DEFAULT (getdate())
)
GO
ALTER TABLE [Auth].[PasswordHistory] ADD CONSTRAINT [PK_PasswordHistory] PRIMARY KEY CLUSTERED  ([PasswordHistoryId])
GO
ALTER TABLE [Auth].[PasswordHistory] ADD CONSTRAINT [FK_PasswordHistory_Activity] FOREIGN KEY ([ActivityId]) REFERENCES [Auth].[Activity] ([ActivityId])
GO
ALTER TABLE [Auth].[PasswordHistory] ADD CONSTRAINT [FK_PasswordHistory_Users] FOREIGN KEY ([UserId]) REFERENCES [Auth].[Users] ([UserId])
GO
