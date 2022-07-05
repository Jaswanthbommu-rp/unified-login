CREATE PROCEDURE [Enterprise].[DeletePersonaProductMatchedPrimaryProperties](
    @ProductId int,
	@PersonaId bigint
 )
AS
BEGIN
	DELETE FROM [Enterprise].[UserSyncProductPrimaryPropertiesStaging] 
	WHERE ProductId = @productId 
	And PersonaId = @personaId
	SELECT	1 AS Id
END
