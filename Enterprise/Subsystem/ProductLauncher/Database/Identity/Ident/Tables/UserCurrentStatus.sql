CREATE TABLE [Ident].[UserCurrentStatus] (
    [UserCurrentStatusId] BIGINT   IDENTITY (1, 1) NOT NULL,
    [UserId]              BIGINT   NOT NULL,
    [StatusTypeId]        INT      NOT NULL,
    [StatusSetDate]       DATETIME CONSTRAINT [DF_UserCurrentStatus_StatusDateTime] DEFAULT (getutcdate()) NOT NULL,
    [FromDate]            DATETIME CONSTRAINT [DF__UserStatu__FromD__12E8C319] DEFAULT (getutcdate()) NOT NULL,
    [ThruDate]            DATETIME NULL,
    CONSTRAINT [PK_UserStatus] PRIMARY KEY CLUSTERED ([UserCurrentStatusId] ASC),
    CONSTRAINT [FK_UserStatus_StatusType] FOREIGN KEY ([StatusTypeId]) REFERENCES [Enterprise].[StatusType] ([StatusTypeId]) ON UPDATE  CASCADE,
    CONSTRAINT [FK_UserStatus_UserLogin] FOREIGN KEY ([UserId]) REFERENCES [Ident].[UserLogin] ([UserId]) ON DELETE CASCADE
);
GO
CREATE INDEX [IX_UserCurrentStatus_UserId01] ON [Ident].[UserCurrentStatus] ([UserId],[ThruDate]) INCLUDE ([StatusTypeId], [StatusSetDate], [FromDate])
GO
CREATE INDEX [IX_UserCurrentStatus_UserId02] ON [Ident].[UserCurrentStatus] ([UserId], [StatusTypeId])
