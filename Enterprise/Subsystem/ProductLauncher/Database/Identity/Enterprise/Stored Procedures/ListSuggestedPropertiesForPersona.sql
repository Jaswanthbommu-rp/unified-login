CREATE PROCEDURE [Enterprise].[ListSuggestedPropertiesForPersona] (
	@PersonaId bigint
 )
AS
BEGIN
	SELECT PP.ProductPropertyId, PI1.InstanceId AS PropertyInstanceId, PP.ProductId
	FROM [Enterprise].[PersonaSuggestedProperties] PP
	INNER JOIN [Enterprise].[PropertyInstance] PI1 ON PP.PropertyInstanceId = PI1.PropertyInstanceId
	WHERE PersonaId = @PersonaId
END
