CREATE PROCEDURE [Security].[ListAllRights]
(
    @PartyID INT,
    @ProductId INT,
    @TargetProductId [Enterprise].[ProductIdType] READONLY
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
           R.RightID AS 'RightValueTypeId',
           R.Value AS 'Right',
           R.RightName AS 'RightNickName'
    FROM  Security.[Right] R          
		INNER JOIN Enterprise.StatusType ST  ON
            ST.StatusTypeId = R.VisibilityStatusId            
    WHERE R.ProductId = @ProductId
          AND R.TargetProductId IN (SELECT ProductId FROM @TargetProductId)  
		  AND ST.Name IN (SELECT * FROM @RightType)
		  AND R.RightId NOT IN  (Select RightId From Security.OrganizationOverRideRight Where OrgPartyId = @PartyID)
		  And ST.Name <> 'HIDDEN'
    ORDER BY R.RightId;

END;