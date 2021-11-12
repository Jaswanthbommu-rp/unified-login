CREATE PROCEDURE [Enterprise].[ListBatchProcessor]
(
    @BatchSize INT,
    @RetryCount TINYINT = 3,
    @IncludeErrorRecord BIT = 'True'
)
AS
BEGIN

    SET NOCOUNT ON;
    DECLARE @PBFiltered TABLE
    (
        [BatchProcessorId] [INT] NOT NULL,
        [CorrelationId] [UNIQUEIDENTIFIER] NOT NULL,
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
        [BatchProcessTypeId] [TINYINT] NOT NULL
    );
    IF
    (
        SELECT ControlValue
        FROM Enterprise.GlobalControl
        WHERE ControlName = 'IsNewBatchService'
    ) = 0
    BEGIN
        BEGIN TRANSACTION; -- HAve to lock the tables so that another process can't come in and scoop up our waiting processes

        INSERT INTO @PBFiltered
        (
            [BatchProcessorId],
            [CorrelationId],
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
            [LastRunDateTime]
        )
        SELECT TOP (@BatchSize)
               [BatchProcessorId],
               [CorrelationId],
               [RealPageId] AS EditorUserRealPageId,
               EditorUserPartyId,
               [EditorUserPersonaId],
               [SubjectUserPersonaId],
               [ProductId],
               [StatusTypeId],
               [RetryCount],
               [BatchProcessTypeId],
               [InputJSON],
               [CreatedDateTime],
               [LastRunDateTime]
        FROM [Enterprise].[BatchProcessor]
            JOIN Enterprise.Party
                ON BatchProcessor.EditorUserPartyId = Party.PartyId
        WHERE (
                  @IncludeErrorRecord = 'True'
                  AND
                  (
                      StatusTypeId = 7
                      AND RetryCount < @RetryCount
                  )
              )
              OR
              (
                  @IncludeErrorRecord = 'False'
                  AND StatusTypeId = 5
              );


        UPDATE Enterprise.BatchProcessor
        SET StatusTypeId = 6 --Running
        FROM Enterprise.BatchProcessor
            JOIN @PBFiltered AS Filtered
                ON Filtered.[BatchProcessorId] = BatchProcessor.[BatchProcessorId];

        SELECT [BatchProcessorId],
               [EditorUserRealPageId],
               [EditorUserPartyId],
               [CorrelationId],
               [EditorUserPersonaId],
               [SubjectUserPersonaId],
               [ProductId],
               [StatusTypeId],
               [RetryCount],
               [BatchProcessTypeId],
               [InputJson],
               [CreatedDateTime],
               [LastRunDateTime]
        FROM @PBFiltered;

        COMMIT TRANSACTION;
    END;
    ELSE
    BEGIN
        EXECUTE [Batch].[ListBatchProcessor] @BatchSize = @BatchSize,
                                             @RetryCount = @RetryCount,
                                             @IncludeErrorRecord = @IncludeErrorRecord;
    END;
END;