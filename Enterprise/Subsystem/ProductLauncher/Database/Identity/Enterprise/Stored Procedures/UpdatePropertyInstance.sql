CREATE PROCEDURE [Enterprise].[UpdatePropertyInstance] (  
 @InstanceId UNIQUEIDENTIFIER ,  
 @Name NVARCHAR(50),
 @Active TINYINT,
 @Address NVARCHAR(200),
 @City nvarchar(60),
 @State nvarchar(20),
 @PostalCode nvarchar(25),
 @Country nvarchar(25),
 @County nvarchar(60)
)  
AS  
BEGIN    
 BEGIN TRY  
  BEGIN TRANSACTION;  
  UPDATE  Enterprise.PropertyInstance 
  SET     Name = ISNULL(@Name, Name) ,
  IsActive = @Active,
  Address = ISNULL(@Address, Address),
  City = ISNULL(@City, City),
  State = ISNULL(@State, State),
  PostalCode = ISNULL(@PostalCode, PostalCode),
  Country = ISNULL(@Country, Country),
  County = ISNULL(@County, County)
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