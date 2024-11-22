
CREATE TABLE [UserAudit].[PrimaryPropertyAudit](
	[PrimaryPropertyAuditId] [bigint] IDENTITY(1,1) NOT FOR REPLICATION NOT NULL,
	[RequestId] [bigint] NOT NULL,
	[ProductId] [int] NOT NULL,
	[ProductName] [nvarchar](256) NULL,
	[PropertyGUID] [nvarchar](256) NULL,
	[PropertyName] [nvarchar](256) NULL,
	[ProductInstanceId] [nvarchar](256) NULL,
	[CreatedDate] [datetime] NOT NULL  DEFAULT getutcdate()
) ;

GO
CREATE NONCLUSTERED INDEX [IDX_PrimaryPropertyAudit_PropertyGUID] ON [UserAudit].[PrimaryPropertyAudit]
(
	[PropertyGUID] ASC
)

GO
CREATE NONCLUSTERED INDEX [IDX_REQPROD_PrimaryPropertyAudit] ON [UserAudit].[PrimaryPropertyAudit]
(
	[RequestId] ASC,
	[ProductId] ASC
)
