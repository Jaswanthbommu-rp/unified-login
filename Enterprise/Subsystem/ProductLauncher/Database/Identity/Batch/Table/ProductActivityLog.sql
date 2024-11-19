CREATE TABLE [Batch].[ProductActivityLog](
	[ProductActivityLogId] [int] IDENTITY(1,1) NOT NULL,
	[BatchProcessorGroupId] [bigint] NOT NULL,
	[ProductId] [int] NOT NULL,
	[Key] [varchar](512) NOT NULL,
	[ActivityJSONMessage] [nvarchar](max) NULL,
 CONSTRAINT [PK_ProductActivityLog] PRIMARY KEY CLUSTERED 
(
	[ProductActivityLogId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO

ALTER TABLE [Batch].[ProductActivityLog]  WITH CHECK ADD  CONSTRAINT [FK_ProductActivityLog_BatchProcessor] FOREIGN KEY([BatchProcessorGroupId])
REFERENCES [Batch].[BatchProcessorGroup] ([BatchProcessorGroupId])
GO

ALTER TABLE [Batch].[ProductActivityLog] CHECK CONSTRAINT [FK_ProductActivityLog_BatchProcessor]
GO
