CREATE PROCEDURE [Security].[ListRolesForProductsByPersonaId] (
	@ProductId int,
	@PersonaId bigint = NULL,
	@PartyId bigint = NULL
)
AS
BEGIN
	SELECT	DISTINCT
					R.RoleName 'Role',
					R.ShortName 'RoleNickName',
					R.RoleID 'RoleId',
					p.ProductId 'Product',
					RT.Value AS RoleType,
					pe.PersonaId,
					ULP.OrganizationPartyId
	FROM		Security.Role R
					INNER JOIN Security.RoleRight RR ON 
						R.RoleId = RR.RoleId
					INNER JOIN Security.[Right] RG ON 
						RR.RightID = RG.RightID
					INNER JOIN Security.RoleType RT ON 
						RT.RoleTypeId = R.RoleTypeID
					INNER JOIN Enterprise.Product P ON 
						P.ProductId = R.ProductId
					INNER JOIN Security.PersonaRole PR ON 
						PR.RoleID = R.RoleID
					INNER JOIN Person.Persona PE ON 
						PE.PersonaId = PR.PersonaId
					INNER JOIN Ident.UserLoginPersona ULP ON 
						ULP.UserLoginPersonaId = PE.UserLoginPersonaId
	WHERE	P.ProductId = @ProductId
	AND			((@PersonaId IS NULL) OR (pe.PersonaId = @PersonaId))
	AND			((@PartyId IS NULL) OR (ULP.OrganizationPartyId = @PartyId))
END;