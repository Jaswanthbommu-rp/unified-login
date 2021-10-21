CREATE PROCEDURE Ident.GetSamlProductAttributes
(
@ProductId bigint
)
AS
BEGIN
	SELECT 
		spa.ProductID
		,sa.DisplayName
		,sa.Name as SamlAttributeName
	FROM  Ident.SamlAttribute sa 
		INNER JOIN Ident.samlProductAttribute spa 
			ON sa.SamlAttributeId = spa.SamlAttributeid 
				AND spa.productid = @ProductId 
				AND ISNULL(sa.DisplayName, '') <> ''
END

