CREATE PROCEDURE [Enterprise].[ListOrganizationDomain]
AS
BEGIN
	SET NOCOUNT ON;

	SELECT
		OrganizationDomainId,
		Name
	FROM
		Enterprise.OrganizationDomain
	WHERE
		Name IS NOT NULL
		AND LTRIM(RTRIM(Name)) <> ''
		AND ( ThruDate IS NULL OR ThruDate > GETUTCDATE() )
	ORDER BY
		Name
END

