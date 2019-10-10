CREATE TABLE [Enterprise].[ProductBatch](
	[ProductBatchId] [int] IDENTITY(1,1) NOT NULL,
	[PersonPartyId] [bigint] NOT NULL,
	[CreateUserPersonaId] [bigint] NOT NULL,
	[AssignUserPersonaId] [bigint] NOT NULL,
	[ProductId] [int] NOT NULL,
	[StatusTypeId] [int] NOT NULL,
	[RetryCount] [tinyint] NOT NULL,
	[InputJson] [nvarchar](max) NOT NULL,
	[LastRunDate] [smalldatetime] NULL,
	[CreatedDate] [smalldatetime] NOT NULL,
	[ModifiedDate] [smalldatetime] NULL,
	[ErrorDetails] [varchar](max) NULL,
	[BatchTypeId] TINYINT NOT NULL DEFAULT ((0)), 
    [CorrelationId] UNIQUEIDENTIFIER NULL, 
    CONSTRAINT [PK_ProductBatch] PRIMARY KEY CLUSTERED 
(
	[ProductBatchId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO

ALTER TABLE [Enterprise].[ProductBatch] ADD  CONSTRAINT [DF_ProductBatch_StatusId]  DEFAULT ((5)) FOR [StatusTypeId]
GO

ALTER TABLE [Enterprise].[ProductBatch] ADD  CONSTRAINT [DF_ProductBatch_RetryCount]  DEFAULT ((0)) FOR [RetryCount]
GO

ALTER TABLE [Enterprise].[ProductBatch] ADD  CONSTRAINT [DF_ProductBatch_CreatedDate]  DEFAULT (getutcdate()) FOR [CreatedDate]
GO

ALTER TABLE [Enterprise].[ProductBatch]  WITH CHECK ADD  CONSTRAINT [FK_ProductBatch_Person] FOREIGN KEY([PersonPartyId])
REFERENCES [Person].[Person] ([PartyId])
GO

ALTER TABLE [Enterprise].[ProductBatch] CHECK CONSTRAINT [FK_ProductBatch_Person]
GO

ALTER TABLE [Enterprise].[ProductBatch]  WITH CHECK ADD  CONSTRAINT [FK_ProductBatch_StatusType] FOREIGN KEY([StatusTypeId])
REFERENCES [Enterprise].[StatusType] ([StatusTypeId]) ON UPDATE  CASCADE
GO

ALTER TABLE [Enterprise].[ProductBatch] CHECK CONSTRAINT [FK_ProductBatch_StatusType]
GO

--ALTER TABLE [Enterprise].[ProductBatch] WITH CHECK ADD CONSTRAINT [FK_ProductBatch_BatchType] FOREIGN KEY([BatchTypeId])
--REFERENCES [Enterprise].[BatchType]([BatchTypeId])
CREATE INDEX [IX_ProductBatch_Comp)1] ON [Enterprise].[ProductBatch] ([AssignUserPersonaId], [ProductId], [StatusTypeId])

