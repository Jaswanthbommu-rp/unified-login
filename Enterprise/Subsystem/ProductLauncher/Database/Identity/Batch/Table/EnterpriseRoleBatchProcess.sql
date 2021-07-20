CREATE TABLE [Batch].[EnterpriseRoleBatchProcess]
(
	[EnterpriseRoleBatchProcessId] [bigint] IDENTITY(1,1) NOT NULL,
	[EditorUserPersonaId] [bigint] NOT NULL,
	[SubjectUserPersonaId] [bigint] NOT NULL,
	[EnterpriseRoleTemplateId] [int] NOT NULL,
	[BatchProcessTypeId] [tinyint] NOT NULL,
	[StatusTypeId] [int] NOT NULL,
	[CreatedDateTime] [datetime] NOT NULL,
    CONSTRAINT [PK_EnterpriseRoleBatchProcess] PRIMARY KEY CLUSTERED 
(
	[EnterpriseRoleBatchProcessId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
)
GO
CREATE INDEX [IX_EnterpriseRoleBatchProcess_SubjectUserPersonaId_StatusTypeId] ON [Batch].[EnterpriseRoleBatchProcess] ([SubjectUserPersonaId], [StatusTypeId]) INCLUDE ([EnterpriseRoleTemplateId])
GO
CREATE INDEX [IX_EnterpriseRoleBatchProcess_EditorUserPersonaId_SubjectUserPersonaId]
ON [Batch].[EnterpriseRoleBatchProcess]
( [EditorUserPersonaId], [SubjectUserPersonaId]
);
