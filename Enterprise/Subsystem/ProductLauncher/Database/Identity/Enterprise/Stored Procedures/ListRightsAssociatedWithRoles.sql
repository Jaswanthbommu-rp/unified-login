CREATE PROCEDURE [Enterprise].[ListRightsAssociatedWithRoles]
(
    @PartyId INT,
    @ProductId INT,
    @TargetProductId PRODUCTIDTYPE READONLY,
    @RightId INT = NULL
)
AS
BEGIN

    /*DO NOT CHANGE THE SEQUENCE*/
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
           r2.RoleID [RoleId],
           RVT.Value [Role],
           RVT.ShortName AS [RoleNickname],
           ST.Name [RoleType],
		   r2.DefaultRole AS 'DefaultRole',
           r.RightID [RightId],
           rvtt.Value [Right],
           rvtt.ShortName [RightNickName],
           rvtt.RightValueTypeId [RightValueTypeId]
    FROM Enterprise.[Right] r
        INNER JOIN Enterprise.RightValueType rvtt
            ON rvtt.RightValueTypeId = r.RightValueTypeId
        INNER JOIN Enterprise.Role r2
            ON r.RoleID = r2.RoleID
        INNER JOIN Enterprise.RoleValueType AS RVT
            ON RVT.RoleValueTypeId = r2.RoleValueTypeId
        INNER JOIN Enterprise.StatusType AS ST
            ON ST.StatusTypeId = RVT.StatusTypeId
        INNER JOIN Enterprise.UserActions UA
            ON UA.RightID = r.RightID
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
    WHERE r2.PartyID = @PartyId
          AND rvtt.ProductId = @ProductId
          AND rvtt.TargetProductId IN
              (
                  SELECT ProductId FROM @TargetProductId
              )
          AND
          (
              r.RightID = @RightId
              OR @RightId IS NULL
          )
          AND STT.Name <> 'HIDDEN'
          --AND rvtt.Value NOT LIKE 'Default%'
          AND r2.RoleID <> -1;
END;