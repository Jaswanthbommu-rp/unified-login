CREATE TABLE [Enterprise].[GlobalControl](
	[GlobalControlId] [int] NOT NULL,
	[ControlName] [nvarchar](50) NOT NULL,
	[ControlValue] [bit] NOT NULL,
	[Description] [nvarchar](200) NULL,
	[CreateDateTime] [datetime] NULL,
 CONSTRAINT [PK_GlobalControl] PRIMARY KEY CLUSTERED 
(
	[GlobalControlId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) 
GO

ALTER TABLE [Enterprise].[GlobalControl] ADD  CONSTRAINT [DF_GlobalControl_CreateDateTime]  DEFAULT (getutcdate()) FOR [CreateDateTime]
GO
