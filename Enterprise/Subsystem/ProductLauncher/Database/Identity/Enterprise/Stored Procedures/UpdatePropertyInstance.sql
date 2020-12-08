CREATE PROCEDURE [Enterprise].[UpdatePropertyInstance] (  
 @InstanceId UNIQUEIDENTIFIER ,  
 @Name NVARCHAR(50)
)  
AS  
BEGIN    
 BEGIN TRY  
  BEGIN TRANSACTION;  
  UPDATE  Enterprise.PropertyInstance 
  SET     Name = ISNULL(@Name, Name)    
  OUTPUT inserted.PropertyInstanceId AS Id,  
    '' AS ErrorMessage  
  FROM    Enterprise.PropertyInstance 
  WHERE   InstanceId = @InstanceId;  
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