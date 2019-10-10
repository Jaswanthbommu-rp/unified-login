CREATE TABLE [Enterprise].[ProductBatch]
(
[ProductBatchId] [int] NOT NULL IDENTITY(1, 1),
[PersonPartyId] [bigint] NOT NULL,
[CreateUserPersonaId] [bigint] NOT NULL,
[AssignUserPersonaId] [bigint] NOT NULL,
[ProductId] [int] NOT NULL,
[StatusTypeId] [int] NOT NULL CONSTRAINT [DF_ProductBatch_StatusId] DEFAULT ((5)),
[RetryCount] [tinyint] NOT NULL CONSTRAINT [DF_ProductBatch_RetryCount] DEFAULT ((0)),
[InputJson] [nvarchar] (max) NOT NULL,
[LastRunDate] [smalldatetime] NULL,
[CreatedDate] [smalldatetime] NOT NULL CONSTRAINT [DF_ProductBatch_CreatedDate] DEFAULT (getutcdate()),
[ModifiedDate] [smalldatetime] NULL,
[ErrorDetails] [varchar] (max) NULL
)
GO
ALTER TABLE [Enterprise].[ProductBatch] ADD CONSTRAINT [PK_ProductBatch] PRIMARY KEY CLUSTERED  ([ProductBatchId])
GO
ALTER TABLE [Enterprise].[ProductBatch] ADD CONSTRAINT [FK_ProductBatch_Person] FOREIGN KEY ([PersonPartyId]) REFERENCES [Person].[Person] ([PartyId])
GO
ALTER TABLE [Enterprise].[ProductBatch] ADD CONSTRAINT [FK_ProductBatch_StatusType] FOREIGN KEY ([StatusTypeId]) REFERENCES [Enterprise].[StatusType] ([StatusTypeId])
GO
