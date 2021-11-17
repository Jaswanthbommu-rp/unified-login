CREATE TABLE [Security].[RightRoute](
	[RightRouteId] [bigint] IDENTITY(1,1) NOT NULL,
	[RightId] INT NOT NULL,
	[RouteId] INT NULL,
	[CreatedBy] [nvarchar](25) NOT NULL,
	[CreatedDate] [datetime] NOT NULL,
 CONSTRAINT [PK_RightRoute] PRIMARY KEY CLUSTERED 
(
	[RightRouteId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
ALTER TABLE [Security].[RightRoute] ADD  CONSTRAINT [DF_RightRoute_CreatedDate]  DEFAULT (getdate()) FOR [CreatedDate]
GO
ALTER TABLE [Security].[RightRoute]  WITH CHECK ADD  CONSTRAINT [FK_RightRoute_Right] FOREIGN KEY([RightId])
REFERENCES [Security].[Right] ([RightId])
GO
ALTER TABLE [Security].[RightRoute] CHECK CONSTRAINT [FK_RightRoute_Right]
GO
ALTER TABLE [Security].[RightRoute]  WITH CHECK ADD  CONSTRAINT [FK_RightRoute_Route] FOREIGN KEY([RouteId])
REFERENCES [Security].[Route] ([RouteId])
GO
ALTER TABLE [Security].[RightRoute] CHECK CONSTRAINT [FK_RightRoute_Route]
GO
