CREATE PROCEDURE [Enterprise].[UpdateProductBatch]
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
         IF
		(
			SELECT ControlValue
			FROM Enterprise.GlobalControl
			WHERE ControlName = 'IsNewBatchService'
		) = 0
             BEGIN
                 IF @StatusTypeId = 7 --Error
                     BEGIN
                         SELECT @RetryCount = RetryCount + 1
                         FROM ProductBatch
                         WHERE ProductBatchId = @ProductBatchID;
                     END;
                     ELSE
                     BEGIN
                         SELECT @RetryCount = RetryCount
                         FROM ProductBatch
                         WHERE ProductBatchId = @ProductBatchID;
                     END;
                 BEGIN TRY
                     BEGIN TRAN;
                     UPDATE [Enterprise].[ProductBatch]
                       SET
                           [StatusTypeId] = @StatusTypeId,
                           [RetryCount] = @RetryCount,
                           [InputJson] = COALESCE(@InputJson, [InputJson]),
                           [LastRunDate] = CASE
                                               WHEN @StatusTypeId = 6
                                               THEN @NOW
                                               ELSE [LastRunDate]
                                           END
                           ,--Running 
                           [ModifiedDate] = @NOW,
                           [ErrorDetails] = COALESCE(@ErrorDetails, [ErrorDetails])
                     WHERE ProductBatchId = @ProductBatchID;
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
             ELSE
             BEGIN
                 EXECUTE [Batch].[UpdateProductBatch]
                         @ProductBatchID = @ProductBatchID,
                         @StatusTypeId = @StatusTypeId,
                         @InputJson = @InputJson,
                         @ErrorDetails = @ErrorDetails;
             END;
     END;