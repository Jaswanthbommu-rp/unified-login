CREATE PROCEDURE [Enterprise].[GetPersonaProductPrimaryProperties]
(
	@PersonaId	BIGINT
)
AS
BEGIN
	SELECT UserSyncPrimaryPropertyId AS	'PersonaProductPropertyId'
		,PersonaId
		,ProductId
		,ProductPropertyId AS 'PropertyId'
		,PropertyInstanceId		
		FROM Enterprise.UserSyncProductPrimaryPropertiesStaging
		WHERE PersonaId = @PersonaId
END