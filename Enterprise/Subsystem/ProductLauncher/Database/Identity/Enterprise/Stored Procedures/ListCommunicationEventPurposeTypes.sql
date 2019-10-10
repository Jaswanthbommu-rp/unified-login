CREATE PROCEDURE [Enterprise].[ListCommunicationEventPurposeTypes]
AS
BEGIN
	SELECT	CommunicationEventPurposeTypeID,
			Description
	FROM [Enterprise].CommunicationEventPurposeType
END;
