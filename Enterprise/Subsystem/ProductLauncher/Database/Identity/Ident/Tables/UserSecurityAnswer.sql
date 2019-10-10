CREATE TABLE [Ident].[UserSecurityAnswer] (
    [UserSecurityAnswerId] INT           IDENTITY (1, 1) NOT NULL,
    [SecurityQuestionId]   INT           NOT NULL,
    [UserId]               BIGINT        NOT NULL,
    [Answer]               NVARCHAR (50) NOT NULL,
    [CreateDateTime]       SMALLDATETIME CONSTRAINT [DF_UserSecurityAnswer_CreateDateTime] DEFAULT (getutcdate()) NOT NULL,
    CONSTRAINT [PK_UserSecurityAnswer] PRIMARY KEY CLUSTERED ([UserSecurityAnswerId] ASC),
    CONSTRAINT [FK_UserSecurityAnswer_SecurityQuestion] FOREIGN KEY ([SecurityQuestionId]) REFERENCES [Ident].[SecurityQuestion] ([SecurityQuestionId]) ON DELETE CASCADE ON UPDATE CASCADE,
    CONSTRAINT [FK_UserSecurityAnswer_UserLogin] FOREIGN KEY ([UserId]) REFERENCES [Ident].[UserLogin] ([UserId]) ON DELETE CASCADE ON UPDATE CASCADE
);


GO
CREATE INDEX [IX_UserSecurityAnswer_UserId] ON [Ident].[UserSecurityAnswer] ([UserId]) INCLUDE ([SecurityQuestionId])