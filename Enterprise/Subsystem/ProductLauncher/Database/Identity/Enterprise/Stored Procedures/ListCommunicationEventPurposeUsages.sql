CREATE PROCEDURE [Enterprise].[ListCommunicationEventPurposeUsages]
AS
BEGIN  
	SELECT	CommunicationEventPurposeUsageId,
			CommunicationEventId,
			CommunicationEventPurposeTypeId
	FROM	Enterprise.CommunicationEventPurposeUsage
END;