CREATE TABLE [Batch].[BatchProcessConfiguration](
	[BatchProcessConfigurationId] [tinyint] NOT NULL,
	[BatchProcessConfigurationTypeId] [tinyint] NOT NULL,
	[Value] [varchar](500) NOT NULL,
 CONSTRAINT [PK_BatchConfiguratuion] PRIMARY KEY CLUSTERED 
(
	[BatchProcessConfigurationId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
)
GO
ALTER TABLE [Batch].[BatchProcessConfiguration]  WITH CHECK ADD  CONSTRAINT [FK_BatchConfiguratuion_BatchConfiguratuionType] FOREIGN KEY([BatchProcessConfigurationTypeId])
REFERENCES [Batch].[BatchProcessConfigurationType] ([BatchProcessConfigurationTypeId])
GO

ALTER TABLE [Batch].[BatchProcessConfiguration] CHECK CONSTRAINT [FK_BatchConfiguratuion_BatchConfiguratuionType]
GO