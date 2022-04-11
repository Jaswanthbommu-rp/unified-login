Create PROCEDURE [Batch].[UpdatePrimaryPropertiesProductBatch]	
(@ProductBatchID BIGINT,  
 @StatusTypeId   INT
)  
AS  
     BEGIN
        BEGIN TRY  
             BEGIN TRAN;
                UPDATE [Batch].[PrimaryPropertiesBatchProcess] SET StatusTypeId = @StatusTypeId,
                                                                   CompletedDateTime = CASE WHEN @StatusTypeId = 8 THEN GETUTCDATE() ELSE NULL END
                WHERE PrimaryPropertyBatchProcessId = @ProductBatchID

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
