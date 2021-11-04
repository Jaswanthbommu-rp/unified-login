CREATE TABLE [Security].[ADGroupProduct](
	[ADGroupProductId] [int] IDENTITY(1,1) NOT NULL,
	[ADGroupId] [int] NOT NULL,
	[ProductId] [int] NOT NULL,
	[AssignmentOrder] TINYINT NOT NULL CONSTRAINT [DF_ADGroupProduct_AssignmentOrder] DEFAULT (1),
	[CreatedBy] [nvarchar](25) NOT NULL,
	[CreatedDate] [datetime] NOT NULL
 CONSTRAINT [PK_ADGroupProduct] PRIMARY KEY CLUSTERED 
(
	[ADGroupProductId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [Security].[ADGroupProduct] ADD  CONSTRAINT [DF_ADGroupProduct_CreatedDate]  DEFAULT (getdate()) FOR [CreatedDate]
GO

ALTER TABLE [Security].[ADGroupProduct]  WITH CHECK ADD  CONSTRAINT [FK_ADGroupProduct_ADGroup] FOREIGN KEY([ADGroupId])
REFERENCES [Security].[ADGroup] ([ADGroupId])
GO

ALTER TABLE [Security].[ADGroupProduct] CHECK CONSTRAINT [FK_ADGroupProduct_ADGroup]
GO

ALTER TABLE [Security].[ADGroupProduct]  WITH CHECK ADD  CONSTRAINT [FK_ADGroupProduct_Product] FOREIGN KEY([ProductId])
REFERENCES [Enterprise].[Product] ([ProductId])
GO

ALTER TABLE [Security].[ADGroupProduct] CHECK CONSTRAINT [FK_ADGroupProduct_Product]
GO
