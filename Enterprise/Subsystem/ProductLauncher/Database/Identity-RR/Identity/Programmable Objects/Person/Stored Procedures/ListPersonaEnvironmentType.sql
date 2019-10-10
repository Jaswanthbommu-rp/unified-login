IF OBJECT_ID('[Person].[ListPersonaEnvironmentType]') IS NOT NULL
	DROP PROCEDURE [Person].[ListPersonaEnvironmentType];

GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_NULLS ON
GO
CREATE PROCEDURE [Person].[ListPersonaEnvironmentType](
	 @Name NVARCHAR(50) = NULL
)

AS

BEGIN
	SELECT 
		 [PersonaEnvironmentTypeID]
		,[Name]
	FROM [Person].[PersonaEnvironmentType]
	WHERE
	(@Name IS NULL OR [Name] = @NAME);
END;
GO
