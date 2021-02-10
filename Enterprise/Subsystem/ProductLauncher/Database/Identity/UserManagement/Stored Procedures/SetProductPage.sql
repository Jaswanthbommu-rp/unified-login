CREATE PROCEDURE [UserManagement].[SetProductPage] (    
 @ProductPageId int output,  
 @ProductId int,  
 @DisplayName nvarchar(510),  
 @CreatedBy bigint,  
 @IsActive bit,  
 @ProductPageTypeId int  
)    
AS    
BEGIN    
 BEGIN TRY    
        BEGIN TRANSACTION;   
  if(@ProductPageId is null)  
  begin  
   insert into[UserManagement].ProductPage(  
    ProductId  
    ,DisplayName  
    ,CreatedBy  
    ,CreatedDate  
    ,IsActive  
    ,ProductPageTypeId)  
   values(  
    @ProductId  
    ,@DisplayName  
    ,@CreatedBy  
    ,getdate()  
    ,@IsActive  
    ,@ProductPageTypeId)  
   SET @ProductPageId =SCOPE_IDENTITY()  
  end  
  else  
  begin  
   update [UserManagement].ProductPage  
   set ProductId = @ProductId  
    ,DisplayName=@DisplayName  
    ,CreatedBy=@CreatedBy  
    ,CreatedDate=getdate()  
    ,IsActive = @IsActive  
    ,ProductPageTypeId = @ProductPageTypeId  
   where ProductPageId=@ProductPageId  
  end   
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