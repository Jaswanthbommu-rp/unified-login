CREATE Procedure [Enterprise].[SetProduct]                 
 @ProductId INT output,    -- ProductId                  
 @productGUID NVARCHAR(50) output,  -- null for new product                   
 @ProductTypeId INT, -- null for new product                  
 @ParentProductTypeId INT, -- null for new product                  
 @productTypeGUID uniqueidentifier, -- null for new product                 
 @ProductName NVARCHAR(100),    -- Product Name used in tiles (UL Responsibility)                  
 @ProductDescription NVARCHAR(2000),                
 @BooksProductCode NVARCHAR(10),   -- Books product code (Books Responsibility)             
 @AssignToAllUsers bit, -- Assign to all users          
 @UDMSourceCode nvarchar(20),          
 @LoginURI NVARCHAR(500),     --Product login page (Product Responsibility)
 @SigningCertificateThumbprint NVARCHAR(50),   -- for SAML products (UL Responsibility)                  
 @Active bit
  --@LoginURL NVARCHAR(500),    -- Same as @LoginURI                  
 --@SubjectIdSamlAttribute NVARCHAR(50), --Needed for saml auth (UL Responsibility)                     
                 
AS                      
BEGIN                    
 BEGIN TRY                    
  BEGIN TRAN;                  
                      
   DECLARE                       
    @ServerName SYSNAME = @@SERVERNAME           
                
   --Set the product type                
   EXEC [Enterprise].[SetProductType]  @ProductTypeId = @ProductTypeId OUTPUT, @ParentProductTypeId = @ParentProductTypeId,                   
     @Name = @ProductName, @Description = @ProductName, @ProductTypeGUID = @productTypeGUID, @PropertyManagementName = null                      
 
	if(isnull(@ProductId,0)<1) SELECT @ProductId = (SELECT MAX(ProductId)+1 from Enterprise.Product)
	
   --New product                
   if (not exists(select top 1 1 from Enterprise.Product where ProductId=@ProductId))                  
   begin             
          
    SELECT @productGUID = ISNULL(@productGUID,NEWID())                  
    INSERT INTO Enterprise.Product (                    
     ProductId ,                    
     ProductGUID ,                    
     Name ,                    
     Description ,                    
     ProductTypeId,                    
     BooksProductCode,          
  AssignToAllUsers,          
  UDMSourceCode,
  [Active]
    )                    
    OUTPUT Inserted.ProductId AS Id, '' AS ErrorMessage                    
    VALUES (                    
     @ProductId ,                    
     @productGUID,                    
     @ProductName ,                    
     @ProductDescription ,                    
     @ProductTypeID,                    
     @BooksProductCode,          
  @AssignToAllUsers,          
  @UDMSourceCode,
  @Active
    );                    
   end                  
   -- Update product                
   else                 
   begin                  
     UPDATE Enterprise.Product set                   
      Name =IsNull(@ProductName, Name),                    
      Description =@ProductDescription,                    
      ProductTypeId=@ProductTypeID,                    
      BooksProductCode=@BooksProductCode,          
      AssignToAllUsers=@AssignToAllUsers,              
      UDMSourceCode=@UDMSourceCode,
      [Active] = @Active
     WHERE ProductId=@ProductId                  
   end                  
                
   --SET SAML Info                
   IF (@LoginUri IS NOT NULL AND @SigningCertificateThumbprint IS NOT NULL)                  
   BEGIN                  
    IF NOT EXISTS  (SELECT 1                  
     FROM Ident.SamlProductSettings                  
     WHERE ProductId = @ProductId)                  
    BEGIN                   
     INSERT INTO Ident.SamlProductSettings (                  
      ProductId,                  
      LoginUri,                  
      SigningCertificateThumbprint,                  
      SubjectIdSamlAttribute)                  
     VALUES (                  
      @ProductId,        
      @LoginUri,                  
      @SigningCertificateThumbprint,                  
      'productUsername');                  
    END                  
    ELSE                
    BEGIN                  
     UPDATE Ident.SamlProductSettings                  
     SET LoginUri = @LoginUri,                  
      SigningCertificateThumbprint = @SigningCertificateThumbprint                  
     WHERE ProductId = @ProductId;                  
    END                  
   END;                  
            
        COMMIT;                   
                 
                
                
                
    END TRY                    
    BEGIN CATCH                    
        DECLARE @ErrorLogID INT;                    
  EXEC dbo.LogError                    
 @ErrorLogID = @ErrorLogID OUTPUT;                  
        SELECT 0 AS Id ,                    
                ErrorMessage                    
        FROM   dbo.ErrorLog                    
        WHERE  ErrorLogID = @ErrorLogID;                    
                    
        ROLLBACK;                    
    END CATCH;                    
END;
