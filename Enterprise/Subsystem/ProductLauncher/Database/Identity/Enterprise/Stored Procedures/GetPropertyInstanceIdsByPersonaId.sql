CREATE PROCEDURE [Enterprise].[GetPropertyInstanceIdsByPersonaId]
	@PersonaId BIGINT,
	@ProductID INT
AS
BEGIN
	SET NOCOUNT ON
	SELECT
		pim.[PropertyInstanceId],
		PRI.CustomerPropertyId    
	FROM 
		Enterprise.PropertyInstanceMapping pim
		INNER JOIN Enterprise.PropertyInstance PRI ON pim.propertyInstanceId = PRI.propertyInstanceId  

	WHERE
		pim.PersonaId = @PersonaId 
		AND
		pim.ProductId = @ProductID
		AND
		pim.Active = 1
END
