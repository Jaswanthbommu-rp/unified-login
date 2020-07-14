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
     Declare @VisibleStatusId INT
   Select @VisibleStatusId = StatusTypeId
   From Enterprise.StatusType Where Name = 'All'

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
           R.RightID AS 'RightValueTypeId',
           R.Value AS 'Right',
           R.RightName AS 'RightNickName'
    FROM  Security.[Right] R          
	Left outer join [Security].OrganizationOverRideRight ORR ON
		ORR.RightId = R.RightId
		And ORR.OrgPartyId = @PartyID	         
    WHERE R.ProductId = @ProductId
          AND R.TargetProductId IN (SELECT ProductId FROM @TargetProductId)  
		  AND (R.VisibilityStatusId = 9 OR  ORR.VisibilityStatusId = 9)
    ORDER BY R.RightId;

END;