CREATE PROCEDURE [Enterprise].[GetCommunicationEmailTemplate]
	@CommunicationEmailTemplateId INT = NULL
AS
BEGIN  
	SELECT	CommunicationEmailTemplateId,
			CommunicationEventAudienceTypeId,
			CommunicationEventPurposeTypeId,
			Subject,
			Body
	FROM	Enterprise.CommunicationEmailTemplate
	WHERE	CommunicationEmailTemplateID = @CommunicationEmailTemplateId;
END;