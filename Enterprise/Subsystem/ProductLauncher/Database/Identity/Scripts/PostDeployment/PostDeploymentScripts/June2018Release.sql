--GO
--update enterprise.action set ObjectValue = 'View Role Right' where ObjectValue = 'View Roles' and ProductId = 3 AND ParentActionId IS NULL
--GO
--update enterprise.action set ObjectValue = 'Manage Role Right' where ObjectValue = 'View Roles & Rights'  and ProductId = 3 
--GO
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
--DECLARE @RVT INT;
--DECLARE @DefaultRoute NVARCHAR(200);
--DECLARE @RightValueTypeId INT;
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
--DECLARE @TRightShortName NVARCHAR(100);

----Delete Migration Tool right from QA,SAT and PROD




--IF OBJECT_ID('tempdb..#HoldOrgsUserRoutes') IS NOT NULL
--BEGIN
--	DROP TABLE #HoldOrgsUserRoutes;
--END;

--SELECT @Status = StatusType.StatusTypeID
--FROM Enterprise.StatusTypeCategoryType
--	 JOIN
--	 Enterprise.StatusTypeCategory
--	 ON StatusTypeCategory.StatusTypeCategoryTypeId = StatusTypeCategoryType.StatusTypeCategoryTypeId
--	 JOIN
--	 Enterprise.StatusTypeCategoryClassification
--	 ON StatusTypeCategoryClassification.StatusTypeCategoryId = StatusTypeCategory.StatusTypeCategoryId
--	 JOIN
--	 Enterprise.StatusType
--	 ON StatusType.StatusTypeId = StatusTypeCategoryClassification.StatusTypeId
--WHERE StatusType.name = 'ALL' AND 
--	  StatusTypeCategoryType.Name = 'Security';

--SELECT @RightCategoryId = StatusType.StatusTypeID
--FROM Enterprise.StatusTypeCategoryType
--	 JOIN
--	 Enterprise.StatusTypeCategory
--	 ON StatusTypeCategory.StatusTypeCategoryTypeId = StatusTypeCategoryType.StatusTypeCategoryTypeId
--	 JOIN
--	 Enterprise.StatusTypeCategoryClassification
--	 ON StatusTypeCategoryClassification.StatusTypeCategoryId = StatusTypeCategory.StatusTypeCategoryId
--	 JOIN
--	 Enterprise.StatusType
--	 ON StatusType.StatusTypeId = StatusTypeCategoryClassification.StatusTypeId
--WHERE StatusType.name = 'DEfault' AND 
--	  StatusTypeCategory.Name = 'Right Type' AND 
--	  StatusTypeCategoryType.Name = 'Security';

--SET @ProductId = 3;

--IF NOT EXISTS
--(
--	SELECT 1
--	FROM Enterprise.ACTION
--	WHERE ObjectValue = 'Manage Role Right' AND 
--		  ParentActionId IS NULL
--)
--BEGIN
--	EXEC [Enterprise].[CreateAction] @ProductID = @ProductId, @Action = 'Manage Role Right', @ActionTarget = N'Right', @ActionbValueTypeId = 1, @Description = 'Grants Manage roles and rights privileges', @ActionID = @ActionID OUTPUT;
--	SELECT @ActionID AS N'@ActionID';
--END;

--SELECT DISTINCT 
--	   IDENTITY(int, 1, 1) AS RowNumber, OrganizationPartyID, 0 AS PStatus
--INTO #HoldOrgsUserRoutes
--FROM Person.Persona AS P
--	 INNER JOIN
--	 Enterprise.Organization AS O
--	 ON P.OrganizationPartyId = O.PartyId;

--WHILE EXISTS
--(
--	SELECT 1
--	FROM #HoldOrgsUserRoutes
--	WHERE PStatus = 0
--)
--BEGIN
--	SELECT TOP 1 @OrgRowNum = Rownumber, @OrgID = OrganizationPartyID
--	FROM #HoldOrgsUserRoutes
--	WHERE PStatus = 0;


--	EXECUTE Enterprise.CreateRight @RoleId = -1, @RightName = 'Default_RoleRightRight', @ShortName = 'RoleAndRight', @RightCategoryId = @RightCategoryId, @PartyId = @OrgId, @ProductId = @ProductId, @Description = '', @RightId = @RightId OUTPUT;
		
	
--		SELECT @ActionID = ActionID
--		FROM Enterprise.ACTION
--		WHERE ObjectValue = 'Roles And Rights' AND 
--			  ObjectType = 'Right' AND 
--			  ParentActionId IS NULL;
--		EXEC [Enterprise].[LinkActionToRights] @ActionID = @ActionID, @RightId = @RightId, @StatusId = @Status, @UserActionId = @UserActionId OUTPUT;

--	EXECUTE Enterprise.CreateRight @RoleId = -1, @RightName = 'Default_ManageRoleRight', @ShortName = 'ViewRoles', @RightCategoryId = @RightCategoryId, @PartyId = @OrgId, @ProductId = @ProductId, @Description = 'View ROles rights', @RightId = @RightId OUTPUT;
--	SELECT @ActionID = ActionID
--	FROM Enterprise.ACTION
--	WHERE ObjectValue = 'Manage Role Right' AND 
--		  ObjectType = 'Right' AND 
--		  ParentActionId IS NULL;
--	SELECT @RightID = RightId
--	FROM Enterprise.[Right] AS R
--		 INNER JOIN
--		 Enterprise.RightValueType AS RVT
--		 ON RVT.RightValueTypeId = R.RightValueTypeId
--	WHERE Value = 'Default_ManageRoleRight' AND 
--		  RoleId = -1;
--	EXEC [Enterprise].[LinkActionToRights] @ActionID = @ActionID, @RightId = @RightId, @StatusId = @Status, @UserActionId = @UserActionId OUTPUT;

--		---->

--	UPDATE #HoldOrgsUserRoutes
--	  SET PStatus = 1
--	WHERE RowNumber = @OrgRowNum;
--END;


--DECLARE @ManageRoleRIght int;
--DECLARE @RolesAndRights int;
--DECLARE @DefaltRoleRight INT


--SELECT @DefaltRoleRight = RightValueTypeId
--FROM Enterprise.RightValueType
--WHERE value = 'Default_RoleRightRight';

--SELECT @ManageRoleRIght = RightValueTypeId
--FROM Enterprise.RightValueType
--WHERE value = 'Default_ManageRoleRight';

--SELECT @RolesAndRights = RightValueTypeId
--FROM Enterprise.RightValueType
--WHERE value = 'Default_RolesAndRights';

--SELECT @RightValueTypeId = RightValueTypeId
--FROM Enterprise.RightValueType
--WHERE value IN( 'Ability to manage roles and rights' );

--IF NOT EXISTS
--(
--	SELECT 1
--	FROM Enterprise.RightDependency
--	WHERE RightValueTypeId = @RightValueTypeId AND 
--		  DependentRightValueTypeId = @RolesAndRights
--)
--BEGIN
--	INSERT INTO Enterprise.RightDependency( RightValueTypeId, DependentRightValueTypeId )
--	VALUES( @RightValueTypeId, @RolesAndRights );
--END;

--IF NOT EXISTS
--(
--	SELECT 1
--	FROM Enterprise.RightDependency
--	WHERE RightValueTypeId = @RightValueTypeId AND 
--		  DependentRightValueTypeId = @ManageRoleRIght
--)
--BEGIN
--	INSERT INTO Enterprise.RightDependency( RightValueTypeId, DependentRightValueTypeId )
--	VALUES( @RightValueTypeId, @ManageRoleRIght );
--END;

--IF NOT EXISTS
--(
--	SELECT 1
--	FROM Enterprise.RightDependency
--	WHERE RightValueTypeId = @RightValueTypeId AND 
--		  DependentRightValueTypeId = @DefaltRoleRight
--)
--BEGIN
--	INSERT INTO Enterprise.RightDependency( RightValueTypeId, DependentRightValueTypeId )
--	VALUES( @RightValueTypeId, @DefaltRoleRight );
--END;


--SELECT @RightValueTypeId = RightValueTypeId
--FROM Enterprise.RightValueType
--WHERE value IN( 'Ability to view roles and rights' );

--IF NOT EXISTS
--(
--	SELECT 1
--	FROM Enterprise.RightDependency
--	WHERE RightValueTypeId = @RightValueTypeId AND 
--		  DependentRightValueTypeId = @DefaltRoleRight
--)
--BEGIN
--	INSERT INTO Enterprise.RightDependency( RightValueTypeId, DependentRightValueTypeId )
--	VALUES( @RightValueTypeId, @DefaltRoleRight );
--END;