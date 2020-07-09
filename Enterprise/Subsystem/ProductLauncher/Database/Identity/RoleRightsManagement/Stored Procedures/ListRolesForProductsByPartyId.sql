CREATE PROCEDURE [Security].[ListRolesForProductsByPartyId]
(@PartyId         INT,
 @ProductId       INT           = NULL,
 @TargetProductId PRODUCTIDTYPE READONLY
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
                 WITH RoleAttribute
                      AS (
                      SELECT RVT.RoleValueTypeId,
                             RTT.Name AS 'AttributeName',
                             RT.Value AS 'AttributeValue'
                      FROM Enterprise.RoleValueType RVT
                           INNER JOIN Enterprise.ROleAttribute RT ON RVT.RoleValueTYpeId = RT.RoleValueTYpeId
                           INNER JOIN Enterprise.ROleAttributeType RTT ON RT.ROleAttributeTypeId = RTT.ROleAttributeTypeID)
                      SELECT DISTINCT
                             R.RoleName,
                             R.ShortName 'RoleNickName',
                             R.RoleID [RoleId],
                             RT.Value AS RoleType,
                             ISNULL(OD.DefaultRole, 0) AS 'DefaultRole',
                             COALESCE(ISNULL(
									(
										SELECT RoleValueTYpeId,
											   AttributeName,
											   AttributeValue
										FROM RoleAttribute
										WHERE RoleValueTYpeId = R.RoleId FOR JSON PATH
									), ''), '') AS RoleAttribute
                      FROM Enterprise.Organization AS O
                           INNER JOIN Security.Role AS R ON 
							O.PartyId = R.OrgPartyID
                           INNER JOIN Security.RoleType AS RT ON 
							RT.RoleTypeId = R.RoleTypeID
                           INNER JOIN Security.[RoleRight] AS RR ON 
							R.RoleID = RR.RoleID
                           INNER JOIN Security.[Right] AS RG ON 
							RG.RightID = RR.RightID  
						   LEFT JOIN @OrgDefaultRole AS OD ON
							OD.RoleId = R.RoleId
                      WHERE O.PartyId = @PartyId
                            AND (RG.ProductId = @ProductId
                                 OR @ProductId IS NULL)
                            AND RG.TargetProductId IN (SELECT ProductId	FROM @TargetProductId)
					 UNION
					 SELECT 
                             R.RoleName,
                             R.ShortName 'RoleNickName',
                             R.RoleID [RoleId],
                             RT.Value AS RoleType,
                            ISNULL(OD.DefaultRole, 0) AS 'DefaultRole',
							COALESCE(ISNULL(
									(
										SELECT RoleValueTYpeId,
											   AttributeName,
											   AttributeValue
										FROM RoleAttribute
										WHERE RoleValueTYpeId = R.RoleId FOR JSON PATH
									), ''), '') AS RoleAttribute
					 FROM   Security.Role AS R 
                           INNER JOIN Security.RoleType AS RT ON
								RT.RoleTypeId = R.RoleTypeID
						   Left JOIN @OrgDefaultRole AS OD ON
							OD.RoleId = R.RoleId
					 WHERE R.OrgPartyID IS NULL
             END;
     END;