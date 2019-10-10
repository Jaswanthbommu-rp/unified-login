CREATE PROCEDURE [Enterprise].[GetCommunicationEventEmailByEventId]
	@CommunicationEventId BIGINT
AS
BEGIN
	SELECT  CommunicationEventEmailID,
			CommunicationEmailTemplateID,
			CommunicationEventId
	FROM	Enterprise.CommunicationEventEmail
	WHERE	CommunicationEventId = @CommunicationEventId;
END;