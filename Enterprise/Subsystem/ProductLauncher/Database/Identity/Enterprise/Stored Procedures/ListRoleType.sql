CREATE PROCEDURE [Enterprise].[ListRoleType] @RoleTypeName        VARCHAR(50) = NULL,
                                             @OrganizationPartyID BIGINT      = NULL
AS
         BEGIN
             SELECT DISTINCT
                    rt1.PartyRoleTypeId,
                    rt1.ParentPartyRoleTypeId,
                    rt1.name
             FROM enterprise.roletype rt
                  INNER JOIN enterprise.roletype rt1 ON rt1.Parentpartyroletypeid = rt.partyroletypeid
                  LEFT OUTER JOIN enterprise.partyrole pr ON pr.roletypeid = rt1.PartyRoleTypeId
             WHERE(@RoleTypeName IS NULL
                   OR RT.Name = @RoleTypeName)
                  AND (@OrganizationPartyID IS NULL
                       OR pr.PartyId = @OrganizationPartyID);
         END;


