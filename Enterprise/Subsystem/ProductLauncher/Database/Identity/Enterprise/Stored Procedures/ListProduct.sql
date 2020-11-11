CREATE PROCEDURE Enterprise.ListProduct @ProductId        [INT]            = NULL,
                                       @ProductGUID      UNIQUEIDENTIFIER = NULL,
                                       @Name             NVARCHAR(50)     = NULL,
                                       @BooksProductCode NVARCHAR(20)     = NULL
AS
     BEGIN
         SELECT ProductId,
                ProductGUID,
                Name,
                Description,
                ProductTypeId,
                BooksProductCode,
                UDMSourceCode
         FROM Enterprise.Product
         WHERE (ProductId = @ProductId
               OR @ProductId IS NULL)
              AND (ProductGUID = @ProductGUID
                   OR @ProductGUID IS NULL)
              AND (Name = @Name
                   OR @Name IS NULL)
              AND (BooksProductCode = @BooksProductCode
                   OR @BooksProductCode IS NULL);
     END;
GO
