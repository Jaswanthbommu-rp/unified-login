CREATE PROCEDURE [Enterprise].[ListOrganizationType]
AS
BEGIN
	SELECT OrganizationTypeId, Name FROM Enterprise.OrganizationType
	WHERE (ThruDate IS NULL OR Thrudate > GETUTCDATE())
END
