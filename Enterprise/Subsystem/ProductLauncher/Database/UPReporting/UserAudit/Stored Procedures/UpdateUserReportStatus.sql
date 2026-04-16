CREATE PROCEDURE [UserAudit].[UpdateUserReportStatus]
    @RequestId BIGINT,
    @StatusId  INT
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE [UserAudit].[Request]
    SET    [Status] = @StatusId
    WHERE  RequestId = @RequestId;
END