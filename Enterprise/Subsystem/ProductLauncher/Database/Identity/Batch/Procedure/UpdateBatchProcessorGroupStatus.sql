Create Procedure [Batch].[UpdateBatchProcessorGroupStatus]  
	@groupId bigint,  
	@activiityLogged bit  
As  
Begin  
	Update Batch.BatchProcessorGroup  
	Set BatchProcessorGroupActivityLogged = @activiityLogged  
	where BatchProcessorGroupID = @groupId
End