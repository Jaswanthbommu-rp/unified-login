CREATE TABLE [Ident].[UserCurrentStatus]
(
[UserCurrentStatusId] [bigint] NOT NULL IDENTITY(1, 1),
[UserId] [bigint] NOT NULL,
[StatusTypeId] [int] NOT NULL,
[StatusSetDate] [datetime] NOT NULL CONSTRAINT [DF_UserCurrentStatus_StatusDateTime] DEFAULT (getutcdate()),
[FromDate] [datetime] NOT NULL CONSTRAINT [DF__UserStatu__FromD__12E8C319] DEFAULT (getutcdate()),
[ThruDate] [datetime] NULL
)
GO
ALTER TABLE [Ident].[UserCurrentStatus] ADD CONSTRAINT [PK_UserStatus] PRIMARY KEY CLUSTERED  ([UserCurrentStatusId])
GO
ALTER TABLE [Ident].[UserCurrentStatus] ADD CONSTRAINT [FK_UserStatus_StatusType] FOREIGN KEY ([StatusTypeId]) REFERENCES [Enterprise].[StatusType] ([StatusTypeId])
GO
ALTER TABLE [Ident].[UserCurrentStatus] ADD CONSTRAINT [FK_UserStatus_UserLogin] FOREIGN KEY ([UserId]) REFERENCES [Ident].[UserLogin] ([UserId]) ON DELETE CASCADE
GO
