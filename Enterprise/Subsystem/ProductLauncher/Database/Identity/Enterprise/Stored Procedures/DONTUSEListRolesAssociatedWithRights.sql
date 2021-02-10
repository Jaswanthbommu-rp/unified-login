CREATE PROCEDURE [Enterprise].[ListRolesAssociatedWithRights]
(
    @Partyid INT,
    @ProductId INT,
    @TargetProductId PRODUCTIDTYPE READONLY,
    @RoleId INT = NULL
)
AS
BEGIN
    IF
    (
        SELECT COUNT(*) FROM @TargetProductId
    ) = 0
    BEGIN
        SELECT 0 AS Id,
               'Target ProductId list is empty.';
        RETURN;
    END;
    SELECT DISTINCT
           RVT.Value [Role],
           RVT.ShortName [RoleNickName],
           r.RoleID [RoleId],
           rvtt.Value [Right],
           rvtt.ShortName [RightNickName],
           r2.RightID [RightId],
           rvtt.RightValueTypeId,
           ST.Name [RoleType],
           r.DefaultRole AS DefaultRole
    FROM Enterprise.Role r
        INNER JOIN Enterprise.RoleValueType AS RVT
            ON RVT.RoleValueTypeId = r.RoleValueTypeId
        INNER JOIN Enterprise.StatusType AS ST
            ON ST.StatusTypeId = RVT.StatusTypeId
        INNER JOIN Enterprise.[Right] r2
            ON r.RoleID = r2.RoleID
        INNER JOIN Enterprise.RightValueType rvtt
            ON rvtt.RightValueTypeId = r2.RightValueTypeId
        INNER JOIN Enterprise.UserActions UA
            ON UA.RightID = r2.RightID
        INNER JOIN Enterprise.Action A
            ON A.ActionID = UA.ActionID
        INNER JOIN Enterprise.StatusType STT
            ON UA.Status = STT.StatusTypeId
        INNER JOIN Enterprise.StatusTypeCategoryClassification STCC
            ON STCC.StatusTypeId = ST.StatusTypeId
        INNER JOIN Enterprise.StatusTypeCategory STC
            ON STC.StatusTypeCategoryId = STCC.StatusTypeCategoryId
        INNER JOIN Enterprise.StatusTypeCategoryType STCT
            ON STCT.StatusTypeCategoryTypeId = STC.StatusTypeCategoryTypeId
    WHERE r.PartyID = @Partyid
          AND rvtt.ProductId = @ProductId
          AND rvtt.TargetProductId IN
              (
                  SELECT ProductId FROM @TargetProductId
              )
          AND
          (
              r.RoleID = @RoleId
              OR @RoleId IS NULL
          )
          AND STT.Name <> 'HIDDEN'
          --AND rvtt.Value NOT LIKE 'Default%'
          AND r2.RoleID <> -1;
END;