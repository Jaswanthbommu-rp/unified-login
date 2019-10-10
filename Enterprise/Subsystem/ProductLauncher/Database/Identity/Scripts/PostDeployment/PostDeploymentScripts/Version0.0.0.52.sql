

IF NOT EXISTS
(
	SELECT 1
	FROM Enterprise.ActionValueType
	WHERE Value = 'Right'
)
BEGIN
	INSERT INTO Enterprise.ActionValueType( Value, Description )
	VALUES( 'RIGHT', 'Right Name' );
END;

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
DECLARE @RightCategoryId INT;

--Delete Migration Tool right from QA,SAT and PROD

DECLARE @RightValueTypeId INT

SELECT @RightValueTypeId = RightValueTypeId FROM Enterprise.RightValueType WHERE Value = 'Access to Green Book Migration Tool'

IF EXISTS(SELECT 1 FROM Enterprise.[Right] WHERE RightValueTypeId = @RightValueTypeId)
BEGIN
	DELETE FROM Enterprise.[Right] WHERE RightValueTypeId = @RightValueTypeId
	DELETE FROM Enterprise.[RightValueType] WHERE RightValueTypeId = @RightValueTypeId
END


IF OBJECT_ID('tempdb..#HoldOrgsUserRoutes') IS NOT NULL
BEGIN
	DROP TABLE #HoldOrgsUserRoutes
END;

SELECT DISTINCT 
	   IDENTITY(int, 1, 1) AS RowNumber, OrganizationPartyID, 0 AS PStatus
INTO #HoldOrgsUserRoutes
FROM Person.Persona;-- WHERE Person.Persona.OrganizationPartyId = 353

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

IF NOT EXISTS
(
	SELECT 1
	FROM Enterprise.RoleValueType
	WHERE RoleValueTypeId = -1
)
BEGIN
	SET IDENTITY_INSERT Enterprise.RoleValueType ON;
	INSERT INTO Enterprise.RoleValueType( RoleValueTypeId, Value, StatusTypeId )
	VALUES( -1, 'SystemRight', @Status );
	SET IDENTITY_INSERT Enterprise.RoleValueType OFF;
END;

SELECT @ProductId = ProductId
FROM Enterprise.Product
WHERE Name = 'Unified Login';

SELECT @ActionValueID = [ActionValueTypeID]
FROM Enterprise.ActionValueType
WHERE Value = 'Right';



----->
IF NOT EXISTS
(
	SELECT 1
	FROM Enterprise.ACTION
	WHERE ObjectValue = 'Resend Invitation' AND 
		  ParentActionId IS NULL
)
BEGIN
	EXEC Enterprise.CreateAction @ProductID = @ProductId, @Action = N'Resend Invitation', @ActionTarget = N'Right', @ActionbValueTypeId = @ActionValueID, @Description = 'Display People in Menu bar', @ActionID = @ActionID OUTPUT;
	SELECT @ActionID AS N'@ActionID';
END;

IF NOT EXISTS
(
	SELECT 1
	FROM Enterprise.ACTION
	WHERE ObjectValue = 'Activate Deactivate User' AND 
		  ParentActionId IS NULL
)
BEGIN
	EXEC [Enterprise].[CreateAction] @ProductID = @ProductId, @Action = N'Activate Deactivate User', @ActionTarget = N'Right', @ActionbValueTypeId = @ActionValueID, @Description = 'Display Role and Rights in Menu bar', @ActionID = @ActionID OUTPUT;
	SELECT @ActionID AS N'@ActionID';
END;
----->

----->
IF NOT EXISTS
(
	SELECT 1
	FROM Enterprise.ACTION
	WHERE ObjectValue = 'Resend Invitation' AND 
		  ParentActionId IS NULL
)
BEGIN
	EXEC Enterprise.CreateAction @ProductID = @ProductId, @Action = N'Resend Invitation', @ActionTarget = N'Right', @ActionbValueTypeId = @ActionValueID, @Description = 'Display People in Menu bar', @ActionID = @ActionID OUTPUT;
	SELECT @ActionID AS N'@ActionID';
END;

IF NOT EXISTS
(
	SELECT 1
	FROM Enterprise.ACTION
	WHERE ObjectValue = 'Activate Deactivate User' AND 
		  ParentActionId IS NULL
)
BEGIN
	EXEC [Enterprise].[CreateAction] @ProductID = @ProductId, @Action = N'Activate Deactivate User', @ActionTarget = N'Right', @ActionbValueTypeId = @ActionValueID, @Description = 'Display Role and Rights in Menu bar', @ActionID = @ActionID OUTPUT;
	SELECT @ActionID AS N'@ActionID';
END;
----->

IF NOT EXISTS
(
	SELECT 1
	FROM Enterprise.ACTION
	WHERE ObjectValue = 'People' AND 
		  ParentActionId IS NULL
)
BEGIN
	EXEC Enterprise.CreateAction @ProductID = @ProductId, @Action = N'People', @ActionTarget = N'Right', @ActionbValueTypeId = @ActionValueID, @Description = 'Display People in Menu bar', @ActionID = @ActionID OUTPUT;
	SELECT @ActionID AS N'@ActionID';
END;

IF NOT EXISTS
(
	SELECT 1
	FROM Enterprise.ACTION
	WHERE ObjectValue = 'Roles And Right' AND 
		  ParentActionId IS NULL
)
BEGIN
	EXEC [Enterprise].[CreateAction] @ProductID = @ProductId, @Action = N'Roles And Rights', @ActionTarget = N'Right', @ActionbValueTypeId = @ActionValueID, @Description = 'Display Role and Rights in Menu bar', @ActionID = @ActionID OUTPUT;
	SELECT @ActionID AS N'@ActionID';
END;

--->

IF NOT EXISTS
(
	SELECT 1
	FROM Enterprise.ACTION
	WHERE ObjectValue = 'UsersList' AND 
		  ParentActionId IS NULL AND 
		  Description = 'User'
)
BEGIN
	EXEC [Enterprise].[CreateAction] @ProductID = @ProductId, @Action = N'UsersList', @ActionTarget = N'Route', @ActionbValueTypeId = @ActionValueID, @Description = 'User', @ActionID = @ActionID OUTPUT;
	SELECT @ActionID AS N'@ActionID';
END;

IF NOT EXISTS
(
	SELECT 1
	FROM Enterprise.ACTION
	WHERE ObjectValue = 'View User' AND 
		  ParentActionId IS NULL
)
BEGIN
	EXEC [Enterprise].[CreateAction] @ProductID = @ProductId, @Action = N'View User', @ActionTarget = N'Right', @ActionbValueTypeId = @ActionValueID, @Description = 'Display User list for UsersList Route', @ActionID = @ActionID OUTPUT;
	SELECT @ActionID AS N'@ActionID';
END;

IF NOT EXISTS
(
	SELECT 1
	FROM Enterprise.ACTION
	WHERE ObjectValue = 'Clone User' AND 
		  ParentActionId IS NULL
)
BEGIN
	EXEC [Enterprise].[CreateAction] @ProductID = @ProductId, @Action = N'Clone User', @ActionTarget = N'Right', @ActionbValueTypeId = @ActionValueID, @Description = 'Display Clone User In Drop Down List', @ActionID = @ActionID OUTPUT;
	SELECT @ActionID AS N'@ActionID';
END;


--->
IF NOT EXISTS
(
	SELECT 1
	FROM Enterprise.ACTION
	WHERE ObjectValue = 'AddUser' AND 
		  ParentActionId IS NULL AND 
		  Description = 'User'
)
BEGIN
	EXEC [Enterprise].[CreateAction] @ProductID = @ProductId, @Action = N'Adduser', @ActionTarget = N'Route', @ActionbValueTypeId = @ActionValueID, @Description = 'User', @ActionID = @ActionID OUTPUT;
	SELECT @ActionID AS N'@ActionID';
END;

IF NOT EXISTS
(
	SELECT 1
	FROM Enterprise.ACTION
	WHERE ObjectValue = 'Create User' AND 
		  ParentActionId IS NULL
)
BEGIN
	EXEC [Enterprise].[CreateAction] @ProductID = @ProductId, @Action = N'Create User', @ActionTarget = N'Right', @ActionbValueTypeId = @ActionValueID, @Description = 'Grants create user privileges', @ActionID = @ActionID OUTPUT;
	SELECT @ActionID AS N'@ActionID';
END;


--->
IF NOT EXISTS
(
	SELECT 1
	FROM Enterprise.ACTION
	WHERE ObjectValue = 'RolesAndRights' AND 
		  ParentActionId IS NULL AND 
		  Description = 'User'
)
BEGIN
	EXEC [Enterprise].[CreateAction] @ProductID = @ProductId, @Action = N'RolesAndRights', @ActionTarget = N'Route', @ActionbValueTypeId = @ActionValueID, @Description = 'User', @ActionID = @ActionID OUTPUT;
	SELECT @ActionID AS N'@ActionID';
END;

IF NOT EXISTS
(
	SELECT 1
	FROM Enterprise.ACTION
	WHERE ObjectValue = 'View Roles' AND 
		  ParentActionId IS NULL
)
BEGIN
	EXEC [Enterprise].[CreateAction] @ProductID = @ProductId, @Action = N'View Roles', @ActionTarget = N'Right', @ActionbValueTypeId = @ActionValueID, @Description = 'Grants view roles and rights privileges', @ActionID = @ActionID OUTPUT;
	SELECT @ActionID AS N'@ActionID';
END;

IF NOT EXISTS
(
	SELECT 1
	FROM Enterprise.ACTION
	WHERE ObjectValue = 'View Rights' AND 
		  ParentActionId IS NULL
)
BEGIN
	EXEC [Enterprise].[CreateAction] @ProductID = @ProductId, @Action = N'View Rights', @ActionTarget = N'Right', @ActionbValueTypeId = @ActionValueID, @Description = 'Grants view roles and rights privileges', @ActionID = @ActionID OUTPUT;
	SELECT @ActionID AS N'@ActionID';
END;

IF NOT EXISTS
(
	SELECT 1
	FROM Enterprise.ACTION
	WHERE ObjectValue = 'Edit User' AND 
		  ParentActionId IS NULL
)
BEGIN
	EXEC [Enterprise].[CreateAction] @ProductID = @ProductId, @Action = N'Edit User', @ActionTarget = N'Right', @ActionbValueTypeId = @ActionValueID, @Description = 'Grants Edit User privileges', @ActionID = @ActionID OUTPUT;
	SELECT @ActionID AS N'@ActionID';
END;

--->
IF NOT EXISTS
(
	SELECT 1
	FROM Enterprise.ACTION
	WHERE ObjectValue = 'EditUser' AND 
		  ParentActionId IS NULL AND Description NOT IN ('SuperUser', 'user')
		  
)
BEGIN
	EXEC [Enterprise].[CreateAction] @ProductID = @ProductId, @Action = N'EditUser', @ActionTarget = N'Route', @ActionbValueTypeId = @ActionValueID, @Description = 'Edit User Route', @ActionID = @ActionID OUTPUT;
	SELECT @ActionID AS N'@ActionID';
END;

IF NOT EXISTS
(
	SELECT 1
	FROM Enterprise.ACTION
	WHERE ObjectValue = 'Edit Own Profile' AND 
		  ParentActionId IS NULL
)
BEGIN
	EXEC [Enterprise].[CreateAction] @ProductID = @ProductId, @Action = N'Edit Own Profile', @ActionTarget = N'Right', @ActionbValueTypeId = @ActionValueID, @Description = 'Grants Edit Own Profile user privileges', @ActionID = @ActionID OUTPUT;
	SELECT @ActionID AS N'@ActionID';
END;


--->

---->
--SET @OrgID = 350;

WHILE EXISTS
(
	SELECT 1
	FROM #HoldOrgsUserRoutes
	WHERE PStatus = 0
)
BEGIN
	SELECT TOP 1 @OrgRowNum = Rownumber, @OrgID = OrganizationPartyID
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
		INSERT INTO Enterprise.Role( RoleId, RoleTypeId, PartyId, RoleValueTypeId )
		VALUES( -1, 0, @OrgId, -1 );
		SET IDENTITY_INSERT Enterprise.Role OFF;
	END;
	EXECUTE Enterprise.CreateRight @RoleId = -1, @RightName = 'Default_Sidemenu_People', @ShortName = 'UserSideMenuPeople', @RightCategoryId = @RightCategoryId, @PartyId = @OrgId, @ProductId = @ProductId, @Description = 'Enable/Disable People in sidemenu', @RightId = @RightId OUTPUT;
	EXECUTE Enterprise.CreateRight @RoleId = -1, @RightName = 'Default_Sidemenu_RoleAndRights', @ShortName = 'UserSideMenuRoleRights', @RightCategoryId = @RightCategoryId, @PartyId = @OrgId, @ProductId = @ProductId, @Description = 'Enable/Disable Role and Right in sidemenu', @RightId = @RightId OUTPUT;
	EXECUTE Enterprise.CreateRight @RoleId = -1, @RightName = 'Default_UserList', @ShortName = 'DefaultUserListForUsers', @RightCategoryId = @RightCategoryId, @PartyId = @OrgId, @ProductId = @ProductId, @Description = 'Enable/Disable users list', @RightId = @RightId OUTPUT;
	EXECUTE Enterprise.CreateRight @RoleId = -1, @RightName = 'Default_ViewUser', @ShortName = 'DefaultViewUserForUsers', @RightCategoryId = @RightCategoryId, @PartyId = @OrgId, @ProductId = @ProductId, @Description = 'Enable/Disable user view in users UsersListRoute', @RightId = @RightId OUTPUT;
	EXECUTE Enterprise.CreateRight @RoleId = -1, @RightName = 'Default_CloneUser', @ShortName = 'CloneUser', @RightCategoryId = @RightCategoryId, @PartyId = @OrgId, @ProductId = @ProductId, @Description = 'Enable/Disable Clone user in dropdown', @RightId = @RightId OUTPUT;
	--->
	EXECUTE Enterprise.CreateRight @RoleId = -1, @RightName = 'Default_AddUser', @ShortName = 'DefaultAddUser', @RightCategoryId = @RightCategoryId, @PartyId = @OrgId, @ProductId = @ProductId, @Description = 'Add User Route', @RightId = @RightId OUTPUT;
	EXECUTE Enterprise.CreateRight @RoleId = -1, @RightName = 'Default_Createuser', @ShortName = 'CreateUser', @RightCategoryId = @RightCategoryId, @PartyId = @OrgId, @ProductId = @ProductId, @Description = 'CreateUser Right', @RightId = @RightId OUTPUT;
	EXECUTE Enterprise.CreateRight @RoleId = -1, @RightName = 'Default_EditUser', @ShortName = 'EditUser', @RightCategoryId = @RightCategoryId, @PartyId = @OrgId, @ProductId = @ProductId, @Description = 'Edit User rights', @RightId = @RightId OUTPUT;
	--->
	EXECUTE Enterprise.CreateRight @RoleId = -1, @RightName = 'Default_RolesAndRights', @ShortName = 'DefaultRolesAndRights', @RightCategoryId = @RightCategoryId, @PartyId = @OrgId, @ProductId = @ProductId, @Description = 'Views Roles and Rights Roles', @RightId = @RightId OUTPUT;
	EXECUTE Enterprise.CreateRight @RoleId = -1, @RightName = 'Default_ViewRoles', @ShortName = 'ViewRoles', @RightCategoryId = @RightCategoryId, @PartyId = @OrgId, @ProductId = @ProductId, @Description = 'View ROles rights', @RightId = @RightId OUTPUT;
	EXECUTE Enterprise.CreateRight @RoleId = -1, @RightName = 'Default_ViewRights', @ShortName = 'ViewRights', @RightCategoryId = @RightCategoryId, @PartyId = @OrgId, @ProductId = @ProductId, @Description = 'View ROles rights', @RightId = @RightId OUTPUT;

    EXECUTE Enterprise.CreateRight @RoleId = -1, @RightName = 'Default_EditUser_Route', @ShortName = 'EditUser', @RightCategoryId = @RightCategoryId, @PartyId = @OrgId, @ProductId = @ProductId, @Description = 'Edit User', @RightId = @RightId OUTPUT;
	EXECUTE Enterprise.CreateRight @RoleId = -1, @RightName = 'Default_EditOwnProfile', @ShortName = 'EditOwnProfile', @RightCategoryId = @RightCategoryId, @PartyId = @OrgId, @ProductId = @ProductId, @Description = 'Edit Own Profile', @RightId = @RightId OUTPUT;

	EXECUTE Enterprise.CreateRight @RoleId = -1, @RightName = 'Default_ResendInvitation', @ShortName = 'ResendInvitation', @RightCategoryId = @RightCategoryId, @PartyId = @OrgId, @ProductId = @ProductId, @Description = 'Resend Email Invitation to User', @RightId = @RightId OUTPUT;
	EXECUTE Enterprise.CreateRight @RoleId = -1, @RightName = 'Default_ActivateDeActivateUser', @ShortName = 'ActivateDeActivateUser', @RightCategoryId = @RightCategoryId, @PartyId = @OrgId, @ProductId = @ProductId, @Description = 'Activate/Deactivate user', @RightId = @RightId OUTPUT;

	SELECT @ActionID = ActionID
	FROM Enterprise.ACTION
	WHERE ObjectValue = 'Activate Deactivate User' AND 
		  ObjectType = 'Right' AND 
		  ParentActionId IS NULL;
	SELECT @RightID = RightId
	FROM Enterprise.[Right] AS R
		 INNER JOIN
		 Enterprise.RightValueType AS RVT
		 ON RVT.RightValueTypeId = R.RightValueTypeId
	WHERE Value = 'Default_ActivateDeActivateUser' AND 
		  RoleId = -1;
	EXEC [Enterprise].[LinkActionToRights] @ActionID = @ActionID, @RightId = @RightId, @StatusId = @Status, @UserActionId = @UserActionId OUTPUT;
	
	SELECT @ActionID = ActionID
	FROM Enterprise.ACTION
	WHERE ObjectValue = 'Resend Invitation' AND 
		  ObjectType = 'Right' AND 
		  ParentActionId IS NULL;
	SELECT @RightID = RightId
	FROM Enterprise.[Right] AS R
		 INNER JOIN
		 Enterprise.RightValueType AS RVT
		 ON RVT.RightValueTypeId = R.RightValueTypeId
	WHERE Value = 'Default_ResendInvitation' AND 
		  RoleId = -1;
	EXEC [Enterprise].[LinkActionToRights] @ActionID = @ActionID, @RightId = @RightId, @StatusId = @Status, @UserActionId = @UserActionId OUTPUT;
    --->

	
	SELECT @ActionId = ActionId
	FROM Enterprise.ACTION
	WHERE ObjectValue = 'EditUser' AND 
		  ObjectType = 'Route' AND 
		  Description = 'Edit User Route' AND
		  ParentActionId IS NULL;
	SELECT @RightID = RightId
	FROM Enterprise.[Right] AS R
		 INNER JOIN
		 Enterprise.RightValueType AS RVT
		 ON RVT.RightValueTypeId = R.RightValueTypeId
	WHERE Value = 'Default_EditUser_Route' AND 
		  RoleId = -1;
	EXEC [Enterprise].[LinkActionToRights] @ActionID = @ActionID, @RightId = @RightId, @StatusId = @Status, @UserActionId = @UserActionId OUTPUT;
	
	SELECT @ActionID = ActionID
	FROM Enterprise.ACTION
	WHERE ObjectValue = 'Edit Own Profile' AND 
		  ObjectType = 'Right' AND 
		  ParentActionId IS NULL;
	SELECT @RightID = RightId
	FROM Enterprise.[Right] AS R
		 INNER JOIN
		 Enterprise.RightValueType AS RVT
		 ON RVT.RightValueTypeId = R.RightValueTypeId
	WHERE Value = 'Default_EditOwnProfile' AND 
		  RoleId = -1;
	EXEC [Enterprise].[LinkActionToRights] @ActionID = @ActionID, @RightId = @RightId, @StatusId = @Status, @UserActionId = @UserActionId OUTPUT;
    --->
	--->
	SELECT @ActionId = ActionId
	FROM Enterprise.ACTION
	WHERE ObjectValue = 'People' AND 
		  ObjectType = 'Right' AND 
		  ParentActionId IS NULL;
	SELECT @RightID = RightId
	FROM Enterprise.[Right] AS R
		 INNER JOIN
		 Enterprise.RightValueType AS RVT
		 ON RVT.RightValueTypeId = R.RightValueTypeId
	WHERE Value = 'Default_Sidemenu_People' AND 
		  RoleId = -1;
	EXEC [Enterprise].[LinkActionToRights] @ActionID = @ActionID, @RightId = @RightId, @StatusId = @Status, @UserActionId = @UserActionId OUTPUT;
	SELECT @ActionID = ActionID
	FROM Enterprise.ACTION
	WHERE ObjectValue = 'Roles And Right' AND 
		  ObjectType = 'Right' AND 
		  ParentActionId IS NULL;
	SELECT @RightID = RightId
	FROM Enterprise.[Right] AS R
		 INNER JOIN
		 Enterprise.RightValueType AS RVT
		 ON RVT.RightValueTypeId = R.RightValueTypeId
	WHERE Value = 'Default_Sidemenu_RoleAndRights' AND 
		  RoleId = -1;
	EXEC [Enterprise].[LinkActionToRights] @ActionID = @ActionID, @RightId = @RightId, @StatusId = @Status, @UserActionId = @UserActionId OUTPUT;
    --->
	SELECT @ActionId = ActionID
	FROM Enterprise.ACTION
	WHERE ObjectValue = 'UsersList' AND 
		  ObjectType = 'Route' AND 
		  Description = 'User';
	SELECT @RightID = RightId
	FROM Enterprise.[Right] AS R
		 INNER JOIN
		 Enterprise.RightValueType AS RVT
		 ON RVT.RightValueTypeId = R.RightValueTypeId
	WHERE Value = 'Default_UserList' AND 
		  RoleId = -1;
	EXEC [Enterprise].[LinkActionToRights] @ActionID = @ActionID, @RightId = @RightId, @StatusId = @Status, @UserActionId = @UserActionId OUTPUT;
	SELECT @ActionID = ActionID
	FROM Enterprise.ACTION
	WHERE ObjectValue = 'View User' AND 
		  ObjectType = 'Right' AND 
		  ParentActionId IS NULL;
	SELECT @RightID = RightId
	FROM Enterprise.[Right] AS R
		 INNER JOIN
		 Enterprise.RightValueType AS RVT
		 ON RVT.RightValueTypeId = R.RightValueTypeId
	WHERE Value = 'Default_ViewUser' AND 
		  RoleId = -1;
	EXEC [Enterprise].[LinkActionToRights] @ActionID = @ActionID, @RightId = @RightId, @StatusId = @Status, @UserActionId = @UserActionId OUTPUT;
	
	
	SELECT @ActionID = ActionID
	FROM Enterprise.ACTION
	WHERE ObjectValue = 'Clone User' AND 
		  ObjectType = 'Right' AND 
		  ParentActionId IS NULL;
	SELECT @RightID = RightId
	FROM Enterprise.[Right] AS R
		 INNER JOIN
		 Enterprise.RightValueType AS RVT
		 ON RVT.RightValueTypeId = R.RightValueTypeId
	WHERE Value = 'Default_CloneUser' AND 
		  RoleId = -1;
	EXEC [Enterprise].[LinkActionToRights] @ActionID = @ActionID, @RightId = @RightId, @StatusId = @Status, @UserActionId = @UserActionId OUTPUT;

    ---->
    SELECT @ActionID = ActionID
	FROM Enterprise.ACTION
	WHERE ObjectValue = 'AddUser' AND 
		  ObjectType = 'Route' AND 
		  ParentActionId IS NULL;
	SELECT @RightID = RightId
	FROM Enterprise.[Right] AS R
		 INNER JOIN
		 Enterprise.RightValueType AS RVT
		 ON RVT.RightValueTypeId = R.RightValueTypeId
	WHERE Value = 'Default_AddUser' AND 
		  RoleId = -1;
	EXEC [Enterprise].[LinkActionToRights] @ActionID = @ActionID, @RightId = @RightId, @StatusId = @Status, @UserActionId = @UserActionId OUTPUT;


	SELECT @ActionID = ActionID
	FROM Enterprise.ACTION
	WHERE ObjectValue = 'Create User' AND 
		  ObjectType = 'Right' AND 
		  ParentActionId IS NULL;
	SELECT @RightID = RightId
	FROM Enterprise.[Right] AS R
		 INNER JOIN
		 Enterprise.RightValueType AS RVT
		 ON RVT.RightValueTypeId = R.RightValueTypeId
	WHERE Value = 'Default_CreateUser' AND 
		  RoleId = -1;
	EXEC [Enterprise].[LinkActionToRights] @ActionID = @ActionID, @RightId = @RightId, @StatusId = @Status, @UserActionId = @UserActionId OUTPUT;
	---->
	--Roles And rights
	 SELECT @ActionID = ActionID
	FROM Enterprise.ACTION
	WHERE ObjectValue = 'RolesAndRights' AND 
		  ObjectType = 'Route' AND 
		  ParentActionId IS NULL;
	SELECT @RightID = RightId
	FROM Enterprise.[Right] AS R
		 INNER JOIN
		 Enterprise.RightValueType AS RVT
		 ON RVT.RightValueTypeId = R.RightValueTypeId
	WHERE Value = 'Default_RolesAndRights' AND 
		  RoleId = -1;
	EXEC [Enterprise].[LinkActionToRights] @ActionID = @ActionID, @RightId = @RightId, @StatusId = @Status, @UserActionId = @UserActionId OUTPUT;


	SELECT @ActionID = ActionID
	FROM Enterprise.ACTION
	WHERE ObjectValue = 'View Roles' AND 
		  ObjectType = 'Right' AND 
		  ParentActionId IS NULL;
	SELECT @RightID = RightId
	FROM Enterprise.[Right] AS R
		 INNER JOIN
		 Enterprise.RightValueType AS RVT
		 ON RVT.RightValueTypeId = R.RightValueTypeId
	WHERE Value = 'Default_ViewRoles' AND 
		  RoleId = -1;
	EXEC [Enterprise].[LinkActionToRights] @ActionID = @ActionID, @RightId = @RightId, @StatusId = @Status, @UserActionId = @UserActionId OUTPUT;

	SELECT @ActionID = ActionID
	FROM Enterprise.ACTION
	WHERE ObjectValue = 'View Rights' AND 
		  ObjectType = 'Right' AND 
		  ParentActionId IS NULL;
	SELECT @RightID = RightId
	FROM Enterprise.[Right] AS R
		 INNER JOIN
		 Enterprise.RightValueType AS RVT
		 ON RVT.RightValueTypeId = R.RightValueTypeId
	WHERE Value = 'Default_ViewRights' AND 
		  RoleId = -1;
	EXEC [Enterprise].[LinkActionToRights] @ActionID = @ActionID, @RightId = @RightId, @StatusId = @Status, @UserActionId = @UserActionId OUTPUT;

	---->
	SELECT @ActionID = ActionID
	FROM Enterprise.ACTION
	WHERE ObjectValue = 'Edit User' AND 
		  ObjectType = 'Right' AND 
		  ParentActionId IS NULL;
	SELECT @RightID = RightId
	FROM Enterprise.[Right] AS R
		 INNER JOIN
		 Enterprise.RightValueType AS RVT
		 ON RVT.RightValueTypeId = R.RightValueTypeId
	WHERE Value = 'Default_EditUser' AND 
		  RoleId = -1;
	EXEC [Enterprise].[LinkActionToRights] @ActionID = @ActionID, @RightId = @RightId, @StatusId = @Status, @UserActionId = @UserActionId OUTPUT;

	---->

	UPDATE #HoldOrgsUserRoutes
	  SET PStatus = 1
	WHERE RowNumber = @OrgRowNum;
END;


--Setup dependencies




DECLARE @ProductLearning INT
DECLARE @People INT;
DECLARE @RoleAndRights INT;
DECLARE @ViewUser INT;
DECLARE @UsersList INT;
--DECLARE @RightValueTypeId INT;
DECLARE @CloneUser INT;
DECLARE @AddUser INT
DECLARE @CreateUser INT
DECLARE @RolesAndRights INT
DECLARE @ViewRoles INT
DECLARE @ViewRights INT
DECLARE @EditUser INT
DECLARE @EditUserRoute INT
DECLARE @EditOwnProfile INT
DECLARE @ResendInvitation INT
DECLARE @ActivateDeactivateUser INT


SELECT @ProductLearning = RightValueTypeId
FROM Enterprise.RightValueType
WHERE value = 'Default_ResendInvitation';

SELECT @ResendInvitation = RightValueTypeId
FROM Enterprise.RightValueType
WHERE value = 'Default_ResendInvitation';

SELECT @ActivateDeactivateUser = RightValueTypeId
FROM Enterprise.RightValueType
WHERE value = 'Default_ActivateDeActivateUser';


SELECT @EditUserRoute = RightValueTypeId
FROM Enterprise.RightValueType
WHERE value = 'Default_EditUser_Route';

SELECT @EditOwnProfile = RightValueTypeId
FROM Enterprise.RightValueType
WHERE value = 'Default_EditOwnProfile';
--->

SELECT @EditUser = RightValueTypeId
FROM Enterprise.RightValueType
WHERE value = 'Default_EditUser';

SELECT @ViewRights = RightValueTypeId
FROM Enterprise.RightValueType
WHERE value = 'Default_ViewRights';

SELECT @RolesAndRights = RightValueTypeId
FROM Enterprise.RightValueType
WHERE value = 'Default_RolesAndRights';

SELECT @ViewRoles = RightValueTypeId
FROM Enterprise.RightValueType
WHERE value = 'Default_ViewRoles';

--->
SELECT @CreateUser = RightValueTypeId
FROM Enterprise.RightValueType
WHERE value = 'Default_CreateUser';

SELECT @AddUser = RightValueTypeId
FROM Enterprise.RightValueType
WHERE value = 'Default_AddUser';
--->
SELECT @People = RightValueTypeId
FROM Enterprise.RightValueType
WHERE value = 'Default_Sidemenu_People';

SELECT @RoleAndRights = RightValueTypeId
FROM Enterprise.RightValueType
WHERE value = 'Default_Sidemenu_RoleAndRights';

SELECT @UsersList = RightValueTypeId
FROM Enterprise.RightValueType
WHERE value = 'Default_UserList';

SELECT @ViewUser = RightValueTypeId
FROM Enterprise.RightValueType
WHERE value = 'Default_ViewUser';

SELECT @CloneUser = RightValueTypeId
FROM Enterprise.RightValueType
WHERE value = 'Default_CloneUser';



SELECT @RightValueTypeId = RightValueTypeId
FROM Enterprise.RightValueType
WHERE value IN ('Edit Own Profile', 'Ability to edit my own profile');

IF NOT EXISTS
(
	SELECT 1
	FROM Enterprise.RightDependency
	WHERE RightValueTypeId = @RightValueTypeId AND 
		  DependentRightValueTypeId = @EditUserRoute
)
BEGIN
	INSERT INTO Enterprise.RightDependency( RightValueTypeId, DependentRightValueTypeId )
	VALUES( @RightValueTypeId, @EditUserRoute );
END;

IF NOT EXISTS
(
	SELECT 1
	FROM Enterprise.RightDependency
	WHERE RightValueTypeId = @RightValueTypeId AND 
		  DependentRightValueTypeId = @EditOwnProfile
)
BEGIN
	INSERT INTO Enterprise.RightDependency( RightValueTypeId, DependentRightValueTypeId )
	VALUES( @RightValueTypeId, @EditOwnProfile );
END;

--->
SELECT @RightValueTypeId = RightValueTypeId
FROM Enterprise.RightValueType
WHERE value IN ('View Roles & Rights', 'Ability to view roles and rights');

IF NOT EXISTS
(
	SELECT 1
	FROM Enterprise.RightDependency
	WHERE RightValueTypeId = @RightValueTypeId AND 
		  DependentRightValueTypeId = @RolesAndRights
)
BEGIN
	INSERT INTO Enterprise.RightDependency( RightValueTypeId, DependentRightValueTypeId )
	VALUES( @RightValueTypeId, @RolesAndRights );
END;

IF NOT EXISTS
(
	SELECT 1
	FROM Enterprise.RightDependency
	WHERE RightValueTypeId = @RightValueTypeId AND 
		  DependentRightValueTypeId = @ViewRoles
)
BEGIN
	INSERT INTO Enterprise.RightDependency( RightValueTypeId, DependentRightValueTypeId )
	VALUES( @RightValueTypeId, @ViewRoles );
END;

IF NOT EXISTS
(
	SELECT 1
	FROM Enterprise.RightDependency
	WHERE RightValueTypeId = @RightValueTypeId AND 
		  DependentRightValueTypeId = @ViewRights
)
BEGIN
	INSERT INTO Enterprise.RightDependency( RightValueTypeId, DependentRightValueTypeId )
	VALUES( @RightValueTypeId, @ViewRights );
END;



---->

SELECT @RightValueTypeId = RightValueTypeId
FROM Enterprise.RightValueType
WHERE value IN ('Create User', 'Ability to create users');

IF NOT EXISTS
(
	SELECT 1
	FROM Enterprise.RightDependency
	WHERE RightValueTypeId = @RightValueTypeId AND 
		  DependentRightValueTypeId = @People
)
BEGIN
	INSERT INTO Enterprise.RightDependency( RightValueTypeId, DependentRightValueTypeId )
	VALUES( @RightValueTypeId, @People );
END;

IF NOT EXISTS
(
	SELECT 1
	FROM Enterprise.RightDependency
	WHERE RightValueTypeId = @RightValueTypeId AND 
		  DependentRightValueTypeId = @AddUser
)
BEGIN
	INSERT INTO Enterprise.RightDependency( RightValueTypeId, DependentRightValueTypeId )
	VALUES( @RightValueTypeId, @AddUser );
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



SELECT @RightValueTypeId = RightValueTypeId
FROM Enterprise.RightValueType
WHERE value IN ('View User', 'Ability to view users');
---->
IF NOT EXISTS
(
	SELECT 1
	FROM Enterprise.RightDependency
	WHERE RightValueTypeId = @RightValueTypeId AND 
		  DependentRightValueTypeId = @People
)
BEGIN
	INSERT INTO Enterprise.RightDependency( RightValueTypeId, DependentRightValueTypeId )
	VALUES( @RightValueTypeId, @People );
END;

IF NOT EXISTS
(
	SELECT 1
	FROM Enterprise.RightDependency
	WHERE RightValueTypeId = @RightValueTypeId AND 
		  DependentRightValueTypeId = @UsersList
)
BEGIN
	INSERT INTO Enterprise.RightDependency( RightValueTypeId, DependentRightValueTypeId )
	VALUES ( @RightValueTypeId, @UsersList );
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
		  DependentRightValueTypeId = @EditUserRoute
)
BEGIN
	INSERT INTO Enterprise.RightDependency( RightValueTypeId, DependentRightValueTypeId )
	VALUES( @RightValueTypeId, @EditUserRoute );
END;



----->


SELECT @RightValueTypeId = RightValueTypeId
FROM Enterprise.RightValueType
WHERE value IN ('Manage Roles & Rights', 'Ability to manage roles and rights');

IF NOT EXISTS
(
	SELECT 1
	FROM Enterprise.RightDependency
	WHERE RightValueTypeId = @RightValueTypeId AND 
		  DependentRightValueTypeId = @RoleAndRights
)
BEGIN
	INSERT INTO Enterprise.RightDependency( RightValueTypeId, DependentRightValueTypeId )
	VALUES( @RightValueTypeId, @RoleAndRights );
END;

SELECT @RightValueTypeId = RightValueTypeId
FROM Enterprise.RightValueType
WHERE value IN ('Clone User', 'Ability to clone users');

IF NOT EXISTS
(
	SELECT 1
	FROM Enterprise.RightDependency
	WHERE RightValueTypeId = @RightValueTypeId AND 
		  DependentRightValueTypeId = @CloneUser
)
BEGIN
	INSERT INTO Enterprise.RightDependency( RightValueTypeId, DependentRightValueTypeId )
	VALUES( @RightValueTypeId, @CloneUser );
END;



SELECT @RightValueTypeId = RightValueTypeId
FROM Enterprise.RightValueType
WHERE value IN ('Edit User', 'Ability to edit users');

IF NOT EXISTS
(
	SELECT 1
	FROM Enterprise.RightDependency
	WHERE RightValueTypeId = @RightValueTypeId AND 
		  DependentRightValueTypeId = @People
)
BEGIN
	INSERT INTO Enterprise.RightDependency( RightValueTypeId, DependentRightValueTypeId )
	VALUES( @RightValueTypeId, @People );
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

SELECT @RightValueTypeId = RightValueTypeId
FROM Enterprise.RightValueType
WHERE value IN ('Ability to lock/unlock users');

IF NOT EXISTS
(
	SELECT 1
	FROM Enterprise.RightDependency
	WHERE RightValueTypeId = @RightValueTypeId AND 
		  DependentRightValueTypeId = @ActivateDeactivateUser
)
BEGIN
	INSERT INTO Enterprise.RightDependency( RightValueTypeId, DependentRightValueTypeId )
	VALUES( @RightValueTypeId, @ActivateDeactivateUser );
END;



SELECT @RightValueTypeId = RightValueTypeId
FROM Enterprise.RightValueType
WHERE value IN ('Access to Product Learning Portal');

IF NOT EXISTS
(
	SELECT 1
	FROM Enterprise.RightDependency
	WHERE RightValueTypeId = @RightValueTypeId AND 
		  DependentRightValueTypeId = @ProductLearning
)
BEGIN
	INSERT INTO Enterprise.RightDependency( RightValueTypeId, DependentRightValueTypeId )
	VALUES( @RightValueTypeId, @ProductLearning);
END;




--Hide Roles

SELECT @Status = StatusType.StatusTypeID     
FROM Enterprise.StatusTypeCategoryType
     JOIN Enterprise.StatusTypeCategory ON StatusTypeCategory.StatusTypeCategoryTypeId = StatusTypeCategoryType.StatusTypeCategoryTypeId
     JOIN Enterprise.StatusTypeCategoryClassification ON StatusTypeCategoryClassification.StatusTypeCategoryId = StatusTypeCategory.StatusTypeCategoryId
     JOIN Enterprise.StatusType ON StatusType.StatusTypeId = StatusTypeCategoryClassification.StatusTypeId
WHERE StatusType.name = 'HIDDEN'
      AND StatusTypeCategoryType.Name = 'Security';

BEGIN TRAN
UPDATE UA 
    SET UA.Status = @Status 
FROM Enterprise.[Right] R
     INNER JOIN Enterprise.RightValueType RVT ON RVT.RightValueTypeId = R.RightValueTypeId
	INNER JOIN Enterprise.UserActions UA ON UA.RightId = R.RightId
WHERE RVT.VALUE IN
	   ('Access to Stars Report'
	   , 'Access to Leasing & Rents Conversion Tool'
	   , 'Access to Identity Provider Configuration Page'
	   , 'Access to Amenities Tool'
	   , 'Access to Property Hierarchy Tool'
	   , 'Access to Employee Management'
	   , 'Ability to Impersonate a User'
	   , 'Impersonate a User'
	   , 'View Audit Trail on Profile Data'
	   )
COMMIT


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
SELECT @Status = StatusType.StatusTypeID
FROM Enterprise.StatusTypeCategoryType
     JOIN Enterprise.StatusTypeCategory ON StatusTypeCategory.StatusTypeCategoryTypeId = StatusTypeCategoryType.StatusTypeCategoryTypeId
     JOIN Enterprise.StatusTypeCategoryClassification ON StatusTypeCategoryClassification.StatusTypeCategoryId = StatusTypeCategory.StatusTypeCategoryId
     JOIN Enterprise.StatusType ON StatusType.StatusTypeId = StatusTypeCategoryClassification.StatusTypeId
WHERE StatusType.name = 'ALL'
      AND StatusTypeCategoryType.Name = 'Security';

--SET @OrgID = 350;


SELECT @ActionValueID = [ActionValueTypeID]
FROM Enterprise.ActionValueType
WHERE Value = 'Right';
SELECT @ProductID = ProductId
FROM Enterprise.Product
WHERE Name = 'Unified Login';
SELECT @ParentActionId = ActionID
FROM Enterprise.ACTION
WHERE ObjectValue = 'UsersList'
      AND ParentActionID IS NULL
      AND Description = 'SuperUser';
IF NOT EXISTS
(
    SELECT 1
    FROM Enterprise.ACTION
    WHERE ObjectValue = 'Resend Invitation'
          AND ParentActionID = @ParentActionId
)
    BEGIN
        EXEC [Enterprise].[CreateAction]
             @ProductID = @ProductId,
             @Action = N'Resend Invitation',
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
    WHERE ObjectValue = 'Activate Deactivate User'
          AND ParentActionID = @ParentActionId
)
    BEGIN
        EXEC [Enterprise].[CreateAction]
             @ProductID = @ProductId,
             @Action = N'Activate Deactivate User',
             @ActionTarget = N'Right',
             @ActionbValueTypeId = @ActionValueID,
             @Description = '',
             @ParentActionID = @ParentActionId,
             @ActionID = @ActionID OUTPUT;
        SELECT @ActionID AS N'@ActionID';
    END;

	


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
WHERE StatusType.name = 'HIDDEN' AND 
	  StatusTypeCategoryType.Name = 'Security';

BEGIN TRAN;

UPDATE UA
  SET UA.Status = @Status
FROM Enterprise.[Right] R
	 INNER JOIN
	 Enterprise.RightValueType RVT
	 ON RVT.RightValueTypeId = R.RightValueTypeId
	 INNER JOIN
	 Enterprise.UserActions UA
	 ON UA.RightId = R.RightId
WHERE RVT.VALUE 
IN 
	('Access to Stars Report', 
	'Access to Leasing & Rents Conversion Tool', 
	'Access to Identity Provider Configuration Page', 
	'Access to Amenities Tool', 
	'Access to Property Hierarchy Tool', 
	'Access to Employee Management', 
	'Ability to Impersonate a User', 
	'Impersonate a User', 
	'Access to Client Portal', 
	'Ability to edit Company information', 
	'Ability to edit password', 
	'Ability to edit Property information', 
	'Ability to manage roles and rights', 
	'Ability to view audit trail on user data', 
	'Ability to view Company page', 
	'Ability to view Property page', 
	'Ability to view roles and rights', 
	'See All RealPage Products');

COMMIT;



EXEC sys.sp_updateextendedproperty @name=N'Build', @value='53'