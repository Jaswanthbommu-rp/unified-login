CREATE PROCEDURE [Security].[GetADGroupsProductRightByPersonaId] (@personaId bigint)
AS
BEGIN
	SELECT g.ADGroupId, g.DisplayName as ADGroupName, pd.ProductsCount, rt.RightsCount
	FROM SECURITY.ADGroupUser u
	JOIN SECURITY.ADGroup g ON g.ADGroupId = u.ADGroupId
	CROSS APPLY (
		SELECT COUNT(art.ADGroupId) AS RightsCount
		FROM SECURITY.ADGroupRight art
	
		WHERE art.ADGroupId = g.ADGroupId) AS rt
	CROSS APPLY(
		SELECT COUNT(apd.ADGroupId) AS ProductsCount
		FROM SECURITY.[ADGroupProduct] apd
		WHERE apd.ADGroupId = g.ADGroupId
		) AS pd
	WHERE u.PersonaId = @personaId
	ORDER BY g.DisplayName
END