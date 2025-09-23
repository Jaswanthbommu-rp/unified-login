CREATE PROCEDURE [Enterprise].[ListPersonaRightsAndActionsByRoute]
( 
				@PersonaID bigint, @RouteID nvarchar(50)
)
AS
BEGIN
	DECLARE @OrganizationId int;
	DECLARE @RoleTypeState bit= 0;
	DECLARE @ActionValueType TABLE(Id INT);
	DECLARE @OrganizationName NVARCHAR(200) = 'RealPage Employee';
	DECLARE @PlatformAdminRoleValue NVARCHAR(200);
	DECLARE @NOW DATETIME = GETUTCDATE();
	SELECT @PlatformAdminRoleValue = ps.Value
	FROM Enterprise.GlobalProductConfiguration gpc
	JOIN Enterprise.ProductConfiguration pc ON pc.ConfigurationId = gpc.ConfigurationId
	JOIN Enterprise.ProductSetting ps ON ps.ProductSettingId = pc.ProductSettingId
	JOIN Enterprise.ProductSettingType pst ON pst.ProductSettingTypeId = ps.ProductSettingTypeId
	WHERE gpc.ProductId = 3
	 AND ((@NOW BETWEEN gpc.FromDate AND gpc.ThruDate) OR (@NOW >= gpc.FromDate AND gpc.ThruDate IS NULL))
	 AND ((@NOW BETWEEN pc.FromDate AND pc.ThruDate) OR (@NOW >= pc.FromDate AND pc.ThruDate IS NULL))
	 AND ((@NOW BETWEEN ps.FromDate AND ps.ThruDate) OR (@NOW >= ps.FromDate AND ps.ThruDate IS NULL))
	 AND pst.Name = 'PlatformAdminRole';
	CREATE TABLE #HoldRoutes
	(ActionId          INT,
	 ObjectType        VARCHAR(200),
	 ObjectValue       VARCHAR(200),
	 ParentActionId    INT,
	 ActionValueTypeId INT
	);

	CREATE TABLE #HoldRights
	(ActionId          INT,
	 ObjectType        VARCHAR(200),
	 ObjectValue       VARCHAR(200),
	 ParentActionId    INT,
	 ActionValueTypeId INT
	);
	IF EXISTS
	(
		SELECT 1
			FROM Enterprise.RoleValueType RVT
			INNER JOIN Enterprise.Role R 
			ON R.RoleValueTypeId = RVT.RoleValueTypeId
			INNER JOIN Enterprise.PersonaPrivilege  PR
				ON R.RoleId = PR.RoleId
		WHERE PR.PersonaId = @PersonaId AND 
				RVT.value  =  @PlatformAdminRoleValue
	)
	BEGIN
		SET @RoleTypeState = 1;
	END;
	SELECT @OrganizationId = OrganizationPartyID
	FROM Person.Persona PE
	INNER JOIN Ident.UserLoginPersona ULP ON ULP.UserLoginPersonaId = PE.UserLoginPersonaId
	WHERE PersonaId = @PersonaId;
	
	IF(@RoleTypeState = 0)
	BEGIN
		with holddependencies (RightValueTypeId, DependentRightValueTypeId)
		AS (	SELECT RD.RightValueTypeId, RD.DependentRightValueTypeId
			--INTO #HoldDependencies
			FROM Enterprise.PersonaPrivilege AS PP
				 INNER JOIN
				 Enterprise.Role AS R
				 ON R.RoleId = PP.RoleId AND R.PartyID = @OrganizationId
				 INNER JOIN
				 Enterprise.[Right] AS RT
				 ON RT.RoleId = R.RoleId
				 INNER JOIN
				 Enterprise.RightValueTYpe AS RVT
				 ON RVT.RightValueTypeId = RT.RightValueTypeId
				 INNER JOIN
				 Enterprise.RightDependency AS RD
				 ON RD.RightValueTypeId = RVT.RightValueTypeId
			WHERE pp.PersonaId = @Personaid)

		, ActionsCTE
			 AS (
			 SELECT A1.ActionID, RLVT.Value AS 'RoleName', RVT.value AS 'RightName', RL.RoleId, R.RightID, A1.ProductId, ObjectValue AS 'Action', ObjectType AS 'ObjectType', ParentActionId, A1.ActionvalueTypeId, 1 AS 'Level'
			 FROM Enterprise.ACTION AS A1
				  INNER JOIN
				  Enterprise.UserActions AS UA
				  ON A1.ActionID = UA.ActionID
				  INNER JOIN
				  Enterprise.[Right] AS R
				  ON R.RightID = UA.RightID
				  INNER JOIN
				  Enterprise.RightValueType AS RVT
				  ON RVT.RightValueTypeId = R.RightValueTypeId
				  INNER JOIN
				  Enterprise.Role AS RL
				  ON RL.RoleID = R.RoleId AND RL.PartyID = @OrganizationId
				  INNER JOIN
				  Enterprise.RoleValueType AS RLVT
				  ON RLVT.RoleValueTypeId = RL.RoleValueTypeId
				  INNER JOIN
				  Enterprise.ActionValueType AS AVT
				  ON A1.ActionvalueTypeId = AVT.ActionValueTypeID
			 WHERE ParentActionId IS NULL AND 
				   AVT.Value = 'Route' AND 
				   ObjectValue = @RouteID
				   AND RL.PartyId = @OrganizationId
			 UNION ALL
			 SELECT A2.ActionID, RLVT.Value, RVT.Value, RL.RoleId, R.RightID, A2.ProductId, A2.ObjectValue, A2.ObjectType, A2.ParentActionId, A2.ActionvalueTypeId, A1.Level + 1
			 FROM ActionsCTE AS A1
				  INNER JOIN
				  Enterprise.ACTION AS A2
				  ON A2.ParentActionId = A1.ActionID
				  INNER JOIN
				  Enterprise.UserActions AS UA
				  ON A1.ActionID = UA.ActionID
				  INNER JOIN
				  Enterprise.[Right] AS R
				  ON UA.RightID = R.RightID
				  INNER JOIN
				  Enterprise.RightValueType AS RVT
				  ON RVT.RightValueTypeId = R.RightValueTypeId
				  INNER JOIN
				  Enterprise.Role AS RL
				  ON RL.RoleID = R.RoleId AND RL.PartyID = @OrganizationId
				  INNER JOIN
				  Enterprise.RoleValueType AS RLVT
				  ON RLVT.RoleValueTypeId = RL.RoleValueTypeId
				  INNER JOIN
				  Enterprise.ActionValueType AS AVT
				  ON A2.ActionvalueTypeId = AVT.ActionValueTypeID
				  INNER JOIN
						 Enterprise.PersonaPrivilege AS PP
						 ON PP.RoleId = RL.RoleId
						 INNER JOIN
						 Person.Persona AS PA
						 ON PA.PersonaID = PP.PersonaId
					WHERE PA.PersonaID = @PersonaID
			 AND A2.ParentActionId IS NOT NULL)
			 INSERT INTO #HoldRoutes( ActionId, ObjectType, ObjectValue, ParentActionId, ActionValueTypeId )
					SELECT DISTINCT 
						   NULL AS 'ActionId', ACT.ObjectType, ACT.ACTION, NULL AS 'ParentActionId', NULL AS 'ActionvalueTypeId'
					FROM ActionsCTE AS ACT
						 
					UNION
					SELECT NULL AS 'ActionId', A.ObjectType, A.ObjectValue, NULL AS 'ParentActionId', NULL AS 'ActionValueTypeId'
					FROM Enterprise.[RightValueType] AS R
						 INNER JOIN
						 Enterprise.RightDependency AS RD
						 ON RD.RightValueTypeId = R.RightValueTypeId
						 INNER JOIN
						 Enterprise.[Right] AS RT
						 ON RT.RightValueTypeId = RD.DependentRightValueTypeId
						 INNER JOIN
						 Enterprise.UserActions AS UA
						 ON RT.RightId = UA.RightId
						 INNER JOIN
						 Enterprise.ACTION AS A
						 ON UA.ActionId = A.ActionId
					WHERE A.ObjectValue NOT LIKE 'DefaultRoute%' AND 
						  RD.DependentRightValueTypeId IN
								(SELECT DependentRightValueTypeId FROM HoldDependencies)
						OPTION (OPTIMIZE FOR UNKNOWN)

		IF @RouteId IN( 'SideMenu' )
		BEGIN
			DELETE FROM #HoldRoutes
			WHERE ObjectValue <> @RouteId AND 
				  ObjectValue NOT IN( N'SideMenu', N'Dashboard', N'Products', N'People', 'Roles and rights', 'Activity Log', 'Manage Unified Settings', 'View Unified Settings',  'Settings',  'Manage Settings Templates', 'Manage Notifications' );
			 IF EXISTS(SELECT 1 FROM #HoldRoutes WHERE Objectvalue IN  ('Dashboard', 'Settings') AND ObjectType = 'Route')
			 BEGIN
				DELETE FROM #HoldRoutes WHERE ObjectType = 'ROute' and ObjectValue IN  ('Dashboard', 'Settings') 
			 END
		END;
		IF @RouteId IN( 'CloneUser' )
		BEGIN
			DELETE FROM #HoldRoutes
			WHERE ObjectValue <> @RouteId AND 
				  ObjectValue NOT IN( N'CloneUser', N'Edit User');
			 
		END;
		IF @RouteId IN( 'Dashboard' )
		BEGIN
			DELETE FROM #HoldRoutes
			WHERE ObjectValue <> @RouteId AND 
				  ObjectValue NOT IN( N'DashBoard', N'Client Portal', N'Product Learning Portal', N'Leasing & Rents Conversion Tool', N'Employee Management', N'Migration Tool', 'Support Tool', 'Property Photos', 'OneSite Conversions',  'Integration Marketplace' , N'Manage Accounting Product Access', N'Manage Asset Optimization Product Access', N'Manage Client Portal Product Access', N'Manage Document Management Product Access', N'Manage ILM Lead Managemement Product Access', N'Manage ILM Leasing Analytics Product Access', N'Manage Lead2Lease Product Access', N'Manage Marketing Center Product Access', N'Manage Prospect Contact Center Product Access', N'Manage OneSite Product Access', N'Manage On-Site Product Access', N'Manage Renters Insurance Product Access', N'Manage Resident Portals Product Access', N'Manage Spend Management Product Access', N'Manage Unified Amenities Product Access', N'Manage Utility Management Product Access', N'Manage Vendor Compliance Product Access', 'Portfolio Management', N'Access HelpCenter');
		END;
		IF @RouteId IN( 'AddUser' )
		BEGIN
			DELETE FROM #HoldRoutes
			WHERE ObjectValue <> @RouteId AND 
				  ObjectValue NOT IN( N'AddUser', N'Create User', N'Activate Deactivate User');
		END;
		IF @RouteId IN( 'EditUser' )
		BEGIN

			DELETE FROM #HoldRoutes
			WHERE ObjectValue <> @RouteId AND 
					ObjectValue NOT IN( N'Edit User', 'EditUser', N'Edit Other User Profile', N'Edit Own Profile', N'Edit Password', 'View User', 'View Audit Trail User Data', N'Activate Deactivate User', 'Edit Password');
		

			INSERT INTO #HoldRoutes( ObjectValue, ObjectType )
			VALUES( 'View User', 'Right' ), ( 'Edituser', 'Route' )

			IF EXISTS (SELECT 1 FROM #HoldRoutes WHERE ObjectType = 'Right' and ObjectValue = 'Edit User')
			BEGIN	
				DELETE FROM #HoldRoutes WHERE ObjectType = 'Right' and ObjectValue = 'View User'
			END
		END;
		IF @RouteId IN( 'UsersList' )
		BEGIN
			DELETE FROM #HoldRoutes
			WHERE ObjectValue <> @RouteId AND 
				  ObjectValue NOT IN( N'UsersList', N'Activate Deactivate User', N'Clone User', N'Create User', N'Edit Other User Profile', N'Edit Own Profile', N'Edit Password', N'Edit User', N'Lock/Unlock User', N'Resend Invitation', N'View User', 'Manage Resident Portal Users', 'Import users'  );
			IF EXISTS (SELECT 1 FROM #HoldRoutes WHERE ObjectType = 'Right' and ObjectValue = 'Edit User')
				BEGIN	
					DELETE FROM #HoldRoutes WHERE ObjectType = 'Right' and ObjectValue = 'View User'
				END
			IF NOT EXISTS (SELECT 1 FROM #HoldRoutes WHERE ObjectType = 'Route')
			BEGIN
				DELETE FROM #HoldROutes
			END
		END;
		IF @RouteId IN( 'SupportTool' )
		BEGIN
			DELETE FROM #HoldRoutes
			WHERE ObjectValue <> @RouteId AND 
				  ObjectValue NOT In (N'SupportTool', N'Access to Unified Platform', N'Access to Unified Settings', N'Employee Access', 'Access Settings in MGMTConsole', 'Manage Settings in MGMTConsole', 'Implement Questionaires in MGMTConsole', 'View Only Support Tool Access')
			 
		END;
		IF @RouteId IN( 'RolesAndRights' )
		BEGIN
			DELETE FROM #HoldRoutes
			WHERE ObjectValue <> @RouteId AND 
				  ObjectValue NOT In (N'RolesAndRights', N'Manage Other User Roles & Rights', N'View Rights', N'View Roles', N'View Roles & Rights', 'View Role Right', 'Manage Role Right')
			 
		END;

		IF @RouteId IN( 'ActivityLog' )
		BEGIN
			DELETE FROM #HoldRoutes
			WHERE ObjectValue <> @RouteId AND 
				  ObjectValue NOT In (N'Activity Log', N'View Audit Trail User Data', N'View Audit Trail')
			 
		END;

		IF @RouteId = 'Settings'
		BEGIN
			DELETE FROM #HoldRoutes
			WHERE ObjectValue <> @RouteId AND 
				  ObjectValue NOT In ('Settings', 'Manage Unified Settings', 'View Unified Settings',  'Manage Custom Fields', 'Manage Platform Security')
		END;


		IF NOT EXISTS (SELECT 1 FROM #HoldRoutes WHERE	ObjectTYpe = 'Right')
		BEGIN
			DELETE FROM #HoldROutes
		END

		SELECT DISTINCT NULL AS 'ActionId', ObjectType, ObjectValue AS 'Action', NULL AS 'ParentActionId', NULL AS 'ActionValueTypeId'
		FROM #HoldRoutes
		ORDER BY ObjectType DESC;
	END;
	ELSE
	BEGIN
		WITH ActionsCTE
			 AS (
			 SELECT A1.ActionID, RLVT.Value AS 'RoleName', RVT.value AS 'RightName', RL.RoleId, R.RightID, A1.ProductId, ObjectValue AS 'Action', ObjectType AS 'ObjectType', ParentActionId, A1.ActionvalueTypeId, 1 AS 'Level'
			 FROM Enterprise.ACTION AS A1
				  INNER JOIN
				  Enterprise.UserActions AS UA
				  ON A1.ActionID = UA.ActionID
				  INNER JOIN
				  Enterprise.[Right] AS R
				  ON R.RightID = UA.RightID
				  INNER JOIN
				  Enterprise.RightValueType AS RVT
				  ON RVT.RightValueTypeId = R.RightValueTypeId
				  INNER JOIN
				  Enterprise.Role AS RL
				  ON RL.RoleID = R.RoleId-- AND RL.PartyID = @OrganizationId
				  INNER JOIN
				  Enterprise.RoleValueType AS RLVT
				  ON RLVT.RoleValueTypeId = RL.RoleValueTypeId
				  INNER JOIN
				  Enterprise.ActionValueType AS AVT
				  ON A1.ActionvalueTypeId = AVT.ActionValueTypeID
			 WHERE ParentActionId IS NULL AND 
				   AVT.Value = 'Route' AND 
				   ObjectValue = @RouteId
				   AND RL.PartyId = @OrganizationId
				   --AND ObjectValue NOT LIKE 'Default%'
			 UNION ALL
			 SELECT A2.ActionID, RLVT.Value, RVT.Value, RL.RoleId, R.RightID, A2.ProductId, A2.ObjectValue, A2.ObjectType, A2.ParentActionId, A2.ActionvalueTypeId, A1.Level + 1
			 FROM ActionsCTE AS A1
				  INNER JOIN
				  Enterprise.ACTION AS A2
				  ON A2.ParentActionId = A1.ActionID
				  INNER JOIN
				  Enterprise.UserActions AS UA
				  ON A1.ActionID = UA.ActionID
				  INNER JOIN
				  Enterprise.[Right] AS R
				  ON UA.RightID = R.RightID
				  INNER JOIN
				  Enterprise.RightValueType AS RVT
				  ON RVT.RightValueTypeId = R.RightValueTypeId
				  INNER JOIN
				  Enterprise.Role AS RL
				  ON RL.RoleID = R.RoleId 
				  INNER JOIN
				  Enterprise.RoleValueType AS RLVT
				  ON RLVT.RoleValueTypeId = RL.RoleValueTypeId
				  INNER JOIN
				  Enterprise.ActionValueType AS AVT
				  ON A2.ActionvalueTypeId = AVT.ActionValueTypeID
				  INNER JOIN Enterprise.PersonaPrivilege AS PP
				  ON PP.RoleId = RL.RoleId
				  INNER JOIN
				  Person.Persona AS PA
				  ON PA.PersonaID = PP.PersonaId
			 WHERE PA.PersonaID = @PersonaId
			 AND RL.PartyID = @OrganizationId
			 AND A2.ParentActionId IS NOT NULL )
			 


			 INSERT INTO #HoldRights ( ActionId, ObjectType, ObjectValue, ParentActionId, ActionValueTypeId )

			 SELECT NULL AS 'ActionId', ACT.ObjectType, ACT.ACTION, NULL AS 'ParentActionId', NULL AS 'ActionvalueTypeId'
			 FROM ActionsCTE AS ACT
			 ORDER BY ObjectType DESC
			 --OPTION (OPTIMIZE FOR UNKNOWN);

			 IF (SELECT O.Name FROM Person.Persona P 
					INNER JOIN Ident.UserLoginPersona ULP ON ULP.UserLoginPersonaId = P.UserLoginPersonaId
					INNER JOIN Enterprise.Organization O ON O.PartyId = ULP.OrganizationPartyId 
					WHERE OrganizationPartyId = @OrganizationId AND PersonaId = @PersonaId ) <> @OrganizationName
			 BEGIN
				DELETE FROM #HoldRights WHERE ObjectValue IN ('Manage Unified Settings', 'View Unified Settings', 'Import Users', 'Manage Custom Fields', 'Manage Platform Security', 'Manage Settings Templates')
			 END
			 SELECT DISTINCT NULL AS 'ActionId', ObjectType, ObjectValue AS 'Action', NULL AS 'ParentActionId', NULL AS 'ActionValueTypeId'
				FROM #HoldRights
				ORDER BY ObjectType DESC;

		DROP TABLE #HoldRoutes
		DROP TABLE #HoldRights
	END;
END;
