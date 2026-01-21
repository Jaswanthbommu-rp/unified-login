CREATE PROCEDURE [Batch].[UpdateProductBatch]  
(@ProductBatchID BIGINT,  
 @StatusTypeId   INT,  
 @InputJson      NVARCHAR(MAX) = NULL,  
 @ErrorDetails   VARCHAR(MAX)  = NULL
)  
AS  
     BEGIN  
         SET NOCOUNT ON;  
         DECLARE @NOW DATETIME= GETUTCDATE();  
         DECLARE @RetryCount TINYINT;
		 DECLARE @BatchComplete TINYINT = 0
		 DECLARE @groupID AS BIGINT, @subjectUserPersonId AS BIGINT, @editoruserId AS BIGINT;
		 DECLARE @BatchStatusCheckCount TINYINT = 0
		 DECLARE @DelayLength CHAR(10)

         BEGIN TRY  
             BEGIN TRAN;  
             SELECT @groupID = p.BatchProcessorGroupId,
					@subjectUserPersonId = p.SubjectUserPersonaId,
					@editoruserId = p.EditorUserPersonaId,
					@RetryCount = P.RetryCount
				FROM Batch.BatchProcessor p WITH(UPDLOCK)
					WHERE p.BatchProcessorId = @ProductBatchID;

             IF @StatusTypeId = 7 --Error  
                 BEGIN  
                     SET @RetryCount = @RetryCount + 1  
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
  
			 IF @StatusTypeId NOT IN (5,6)
			 BEGIN
				 WHILE (@BatchStatusCheckCount < 5)
				 BEGIN
					SELECT @BatchComplete = CASE WHEN COUNT(*) > 0 THEN 0 ELSE 1 END
						FROM Batch.BatchProcessor   
							WHERE BatchProcessorGroupId = @groupID
								AND SubjectUserPersonaId = @subjectUserPersonId  
								AND EditorUserPersonaId = @editoruserId
								AND ( (StatusTypeId <> 8)   
								AND (StatusTypeId <> 7 OR (StatusTypeId = 7 AND RetryCount < 3)))  
					IF @BatchComplete = 1 OR @BatchStatusCheckCount > 4
						BREAK

					SET @BatchStatusCheckCount = @BatchStatusCheckCount + 1
					SET @DelayLength = '00:00:00:20' 
                    WAITFOR DELAY @DelayLength
				END
			END
            
			--Insert record in error table
            INSERT INTO Batch.[BatchProcessorError] ([BatchProcessorId], [Error]) 
				VALUES (@ProductBatchID,@ErrorDetails);  

            COMMIT;
			SELECT @BatchComplete
         END TRY  
         BEGIN CATCH  
             ROLLBACK;  
             DECLARE @ErrorLogID INT;  
             EXEC dbo.LogError  
                  @ErrorLogID = @ErrorLogID OUTPUT;  
             SELECT 0
         END CATCH;  
     END;
