IF OBJECT_ID('[Enterprise].[ListContactMechanismTypes]') IS NOT NULL
	DROP PROCEDURE [Enterprise].[ListContactMechanismTypes];

GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_NULLS ON
GO
CREATE PROCEDURE [Enterprise].[ListContactMechanismTypes]
AS
BEGIN
	SELECT	ContactMechanismTypeID,
			Description
	FROM [Enterprise].ContactMechanismType
END;
GO
