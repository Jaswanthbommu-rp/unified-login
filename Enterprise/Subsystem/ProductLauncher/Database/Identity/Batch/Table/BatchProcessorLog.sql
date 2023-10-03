
CREATE TABLE [Batch].[BatchProcessorLog]
(
	BatchProcessorLogId [bigint] IDENTITY(1,1) NOT NULL,
	[BatchProcessorId] [bigint] NOT NULL,
	StartDatetime DATETIME2 NOT NULL,
	EndDatetime DATETIME2 NULL,
	[CreatedDateTime] [datetime] NOT NULL,
 CONSTRAINT [PK_BatchProcessorLogId] PRIMARY KEY CLUSTERED 
(
	BatchProcessorLogId ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) 
GO

ALTER TABLE [Batch].[BatchProcessorLog] ADD  CONSTRAINT [DF_BatchProcessorLog_CreatedDateTime]  DEFAULT (getutcdate()) FOR [CreatedDateTime]
GO

ALTER TABLE [Batch].[BatchProcessorLog]  WITH CHECK ADD  CONSTRAINT [FK_BatchLog_BatchProcessor] FOREIGN KEY([BatchProcessorId])
REFERENCES [Batch].[BatchProcessor] ([BatchProcessorId])
GO

ALTER TABLE [Batch].[BatchProcessorLog] CHECK CONSTRAINT [FK_BatchLog_BatchProcessor]
GO

CREATE NONCLUSTERED INDEX [IDX_BatchProcessorLog_BatchProcessorId]
	ON [Batch].[BatchProcessorLog] ([BatchProcessorId])

GO