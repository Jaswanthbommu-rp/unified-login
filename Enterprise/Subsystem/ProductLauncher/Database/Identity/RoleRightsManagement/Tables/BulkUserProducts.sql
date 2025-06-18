
CREATE TABLE [Security].[BulkUserProducts](
	[BulkUserProductId] [bigint] IDENTITY(1,1) NOT NULL,
	[BulkUserBatchProcessId] [bigint] NULL,
	[ProductId] [int] NULL,
	[CreatedDateTime] [datetime] NOT NULL
	,CONSTRAINT [PK_BulkUserProductId] PRIMARY KEY CLUSTERED ([BulkUserProductId] ASC)
	,CONSTRAINT [FK_BulkUserBatchProcessId] FOREIGN KEY ([BulkUserBatchProcessId]) REFERENCES [Batch].[BulkUserBatchProcess]([BulkUserBatchProcessId])
	,CONSTRAINT [FK_BulkUser_ProductId] FOREIGN KEY ([ProductId]) REFERENCES [Enterprise].[Product] ([ProductId])
) ON [PRIMARY]
GO