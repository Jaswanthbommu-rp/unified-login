
CREATE PROCEDURE [Batch].[ListBatchProcessor]
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
        [LastRunDateTime] [SMALLDATETIME] NULL,
        [CreatedDateTime] [SMALLDATETIME] NOT NULL,
        [BatchProcessTypeId] [TINYINT] NOT NULL
    );

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
           [InputJson],
           [CreatedDateTime],
           [LastRunDateTime]
    FROM Batch.[BatchProcessor] BP
        JOIN Enterprise.Party P
            ON BP.EditorUserPartyId = P.PartyId
    WHERE (
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
          );


    UPDATE Batch.BatchProcessor
    SET StatusTypeId = 6 --Running
    FROM Batch.BatchProcessor BP
        JOIN @PBFiltered F
            ON F.[BatchProcessorId] = BP.[BatchProcessorId];

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
GO
