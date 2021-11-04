CREATE PROCEDURE [Enterprise].[GetEmployeeProductADGroupMapping]
	@ProductId INT,
	@PersonaId BIGINT
AS
BEGIN
	SELECT
		EmployeeProductMappingId,
		PersonaId, 
		ProductId, 
		ADGroupId, 
		CreatedDate 
	FROM 
		Enterprise.EmployeeProductMapping 
	WHERE 
		PersonaId = @PersonaId 
		AND ProductId = @ProductId
END
