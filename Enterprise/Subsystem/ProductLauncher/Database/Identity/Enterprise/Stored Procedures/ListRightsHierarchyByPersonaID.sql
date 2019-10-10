CREATE PROCEDURE [Enterprise].[ListRightsHierarchyByPersonaID](@PersonaId BIGINT)
AS
     BEGIN
         SELECT PR.FirstName,
                PR.LastName,
                UL.LoginName,
                RLVT.Value 'Role',
			 RLVT.ShortName 'RoleNickName', 
                RVT.value 'Right',
			 RVT.ShortName 'RightNickName',
                ST.Name 'Visibility',
                A1.ObjectValue 'Action',
                A1.ObjectType 'ActionObjectType',
                P.Name 'Product'
         FROM Enterprise.ACTION A1
              INNER JOIN Enterprise.UserActions UA ON A1.ActionID = UA.ActionID
              INNER JOIN Enterprise.[Right] R ON R.RightID = UA.RightID
              INNER JOIN Enterprise.RightValueType RVT ON RVT.RightValueTypeId = R.RightValueTypeId
              INNER JOIN Enterprise.Role RR ON RR.RoleID = R.RoleID
              INNER JOIN Enterprise.RoleValueType RLVT ON RLVT.RoleValueTypeId = RR.RoleValueTypeId
              INNER JOIN Enterprise.Product P ON A1.ProductId = P.ProductId
              INNER JOIN Enterprise.PersonaPrivilege PP ON PP.RoleID = RR.RoleID
              INNER JOIN Person.Persona PE ON PE.PersonaId = PP.PersonaId
              INNER JOIN Enterprise.PersonaIdentityUserLogin PU ON PU.PersonaID = PE.PersonaId
              INNER JOIN Ident.UserLogin UL ON UL.UserId = PU.UserID
              INNER JOIN Person.Person PR ON PR.PartyId = UL.PersonPartyId
              INNER JOIN Enterprise.StatusType ST ON UA.Status = ST.StatusTypeId
              INNER JOIN Enterprise.StatusTypeCategoryClassification STCC ON STCC.StatusTypeId = ST.StatusTypeId
              INNER JOIN Enterprise.StatusTypeCategory STC ON STC.StatusTypeCategoryId = STCC.StatusTypeCategoryId
              INNER JOIN Enterprise.StatusTypeCategoryType STCT ON STC.StatusTypeCategoryTypeId = STCT.StatusTypeCategoryTypeId
         WHERE A1.ParentActionId IS NULL
               AND STCT.Name = 'Security'
               AND PE.PersonaId = @PersonaId;
     END;