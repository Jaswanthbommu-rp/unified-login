CREATE OR ALTER PROCEDURE [Person].[GetProductsByPersonaId]
	@PersonaId int = 0,
	@StatusTypeId int = 8
AS
BEGIN
	SET NOCOUNT ON
	SELECT 
		PC.ProductId,
		P.Name,
		P.BooksProductCode,
		PC.isFavorite,
		PC.StatusTypeId
		--,*
	FROM
		Enterprise.PersonaConfiguration PC
		INNER JOIN Enterprise.Product P  ON PC.ProductId = P.ProductId

	where 
		PC.PersonaId = @PersonaId
		AND PC.ThruDate IS NULL
		AND PC.StatusTypeId = @StatusTypeId

END