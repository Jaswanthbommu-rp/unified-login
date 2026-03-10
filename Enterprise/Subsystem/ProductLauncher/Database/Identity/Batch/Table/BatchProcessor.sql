CREATE TABLE [Batch].[BatchProcessor](
	[BatchProcessorId] [bigint] IDENTITY(1,1) NOT NULL,
	[CorrelationId] [uniqueidentifier] NOT NULL,
	[BatchProcessorGroupId] [bigint] NULL,
	[EditorUserPartyId] [bigint] NOT NULL,
	[EditorUserPersonaId] [bigint] NOT NULL,
	[SubjectUserPersonaId] [bigint] NOT NULL,
	[BatchProcessTypeId] [tinyint] NOT NULL,
	[ProductId] [int] NOT NULL,
	[StatusTypeId] [int] NOT NULL,
	[RetryCount] [tinyint] NOT NULL,
	[InputJSON] [nvarchar](max) NOT NULL,
	[CreatedDateTime] [datetime] NOT NULL,
	[LastRunDateTime] [datetime] NOT NULL,
	[ImpersonatorUserId] [BIGINT] DEFAULT 0,
    CONSTRAINT [PK_BatchProcessor] PRIMARY KEY CLUSTERED 
(
	[BatchProcessorId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
CREATE NONCLUSTERED INDEX [IX_BatchProcessor_CreateDateTime] ON [Batch].[BatchProcessor]
(
[CreatedDateTime] ASC,
[EditorUserPartyId] ASC
)
INCLUDE([BatchProcessorId],[CorrelationId],[BatchProcessorGroupId],[EditorUserPersonaId],[SubjectUserPersonaId],[BatchProcessTypeId],[ProductId],[StatusTypeId],[RetryCount],[InputJSON],[LastRunDateTime]) WITH (SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF) ON [PRIMARY]

GO

CREATE NONCLUSTERED INDEX [IDX_BatchProcessor_BatchProcessorGroupId_EditorUserPersonaId] ON [Batch].[BatchProcessor] 
(
	[BatchProcessorGroupId],
	[EditorUserPersonaId],
	[SubjectUserPersonaId],
	[BatchProcessorId],
	[StatusTypeId]
)
INCLUDE ([RetryCount])
GO

CREATE NONCLUSTERED INDEX BatchProcessor_SubjectUserPersonaId_ProductId_StatusTypeId
ON [Batch].[BatchProcessor] ([SubjectUserPersonaId],[ProductId],[StatusTypeId])
INCLUDE ([InputJSON])
GO
