CREATE PROCEDURE [Security].[ListRolesForProductsByPersonaId] (
	@ProductId int,
	@PersonaId bigint = NULL,
	@PartyId bigint = NULL
)
AS
BEGIN
       DECLARE @PersonaRoleId INT
       DECLARE @final NVARCHAR(MAX)
       DECLARE @result_sp TABLE (results NVARCHAR(MAX))
       DECLARE @json VARCHAR(max)

       SELECT @PersonaRoleId = PR.RoleId
       FROM Security.PersonaRole PR
       JOIN Security.ROLE R ON R.RoleId = PR.RoleId
       WHERE r.ProductId = @ProductId
              AND Pr.PersonaId = @PersonaId

       IF @PersonaRoleId IS NOT NULL
       BEGIN
              SELECT DISTINCT R.RoleName 'Role'
                     ,R.ShortName 'RoleNickName'
                     ,R.RoleID 'RoleId'
                     ,p.ProductId 'Product'
                     ,RT.Value AS RoleType
                     ,pe.PersonaId
                     ,ULP.OrganizationPartyId
              FROM Security.ROLE R
              INNER JOIN Security.RoleRight RR ON R.RoleId = RR.RoleId
              INNER JOIN Security.[Right] RG ON RR.RightID = RG.RightID
              INNER JOIN Security.RoleType RT ON RT.RoleTypeId = R.RoleTypeID
              INNER JOIN Enterprise.Product P ON P.ProductId = R.ProductId
              INNER JOIN Security.PersonaRole PR ON PR.RoleID = R.RoleID
              INNER JOIN Person.Persona PE ON PE.PersonaId = PR.PersonaId
              INNER JOIN Ident.UserLoginPersona ULP ON ULP.UserLoginPersonaId = PE.UserLoginPersonaId
              WHERE P.ProductId = @ProductId
                     AND (
                           (@PersonaId IS NULL)
                           OR (pe.PersonaId = @PersonaId)
                           )
                     AND (
                           (@PartyId IS NULL)
                           OR (ULP.OrganizationPartyId = @PartyId)
                           )
       END
       ELSE
       BEGIN
              INSERT INTO @result_sp
              EXEC [Enterprise].[GetUserProductBatchJsonData] @ProductId
                     ,@PersonaId

              SELECT @json = results
              FROM @result_sp

              SELECT DISTINCT R.RoleName 'Role'
                     ,R.ShortName 'RoleNickName'
                     ,CONVERT(BIGINT,JSON_VALUE(@JSON, '$.RoleList[0]')) 'RoleId'
                     ,r.ProductId 'Product'
                     ,RT.Value AS RoleType
                     ,@PersonaId AS PersonaId
                     ,OrgPartyID OrganizationPartyId
              FROM Security.ROLE R
              INNER JOIN Security.RoleType RT ON RT.RoleTypeId = R.RoleTypeID
              WHERE r.ProductId = @ProductId
                     AND R.RoleId = CONVERT(BIGINT,JSON_VALUE(@JSON, '$.RoleList[0]')) 
       END
END;

