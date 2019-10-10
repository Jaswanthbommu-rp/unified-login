CREATE PROCEDURE [Enterprise].[ListProductBatchByRealPageId]
(
    @RealPageId UNIQUEIDENTIFIER,
    @AssignUserPersonaId BIGINT
)
AS
BEGIN

    SET NOCOUNT ON;
    IF
    (
        SELECT ControlValue
        FROM Enterprise.GlobalControl
        WHERE ControlName = 'IsNewBatchService'
    ) = 0
    BEGIN
        SELECT [ProductBatchId],
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
               [ErrorDetails],
               [BatchTypeId],
               [CorrelationId]
        FROM [Enterprise].[ProductBatch]
            JOIN Enterprise.Party
                ON ProductBatch.PersonPartyId = Party.PartyId
        WHERE Party.RealPageId = @RealPageId
              AND AssignUserPersonaId = @AssignUserPersonaId;
    END;
    ELSE
    BEGIN


        SELECT [BatchProcessorId],
               [CorrelationId],
               [EditorUserPartyId],
               [EditorUserPersonaId],
               [SubjectUserPersonaId],
               [BatchProcessTypeId],
               [ProductId],
               [StatusTypeId],
               [RetryCount],
               [InputJSON],
               [CreatedDateTime],
               [LastRunDateTime]
        FROM [Batch].[BatchProcessor] BP
            JOIN Enterprise.Party P
                ON BP.EditorUserPartyId = P.PartyId
        WHERE P.RealPageId = @RealPageId
              AND BP.SubjectUserPersonaId = @AssignUserPersonaId;
    END;
END;