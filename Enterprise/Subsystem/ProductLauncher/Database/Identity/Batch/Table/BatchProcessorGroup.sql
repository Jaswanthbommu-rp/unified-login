CREATE TABLE [Batch].[BatchProcessorGroup](
	[BatchProcessorGroupId] [bigint] IDENTITY(1,1) NOT NULL,
	[BatchProcessorGroupActivityLogged] [bit] NOT NULL,
	[CreateDateTime] [datetime] NOT NULL,
 CONSTRAINT [PK_BatchProcessorGroup] PRIMARY KEY CLUSTERED 
(
	[BatchProcessorGroupId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO


GO

ALTER TABLE [Batch].[BatchProcessorGroup] ADD  CONSTRAINT [DF_BatchProcessorGroup_ActivityLogged]  DEFAULT ((0)) FOR [BatchProcessorGroupActivityLogged]
GO

ALTER TABLE [Batch].[BatchProcessorGroup] ADD  CONSTRAINT [DF_BatchProcessorGroup_CreateDateTime]  DEFAULT (getutcdate()) FOR [CreateDateTime]
GO