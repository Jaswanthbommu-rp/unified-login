CREATE PROCEDURE [Enterprise].[ListRolesHierarchy]
AS
     BEGIN
         SELECT RVTR.Value 'Role',
			 RVTR.Shortname 'RoleNickName',
                RVT.value 'Right',
			 RVT.ShortName 'RightNickName',
                ST.Name 'Visibility',
                A1.ObjectValue 'Action',
                A1.ObjectType 'ActionObjectType',
                P.Name 'Product'
         FROM Enterprise.ACTION A1
              INNER JOIN Enterprise.UserActions UA ON A1.ActionID = UA.ActionID
              INNER JOIN Enterprise.[Right] R ON R.RightID = UA.RightID
              INNER JOIN Enterprise.RightValueType RVT ON R.RightValueTypeId = RVT.RightValueTypeId
              INNER JOIN Enterprise.Role RR ON RR.RoleID = R.RoleID
              INNER JOIN Enterprise.RoleValueType RVTR ON RVTR.RoleValueTypeId = RR.RoleValueTypeId
              INNER JOIN Enterprise.Product P ON A1.ProductId = P.ProductId
              INNER JOIN Enterprise.StatusType ST ON UA.Status = ST.StatusTypeId
              INNER JOIN Enterprise.StatusTypeCategoryClassification STCC ON STCC.StatusTypeId = ST.StatusTypeId
              INNER JOIN Enterprise.StatusTypeCategory STC ON STC.StatusTypeCategoryId = STCC.StatusTypeCategoryId
              INNER JOIN Enterprise.StatusTypeCategoryType STCT ON STC.StatusTypeCategoryTypeId = STCT.StatusTypeCategoryTypeId
         WHERE A1.ParentActionId IS NULL
               AND STCT.Name = 'Security';
     END;