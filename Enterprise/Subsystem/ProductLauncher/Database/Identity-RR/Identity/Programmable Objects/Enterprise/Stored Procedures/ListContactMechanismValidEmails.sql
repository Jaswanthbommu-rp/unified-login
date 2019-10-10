IF OBJECT_ID('[Enterprise].[ListContactMechanismValidEmails]') IS NOT NULL
	DROP PROCEDURE [Enterprise].[ListContactMechanismValidEmails];

GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_NULLS ON
GO
CREATE PROCEDURE [Enterprise].[ListContactMechanismValidEmails]
AS
BEGIN  
	SELECT	ContactMechanismValidEmailId,
			ContactMechanismTypeId
	FROM	Enterprise.ContactMechanismValidEmail
END;
GO
