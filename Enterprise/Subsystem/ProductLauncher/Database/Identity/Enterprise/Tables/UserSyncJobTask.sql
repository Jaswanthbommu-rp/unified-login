CREATE TABLE [Enterprise].[UserSyncJobTask]
(
	[UserSyncJobTaskId]	BIGINT			NOT NULL CONSTRAINT [PK_UserSyncJobTask_SyncJobTaskId] PRIMARY KEY IDENTITY(1, 1),
	[UserSyncJobId]		BIGINT			NOT NULL,
	[StatusTypeId]		INT				NOT NULL,
	[Source]			NVARCHAR (25)   NOT NULL,
	[CreatedDate]		DATETIME		NOT NULL CONSTRAINT [DF_UserSyncJobTask_CreatedDate] DEFAULT (GETUTCDATE()),
	[ModifiedDate]		DATETIME		NULL,
	CONSTRAINT [FK_UserSyncJobTask_SyncJob] FOREIGN KEY ([UserSyncJobId]) REFERENCES [Enterprise].[UserSyncJob]([UserSyncJobId]),
	CONSTRAINT [FK_UserSyncJobTask_StatusType] FOREIGN KEY ([StatusTypeId]) REFERENCES [Enterprise].[StatusType]([StatusTypeId])
)
