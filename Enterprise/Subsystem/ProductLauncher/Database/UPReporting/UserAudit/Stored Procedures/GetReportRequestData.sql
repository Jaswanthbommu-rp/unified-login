
CREATE PROCEDURE [UserAudit].[GetReportRequestData]
(
	@RequestId INT
)
AS
BEGIN  
	SELECT UR.RequestId, UR.ReportKey, UR.OrgPartyId, UP.ReportParameterId, UP.ReportParameterName, URP.SelectedValue, UR.[Status] FROM UserAudit.Request UR 
JOIN UserAudit.RequestReportParameter URP ON UR.RequestId = URP.RequestId
JOIN UserAudit.ReportParameter UP ON UP.ReportParameterId = urp.ReportParameterId
WHERE UR.RequestId=@RequestId
END