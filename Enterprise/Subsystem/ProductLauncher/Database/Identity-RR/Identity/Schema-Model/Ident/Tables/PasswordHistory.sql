CREATE TABLE [Ident].[PasswordHistory]
(
[PasswordHistoryId] [int] NOT NULL IDENTITY(1, 1),
[UserId] [bigint] NOT NULL,
[ActivityId] [int] NOT NULL,
[ChangedPasswordHash] [nvarchar] (1000) NOT NULL,
[ChangedPasswordSalt] [nvarchar] (255) NULL,
[ChangedPasswordDateTime] [smalldatetime] NOT NULL CONSTRAINT [DF_PasswordHistory_ChangedPasswordDateTime] DEFAULT (getutcdate())
)
GO
ALTER TABLE [Ident].[PasswordHistory] ADD CONSTRAINT [PK_PasswordHistory] PRIMARY KEY CLUSTERED  ([PasswordHistoryId])
GO
ALTER TABLE [Ident].[PasswordHistory] ADD CONSTRAINT [FK_PasswordHistory_Activity] FOREIGN KEY ([ActivityId]) REFERENCES [Ident].[Activity] ([ActivityId])
GO
ALTER TABLE [Ident].[PasswordHistory] ADD CONSTRAINT [FK_PasswordHistory_UserLogin] FOREIGN KEY ([UserId]) REFERENCES [Ident].[UserLogin] ([UserId])
GO
