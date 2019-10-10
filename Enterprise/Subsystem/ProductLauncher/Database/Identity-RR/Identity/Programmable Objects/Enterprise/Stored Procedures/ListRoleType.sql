IF OBJECT_ID('[Enterprise].[ListRoleType]') IS NOT NULL
	DROP PROCEDURE [Enterprise].[ListRoleType];

GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_NULLS ON
GO
CREATE PROCEDURE [Enterprise].[ListRoleType]
	@RoleTypeName varchar(50) = NULL
AS
BEGIN
	SELECT	rt.PartyRoleTypeId,
			rt.ParentPartyRoleTypeId,
			rt.Name
	FROM	Enterprise.RoleType rt
			LEFT OUTER JOIN Enterprise.RoleType prt ON (rt.ParentPartyRoleTypeId = prt.PartyRoleTypeId)
	WHERE	(@RoleTypeName IS NULL OR prt.Name = @RoleTypeName)
END
GO
