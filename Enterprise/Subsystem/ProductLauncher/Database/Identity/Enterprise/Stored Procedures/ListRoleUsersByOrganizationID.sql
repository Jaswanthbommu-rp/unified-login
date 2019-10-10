CREATE PROCEDURE [Enterprise].[ListRoleUsersByOrganizationID]
(@OrganizationID INT,
 @RoleId         INT
)
AS
     BEGIN
         SELECT ul.LoginName,
                rvt.Value AS Role,
                r.roleid AS RoleId,
                ST.name AS RoleType
         FROM Enterprise.Role AS r
	       INNER JOIN Enterprise.RoleValueType AS RVT ON RVT.RoleValueTypeId = R.RoleValueTYpeId
              INNER JOIN Enterprise.StatusType AS ST ON ST.StatustypeId = RVT.StatusTypeId
              INNER JOIN Enterprise.PersonaPrivilege AS pp ON r.RoleID = pp.RoleID
              INNER JOIN Person.Persona AS p ON pp.PersonaId = p.PersonaId
			  INNER JOIN Ident.UserLoginPersona ULP ON P.UserLoginPersonaId = ULP.UserLoginPersonaId
              INNER JOIN Ident.UserLogin AS ul ON UL.UserId = ULP.UserLoginId--p.UserId = ul.UserId
         WHERE r.PartyID = @OrganizationId
               AND r.RoleId = @Roleid;
     END;
