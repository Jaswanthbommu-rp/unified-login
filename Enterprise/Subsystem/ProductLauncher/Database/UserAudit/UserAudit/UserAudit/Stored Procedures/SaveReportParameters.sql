CREATE PROCEDURE [UserAudit].[SaveReportParameters]  
(  
@ReportKey VARCHAR(100)  
,@OrgPartyId BIGINT  
,@ReportParams [UserAudit].[ReportParameters] READONLY  
)  
AS  
BEGIN  
 INSERT INTO UserAudit.Request(CreatedDate, ReportKey, OrgpartyId)  
 VALUES(GETUTCDATE(),@ReportKey,@OrgPartyId)  
  
 DECLARE @ReportId INT  
  
 SELECT @ReportId = IDENT_CURRENT('[UserAudit].[Request]');  
  
 INSERT INTO [UserAudit].[RequestReportParameter](RequestId, ReportParameterId, SelectedValue)  
 --VALUES(@ReportId,)  
 SELECT @ReportId, [ReportParameterId], [SelectedValue] FROM @ReportParams  
  
 SELECT @ReportId;  
END
