CREATE PROCEDURE [Enterprise].[ListRightsByPersonaID](@PersonaID BIGINT)
AS
         BEGIN
             IF EXISTS
(
    SELECT 1
    FROM Enterprise.Role R
         INNER JOIN Enterprise.PersonaPrivilege PP ON R.RoleID = PP.RoleID
    WHERE PersonaId = @PersonaID
)
                 BEGIN
                     SELECT RT.RightID,
                            RVT.value,
					   RVT.ShortName as RightNickName
                     FROM Enterprise.PersonaPrivilege PP
                          INNER JOIN Enterprise.Role R ON R.RoleID = PP.RoleID
                          INNER JOIN Enterprise.[Right] RT ON RT.RoleId = R.RoleId
                          INNER JOIN Enterprise.RightValueType RVT ON RT.RightValueTypeID = RVT.RightValueTypeId
                     WHERE PersonaId = @PersonaID;
                 END;
                 ELSE
                 BEGIN
                     SELECT 'No Roles assinged to the persona. Please assign roles to get the list of rights.';
                 END;
         END;