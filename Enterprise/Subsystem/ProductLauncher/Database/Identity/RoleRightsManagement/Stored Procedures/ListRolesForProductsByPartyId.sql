CREATE PROCEDURE [Security].[ListRolesForProductsByPartyId]
(@PartyId         INT,
 @ProductId       INT           = NULL,
 @TargetProductId [Enterprise].[ProductIdType] READONLY
)
AS
     BEGIN
        IF (SELECT COUNT(*)	FROM @TargetProductId) = 0
        BEGIN
            SELECT 0 AS Id,
                'Target ProductId list is empty.';
            RETURN;
        END;
		 
		 Declare @OrgDefaultRole AS TABLE (
			RoleId int,
			DefaultRole bit default 0);

		Insert Into @OrgDefaultRole(RoleId,DefaultRole)
		Select RoleId,1 From Security.OrganizationDefaultRole
		Where OrgPartyId = @PartyId

         IF @ProductId IS NOT NULL
             BEGIN
                      SELECT DISTINCT
                             R.RoleName 'value',
                             R.ShortName 'RoleNickName',
                             R.RoleID [RoleId],
                             RT.Value AS RoleType,
                             ISNULL(OD.DefaultRole, 0) AS 'DefaultRole',
                             '' AS RoleAttribute
                      FROM Enterprise.Organization AS O
                           INNER JOIN Security.Role AS R ON 
							O.PartyId = R.OrgPartyID
                           INNER JOIN Security.RoleType AS RT ON 
							RT.RoleTypeId = R.RoleTypeID                          
						   LEFT JOIN @OrgDefaultRole AS OD ON
							OD.RoleId = R.RoleId
                      WHERE O.PartyId = @PartyId
                            AND (R.ProductId = @ProductId
                                 OR @ProductId IS NULL)
                            AND R.RoleID NOT IN (SELECT ORR.RoleId FROM Security.OrganizationOverRideRole ORR Where OrgPartyID = @PartyId)
					 UNION
					 SELECT 
                             R.RoleName 'value',
                             R.ShortName 'RoleNickName',
                             R.RoleID [RoleId],
                             RT.Value AS RoleType,
                            ISNULL(OD.DefaultRole, 0) AS 'DefaultRole',
							'' AS RoleAttribute
					 FROM   Security.Role AS R 
                           INNER JOIN Security.RoleType AS RT ON
								RT.RoleTypeId = R.RoleTypeID
						   Left JOIN @OrgDefaultRole AS OD ON
							OD.RoleId = R.RoleId
					 WHERE R.OrgPartyID IS NULL
					 AND R.ProductId = @ProductId 
                     AND R.RoleID NOT IN (SELECT ORR.RoleId FROM Security.OrganizationOverRideRole ORR Where OrgPartyID = @PartyId)
             END;
     END;