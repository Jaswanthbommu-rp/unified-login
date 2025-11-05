CREATE PROCEDURE Batch.InsertBatchCompanyJob        
    @CompanyInstanceSourceId UNIQUEIDENTIFIER,          
    @IsActive bit,  
    @StatusTypeId INT,          
    @CreatedBy BIGINT = NULL,      
    @CreateUserPersonaId BIGINT = NULL,    
    @BatchProcessTypeId INT    
AS          
BEGIN          
    SET NOCOUNT ON;          
    Declare @CreatedDateTime DateTime      
    SET @CreatedDateTime = GETUTCDATE();          
          
    IF @CompanyInstanceSourceId IS NOT NULL       
    BEGIN          
        -- Insert new record          
        INSERT INTO [Batch].[CompanyPropertiesBatchProcess] (          
            CompanyInstanceSourceId,   
            IsActive,  
            StatusTypeId,      
            CreateUserPersonaId,      
            BatchProcessTypeId,    
            CreatedDateTime,          
            CreatedBy          
        )          
        VALUES (          
            @CompanyInstanceSourceId,   
            @IsActive,  
            @StatusTypeId,         
            @CreateUserPersonaId,      
            @BatchProcessTypeId,    
            @CreatedDateTime,          
            @CreatedBy          
        );          
          
        -- Optionally return the new ID          
        SELECT SCOPE_IDENTITY() AS NewCompanyJobId;          
    END          
             
END;