CREATE TABLE [Enterprise].[UserSyncJob]
(
	[UserSyncJobId]	BIGINT   NOT NULL CONSTRAINT [PK_UserSyncJob_SyncJobId] PRIMARY KEY IDENTITY(1, 1),
	[PersonaId]		BIGINT   NOT NULL,
	[StatusTypeId]	INT		 NOT NULL,
	[CreatedDate]   DATETIME NOT NULL CONSTRAINT [DF_UserSyncJob_CreatedDate] DEFAULT (GETUTCDATE()),
	[ModifiedDate]	DATETIME NULL,
	CONSTRAINT [FK_UserSyncJob_Persona] FOREIGN KEY ([PersonaId]) REFERENCES [Person].[Persona]([PersonaId]),
	CONSTRAINT [FK_UserSyncJob_StatusType] FOREIGN KEY ([StatusTypeId]) REFERENCES [Enterprise].[StatusType]([StatusTypeId])
)
