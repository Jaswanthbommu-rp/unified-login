CREATE TABLE [Maintenance].[PurgeJobHist](
	[JobStartTime] [datetime2](6) NOT NULL,
	[JobProgramName] [nvarchar](20) NOT NULL,
	[TableName] [nvarchar](50) NOT NULL,
	[DeletedRecords] [int] NULL,
	[JobStatus] [nvarchar](2000) NULL,
	[JobRunTime] [numeric](15, 4) NULL
) 

GO