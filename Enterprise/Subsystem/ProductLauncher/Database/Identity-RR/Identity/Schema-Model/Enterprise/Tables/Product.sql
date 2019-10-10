CREATE TABLE [Enterprise].[Product]
(
[ProductId] [int] NOT NULL,
[ProductGUID] [uniqueidentifier] NOT NULL CONSTRAINT [DF__Product__Product__0880433F] DEFAULT (newid()),
[Name] [nvarchar] (50) NOT NULL,
[Description] [nvarchar] (100) NULL,
[ProductTypeId] [int] NULL
)
GO
ALTER TABLE [Enterprise].[Product] ADD CONSTRAINT [PK_Product] PRIMARY KEY CLUSTERED  ([ProductId])
GO
ALTER TABLE [Enterprise].[Product] ADD CONSTRAINT [FK_Product_ProductType] FOREIGN KEY ([ProductTypeId]) REFERENCES [Enterprise].[ProductType] ([ProductTypeId])
GO
