
DECLARE @ClientName NVARCHAR(500);
DECLARE @ClientCode NVARCHAR(500);
DECLARE @UnifiedSettingRoute INT;
DECLARE @SidemenuPeople INT;
DECLARE @UserList INT;
DECLARE @ViewUser INT;
DECLARE @Adduser INT;
DECLARE @CreateUser INT;
DECLARE @EditUser INT;
DECLARE @EditUserROute INT;
DECLARE @ResidentPortal INT;
DECLARE @TitleUniqueId UNIQUEIDENTIFIER;
DECLARE @ParentProductTypeId INT;
DECLARE @SamlProductSettingId INT;
DECLARE @ClientId INT;
DECLARE @viewCompany INT;
DECLARE @ViewProperty INT;
DECLARE @RightValueTYpeId INT;
DECLARE @RootRoleId INT;
DECLARE @PartyId INT;
DECLARE @PartyRoleTypeId INT;
DECLARE @AuditTrail INT;
DECLARE @ResendInvite INT;
DECLARE @Lock INT;
DECLARE @OrgRowNum INT;
DECLARE @ActionID INT;
DECLARE @RightID INT;
DECLARE @RoleID INT;
DECLARE @Status INT;
DECLARE @ActionValueID INT;
DECLARE @OrgID INT;
DECLARE @ProductID INT;
DECLARE @ParentActionId INT;
DECLARE @UserActionId INT;
DECLARE @RightCategoryId INT;
DECLARE @RightName VARCHAR(100);
DECLARE @RVT INT;
DECLARE @DefaultRoute NVARCHAR(200);
DECLARE @StatusId INT;
DECLARE @PersonaId INT;
DECLARE @FromDate DATETIME;
DECLARE @TRoleId INT;
DECLARE @TRoleName NVARCHAR(500);
DECLARE @TRoleDesc NVARCHAR(500);
DECLARE @TRightId INT;
DECLARE @TRightName NVARCHAR(500);
DECLARE @TRightDesc NVARCHAR(500);
DECLARE @RightCategory INT;
DECLARE @RoleCategory INT;
DECLARE @RoleName NVARCHAR(500);
DECLARE @RoleTypeID INT;
DECLARE @PerosonaP INT;
DECLARE @PartyRowNum INT;
DECLARE @TRightShortName VARCHAR(100);

DECLARE @AuthClients TABLE
( 
						   ClientId int
);

SET @FromDate = GETUTCDATE();


/**-----------------------------
Scirpt 61
------------------------------**/

--Resident Portal

IF OBJECT_ID('tempdb..#NewRight10') IS NOT NULL
BEGIN
	DROP TABLE #NewRight10;
END;

IF OBJECT_ID('tempdb..#HoldPartyFornewRights10') IS NOT NULL
BEGIN
	DROP TABLE #HoldPartyFornewRights10;
END;

CREATE TABLE #NewRight10
( 
			 RightId int, Name nvarchar(500), description nvarchar(500), shortname varchar(100)
);

INSERT INTO #NewRight10( rightid, name, description, shortname )
VALUES( 1, 'Ability to create and manage users for Resident Portals', 'Ability to create and manage users for Resident Portals', 'AddEditResidentPortalUser' );

SELECT @ProductId = ProductId
FROM Enterprise.Product
WHERE name IN ( 'Unified Login', 'Unified Platform');

SELECT @ActionValueID = [ActionValueTypeID]
FROM Enterprise.ActionValueType
WHERE Value = 'Right';

SELECT @RoleCategory = ST.StatusTypeId
FROM Enterprise.StatusTypeCategoryType AS STCT
	 JOIN
	 Enterprise.StatusTypeCategory AS STC
	 ON STCT.StatusTypeCategoryTypeId = STC.StatusTypeCategoryTypeId
	 JOIN
	 Enterprise.StatusTypeCategoryClassification AS STCC
	 ON STCC.StatusTypeCategoryId = STC.StatusTypeCategoryId
	 JOIN
	 Enterprise.StatusType AS ST
	 ON ST.StatusTypeId = STCC.StatusTypeId
WHERE STC.Name = 'Role Type' AND 
	  ST.Name = 'Default';

SELECT @RightCategory = ST.StatusTypeId
FROM Enterprise.StatusTypeCategoryType AS STCT
	 JOIN
	 Enterprise.StatusTypeCategory AS STC
	 ON STCT.StatusTypeCategoryTypeId = STC.StatusTypeCategoryTypeId
	 JOIN
	 Enterprise.StatusTypeCategoryClassification AS STCC
	 ON STCC.StatusTypeCategoryId = STC.StatusTypeCategoryId
	 JOIN
	 Enterprise.StatusType AS ST
	 ON ST.StatusTypeId = STCC.StatusTypeId
WHERE STC.Name = 'Right Type' AND 
	  ST.Name = 'Default';

IF NOT EXISTS
(
	SELECT 1
	FROM Enterprise.ACTION
	WHERE ObjectValue = 'Manage Resident Portal Users' AND 
		  ParentActionId IS NULL
)
BEGIN
	EXEC Enterprise.CreateAction @ProductID = @ProductId, @Action = N'Manage Resident Portal Users', @ActionTarget = N'Right', @ActionbValueTypeId = @ActionValueID, @Description = 'Edit Other User Profile', @ActionID = @ActionID OUTPUT;
	SELECT @ActionID AS N'@ActionID';
END;

SELECT @ParentActionId = ActionId
FROM Enterprise.ACTION
WHERE ObjectValue = 'AddUser' AND 
	  ObjectType = 'Route' AND 
	  Description = 'SuperUser';

IF NOT EXISTS
(
	SELECT 1
	FROM Enterprise.ACTION
	WHERE ObjectValue = 'Manage Resident Portal Users' AND 
		  ParentActionID = @ParentActionId
)
BEGIN
	EXEC [Enterprise].[CreateAction] @ProductID = @ProductId, @Action = N'Manage Resident Portal Users', @ActionTarget = N'Right', @ActionbValueTypeId = @ActionValueID, @Description = '', @ParentActionID = @ParentActionId, @ActionID = @ActionID OUTPUT;
	SELECT @ActionID AS N'@ActionID';
END;

SELECT @ParentActionId = ActionId
FROM Enterprise.ACTION
WHERE ObjectValue = 'EditUser' AND 
	  ObjectType = 'Route' AND 
	  Description = 'SuperUser';

IF NOT EXISTS
(
	SELECT 1
	FROM Enterprise.ACTION
	WHERE ObjectValue = 'Manage Resident Portal Users' AND 
		  ParentActionID = @ParentActionId
)
BEGIN
	EXEC [Enterprise].[CreateAction] @ProductID = @ProductId, @Action = N'Manage Resident Portal Users', @ActionTarget = N'Right', @ActionbValueTypeId = @ActionValueID, @Description = '', @ParentActionID = @ParentActionId, @ActionID = @ActionID OUTPUT;
	SELECT @ActionID AS N'@ActionID';
END;

SELECT DISTINCT 
	   IDENTITY(int, 1, 1) AS RowNumber, OrganizationPartyID, 0 AS PStatus
INTO #HoldPartyFornewRights10
FROM Person.Persona;

WHILE EXISTS
(
	SELECT 1
	FROM #HoldPartyFornewRights10
	WHERE PStatus = 0
)
BEGIN
	SELECT TOP 1 @PartyRowNum = Rownumber, @PartyId = OrganizationPartyID
	FROM #HoldPartyFornewRights10
	WHERE PStatus = 0;
	SELECT @RoleId = RoleId
	FROM Enterprise.Role AS R
		 INNER JOIN
		 Enterprise.RoleValueType AS RR
		 ON RR.RoleValueTypeId = R.RoleValueTypeId
	WHERE RR.Value = 'User Administrator' AND 
		  R.PartyId = @PartyId;
	DECLARE Rights CURSOR
	FOR SELECT RightId, Name, Description, ShortName
		FROM #NewRight10;
	OPEN Rights;
	FETCH Rights INTO @TRightId, @TRightName, @TRightDesc, @TRightShortName;
	WHILE @@FETCH_STATUS = 0
	BEGIN
		EXECUTE Enterprise.CreateRight @RoleId = -1, @RightName = @TRightName, @RightCategoryId = @RightCategory, @PartyId = @PartyId, @ProductId = @ProductId, @Shortname = '', @Description = @TRightDesc, @RightId = @RightId OUTPUT;
		EXECUTE Enterprise.CreateRight @RoleId = -1, @RightName = 'Default_AddEditResidentPortalUser', @ShortName = 'AddEditResidentPortalUser', @RightCategoryId = @RightCategory, @PartyId = @PartyId, @ProductId = @ProductId, @Description = 'Manage Resident Portal Users', @RightId = @RightId OUTPUT;
		SELECT @RightId = RightId
		FROM Enterprise.[Right] AS R
			 INNER JOIN
			 Enterprise.RightValueType AS RR
			 ON RR.RightValueTypeId = R.RightValueTypeId
		WHERE RR.Value = 'Default_AddEditResidentPortalUser';
		SELECT @ActionID = ActionID
		FROM Enterprise.ACTION
		WHERE ObjectValue = 'Manage Resident Portal Users' AND 
			  ObjectType = 'Right' AND 
			  ParentActionId IS NULL;
		EXEC [Enterprise].[LinkActionToRights] @ActionID = @ActionID, @RightId = @RightId, @StatusId = @Status, @UserActionId = @UserActionId OUTPUT;
		SELECT @RightID = NULL, @ActionId = NULL;
		FETCH Rights INTO @TRightId, @TRightName, @TRightDesc, @TRightShortName;
	END;
	CLOSE Rights;
	DEALLOCATE Rights;
	UPDATE #HoldPartyFornewRights10
	  SET PStatus = 1
	WHERE RowNumber = @PartyRowNum;
END;



SELECT @SidemenuPeople = RightValueTypeId
FROM Enterprise.RightValueType
WHERE value = 'Default_Sidemenu_People';

SELECT @UserList = RightValueTypeId
FROM Enterprise.RightValueType
WHERE value = 'Default_UserList';

SELECT @ViewUser = RightValueTypeId
FROM Enterprise.RightValueType
WHERE value = 'Default_ViewUser';

SELECT @Adduser = RightValueTypeId
FROM Enterprise.RightValueType
WHERE value = 'Default_AddUser';

SELECT @CreateUser = RightValueTypeId
FROM Enterprise.RightValueType
WHERE value = 'Default_Createuser';

SELECT @EditUser = RightValueTypeId
FROM Enterprise.RightValueType
WHERE value = 'Default_EditUser';

SELECT @EditUserROute = RightValueTypeId
FROM Enterprise.RightValueType
WHERE value = 'Default_EditUser_Route';

SELECT @ResidentPortal = RightValueTypeId
FROM Enterprise.RightValueType
WHERE value = 'Default_AddEditResidentPortalUser';

SELECT @RightValueTypeId = RightValueTypeId
FROM Enterprise.RightValueType
WHERE value IN( 'Ability to create and manage users for Resident Portals' );

IF NOT EXISTS
(
	SELECT 1
	FROM Enterprise.RightDependency
	WHERE RightValueTypeId = @RightValueTypeId AND 
		  DependentRightValueTypeId = @SidemenuPeople
)
BEGIN
	INSERT INTO Enterprise.RightDependency( RightValueTypeId, DependentRightValueTypeId )
	VALUES( @RightValueTypeId, @SidemenuPeople );
END;

IF NOT EXISTS
(
	SELECT 1
	FROM Enterprise.RightDependency
	WHERE RightValueTypeId = @RightValueTypeId AND 
		  DependentRightValueTypeId = @UserList
)
BEGIN
	INSERT INTO Enterprise.RightDependency( RightValueTypeId, DependentRightValueTypeId )
	VALUES( @RightValueTypeId, @UserList );
END;

IF NOT EXISTS
(
	SELECT 1
	FROM Enterprise.RightDependency
	WHERE RightValueTypeId = @RightValueTypeId AND 
		  DependentRightValueTypeId = @ViewUser
)
BEGIN
	INSERT INTO Enterprise.RightDependency( RightValueTypeId, DependentRightValueTypeId )
	VALUES( @RightValueTypeId, @ViewUser );
END;

IF NOT EXISTS
(
	SELECT 1
	FROM Enterprise.RightDependency
	WHERE RightValueTypeId = @RightValueTypeId AND 
		  DependentRightValueTypeId = @Adduser
)
BEGIN
	INSERT INTO Enterprise.RightDependency( RightValueTypeId, DependentRightValueTypeId )
	VALUES( @RightValueTypeId, @Adduser );
END;

IF NOT EXISTS
(
	SELECT 1
	FROM Enterprise.RightDependency
	WHERE RightValueTypeId = @RightValueTypeId AND 
		  DependentRightValueTypeId = @CreateUser
)
BEGIN
	INSERT INTO Enterprise.RightDependency( RightValueTypeId, DependentRightValueTypeId )
	VALUES( @RightValueTypeId, @CreateUser );
END;

IF NOT EXISTS
(
	SELECT 1
	FROM Enterprise.RightDependency
	WHERE RightValueTypeId = @RightValueTypeId AND 
		  DependentRightValueTypeId = @EditUser
)
BEGIN
	INSERT INTO Enterprise.RightDependency( RightValueTypeId, DependentRightValueTypeId )
	VALUES( @RightValueTypeId, @EditUser );
END;

IF NOT EXISTS
(
	SELECT 1
	FROM Enterprise.RightDependency
	WHERE RightValueTypeId = @RightValueTypeId AND 
		  DependentRightValueTypeId = @EditUserROute
)
BEGIN
	INSERT INTO Enterprise.RightDependency( RightValueTypeId, DependentRightValueTypeId )
	VALUES( @RightValueTypeId, @EditUserROute );
END;

IF NOT EXISTS
(
	SELECT 1
	FROM Enterprise.RightDependency
	WHERE RightValueTypeId = @RightValueTypeId AND 
		  DependentRightValueTypeId = @ResidentPortal
)
BEGIN
	INSERT INTO Enterprise.RightDependency( RightValueTypeId, DependentRightValueTypeId )
	VALUES( @RightValueTypeId, @ResidentPortal );
END;
--->

--Audit trail Related chnages

SELECT @ProductId = ProductId
FROM Enterprise.Product
WHERE Name IN ( 'Unified Login', 'Unified Platform');;

SELECT @ActionValueID = ActionValueTypeID
FROM Enterprise.ActionValueType
WHERE value = 'Right';

SELECT @ParentActionId = ActionId
FROM Enterprise.ACTION
WHERE ObjectValue = 'EditUser' AND 
	  ObjectType = 'Route' AND 
	  Description = 'SuperUser';

IF NOT EXISTS
(
	SELECT 1
	FROM Enterprise.ACTION
	WHERE ObjectValue = 'View Audit Trail' AND 
		  ParentActionID = @ParentActionId
)
BEGIN
	EXEC [Enterprise].[CreateAction] @ProductID = @ProductId, @Action = N'View Audit Trail', @ActionTarget = N'Right', @ActionbValueTypeId = @ActionValueID, @Description = '', @ParentActionID = @ParentActionId, @ActionID = @ActionID OUTPUT;
	SELECT @ActionID AS N'@ActionID';
END;

SELECT @AuditTrail = RightValueTypeId
FROM Enterprise.RightValueType
WHERE value = 'Default_ViewAuditTrailData';

SELECT @RightValueTypeId = RightValueTypeId
FROM Enterprise.RightValueType
WHERE value IN( 'Ability to view audit trail on user data' );

IF NOT EXISTS
(
	SELECT 1
	FROM Enterprise.RightDependency
	WHERE RightValueTypeId = @RightValueTypeId AND 
		  DependentRightValueTypeId = @AuditTrail
)
BEGIN
	INSERT INTO Enterprise.RightDependency( RightValueTypeId, DependentRightValueTypeId )
	VALUES( @RightValueTypeId, @AuditTrail );
END;

SELECT @RightValueTypeId = RightValueTypeId
FROM Enterprise.RightValueType
WHERE value IN( 'Ability to edit users' );

IF NOT EXISTS
(
	SELECT 1
	FROM Enterprise.RightDependency
	WHERE RightValueTypeId = @RightValueTypeId AND 
		  DependentRightValueTypeId = @AuditTrail
)
BEGIN
	INSERT INTO Enterprise.RightDependency( RightValueTypeId, DependentRightValueTypeId )
	VALUES( @RightValueTypeId, @AuditTrail );
END;

--External User




--SELECT @RootRoleId = PartyRoleTypeId
--FROM Enterprise.RoleType
--WHERE Name = 'User Role';

--IF NOT EXISTS
--(
--	SELECT 1
--	FROM Enterprise.RoleType
--	WHERE Name = 'External User' AND 
--		  ParentPartyRoleTypeId = @RootRoleId
--)
--BEGIN
--	INSERT INTO Enterprise.RoleType( PartyRoleTypeId, ParentPartyRoleTypeId, Name )
--	VALUES( 405, @RootRoleId, 'External User' );
--END;

--SET @PartyRoleTypeId = 405;

--DECLARE OrgName CURSOR
--FOR SELECT PartyId
--	FROM Enterprise.Organization;

--OPEN OrgName;

--FETCH OrgName INTO @PartyId;

--WHILE @@FETCH_STATUS = 0
--BEGIN
--	IF NOT EXISTS
--(
--	SELECT 1
--	FROM Enterprise.PartyROle
--	WHERE PartyId = @PartyId AND 
--		  RoleTypeId = @PartyRoleTypeId
--)
--	BEGIN
--		INSERT INTO Enterprise.PartyRole( PartyId, RoleTypeId )
--		VALUES( @PartyId, @PartyRoleTypeId );
--	END;
--	FETCH OrgName INTO @PartyId;
--END;

--CLOSE OrgName;

--DEALLOCATE OrgName;

--Company and property related mapping

SELECT @ProductId = ProductId
FROM Enterprise.Product
WHERE Name IN ( 'Unified Login', 'Unified Platform');;

SELECT @ActionValueID = ActionValueTypeID
FROM Enterprise.ActionValueType
WHERE value = 'Right';

SELECT @ParentActionId = ActionId
FROM Enterprise.ACTION
WHERE ObjectValue = 'EditUser' AND 
	  ObjectType = 'Route' AND 
	  Description = 'SuperUser';

IF NOT EXISTS
(
	SELECT 1
	FROM Enterprise.ACTION
	WHERE ObjectValue = 'View Audit Trail' AND 
		  ParentActionID = @ParentActionId
)
BEGIN
	EXEC [Enterprise].[CreateAction] @ProductID = @ProductId, @Action = N'View Audit Trail', @ActionTarget = N'Right', @ActionbValueTypeId = @ActionValueID, @Description = '', @ParentActionID = @ParentActionId, @ActionID = @ActionID OUTPUT;
	SELECT @ActionID AS N'@ActionID';
END;

IF NOT EXISTS
(
	SELECT 1
	FROM Enterprise.ACTION
	WHERE ObjectValue = 'Property' AND 
		  ParentActionID = @ParentActionId
)
BEGIN
	EXEC [Enterprise].[CreateAction] @ProductID = @ProductId, @Action = N'Property', @ActionTarget = N'Right', @ActionbValueTypeId = @ActionValueID, @Description = '', @ParentActionID = @ParentActionId, @ActionID = @ActionID OUTPUT;
	SELECT @ActionID AS N'@ActionID';
END;

SELECT @viewCompany = RightValueTypeId
FROM Enterprise.RightValueType
WHERE value = 'Default_ViewCompanyPage';

SELECT @ViewProperty = RightValueTypeId
FROM Enterprise.RightValueType
WHERE value IN( 'Default_ViewProperty' );

SELECT @RightValueTypeId = RightValueTypeId
FROM Enterprise.RightValueType
WHERE value IN( 'View Company Page' );

IF NOT EXISTS
(
	SELECT 1
	FROM Enterprise.RightDependency
	WHERE RightValueTypeId = @RightValueTypeId AND 
		  DependentRightValueTypeId = @viewCompany
)
BEGIN
	INSERT INTO Enterprise.RightDependency( RightValueTypeId, DependentRightValueTypeId )
	VALUES( @RightValueTypeId, @viewCompany );
END;

SELECT @RightValueTypeId = RightValueTypeId
FROM Enterprise.RightValueType
WHERE value IN( 'View Property Page' );

IF NOT EXISTS
(
	SELECT 1
	FROM Enterprise.RightDependency
	WHERE RightValueTypeId = @RightValueTypeId AND 
		  DependentRightValueTypeId = @RightValueTypeId
)
BEGIN
	INSERT INTO Enterprise.RightDependency( RightValueTypeId, DependentRightValueTypeId )
	VALUES( @RightValueTypeId, @RightValueTypeId );
END;

--Unified Settings

IF OBJECT_ID('tempdb..#RightsUnifiedSettings') IS NOT NULL
BEGIN
	DROP TABLE #RightsUnifiedSettings;
END;

IF OBJECT_ID('tempdb..#HoldPartyForUnifiedSettings') IS NOT NULL
BEGIN
	DROP TABLE #HoldPartyForUnifiedSettings;
END;

CREATE TABLE #RightsUnifiedSettings
( 
			 RightId int, Name nvarchar(500), description nvarchar(500), shortname varchar(100)
);

INSERT INTO #RightsUnifiedSettings( rightid, name, description, shortname )
VALUES( 1, 'View Unified Settings', 'Ability to View Unified Settings', 'ViewUnifiedSettings' ), ( 2, 'Ability to manage Unified Settings', 'Ability to manage Unified Settings', 'ManageUnifiedSettings' );

SELECT @ProductId = ProductId
FROM Enterprise.Product
WHERE name IN ( 'Unified Login', 'Unified Platform');;

SELECT @ActionValueID = [ActionValueTypeID]
FROM Enterprise.ActionValueType
WHERE Value = 'Right';

SELECT @RoleCategory = ST.StatusTypeId
FROM Enterprise.StatusTypeCategoryType AS STCT
	 JOIN
	 Enterprise.StatusTypeCategory AS STC
	 ON STCT.StatusTypeCategoryTypeId = STC.StatusTypeCategoryTypeId
	 JOIN
	 Enterprise.StatusTypeCategoryClassification AS STCC
	 ON STCC.StatusTypeCategoryId = STC.StatusTypeCategoryId
	 JOIN
	 Enterprise.StatusType AS ST
	 ON ST.StatusTypeId = STCC.StatusTypeId
WHERE STC.Name = 'Role Type' AND 
	  ST.Name = 'Default';

SELECT @RightCategory = ST.StatusTypeId
FROM Enterprise.StatusTypeCategoryType AS STCT
	 JOIN
	 Enterprise.StatusTypeCategory AS STC
	 ON STCT.StatusTypeCategoryTypeId = STC.StatusTypeCategoryTypeId
	 JOIN
	 Enterprise.StatusTypeCategoryClassification AS STCC
	 ON STCC.StatusTypeCategoryId = STC.StatusTypeCategoryId
	 JOIN
	 Enterprise.StatusType AS ST
	 ON ST.StatusTypeId = STCC.StatusTypeId
WHERE STC.Name = 'Right Type' AND 
	  ST.Name = 'Default';

IF NOT EXISTS
(
	SELECT 1
	FROM Enterprise.ACTION
	WHERE ObjectValue = 'Unified Settings' AND 
		  ParentActionId IS NULL
)
BEGIN
	EXEC Enterprise.CreateAction @ProductID = @ProductId, @Action = N'Unified Settings', @ActionTarget = N'Route', @ActionbValueTypeId = @ActionValueID, @Description = 'Edit Other User Profile', @ActionID = @ActionID OUTPUT;
	SELECT @ActionID AS N'@ActionID';
END;

SELECT @ParentActionId = ActionId
FROM Enterprise.ACTION
WHERE ObjectValue = 'SideMenu' AND 
	  ObjectType = 'Route' AND 
	  Description = 'SuperUser';

IF NOT EXISTS
(
	SELECT 1
	FROM Enterprise.ACTION
	WHERE ObjectValue = 'View Unified Settings' AND 
		  ParentActionID = @ParentActionId
)
BEGIN
	EXEC [Enterprise].[CreateAction] @ProductID = @ProductId, @Action = N'View Unified Settings', @ActionTarget = N'Right', @ActionbValueTypeId = @ActionValueID, @Description = '', @ParentActionID = @ParentActionId, @ActionID = @ActionID OUTPUT;
	SELECT @ActionID AS N'@ActionID';
END;

IF NOT EXISTS
(
	SELECT 1
	FROM Enterprise.ACTION
	WHERE ObjectValue = 'Manage Unified Settings' AND 
		  ParentActionID = @ParentActionId
)
BEGIN
	EXEC [Enterprise].[CreateAction] @ProductID = @ProductId, @Action = N'Manage Unified Settings', @ActionTarget = N'Right', @ActionbValueTypeId = @ActionValueID, @Description = '', @ParentActionID = @ParentActionId, @ActionID = @ActionID OUTPUT;
	SELECT @ActionID AS N'@ActionID';
END;

SELECT DISTINCT 
	   IDENTITY(int, 1, 1) AS RowNumber, OrganizationPartyID, 0 AS PStatus
INTO #HoldPartyForUnifiedSettings
FROM Person.Persona AS P
	 INNER JOIN
	 Enterprise.Organization AS O
	 ON P.OrganizationPartyId = O.PartyId
WHERE O.Name = 'RealPage Employee';

WHILE EXISTS
(
	SELECT 1
	FROM #HoldPartyForUnifiedSettings
	WHERE PStatus = 0
)
BEGIN
	SELECT TOP 1 @PartyRowNum = Rownumber, @PartyId = OrganizationPartyID
	FROM #HoldPartyForUnifiedSettings
	WHERE PStatus = 0;
	SELECT @RoleId = RoleId
	FROM Enterprise.Role AS R
		 INNER JOIN
		 Enterprise.RoleValueType AS RR
		 ON RR.RoleValueTypeId = R.RoleValueTypeId
	WHERE RR.Value = 'User Administrator' AND 
		  R.PartyId = @PartyId;
	DECLARE Rights CURSOR
	FOR SELECT RightId, Name, Description, ShortName
		FROM #RightsUnifiedSettings;
	OPEN Rights;
	FETCH Rights INTO @TRightId, @TRightName, @TRightDesc, @TRightShortName;
	WHILE @@FETCH_STATUS = 0
	BEGIN
		EXECUTE Enterprise.CreateRight @RoleId = @RoleId, @RightName = @TRightName, @RightCategoryId = @RightCategory, @PartyId = @PartyId, @ProductId = @ProductId, @Shortname = @TRightShortName, @Description = @TRightDesc, @RightId = @RightId OUTPUT;
		EXECUTE Enterprise.CreateRight @RoleId = -1, @RightName = 'Default_ViewUnifiedSetting', @ShortName = 'ViewUnifiedSetting', @RightCategoryId = @RightCategory, @PartyId = @PartyId, @ProductId = @ProductId, @Description = 'View Unified Setting', @RightId = @RightId OUTPUT;
		EXECUTE Enterprise.CreateRight @RoleId = -1, @RightName = 'Default_ManageUnifiedSetting', @ShortName = 'ManageUnifiedSetting', @RightCategoryId = @RightCategory, @PartyId = @PartyId, @ProductId = @ProductId, @Description = 'Manage/View Unified Setting', @RightId = @RightId OUTPUT;
		EXECUTE Enterprise.CreateRight @RoleId = -1, @RightName = 'Default_Sidemenu_UnifiedSetting', @ShortName = 'UnifiedSettingRoute', @RightCategoryId = @RightCategory, @PartyId = @PartyId, @ProductId = @ProductId, @Description = 'Route for SideMenu', @RightId = @RightId OUTPUT;
		SELECT @RightId = RightId
		FROM Enterprise.[Right] AS R
			 INNER JOIN
			 Enterprise.RightValueType AS RR
			 ON RR.RightValueTypeId = R.RightValueTypeId
		WHERE RR.Value = 'Default_ViewUnifiedSetting';
		SELECT @ActionID = ActionID
		FROM Enterprise.ACTION
		WHERE ObjectValue = 'View Unified Settings' AND 
			  ObjectType = 'Right' AND 
			  ParentActionId IS NULL;
		EXEC [Enterprise].[LinkActionToRights] @ActionID = @ActionID, @RightId = @RightId, @StatusId = @Status, @UserActionId = @UserActionId OUTPUT;
		SELECT @RightId = RightId
		FROM Enterprise.[Right] AS R
			 INNER JOIN
			 Enterprise.RightValueType AS RR
			 ON RR.RightValueTypeId = R.RightValueTypeId
		WHERE RR.Value = 'Default_ManageUnifiedSetting';
		SELECT @ActionID = ActionID
		FROM Enterprise.ACTION
		WHERE ObjectValue = 'Manage Unified Settings' AND 
			  ObjectType = 'Right' AND 
			  ParentActionId IS NULL;
		EXEC [Enterprise].[LinkActionToRights] @ActionID = @ActionID, @RightId = @RightId, @StatusId = @Status, @UserActionId = @UserActionId OUTPUT;
		SELECT @RightId = RightId
		FROM Enterprise.[Right] AS R
			 INNER JOIN
			 Enterprise.RightValueType AS RR
			 ON RR.RightValueTypeId = R.RightValueTypeId
		WHERE RR.Value = 'Default_Sidemenu_UnifiedSetting';
		SELECT @ActionID = ActionID
		FROM Enterprise.ACTION
		WHERE ObjectValue = 'Unified Settings' AND 
			  ObjectType = 'Route' AND 
			  ParentActionId IS NULL;
		EXEC [Enterprise].[LinkActionToRights] @ActionID = @ActionID, @RightId = @RightId, @StatusId = @Status, @UserActionId = @UserActionId OUTPUT;
		SELECT @RightID = NULL, @ActionId = NULL;
		FETCH Rights INTO @TRightId, @TRightName, @TRightDesc, @TRightShortName;
	END;
	CLOSE Rights;
	DEALLOCATE Rights;
	UPDATE #HoldPartyForUnifiedSettings
	  SET PStatus = 1
	WHERE RowNumber = @PartyRowNum;
END;



--INSERT INTO Enterprise.RightDependency(RightValueTypeId, DependentRightValueTypeId )


SELECT @UnifiedSettingRoute = RightValueTypeId
FROM Enterprise.RightValueType
WHERE value = 'Default_Sidemenu_UnifiedSetting';

SELECT @RightValueTypeId = RightValueTypeId
FROM Enterprise.RightValueType
WHERE value IN( 'View Unified Settings' );

IF NOT EXISTS
(
	SELECT 1
	FROM Enterprise.RightDependency
	WHERE RightValueTypeId = @RightValueTypeId AND 
		  DependentRightValueTypeId = @UnifiedSettingRoute
)
BEGIN
	INSERT INTO Enterprise.RightDependency( RightValueTypeId, DependentRightValueTypeId )
	VALUES( @RightValueTypeId, @UnifiedSettingRoute );
END;

SELECT @RightValueTypeId = RightValueTypeId
FROM Enterprise.RightValueType
WHERE value IN( 'Ability to manage Unified Settings' );

IF NOT EXISTS
(
	SELECT 1
	FROM Enterprise.RightDependency
	WHERE RightValueTypeId = @RightValueTypeId AND 
		  DependentRightValueTypeId = @UnifiedSettingRoute
)
BEGIN
	INSERT INTO Enterprise.RightDependency( RightValueTypeId, DependentRightValueTypeId )
	VALUES( @RightValueTypeId, @UnifiedSettingRoute );
END;

/*-----------------------
--Script 58
-----------------------*/

DECLARE @ProductSettingTypeId int;

DECLARE @ProductSettingId int;

DECLARE @ConfigurationId int;

SELECT @ProductId = ProductId
FROM Enterprise.Product
WHERE Name = 'OneSite';

SELECT @ConfigurationId = ConfigurationId
FROM Enterprise.GlobalProductConfiguration
WHERE ProductId = @ProductId AND 
	  ( GETDATE() BETWEEN FromDate AND ThruDate OR 
		ThruDate IS NULL
	  );

SET @ProductSettingTypeId = NULL;

SET @ProductSettingId = NULL;

IF
(
	SELECT 1
	FROM Enterprise.ProductSettingType
	WHERE Name = 'MTApiEndPoint'
) IS NULL
BEGIN
	EXEC Enterprise.CreateProductSettingType 'MTApiEndPoint', 'MTApiEndPoint', @ProductSettingTypeId OUTPUT;
	EXEC Enterprise.CreateProductSetting @ProductId, @ProductSettingTypeId, 'api/core/common/ulmigration', NULL, NULL, @ProductSettingId OUTPUT;
	EXEC Enterprise.LinkProductSettingToConfiguration @ConfigurationId, @ProductSettingId, NULL, NULL;
END;

SET @ProductSettingTypeId = NULL;

SET @ProductSettingId = NULL;

IF
(
	SELECT 1
	FROM Enterprise.ProductSettingType
	WHERE Name = 'MTTokenEndPoint'
) IS NULL
BEGIN
	EXEC Enterprise.CreateProductSettingType 'MTTokenEndPoint', 'MTTokenEndPoint', @ProductSettingTypeId OUTPUT;
	EXEC Enterprise.CreateProductSetting @ProductId, @ProductSettingTypeId, 'api/core/authentication/login', NULL, NULL, @ProductSettingId OUTPUT;
	EXEC Enterprise.LinkProductSettingToConfiguration @ConfigurationId, @ProductSettingId, NULL, NULL;
END;

SET @ProductSettingTypeId = NULL;

SET @ProductSettingId = NULL;

IF
(
	SELECT 1
	FROM Enterprise.ProductSettingType
	WHERE Name = 'MTClientId'
) IS NULL
BEGIN
	EXEC Enterprise.CreateProductSettingType 'MTClientId', 'MTClientId', @ProductSettingTypeId OUTPUT;
	EXEC Enterprise.CreateProductSetting @ProductId, @ProductSettingTypeId, 'Unified_Login', NULL, NULL, @ProductSettingId OUTPUT;
	EXEC Enterprise.LinkProductSettingToConfiguration @ConfigurationId, @ProductSettingId, NULL, NULL;
END;

SET @ProductSettingTypeId = NULL;

SET @ProductSettingId = NULL;

IF
(
	SELECT 1
	FROM Enterprise.ProductSettingType
	WHERE Name = 'MTClientSecret'
) IS NULL
BEGIN
	EXEC Enterprise.CreateProductSettingType 'MTClientSecret', 'MTClientSecret', @ProductSettingTypeId OUTPUT;
	EXEC Enterprise.CreateProductSetting @ProductId, @ProductSettingTypeId, '7C858876-B6D4-47AD-86DB-6B8FC95C4420', NULL, NULL, @ProductSettingId OUTPUT;
	EXEC Enterprise.LinkProductSettingToConfiguration @ConfigurationId, @ProductSettingId, NULL, NULL;
END;

/*-----------------------
--Script 59
-----------------------*/

-- Step 1 - In case product type is missing, add one
-- Removed because not needed

-- Step 2 - Add product

SET @ProductId = 28;

IF NOT EXISTS
(
	SELECT 1
	FROM Enterprise.Product
	WHERE Name = 'Product Updates'
)
BEGIN
	EXEC Enterprise.CreateProduct @ProductId, NULL, 'Product Updates', 'Product Updates', NULL;
END;

-- Step 3 - Add all settings

SET @FromDate = GETUTCDATE();

IF NOT EXISTS
(
	SELECT 1
	FROM Ident.SamlProductSettings
	WHERE ProductId = @ProductId
)
BEGIN
	EXEC Ident.CreateSamlProductSetting @ProductId, 'http://w2w.realpage.com/products/WNWC.asp', 'NA', 'productUsername', @SamlProductSettingId OUTPUT;
END;

IF NOT EXISTS
(
	SELECT 1
	FROM Enterprise.GlobalProductConfiguration
	WHERE ProductId = @ProductId
)
BEGIN
	EXEC Enterprise.CreateProductConfiguration @ConfigurationId OUTPUT;

	--***** Start a Product setting loop for each attribute / value that needs set. *****
	------------------------------------------------------------------------------------------------------------------------------------------------------
	-- Get the ProductSettingTypeId for the attribute you need to select, the API should have all these cached though.
	SET @ProductSettingId = NULL;
	SET @ProductSettingTypeId = NULL;
	EXEC Enterprise.GetProductSettingType @Name = 'ClassName', -- varchar(50)
	@ProductSettingTypeId = @ProductSettingTypeId OUTPUT; -- int

	-- Create the Value and assign it to the Product and ProductSettingType
	EXEC Enterprise.CreateProductSetting @ProductId = @ProductId, -- int
	@ProductSettingTypeId = @ProductSettingTypeId, -- int
	@Value = N'productupdates', -- nvarchar(1000)
	@FromDate = @FromDate, -- datetime
	@ThruDate = NULL, -- datetime
	@ProductSettingId = @ProductSettingId OUTPUT; -- int

	-- Link the Product Setting to an actual configuration
	EXEC Enterprise.LinkProductSettingToConfiguration @ConfigurationId = @ConfigurationId, -- int
	@ProductSettingId = @ProductSettingId, -- int
	@FromDate = @FromDate, -- datetime
	@ThruDate = NULL;   -- datetime

	------------------------------------------------------------------------------------------------------------------------------------------------------
	-- Get the ProductSettingTypeId for the attribute you need to select, the API should have all these cached though.
	SET @ProductSettingId = NULL;
	SET @ProductSettingTypeId = NULL;
	EXEC Enterprise.GetProductSettingType @Name = 'TitleId', -- varchar(50)
	@ProductSettingTypeId = @ProductSettingTypeId OUTPUT; -- int

	-- Create the Value and assign it to the Product and ProductSettingType
	EXEC Enterprise.CreateProductSetting @ProductId = @ProductId, -- int
	@ProductSettingTypeId = @ProductSettingTypeId, -- int
	@Value = N'Product Updates', -- nvarchar(1000)
	@FromDate = @FromDate, -- datetime
	@ThruDate = NULL, -- datetime
	@ProductSettingId = @ProductSettingId OUTPUT; -- int

	-- Link the Product Setting to an actual configuration
	EXEC Enterprise.LinkProductSettingToConfiguration @ConfigurationId = @ConfigurationId, -- int
	@ProductSettingId = @ProductSettingId, -- int
	@FromDate = @FromDate, -- datetime
	@ThruDate = NULL;   -- datetime

	------------------------------------------------------------------------------------------------------------------------------------------------------
	-- Get the ProductSettingTypeId for the attribute you need to select, the API should have all these cached though.
	SET @ProductSettingId = NULL;
	SET @ProductSettingTypeId = NULL;
	EXEC Enterprise.GetProductSettingType @Name = 'TitleUniqueId', -- varchar(50)
	@ProductSettingTypeId = @ProductSettingTypeId OUTPUT; -- int

	-- Create the Value and assign it to the Product and ProductSettingType
	EXEC Enterprise.CreateProductSetting @ProductId = @ProductId, -- int
	@ProductSettingTypeId = @ProductSettingTypeId, -- int
	@Value = '7F3DE727-1BBF-4D83-94A7-D4D8449E3703', -- nvarchar(1000)
	@FromDate = @FromDate, -- datetime
	@ThruDate = NULL, -- datetime
	@ProductSettingId = @ProductSettingId OUTPUT; -- int

	-- Link the Product Setting to an actual configuration
	EXEC Enterprise.LinkProductSettingToConfiguration @ConfigurationId = @ConfigurationId, -- int
	@ProductSettingId = @ProductSettingId, -- int
	@FromDate = @FromDate, -- datetime
	@ThruDate = NULL;   -- datetime

	------------------------------------------------------------------------------------------------------------------------------------------------------
	-- Get the ProductSettingTypeId for the attribute you need to select, the API should have all these cached though.
	SET @ProductSettingId = NULL;
	SET @ProductSettingTypeId = NULL;
	EXEC Enterprise.GetProductSettingType @Name = 'IsNewTab', -- varchar(50)
	@ProductSettingTypeId = @ProductSettingTypeId OUTPUT; -- int

	-- Create the Value and assign it to the Product and ProductSettingType
	EXEC Enterprise.CreateProductSetting @ProductId = @ProductId, -- int
	@ProductSettingTypeId = @ProductSettingTypeId, -- int
	@Value = N'1', -- nvarchar(1000)
	@FromDate = @FromDate, -- datetime
	@ThruDate = NULL, -- datetime
	@ProductSettingId = @ProductSettingId OUTPUT; -- int

	-- Link the Product Setting to an actual configuration
	EXEC Enterprise.LinkProductSettingToConfiguration @ConfigurationId = @ConfigurationId, -- int
	@ProductSettingId = @ProductSettingId, -- int
	@FromDate = @FromDate, -- datetime
	@ThruDate = NULL;   -- datetime

	------------------------------------------------------------------------------------------------------------------------------------------------------
	-- Get the ProductSettingTypeId for the attribute you need to select, the API should have all these cached though.
	SET @ProductSettingId = NULL;
	SET @ProductSettingTypeId = NULL;
	EXEC Enterprise.GetProductSettingType @Name = 'MetatagUniqueId', -- varchar(50)
	@ProductSettingTypeId = @ProductSettingTypeId OUTPUT; -- int

	-- Create the Value and assign it to the Product and ProductSettingType
	EXEC Enterprise.CreateProductSetting @ProductId = @ProductId, -- int
	@ProductSettingTypeId = @ProductSettingTypeId, -- int
	@Value = N'ProductUpdates', -- nvarchar(1000)
	@FromDate = @FromDate, -- datetime
	@ThruDate = NULL, -- datetime
	@ProductSettingId = @ProductSettingId OUTPUT; -- int

	-- Link the Product Setting to an actual configuration
	EXEC Enterprise.LinkProductSettingToConfiguration @ConfigurationId = @ConfigurationId, -- int
	@ProductSettingId = @ProductSettingId, -- int
	@FromDate = @FromDate, -- datetime
	@ThruDate = NULL;   -- datetime

	------------------------------------------------------------------------------------------------------------------------------------------------------
	-- Get the ProductSettingTypeId for the attribute you need to select, the API should have all these cached though.
	SET @ProductSettingId = NULL;
	SET @ProductSettingTypeId = NULL;
	EXEC Enterprise.GetProductSettingType @Name = 'IsFavorite', -- varchar(50)
	@ProductSettingTypeId = @ProductSettingTypeId OUTPUT; -- int

	-- Create the Value and assign it to the Product and ProductSettingType
	EXEC Enterprise.CreateProductSetting @ProductId = @ProductId, -- int
	@ProductSettingTypeId = @ProductSettingTypeId, -- int
	@Value = N'0', -- nvarchar(1000)
	@FromDate = @FromDate, -- datetime
	@ThruDate = NULL, -- datetime
	@ProductSettingId = @ProductSettingId OUTPUT; -- int

	-- Link the Product Setting to an actual configuration
	EXEC Enterprise.LinkProductSettingToConfiguration @ConfigurationId = @ConfigurationId, -- int
	@ProductSettingId = @ProductSettingId, -- int
	@FromDate = @FromDate, -- datetime
	@ThruDate = NULL;   -- datetime

	------------------------------------------------------------------------------------------------------------------------------------------------------
	-- Get the ProductSettingTypeId for the attribute you need to select, the API should have all these cached though.
	SET @ProductSettingId = NULL;
	SET @ProductSettingTypeId = NULL;
	EXEC Enterprise.GetProductSettingType @Name = 'IsResource', -- varchar(50)
	@ProductSettingTypeId = @ProductSettingTypeId OUTPUT; -- int

	-- Create the Value and assign it to the Product and ProductSettingType
	EXEC Enterprise.CreateProductSetting @ProductId = @ProductId, -- int
	@ProductSettingTypeId = @ProductSettingTypeId, -- int
	@Value = N'1', -- nvarchar(1000)
	@FromDate = @FromDate, -- datetime
	@ThruDate = NULL, -- datetime
	@ProductSettingId = @ProductSettingId OUTPUT; -- int

	-- Link the Product Setting to an actual configuration
	EXEC Enterprise.LinkProductSettingToConfiguration @ConfigurationId = @ConfigurationId, -- int
	@ProductSettingId = @ProductSettingId, -- int
	@FromDate = @FromDate, -- datetime
	@ThruDate = NULL;   -- datetime

	------------------------------------------------------------------------------------------------------------------------------------------------------
	-- Get the ProductSettingTypeId for the attribute you need to select, the API should have all these cached though.
	SET @ProductSettingId = NULL;
	SET @ProductSettingTypeId = NULL;
	EXEC Enterprise.GetProductSettingType @Name = 'LearnMore', -- varchar(50)
	@ProductSettingTypeId = @ProductSettingTypeId OUTPUT; -- int

	-- Create the Value and assign it to the Product and ProductSettingType
	EXEC Enterprise.CreateProductSetting @ProductId = @ProductId, -- int
	@ProductSettingTypeId = @ProductSettingTypeId, -- int
	@Value = N'https://www.realpage.com/', -- nvarchar(1000)
	@FromDate = @FromDate, -- datetime
	@ThruDate = NULL, -- datetime
	@ProductSettingId = @ProductSettingId OUTPUT; -- int

	-- Link the Product Setting to an actual configuration
	EXEC Enterprise.LinkProductSettingToConfiguration @ConfigurationId = @ConfigurationId, -- int
	@ProductSettingId = @ProductSettingId, -- int
	@FromDate = @FromDate, -- datetime
	@ThruDate = NULL;   -- datetime

	------------------------------------------------------------------------------------------------------------------------------------------------------
	-- Get the ProductSettingTypeId for the attribute you need to select, the API should have all these cached though.
	SET @ProductSettingId = NULL;
	SET @ProductSettingTypeId = NULL;
	EXEC Enterprise.GetProductSettingType @Name = 'ProductStatus', -- varchar(50)
	@ProductSettingTypeId = @ProductSettingTypeId OUTPUT; -- int

	-- Create the Value and assign it to the Product and ProductSettingType
	EXEC Enterprise.CreateProductSetting @ProductId = @ProductId, -- int
	@ProductSettingTypeId = @ProductSettingTypeId, -- int
	@Value = N'8', -- nvarchar(1000)
	@FromDate = @FromDate, -- datetime
	@ThruDate = NULL, -- datetime
	@ProductSettingId = @ProductSettingId OUTPUT; -- int

	-- Link the Product Setting to an actual configuration
	EXEC Enterprise.LinkProductSettingToConfiguration @ConfigurationId = @ConfigurationId, -- int
	@ProductSettingId = @ProductSettingId, -- int
	@FromDate = @FromDate, -- datetime
	@ThruDate = NULL;   -- datetime

	------------------------------------------------------------------------------------------------------------------------------------------------------
	-- Get the ProductSettingTypeId for the attribute you need to select, the API should have all these cached though.
	SET @ProductSettingId = NULL;
	SET @ProductSettingTypeId = NULL;
	EXEC Enterprise.GetProductSettingType @Name = 'ProductUrl', -- varchar(50)
	@ProductSettingTypeId = @ProductSettingTypeId OUTPUT; -- int

	-- Create the Value and assign it to the Product and ProductSettingType
	EXEC Enterprise.CreateProductSetting @ProductId = @ProductId, -- int
	@ProductSettingTypeId = @ProductSettingTypeId, -- int
	@Value = N'http://w2w.realpage.com/products/WNWC.asp', -- nvarchar(1000)
	@FromDate = @FromDate, -- datetime
	@ThruDate = NULL, -- datetime
	@ProductSettingId = @ProductSettingId OUTPUT; -- int

	-- Link the Product Setting to an actual configuration
	EXEC Enterprise.LinkProductSettingToConfiguration @ConfigurationId = @ConfigurationId, -- int
	@ProductSettingId = @ProductSettingId, -- int
	@FromDate = @FromDate, -- datetime
	@ThruDate = NULL;   -- datetime


	------------------------------------------------------------------------------------------------------------------------------------------------------
	EXEC Enterprise.LinkGlobalConfigurationToProduct @ConfigurationId = @ConfigurationId, -- int
	@ProductId = @ProductId, -- int
	@FromDate = @FromDate, -- datetime
	@ThruDate = NULL;   -- datetime

	EXEC Enterprise.ListGlobalSettingsForProduct @productid = @ProductId;
END;

WITH ProductUpdateGlobal(configid)
	 AS (
	 SELECT TOP 1 ConfigurationId
	 FROM enterprise.GlobalProductConfiguration
	 WHERE productid = @ProductId AND 
		   thrudate IS NULL)
	 INSERT INTO Enterprise.OrganizationProduct( PartyId, ConfigurationId, ProductId, FromDate )
			SELECT O.PartyId, PUG.configid, @ProductId AS productid, GETUTCDATE()
			FROM Enterprise.Organization AS O
				 LEFT OUTER JOIN
				 Enterprise.OrganizationProduct AS OP
				 ON O.PartyId = OP.PartyId AND 
					OP.ProductId = @ProductId AND 
					OP.THRUDATE IS NULL
				 CROSS JOIN
				 ProductUpdateGlobal AS PUG
			WHERE OP.partyid is null;

/*-----------------------------
Script 60 - Setting product Codes for
Activity Purposes.
-----------------------------*/

IF OBJECT_ID('tempdb..#BooksProductCode') IS NOT NULL
BEGIN
	DROP TABLE #BooksProductCode;
END;

CREATE TABLE #BooksProductCode
( 
			 ProductId int, BooksProductCode nvarchar(20)
);

INSERT INTO #BooksProductCode( ProductId, BooksProductCode )
VALUES( 1, 'OS' ), ( 2, 'UI' ), ( 3, 'UL' ), ( 4, 'AO' ), ( 5, 'PW' ), ( 6, 'L2L' ), ( 7, 'YS' ), ( 8, 'ACCT' ), ( 9, 'LS' ), ( 10, 'LVL1' ), ( 11, 'NULL' ), ( 12, 'OPSB' ), ( 13, 'SM' ), ( 14, 'OMS' ), ( 15, 'LD' ), ( 16, 'CD' ), ( 17, 'AB' ), ( 18, 'NWP' ), ( 19, 'LP' ), ( 20, 'DOC' ), ( 21, 'OSC' ), ( 22, 'OC' ), ( 23, 'ONST' ), ( 24, 'RA' ), ( 25, 'SP' ), ( 26, 'UA' ), ( 27, 'MT' ), ( 28, 'NULL' ), (35, 'ST');


UPDATE P
  SET
      P.BooksProductCode = PC.BooksProductCode
FROM Enterprise.Product P
     INNER JOIN #BooksProductCode PC ON PC.ProductId = P.ProductId;


/**-----------------------------
Scirpt 62 - Research Application
------------------------------**/

SET @ClientName = 'UnifiedLogin Research Application';

SET @ClientCode = 'UnifiedLoginResearchApp';

--Research Application

SET @ClientId = NULL;

SET @FromDate = GETUTCDATE();

-- If this is a new Global Product Configuration then create a Product Configuration if you do not have one to use.

SELECT @ProductId = ProductId
FROM Enterprise.Product
WHERE Name = 'Research Application';

DELETE FROM @AuthClients;

IF NOT EXISTS
(
	SELECT TOP 1 1
	FROM Auth.Scopes
	WHERE Name = @ClientCode
)
BEGIN
	INSERT INTO Auth.Scopes( Name, DisplayName, Description, ClaimsRule, Enabled, Required, Emphasize, Type, IncludeAllClaimsForUser, ShowInDiscoveryDocument, AllowUnrestrictedIntrospection )
	VALUES( @ClientCode, --Name,
	@ClientName, --DisplayName,
	NULL, --Description,
	NULL, --ClaimsRule,
	1, --Enabled,
	0, --Required,
	0, --Emphasize,
	1, --Type,
	1, --IncludeAllClaimsForUser,
	1, --ShowInDiscoveryDocument,
	0 --AllowUnrestrictedIntrospection
	);
END;

--ClientCredentials Flow

IF NOT EXISTS
(
	SELECT TOP 1 1
	FROM Auth.Clients
	WHERE ClientCode = @ClientCode AND 
		  Flow = 3
)
BEGIN
	INSERT INTO Auth.Clients( ClientCode, ClientName, ClientUri, LogoUri, Flow, LogoutUri, IdentityTokenLifetime, AccessTokenLifetime, AuthorizationCodeLifetime, AbsoluteRefreshTokenLifetime, SlidingRefreshTokenLifetime, RefreshTokenUsage, RefreshTokenExpiration, AccessTokenType, UpdateAccessTokenOnRefresh, Enabled, LogoutSessionRequired, RequireSignOutPrompt, AllowAccessToAllScopes, AllowClientCredentialsOnly, RequireConsent, AllowRememberConsent, EnableLocalLogin, IncludeJwtId, AlwaysSendClientClaims, PrefixClientClaims, AllowAccessToAllGrantTypes )
	OUTPUT INSERTED.ClientId
		   INTO @AuthClients(ClientId)
	VALUES( @ClientCode, --ClientCode
	@ClientName, --ClientName
	NULL, --ClientUri
	NULL, --LogoUri
	3, --ClientCredentials
	NULL, --LogoutUri
	36000, --IdentityTokenLifetime
	36000, --AccessTokenLifetime
	36000, --AuthorizationCodeLifetime
	86400, --AbsoluteRefreshTokenLifetime
	36000, --SlidingRefreshTokenLifetime
	0, --RefreshTokenUsage
	0, --RefreshTokenExpiration
	0, --AccessTokenType
	1, --UpdateAccessTokenOnRefresh
	1, --Enabled
	0, --LogoutSessionRequired
	0, --RequireSignOutPrompt
	1, --AllowAccessToAllScopes
	0, --AllowClientCredentialsOnly
	0, --RequireConsent
	1, --AllowRememberConsent
	1, --EnableLocalLogin
	1, --IncludeJwtId
	1, --AlwaysSendClientClaims
	1, --PrefixClientClaims
	1 --AllowAccessToAllGrantTypes
	);
	SELECT @ClientId = ClientId
	FROM @AuthClients;
	INSERT INTO Auth.ClientScopes( ClientId, Scope )
		   SELECT @ClientId, --ClientId
		   ScopeId --Scope
		   FROM Auth.Scopes
		   WHERE Name = @ClientCode;
	INSERT INTO Auth.ClientSecrets( ClientId, Value, Type, Description, Expiration )
	VALUES( @ClientId, 'RnLaeOYVnJjS7Qzy+l/93sveiwaoc7G26bHTYgSXiVg=', NULL, @ClientName, '2099-12-31 00:00:00.0000000 -06:00' );
END;

------------------------------------------------------------------------------------------------------------------------------------------------------

IF(@ProductId = 24)
BEGIN
	SELECT @ConfigurationId = pc.ConfigurationId
	FROM Enterprise.GlobalProductConfiguration AS gpc
		 JOIN
		 Enterprise.ProductConfiguration AS pc
		 ON pc.ConfigurationId = gpc.ConfigurationId
		 JOIN
		 Enterprise.ProductSetting AS ps
		 ON ps.ProductSettingId = pc.ProductSettingId
		 JOIN
		 Enterprise.ProductSettingType AS pst
		 ON pst.ProductSettingTypeId = ps.ProductSettingTypeId
	WHERE gpc.ProductId = @ProductId AND 
		  pst.Name = 'ClassName' AND 
		  ( ( @FromDate BETWEEN gpc.FromDate AND gpc.ThruDate
			) OR 
			( @FromDate >= gpc.FromDate AND 
			  gpc.ThruDate IS NULL
			)
		  ) AND 
		  ( ( @FromDate BETWEEN pc.FromDate AND pc.ThruDate
			) OR 
			( @FromDate >= pc.FromDate AND 
			  pc.ThruDate IS NULL
			)
		  ) AND 
		  ( ( @FromDate BETWEEN ps.FromDate AND ps.ThruDate
			) OR 
			( @FromDate >= ps.FromDate AND 
			  ps.ThruDate IS NULL
			)
		  ); 
	------------------------------------------------------------------------------------------------------------------------------------------------------
	-- Get the ProductSettingTypeId for the attribute you need to select, the API should have all these cached though.
	SET @ProductSettingId = NULL;
	SET @ProductSettingTypeId = NULL;
	EXEC Enterprise.GetProductSettingType @Name = 'ApiEndPoint', -- varchar(50)
	@ProductSettingTypeId = @ProductSettingTypeId OUTPUT; -- int

	-- Create the Value and assign it to the Product and ProductSettingType
	EXEC Enterprise.CreateProductSetting @ProductId = @ProductId, -- int
	@ProductSettingTypeId = @ProductSettingTypeId, -- int
	@Value = N'https://blackbookresearch-dev.realpage.com', -- nvarchar(1000)
	@FromDate = @FromDate, -- datetime
	@ThruDate = NULL, -- datetime
	@ProductSettingId = @ProductSettingId OUTPUT; -- int

	-- Link the Product Setting to an actual configuration
	EXEC Enterprise.LinkProductSettingToConfiguration @ConfigurationId = @ConfigurationId, -- int
	@ProductSettingId = @ProductSettingId, -- int
	@FromDate = @FromDate, -- datetime
	@ThruDate = NULL;   -- datetime

	EXEC Enterprise.ListGlobalSettingsForProduct @productid = @ProductId;
END;

SELECT @ProductId = ProductId
FROM Enterprise.Product
WHERE Name IN ( 'Unified Login', 'Unified Platform');;

IF(@ProductId = 3)
BEGIN
	SELECT @ConfigurationId = pc.ConfigurationId
	FROM Enterprise.GlobalProductConfiguration AS gpc
		 JOIN
		 Enterprise.ProductConfiguration AS pc
		 ON pc.ConfigurationId = gpc.ConfigurationId
		 JOIN
		 Enterprise.ProductSetting AS ps
		 ON ps.ProductSettingId = pc.ProductSettingId
		 JOIN
		 Enterprise.ProductSettingType AS pst
		 ON pst.ProductSettingTypeId = ps.ProductSettingTypeId
	WHERE gpc.ProductId = @ProductId AND 
		  pst.Name = 'ClientId' AND 
		  ( ( @FromDate BETWEEN gpc.FromDate AND gpc.ThruDate
			) OR 
			( @FromDate >= gpc.FromDate AND 
			  gpc.ThruDate IS NULL
			)
		  ) AND 
		  ( ( @FromDate BETWEEN pc.FromDate AND pc.ThruDate
			) OR 
			( @FromDate >= pc.FromDate AND 
			  pc.ThruDate IS NULL
			)
		  ) AND 
		  ( ( @FromDate BETWEEN ps.FromDate AND ps.ThruDate
			) OR 
			( @FromDate >= ps.FromDate AND 
			  ps.ThruDate IS NULL
			)
		  ); 

	------------------------------------------------------------------------------------------------------------------------------------------------------
	-- Get the ProductSettingTypeId for the attribute you need to select, the API should have all these cached though.
	SET @ProductSettingId = NULL;
	SET @ProductSettingTypeId = NULL;
	EXEC Enterprise.GetProductSettingType @Name = 'UnifiedLoginResearchApplicationClientSecret', -- varchar(50)
	@ProductSettingTypeId = @ProductSettingTypeId OUTPUT; -- int

	IF( @ProductSettingTypeId IS NULL
	  )
	BEGIN
		EXEC Enterprise.CreateProductSettingType @ProductSettingTypeName = 'UnifiedLoginResearchApplicationClientSecret', @ProductSettingTypeDescription = 'UnifiedLogin Research Application Client Secret', @ProductSettingTypeId = @ProductSettingTypeId OUTPUT;
	END;

	-- Create the Value and assign it to the Product and ProductSettingType
	EXEC Enterprise.CreateProductSetting @ProductId = @ProductId, -- int
	@ProductSettingTypeId = @ProductSettingTypeId, -- int
	@Value = N'MzNCNUY3OTgtQkU1NS00MkJDLThBQTgtMDAyNUI5MDNEQzNC', -- nvarchar(1000)
	@FromDate = @FromDate, -- datetime
	@ThruDate = NULL, -- datetime
	@ProductSettingId = @ProductSettingId OUTPUT; -- int

	-- Link the Product Setting to an actual configuration
	EXEC Enterprise.LinkProductSettingToConfiguration @ConfigurationId = @ConfigurationId, -- int
	@ProductSettingId = @ProductSettingId, -- int
	@FromDate = @FromDate, -- datetime
	@ThruDate = NULL;   -- datetime

	EXEC Enterprise.ListGlobalSettingsForProduct @productid = @ProductId;
END;

/**--------------------------------

Script 63 - Support Tools
-----------------------------------*/

SET @ProductId = 35;

IF NOT EXISTS
(
	SELECT 1
	FROM Enterprise.Product
	WHERE Name = 'Support Tool'
)
BEGIN
	EXEC Enterprise.CreateProduct @ProductId, NULL, 'Support Tool', 'Support Tool', NULL;
END;

EXEC Enterprise.CreateProductConfiguration @ConfigurationId = @ConfigurationId OUTPUT;

SET @ClientName = 'UnifiedLogin Support Tool';

SET @ClientCode = 'UnifiedLoginSupportTool';

--Research Application

SET @ClientId = NULL;

SET @FromDate = GETUTCDATE();

-- If this is a new Global Product Configuration then create a Product Configuration if you do not have one to use.

SELECT @ProductId = ProductId
FROM Enterprise.Product
WHERE Name = 'Support Tool';

DELETE FROM @AuthClients;

IF NOT EXISTS
(
	SELECT TOP 1 1
	FROM Auth.Scopes
	WHERE Name = @ClientCode
)
BEGIN
	INSERT INTO Auth.Scopes( Name, DisplayName, Description, ClaimsRule, Enabled, Required, Emphasize, Type, IncludeAllClaimsForUser, ShowInDiscoveryDocument, AllowUnrestrictedIntrospection )
	VALUES( @ClientCode, --Name,
	@ClientName, --DisplayName,
	NULL, --Description,
	NULL, --ClaimsRule,
	1, --Enabled,
	0, --Required,
	0, --Emphasize,
	1, --Type,
	1, --IncludeAllClaimsForUser,
	1, --ShowInDiscoveryDocument,
	0 --AllowUnrestrictedIntrospection
	);
END;

--ClientCredentials Flow

IF NOT EXISTS
(
	SELECT TOP 1 1
	FROM Auth.Clients
	WHERE ClientCode = @ClientCode AND 
		  Flow = 3
)
BEGIN
	INSERT INTO Auth.Clients( ClientCode, ClientName, ClientUri, LogoUri, Flow, LogoutUri, IdentityTokenLifetime, AccessTokenLifetime, AuthorizationCodeLifetime, AbsoluteRefreshTokenLifetime, SlidingRefreshTokenLifetime, RefreshTokenUsage, RefreshTokenExpiration, AccessTokenType, UpdateAccessTokenOnRefresh, Enabled, LogoutSessionRequired, RequireSignOutPrompt, AllowAccessToAllScopes, AllowClientCredentialsOnly, RequireConsent, AllowRememberConsent, EnableLocalLogin, IncludeJwtId, AlwaysSendClientClaims, PrefixClientClaims, AllowAccessToAllGrantTypes )
	OUTPUT INSERTED.ClientId
		   INTO @AuthClients(ClientId)
	VALUES( @ClientCode, --ClientCode
	@ClientName, --ClientName
	NULL, --ClientUri
	NULL, --LogoUri
	3, --ClientCredentials
	NULL, --LogoutUri
	36000, --IdentityTokenLifetime
	36000, --AccessTokenLifetime
	36000, --AuthorizationCodeLifetime
	86400, --AbsoluteRefreshTokenLifetime
	36000, --SlidingRefreshTokenLifetime
	0, --RefreshTokenUsage
	0, --RefreshTokenExpiration
	0, --AccessTokenType
	1, --UpdateAccessTokenOnRefresh
	1, --Enabled
	0, --LogoutSessionRequired
	0, --RequireSignOutPrompt
	1, --AllowAccessToAllScopes
	0, --AllowClientCredentialsOnly
	0, --RequireConsent
	1, --AllowRememberConsent
	1, --EnableLocalLogin
	1, --IncludeJwtId
	1, --AlwaysSendClientClaims
	1, --PrefixClientClaims
	1 --AllowAccessToAllGrantTypes
	);
	SELECT @ClientId = ClientId
	FROM @AuthClients;
	INSERT INTO Auth.ClientScopes( ClientId, Scope )
		   SELECT @ClientId, --ClientId
		   ScopeId --Scope
		   FROM Auth.Scopes
		   WHERE Name = @ClientCode;
	INSERT INTO Auth.ClientSecrets( ClientId, Value, Type, Description, Expiration )
	VALUES( @ClientId, 'RnLaeOYVnJjS7Qzy+l/93sveiwaoc7G26bHTYgSXiVg=', NULL, @ClientName, '2099-12-31 00:00:00.0000000 -06:00' );
END;

------------------------------------------------------------------------------------------------------------------------------------------------------

IF(@ProductId = 35)
BEGIN
	SELECT @ConfigurationId = pc.ConfigurationId
	FROM Enterprise.GlobalProductConfiguration AS gpc
		 JOIN
		 Enterprise.ProductConfiguration AS pc
		 ON pc.ConfigurationId = gpc.ConfigurationId
		 JOIN
		 Enterprise.ProductSetting AS ps
		 ON ps.ProductSettingId = pc.ProductSettingId
		 JOIN
		 Enterprise.ProductSettingType AS pst
		 ON pst.ProductSettingTypeId = ps.ProductSettingTypeId
	WHERE gpc.ProductId = @ProductId AND 
		  pst.Name = 'ClassName' AND 
		  ( ( @FromDate BETWEEN gpc.FromDate AND gpc.ThruDate
			) OR 
			( @FromDate >= gpc.FromDate AND 
			  gpc.ThruDate IS NULL
			)
		  ) AND 
		  ( ( @FromDate BETWEEN pc.FromDate AND pc.ThruDate
			) OR 
			( @FromDate >= pc.FromDate AND 
			  pc.ThruDate IS NULL
			)
		  ) AND 
		  ( ( @FromDate BETWEEN ps.FromDate AND ps.ThruDate
			) OR 
			( @FromDate >= ps.FromDate AND 
			  ps.ThruDate IS NULL
			)
		  ); 
	------------------------------------------------------------------------------------------------------------------------------------------------------
	-- Get the ProductSettingTypeId for the attribute you need to select, the API should have all these cached though.
	-- Commented out per Vinay - this does not need to be set for this product.
	SET @ProductSettingId = NULL;
	SET @ProductSettingTypeId = NULL;

	--EXEC Enterprise.GetProductSettingType
	--	@Name = 'ApiEndPoint',                                         -- varchar(50)
	--	@ProductSettingTypeId = @ProductSettingTypeId OUTPUT -- int

	---- Create the Value and assign it to the Product and ProductSettingType
	--EXEC Enterprise.CreateProductSetting
	--	@ProductId = @ProductId,                             -- int
	--	@ProductSettingTypeId = @ProductSettingTypeId,                  -- int
	--	@Value = N'https://mylocal.corp.realpage.com/#/employee-access',                               -- nvarchar(1000)
	--	@FromDate = @FromDate,          -- datetime
	--	@ThruDate = NULL,          -- datetime
	--	@ProductSettingId = @ProductSettingId OUTPUT -- int

	---- Link the Product Setting to an actual configuration
	--EXEC Enterprise.LinkProductSettingToConfiguration
	--	@ConfigurationId = @ConfigurationId,              -- int
	--	@ProductSettingId = @ProductSettingId,             -- int
	--	@FromDate = @FromDate, -- datetime
	--	@ThruDate = NULL   -- datetime

	EXEC Enterprise.ListGlobalSettingsForProduct @productid = @ProductId;
END;

---- Step 3 - Add all settings

SET @FromDate = GETUTCDATE();

IF NOT EXISTS
(
	SELECT 1
	FROM Ident.SamlProductSettings
	WHERE ProductId = @ProductId
)
BEGIN
	EXEC Ident.CreateSamlProductSetting @ProductId, 'https://mylocal.corp.realpage.com/#/employee-access', 'NA', 'productUsername', @SamlProductSettingId OUTPUT;
END;

IF NOT EXISTS
(
	SELECT 1
	FROM Enterprise.GlobalProductConfiguration
	WHERE ProductId = @ProductId
)
BEGIN
	--EXEC Enterprise.CreateProductConfiguration @ConfigurationId OUTPUT

	SELECT @ClientId = ClientId
	FROM Auth.Clients
	WHERE ClientCode = 'UnifiedLoginSupportTool';

	--***** Start a Product setting loop for each attribute / value that needs set. *****
	------------------------------------------------------------------------------------------------------------------------------------------------------
	-- Get the ProductSettingTypeId for the attribute you need to select, the API should have all these cached though.
	SET @ProductSettingId = NULL;
	SET @ProductSettingTypeId = NULL;
	EXEC Enterprise.GetProductSettingType @Name = 'ClientId', -- varchar(50)
	@ProductSettingTypeId = @ProductSettingTypeId OUTPUT; -- int

	-- Create the Value and assign it to the Product and ProductSettingType
	EXEC Enterprise.CreateProductSetting @ProductId = @ProductId, -- int
	@ProductSettingTypeId = @ProductSettingTypeId, -- int
	@Value = @clientID, -- nvarchar(1000)
	@FromDate = @FromDate, -- datetime
	@ThruDate = NULL, -- datetime
	@ProductSettingId = @ProductSettingId OUTPUT; -- int

	-- Link the Product Setting to an actual configuration
	EXEC Enterprise.LinkProductSettingToConfiguration @ConfigurationId = @ConfigurationId, -- int
	@ProductSettingId = @ProductSettingId, -- int
	@FromDate = @FromDate, -- datetime
	@ThruDate = NULL;   -- datetime

		------------------------------------------------------------------------------------------------------------------------------------------------------
	-- Get the ProductSettingTypeId for the attribute you need to select, the API should have all these cached though.
	SET @ProductSettingId = NULL;
	SET @ProductSettingTypeId = NULL;
	EXEC Enterprise.GetProductSettingType @Name = 'ClassName', -- varchar(50)
	@ProductSettingTypeId = @ProductSettingTypeId OUTPUT; -- int

	-- Create the Value and assign it to the Product and ProductSettingType
	EXEC Enterprise.CreateProductSetting @ProductId = @ProductId, -- int
	@ProductSettingTypeId = @ProductSettingTypeId, -- int
	@Value = N'supporttool', -- nvarchar(1000)
	@FromDate = @FromDate, -- datetime
	@ThruDate = NULL, -- datetime
	@ProductSettingId = @ProductSettingId OUTPUT; -- int

	-- Link the Product Setting to an actual configuration
	EXEC Enterprise.LinkProductSettingToConfiguration @ConfigurationId = @ConfigurationId, -- int
	@ProductSettingId = @ProductSettingId, -- int
	@FromDate = @FromDate, -- datetime
	@ThruDate = NULL;   -- datetime

	------------------------------------------------------------------------------------------------------------------------------------------------------
	-- Get the ProductSettingTypeId for the attribute you need to select, the API should have all these cached though.
	SET @ProductSettingId = NULL;
	SET @ProductSettingTypeId = NULL;
	EXEC Enterprise.GetProductSettingType @Name = 'TitleId', -- varchar(50)
	@ProductSettingTypeId = @ProductSettingTypeId OUTPUT; -- int

	-- Create the Value and assign it to the Product and ProductSettingType
	EXEC Enterprise.CreateProductSetting @ProductId = @ProductId, -- int
	@ProductSettingTypeId = @ProductSettingTypeId, -- int
	@Value = N'Support Tool', -- nvarchar(1000)
	@FromDate = @FromDate, -- datetime
	@ThruDate = NULL, -- datetime
	@ProductSettingId = @ProductSettingId OUTPUT; -- int

	-- Link the Product Setting to an actual configuration
	EXEC Enterprise.LinkProductSettingToConfiguration @ConfigurationId = @ConfigurationId, -- int
	@ProductSettingId = @ProductSettingId, -- int
	@FromDate = @FromDate, -- datetime
	@ThruDate = NULL;   -- datetime

	------------------------------------------------------------------------------------------------------------------------------------------------------
	-- Get the ProductSettingTypeId for the attribute you need to select, the API should have all these cached though.
	SET @ProductSettingId = NULL;
	SET @ProductSettingTypeId = NULL;
	SET @TitleUniqueId = NEWID();
	EXEC Enterprise.GetProductSettingType @Name = 'TitleUniqueId', -- varchar(50)
	@ProductSettingTypeId = @ProductSettingTypeId OUTPUT; -- int

	-- Create the Value and assign it to the Product and ProductSettingType
	EXEC Enterprise.CreateProductSetting @ProductId = @ProductId, -- int
	@ProductSettingTypeId = @ProductSettingTypeId, -- int
	@Value = @TitleUniqueId, -- nvarchar(1000)
	@FromDate = @FromDate, -- datetime
	@ThruDate = NULL, -- datetime
	@ProductSettingId = @ProductSettingId OUTPUT; -- int

	-- Link the Product Setting to an actual configuration
	EXEC Enterprise.LinkProductSettingToConfiguration @ConfigurationId = @ConfigurationId, -- int
	@ProductSettingId = @ProductSettingId, -- int
	@FromDate = @FromDate, -- datetime
	@ThruDate = NULL;   -- datetime

	------------------------------------------------------------------------------------------------------------------------------------------------------
	-- Get the ProductSettingTypeId for the attribute you need to select, the API should have all these cached though.
	SET @ProductSettingId = NULL;
	SET @ProductSettingTypeId = NULL;
	EXEC Enterprise.GetProductSettingType @Name = 'IsNewTab', -- varchar(50)
	@ProductSettingTypeId = @ProductSettingTypeId OUTPUT; -- int

	-- Create the Value and assign it to the Product and ProductSettingType
	EXEC Enterprise.CreateProductSetting @ProductId = @ProductId, -- int
	@ProductSettingTypeId = @ProductSettingTypeId, -- int
	@Value = N'0', -- nvarchar(1000)
	@FromDate = @FromDate, -- datetime
	@ThruDate = NULL, -- datetime
	@ProductSettingId = @ProductSettingId OUTPUT; -- int

	-- Link the Product Setting to an actual configuration
	EXEC Enterprise.LinkProductSettingToConfiguration @ConfigurationId = @ConfigurationId, -- int
	@ProductSettingId = @ProductSettingId, -- int
	@FromDate = @FromDate, -- datetime
	@ThruDate = NULL;   -- datetime

	------------------------------------------------------------------------------------------------------------------------------------------------------
	-- Get the ProductSettingTypeId for the attribute you need to select, the API should have all these cached though.
	SET @ProductSettingId = NULL;
	SET @ProductSettingTypeId = NULL;
	EXEC Enterprise.GetProductSettingType @Name = 'MetatagUniqueId', -- varchar(50)
	@ProductSettingTypeId = @ProductSettingTypeId OUTPUT; -- int

	-- Create the Value and assign it to the Product and ProductSettingType
	EXEC Enterprise.CreateProductSetting @ProductId = @ProductId, -- int
	@ProductSettingTypeId = @ProductSettingTypeId, -- int
	@Value = N'SupportTool', -- nvarchar(1000)
	@FromDate = @FromDate, -- datetime
	@ThruDate = NULL, -- datetime
	@ProductSettingId = @ProductSettingId OUTPUT; -- int

	-- Link the Product Setting to an actual configuration
	EXEC Enterprise.LinkProductSettingToConfiguration @ConfigurationId = @ConfigurationId, -- int
	@ProductSettingId = @ProductSettingId, -- int
	@FromDate = @FromDate, -- datetime
	@ThruDate = NULL;   -- datetime

	------------------------------------------------------------------------------------------------------------------------------------------------------
	-- Get the ProductSettingTypeId for the attribute you need to select, the API should have all these cached though.
	SET @ProductSettingId = NULL;
	SET @ProductSettingTypeId = NULL;
	EXEC Enterprise.GetProductSettingType @Name = 'IsFavorite', -- varchar(50)
	@ProductSettingTypeId = @ProductSettingTypeId OUTPUT; -- int

	-- Create the Value and assign it to the Product and ProductSettingType
	EXEC Enterprise.CreateProductSetting @ProductId = @ProductId, -- int
	@ProductSettingTypeId = @ProductSettingTypeId, -- int
	@Value = N'0', -- nvarchar(1000)
	@FromDate = @FromDate, -- datetime
	@ThruDate = NULL, -- datetime
	@ProductSettingId = @ProductSettingId OUTPUT; -- int

	-- Link the Product Setting to an actual configuration
	EXEC Enterprise.LinkProductSettingToConfiguration @ConfigurationId = @ConfigurationId, -- int
	@ProductSettingId = @ProductSettingId, -- int
	@FromDate = @FromDate, -- datetime
	@ThruDate = NULL;   -- datetime

	------------------------------------------------------------------------------------------------------------------------------------------------------
	-- Get the ProductSettingTypeId for the attribute you need to select, the API should have all these cached though.
	SET @ProductSettingId = NULL;
	SET @ProductSettingTypeId = NULL;
	EXEC Enterprise.GetProductSettingType @Name = 'IsResource', -- varchar(50)
	@ProductSettingTypeId = @ProductSettingTypeId OUTPUT; -- int

	-- Create the Value and assign it to the Product and ProductSettingType
	EXEC Enterprise.CreateProductSetting @ProductId = @ProductId, -- int
	@ProductSettingTypeId = @ProductSettingTypeId, -- int
	@Value = N'1', -- nvarchar(1000)
	@FromDate = @FromDate, -- datetime
	@ThruDate = NULL, -- datetime
	@ProductSettingId = @ProductSettingId OUTPUT; -- int

	-- Link the Product Setting to an actual configuration
	EXEC Enterprise.LinkProductSettingToConfiguration @ConfigurationId = @ConfigurationId, -- int
	@ProductSettingId = @ProductSettingId, -- int
	@FromDate = @FromDate, -- datetime
	@ThruDate = NULL;   -- datetime

	------------------------------------------------------------------------------------------------------------------------------------------------------
	-- Get the ProductSettingTypeId for the attribute you need to select, the API should have all these cached though.
	SET @ProductSettingId = NULL;
	SET @ProductSettingTypeId = NULL;
	EXEC Enterprise.GetProductSettingType @Name = 'LearnMore', -- varchar(50)
	@ProductSettingTypeId = @ProductSettingTypeId OUTPUT; -- int

	-- Create the Value and assign it to the Product and ProductSettingType
	EXEC Enterprise.CreateProductSetting @ProductId = @ProductId, -- int
	@ProductSettingTypeId = @ProductSettingTypeId, -- int
	@Value = N'https://www.realpage.com/', -- nvarchar(1000)
	@FromDate = @FromDate, -- datetime
	@ThruDate = NULL, -- datetime
	@ProductSettingId = @ProductSettingId OUTPUT; -- int

	-- Link the Product Setting to an actual configuration
	EXEC Enterprise.LinkProductSettingToConfiguration @ConfigurationId = @ConfigurationId, -- int
	@ProductSettingId = @ProductSettingId, -- int
	@FromDate = @FromDate, -- datetime
	@ThruDate = NULL;   -- datetime

	------------------------------------------------------------------------------------------------------------------------------------------------------
	-- Get the ProductSettingTypeId for the attribute you need to select, the API should have all these cached though.
	SET @ProductSettingId = NULL;
	SET @ProductSettingTypeId = NULL;
	EXEC Enterprise.GetProductSettingType @Name = 'ProductStatus', -- varchar(50)
	@ProductSettingTypeId = @ProductSettingTypeId OUTPUT; -- int

	-- Create the Value and assign it to the Product and ProductSettingType
	EXEC Enterprise.CreateProductSetting @ProductId = @ProductId, -- int
	@ProductSettingTypeId = @ProductSettingTypeId, -- int
	@Value = N'8', -- nvarchar(1000)
	@FromDate = @FromDate, -- datetime
	@ThruDate = NULL, -- datetime
	@ProductSettingId = @ProductSettingId OUTPUT; -- int

	-- Link the Product Setting to an actual configuration
	EXEC Enterprise.LinkProductSettingToConfiguration @ConfigurationId = @ConfigurationId, -- int
	@ProductSettingId = @ProductSettingId, -- int
	@FromDate = @FromDate, -- datetime
	@ThruDate = NULL;   -- datetime

	------------------------------------------------------------------------------------------------------------------------------------------------------
	-- Get the ProductSettingTypeId for the attribute you need to select, the API should have all these cached though.
	SET @ProductSettingId = NULL;
	SET @ProductSettingTypeId = NULL;
	EXEC Enterprise.GetProductSettingType @Name = 'ProductUrl', -- varchar(50)
	@ProductSettingTypeId = @ProductSettingTypeId OUTPUT; -- int

	-- Create the Value and assign it to the Product and ProductSettingType
	EXEC Enterprise.CreateProductSetting @ProductId = @ProductId, -- int
	@ProductSettingTypeId = @ProductSettingTypeId, -- int
	@Value = N'/#/employee-access', -- nvarchar(1000)
	@FromDate = @FromDate, -- datetime
	@ThruDate = NULL, -- datetime
	@ProductSettingId = @ProductSettingId OUTPUT; -- int

	-- Link the Product Setting to an actual configuration
	EXEC Enterprise.LinkProductSettingToConfiguration @ConfigurationId = @ConfigurationId, -- int
	@ProductSettingId = @ProductSettingId, -- int
	@FromDate = @FromDate, -- datetime
	@ThruDate = NULL;   -- datetime


	------------------------------------------------------------------------------------------------------------------------------------------------------
	EXEC Enterprise.LinkGlobalConfigurationToProduct @ConfigurationId = @ConfigurationId, -- int
	@ProductId = @ProductId, -- int
	@FromDate = @FromDate, -- datetime
	@ThruDate = NULL;   -- datetime

	EXEC Enterprise.ListGlobalSettingsForProduct @productid = @ProductId;
END;



/*---------------------------
Script 64
---------------------------*/

--GB-854
SET @ClientId = NULL
SET @FromDate = GETUTCDATE();


IF NOT EXISTS
(
	SELECT 1
	FROM Auth.Scopes
	WHERE Name = 'settings-management-tool'
)
BEGIN
	INSERT INTO Auth.Scopes( Name, DisplayName, Enabled, Required, Emphasize, Type, IncludeAllClaimsForUser, ShowInDiscoveryDocument, AllowUnrestrictedIntrospection )
	VALUES( 'settings-management-tool', 'Settings Management Tool', 1, 0, 1, 1, 1, 1, 0 );
END;

IF NOT EXISTS
(
	SELECT 1
	FROM Auth.Scopes
	WHERE Name = 'unifiedsettingsapi'
)
BEGIN
	INSERT INTO Auth.Scopes( Name, DisplayName, Enabled, Required, Emphasize, Type, IncludeAllClaimsForUser, ShowInDiscoveryDocument, AllowUnrestrictedIntrospection )
	VALUES( 'unifiedsettingsapi', 'Unified Settings API', 1, 0, 1, 1, 1, 1, 0 );
END;

SELECT @ClientId = ClientId
FROM Auth.Clients
WHERE ClientCode = 'settings-management' AND 
	  Flow = 2;

IF @ClientId IS NULL
BEGIN
	INSERT INTO Auth.Clients( ClientCode, ClientName, Flow, IdentityTokenLifetime, AccessTokenLifetime, AuthorizationCodeLifetime, AbsoluteRefreshTokenLifetime, SlidingRefreshTokenLifetime, RefreshTokenUsage, RefreshTokenExpiration, AccessTokenType, UpdateAccessTokenOnRefresh, Enabled, LogoutSessionRequired, RequireSignOutPrompt, AllowAccessToAllScopes, AllowClientCredentialsOnly, RequireConsent, AllowRememberConsent, EnableLocalLogin, IncludeJwtId, AlwaysSendClientClaims, PrefixClientClaims, AllowAccessToAllGrantTypes )
	VALUES( 'settings-management', 'Settings Management', 2, 360, 3600, 360, 86400, 3600, 0, 0, 1, 'True', 'True', 'False', 'False', 'True', 'False', 'False', 'True', 'True', 'True', 'True', 'True', 'True' );
	SELECT @ClientId = SCOPE_IDENTITY();
	INSERT INTO Auth.ClientScopes( ClientId, Scope )
	VALUES( @ClientId, 'settings-management-tool' );
	INSERT INTO Auth.ClientScopes( ClientId, Scope )
	VALUES( @ClientId, 'unifiedsettingsapi' );
	INSERT INTO Auth.ClientScopes( ClientId, Scope )
	VALUES( @ClientId, 'offline_access' );
	INSERT INTO Auth.ClientSecrets( ClientId, Value, Description, Expiration )
	VALUES( @ClientId, 'VzsmfPiRyUunha36V6LY0EOFIHkqlB6w0coPJDfmHbY=', 'Settings Management Console Application', '2099-12-31 00:00:00.0000000 -06:00' );
	
	IF @@SERVERNAME = 'RCDUSODBSQL001'
	BEGIN
		IF DB_NAME() = 'IdentityDevelopment'
		BEGIN
			INSERT INTO Auth.ClientRedirectUris( ClientId, Uri )
			VALUES( @ClientId, 'https://settings-management-dev.corp.realpage.com/signin-oidc' )
		END;
		IF DB_NAME() = 'Identity'
		BEGIN
			INSERT INTO Auth.ClientRedirectUris( ClientId, Uri )
			VALUES( @ClientId, 'https://settings-management-Dev.realpage.com/signin-oidc' )
		END;
	END;
	
	IF @@SERVERNAME = 'RCTUSODBSQL001'
	BEGIN
		INSERT INTO Auth.ClientRedirectUris( ClientId, Uri )
		VALUES( @ClientId, 'https://settings-management-qa.realpage.com/signin-oidc' );
		INSERT INTO Auth.ClientRedirectUris(ClientId, Uri)
		VALUES(@ClientId, 'https://settings-management-int.realpage.com/signin-oidc')

	END;
	
	
	IF @@SERVERNAME = 'RCQUSODBSQL001'
	BEGIN
		INSERT INTO Auth.ClientRedirectUris( ClientId, Uri )
		VALUES( @ClientId, 'https://settings-management-sat.realpage.com/signin-oidc' );
	END;
	IF @@SERVERNAME IN('RCPGBKDBSQL005A', 'RCPGBKDBSQL005B')
	BEGIN
		INSERT INTO Auth.ClientRedirectUris( ClientId, Uri )
		VALUES( @ClientId, 'https://settings-management.realpage.com/signin-oidc' );
		INSERT INTO Auth.ClientRedirectUris(ClientId, Uri)
		VALUES(@ClientId, 'https://settings-management-demo.realpage.com/signin-oidc')
	
	END;
END;

/*------------------------
--GB-853
------------------------*/

IF NOT EXISTS
(
    SELECT *
    FROM Enterprise.ProductSettingType
    WHERE Name = 'TokenEndPoint'
)
    BEGIN
        EXEC Enterprise.CreateProductSettingType
             'TokenEndPoint',
             'TokenEndPoint',
             @ProductSettingTypeId OUTPUT;
    END;
    ELSE
    BEGIN
        SELECT @ProductSettingTypeId = ProductSettingTypeId
        FROM Enterprise.ProductSettingType
        WHERE Name = 'TokenEndPoint';
    END;

SELECT @ProductId = ProductId
FROM Enterprise.Product
WHERE Name = 'Vendor Services';

SET @FromDate = GETUTCDATE();
IF  @@SERVERNAME = 'RCDUSODBSQL001'
    BEGIN
        EXEC Enterprise.CreateProductSetting
             @ProductId,
             @ProductSettingTypeId,
             'https://mylsat.realpage.com/identity/connect/token',
             @FromDate,
             NULL,
             @ProductSettingId OUTPUT;
		
        SELECT @COnfigurationId = ConfigurationId
        FROM Enterprise.GlobalProductConfiguration
        WHERE ProductId = @ProductId
              AND ThruDate IS NULL;
	
        EXEC Enterprise.LinkProductSettingToConfiguration
             @ConfigurationId,
             @ProductSettingID,
             NULL,
             NULL;
    END;

IF @@SERVERNAME = 'RCTUSODBSQL001'
    BEGIN
        EXEC Enterprise.CreateProductSetting
             @ProductId,
             @ProductSettingTypeId,
             'https://mylqa.realpage.com/identity/connect/token',
             @FromDate,
             NULL,
             @ProductSettingId OUTPUT;
        SELECT @ConfigurationId = ConfigurationId
        FROM Enterprise.GlobalProductConfiguration
        WHERE ProductId = @ProductId
              AND ThruDate IS NULL;
        EXEC Enterprise.LinkProductSettingToConfiguration
             @ConfigurationId,
             @ProductSettingID,
             NULL,
             NULL;
    END;
IF @@SERVERNAME = 'RCQUSODBSQL001'
    BEGIN
        EXEC Enterprise.CreateProductSetting
             @ProductId,
             @ProductSettingTypeId,
             'https://mylsat.realpage.com/identity/connect/token',
             @FromDate,
             NULL,
             @ProductSettingId OUTPUT;
        SELECT @ConfigurationId = ConfigurationId
        FROM Enterprise.GlobalProductConfiguration
        WHERE ProductId = @ProductId
              AND ThruDate IS NULL;
        EXEC Enterprise.LinkProductSettingToConfiguration
             @ConfigurationId,
             @ProductSettingID,
             NULL,
             NULL;
    END;
IF @@SERVERNAME IN('RCPGBKDBSQL005A', 'RCPGBKDBSQL005B')
    BEGIN
        EXEC Enterprise.CreateProductSetting
             @ProductId,
             @ProductSettingTypeId,
             'https://myl.realpage.com/identity/connect/token',
             @FromDate,
             NULL,
             @ProductSettingId OUTPUT;
        SELECT @ConfigurationId = ConfigurationId
        FROM Enterprise.GlobalProductConfiguration
        WHERE ProductId = @ProductId
              AND ThruDate IS NULL;
        EXEC Enterprise.LinkProductSettingToConfiguration
             @ConfigurationId,
             @ProductSettingID,
             NULL,
             NULL;
    END;

	EXECUTE [Enterprise].[ListGlobalSettingsForProduct] @ProductId = @ProductId

/*********************************
--GB-757/GB-817
*********************************/
IF EXISTS
(
	SELECT 1
	FROM enterprise.rightvaluetype
	WHERE value = 'Ability to create and manage users for Resident Portals'
)
BEGIN
	UPDATE [Enterprise].[RightValueType]
	  SET ShortName = 'AddEditResidentPortalUser'
	WHERE value = 'Ability to create and manage users for Resident Portals';
END;

IF EXISTS
(
	SELECT 1
	FROM Enterprise.Product
	WHERE Name = 'Unified Login'
)
BEGIN
	UPDATE Enterprise.Product
	  SET Name = 'Unified Platform'
	WHERE Name = 'Unified Login';
END;

IF EXISTS
(
	SELECT 1
	FROM Enterprise.Product
	WHERE Name = 'Websites & Syndication'
)
BEGIN
	UPDATE Enterprise.Product
	  SET Name = 'Marketing Center'
	WHERE Name = 'Websites & Syndication';
END;

-- Move Unified Amenities to Property Management family GB42
if exists ( select top 1 1 from enterprise.product where productid = 26 and ProductTypeId = 505 )
begin
       UPDATE Enterprise.Product set ProductTypeId = 100 where productid = 26
       UPDATE Enterprise.ProductType SET ProductTypeId = 107, ParentProductTypeId = 100 WHERE ProductTypeId = 505 and Name = 'Unified Amenities'
       UPDATE Enterprise.Product set ProductTypeId = 107 where productid = 26     
end

;with UnifiedAmenitiesResource ( productsettingid ) as 
( select ps.ProductSettingId from enterprise.GlobalProductConfiguration gpc inner join enterprise.ProductConfiguration pc on gpc.ConfigurationId = pc.ConfigurationId 
       inner join enterprise.productsetting ps on pc.ProductSettingId = ps.ProductSettingId inner join enterprise.ProductSettingType pst on ps.ProductSettingTypeId = pst.ProductSettingTypeId
       where gpc.productid = 26 and pst.Name = 'IsResource' and ps.value = '1'
)
update ps 
set value = '0'
--select *
from Enterprise.ProductSetting PS 
inner join UnifiedAmenitiesResource UAR on PS.ProductSettingId = UAR.productsettingid

-- Move Unified Amenities to Property Management family GB42

-- Adding new Scope for Property picker GB42

if not exists ( select top 1 1 from auth.Scopes where name = 'userinfoapi' )
begin
       insert into auth.Scopes ( name, DisplayName, Enabled, Required, Emphasize, Type, IncludeAllClaimsForUser, ShowInDiscoveryDocument, AllowUnrestrictedIntrospection )
              values ( 'userinfoapi', 'User Information API', 1, 0, 0, 1, 1, 1, 0 )
end

/****************************
Add Property Photos to 
system  GB585/GB-874
****************************/

SET @ClientName = 'PropertyPhotos Application'
SET @ClientCode = 'propertyphotos'

--Research Application
SET @ClientId = NULL
SET @FromDate = GETUTCDATE();


IF NOT EXISTS(SELECT TOP 1 1 FROM Auth.Scopes WHERE Name = @ClientCode)
BEGIN
       INSERT INTO Auth.Scopes (
              Name,
              DisplayName,
              Description,
              ClaimsRule,
              Enabled,
              Required,
              Emphasize,
              Type,
              IncludeAllClaimsForUser,
              ShowInDiscoveryDocument,
              AllowUnrestrictedIntrospection
       )
       VALUES (
              @ClientCode, --Name,
              @ClientName, --DisplayName,
              NULL, --Description,
              NULL, --ClaimsRule,
              1, --Enabled,
              0, --Required,
              0, --Emphasize,
              1, --Type,
              1, --IncludeAllClaimsForUser,
              1, --ShowInDiscoveryDocument,
              0 --AllowUnrestrictedIntrospection
       )
END

--ClientCredentials Flow
IF NOT EXISTS(SELECT TOP 1 1 FROM Auth.Clients WHERE ClientCode = @ClientCode AND Flow = 0)
BEGIN
       INSERT INTO Auth.Clients (
                                  ClientCode,
                                  ClientName,
                                  ClientUri,
                                  LogoUri,
                                  Flow,
                                  LogoutUri,
                                  IdentityTokenLifetime,
                                  AccessTokenLifetime,
                                  AuthorizationCodeLifetime,
                                  AbsoluteRefreshTokenLifetime,
                                  SlidingRefreshTokenLifetime,
                                  RefreshTokenUsage,
                                  RefreshTokenExpiration,
                                  AccessTokenType,
                                  UpdateAccessTokenOnRefresh,
                                  Enabled,
                                  LogoutSessionRequired,
                                  RequireSignOutPrompt,
                                  AllowAccessToAllScopes,
                                  AllowClientCredentialsOnly,
                                  RequireConsent,
                                  AllowRememberConsent,
                                  EnableLocalLogin,
                                  IncludeJwtId,
                                  AlwaysSendClientClaims,
                                  PrefixClientClaims,
                                  AllowAccessToAllGrantTypes
       )
       OUTPUT INSERTED.ClientId INTO @AuthClients (ClientId)
       VALUES (
                                  @ClientCode, --ClientCode
                                  @ClientName, --ClientName
                                  NULL, --ClientUri
                                  NULL, --LogoUri
                                  0, --ClientCredentials
                                  NULL, --LogoutUri
                                  360, --IdentityTokenLifetime
                                  3600, --AccessTokenLifetime
                                  360, --AuthorizationCodeLifetime
                                  86400, --AbsoluteRefreshTokenLifetime
                                  3600, --SlidingRefreshTokenLifetime
                                  0, --RefreshTokenUsage
                                  0, --RefreshTokenExpiration
                                  0, --AccessTokenType
                                  1, --UpdateAccessTokenOnRefresh
                                  1, --Enabled
                                  0, --LogoutSessionRequired
                                  0, --RequireSignOutPrompt
                                  1, --AllowAccessToAllScopes
                                  0, --AllowClientCredentialsOnly
                                  0, --RequireConsent
                                  1, --AllowRememberConsent
                                  1, --EnableLocalLogin
                                  1, --IncludeJwtId
                                  1, --AlwaysSendClientClaims
                                  1, --PrefixClientClaims
                                  1 --AllowAccessToAllGrantTypes
       )

       SELECT @ClientId = ClientId
       FROM   @AuthClients

       INSERT INTO Auth.ClientScopes (
              ClientId,
              Scope
       )
       SELECT @ClientId, --ClientId
                                  ScopeId --Scope
       FROM          Auth.Scopes
       WHERE Name = @ClientCode

       INSERT INTO Auth.ClientSecrets (
              ClientId,
              Value,
              Type,
              Description,
              Expiration
       )
       VALUES (
              @ClientId,
              'iQbkvbt652bRw5DgmRYsQbFQ5ZqTQ9oEZLgzouhg+BI=',
              NULL,
              @ClientName,
              '2099-12-31 00:00:00.0000000 -06:00'
       )

       INSERT INTO Auth.ClientRedirectUris (
              ClientId,
              Uri
       )
       VALUES (
              @ClientId,
              'http://photo.localhost/greenbook/login'
       )

END





/******************************
Migration tool - Client Scopes
******************************/



SET @ClientName = 'ULMT Staging Service'
SET @ClientCode = 'ULMTStagingService'

--Research Application
SET @ClientId = NULL
SET @FromDate = GETUTCDATE();


DELETE
FROM	@AuthClients;

IF NOT EXISTS(SELECT TOP 1 1 FROM Auth.Scopes WHERE Name = @ClientCode)
BEGIN
	INSERT INTO Auth.Scopes (
		Name,
		DisplayName,
		Description,
		ClaimsRule,
		Enabled,
		Required,
		Emphasize,
		Type,
		IncludeAllClaimsForUser,
		ShowInDiscoveryDocument,
		AllowUnrestrictedIntrospection
	)
	VALUES (
		@ClientCode, --Name,
		@ClientName, --DisplayName,
		NULL, --Description,
		NULL, --ClaimsRule,
		1, --Enabled,
		0, --Required,
		0, --Emphasize,
		1, --Type,
		1, --IncludeAllClaimsForUser,
		1, --ShowInDiscoveryDocument,
		0 --AllowUnrestrictedIntrospection
	)
END

--ClientCredentials Flow
IF NOT EXISTS(SELECT TOP 1 1 FROM Auth.Clients WHERE ClientCode = @ClientCode AND Flow = 3)
BEGIN
	INSERT INTO Auth.Clients (
					ClientCode,
					ClientName,
					ClientUri,
					LogoUri,
					Flow,
					LogoutUri,
					IdentityTokenLifetime,
					AccessTokenLifetime,
					AuthorizationCodeLifetime,
					AbsoluteRefreshTokenLifetime,
					SlidingRefreshTokenLifetime,
					RefreshTokenUsage,
					RefreshTokenExpiration,
					AccessTokenType,
					UpdateAccessTokenOnRefresh,
					Enabled,
					LogoutSessionRequired,
					RequireSignOutPrompt,
					AllowAccessToAllScopes,
					AllowClientCredentialsOnly,
					RequireConsent,
					AllowRememberConsent,
					EnableLocalLogin,
					IncludeJwtId,
					AlwaysSendClientClaims,
					PrefixClientClaims,
					AllowAccessToAllGrantTypes
	)
	OUTPUT INSERTED.ClientId INTO @AuthClients (ClientId)
	VALUES (
					@ClientCode, --ClientCode
					@ClientName, --ClientName
					NULL, --ClientUri
					NULL, --LogoUri
					3, --ClientCredentials
					NULL, --LogoutUri
					36000, --IdentityTokenLifetime
					36000, --AccessTokenLifetime
					36000, --AuthorizationCodeLifetime
					86400, --AbsoluteRefreshTokenLifetime
					36000, --SlidingRefreshTokenLifetime
					0, --RefreshTokenUsage
					0, --RefreshTokenExpiration
					0, --AccessTokenType
					1, --UpdateAccessTokenOnRefresh
					1, --Enabled
					0, --LogoutSessionRequired
					0, --RequireSignOutPrompt
					1, --AllowAccessToAllScopes
					0, --AllowClientCredentialsOnly
					0, --RequireConsent
					1, --AllowRememberConsent
					1, --EnableLocalLogin
					1, --IncludeJwtId
					1, --AlwaysSendClientClaims
					1, --PrefixClientClaims
					1 --AllowAccessToAllGrantTypes
	)

	SELECT	@ClientId = ClientId
	FROM	@AuthClients

	INSERT INTO Auth.ClientScopes (
		ClientId,
		Scope
	)
	SELECT	@ClientId, --ClientId
					ScopeId --Scope
	FROM		Auth.Scopes
	WHERE Name = @ClientCode

	IF NOT EXISTS (SELECT 1 FROM [Auth].[ClientScopes] WHERE ClientId = @ClientId and Scope = 'rplandingapi')
	BEGIN
		INSERT [Auth].[ClientScopes] ([ClientId], [Scope]) VALUES (@ClientId, N'rplandingapi')
	END
	IF NOT EXISTS (SELECT 1 FROM [Auth].[ClientScopes] WHERE ClientId = @ClientId and Scope = 'migrationapi')
	BEGIN
		INSERT [Auth].[ClientScopes] ([ClientId], [Scope]) VALUES (@ClientId, N'migrationapi')
	END

	INSERT INTO Auth.ClientSecrets (
		ClientId,
		Value,
		Type,
		Description,
		Expiration
	)
	VALUES (
		@ClientId,
		'LhMc2x1ObIAwGG6+YwrhBzKnZwho90Z+Pghttgz39d0=',
		NULL,
		@ClientName,
		'2099-12-31 00:00:00.0000000 -06:00'
	)
END

GO
/*****************************************
UnifiedPlatform and UnifiedSettings Rights
*****************************************/

DECLARE @ResendInvite int;
DECLARE @Lock INT
DECLARE @OrgRowNum INT;
DECLARE @ActionID INT;
DECLARE @RightID INT;
DECLARE @RoleID INT;
DECLARE @Status INT;
DECLARE @ActionValueID INT;
DECLARE @OrgID INT;
DECLARE @ProductID INT;
DECLARE @ParentActionId INT;
DECLARE @UserActionId INT;
DECLARE @RightCategoryId INT;
DECLARE @PartyId INT;
DECLARE @RightName VARCHAR(100);
DECLARE @RVT INT;
DECLARE @DefaultRoute NVARCHAR(200);
DECLARE @RightValueTypeId INT;
DECLARE @StatusId INT;
DECLARE @PersonaId INT;
DECLARE @FromDate DATETIME;
DECLARE @TRoleId INT;
DECLARE @TRoleName NVARCHAR(500);
DECLARE @TRoleDesc NVARCHAR(500);
DECLARE @TRightId INT;
DECLARE @TRightName NVARCHAR(500);
DECLARE @TRightDesc NVARCHAR(500);
DECLARE @RightCategory INT;
DECLARE @RoleCategory INT;
DECLARE @RoleName NVARCHAR(500);
DECLARE @RoleTypeID INT;
DECLARE @PerosonaP INT;
DECLARE @PartyRowNum INT;


SELECT @ProductId = ProductId
FROM Enterprise.Product
WHERE Name = 'Unified Platform';

SELECT @ActionValueID = [ActionValueTypeID]
FROM Enterprise.ActionValueType
WHERE Value = 'Right';

IF NOT EXISTS
(
	SELECT 1
	FROM Enterprise.ACTION
	WHERE ObjectValue = 'SupportTool' AND 
		  ParentActionId IS NULL
)
BEGIN
	EXEC Enterprise.CreateAction @ProductID = @ProductId, @Action = N'SupportTool', @ActionTarget = N'Route', @ActionbValueTypeId = @ActionValueID, @Description = 'SupportRoute Route.', @ActionID = @ActionID OUTPUT;
	SELECT @ActionID AS N'@ActionID';
END;



SELECT @ParentActionId = ActionId
FROM Enterprise.ACTION
WHERE ObjectValue = 'Dashboard' AND 
	  ObjectType = 'Route' AND 
	  Description = 'SuperUser';

IF NOT EXISTS
(
	SELECT 1
	FROM Enterprise.ACTION
	WHERE ObjectValue = 'Support Tool' AND 
		  ParentActionID = @ParentActionId
)
BEGIN
	EXEC [Enterprise].[CreateAction] @ProductID = @ProductId, @Action = N'Support Tool', @ActionTarget = N'Right', @ActionbValueTypeId = @ActionValueID, @Description = '', @ParentActionID = @ParentActionId, @ActionID = @ActionID OUTPUT;
	SELECT @ActionID AS N'@ActionID';
END;

IF NOT EXISTS
(
	SELECT 1
	FROM Enterprise.ACTION
	WHERE ObjectValue = 'SupportTool' AND Description = 'SuperUser' AND OBjectType = 'Route'
		 AND ParentActionId IS NULL
)
BEGIN
	EXEC Enterprise.CreateAction @ProductID = @ProductId, @Action = N'SupportTool', @ActionTarget = N'Route', @ActionbValueTypeId = @ActionValueID, @Description = 'Superuser', @ActionID = @ActionID OUTPUT;
	SELECT @ParentActionId = @ActionID;
END;


IF NOT EXISTS
(
	SELECT 1
	FROM Enterprise.ACTION
	WHERE ObjectValue = 'Support Tool' AND ObjectType = 'Right' AND
		  ParentActionID IS NULL
)
BEGIN
	EXEC [Enterprise].[CreateAction] @ProductID = @ProductId, @Action = N'Support Tool', @ActionTarget = N'Right', @ActionbValueTypeId = @ActionValueID, @Description = '', @ActionID = @ActionID OUTPUT;
	SELECT @ActionID AS N'@ActionID';
END;


IF NOT EXISTS
(
	SELECT 1
	FROM Enterprise.ACTION
	WHERE ObjectValue = 'Access to Unified Platform' AND 
		  ParentActionID = @ParentActionId
)
BEGIN
	EXEC [Enterprise].[CreateAction] @ProductID = @ProductId, @Action = N'Access to Unified Platform', @ActionTarget = N'Right', @ActionbValueTypeId = @ActionValueID, @Description = '', @ParentActionID = @ParentActionId, @ActionID = @ActionID OUTPUT;
	SELECT @ActionID AS N'@ActionID';
END;

IF NOT EXISTS
(
	SELECT 1
	FROM Enterprise.ACTION
	WHERE ObjectValue = 'Access to Unified Settings' AND 
		  ParentActionID = @ParentActionId
)
BEGIN
	EXEC [Enterprise].[CreateAction] @ProductID = @ProductId, @Action = N'Access to Unified Settings', @ActionTarget = N'Right', @ActionbValueTypeId = @ActionValueID, @Description = '', @ParentActionID = @ParentActionId, @ActionID = @ActionID OUTPUT;
	SELECT @ActionID AS N'@ActionID';
END;

IF NOT EXISTS
(
	SELECT 1
	FROM Enterprise.ACTION
	WHERE ObjectValue = 'Employee Access' AND 
		  ParentActionID = @ParentActionId
)
BEGIN
	EXEC [Enterprise].[CreateAction] @ProductID = @ProductId, @Action = N'Employee Access', @ActionTarget = N'Right', @ActionbValueTypeId = @ActionValueID, @Description = '', @ParentActionID = @ParentActionId, @ActionID = @ActionID OUTPUT;
	SELECT @ActionID AS N'@ActionID';
END;

IF NOT EXISTS
(
	SELECT 1
	FROM Enterprise.ACTION
	WHERE ObjectValue = 'Employee Access' AND 
		  ParentActionId IS NULL
)
BEGIN
	EXEC Enterprise.CreateAction @ProductID = @ProductId, @Action = N'Employee Access', @ActionTarget = N'Right', @ActionbValueTypeId = @ActionValueID, @Description = 'Edit Other User Profile', @ActionID = @ActionID OUTPUT;
	SELECT @ActionID AS N'@ActionID';
END;

IF NOT EXISTS
(
	SELECT 1
	FROM Enterprise.ACTION
	WHERE ObjectValue = 'Access to Unified Platform' AND 
		  ParentActionId IS NULL
)
BEGIN
	EXEC Enterprise.CreateAction @ProductID = @ProductId, @Action = N'Access to Unified Platform', @ActionTarget = N'Right', @ActionbValueTypeId = @ActionValueID, @Description = 'Edit Other User Profile', @ActionID = @ActionID OUTPUT;
	SELECT @ActionID AS N'@ActionID';
END;

IF NOT EXISTS
(
	SELECT 1
	FROM Enterprise.ACTION
	WHERE ObjectValue = 'Access to Unified Settings' AND 
		  ParentActionId IS NULL
)
BEGIN
	EXEC Enterprise.CreateAction @ProductID = @ProductId, @Action = N'Access to Unified Settings', @ActionTarget = N'Right', @ActionbValueTypeId = @ActionValueID, @Description = 'Edit Other User Profile', @ActionID = @ActionID OUTPUT;
	SELECT @ActionID AS N'@ActionID';
END;





IF OBJECT_ID('tempdb..#HoldOrgsEditOthersProfile') IS NOT NULL
BEGIN
	DROP TABLE #HoldOrgsEditOthersProfile;
END;

SELECT DISTINCT 
	   IDENTITY(int, 1, 1) AS RowNumber, OrganizationPartyID, 0 AS PStatus
INTO #HoldOrgsEditOthersProfile
FROM Person.Persona AS P
	 INNER JOIN
	 Enterprise.Organization AS O
	 ON P.OrganizationPartyId = O.PartyId
WHERE O.Name = 'RealPage Employee';;-- WHERE Person.Persona.OrganizationPartyId = 353

SELECT @Status = StatusType.StatusTypeID
FROM Enterprise.StatusTypeCategoryType
	 JOIN
	 Enterprise.StatusTypeCategory
	 ON StatusTypeCategory.StatusTypeCategoryTypeId = StatusTypeCategoryType.StatusTypeCategoryTypeId
	 JOIN
	 Enterprise.StatusTypeCategoryClassification
	 ON StatusTypeCategoryClassification.StatusTypeCategoryId = StatusTypeCategory.StatusTypeCategoryId
	 JOIN
	 Enterprise.StatusType
	 ON StatusType.StatusTypeId = StatusTypeCategoryClassification.StatusTypeId
WHERE StatusType.name = 'ALL' AND 
	  StatusTypeCategoryType.Name = 'Security';

SELECT @RightCategoryId = StatusType.StatusTypeID
FROM Enterprise.StatusTypeCategoryType
	 JOIN
	 Enterprise.StatusTypeCategory
	 ON StatusTypeCategory.StatusTypeCategoryTypeId = StatusTypeCategoryType.StatusTypeCategoryTypeId
	 JOIN
	 Enterprise.StatusTypeCategoryClassification
	 ON StatusTypeCategoryClassification.StatusTypeCategoryId = StatusTypeCategory.StatusTypeCategoryId
	 JOIN
	 Enterprise.StatusType
	 ON StatusType.StatusTypeId = StatusTypeCategoryClassification.StatusTypeId
WHERE StatusType.name = 'DEfault' AND 
	  StatusTypeCategory.Name = 'Right Type' AND 
	  StatusTypeCategoryType.Name = 'Security';






WHILE EXISTS
(
	SELECT 1
	FROM #HoldOrgsEditOthersProfile
	WHERE PStatus = 0
)
BEGIN
	SELECT TOP 1 @OrgRowNum = Rownumber, @OrgID = OrganizationPartyID
	FROM #HoldOrgsEditOthersProfile
	WHERE PStatus = 0;

	SELECT @RoleId = RoleId
		FROM Enterprise.Role AS R
			 INNER JOIN
			 Enterprise.RoleValueType AS RR
			 ON RR.RoleValueTypeId = R.RoleValueTypeId
		WHERE RR.Value = 'User Administrator' AND 
			  R.PartyId = @OrgId;

		EXECUTE Enterprise.CreateRight @RoleId = @RoleId, @RightName = 'Access to Unified Platform via Support Tool', @ShortName = 'AccessToUnifiedPlatform', @RightCategoryId = @RightCategoryId, @PartyId = @OrgId, @ProductId = @ProductId, @Description = 'Access to Unified Platform Login via Support Tool', @RightId = @RightId OUTPUT;
		SELECT @ActionID = ActionID FROM Enterprise.Action WHERE ObjectValue = 'SupportTool' and ObjectType = 'ROUTE' and Description = 'SuperUser'
		SELECT @RightID = RightId FROM Enterprise.[Right] R
		INNER JOIN Enterprise.RightValueType RR on RR.RightValueTypeId = R.RightValueTypeId
			 WHERE Value = 'Access to Unified Platform via Support Tool' and RoleId = @RoleID
		EXEC	[Enterprise].[LinkActionToRights]  @ActionID = @ActionID, @RightId = @RightId,	@StatusId = @Status,	@UserActionId = @UserActionId OUTPUT

		EXECUTE Enterprise.CreateRight @RoleId = @RoleId, @RightName = 'Access to Unified Settings via Support Tool', @ShortName = 'AccessToUnifiedSettings', @RightCategoryId = @RightCategoryId, @PartyId = @OrgId, @ProductId = @ProductId, @Description = 'Ability to manage settings for Unified Platform', @RightId = @RightId OUTPUT;
		SELECT @ActionID = ActionID FROM Enterprise.Action WHERE ObjectValue = 'SupportTool' and ObjectType = 'ROUTE' and Description = 'SuperUser'
		SELECT @RightID = RightId FROM Enterprise.[Right] R
		INNER JOIN Enterprise.RightValueType RR on RR.RightValueTypeId = R.RightValueTypeId
			 WHERE Value = 'Access to Unified Settings via Support Tool' and RoleId = @RoleID
		EXEC	[Enterprise].[LinkActionToRights]  @ActionID = @ActionID, @RightId = @RightId,	@StatusId = @Status,	@UserActionId = @UserActionId OUTPUT


		EXECUTE Enterprise.CreateRight @RoleId = -1, @RightName = 'Default_SupportToolRight', @ShortName = 'SupportToolRight', @RightCategoryId = @RightCategoryId, @PartyId = @OrgId, @ProductId = @ProductId, @Description = '', @RightId = @RightId OUTPUT;
		
	
		SELECT @ActionID = ActionID
		FROM Enterprise.ACTION
		WHERE ObjectValue = 'Support Tool' AND 
			  ObjectType = 'Right' AND 
			  ParentActionId IS NULL;
		EXEC [Enterprise].[LinkActionToRights] @ActionID = @ActionID, @RightId = @RightId, @StatusId = @Status, @UserActionId = @UserActionId OUTPUT;

		EXECUTE Enterprise.CreateRight @RoleId = -1, @RightName = 'Default_SupportToolRoute', @ShortName = 'SupportToolRoute', @RightCategoryId = @RightCategoryId, @PartyId = @OrgId, @ProductId = @ProductId, @Description = '', @RightId = @RightId OUTPUT;
		SELECT @ActionID = ActionID
		FROM Enterprise.ACTION
		WHERE ObjectValue = 'SupportTool' AND 
			  ObjectType = 'Route' AND 
			  ParentActionId IS NULL;
		EXEC [Enterprise].[LinkActionToRights] @ActionID = @ActionID, @RightId = @RightId, @StatusId = @Status, @UserActionId = @UserActionId OUTPUT;

		EXECUTE Enterprise.CreateRight @RoleId = -1, @RightName = 'Default_EmployeeAccess', @ShortName = 'EmployeeAccess', @RightCategoryId = @RightCategoryId, @PartyId = @OrgId, @ProductId = @ProductId, @Description = '', @RightId = @RightId OUTPUT;
		SELECT @ActionID = ActionID
		FROM Enterprise.ACTION
		WHERE ObjectValue = 'Employee Access' AND 
			  ObjectType = 'Right' AND 
			  ParentActionId IS NULL;
		EXEC [Enterprise].[LinkActionToRights] @ActionID = @ActionID, @RightId = @RightId, @StatusId = @Status, @UserActionId = @UserActionId OUTPUT;
		

		EXECUTE Enterprise.CreateRight @RoleId = -1, @RightName = 'Default_AccessToUnifiedSettings', @ShortName = 'AccesstoUnifiedSettings', @RightCategoryId = @RightCategoryId, @PartyId = @OrgId, @ProductId = @ProductId, @Description = '', @RightId = @RightId OUTPUT;
		SELECT @ActionID = ActionID
		FROM Enterprise.ACTION
		WHERE ObjectValue = 'Access to Unified Settings' AND 
			  ObjectType = 'Right' AND 
			  ParentActionId IS NULL;
		EXEC [Enterprise].[LinkActionToRights] @ActionID = @ActionID, @RightId = @RightId, @StatusId = @Status, @UserActionId = @UserActionId OUTPUT;
		
		EXECUTE Enterprise.CreateRight @RoleId = -1, @RightName = 'Default_AccessToUnifiedPlatform', @ShortName = 'AccessToUnifiedPlatform', @RightCategoryId = @RightCategoryId, @PartyId = @OrgId, @ProductId = @ProductId, @Description = '', @RightId = @RightId OUTPUT;
		
		SELECT @ActionID = ActionID
		FROM Enterprise.ACTION
		WHERE ObjectValue = 'Access to Unified Platform' AND 
			  ObjectType = 'Right' AND 
			  ParentActionId IS NULL;
		EXEC [Enterprise].[LinkActionToRights] @ActionID = @ActionID, @RightId = @RightId, @StatusId = @Status, @UserActionId = @UserActionId OUTPUT;
		UPDATE #HoldOrgsEditOthersProfile
		  SET PStatus = 1
		WHERE RowNumber = @OrgRowNum;
	END;

GO
DECLARE @RightValueTypeId INT
DECLARE @SupportToolsRoute int;
DECLARE @EmployeeAccess INT
DECLARE @AccessUnifiedSettings INT
DECLARE @AccessUnifiedPlatform INT
DECLARE @DashBoard INT
DECLARE @SupportToolRight INT

SELECT  @DashBoard = RightValueTypeId
FROM Enterprise.RightValueType 
WHERE value = 'Default_Dashboard_Users';


SELECT @SupportToolsRoute = RightValueTypeId
FROM Enterprise.RightValueType
WHERE value = 'Default_SupportToolRoute';

SELECT @EmployeeAccess = RightValueTypeId
FROM Enterprise.RightValueType
WHERE value = 'Default_EmployeeAccess'

SELECT @AccessUnifiedSettings = RightValueTypeId
FROM Enterprise.RightValueType
WHERE value = 'Default_AccessToUnifiedSettings'

SELECT @AccessUnifiedPlatform = RightValueTypeId
FROM Enterprise.RightValueType
WHERE value = 'Default_AccessToUnifiedPlatform'

SELECT  @SupportToolRight = RightValueTypeId
FROM Enterprise.RightValueType 
WHERE value = 'Default_SupportToolRight';



SELECT @RightValueTypeId = RightValueTypeId
FROM Enterprise.RightValueType
WHERE value IN( 'Access to Unified Platform via Support Tool');

IF NOT EXISTS
(
	SELECT 1
	FROM Enterprise.RightDependency
	WHERE RightValueTypeId = @RightValueTypeId AND 
		  DependentRightValueTypeId = @SupportToolsRoute
)
BEGIN
	INSERT INTO Enterprise.RightDependency( RightValueTypeId, DependentRightValueTypeId )
	VALUES( @RightValueTypeId, @SupportToolsRoute );
END;



IF NOT EXISTS
(
	SELECT 1
	FROM Enterprise.RightDependency
	WHERE RightValueTypeId = @RightValueTypeId AND 
		  DependentRightValueTypeId = @EmployeeAccess
)
BEGIN
	INSERT INTO Enterprise.RightDependency( RightValueTypeId, DependentRightValueTypeId )
	VALUES( @RightValueTypeId, @EmployeeAccess );
END;

IF NOT EXISTS
(
	SELECT 1
	FROM Enterprise.RightDependency
	WHERE RightValueTypeId = @RightValueTypeId AND 
		  DependentRightValueTypeId = @AccessUnifiedPlatform
)
BEGIN
	INSERT INTO Enterprise.RightDependency( RightValueTypeId, DependentRightValueTypeId )
	VALUES( @RightValueTypeId, @AccessUnifiedPlatform );
END;

IF NOT EXISTS
(
	SELECT 1
	FROM Enterprise.RightDependency
	WHERE RightValueTypeId = @RightValueTypeId AND 
		  DependentRightValueTypeId = @SupportToolRight
)
BEGIN
	INSERT INTO Enterprise.RightDependency( RightValueTypeId, DependentRightValueTypeId )
	VALUES( @RightValueTypeId, @SupportToolRight );
END;

----->

SELECT @RightValueTypeId = RightValueTypeId
FROM Enterprise.RightValueType
WHERE value IN( 'Access to Unified Settings via Support Tool');

IF NOT EXISTS
(
	SELECT 1
	FROM Enterprise.RightDependency
	WHERE RightValueTypeId = @RightValueTypeId AND 
		  DependentRightValueTypeId = @SupportToolsRoute
)
BEGIN
	INSERT INTO Enterprise.RightDependency( RightValueTypeId, DependentRightValueTypeId )
	VALUES( @RightValueTypeId, @SupportToolsRoute );
END;

IF NOT EXISTS
(
	SELECT 1
	FROM Enterprise.RightDependency
	WHERE RightValueTypeId = @RightValueTypeId AND 
		  DependentRightValueTypeId = @EmployeeAccess
)
BEGIN
	INSERT INTO Enterprise.RightDependency( RightValueTypeId, DependentRightValueTypeId )
	VALUES( @RightValueTypeId, @EmployeeAccess );
END;

IF NOT EXISTS
(
	SELECT 1
	FROM Enterprise.RightDependency
	WHERE RightValueTypeId = @RightValueTypeId AND 
		  DependentRightValueTypeId = @AccessUnifiedSettings
)
BEGIN
	INSERT INTO Enterprise.RightDependency( RightValueTypeId, DependentRightValueTypeId )
	VALUES( @RightValueTypeId, @AccessUnifiedSettings );
END;

IF NOT EXISTS
(
	SELECT 1
	FROM Enterprise.RightDependency
	WHERE RightValueTypeId = @RightValueTypeId AND 
		  DependentRightValueTypeId = @SupportToolRight
)
BEGIN
	INSERT INTO Enterprise.RightDependency( RightValueTypeId, DependentRightValueTypeId )
	VALUES( @RightValueTypeId, @SupportToolRight );
END;

GO


/********************************
Custom Fields
********************************/


DECLARE @OrgRowNum INT;
DECLARE @ActionID INT;
DECLARE @RightID INT;
DECLARE @RoleID INT;
DECLARE @Status INT;
DECLARE @ActionValueID INT;
DECLARE @OrgID INT;
DECLARE @ProductID INT;
DECLARE @ParentActionId INT;
DECLARE @UserActionId INT;
DECLARE @RightCategoryId INT;
DECLARE @PartyId INT;
DECLARE @RightName VARCHAR(100);
DECLARE @RVT INT;
DECLARE @DefaultRoute NVARCHAR(200);
DECLARE @RightValueTypeId INT;
DECLARE @StatusId INT;
DECLARE @PersonaId INT;
DECLARE @FromDate DATETIME;
DECLARE @TRoleId INT;
DECLARE @TRoleName NVARCHAR(500);
DECLARE @TRoleDesc NVARCHAR(500);
DECLARE @TRightId INT;
DECLARE @TRightName NVARCHAR(500);
DECLARE @TRightDesc NVARCHAR(500);
DECLARE @RightCategory INT;
DECLARE @RoleCategory INT;
DECLARE @RoleName NVARCHAR(500);
DECLARE @RoleTypeID INT;
DECLARE @PerosonaP INT;
DECLARE @PartyRowNum INT;
DECLARE @TRightShortName NVARCHAR(100)
IF OBJECT_ID('tempdb..#RightsUnifiedSettings') IS NOT NULL
BEGIN
	DROP TABLE #RightsUnifiedSettings;
END;

IF OBJECT_ID('tempdb..#HoldPartyForUnifiedSettings') IS NOT NULL
BEGIN
	DROP TABLE #HoldPartyForUnifiedSettings;
END;

CREATE TABLE #RightsUnifiedSettings
( 
			 RightId int, Name nvarchar(500), description nvarchar(500), shortname varchar(100)
);

INSERT INTO #RightsUnifiedSettings( rightid, name, description, shortname )
VALUES( 1, 'Ability to Configure Custom Fields for Users', 'Ability to Configure Custom Fields for Users', 'ConfigureCustomFields' );

SELECT @ProductId = ProductId
FROM Enterprise.Product
WHERE name = 'Unified Platform';

SELECT @ActionValueID = [ActionValueTypeID]
FROM Enterprise.ActionValueType
WHERE Value = 'Right';

SELECT @RoleCategory = ST.StatusTypeId
FROM Enterprise.StatusTypeCategoryType AS STCT
	 JOIN
	 Enterprise.StatusTypeCategory AS STC
	 ON STCT.StatusTypeCategoryTypeId = STC.StatusTypeCategoryTypeId
	 JOIN
	 Enterprise.StatusTypeCategoryClassification AS STCC
	 ON STCC.StatusTypeCategoryId = STC.StatusTypeCategoryId
	 JOIN
	 Enterprise.StatusType AS ST
	 ON ST.StatusTypeId = STCC.StatusTypeId
WHERE STC.Name = 'Role Type' AND 
	  ST.Name = 'Default';

SELECT @RightCategory = ST.StatusTypeId
FROM Enterprise.StatusTypeCategoryType AS STCT
	 JOIN
	 Enterprise.StatusTypeCategory AS STC
	 ON STCT.StatusTypeCategoryTypeId = STC.StatusTypeCategoryTypeId
	 JOIN
	 Enterprise.StatusTypeCategoryClassification AS STCC
	 ON STCC.StatusTypeCategoryId = STC.StatusTypeCategoryId
	 JOIN
	 Enterprise.StatusType AS ST
	 ON ST.StatusTypeId = STCC.StatusTypeId
WHERE STC.Name = 'Right Type' AND 
	  ST.Name = 'Default';


SELECT DISTINCT 
	   IDENTITY(int, 1, 1) AS RowNumber, OrganizationPartyID, 0 AS PStatus
INTO #HoldPartyForUnifiedSettings
FROM Person.Persona AS P
	 INNER JOIN
	 Enterprise.Organization AS O
	 ON P.OrganizationPartyId = O.PartyId
WHERE O.Name = 'RealPage Employee';

WHILE EXISTS
(
	SELECT 1
	FROM #HoldPartyForUnifiedSettings
	WHERE PStatus = 0
)
BEGIN
	SELECT TOP 1 @PartyRowNum = Rownumber, @PartyId = OrganizationPartyID
	FROM #HoldPartyForUnifiedSettings
	WHERE PStatus = 0;
	SELECT @RoleId = RoleId
	FROM Enterprise.Role AS R
		 INNER JOIN
		 Enterprise.RoleValueType AS RR
		 ON RR.RoleValueTypeId = R.RoleValueTypeId
	WHERE RR.Value = 'User Administrator' AND 
		  R.PartyId = @PartyId;
	DECLARE Rights CURSOR
	FOR SELECT RightId, Name, Description, ShortName
		FROM #RightsUnifiedSettings;
	OPEN Rights;
	FETCH Rights INTO @TRightId, @TRightName, @TRightDesc, @TRightShortName;
	WHILE @@FETCH_STATUS = 0
	BEGIN
		EXECUTE Enterprise.CreateRight @RoleId = -1, @RightName = @TRightName, @RightCategoryId = @RightCategory, @PartyId = @PartyId, @ProductId = @ProductId, @Shortname = @TRightShortName, @Description = @TRightDesc, @RightId = @RightId OUTPUT;
		
		FETCH Rights INTO @TRightId, @TRightName, @TRightDesc, @TRightShortName;
	END;
	CLOSE Rights;
	DEALLOCATE Rights;
	UPDATE #HoldPartyForUnifiedSettings
	  SET PStatus = 1
	WHERE RowNumber = @PartyRowNum;
END;

--GB-904 - Change Super User to System Administrator
IF NOT EXISTS (SELECT * FROM Person.PersonaType WHERE PersonaTypeId = 1 AND Name = 'System Administrator')
UPDATE Person.PersonaType SET Name = 'System Administrator' WHERE PersonaTypeId = 1

--Corrected per DJ to use PartyRelationship instead of PartyRole
UPDATE P SET PersonaTypeId = 1
FROM Person.Persona P
INNER JOIN Enterprise.PartyRelationship PR
ON P.PersonPartyId = PR.PartyIdFrom
	AND PR.RoleTypeIdFrom = 402
INNER JOIN Enterprise.Organization O
ON P.OrganizationPartyId = O.PartyId


/**********************************
Property Photos Right - GB623
**********************************/
GO

DECLARE @OrgRowNum INT;
DECLARE @ActionID INT;
DECLARE @RightID INT;
DECLARE @RoleID INT;
DECLARE @Status INT;
DECLARE @ActionValueID INT;
DECLARE @OrgID INT;
DECLARE @ProductID INT;
DECLARE @ParentActionId INT;
DECLARE @UserActionId INT;
DECLARE @RightCategoryId INT;
DECLARE @PartyId INT;
DECLARE @RightName VARCHAR(100);
DECLARE @RVT INT;
DECLARE @DefaultRoute NVARCHAR(200);
DECLARE @RightValueTypeId INT;
DECLARE @StatusId INT;
DECLARE @PersonaId INT;
DECLARE @FromDate DATETIME;
DECLARE @TRoleId INT;
DECLARE @TRoleName NVARCHAR(500);
DECLARE @TRoleDesc NVARCHAR(500);
DECLARE @TRightId INT;
DECLARE @TRightName NVARCHAR(500);
DECLARE @TRightDesc NVARCHAR(500);
DECLARE @RightCategory INT;
DECLARE @RoleCategory INT;
DECLARE @RoleName NVARCHAR(500);
DECLARE @RoleTypeID INT;
DECLARE @PerosonaP INT;
DECLARE @PartyRowNum INT;
DECLARE @TRightShortName NVARCHAR(100)
IF OBJECT_ID('tempdb..#RightsUnifiedSettings') IS NOT NULL
BEGIN
	DROP TABLE #RightsUnifiedSettings;
END;

IF OBJECT_ID('tempdb..#HoldPartyForUnifiedSettings') IS NOT NULL
BEGIN
	DROP TABLE #HoldPartyForUnifiedSettings;
END;

CREATE TABLE #RightsUnifiedSettings
( 
			 RightId int, Name nvarchar(500), description nvarchar(500), shortname varchar(100)
);

INSERT INTO #RightsUnifiedSettings( rightid, name, description, shortname )
VALUES( 1, 'Access to Property Photos', 'Access to Property Photos', 'AccessPropertyPhotos' );

SELECT @ProductId = ProductId
FROM Enterprise.Product
WHERE name = 'Unified Platform';

SELECT @ActionValueID = [ActionValueTypeID]
FROM Enterprise.ActionValueType
WHERE Value = 'Right';

SELECT @RoleCategory = ST.StatusTypeId
FROM Enterprise.StatusTypeCategoryType AS STCT
	 JOIN
	 Enterprise.StatusTypeCategory AS STC
	 ON STCT.StatusTypeCategoryTypeId = STC.StatusTypeCategoryTypeId
	 JOIN
	 Enterprise.StatusTypeCategoryClassification AS STCC
	 ON STCC.StatusTypeCategoryId = STC.StatusTypeCategoryId
	 JOIN
	 Enterprise.StatusType AS ST
	 ON ST.StatusTypeId = STCC.StatusTypeId
WHERE STC.Name = 'Role Type' AND 
	  ST.Name = 'Default';

SELECT @RightCategory = ST.StatusTypeId
FROM Enterprise.StatusTypeCategoryType AS STCT
	 JOIN
	 Enterprise.StatusTypeCategory AS STC
	 ON STCT.StatusTypeCategoryTypeId = STC.StatusTypeCategoryTypeId
	 JOIN
	 Enterprise.StatusTypeCategoryClassification AS STCC
	 ON STCC.StatusTypeCategoryId = STC.StatusTypeCategoryId
	 JOIN
	 Enterprise.StatusType AS ST
	 ON ST.StatusTypeId = STCC.StatusTypeId
WHERE STC.Name = 'Right Type' AND 
	  ST.Name = 'Default';


SELECT DISTINCT 
	   IDENTITY(int, 1, 1) AS RowNumber, OrganizationPartyID, 0 AS PStatus
INTO #HoldPartyForUnifiedSettings
FROM Person.Persona AS P
	 INNER JOIN
	 Enterprise.Organization AS O
	 ON P.OrganizationPartyId = O.PartyId
WHERE O.Name = 'RealPage Employee';

WHILE EXISTS
(
	SELECT 1
	FROM #HoldPartyForUnifiedSettings
	WHERE PStatus = 0
)
BEGIN
	SELECT TOP 1 @PartyRowNum = Rownumber, @PartyId = OrganizationPartyID
	FROM #HoldPartyForUnifiedSettings
	WHERE PStatus = 0;
	SELECT @RoleId = RoleId
	FROM Enterprise.Role AS R
		 INNER JOIN
		 Enterprise.RoleValueType AS RR
		 ON RR.RoleValueTypeId = R.RoleValueTypeId
	WHERE RR.Value = 'User Administrator' AND 
		  R.PartyId = @PartyId;
	DECLARE Rights CURSOR
	FOR SELECT RightId, Name, Description, ShortName
		FROM #RightsUnifiedSettings;
	OPEN Rights;
	FETCH Rights INTO @TRightId, @TRightName, @TRightDesc, @TRightShortName;
	WHILE @@FETCH_STATUS = 0
	BEGIN
		EXECUTE Enterprise.CreateRight @RoleId = @RoleId, @RightName = @TRightName, @RightCategoryId = @RightCategory, @PartyId = @PartyId, @ProductId = @ProductId, @Shortname = '', @Description = @TRightDesc, @RightId = @RightId OUTPUT;
		
		FETCH Rights INTO @TRightId, @TRightName, @TRightDesc, @TRightShortName;
	END;
	CLOSE Rights;
	DEALLOCATE Rights;
	UPDATE #HoldPartyForUnifiedSettings
	  SET PStatus = 1
	WHERE RowNumber = @PartyRowNum;
END;

GO
/***********************
Master Setting type
***********************/
DECLARE @MasterConFigurationTypeId VARCHAR(100);
SELECT @MasterConFigurationTypeId = MasterConfigurationTypeId
FROM Enterprise.MasterConfigurationType
WHERE Name = 'UserLogin';
IF NOT EXISTS(SELECT 1 FROM Enterprise.MasterSettingType WHERE Name = 'CustomFields')
BEGIN
INSERT INTO Enterprise.MasterSettingType
(Name,
 MasterConfigurationTypeId
)
VALUES
('CustomFields',
 @MasterConFigurationTypeId
);
END

GO

/**************************************
--Assign Support Tool to Organizations
**************************************/

--DECLARE @ProductId INT
--DECLARE @OrganizationId INT
--DECLARE @Now DATETIME = GETUTCDATE()

--DECLARE @ProductList TABLE (ProductID INT)
--INSERT INTO @ProductList(ProductID)
--VALUES(35); --Support Tool

--SET @ProductId = 35
--DECLARE PartyList CURSOR
--FOR SELECT PartyId FROM Enterprise.Organization

--SET @OrganizationId = NULL;
--OPEN PartyList;
--FETCH NEXT FROM PartyList INTO @OrganizationId;
--WHILE @@FETCH_STATUS = 0
--    BEGIN
--        INSERT INTO Enterprise.OrganizationProduct
--(PartyId,
-- ConfigurationId,
-- ProductId,
-- FromDate
--)
--           SELECT @OrganizationId,
--                  GPC.ConfigurationId,
--                  GPC.ProductId,
--                  @Now
--           FROM Enterprise.GlobalProductConfiguration GPC
--                INNER JOIN @ProductList PL ON GPC.ProductId = PL.ProductID
--                                              AND GPC.ThruDate IS NULL
--                LEFT OUTER JOIN Enterprise.OrganizationProduct OP ON OP.PartyId = @OrganizationId
--                                                                     AND OP.ProductId = GPC.ProductId
--           WHERE GPC.ThruDate IS NULL
--                 AND (OP.ConfigurationId IS NULL
--                      OR (OP.ConfigurationId IS NOT NULL
--                          AND OP.ThruDate IS NOT NULL));
--        FETCH NEXT FROM PartyList INTO @OrganizationId;
--    END;
--CLOSE PartyList;
--DEALLOCATE PartyList;


GO

/***********************************
Add EmployeeAccessId
***********************************/
DECLARE @ProductSettingTypeId INT
DECLARE @ProductSettingId INT
DECLARE @PRoductId INT
DECLARE @COnfigurationId INT
DECLARE @MasterConFigurationTypeId VARCHAR(100);
SELECT @MasterConFigurationTypeId = MasterConfigurationTypeId
FROM Enterprise.MasterConfigurationType
WHERE Name = 'Organization';
IF NOT EXISTS(SELECT 1 FROM Enterprise.MasterSettingType WHERE Name = 'RealPageEmployeeAccessID')
BEGIN
INSERT INTO Enterprise.MasterSettingType
(Name,
 MasterConfigurationTypeId
)
VALUES
('RealPageEmployeeAccessID',
 @MasterConFigurationTypeId
);
END

-- Lead2Lease Migraiton API EndPoint 
SET @ProductSettingTypeId = NULL;
SET @ProductSettingId = NULL;

EXEC Enterprise.GetProductSettingType @Name = 'MTApiEndPoint', 	@ProductSettingTypeId = @ProductSettingTypeId OUTPUT;
SELECT @ProductId = ProductId FROM Enterprise.Product WHERE Name = 'Lead2Lease';
SELECT @ConfigurationId = ConfigurationId FROM Enterprise.GlobalProductConfiguration WHERE ProductId = @ProductId AND (GETDATE() BETWEEN FromDate AND ThruDate OR  ThruDate IS NULL);

IF @@SERVERNAME = 'RCDUSODBSQL001'
BEGIN
	EXEC Enterprise.CreateProductSetting @ProductId, @ProductSettingTypeId, 'https://ulmt.dev.lead2lease.com/api', NULL, NULL, @ProductSettingId OUTPUT;
END;
	
IF @@SERVERNAME = 'RCTUSODBSQL001'
BEGIN
	EXEC Enterprise.CreateProductSetting @ProductId, @ProductSettingTypeId, 'https://ulmt.qa.lead2lease.com/api', NULL, NULL, @ProductSettingId OUTPUT;
END;
	
IF @@SERVERNAME = 'RCQUSODBSQL001'
BEGIN
	EXEC Enterprise.CreateProductSetting @ProductId, @ProductSettingTypeId, 'https://ulmt.qa.lead2lease.com/api', NULL, NULL, @ProductSettingId OUTPUT;
END;
	
IF @@SERVERNAME IN('RCPGBKDBSQL005A', 'RCPGBKDBSQL005B')
BEGIN
	EXEC Enterprise.CreateProductSetting @ProductId, @ProductSettingTypeId, 'https://ulmt.lead2lease.com/api', NULL, NULL, @ProductSettingId OUTPUT;
END;

EXEC Enterprise.LinkProductSettingToConfiguration @ConfigurationId, @ProductSettingId, NULL, NULL;

SET @ProductSettingTypeId = NULL;
SET @ProductSettingId = NULL;
GO

/*****************************************
AO INtegration
*****************************************/

--DECLARATION BLOCK
DECLARE @ParentProductTypeId INT
DECLARE @ProductTypeId int
DECLARE @Description varchar(1000)
DECLARE @ProductTypeGUID uniqueidentifier
DECLARE @ProductID INT;
DECLARE @ProductGUID UNIQUEIDENTIFIER;
DECLARE @Name NVARCHAR(50);
DECLARE @ProductSettingTypeName VARCHAR(50);
DECLARE @ProductSettingTypeDescription VARCHAR(100);
DECLARE @ProductSettingTypeId INT;
DECLARE @fromdate DATETIME;
DECLARE @ConfigurationId INT;
DECLARE @ProductSettingId INT;

--Setup new product type

SELECT @ParentProductTypeId = ProductTypeId  FROM Enterprise.ProductType WHERE Name = 'Asset Optimization'

IF NOT EXISTS (SELECT 1 FROM Enterprise.ProductType WHERE Name = 'Revenue Management' AND ParentProductTypeId = @ParentProductTypeId)
BEGIN
	EXECUTE  [Enterprise].[CreateProductType] 
	   @ProductTypeId = 401
	  ,@ParentProductTypeId = @ParentProductTypeId
	  ,@Name = 'Revenue Management'
	  ,@Description = 'Revenue Management'
	  ,@ProductTypeGUID = '751761DD-4053-4321-B499-504FCBB34B02'
END
ELSE
BEGIN
	UPDATE Enterprise.ProductType
		SET ProductTypeId = 401
		, Name = 'Revenue Management'
		, Description = 'Revenue Management'
		, ProductTypeGUID = '751761DD-4053-4321-B499-504FCBB34B02'
		, ParentProductTypeId = @ParentProductTypeId
	WHERE Name = 'Revenue Management'
		AND ParentProductTypeId = @ParentProductTypeId
END

IF NOT EXISTS (SELECT 1 FROM Enterprise.ProductType WHERE Name = 'Business Intelligence' AND ParentProductTypeId = @ParentProductTypeId)
BEGIN
	EXECUTE  [Enterprise].[CreateProductType] 
	   @ProductTypeId = 402
	  ,@ParentProductTypeId = @ParentProductTypeId
	  ,@Name = 'Business Intelligence'
	  ,@Description = 'Business Intelligence'
	  ,@ProductTypeGUID = '39307163-A3E9-4325-B4AF-CBFF9FA04EEA'
END
ELSE
BEGIN
	UPDATE Enterprise.ProductType
		SET ProductTypeId = 402
		, Name = 'Business Intelligence'
		, Description = 'Business Intelligence'
		, ProductTypeGUID = '39307163-A3E9-4325-B4AF-CBFF9FA04EEA'
		, ParentProductTypeId = @ParentProductTypeId
	WHERE Name = 'Business Intelligence'
		AND ParentProductTypeId = @ParentProductTypeId
END

IF NOT EXISTS (SELECT 1 FROM Enterprise.ProductType WHERE Name = 'Performance Analytics' AND ParentProductTypeId = @ParentProductTypeId)
BEGIN
	EXECUTE  [Enterprise].[CreateProductType] 
	   @ProductTypeId = 403
	  ,@ParentProductTypeId = @ParentProductTypeId
	  ,@Name = 'Performance Analytics'
	  ,@Description = 'Performance Analytics'
	  ,@ProductTypeGUID = 'EF812580-BB8A-4192-BB96-722F9FCB0BAB'
END
ELSE
BEGIN
	UPDATE Enterprise.ProductType
		SET ProductTypeId = 403
		, Name = 'Performance Analytics'
		, Description = 'Performance Analytics'
		, ProductTypeGUID = 'EF812580-BB8A-4192-BB96-722F9FCB0BAB'
		, ParentProductTypeId = @ParentProductTypeId
	WHERE Name = 'Performance Analytics'
		AND ParentProductTypeId = @ParentProductTypeId
END

IF NOT EXISTS (SELECT 1 FROM Enterprise.ProductType WHERE Name = 'Investment Analytics' AND ParentProductTypeId = @ParentProductTypeId)
BEGIN
	EXECUTE  [Enterprise].[CreateProductType] 
	   @ProductTypeId = 404
	  ,@ParentProductTypeId = @ParentProductTypeId
	  ,@Name = 'Investment Analytics'
	  ,@Description = 'Investment Analytics'
	  ,@ProductTypeGUID = 'EDEE6143-7368-4F9F-B71F-45498D9D0D4F'
END
ELSE
BEGIN
	UPDATE Enterprise.ProductType
		SET ProductTypeId = 404
		, Name = 'Investment Analytics'
		, Description = 'Investment Analytics'
		, ProductTypeGUID = 'EDEE6143-7368-4F9F-B71F-45498D9D0D4F'
		, ParentProductTypeId = @ParentProductTypeId
	WHERE Name = 'Investment Analytics'
		AND ParentProductTypeId = @ParentProductTypeId
END


IF NOT EXISTS (SELECT 1 FROM Enterprise.ProductType WHERE Name = 'Benchmarking' AND ParentProductTypeId = @ParentProductTypeId)
BEGIN
	EXECUTE  [Enterprise].[CreateProductType] 
	   @ProductTypeId = 405
	  ,@ParentProductTypeId = @ParentProductTypeId
	  ,@Name = 'Benchmarking'
	  ,@Description = 'Benchmarking'
	  ,@ProductTypeGUID = '8C3E2109-F7E9-43DC-882C-E0873083BD07'
END
ELSE
BEGIN
	UPDATE Enterprise.ProductType
		SET ProductTypeId = 405
		, Name = 'Benchmarking'
		, Description = 'Benchmarking'
		, ProductTypeGUID = '8C3E2109-F7E9-43DC-882C-E0873083BD07'
		, ParentProductTypeId = @ParentProductTypeId
	WHERE Name = 'Benchmarking'
		AND ParentProductTypeId = @ParentProductTypeId
END

IF NOT EXISTS (SELECT 1 FROM Enterprise.ProductType WHERE Name = 'Axiometrics' AND ParentProductTypeId = @ParentProductTypeId)
BEGIN
	EXECUTE  [Enterprise].[CreateProductType] 
	   @ProductTypeId = 406
	  ,@ParentProductTypeId = @ParentProductTypeId
	  ,@Name = 'Axiometrics'
	  ,@Description = 'Axiometrics'
	  ,@ProductTypeGUID = '9F5A46F0-1387-4632-9326-1C8F07D86A3A'
END
ELSE
BEGIN
	UPDATE Enterprise.ProductType
		SET ProductTypeId = 406
		, Name = 'Axiometrics'
		, Description = 'Axiometrics'
		, ProductTypeGUID = '9F5A46F0-1387-4632-9326-1C8F07D86A3A'
		, ParentProductTypeId = @ParentProductTypeId
	WHERE Name = 'Axiometrics'
		AND ParentProductTypeId = @ParentProductTypeId
END




--Change settings for Exsiting AO - ProductId = 4



SELECT @ProductId = ProductId
FROM Enterprise.Product
WHERE Name = 'Asset Optimization';

/***********************************
Add New products
***********************************/

--1 AO

IF NOT EXISTS
(
    SELECT 1
    FROM Enterprise.Product
    WHERE Name = 'Asset Optimization'
)
    BEGIN
        SET @ProductId = 4;
        SET @ProductGUID = 'AA4B4561-DE40-4BF1-A934-CAB15D9E8D57';
        SET @Name = 'Asset Optimization';
        SET @Description = 'RealPage Portfolio Asset Management (PAM) is a solution designed and developed specifically for general partners, limited partners, and property management professionals, to provide the portfolio data, critical metrics, and thorough analysis you need, regardless of asset type or operational platform. You’ll have the power to collect financial and operating data and collaborate with property management partners, enabling them to continue to leverage their existing operational structures and best business practices.';
        SET @ProductTypeID = 400;
        EXECUTE Enterprise.CreateProduct
                @ProductId,
                @ProductGUID,
                @Name,
                @Description,
                @ProductTypeID;
    END;
IF EXISTS
(
    SELECT 1
    FROM Enterprise.Product
    WHERE Name = 'Asset Optimization'
)
    BEGIN
        SELECT @ProductId = ProductId
        FROM Enterprise.Product
        WHERE Name = 'Asset Optimization';
        UPDATE Enterprise.Product
          SET
              Name = 'Asset Optimization',
              Description = 'RealPage Portfolio Asset Management (PAM) is a solution designed and developed specifically for general partners, limited partners, and property management professionals, to provide the portfolio data, critical metrics, and thorough analysis you need, regardless of asset type or operational platform. You’ll have the power to collect financial and operating data and collaborate with property management partners, enabling them to continue to leverage their existing operational structures and best business practices.',
              ProductTypeId = 400,
              ProductGUID = 'AA4B4561-DE40-4BF1-A934-CAB15D9E8D57',
              BooksProductCode = 'AO'
        WHERE ProductId = @ProductId;
    END;
--2 BI

IF NOT EXISTS
(
    SELECT 1
    FROM Enterprise.Product
    WHERE Name = 'Business Intelligence'
)
    BEGIN
        SET @ProductId = 29;
        SET @ProductGUID = 'B4B68C15-9C1E-44B7-B830-5D7E2F1F6CBB';
        SET @Name = 'Business Intelligence';
        SET @Description = 'Business Intelligence';
        SET @ProductTypeID = 402;
        EXECUTE Enterprise.CreateProduct
                @ProductId,
                @ProductGUID,
                @Name,
                @Description,
                @ProductTypeID;
    END;
IF EXISTS
(
    SELECT 1
    FROM Enterprise.Product
    WHERE Name = 'Business Intelligence'
)
    BEGIN
        SELECT @ProductId = ProductId
        FROM Enterprise.Product
        WHERE Name = 'Business Intelligence';
        UPDATE Enterprise.Product
          SET
              Name = 'Business Intelligence',
              Description = 'Business Intelligence',
              ProductTypeId = 402,
              ProductGUID = 'B4B68C15-9C1E-44B7-B830-5D7E2F1F6CBB',
              BooksProductCode = 'BI'
        WHERE ProductId = @ProductId;
    END;
--3 PA

IF NOT EXISTS
(
    SELECT 1
    FROM Enterprise.Product
    WHERE Name = 'Performance Analytics'
)
    BEGIN
        SET @ProductId = 30;
        SET @ProductGUID = 'C842310A-5488-44C0-87CD-4E311F264135';
        SET @Name = 'Performance Analytics';
        SET @Description = 'Performance Analytics';
        SET @ProductTypeID = 403;
        EXECUTE Enterprise.CreateProduct
                @ProductId,
                @ProductGUID,
                @Name,
                @Description,
                @ProductTypeID;
    END;
IF EXISTS
(
    SELECT 1
    FROM Enterprise.Product
    WHERE Name = 'Performance Analytics'
)
    BEGIN
        SELECT @ProductId = ProductId
        FROM Enterprise.Product
        WHERE Name = 'Performance Analytics';
        UPDATE Enterprise.Product
          SET
              Name = 'Performance Analytics',
              Description = 'Performance Analytics',
              ProductTypeId = 403,
              ProductGUID = 'C842310A-5488-44C0-87CD-4E311F264135',
              BooksProductCode = 'PA'
        WHERE ProductId = @ProductId;
    END;
--4 MA

IF NOT EXISTS
(
    SELECT 1
    FROM Enterprise.Product
    WHERE Name = 'Investment Analytics'
)
    BEGIN
        SET @ProductId = 31;
        SET @ProductGUID = '8033BC51-7603-44E5-A60F-643D023064F9';
        SET @Name = 'Investment Analytics';
        SET @Description = 'Investment Analytics';
        SET @ProductTypeID = 404;
        EXECUTE Enterprise.CreateProduct
                @ProductId,
                @ProductGUID,
                @Name,
                @Description,
                @ProductTypeID;
    END;
IF EXISTS
(
    SELECT 1
    FROM Enterprise.Product
    WHERE Name = 'Investment Analytics'
)
    BEGIN
        SELECT @ProductId = ProductId
        FROM Enterprise.Product
        WHERE Name = 'Investment Analytics';
        UPDATE Enterprise.Product
          SET
              Name = 'Investment Analytics',
              Description = 'Investment Analytics',
              ProductTypeId = 404,
              ProductGUID = '8033BC51-7603-44E5-A60F-643D023064F9',
              BooksProductCode = 'MA'
        WHERE ProductId = @ProductId;
    END;
--5 MA

IF NOT EXISTS
(
    SELECT 1
    FROM Enterprise.Product
    WHERE Name = 'Revenue Management'
)
    BEGIN
        SET @ProductId = 32;
        SET @ProductGUID = 'DE87C603-6497-4479-8E55-4181D8699333';
        SET @Name = 'Revenue Management';
        SET @Description = 'Revenue Management';
        SET @ProductTypeID = 401;
        EXECUTE Enterprise.CreateProduct
                @ProductId,
                @ProductGUID,
                @Name,
                @Description,
                @ProductTypeID;
    END;
IF EXISTS
(
    SELECT 1
    FROM Enterprise.Product
    WHERE Name = 'Revenue Management'
)
    BEGIN
        SELECT @ProductId = ProductId
        FROM Enterprise.Product
        WHERE Name = 'Revenue Management';
        UPDATE Enterprise.Product
          SET
              Name = 'Revenue Management',
              Description = 'AO-Revenue Management',
              ProductTypeId = 401,
              ProductGUID = 'DE87C603-6497-4479-8E55-4181D8699333',
              BooksProductCode = 'PO'
        WHERE ProductId = @ProductId;
    END;
--6 AX

IF NOT EXISTS
(
    SELECT 1
    FROM Enterprise.Product
    WHERE Name = 'Axiometrics'
)
    BEGIN
        SET @ProductId = 33;
        SET @ProductGUID = '520CC62B-E5DF-4B15-B74E-A00C1483F61B';
        SET @Name = 'Axiometrics';
        SET @Description = 'Axiometrics';
        SET @ProductTypeID = 406;
        EXECUTE Enterprise.CreateProduct
                @ProductId,
                @ProductGUID,
                @Name,
                @Description,
                @ProductTypeID;
    END;
IF EXISTS
(
    SELECT 1
    FROM Enterprise.Product
    WHERE Name = 'Axiometrics'
)
    BEGIN
        SELECT @ProductId = ProductId
        FROM Enterprise.Product
        WHERE Name = 'Axiometrics';
        UPDATE Enterprise.Product
          SET
              Name = 'Axiometrics',
              Description = 'Axiometrics',
              ProductTypeId = 406,
              ProductGUID = '520CC62B-E5DF-4B15-B74E-A00C1483F61B',
              BooksProductCode = 'AX'
        WHERE ProductId = @ProductId;
    END;
--7 bench

IF NOT EXISTS
(
    SELECT 1
    FROM Enterprise.Product
    WHERE Name = 'Benchmarking'
)
    BEGIN
        SET @ProductId = 34;
        SET @ProductGUID = '0AC7AD57-E84D-4C39-845C-D05097EE736A';
        SET @Name = 'Benchmarking';
        SET @Description = 'Benchmarking';
        SET @ProductTypeID = 405;
        EXECUTE Enterprise.CreateProduct
                @ProductId,
                @ProductGUID,
                @Name,
                @Description,
                @ProductTypeID;
    END;
IF EXISTS
(
    SELECT 1
    FROM Enterprise.Product
    WHERE Name = 'Benchmarking'
)
    BEGIN
        SELECT @ProductId = ProductId
        FROM Enterprise.Product
        WHERE Name = 'Benchmarking';
        UPDATE Enterprise.Product
          SET
              Name = 'Benchmarking',
              Description = 'Benchmarking',
              ProductTypeId = 405,
              ProductGUID = '0AC7AD57-E84D-4C39-845C-D05097EE736A',
              BooksProductCode = 'BM'
        WHERE ProductId = @ProductId;
    END;

/***********************************
Validating ProductSettingType
***********************************/

IF NOT EXISTS
(
    SELECT 1
    FROM Enterprise.ProductSettingType
    WHERE Name = 'ProductSuperUserLoginName'
)
    BEGIN
        SET @ProductSettingTypeName = 'ProductSuperUserLoginName';
        SET @ProductSettingTypeDescription = 'Product Super User Login Name';
        EXECUTE Enterprise.CreateProductSettingType
                @ProductSettingTypeName = @ProductSettingTypeName,
                @ProductSettingTypeDescription = @ProductSettingTypeDescription,
                @ProductSettingTypeId = @ProductSettingTypeId OUTPUT;
    END;

/****** 
Add or update Products Setting in table [Ident].[ProductSetting] 
******/

--Step 3 -- Add all settings

SET @FromDate = GETUTCDATE();
-- If this is a new Global Product Configuration then create a Product Configuration if you do not have one to use.

SELECT @ProductId = ProductId
FROM Enterprise.Product
WHERE Name = 'Asset Optimization';
IF @ProductId = 4
    BEGIN
        IF NOT EXISTS
(
    SELECT 1
    FROM Ident.SamlProductSettings
    WHERE ProductId = @ProductId
)
            BEGIN
                IF @@SERVERNAME = 'RCTUSODBSQL001'
                    BEGIN
                        INSERT INTO Ident.SamlProductSettings
						(ProductId,
						 LoginUri,
						 SigningCertificateThumbprint,
						 SubjectIdSamlAttribute
						)
                        VALUES
					(@ProductId,
					 'https://aosat.realpage.com/ysconfig/sso/unified',
					 '0BE4C7B686D1918A4B2B571E8BF098B994990CAB',
					 'productUsername'
					);
                    END;
                IF @@SERVERNAME = 'RCQUSODBSQL001'
                    BEGIN
                        INSERT INTO Ident.SamlProductSettings
							(ProductId,
							 LoginUri,
							 SigningCertificateThumbprint,
							 SubjectIdSamlAttribute
							)
                        VALUES
				(@ProductId,
				 'https://aosat.realpage.com/ysconfig/sso/unified',
				 'EF26FEC08C554976572E8A9767DDA437AC452CF6',
				 'productUsername'
				);
                    END;
                IF @@SERVERNAME IN('RCPGBKDBSQL005A', 'RCPGBKDBSQL005B')
                    BEGIN
                        INSERT INTO Ident.SamlProductSettings
(ProductId,
 LoginUri,
 SigningCertificateThumbprint,
 SubjectIdSamlAttribute
)
                        VALUES
(@ProductId,
 'https://ao.realpage.com/ysconfig/sso/unified',
 'EF26FEC08C554976572E8A9767DDA437AC452CF6',
 'productUsername'
);
                    END;
            END;
            ELSE
            BEGIN
                IF @@SERVERNAME = 'RCTUSODBSQL001'
                    BEGIN
                        UPDATE Ident.SamlProductSettings
                          SET
                              LoginUri = 'https://aosat.realpage.com/ysconfig/sso/unified',
                              SigningCertificateThumbprint = '0BE4C7B686D1918A4B2B571E8BF098B994990CAB'
                        WHERE ProductId = @ProductId;
                    END;
                IF @@SERVERNAME = 'RCQUSODBSQL001'
                    BEGIN
                        UPDATE Ident.SamlProductSettings
                          SET
                              LoginUri = 'https://aosat.realpage.com/ysconfig/sso/unified',
                              SigningCertificateThumbprint = 'EF26FEC08C554976572E8A9767DDA437AC452CF6'
                        WHERE ProductId = @ProductId;
                    END;
                IF @@SERVERNAME IN('RCPGBKDBSQL005A', 'RCPGBKDBSQL005B')
                    BEGIN
                        UPDATE Ident.SamlProductSettings
                          SET
                              LoginUri = 'https://ao.realpage.com/ysconfig/sso/unified',
                              SigningCertificateThumbprint = 'EF26FEC08C554976572E8A9767DDA437AC452CF6'
                        WHERE ProductId = @ProductId;
                    END;
            END;
        IF EXISTS
(
    SELECT 1
    FROM Enterprise.GlobalProductConfiguration
    WHERE ProductId = @ProductId
          AND (GETDATE() BETWEEN FromDate AND ThruDate
               OR ThruDate IS NULL)
)
            BEGIN
                SELECT @ConfigurationId = ConfigurationId
                FROM Enterprise.GlobalProductConfiguration
                WHERE ProductId = @ProductId
                      AND (GETDATE() BETWEEN FromDate AND ThruDate
                           OR ThruDate IS NULL);
            END;
            ELSE
            BEGIN
                EXEC Enterprise.CreateProductConfiguration
                     @ConfigurationId = @ConfigurationId OUTPUT; -- int
            END;
--***** Start a Product setting loop for each attribute / value that needs set. *****
------------------------------------------------------------------------------------------------------------------------------------------------------
-- Get the ProductSettingTypeId for the attribute you need to select, the API should have all these cached though.

        SET @ProductSettingId = NULL;
        SET @ProductSettingTypeId = NULL;
        EXEC Enterprise.GetProductSettingType
             @Name = 'ProductUrl', -- varchar(50)
             @ProductSettingTypeId = @ProductSettingTypeId OUTPUT; -- int


-- Create the Value and assign it to the Product and ProductSettingType
        IF EXISTS
(
    SELECT 1
    FROM enterprise.ProductSetting AS PS
         INNER JOIN Enterprise.productsettingtype AS pp ON PS.productsettingtypeid = pp.productsettingtypeid
    WHERE productid = @ProductId
          AND PP.ProductSettingTypeId = @ProductSettingTypeId
)
            BEGIN
                UPDATE PS
                  SET
                      value = '/product/assetoptimizer'
                FROM Enterprise.ProductSetting PS
                     INNER JOIN Enterprise.productsettingtype pp ON PS.productsettingtypeid = pp.productsettingtypeid
                WHERE productid = @ProductId
                      AND PP.ProductSettingTypeId = @ProductSettingTypeId;
            END;
            ELSE
            BEGIN
                EXEC Enterprise.CreateProductSetting
                     @ProductId = @ProductId, -- int
                     @ProductSettingTypeId = @ProductSettingTypeId, -- int
                     @Value = N'/product/assetoptimizer', -- nvarchar(1000)
                     @FromDate = @FromDate, -- datetime
                     @ThruDate = NULL, -- datetime
                     @ProductSettingId = @ProductSettingId OUTPUT; -- int

-- Link the Product Setting to an actual configuration
                EXEC Enterprise.LinkProductSettingToConfiguration
                     @ConfigurationId = @ConfigurationId, -- int
                     @ProductSettingId = @ProductSettingId, -- int
                     @FromDate = @FromDate, -- datetime
                     @ThruDate = NULL;   -- datetime
            END;
------------------------------------------------------------------------------------------------------------------------------------------------------
-- Get the ProductSettingTypeId for the attribute you need to select, the API should have all these cached though.

        SET @ProductSettingId = NULL;
        SET @ProductSettingTypeId = NULL;
        EXEC Enterprise.GetProductSettingType
             @Name = 'TitleId', -- varchar(50)
             @ProductSettingTypeId = @ProductSettingTypeId OUTPUT; -- int

        IF EXISTS
(
    SELECT 1
    FROM enterprise.ProductSetting AS PS
         INNER JOIN Enterprise.productsettingtype AS pp ON PS.productsettingtypeid = pp.productsettingtypeid
    WHERE productid = @ProductId
          AND PP.ProductSettingTypeId = @ProductSettingTypeId
)
            BEGIN
                UPDATE PS
                  SET
                      value = 'Asset Optimization'
                FROM Enterprise.ProductSetting PS
                     INNER JOIN Enterprise.productsettingtype pp ON PS.productsettingtypeid = pp.productsettingtypeid
                WHERE productid = @ProductId
                      AND PP.ProductSettingTypeId = @ProductSettingTypeId;
            END;
            ELSE
            BEGIN
                EXEC Enterprise.CreateProductSetting
                     @ProductId = @ProductId, -- int
                     @ProductSettingTypeId = @ProductSettingTypeId, -- int
                     @Value = N'Asset Optimization', -- nvarchar(1000)
                     @FromDate = @FromDate, -- datetime
                     @ThruDate = NULL, -- datetime
                     @ProductSettingId = @ProductSettingId OUTPUT; -- int

-- Link the Product Setting to an actual configuration
                EXEC Enterprise.LinkProductSettingToConfiguration
                     @ConfigurationId = @ConfigurationId, -- int
                     @ProductSettingId = @ProductSettingId, -- int
                     @FromDate = @FromDate, -- datetime
                     @ThruDate = NULL;   -- datetime
            END;
------------------------------------------------------------------------------------------------------------------------------------------------------
-- Get the ProductSettingTypeId for the attribute you need to select, the API should have all these cached though.
        SET @ProductSettingId = NULL;
        SET @ProductSettingTypeId = NULL;
        EXEC Enterprise.GetProductSettingType
             @Name = 'TitleUniqueId', -- varchar(50)
             @ProductSettingTypeId = @ProductSettingTypeId OUTPUT; -- int

-- Create the Value and assign it to the Product and ProductSettingType
        IF EXISTS
(
    SELECT 1
    FROM enterprise.ProductSetting AS PS
         INNER JOIN Enterprise.productsettingtype AS pp ON PS.productsettingtypeid = pp.productsettingtypeid
    WHERE productid = @ProductId
          AND PP.ProductSettingTypeId = @ProductSettingTypeId
)
            BEGIN
                UPDATE PS
                  SET
                      value = 'EA66353F-338E-444E-8775-06FDC6B4D020'
                FROM Enterprise.ProductSetting PS
                     INNER JOIN Enterprise.productsettingtype pp ON PS.productsettingtypeid = pp.productsettingtypeid
                WHERE productid = @ProductId
                      AND PP.ProductSettingTypeId = @ProductSettingTypeId;
            END;
            ELSE
            BEGIN
                EXEC Enterprise.CreateProductSetting
                     @ProductId = @ProductId, -- int
                     @ProductSettingTypeId = @ProductSettingTypeId, -- int
                     @Value = N'EA66353F-338E-444E-8775-06FDC6B4D020', -- nvarchar(1000)
                     @FromDate = @FromDate, -- datetime
                     @ThruDate = NULL, -- datetime
                     @ProductSettingId = @ProductSettingId OUTPUT; -- int

-- Link the Product Setting to an actual configuration
                EXEC Enterprise.LinkProductSettingToConfiguration
                     @ConfigurationId = @ConfigurationId, -- int
                     @ProductSettingId = @ProductSettingId, -- int
                     @FromDate = @FromDate, -- datetime
                     @ThruDate = NULL;   -- datetime
            END;
------------------------------------------------------------------------------------------------------------------------------------------------------
-- Get the ProductSettingTypeId for the attribute you need to select, the API should have all these cached though.
        SET @ProductSettingId = NULL;
        SET @ProductSettingTypeId = NULL;
        EXEC Enterprise.GetProductSettingType
             @Name = 'IsNewTab', -- varchar(50)
             @ProductSettingTypeId = @ProductSettingTypeId OUTPUT; -- int

-- Create the Value and assign it to the Product and ProductSettingType
        IF EXISTS
(
    SELECT 1
    FROM enterprise.ProductSetting AS PS
         INNER JOIN Enterprise.productsettingtype AS pp ON PS.productsettingtypeid = pp.productsettingtypeid
    WHERE productid = @ProductId
          AND PP.ProductSettingTypeId = @ProductSettingTypeId
)
            BEGIN
                UPDATE PS
                  SET
                      value = '1'
                FROM Enterprise.ProductSetting PS
                     INNER JOIN Enterprise.productsettingtype pp ON PS.productsettingtypeid = pp.productsettingtypeid
                WHERE productid = @ProductId
                      AND PP.ProductSettingTypeId = @ProductSettingTypeId;
            END;
            ELSE
            BEGIN
                EXEC Enterprise.CreateProductSetting
                     @ProductId = @ProductId, -- int
                     @ProductSettingTypeId = @ProductSettingTypeId, -- int
                     @Value = N'1', -- nvarchar(1000)
                     @FromDate = @FromDate, -- datetime
                     @ThruDate = NULL, -- datetime
                     @ProductSettingId = @ProductSettingId OUTPUT; -- int

-- Link the Product Setting to an actual configuration
                EXEC Enterprise.LinkProductSettingToConfiguration
                     @ConfigurationId = @ConfigurationId, -- int
                     @ProductSettingId = @ProductSettingId, -- int
                     @FromDate = @FromDate, -- datetime
                     @ThruDate = NULL;   -- datetime
            END;
------------------------------------------------------------------------------------------------------------------------------------------------------
-- Get the ProductSettingTypeId for the attribute you need to select, the API should have all these cached though.
        SET @ProductSettingId = NULL;
        SET @ProductSettingTypeId = NULL;
        EXEC Enterprise.GetProductSettingType
             @Name = 'MetatagUniqueId', -- varchar(50)
             @ProductSettingTypeId = @ProductSettingTypeId OUTPUT; -- int

-- Create the Value and assign it to the Product and ProductSettingType
        IF EXISTS
(
    SELECT 1
    FROM enterprise.ProductSetting AS PS
         INNER JOIN Enterprise.productsettingtype AS pp ON PS.productsettingtypeid = pp.productsettingtypeid
    WHERE productid = @ProductId
          AND PP.ProductSettingTypeId = @ProductSettingTypeId
)
            BEGIN
                UPDATE PS
                  SET
                      value = 'Asset Optimization'
                FROM Enterprise.ProductSetting PS
                     INNER JOIN Enterprise.productsettingtype pp ON PS.productsettingtypeid = pp.productsettingtypeid
                WHERE productid = @ProductId
                      AND PP.ProductSettingTypeId = @ProductSettingTypeId;
            END;
            ELSE
            BEGIN
                EXEC Enterprise.CreateProductSetting
                     @ProductId = @ProductId, -- int
                     @ProductSettingTypeId = @ProductSettingTypeId, -- int
                     @Value = N'Asset Optimization', -- nvarchar(1000)
                     @FromDate = @FromDate, -- datetime
                     @ThruDate = NULL, -- datetime
                     @ProductSettingId = @ProductSettingId OUTPUT; -- int

-- Link the Product Setting to an actual configuration
                EXEC Enterprise.LinkProductSettingToConfiguration
                     @ConfigurationId = @ConfigurationId, -- int
                     @ProductSettingId = @ProductSettingId, -- int
                     @FromDate = @FromDate, -- datetime
                     @ThruDate = NULL;   -- datetime
            END;
------------------------------------------------------------------------------------------------------------------------------------------------------
-- Get the ProductSettingTypeId for the attribute you need to select, the API should have all these cached though.
        SET @ProductSettingId = NULL;
        SET @ProductSettingTypeId = NULL;
        EXEC Enterprise.GetProductSettingType
             @Name = 'IsFavorite', -- varchar(50)
             @ProductSettingTypeId = @ProductSettingTypeId OUTPUT; -- int

-- Create the Value and assign it to the Product and ProductSettingType
        IF EXISTS
(
    SELECT 1
    FROM enterprise.ProductSetting AS PS
         INNER JOIN Enterprise.productsettingtype AS pp ON PS.productsettingtypeid = pp.productsettingtypeid
    WHERE productid = @ProductId
          AND PP.ProductSettingTypeId = @ProductSettingTypeId
)
            BEGIN
                UPDATE PS
                  SET
                      value = '1'
                FROM Enterprise.ProductSetting PS
                     INNER JOIN Enterprise.productsettingtype pp ON PS.productsettingtypeid = pp.productsettingtypeid
                WHERE productid = @ProductId
                      AND PP.ProductSettingTypeId = @ProductSettingTypeId;
            END;
            ELSE
            BEGIN
                EXEC Enterprise.CreateProductSetting
                     @ProductId = @ProductId, -- int
                     @ProductSettingTypeId = @ProductSettingTypeId, -- int
                     @Value = N'1', -- nvarchar(1000)
                     @FromDate = @FromDate, -- datetime
                     @ThruDate = NULL, -- datetime
                     @ProductSettingId = @ProductSettingId OUTPUT; -- int

-- Link the Product Setting to an actual configuration
                EXEC Enterprise.LinkProductSettingToConfiguration
                     @ConfigurationId = @ConfigurationId, -- int
                     @ProductSettingId = @ProductSettingId, -- int
                     @FromDate = @FromDate, -- datetime
                     @ThruDate = NULL;   -- datetime
            END;
------------------------------------------------------------------------------------------------------------------------------------------------------
-- Get the ProductSettingTypeId for the attribute you need to select, the API should have all these cached though.
        SET @ProductSettingId = NULL;
        SET @ProductSettingTypeId = NULL;
        EXEC Enterprise.GetProductSettingType
             @Name = 'LearnMore', -- varchar(50)
             @ProductSettingTypeId = @ProductSettingTypeId OUTPUT; -- int

-- Create the Value and assign it to the Product and ProductSettingType
        IF EXISTS
(
    SELECT 1
    FROM enterprise.ProductSetting AS PS
         INNER JOIN Enterprise.productsettingtype AS pp ON PS.productsettingtypeid = pp.productsettingtypeid
    WHERE productid = @ProductId
          AND PP.ProductSettingTypeId = @ProductSettingTypeId
)
            BEGIN
                UPDATE PS
                  SET
                      value = 'https://www.realpage.com/asset-investment-management/portfolio-asset-management/'
                FROM Enterprise.ProductSetting PS
                     INNER JOIN Enterprise.productsettingtype pp ON PS.productsettingtypeid = pp.productsettingtypeid
                WHERE productid = @ProductId
                      AND PP.ProductSettingTypeId = @ProductSettingTypeId;
            END;
            ELSE
            BEGIN
                EXEC Enterprise.CreateProductSetting
                     @ProductId = @ProductId, -- int
                     @ProductSettingTypeId = @ProductSettingTypeId, -- int
                     @Value = N'https://www.realpage.com/asset-investment-management/portfolio-asset-management/', -- nvarchar(1000)
                     @FromDate = @FromDate, -- datetime
                     @ThruDate = NULL, -- datetime
                     @ProductSettingId = @ProductSettingId OUTPUT; -- int

-- Link the Product Setting to an actual configuration
                EXEC Enterprise.LinkProductSettingToConfiguration
                     @ConfigurationId = @ConfigurationId, -- int
                     @ProductSettingId = @ProductSettingId, -- int
                     @FromDate = @FromDate, -- datetime
                     @ThruDate = NULL;   -- datetime
            END;
------------------------------------------------------------------------------------------------------------------------------------------------------
-- Get the ProductSettingTypeId for the attribute you need to select, the API should have all these cached though.
        SET @ProductSettingId = NULL;
        SET @ProductSettingTypeId = NULL;
        EXEC Enterprise.GetProductSettingType
             @Name = 'ApiEndPoint', -- varchar(50)
             @ProductSettingTypeId = @ProductSettingTypeId OUTPUT; -- int

-- Create the Value and assign it to the Product and ProductSettingType
        IF EXISTS
(
    SELECT 1
    FROM enterprise.ProductSetting AS PS
         INNER JOIN Enterprise.productsettingtype AS pp ON PS.productsettingtypeid = pp.productsettingtypeid
    WHERE productid = @ProductId
          AND PP.ProductSettingTypeId = @ProductSettingTypeId
)
            BEGIN
                UPDATE PS
                  SET
                      value = 'https://aosat.realpage.com/ysconfig/ws/'
                FROM Enterprise.ProductSetting PS
                     INNER JOIN Enterprise.productsettingtype pp ON PS.productsettingtypeid = pp.productsettingtypeid
                WHERE productid = @ProductId
                      AND PP.ProductSettingTypeId = @ProductSettingTypeId;
            END;
            ELSE
            BEGIN
                EXEC Enterprise.CreateProductSetting
                     @ProductId = @ProductId, -- int
                     @ProductSettingTypeId = @ProductSettingTypeId, -- int
                     @Value = N'https://aosat.realpage.com/ysconfig/ws/', -- nvarchar(1000)
                     @FromDate = @FromDate, -- datetime
                     @ThruDate = NULL, -- datetime
                     @ProductSettingId = @ProductSettingId OUTPUT; -- int

-- Link the Product Setting to an actual configuration
                EXEC Enterprise.LinkProductSettingToConfiguration
                     @ConfigurationId = @ConfigurationId, -- int
                     @ProductSettingId = @ProductSettingId, -- int
                     @FromDate = @FromDate, -- datetime
                     @ThruDate = NULL;   -- datetime
            END;
------------------------------------------------------------------------------------------------------------------------------------------------------
-- Get the ProductSettingTypeId for the attribute you need to select, the API should have all these cached though.
------------------------------------------------------------------------------------------------------------------------------------------------------
        SET @ProductSettingId = NULL;
        SET @ProductSettingTypeId = NULL;
        EXEC Enterprise.GetProductSettingType
             @Name = 'ApiUserName', -- varchar(50)
             @ProductSettingTypeId = @ProductSettingTypeId OUTPUT; -- int

-- Create the Value and assign it to the Product and ProductSettingType
        IF EXISTS
(
    SELECT 1
    FROM enterprise.ProductSetting AS PS
         INNER JOIN Enterprise.productsettingtype AS pp ON PS.productsettingtypeid = pp.productsettingtypeid
    WHERE productid = @ProductId
          AND PP.ProductSettingTypeId = @ProductSettingTypeId
)
            BEGIN
                UPDATE PS
                  SET
                      value = 'wsuser'
                FROM Enterprise.ProductSetting PS
                     INNER JOIN Enterprise.productsettingtype pp ON PS.productsettingtypeid = pp.productsettingtypeid
                WHERE productid = @ProductId
                      AND PP.ProductSettingTypeId = @ProductSettingTypeId;
            END;
            ELSE
            BEGIN
                EXEC Enterprise.CreateProductSetting
                     @ProductId = @ProductId, -- int
                     @ProductSettingTypeId = @ProductSettingTypeId, -- int
                     @Value = N'wsuser', -- nvarchar(1000)
                     @FromDate = @FromDate, -- datetime
                     @ThruDate = NULL, -- datetime
                     @ProductSettingId = @ProductSettingId OUTPUT; -- int

-- Link the Product Setting to an actual configuration
                EXEC Enterprise.LinkProductSettingToConfiguration
                     @ConfigurationId = @ConfigurationId, -- int
                     @ProductSettingId = @ProductSettingId, -- int
                     @FromDate = @FromDate, -- datetime
                     @ThruDate = NULL;   -- datetime
            END;
------------------------------------------------------------------------------------------------------------------------------------------------------
-- Get the ProductSettingTypeId for the attribute you need to select, the API should have all these cached though.
------------------------------------------------------------------------------------------------------------------------------------------------------
        SET @ProductSettingId = NULL;
        SET @ProductSettingTypeId = NULL;
        EXEC Enterprise.GetProductSettingType
             @Name = 'ApiPassword', -- varchar(50)
             @ProductSettingTypeId = @ProductSettingTypeId OUTPUT; -- int

-- Create the Value and assign it to the Product and ProductSettingType
        IF EXISTS
(
    SELECT 1
    FROM enterprise.ProductSetting AS PS
         INNER JOIN Enterprise.productsettingtype AS pp ON PS.productsettingtypeid = pp.productsettingtypeid
    WHERE productid = @ProductId
          AND PP.ProductSettingTypeId = @ProductSettingTypeId
)
            BEGIN
                UPDATE PS
                  SET
                      value = 'cGdAIXcyM3Jn'
                FROM Enterprise.ProductSetting PS
                     INNER JOIN Enterprise.productsettingtype pp ON PS.productsettingtypeid = pp.productsettingtypeid
                WHERE productid = @ProductId
                      AND PP.ProductSettingTypeId = @ProductSettingTypeId;
            END;
            ELSE
            BEGIN
                EXEC Enterprise.CreateProductSetting
                     @ProductId = @ProductId, -- int
                     @ProductSettingTypeId = @ProductSettingTypeId, -- int
                     @Value = N'cGdAIXcyM3Jn', -- nvarchar(1000)
                     @FromDate = @FromDate, -- datetime
                     @ThruDate = NULL, -- datetime
                     @ProductSettingId = @ProductSettingId OUTPUT; -- int

-- Link the Product Setting to an actual configuration
                EXEC Enterprise.LinkProductSettingToConfiguration
                     @ConfigurationId = @ConfigurationId, -- int
                     @ProductSettingId = @ProductSettingId, -- int
                     @FromDate = @FromDate, -- datetime
                     @ThruDate = NULL;   -- datetime
            END;
------------------------------------------------------------------------------------------------------------------------------------------------------
-- Get the ProductSettingTypeId for the attribute you need to select, the API should have all these cached though.
------------------------------------------------------------------------------------------------------------------------------------------------------
        SET @ProductSettingId = NULL;
        SET @ProductSettingTypeId = NULL;
        EXEC Enterprise.GetProductSettingType
             @Name = 'ProductSuperUserLoginName', -- varchar(50)
             @ProductSettingTypeId = @ProductSettingTypeId OUTPUT; -- int

-- Create the Value and assign it to the Product and ProductSettingType
        IF EXISTS
(
    SELECT 1
    FROM enterprise.ProductSetting AS PS
         INNER JOIN Enterprise.productsettingtype AS pp ON PS.productsettingtypeid = pp.productsettingtypeid
    WHERE productid = @ProductId
          AND PP.ProductSettingTypeId = @ProductSettingTypeId
)
            BEGIN
                UPDATE PS
                  SET
                      value = 'amungale'
                FROM Enterprise.ProductSetting PS
                     INNER JOIN Enterprise.productsettingtype pp ON PS.productsettingtypeid = pp.productsettingtypeid
                WHERE productid = @ProductId
                      AND PP.ProductSettingTypeId = @ProductSettingTypeId;
            END;
            ELSE
            BEGIN
                EXEC Enterprise.CreateProductSetting
                     @ProductId = @ProductId, -- int
                     @ProductSettingTypeId = @ProductSettingTypeId, -- int
                     @Value = N'amungale', -- nvarchar(1000)
                     @FromDate = @FromDate, -- datetime
                     @ThruDate = NULL, -- datetime
                     @ProductSettingId = @ProductSettingId OUTPUT; -- int

-- Link the Product Setting to an actual configuration
                EXEC Enterprise.LinkProductSettingToConfiguration
                     @ConfigurationId = @ConfigurationId, -- int
                     @ProductSettingId = @ProductSettingId, -- int
                     @FromDate = @FromDate, -- datetime
                     @ThruDate = NULL;   -- datetime
            END;
------------------------------------------------------------------------------------------------------------------------------------------------------

        EXEC Enterprise.LinkGlobalConfigurationToProduct
             @ConfigurationId = @ConfigurationId, -- int
             @ProductId = @ProductId, -- int
             @FromDate = @FromDate, -- datetime
             @ThruDate = NULL;   -- datetime

        EXEC Enterprise.ListGlobalSettingsForProduct
             @productid = @ProductId;
    END;
----BI---------

--Step 3 -- Add all settings

SET @FromDate = GETUTCDATE();
-- If this is a new Global Product Configuration then create a Product Configuration if you do not have one to use.

SELECT @ProductId = ProductId
FROM Enterprise.Product
WHERE Name = 'Business Intelligence';
IF @ProductId = 29
    BEGIN
        IF NOT EXISTS
(
    SELECT 1
    FROM Ident.SamlProductSettings
    WHERE ProductId = @ProductId
)
            BEGIN
                IF @@SERVERNAME = 'RCTUSODBSQL001'
                    BEGIN
                        INSERT INTO Ident.SamlProductSettings
(ProductId,
 LoginUri,
 SigningCertificateThumbprint,
 SubjectIdSamlAttribute
)
                        VALUES
(@ProductId,
 'https://aouat.realpage.com/ysconfig/sso/unified',
 'EF26FEC08C554976572E8A9767DDA437AC452CF6',
 'productUsername'
);
                    END;
                IF @@SERVERNAME = 'RCQUSODBSQL001'
                    BEGIN
                        INSERT INTO Ident.SamlProductSettings
(ProductId,
 LoginUri,
 SigningCertificateThumbprint,
 SubjectIdSamlAttribute
)
                        VALUES
(@ProductId,
 'https://aosat.realpage.com/ysconfig/sso/unified',
 'EF26FEC08C554976572E8A9767DDA437AC452CF6',
 'productUsername'
);
                    END;
                IF @@SERVERNAME IN('RCPGBKDBSQL005A', 'RCPGBKDBSQL005B')
                    BEGIN
                        INSERT INTO Ident.SamlProductSettings
(ProductId,
 LoginUri,
 SigningCertificateThumbprint,
 SubjectIdSamlAttribute
)
                        VALUES
(@ProductId,
 'https://ao.realpage.com/ysconfig/sso/unified',
 'EF26FEC08C554976572E8A9767DDA437AC452CF6',
 'productUsername'
);
                    END;
            END;
            ELSE
            BEGIN
                IF @@SERVERNAME = 'RCTUSODBSQL001'
                    BEGIN
                        UPDATE Ident.SamlProductSettings
                          SET
                              LoginUri = 'https://aouat.realpage.com/ysconfig/sso/unified',
                              SigningCertificateThumbprint = 'EF26FEC08C554976572E8A9767DDA437AC452CF6'
                        WHERE ProductId = @ProductId;
                    END;
                IF @@SERVERNAME = 'RCQUSODBSQL001'
                    BEGIN
                        UPDATE Ident.SamlProductSettings
                          SET
                              LoginUri = 'https://aosat.realpage.com/ysconfig/sso/unified',
                              SigningCertificateThumbprint = 'EF26FEC08C554976572E8A9767DDA437AC452CF6'
                        WHERE ProductId = @ProductId;
                    END;
                IF @@SERVERNAME IN('RCPGBKDBSQL005A', 'RCPGBKDBSQL005B')
                    BEGIN
                        UPDATE Ident.SamlProductSettings
                          SET
                              LoginUri = 'https://ao.realpage.com/ysconfig/sso/unified',
                              SigningCertificateThumbprint = 'EF26FEC08C554976572E8A9767DDA437AC452CF6'
                        WHERE ProductId = @ProductId;
                    END;
            END;
        IF EXISTS
(
    SELECT 1
    FROM Enterprise.GlobalProductConfiguration
    WHERE ProductId = @ProductId
          AND (GETDATE() BETWEEN FromDate AND ThruDate
               OR ThruDate IS NULL)
)
            BEGIN
                SELECT @ConfigurationId = ConfigurationId
                FROM Enterprise.GlobalProductConfiguration
                WHERE ProductId = @ProductId
                      AND (GETDATE() BETWEEN FromDate AND ThruDate
                           OR ThruDate IS NULL);
            END;
            ELSE
            BEGIN
                EXEC Enterprise.CreateProductConfiguration
                     @ConfigurationId = @ConfigurationId OUTPUT; -- int
            END;
--***** Start a Product setting loop for each attribute / value that needs set. *****
------------------------------------------------------------------------------------------------------------------------------------------------------
-- Get the ProductSettingTypeId for the attribute you need to select, the API should have all these cached though.

        SET @ProductSettingId = NULL;
        SET @ProductSettingTypeId = NULL;
        EXEC Enterprise.GetProductSettingType
             @Name = 'ProductUrl', -- varchar(50)
             @ProductSettingTypeId = @ProductSettingTypeId OUTPUT; -- int


-- Create the Value and assign it to the Product and ProductSettingType
        IF EXISTS
(
    SELECT 1
    FROM enterprise.ProductSetting AS PS
         INNER JOIN Enterprise.productsettingtype AS pp ON PS.productsettingtypeid = pp.productsettingtypeid
    WHERE productid = @ProductId
          AND PP.ProductSettingTypeId = @ProductSettingTypeId
)
            BEGIN
                UPDATE PS
                  SET
                      value = '/product/businessintelligence'
                FROM Enterprise.ProductSetting PS
                     INNER JOIN Enterprise.productsettingtype pp ON PS.productsettingtypeid = pp.productsettingtypeid
                WHERE productid = @ProductId
                      AND PP.ProductSettingTypeId = @ProductSettingTypeId;
            END;
            ELSE
            BEGIN
                EXEC Enterprise.CreateProductSetting
                     @ProductId = @ProductId, -- int
                     @ProductSettingTypeId = @ProductSettingTypeId, -- int
                     @Value = N'/product/businessintelligence', -- nvarchar(1000)
                     @FromDate = @FromDate, -- datetime
                     @ThruDate = NULL, -- datetime
                     @ProductSettingId = @ProductSettingId OUTPUT; -- int

-- Link the Product Setting to an actual configuration
                EXEC Enterprise.LinkProductSettingToConfiguration
                     @ConfigurationId = @ConfigurationId, -- int
                     @ProductSettingId = @ProductSettingId, -- int
                     @FromDate = @FromDate, -- datetime
                     @ThruDate = NULL;   -- datetime
            END;
------------------------------------------------------------------------------------------------------------------------------------------------------
-- Get the ProductSettingTypeId for the attribute you need to select, the API should have all these cached though.

        SET @ProductSettingId = NULL;
        SET @ProductSettingTypeId = NULL;
        EXEC Enterprise.GetProductSettingType
             @Name = 'TitleId', -- varchar(50)
             @ProductSettingTypeId = @ProductSettingTypeId OUTPUT; -- int

        IF EXISTS
(
    SELECT 1
    FROM enterprise.ProductSetting AS PS
         INNER JOIN Enterprise.productsettingtype AS pp ON PS.productsettingtypeid = pp.productsettingtypeid
    WHERE productid = @ProductId
          AND PP.ProductSettingTypeId = @ProductSettingTypeId
)
            BEGIN
                UPDATE PS
                  SET
                      value = 'Business Intelligence'
                FROM Enterprise.ProductSetting PS
                     INNER JOIN Enterprise.productsettingtype pp ON PS.productsettingtypeid = pp.productsettingtypeid
                WHERE productid = @ProductId
                      AND PP.ProductSettingTypeId = @ProductSettingTypeId;
            END;
            ELSE
            BEGIN
                EXEC Enterprise.CreateProductSetting
                     @ProductId = @ProductId, -- int
                     @ProductSettingTypeId = @ProductSettingTypeId, -- int
                     @Value = N'Business Intelligence', -- nvarchar(1000)
                     @FromDate = @FromDate, -- datetime
                     @ThruDate = NULL, -- datetime
                     @ProductSettingId = @ProductSettingId OUTPUT; -- int

-- Link the Product Setting to an actual configuration
                EXEC Enterprise.LinkProductSettingToConfiguration
                     @ConfigurationId = @ConfigurationId, -- int
                     @ProductSettingId = @ProductSettingId, -- int
                     @FromDate = @FromDate, -- datetime
                     @ThruDate = NULL;   -- datetime
            END;
------------------------------------------------------------------------------------------------------------------------------------------------------
-- Get the ProductSettingTypeId for the attribute you need to select, the API should have all these cached though.
        SET @ProductSettingId = NULL;
        SET @ProductSettingTypeId = NULL;
        EXEC Enterprise.GetProductSettingType
             @Name = 'TitleUniqueId', -- varchar(50)
             @ProductSettingTypeId = @ProductSettingTypeId OUTPUT; -- int

-- Create the Value and assign it to the Product and ProductSettingType
        IF EXISTS
(
    SELECT 1
    FROM enterprise.ProductSetting AS PS
         INNER JOIN Enterprise.productsettingtype AS pp ON PS.productsettingtypeid = pp.productsettingtypeid
    WHERE productid = @ProductId
          AND PP.ProductSettingTypeId = @ProductSettingTypeId
)
            BEGIN
                UPDATE PS
                  SET
                      value = '1A66353F-338E-444E-8775-06FDC6B4D020'
                FROM Enterprise.ProductSetting PS
                     INNER JOIN Enterprise.productsettingtype pp ON PS.productsettingtypeid = pp.productsettingtypeid
                WHERE productid = @ProductId
                      AND PP.ProductSettingTypeId = @ProductSettingTypeId;
            END;
            ELSE
            BEGIN
                EXEC Enterprise.CreateProductSetting
                     @ProductId = @ProductId, -- int
                     @ProductSettingTypeId = @ProductSettingTypeId, -- int
                     @Value = N'1A66353F-338E-444E-8775-06FDC6B4D020', -- nvarchar(1000)
                     @FromDate = @FromDate, -- datetime
                     @ThruDate = NULL, -- datetime
                     @ProductSettingId = @ProductSettingId OUTPUT; -- int

-- Link the Product Setting to an actual configuration
                EXEC Enterprise.LinkProductSettingToConfiguration
                     @ConfigurationId = @ConfigurationId, -- int
                     @ProductSettingId = @ProductSettingId, -- int
                     @FromDate = @FromDate, -- datetime
                     @ThruDate = NULL;   -- datetime
            END;
------------------------------------------------------------------------------------------------------------------------------------------------------
-- Get the ProductSettingTypeId for the attribute you need to select, the API should have all these cached though.
        SET @ProductSettingId = NULL;
        SET @ProductSettingTypeId = NULL;
        EXEC Enterprise.GetProductSettingType
             @Name = 'IsNewTab', -- varchar(50)
             @ProductSettingTypeId = @ProductSettingTypeId OUTPUT; -- int

-- Create the Value and assign it to the Product and ProductSettingType
        IF EXISTS
(
    SELECT 1
    FROM enterprise.ProductSetting AS PS
         INNER JOIN Enterprise.productsettingtype AS pp ON PS.productsettingtypeid = pp.productsettingtypeid
    WHERE productid = @ProductId
          AND PP.ProductSettingTypeId = @ProductSettingTypeId
)
            BEGIN
                UPDATE PS
                  SET
                      value = '1'
                FROM Enterprise.ProductSetting PS
                     INNER JOIN Enterprise.productsettingtype pp ON PS.productsettingtypeid = pp.productsettingtypeid
                WHERE productid = @ProductId
                      AND PP.ProductSettingTypeId = @ProductSettingTypeId;
            END;
            ELSE
            BEGIN
                EXEC Enterprise.CreateProductSetting
                     @ProductId = @ProductId, -- int
                     @ProductSettingTypeId = @ProductSettingTypeId, -- int
                     @Value = N'1', -- nvarchar(1000)
                     @FromDate = @FromDate, -- datetime
                     @ThruDate = NULL, -- datetime
                     @ProductSettingId = @ProductSettingId OUTPUT; -- int

-- Link the Product Setting to an actual configuration
                EXEC Enterprise.LinkProductSettingToConfiguration
                     @ConfigurationId = @ConfigurationId, -- int
                     @ProductSettingId = @ProductSettingId, -- int
                     @FromDate = @FromDate, -- datetime
                     @ThruDate = NULL;   -- datetime
            END;
------------------------------------------------------------------------------------------------------------------------------------------------------
-- Get the ProductSettingTypeId for the attribute you need to select, the API should have all these cached though.
        SET @ProductSettingId = NULL;
        SET @ProductSettingTypeId = NULL;
        EXEC Enterprise.GetProductSettingType
             @Name = 'MetatagUniqueId', -- varchar(50)
             @ProductSettingTypeId = @ProductSettingTypeId OUTPUT; -- int

-- Create the Value and assign it to the Product and ProductSettingType
        IF EXISTS
(
    SELECT 1
    FROM enterprise.ProductSetting AS PS
         INNER JOIN Enterprise.productsettingtype AS pp ON PS.productsettingtypeid = pp.productsettingtypeid
    WHERE productid = @ProductId
          AND PP.ProductSettingTypeId = @ProductSettingTypeId
)
            BEGIN
                UPDATE PS
                  SET
                      value = 'Business Intelligence'
                FROM Enterprise.ProductSetting PS
                     INNER JOIN Enterprise.productsettingtype pp ON PS.productsettingtypeid = pp.productsettingtypeid
                WHERE productid = @ProductId
                      AND PP.ProductSettingTypeId = @ProductSettingTypeId;
            END;
            ELSE
            BEGIN
                EXEC Enterprise.CreateProductSetting
                     @ProductId = @ProductId, -- int
                     @ProductSettingTypeId = @ProductSettingTypeId, -- int
                     @Value = N'Business Intelligence', -- nvarchar(1000)
                     @FromDate = @FromDate, -- datetime
                     @ThruDate = NULL, -- datetime
                     @ProductSettingId = @ProductSettingId OUTPUT; -- int

-- Link the Product Setting to an actual configuration
                EXEC Enterprise.LinkProductSettingToConfiguration
                     @ConfigurationId = @ConfigurationId, -- int
                     @ProductSettingId = @ProductSettingId, -- int
                     @FromDate = @FromDate, -- datetime
                     @ThruDate = NULL;   -- datetime
            END;
------------------------------------------------------------------------------------------------------------------------------------------------------
-- Get the ProductSettingTypeId for the attribute you need to select, the API should have all these cached though.
        SET @ProductSettingId = NULL;
        SET @ProductSettingTypeId = NULL;
        EXEC Enterprise.GetProductSettingType
             @Name = 'IsFavorite', -- varchar(50)
             @ProductSettingTypeId = @ProductSettingTypeId OUTPUT; -- int

-- Create the Value and assign it to the Product and ProductSettingType
        IF EXISTS
(
    SELECT 1
    FROM enterprise.ProductSetting AS PS
         INNER JOIN Enterprise.productsettingtype AS pp ON PS.productsettingtypeid = pp.productsettingtypeid
    WHERE productid = @ProductId
          AND PP.ProductSettingTypeId = @ProductSettingTypeId
)
            BEGIN
                UPDATE PS
                  SET
                      value = '1'
                FROM Enterprise.ProductSetting PS
                     INNER JOIN Enterprise.productsettingtype pp ON PS.productsettingtypeid = pp.productsettingtypeid
                WHERE productid = @ProductId
                      AND PP.ProductSettingTypeId = @ProductSettingTypeId;
            END;
            ELSE
            BEGIN
                EXEC Enterprise.CreateProductSetting
                     @ProductId = @ProductId, -- int
                     @ProductSettingTypeId = @ProductSettingTypeId, -- int
                     @Value = N'1', -- nvarchar(1000)
                     @FromDate = @FromDate, -- datetime
                     @ThruDate = NULL, -- datetime
                     @ProductSettingId = @ProductSettingId OUTPUT; -- int

-- Link the Product Setting to an actual configuration
                EXEC Enterprise.LinkProductSettingToConfiguration
                     @ConfigurationId = @ConfigurationId, -- int
                     @ProductSettingId = @ProductSettingId, -- int
                     @FromDate = @FromDate, -- datetime
                     @ThruDate = NULL;   -- datetime
            END;
------------------------------------------------------------------------------------------------------------------------------------------------------
-- Get the ProductSettingTypeId for the attribute you need to select, the API should have all these cached though.
        SET @ProductSettingId = NULL;
        SET @ProductSettingTypeId = NULL;
        EXEC Enterprise.GetProductSettingType
             @Name = 'LearnMore', -- varchar(50)
             @ProductSettingTypeId = @ProductSettingTypeId OUTPUT; -- int

-- Create the Value and assign it to the Product and ProductSettingType
        IF EXISTS
(
    SELECT 1
    FROM enterprise.ProductSetting AS PS
         INNER JOIN Enterprise.productsettingtype AS pp ON PS.productsettingtypeid = pp.productsettingtypeid
    WHERE productid = @ProductId
          AND PP.ProductSettingTypeId = @ProductSettingTypeId
)
            BEGIN
                UPDATE PS
                  SET
                      value = 'https://www.realpage.com/asset-investment-management/portfolio-asset-management/'
                FROM Enterprise.ProductSetting PS
                     INNER JOIN Enterprise.productsettingtype pp ON PS.productsettingtypeid = pp.productsettingtypeid
                WHERE productid = @ProductId
                      AND PP.ProductSettingTypeId = @ProductSettingTypeId;
            END;
            ELSE
            BEGIN
                EXEC Enterprise.CreateProductSetting
                     @ProductId = @ProductId, -- int
                     @ProductSettingTypeId = @ProductSettingTypeId, -- int
                     @Value = N'https://www.realpage.com/asset-investment-management/portfolio-asset-management/', -- nvarchar(1000)
                     @FromDate = @FromDate, -- datetime
                     @ThruDate = NULL, -- datetime
                     @ProductSettingId = @ProductSettingId OUTPUT; -- int

-- Link the Product Setting to an actual configuration
                EXEC Enterprise.LinkProductSettingToConfiguration
                     @ConfigurationId = @ConfigurationId, -- int
                     @ProductSettingId = @ProductSettingId, -- int
                     @FromDate = @FromDate, -- datetime
                     @ThruDate = NULL;   -- datetime
            END;
------------------------------------------------------------------------------------------------------------------------------------------------------
-- Get the ProductSettingTypeId for the attribute you need to select, the API should have all these cached though.
        SET @ProductSettingId = NULL;
        SET @ProductSettingTypeId = NULL;
        EXEC Enterprise.GetProductSettingType
             @Name = 'ApiEndPoint', -- varchar(50)
             @ProductSettingTypeId = @ProductSettingTypeId OUTPUT; -- int

-- Create the Value and assign it to the Product and ProductSettingType
        IF EXISTS
(
    SELECT 1
    FROM enterprise.ProductSetting AS PS
         INNER JOIN Enterprise.productsettingtype AS pp ON PS.productsettingtypeid = pp.productsettingtypeid
    WHERE productid = @ProductId
          AND PP.ProductSettingTypeId = @ProductSettingTypeId
)
            BEGIN
                UPDATE PS
                  SET
                      value = 'https://aodev.realpage.com/ysconfig/ws/'
                FROM Enterprise.ProductSetting PS
                     INNER JOIN Enterprise.productsettingtype pp ON PS.productsettingtypeid = pp.productsettingtypeid
                WHERE productid = @ProductId
                      AND PP.ProductSettingTypeId = @ProductSettingTypeId;
            END;
            ELSE
            BEGIN
                EXEC Enterprise.CreateProductSetting
                     @ProductId = @ProductId, -- int
                     @ProductSettingTypeId = @ProductSettingTypeId, -- int
                     @Value = N'https://aodev.realpage.com/ysconfig/ws/', -- nvarchar(1000)
                     @FromDate = @FromDate, -- datetime
                     @ThruDate = NULL, -- datetime
                     @ProductSettingId = @ProductSettingId OUTPUT; -- int

-- Link the Product Setting to an actual configuration
                EXEC Enterprise.LinkProductSettingToConfiguration
                     @ConfigurationId = @ConfigurationId, -- int
                     @ProductSettingId = @ProductSettingId, -- int
                     @FromDate = @FromDate, -- datetime
                     @ThruDate = NULL;   -- datetime
            END;
------------------------------------------------------------------------------------------------------------------------------------------------------
-- Get the ProductSettingTypeId for the attribute you need to select, the API should have all these cached though.
------------------------------------------------------------------------------------------------------------------------------------------------------
        SET @ProductSettingId = NULL;
        SET @ProductSettingTypeId = NULL;
        EXEC Enterprise.GetProductSettingType
             @Name = 'ApiUserName', -- varchar(50)
             @ProductSettingTypeId = @ProductSettingTypeId OUTPUT; -- int

-- Create the Value and assign it to the Product and ProductSettingType
        IF EXISTS
(
    SELECT 1
    FROM enterprise.ProductSetting AS PS
         INNER JOIN Enterprise.productsettingtype AS pp ON PS.productsettingtypeid = pp.productsettingtypeid
    WHERE productid = @ProductId
          AND PP.ProductSettingTypeId = @ProductSettingTypeId
)
            BEGIN
                UPDATE PS
                  SET
                      value = 'wsuser'
                FROM Enterprise.ProductSetting PS
                     INNER JOIN Enterprise.productsettingtype pp ON PS.productsettingtypeid = pp.productsettingtypeid
                WHERE productid = @ProductId
                      AND PP.ProductSettingTypeId = @ProductSettingTypeId;
            END;
            ELSE
            BEGIN
                EXEC Enterprise.CreateProductSetting
                     @ProductId = @ProductId, -- int
                     @ProductSettingTypeId = @ProductSettingTypeId, -- int
                     @Value = N'wsuser', -- nvarchar(1000)
                     @FromDate = @FromDate, -- datetime
                     @ThruDate = NULL, -- datetime
                     @ProductSettingId = @ProductSettingId OUTPUT; -- int

-- Link the Product Setting to an actual configuration
                EXEC Enterprise.LinkProductSettingToConfiguration
                     @ConfigurationId = @ConfigurationId, -- int
                     @ProductSettingId = @ProductSettingId, -- int
                     @FromDate = @FromDate, -- datetime
                     @ThruDate = NULL;   -- datetime
            END;
------------------------------------------------------------------------------------------------------------------------------------------------------
-- Get the ProductSettingTypeId for the attribute you need to select, the API should have all these cached though.
------------------------------------------------------------------------------------------------------------------------------------------------------
        SET @ProductSettingId = NULL;
        SET @ProductSettingTypeId = NULL;
        EXEC Enterprise.GetProductSettingType
             @Name = 'ApiPassword', -- varchar(50)
             @ProductSettingTypeId = @ProductSettingTypeId OUTPUT; -- int

-- Create the Value and assign it to the Product and ProductSettingType
        IF EXISTS
(
    SELECT 1
    FROM enterprise.ProductSetting AS PS
         INNER JOIN Enterprise.productsettingtype AS pp ON PS.productsettingtypeid = pp.productsettingtypeid
    WHERE productid = @ProductId
          AND PP.ProductSettingTypeId = @ProductSettingTypeId
)
            BEGIN
                UPDATE PS
                  SET
                      value = 'cGdAIXcyM3Jn'
                FROM Enterprise.ProductSetting PS
                     INNER JOIN Enterprise.productsettingtype pp ON PS.productsettingtypeid = pp.productsettingtypeid
                WHERE productid = @ProductId
                      AND PP.ProductSettingTypeId = @ProductSettingTypeId;
            END;
            ELSE
            BEGIN
                EXEC Enterprise.CreateProductSetting
                     @ProductId = @ProductId, -- int
                     @ProductSettingTypeId = @ProductSettingTypeId, -- int
                     @Value = N'cGdAIXcyM3Jn', -- nvarchar(1000)
                     @FromDate = @FromDate, -- datetime
                     @ThruDate = NULL, -- datetime
                     @ProductSettingId = @ProductSettingId OUTPUT; -- int

-- Link the Product Setting to an actual configuration
                EXEC Enterprise.LinkProductSettingToConfiguration
                     @ConfigurationId = @ConfigurationId, -- int
                     @ProductSettingId = @ProductSettingId, -- int
                     @FromDate = @FromDate, -- datetime
                     @ThruDate = NULL;   -- datetime
            END;
------------------------------------------------------------------------------------------------------------------------------------------------------
-- Get the ProductSettingTypeId for the attribute you need to select, the API should have all these cached though.
------------------------------------------------------------------------------------------------------------------------------------------------------
        SET @ProductSettingId = NULL;
        SET @ProductSettingTypeId = NULL;
        EXEC Enterprise.GetProductSettingType
             @Name = 'ProductSuperUserLoginName', -- varchar(50)
             @ProductSettingTypeId = @ProductSettingTypeId OUTPUT; -- int

-- Create the Value and assign it to the Product and ProductSettingType
        IF EXISTS
(
    SELECT 1
    FROM enterprise.ProductSetting AS PS
         INNER JOIN Enterprise.productsettingtype AS pp ON PS.productsettingtypeid = pp.productsettingtypeid
    WHERE productid = @ProductId
          AND PP.ProductSettingTypeId = @ProductSettingTypeId
)
            BEGIN
                UPDATE PS
                  SET
                      value = 'amungale'
                FROM Enterprise.ProductSetting PS
                     INNER JOIN Enterprise.productsettingtype pp ON PS.productsettingtypeid = pp.productsettingtypeid
                WHERE productid = @ProductId
                      AND PP.ProductSettingTypeId = @ProductSettingTypeId;
            END;
            ELSE
            BEGIN
                EXEC Enterprise.CreateProductSetting
                     @ProductId = @ProductId, -- int
                     @ProductSettingTypeId = @ProductSettingTypeId, -- int
                     @Value = N'amungale', -- nvarchar(1000)
                     @FromDate = @FromDate, -- datetime
                     @ThruDate = NULL, -- datetime
                     @ProductSettingId = @ProductSettingId OUTPUT; -- int

-- Link the Product Setting to an actual configuration
                EXEC Enterprise.LinkProductSettingToConfiguration
                     @ConfigurationId = @ConfigurationId, -- int
                     @ProductSettingId = @ProductSettingId, -- int
                     @FromDate = @FromDate, -- datetime
                     @ThruDate = NULL;   -- datetime
            END;
------------------------------------------------------------------------------------------------------------------------------------------------------
-- Get the ProductSettingTypeId for the attribute you need to select, the API should have all these cached though.
------------------------------------------------------------------------------------------------------------------------------------------------------
        SET @ProductSettingId = NULL;
        SET @ProductSettingTypeId = NULL;
        EXEC Enterprise.GetProductSettingType
             @Name = 'ProductStatus', -- varchar(50)
             @ProductSettingTypeId = @ProductSettingTypeId OUTPUT; -- int

-- Create the Value and assign it to the Product and ProductSettingType
        IF EXISTS
(
    SELECT 1
    FROM enterprise.ProductSetting AS PS
         INNER JOIN Enterprise.productsettingtype AS pp ON PS.productsettingtypeid = pp.productsettingtypeid
    WHERE productid = @ProductId
          AND PP.ProductSettingTypeId = @ProductSettingTypeId
)
            BEGIN
                UPDATE PS
                  SET
                      value = '8'
                FROM Enterprise.ProductSetting PS
                     INNER JOIN Enterprise.productsettingtype pp ON PS.productsettingtypeid = pp.productsettingtypeid
                WHERE productid = @ProductId
                      AND PP.ProductSettingTypeId = @ProductSettingTypeId;
            END;
            ELSE
            BEGIN
                EXEC Enterprise.CreateProductSetting
                     @ProductId = @ProductId, -- int
                     @ProductSettingTypeId = @ProductSettingTypeId, -- int
                     @Value = N'8', -- nvarchar(1000)
                     @FromDate = @FromDate, -- datetime
                     @ThruDate = NULL, -- datetime
                     @ProductSettingId = @ProductSettingId OUTPUT; -- int

-- Link the Product Setting to an actual configuration
                EXEC Enterprise.LinkProductSettingToConfiguration
                     @ConfigurationId = @ConfigurationId, -- int
                     @ProductSettingId = @ProductSettingId, -- int
                     @FromDate = @FromDate, -- datetime
                     @ThruDate = NULL;   -- datetime
            END;
------------------------------------------------------------------------------------------------------------------------------------------------------

        EXEC Enterprise.LinkGlobalConfigurationToProduct
             @ConfigurationId = @ConfigurationId, -- int
             @ProductId = @ProductId, -- int
             @FromDate = @FromDate, -- datetime
             @ThruDate = NULL;   -- datetime

        EXEC Enterprise.ListGlobalSettingsForProduct
             @productid = @ProductId;
    END;
----Performance Analytics

--Step 3 -- Add all settings

SET @FromDate = GETUTCDATE();
-- If this is a new Global Product Configuration then create a Product Configuration if you do not have one to use.

SELECT @ProductId = ProductId
FROM Enterprise.Product
WHERE Name = 'Performance Analytics';
IF @ProductId = 30
    BEGIN
        IF NOT EXISTS
(
    SELECT 1
    FROM Ident.SamlProductSettings
    WHERE ProductId = @ProductId
)
            BEGIN
                IF @@SERVERNAME = 'RCTUSODBSQL001'
                    BEGIN
                        INSERT INTO Ident.SamlProductSettings
							(ProductId,
							 LoginUri,
							 SigningCertificateThumbprint,
							 SubjectIdSamlAttribute
							)
                        VALUES
						(@ProductId,
						 'https://aouat.realpage.com/ysconfig/sso/unified',
						 'EF26FEC08C554976572E8A9767DDA437AC452CF6',
						 'productUsername'
						);
                    END;
                IF @@SERVERNAME = 'RCQUSODBSQL001'
                    BEGIN
                        INSERT INTO Ident.SamlProductSettings
(ProductId,
 LoginUri,
 SigningCertificateThumbprint,
 SubjectIdSamlAttribute
)
                        VALUES
(@ProductId,
 'https://aosat.realpage.com/ysconfig/sso/unified',
 'EF26FEC08C554976572E8A9767DDA437AC452CF6',
 'productUsername'
);
                    END;
                IF @@SERVERNAME IN('RCPGBKDBSQL005A', 'RCPGBKDBSQL005B')
                    BEGIN
                        INSERT INTO Ident.SamlProductSettings
(ProductId,
 LoginUri,
 SigningCertificateThumbprint,
 SubjectIdSamlAttribute
)
                        VALUES
(@ProductId,
 'https://ao.realpage.com/ysconfig/sso/unified',
 'EF26FEC08C554976572E8A9767DDA437AC452CF6',
 'productUsername'
);
                    END;
            END;
            ELSE
            BEGIN
                IF @@SERVERNAME = 'RCTUSODBSQL001'
                    BEGIN
                        UPDATE Ident.SamlProductSettings
                          SET
                              LoginUri = 'https://aouat.realpage.com/ysconfig/sso/unified',
                              SigningCertificateThumbprint = 'EF26FEC08C554976572E8A9767DDA437AC452CF6'
                        WHERE ProductId = @ProductId;
                    END;
                IF @@SERVERNAME = 'RCQUSODBSQL001'
                    BEGIN
                        UPDATE Ident.SamlProductSettings
                          SET
                              LoginUri = 'https://aosat.realpage.com/ysconfig/sso/unified',
                              SigningCertificateThumbprint = 'EF26FEC08C554976572E8A9767DDA437AC452CF6'
                        WHERE ProductId = @ProductId;
                    END;
                IF @@SERVERNAME IN('RCPGBKDBSQL005A', 'RCPGBKDBSQL005B')
                    BEGIN
                        UPDATE Ident.SamlProductSettings
                          SET
                              LoginUri = 'https://ao.realpage.com/ysconfig/sso/unified',
                              SigningCertificateThumbprint = 'EF26FEC08C554976572E8A9767DDA437AC452CF6'
                        WHERE ProductId = @ProductId;
                    END;
            END;
        IF EXISTS
(
    SELECT 1
    FROM Enterprise.GlobalProductConfiguration
    WHERE ProductId = @ProductId
          AND (GETDATE() BETWEEN FromDate AND ThruDate
               OR ThruDate IS NULL)
)
            BEGIN
                SELECT @ConfigurationId = ConfigurationId
                FROM Enterprise.GlobalProductConfiguration
                WHERE ProductId = @ProductId
                      AND (GETDATE() BETWEEN FromDate AND ThruDate
                           OR ThruDate IS NULL);
            END;
            ELSE
            BEGIN
                EXEC Enterprise.CreateProductConfiguration
                     @ConfigurationId = @ConfigurationId OUTPUT; -- int
            END;
--***** Start a Product setting loop for each attribute / value that needs set. *****
------------------------------------------------------------------------------------------------------------------------------------------------------
-- Get the ProductSettingTypeId for the attribute you need to select, the API should have all these cached though.

        SET @ProductSettingId = NULL;
        SET @ProductSettingTypeId = NULL;
        EXEC Enterprise.GetProductSettingType
             @Name = 'ProductUrl', -- varchar(50)
             @ProductSettingTypeId = @ProductSettingTypeId OUTPUT; -- int


-- Create the Value and assign it to the Product and ProductSettingType
        IF EXISTS
(
    SELECT 1
    FROM enterprise.ProductSetting AS PS
         INNER JOIN Enterprise.productsettingtype AS pp ON PS.productsettingtypeid = pp.productsettingtypeid
    WHERE productid = @ProductId
          AND PP.ProductSettingTypeId = @ProductSettingTypeId
)
            BEGIN
                UPDATE PS
                  SET
                      value = '/product/performanceanalytics'
                FROM Enterprise.ProductSetting PS
                     INNER JOIN Enterprise.productsettingtype pp ON PS.productsettingtypeid = pp.productsettingtypeid
                WHERE productid = @ProductId
                      AND PP.ProductSettingTypeId = @ProductSettingTypeId;
            END;
            ELSE
            BEGIN
                EXEC Enterprise.CreateProductSetting
                     @ProductId = @ProductId, -- int
                     @ProductSettingTypeId = @ProductSettingTypeId, -- int
                     @Value = N'/product/businessintelligence', -- nvarchar(1000)
                     @FromDate = @FromDate, -- datetime
                     @ThruDate = NULL, -- datetime
                     @ProductSettingId = @ProductSettingId OUTPUT; -- int

-- Link the Product Setting to an actual configuration
                EXEC Enterprise.LinkProductSettingToConfiguration
                     @ConfigurationId = @ConfigurationId, -- int
                     @ProductSettingId = @ProductSettingId, -- int
                     @FromDate = @FromDate, -- datetime
                     @ThruDate = NULL;   -- datetime
            END;
------------------------------------------------------------------------------------------------------------------------------------------------------
-- Get the ProductSettingTypeId for the attribute you need to select, the API should have all these cached though.

        SET @ProductSettingId = NULL;
        SET @ProductSettingTypeId = NULL;
        EXEC Enterprise.GetProductSettingType
             @Name = 'TitleId', -- varchar(50)
             @ProductSettingTypeId = @ProductSettingTypeId OUTPUT; -- int

        IF EXISTS
(
    SELECT 1
    FROM enterprise.ProductSetting AS PS
         INNER JOIN Enterprise.productsettingtype AS pp ON PS.productsettingtypeid = pp.productsettingtypeid
    WHERE productid = @ProductId
          AND PP.ProductSettingTypeId = @ProductSettingTypeId
)
            BEGIN
                UPDATE PS
                  SET
                      value = 'Performance Analytics'
                FROM Enterprise.ProductSetting PS
                     INNER JOIN Enterprise.productsettingtype pp ON PS.productsettingtypeid = pp.productsettingtypeid
                WHERE productid = @ProductId
                      AND PP.ProductSettingTypeId = @ProductSettingTypeId;
            END;
            ELSE
            BEGIN
                EXEC Enterprise.CreateProductSetting
                     @ProductId = @ProductId, -- int
                     @ProductSettingTypeId = @ProductSettingTypeId, -- int
                     @Value = N'Performance Analytics', -- nvarchar(1000)
                     @FromDate = @FromDate, -- datetime
                     @ThruDate = NULL, -- datetime
                     @ProductSettingId = @ProductSettingId OUTPUT; -- int

-- Link the Product Setting to an actual configuration
                EXEC Enterprise.LinkProductSettingToConfiguration
                     @ConfigurationId = @ConfigurationId, -- int
                     @ProductSettingId = @ProductSettingId, -- int
                     @FromDate = @FromDate, -- datetime
                     @ThruDate = NULL;   -- datetime
            END;
------------------------------------------------------------------------------------------------------------------------------------------------------
-- Get the ProductSettingTypeId for the attribute you need to select, the API should have all these cached though.
        SET @ProductSettingId = NULL;
        SET @ProductSettingTypeId = NULL;
        EXEC Enterprise.GetProductSettingType
             @Name = 'TitleUniqueId', -- varchar(50)
             @ProductSettingTypeId = @ProductSettingTypeId OUTPUT; -- int

-- Create the Value and assign it to the Product and ProductSettingType
        IF EXISTS
(
    SELECT 1
    FROM enterprise.ProductSetting AS PS
         INNER JOIN Enterprise.productsettingtype AS pp ON PS.productsettingtypeid = pp.productsettingtypeid
    WHERE productid = @ProductId
          AND PP.ProductSettingTypeId = @ProductSettingTypeId
)
            BEGIN
                UPDATE PS
                  SET
                      value = '2A66353F-338E-444E-8775-06FDC6B4D020'
                FROM Enterprise.ProductSetting PS
                     INNER JOIN Enterprise.productsettingtype pp ON PS.productsettingtypeid = pp.productsettingtypeid
                WHERE productid = @ProductId
                      AND PP.ProductSettingTypeId = @ProductSettingTypeId;
            END;
            ELSE
            BEGIN
                EXEC Enterprise.CreateProductSetting
                     @ProductId = @ProductId, -- int
                     @ProductSettingTypeId = @ProductSettingTypeId, -- int
                     @Value = N'2A66353F-338E-444E-8775-06FDC6B4D020', -- nvarchar(1000)
                     @FromDate = @FromDate, -- datetime
                     @ThruDate = NULL, -- datetime
                     @ProductSettingId = @ProductSettingId OUTPUT; -- int

-- Link the Product Setting to an actual configuration
                EXEC Enterprise.LinkProductSettingToConfiguration
                     @ConfigurationId = @ConfigurationId, -- int
                     @ProductSettingId = @ProductSettingId, -- int
                     @FromDate = @FromDate, -- datetime
                     @ThruDate = NULL;   -- datetime
            END;
------------------------------------------------------------------------------------------------------------------------------------------------------
-- Get the ProductSettingTypeId for the attribute you need to select, the API should have all these cached though.
        SET @ProductSettingId = NULL;
        SET @ProductSettingTypeId = NULL;
        EXEC Enterprise.GetProductSettingType
             @Name = 'IsNewTab', -- varchar(50)
             @ProductSettingTypeId = @ProductSettingTypeId OUTPUT; -- int

-- Create the Value and assign it to the Product and ProductSettingType
        IF EXISTS
(
    SELECT 1
    FROM enterprise.ProductSetting AS PS
         INNER JOIN Enterprise.productsettingtype AS pp ON PS.productsettingtypeid = pp.productsettingtypeid
    WHERE productid = @ProductId
          AND PP.ProductSettingTypeId = @ProductSettingTypeId
)
            BEGIN
                UPDATE PS
                  SET
                      value = '1'
                FROM Enterprise.ProductSetting PS
                     INNER JOIN Enterprise.productsettingtype pp ON PS.productsettingtypeid = pp.productsettingtypeid
                WHERE productid = @ProductId
                      AND PP.ProductSettingTypeId = @ProductSettingTypeId;
            END;
            ELSE
            BEGIN
                EXEC Enterprise.CreateProductSetting
                     @ProductId = @ProductId, -- int
                     @ProductSettingTypeId = @ProductSettingTypeId, -- int
                     @Value = N'1', -- nvarchar(1000)
                     @FromDate = @FromDate, -- datetime
                     @ThruDate = NULL, -- datetime
                     @ProductSettingId = @ProductSettingId OUTPUT; -- int

-- Link the Product Setting to an actual configuration
                EXEC Enterprise.LinkProductSettingToConfiguration
                     @ConfigurationId = @ConfigurationId, -- int
                     @ProductSettingId = @ProductSettingId, -- int
                     @FromDate = @FromDate, -- datetime
                     @ThruDate = NULL;   -- datetime
            END;
------------------------------------------------------------------------------------------------------------------------------------------------------
-- Get the ProductSettingTypeId for the attribute you need to select, the API should have all these cached though.
        SET @ProductSettingId = NULL;
        SET @ProductSettingTypeId = NULL;
        EXEC Enterprise.GetProductSettingType
             @Name = 'MetatagUniqueId', -- varchar(50)
             @ProductSettingTypeId = @ProductSettingTypeId OUTPUT; -- int

-- Create the Value and assign it to the Product and ProductSettingType
        IF EXISTS
(
    SELECT 1
    FROM enterprise.ProductSetting AS PS
         INNER JOIN Enterprise.productsettingtype AS pp ON PS.productsettingtypeid = pp.productsettingtypeid
    WHERE productid = @ProductId
          AND PP.ProductSettingTypeId = @ProductSettingTypeId
)
            BEGIN
                UPDATE PS
                  SET
                      value = 'Performance Analytics'
                FROM Enterprise.ProductSetting PS
                     INNER JOIN Enterprise.productsettingtype pp ON PS.productsettingtypeid = pp.productsettingtypeid
                WHERE productid = @ProductId
                      AND PP.ProductSettingTypeId = @ProductSettingTypeId;
            END;
            ELSE
            BEGIN
                EXEC Enterprise.CreateProductSetting
                     @ProductId = @ProductId, -- int
                     @ProductSettingTypeId = @ProductSettingTypeId, -- int
                     @Value = N'Performance Analytics', -- nvarchar(1000)
                     @FromDate = @FromDate, -- datetime
                     @ThruDate = NULL, -- datetime
                     @ProductSettingId = @ProductSettingId OUTPUT; -- int

-- Link the Product Setting to an actual configuration
                EXEC Enterprise.LinkProductSettingToConfiguration
                     @ConfigurationId = @ConfigurationId, -- int
                     @ProductSettingId = @ProductSettingId, -- int
                     @FromDate = @FromDate, -- datetime
                     @ThruDate = NULL;   -- datetime
            END;
------------------------------------------------------------------------------------------------------------------------------------------------------
-- Get the ProductSettingTypeId for the attribute you need to select, the API should have all these cached though.
        SET @ProductSettingId = NULL;
        SET @ProductSettingTypeId = NULL;
        EXEC Enterprise.GetProductSettingType
             @Name = 'IsFavorite', -- varchar(50)
             @ProductSettingTypeId = @ProductSettingTypeId OUTPUT; -- int

-- Create the Value and assign it to the Product and ProductSettingType
        IF EXISTS
(
    SELECT 1
    FROM enterprise.ProductSetting AS PS
         INNER JOIN Enterprise.productsettingtype AS pp ON PS.productsettingtypeid = pp.productsettingtypeid
    WHERE productid = @ProductId
          AND PP.ProductSettingTypeId = @ProductSettingTypeId
)
            BEGIN
                UPDATE PS
                  SET
                      value = '1'
                FROM Enterprise.ProductSetting PS
                     INNER JOIN Enterprise.productsettingtype pp ON PS.productsettingtypeid = pp.productsettingtypeid
                WHERE productid = @ProductId
                      AND PP.ProductSettingTypeId = @ProductSettingTypeId;
            END;
            ELSE
            BEGIN
                EXEC Enterprise.CreateProductSetting
                     @ProductId = @ProductId, -- int
                     @ProductSettingTypeId = @ProductSettingTypeId, -- int
                     @Value = N'1', -- nvarchar(1000)
                     @FromDate = @FromDate, -- datetime
                     @ThruDate = NULL, -- datetime
                     @ProductSettingId = @ProductSettingId OUTPUT; -- int

-- Link the Product Setting to an actual configuration
                EXEC Enterprise.LinkProductSettingToConfiguration
                     @ConfigurationId = @ConfigurationId, -- int
                     @ProductSettingId = @ProductSettingId, -- int
                     @FromDate = @FromDate, -- datetime
                     @ThruDate = NULL;   -- datetime
            END;
------------------------------------------------------------------------------------------------------------------------------------------------------
-- Get the ProductSettingTypeId for the attribute you need to select, the API should have all these cached though.
        SET @ProductSettingId = NULL;
        SET @ProductSettingTypeId = NULL;
        EXEC Enterprise.GetProductSettingType
             @Name = 'LearnMore', -- varchar(50)
             @ProductSettingTypeId = @ProductSettingTypeId OUTPUT; -- int

-- Create the Value and assign it to the Product and ProductSettingType
        IF EXISTS
(
    SELECT 1
    FROM enterprise.ProductSetting AS PS
         INNER JOIN Enterprise.productsettingtype AS pp ON PS.productsettingtypeid = pp.productsettingtypeid
    WHERE productid = @ProductId
          AND PP.ProductSettingTypeId = @ProductSettingTypeId
)
            BEGIN
                UPDATE PS
                  SET
                      value = 'https://www.realpage.com/asset-investment-management/portfolio-asset-management/'
                FROM Enterprise.ProductSetting PS
                     INNER JOIN Enterprise.productsettingtype pp ON PS.productsettingtypeid = pp.productsettingtypeid
                WHERE productid = @ProductId
                      AND PP.ProductSettingTypeId = @ProductSettingTypeId;
            END;
            ELSE
            BEGIN
                EXEC Enterprise.CreateProductSetting
                     @ProductId = @ProductId, -- int
                     @ProductSettingTypeId = @ProductSettingTypeId, -- int
                     @Value = N'https://www.realpage.com/asset-investment-management/portfolio-asset-management/', -- nvarchar(1000)
                     @FromDate = @FromDate, -- datetime
                     @ThruDate = NULL, -- datetime
                     @ProductSettingId = @ProductSettingId OUTPUT; -- int

-- Link the Product Setting to an actual configuration
                EXEC Enterprise.LinkProductSettingToConfiguration
                     @ConfigurationId = @ConfigurationId, -- int
                     @ProductSettingId = @ProductSettingId, -- int
                     @FromDate = @FromDate, -- datetime
                     @ThruDate = NULL;   -- datetime
            END;
------------------------------------------------------------------------------------------------------------------------------------------------------
-- Get the ProductSettingTypeId for the attribute you need to select, the API should have all these cached though.
        SET @ProductSettingId = NULL;
        SET @ProductSettingTypeId = NULL;
        EXEC Enterprise.GetProductSettingType
             @Name = 'ApiEndPoint', -- varchar(50)
             @ProductSettingTypeId = @ProductSettingTypeId OUTPUT; -- int

-- Create the Value and assign it to the Product and ProductSettingType
        IF EXISTS
(
    SELECT 1
    FROM enterprise.ProductSetting AS PS
         INNER JOIN Enterprise.productsettingtype AS pp ON PS.productsettingtypeid = pp.productsettingtypeid
    WHERE productid = @ProductId
          AND PP.ProductSettingTypeId = @ProductSettingTypeId
)
            BEGIN
                UPDATE PS
                  SET
                      value = 'https://aodev.realpage.com/ysconfig/ws/'
                FROM Enterprise.ProductSetting PS
                     INNER JOIN Enterprise.productsettingtype pp ON PS.productsettingtypeid = pp.productsettingtypeid
                WHERE productid = @ProductId
                      AND PP.ProductSettingTypeId = @ProductSettingTypeId;
            END;
            ELSE
            BEGIN
                EXEC Enterprise.CreateProductSetting
                     @ProductId = @ProductId, -- int
                     @ProductSettingTypeId = @ProductSettingTypeId, -- int
                     @Value = N'https://aodev.realpage.com/ysconfig/ws/', -- nvarchar(1000)
                     @FromDate = @FromDate, -- datetime
                     @ThruDate = NULL, -- datetime
                     @ProductSettingId = @ProductSettingId OUTPUT; -- int

-- Link the Product Setting to an actual configuration
                EXEC Enterprise.LinkProductSettingToConfiguration
                     @ConfigurationId = @ConfigurationId, -- int
                     @ProductSettingId = @ProductSettingId, -- int
                     @FromDate = @FromDate, -- datetime
                     @ThruDate = NULL;   -- datetime
            END;
------------------------------------------------------------------------------------------------------------------------------------------------------
-- Get the ProductSettingTypeId for the attribute you need to select, the API should have all these cached though.
------------------------------------------------------------------------------------------------------------------------------------------------------
        SET @ProductSettingId = NULL;
        SET @ProductSettingTypeId = NULL;
        EXEC Enterprise.GetProductSettingType
             @Name = 'ApiUserName', -- varchar(50)
             @ProductSettingTypeId = @ProductSettingTypeId OUTPUT; -- int

-- Create the Value and assign it to the Product and ProductSettingType
        IF EXISTS
(
    SELECT 1
    FROM enterprise.ProductSetting AS PS
         INNER JOIN Enterprise.productsettingtype AS pp ON PS.productsettingtypeid = pp.productsettingtypeid
    WHERE productid = @ProductId
          AND PP.ProductSettingTypeId = @ProductSettingTypeId
)
            BEGIN
                UPDATE PS
                  SET
                      value = 'wsuser'
                FROM Enterprise.ProductSetting PS
                     INNER JOIN Enterprise.productsettingtype pp ON PS.productsettingtypeid = pp.productsettingtypeid
                WHERE productid = @ProductId
                      AND PP.ProductSettingTypeId = @ProductSettingTypeId;
            END;
            ELSE
            BEGIN
                EXEC Enterprise.CreateProductSetting
                     @ProductId = @ProductId, -- int
                     @ProductSettingTypeId = @ProductSettingTypeId, -- int
                     @Value = N'wsuser', -- nvarchar(1000)
                     @FromDate = @FromDate, -- datetime
                     @ThruDate = NULL, -- datetime
                     @ProductSettingId = @ProductSettingId OUTPUT; -- int

-- Link the Product Setting to an actual configuration
                EXEC Enterprise.LinkProductSettingToConfiguration
                     @ConfigurationId = @ConfigurationId, -- int
                     @ProductSettingId = @ProductSettingId, -- int
                     @FromDate = @FromDate, -- datetime
                     @ThruDate = NULL;   -- datetime
            END;
------------------------------------------------------------------------------------------------------------------------------------------------------
-- Get the ProductSettingTypeId for the attribute you need to select, the API should have all these cached though.
------------------------------------------------------------------------------------------------------------------------------------------------------
        SET @ProductSettingId = NULL;
        SET @ProductSettingTypeId = NULL;
        EXEC Enterprise.GetProductSettingType
             @Name = 'ApiPassword', -- varchar(50)
             @ProductSettingTypeId = @ProductSettingTypeId OUTPUT; -- int

-- Create the Value and assign it to the Product and ProductSettingType
        IF EXISTS
(
    SELECT 1
    FROM enterprise.ProductSetting AS PS
         INNER JOIN Enterprise.productsettingtype AS pp ON PS.productsettingtypeid = pp.productsettingtypeid
    WHERE productid = @ProductId
          AND PP.ProductSettingTypeId = @ProductSettingTypeId
)
            BEGIN
                UPDATE PS
                  SET
                      value = 'cGdAIXcyM3Jn'
                FROM Enterprise.ProductSetting PS
                     INNER JOIN Enterprise.productsettingtype pp ON PS.productsettingtypeid = pp.productsettingtypeid
                WHERE productid = @ProductId
                      AND PP.ProductSettingTypeId = @ProductSettingTypeId;
            END;
            ELSE
            BEGIN
                EXEC Enterprise.CreateProductSetting
                     @ProductId = @ProductId, -- int
                     @ProductSettingTypeId = @ProductSettingTypeId, -- int
                     @Value = N'cGdAIXcyM3Jn', -- nvarchar(1000)
                     @FromDate = @FromDate, -- datetime
                     @ThruDate = NULL, -- datetime
                     @ProductSettingId = @ProductSettingId OUTPUT; -- int

-- Link the Product Setting to an actual configuration
                EXEC Enterprise.LinkProductSettingToConfiguration
                     @ConfigurationId = @ConfigurationId, -- int
                     @ProductSettingId = @ProductSettingId, -- int
                     @FromDate = @FromDate, -- datetime
                     @ThruDate = NULL;   -- datetime
            END;
------------------------------------------------------------------------------------------------------------------------------------------------------
-- Get the ProductSettingTypeId for the attribute you need to select, the API should have all these cached though.
------------------------------------------------------------------------------------------------------------------------------------------------------
        SET @ProductSettingId = NULL;
        SET @ProductSettingTypeId = NULL;
        EXEC Enterprise.GetProductSettingType
             @Name = 'ProductSuperUserLoginName', -- varchar(50)
             @ProductSettingTypeId = @ProductSettingTypeId OUTPUT; -- int

-- Create the Value and assign it to the Product and ProductSettingType
        IF EXISTS
(
    SELECT 1
    FROM enterprise.ProductSetting AS PS
         INNER JOIN Enterprise.productsettingtype AS pp ON PS.productsettingtypeid = pp.productsettingtypeid
    WHERE productid = @ProductId
          AND PP.ProductSettingTypeId = @ProductSettingTypeId
)
            BEGIN
                UPDATE PS
                  SET
                      value = 'amungale'
                FROM Enterprise.ProductSetting PS
                     INNER JOIN Enterprise.productsettingtype pp ON PS.productsettingtypeid = pp.productsettingtypeid
                WHERE productid = @ProductId
                      AND PP.ProductSettingTypeId = @ProductSettingTypeId;
            END;
            ELSE
            BEGIN
                EXEC Enterprise.CreateProductSetting
                     @ProductId = @ProductId, -- int
                     @ProductSettingTypeId = @ProductSettingTypeId, -- int
                     @Value = N'amungale', -- nvarchar(1000)
                     @FromDate = @FromDate, -- datetime
                     @ThruDate = NULL, -- datetime
                     @ProductSettingId = @ProductSettingId OUTPUT; -- int

-- Link the Product Setting to an actual configuration
                EXEC Enterprise.LinkProductSettingToConfiguration
                     @ConfigurationId = @ConfigurationId, -- int
                     @ProductSettingId = @ProductSettingId, -- int
                     @FromDate = @FromDate, -- datetime
                     @ThruDate = NULL;   -- datetime
            END;
------------------------------------------------------------------------------------------------------------------------------------------------------
-- Get the ProductSettingTypeId for the attribute you need to select, the API should have all these cached though.
------------------------------------------------------------------------------------------------------------------------------------------------------
        SET @ProductSettingId = NULL;
        SET @ProductSettingTypeId = NULL;
        EXEC Enterprise.GetProductSettingType
             @Name = 'ProductStatus', -- varchar(50)
             @ProductSettingTypeId = @ProductSettingTypeId OUTPUT; -- int

-- Create the Value and assign it to the Product and ProductSettingType
        IF EXISTS
(
    SELECT 1
    FROM enterprise.ProductSetting AS PS
         INNER JOIN Enterprise.productsettingtype AS pp ON PS.productsettingtypeid = pp.productsettingtypeid
    WHERE productid = @ProductId
          AND PP.ProductSettingTypeId = @ProductSettingTypeId
)
            BEGIN
                UPDATE PS
                  SET
                      value = '8'
                FROM Enterprise.ProductSetting PS
                     INNER JOIN Enterprise.productsettingtype pp ON PS.productsettingtypeid = pp.productsettingtypeid
                WHERE productid = @ProductId
                      AND PP.ProductSettingTypeId = @ProductSettingTypeId;
            END;
            ELSE
            BEGIN
                EXEC Enterprise.CreateProductSetting
                     @ProductId = @ProductId, -- int
                     @ProductSettingTypeId = @ProductSettingTypeId, -- int
                     @Value = N'8', -- nvarchar(1000)
                     @FromDate = @FromDate, -- datetime
                     @ThruDate = NULL, -- datetime
                     @ProductSettingId = @ProductSettingId OUTPUT; -- int

-- Link the Product Setting to an actual configuration
                EXEC Enterprise.LinkProductSettingToConfiguration
                     @ConfigurationId = @ConfigurationId, -- int
                     @ProductSettingId = @ProductSettingId, -- int
                     @FromDate = @FromDate, -- datetime
                     @ThruDate = NULL;   -- datetime
            END;
------------------------------------------------------------------------------------------------------------------------------------------------------

        EXEC Enterprise.LinkGlobalConfigurationToProduct
             @ConfigurationId = @ConfigurationId, -- int
             @ProductId = @ProductId, -- int
             @FromDate = @FromDate, -- datetime
             @ThruDate = NULL;   -- datetime

        EXEC Enterprise.ListGlobalSettingsForProduct
             @productid = @ProductId;
    END;
----Investment Analytics
--Step 3 -- Add all settings

SET @FromDate = GETUTCDATE();
-- If this is a new Global Product Configuration then create a Product Configuration if you do not have one to use.

SELECT @ProductId = ProductId
FROM Enterprise.Product
WHERE Name = 'Investment Analytics';
IF @ProductId = 31
    BEGIN
        IF NOT EXISTS
(
    SELECT 1
    FROM Ident.SamlProductSettings
    WHERE ProductId = @ProductId
)
            BEGIN
                IF @@SERVERNAME = 'RCTUSODBSQL001'
                    BEGIN
                        INSERT INTO Ident.SamlProductSettings
(ProductId,
 LoginUri,
 SigningCertificateThumbprint,
 SubjectIdSamlAttribute
)
                        VALUES
(@ProductId,
 'https://aouat.realpage.com/ysconfig/sso/unified',
 'EF26FEC08C554976572E8A9767DDA437AC452CF6',
 'productUsername'
);
                    END;
                IF @@SERVERNAME = 'RCQUSODBSQL001'
                    BEGIN
                        INSERT INTO Ident.SamlProductSettings
(ProductId,
 LoginUri,
 SigningCertificateThumbprint,
 SubjectIdSamlAttribute
)
                        VALUES
(@ProductId,
 'https://aosat.realpage.com/ysconfig/sso/unified',
 'EF26FEC08C554976572E8A9767DDA437AC452CF6',
 'productUsername'
);
                    END;
                IF @@SERVERNAME IN('RCPGBKDBSQL005A', 'RCPGBKDBSQL005B')
                    BEGIN
                        INSERT INTO Ident.SamlProductSettings
(ProductId,
 LoginUri,
 SigningCertificateThumbprint,
 SubjectIdSamlAttribute
)
                        VALUES
(@ProductId,
 'https://ao.realpage.com/ysconfig/sso/unified',
 'EF26FEC08C554976572E8A9767DDA437AC452CF6',
 'productUsername'
);
                    END;
            END;
            ELSE
            BEGIN
                IF @@SERVERNAME = 'RCTUSODBSQL001'
                    BEGIN
                        UPDATE Ident.SamlProductSettings
                          SET
                              LoginUri = 'https://aouat.realpage.com/ysconfig/sso/unified',
                              SigningCertificateThumbprint = 'EF26FEC08C554976572E8A9767DDA437AC452CF6'
                        WHERE ProductId = @ProductId;
                    END;
                IF @@SERVERNAME = 'RCQUSODBSQL001'
                    BEGIN
                        UPDATE Ident.SamlProductSettings
                          SET
                              LoginUri = 'https://aosat.realpage.com/ysconfig/sso/unified',
                              SigningCertificateThumbprint = 'EF26FEC08C554976572E8A9767DDA437AC452CF6'
                        WHERE ProductId = @ProductId;
                    END;
                IF @@SERVERNAME IN('RCPGBKDBSQL005A', 'RCPGBKDBSQL005B')
                    BEGIN
                        UPDATE Ident.SamlProductSettings
                          SET
                              LoginUri = 'https://ao.realpage.com/ysconfig/sso/unified',
                              SigningCertificateThumbprint = 'EF26FEC08C554976572E8A9767DDA437AC452CF6'
                        WHERE ProductId = @ProductId;
                    END;
            END;
        IF EXISTS
(
    SELECT 1
    FROM Enterprise.GlobalProductConfiguration
    WHERE ProductId = @ProductId
          AND (GETDATE() BETWEEN FromDate AND ThruDate
               OR ThruDate IS NULL)
)
            BEGIN
                SELECT @ConfigurationId = ConfigurationId
                FROM Enterprise.GlobalProductConfiguration
                WHERE ProductId = @ProductId
                      AND (GETDATE() BETWEEN FromDate AND ThruDate
                           OR ThruDate IS NULL);
            END;
            ELSE
            BEGIN
                EXEC Enterprise.CreateProductConfiguration
                     @ConfigurationId = @ConfigurationId OUTPUT; -- int
            END;
--***** Start a Product setting loop for each attribute / value that needs set. *****
------------------------------------------------------------------------------------------------------------------------------------------------------
-- Get the ProductSettingTypeId for the attribute you need to select, the API should have all these cached though.

        SET @ProductSettingId = NULL;
        SET @ProductSettingTypeId = NULL;
        EXEC Enterprise.GetProductSettingType
             @Name = 'ProductUrl', -- varchar(50)
             @ProductSettingTypeId = @ProductSettingTypeId OUTPUT; -- int


-- Create the Value and assign it to the Product and ProductSettingType
        IF EXISTS
(
    SELECT 1
    FROM enterprise.ProductSetting AS PS
         INNER JOIN Enterprise.productsettingtype AS pp ON PS.productsettingtypeid = pp.productsettingtypeid
    WHERE productid = @ProductId
          AND PP.ProductSettingTypeId = @ProductSettingTypeId
)
            BEGIN
                UPDATE PS
                  SET
                      value = '/product/investmentanalytics'
                FROM Enterprise.ProductSetting PS
                     INNER JOIN Enterprise.productsettingtype pp ON PS.productsettingtypeid = pp.productsettingtypeid
                WHERE productid = @ProductId
                      AND PP.ProductSettingTypeId = @ProductSettingTypeId;
            END;
            ELSE
            BEGIN
                EXEC Enterprise.CreateProductSetting
                     @ProductId = @ProductId, -- int
                     @ProductSettingTypeId = @ProductSettingTypeId, -- int
                     @Value = N'/product/investmentanalytics', -- nvarchar(1000)
                     @FromDate = @FromDate, -- datetime
                     @ThruDate = NULL, -- datetime
                     @ProductSettingId = @ProductSettingId OUTPUT; -- int

-- Link the Product Setting to an actual configuration
                EXEC Enterprise.LinkProductSettingToConfiguration
                     @ConfigurationId = @ConfigurationId, -- int
                     @ProductSettingId = @ProductSettingId, -- int
                     @FromDate = @FromDate, -- datetime
                     @ThruDate = NULL;   -- datetime
            END;
------------------------------------------------------------------------------------------------------------------------------------------------------
-- Get the ProductSettingTypeId for the attribute you need to select, the API should have all these cached though.

        SET @ProductSettingId = NULL;
        SET @ProductSettingTypeId = NULL;
        EXEC Enterprise.GetProductSettingType
             @Name = 'TitleId', -- varchar(50)
             @ProductSettingTypeId = @ProductSettingTypeId OUTPUT; -- int

        IF EXISTS
(
    SELECT 1
    FROM enterprise.ProductSetting AS PS
         INNER JOIN Enterprise.productsettingtype AS pp ON PS.productsettingtypeid = pp.productsettingtypeid
    WHERE productid = @ProductId
          AND PP.ProductSettingTypeId = @ProductSettingTypeId
)
            BEGIN
                UPDATE PS
                  SET
                      value = 'Investment Analytics'
                FROM Enterprise.ProductSetting PS
                     INNER JOIN Enterprise.productsettingtype pp ON PS.productsettingtypeid = pp.productsettingtypeid
                WHERE productid = @ProductId
                      AND PP.ProductSettingTypeId = @ProductSettingTypeId;
            END;
            ELSE
            BEGIN
                EXEC Enterprise.CreateProductSetting
                     @ProductId = @ProductId, -- int
                     @ProductSettingTypeId = @ProductSettingTypeId, -- int
                     @Value = N'Investment Analytics', -- nvarchar(1000)
                     @FromDate = @FromDate, -- datetime
                     @ThruDate = NULL, -- datetime
                     @ProductSettingId = @ProductSettingId OUTPUT; -- int

-- Link the Product Setting to an actual configuration
                EXEC Enterprise.LinkProductSettingToConfiguration
                     @ConfigurationId = @ConfigurationId, -- int
                     @ProductSettingId = @ProductSettingId, -- int
                     @FromDate = @FromDate, -- datetime
                     @ThruDate = NULL;   -- datetime
            END;
------------------------------------------------------------------------------------------------------------------------------------------------------
-- Get the ProductSettingTypeId for the attribute you need to select, the API should have all these cached though.
        SET @ProductSettingId = NULL;
        SET @ProductSettingTypeId = NULL;
        EXEC Enterprise.GetProductSettingType
             @Name = 'TitleUniqueId', -- varchar(50)
             @ProductSettingTypeId = @ProductSettingTypeId OUTPUT; -- int

-- Create the Value and assign it to the Product and ProductSettingType
        IF EXISTS
(
    SELECT 1
    FROM enterprise.ProductSetting AS PS
         INNER JOIN Enterprise.productsettingtype AS pp ON PS.productsettingtypeid = pp.productsettingtypeid
    WHERE productid = @ProductId
          AND PP.ProductSettingTypeId = @ProductSettingTypeId
)
            BEGIN
                UPDATE PS
                  SET
                      value = '3A66353F-338E-444E-8775-06FDC6B4D020'
                FROM Enterprise.ProductSetting PS
                     INNER JOIN Enterprise.productsettingtype pp ON PS.productsettingtypeid = pp.productsettingtypeid
                WHERE productid = @ProductId
                      AND PP.ProductSettingTypeId = @ProductSettingTypeId;
            END;
            ELSE
            BEGIN
                EXEC Enterprise.CreateProductSetting
                     @ProductId = @ProductId, -- int
                     @ProductSettingTypeId = @ProductSettingTypeId, -- int
                     @Value = N'3A66353F-338E-444E-8775-06FDC6B4D020', -- nvarchar(1000)
                     @FromDate = @FromDate, -- datetime
                     @ThruDate = NULL, -- datetime
                     @ProductSettingId = @ProductSettingId OUTPUT; -- int

-- Link the Product Setting to an actual configuration
                EXEC Enterprise.LinkProductSettingToConfiguration
                     @ConfigurationId = @ConfigurationId, -- int
                     @ProductSettingId = @ProductSettingId, -- int
                     @FromDate = @FromDate, -- datetime
                     @ThruDate = NULL;   -- datetime
            END;
------------------------------------------------------------------------------------------------------------------------------------------------------
-- Get the ProductSettingTypeId for the attribute you need to select, the API should have all these cached though.
        SET @ProductSettingId = NULL;
        SET @ProductSettingTypeId = NULL;
        EXEC Enterprise.GetProductSettingType
             @Name = 'IsNewTab', -- varchar(50)
             @ProductSettingTypeId = @ProductSettingTypeId OUTPUT; -- int

-- Create the Value and assign it to the Product and ProductSettingType
        IF EXISTS
(
    SELECT 1
    FROM enterprise.ProductSetting AS PS
         INNER JOIN Enterprise.productsettingtype AS pp ON PS.productsettingtypeid = pp.productsettingtypeid
    WHERE productid = @ProductId
          AND PP.ProductSettingTypeId = @ProductSettingTypeId
)
            BEGIN
                UPDATE PS
                  SET
                      value = '1'
                FROM Enterprise.ProductSetting PS
                     INNER JOIN Enterprise.productsettingtype pp ON PS.productsettingtypeid = pp.productsettingtypeid
                WHERE productid = @ProductId
                      AND PP.ProductSettingTypeId = @ProductSettingTypeId;
            END;
            ELSE
            BEGIN
                EXEC Enterprise.CreateProductSetting
                     @ProductId = @ProductId, -- int
                     @ProductSettingTypeId = @ProductSettingTypeId, -- int
                     @Value = N'1', -- nvarchar(1000)
                     @FromDate = @FromDate, -- datetime
                     @ThruDate = NULL, -- datetime
                     @ProductSettingId = @ProductSettingId OUTPUT; -- int

-- Link the Product Setting to an actual configuration
                EXEC Enterprise.LinkProductSettingToConfiguration
                     @ConfigurationId = @ConfigurationId, -- int
                     @ProductSettingId = @ProductSettingId, -- int
                     @FromDate = @FromDate, -- datetime
                     @ThruDate = NULL;   -- datetime
            END;
------------------------------------------------------------------------------------------------------------------------------------------------------
-- Get the ProductSettingTypeId for the attribute you need to select, the API should have all these cached though.
        SET @ProductSettingId = NULL;
        SET @ProductSettingTypeId = NULL;
        EXEC Enterprise.GetProductSettingType
             @Name = 'MetatagUniqueId', -- varchar(50)
             @ProductSettingTypeId = @ProductSettingTypeId OUTPUT; -- int

-- Create the Value and assign it to the Product and ProductSettingType
        IF EXISTS
(
    SELECT 1
    FROM enterprise.ProductSetting AS PS
         INNER JOIN Enterprise.productsettingtype AS pp ON PS.productsettingtypeid = pp.productsettingtypeid
    WHERE productid = @ProductId
          AND PP.ProductSettingTypeId = @ProductSettingTypeId
)
            BEGIN
                UPDATE PS
                  SET
                      value = 'Investment Analytics'
                FROM Enterprise.ProductSetting PS
                     INNER JOIN Enterprise.productsettingtype pp ON PS.productsettingtypeid = pp.productsettingtypeid
                WHERE productid = @ProductId
                      AND PP.ProductSettingTypeId = @ProductSettingTypeId;
            END;
            ELSE
            BEGIN
                EXEC Enterprise.CreateProductSetting
                     @ProductId = @ProductId, -- int
                     @ProductSettingTypeId = @ProductSettingTypeId, -- int
                     @Value = N'Investment Analytics', -- nvarchar(1000)
                     @FromDate = @FromDate, -- datetime
                     @ThruDate = NULL, -- datetime
                     @ProductSettingId = @ProductSettingId OUTPUT; -- int

-- Link the Product Setting to an actual configuration
                EXEC Enterprise.LinkProductSettingToConfiguration
                     @ConfigurationId = @ConfigurationId, -- int
                     @ProductSettingId = @ProductSettingId, -- int
                     @FromDate = @FromDate, -- datetime
                     @ThruDate = NULL;   -- datetime
            END;
------------------------------------------------------------------------------------------------------------------------------------------------------
-- Get the ProductSettingTypeId for the attribute you need to select, the API should have all these cached though.
        SET @ProductSettingId = NULL;
        SET @ProductSettingTypeId = NULL;
        EXEC Enterprise.GetProductSettingType
             @Name = 'IsFavorite', -- varchar(50)
             @ProductSettingTypeId = @ProductSettingTypeId OUTPUT; -- int

-- Create the Value and assign it to the Product and ProductSettingType
        IF EXISTS
(
    SELECT 1
    FROM enterprise.ProductSetting AS PS
         INNER JOIN Enterprise.productsettingtype AS pp ON PS.productsettingtypeid = pp.productsettingtypeid
    WHERE productid = @ProductId
          AND PP.ProductSettingTypeId = @ProductSettingTypeId
)
            BEGIN
                UPDATE PS
                  SET
                      value = '1'
                FROM Enterprise.ProductSetting PS
                     INNER JOIN Enterprise.productsettingtype pp ON PS.productsettingtypeid = pp.productsettingtypeid
                WHERE productid = @ProductId
                      AND PP.ProductSettingTypeId = @ProductSettingTypeId;
            END;
            ELSE
            BEGIN
                EXEC Enterprise.CreateProductSetting
                     @ProductId = @ProductId, -- int
                     @ProductSettingTypeId = @ProductSettingTypeId, -- int
                     @Value = N'1', -- nvarchar(1000)
                     @FromDate = @FromDate, -- datetime
                     @ThruDate = NULL, -- datetime
                     @ProductSettingId = @ProductSettingId OUTPUT; -- int

-- Link the Product Setting to an actual configuration
                EXEC Enterprise.LinkProductSettingToConfiguration
                     @ConfigurationId = @ConfigurationId, -- int
                     @ProductSettingId = @ProductSettingId, -- int
                     @FromDate = @FromDate, -- datetime
                     @ThruDate = NULL;   -- datetime
            END;
------------------------------------------------------------------------------------------------------------------------------------------------------
-- Get the ProductSettingTypeId for the attribute you need to select, the API should have all these cached though.
        SET @ProductSettingId = NULL;
        SET @ProductSettingTypeId = NULL;
        EXEC Enterprise.GetProductSettingType
             @Name = 'LearnMore', -- varchar(50)
             @ProductSettingTypeId = @ProductSettingTypeId OUTPUT; -- int

-- Create the Value and assign it to the Product and ProductSettingType
        IF EXISTS
(
    SELECT 1
    FROM enterprise.ProductSetting AS PS
         INNER JOIN Enterprise.productsettingtype AS pp ON PS.productsettingtypeid = pp.productsettingtypeid
    WHERE productid = @ProductId
          AND PP.ProductSettingTypeId = @ProductSettingTypeId
)
            BEGIN
                UPDATE PS
                  SET
                      value = 'https://www.realpage.com/asset-investment-management/portfolio-asset-management/'
                FROM Enterprise.ProductSetting PS
                     INNER JOIN Enterprise.productsettingtype pp ON PS.productsettingtypeid = pp.productsettingtypeid
                WHERE productid = @ProductId
                      AND PP.ProductSettingTypeId = @ProductSettingTypeId;
            END;
            ELSE
            BEGIN
                EXEC Enterprise.CreateProductSetting
                     @ProductId = @ProductId, -- int
                     @ProductSettingTypeId = @ProductSettingTypeId, -- int
                     @Value = N'https://www.realpage.com/asset-investment-management/portfolio-asset-management/', -- nvarchar(1000)
                     @FromDate = @FromDate, -- datetime
                     @ThruDate = NULL, -- datetime
                     @ProductSettingId = @ProductSettingId OUTPUT; -- int

-- Link the Product Setting to an actual configuration
                EXEC Enterprise.LinkProductSettingToConfiguration
                     @ConfigurationId = @ConfigurationId, -- int
                     @ProductSettingId = @ProductSettingId, -- int
                     @FromDate = @FromDate, -- datetime
                     @ThruDate = NULL;   -- datetime
            END;
------------------------------------------------------------------------------------------------------------------------------------------------------
-- Get the ProductSettingTypeId for the attribute you need to select, the API should have all these cached though.
        SET @ProductSettingId = NULL;
        SET @ProductSettingTypeId = NULL;
        EXEC Enterprise.GetProductSettingType
             @Name = 'ApiEndPoint', -- varchar(50)
             @ProductSettingTypeId = @ProductSettingTypeId OUTPUT; -- int

-- Create the Value and assign it to the Product and ProductSettingType
        IF EXISTS
(
    SELECT 1
    FROM enterprise.ProductSetting AS PS
         INNER JOIN Enterprise.productsettingtype AS pp ON PS.productsettingtypeid = pp.productsettingtypeid
    WHERE productid = @ProductId
          AND PP.ProductSettingTypeId = @ProductSettingTypeId
)
            BEGIN
                UPDATE PS
                  SET
                      value = 'https://aodev.realpage.com/ysconfig/ws/'
                FROM Enterprise.ProductSetting PS
                     INNER JOIN Enterprise.productsettingtype pp ON PS.productsettingtypeid = pp.productsettingtypeid
                WHERE productid = @ProductId
                      AND PP.ProductSettingTypeId = @ProductSettingTypeId;
            END;
            ELSE
            BEGIN
                EXEC Enterprise.CreateProductSetting
                     @ProductId = @ProductId, -- int
                     @ProductSettingTypeId = @ProductSettingTypeId, -- int
                     @Value = N'https://aodev.realpage.com/ysconfig/ws/', -- nvarchar(1000)
                     @FromDate = @FromDate, -- datetime
                     @ThruDate = NULL, -- datetime
                     @ProductSettingId = @ProductSettingId OUTPUT; -- int

-- Link the Product Setting to an actual configuration
                EXEC Enterprise.LinkProductSettingToConfiguration
                     @ConfigurationId = @ConfigurationId, -- int
                     @ProductSettingId = @ProductSettingId, -- int
                     @FromDate = @FromDate, -- datetime
                     @ThruDate = NULL;   -- datetime
            END;
------------------------------------------------------------------------------------------------------------------------------------------------------
-- Get the ProductSettingTypeId for the attribute you need to select, the API should have all these cached though.
------------------------------------------------------------------------------------------------------------------------------------------------------
        SET @ProductSettingId = NULL;
        SET @ProductSettingTypeId = NULL;
        EXEC Enterprise.GetProductSettingType
             @Name = 'ApiUserName', -- varchar(50)
             @ProductSettingTypeId = @ProductSettingTypeId OUTPUT; -- int

-- Create the Value and assign it to the Product and ProductSettingType
        IF EXISTS
(
    SELECT 1
    FROM enterprise.ProductSetting AS PS
         INNER JOIN Enterprise.productsettingtype AS pp ON PS.productsettingtypeid = pp.productsettingtypeid
    WHERE productid = @ProductId
          AND PP.ProductSettingTypeId = @ProductSettingTypeId
)
            BEGIN
                UPDATE PS
                  SET
                      value = 'wsuser'
                FROM Enterprise.ProductSetting PS
                     INNER JOIN Enterprise.productsettingtype pp ON PS.productsettingtypeid = pp.productsettingtypeid
                WHERE productid = @ProductId
                      AND PP.ProductSettingTypeId = @ProductSettingTypeId;
            END;
            ELSE
            BEGIN
                EXEC Enterprise.CreateProductSetting
                     @ProductId = @ProductId, -- int
                     @ProductSettingTypeId = @ProductSettingTypeId, -- int
                     @Value = N'wsuser', -- nvarchar(1000)
                     @FromDate = @FromDate, -- datetime
                     @ThruDate = NULL, -- datetime
                     @ProductSettingId = @ProductSettingId OUTPUT; -- int

-- Link the Product Setting to an actual configuration
                EXEC Enterprise.LinkProductSettingToConfiguration
                     @ConfigurationId = @ConfigurationId, -- int
                     @ProductSettingId = @ProductSettingId, -- int
                     @FromDate = @FromDate, -- datetime
                     @ThruDate = NULL;   -- datetime
            END;
------------------------------------------------------------------------------------------------------------------------------------------------------
-- Get the ProductSettingTypeId for the attribute you need to select, the API should have all these cached though.
------------------------------------------------------------------------------------------------------------------------------------------------------
        SET @ProductSettingId = NULL;
        SET @ProductSettingTypeId = NULL;
        EXEC Enterprise.GetProductSettingType
             @Name = 'ApiPassword', -- varchar(50)
             @ProductSettingTypeId = @ProductSettingTypeId OUTPUT; -- int

-- Create the Value and assign it to the Product and ProductSettingType
        IF EXISTS
(
    SELECT 1
    FROM enterprise.ProductSetting AS PS
         INNER JOIN Enterprise.productsettingtype AS pp ON PS.productsettingtypeid = pp.productsettingtypeid
    WHERE productid = @ProductId
          AND PP.ProductSettingTypeId = @ProductSettingTypeId
)
            BEGIN
                UPDATE PS
                  SET
                      value = 'cGdAIXcyM3Jn'
                FROM Enterprise.ProductSetting PS
                     INNER JOIN Enterprise.productsettingtype pp ON PS.productsettingtypeid = pp.productsettingtypeid
                WHERE productid = @ProductId
                      AND PP.ProductSettingTypeId = @ProductSettingTypeId;
            END;
            ELSE
            BEGIN
                EXEC Enterprise.CreateProductSetting
                     @ProductId = @ProductId, -- int
                     @ProductSettingTypeId = @ProductSettingTypeId, -- int
                     @Value = N'cGdAIXcyM3Jn', -- nvarchar(1000)
                     @FromDate = @FromDate, -- datetime
                     @ThruDate = NULL, -- datetime
                     @ProductSettingId = @ProductSettingId OUTPUT; -- int

-- Link the Product Setting to an actual configuration
                EXEC Enterprise.LinkProductSettingToConfiguration
                     @ConfigurationId = @ConfigurationId, -- int
                     @ProductSettingId = @ProductSettingId, -- int
                     @FromDate = @FromDate, -- datetime
                     @ThruDate = NULL;   -- datetime
            END;
------------------------------------------------------------------------------------------------------------------------------------------------------
-- Get the ProductSettingTypeId for the attribute you need to select, the API should have all these cached though.
------------------------------------------------------------------------------------------------------------------------------------------------------
        SET @ProductSettingId = NULL;
        SET @ProductSettingTypeId = NULL;
        EXEC Enterprise.GetProductSettingType
             @Name = 'ProductSuperUserLoginName', -- varchar(50)
             @ProductSettingTypeId = @ProductSettingTypeId OUTPUT; -- int

-- Create the Value and assign it to the Product and ProductSettingType
        IF EXISTS
(
    SELECT 1
    FROM enterprise.ProductSetting AS PS
         INNER JOIN Enterprise.productsettingtype AS pp ON PS.productsettingtypeid = pp.productsettingtypeid
    WHERE productid = @ProductId
          AND PP.ProductSettingTypeId = @ProductSettingTypeId
)
            BEGIN
                UPDATE PS
                  SET
                      value = 'amungale'
                FROM Enterprise.ProductSetting PS
                     INNER JOIN Enterprise.productsettingtype pp ON PS.productsettingtypeid = pp.productsettingtypeid
                WHERE productid = @ProductId
                      AND PP.ProductSettingTypeId = @ProductSettingTypeId;
            END;
            ELSE
            BEGIN
                EXEC Enterprise.CreateProductSetting
                     @ProductId = @ProductId, -- int
                     @ProductSettingTypeId = @ProductSettingTypeId, -- int
                     @Value = N'amungale', -- nvarchar(1000)
                     @FromDate = @FromDate, -- datetime
                     @ThruDate = NULL, -- datetime
                     @ProductSettingId = @ProductSettingId OUTPUT; -- int

-- Link the Product Setting to an actual configuration
                EXEC Enterprise.LinkProductSettingToConfiguration
                     @ConfigurationId = @ConfigurationId, -- int
                     @ProductSettingId = @ProductSettingId, -- int
                     @FromDate = @FromDate, -- datetime
                     @ThruDate = NULL;   -- datetime
            END;
------------------------------------------------------------------------------------------------------------------------------------------------------
-- Get the ProductSettingTypeId for the attribute you need to select, the API should have all these cached though.
------------------------------------------------------------------------------------------------------------------------------------------------------
        SET @ProductSettingId = NULL;
        SET @ProductSettingTypeId = NULL;
        EXEC Enterprise.GetProductSettingType
             @Name = 'ProductStatus', -- varchar(50)
             @ProductSettingTypeId = @ProductSettingTypeId OUTPUT; -- int

-- Create the Value and assign it to the Product and ProductSettingType
        IF EXISTS
(
    SELECT 1
    FROM enterprise.ProductSetting AS PS
         INNER JOIN Enterprise.productsettingtype AS pp ON PS.productsettingtypeid = pp.productsettingtypeid
    WHERE productid = @ProductId
          AND PP.ProductSettingTypeId = @ProductSettingTypeId
)
            BEGIN
                UPDATE PS
                  SET
                      value = '8'
                FROM Enterprise.ProductSetting PS
                     INNER JOIN Enterprise.productsettingtype pp ON PS.productsettingtypeid = pp.productsettingtypeid
                WHERE productid = @ProductId
                      AND PP.ProductSettingTypeId = @ProductSettingTypeId;
            END;
            ELSE
            BEGIN
                EXEC Enterprise.CreateProductSetting
                     @ProductId = @ProductId, -- int
                     @ProductSettingTypeId = @ProductSettingTypeId, -- int
                     @Value = N'8', -- nvarchar(1000)
                     @FromDate = @FromDate, -- datetime
                     @ThruDate = NULL, -- datetime
                     @ProductSettingId = @ProductSettingId OUTPUT; -- int

-- Link the Product Setting to an actual configuration
                EXEC Enterprise.LinkProductSettingToConfiguration
                     @ConfigurationId = @ConfigurationId, -- int
                     @ProductSettingId = @ProductSettingId, -- int
                     @FromDate = @FromDate, -- datetime
                     @ThruDate = NULL;   -- datetime
            END;
------------------------------------------------------------------------------------------------------------------------------------------------------

        EXEC Enterprise.LinkGlobalConfigurationToProduct
             @ConfigurationId = @ConfigurationId, -- int
             @ProductId = @ProductId, -- int
             @FromDate = @FromDate, -- datetime
             @ThruDate = NULL;   -- datetime

        EXEC Enterprise.ListGlobalSettingsForProduct
             @productid = @ProductId;
    END;
--------Revenue Management

--Step 3 -- Add all settings

SET @FromDate = GETUTCDATE();
-- If this is a new Global Product Configuration then create a Product Configuration if you do not have one to use.

SELECT @ProductId = ProductId
FROM Enterprise.Product
WHERE Name = 'Revenue Management';
IF @ProductId = 32
    BEGIN
        IF NOT EXISTS
(
    SELECT 1
    FROM Ident.SamlProductSettings
    WHERE ProductId = @ProductId
)
            BEGIN
                IF @@SERVERNAME = 'RCTUSODBSQL001'
                    BEGIN
                        INSERT INTO Ident.SamlProductSettings
(ProductId,
 LoginUri,
 SigningCertificateThumbprint,
 SubjectIdSamlAttribute
)
                        VALUES
(@ProductId,
 'https://aouat.realpage.com/ysconfig/sso/unified',
 'EF26FEC08C554976572E8A9767DDA437AC452CF6',
 'productUsername'
);
                    END;
                IF @@SERVERNAME = 'RCQUSODBSQL001'
                    BEGIN
                        INSERT INTO Ident.SamlProductSettings
(ProductId,
 LoginUri,
 SigningCertificateThumbprint,
 SubjectIdSamlAttribute
)
                        VALUES
(@ProductId,
 'https://aosat.realpage.com/ysconfig/sso/unified',
 'EF26FEC08C554976572E8A9767DDA437AC452CF6',
 'productUsername'
);
                    END;
                IF @@SERVERNAME IN('RCPGBKDBSQL005A', 'RCPGBKDBSQL005B')
                    BEGIN
                        INSERT INTO Ident.SamlProductSettings
(ProductId,
 LoginUri,
 SigningCertificateThumbprint,
 SubjectIdSamlAttribute
)
                        VALUES
(@ProductId,
 'https://ao.realpage.com/ysconfig/sso/unified',
 'EF26FEC08C554976572E8A9767DDA437AC452CF6',
 'productUsername'
);
                    END;
            END;
            ELSE
            BEGIN
                IF @@SERVERNAME = 'RCTUSODBSQL001'
                    BEGIN
                        UPDATE Ident.SamlProductSettings
                          SET
                              LoginUri = 'https://aouat.realpage.com/ysconfig/sso/unified',
                              SigningCertificateThumbprint = 'EF26FEC08C554976572E8A9767DDA437AC452CF6'
                        WHERE ProductId = @ProductId;
                    END;
                IF @@SERVERNAME = 'RCQUSODBSQL001'
                    BEGIN
                        UPDATE Ident.SamlProductSettings
                          SET
                              LoginUri = 'https://aosat.realpage.com/ysconfig/sso/unified',
                              SigningCertificateThumbprint = 'EF26FEC08C554976572E8A9767DDA437AC452CF6'
                        WHERE ProductId = @ProductId;
                    END;
                IF @@SERVERNAME IN('RCPGBKDBSQL005A', 'RCPGBKDBSQL005B')
                    BEGIN
                        UPDATE Ident.SamlProductSettings
                          SET
                              LoginUri = 'https://ao.realpage.com/ysconfig/sso/unified',
                              SigningCertificateThumbprint = 'EF26FEC08C554976572E8A9767DDA437AC452CF6'
                        WHERE ProductId = @ProductId;
                    END;
            END;
        IF EXISTS
(
    SELECT 1
    FROM Enterprise.GlobalProductConfiguration
    WHERE ProductId = @ProductId
          AND (GETDATE() BETWEEN FromDate AND ThruDate
               OR ThruDate IS NULL)
)
            BEGIN
                SELECT @ConfigurationId = ConfigurationId
                FROM Enterprise.GlobalProductConfiguration
                WHERE ProductId = @ProductId
                      AND (GETDATE() BETWEEN FromDate AND ThruDate
                           OR ThruDate IS NULL);
            END;
            ELSE
            BEGIN
                EXEC Enterprise.CreateProductConfiguration
                     @ConfigurationId = @ConfigurationId OUTPUT; -- int
            END;
--***** Start a Product setting loop for each attribute / value that needs set. *****
------------------------------------------------------------------------------------------------------------------------------------------------------
-- Get the ProductSettingTypeId for the attribute you need to select, the API should have all these cached though.

        SET @ProductSettingId = NULL;
        SET @ProductSettingTypeId = NULL;
        EXEC Enterprise.GetProductSettingType
             @Name = 'ProductUrl', -- varchar(50)
             @ProductSettingTypeId = @ProductSettingTypeId OUTPUT; -- int


-- Create the Value and assign it to the Product and ProductSettingType
        IF EXISTS
(
    SELECT 1
    FROM enterprise.ProductSetting AS PS
         INNER JOIN Enterprise.productsettingtype AS pp ON PS.productsettingtypeid = pp.productsettingtypeid
    WHERE productid = @ProductId
          AND PP.ProductSettingTypeId = @ProductSettingTypeId
)
            BEGIN
                UPDATE PS
                  SET
                      value = '/product/revenuemanagement'
                FROM Enterprise.ProductSetting PS
                     INNER JOIN Enterprise.productsettingtype pp ON PS.productsettingtypeid = pp.productsettingtypeid
                WHERE productid = @ProductId
                      AND PP.ProductSettingTypeId = @ProductSettingTypeId;
            END;
            ELSE
            BEGIN
                EXEC Enterprise.CreateProductSetting
                     @ProductId = @ProductId, -- int
                     @ProductSettingTypeId = @ProductSettingTypeId, -- int
                     @Value = N'/product/revenuemanagement', -- nvarchar(1000)
                     @FromDate = @FromDate, -- datetime
                     @ThruDate = NULL, -- datetime
                     @ProductSettingId = @ProductSettingId OUTPUT; -- int

-- Link the Product Setting to an actual configuration
                EXEC Enterprise.LinkProductSettingToConfiguration
                     @ConfigurationId = @ConfigurationId, -- int
                     @ProductSettingId = @ProductSettingId, -- int
                     @FromDate = @FromDate, -- datetime
                     @ThruDate = NULL;   -- datetime
            END;
------------------------------------------------------------------------------------------------------------------------------------------------------
-- Get the ProductSettingTypeId for the attribute you need to select, the API should have all these cached though.

        SET @ProductSettingId = NULL;
        SET @ProductSettingTypeId = NULL;
        EXEC Enterprise.GetProductSettingType
             @Name = 'TitleId', -- varchar(50)
             @ProductSettingTypeId = @ProductSettingTypeId OUTPUT; -- int

        IF EXISTS
(
    SELECT 1
    FROM enterprise.ProductSetting AS PS
         INNER JOIN Enterprise.productsettingtype AS pp ON PS.productsettingtypeid = pp.productsettingtypeid
    WHERE productid = @ProductId
          AND PP.ProductSettingTypeId = @ProductSettingTypeId
)
            BEGIN
                UPDATE PS
                  SET
                      value = 'Revenue Management'
                FROM Enterprise.ProductSetting PS
                     INNER JOIN Enterprise.productsettingtype pp ON PS.productsettingtypeid = pp.productsettingtypeid
                WHERE productid = @ProductId
                      AND PP.ProductSettingTypeId = @ProductSettingTypeId;
            END;
            ELSE
            BEGIN
                EXEC Enterprise.CreateProductSetting
                     @ProductId = @ProductId, -- int
                     @ProductSettingTypeId = @ProductSettingTypeId, -- int
                     @Value = N'Revenue Management', -- nvarchar(1000)
                     @FromDate = @FromDate, -- datetime
                     @ThruDate = NULL, -- datetime
                     @ProductSettingId = @ProductSettingId OUTPUT; -- int

-- Link the Product Setting to an actual configuration
                EXEC Enterprise.LinkProductSettingToConfiguration
                     @ConfigurationId = @ConfigurationId, -- int
                     @ProductSettingId = @ProductSettingId, -- int
                     @FromDate = @FromDate, -- datetime
                     @ThruDate = NULL;   -- datetime
            END;
------------------------------------------------------------------------------------------------------------------------------------------------------
-- Get the ProductSettingTypeId for the attribute you need to select, the API should have all these cached though.
        SET @ProductSettingId = NULL;
        SET @ProductSettingTypeId = NULL;
        EXEC Enterprise.GetProductSettingType
             @Name = 'TitleUniqueId', -- varchar(50)
             @ProductSettingTypeId = @ProductSettingTypeId OUTPUT; -- int

-- Create the Value and assign it to the Product and ProductSettingType
        IF EXISTS
(
    SELECT 1
    FROM enterprise.ProductSetting AS PS
         INNER JOIN Enterprise.productsettingtype AS pp ON PS.productsettingtypeid = pp.productsettingtypeid
    WHERE productid = @ProductId
          AND PP.ProductSettingTypeId = @ProductSettingTypeId
)
            BEGIN
                UPDATE PS
                  SET
                      value = '4A66353F-338E-444E-8775-06FDC6B4D020'
                FROM Enterprise.ProductSetting PS
                     INNER JOIN Enterprise.productsettingtype pp ON PS.productsettingtypeid = pp.productsettingtypeid
                WHERE productid = @ProductId
                      AND PP.ProductSettingTypeId = @ProductSettingTypeId;
            END;
            ELSE
            BEGIN
                EXEC Enterprise.CreateProductSetting
                     @ProductId = @ProductId, -- int
                     @ProductSettingTypeId = @ProductSettingTypeId, -- int
                     @Value = N'4A66353F-338E-444E-8775-06FDC6B4D020', -- nvarchar(1000)
                     @FromDate = @FromDate, -- datetime
                     @ThruDate = NULL, -- datetime
                     @ProductSettingId = @ProductSettingId OUTPUT; -- int

-- Link the Product Setting to an actual configuration
                EXEC Enterprise.LinkProductSettingToConfiguration
                     @ConfigurationId = @ConfigurationId, -- int
                     @ProductSettingId = @ProductSettingId, -- int
                     @FromDate = @FromDate, -- datetime
                     @ThruDate = NULL;   -- datetime
            END;
------------------------------------------------------------------------------------------------------------------------------------------------------
-- Get the ProductSettingTypeId for the attribute you need to select, the API should have all these cached though.
        SET @ProductSettingId = NULL;
        SET @ProductSettingTypeId = NULL;
        EXEC Enterprise.GetProductSettingType
             @Name = 'IsNewTab', -- varchar(50)
             @ProductSettingTypeId = @ProductSettingTypeId OUTPUT; -- int

-- Create the Value and assign it to the Product and ProductSettingType
        IF EXISTS
(
    SELECT 1
    FROM enterprise.ProductSetting AS PS
         INNER JOIN Enterprise.productsettingtype AS pp ON PS.productsettingtypeid = pp.productsettingtypeid
    WHERE productid = @ProductId
          AND PP.ProductSettingTypeId = @ProductSettingTypeId
)
            BEGIN
                UPDATE PS
                  SET
                      value = '1'
                FROM Enterprise.ProductSetting PS
                     INNER JOIN Enterprise.productsettingtype pp ON PS.productsettingtypeid = pp.productsettingtypeid
                WHERE productid = @ProductId
                      AND PP.ProductSettingTypeId = @ProductSettingTypeId;
            END;
            ELSE
            BEGIN
                EXEC Enterprise.CreateProductSetting
                     @ProductId = @ProductId, -- int
                     @ProductSettingTypeId = @ProductSettingTypeId, -- int
                     @Value = N'1', -- nvarchar(1000)
                     @FromDate = @FromDate, -- datetime
                     @ThruDate = NULL, -- datetime
                     @ProductSettingId = @ProductSettingId OUTPUT; -- int

-- Link the Product Setting to an actual configuration
                EXEC Enterprise.LinkProductSettingToConfiguration
                     @ConfigurationId = @ConfigurationId, -- int
                     @ProductSettingId = @ProductSettingId, -- int
                     @FromDate = @FromDate, -- datetime
                     @ThruDate = NULL;   -- datetime
            END;
------------------------------------------------------------------------------------------------------------------------------------------------------
-- Get the ProductSettingTypeId for the attribute you need to select, the API should have all these cached though.
        SET @ProductSettingId = NULL;
        SET @ProductSettingTypeId = NULL;
        EXEC Enterprise.GetProductSettingType
             @Name = 'MetatagUniqueId', -- varchar(50)
             @ProductSettingTypeId = @ProductSettingTypeId OUTPUT; -- int

-- Create the Value and assign it to the Product and ProductSettingType
        IF EXISTS
(
    SELECT 1
    FROM enterprise.ProductSetting AS PS
         INNER JOIN Enterprise.productsettingtype AS pp ON PS.productsettingtypeid = pp.productsettingtypeid
    WHERE productid = @ProductId
          AND PP.ProductSettingTypeId = @ProductSettingTypeId
)
            BEGIN
                UPDATE PS
                  SET
                      value = 'Revenue Management'
                FROM Enterprise.ProductSetting PS
                     INNER JOIN Enterprise.productsettingtype pp ON PS.productsettingtypeid = pp.productsettingtypeid
                WHERE productid = @ProductId
                      AND PP.ProductSettingTypeId = @ProductSettingTypeId;
            END;
            ELSE
            BEGIN
                EXEC Enterprise.CreateProductSetting
                     @ProductId = @ProductId, -- int
                     @ProductSettingTypeId = @ProductSettingTypeId, -- int
                     @Value = N'Revenue Management', -- nvarchar(1000)
                     @FromDate = @FromDate, -- datetime
                     @ThruDate = NULL, -- datetime
                     @ProductSettingId = @ProductSettingId OUTPUT; -- int

-- Link the Product Setting to an actual configuration
                EXEC Enterprise.LinkProductSettingToConfiguration
                     @ConfigurationId = @ConfigurationId, -- int
                     @ProductSettingId = @ProductSettingId, -- int
                     @FromDate = @FromDate, -- datetime
                     @ThruDate = NULL;   -- datetime
            END;
------------------------------------------------------------------------------------------------------------------------------------------------------
-- Get the ProductSettingTypeId for the attribute you need to select, the API should have all these cached though.
        SET @ProductSettingId = NULL;
        SET @ProductSettingTypeId = NULL;
        EXEC Enterprise.GetProductSettingType
             @Name = 'IsFavorite', -- varchar(50)
             @ProductSettingTypeId = @ProductSettingTypeId OUTPUT; -- int

-- Create the Value and assign it to the Product and ProductSettingType
        IF EXISTS
(
    SELECT 1
    FROM enterprise.ProductSetting AS PS
         INNER JOIN Enterprise.productsettingtype AS pp ON PS.productsettingtypeid = pp.productsettingtypeid
    WHERE productid = @ProductId
          AND PP.ProductSettingTypeId = @ProductSettingTypeId
)
            BEGIN
                UPDATE PS
                  SET
                      value = '1'
                FROM Enterprise.ProductSetting PS
                     INNER JOIN Enterprise.productsettingtype pp ON PS.productsettingtypeid = pp.productsettingtypeid
                WHERE productid = @ProductId
                      AND PP.ProductSettingTypeId = @ProductSettingTypeId;
            END;
            ELSE
            BEGIN
                EXEC Enterprise.CreateProductSetting
                     @ProductId = @ProductId, -- int
                     @ProductSettingTypeId = @ProductSettingTypeId, -- int
                     @Value = N'1', -- nvarchar(1000)
                     @FromDate = @FromDate, -- datetime
                     @ThruDate = NULL, -- datetime
                     @ProductSettingId = @ProductSettingId OUTPUT; -- int

-- Link the Product Setting to an actual configuration
                EXEC Enterprise.LinkProductSettingToConfiguration
                     @ConfigurationId = @ConfigurationId, -- int
                     @ProductSettingId = @ProductSettingId, -- int
                     @FromDate = @FromDate, -- datetime
                     @ThruDate = NULL;   -- datetime
            END;
------------------------------------------------------------------------------------------------------------------------------------------------------
-- Get the ProductSettingTypeId for the attribute you need to select, the API should have all these cached though.
        SET @ProductSettingId = NULL;
        SET @ProductSettingTypeId = NULL;
        EXEC Enterprise.GetProductSettingType
             @Name = 'LearnMore', -- varchar(50)
             @ProductSettingTypeId = @ProductSettingTypeId OUTPUT; -- int

-- Create the Value and assign it to the Product and ProductSettingType
        IF EXISTS
(
    SELECT 1
    FROM enterprise.ProductSetting AS PS
         INNER JOIN Enterprise.productsettingtype AS pp ON PS.productsettingtypeid = pp.productsettingtypeid
    WHERE productid = @ProductId
          AND PP.ProductSettingTypeId = @ProductSettingTypeId
)
            BEGIN
                UPDATE PS
                  SET
                      value = 'https://www.realpage.com/asset-investment-management/portfolio-asset-management/'
                FROM Enterprise.ProductSetting PS
                     INNER JOIN Enterprise.productsettingtype pp ON PS.productsettingtypeid = pp.productsettingtypeid
                WHERE productid = @ProductId
                      AND PP.ProductSettingTypeId = @ProductSettingTypeId;
            END;
            ELSE
            BEGIN
                EXEC Enterprise.CreateProductSetting
                     @ProductId = @ProductId, -- int
                     @ProductSettingTypeId = @ProductSettingTypeId, -- int
                     @Value = N'https://www.realpage.com/asset-investment-management/portfolio-asset-management/', -- nvarchar(1000)
                     @FromDate = @FromDate, -- datetime
                     @ThruDate = NULL, -- datetime
                     @ProductSettingId = @ProductSettingId OUTPUT; -- int

-- Link the Product Setting to an actual configuration
                EXEC Enterprise.LinkProductSettingToConfiguration
                     @ConfigurationId = @ConfigurationId, -- int
                     @ProductSettingId = @ProductSettingId, -- int
                     @FromDate = @FromDate, -- datetime
                     @ThruDate = NULL;   -- datetime
            END;
------------------------------------------------------------------------------------------------------------------------------------------------------
-- Get the ProductSettingTypeId for the attribute you need to select, the API should have all these cached though.
        SET @ProductSettingId = NULL;
        SET @ProductSettingTypeId = NULL;
        EXEC Enterprise.GetProductSettingType
             @Name = 'ApiEndPoint', -- varchar(50)
             @ProductSettingTypeId = @ProductSettingTypeId OUTPUT; -- int

-- Create the Value and assign it to the Product and ProductSettingType
        IF EXISTS
(
    SELECT 1
    FROM enterprise.ProductSetting AS PS
         INNER JOIN Enterprise.productsettingtype AS pp ON PS.productsettingtypeid = pp.productsettingtypeid
    WHERE productid = @ProductId
          AND PP.ProductSettingTypeId = @ProductSettingTypeId
)
            BEGIN
                UPDATE PS
                  SET
                      value = 'https://aodev.realpage.com/ysconfig/ws/'
                FROM Enterprise.ProductSetting PS
                     INNER JOIN Enterprise.productsettingtype pp ON PS.productsettingtypeid = pp.productsettingtypeid
                WHERE productid = @ProductId
                      AND PP.ProductSettingTypeId = @ProductSettingTypeId;
            END;
            ELSE
            BEGIN
                EXEC Enterprise.CreateProductSetting
                     @ProductId = @ProductId, -- int
                     @ProductSettingTypeId = @ProductSettingTypeId, -- int
                     @Value = N'https://aodev.realpage.com/ysconfig/ws/', -- nvarchar(1000)
                     @FromDate = @FromDate, -- datetime
                     @ThruDate = NULL, -- datetime
                     @ProductSettingId = @ProductSettingId OUTPUT; -- int

-- Link the Product Setting to an actual configuration
                EXEC Enterprise.LinkProductSettingToConfiguration
                     @ConfigurationId = @ConfigurationId, -- int
                     @ProductSettingId = @ProductSettingId, -- int
                     @FromDate = @FromDate, -- datetime
                     @ThruDate = NULL;   -- datetime
            END;
------------------------------------------------------------------------------------------------------------------------------------------------------
-- Get the ProductSettingTypeId for the attribute you need to select, the API should have all these cached though.
------------------------------------------------------------------------------------------------------------------------------------------------------
        SET @ProductSettingId = NULL;
        SET @ProductSettingTypeId = NULL;
        EXEC Enterprise.GetProductSettingType
             @Name = 'ApiUserName', -- varchar(50)
             @ProductSettingTypeId = @ProductSettingTypeId OUTPUT; -- int

-- Create the Value and assign it to the Product and ProductSettingType
        IF EXISTS
(
    SELECT 1
    FROM enterprise.ProductSetting AS PS
         INNER JOIN Enterprise.productsettingtype AS pp ON PS.productsettingtypeid = pp.productsettingtypeid
    WHERE productid = @ProductId
          AND PP.ProductSettingTypeId = @ProductSettingTypeId
)
            BEGIN
                UPDATE PS
                  SET
                      value = 'wsuser'
                FROM Enterprise.ProductSetting PS
                     INNER JOIN Enterprise.productsettingtype pp ON PS.productsettingtypeid = pp.productsettingtypeid
                WHERE productid = @ProductId
                      AND PP.ProductSettingTypeId = @ProductSettingTypeId;
            END;
            ELSE
            BEGIN
                EXEC Enterprise.CreateProductSetting
                     @ProductId = @ProductId, -- int
                     @ProductSettingTypeId = @ProductSettingTypeId, -- int
                     @Value = N'wsuser', -- nvarchar(1000)
                     @FromDate = @FromDate, -- datetime
                     @ThruDate = NULL, -- datetime
                     @ProductSettingId = @ProductSettingId OUTPUT; -- int

-- Link the Product Setting to an actual configuration
                EXEC Enterprise.LinkProductSettingToConfiguration
                     @ConfigurationId = @ConfigurationId, -- int
                     @ProductSettingId = @ProductSettingId, -- int
                     @FromDate = @FromDate, -- datetime
                     @ThruDate = NULL;   -- datetime
            END;
------------------------------------------------------------------------------------------------------------------------------------------------------
-- Get the ProductSettingTypeId for the attribute you need to select, the API should have all these cached though.
------------------------------------------------------------------------------------------------------------------------------------------------------
        SET @ProductSettingId = NULL;
        SET @ProductSettingTypeId = NULL;
        EXEC Enterprise.GetProductSettingType
             @Name = 'ApiPassword', -- varchar(50)
             @ProductSettingTypeId = @ProductSettingTypeId OUTPUT; -- int

-- Create the Value and assign it to the Product and ProductSettingType
        IF EXISTS
(
    SELECT 1
    FROM enterprise.ProductSetting AS PS
         INNER JOIN Enterprise.productsettingtype AS pp ON PS.productsettingtypeid = pp.productsettingtypeid
    WHERE productid = @ProductId
          AND PP.ProductSettingTypeId = @ProductSettingTypeId
)
            BEGIN
                UPDATE PS
                  SET
                      value = 'cGdAIXcyM3Jn'
                FROM Enterprise.ProductSetting PS
                     INNER JOIN Enterprise.productsettingtype pp ON PS.productsettingtypeid = pp.productsettingtypeid
                WHERE productid = @ProductId
                      AND PP.ProductSettingTypeId = @ProductSettingTypeId;
            END;
            ELSE
            BEGIN
                EXEC Enterprise.CreateProductSetting
                     @ProductId = @ProductId, -- int
                     @ProductSettingTypeId = @ProductSettingTypeId, -- int
                     @Value = N'cGdAIXcyM3Jn', -- nvarchar(1000)
                     @FromDate = @FromDate, -- datetime
                     @ThruDate = NULL, -- datetime
                     @ProductSettingId = @ProductSettingId OUTPUT; -- int

-- Link the Product Setting to an actual configuration
                EXEC Enterprise.LinkProductSettingToConfiguration
                     @ConfigurationId = @ConfigurationId, -- int
                     @ProductSettingId = @ProductSettingId, -- int
                     @FromDate = @FromDate, -- datetime
                     @ThruDate = NULL;   -- datetime
            END;
------------------------------------------------------------------------------------------------------------------------------------------------------
-- Get the ProductSettingTypeId for the attribute you need to select, the API should have all these cached though.
------------------------------------------------------------------------------------------------------------------------------------------------------
        SET @ProductSettingId = NULL;
        SET @ProductSettingTypeId = NULL;
        EXEC Enterprise.GetProductSettingType
             @Name = 'ProductSuperUserLoginName', -- varchar(50)
             @ProductSettingTypeId = @ProductSettingTypeId OUTPUT; -- int

-- Create the Value and assign it to the Product and ProductSettingType
        IF EXISTS
(
    SELECT 1
    FROM enterprise.ProductSetting AS PS
         INNER JOIN Enterprise.productsettingtype AS pp ON PS.productsettingtypeid = pp.productsettingtypeid
    WHERE productid = @ProductId
          AND PP.ProductSettingTypeId = @ProductSettingTypeId
)
            BEGIN
                UPDATE PS
                  SET
                      value = 'amungale'
                FROM Enterprise.ProductSetting PS
                     INNER JOIN Enterprise.productsettingtype pp ON PS.productsettingtypeid = pp.productsettingtypeid
                WHERE productid = @ProductId
                      AND PP.ProductSettingTypeId = @ProductSettingTypeId;
            END;
            ELSE
            BEGIN
                EXEC Enterprise.CreateProductSetting
                     @ProductId = @ProductId, -- int
                     @ProductSettingTypeId = @ProductSettingTypeId, -- int
                     @Value = N'amungale', -- nvarchar(1000)
                     @FromDate = @FromDate, -- datetime
                     @ThruDate = NULL, -- datetime
                     @ProductSettingId = @ProductSettingId OUTPUT; -- int

-- Link the Product Setting to an actual configuration
                EXEC Enterprise.LinkProductSettingToConfiguration
                     @ConfigurationId = @ConfigurationId, -- int
                     @ProductSettingId = @ProductSettingId, -- int
                     @FromDate = @FromDate, -- datetime
                     @ThruDate = NULL;   -- datetime
            END;
------------------------------------------------------------------------------------------------------------------------------------------------------
-- Get the ProductSettingTypeId for the attribute you need to select, the API should have all these cached though.
------------------------------------------------------------------------------------------------------------------------------------------------------
        SET @ProductSettingId = NULL;
        SET @ProductSettingTypeId = NULL;
        EXEC Enterprise.GetProductSettingType
             @Name = 'ProductStatus', -- varchar(50)
             @ProductSettingTypeId = @ProductSettingTypeId OUTPUT; -- int

-- Create the Value and assign it to the Product and ProductSettingType
        IF EXISTS
(
    SELECT 1
    FROM enterprise.ProductSetting AS PS
         INNER JOIN Enterprise.productsettingtype AS pp ON PS.productsettingtypeid = pp.productsettingtypeid
    WHERE productid = @ProductId
          AND PP.ProductSettingTypeId = @ProductSettingTypeId
)
            BEGIN
                UPDATE PS
                  SET
                      value = '8'
                FROM Enterprise.ProductSetting PS
                     INNER JOIN Enterprise.productsettingtype pp ON PS.productsettingtypeid = pp.productsettingtypeid
                WHERE productid = @ProductId
                      AND PP.ProductSettingTypeId = @ProductSettingTypeId;
            END;
            ELSE
            BEGIN
                EXEC Enterprise.CreateProductSetting
                     @ProductId = @ProductId, -- int
                     @ProductSettingTypeId = @ProductSettingTypeId, -- int
                     @Value = N'8', -- nvarchar(1000)
                     @FromDate = @FromDate, -- datetime
                     @ThruDate = NULL, -- datetime
                     @ProductSettingId = @ProductSettingId OUTPUT; -- int

-- Link the Product Setting to an actual configuration
                EXEC Enterprise.LinkProductSettingToConfiguration
                     @ConfigurationId = @ConfigurationId, -- int
                     @ProductSettingId = @ProductSettingId, -- int
                     @FromDate = @FromDate, -- datetime
                     @ThruDate = NULL;   -- datetime
            END;
------------------------------------------------------------------------------------------------------------------------------------------------------

        EXEC Enterprise.LinkGlobalConfigurationToProduct
             @ConfigurationId = @ConfigurationId, -- int
             @ProductId = @ProductId, -- int
             @FromDate = @FromDate, -- datetime
             @ThruDate = NULL;   -- datetime

        EXEC Enterprise.ListGlobalSettingsForProduct
             @productid = @ProductId;
    END;
-----Axiometrics

--Step 3 -- Add all settings

SET @FromDate = GETUTCDATE();
-- If this is a new Global Product Configuration then create a Product Configuration if you do not have one to use.

SELECT @ProductId = ProductId
FROM Enterprise.Product
WHERE Name = 'Axiometrics';
IF @ProductId = 33
    BEGIN
        IF NOT EXISTS
(
    SELECT 1
    FROM Ident.SamlProductSettings
    WHERE ProductId = @ProductId
)
            BEGIN
                IF @@SERVERNAME = 'RCTUSODBSQL001'
                    BEGIN
                        INSERT INTO Ident.SamlProductSettings
(ProductId,
 LoginUri,
 SigningCertificateThumbprint,
 SubjectIdSamlAttribute
)
                        VALUES
(@ProductId,
 'https://aouat.realpage.com/ysconfig/sso/unified',
 'EF26FEC08C554976572E8A9767DDA437AC452CF6',
 'productUsername'
);
                    END;
                IF @@SERVERNAME = 'RCQUSODBSQL001'
                    BEGIN
                        INSERT INTO Ident.SamlProductSettings
(ProductId,
 LoginUri,
 SigningCertificateThumbprint,
 SubjectIdSamlAttribute
)
                        VALUES
(@ProductId,
 'https://aosat.realpage.com/ysconfig/sso/unified',
 'EF26FEC08C554976572E8A9767DDA437AC452CF6',
 'productUsername'
);
                    END;
                IF @@SERVERNAME IN('RCPGBKDBSQL005A', 'RCPGBKDBSQL005B')
                    BEGIN
                        INSERT INTO Ident.SamlProductSettings
(ProductId,
 LoginUri,
 SigningCertificateThumbprint,
 SubjectIdSamlAttribute
)
                        VALUES
(@ProductId,
 'https://ao.realpage.com/ysconfig/sso/unified',
 'EF26FEC08C554976572E8A9767DDA437AC452CF6',
 'productUsername'
);
                    END;
            END;
            ELSE
            BEGIN
                IF @@SERVERNAME = 'RCTUSODBSQL001'
                    BEGIN
                        UPDATE Ident.SamlProductSettings
                          SET
                              LoginUri = 'https://aouat.realpage.com/ysconfig/sso/unified',
                              SigningCertificateThumbprint = 'EF26FEC08C554976572E8A9767DDA437AC452CF6'
                        WHERE ProductId = @ProductId;
                    END;
                IF @@SERVERNAME = 'RCQUSODBSQL001'
                    BEGIN
                        UPDATE Ident.SamlProductSettings
                          SET
                              LoginUri = 'https://aosat.realpage.com/ysconfig/sso/unified',
                              SigningCertificateThumbprint = 'EF26FEC08C554976572E8A9767DDA437AC452CF6'
                        WHERE ProductId = @ProductId;
                    END;
                IF @@SERVERNAME IN('RCPGBKDBSQL005A', 'RCPGBKDBSQL005B')
                    BEGIN
                        UPDATE Ident.SamlProductSettings
                          SET
                              LoginUri = 'https://ao.realpage.com/ysconfig/sso/unified',
                              SigningCertificateThumbprint = 'EF26FEC08C554976572E8A9767DDA437AC452CF6'
                        WHERE ProductId = @ProductId;
                    END;
            END;
        IF EXISTS
(
    SELECT 1
    FROM Enterprise.GlobalProductConfiguration
    WHERE ProductId = @ProductId
          AND (GETDATE() BETWEEN FromDate AND ThruDate
               OR ThruDate IS NULL)
)
            BEGIN
                SELECT @ConfigurationId = ConfigurationId
                FROM Enterprise.GlobalProductConfiguration
                WHERE ProductId = @ProductId
                      AND (GETDATE() BETWEEN FromDate AND ThruDate
                           OR ThruDate IS NULL);
            END;
            ELSE
            BEGIN
                EXEC Enterprise.CreateProductConfiguration
                     @ConfigurationId = @ConfigurationId OUTPUT; -- int
            END;
--***** Start a Product setting loop for each attribute / value that needs set. *****
------------------------------------------------------------------------------------------------------------------------------------------------------
-- Get the ProductSettingTypeId for the attribute you need to select, the API should have all these cached though.

        SET @ProductSettingId = NULL;
        SET @ProductSettingTypeId = NULL;
        EXEC Enterprise.GetProductSettingType
             @Name = 'ProductUrl', -- varchar(50)
             @ProductSettingTypeId = @ProductSettingTypeId OUTPUT; -- int


-- Create the Value and assign it to the Product and ProductSettingType
        IF EXISTS
(
    SELECT 1
    FROM enterprise.ProductSetting AS PS
         INNER JOIN Enterprise.productsettingtype AS pp ON PS.productsettingtypeid = pp.productsettingtypeid
    WHERE productid = @ProductId
          AND PP.ProductSettingTypeId = @ProductSettingTypeId
)
            BEGIN
                UPDATE PS
                  SET
                      value = '/product/axiometrics'
                FROM Enterprise.ProductSetting PS
                     INNER JOIN Enterprise.productsettingtype pp ON PS.productsettingtypeid = pp.productsettingtypeid
                WHERE productid = @ProductId
                      AND PP.ProductSettingTypeId = @ProductSettingTypeId;
            END;
            ELSE
            BEGIN
                EXEC Enterprise.CreateProductSetting
                     @ProductId = @ProductId, -- int
                     @ProductSettingTypeId = @ProductSettingTypeId, -- int
                     @Value = N'/product/axiometrics', -- nvarchar(1000)
                     @FromDate = @FromDate, -- datetime
                     @ThruDate = NULL, -- datetime
                     @ProductSettingId = @ProductSettingId OUTPUT; -- int

-- Link the Product Setting to an actual configuration
                EXEC Enterprise.LinkProductSettingToConfiguration
                     @ConfigurationId = @ConfigurationId, -- int
                     @ProductSettingId = @ProductSettingId, -- int
                     @FromDate = @FromDate, -- datetime
                     @ThruDate = NULL;   -- datetime
            END;
------------------------------------------------------------------------------------------------------------------------------------------------------
-- Get the ProductSettingTypeId for the attribute you need to select, the API should have all these cached though.

        SET @ProductSettingId = NULL;
        SET @ProductSettingTypeId = NULL;
        EXEC Enterprise.GetProductSettingType
             @Name = 'TitleId', -- varchar(50)
             @ProductSettingTypeId = @ProductSettingTypeId OUTPUT; -- int

        IF EXISTS
(
    SELECT 1
    FROM enterprise.ProductSetting AS PS
         INNER JOIN Enterprise.productsettingtype AS pp ON PS.productsettingtypeid = pp.productsettingtypeid
    WHERE productid = @ProductId
          AND PP.ProductSettingTypeId = @ProductSettingTypeId
)
            BEGIN
                UPDATE PS
                  SET
                      value = 'Axiometrics'
                FROM Enterprise.ProductSetting PS
                     INNER JOIN Enterprise.productsettingtype pp ON PS.productsettingtypeid = pp.productsettingtypeid
                WHERE productid = @ProductId
                      AND PP.ProductSettingTypeId = @ProductSettingTypeId;
            END;
            ELSE
            BEGIN
                EXEC Enterprise.CreateProductSetting
                     @ProductId = @ProductId, -- int
                     @ProductSettingTypeId = @ProductSettingTypeId, -- int
                     @Value = N'Axiometrics', -- nvarchar(1000)
                     @FromDate = @FromDate, -- datetime
                     @ThruDate = NULL, -- datetime
                     @ProductSettingId = @ProductSettingId OUTPUT; -- int

-- Link the Product Setting to an actual configuration
                EXEC Enterprise.LinkProductSettingToConfiguration
                     @ConfigurationId = @ConfigurationId, -- int
                     @ProductSettingId = @ProductSettingId, -- int
                     @FromDate = @FromDate, -- datetime
                     @ThruDate = NULL;   -- datetime
            END;
------------------------------------------------------------------------------------------------------------------------------------------------------
-- Get the ProductSettingTypeId for the attribute you need to select, the API should have all these cached though.
        SET @ProductSettingId = NULL;
        SET @ProductSettingTypeId = NULL;
        EXEC Enterprise.GetProductSettingType
             @Name = 'TitleUniqueId', -- varchar(50)
             @ProductSettingTypeId = @ProductSettingTypeId OUTPUT; -- int

-- Create the Value and assign it to the Product and ProductSettingType
        IF EXISTS
(
    SELECT 1
    FROM enterprise.ProductSetting AS PS
         INNER JOIN Enterprise.productsettingtype AS pp ON PS.productsettingtypeid = pp.productsettingtypeid
    WHERE productid = @ProductId
          AND PP.ProductSettingTypeId = @ProductSettingTypeId
)
            BEGIN
                UPDATE PS
                  SET
                      value = '5A66353F-338E-444E-8775-06FDC6B4D020'
                FROM Enterprise.ProductSetting PS
                     INNER JOIN Enterprise.productsettingtype pp ON PS.productsettingtypeid = pp.productsettingtypeid
                WHERE productid = @ProductId
                      AND PP.ProductSettingTypeId = @ProductSettingTypeId;
            END;
            ELSE
            BEGIN
                EXEC Enterprise.CreateProductSetting
                     @ProductId = @ProductId, -- int
                     @ProductSettingTypeId = @ProductSettingTypeId, -- int
                     @Value = N'5A66353F-338E-444E-8775-06FDC6B4D020', -- nvarchar(1000)
                     @FromDate = @FromDate, -- datetime
                     @ThruDate = NULL, -- datetime
                     @ProductSettingId = @ProductSettingId OUTPUT; -- int

-- Link the Product Setting to an actual configuration
                EXEC Enterprise.LinkProductSettingToConfiguration
                     @ConfigurationId = @ConfigurationId, -- int
                     @ProductSettingId = @ProductSettingId, -- int
                     @FromDate = @FromDate, -- datetime
                     @ThruDate = NULL;   -- datetime
            END;
------------------------------------------------------------------------------------------------------------------------------------------------------
-- Get the ProductSettingTypeId for the attribute you need to select, the API should have all these cached though.
        SET @ProductSettingId = NULL;
        SET @ProductSettingTypeId = NULL;
        EXEC Enterprise.GetProductSettingType
             @Name = 'IsNewTab', -- varchar(50)
             @ProductSettingTypeId = @ProductSettingTypeId OUTPUT; -- int

-- Create the Value and assign it to the Product and ProductSettingType
        IF EXISTS
(
    SELECT 1
    FROM enterprise.ProductSetting AS PS
         INNER JOIN Enterprise.productsettingtype AS pp ON PS.productsettingtypeid = pp.productsettingtypeid
    WHERE productid = @ProductId
          AND PP.ProductSettingTypeId = @ProductSettingTypeId
)
            BEGIN
                UPDATE PS
                  SET
                      value = '1'
                FROM Enterprise.ProductSetting PS
                     INNER JOIN Enterprise.productsettingtype pp ON PS.productsettingtypeid = pp.productsettingtypeid
                WHERE productid = @ProductId
                      AND PP.ProductSettingTypeId = @ProductSettingTypeId;
            END;
            ELSE
            BEGIN
                EXEC Enterprise.CreateProductSetting
                     @ProductId = @ProductId, -- int
                     @ProductSettingTypeId = @ProductSettingTypeId, -- int
                     @Value = N'1', -- nvarchar(1000)
                     @FromDate = @FromDate, -- datetime
                     @ThruDate = NULL, -- datetime
                     @ProductSettingId = @ProductSettingId OUTPUT; -- int

-- Link the Product Setting to an actual configuration
                EXEC Enterprise.LinkProductSettingToConfiguration
                     @ConfigurationId = @ConfigurationId, -- int
                     @ProductSettingId = @ProductSettingId, -- int
                     @FromDate = @FromDate, -- datetime
                     @ThruDate = NULL;   -- datetime
            END;
------------------------------------------------------------------------------------------------------------------------------------------------------
-- Get the ProductSettingTypeId for the attribute you need to select, the API should have all these cached though.
        SET @ProductSettingId = NULL;
        SET @ProductSettingTypeId = NULL;
        EXEC Enterprise.GetProductSettingType
             @Name = 'MetatagUniqueId', -- varchar(50)
             @ProductSettingTypeId = @ProductSettingTypeId OUTPUT; -- int

-- Create the Value and assign it to the Product and ProductSettingType
        IF EXISTS
(
    SELECT 1
    FROM enterprise.ProductSetting AS PS
         INNER JOIN Enterprise.productsettingtype AS pp ON PS.productsettingtypeid = pp.productsettingtypeid
    WHERE productid = @ProductId
          AND PP.ProductSettingTypeId = @ProductSettingTypeId
)
            BEGIN
                UPDATE PS
                  SET
                      value = 'Axiometrics'
                FROM Enterprise.ProductSetting PS
                     INNER JOIN Enterprise.productsettingtype pp ON PS.productsettingtypeid = pp.productsettingtypeid
                WHERE productid = @ProductId
                      AND PP.ProductSettingTypeId = @ProductSettingTypeId;
            END;
            ELSE
            BEGIN
                EXEC Enterprise.CreateProductSetting
                     @ProductId = @ProductId, -- int
                     @ProductSettingTypeId = @ProductSettingTypeId, -- int
                     @Value = N'Axiometrics', -- nvarchar(1000)
                     @FromDate = @FromDate, -- datetime
                     @ThruDate = NULL, -- datetime
                     @ProductSettingId = @ProductSettingId OUTPUT; -- int

-- Link the Product Setting to an actual configuration
                EXEC Enterprise.LinkProductSettingToConfiguration
                     @ConfigurationId = @ConfigurationId, -- int
                     @ProductSettingId = @ProductSettingId, -- int
                     @FromDate = @FromDate, -- datetime
                     @ThruDate = NULL;   -- datetime
            END;
------------------------------------------------------------------------------------------------------------------------------------------------------
-- Get the ProductSettingTypeId for the attribute you need to select, the API should have all these cached though.
        SET @ProductSettingId = NULL;
        SET @ProductSettingTypeId = NULL;
        EXEC Enterprise.GetProductSettingType
             @Name = 'IsFavorite', -- varchar(50)
             @ProductSettingTypeId = @ProductSettingTypeId OUTPUT; -- int

-- Create the Value and assign it to the Product and ProductSettingType
        IF EXISTS
(
    SELECT 1
    FROM enterprise.ProductSetting AS PS
         INNER JOIN Enterprise.productsettingtype AS pp ON PS.productsettingtypeid = pp.productsettingtypeid
    WHERE productid = @ProductId
          AND PP.ProductSettingTypeId = @ProductSettingTypeId
)
            BEGIN
                UPDATE PS
                  SET
                      value = '1'
                FROM Enterprise.ProductSetting PS
                     INNER JOIN Enterprise.productsettingtype pp ON PS.productsettingtypeid = pp.productsettingtypeid
                WHERE productid = @ProductId
                      AND PP.ProductSettingTypeId = @ProductSettingTypeId;
            END;
            ELSE
            BEGIN
                EXEC Enterprise.CreateProductSetting
                     @ProductId = @ProductId, -- int
                     @ProductSettingTypeId = @ProductSettingTypeId, -- int
                     @Value = N'1', -- nvarchar(1000)
                     @FromDate = @FromDate, -- datetime
                     @ThruDate = NULL, -- datetime
                     @ProductSettingId = @ProductSettingId OUTPUT; -- int

-- Link the Product Setting to an actual configuration
                EXEC Enterprise.LinkProductSettingToConfiguration
                     @ConfigurationId = @ConfigurationId, -- int
                     @ProductSettingId = @ProductSettingId, -- int
                     @FromDate = @FromDate, -- datetime
                     @ThruDate = NULL;   -- datetime
            END;
------------------------------------------------------------------------------------------------------------------------------------------------------
-- Get the ProductSettingTypeId for the attribute you need to select, the API should have all these cached though.
        SET @ProductSettingId = NULL;
        SET @ProductSettingTypeId = NULL;
        EXEC Enterprise.GetProductSettingType
             @Name = 'LearnMore', -- varchar(50)
             @ProductSettingTypeId = @ProductSettingTypeId OUTPUT; -- int

-- Create the Value and assign it to the Product and ProductSettingType
        IF EXISTS
(
    SELECT 1
    FROM enterprise.ProductSetting AS PS
         INNER JOIN Enterprise.productsettingtype AS pp ON PS.productsettingtypeid = pp.productsettingtypeid
    WHERE productid = @ProductId
          AND PP.ProductSettingTypeId = @ProductSettingTypeId
)
            BEGIN
                UPDATE PS
                  SET
                      value = 'https://www.realpage.com/asset-investment-management/portfolio-asset-management/'
                FROM Enterprise.ProductSetting PS
                     INNER JOIN Enterprise.productsettingtype pp ON PS.productsettingtypeid = pp.productsettingtypeid
                WHERE productid = @ProductId
                      AND PP.ProductSettingTypeId = @ProductSettingTypeId;
            END;
            ELSE
            BEGIN
                EXEC Enterprise.CreateProductSetting
                     @ProductId = @ProductId, -- int
                     @ProductSettingTypeId = @ProductSettingTypeId, -- int
                     @Value = N'https://www.realpage.com/asset-investment-management/portfolio-asset-management/', -- nvarchar(1000)
                     @FromDate = @FromDate, -- datetime
                     @ThruDate = NULL, -- datetime
                     @ProductSettingId = @ProductSettingId OUTPUT; -- int

-- Link the Product Setting to an actual configuration
                EXEC Enterprise.LinkProductSettingToConfiguration
                     @ConfigurationId = @ConfigurationId, -- int
                     @ProductSettingId = @ProductSettingId, -- int
                     @FromDate = @FromDate, -- datetime
                     @ThruDate = NULL;   -- datetime
            END;
------------------------------------------------------------------------------------------------------------------------------------------------------
-- Get the ProductSettingTypeId for the attribute you need to select, the API should have all these cached though.
        SET @ProductSettingId = NULL;
        SET @ProductSettingTypeId = NULL;
        EXEC Enterprise.GetProductSettingType
             @Name = 'ApiEndPoint', -- varchar(50)
             @ProductSettingTypeId = @ProductSettingTypeId OUTPUT; -- int

-- Create the Value and assign it to the Product and ProductSettingType
        IF EXISTS
(
    SELECT 1
    FROM enterprise.ProductSetting AS PS
         INNER JOIN Enterprise.productsettingtype AS pp ON PS.productsettingtypeid = pp.productsettingtypeid
    WHERE productid = @ProductId
          AND PP.ProductSettingTypeId = @ProductSettingTypeId
)
            BEGIN
                UPDATE PS
                  SET
                      value = 'https://aodev.realpage.com/ysconfig/ws/'
                FROM Enterprise.ProductSetting PS
                     INNER JOIN Enterprise.productsettingtype pp ON PS.productsettingtypeid = pp.productsettingtypeid
                WHERE productid = @ProductId
                      AND PP.ProductSettingTypeId = @ProductSettingTypeId;
            END;
            ELSE
            BEGIN
                EXEC Enterprise.CreateProductSetting
                     @ProductId = @ProductId, -- int
                     @ProductSettingTypeId = @ProductSettingTypeId, -- int
                     @Value = N'https://aodev.realpage.com/ysconfig/ws/', -- nvarchar(1000)
                     @FromDate = @FromDate, -- datetime
                     @ThruDate = NULL, -- datetime
                     @ProductSettingId = @ProductSettingId OUTPUT; -- int

-- Link the Product Setting to an actual configuration
                EXEC Enterprise.LinkProductSettingToConfiguration
                     @ConfigurationId = @ConfigurationId, -- int
                     @ProductSettingId = @ProductSettingId, -- int
                     @FromDate = @FromDate, -- datetime
                     @ThruDate = NULL;   -- datetime
            END;
------------------------------------------------------------------------------------------------------------------------------------------------------
-- Get the ProductSettingTypeId for the attribute you need to select, the API should have all these cached though.
------------------------------------------------------------------------------------------------------------------------------------------------------
        SET @ProductSettingId = NULL;
        SET @ProductSettingTypeId = NULL;
        EXEC Enterprise.GetProductSettingType
             @Name = 'ApiUserName', -- varchar(50)
             @ProductSettingTypeId = @ProductSettingTypeId OUTPUT; -- int

-- Create the Value and assign it to the Product and ProductSettingType
        IF EXISTS
(
    SELECT 1
    FROM enterprise.ProductSetting AS PS
         INNER JOIN Enterprise.productsettingtype AS pp ON PS.productsettingtypeid = pp.productsettingtypeid
    WHERE productid = @ProductId
          AND PP.ProductSettingTypeId = @ProductSettingTypeId
)
            BEGIN
                UPDATE PS
                  SET
                      value = 'wsuser'
                FROM Enterprise.ProductSetting PS
                     INNER JOIN Enterprise.productsettingtype pp ON PS.productsettingtypeid = pp.productsettingtypeid
                WHERE productid = @ProductId
                      AND PP.ProductSettingTypeId = @ProductSettingTypeId;
            END;
            ELSE
            BEGIN
                EXEC Enterprise.CreateProductSetting
                     @ProductId = @ProductId, -- int
                     @ProductSettingTypeId = @ProductSettingTypeId, -- int
                     @Value = N'wsuser', -- nvarchar(1000)
                     @FromDate = @FromDate, -- datetime
                     @ThruDate = NULL, -- datetime
                     @ProductSettingId = @ProductSettingId OUTPUT; -- int

-- Link the Product Setting to an actual configuration
                EXEC Enterprise.LinkProductSettingToConfiguration
                     @ConfigurationId = @ConfigurationId, -- int
                     @ProductSettingId = @ProductSettingId, -- int
                     @FromDate = @FromDate, -- datetime
                     @ThruDate = NULL;   -- datetime
            END;
------------------------------------------------------------------------------------------------------------------------------------------------------
-- Get the ProductSettingTypeId for the attribute you need to select, the API should have all these cached though.
------------------------------------------------------------------------------------------------------------------------------------------------------
        SET @ProductSettingId = NULL;
        SET @ProductSettingTypeId = NULL;
        EXEC Enterprise.GetProductSettingType
             @Name = 'ApiPassword', -- varchar(50)
             @ProductSettingTypeId = @ProductSettingTypeId OUTPUT; -- int

-- Create the Value and assign it to the Product and ProductSettingType
        IF EXISTS
(
    SELECT 1
    FROM enterprise.ProductSetting AS PS
         INNER JOIN Enterprise.productsettingtype AS pp ON PS.productsettingtypeid = pp.productsettingtypeid
    WHERE productid = @ProductId
          AND PP.ProductSettingTypeId = @ProductSettingTypeId
)
            BEGIN
                UPDATE PS
                  SET
                      value = 'cGdAIXcyM3Jn'
                FROM Enterprise.ProductSetting PS
                     INNER JOIN Enterprise.productsettingtype pp ON PS.productsettingtypeid = pp.productsettingtypeid
                WHERE productid = @ProductId
                      AND PP.ProductSettingTypeId = @ProductSettingTypeId;
            END;
            ELSE
            BEGIN
                EXEC Enterprise.CreateProductSetting
                     @ProductId = @ProductId, -- int
                     @ProductSettingTypeId = @ProductSettingTypeId, -- int
                     @Value = N'cGdAIXcyM3Jn', -- nvarchar(1000)
                     @FromDate = @FromDate, -- datetime
                     @ThruDate = NULL, -- datetime
                     @ProductSettingId = @ProductSettingId OUTPUT; -- int

-- Link the Product Setting to an actual configuration
                EXEC Enterprise.LinkProductSettingToConfiguration
                     @ConfigurationId = @ConfigurationId, -- int
                     @ProductSettingId = @ProductSettingId, -- int
                     @FromDate = @FromDate, -- datetime
                     @ThruDate = NULL;   -- datetime
            END;
------------------------------------------------------------------------------------------------------------------------------------------------------
-- Get the ProductSettingTypeId for the attribute you need to select, the API should have all these cached though.
------------------------------------------------------------------------------------------------------------------------------------------------------
        SET @ProductSettingId = NULL;
        SET @ProductSettingTypeId = NULL;
        EXEC Enterprise.GetProductSettingType
             @Name = 'ProductSuperUserLoginName', -- varchar(50)
             @ProductSettingTypeId = @ProductSettingTypeId OUTPUT; -- int

-- Create the Value and assign it to the Product and ProductSettingType
        IF EXISTS
(
    SELECT 1
    FROM enterprise.ProductSetting AS PS
         INNER JOIN Enterprise.productsettingtype AS pp ON PS.productsettingtypeid = pp.productsettingtypeid
    WHERE productid = @ProductId
          AND PP.ProductSettingTypeId = @ProductSettingTypeId
)
            BEGIN
                UPDATE PS
                  SET
                      value = 'amungale'
                FROM Enterprise.ProductSetting PS
                     INNER JOIN Enterprise.productsettingtype pp ON PS.productsettingtypeid = pp.productsettingtypeid
                WHERE productid = @ProductId
                      AND PP.ProductSettingTypeId = @ProductSettingTypeId;
            END;
            ELSE
            BEGIN
                EXEC Enterprise.CreateProductSetting
                     @ProductId = @ProductId, -- int
                     @ProductSettingTypeId = @ProductSettingTypeId, -- int
                     @Value = N'amungale', -- nvarchar(1000)
                     @FromDate = @FromDate, -- datetime
                     @ThruDate = NULL, -- datetime
                     @ProductSettingId = @ProductSettingId OUTPUT; -- int

-- Link the Product Setting to an actual configuration
                EXEC Enterprise.LinkProductSettingToConfiguration
                     @ConfigurationId = @ConfigurationId, -- int
                     @ProductSettingId = @ProductSettingId, -- int
                     @FromDate = @FromDate, -- datetime
                     @ThruDate = NULL;   -- datetime
            END;
------------------------------------------------------------------------------------------------------------------------------------------------------
-- Get the ProductSettingTypeId for the attribute you need to select, the API should have all these cached though.
------------------------------------------------------------------------------------------------------------------------------------------------------
        SET @ProductSettingId = NULL;
        SET @ProductSettingTypeId = NULL;
        EXEC Enterprise.GetProductSettingType
             @Name = 'ProductStatus', -- varchar(50)
             @ProductSettingTypeId = @ProductSettingTypeId OUTPUT; -- int

-- Create the Value and assign it to the Product and ProductSettingType
        IF EXISTS
(
    SELECT 1
    FROM enterprise.ProductSetting AS PS
         INNER JOIN Enterprise.productsettingtype AS pp ON PS.productsettingtypeid = pp.productsettingtypeid
    WHERE productid = @ProductId
          AND PP.ProductSettingTypeId = @ProductSettingTypeId
)
            BEGIN
                UPDATE PS
                  SET
                      value = '8'
                FROM Enterprise.ProductSetting PS
                     INNER JOIN Enterprise.productsettingtype pp ON PS.productsettingtypeid = pp.productsettingtypeid
                WHERE productid = @ProductId
                      AND PP.ProductSettingTypeId = @ProductSettingTypeId;
            END;
            ELSE
            BEGIN
                EXEC Enterprise.CreateProductSetting
                     @ProductId = @ProductId, -- int
                     @ProductSettingTypeId = @ProductSettingTypeId, -- int
                     @Value = N'8', -- nvarchar(1000)
                     @FromDate = @FromDate, -- datetime
                     @ThruDate = NULL, -- datetime
                     @ProductSettingId = @ProductSettingId OUTPUT; -- int

-- Link the Product Setting to an actual configuration
                EXEC Enterprise.LinkProductSettingToConfiguration
                     @ConfigurationId = @ConfigurationId, -- int
                     @ProductSettingId = @ProductSettingId, -- int
                     @FromDate = @FromDate, -- datetime
                     @ThruDate = NULL;   -- datetime
            END;
------------------------------------------------------------------------------------------------------------------------------------------------------

        EXEC Enterprise.LinkGlobalConfigurationToProduct
             @ConfigurationId = @ConfigurationId, -- int
             @ProductId = @ProductId, -- int
             @FromDate = @FromDate, -- datetime
             @ThruDate = NULL;   -- datetime

        EXEC Enterprise.ListGlobalSettingsForProduct
             @productid = @ProductId;
    END;


	GO

/****************************
RedBookAPIEndPoint
****************************/
--GB-988 - Create RedBookApiEndPoint

DECLARE @ProductSettingTypeId INT
DECLARE @ProductSettingId INT
DECLARE @ProductID INT
DECLARE @FromDate DATETIME
DECLARE @ConfigurationId INT

IF NOT EXISTS
(
    SELECT *
    FROM Enterprise.ProductSettingType
    WHERE Name = 'RedBookApiEndPoint'
)
    BEGIN
        EXEC Enterprise.CreateProductSettingType
             'RedBookApiEndPoint',
             'RedBookApiEndPoint',
             @ProductSettingTypeId OUTPUT;
    END;
    ELSE
    BEGIN
        SELECT @ProductSettingTypeId = ProductSettingTypeId
        FROM Enterprise.ProductSettingType
        WHERE Name = 'RedBookApiEndPoint';
    END;

SELECT @ProductId = ProductId
FROM Enterprise.Product
WHERE Name = 'Unified Platform';

SET @FromDate = GETUTCDATE();
IF  @@SERVERNAME = 'RCDUSODBSQL001'
    BEGIN
        EXEC Enterprise.CreateProductSetting
             @ProductId,
             @ProductSettingTypeId,
             'https://settings-dev.corp.realpage.com/',
             @FromDate,
             NULL,
             @ProductSettingId OUTPUT;
		
        SELECT @COnfigurationId = ConfigurationId
        FROM Enterprise.GlobalProductConfiguration
        WHERE ProductId = @ProductId
              AND ThruDate IS NULL;
	
        EXEC Enterprise.LinkProductSettingToConfiguration
             @ConfigurationId,
             @ProductSettingID,
             NULL,
             NULL;
    END;

IF @@SERVERNAME = 'RCTUSODBSQL001'
    BEGIN
        EXEC Enterprise.CreateProductSetting
             @ProductId,
             @ProductSettingTypeId,
             'https://settings-qa.realpage.com/',
             @FromDate,
             NULL,
             @ProductSettingId OUTPUT;
        SELECT @ConfigurationId = ConfigurationId
        FROM Enterprise.GlobalProductConfiguration
        WHERE ProductId = @ProductId
              AND ThruDate IS NULL;
        EXEC Enterprise.LinkProductSettingToConfiguration
             @ConfigurationId,
             @ProductSettingID,
             NULL,
             NULL;
    END;
IF @@SERVERNAME = 'RCQUSODBSQL001'
    BEGIN
        EXEC Enterprise.CreateProductSetting
             @ProductId,
             @ProductSettingTypeId,
             'https://settings-sat.realpage.com/',
             @FromDate,
             NULL,
             @ProductSettingId OUTPUT;
        SELECT @ConfigurationId = ConfigurationId
        FROM Enterprise.GlobalProductConfiguration
        WHERE ProductId = @ProductId
              AND ThruDate IS NULL;
        EXEC Enterprise.LinkProductSettingToConfiguration
             @ConfigurationId,
             @ProductSettingID,
             NULL,
             NULL;
    END;
IF @@SERVERNAME IN('RCPGBKDBSQL005A', 'RCPGBKDBSQL005B')
    BEGIN
        EXEC Enterprise.CreateProductSetting
             @ProductId,
             @ProductSettingTypeId,
             'https://settings.realpage.com/',
             @FromDate,
             NULL,
             @ProductSettingId OUTPUT;
        SELECT @ConfigurationId = ConfigurationId
        FROM Enterprise.GlobalProductConfiguration
        WHERE ProductId = @ProductId
              AND ThruDate IS NULL;
        EXEC Enterprise.LinkProductSettingToConfiguration
             @ConfigurationId,
             @ProductSettingID,
             NULL,
             NULL;
    END;
	GO

	/*********************
EASYLMS
************************/
GO
DECLARE @ProductName nvarchar(100) = 'EasyLMS',
	@ProductId int,
	@FromDate datetime,
	@ProductSettingId int,
	@ProductSettingTypeId int,
	@ConfigurationId int

SET @ProductId = 36
SET @FromDate = GETUTCDATE()

IF NOT EXISTS(SELECT TOP 1 1 FROM Enterprise.Product WHERE ProductId = @ProductId AND Name = @ProductName)
BEGIN
	INSERT INTO Enterprise.Product (
		ProductId,
		ProductGUID,
		Name,
		Description,
		ProductTypeId,
		BooksProductCode
	)
	SELECT	@ProductId, --ProductId
					N'5C097B77-CEA2-4DC6-87D9-5E24333DCFB9', --ProductGUID
					@ProductName, --Name
					@ProductName, --Description
					NULL AS ProductTypeID,
					'ELMS'
	
	EXEC Enterprise.CreateProductConfiguration @ConfigurationId = @ConfigurationId OUTPUT -- int

	--***** Start a Product setting loop for each attribute / value that needs set. *****
	------------------------------------------------------------------------------------------------------------------------------------------------------
	-- Get the ProductSettingTypeId for the attribute you need to select, the API should have all these cached though.
	SET @ProductSettingId = NULL
	SET @ProductSettingTypeId = NULL

	EXEC Enterprise.GetProductSettingType
		@Name = 'TitleId',                                         -- varchar(50)
		@ProductSettingTypeId = @ProductSettingTypeId OUTPUT -- int

	-- Create the Value and assign it to the Product and ProductSettingType
	EXEC Enterprise.CreateProductSetting
		@ProductId = @ProductId,                             -- int
		@ProductSettingTypeId = @ProductSettingTypeId,                  -- int
		@Value = @ProductName,                               -- nvarchar(1000)
		@FromDate = @FromDate,          -- datetime
		@ThruDate = NULL,          -- datetime
		@ProductSettingId = @ProductSettingId OUTPUT -- int

	-- Link the Product Setting to an actual configuration
	EXEC Enterprise.LinkProductSettingToConfiguration
		@ConfigurationId = @ConfigurationId,              -- int
		@ProductSettingId = @ProductSettingId,             -- int
		@FromDate = @FromDate, -- datetime
		@ThruDate = NULL   -- datetime

	------------------------------------------------------------------------------------------------------------------------------------------------------
	-- Get the ProductSettingTypeId for the attribute you need to select, the API should have all these cached though.
	SET @ProductSettingId = NULL
	SET @ProductSettingTypeId = NULL

	EXEC Enterprise.GetProductSettingType
		@Name = 'TitleUniqueId',                                         -- varchar(50)
		@ProductSettingTypeId = @ProductSettingTypeId OUTPUT -- int

	-- Create the Value and assign it to the Product and ProductSettingType
	EXEC Enterprise.CreateProductSetting
		@ProductId = @ProductId,                             -- int
		@ProductSettingTypeId = @ProductSettingTypeId,                  -- int
		@Value = N'225C7281-6AEC-4E05-8463-F256C98D00E8',                               -- nvarchar(1000)
		@FromDate = @FromDate,          -- datetime
		@ThruDate = NULL,          -- datetime
		@ProductSettingId = @ProductSettingId OUTPUT -- int

	-- Link the Product Setting to an actual configuration
	EXEC Enterprise.LinkProductSettingToConfiguration
		@ConfigurationId = @ConfigurationId,              -- int
		@ProductSettingId = @ProductSettingId,             -- int
		@FromDate = @FromDate, -- datetime
		@ThruDate = NULL   -- datetime

	------------------------------------------------------------------------------------------------------------------------------------------------------
	-- Get the ProductSettingTypeId for the attribute you need to select, the API should have all these cached though.
	SET @ProductSettingId = NULL
	SET @ProductSettingTypeId = NULL

	EXEC Enterprise.GetProductSettingType
		@Name = 'IsNewTab',                                         -- varchar(50)
		@ProductSettingTypeId = @ProductSettingTypeId OUTPUT -- int

	-- Create the Value and assign it to the Product and ProductSettingType
	EXEC Enterprise.CreateProductSetting
		@ProductId = @ProductId,                             -- int
		@ProductSettingTypeId = @ProductSettingTypeId,                  -- int
		@Value = N'1',                               -- nvarchar(1000)
		@FromDate = @FromDate,          -- datetime
		@ThruDate = NULL,          -- datetime
		@ProductSettingId = @ProductSettingId OUTPUT -- int

	-- Link the Product Setting to an actual configuration
	EXEC Enterprise.LinkProductSettingToConfiguration
		@ConfigurationId = @ConfigurationId,              -- int
		@ProductSettingId = @ProductSettingId,             -- int
		@FromDate = @FromDate, -- datetime
		@ThruDate = NULL   -- datetime

	------------------------------------------------------------------------------------------------------------------------------------------------------
	-- Get the ProductSettingTypeId for the attribute you need to select, the API should have all these cached though.
	SET @ProductSettingId = NULL
	SET @ProductSettingTypeId = NULL

	EXEC Enterprise.GetProductSettingType
		@Name = 'MetatagUniqueId',                                         -- varchar(50)
		@ProductSettingTypeId = @ProductSettingTypeId OUTPUT -- int

	-- Create the Value and assign it to the Product and ProductSettingType
	EXEC Enterprise.CreateProductSetting
		@ProductId = @ProductId,                             -- int
		@ProductSettingTypeId = @ProductSettingTypeId,                  -- int
		@Value = @ProductName,                               -- nvarchar(1000)
		@FromDate = @FromDate,          -- datetime
		@ThruDate = NULL,          -- datetime
		@ProductSettingId = @ProductSettingId OUTPUT -- int

	-- Link the Product Setting to an actual configuration
	EXEC Enterprise.LinkProductSettingToConfiguration
		@ConfigurationId = @ConfigurationId,              -- int
		@ProductSettingId = @ProductSettingId,             -- int
		@FromDate = @FromDate, -- datetime
		@ThruDate = NULL   -- datetime

	------------------------------------------------------------------------------------------------------------------------------------------------------
	-- Get the ProductSettingTypeId for the attribute you need to select, the API should have all these cached though.
	SET @ProductSettingId = NULL
	SET @ProductSettingTypeId = NULL

	EXEC Enterprise.GetProductSettingType
		@Name = 'ProductUrl',                                         -- varchar(50)
		@ProductSettingTypeId = @ProductSettingTypeId OUTPUT -- int

	-- Create the Value and assign it to the Product and ProductSettingType
	EXEC Enterprise.CreateProductSetting
		@ProductId = @ProductId,                             -- int
		@ProductSettingTypeId = @ProductSettingTypeId,                  -- int
		@Value = N'http://realpagelearning-dev.com/api.aspx',                               -- nvarchar(1000)
		@FromDate = @FromDate,          -- datetime
		@ThruDate = NULL,          -- datetime
		@ProductSettingId = @ProductSettingId OUTPUT -- int

	-- Link the Product Setting to an actual configuration
	EXEC Enterprise.LinkProductSettingToConfiguration
		@ConfigurationId = @ConfigurationId,              -- int
		@ProductSettingId = @ProductSettingId,             -- int
		@FromDate = @FromDate, -- datetime
		@ThruDate = NULL   -- datetime

	------------------------------------------------------------------------------------------------------------------------------------------------------
	-- Get the ProductSettingTypeId for the attribute you need to select, the API should have all these cached though.
	SET @ProductSettingId = NULL
	SET @ProductSettingTypeId = NULL

	EXEC Enterprise.GetProductSettingType
		@Name = 'IsResource',                                         -- varchar(50)
		@ProductSettingTypeId = @ProductSettingTypeId OUTPUT -- int

	-- Create the Value and assign it to the Product and ProductSettingType
	EXEC Enterprise.CreateProductSetting
		@ProductId = @ProductId,                             -- int
		@ProductSettingTypeId = @ProductSettingTypeId,                  -- int
		@Value = N'0',                               -- nvarchar(1000)
		@FromDate = @FromDate,          -- datetime
		@ThruDate = NULL,          -- datetime
		@ProductSettingId = @ProductSettingId OUTPUT -- int

	-- Link the Product Setting to an actual configuration
	EXEC Enterprise.LinkProductSettingToConfiguration
		@ConfigurationId = @ConfigurationId,              -- int
		@ProductSettingId = @ProductSettingId,             -- int
		@FromDate = @FromDate, -- datetime
		@ThruDate = NULL   -- datetime

	------------------------------------------------------------------------------------------------------------------------------------------------------
	EXEC Enterprise.LinkGlobalConfigurationToProduct
		@ConfigurationId = @ConfigurationId,              -- int
		@ProductId = @ProductId,                    -- int
		@FromDate = @FromDate, -- datetime
		@ThruDate = NULL   -- datetime

	EXEC Enterprise.ListGlobalSettingsForProduct @productid = @ProductId
END

IF NOT EXISTS(SELECT TOP 1 1 FROM Ident.SamlProductSettings WHERE ProductId = @ProductId)
BEGIN
	INSERT INTO Ident.SamlProductSettings (
		ProductId,
		LoginUri,
		SigningCertificateThumbprint,
		SubjectIdSamlAttribute
	)
	VALUES (
		@ProductId, --ProductId
		N'https://www.clientaccess.leasingdesk.realpage.com/acs.aspx', --LoginUri
		N'E30F7625D160B57CE3ED94E49FEDBFE178C80DC4', --SigningCertificateThumbprint
		N'productUsername' --SubjectIdSamlAttribute
	)
END
GO

-- Resident Portal Migraiton API EndPoint 
DECLARE @ProductSettingTypeId INT
DECLARE @ProductSettingId INT
DECLARE @ProductId INT
DECLARE @ConfigurationId INT

SET @ProductSettingTypeId = NULL;
SET @ProductSettingId = NULL;

EXEC Enterprise.GetProductSettingType @Name = 'MTApiEndPoint', 	@ProductSettingTypeId = @ProductSettingTypeId OUTPUT;
SELECT @ProductId = ProductId FROM Enterprise.Product WHERE Name = 'Resident Portal';
SELECT @ConfigurationId = ConfigurationId FROM Enterprise.GlobalProductConfiguration WHERE ProductId = @ProductId AND (GETDATE() BETWEEN FromDate AND ThruDate OR  ThruDate IS NULL);

IF @@SERVERNAME = 'RCDUSODBSQL001'
BEGIN
	EXEC Enterprise.CreateProductSetting @ProductId, @ProductSettingTypeId, 'http://rcqacbwwngx005.realpage.com/api', NULL, NULL, @ProductSettingId OUTPUT;
END;
	
IF @@SERVERNAME = 'RCTUSODBSQL001'
BEGIN
	EXEC Enterprise.CreateProductSetting @ProductId, @ProductSettingTypeId, 'http://rcqacbwwngx005.realpage.com/api', NULL, NULL, @ProductSettingId OUTPUT;
END;
	
IF @@SERVERNAME = 'RCQUSODBSQL001'
BEGIN
	EXEC Enterprise.CreateProductSetting @ProductId, @ProductSettingTypeId, 'http://rcqacbwwngx005.realpage.com/api', NULL, NULL, @ProductSettingId OUTPUT;
END;
	
IF @@SERVERNAME IN('RCPGBKDBSQL005A', 'RCPGBKDBSQL005B')
BEGIN
	EXEC Enterprise.CreateProductSetting @ProductId, @ProductSettingTypeId, 'http://rcqacbwwngx005.realpage.com/api', NULL, NULL, @ProductSettingId OUTPUT;
END;

EXEC Enterprise.LinkProductSettingToConfiguration @ConfigurationId, @ProductSettingId, NULL, NULL;

SET @ProductSettingTypeId = NULL;
SET @ProductSettingId = NULL;
GO

/*******************************
GB 1000 - Bug 93172
*******************************/
IF EXISTS (SELECT 1 FROM Enterprise.Product WHERE Name = 'Resident Portal')
BEGIN
	UPDATE Enterprise.Product
		SET Name = 'Resident Portals'
	WHERE Name = 'Resident Portal'
END


/*****************************
Fix CLone issues if there are any
*****************************/
--SELECT DISTINCT OrganizationPartyID, 0 PStatus  FROM Person.Persona
/*
select * from enterprise.action
select * from enterprise.useractions

*/


DECLARE @OrgRowNum INT
Declare @ActionID INT
DECLARE @RightID INT
DECLARE @RoleID INT
DECLARE @Status INT
DECLARE @ActionValueID INT
DECLARE @OrgID INT
DECLARE @ProductID INT
DECLARE @ParentActionId INT
DECLARE @UserActionId INT

IF OBJECT_ID('tempdb..#HoldOrgs') IS NOT NULL
    DROP TABLE #HoldOrgs

SELECT DISTINCT IDENTITY(INT, 1,1) RowNumber, OrganizationPartyID, 0 PStatus INTO #HoldOrgs FROM Person.Persona-- WHERE Person.Persona.OrganizationPartyId = 353


SELECT @Status = StatusType.StatusTypeID
FROM Enterprise.StatusTypeCategoryType
JOIN Enterprise.StatusTypeCategory ON StatusTypeCategory.StatusTypeCategoryTypeId = StatusTypeCategoryType.StatusTypeCategoryTypeId
JOIN Enterprise.StatusTypeCategoryClassification ON StatusTypeCategoryClassification.StatusTypeCategoryId = StatusTypeCategory.StatusTypeCategoryId
JOIN Enterprise.StatusType ON StatusType.StatusTypeId = StatusTypeCategoryClassification.StatusTypeId
	   WHERE StatusType.name = 'ALL'
		  AND StatusTypeCategoryType.Name = 'Security'

--SET @OrgID = 350;
WHILE EXISTS(SELECT 1 FROM #HoldOrgs WHERE PStatus = 0)
BEGIN
    SELECT TOP 1 @OrgRowNum = Rownumber, @OrgID = OrganizationPartyID 
	   FROM #HoldOrgs 
         WHERE PStatus = 0


    SELECT @ActionValueID = [ActionValueTypeID] FROM Enterprise.ActionValueType WHERE Value = 'ROUTE'
    SELECT @ProductID = ProductId FROM Enterprise.Product WHERE Name = 'Unified Login'

    IF NOT EXISTS (SELECT 1 FROM Enterprise.Action WHERE ObjectValue = 'CloneUser'and Description = 'SuperUser')
    BEGIN
		  EXEC [Enterprise].[CreateAction] @ProductID = @ProductId, @Action = N'CloneUser', @ActionTarget = N'Route', @ActionbValueTypeId = @ActionValueID, @Description = 'SuperUser', @ActionID = @ActionID OUTPUT
		  SELECT	@ActionID as N'@ActionID'
    END

    SELECT @ParentActionId = ActionID  FROM Enterprise.Action Where ObjectValue = 'CloneUser' and ParentActionID is NULL and Description = 'SuperUser'
    IF NOT EXISTS (SELECT 1 FROM Enterprise.Action WHERE ObjectValue = 'Edit User' and ParentActionID = @ParentActionId)
    BEGIN
			 EXEC [Enterprise].[CreateAction] @ProductID = @ProductId, @Action = N'Edit User', @ActionTarget = N'Right', @ActionbValueTypeId = @ActionValueID, @Description = '', @ParentActionID = @ParentActionId, @ActionID = @ActionID OUTPUT
			 SELECT	@ActionID as N'@ActionID'
    END



    SELECT @ActionID = ActionID FROM Enterprise.Action WHERE ObjectValue = 'CloneUser' and ObjectType = 'ROUTE' and Description = 'SuperUser'
    SELECT @RoleID = RoleID FROM Enterprise.Role R INNER JOIN Enterprise.RoleValueType RR on RR.RoleValueTypeId = R.RoleValueTypeId WHere RR.value IN ('User Administrator') AND PartyID = @OrgID
    SELECT @RightID = RightId FROM Enterprise.[Right] R INNER JOIN Enterprise.RightValueType RR ON RR.RightValueTypeId = R.RightValueTypeId WHERE Value = 'Ability to clone users' and RoleId = @RoleID
    EXEC	[Enterprise].[LinkActionToRights]  @ActionID = @ActionID, @RightId = @RightId,	@StatusId = @Status,	@UserActionId = @UserActionId OUTPUT

    UPDATE #HoldOrgs
    SET
       #HoldOrgs.PStatus = 1
    WHERE RowNumber = @OrgRowNum
END
GO

DECLARE @OrgRowNum INT;
DECLARE @ActionID INT;
DECLARE @RightID INT;
DECLARE @RoleID INT;
DECLARE @Status INT;
DECLARE @ActionValueID INT;
DECLARE @OrgID INT;
DECLARE @ProductID INT;
DECLARE @ParentActionId INT;
DECLARE @UserActionId INT;
DECLARE @RightCategoryId INT;
DECLARE @RightValueTypeId INT;
SELECT DISTINCT
       IDENTITY( INT, 1, 1) AS RowNumber,
       OrganizationPartyID,
       0 AS PStatus
INTO #HoldOrgsUserRoutes
FROM Person.Persona;-- WHERE Person.Persona.OrganizationPartyId = 353

SELECT @Status = StatusType.StatusTypeID
FROM Enterprise.StatusTypeCategoryType
     JOIN Enterprise.StatusTypeCategory ON StatusTypeCategory.StatusTypeCategoryTypeId = StatusTypeCategoryType.StatusTypeCategoryTypeId
     JOIN Enterprise.StatusTypeCategoryClassification ON StatusTypeCategoryClassification.StatusTypeCategoryId = StatusTypeCategory.StatusTypeCategoryId
     JOIN Enterprise.StatusType ON StatusType.StatusTypeId = StatusTypeCategoryClassification.StatusTypeId
WHERE StatusType.name = 'ALL'
      AND StatusTypeCategoryType.Name = 'Security';
SELECT @RightCategoryId = StatusType.StatusTypeID
FROM Enterprise.StatusTypeCategoryType
     JOIN Enterprise.StatusTypeCategory ON StatusTypeCategory.StatusTypeCategoryTypeId = StatusTypeCategoryType.StatusTypeCategoryTypeId
     JOIN Enterprise.StatusTypeCategoryClassification ON StatusTypeCategoryClassification.StatusTypeCategoryId = StatusTypeCategory.StatusTypeCategoryId
     JOIN Enterprise.StatusType ON StatusType.StatusTypeId = StatusTypeCategoryClassification.StatusTypeId
WHERE StatusType.name = 'DEfault'
      AND StatusTypeCategory.Name = 'Right Type'
      AND StatusTypeCategoryType.Name = 'Security';
IF NOT EXISTS
(
    SELECT 1
    FROM Enterprise.ACTION
    WHERE ObjectValue = 'CloneUser'
          AND ParentActionId IS NULL
          AND Description = 'User'
)
    BEGIN
        EXEC [Enterprise].[CreateAction]
             @ProductID = @ProductId,
             @Action = N'CloneUser',
             @ActionTarget = N'Route',
             @ActionbValueTypeId = @ActionValueID,
             @Description = 'Cloneuser Route for custom roles.',
             @ActionID = @ActionID OUTPUT;
        SELECT @ActionID AS N'@ActionID';
    END;
WHILE EXISTS
(
    SELECT 1
    FROM #HoldOrgsUserRoutes
    WHERE PStatus = 0
)
    BEGIN
        SELECT TOP 1 @OrgRowNum = Rownumber,
                     @OrgID = OrganizationPartyID
        FROM #HoldOrgsUserRoutes
        WHERE PStatus = 0;
        IF NOT EXISTS
(
    SELECT 1
    FROM Enterprise.Role
    WHERE RoleId = -1
)
            BEGIN
                SET IDENTITY_INSERT Enterprise.Role ON;
                INSERT INTO Enterprise.Role
(RoleId,
 RoleTypeId,
 PartyId,
 RoleValueTypeId
)
                VALUES
(-1,
 0,
 @OrgId,
 -1
);
                SET IDENTITY_INSERT Enterprise.Role OFF;
            END;
        EXECUTE Enterprise.CreateRight
                @RoleId = -1,
                @RightName = 'Default_CloneUser_Route',
                @ShortName = 'CloneUserRoute',
                @RightCategoryId = @RightCategoryId,
                @PartyId = @OrgId,
                @ProductId = @ProductId,
                @Description = 'Edit User',
                @RightId = @RightId OUTPUT;
        SELECT @ActionID = ActionID
        FROM Enterprise.ACTION
        WHERE ObjectValue = 'CloneUser'
              AND ObjectType = 'Route'
              AND ParentActionId IS NULL;
        EXEC [Enterprise].[LinkActionToRights]
             @ActionID = @ActionID,
             @RightId = @RightId,
             @StatusId = @Status,
             @UserActionId = @UserActionId OUTPUT;
        UPDATE #HoldOrgsUserRoutes
          SET
              PStatus = 1
        WHERE RowNumber = @OrgRowNum;
    END;
DECLARE @CloneUserRoute INT;
SELECT @CloneUserRoute = RightValueTypeId
FROM Enterprise.RightValueType
WHERE value = 'Default_CloneUser_Route';
SELECT @RightValueTypeId = RightValueTypeId
FROM Enterprise.RightValueType
WHERE value IN('Ability to Clone Users');
IF NOT EXISTS
(
    SELECT 1
    FROM Enterprise.RightDependency
    WHERE RightValueTypeId = @RightValueTypeId
          AND DependentRightValueTypeId = @CloneUserRoute
)
    BEGIN
        INSERT INTO Enterprise.RightDependency
(RightValueTypeId,
 DependentRightValueTypeId
)
        VALUES
(@RightValueTypeId,
 @CloneUserRoute
);
    END;
	GO

/*****************************
Setup defualt routes
*****************************/

DECLARE @ResendInvite INT;
DECLARE @Lock INT;
DECLARE @OrgRowNum INT;
DECLARE @ActionID INT;
DECLARE @RightID INT;
DECLARE @RoleID INT;
DECLARE @Status INT;
DECLARE @ActionValueID INT;
DECLARE @OrgID INT;
DECLARE @ProductID INT;
DECLARE @ParentActionId INT;
DECLARE @UserActionId INT;
DECLARE @RightCategoryId INT;
DECLARE @PartyId INT;
DECLARE @RightName VARCHAR(100);
DECLARE @RVT INT;
DECLARE @DefaultRoute NVARCHAR(200);
DECLARE @RightValueTypeId INT;
DECLARE @StatusId INT;
DECLARE @PersonaId INT;
DECLARE @FromDate DATETIME;
DECLARE @TRoleId INT;
DECLARE @TRoleName NVARCHAR(500);
DECLARE @TRoleDesc NVARCHAR(500);
DECLARE @TRightId INT;
DECLARE @TRightName NVARCHAR(500);
DECLARE @TRightDesc NVARCHAR(500);
DECLARE @RightCategory INT;
DECLARE @RoleCategory INT;
DECLARE @RoleName NVARCHAR(500);
DECLARE @RoleTypeID INT;
DECLARE @PerosonaP INT;
DECLARE @PartyRowNum INT;
DECLARE @TRightShortName varchar(100);
SET @FromDate = GETUTCDATE();

IF OBJECT_ID('tempdb..#NewRight') IS NOT NULL
BEGIN
	DROP TABLE #NewRight;
END;

IF OBJECT_ID('tempdb..#HoldPartyFornewRights') IS NOT NULL
BEGIN
	DROP TABLE #HoldPartyFornewRights;
END;


CREATE TABLE #NewRight
( 
			 RightId int identity(1,1), Name nvarchar(500), description nvarchar(500), shortname varchar(100)
);

INSERT INTO #NewRight(name, description, shortname )
SELECT Value, Description, ShortName FROM Enterprise.RightValueType WHERE value In (N'Default_Sidemenu_People', N'Default_Sidemenu_RoleAndRights', N'Default_UserList', N'Default_ViewUser', N'Default_CloneUser', N'Default_AddUser', N'Default_Createuser', N'Default_EditUser', N'Default_RolesAndRights', N'Default_ViewRoles', N'Default_ViewRights', N'Default_EditUser_Route', N'Default_EditOwnProfile', N'Default_ResendInvitation', N'Default_ActivateDeActivateUser', N'Default_ProductLearningPortal', N'Default_MigrationTool_Route', N'Default_MigrationTool_Right', N'Default_ViewCompanyPage', N'Default_EditCompany', N'Default_ViewProperty', N'Default_EditProperty', N'Default_LockUnLockUser', N'Default_ViewAuditTrailData', N'Default_ViewRealPageProducts', N'Default_CloneUser_Route', N'Default_EditOtherUserProfile', N'Default_AddEditResidentPortalUser', N'Default_ViewUnifiedSetting', N'Default_ManageUnifiedSetting', N'Default_Sidemenu_UnifiedSetting', N'Default_SupportToolRight', N'Default_SupportToolRoute', N'Default_EmployeeAccess', N'Default_AccessToUnifiedSettings', N'Default_AccessToUnifiedPlatform')


SELECT @ProductId = ProductId
FROM Enterprise.Product
WHERE name = 'Unified Platform';

SELECT @RightCategory = ST.StatusTypeId
FROM Enterprise.StatusTypeCategoryType AS STCT
	 JOIN
	 Enterprise.StatusTypeCategory AS STC
	 ON STCT.StatusTypeCategoryTypeId = STC.StatusTypeCategoryTypeId
	 JOIN
	 Enterprise.StatusTypeCategoryClassification AS STCC
	 ON STCC.StatusTypeCategoryId = STC.StatusTypeCategoryId
	 JOIN
	 Enterprise.StatusType AS ST
	 ON ST.StatusTypeId = STCC.StatusTypeId
WHERE STC.Name = 'Right Type' AND 
	  ST.Name = 'Default';


SELECT DISTINCT 
	   IDENTITY(int, 1, 1) AS RowNumber, OrganizationPartyID, 0 AS PStatus
INTO #HoldPartyFornewRights
FROM Person.Persona;

WHILE EXISTS
(
	SELECT 1
	FROM #HoldPartyFornewRights
	WHERE PStatus = 0
)
BEGIN
	SELECT TOP 1 @PartyRowNum = Rownumber, @PartyId = OrganizationPartyID
	FROM #HoldPartyFornewRights
	WHERE PStatus = 0;

	DECLARE Rights CURSOR
	FOR SELECT RightId, Name, Description, ShortName
		FROM #NewRight;
	OPEN Rights;
	FETCH Rights INTO @TRightId, @TRightName, @TRightDesc, @TRightShortName;
	WHILE @@FETCH_STATUS = 0
	BEGIN
		
		EXECUTE Enterprise.CreateRight @RoleId = -1, @RightName = @TRightName, @RightCategoryId = @RightCategory, @PartyId = @PartyId, @ProductId = @ProductId, @Shortname = @TRightShortName, @Description = @TRightDesc, @RightId = @RightId OUTPUT;
		--select @RoleId,  @TRightName, @RightCategory, @PartyId,  @TRightShortName,  @TRightDesc		
		FETCH Rights INTO @TRightId, @TRightName, @TRightDesc, @TRightShortName;
	END;
	CLOSE Rights;
	DEALLOCATE Rights;
	UPDATE #HoldPartyFornewRights
	  SET PStatus = 1
	WHERE RowNumber = @PartyRowNum;
END;
GO

/*****************************
FIx default rights
*****************************/

DECLARE @ResendInvite INT;
DECLARE @Lock INT;
DECLARE @OrgRowNum INT;
DECLARE @ActionID INT;
DECLARE @RightID INT;
DECLARE @RoleID INT;
DECLARE @Status INT;
DECLARE @ActionValueID INT;
DECLARE @OrgID INT;
DECLARE @ProductID INT;
DECLARE @ParentActionId INT;
DECLARE @UserActionId INT;
DECLARE @RightCategoryId INT;
DECLARE @PartyId INT;
DECLARE @RightName VARCHAR(100);
DECLARE @RVT INT;
DECLARE @DefaultRoute NVARCHAR(200);
DECLARE @RightValueTypeId INT;
DECLARE @StatusId INT;
DECLARE @PersonaId INT;
DECLARE @FromDate DATETIME;
DECLARE @TRoleId INT;
DECLARE @TRoleName NVARCHAR(500);
DECLARE @TRoleDesc NVARCHAR(500);
DECLARE @TRightId INT;
DECLARE @TRightName NVARCHAR(500);
DECLARE @TRightDesc NVARCHAR(500);
DECLARE @RightCategory INT;
DECLARE @RoleCategory INT;
DECLARE @RoleName NVARCHAR(500);
DECLARE @RoleTypeID INT;
DECLARE @PerosonaP INT;
DECLARE @PartyRowNum INT;
DECLARE @TRightShortName varchar(100);
SET @FromDate = GETUTCDATE();

IF OBJECT_ID('tempdb..#NewRight') IS NOT NULL
BEGIN
	DROP TABLE #NewRight;
END;

IF OBJECT_ID('tempdb..#HoldPartyFornewRights') IS NOT NULL
BEGIN
	DROP TABLE #HoldPartyFornewRights;
END;


CREATE TABLE #NewRight
( 
			 RightId int, Name nvarchar(500), description nvarchar(500), shortname varchar(100)
);

INSERT INTO #NewRight( rightid, name, description, shortname )
VALUES( 1, 'Ability to edit password', 'Ability to edit password', 'EditPassword' ),
	(2, 'Ability to manage roles and rights', 'Ability to manage roles and rights', 'ManageRoleRight'),
	(3, 'Ability to Resend Invite', 'Ability to Resend Invite', 'resendinvitation'),
	(4, 'Ability to Activate/Deactivate User', 'Ability to Activate/Deactivate User', 'activatedeactivateusers'),
	(5, 'Ability to Migrate Users', 'Ability to Migrate Users', 'MigrationTool')

SELECT @ProductId = ProductId
FROM Enterprise.Product
WHERE name = 'Unified Platform';

SELECT @RightCategory = ST.StatusTypeId
FROM Enterprise.StatusTypeCategoryType AS STCT
	 JOIN
	 Enterprise.StatusTypeCategory AS STC
	 ON STCT.StatusTypeCategoryTypeId = STC.StatusTypeCategoryTypeId
	 JOIN
	 Enterprise.StatusTypeCategoryClassification AS STCC
	 ON STCC.StatusTypeCategoryId = STC.StatusTypeCategoryId
	 JOIN
	 Enterprise.StatusType AS ST
	 ON ST.StatusTypeId = STCC.StatusTypeId
WHERE STC.Name = 'Right Type' AND 
	  ST.Name = 'Default';


SELECT DISTINCT 
	   IDENTITY(int, 1, 1) AS RowNumber, OrganizationPartyID, 0 AS PStatus
INTO #HoldPartyFornewRights
FROM Person.Persona;

WHILE EXISTS
(
	SELECT 1
	FROM #HoldPartyFornewRights
	WHERE PStatus = 0
)
BEGIN
	SELECT TOP 1 @PartyRowNum = Rownumber, @PartyId = OrganizationPartyID
	FROM #HoldPartyFornewRights
	WHERE PStatus = 0;
	SELECT @RoleId = RoleId
	FROM Enterprise.Role AS R
		 INNER JOIN
		 Enterprise.RoleValueType AS RR
		 ON RR.RoleValueTypeId = R.RoleValueTypeId
	WHERE RR.Value = 'User Administrator' AND 
		  R.PartyId = @PartyId;
	DECLARE Rights CURSOR
	FOR SELECT RightId, Name, Description, ShortName
		FROM #NewRight;
	OPEN Rights;
	FETCH Rights INTO @TRightId, @TRightName, @TRightDesc, @TRightShortName;
	WHILE @@FETCH_STATUS = 0
	BEGIN
		SELECT @RoleId = RoleId
		FROM Enterprise.Role AS R
			 INNER JOIN
			 Enterprise.RoleValueType AS RR
			 ON RR.RoleValueTypeId = R.RoleValueTypeId
		WHERE RR.Value = 'User Administrator' AND 
			  R.PartyId = @PartyId;
		EXECUTE Enterprise.CreateRight @RoleId = @RoleId, @RightName = @TRightName, @RightCategoryId = @RightCategory, @PartyId = @PartyId, @ProductId = @ProductId, @Shortname = @TRightShortName, @Description = @TRightDesc, @RightId = @RightId OUTPUT;
		--select @RoleId,  @TRightName, @RightCategory, @PartyId,  @TRightShortName,  @TRightDesc		
		FETCH Rights INTO @TRightId, @TRightName, @TRightDesc, @TRightShortName;
	END;
	CLOSE Rights;
	DEALLOCATE Rights;
	UPDATE #HoldPartyFornewRights
	  SET PStatus = 1
	WHERE RowNumber = @PartyRowNum;
END;

GO
/*****************************
Roles and Right 
(Thats going to production)
*****************************/

DECLARE @VisibilityStatus INT  
DECLARE @RightTypeId INT         
UPDATE RVT              
              SET RVT.ProductId =  A.ProductId,
				  RVT.VisibilityStatusId = UA.Status 
        FROM Enterprise.RightValueType RVT
            INNER JOIN  Enterprise.[Right] R ON R.RightValueTypeId = RVT.RightValueTypeId
       INNER JOIN Enterprise.UserActions UA ON UA.RightId = R.RightId
            INNER JOIN Enterprise.ACTION A ON A.ActionId = UA.ActionId
			INNER JOIN Enterprise.StatusType ST on ST.StatusTypeId = UA.Status
		WHERE  UA.status not in (13, 14)
			
SET @VisibilityStatus = NULL
SELECT @VisibilityStatus = StatusType.StatusTypeID
         FROM Enterprise.StatusTypeCategoryType
              JOIN Enterprise.StatusTypeCategory ON StatusTypeCategory.StatusTypeCategoryTypeId = StatusTypeCategoryType.StatusTypeCategoryTypeId
              JOIN Enterprise.StatusTypeCategoryClassification ON StatusTypeCategoryClassification.StatusTypeCategoryId = StatusTypeCategory.StatusTypeCategoryId
              JOIN Enterprise.StatusType ON StatusType.StatusTypeId = StatusTypeCategoryClassification.StatusTypeId
         WHERE StatusType.name = 'ALL'
               AND StatusTypeCategoryType.Name = 'Security';

UPDATE Enterprise.RightValueType
SET ProductId = 3,
	VisibilityStatusId = @VisibilityStatus
WHERE  Value IN 
	(
	'Ability to Activate/Deactivate User'
	,'Ability to clone users'
	,'Ability to create and manage users for Resident Portals'
	,'Ability to create users'
	,'Ability to edit my own profile'
	,'Ability to edit password'
	,'Ability to edit profile of other users'
	,'Ability to edit users'
	,'Ability to lock/unlock users'
	,'Ability to Migrate Users'
	,'Ability to Resend Invite'
	,'Ability to view audit trail on user data'
	,'Ability to view users'
	,'Access to Product Learning Portal'
	,'Ability to manage roles and rights'
	,'Ability to view roles and rights'
	)

SET @VisibilityStatus = NULL
SELECT  @VisibilityStatus = StatusType.StatusTypeID
        FROM Enterprise.StatusTypeCategoryType
            JOIN Enterprise.StatusTypeCategory ON StatusTypeCategory.StatusTypeCategoryTypeId = StatusTypeCategoryType.StatusTypeCategoryTypeId
            JOIN Enterprise.StatusTypeCategoryClassification ON StatusTypeCategoryClassification.StatusTypeCategoryId = StatusTypeCategory.StatusTypeCategoryId
            JOIN Enterprise.StatusType ON StatusType.StatusTypeId = StatusTypeCategoryClassification.StatusTypeId
        WHERE StatusType.name = 'Internal Only'
            AND StatusTypeCategoryType.Name = 'Security';

UPDATE Enterprise.RightValueType
SET ProductId = 3,
	VisibilityStatusId = @VisibilityStatus
WHERE  Value IN 
	(
	'Access to Property Photos'
	,'Access to Unified Platform via Support Tool'
	,'Access to Unified Settings via Support Tool'
	,'Ability to Configure Custom Fields for Users'
	,'View Unified Settings'
	,'Ability to Manage Settings for Unified Platform'
	,'Ability to manage Unified Settings'
	)

SELECT @RightTypeId =  StatusType.StatusTypeID
         FROM Enterprise.StatusTypeCategoryType
              JOIN Enterprise.StatusTypeCategory ON StatusTypeCategory.StatusTypeCategoryTypeId = StatusTypeCategoryType.StatusTypeCategoryTypeId
              JOIN Enterprise.StatusTypeCategoryClassification ON StatusTypeCategoryClassification.StatusTypeCategoryId = StatusTypeCategory.StatusTypeCategoryId
              JOIN Enterprise.StatusType ON StatusType.StatusTypeId = StatusTypeCategoryClassification.StatusTypeId
         WHERE StatusType.name = 'Default'
				AND Enterprise.StatusTypeCategory.Name = 'Role Type'
               AND StatusTypeCategoryType.Name = 'Security';
UPDATE Enterprise.RightValueType
	SET StatusTypeId = @RightTypeId 
WHERE StatusTypeId IS NULL

SET @VisibilityStatus = NULL
SELECT  @VisibilityStatus = StatusType.StatusTypeID
        FROM Enterprise.StatusTypeCategoryType
            JOIN Enterprise.StatusTypeCategory ON StatusTypeCategory.StatusTypeCategoryTypeId = StatusTypeCategoryType.StatusTypeCategoryTypeId
            JOIN Enterprise.StatusTypeCategoryClassification ON StatusTypeCategoryClassification.StatusTypeCategoryId = StatusTypeCategory.StatusTypeCategoryId
            JOIN Enterprise.StatusType ON StatusType.StatusTypeId = StatusTypeCategoryClassification.StatusTypeId
        WHERE StatusType.name = 'Hidden'
            AND StatusTypeCategoryType.Name = 'Security';

UPDATE Enterprise.RightValueType
SET ProductId = 3,
	VisibilityStatusId = @VisibilityStatus
WHERE  Value IN 
	(
	'Access to Green Book Migration Tool'
	)

GO

-- GB-989 - Update product descriptions

DECLARE @ProductId INT

SELECT @ProductId = ProductId FROM Enterprise.Product WHERE Name = 'Business Intelligence'
UPDATE Enterprise.Product 
SET Description = 'Business Intelligence provides portfolio reporting spanning multiple source systems ' +
				  'which serves up key business metrics with a front end business analytics tool.  ' +
				  'Data has been normalized into business models to improve reporting quality.'
WHERE ProductId = @ProductId

SELECT @ProductId = ProductId FROM Enterprise.Product WHERE Name = 'Performance Analytics'
UPDATE Enterprise.Product 
SET Description = 'Performance Analytics provides real-time cross platform analytics on key revenue ' + 
				  'and operating metrics, with the option to include Benchmarking of asset-level ' +
				  'performance leveraging RealPage’s broad data resources.'
WHERE ProductId = @ProductId

SELECT @ProductId = ProductId FROM Enterprise.Product WHERE Name = 'Investment Analytics'
UPDATE Enterprise.Product 
SET Description = 'Investment Analytics provides on-demand tools to track market fundamentals and capital ' + 
				  'markets from a market level down to submarket and zip code in more than 150 metros ' + 
				  'across the country.'
WHERE ProductId = @ProductId

SELECT @ProductId = ProductId FROM Enterprise.Product WHERE Name = 'Revenue Management'
UPDATE Enterprise.Product 
SET Description = 'YieldStar Price Optimizer provides daily unit-level pricing adjustments based on ' +
				  'forecasted supply and demand, employing our exclusive, proprietary YieldStar Market ' + 
				  'Response Model coupled with robust decision support tools.'
WHERE ProductId = @ProductId

SELECT @ProductId = ProductId FROM Enterprise.Product WHERE Name = 'Axiometrics'
UPDATE Enterprise.Product 
SET Description = 'AxioApartment offers market research, asset details and rent history for conventional ' + 
				  'multifamily properties. '
WHERE ProductId = @ProductId
GO

-- GB-1113 - Correct text for OneSite description

DECLARE @ProductId INT

SELECT @ProductId = ProductId FROM Enterprise.Product WHERE Name = 'OneSite'
UPDATE Enterprise.Product 
SET Description = 'The OneSite environment provides access to Leasing and Rents, Facilities, Purchasing, and ' +
				  'Document Management for your properties, depending upon the mix of products which are licensed.'
WHERE ProductId = @ProductId
GO

-- GB-1110 - Update new user email template for indefinite time frame

DECLARE @CommunicationEventAudienceTypeId INT
DECLARE @CommunicationEventPurposeTypeId INT

SELECT @CommunicationEventAudienceTypeId = CommunicationEventAudienceTypeId
FROM Enterprise.CommunicationEventAudienceType
WHERE [Description] = 'Regular User'

SELECT @CommunicationEventPurposeTypeId = CommunicationEventPurposeTypeId
FROM Enterprise.CommunicationEventPurposeType
WHERE [Description] = 'New User Setup'

UPDATE Enterprise.CommunicationEmailTemplate
SET Body = REPLACE(Body, '72 hours', 'a limited amount of time')
WHERE CommunicationEventAudienceTypeId = @CommunicationEventAudienceTypeId
	AND CommunicationEventPurposeTypeId = @CommunicationEventPurposeTypeId
GO

-- GB-1111 - Changing configuration for new user expiration from 72 hours to 1 week

DECLARE @ActivityId INT

SELECT @ActivityId = ActivityId FROM Ident.Activity WHERE ActivityCode = 'NewUserRegistration'

UPDATE Ident.Activity 
SET [Description] = REPLACE([Description], '72 hrs', '7 days'),
	ActivityTokenExpirationMinutes = 10080 -- 60 mins x 24 hrs x 7 days = 10080 mins
WHERE ActivityId = @ActivityId

SELECT @ActivityId = ActivityId FROM Ident.Activity WHERE ActivityCode = 'NewUserRegistrationVerification'

UPDATE Ident.Activity 
SET [Description] = REPLACE([Description], '72 hrs', '7 days'),
	ActivityTokenExpirationMinutes = 10080 -- 60 mins x 24 hrs x 7 days = 10080 mins
WHERE ActivityId = @ActivityId
GO

--DELETE Old Migration RIght
DECLARE @rvt INT;
SELECT @rvt = RightValueTypeId
FROM Enterprise.RightValueType
WHERE Value = 'Access to Unified Login Migration Tool';
DELETE FROM Enterprise.[Right]
WHERE RightValueTYpeId = @rvt;
DELETE FROM Enterprise.RIghtValueTYpe
WHERE RightValueTypeId = @rvt;

GO
update enterprise.action set actionvaluetypeid = 1 where actionvaluetypeid = 4
GO
--UPDATE ShortNames
UPDATE Enterprise.RightValueType
  SET
      ShortName = 'EditOwnProfile'
WHERE value = 'Ability to edit my own profile';
UPDATE Enterprise.RightValueType
  SET
      ShortName = 'AccessIDPTool'
WHERE value = 'Access to Identity Provider Configuration Page';
UPDATE Enterprise.RightValueType
  SET
      ShortName = 'AccessLeasingRentTool'
WHERE value = 'Access to Leasing & Rents Conversion Tool';
UPDATE Enterprise.RightValueType
  SET
      ShortName = 'ProductLearningPortal'
WHERE value = 'Access to Product Learning Portal';
UPDATE Enterprise.RightValueType
  SET
      ShortName = 'AccessPropertyHierarchyTool'
WHERE value = 'Access to Property Hierarchy Tool';
UPDATE Enterprise.RightValueType
  SET
      ShortName = 'CloneUser'
WHERE value = 'Ability to clone users';
UPDATE Enterprise.RightValueType
  SET
      ShortName = 'CreateUser'
WHERE value = 'Ability to create users';
UPDATE Enterprise.RightValueType
  SET
      ShortName = 'EditPassword'
WHERE value = 'Ability to edit password';
UPDATE Enterprise.RightValueType
  SET
      ShortName = 'EditOtherProfile'
WHERE value = 'Ability to edit profile of other users';
UPDATE Enterprise.RightValueType
  SET
      ShortName = 'EditUsers'
WHERE value = 'Ability to edit users';
UPDATE Enterprise.RightValueType
  SET
      ShortName = 'ImersonateUser'
WHERE value = 'Impersonate a User';
UPDATE Enterprise.RightValueType
  SET
      ShortName = 'LockUnlockUsers'
WHERE value = 'Ability to lock/unlock users';
UPDATE Enterprise.RightValueType
  SET
      ShortName = 'ManageRoleRight'
WHERE value = 'Ability to manage roles and rights';
UPDATE Enterprise.RightValueType
  SET
      ShortName = 'SeeAllProducts'
WHERE value = 'See All RealPage Products';
UPDATE Enterprise.RightValueType
  SET
      ShortName = 'ViewAuditTrailProfileData'
WHERE value = 'View Audit Trail on Profile Data';
UPDATE Enterprise.RightValueType
  SET
      ShortName = 'ViewAuditTrailUserData'
WHERE value = 'Ability to view audit trail on user data';
UPDATE Enterprise.RightValueType
  SET
      ShortName = 'ViewRoleRight'
WHERE value = 'Ability to view roles and rights';
UPDATE Enterprise.RightValueType
  SET
      ShortName = 'ViewUsers'
WHERE value = 'Ability to view users';
UPDATE Enterprise.RightValueType
  SET
      ShortName = 'CloneUser'
WHERE value = 'Default_CloneUser';
UPDATE Enterprise.RightValueType
  SET
      ShortName = 'MigrationTool'
WHERE value = 'Ability to Migrate Users';
UPDATE Enterprise.RightValueType
  SET
      ShortName = 'ViewCompanyPage'
WHERE value = 'Ability to view Company page';
UPDATE Enterprise.RightValueType
  SET
      ShortName = 'EditCompanyPage'
WHERE value = 'Ability to edit Company information';
UPDATE Enterprise.RightValueType
  SET
      ShortName = 'ViewProperty'
WHERE value = 'Ability to view Property page';
UPDATE Enterprise.RightValueType
  SET
      ShortName = 'EditProperty'
WHERE value = 'Ability to edit Property information';
UPDATE Enterprise.RightValueType
  SET
      ShortName = 'activatedeactivateusers'
WHERE value = 'Ability to Activate/Deactivate User';
UPDATE Enterprise.RightValueType
  SET
      ShortName = 'resendinvitation'
WHERE value = 'Ability to Resend Invite';
UPDATE Enterprise.RightValueType
  SET
      ShortName = 'AddEditResidentPortalUser'
WHERE value = 'Ability to create and manage users for Resident Portals';
UPDATE Enterprise.RightValueType
  SET
      ShortName = 'ViewUnifiedSettings'
WHERE value = 'View Unified Settings';
UPDATE Enterprise.RightValueType
  SET
      ShortName = 'ManageUnifiedSettings'
WHERE value = 'Ability to Manage Settings for Unified Platform';
UPDATE Enterprise.RightValueType
  SET
      ShortName = 'ManageUnifiedSettings'
WHERE value = 'Ability to manage Unified Settings';
UPDATE Enterprise.RightValueType
  SET
      ShortName = 'AccessToUnifiedPlatform'
WHERE value = 'Access to Unified Platform via Support Tool';
UPDATE Enterprise.RightValueType
  SET
      ShortName = 'ConfigureCustomFields'
WHERE value = 'Ability to Configure Custom Fields for Users';
UPDATE Enterprise.RightValueType
  SET
      ShortName = 'AccessPropertyPhotos'
WHERE value = 'Access to Property Photos';
UPDATE Enterprise.RightValueType
  SET
      ShortName = 'AccessToUnifiedSettings'
WHERE value = 'Access to Unified Settings via Support Tool';
UPDATE enterprise.rightvaluetype
  SET
      ShortName = ''
WHERE ShortName IS NULL;
GO



--Update marketign center
DECLARE @ProductId INT
DECLARE @ProductSettingTypeId INT

SELECT @ProductId = ProductId FROM Enterprise.Product WHERE Name = 'Marketing Center'
SELECT @ProductSettingTypeId = ProductSettingTypeId FROM Enterprise.ProductSettingType WHERE Name = 'TitleId'

UPDATE Enterprise.ProductSetting
SET Value = 'Marketing Center'
WHERE ProductId = @ProductId 
       AND ProductSettingTypeId = @ProductSettingTypeId
GO