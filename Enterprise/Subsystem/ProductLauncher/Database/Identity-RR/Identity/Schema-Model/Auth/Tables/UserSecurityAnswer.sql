CREATE TABLE [Auth].[UserSecurityAnswer]
(
[UserSecurityAnswerId] [int] NOT NULL IDENTITY(1, 1),
[SecurityQuestionId] [int] NOT NULL,
[UserId] [bigint] NOT NULL,
[Answer] [nvarchar] (50) NOT NULL,
[CreateDateTime] [smalldatetime] NOT NULL CONSTRAINT [DF_UserSecurityAnswer_CreateDateTime] DEFAULT (getdate())
)
GO
ALTER TABLE [Auth].[UserSecurityAnswer] ADD CONSTRAINT [PK_UserSecurityAnswer] PRIMARY KEY CLUSTERED  ([UserSecurityAnswerId])
GO
ALTER TABLE [Auth].[UserSecurityAnswer] ADD CONSTRAINT [FK_UserSecurityAnswer_SecurityQuestion] FOREIGN KEY ([SecurityQuestionId]) REFERENCES [Auth].[SecurityQuestion] ([SecurityQuestionId])
GO
ALTER TABLE [Auth].[UserSecurityAnswer] ADD CONSTRAINT [FK_UserSecurityAnswer_Users] FOREIGN KEY ([UserId]) REFERENCES [Auth].[Users] ([UserId])
GO
