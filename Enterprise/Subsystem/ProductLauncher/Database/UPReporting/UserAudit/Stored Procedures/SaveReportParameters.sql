CREATE PROCEDURE [UserAudit].[SaveReportParameters]  
(
@ReportKey VARCHAR(100)
,@OrgPartyId BIGINT
,@Status INT
,@ReportParams [UserAudit].[ReportParameters] READONLY
)
AS
BEGIN
 INSERT INTO UserAudit.Request(CreatedDate, ReportKey, OrgpartyId, [Status])  
 VALUES(GETUTCDATE(),@ReportKey,@OrgPartyId, @Status)  

	DECLARE @ReportId INT

	SELECT @ReportId = IDENT_CURRENT('[UserAudit].[Request]');

	INSERT INTO [UserAudit].[RequestReportParameter](RequestId, ReportParameterId, SelectedValue)
	SELECT @ReportId, [ReportParameterId], [SelectedValue] FROM @ReportParams

	SELECT @ReportId;
END
