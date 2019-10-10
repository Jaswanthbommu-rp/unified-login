IF OBJECT_ID('[Enterprise].[UpdateProductBatch]') IS NOT NULL
	DROP PROCEDURE [Enterprise].[UpdateProductBatch];

GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_NULLS ON
GO
CREATE PROCEDURE [Enterprise].[UpdateProductBatch](
	 @ProductBatchID int
	,@StatusTypeId int
	,@InputJson nvarchar(max)
	,@ErrorDetails varchar(max) = NULL
	) 

AS

BEGIN

	SET NOCOUNT ON;
	DECLARE @NOW datetime = GETUTCDATE();
	DECLARE @RetryCount tinyint;

	IF @StatusTypeId = 7 --Error
	BEGIN
		SELECT @RetryCount = RetryCount + 1
		FROM ProductBatch 
		WHERE ProductBatchId = @ProductBatchID
	END
	ELSE
	BEGIN
		SELECT @RetryCount = RetryCount 
		FROM ProductBatch 
		WHERE ProductBatchId = @ProductBatchID
	END;

	 BEGIN TRY
            BEGIN TRAN;
				UPDATE [Enterprise].[ProductBatch] SET
					    [StatusTypeId] = @StatusTypeId
					   ,[RetryCount] = @RetryCount 
					   ,[InputJson] = COALESCE(@InputJson,[InputJson])
					   ,[LastRunDate] = CASE WHEN @StatusTypeId = 6 THEN  @NOW ELSE [LastRunDate] END--Running
					   ,[ModifiedDate] = @NOW
					   ,[ErrorDetails] = COALESCE(@ErrorDetails,[ErrorDetails])
				WHERE	
					ProductBatchId = @ProductBatchID

				SELECT @ProductBatchID as Id, '' AS ErrorMessage
			COMMIT;
        END TRY
        BEGIN CATCH
            DECLARE @ErrorLogID INT;
            EXEC dbo.LogError @ErrorLogID = @ErrorLogID OUTPUT;

            SELECT 0 AS Id ,
                   ErrorMessage
            FROM   dbo.ErrorLog
            WHERE  ErrorLogID = @ErrorLogID;

            ROLLBACK;
        END CATCH;
END;
GO
