
CREATE PROCEDURE [Batch].[UpdateBulkUserProductBatch]   
(@ProductBatchID BIGINT,    
 @StatusTypeId   INT  
)    
AS    
     BEGIN  
        BEGIN TRY    
             BEGIN TRAN;  
                UPDATE [Batch].[BulkUserBatchProcess] SET StatusTypeId = @StatusTypeId,  
                                                                   CompletedDateTime = CASE WHEN @StatusTypeId = 8 THEN GETUTCDATE() ELSE NULL END  
                WHERE BulkUserBatchProcessId = @ProductBatchID  
  
                Select 1 AS BIT;  
             COMMIT;     
         END TRY    
         BEGIN CATCH    
             DECLARE @ErrorLogID INT;    
             EXEC dbo.LogError    
                  @ErrorLogID = @ErrorLogID OUTPUT;    
             ROLLBACK;    
         END CATCH;    
     END;