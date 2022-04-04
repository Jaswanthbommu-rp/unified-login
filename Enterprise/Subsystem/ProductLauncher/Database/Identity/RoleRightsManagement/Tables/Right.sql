CREATE TABLE [Security].[Right](
	[RightId] INT IDENTITY(1,1) NOT NULL,
	[RightName] [nvarchar](100) NOT NULL,
	[Description] [nvarchar](255) NULL,
	[Value] [nvarchar](255) NULL,
	[StatusTypeId] [int] NULL,
	[VisibilityStatusId] [int] NULL,
	[ProductId] [int] NULL,
	[TargetProductId] [int] NULL,	
	[CreatedBy] [nvarchar](255) NOT NULL,
	[CreatedDate] [datetime] NOT NULL,
	[PersistRight] BIT NOT NULL DEFAULT 0, 
 CONSTRAINT [PK_SecurityRight] PRIMARY KEY CLUSTERED 
(
	[RightId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
ALTER TABLE [Security].[Right]  WITH CHECK ADD  CONSTRAINT [FK_Right_StatusType] FOREIGN KEY([StatusTypeId])
REFERENCES [Enterprise].[StatusType] ([StatusTypeId])
GO
ALTER TABLE [Security].[Right]  WITH CHECK ADD  CONSTRAINT [FK_Right_VisibilityStatus_StatusType] FOREIGN KEY([VisibilityStatusId])
REFERENCES [Enterprise].[StatusType] ([StatusTypeId])
GO
ALTER TABLE [Security].[Right]  WITH CHECK ADD  CONSTRAINT [FK_Right_Product] FOREIGN KEY([ProductId])
REFERENCES [Enterprise].[Product] ([ProductId])
GO
ALTER TABLE [Security].[Right]  WITH CHECK ADD  CONSTRAINT [FK_Right_TargetProduct] FOREIGN KEY([TargetProductId])
REFERENCES [Enterprise].[Product] ([ProductId])
GO
