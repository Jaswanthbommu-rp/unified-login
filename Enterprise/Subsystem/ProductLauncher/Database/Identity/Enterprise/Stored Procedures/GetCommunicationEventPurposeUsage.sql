CREATE PROCEDURE [Enterprise].[GetCommunicationEventPurposeUsage]
	@CommunicationEventPurposeUsageID BIGINT
AS
BEGIN
	SELECT	CommunicationEventPurposeUsageID,
			CommunicationEventId,
			CommunicationEventPurposeTypeID
	FROM	[Enterprise].CommunicationEventPurposeUsage
	WHERE	CommunicationEventPurposeUsageID = @CommunicationEventPurposeUsageID;
END;

