CREATE PROCEDURE [UserAudit].[UsersToProcessByReportId]
    @ReportId BIGINT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        u.AuditUserId           AS [UserAuditId],
        rp.RequestId            AS [ReportId],
        u.UserName,
        u.PersonaId,
        u.OrganizationPartyId,
        u.OrganizationRealPageId,
        u.Complete
    FROM [UserAudit].[User] u
    INNER JOIN [UserAudit].[Request] rp ON u.RequestId = rp.RequestId
    WHERE rp.RequestId = @ReportId;
END