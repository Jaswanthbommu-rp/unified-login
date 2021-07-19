CREATE TABLE [Batch].[BatchProcessEnterpriseRoleProductUpdate]
(
	[BatchProcessEnterpriseRoleProductUpdateId] [bigint] IDENTITY(1,1) NOT NULL,
	[EditorUserPersonaId] [bigint] NOT NULL,
	[SubjectUserPersonaId] [bigint] NOT NULL,
	[EnterpriseRoleTemplateId] [int] NOT NULL,
	[BatchProcessTypeId] [tinyint] NOT NULL,
	[StatusTypeId] [int] NOT NULL,
	[CreatedDateTime] [datetime] NOT NULL,
    CONSTRAINT [PK_BatchProcessEnterpriseRoleProductUpdate] PRIMARY KEY CLUSTERED 
(
	[BatchProcessEnterpriseRoleProductUpdateId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
CREATE INDEX [IX_BatchProcessEnterproseRoleProductUpdate_SubjectUserPersonaId_StatusTypeId] ON [Batch].[BatchProcessEnterpriseRoleProductUpdate] ([SubjectUserPersonaId], [StatusTypeId]) INCLUDE ([EnterpriseRoleTemplateId])
GO
CREATE INDEX [IX_BatchProcessEnterproseRoleProductUpdate_EditorUserPersonaId_SubjectUserPersonaId]
ON [Batch].[BatchProcessEnterpriseRoleProductUpdate]
( [EditorUserPersonaId], [SubjectUserPersonaId]
);
