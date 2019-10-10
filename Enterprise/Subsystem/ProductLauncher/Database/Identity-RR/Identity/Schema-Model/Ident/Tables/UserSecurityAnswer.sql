CREATE TABLE [Ident].[UserSecurityAnswer]
(
[UserSecurityAnswerId] [int] NOT NULL IDENTITY(1, 1),
[SecurityQuestionId] [int] NOT NULL,
[UserId] [bigint] NOT NULL,
[Answer] [nvarchar] (50) NOT NULL,
[CreateDateTime] [smalldatetime] NOT NULL CONSTRAINT [DF_UserSecurityAnswer_CreateDateTime] DEFAULT (getutcdate())
)
GO
ALTER TABLE [Ident].[UserSecurityAnswer] ADD CONSTRAINT [PK_UserSecurityAnswer] PRIMARY KEY CLUSTERED  ([UserSecurityAnswerId])
GO
ALTER TABLE [Ident].[UserSecurityAnswer] ADD CONSTRAINT [FK_UserSecurityAnswer_SecurityQuestion] FOREIGN KEY ([SecurityQuestionId]) REFERENCES [Ident].[SecurityQuestion] ([SecurityQuestionId]) ON DELETE CASCADE ON UPDATE CASCADE
GO
ALTER TABLE [Ident].[UserSecurityAnswer] ADD CONSTRAINT [FK_UserSecurityAnswer_UserLogin] FOREIGN KEY ([UserId]) REFERENCES [Ident].[UserLogin] ([UserId]) ON DELETE CASCADE ON UPDATE CASCADE
GO
