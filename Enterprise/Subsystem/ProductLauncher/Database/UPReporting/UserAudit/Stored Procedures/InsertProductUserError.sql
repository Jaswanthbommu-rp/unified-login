CREATE PROCEDURE [UserAudit].[InsertProductUserError]
    @UserProductId BIGINT,
    @ErrorText     NVARCHAR(MAX)
AS
BEGIN
    SET NOCOUNT ON;

    IF NOT EXISTS (
        SELECT TOP 1 1
        FROM [UserAudit].[UserProductError]
        WHERE UserProductId = @UserProductId
          AND ErrorText      = @ErrorText
    )
    BEGIN
        INSERT INTO [UserAudit].[UserProductError] (UserProductId, ErrorText)
        VALUES (@UserProductId, @ErrorText);
    END
END