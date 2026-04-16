CREATE PROCEDURE [UserAudit].[UpdateUserProductStatus]
    @UserProductId BIGINT,
    @StatusId      INT
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE [UserAudit].[UserProduct]
    SET    CompletedDate = GETUTCDATE(),
           [Status]      = @StatusId
    WHERE  UserProductId = @UserProductId;
END