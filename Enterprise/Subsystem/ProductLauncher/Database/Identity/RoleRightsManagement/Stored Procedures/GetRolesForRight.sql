CREATE PROCEDURE [Security].[GetRolesForRight]
    @RightId int
AS
    BEGIN
		SELECT RR.RoleId,
		RL.RoleName,
		RL.[Description],
		RL.ProductId,
		RL.RoleTypeID,
		RL.OrgPartyID,
		RL.ShortName,
		RL.CreatedBy
		FROM [Security].[RoleRight] RR
		INNER JOIN [Security].[Role] RL ON RL.RoleId = RR.RoleId
		WHERE RR.RightId = @RightId
    END;
