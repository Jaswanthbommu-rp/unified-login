
Declare @ActionID INT
DECLARE @RightID INT
DECLARE @RoleID INT
DECLARE @Status INT
DECLARE @ActionValueID INT

DECLARE @ProductID INT
DECLARE @ParentActionId INT
DECLARE @statustypecategoryid INT
DECLARE @ident INT
DECLARE @userrightsid INT

--/*Populate Status tables*/
--DECLARE @userrightsid INT
--IF NOT EXISTS (SELECT 1 FROM Enterprise.StatusTypeCategoryType WHERE Name = 'Security')
--BEGIN
--	INSERT INTO Enterprise.StatusTypeCategoryType (ParentStatusTypeCategoryTypeId, Name)
--	VALUES
--	(   NULL, 'Security' )
--END

--SELECT @userrightsid = StatusTypeCategoryTypeid FROM Enterprise.StatusTypeCategoryType WHERE name = 'Security'

--IF NOT EXISTS (SELECT 1 FROM Enterprise.StatusTypeCategory WHERE name = 'Roles and Rights')
--BEGIN
--	INSERT INTO Enterprise.StatusTypeCategory
--	(
--		ParentStatusTypeCategoryId,
--		StatusTypeCategoryTypeId,
--		Name
--	)
--	VALUES
--	(   null, -- ParentStatusTypeCategoryId - int
--		@userrightsid, -- StatusTypeCategoryTypeId - int
--		'Roles and Rights' -- Name - varchar(50)
--	)
--END

--DECLARE @statustypecategoryid INT
--DECLARE @ident INT
--SELECT @statustypecategoryid = StatusTypeCategoryId FROM Enterprise.StatusTypeCategory WHERE name = 'Roles and Rights'

--IF NOT EXISTS (SELECT 1 FROM Enterprise.StatusType WHERE name = 'ALL')
--BEGIN
--	INSERT INTO Enterprise.StatusType(name)
--	VALUES
--	( 'All' )
--	SELECT @ident = @@IDENTITY
--	INSERT INTO Enterprise.StatusTypeCategoryClassification
--	(
--		StatusTypeId,
--		StatusTypeCategoryId,
--		FromDate,
--		ThruDate
--	)
--	VALUES
--	(   @ident,         -- StatusTypeId - int
--		@statustypecategoryid,         -- StatusTypeCategoryId - int
--		GETDATE(), -- FromDate - datetime
--		null  -- ThruDate - datetime
--	)
--END

--IF NOT EXISTS (SELECT 1 FROM Enterprise.StatusType WHERE name = 'Hidden')
--BEGIN
--	INSERT INTO Enterprise.StatusType(name)
--	VALUES
--	( 'Hidden' )
--	SELECT @ident = @@IDENTITY
--	INSERT INTO Enterprise.StatusTypeCategoryClassification
--	(
--		StatusTypeId,
--		StatusTypeCategoryId,
--		FromDate,
--		ThruDate
--	)
--	VALUES
--	(   @ident,         -- StatusTypeId - int
--		@statustypecategoryid,         -- StatusTypeCategoryId - int
--		GETDATE(), -- FromDate - datetime
--		null  -- ThruDate - datetime
--	)
--END

--IF NOT EXISTS (SELECT 1 FROM Enterprise.StatusType WHERE name = 'Internal Only')
--BEGIN
--	INSERT INTO Enterprise.StatusType(name)
--	VALUES
--	( 'Internal Only' )
--	SELECT @ident = @@IDENTITY
--	INSERT INTO Enterprise.StatusTypeCategoryClassification
--	(
--		StatusTypeId,
--		StatusTypeCategoryId,
--		FromDate,
--		ThruDate
--	)
--	VALUES
--	(   @ident,         -- StatusTypeId - int
--		@statustypecategoryid,         -- StatusTypeCategoryId - int
--		GETDATE(), -- FromDate - datetime
--		null  -- ThruDate - datetime
--	)
--END

--/*Populate ActionvalueTypes*/

--DECLARE @ActionValueTypeId INT
--EXECUTE [Enterprise].[CreateActionValueType] @ActionValueName=N'Route', @Description=N'Route Name', @ActionValueTypeId=@ActionValueTypeId OUTPUT
--SELECT @ActionValueTypeId
--EXECUTE [Enterprise].[CreateActionValueType] @ActionValueName=N'Page', @Description=N'Page ID', @ActionValueTypeId=@ActionValueTypeId OUTPUT
--SELECT @ActionValueTypeId
--EXECUTE [Enterprise].[CreateActionValueType] @ActionValueName=N'Control', @Description=N'Control on page', @ActionValueTypeId=@ActionValueTypeId OUTPUT
--SELECT @ActionValueTypeId



--/*Populate ROOT Level Actions*/
--Declare @ActionID INT
--DECLARE @RightID INT
--DECLARE @RoleID INT
--DECLARE @Status INT
--DECLARE @ActionValueID INT

--DECLARE @ProductID INT
--DECLARE @ParentActionId INT
--SELECT @ActionValueID = [ActionValueTypeID] FROM Enterprise.ActionValueType WHERE Value = 'ROUTE'
--SELECT @ProductID = ProductId FROM Enterprise.Product WHERE Name = 'RealPage Document Management'

--IF NOT EXISTS (SELECT 1 FROM Enterprise.Action WHERE ObjectValue = 'UsersList')
--BEGIN
--	EXEC [Enterprise].[CreateAction] @ProductID = @ProductId, @Action = N'UsersList', @ActionTarget = N'Route', @ActionbValueTypeId = @ActionValueID, @Description = '', @ActionID = @ActionID OUTPUT
--	SELECT	@ActionID as N'@ActionID'
--END
--IF NOT EXISTS (SELECT 1 FROM Enterprise.Action WHERE ObjectValue = 'Edit User')
--BEGIN
--	EXEC [Enterprise].[CreateAction] @ProductID = @ProductId, @Action = N'Edit User', @ActionTarget = N'Route', @ActionbValueTypeId = @ActionValueID, @Description = '', @ActionID = @ActionID OUTPUT
--	SELECT	@ActionID as N'@ActionID'
--END

--IF NOT EXISTS (SELECT 1 FROM Enterprise.Action WHERE ObjectValue = 'SideMenu')
--BEGIN
--	EXEC [Enterprise].[CreateAction] @ProductID = @ProductId, @Action = N'SideMenu', @ActionTarget = N'Route', @ActionbValueTypeId = @ActionValueID, @Description = '', @ActionID = @ActionID OUTPUT
--	SELECT	@ActionID as N'@ActionID'
--END
--IF NOT EXISTS (SELECT 1 FROM Enterprise.Action WHERE ObjectValue = 'DashBoard')
--BEGIN
--	EXEC [Enterprise].[CreateAction] @ProductID = @ProductId, @Action = N'DashBoard', @ActionTarget = N'Route', @ActionbValueTypeId = @ActionValueID, @Description = '', @ActionID = @ActionID OUTPUT
--	SELECT	@ActionID as N'@ActionID'
--END
--IF NOT EXISTS (SELECT 1 FROM Enterprise.Action WHERE ObjectValue = 'RolesAndRights')
--BEGIN
--	EXEC [Enterprise].[CreateAction] @ProductID = @ProductId, @Action = N'RolesAndRights', @ActionTarget = N'Route', @ActionbValueTypeId = @ActionValueID, @Description = '', @ActionID = @ActionID OUTPUT
--	SELECT	@ActionID as N'@ActionID'
--END

--/*Populate Child level Actions*/
--SELECT @ParentActionId = ActionID  FROM Enterprise.Action Where ObjectValue = 'UsersList' and ParentActionID is NULL
--IF NOT EXISTS (SELECT 1 FROM Enterprise.Action WHERE ObjectValue = 'Create User' and ParentActionID = @ParentActionId)
--BEGIN
--	EXEC [Enterprise].[CreateAction] @ProductID = @ProductId, @Action = N'Create User', @ActionTarget = N'Right', @ActionbValueTypeId = @ActionValueID, @Description = '', @ParentActionID = @ParentActionId, @ActionID = @ActionID OUTPUT
--	SELECT	@ActionID as N'@ActionID'
--END

--IF NOT EXISTS (SELECT 1 FROM Enterprise.Action WHERE ObjectValue = 'Lock/Unlock User' and ParentActionID = @ParentActionId)
--BEGIN
--	EXEC [Enterprise].[CreateAction] @ProductID = @ProductId, @Action = N'Lock/Unlock User', @ActionTarget = N'Right', @ActionbValueTypeId = @ActionValueID, @Description = '', @ParentActionID = @ParentActionId, @ActionID = @ActionID OUTPUT
--	SELECT	@ActionID as N'@ActionID'
--END

--IF NOT EXISTS (SELECT 1 FROM Enterprise.Action WHERE ObjectValue = 'View User' and ParentActionID = @ParentActionId)
--BEGIN
--	EXEC [Enterprise].[CreateAction] @ProductID = @ProductId, @Action = N'View User', @ActionTarget = N'Right', @ActionbValueTypeId = @ActionValueID, @Description = '', @ParentActionID = @ParentActionId, @ActionID = @ActionID OUTPUT
--	SELECT	@ActionID as N'@ActionID'
--END

--IF NOT EXISTS (SELECT 1 FROM Enterprise.Action WHERE ObjectValue = 'Edit User' and ParentActionID = @ParentActionId)
--BEGIN
--	EXEC [Enterprise].[CreateAction] @ProductID = @ProductId, @Action = N'Edit User', @ActionTarget = N'Right', @ActionbValueTypeId = @ActionValueID, @Description = '', @ParentActionID = @ParentActionId, @ActionID = @ActionID OUTPUT
--	SELECT	@ActionID as N'@ActionID'
--END



--SELECT @ParentActionId = ActionID FROM Enterprise.Action Where ObjectValue = 'Edit User' and ParentActionID is NULL
--IF NOT EXISTS (SELECT 1 FROM Enterprise.Action WHERE ObjectValue = 'Edit Other User Profile' and ParentActionID = @ParentActionId)
--BEGIN
--	EXEC [Enterprise].[CreateAction] @ProductID = @ProductId, @Action = N'Edit Other User Profile', @ActionTarget = N'Right', @ActionbValueTypeId = @ActionValueID, @Description = '', @ParentActionID = @ParentActionId, @ActionID = @ActionID OUTPUT
--	SELECT	@ActionID as N'@ActionID'
--END


--IF NOT EXISTS (SELECT 1 FROM Enterprise.Action WHERE ObjectValue = 'Edit Own Profile' and ParentActionID = @ParentActionId)
--BEGIN
--	EXEC [Enterprise].[CreateAction] @ProductID = @ProductId, @Action = N'Edit Own Profile', @ActionTarget = N'Right', @ActionbValueTypeId = @ActionValueID, @Description = '', @ParentActionID = @ParentActionId, @ActionID = @ActionID OUTPUT
--	SELECT	@ActionID as N'@ActionID'
--END


--IF NOT EXISTS (SELECT 1 FROM Enterprise.Action WHERE ObjectValue = 'Edit Password' and ParentActionID = @ParentActionId)
--BEGIN
--	EXEC [Enterprise].[CreateAction] @ProductID = @ProductId, @Action = N'Edit Password', @ActionTarget = N'Right', @ActionbValueTypeId = @ActionValueID, @Description = '', @ParentActionID = @ParentActionId, @ActionID = @ActionID OUTPUT
--	SELECT	@ActionID as N'@ActionID'
--END

------
--SELECT @ParentActionId = ActionID FROM Enterprise.Action Where ObjectValue = 'SideMenu' and ParentActionID is NULL
--IF NOT EXISTS (SELECT 1 FROM Enterprise.Action WHERE ObjectValue = 'Dashboard' and ParentActionID = @ParentActionId)
--BEGIN
--	EXEC [Enterprise].[CreateAction] @ProductID = @ProductId, @Action = N'Dashboard', @ActionTarget = N'Right', @ActionbValueTypeId = @ActionValueID, @Description = '', @ParentActionID = @ParentActionId, @ActionID = @ActionID OUTPUT
--	SELECT	@ActionID as N'@ActionID'
--END

--IF NOT EXISTS (SELECT 1 FROM Enterprise.Action WHERE ObjectValue = 'Products' and ParentActionID = @ParentActionId)
--BEGIN
--	EXEC [Enterprise].[CreateAction] @ProductID = @ProductId, @Action = N'Products', @ActionTarget = N'Right', @ActionbValueTypeId = @ActionValueID, @Description = '', @ParentActionID = @ParentActionId, @ActionID = @ActionID OUTPUT
--	SELECT	@ActionID as N'@ActionID'
--END
--IF NOT EXISTS (SELECT 1 FROM Enterprise.Action WHERE ObjectValue = 'People' and ParentActionID = @ParentActionId)
--BEGIN
--	EXEC [Enterprise].[CreateAction] @ProductID = @ProductId, @Action = N'People', @ActionTarget = N'Right', @ActionbValueTypeId = @ActionValueID, @Description = '', @ParentActionID = @ParentActionId, @ActionID = @ActionID OUTPUT
--	SELECT	@ActionID as N'@ActionID'
--END
--IF NOT EXISTS (SELECT 1 FROM Enterprise.Action WHERE ObjectValue = 'Roles and rights' and ParentActionID = @ParentActionId)
--BEGIN
--	EXEC [Enterprise].[CreateAction] @ProductID = @ProductId, @Action = N'Roles and rights', @ActionTarget = N'Right', @ActionbValueTypeId = @ActionValueID, @Description = '', @ParentActionID = @ParentActionId, @ActionID = @ActionID OUTPUT
--	SELECT	@ActionID as N'@ActionID'
--END
--------

--SELECT @ParentActionId = ActionID FROM Enterprise.Action Where ObjectValue = 'Dashboard' and ParentActionID is NULL
--IF NOT EXISTS (SELECT 1 FROM Enterprise.Action WHERE ObjectValue = 'Client Portal' and ParentActionID = @ParentActionId)
--BEGIN
--	EXEC [Enterprise].[CreateAction] @ProductID = @ProductId, @Action = N'Client Portal', @ActionTarget = N'Right', @ActionbValueTypeId = @ActionValueID, @Description = '', @ParentActionID = @ParentActionId, @ActionID = @ActionID OUTPUT
--	SELECT	@ActionID as N'@ActionID'
--END

--IF NOT EXISTS (SELECT 1 FROM Enterprise.Action WHERE ObjectValue = 'Product Learning Portal' and ParentActionID = @ParentActionId)
--BEGIN
--	EXEC [Enterprise].[CreateAction] @ProductID = @ProductId, @Action = N'Product Learning Portal', @ActionTarget = N'Right', @ActionbValueTypeId = @ActionValueID, @Description = '', @ParentActionID = @ParentActionId, @ActionID = @ActionID OUTPUT
--	SELECT	@ActionID as N'@ActionID'
--END
--IF NOT EXISTS (SELECT 1 FROM Enterprise.Action WHERE ObjectValue = 'Leasing & Rents Conversion Tool' and ParentActionID = @ParentActionId)
--BEGIN
--	EXEC [Enterprise].[CreateAction] @ProductID = @ProductId, @Action = N'Leasing & Rents Conversion Tool', @ActionTarget = N'Right', @ActionbValueTypeId = @ActionValueID, @Description = '', @ParentActionID = @ParentActionId, @ActionID = @ActionID OUTPUT
--	SELECT	@ActionID as N'@ActionID'
--END
--IF NOT EXISTS (SELECT 1 FROM Enterprise.Action WHERE ObjectValue = 'Employee Management' and ParentActionID = @ParentActionId)
--BEGIN
--	EXEC [Enterprise].[CreateAction] @ProductID = @ProductId, @Action = N'Employee Management', @ActionTarget = N'Right', @ActionbValueTypeId = @ActionValueID, @Description = '', @ParentActionID = @ParentActionId, @ActionID = @ActionID OUTPUT
--	SELECT	@ActionID as N'@ActionID'
--END

-----
--SELECT @ParentActionId = ActionID FROM Enterprise.Action Where ObjectValue = 'RolesAndRights' and ParentActionID is NULL
--IF NOT EXISTS (SELECT 1 FROM Enterprise.Action WHERE ObjectValue = 'Manage Other User Roles & Rights' and ParentActionID = @ParentActionId)
--BEGIN
--	EXEC [Enterprise].[CreateAction] @ProductID = @ProductId, @Action = N'Manage Other User Roles & Rights', @ActionTarget = N'Right', @ActionbValueTypeId = @ActionValueID, @Description = '', @ParentActionID = @ParentActionId, @ActionID = @ActionID OUTPUT
--	SELECT	@ActionID as N'@ActionID'
--END

--IF NOT EXISTS (SELECT 1 FROM Enterprise.Action WHERE ObjectValue = 'View Roles & Rights' and ParentActionID = @ParentActionId)
--BEGIN
--	EXEC [Enterprise].[CreateAction] @ProductID = @ProductId, @Action = N'View Roles & Rights', @ActionTarget = N'Right', @ActionbValueTypeId = @ActionValueID, @Description = '', @ParentActionID = @ParentActionId, @ActionID = @ActionID OUTPUT
--	SELECT	@ActionID as N'@ActionID'
--END


------
--IF NOT EXISTS (SELECT 1 FROM Enterprise.[Group] WHERE Value = 'RealPage')
--BEGIN
--	SET IDENTITY_INSERT Enterprise.[Group] ON
--	INSERT [Enterprise].[Group] ([GroupId], [OrganizationPartyId], [Value], [Description], [FromDate], [ThruDate]) VALUES (1, 355, N'Realpage', NULL, CAST(N'2017-08-24T14:00:41.883' AS DateTime), NULL)
--	SET IDENTITY_INSERT Enterprise.[Group] OFF
--END


--IF NOT EXISTS (SELECT 1 FROM Enterprise.Role WHERE value = 'RealPage Super User Role' and PartyID = 33)
--BEGIN

--	EXEC	[Enterprise].[CreateRole]
--			@RoleName = N'RealPage Super User Role',
--			@Description = N'',
--			@RoleTypeID = 0	,
--			@PartyID = 33,
--			@RoleID = @RoleID OUTPUT
--END


--IF NOT EXISTS (SELECT 1 FROM Enterprise.Role WHERE value = 'User Role' and PartyID = 36)
--BEGIN
--	EXEC	[Enterprise].[CreateRole]
--			@RoleName = N'User Role',
--			@Description = N'',
--			@RoleTypeID = 400	,
--			@PartyID = 36,
--			@RoleID = @RoleID OUTPUT
--END

--Declare @RoleName varchar(100)
--SET @RoleName = 'RealPage Super User Role'
--Select @RoleID = RoleId
--	FROM Enterprise.Role
--	WHERE value = @RoleName and PartyId = 33



--Execute Enterprise.CreateRight @RoleId = @RoleId, @RightName='Create User',  @RightID = @RightID OUTPUT,  @Description = 'User will have access to Users List View, Actions and specifically the New User button'
--select @RightId
--Execute Enterprise.CreateRight @RoleId = @RoleId, @RightName='Edit User',  @RightID = @RightID OUTPUT,  @Description = 'User will have access to Users List View and the Action for Edit User to allow Edits to current users'
--select @RightId
--Execute Enterprise.CreateRight @RoleId = @RoleId, @RightName='Edit Profile of Others',  @RightID = @RightID OUTPUT,  @Description = 'User will have access to Users List View, ability to drill into an individuals User record, and click Manage Profile with ability to edit profile data normally managed by the end user'
--select @RightId
--Execute Enterprise.CreateRight @RoleId = @RoleId, @RightName='Lock/Unlock User',  @RightID = @RightID OUTPUT,  @Description = 'User will have access to the Lock and Unlock buttons on the Users List View and User record'
--select @RightId
--Execute Enterprise.CreateRight @RoleId = @RoleId, @RightName='View users',  @RightID = @RightID OUTPUT,  @Description = 'User will have access to Users List View and ability to drill down to see the User record for a user and click a link to View/Manage Profile but make no edits (Profile view excluded for External Users)'
--select @RightId
--Execute Enterprise.CreateRight @RoleId = @RoleId, @RightName='Edit Profile',  @RightID = @RightID OUTPUT,  @Description = 'All users will be able to click Manage Profile.Users without this Right will not be able to edit the first tab for “Profile”.Users will be able to edit Security Questions and access Change Password unless they are using third-party Identity Provider in which case these are moot and hidden.Edit access will NOT include First / Last /Username.'
--select @RightId
--Execute Enterprise.CreateRight @RoleId = @RoleId, @RightName='Edit Password',  @RightID = @RightID OUTPUT,  @Description = 'User will have the access to edit the password on the Add User page. (Administrator cannot change another System Administrators password and cannot change it, but can see other for other user types and can change it.'
--select @RightId
--Execute Enterprise.CreateRight @RoleId = @RoleId, @RightName='Clone User',  @RightID = @RightID OUTPUT,  @Description = 'Provides access to a Clone button on Users List View and on page for a specific User so new user can be created as a clone of selected user(For discussion:could be combined with ADD USER)'
--select @RightId
--Execute Enterprise.CreateRight @RoleId = @RoleId, @RightName='Manage Roles & Rights',  @RightID = @RightID OUTPUT,  @Description = 'Access to Roles & Rights link on side menu and all edit capability provided'
--select @RightId
--Execute Enterprise.CreateRight @RoleId = @RoleId, @RightName='View Roles & Rights',  @RightID = @RightID OUTPUT,  @Description = 'Access to Roles & Rights link on side menu with ability to VIEW and NOT CHANGE any data'
--select @RightId
--Execute Enterprise.CreateRight @RoleId = @RoleId, @RightName='Access to Client Portal',  @RightID = @RightID OUTPUT,  @Description = 'Exposes link on Resources section for “Client Portal” '
--select @RightId
--Execute Enterprise.CreateRight @RoleId = @RoleId, @RightName='Access to Product Learning Portal',  @RightID = @RightID OUTPUT,  @Description = 'Exposes link on Resources section for “Product Learning Portal”(Use Cases:Customer buys LMS or wants users to access content within their separate LMS only.)'
--select @RightId
--Execute Enterprise.CreateRight @RoleId = @RoleId, @RightName='Access to Leasing & Rents Conversion Tool',  @RightID = @RightID OUTPUT,  @Description = 'Exposes link on Resources section for “Leasing & Rents Conversion Tool” which provides access to this external application without an extra login.May default to “Internal Only” at Clay Hannah’s discretion. NOTE:Link to be added when this integration is addressed.'
--select @RightId
--Execute Enterprise.CreateRight @RoleId = @RoleId, @RightName='Access to Identity Provider Configuration Page',  @RightID = @RightID OUTPUT,  @Description = 'Defaults to “Internal Only” until specified otherwise.'
--select @RightId
--Execute Enterprise.CreateRight @RoleId = @RoleId, @RightName='Access to Green Book Migration Tool',  @RightID = @RightID OUTPUT,  @Description = 'May default to Internal Only so we can suppress for new customers.Once customers are adopted + 90 days this should no longer be available #discuss'
--select @RightId
--Execute Enterprise.CreateRight @RoleId = @RoleId, @RightName='Access to Amenities Tool',  @RightID = @RightID OUTPUT,  @Description = 'Rights and Roles control for the new Amenities tool should leverage Green Book Rights'
--select @RightId
--Execute Enterprise.CreateRight @RoleId = @RoleId, @RightName='Access to Property Hierarchy Tool',  @RightID = @RightID OUTPUT,  @Description = 'Rights and Roles control for the new Property Hierarchy tool should leverage Green Book Rights'
--select @RightId
--Execute Enterprise.CreateRight @RoleId = @RoleId, @RightName='Access to Employee Management',  @RightID = @RightID OUTPUT,  @Description = 'We will want to support granular management of employee access'
--select @RightId
--Execute Enterprise.CreateRight @RoleId = @RoleId, @RightName='View Audit Trail on User Data',  @RightID = @RightID OUTPUT,  @Description = 'User will have ability to see a section on the page which shows date / time of edits with old and new values'
--select @RightId
--Execute Enterprise.CreateRight @RoleId = @RoleId, @RightName='View Audit Trail on Profile Data',  @RightID = @RightID OUTPUT,  @Description = 'User will have ability to see a section on the page which shows date / time of edits with old and new values'
--select @RightId
--Execute Enterprise.CreateRight @RoleId = @RoleId, @RightName='Impersonate a User',  @RightID = @RightID OUTPUT,  @Description = 'Provides the ability to access a button on the Users List View for impersonating a user to troubleshoot their access (Right must exist when functionality created.)'
--select @RightId
--Execute Enterprise.CreateRight @RoleId = @RoleId, @RightName='See All RealPage Products',  @RightID = @RightID OUTPUT,  @Description = 'With this right, user sees all available Products on this page.Otherwise, only Products to which the end user has access is available.'
--select @RightId
--Execute Enterprise.CreateRight @RoleId = @RoleId, @RightName='Default_SideMenu_Admin',  @RightID = @RightID OUTPUT,  @Description = 'Rights for SideMenufor super users'
--select @RightId
--Execute Enterprise.CreateRight @RoleId = @RoleId, @RightName='Default_Dashboard_Admin',  @RightID = @RightID OUTPUT,  @Description = 'Rights for dashboard for superusers'
--select @RightId


--SET @RoleName = 'User Role'
--Select @RoleID = RoleId
--	FROM Enterprise.Role
--	WHERE value = @RoleName and PartyId = 33

--Execute Enterprise.CreateRight @RoleId = @RoleId, @RightName='Ability to edit my own profile', 	@RightID = @RightID OUTPUT,  @Description = 'All users will be able to click Manage Profile.  Users without this Right will not be able to edit the first tab for “Profile”.  Users will be able to edit Security Questions and access Change Password unless they are using third-party Identity Provider in which case these are moot and hidden.  Edit access will NOT include First / Last /  Username.'
--select @RightId
--Execute Enterprise.CreateRight @RoleId = @RoleId, @RightName='Access to Client Portal', @RightID = @RightID OUTPUT,  @Description = 'Exposes link on Resources section for “Client Portal” '
--select @RightId
--Execute Enterprise.CreateRight @RoleId = @RoleId, @RightName='Access to Product Learning Portal', @RightID = @RightID OUTPUT,  @Description = 'Exposes link on Resources section for “Product Learning Portal”  (Use Cases:  Customer buys LMS or wants users to access content within their separate LMS only.)'
--select @RightId
--Execute Enterprise.CreateRight @RoleId = @RoleId, @RightName='Default_SideMenu_Users', @RightID = @RightID OUTPUT,  @Description = 'Exposes link on Resources section for “Product Learning Portal”  (Use Cases:  Customer buys LMS or wants users to access content within their separate LMS only.)'
--select @RightId
--Execute Enterprise.CreateRight @RoleId = @RoleId, @RightName='Default_Dashboard_Users', @RightID = @RightID OUTPUT,  @Description = 'Exposes link on Resources section for “Product Learning Portal”  (Use Cases:  Customer buys LMS or wants users to access content within their separate LMS only.)'
--select @RightId



--DECLARE	@UserActionId int


--SELECT @Status = StatusType.StatusTypeID
--FROM Enterprise.StatusTypeCategoryType
--JOIN Enterprise.StatusTypeCategory ON StatusTypeCategory.StatusTypeCategoryTypeId = StatusTypeCategoryType.StatusTypeCategoryTypeId
--JOIN Enterprise.StatusTypeCategoryClassification ON StatusTypeCategoryClassification.StatusTypeCategoryId = StatusTypeCategory.StatusTypeCategoryId
--JOIN Enterprise.StatusType ON StatusType.StatusTypeId = StatusTypeCategoryClassification.StatusTypeId
--	WHERE StatusType.name = 'ALL'
--		AND StatusTypeCategoryType.Name = 'Security'
--SELECT @ActionID = ActionID FROM Enterprise.Action WHERE ObjectValue = 'Userslist' and ObjectType = 'ROUTE'
--SELECT @RoleID = RoleID FROM Enterprise.Role WHere value = 'RealPage Super User Role'
--SELECT @RightID = RightId FROM Enterprise.[Right] WHERE Value = 'Create User' and RoleId = @RoleID
--EXEC	[Enterprise].[LinkActionToRights]  @ActionID = @ActionID, @RightId = @RightId,	@StatusId = @Status,	@UserActionId = @UserActionId OUTPUT

--SELECT @ActionID = ActionID FROM Enterprise.Action WHERE ObjectValue = 'EditUser' and ObjectType = 'ROUTE'
--SELECT @RoleID = RoleID FROM Enterprise.Role WHere value = 'RealPage Super User Role'
--SELECT @RightID = RightId FROM Enterprise.[Right] WHERE Value = 'Edit User' and RoleId = @RoleID
--EXEC	[Enterprise].[LinkActionToRights]  @ActionID = @ActionID, @RightId = @RightId,	@StatusId = @Status,	@UserActionId = @UserActionId OUTPUT

--SELECT @ActionID = ActionID FROM Enterprise.Action WHERE ObjectValue = 'SideMenu' and ObjectType = 'ROUTE'
--SELECT @RoleID = RoleID FROM Enterprise.Role WHere value = 'RealPage Super User Role'
--SELECT @RightID = RightId FROM Enterprise.[Right] WHERE Value = 'Default_SideMenu_Admin' and RoleId = @RoleID
--EXEC	[Enterprise].[LinkActionToRights]  @ActionID = @ActionID, @RightId = @RightId,	@StatusId = @Status,	@UserActionId = @UserActionId OUTPUT

--SELECT @ActionID = ActionID FROM Enterprise.Action WHERE ObjectValue = 'Dashboard' and ObjectType = 'ROUTE'
--SELECT @RoleID = RoleID FROM Enterprise.Role WHere value = 'RealPage Super User Role'
--SELECT @RightID = RightId FROM Enterprise.[Right] WHERE Value = 'Default_Dashboard_Admin' and RoleId = @RoleID
--EXEC	[Enterprise].[LinkActionToRights]  @ActionID = @ActionID, @RightId = @RightId,	@StatusId = @Status,	@UserActionId = @UserActionId OUTPUT

--SELECT @ActionID = ActionID FROM Enterprise.Action WHERE ObjectValue = 'RolesAndRights' and ObjectType = 'ROUTE'
--SELECT @RoleID = RoleID FROM Enterprise.Role WHere value = 'RealPage Super User Role'
--SELECT @RightID = RightId FROM Enterprise.[Right] WHERE Value = 'Manage Roles & Rights' and RoleId = @RoleID
--EXEC	[Enterprise].[LinkActionToRights]  @ActionID = @ActionID, @RightId = @RightId,	@StatusId = @Status,	@UserActionId = @UserActionId OUTPUT

--SELECT @ActionID = ActionID FROM Enterprise.Action WHERE ObjectValue = 'SideMenu' and ObjectType = 'ROUTE'
--SELECT @RoleID = RoleID FROM Enterprise.Role WHere value = 'User Role'
--SELECT @RightID = RightId FROM Enterprise.[Right] WHERE Value = 'Default_SideMenu_Users' and RoleId = @RoleID
--EXEC	[Enterprise].[LinkActionToRights]  @ActionID = @ActionID, @RightId = @RightId,	@StatusId = @Status,	@UserActionId = @UserActionId OUTPUT

--SELECT @ActionID = ActionID FROM Enterprise.Action WHERE ObjectValue = 'Dashboard' and ObjectType = 'ROUTE'
--SELECT @RoleID = RoleID FROM Enterprise.Role WHere value = 'User Role'
--SELECT @RightID = RightId FROM Enterprise.[Right] WHERE Value = 'Default_Dashboard_Users' and RoleId = @RoleID
--EXEC	[Enterprise].[LinkActionToRights]  @ActionID = @ActionID, @RightId = @RightId,	@StatusId = @Status,	@UserActionId = @UserActionId OUTPUT

--/*Link Person to Roles*/

--SELECT @RoleID =  RoleID FROM Enterprise.Role where value = 'RealPage Super User Role'


--DECLARE @PersonaPrivilgeID INT 
--EXEC   [Enterprise].[LinkPersonaToRole]
--             @PersonaID = 33,
--             @RoleID = @RoleId, 
--             @PersonaPrivilgeID = @PersonaPrivilgeID OUTPUT


--EXEC   [Enterprise].[LinkPersonaToRole]
--             @PersonaID = 36,
--             @RoleID = @RoleId, 
--             @PersonaPrivilgeID = @PersonaPrivilgeID OUTPUT

--SELECT @RoleID =  RoleID FROM Enterprise.Role where value = 'User Role' and PartyID = 36

--EXEC   [Enterprise].[LinkPersonaToRole]
--             @PersonaID = 36,
--             @RoleID = @RoleId, 
--             @PersonaPrivilgeID = @PersonaPrivilgeID OUTPUT


EXEC sys.sp_updateextendedproperty @name=N'Build', @value='11'


