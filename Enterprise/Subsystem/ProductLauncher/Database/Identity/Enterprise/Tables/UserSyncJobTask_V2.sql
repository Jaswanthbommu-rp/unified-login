CREATE TABLE [Enterprise].[UserSyncJobTask_V2](
	[UserSyncJobTaskId] [bigint] IDENTITY(1,1) NOT NULL,
	[UserSyncJobId] [bigint] NOT NULL,
	[StatusTypeId] [int] NOT NULL,
	[Source] [nvarchar](25) NOT NULL,
	[CreatedDate] DATETIME2 NOT NULL,
	[ModifiedDate] DATETIME2 NULL,
 CONSTRAINT [PK_UserSyncJobTask_V2_SyncJobTaskId] PRIMARY KEY CLUSTERED 
(
	[UserSyncJobTaskId] ASC
)WITH (PAD_INDEX = ON, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 95) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [Enterprise].[UserSyncJobTask_V2] ADD  CONSTRAINT [DF_UserSyncJobTask_V2_CreatedDate]  DEFAULT (getutcdate()) FOR [CreatedDate]
GO

ALTER TABLE [Enterprise].[UserSyncJobTask_V2]  WITH CHECK ADD  CONSTRAINT [FK_UserSyncJobTask_V2_StatusType] FOREIGN KEY([StatusTypeId])
REFERENCES [Enterprise].[StatusType] ([StatusTypeId])
GO

ALTER TABLE [Enterprise].[UserSyncJobTask_V2] CHECK CONSTRAINT [FK_UserSyncJobTask_V2_StatusType]
GO

ALTER TABLE [Enterprise].[UserSyncJobTask_V2]  WITH CHECK ADD  CONSTRAINT [FK_UserSyncJobTask_V2_SyncJob] FOREIGN KEY([UserSyncJobId])
REFERENCES [Enterprise].[UserSyncJob_V2] ([UserSyncJobId])
GO

ALTER TABLE [Enterprise].[UserSyncJobTask_V2] CHECK CONSTRAINT [FK_UserSyncJobTask_V2_SyncJob]
GO
