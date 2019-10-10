--FIX Right-Route Mapping issues.


--FIX Right-Route Mapping issues.

--DECLARE @ActivateDeactivateUser int;
DECLARE @ResendInvite int;
DECLARE @Lock INT
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
--DECLARE @PartyId INT;
--DECLARE @RightName VARCHAR(100);
DECLARE @RVT INT;
DECLARE @DefaultRoute NVARCHAR(200);
--DECLARE @RightValueTypeId INT;
DECLARE @StatusId INT;
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

SELECT @ProductId = ProductId
FROM Enterprise.Product
WHERE Name = 'Unified Login';

SELECT @StatusId = StatusType.StatusTypeID
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

SET @DefaultRoute = 'DefaultRouteFor_RVT_'+CONVERT(nvarchar, @RVT)+'_'+CONVERT(nvarchar, @PartyId);

IF EXISTS
(
	SELECT 1
	FROM Enterprise.UserActions
	WHERE ActionId IS NULL
)
BEGIN
	DELETE FROM Enterprise.UserActions
	WHERE ActionID IS NULL;
END;

DECLARE RightList CURSOR
FOR SELECT DISTINCT 
		   R.RightId, R.PartyId, RVT.RightValueTypeId
	FROM Enterprise.[Right] AS R
		 LEFT OUTER JOIN
		 Enterprise.RightValueType AS RVT
		 ON RVT.RightValueTypeId = R.RightValueTypeId
		 LEFT OUTER JOIN
		 Enterprise.UserActions AS U
		 ON U.RightId = R.RightId
		 LEFT OUTER JOIN
		 Enterprise.ACTION AS A
		 ON A.ActionId = U.ActionId
	WHERE A.ObjectValue IS NULL;

OPEN RightList;

FETCH RightList INTO @RightId, @PartyId, @RightValueTypeId;

WHILE @@FETCH_STATUS = 0
BEGIN
	SET @DefaultRoute = 'DefaultRouteFor_RVT_'+CONVERT(nvarchar, @RightValueTypeId)+'_'+CONVERT(nvarchar, @RightId)+'_'+CONVERT(nvarchar, @PartyId);
	IF NOT EXISTS
(
	SELECT 1
	FROM Enterprise.ACTION
	WHERE ObjectValue = @DefaultRoute
)
	BEGIN
		SELECT @ActionValueID = [ActionValueTypeID]
		FROM Enterprise.ActionValueType
		WHERE Value = 'ROUTE';
		EXEC [Enterprise].[CreateAction] @ProductID = @ProductId, @Action = @DefaultRoute, @ActionTarget = N'Route', @ActionbValueTypeId = @ActionValueID, @Description = 'DefaultRoute', @ActionID = @ActionID OUTPUT;
		EXEC [Enterprise].[LinkActionToRights] @ActionID = @ActionID, @RightId = @RightId, @StatusId = @StatusId, @UserActionId = @UserActionId OUTPUT;
		FETCH RightList INTO @RightId, @PartyId, @RightValueTypeId;
	END;
END;

CLOSE RightList;

DEALLOCATE RightList;

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
WHERE RVT.VALUE IN( 'Access to Stars Report', 'Access to Leasing & Rents Conversion Tool', 'Access to Identity Provider Configuration Page', 'Access to Amenities Tool', 'Access to Property Hierarchy Tool', 'Access to Employee Management', 'Ability to Impersonate a User', 'Impersonate a User', 'Access to Client Portal' );

COMMIT;


--ADD New rights for the organization


SET @FromDate = GETUTCDATE();

IF OBJECT_ID('tempdb..#NewRight') IS NOT NULL
BEGIN
	DROP TABLE #NewRight;
END;

CREATE TABLE #NewRight
( 
			 RightId int, Name nvarchar(500), description nvarchar(500)
);

INSERT INTO #NewRight( rightid, name, description )
VALUES( 1, 'Ability to view Company page', 'Ability to view Company page' ), ( 2, 'Ability to edit Company information', 'Ability to edit Company information' ), ( 3, 'Ability to view Property page', 'Ability to view Property page' ), ( 4, 'Ability to edit Property information', 'Ability to edit Property information' );

SELECT @ProductId = ProductId
FROM Enterprise.Product
WHERE name = 'Unified Login';

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
		FOR SELECT RightId, Name, Description
			FROM #NewRight;
		OPEN Rights;
		FETCH Rights INTO @TRightId, @TRightName, @TRightDesc;
		WHILE @@FETCH_STATUS = 0
		BEGIN
			EXECUTE Enterprise.CreateRight @RoleId = @RoleId, @RightName = @TRightName, @RightCategoryId = @RightCategory, @PartyId = @PartyId, @ProductId = @ProductId, @Shortname = '', @Description = @TRightDesc, @RightId = @RightId OUTPUT;
			FETCH Rights INTO @TRightId, @TRightName, @TRightDesc;
		END;
		CLOSE Rights;
		DEALLOCATE Rights;
		UPDATE #HoldPartyFornewRights
		  SET PStatus = 1
		WHERE RowNumber = @PartyRowNum;
	END;


--CREATE Standalone Rights

IF NOT EXISTS
(
	SELECT 1
	FROM Enterprise.ACTION
	WHERE ObjectValue = 'View Company Page' AND 
		  ParentActionId IS NULL
)
BEGIN
	EXEC [Enterprise].[CreateAction] @ProductID = @ProductId, @Action = N'View Company Page', @ActionTarget = N'Right', @ActionbValueTypeId = @ActionValueID, @Description = 'Display User list for UsersList Route', @ActionID = @ActionID OUTPUT;
	SELECT @ActionID AS N'@ActionID';
END;

IF NOT EXISTS
(
	SELECT 1
	FROM Enterprise.ACTION
	WHERE ObjectValue = 'Edit Company Information' AND 
		  ParentActionId IS NULL
)
BEGIN
	EXEC [Enterprise].[CreateAction] @ProductID = @ProductId, @Action = N'Edit Company Information', @ActionTarget = N'Right', @ActionbValueTypeId = @ActionValueID, @Description = 'Display User list for UsersList Route', @ActionID = @ActionID OUTPUT;
	SELECT @ActionID AS N'@ActionID';
END;

IF NOT EXISTS
(
	SELECT 1
	FROM Enterprise.ACTION
	WHERE ObjectValue = 'View Property Page' AND 
		  ParentActionId IS NULL
)
BEGIN
	EXEC [Enterprise].[CreateAction] @ProductID = @ProductId, @Action = N'View Property Page', @ActionTarget = N'Right', @ActionbValueTypeId = @ActionValueID, @Description = 'Display User list for UsersList Route', @ActionID = @ActionID OUTPUT;
	SELECT @ActionID AS N'@ActionID';
END;

IF NOT EXISTS
(
	SELECT 1
	FROM Enterprise.ACTION
	WHERE ObjectValue = 'Edit Property Information' AND 
		  ParentActionId IS NULL
)
BEGIN
	EXEC [Enterprise].[CreateAction] @ProductID = @ProductId, @Action = N'Edit Property Information', @ActionTarget = N'Right', @ActionbValueTypeId = @ActionValueID, @Description = 'Display User list for UsersList Route', @ActionID = @ActionID OUTPUT;
	SELECT @ActionID AS N'@ActionID';
END;

IF NOT EXISTS
(
	SELECT 1
	FROM Enterprise.ACTION
	WHERE ObjectValue = 'Product Learning Portal' AND 
		  ParentActionId IS NULL
)
BEGIN
	EXEC [Enterprise].[CreateAction] @ProductID = @ProductId, @Action = N'Product Learning Portal', @ActionTarget = N'Right', @ActionbValueTypeId = @ActionValueID, @Description = 'Display User list for UsersList Route', @ActionID = @ActionID OUTPUT;
	SELECT @ActionID AS N'@ActionID';
END;

IF NOT EXISTS
(
	SELECT 1
	FROM Enterprise.ACTION
	WHERE ObjectValue = 'Lock/Unlock User' AND 
		  ParentActionId IS NULL
)
BEGIN
	EXEC [Enterprise].[CreateAction] @ProductID = @ProductId, @Action = N'Lock/Unlock User', @ActionTarget = N'Right', @ActionbValueTypeId = @ActionValueID, @Description = 'Display User list for UsersList Route', @ActionID = @ActionID OUTPUT;
	SELECT @ActionID AS N'@ActionID';
END;

IF NOT EXISTS
(
	SELECT 1
	FROM Enterprise.ACTION
	WHERE ObjectValue = 'Audit Trail Profile Data' AND 
		  ParentActionId IS NULL
)
BEGIN
	EXEC [Enterprise].[CreateAction] @ProductID = @ProductId, @Action = N'Audit Trail Profile Data', @ActionTarget = N'Right', @ActionbValueTypeId = @ActionValueID, @Description = 'Display User list for UsersList Route', @ActionID = @ActionID OUTPUT;
	SELECT @ActionID AS N'@ActionID';
END;

IF NOT EXISTS
(
	SELECT 1
	FROM Enterprise.ACTION
	WHERE ObjectValue = 'View RealPage Products' AND 
		  ParentActionId IS NULL
)
BEGIN
	EXEC [Enterprise].[CreateAction] @ProductID = @ProductId, @Action = N'View RealPage Products', @ActionTarget = N'Right', @ActionbValueTypeId = @ActionValueID, @Description = 'Display User list for UsersList Route', @ActionID = @ActionID OUTPUT;
	SELECT @ActionID AS N'@ActionID';
END;

SELECT DISTINCT 
	   IDENTITY(int, 1, 1) AS RowNumber, OrganizationPartyID, 0 AS PStatus
INTO #HoldOrgsUserRoutesA
FROM Person.Persona;

WHILE EXISTS
(
	SELECT 1
	FROM #HoldOrgsUserRoutesA
	WHERE PStatus = 0
)
BEGIN
	SELECT TOP 1 @OrgRowNum = Rownumber, @OrgID = OrganizationPartyID
	FROM #HoldOrgsUserRoutesA
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
	-->
	EXECUTE Enterprise.CreateRight @RoleId = -1, @RightName = 'Default_ViewCompanyPage', @ShortName = 'ViewCompanyPage', @RightCategoryId = @RightCategoryId, @PartyId = @OrgId, @ProductId = @ProductId, @Description = 'Enable/Disable People To view company information', @RightId = @RightId OUTPUT;
	SELECT @ActionID = ActionID
	FROM Enterprise.ACTION
	WHERE ObjectValue = 'View Company Page' AND 
		  ObjectType = 'Right' AND 
		  ParentActionId IS NULL;
	EXEC [Enterprise].[LinkActionToRights] @ActionID = @ActionID, @RightId = @RightId, @StatusId = @Status, @UserActionId = @UserActionId OUTPUT;
	-->
	EXECUTE Enterprise.CreateRight @RoleId = -1, @RightName = 'Default_EditCompany', @ShortName = 'EditCompanyPage', @RightCategoryId = @RightCategoryId, @PartyId = @OrgId, @ProductId = @ProductId, @Description = 'Enable/Disable People To edit company information', @RightId = @RightId OUTPUT;
	SELECT @ActionID = ActionID
	FROM Enterprise.ACTION
	WHERE ObjectValue = 'Edit Company Page' AND 
		  ObjectType = 'Right' AND 
		  ParentActionId IS NULL;
	EXEC [Enterprise].[LinkActionToRights] @ActionID = @ActionID, @RightId = @RightId, @StatusId = @Status, @UserActionId = @UserActionId OUTPUT;
	-->
	EXECUTE Enterprise.CreateRight @RoleId = -1, @RightName = 'Default_ViewProperty', @ShortName = 'ViewProperty', @RightCategoryId = @RightCategoryId, @PartyId = @OrgId, @ProductId = @ProductId, @Description = 'Enable/Disable People To view property information', @RightId = @RightId OUTPUT;
	SELECT @ActionID = ActionID
	FROM Enterprise.ACTION
	WHERE ObjectValue = 'View Property Page' AND 
		  ObjectType = 'Right' AND 
		  ParentActionId IS NULL;
	EXEC [Enterprise].[LinkActionToRights] @ActionID = @ActionID, @RightId = @RightId, @StatusId = @Status, @UserActionId = @UserActionId OUTPUT;
	-->
	EXECUTE Enterprise.CreateRight @RoleId = -1, @RightName = 'Default_EditProperty', @ShortName = 'EditProperty', @RightCategoryId = @RightCategoryId, @PartyId = @OrgId, @ProductId = @ProductId, @Description = 'Enable/Disable People To edit property information', @RightId = @RightId OUTPUT;
	SELECT @ActionID = ActionID
	FROM Enterprise.ACTION
	WHERE ObjectValue = 'Edit Property Page' AND 
		  ObjectType = 'Right' AND 
		  ParentActionId IS NULL;
	EXEC [Enterprise].[LinkActionToRights] @ActionID = @ActionID, @RightId = @RightId, @StatusId = @Status, @UserActionId = @UserActionId OUTPUT;
	-->
	EXECUTE Enterprise.CreateRight @RoleId = -1, @RightName = 'Default_ProductLearningPortal', @ShortName = 'ProductLearningPortal', @RightCategoryId = @RightCategoryId, @PartyId = @OrgId, @ProductId = @ProductId, @Description = 'Enable/Disable People To edit property information', @RightId = @RightId OUTPUT;
	SELECT @ActionID = ActionID
	FROM Enterprise.ACTION
	WHERE ObjectValue = 'Product Learning Portal' AND 
		  ObjectType = 'Right' AND 
		  ParentActionId IS NULL;
	EXEC [Enterprise].[LinkActionToRights] @ActionID = @ActionID, @RightId = @RightId, @StatusId = @Status, @UserActionId = @UserActionId OUTPUT;
	-->
	EXECUTE Enterprise.CreateRight @RoleId = -1, @RightName = 'Default_LockUnLockUser', @ShortName = 'LockUnLockUser', @RightCategoryId = @RightCategoryId, @PartyId = @OrgId, @ProductId = @ProductId, @Description = 'Enable/Disable People To edit property information', @RightId = @RightId OUTPUT;
	SELECT @ActionID = ActionID
	FROM Enterprise.ACTION
	WHERE ObjectValue = 'Lock/Unlock User' AND 
		  ObjectType = 'Right' AND 
		  ParentActionId IS NULL;
	EXEC [Enterprise].[LinkActionToRights] @ActionID = @ActionID, @RightId = @RightId, @StatusId = @Status, @UserActionId = @UserActionId OUTPUT;
	-->
	EXECUTE Enterprise.CreateRight @RoleId = -1, @RightName = 'Default_ViewAuditTrailData', @ShortName = 'ViewAuditTrailData', @RightCategoryId = @RightCategoryId, @PartyId = @OrgId, @ProductId = @ProductId, @Description = 'Enable/Disable People To edit property information', @RightId = @RightId OUTPUT;
	SELECT @ActionID = ActionID
	FROM Enterprise.ACTION
	WHERE ObjectValue = 'View Audit Trail Data' AND 
		  ObjectType = 'Right' AND 
		  ParentActionId IS NULL;
	EXEC [Enterprise].[LinkActionToRights] @ActionID = @ActionID, @RightId = @RightId, @StatusId = @Status, @UserActionId = @UserActionId OUTPUT;
	-->
	EXECUTE Enterprise.CreateRight @RoleId = -1, @RightName = 'Default_ViewRealPageProducts', @ShortName = 'ViewAuditTrailData', @RightCategoryId = @RightCategoryId, @PartyId = @OrgId, @ProductId = @ProductId, @Description = 'Enable/Disable People To edit property information', @RightId = @RightId OUTPUT;
	SELECT @ActionID = ActionID
	FROM Enterprise.ACTION
	WHERE ObjectValue = 'View RealPage Products' AND 
		  ObjectType = 'Right' AND 
		  ParentActionId IS NULL;
	EXEC [Enterprise].[LinkActionToRights] @ActionID = @ActionID, @RightId = @RightId, @StatusId = @Status, @UserActionId = @UserActionId OUTPUT;
	-->


	UPDATE #HoldOrgsUserRoutesA
	  SET PStatus = 1
	WHERE RowNumber = @OrgRowNum;
END;

--Fix Product Learning Portal



--Add CloneUser Route/Right


IF OBJECT_ID('tempdb..#HoldOrgsForCloneRoute') IS NOT NULL
BEGIN
	DROP TABLE #HoldOrgsForCloneRoute
END;

SELECT DISTINCT 
	   IDENTITY(int, 1, 1) AS RowNumber, OrganizationPartyID, 0 AS PStatus
INTO #HoldOrgsForCloneRoute
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

--SET @OrgID = 350;

WHILE EXISTS
(
	SELECT 1
	FROM #HoldOrgsForCloneRoute
	WHERE PStatus = 0
)
BEGIN
	SELECT TOP 1 @OrgRowNum = Rownumber, @OrgID = OrganizationPartyID
	FROM #HoldOrgsForCloneRoute
	WHERE PStatus = 0;
	SELECT @ActionValueID = [ActionValueTypeID]
	FROM Enterprise.ActionValueType
	WHERE Value = 'ROUTE';
	SELECT @ProductID = ProductId
	FROM Enterprise.Product
	WHERE Name = 'Unified Login';
	IF NOT EXISTS
(
	SELECT 1
	FROM Enterprise.ACTION
	WHERE ObjectValue = 'CloneUser' AND 
		  Description = 'SuperUser'
)
	BEGIN
		EXEC [Enterprise].[CreateAction] @ProductID = @ProductId, @Action = N'CloneUser', @ActionTarget = N'Route', @ActionbValueTypeId = @ActionValueID, @Description = 'SuperUser', @ActionID = @ActionID OUTPUT;
		SELECT @ActionID AS N'@ActionID';
	END;
	SELECT @ParentActionId = ActionID
	FROM Enterprise.ACTION
	WHERE ObjectValue = 'CloneUser' AND 
		  ParentActionID IS NULL AND 
		  Description = 'SuperUser';
	IF NOT EXISTS
(
	SELECT 1
	FROM Enterprise.ACTION
	WHERE ObjectValue = 'Edit User' AND 
		  ParentActionID = @ParentActionId
)
	BEGIN
		EXEC [Enterprise].[CreateAction] @ProductID = @ProductId, @Action = N'Edit User', @ActionTarget = N'Right', @ActionbValueTypeId = @ActionValueID, @Description = '', @ParentActionID = @ParentActionId, @ActionID = @ActionID OUTPUT;
		SELECT @ActionID AS N'@ActionID';
	END;
	SELECT @ActionID = ActionID
	FROM Enterprise.ACTION
	WHERE ObjectValue = 'CloneUser' AND 
		  ObjectType = 'ROUTE' AND 
		  Description = 'SuperUser';
	SELECT @RoleID = RoleID
	FROM Enterprise.Role AS R
		 INNER JOIN
		 Enterprise.RoleValueType AS RR
		 ON RR.RoleValueTypeId = R.RoleValueTypeId
	WHERE RR.value IN( 'User Administrator' ) AND 
		  PartyID = @OrgID;
	SELECT @RightID = RightId
	FROM Enterprise.[Right] AS R
		 INNER JOIN
		 Enterprise.RightValueType AS RR
		 ON RR.RightValueTypeId = R.RightValueTypeId
	WHERE Value = 'Ability to clone users' AND 
		  RoleId = @RoleID;
	EXEC [Enterprise].[LinkActionToRights] @ActionID = @ActionID, @RightId = @RightId, @StatusId = @Status, @UserActionId = @UserActionId OUTPUT;
	UPDATE #HoldOrgsForCloneRoute
	  SET PStatus = 1
	WHERE RowNumber = @OrgRowNum;
END;

SELECT DISTINCT 
	   IDENTITY(int, 1, 1) AS RowNumber, OrganizationPartyID, 0 AS PStatus
INTO #HoldOrgsCloneRoutes
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
	FROM Enterprise.ACTION
	WHERE ObjectValue = 'CloneUser' AND 
		  ParentActionId IS NULL AND 
		  Description = 'User'
)
BEGIN
	EXEC [Enterprise].[CreateAction] @ProductID = @ProductId, @Action = N'CloneUser', @ActionTarget = N'Route', @ActionbValueTypeId = @ActionValueID, @Description = 'Cloneuser Route for custom roles.', @ActionID = @ActionID OUTPUT;
	SELECT @ActionID AS N'@ActionID';
END;

WHILE EXISTS
(
	SELECT 1
	FROM #HoldOrgsCloneRoutes
	WHERE PStatus = 0
)
BEGIN
	SELECT TOP 1 @OrgRowNum = Rownumber, @OrgID = OrganizationPartyID
	FROM #HoldOrgsCloneRoutes
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
	EXECUTE Enterprise.CreateRight @RoleId = -1, @RightName = 'Default_CloneUser_Route', @ShortName = 'CloneUserRoute', @RightCategoryId = @RightCategoryId, @PartyId = @OrgId, @ProductId = @ProductId, @Description = 'Edit User', @RightId = @RightId OUTPUT;
	SELECT @ActionID = ActionID
	FROM Enterprise.ACTION
	WHERE ObjectValue = 'CloneUser' AND 
		  ObjectType = 'Route' AND 
		  ParentActionId IS NULL;
	EXEC [Enterprise].[LinkActionToRights] @ActionID = @ActionID, @RightId = @RightId, @StatusId = @Status, @UserActionId = @UserActionId OUTPUT;
	UPDATE #HoldOrgsCloneRoutes
	  SET PStatus = 1
	WHERE RowNumber = @OrgRowNum;
END;

--Create Right-Route Mapping


DECLARE @CloneUserRoute int;

SELECT @CloneUserRoute = RightValueTypeId
FROM Enterprise.RightValueType
WHERE value = 'Default_CloneUser_Route';

SELECT @RightValueTypeId = RightValueTypeId
FROM Enterprise.RightValueType
WHERE value IN( 'Ability to Clone Users' );

IF NOT EXISTS
(
	SELECT 1
	FROM Enterprise.RightDependency
	WHERE RightValueTypeId = @RightValueTypeId AND 
		  DependentRightValueTypeId = @CloneUserRoute
)
BEGIN
	INSERT INTO Enterprise.RightDependency( RightValueTypeId, DependentRightValueTypeId )
	VALUES( @RightValueTypeId, @CloneUserRoute );
END;

--Fix Activate/deactivate, Lock/Unlocl


SET @FromDate = GETUTCDATE();

IF OBJECT_ID('tempdb..#HoldPartyFornewRightsA') IS NOT NULL
BEGIN
	DROP TABLE #HoldPartyFornewRightsA;
END;

IF OBJECT_ID('tempdb..#NewRightB') IS NOT NULL
BEGIN
	DROP TABLE #NewRightB;
END;

CREATE TABLE #NewRightB
( 
			 RightId int, Name nvarchar(500), description nvarchar(500)
);

INSERT INTO #NewRightB( rightid, name, description )
VALUES( 1, 'Ability to Activate/Deactivate User', 'Ability to Activate/Deactivate User' ), (2, 'Ability to Resend Invite', 'Ability to Resend Invite')

SELECT @ProductId = ProductId
FROM Enterprise.Product
WHERE name = 'Unified Login';

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
INTO #HoldPartyFornewRightsA
FROM Person.Persona;

WHILE EXISTS
(
	SELECT 1
	FROM #HoldPartyFornewRightsA
	WHERE PStatus = 0
)
	BEGIN
		SELECT TOP 1 @PartyRowNum = Rownumber, @PartyId = OrganizationPartyID
		FROM #HoldPartyFornewRightsA
		WHERE PStatus = 0;
		SELECT @RoleId = RoleId
		FROM Enterprise.Role AS R
			 INNER JOIN
			 Enterprise.RoleValueType AS RR
			 ON RR.RoleValueTypeId = R.RoleValueTypeId
		WHERE RR.Value = 'User Administrator' AND 
			  R.PartyId = @PartyId;
		DECLARE Rights CURSOR
		FOR SELECT RightId, Name, Description
			FROM #NewRightB;
		OPEN Rights;
		FETCH Rights INTO @TRightId, @TRightName, @TRightDesc;
		WHILE @@FETCH_STATUS = 0
		BEGIN
			EXECUTE Enterprise.CreateRight @RoleId = @RoleId, @RightName = @TRightName, @RightCategoryId = @RightCategory, @PartyId = @PartyId, @ProductId = @ProductId, @Shortname = '', @Description = @TRightDesc, @RightId = @RightId OUTPUT;
			FETCH Rights INTO @TRightId, @TRightName, @TRightDesc;
		END;
		CLOSE Rights;
		DEALLOCATE Rights;
		UPDATE #HoldPartyFornewRightsA
		  SET PStatus = 1
		WHERE RowNumber = @PartyRowNum;
	END;


SELECT @ActivateDeactivateUser = RightValueTypeId
FROM Enterprise.RightValueType
WHERE value = 'Default_ActivateDeActivateUser';

SELECT @RightValueTypeId = RightValueTypeId
FROM Enterprise.RightValueType
WHERE value IN( 'Ability to Activate/Deactivate User' );

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



SELECT @ResendInvite = RightValueTypeId
FROM Enterprise.RightValueType
WHERE value = 'Default_ResendInvitation';

SELECT @RightValueTypeId = RightValueTypeId
FROM Enterprise.RightValueType
WHERE value IN( 'Ability to Resend Invite' );

IF NOT EXISTS
(
	SELECT 1
	FROM Enterprise.RightDependency
	WHERE RightValueTypeId = @RightValueTypeId AND 
		  DependentRightValueTypeId = @ResendInvite
)
BEGIN
	INSERT INTO Enterprise.RightDependency( RightValueTypeId, DependentRightValueTypeId )
	VALUES( @RightValueTypeId, @ResendInvite );
END;

SELECT @Lock = RightValueTypeId
FROM Enterprise.RightValueType
WHERE value = 'Default_LockUnLockUser';

SELECT @RightValueTypeId = RightValueTypeId
FROM Enterprise.RightValueType
WHERE value IN( 'Ability to lock/unlock users' );

IF NOT EXISTS
(
	SELECT 1
	FROM Enterprise.RightDependency
	WHERE RightValueTypeId = @RightValueTypeId AND 
		  DependentRightValueTypeId = @Lock
)
BEGIN
	INSERT INTO Enterprise.RightDependency( RightValueTypeId, DependentRightValueTypeId )
	VALUES( @RightValueTypeId, @Lock );
END;


SELECT @ProductLearning = RightValueTypeId
FROM Enterprise.RightValueType
WHERE value = 'Default_ProductLearningPortal';



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

UPDATE Enterprise.RightValueType
    SET ShortName =  'resendinvitation'
WHERE Value = 'Ability to Resend Invite'

UPDATE Enterprise.RightValueType
    SET ShortName =  'activatedeactivateusers'
WHERE Value = 'Ability to Activate/Deactivate User'

UPDATE Enterprise.RightValueType
    SET ShortName =  'EditOtherProfile'
WHERE Value = 'Ability to edit profile of other users'

UPDATE Enterprise.RightValueType
    SET ShortName =  'EditOwnProfile'
WHERE Value = 'Ability to edit my own profile'


IF NOT EXISTS
(
	SELECT 1
	FROM Enterprise.ACTION
	WHERE ObjectValue = 'Edit Other User Profile' AND 
		  ParentActionId IS NULL
)
BEGIN
	EXEC Enterprise.CreateAction @ProductID = @ProductId, @Action = N'Edit Other User Profile', @ActionTarget = N'Right', @ActionbValueTypeId = @ActionValueID, @Description = 'Edit Other User Profile', @ActionID = @ActionID OUTPUT;
	SELECT @ActionID AS N'@ActionID';
END;

IF OBJECT_ID('tempdb..#HoldOrgsEditOthersProfile') IS NOT NULL
BEGIN
	DROP TABLE #HoldOrgsEditOthersProfile;
END;

SELECT DISTINCT 
	   IDENTITY(int, 1, 1) AS RowNumber, OrganizationPartyID, 0 AS PStatus
INTO #HoldOrgsEditOthersProfile
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



SELECT @ProductId = ProductId
FROM Enterprise.Product
WHERE Name = 'Unified Login';

SELECT @ActionValueID = [ActionValueTypeID]
FROM Enterprise.ActionValueType
WHERE Value = 'Right';

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

		EXECUTE Enterprise.CreateRight @RoleId = -1, @RightName = 'Default_EditOtherUserProfile', @ShortName = 'EditOtherUserProfile', @RightCategoryId = @RightCategoryId, @PartyId = @OrgId, @ProductId = @ProductId, @Description = 'Edit Other User Profile', @RightId = @RightId OUTPUT;
		SELECT @ActionID = ActionID
		FROM Enterprise.ACTION
		WHERE ObjectValue = 'Edit Other User Profile' AND 
			  ObjectType = 'Right' AND 
			  ParentActionId IS NULL;
		EXEC [Enterprise].[LinkActionToRights] @ActionID = @ActionID, @RightId = @RightId, @StatusId = @Status, @UserActionId = @UserActionId OUTPUT;
		UPDATE #HoldOrgsEditOthersProfile
		  SET PStatus = 1
		WHERE RowNumber = @OrgRowNum;
	END;

DECLARE @EditOthersProfile int;

SELECT @EditOthersProfile = RightValueTypeId
FROM Enterprise.RightValueType
WHERE value = 'Default_EditOtherUserProfile';

SELECT @RightValueTypeId = RightValueTypeId
FROM Enterprise.RightValueType
WHERE value IN( 'Ability to edit profile of other users' );

IF NOT EXISTS
(
	SELECT 1
	FROM Enterprise.RightDependency
	WHERE RightValueTypeId = @RightValueTypeId AND 
		  DependentRightValueTypeId = @EditOthersProfile
)
BEGIN
	INSERT INTO Enterprise.RightDependency( RightValueTypeId, DependentRightValueTypeId )
	VALUES( @RightValueTypeId, @EditOthersProfile );
END;


--HIDE ROLES

--DECLARE @Status int;

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




EXEC sys.sp_updateextendedproperty @name = N'Build', @value = '58';