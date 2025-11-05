CREATE PROCEDURE [Batch].[ListCompanyBatchData]  
(  
 @BatchSize INT,  
 @StatusTypeId INT  
)  
AS  
BEGIN  
    SET NOCOUNT ON;  
    IF @BatchSize = 0  
    BEGIN   
        SET @BatchSize = 1  
    END  
    -- Temporary table to hold selected IDs  
    DECLARE @SelectedIds TABLE (CompanyBatchJobId BIGINT);  
  
    -- Select records matching criteria and limit by batch size  
    INSERT INTO @SelectedIds (CompanyBatchJobId)  
    SELECT TOP (@BatchSize) CompanyBatchJobId  
    FROM [Batch].[CompanyPropertiesBatchProcess]  
    WHERE StatusTypeId = @StatusTypeId  
      AND CreatedDateTime <= DATEADD(MINUTE, 15, GETUTCDATE())  
    ORDER BY CreatedDateTime;  
  
    -- Update status to 6 for selected records  
    UPDATE [Batch].[CompanyPropertiesBatchProcess]  
    SET StatusTypeId = 6, LastRunDateTime = GETUTCDATE()
    WHERE CompanyBatchJobId IN (SELECT CompanyBatchJobId FROM @SelectedIds);  
  
    -- Return the updated records  
    SELECT CompanyBatchJobId, CONVERT(varchar(100), CompanyInstanceSourceId) AS CompanyInstanceSourceId, IsActive, StatusTypeId, CreateUserPersonaId, BatchProcessTypeId as BatchProcessorTypeId  
    FROM [Batch].[CompanyPropertiesBatchProcess]  
    WHERE CompanyBatchJobId IN (SELECT CompanyBatchJobId FROM @SelectedIds);  
END;  