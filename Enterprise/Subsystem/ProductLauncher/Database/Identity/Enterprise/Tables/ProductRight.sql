CREATE TABLE [Enterprise].[ProductRight]
(
	[Id] INT NOT NULL PRIMARY KEY IDENTITY(1,1), 
    [ProductId] INT NOT NULL, 
    [RightShortName] NCHAR(100) NOT NULL, 
    [DependantProductId] INT NULL, 
    CONSTRAINT [FK_ProductRight_ProductId] FOREIGN KEY ([ProductId]) REFERENCES [Enterprise].[Product] ([ProductId]), 

)
