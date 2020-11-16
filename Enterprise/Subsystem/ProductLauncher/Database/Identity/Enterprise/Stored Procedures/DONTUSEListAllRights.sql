CREATE PROCEDURE [Enterprise].[ListAllRights]
(
    @PartyID INT,
    @ProductId INT,
    @TargetProductId PRODUCTIDTYPE READONLY
)
AS
BEGIN
    --Logic may change in future based on rights associated with organization or party
    --Output should not change
    DECLARE @RightType TABLE
    (
        RightType NVARCHAR(50)
    );
    IF
    (
        SELECT COUNT(*) FROM @TargetProductId
    ) = 0
    BEGIN
        SELECT 0 AS Id,
               'Target ProductId list is empty.';
        RETURN;
    END;
    IF EXISTS
    (
        SELECT 1 FROM Enterprise.Organization WHERE PartyId = @PartyID
    AND Name IN ( 'RealPage', 'RealPage Employee' ))
    BEGIN
        INSERT INTO @RightType
        SELECT 'ALL'
        UNION
        SELECT 'Internal Only';
    END;
    ELSE
    BEGIN
        INSERT INTO @RightType
        SELECT 'ALL';
    END;

    SELECT DISTINCT
           RVT.RightValueTypeId,
           RVT.Value AS 'Right',
           RVT.ShortName AS 'RightNickName'
    FROM Enterprise.RightValueType RVT
        RIGHT OUTER JOIN Enterprise.[Right] R
            ON R.RightValueTypeId = RVT.RightValueTypeId
               AND RVT.Value NOT LIKE 'Default%'
        INNER JOIN Enterprise.StatusType ST
            ON ST.StatusTypeId = RVT.VisibilityStatusId
               AND ST.Name IN
                   (
                       SELECT * FROM @RightType
                   )
        INNER JOIN Enterprise.StatusType STP
            ON STP.Name IN (SELECT * FROM @RightType)
    WHERE RVT.ProductId = @ProductId
          AND RVT.TargetProductId IN
              (
                  SELECT ProductId FROM @TargetProductId
              )
          AND R.PartyId = @PartyID
    ORDER BY RVT.RightValueTypeId;


END;