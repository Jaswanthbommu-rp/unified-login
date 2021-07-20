CREATE TABLE [Batch].[BatchProcessConfigurationType](
	[BatchProcessConfigurationTypeId] [tinyint] NOT NULL,
	[Name] [varchar](50) NOT NULL,
	[Description] [varchar](100) NULL,
 CONSTRAINT [PK_BatchConfiguratuionType] PRIMARY KEY CLUSTERED 
(
	[BatchProcessConfigurationTypeId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) 
GO

ALTER TABLE [Batch].[BatchProcessor]  WITH CHECK ADD  CONSTRAINT [FK_BatchProcessor_BatchProcessType1] FOREIGN KEY([BatchProcessTypeId])
REFERENCES [Batch].[BatchProcessType] ([BatchProcessTypeId])
GO

ALTER TABLE [Batch].[BatchProcessor] CHECK CONSTRAINT [FK_BatchProcessor_BatchProcessType1]
GO

ALTER TABLE [Batch].[BatchProcessor] ADD  CONSTRAINT [DF_BatchProcessor_CorrelationId]  DEFAULT (NEWSEQUENTIALID()) FOR [CorrelationId]
GO

ALTER TABLE [Batch].[BatchProcessor] ADD  CONSTRAINT [DF_BatchProcessor_RetryCount]  DEFAULT ((0)) FOR [RetryCount]
GO

ALTER TABLE [Batch].[BatchProcessor] ADD  CONSTRAINT [DF_BatchProcessor_CreatedDateTime]  DEFAULT (getutcdate()) FOR [CreatedDateTime]
GO

ALTER TABLE [Batch].[BatchProcessor] ADD  CONSTRAINT [DF_BatchProcessor_LastRunDateTime]  DEFAULT (getutcdate()) FOR [LastRunDateTime]
GO
