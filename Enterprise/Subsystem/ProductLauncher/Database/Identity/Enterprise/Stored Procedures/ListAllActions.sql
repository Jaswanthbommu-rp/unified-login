CREATE PROCEDURE [Enterprise].[ListAllActions](@Depth NVARCHAR(20) = 'ROOT')
AS
     BEGIN
         DECLARE @Level INT;
         SET @Level = 1;
         WITH ActionsCTE
              AS (
              SELECT A1.ActionID,
                     RVT.value,
                     R.RightID,
                     A1.ProductId,
                     ObjectType 'RootLevel',
                     ObjectType 'ChildLevel',
                     ObjectValue,
                     ParentActionId,
                     1 AS 'Level'
              FROM Enterprise.ACTION A1
                   INNER JOIN Enterprise.UserActions UA ON A1.ActionID = UA.ActionID
                   INNER JOIN Enterprise.[Right] R ON R.RightID = UA.RightID
                   INNER JOIN Enterprise.[RightValueType] RVT ON R.RightValueTypeId = RVT.RightValueTypeId
              WHERE ParentActionId IS NULL
              UNION ALL
              SELECT A2.ActionID,
                     RVT.Value,
                     R.RightID,
                     A2.ProductId,
                     A1.RootLevel,
                     A2.ObjectType,
                     A2.ObjectValue,
                     A2.ParentActionId,
                     @Level + 1
              FROM ActionsCTE A1
                   INNER JOIN Enterprise.ACTION A2 ON A2.ParentActionId = A1.ActionID
                   INNER JOIN Enterprise.UserActions UA ON A1.ActionID = UA.ActionID
                   INNER JOIN Enterprise.[Right] R ON UA.RightID = R.RightID
                   INNER JOIN Enterprise.[RightValueType] RVT ON R.RightValueTypeId = RVT.RightValueTypeId
              WHERE A2.ParentActionId IS NOT NULL)
              SELECT ACT.ActionID 'ActionId',
                     ACT.Value 'RightName',
                     Act.RightID,
                     ACT.ProductId,
                     ACT.RootLevel,
                     ACT.ChildLevel,
                     ACT.ParentActionId,
                     ACT.Level
              FROM ActionsCTE ACT
              WHERE ISNULL(ParentActionId, '') = CASE
                                                     WHEN @Depth = 'ROOT'
                                                     THEN ISNULL(NULL, '')
                                                     WHEN @Depth = 'ALL'
                                                     THEN ISNULL(ParentActionId, '')
                                                 END
              ORDER BY ACT.ActionID;
     END;