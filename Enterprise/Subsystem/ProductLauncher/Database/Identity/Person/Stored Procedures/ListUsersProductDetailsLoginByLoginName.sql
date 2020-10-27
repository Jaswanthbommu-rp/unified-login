CREATE PROCEDURE Person.ListUsersProductDetailsLoginByLoginName(@loginName VARCHAR(100))
AS
BEGIN
	;WITH UserAttributes
	(
		ProductId,
		UserLoginPersonaId,
		Value,
		Name
	)
	AS
	(
		SELECT
				sua.ProductId,
				ulp.UserLoginPersonaId,
				sua.Value,
				Sa.Name
		FROM	Person.Persona p
				INNER JOIN Ident.SamlUserAttribute sua ON (p.PersonaId = sua.PersonaId)
				INNER JOIN Ident.SamlAttribute sa ON (sua.SamlAttributeId = sa.SamlAttributeId)
				INNER JOIN Ident.SamlAttributeType sat ON (sa.SamlAttributeTypeId = sat.SamlAttributeTypeId)
				INNER JOIN Enterprise.PersonaConfiguration AS pc ON pc.PersonaId = sua.PersonaId and sua.ProductId = pc.ProductId
				INNER JOIN Ident.UserLoginPersona ulp ON p.UserLoginPersonaId = ulp.UserLoginPersonaId
				INNER JOIN Ident.UserLogin ul ON ulp.UserLoginId = ul.UserId
				INNER JOIN Enterprise.PartyRelationship PR ON(PR.PartyIdFrom = UL.PersonPartyId AND PR.PartyIdTo = ULP.OrganizationPartyId
																AND PR.RoleTypeIdTo = 205
																AND PR.ThruDate IS NULL)

		WHERE	
			ul.LoginName = @loginName AND
			pc.StatusTypeId = 8 AND
			sua.ThruDate IS NULL AND
			PR.RoleTypeIdFrom IN (401,405) --ONLY REGULAR AND EXTERNAL USERS
	)

	SELECT DISTINCT
		p.ProductId,
		p.BooksProductCode AS ProductCode,
		o.Name AS Company,
		(
			SELECT
				Name,
				Value
			FROM UserAttributes
			WHERE
				UserAttributes.ProductId = p.ProductId
				AND UserAttributes.UserLoginPersonaId = ulp.UserLoginPersonaId
			FOR JSON AUTO, INCLUDE_NULL_VALUES
		) AS UserAttribute

	FROM Enterprise.Product AS p
		INNER JOIN UserAttributes AS ua ON p.ProductId = ua.ProductId
		INNER JOIN Ident.UserLoginPersona ulp ON ulp.UserLoginPersonaId = ua.UserLoginPersonaId
		INNER JOIN Enterprise.Organization o ON ulp.OrganizationPartyId = o.PartyId
		
	ORDER BY o.Name ASC

END