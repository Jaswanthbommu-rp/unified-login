CREATE PROCEDURE [Enterprise].[ListOrganizationDomain]
AS
BEGIN
	SELECT 
		OrganizationDomainId,
		Name
	FROM
		Enterprise.OrganizationDomain
	WHERE
		ThruDate IS NULL 
		OR 
		Thrudate > GETUTCDATE()
END

