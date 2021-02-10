CREATE PROCEDURE [Enterprise].[ListRolesByParty]
(@PartyID INT, 
 @TargetProductId PRODUCTIDTYPE READONLY )
AS
BEGIN
    /*
	SELECT R.Value, R.RoleID, RT.PartyRoleTypeID, RT.ParentPartyRoleTypeId
	FROM Enterprise.Role R
		INNER JOIN Enterprise.Party P
			ON R.PartyID = P.PartyId
		INNER JOIN Enterprise.RoleType RT
			On R.RoleTypeID = RT.PartyRoleTypeId
		WHERE RT.Name = 'System User'
    UNION 
    */
    IF
    (
        SELECT COUNT(*) FROM @TargetProductId
    ) = 0
    BEGIN
        SELECT 0 AS Id,
               'Target ProductId list is empty.';
        RETURN;
    END;
	SELECT DISTINCT RVT.Value Role,
           RVT.ShortName RoleNickname,
           R.RoleID,
           RT.PartyRoleTypeId,
           RT.ParentPartyRoleTypeId
    FROM Enterprise.Role R
	INNER JOIN Enterprise.RoleValueType AS RVT
            ON RVT.RoleValueTypeId = R.RoleValueTypeId
	INNER JOIN Enterprise.[Right] RGT
			ON RGT.RoleId = R.RoleID
		INNER JOIN Enterprise.RightValueType RVTT
			ON RVTT.RightValueTypeId = RGT.RightValueTypeId
        INNER JOIN Enterprise.Party P
            ON R.PartyID = P.PartyId
        INNER JOIN Enterprise.RoleType RT
            ON R.RoleTypeID = RT.PartyRoleTypeId
    WHERE P.PartyId = @PartyID
	 AND RVTT.TargetProductId IN
              (
                  SELECT ProductId FROM @TargetProductId
              )
END;