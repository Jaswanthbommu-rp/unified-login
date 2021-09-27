Create Procedure [Batch].[GetUserBatchRecords]            
 @batchProcessorGroupId bigint,           
 @editorUserPersonId bigint,         
 @subjectUserPersonId bigint      
As            
Begin            
 select bp.StatusTypeId, p.Name, bg.BatchProcessorGroupActivityLogged, bp.InputJSON          
 from Batch.BatchProcessor bp            
 join Enterprise.Product p on p.ProductId = bp.ProductId     
 join Batch.BatchProcessorGroup bg on bg.BatchProcessorGroupId = bp.BatchProcessorGroupId                   
 where bp.BatchProcessorGroupId = @batchProcessorGroupId    
 and SubjectUserPersonaId = @subjectUserPersonId        
 and EditorUserPersonaId = @editorUserPersonId
 and bp.ProductId <> 42 -- ignoring 42, salesforce, since its a dummy product
End 