  
CREATE PROCEDURE [Enterprise].[SetProductType](          
  @ProductTypeId int OUTPUT          
 ,@ParentProductTypeId int       
 ,@Name varchar(50)          
 ,@Description varchar(1000)          
 ,@ProductTypeGUID uniqueidentifier        
 ,@PropertyManagementName Nvarchar(50)      
 )           
AS          
BEGIN          
 SET NOCOUNT ON;          
            
 BEGIN TRY          
  BEGIN TRAN;         
 --if product type doesn't exist add a new one      
  if (not exists(select top 1 1 from [Enterprise].[ProductType] where [ProductTypeId]=@ProductTypeId))      
  begin       
    
  SET @productTypeGUID = ISNULL(@productTypeGUID, NEWID())      
  if(@ParentProductTypeId is null)      
  begin      
   SELECT @ParentProductTypeId = ProductTypeId FROM Enterprise.ProductType WHERE Name = @PropertyManagementName AND ParentProductTypeId IS NULL;          
  end      
      
  if(@ProductTypeId is null)      
  begin      
   SET @ProductTypeId = (SELECT MAX(productTypeid) + 1 'ProductTypeId'  FROM [Enterprise].[ProductType]    
   WHERE ParentProductTypeId = @ParentProductTypeId)          
  end      
       
  INSERT INTO [Enterprise].[ProductType]      
  (        
   [ProductTypeId]        
   ,[ParentProductTypeId]        
   ,[Name]        
   ,[Description]        
   ,[ProductTypeGUID]        
  )        
  OUTPUT Inserted.ProductTypeId AS Id,        
  '' AS ErrorMessage          
  VALUES        
  (        
   @ProductTypeId          
   ,@ParentProductTypeId          
   ,@Name        
   ,@Description        
   ,@ProductTypeGUID        
  )        
  end      
  --if product exists update      
  else      
  begin      
   update  [Enterprise].[ProductType]      
    set       
  [ParentProductTypeId] = IsNUll(@ParentProductTypeId, ParentProductTypeId)    
  ,[Name]  =@Name      
  ,[Description] = IsNUll(@Description, Description)    
  ,ProductTypeGuid  = isnull(@productTypeGUID, ProductTypeGuid)    
    where [ProductTypeId]= @ProductTypeId    
  end      
  COMMIT;          
 END TRY          
    BEGIN CATCH        
        ROLLBACK;       
        DECLARE @ErrorLogID INT;        
        EXEC dbo.LogError @ErrorLogID = @ErrorLogID OUTPUT;        
        SELECT 0 AS Id, ErrorMessage        
        FROM dbo.ErrorLog        
        WHERE ErrorLogID = @ErrorLogID;        
    END CATCH;         
END; 