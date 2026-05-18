CREATE PROCEDURE [Batch].[UpdateBulkResetPasswordStatus]
    @Id     BIGINT,
    @Status BIT
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE [Batch].[BulkResetPassword]
    SET [Status] = @Status
    WHERE [Id] = @Id;

    SELECT @@ROWCOUNT AS [Id], CAST(NULL AS NVARCHAR(MAX)) AS [ErrorMessage];
END
