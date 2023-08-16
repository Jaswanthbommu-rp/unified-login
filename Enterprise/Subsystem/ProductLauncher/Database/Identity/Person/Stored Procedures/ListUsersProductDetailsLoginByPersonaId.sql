CREATE PROCEDURE [Person].[ListUsersProductDetailsLoginByPersonaId](@PersonaId BIGINT) AS
BEGIN
	;WITH UserAttributes
	(
		ProductId,
		Name,
		Value
	)
	AS
	(
		SELECT DISTINCT 
			sua.ProductId,
			Name,
			Value
				
			FROM Ident.SamlAttribute AS sm
				INNER JOIN Ident.SamlUserAttribute AS sua ON sm.SamlAttributeId = sua.SamlAttributeId
				INNER JOIN Enterprise.PersonaConfiguration AS pc ON pc.PersonaId = sua.PersonaId and sua.ProductId = pc.ProductId
			WHERE 
				sua.PersonaId = @PersonaId
				AND pc.StatusTypeId = 8
				AND sua.ThruDate IS NULL
	)
	
	SELECT DISTINCT
		p.ProductId,
		p.BooksProductCode AS ProductCode,
		(
			SELECT
				Name,
				Value
			FROM UserAttributes
			WHERE
				UserAttributes.ProductId = p.ProductId
			FOR JSON AUTO, INCLUDE_NULL_VALUES
		) AS UserAttribute

	FROM Enterprise.Product AS p
		INNER JOIN UserAttributes AS ua ON p.ProductId = ua.ProductId
	ORDER BY p.ProductId ASC
END;