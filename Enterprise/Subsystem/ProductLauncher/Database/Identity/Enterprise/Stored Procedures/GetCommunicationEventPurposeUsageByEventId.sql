CREATE PROCEDURE [Enterprise].[GetCommunicationEventPurposeUsageByEventId]
	@CommunicationEventID BIGINT
AS
BEGIN
	SELECT	CommunicationEventPurposeUsageID,
			CommunicationEventId,
			CommunicationEventPurposeTypeID
	FROM	[Enterprise].CommunicationEventPurposeUsage
	WHERE	CommunicationEventID = @CommunicationEventID;
END;

