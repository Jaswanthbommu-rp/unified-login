CREATE PROCEDURE [Batch].[CreateProductBatch](    
 @PersonRealPageId UNIQUEIDENTIFIER     
 ,@CreateUserPersonaId bigint     
 ,@AssignUserPersonaId bigint    
 ,@ProductId int
 ,@BatchProcessorGroupId bigint
 ,@StatusTypeId int = 5 --waiting    
 ,@RetryCount tinyint = 0 -- no retries on a new    
 ,@InputJson nvarchar(max)    
 ,@LastRunDate DATETIME = NULL    
 ,@CreatedDate DATETIME = NULL  
 ,@ModifiedDate DATETIME = NULL    
 ,@ErrorDetails varchar(max) = NULL    
 ,@BatchProcessTypeId tinyint =1 --default create update user  
 ,@CorrelationId uniqueidentifier =NULL
 ,@ImpersonatorUserId BIGINT = 0
 ,@UseAPIV2 BIT = 0
 )     
AS    
BEGIN    
 SET NOCOUNT ON;    
  
 IF @CorrelationId IS NULL  
   SET @CorrelationId = NEWID()  
  
 DECLARE @NOW DATETIME = GETUTCDATE(),  
  @PersonPartyId bigint;    
     
 IF @CreatedDate IS NULL  
 BEGIN  
  SET @CreatedDate = @NOW;  
 END  
  
 IF @ModifiedDate IS NULL  
 BEGIN  
  SET @ModifiedDate = @NOW;  
 END  
    
 SELECT @PersonPartyId = PartyId     
 FROM Enterprise.Party    
 WHERE RealPageId = @PersonRealPageId;    
      
 BEGIN TRY      
  INSERT INTO [Batch].[BatchProcessor]    
  (  
            [CorrelationId]  
           ,[EditorUserPersonaId]  
           ,[SubjectUserPersonaId]  
		   ,[EditorUserPartyId]  
           ,[ProductId]  
           ,[StatusTypeId]  
           ,[BatchProcessTypeId]  
           ,[InputJSON]  
           ,[RetryCount]  
           ,[CreatedDateTime]  
           ,[LastRunDateTime]
		   ,[BatchProcessorGroupId]
           ,[ImpersonatorUserId]
           ,UseAPIV2
  )  
  OUTPUT Inserted.BatchProcessorId AS Id,  
    '' AS ErrorMessage    
  VALUES  
  (  
    @CorrelationId  
   ,@CreateUserPersonaId    
   ,@AssignUserPersonaId    
   ,@PersonPartyId  
   ,@ProductId    
   ,@StatusTypeId    
   ,@BatchProcessTypeId     
   ,@InputJson    
   ,@RetryCount    
   ,@CreatedDate    
   ,@ModifiedDate
   ,@BatchProcessorGroupId
   ,@ImpersonatorUserId
   ,@UseAPIV2
  )     
 END TRY    
    BEGIN CATCH          
  
        DECLARE @ErrorLogID INT;  
        EXEC dbo.LogError @ErrorLogID = @ErrorLogID OUTPUT;  
  
        SELECT 0 AS Id,  
    ErrorMessage  
        FROM dbo.ErrorLog  
        WHERE ErrorLogID = @ErrorLogID;  
    END CATCH;   
END;