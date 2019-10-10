IF OBJECT_ID('[Enterprise].[LinkEmailTemplateToValidEmail]') IS NOT NULL
	DROP PROCEDURE [Enterprise].[LinkEmailTemplateToValidEmail];

GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_NULLS ON
GO
CREATE PROCEDURE [Enterprise].[LinkEmailTemplateToValidEmail]
	@CommunicationEmailTemplateId INT,
	@ContactMechanismValidEmailID INT
AS
BEGIN
	INSERT INTO [Enterprise].CommunicationEventEmail (CommunicationEmailTemplateID, ContactMechanismValidEmailID)
	SELECT @CommunicationEmailTemplateId, @ContactMechanismValidEmailID
END;
GO
