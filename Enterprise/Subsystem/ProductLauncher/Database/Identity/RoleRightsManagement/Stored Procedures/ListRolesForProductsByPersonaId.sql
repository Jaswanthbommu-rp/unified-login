CREATE  PROCEDURE [Security].[ListRolesForProductsByPersonaId] (
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
	   DECLARE @roleID Int
	   DECLARE @roleName Varchar(256)
	   DECLARE @OrgPartyId BigInt

       SELECT @PersonaRoleId = SPR.RoleId
       FROM Security.PersonaRole SPR
       INNER JOIN Security.ROLE SR ON SR.RoleId = SPR.RoleId
       WHERE SR.ProductId = @ProductId
              AND SPR.PersonaId = @PersonaId

		Select @OrgPartyId = @PartyId
		IF (@PartyId IS NULL)
		BEGIN
			Select @OrgPartyId = up.OrganizationPartyId From Person.Persona P
			Join Ident.UserLoginPersona UP ON
				p.UserLoginPersonaId = UP.UserLoginPersonaId
			Where p.PersonaId = @PersonaId
		END
			  
	IF(@PersonaRoleId IS NULL AND @ProductId IN (57)) --26 is for Unified Amenities,57 is IB
       BEGIN
              INSERT INTO @result_sp
              EXEC [Enterprise].[GetUserProductBatchJsonData] @ProductId,@PersonaId

              SELECT @json = results
              FROM @result_sp

			  SELECT @roleID = CONVERT(BIGINT,JSON_VALUE(@JSON, '$.RoleList[0]'))

			  IF (@roleID > 0 AND NOT EXISTS (SELECT 1 From Security.Role WHERE RoleId = @roleID AND ProductId = @ProductId))
			  BEGIN
				Select @roleName = RV.Value
				From Enterprise.Role R
				JOIN Enterprise.RoleValueType RV ON
					R.RoleValueTypeId = r.RoleValueTypeId
				Where R.RoleID = @roleID
				And R.PartyID = @OrgPartyId

				Select @roleID = RoleId From Security.Role Where RoleName = @roleName AND ProductId = @ProductId
			  END

              SELECT DISTINCT 
					  R.RoleName 'Role'
                     ,R.ShortName 'RoleNickName'
                     ,@roleID 'RoleId'
                     ,r.ProductId 'Product'
                     ,RT.Value AS RoleType
                     ,@PersonaId AS PersonaId
                     ,OrgPartyID OrganizationPartyId
              FROM  Security.Role R
              INNER JOIN Security.RoleType RT ON RT.RoleTypeId = R.RoleTypeID
              WHERE r.ProductId = @ProductId
                     AND R.RoleId = @roleID
       END    
    ELSE IF (@PersonaId Is NULL)
		BEGIN
			  SELECT DISTINCT
						R.RoleName 'Role',
						R.ShortName 'RoleNickName',
						R.RoleID 'RoleId',
						p.ProductId 'Product',
						RT.Value AS RoleType,
						pe.PersonaId,
						ULP.OrganizationPartyId
			  FROM	Security.Role R
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
		END
		Else 
		Begin
		 SELECT DISTINCT
						R.RoleName 'Role',
						R.ShortName 'RoleNickName',
						R.RoleID 'RoleId',
						p.ProductId 'Product',
						RT.Value AS RoleType,
						pe.PersonaId,
						ULP.OrganizationPartyId
			  FROM	Security.Role R
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
			     AND pe.PersonaId = @PersonaId
		END
END;