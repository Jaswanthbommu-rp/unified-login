CREATE PROCEDURE [Enterprise].[GetCESCommunicationEvent]
	@CESCommunicationEventId BIGINT
AS
BEGIN
	SELECT	CESCommunicationEventId,
			CESId,
			CommunicationEventId
	FROM	Enterprise.CESCommunicationEvent
	WHERE	CESCommunicationEventId = @CESCommunicationEventId;
END;
