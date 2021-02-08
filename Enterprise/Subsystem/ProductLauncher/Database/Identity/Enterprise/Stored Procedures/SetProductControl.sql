CREATE PROCEDURE [UserManagement].[SetProductControl] (    
  @ControlId INT    
 ,@ParentControlId INT    
 ,@ProductPageId int  
 ,@ControlTypeId INT    
 ,@UIId NVARCHAR(510)    
 ,@DisplayName NVARCHAR(510)    
 ,@DataSource NTEXT    
 ,@Sequence TINYINT    
 ,@CreatedBy BIGINT    
)     
AS     
begin  
 BEGIN TRY    
        BEGIN TRANSACTION;   
   IF (@ControlId is not null)  
   BEGIN    
     UPDATE [UserManagement].[Control] SET     
     [ParentControlId] = @ParentControlId    
     ,[ControlTypeId] = @ControlTypeId    
     ,[UIId] = @UIId    
     ,[DisplayName] = @DisplayName    
     ,[DataSource] = @DataSource    
     ,[Sequence] = @Sequence    
     ,[CreatedBy] = @CreatedBy    
     ,[CreatedDate] = getdate()    
    WHERE    
     [ControlId] = @ControlId   
   END    
   ELSE    
   BEGIN    
    --if this is a new item make sure that if it has no parent that we dont have a record in [UserManagement].ProductPageControl so we don't have orphen records  
    if(ISNULL(@ParentControlId,-1)<0  
     or (ISNULL(@ParentControlId,-1)<1 and   
      ISNULL(@ProductPageId,-1)>0 and  
      (not exists(select top 1 1 from [UserManagement].ProductPageControl where ProductPageId=@ProductPageId))))  
    begin       
     INSERT INTO [UserManagement].[Control] (    
      [ParentControlId]    
      ,[ControlTypeId]    
      ,[UIId]    
      ,[DisplayName]    
      ,[DataSource]    
      ,[Sequence]    
      ,[CreatedBy]    
      ,[CreatedDate])    
     VALUES(    
      @ParentControlId    
      ,@ControlTypeId    
      ,@UIId    
      ,@DisplayName    
      ,@DataSource    
      ,@Sequence    
      ,@CreatedBy    
      ,getdate());    
     SET @ControlId = SCOPE_IDENTITY()  
  
     --add the control to the page if this is a root control  
     --if there is no parent control then this is a root item that is under the page  
     if(ISNULL(@ParentControlId,-1)<1)  
     begin  
      insert into [UserManagement].ProductPageControl(ProductPageId  
       ,ControlId  
       ,CreatedBy  
       ,CreatedDate)  
      values(@ProductPageId  
       ,@ControlId  
       ,@CreatedBy  
       ,getdate())  
     end  
    end  
   END;  
   
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