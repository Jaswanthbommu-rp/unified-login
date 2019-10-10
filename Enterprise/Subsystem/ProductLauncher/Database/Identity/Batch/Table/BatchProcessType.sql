CREATE TABLE [Batch].[BatchProcessType](
	[BatchProcessTypeId] [tinyint] NOT NULL,
	[BatchProcessConfigurationId] [tinyint] NOT NULL,
	[Description] [varchar](100) NULL,
	[Name] [varchar](50) NOT NULL,
 CONSTRAINT [PK__BatchTyp__752A87EECADA4BBD] PRIMARY KEY CLUSTERED 
(
	[BatchProcessTypeId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) 
GO
ALTER TABLE [Batch].[BatchProcessType]  WITH CHECK ADD  CONSTRAINT [FK_BatchProcessType_BatchConfiguratuion] FOREIGN KEY([BatchProcessConfigurationId])
REFERENCES [Batch].[BatchProcessConfiguration] ([BatchProcessConfigurationId])
GO

ALTER TABLE [Batch].[BatchProcessType] CHECK CONSTRAINT [FK_BatchProcessType_BatchConfiguratuion]
GO