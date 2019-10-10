IF OBJECT_ID('[Enterprise].[ListCommunicationEventPurposeTypes]') IS NOT NULL
	DROP PROCEDURE [Enterprise].[ListCommunicationEventPurposeTypes];

GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_NULLS ON
GO
CREATE PROCEDURE [Enterprise].[ListCommunicationEventPurposeTypes]
AS
BEGIN
	SELECT	CommunicationEventPurposeTypeID,
			Description
	FROM [Enterprise].CommunicationEventPurposeType
END;
GO
