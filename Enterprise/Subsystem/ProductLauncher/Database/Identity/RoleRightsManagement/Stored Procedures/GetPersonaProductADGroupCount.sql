CREATE PROCEDURE [Security].[GetPersonaProductsADGroupsCount] (@PersonaId BIGINT)
AS
BEGIN
	SELECT AGP.ProductId, COUNT(1) AS ADGroupCount
	FROM [UPQA].[Security].[ADGroupUser] AU
	  JOIN Security.ADGroupProduct AGP on au.ADGroupId = AGP.ADGroupId	
	WHERE AU.PersonaId = @PersonaId
	GROUP BY AGP.ProductId
END
