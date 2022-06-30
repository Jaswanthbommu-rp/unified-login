CREATE PROCEDURE [Batch].[GetBatchProcessStatus]            
(@ProductBatchID BIGINT )    
AS             
BEGIN     

         SET NOCOUNT ON;    
    
  DECLARE @BatchComplete TINYINT = 0  
   DECLARE @groupID AS BIGINT, @subjectUserPersonId AS BIGINT, @editoruserId AS BIGINT

      SELECT @groupID = p.BatchProcessorGroupId,  
     @subjectUserPersonId = p.SubjectUserPersonaId,  
     @editoruserId = p.EditorUserPersonaId
    FROM Batch.BatchProcessor p WITH(UPDLOCK)  
     WHERE p.BatchProcessorId = @ProductBatchID;  

     SELECT @BatchComplete = CASE WHEN COUNT(1) > 0 THEN 0 ELSE 1 END  
     FROM Batch.BatchProcessor     
       WHERE BatchProcessorGroupId = @groupID  
        AND SubjectUserPersonaId = @subjectUserPersonId    
        AND EditorUserPersonaId = @editoruserId  
        AND ( (StatusTypeId <> 8)     
        AND (StatusTypeId <> 7 OR (StatusTypeId = 7 AND RetryCount < 3) ))
		
	   SELECT @BatchComplete  
END