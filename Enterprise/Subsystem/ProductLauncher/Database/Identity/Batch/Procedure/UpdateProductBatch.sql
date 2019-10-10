
CREATE PROCEDURE [Batch].[UpdateProductBatch]
(@ProductBatchID INT,
 @StatusTypeId   INT,
 @InputJson      NVARCHAR(MAX) = NULL,
 @ErrorDetails   VARCHAR(MAX)  = NULL
)
AS
     BEGIN
         SET NOCOUNT ON;
         DECLARE @NOW DATETIME= GETUTCDATE();
         DECLARE @RetryCount TINYINT;
         BEGIN TRY
             BEGIN TRAN;
             IF @StatusTypeId = 7 --Error
                 BEGIN
                     SELECT @RetryCount = RetryCount + 1
                     FROM BatchProcessor
                     WHERE BatchProcessorId = @ProductBatchID;
                 END;
                 ELSE
                 BEGIN
                     SELECT @RetryCount = RetryCount
                     FROM BatchProcessor
                     WHERE BatchProcessorId = @ProductBatchID;
                 END;
             UPDATE Batch.[BatchProcessor]
               SET
                   [StatusTypeId] = @StatusTypeId,
                   [RetryCount] = @RetryCount,
                   [InputJson] = COALESCE(@InputJson, [InputJson]),
                   [LastRunDateTime] = CASE
                                           WHEN @StatusTypeId = 6
                                           THEN @NOW
                                           ELSE [LastRunDateTime]
                                       END --Running

             WHERE BatchProcessorId = @ProductBatchID;

            --Insert record in error table

             INSERT INTO Batch.[BatchProcessorError]
				([BatchProcessorId],
				 [Error]
				)
			 VALUES
				(@ProductBatchID,
				 @ErrorDetails
				);
             SELECT @ProductBatchID AS Id,
                    '' AS ErrorMessage;
             COMMIT;
         END TRY
         BEGIN CATCH
             DECLARE @ErrorLogID INT;
             EXEC dbo.LogError
                  @ErrorLogID = @ErrorLogID OUTPUT;
             SELECT 0 AS Id,
                    ErrorMessage
             FROM dbo.ErrorLog
             WHERE ErrorLogID = @ErrorLogID;
             ROLLBACK;
         END CATCH;
     END;