DECLARE @UnifiedSettingRoute INT
DECLARE @SidemenuPeople INT
DECLARE @UserList INT
--DECLARE @ViewUser INT
--DECLARE @Adduser INT
--DECLARE @CreateUser INT
--DECLARE @EditUser INT
--DECLARE @EditUserROute INT
DECLARE @ResidentPortal INT

DECLARE @viewCompany INT
DECLARE @ViewProperty INT
--DECLARE @RightValueTYpeId INT
DECLARE @RootRoleId INT
--DECLARE @PartyId INT
--DECLARE @PartyRoleTypeId INT

DECLARE @AuditTrail INT
--DECLARE @ResendInvite INT;
--DECLARE @Lock INT;
--DECLARE @OrgRowNum INT;
--DECLARE @ActionID INT;
--DECLARE @RightID INT;
--DECLARE @RoleID INT;
--DECLARE @Status INT;
--DECLARE @ActionValueID INT;
--DECLARE @OrgID INT;
--DECLARE @ProductID INT;
--DECLARE @ParentActionId INT;
--DECLARE @UserActionId INT;
--DECLARE @RightCategoryId INT;
--DECLARE @RightName VARCHAR(100);
--DECLARE @RVT INT;
--DECLARE @DefaultRoute NVARCHAR(200);
--DECLARE @StatusId INT;
--DECLARE @PersonaId INT;
--DECLARE @FromDate DATETIME;
--DECLARE @TRoleId INT;
--DECLARE @TRoleName NVARCHAR(500);
--DECLARE @TRoleDesc NVARCHAR(500);
--DECLARE @TRightId INT;
--DECLARE @TRightName NVARCHAR(500);
--DECLARE @TRightDesc NVARCHAR(500);
--DECLARE @RightCategory INT;
--DECLARE @RoleCategory INT;
--DECLARE @RoleName NVARCHAR(500);
--DECLARE @RoleTypeID INT;
--DECLARE @PerosonaP INT;
--DECLARE @PartyRowNum INT;
--DECLARE @TRightShortName varchar(100);
SET @FromDate = GETUTCDATE();

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
WHERE name = 'Unified Login';


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
		EXECUTE Enterprise.CreateRight @RoleId = @RoleId, @RightName = @TRightName, @RightCategoryId = @RightCategory, @PartyId = @PartyId, @ProductId = @ProductId, @Shortname = '', @Description = @TRightDesc, @RightId = @RightId OUTPUT;
		EXECUTE Enterprise.CreateRight @RoleId = -1, @RightName = 'Default_AddEditResidentPortalUser', @ShortName = 'AddEditResidentPortalUser', @RightCategoryId = @RightCategory, @PartyId = @PartyId, @ProductId = @ProductId, @Description = 'Manage Resident Portal Users', @RightId = @RightId OUTPUT;
		SELECT @RightId = RightId
			FROM Enterprise.[Right] R INNER JOIN Enterprise.RightValueType RR
				ON RR.RightValueTypeId = R.RightValueTypeId 
			WHERE RR.Value = 'Default_AddEditResidentPortalUser'
		SELECT @ActionID = ActionID
		FROM Enterprise.ACTION
		WHERE ObjectValue = 'Manage Resident Portal Users' AND 
			  ObjectType = 'Right' AND 
			  ParentActionId IS NULL;
		EXEC [Enterprise].[LinkActionToRights] @ActionID = @ActionID, @RightId = @RightId, @StatusId = @Status, @UserActionId = @UserActionId OUTPUT;
		SELECT @RightID = NULL, @ActionId = NULL
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
END
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
END
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
END
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
END
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
END
--->

--Audit trail Related chnages

SELECT @ProductId = ProductId FROM Enterprise.Product WHERE Name = 'Unified Login'
SELECT @ActionValueID =  ActionValueTypeID FROM Enterprise.ActionValueType WHERE value = 'Right'
SELECT @ParentActionId = ActionId
FROM Enterprise.ACTION
WHERE ObjectValue = 'EditUser'
      AND ObjectType = 'Route'
      AND Description = 'SuperUser';
IF NOT EXISTS
(
    SELECT 1
    FROM Enterprise.ACTION
    WHERE ObjectValue = 'View Audit Trail'
          AND ParentActionID = @ParentActionId
)
    BEGIN
        EXEC [Enterprise].[CreateAction]
             @ProductID = @ProductId,
             @Action = N'View Audit Trail',
             @ActionTarget = N'Right',
             @ActionbValueTypeId = @ActionValueID,
             @Description = '',
             @ParentActionID = @ParentActionId,
             @ActionID = @ActionID OUTPUT;
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



/*
SELECT @RootRoleId = PartyRoleTypeId 
	FROM Enterprise.RoleType WHERE Name = 'User Role'

IF NOT EXISTS (SELECT 1 FROM Enterprise.RoleType WHERE Name = 'External User' AND ParentPartyRoleTypeId = @RootRoleId)
BEGIN
	INSERT INTO Enterprise.RoleType (PartyRoleTypeId, ParentPartyRoleTypeId, Name)
	VALUES (405, @RootRoleId, 'External User')
END

SET @PartyRoleTypeId = 405

DECLARE OrgName CURSOR FOR
	SELECT PartyId FROM Enterprise.Organization
OPEN OrgName
FETCH OrgName INTO @PartyId
WHILE @@FETCH_STATUS = 0
BEGIN
	IF NOT EXISTS (SELECT 1 FROM Enterprise.PartyROle where PartyId = @PartyId AND RoleTypeId = @PartyRoleTypeId)
	BEGIN
		INSERT INTO Enterprise.PartyRole (PartyId, RoleTypeId)
		VALUES (@PartyId, @PartyRoleTypeId)
	END
	FETCH OrgName INTO @PartyId
END
CLOSE OrgName
DEALLOCATE OrgName
*/

--Company and property related mapping

SELECT @ProductId = ProductId FROM Enterprise.Product WHERE Name = 'Unified Login'
SELECT @ActionValueID =  ActionValueTypeID FROM Enterprise.ActionValueType WHERE value = 'Right'
SELECT @ParentActionId = ActionId
FROM Enterprise.ACTION
WHERE ObjectValue = 'EditUser'
      AND ObjectType = 'Route'
      AND Description = 'SuperUser';
IF NOT EXISTS
(
    SELECT 1
    FROM Enterprise.ACTION
    WHERE ObjectValue = 'View Audit Trail'
          AND ParentActionID = @ParentActionId
)
    BEGIN
        EXEC [Enterprise].[CreateAction]
             @ProductID = @ProductId,
             @Action = N'View Audit Trail',
             @ActionTarget = N'Right',
             @ActionbValueTypeId = @ActionValueID,
             @Description = '',
             @ParentActionID = @ParentActionId,
             @ActionID = @ActionID OUTPUT;
        SELECT @ActionID AS N'@ActionID';
    END;
IF NOT EXISTS
(
    SELECT 1
    FROM Enterprise.ACTION
    WHERE ObjectValue = 'Property'
          AND ParentActionID = @ParentActionId
)
    BEGIN
        EXEC [Enterprise].[CreateAction]
             @ProductID = @ProductId,
             @Action = N'Property',
             @ActionTarget = N'Right',
             @ActionbValueTypeId = @ActionValueID,
             @Description = '',
             @ParentActionID = @ParentActionId,
             @ActionID = @ActionID OUTPUT;
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
VALUES( 1, 'View Unified Settings', 'Ability to View Unified Settings', 'ViewUnifiedSettings' )
, ( 2, 'Ability to Manage Settings for Unified Platform', 'Ability to Manage Settings for Unified Platform', 'ManageUnifiedSettings' );

SELECT @ProductId = ProductId
FROM Enterprise.Product
WHERE name = 'Unified Login';


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
FROM Person.Persona P
	INNER JOIN Enterprise.Organization O
		ON P.OrganizationPartyId = O.PartyId
	WHERE O.Name = 'RealPage Employee'
;

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
		EXECUTE Enterprise.CreateRight @RoleId = -1, @RightName = 'Default_ViewUnifiedSetting', @ShortName = 'ViewUnifiedSetting', @RightCategoryId = @RightCategory, @PartyId = @PartyId, @ProductId = @ProductId, @Description = 'View Unified Setting', @RightId = @RightId OUTPUT;
		EXECUTE Enterprise.CreateRight @RoleId = -1, @RightName = 'Default_ManageUnifiedSetting', @ShortName = 'ManageUnifiedSetting', @RightCategoryId = @RightCategory, @PartyId = @PartyId, @ProductId = @ProductId, @Description = 'Manage/View Unified Setting', @RightId = @RightId OUTPUT;
		EXECUTE Enterprise.CreateRight @RoleId = -1, @RightName = 'Default_Sidemenu_UnifiedSetting', @ShortName = 'UnifiedSettingRoute', @RightCategoryId = @RightCategory, @PartyId = @PartyId, @ProductId = @ProductId, @Description = 'Route for SideMenu', @RightId = @RightId OUTPUT;
		
		SELECT @RightId = RightId
			FROM Enterprise.[Right] R INNER JOIN Enterprise.RightValueType RR
				ON RR.RightValueTypeId = R.RightValueTypeId 
			WHERE RR.Value = 'Default_ViewUnifiedSetting'
		SELECT @ActionID = ActionID
		FROM Enterprise.ACTION
		WHERE ObjectValue = 'View Unified Settings' AND 
			  ObjectType = 'Right' AND 
			  ParentActionId IS NULL;
		EXEC [Enterprise].[LinkActionToRights] @ActionID = @ActionID, @RightId = @RightId, @StatusId = @Status, @UserActionId = @UserActionId OUTPUT;

		SELECT @RightId = RightId
			FROM Enterprise.[Right] R INNER JOIN Enterprise.RightValueType RR
				ON RR.RightValueTypeId = R.RightValueTypeId 
			WHERE RR.Value = 'Default_ManageUnifiedSetting'
		SELECT @ActionID = ActionID
		FROM Enterprise.ACTION
		WHERE ObjectValue = 'Manage Unified Settings' AND 
			  ObjectType = 'Right' AND 
			  ParentActionId IS NULL;
		EXEC [Enterprise].[LinkActionToRights] @ActionID = @ActionID, @RightId = @RightId, @StatusId = @Status, @UserActionId = @UserActionId OUTPUT;

		SELECT @RightId = RightId
			FROM Enterprise.[Right] R INNER JOIN Enterprise.RightValueType RR
				ON RR.RightValueTypeId = R.RightValueTypeId 
			WHERE RR.Value = 'Default_Sidemenu_UnifiedSetting'
		SELECT @ActionID = ActionID
		FROM Enterprise.ACTION
		WHERE ObjectValue = 'Unified Settings' AND 
			  ObjectType = 'Route' AND 
			  ParentActionId IS NULL;
		EXEC [Enterprise].[LinkActionToRights] @ActionID = @ActionID, @RightId = @RightId, @StatusId = @Status, @UserActionId = @UserActionId OUTPUT;

		SELECT @RightID = NULL, @ActionId = NULL
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
WHERE value IN( 'Ability to Manage Settings for Unified Platform' );

IF NOT EXISTS
(
	SELECT 1
	FROM Enterprise.RightDependency
	WHERE RightValueTypeId = @RightValueTypeId AND 
		  DependentRightValueTypeId = @UnifiedSettingRoute
)
BEGIN
	INSERT INTO Enterprise.RightDependency( RightValueTypeId, DependentRightValueTypeId )
	VALUES( @RightValueTypeId, @UnifiedSettingRoute )
END;

EXEC sys.sp_updateextendedproperty @name=N'Build', @value='62'