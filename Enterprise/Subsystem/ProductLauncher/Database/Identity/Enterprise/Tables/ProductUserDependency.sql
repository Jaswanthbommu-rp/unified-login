CREATE TABLE [Enterprise].[ProductUserDependency](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[ProductId] [int] NOT NULL,
	[DependentProductId] [int] NOT NULL,
CONSTRAINT [PK_ProductUserDependency_Id]  PRIMARY KEY CLUSTERED 
(
	[Id] ASC
),
 CONSTRAINT [FK_ProductUserDependency_DependentProductId] FOREIGN KEY([DependentProductId]) REFERENCES [Enterprise].[Product] ([ProductId]),
 CONSTRAINT [FK_ProductUserDependency_ProductId] FOREIGN KEY([ProductId]) REFERENCES [Enterprise].[Product] ([ProductId]),
) ON [PRIMARY]
