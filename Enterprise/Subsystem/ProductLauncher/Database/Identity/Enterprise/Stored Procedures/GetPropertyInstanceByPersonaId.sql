CREATE PROCEDURE [Enterprise].[GetPropertyInstanceByPersonaId]
	@PersonaId BIGINT,
	@ProductID INT
AS
BEGIN
	SET NOCOUNT ON
	SELECT
		PI1.[PropertyInstanceId]
		,[Name]
		,[Address]
		,[City]
		,[State]
		,[PostalCode]
		,[Country]
		,[County]
		,[Latitude]
		,[Longitude]
		,[InstanceId]

	FROM 
		[Enterprise].[PropertyInstance] pi1
		INNER JOIN 
		Enterprise.PropertyInstanceMapping pim 
			ON pim.PropertyInstanceId = pi1.PropertyInstanceId 

	WHERE
		pim.PersonaId = @PersonaId 
		AND	pim.ProductId = @ProductID
		AND	pim.Active = 1
		AND pi1.IsDeleted = 0
		AND pim.PropertyInstanceId <> 0
	ORDER BY Name
END
