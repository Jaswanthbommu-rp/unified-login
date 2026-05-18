CREATE PROCEDURE [Batch].[ListPendingBulkResetPassword]
    @BatchSize INT
AS
BEGIN
    SET NOCOUNT ON;

    -- Returns up to @BatchSize unprocessed bulk-reset rows for BatchProcessor to pick up.
    -- Status = 0 means pending (not yet processed).
    SELECT TOP (@BatchSize)
        [Id],
        [RealPageId],
        [CreatedDateTime]
    FROM [Batch].[BulkResetPassword]
    WHERE [Status] = 0
    ORDER BY [CreatedDateTime] ASC;
END
