CREATE PROCEDURE [Batch].[UpdateEnterpriseRoleProductBatch]	
(@ProductBatchID BIGINT,  
 @StatusTypeId   INT
)  
AS  
     BEGIN
        BEGIN TRY  
             BEGIN TRAN;
                UPDATE [Batch].[EnterpriseRoleBatchProcess] SET StatusTypeId = @StatusTypeId
                WHERE EnterpriseRoleBatchProcessId = @ProductBatchID

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
