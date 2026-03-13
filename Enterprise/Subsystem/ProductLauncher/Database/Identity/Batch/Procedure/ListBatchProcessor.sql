CREATE PROCEDURE [Batch].[ListBatchProcessor]  
(  
    @BatchSize INT,  
    @RetryCount TINYINT = 3,  
    @IncludeErrorRecord BIT = 'True',
    @UseAPIV2 BIT = 0
)  
AS  
BEGIN  
  
    SET NOCOUNT ON;  
    SET @BatchSize = 30;
    DECLARE @PBFiltered TABLE  
    (  
        [BatchProcessorId] [INT] NOT NULL,  
        [CorrelationId] [UNIQUEIDENTIFIER] NOT NULL,
		[BatchProcessorGroupId] BIGINT,
        [EditorUserRealPageId] UNIQUEIDENTIFIER NOT NULL,  
        [EditorUserPartyId] [BIGINT] NOT NULL,  
        [EditorUserPersonaId] [BIGINT] NOT NULL,  
        [SubjectUserPersonaId] [BIGINT] NOT NULL,  
        [ProductId] [INT] NOT NULL,  
        [StatusTypeId] [INT] NOT NULL,  
        [RetryCount] [TINYINT] NOT NULL,  
        [InputJson] [NVARCHAR](MAX) NOT NULL,  
        [LastRunDateTime] [DATETIME] NULL,  
        [CreatedDateTime] [DATETIME] NOT NULL,  
        [BatchProcessTypeId] [TINYINT] NOT NULL,
		[ImpersonatorUserId] [BIGINT] NULL  
    );  
  
    BEGIN TRANSACTION; -- HAve to lock the tables so that another process can't come in and scoop up our waiting processes  
  
 ;with batchtoprocess as (  
  SELECT  
      [BatchProcessorId],  
      [CorrelationId], 
	  [BatchProcessorGroupId], 
      [RealPageId] AS EditorUserRealPageId,  
      EditorUserPartyId,  
      [EditorUserPersonaId],  
      [SubjectUserPersonaId],  
      [ProductId],  
      [StatusTypeId],  
      [RetryCount],  
      [BatchProcessTypeId],  
      [InputJson],  
      [CreatedDateTime],  
      [LastRunDateTime],  
      row_number() over (partition by subjectuserpersonaid, productid order by batchprocessorid asc ) as rn,  
      row_number() over (partition by editoruserpersonaid order by batchprocessorid asc ) as rn2,  
     [ImpersonatorUserId]
  FROM Batch.[BatchProcessor] BP  
   JOIN Enterprise.Party P  
    ON BP.EditorUserPartyId = P.PartyId  
  WHERE bp.createddatetime > dateadd(dd, -3, getutcdate())
  AND BP.UseAPIV2 = @UseAPIV2
  AND
  (
        (
              @IncludeErrorRecord = 'True'  
              AND  
              (  
               BP.StatusTypeId = 7  
               AND BP.RetryCount < @RetryCount  
              )  
             )  
             OR  
             (  
              @IncludeErrorRecord = 'False'  
              AND BP.StatusTypeID = 5  
             )  
        ) 
   )
    INSERT INTO @PBFiltered  
    (  
        [BatchProcessorId],  
        [CorrelationId], 
		[BatchProcessorGroupId], 
        [EditorUserRealPageId],  
        [EditorUserPartyId],  
        [EditorUserPersonaId],  
        [SubjectUserPersonaId],  
        [ProductId],  
        [StatusTypeId],  
        [RetryCount],  
        [BatchProcessTypeId],  
        [InputJson],  
        [CreatedDateTime],  
        [LastRunDateTime],
        [ImpersonatorUserId]  
    )  
 SELECT TOP (@BatchSize)  
  [BatchProcessorId],  
        [CorrelationId],  
		[BatchProcessorGroupId],
        [EditorUserRealPageId],  
        [EditorUserPartyId],  
        [EditorUserPersonaId],  
        [SubjectUserPersonaId],  
        [ProductId],  
        [StatusTypeId],  
        [RetryCount],  
        [BatchProcessTypeId],  
        [InputJson],  
        [CreatedDateTime],  
        [LastRunDateTime],
        [ImpersonatorUserId]  
  From batchtoprocess   
  WHERE   
   rn = 1   
   and   
   rn2 <= 7  
  
    UPDATE Batch.BatchProcessor  
    SET StatusTypeId = 6 --Running  
    FROM Batch.BatchProcessor BP  
        JOIN @PBFiltered F  
            ON F.[BatchProcessorId] = BP.[BatchProcessorId];  
  
    SELECT [BatchProcessorId],  
           [EditorUserRealPageId],  
           [EditorUserPartyId],  
           [CorrelationId],
		   [BatchProcessorGroupId],  
           [EditorUserPersonaId],  
           [SubjectUserPersonaId],  
           [ProductId],  
           [StatusTypeId],  
           [RetryCount],  
           [BatchProcessTypeId],  
           [InputJson],  
           [CreatedDateTime],  
           [LastRunDateTime],
           [ImpersonatorUserId]  
    FROM @PBFiltered;  
  
    COMMIT TRANSACTION;  
END;  