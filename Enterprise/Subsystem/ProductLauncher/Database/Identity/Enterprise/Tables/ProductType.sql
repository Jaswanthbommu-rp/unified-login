CREATE TABLE [Enterprise].[ProductType]
(
	[ProductTypeId] INT NOT NULL, 
    [ParentProductTypeId] INT NULL, 
    [Name] VARCHAR(50) NOT NULL, 
    [Description] VARCHAR(1000) NULL, 
    [ProductTypeGUID] UNIQUEIDENTIFIER NOT NULL DEFAULT NEWID(), 
    CONSTRAINT [PK_ProductType] PRIMARY KEY ([ProductTypeId])	
)
