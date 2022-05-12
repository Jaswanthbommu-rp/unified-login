CREATE TABLE [Enterprise].[UserSyncJob_V2](
       [UserSyncJobId] [bigint] IDENTITY(1,1) NOT NULL,
       [UserPersonaId] [bigint] NOT NULL,
       [EditorUserPersonaId] [bigint] NULL,
       [UserSyncJobTypeId] [tinyint] NOT NULL,
       [StatusTypeId] [int] NOT NULL,
       [CreatedDate] DATETIME2 NOT NULL,
       [ModifiedDate] DATETIME2 NULL,
CONSTRAINT [PK_UserSyncJob_V2_SyncJobId] PRIMARY KEY CLUSTERED 
(
       [UserSyncJobId] ASC
)WITH (PAD_INDEX = ON, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 95) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [Enterprise].[UserSyncJob_V2] ADD  CONSTRAINT [DF_UserSyncJob_V2_CreatedDate]  DEFAULT (getutcdate()) FOR [CreatedDate]
GO

ALTER TABLE [Enterprise].[UserSyncJob_V2]  WITH CHECK ADD  CONSTRAINT [FK_UserSyncJob_V2_UserPersona] FOREIGN KEY([UserPersonaId])
REFERENCES [Person].[Persona] ([PersonaId])
GO
ALTER TABLE [Enterprise].[UserSyncJob_V2]  WITH CHECK ADD  CONSTRAINT [FK_UserSyncJob_V2_EditorUserPersona] FOREIGN KEY([EditorUserPersonaId])
REFERENCES [Person].[Persona] ([PersonaId])
GO
ALTER TABLE [Enterprise].[UserSyncJob_V2] CHECK CONSTRAINT [FK_UserSyncJob_V2_UserPersona]
GO

ALTER TABLE [Enterprise].[UserSyncJob_V2]  WITH CHECK ADD  CONSTRAINT [FK_UserSyncJob_V2_StatusType] FOREIGN KEY([StatusTypeId])
REFERENCES [Enterprise].[StatusType] ([StatusTypeId])
GO

ALTER TABLE [Enterprise].[UserSyncJob_V2] CHECK CONSTRAINT [FK_UserSyncJob_V2_StatusType]
GO

ALTER TABLE [Enterprise].[UserSyncJob_V2]  WITH CHECK ADD  CONSTRAINT [FK_UserSyncJob_V2_UserSyncJobType] FOREIGN KEY([UserSyncJobTypeId])
REFERENCES [Enterprise].[UserSyncJobType] ([UserSyncJobTypeId])
GO

ALTER TABLE [Enterprise].[UserSyncJob_V2] CHECK CONSTRAINT [FK_UserSyncJob_V2_UserSyncJobType]
GO
