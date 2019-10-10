CREATE PROCEDURE [Enterprise].[UpdateProduct](
	 @ProductID int
	,@ProductGUID uniqueidentifier = null
	,@Name nvarchar(100) = null
	,@Description nvarchar(1000) = null
	,@ProductTypeId int = null
	) 

AS


BEGIN

	SET NOCOUNT ON;
	
	 BEGIN TRY
            BEGIN TRAN;
				UPDATE [Enterprise].[Product]
				   SET 
					   [ProductGUID] = @ProductGUID
					  ,[Name] = @Name
					  ,[Description] = @Description
					  ,[ProductTypeId] = @ProductTypeId
					WHERE
						ProductId = @ProductId

				SELECT @ProductID as Id, '' AS ErrorMessage
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