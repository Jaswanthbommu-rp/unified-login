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

             DECLARE @groupID AS BIGINT, @subjectUserPersonId AS BIGINT, @editoruserId AS BIGINT;  
             SELECT @groupID = p.BatchProcessorGroupId, @subjectUserPersonId = p.SubjectUserPersonaId, @editoruserId = p.EditorUserPersonaId  
             FROM Batch.BatchProcessor p WITH(UPDLOCK)
             WHERE p.BatchProcessorId = @ProductBatchID;

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
             INSERT INTO Batch.[BatchProcessorError] ([BatchProcessorId], [Error]) 
				VALUES (@ProductBatchID,@ErrorDetails);  
  
             SELECT @groupID = p.BatchProcessorGroupId, @subjectUserPersonId = p.SubjectUserPersonaId, @editoruserId = p.EditorUserPersonaId  
             FROM Batch.BatchProcessor p  
             Where p.BatchProcessorId = @ProductBatchID;  
  
             SELECT   
				CAST(CASE WHEN COUNT(*) > 0 THEN 0 ELSE 1 END AS BIT)
             FROM Batch.BatchProcessor   
             WHERE BatchProcessorGroupId = @groupID
             AND SubjectUserPersonaId = @subjectUserPersonId  
			 AND EditorUserPersonaId = @editoruserId
			 AND ( (StatusTypeId <> 8)   
			 AND (StatusTypeId <> 7 OR (StatusTypeId = 7 AND RetryCount < 3)))  
  
            COMMIT;   
         END TRY  
         BEGIN CATCH  
             ROLLBACK;  
             DECLARE @ErrorLogID INT;  
             EXEC dbo.LogError  
                  @ErrorLogID = @ErrorLogID OUTPUT;  
             
         END CATCH;  
     END;