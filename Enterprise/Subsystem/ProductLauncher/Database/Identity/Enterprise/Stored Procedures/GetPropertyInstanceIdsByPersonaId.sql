CREATE PROCEDURE [Enterprise].[GetPropertyInstanceIdsByPersonaId]
	@PersonaId BIGINT,
	@ProductID INT
AS
BEGIN
	SET NOCOUNT ON
	SELECT
		[PropertyInstanceId]
	FROM 
		Enterprise.PropertyInstanceMapping pim 

	WHERE	pim.PersonaId = @PersonaId 
	AND		pim.ProductId = @ProductID
	AND		pim.Active = 1
	AND     pim.PropertyInstanceId <> 0
END
