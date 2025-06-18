
CREATE TABLE [Batch].[BulkUserBatchProcess](
	[BulkUserBatchProcessId] [bigint] IDENTITY(1,1) NOT NULL,
	[EditorUserPersonaId] [bigint] NOT NULL,
	[SubjectUserPersonaId] [bigint] NOT NULL,
	[BatchProcessTypeId] [tinyint] NOT NULL,
	[StatusTypeId] [int] NOT NULL,
	[CreatedDateTime] [datetime] NOT NULL,
	[CompletedDateTime] [datetime] NULL,
 CONSTRAINT [PK_BulkUserBatchProcess] PRIMARY KEY CLUSTERED 
(
	[BulkUserBatchProcessId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
CREATE NONCLUSTERED INDEX [IDX_BulkUserBatchProcess_CreateDateTime] ON [Batch].[BulkUserBatchProcess]
(
[CreatedDateTime] ASC
)
INCLUDE([BulkUserBatchProcessId],[EditorUserPersonaId],[SubjectUserPersonaId],[BatchProcessTypeId],[StatusTypeId]) WITH (SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF) ON [PRIMARY]
