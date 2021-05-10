CREATE PROCEDURE [Enterprise].[DeletePersonaSuggestedProperties] (
	@PersonaId bigint
 )
AS
BEGIN
	DELETE 
	FROM [Enterprise].[PersonaSuggestedProperties]
	WHERE PersonaId = @PersonaId
END
