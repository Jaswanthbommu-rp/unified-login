CREATE PROCEDURE [Person].[ListPreferredContactMethods]
AS
BEGIN
	SELECT	1 AS PreferredContactMethodId,
			'Phone' AS Name
	UNION ALL
	SELECT	2 AS PreferredContactMethodId,
			'Email' AS Name
END