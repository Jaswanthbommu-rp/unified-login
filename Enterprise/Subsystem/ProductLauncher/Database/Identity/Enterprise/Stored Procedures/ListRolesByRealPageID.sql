CREATE PROCEDURE Enterprise.ListRolesByRealPageID(@RealPageID UNIQUEIDENTIFIER)
AS
     BEGIN
         SELECT RVT.Value Role,
			 RVT.ShortName 'RoleNickName',
                R.RoleID,
                RT.PartyRoleTypeID,
                RT.ParentPartyRoleTypeId
         FROM Enterprise.Role R
              INNER JOIN Enterprise.RoleValueType RVT ON RVT.RoleValueTypeId = R.RoleValueTypeId
              INNER JOIN Enterprise.Party P ON R.PartyID = P.PartyId
              INNER JOIN Enterprise.RoleType RT ON R.RoleTypeID = RT.PartyRoleTypeId
         WHERE P.RealPageId = @RealPageID;
     END;
GO