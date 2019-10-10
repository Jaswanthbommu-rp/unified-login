CREATE TABLE [Enterprise].[Product]
(	
	[ProductId] INT NOT NULL, 
	[ProductGUID] UNIQUEIDENTIFIER NOT NULL DEFAULT NEWID(),
    [Name] NVARCHAR(50) NOT NULL
    CONSTRAINT [PK_Product] PRIMARY KEY ([ProductId]), 
    [Description] NVARCHAR(1000) NULL, 
    [ProductTypeId] INT NULL, 
    [BooksProductCode] NVARCHAR(20) NULL, 
    CONSTRAINT [FK_Product_ProductType] FOREIGN KEY ([ProductTypeId]) REFERENCES [Enterprise].[ProductType]([ProductTypeId])
)
