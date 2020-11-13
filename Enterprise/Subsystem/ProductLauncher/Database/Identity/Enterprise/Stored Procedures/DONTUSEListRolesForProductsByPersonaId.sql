CREATE PROCEDURE Enterprise.ListRolesForProductsByPersonaId (
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

       SELECT @PersonaRoleId = rr.RoleID FROM
							Enterprise.Action a
							INNER JOIN Enterprise.UserActions ua ON (a.ActionID = ua.ActionID)
							INNER JOIN Enterprise.[Right] r ON (r.RightID = ua.RightID)
							INNER JOIN Enterprise.Role rr ON (rr.RoleID = r.RoleID)
							INNER JOIN Enterprise.RoleValueType rvt ON (rvt.RoleValueTypeId = rr.RoleValueTYpeId)
							INNER JOIN Enterprise.StatusType st ON (st.StatustypeId = rvt.StatusTypeId)
							INNER JOIN Enterprise.Product p ON (a.ProductId = p.ProductId)
							INNER JOIN Enterprise.PersonaPrivilege pp ON (pp.RoleID = rr.RoleID)
							INNER JOIN Person.Persona pe ON (pe.PersonaId = pp.PersonaId)
							--INNER JOIN Ident.UserLoginPersona ULP ON ULP.UserLoginPersonaId = PE.UserLoginPersonaId
			WHERE	P.ProductId = @ProductId
              AND pe.PersonaId = @PersonaId

			  select @PersonaRoleId
			  
	IF(@PersonaRoleId IS NULL AND @ProductId = 26) --26 is for Unified Amenities
       BEGIN
              INSERT INTO @result_sp
              EXEC [Enterprise].[GetUserProductBatchJsonData] @ProductId,@PersonaId

              SELECT @json = results
              FROM @result_sp

              SELECT	DISTINCT
							rvt.Value 'Role',
							rvt.ShortName 'RoleNickName',
							rr.RoleID 'RoleId',
							p.ProductId 'Product',
							st.Name AS RoleType,
							pe.PersonaId,
							ULP.OrganizationPartyId
			FROM		Enterprise.Action a
							INNER JOIN Enterprise.UserActions ua ON (a.ActionID = ua.ActionID)
							INNER JOIN Enterprise.[Right] r ON (r.RightID = ua.RightID)
							INNER JOIN Enterprise.Role rr ON (rr.RoleID = r.RoleID)
							INNER JOIN Enterprise.RoleValueType rvt ON (rvt.RoleValueTypeId = rr.RoleValueTYpeId)
							INNER JOIN Enterprise.StatusType st ON (st.StatustypeId = rvt.StatusTypeId)
							INNER JOIN Enterprise.Product p ON (a.ProductId = p.ProductId)
							INNER JOIN Enterprise.PersonaPrivilege pp ON (pp.RoleID = rr.RoleID)
							INNER JOIN Person.Persona pe ON (pe.PersonaId = pp.PersonaId)
							INNER JOIN Ident.UserLoginPersona ULP ON ULP.UserLoginPersonaId = PE.UserLoginPersonaId
			WHERE	P.ProductId = @ProductId
                     AND R.RoleId = CONVERT(BIGINT,JSON_VALUE(@JSON, '$.RoleList[0]')) 
       END    
    ELSE
		BEGIN
			SELECT	DISTINCT
							rvt.Value 'Role',
							rvt.ShortName 'RoleNickName',
							rr.RoleID 'RoleId',
							p.ProductId 'Product',
							st.Name AS RoleType,
							pe.PersonaId,
							ULP.OrganizationPartyId
			FROM		Enterprise.Action a
							INNER JOIN Enterprise.UserActions ua ON (a.ActionID = ua.ActionID)
							INNER JOIN Enterprise.[Right] r ON (r.RightID = ua.RightID)
							INNER JOIN Enterprise.Role rr ON (rr.RoleID = r.RoleID)
							INNER JOIN Enterprise.RoleValueType rvt ON (rvt.RoleValueTypeId = rr.RoleValueTYpeId)
							INNER JOIN Enterprise.StatusType st ON (st.StatustypeId = rvt.StatusTypeId)
							INNER JOIN Enterprise.Product p ON (a.ProductId = p.ProductId)
							INNER JOIN Enterprise.PersonaPrivilege pp ON (pp.RoleID = rr.RoleID)
							INNER JOIN Person.Persona pe ON (pe.PersonaId = pp.PersonaId)
							INNER JOIN Ident.UserLoginPersona ULP ON ULP.UserLoginPersonaId = PE.UserLoginPersonaId
			WHERE	P.ProductId = @ProductId
			AND			((@PersonaId IS NULL) OR (pe.PersonaId = @PersonaId))
			AND			((@PartyId IS NULL) OR (ULP.OrganizationPartyId = @PartyId))
		END
END;