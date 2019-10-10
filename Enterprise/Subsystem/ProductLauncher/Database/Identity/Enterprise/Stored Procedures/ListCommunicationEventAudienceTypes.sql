CREATE PROCEDURE [Enterprise].[ListCommunicationEventAudienceTypes]
AS
BEGIN
	SELECT	CommunicationEventAudienceTypeID,
			Description
	FROM [Enterprise].CommunicationEventAudienceType
END;
