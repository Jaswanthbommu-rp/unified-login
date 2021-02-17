CREATE TYPE [UserManagement].[ProductPageType] AS TABLE(
	[ProductPageId] [int] NULL,
	[ProductId] [int] NULL,
	[DisplayName] [nvarchar](max) NULL,
	[IsActive] [bit] NULL,
	[ProductPageTypeId] [int] NULL
)
GO


