IF OBJECT_ID('[Enterprise].[ListCommunicationEventPurposeUsages]') IS NOT NULL
	DROP PROCEDURE [Enterprise].[ListCommunicationEventPurposeUsages];

GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_NULLS ON
GO
CREATE PROCEDURE [Enterprise].[ListCommunicationEventPurposeUsages]
AS
BEGIN  
	SELECT	CommunicationEventPurposeUsageId,
			CommunicationEventId,
			CommunicationEventPurposeTypeId
	FROM	Enterprise.CommunicationEventPurposeUsage
END;
GO
