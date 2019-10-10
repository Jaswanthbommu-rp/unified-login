IF OBJECT_ID('[Enterprise].[CreateProductBatch]') IS NOT NULL
	DROP PROCEDURE [Enterprise].[CreateProductBatch];

GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_NULLS ON
GO
CREATE PROCEDURE [Enterprise].[CreateProductBatch](  
	@PersonRealPageId UNIQUEIDENTIFIER   
	,@CreateUserPersonaId bigint   
	,@AssignUserPersonaId bigint  
	,@ProductId int  
	,@StatusTypeId int = 5 --waiting  
	,@RetryCount tinyint = 0 -- no retries on a new  
	,@InputJson nvarchar(max)  
	,@LastRunDate smalldatetime = NULL  
	,@CreatedDate smalldatetime = NULL
	,@ModifiedDate smalldatetime = NULL  
	,@ErrorDetails varchar(max) = NULL  
 )   
AS  
BEGIN  
	SET NOCOUNT ON;  

	DECLARE @NOW smalldatetime = GETUTCDATE(),
		@PersonPartyId bigint;  
   
	IF @CreatedDate IS NULL
	BEGIN
		SET @CreatedDate = @NOW;
	END

	IF @ModifiedDate IS NULL
	BEGIN
		SET @ModifiedDate = @NOW;
	END
  
	SELECT	@PersonPartyId = PartyId   
	FROM	Enterprise.Party  
	WHERE	RealPageId = @PersonRealPageId;  
	   
	BEGIN TRY  
		BEGIN TRAN;  
		INSERT INTO [Enterprise].[ProductBatch]  
		(
			[PersonPartyId]  
			,[CreateUserPersonaId]  
			,[AssignUserPersonaId]  
			,[ProductId]  
			,[StatusTypeId]  
			,[RetryCount]  
			,[InputJson]  
			,[LastRunDate]  
			,[CreatedDate]  
			,[ModifiedDate]  
			,[ErrorDetails]
		)
		OUTPUT	Inserted.ProductBatchId AS Id,
				'' AS ErrorMessage  
		VALUES
		(
			@PersonPartyId  
			,@CreateUserPersonaId  
			,@AssignUserPersonaId  
			,@ProductId  
			,@StatusTypeId  
			,@RetryCount  
			,@InputJson  
			,@LastRunDate  
			,@CreatedDate  
			,@ModifiedDate  
			,@ErrorDetails
		)
		COMMIT;  
	END TRY  
    BEGIN CATCH
        ROLLBACK;

        DECLARE @ErrorLogID INT;
        EXEC dbo.LogError @ErrorLogID = @ErrorLogID OUTPUT;

        SELECT	0 AS Id,
				ErrorMessage
        FROM	dbo.ErrorLog
        WHERE	ErrorLogID = @ErrorLogID;
    END CATCH; 
END;
GO
