CREATE PROCEDURE [Enterprise].[InsertProductLoginActivitybyUser]
(    
	@ProductId INT,
	@PersonaId BIGINT,
	@ImpersonatorUserId BIGINT
)     
AS    
BEGIN

 SET NOCOUNT ON      
 BEGIN TRY    
   
	  INSERT INTO [Enterprise].[ProductLoginActivitybyUser](ProductId,PersonaId,ImpersonatorUserId,CreateDate)    
	  VALUES (@ProductId,@PersonaId,@ImpersonatorUserId,GETUTCDATE())

 END TRY     
 BEGIN CATCH            
    
  DECLARE @ErrorLogID INT;    
  EXEC dbo.LogError @ErrorLogID = @ErrorLogID OUTPUT;    
    
  SELECT 0 AS Id, ErrorMessage  FROM dbo.ErrorLog  WHERE ErrorLogID = @ErrorLogID;    
  
 END CATCH   
  
END;