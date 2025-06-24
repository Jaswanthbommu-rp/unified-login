

-- Batch.GetBulkUserBatchRecords 43564

CREATE PROCEDURE Batch.GetBulkUserBatchRecords (@BulkUserBatchProcessId INT)  
   AS  
   BEGIN  
  
   SELECT bu.BulkUserBatchProcessId,EditorUserPersonaId,SubjectUserPersonaId,StatusTypeId,BatchProcessTypeId ,bup.ProductId FROM  
   Batch.[BulkUserBatchProcess] bu  
   inner join [Security].[BulkUserProducts] bup on bu.bulkUserBatchProcessId = bup.bulkUserBatchProcessId  
   WHERE bu.bulkUserBatchProcessId = @BulkUserBatchProcessId  
  
   END
