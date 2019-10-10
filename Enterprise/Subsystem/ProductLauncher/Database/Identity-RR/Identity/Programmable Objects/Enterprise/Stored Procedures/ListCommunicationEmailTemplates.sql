IF OBJECT_ID('[Enterprise].[ListCommunicationEmailTemplates]') IS NOT NULL
	DROP PROCEDURE [Enterprise].[ListCommunicationEmailTemplates];

GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_NULLS ON
GO
CREATE PROCEDURE [Enterprise].[ListCommunicationEmailTemplates]
AS
BEGIN  
	SELECT	CommunicationEmailTemplateId,
			Subject,
			Body
	FROM	Enterprise.CommunicationEmailTemplate
END;
GO
