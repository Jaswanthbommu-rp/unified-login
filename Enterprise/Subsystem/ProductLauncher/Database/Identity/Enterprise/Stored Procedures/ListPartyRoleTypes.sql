CREATE PROCEDURE Enterprise.ListPartyRoleTypes
    @PartyRoleTypeId AS INT = 100
AS
    BEGIN
        WITH    Subs
                  AS (
					   -- Anchor member that returns the root node
                       SELECT   PartyRoleTypeId ,
                                Name ,
                                0 AS lvl
                       FROM     Enterprise.RoleType
                       WHERE    PartyRoleTypeId = @PartyRoleTypeId
                       UNION ALL
					   -- Recursive member returns next level of children
                       SELECT   C.PartyRoleTypeId ,
                                C.Name ,
                                P.lvl + 1
                       FROM     Subs AS P
                                JOIN Enterprise.RoleType AS C ON C.ParentPartyRoleTypeId = P.PartyRoleTypeId
                     )
            SELECT  Subs.PartyRoleTypeId ,
                    Subs.Name ,
                    Subs.lvl
            FROM    Subs;    
    END;

