
CREATE TABLE [UserAudit].[PrimaryPropertyAudit](
	[PrimaryPropertyAuditId] [bigint] IDENTITY(1,1) NOT FOR REPLICATION NOT NULL,
	[RequestId] [bigint] NOT NULL,
	[ProductId] [int] NOT NULL,
	[ProductName] [nvarchar](256) NULL,
	[PropertyGUID] [nvarchar](256) NULL,
	[PropertyName] [nvarchar](256) NULL,
	[ProductInstanceId] [nvarchar](256) NULL,
	[CreatedDate] [datetime] NOT NULL  DEFAULT getutcdate()
) 
