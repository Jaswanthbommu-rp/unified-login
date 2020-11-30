CREATE PROCEDURE [Enterprise].[ListRightForProductsByPartyId]
(@PartyId   INT,
 @ProductId INT = NULL
)
AS
     BEGIN
         IF(@ProductId IS NOT NULL)
           OR (@ProductId = '')
             BEGIN
                 SELECT DISTINCT
                        rvtt.value,
                        rvtt.RightValueTypeId [RightValueTypeId],
						rvtt.ShortName AS 'RightNickName',
                        ST.Name AS RoleType
                 FROM Enterprise.Organization AS o
                      INNER JOIN Enterprise.Role AS r ON o.PartyId = r.PartyID
                      INNER JOIN Enterprise.RoleValueType AS RVT ON RVT.RoleValueTypeId = R.RoleValueTYpeId
                      INNER JOIN Enterprise.StatusType AS ST ON ST.StatustypeId = RVT.StatusTypeId
                      INNER JOIN Enterprise.[Right] AS r2 ON r.RoleID = r2.RoleID
                      INNER JOIN Enterprise.RightValueType RVTT ON R2.RightValueTypeId = RVTT.RightValueTypeId
                      INNER JOIN Enterprise.UserActions AS ua ON r2.RightID = ua.RightID
                      INNER JOIN Enterprise.[Action] AS a ON ua.ActionID = a.ActionID
                 WHERE o.partyid = @PartyId
                       AND a.ProductId = @ProductId;
         END;
         IF(@ProductId IS NULL)
           OR (@ProductId = '')
             BEGIN
                 SELECT DISTINCT
                        rvtt.value,
                        rvtt.RightValueTypeId [RightValueTypeId],
                        ST.Name AS RoleType
                 FROM Enterprise.Organization AS o
                      INNER JOIN Enterprise.Role AS r ON o.PartyId = r.PartyID
                      INNER JOIN Enterprise.RoleValueType AS RVT ON RVT.RoleValueTypeId = R.RoleValueTYpeId
                      INNER JOIN Enterprise.StatusType AS ST ON ST.StatustypeId = RVT.StatusTypeId
                      INNER JOIN Enterprise.[Right] AS r2 ON r.RoleID = r2.RoleID
                      INNER JOIN Enterprise.RightValueType RVTT ON R2.RightValueTypeId = RVTT.RightValueTypeId
                      INNER JOIN Enterprise.UserActions AS ua ON r2.RightID = ua.RightID
                      INNER JOIN Enterprise.[Action] AS a ON ua.ActionID = a.ActionID
                 WHERE o.partyid = @PartyId;
         END;
     END;