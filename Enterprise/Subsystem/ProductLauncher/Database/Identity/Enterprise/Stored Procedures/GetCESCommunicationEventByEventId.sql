CREATE PROCEDURE [Enterprise].[GetCESCommunicationEventByEventId]
	@CommunicationEventId BIGINT
AS
BEGIN
	SELECT	CESCommunicationEventId,
			CESId,
			CommunicationEventId
	FROM	Enterprise.CESCommunicationEvent
	WHERE	CommunicationEventId = @CommunicationEventId;
END;
