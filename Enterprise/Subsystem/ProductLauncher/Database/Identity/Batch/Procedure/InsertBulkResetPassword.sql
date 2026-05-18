CREATE PROCEDURE [Batch].[InsertBulkResetPassword]
    @RealPageIds [dbo].[PartyGUID] READONLY
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO [Batch].[BulkResetPassword] ([RealPageId])
    SELECT [RealPageID]
    FROM @RealPageIds
    WHERE [RealPageID] IS NOT NULL;

    SELECT @@ROWCOUNT AS [Id], CAST(NULL AS NVARCHAR(MAX)) AS [ErrorMessage];
END
