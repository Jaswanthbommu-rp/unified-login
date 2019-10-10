IF OBJECT_ID('[Person].[ListPreferredContactMethods]') IS NOT NULL
	DROP PROCEDURE [Person].[ListPreferredContactMethods];

GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_NULLS ON
GO
CREATE PROCEDURE [Person].[ListPreferredContactMethods]
AS
BEGIN
	SELECT	1 AS PreferredContactMethodId,
			'Phone' AS Name
	UNION ALL
	SELECT	2 AS PreferredContactMethodId,
			'Email' AS Name
END
GO
