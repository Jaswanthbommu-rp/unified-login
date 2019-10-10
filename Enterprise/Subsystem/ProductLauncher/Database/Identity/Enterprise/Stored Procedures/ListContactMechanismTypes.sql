CREATE PROCEDURE [Enterprise].[ListContactMechanismTypes]
AS
BEGIN
	SELECT	ContactMechanismTypeID,
			Description
	FROM [Enterprise].ContactMechanismType
END;
