CREATE PROCEDURE [Enterprise].[GetCommunicationEventPurposeType]
	@CommunicationEventPurposeTypeID INT
AS
BEGIN
	SELECT	CommunicationEventPurposeTypeID,
			Description
	FROM	[Enterprise].CommunicationEventPurposeType
	WHERE	CommunicationEventPurposeTypeID = @CommunicationEventPurposeTypeID
END;

