CREATE PROCEDURE [UserAudit].[UpdateAuditUserStatus]
    @UserAuditId BIGINT,
    @IsComplete  BIT
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE [UserAudit].[User]
    SET    CompletedDate = GETUTCDATE(),
           Complete      = @IsComplete
    WHERE  AuditUserId = @UserAuditId;
END