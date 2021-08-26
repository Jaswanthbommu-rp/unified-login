CREATE TABLE [Security].[ADGroupRight](
	[ADGroupRightId] [int] IDENTITY(1,1) NOT NULL,
	[ADGroupId] [int] NOT NULL,
	[RightId] [int] NOT NULL,
	[CreatedBy] [nvarchar](25) NOT NULL,
	[CreatedDate] [datetime] NOT NULL
 CONSTRAINT [PK_ADGroupRight] PRIMARY KEY CLUSTERED 
(
	[ADGroupRightId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [Security].[ADGroupRight] ADD  CONSTRAINT [DF_ADGroupRight_CreatedDate]  DEFAULT (getdate()) FOR [CreatedDate]
GO

ALTER TABLE [Security].[ADGroupRight]  WITH CHECK ADD  CONSTRAINT [FK_ADGroupRight_ADGroup] FOREIGN KEY([ADGroupId])
REFERENCES [Security].[ADGroup] ([ADGroupId])
GO

ALTER TABLE [Security].[ADGroupRight] CHECK CONSTRAINT [FK_ADGroupRight_ADGroup]
GO

ALTER TABLE [Security].[ADGroupRight] WITH CHECK ADD  CONSTRAINT [FK_ADGroupRight_Right] FOREIGN KEY([RightId])
REFERENCES [Security].[Right] ([RightId])
GO

ALTER TABLE [Security].[ADGroupRight] CHECK CONSTRAINT [FK_ADGroupRight_Right]
GO
