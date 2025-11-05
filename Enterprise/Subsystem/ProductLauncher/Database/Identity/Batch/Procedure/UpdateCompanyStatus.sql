CREATE PROCEDURE Batch.UpdateCompanyStatus(
 @CompanyBatchJobId bigint,        
 @StatusTypeId INT,
 @ErrorMessage varchar(MAX)
)    
AS     
BEGIN         
	SET NOCOUNT ON;          

	UPDATE [Batch].[CompanyPropertiesBatchProcess]         
	SET StatusTypeId = @StatusTypeId    
	, ErrorMessage = @ErrorMessage
	WHERE companyBatchJobId = @CompanyBatchJobId;     

END;