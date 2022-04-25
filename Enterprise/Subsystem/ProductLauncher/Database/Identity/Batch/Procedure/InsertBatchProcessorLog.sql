
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

		IF @StartDatetime IS NULL 
		RAISERROR('NULL Value is passed for StartDatetime',15,1)
		
		IF @EndDatetime IS NULL 
		RAISERROR('NULL Value is passed for EndDatetime',15,1)

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

