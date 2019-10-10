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
