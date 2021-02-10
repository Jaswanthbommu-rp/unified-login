CREATE PROCEDURE [Enterprise].[ListRolesForProductsByPartyId]
(@PartyId         INT,
 @ProductId       INT           = NULL,
 @TargetProductId PRODUCTIDTYPE READONLY
)
AS
     BEGIN
         IF
		(
			SELECT COUNT(*)
			FROM @TargetProductId
		) = 0
             BEGIN
                 SELECT 0 AS Id,
                        'Target ProductId list is empty.';
                 RETURN;
             END;
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
                             RVT.value,
                             RVT.ShortName 'RoleNickName',
                             r.RoleID [RoleId],
                             ST.Name AS RoleType,
                             r.DefaultRole AS 'DefaultRole',
                             COALESCE(ISNULL(
									(
										SELECT RoleValueTYpeId,
											   AttributeName,
											   AttributeValue
										FROM RoleAttribute
										WHERE RoleValueTYpeId = RVT.RoleValueTYpeId FOR JSON PATH
									), ''), '') AS RoleAttribute
                      FROM Enterprise.Organization AS o
                           INNER JOIN Enterprise.Role AS r ON o.PartyId = r.PartyID
                           INNER JOIN Enterprise.RoleValueType AS RVT ON RVT.RoleValueTypeId = r.RoleValueTypeId
                           INNER JOIN Enterprise.StatusType AS ST ON ST.StatusTypeId = RVT.StatusTypeId
                           INNER JOIN Enterprise.[Right] AS r2 ON r.RoleID = r2.RoleID
                           INNER JOIN Enterprise.UserActions AS ua ON r2.RightID = ua.RightID
                           INNER JOIN Enterprise.RightValueType RVTT ON RVTT.RightValueTypeId = R2.RightValueTypeId
                           INNER JOIN Enterprise.[Action] AS a ON ua.ActionID = a.ActionID
                      WHERE o.PartyId = @PartyId
                            AND (a.ProductId = @ProductId
                                 OR @ProductId IS NULL)
                            AND RVTT.TargetProductId IN
									(
										SELECT ProductId
										FROM @TargetProductId
									);
             END;
     END;