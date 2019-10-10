IF OBJECT_ID('[Enterprise].[ListProductBatch]') IS NOT NULL
	DROP PROCEDURE [Enterprise].[ListProductBatch];

GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_NULLS ON
GO
CREATE PROCEDURE [Enterprise].[ListProductBatch](
	 @IncludeErrorRecord bit = 'True'
	,@BatchSize int
	,@RetryCount tinyint = 3
	) 

AS

BEGIN

	SET NOCOUNT ON;
		DECLARE @PBFiltered TABLE(
		[ProductBatchId] [int] NOT NULL,
		[RealPageId] UNIQUEIDENTIFIER NOT NULL,
		[PersonPartyID] [bigint] NOT NULL,
		[CreateUserPersonaId] [bigint] NOT NULL,
		[AssignUserPersonaId] [bigint] NOT NULL,
		[ProductId] [int] NOT NULL,
		[StatusTypeId] [int] NOT NULL,
		[RetryCount] [tinyint] NOT NULL,
		[InputJson] [nvarchar](max) NOT NULL,
		[LastRunDate] [smalldatetime] NULL,
		[CreatedDate] [smalldatetime] NOT NULL,
		[ModifiedDate] [smalldatetime] NULL,
		[ErrorDetails] [varchar](max) NULL)
		
		BEGIN TRANSACTION-- HAve to lock the tables so that another process can't come in and scoop up our waiting processes

			INSERT INTO @PBFiltered(
				 [ProductBatchId]
				,[RealPageId]
				,[PersonPartyId]
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
			SELECT TOP (@BatchSize)
				 [ProductBatchId]
				,[RealpageId]
				,[PersonPartyId]
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
			FROM [Enterprise].[ProductBatch]
			JOIN Enterprise.Party ON ProductBatch.PersonPartyID = Party.PartyID
			WHERE
				(@IncludeErrorRecord = 'True' AND ( StatusTypeId = 7 and RetryCount < @Retrycount))
				OR  
				( @IncludeErrorRecord = 'False' AND StatusTypeID = 5)


		UPDATE Enterprise.ProductBatch SET StatusTypeId = 6 --Running
		FROM Enterprise.ProductBatch
		JOIN @PBFiltered AS Filtered ON Filtered.ProductBatchId = ProductBatch.ProductBatchId

		SELECT 
			 [ProductBatchId]
			,[RealPageId]
			,[PersonPartyId]
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
		FROM @PBFiltered

	COMMIT TRANSACTION
END;
GO
