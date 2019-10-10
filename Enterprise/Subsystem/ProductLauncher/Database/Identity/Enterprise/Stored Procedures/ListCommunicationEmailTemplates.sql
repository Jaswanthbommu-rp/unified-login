CREATE PROCEDURE [Enterprise].[ListCommunicationEmailTemplates]
	@CommunicationEventAudienceTypeId INT = NULL,
	@CommunicationEventPurposeTypeId INT = NULL
AS
BEGIN  
	SELECT	CommunicationEmailTemplateId,
			CommunicationEventAudienceTypeId,
			CommunicationEventPurposeTypeId,
			Subject,
			Body
	FROM	Enterprise.CommunicationEmailTemplate
	WHERE	(CommunicationEventAudienceTypeId = @CommunicationEventAudienceTypeId OR @CommunicationEventAudienceTypeId IS NULL)
	  AND	(CommunicationEventPurposeTypeId = @CommunicationEventPurposeTypeId OR @CommunicationEventPurposeTypeId IS NULL)
END;