
CREATE PROCEDURE [Batch].[InsertBatchProcessorLog]
(    
	@BatchProcessorId BIGINT,
	@StartDatetime DATETIME2,
	@EndDatetime DATETIME2 
)     
AS    
BEGIN

 SET NOCOUNT ON      
        
 BEGIN TRY    
  
  IF @BatchProcessorId IS NULL   
  RAISERROR('NULL Value is passed for BatchProcessorId',15,1)  
  
  IF @StartDatetime IS NULL AND @EndDatetime IS NULL
  RAISERROR('NULL Value is passed for StartDatetime and EndDatetime',15,1)  
    
	-- @StartDatetime and @EndDatetime will come from API
  IF @StartDatetime IS NOT NULL AND @EndDatetime IS NULL
  BEGIN
      IF EXISTS (SELECT TOP 1 1 FROM Batch.BatchProcessorLog WHERE BatchProcessorId = @BatchProcessorId)
	  BEGIN
		  --When Batch Process reruns then it updates start and end datetimes  
		  UPDATE [Batch].[BatchProcessorLog]
		  SET StartDatetime = @StartDatetime,
		  EndDatetime = @EndDatetime
		  WHERE BatchProcessorId = @BatchProcessorId AND EndDatetime IS NULL
		  

	  END
	  ELSE
	  BEGIN
	      INSERT INTO [Batch].[BatchProcessorLog]      
		  (    
		   [BatchProcessorId],  
		   StartDatetime,  
		   EndDatetime  
		  )    
		  VALUES    
		  (    
		   @BatchProcessorId,  
		   @StartDatetime,  
		   @EndDatetime         
		  )
	  END
	       
  END
  ELSE IF @StartDatetime IS NULL AND @EndDatetime IS NOT NULL
  BEGIN
      UPDATE [Batch].[BatchProcessorLog]
	  SET EndDatetime = @EndDatetime
	  WHERE BatchProcessorId = @BatchProcessorId AND EndDatetime IS NULL
  END

 END TRY     
   
 BEGIN CATCH            
    
  DECLARE @ErrorLogID INT;    
  EXEC dbo.LogError @ErrorLogID = @ErrorLogID OUTPUT;    
    
  SELECT 0 AS Id,    
   ErrorMessage    
  FROM dbo.ErrorLog    
  WHERE ErrorLogID = @ErrorLogID;    
  
 END CATCH   
  
END;

