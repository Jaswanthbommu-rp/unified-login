CREATE PROCEDURE [Security].[GetADGroupsByPersonaId] (@personaId bigint)
AS
BEGIN
	SELECT g.ADGroupId, g.DisplayName as ADGroupName, pd.*, rls.*, rt.*
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
	CROSS APPLY(
		SELECT COUNT(distinct rr.RoleId) AS RolesCount
		FROM SECURITY.[ADGroupRight] art
		JOIN SECURITY.[RoleRight] rr ON rr.RightId = art.RightId
		WHERE art.ADGroupId = g.ADGroupId
		) AS rls
	WHERE u.PersonaId = @personaId
	ORDER BY g.DisplayName
END