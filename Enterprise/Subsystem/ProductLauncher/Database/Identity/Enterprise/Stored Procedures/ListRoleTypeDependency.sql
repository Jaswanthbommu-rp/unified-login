
CREATE PROCEDURE [Enterprise].[ListRoleTypeDependency]
(@PartyId          INT,
 @ParentRoleTypeID INT
)
AS
     BEGIN
         DECLARE @RoleTypeName NVARCHAR(100)= 'User Role';
         
		SELECT  DISTINCT RT1.Name AS 'Name'
               , RD.ChildRoleTypeId 'PartyRoleTypeId'
				, RD.SortOrder AS 'SortOrder'
         FROM enterprise.roletype rt
              INNER JOIN  enterprise.roletype rt1 ON rt1.Parentpartyroletypeid = rt.partyroletypeid
              INNER JOIN  enterprise.partyrole pr ON pr.roletypeid = rt1.PartyRoleTypeId
              INNER JOIN  Enterprise.RoleTypeDependency RD ON RD.ChildRoleTypeId = PR.RoleTypeId
         WHERE ( RT.Name = @RoleTypeName OR @RoleTypeName IS NULL)
              AND  (pr.PartyId = @PartyId OR @PartyId is NULL)
              AND RD.ParentRoleTypeId =CASE WHEN @ParentRoleTypeId IS NULL
							THEN 403
                   ELSE   @ParentRoleTypeId
				   END
			ORDER BY RD.SortOrder
			 
	
     END;
