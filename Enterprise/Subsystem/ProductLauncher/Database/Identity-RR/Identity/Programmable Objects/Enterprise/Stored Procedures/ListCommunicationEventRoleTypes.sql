IF OBJECT_ID('[Enterprise].[ListCommunicationEventRoleTypes]') IS NOT NULL
	DROP PROCEDURE [Enterprise].[ListCommunicationEventRoleTypes];

GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_NULLS ON
GO
CREATE PROCEDURE [Enterprise].[ListCommunicationEventRoleTypes]
AS
BEGIN
	SELECT	CommunicationEventRoleTypeID,
			Description
	FROM [Enterprise].CommunicationEventRoleType
END;
GO
