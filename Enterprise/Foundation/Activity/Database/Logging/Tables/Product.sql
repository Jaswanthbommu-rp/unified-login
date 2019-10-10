CREATE TABLE [Logging].[Product] (
    [ProductID]        INT           IDENTITY (1, 1) NOT NULL,
    [BooksProductCode] NVARCHAR (20) NULL,
    CONSTRAINT [PK_Product] PRIMARY KEY CLUSTERED ([ProductID] ASC)
);








GO
CREATE NONCLUSTERED INDEX [IX_Product]
    ON [Logging].[Product]([BooksProductCode] ASC);



