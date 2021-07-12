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
    CONSTRAINT [PK_BatchProcessor] PRIMARY KEY CLUSTERED 
(
	[BatchProcessorId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
CREATE INDEX [IX_BatchProcessor_SubjectUserPersonaId_StatusTypeId] ON [Batch].[BatchProcessor] ([SubjectUserPersonaId], [StatusTypeId]) INCLUDE ([ProductId])
GO
CREATE INDEX [IX_BatchProcessor_EditorUserPersonaId_SubjectUserPersonaId]
ON [Batch].[BatchProcessor]
( [EditorUserPersonaId], [SubjectUserPersonaId]
);