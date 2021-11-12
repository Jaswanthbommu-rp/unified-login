CREATE TABLE [Batch].[BatchProcessorError](
	[BatchProcessorErrorId] [int] IDENTITY(1,1) NOT NULL,
	[BatchProcessorId] [bigint] NOT NULL,
	[Error] [varchar](max) NULL,
	[CreatedDateTime] DATETIME NOT NULL,
 CONSTRAINT [PK_BatchError] PRIMARY KEY CLUSTERED 
(
	[BatchProcessorErrorId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) 
GO
CREATE INDEX [IX_BatchProcessorError_BatchProcessorId]
ON [Batch].[BatchProcessorError]
( [BatchProcessorId]
) INCLUDE( [Error] );
GO
ALTER TABLE [Batch].[BatchProcessorError]  WITH CHECK ADD  CONSTRAINT [FK_BatchError_BatchProcessor] FOREIGN KEY([BatchProcessorId])
REFERENCES [Batch].[BatchProcessor] ([BatchProcessorId])
GO

ALTER TABLE [Batch].[BatchProcessorError] CHECK CONSTRAINT [FK_BatchError_BatchProcessor]
GO

ALTER TABLE [Batch].[BatchProcessorError] ADD  CONSTRAINT [DF_BatchProcessorError_CreatedDateTime]  DEFAULT (getutcdate()) FOR [CreatedDateTime]
GO