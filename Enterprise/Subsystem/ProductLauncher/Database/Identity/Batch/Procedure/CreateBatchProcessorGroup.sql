CREATE PROCEDURE [Batch].[CreateBatchProcessorGroup]  
 @BatchProcessorGroupID BIGINT OUTPUT  
AS  
BEGIN  
  BEGIN TRY
        BEGIN TRANSACTION;
		INSERT INTO Batch.BatchProcessorGroup(BatchProcessorGroupActivityLogged, CreateDateTime)
		OUTPUT	
			Inserted.BatchProcessorGroupID
		VALUES (0, GETUTCDATE());

		SET @BatchProcessorGroupID = SCOPE_IDENTITY();
		COMMIT;
	END TRY  
	BEGIN CATCH
        ROLLBACK;

        DECLARE @ErrorLogID INT;
        EXEC dbo.LogError @ErrorLogID = @ErrorLogID OUTPUT;

        SELECT  0 AS Id,
				ErrorMessage
        FROM    dbo.ErrorLog
        WHERE   ErrorLogID = @ErrorLogID;
	END CATCH  
END
