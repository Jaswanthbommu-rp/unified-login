CREATE   PROCEDURE [Enterprise].[ListRoleType_Ver01] (
	@ParentRoleTypeName varchar(50) = NULL,
	@RoleTypeName varchar(50) = NULL,
	@OrganizationPartyID bigint = NULL
)
AS
BEGIN
	SELECT DISTINCT
				rt1.PartyRoleTypeId,
				rt1.ParentPartyRoleTypeId,
				rt1.name
	FROM	enterprise.roletype rt
				INNER JOIN enterprise.roletype rt1 ON (rt1.Parentpartyroletypeid = rt.partyroletypeid)
				LEFT OUTER JOIN enterprise.partyrole pr ON pr.roletypeid = rt1.PartyRoleTypeId
	WHERE	(@ParentRoleTypeName IS NULL OR RT.Name = @ParentRoleTypeName)
	AND		(@RoleTypeName IS NULL OR RT1.Name = @RoleTypeName)
	AND		(@OrganizationPartyID IS NULL OR pr.PartyId = @OrganizationPartyID);
END;