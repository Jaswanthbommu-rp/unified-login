CREATE PROCEDURE [Batch].[DeleteProductActivityLog](  
@BatchProcessorGroupId bigint)  
AS  
BEGIN  
 DELETE from batch.ProductActivityLog  
 where BatchProcessorGroupId = @BatchProcessorGroupId  
END
