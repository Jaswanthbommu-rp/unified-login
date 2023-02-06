CREATE PROCEDURE [UserAudit].[GetReportRequest]
(
	@OrgPartyId BIGINT
)
AS
BEGIN
	SELECT RequestId,CreatedDate,ReportKey,OrgPartyId FROM UserAudit.Request WHERE OrgPartyId = @OrgPartyId AND CONVERT(DATE, CreatedDate) = CONVERT(DATE, GETUTCDATE());
END