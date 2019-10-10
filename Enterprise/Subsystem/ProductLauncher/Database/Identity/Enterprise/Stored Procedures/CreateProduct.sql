CREATE PROCEDURE [Enterprise].[CreateProduct]
    @ProductId INT ,
    @ProductGUID UNIQUEIDENTIFIER = NULL ,
    @Name NVARCHAR(50) ,
    @Description NVARCHAR(1000) ,
    @ProductTypeID INT = NULL,
	@BooksProductCode NVARCHAR(10) = NULL
AS
    BEGIN
        BEGIN TRY
            BEGIN TRAN;
            INSERT INTO Enterprise.Product (   ProductId ,
                                               ProductGUID ,
                                               Name ,
                                               Description ,
                                               ProductTypeId
                                           )
			OUTPUT Inserted.ProductId AS Id, '' AS ErrorMessage
            VALUES ( @ProductId ,
                     ISNULL(@ProductGUID, NEWID()),
                     @Name ,
                     @Description ,
                     @ProductTypeID
                   );
            COMMIT;
        END TRY
        BEGIN CATCH
            DECLARE @ErrorLogID INT;
            EXEC dbo.LogError @ErrorLogID = @ErrorLogID OUTPUT;

            SELECT 0 AS Id ,
                   ErrorMessage
            FROM   dbo.ErrorLog
            WHERE  ErrorLogID = @ErrorLogID;

            ROLLBACK;
        END CATCH;
    END;