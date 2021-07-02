CREATE PROCEDURE [Enterprise].[CreateProductBatch]  
(  
    @PersonRealPageId UNIQUEIDENTIFIER,  
    @CreateUserPersonaId BIGINT,  
    @AssignUserPersonaId BIGINT,  
    @ProductId INT,
	@BatchProcessorGroupId BIGINT,  
    @StatusTypeId INT = 5,           --waiting    
    @RetryCount TINYINT = 0,         -- no retries on a new    
    @InputJson NVARCHAR(MAX),  
    @LastRunDate SMALLDATETIME = NULL,  
    @CreatedDate SMALLDATETIME = NULL,  
    @ModifiedDate SMALLDATETIME = NULL,  
    @ErrorDetails VARCHAR(MAX) = NULL,  
    @BatchProcessTypeId TINYINT = 1, --default create update user  
    @CorrelationId UNIQUEIDENTIFIER = NULL
)  
AS  
BEGIN  
    SET NOCOUNT ON;  
  
    DECLARE @NOW SMALLDATETIME = GETUTCDATE(),  
            @PersonPartyId BIGINT;  
  
    IF @CreatedDate IS NULL  
    BEGIN  
        SET @CreatedDate = @NOW;  
    END;  
  
    IF @ModifiedDate IS NULL  
    BEGIN  
        SET @ModifiedDate = @NOW;  
    END;  
  
     
    IF  
    (  
        SELECT ControlValue  
        FROM Enterprise.GlobalControl  
        WHERE ControlName = 'IsNewBatchService'  
    ) = 0  
    BEGIN  
        BEGIN TRY             
            SELECT @PersonPartyId = PartyId  
   FROM Enterprise.Party  
   WHERE RealPageId = @PersonRealPageId;  
  
      INSERT INTO [Enterprise].[ProductBatch]  
            (  
                [PersonPartyId],  
                [CreateUserPersonaId],  
                [AssignUserPersonaId],  
                [ProductId],  
                [StatusTypeId],  
                [RetryCount],  
                [InputJson],  
                [LastRunDate],  
                [CreatedDate],  
                [ModifiedDate],  
                [ErrorDetails]  
            )  
            OUTPUT Inserted.ProductBatchId AS Id,  
                   '' AS ErrorMessage  
            VALUES  
            (@PersonPartyId, @CreateUserPersonaId, @AssignUserPersonaId, @ProductId, @StatusTypeId, @RetryCount,  
             @InputJson, @LastRunDate, @CreatedDate, @ModifiedDate, @ErrorDetails);              
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
    ELSE  
    BEGIN  
        EXEC [Batch].[CreateProductBatch] @PersonRealPageId = @PersonRealPageId,  
                                          @CreateUserPersonaId = @CreateUserPersonaId,  
                                          @AssignUserPersonaId = @AssignUserPersonaId,  
                                          @ProductId = @ProductId,
										  @BatchProcessorGroupId = @BatchProcessorGroupId, 
                                          @StatusTypeId = @StatusTypeId,  
                                          @RetryCount = @RetryCount,  
                                          @InputJson = @InputJson,  
                                          @LastRunDate = @LastRunDate,  
                                          @CreatedDate = @CreatedDate,  
                                          @ModifiedDate = @ModifiedDate,  
                                          @ErrorDetails = @ErrorDetails,  
                                          @BatchProcessTypeId = @BatchProcessTypeId,  
                                          @CorrelationId = @CorrelationId;  
  
    END;  
END;