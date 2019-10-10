IF OBJECT_ID('[Ident].[GetProductSamlDetails]') IS NOT NULL
	DROP PROCEDURE [Ident].[GetProductSamlDetails];

GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_NULLS ON
GO
CREATE PROCEDURE [Ident].[GetProductSamlDetails] (
	@PersonaId bigint,
	@ProductId int
)
AS
BEGIN

		DECLARE @NOW  DATETIME = GETUTCDATE();

	SELECT	sa.SamlAttributeId,
			sa.Name,
			sat.Name AS [Type],
			sua.SamlUserAttributeId,
			sua.Value
	FROM	Person.Persona p
			INNER JOIN Ident.SamlUserAttribute sua ON (p.PersonaId = sua.PersonaId)
			INNER JOIN Ident.SamlAttribute sa ON (sua.SamlAttributeId = sa.SamlAttributeId)
			INNER JOIN Ident.SamlAttributeType sat ON (sa.SamlAttributeTypeId = sat.SamlAttributeTypeId)
	WHERE	p.PersonaId = @PersonaId
	AND		sua.ProductId = @ProductId
	AND ((@NOW BETWEEN p.FromDate AND p.ThruDate) OR (@NOW >= p.FromDate AND p.ThruDate IS NULL))
END
GO
