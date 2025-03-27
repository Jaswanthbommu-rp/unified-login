CREATE TABLE [Enterprise].[OmnibarProductCodes](
	[Id] [INT] IDENTITY(1,1) NOT NULL,
	[ProductId] [INT] NOT NULL,
	[OmnibarProductCode] [VARCHAR](100) NOT NULL,
	[Description] [VARCHAR](100) NOT NULL,
	[IsActive] [BIT] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [Enterprise].[OmnibarProductCodes] ADD  DEFAULT ((1)) FOR [IsActive]
GO

ALTER TABLE [Enterprise].[OmnibarProductCodes]  WITH CHECK ADD  CONSTRAINT [FK_OmnibarProductCodes_ProductId] FOREIGN KEY([ProductId])
REFERENCES [Enterprise].[Product] ([ProductId])
ON UPDATE CASCADE
ON DELETE CASCADE
GO

ALTER TABLE [Enterprise].[OmnibarProductCodes] CHECK CONSTRAINT [FK_OmnibarProductCodes_ProductId]
GO