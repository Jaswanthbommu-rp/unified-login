CREATE TABLE [Maintenance].[PurgeConfigParams](
	[ProgramName] [NVARCHAR](20) NOT NULL,
	[PurgeId] [INT] NULL,
	[SchemaName] [NVARCHAR](50) NOT NULL,
	[TableName] [NVARCHAR](50) NOT NULL,
	[ColumnName] [NVARCHAR](30) NULL,
	[Hst_TableName] [NVARCHAR](50) NULL,
	[RetentionDays] [INT] NULL,
	[CommitPoint] [INT] NULL,
	[PurgeFlag] [NCHAR](1) NULL,
	[BackupFlag] [NCHAR](1) NULL,
 CONSTRAINT [Pk_Purge] PRIMARY KEY CLUSTERED 
(
	[ProgramName] ASC,
	[TableName] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)
) 
GO
