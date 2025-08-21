

CREATE PROCEDURE [Enterprise].[UpdateUPFMPropertyInstances] (    
@InstanceList [Enterprise].[PropertyInstanceType] READONLY ,  
@Active TINYINT 
)    

AS    
BEGIN      
 BEGIN TRY    
  BEGIN TRANSACTION;    
  UPDATE  PII  SET PII.IsActive = @Active  OUTPUT inserted.PropertyInstanceId AS Id,    
    '' AS ErrorMessage 
      from Enterprise.PropertyInstance PII inner join @InstanceList PIT on PIT.InstanceId = PII.InstanceId

  COMMIT;    
 END TRY      
 BEGIN CATCH    
        ROLLBACK;    
    
        DECLARE @ErrorLogID INT;    
        EXEC dbo.LogError @ErrorLogID = @ErrorLogID OUTPUT;    
    
        SELECT  0 AS Id,    
    ErrorMessage    
        FROM    dbo.ErrorLog    
        WHERE   ErrorLogID = @ErrorLogID;    
 END CATCH    
END;