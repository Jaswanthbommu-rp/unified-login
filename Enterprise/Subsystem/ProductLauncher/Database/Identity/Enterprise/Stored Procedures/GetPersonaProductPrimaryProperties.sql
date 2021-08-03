CREATE PROCEDURE [Enterprise].[GetPersonaProductPrimaryProperties]
(
	@PersonaId	BIGINT
)
AS
BEGIN
	SELECT 
		PersonaProductPropertyId
		,PersonaId
		,ProductId
		,PropertyId
		,PropertyInstanceId		
		FROM Enterprise.PersonaProductProperty
		WHERE PersonaId = @PersonaId
END