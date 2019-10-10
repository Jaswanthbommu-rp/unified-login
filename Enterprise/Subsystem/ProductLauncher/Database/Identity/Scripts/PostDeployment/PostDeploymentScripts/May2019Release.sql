IF NOT EXISTS(SELECT 1 FROM ENterprise.ROleTYpe WHERE NAme = 'External User')
BEGIN
	INSERT INTO Enterprise.RoleType (PartyRoleTypeId, ParentPartyRoleTypeId, Name)
		VALUES(405, 400, 'External User')
END
GO
IF NOT EXISTS
(
	SELECT 1
	FROM Enterprise.RelationshipType
	WHERE RoleTypeIdValidFrom = 405 AND 
		  ROleTypeIdValidTo = 205
)
BEGIN
	INSERT INTO [Enterprise].[RelationshipType]( [RoleTypeIdValidFrom], [RoleTypeIdValidTo], [Name], [Description] )
	VALUES( 405, 205, 'User Type', 'User Type of a person in an organization' );
END;
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
DECLARE @TargetProductId INT;
DECLARE @VisibilityStatusId INT
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
VALUES( 1, 'Employee Access to Manage Questionnaires in CIMPL', 'Employee Access to Manage Questionnaires in CIMPL', 'EmployeeManageCIMPLQuestions' );

SELECT @ProductId = ProductId
FROM Enterprise.Product
WHERE name = 'Unified Platform';

--select * from enterprise.product where name like '%lead%'

SELECT @TargetProductId = ProductId
FROM Enterprise.Product
WHERE Name = 'Unified Platform';

SET @ActionValueID = 1

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
	  ST.Name = 'System';

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
	  ST.Name = 'System';


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

SET @VisibilityStatusId = @Status;

IF NOT EXISTS
(
	SELECT 1
	FROM Enterprise.ACTION
	WHERE ObjectValue = 'Employee Manage CIMPL Questions' AND 
		  ParentActionId IS NULL
)
BEGIN
	EXEC Enterprise.CreateAction @ProductID = @ProductId, @Action = N'Employee Manage CIMPL Questions', @ActionTarget = N'Right', @ActionbValueTypeId = 1, @Description = '', @ActionID = @ActionID OUTPUT;
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
	WHERE ObjectValue = 'Employee Manage CIMPL Questions' AND 
		  ParentActionID = @ParentActionId
)
BEGIN
	EXEC [Enterprise].[CreateAction] @ProductID = @ProductId, @Action = N'Employee Manage CIMPL Questions', @ActionTarget = N'Right', @ActionbValueTypeId = 1, @Description = '', @ParentActionID = @ParentActionId, @ActionID = @ActionID OUTPUT;
	SELECT @ActionID AS N'@ActionID';
END;

SELECT DISTINCT 
	   IDENTITY(int, 1, 1) AS RowNumber, OrganizationPartyID, 0 AS PStatus
INTO #HoldPartyForUnifiedSettings
FROM Person.Persona AS P
	 INNER JOIN
	 Enterprise.Organization AS O
	 ON P.OrganizationPartyId = O.PartyId
WHERE O.Name = 'RealPage Employee'

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
		EXECUTE Enterprise.CreateRight @RoleId = -1, @RightName = 'Default_EmployeeManageCIMPLQuestions', @ShortName = 'EmployeeManageCIMPLQuestions', @RightCategoryId = @RightCategory, @PartyId = @PartyId, @ProductId = @ProductId, @Description = '',  @TargetProductId = @TargetProductId, @VisibilityStatusId = @VisibilityStatusId, @RightId = @RightId OUTPUT;
		EXECUTE Enterprise.CreateRight @RoleId = @RoleId, @RightName = @TRightName, @RightCategoryId = @RightCategory, @PartyId = @PartyId, @ProductId = @ProductId, @Shortname = @TRightShortName, @Description = @TRightDesc, @TargetProductId = @TargetProductId, @VisibilityStatusId = @VisibilityStatusId, @RightId = @RightId OUTPUT;


		SELECT @ActionID = ActionID
		FROM Enterprise.ACTION
		WHERE ObjectValue = 'Employee Manage CIMPL Questions'  AND 
			  ObjectType = 'Right' AND 
			  ParentActionId IS NULL;
		EXEC [Enterprise].[LinkActionToRights] @ActionID = @ActionID, @RightId = @RightId, @StatusId = @Status, @UserActionId = @UserActionId OUTPUT;

		SELECT @RightId = RightId
		FROM Enterprise.[Right] AS R
			 INNER JOIN
			 Enterprise.RightValueType AS RR
			 ON RR.RightValueTypeId = R.RightValueTypeId
		WHERE RR.Value = 'Default_EmployeeManageCIMPLQuestions';
		SELECT @ActionID = ActionID
		FROM Enterprise.ACTION
		WHERE ObjectValue = 'Employee Manage CIMPL Questions'   AND 
			  ObjectType = 'Right' AND 
			  ParentActionId IS NULL;
		EXEC [Enterprise].[LinkActionToRights] @ActionID = @ActionID, @RightId = @RightId, @StatusId = @Status, @UserActionId = @UserActionId OUTPUT;



		FETCH Rights INTO @TRightId, @TRightName, @TRightDesc, @TRightShortName;
	END;
	CLOSE Rights;
	DEALLOCATE Rights;
	UPDATE #HoldPartyForUnifiedSettings
	  SET PStatus = 1
	WHERE RowNumber = @PartyRowNum;
END;

DECLARE @Dashboard int;

SELECT @DashBoard = RightValueTypeId
FROM Enterprise.RightValueType
WHERE value = 'Default_EmployeeManageCIMPLQuestions';

SELECT @RightValueTypeId = RightValueTypeId
FROM Enterprise.RightValueType
WHERE value IN( 'Employee Access to Manage Questionnaires in CIMPL' );

IF NOT EXISTS
(
	SELECT 1
	FROM Enterprise.RightDependency
	WHERE RightValueTypeId = @RightValueTypeId AND 
		  DependentRightValueTypeId = @DashBoard
)
BEGIN
	INSERT INTO Enterprise.RightDependency( RightValueTypeId, DependentRightValueTypeId )
	VALUES( @RightValueTypeId, @DashBoard );

END;
GO

IF NOT EXISTS
(
	SELECT 1
	FROM Batch.BatchProcessType
	WHERE BatchProcessTypeId = 8 AND BatchProcessConfigurationId = 1 AND Name = 'UserTypeExternalToAdmin'
)
BEGIN
       INSERT INTO Batch.BatchProcessType (BatchProcessTypeId, BatchProcessConfigurationId, Name, Description)
              VALUES(8, 1, 'UserTypeExternalToAdmin', 'User Type changed from External To Admin')
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
DECLARE @TargetProductId INT;
DECLARE @VisibilityStatusId INT
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
VALUES( 1, 'Employee View-Only Access to CIMPL', 'Employee View-Only Access to CIMPL', 'EmployeeViewCIMPLQuestions' );

SELECT @ProductId = ProductId
FROM Enterprise.Product
WHERE name = 'Unified Platform';

--select * from enterprise.product where name like '%lead%'

SELECT @TargetProductId = ProductId
FROM Enterprise.Product
WHERE Name = 'Unified Platform';

SET @ActionValueID = 1

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
	  ST.Name = 'System';

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
	  ST.Name = 'System';


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

SET @VisibilityStatusId = @Status;

IF NOT EXISTS
(
	SELECT 1
	FROM Enterprise.ACTION
	WHERE ObjectValue = 'Employee View CIMPL Questions' AND 
		  ParentActionId IS NULL
)
BEGIN
	EXEC Enterprise.CreateAction @ProductID = @ProductId, @Action = N'Employee View CIMPL Questions', @ActionTarget = N'Right', @ActionbValueTypeId = 1, @Description = '', @ActionID = @ActionID OUTPUT;
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
	WHERE ObjectValue = 'Employee View CIMPL Questions' AND 
		  ParentActionID = @ParentActionId
)
BEGIN
	EXEC [Enterprise].[CreateAction] @ProductID = @ProductId, @Action = N'Employee View CIMPL Questions', @ActionTarget = N'Right', @ActionbValueTypeId = 1, @Description = '', @ParentActionID = @ParentActionId, @ActionID = @ActionID OUTPUT;
	SELECT @ActionID AS N'@ActionID';
END;

SELECT DISTINCT 
	   IDENTITY(int, 1, 1) AS RowNumber, OrganizationPartyID, 0 AS PStatus
INTO #HoldPartyForUnifiedSettings
FROM Person.Persona AS P
	 INNER JOIN
	 Enterprise.Organization AS O
	 ON P.OrganizationPartyId = O.PartyId
WHERE O.Name = 'RealPage Employee'

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
		EXECUTE Enterprise.CreateRight @RoleId = -1, @RightName = 'Default_EmployeeViewCIMPLQuestions', @ShortName = 'EmployeeViewCIMPLQuestions', @RightCategoryId = @RightCategory, @PartyId = @PartyId, @ProductId = @ProductId, @Description = '',  @TargetProductId = @TargetProductId, @VisibilityStatusId = @VisibilityStatusId, @RightId = @RightId OUTPUT;
		EXECUTE Enterprise.CreateRight @RoleId = @RoleId, @RightName = @TRightName, @RightCategoryId = @RightCategory, @PartyId = @PartyId, @ProductId = @ProductId, @Shortname = @TRightShortName, @Description = @TRightDesc, @TargetProductId = @TargetProductId, @VisibilityStatusId = @VisibilityStatusId, @RightId = @RightId OUTPUT;


		SELECT @ActionID = ActionID
		FROM Enterprise.ACTION
		WHERE ObjectValue = 'Employee View CIMPL Questions'  AND 
			  ObjectType = 'Right' AND 
			  ParentActionId IS NULL;
		EXEC [Enterprise].[LinkActionToRights] @ActionID = @ActionID, @RightId = @RightId, @StatusId = @Status, @UserActionId = @UserActionId OUTPUT;

		SELECT @RightId = RightId
		FROM Enterprise.[Right] AS R
			 INNER JOIN
			 Enterprise.RightValueType AS RR
			 ON RR.RightValueTypeId = R.RightValueTypeId
		WHERE RR.Value = 'Default_EmployeeViewCIMPLQuestions';
		SELECT @ActionID = ActionID
		FROM Enterprise.ACTION
		WHERE ObjectValue = 'Employee View CIMPL Questions'   AND 
			  ObjectType = 'Right' AND 
			  ParentActionId IS NULL;
		EXEC [Enterprise].[LinkActionToRights] @ActionID = @ActionID, @RightId = @RightId, @StatusId = @Status, @UserActionId = @UserActionId OUTPUT;



		FETCH Rights INTO @TRightId, @TRightName, @TRightDesc, @TRightShortName;
	END;
	CLOSE Rights;
	DEALLOCATE Rights;
	UPDATE #HoldPartyForUnifiedSettings
	  SET PStatus = 1
	WHERE RowNumber = @PartyRowNum;
END;

DECLARE @Dashboard int;

SELECT @DashBoard = RightValueTypeId
FROM Enterprise.RightValueType
WHERE value = 'Default_EmployeeViewCIMPLQuestions';

SELECT @RightValueTypeId = RightValueTypeId
FROM Enterprise.RightValueType
WHERE value IN( 'Employee View-Only Access to CIMPL' );

IF NOT EXISTS
(
	SELECT 1
	FROM Enterprise.RightDependency
	WHERE RightValueTypeId = @RightValueTypeId AND 
		  DependentRightValueTypeId = @DashBoard
)
BEGIN
	INSERT INTO Enterprise.RightDependency( RightValueTypeId, DependentRightValueTypeId )
	VALUES( @RightValueTypeId, @DashBoard );

END;
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
DECLARE @TargetProductId INT;
DECLARE @VisibilityStatusId INT
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
VALUES( 1, 'Employee Access to add Implementation Records in CIMPL', 'Employee Access to add Implementation Records in CIMPL', 'EmployeeImplementRecordsCIMPL' );

SELECT @ProductId = ProductId
FROM Enterprise.Product
WHERE name = 'Unified Platform';

--select * from enterprise.product where name like '%lead%'

SELECT @TargetProductId = ProductId
FROM Enterprise.Product
WHERE Name = 'Unified Platform';

SET @ActionValueID = 1

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
	  ST.Name = 'System';

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
	  ST.Name = 'System';


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

SET @VisibilityStatusId = @Status;

IF NOT EXISTS
(
	SELECT 1
	FROM Enterprise.ACTION
	WHERE ObjectValue = 'Employee Implement Records CIMPL' AND 
		  ParentActionId IS NULL
)
BEGIN
	EXEC Enterprise.CreateAction @ProductID = @ProductId, @Action = N'Employee Implement Records CIMPL', @ActionTarget = N'Right', @ActionbValueTypeId = 1, @Description = '', @ActionID = @ActionID OUTPUT;
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
	WHERE ObjectValue = 'Employee Implement Records CIMPL' AND 
		  ParentActionID = @ParentActionId
)
BEGIN
	EXEC [Enterprise].[CreateAction] @ProductID = @ProductId, @Action = N'Employee Implement Records CIMPL', @ActionTarget = N'Right', @ActionbValueTypeId = 1, @Description = '', @ParentActionID = @ParentActionId, @ActionID = @ActionID OUTPUT;
	SELECT @ActionID AS N'@ActionID';
END;

SELECT DISTINCT 
	   IDENTITY(int, 1, 1) AS RowNumber, OrganizationPartyID, 0 AS PStatus
INTO #HoldPartyForUnifiedSettings
FROM Person.Persona AS P
	 INNER JOIN
	 Enterprise.Organization AS O
	 ON P.OrganizationPartyId = O.PartyId
WHERE O.Name = 'RealPage Employee'

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
		EXECUTE Enterprise.CreateRight @RoleId = -1, @RightName = 'Default_EmployeeImplementRecordsCIMPL', @ShortName = 'EmployeeImplementRecordsCIMPL', @RightCategoryId = @RightCategory, @PartyId = @PartyId, @ProductId = @ProductId, @Description = '',  @TargetProductId = @TargetProductId, @VisibilityStatusId = @VisibilityStatusId, @RightId = @RightId OUTPUT;
		EXECUTE Enterprise.CreateRight @RoleId = @RoleId, @RightName = @TRightName, @RightCategoryId = @RightCategory, @PartyId = @PartyId, @ProductId = @ProductId, @Shortname = @TRightShortName, @Description = @TRightDesc, @TargetProductId = @TargetProductId, @VisibilityStatusId = @VisibilityStatusId, @RightId = @RightId OUTPUT;


		SELECT @ActionID = ActionID
		FROM Enterprise.ACTION
		WHERE ObjectValue = 'Employee Implement Records CIMPL'  AND 
			  ObjectType = 'Right' AND 
			  ParentActionId IS NULL;
		EXEC [Enterprise].[LinkActionToRights] @ActionID = @ActionID, @RightId = @RightId, @StatusId = @Status, @UserActionId = @UserActionId OUTPUT;

		SELECT @RightId = RightId
		FROM Enterprise.[Right] AS R
			 INNER JOIN
			 Enterprise.RightValueType AS RR
			 ON RR.RightValueTypeId = R.RightValueTypeId
		WHERE RR.Value = 'Default_EmployeeImplementRecordsCIMPL';
		SELECT @ActionID = ActionID
		FROM Enterprise.ACTION
		WHERE ObjectValue = 'Employee Implement Records CIMPL'   AND 
			  ObjectType = 'Right' AND 
			  ParentActionId IS NULL;
		EXEC [Enterprise].[LinkActionToRights] @ActionID = @ActionID, @RightId = @RightId, @StatusId = @Status, @UserActionId = @UserActionId OUTPUT;



		FETCH Rights INTO @TRightId, @TRightName, @TRightDesc, @TRightShortName;
	END;
	CLOSE Rights;
	DEALLOCATE Rights;
	UPDATE #HoldPartyForUnifiedSettings
	  SET PStatus = 1
	WHERE RowNumber = @PartyRowNum;
END;

DECLARE @Dashboard int;

SELECT @DashBoard = RightValueTypeId
FROM Enterprise.RightValueType
WHERE value = 'Default_EmployeeImplementRecordsCIMPL';

SELECT @RightValueTypeId = RightValueTypeId
FROM Enterprise.RightValueType
WHERE value IN( 'Employee Access to add Implementation Records in CIMPL' );

IF NOT EXISTS
(
	SELECT 1
	FROM Enterprise.RightDependency
	WHERE RightValueTypeId = @RightValueTypeId AND 
		  DependentRightValueTypeId = @DashBoard
)
BEGIN
	INSERT INTO Enterprise.RightDependency( RightValueTypeId, DependentRightValueTypeId )
	VALUES( @RightValueTypeId, @DashBoard );

END;
GO
IF EXISTS(SELECT 1 FROM Enterprise.RightValueTYpe WHERE value IN('Ability to Manage Questions  for CIMPL','Ability to Manage Questions for CIMPL') )
BEGIN
	UPDATE Enterprise.RightValueType SET VALUE = 'Ability to Answer Questions for CIMPL'
		WHERE Value IN('Ability to Manage Questions  for CIMPL','Ability to Manage Questions for CIMPL')
END
GO

IF EXISTS
(
    SELECT 1
    FROM Enterprise.ProductSetting PS
         INNER JOIN Enterprise.ProductSettingType PT ON PT.ProductSettingTypeId = PS.ProductSettingTypeId
         INNER JOIN Enterprise.Product P ON P.ProductId = PS.ProductId
    WHERE P.NAME = 'Vendor Services'
          AND PT.Name IN(N'TitleId', N'MetatagUniqueId')
)
    BEGIN
        UPDATE PS
          SET
              PS.Value = 'Vendor Credentialing'
        FROM Enterprise.ProductSetting PS
             INNER JOIN Enterprise.ProductSettingType PT ON PT.ProductSettingTypeId = PS.ProductSettingTypeId
             INNER JOIN Enterprise.Product P ON P.ProductId = PS.ProductId
        WHERE P.NAME = 'Vendor Services'
              AND PT.Name IN(N'TitleId', N'MetatagUniqueId');
    END;

IF EXISTS
(
    SELECT 1
    FROM Enterprise.Product
    WHERE NAME = 'Vendor Services'
)
    BEGIN
        UPDATE Enterprise.Product
          SET
              Name = 'Vendor Credentialing'
        WHERE Name = 'Vendor Services';
    END;
IF EXISTS
(
    SELECT 1
    FROM Enterprise.ProductType
    WHERE Name = 'Vendor Services'
)
    BEGIN
        UPDATE Enterprise.ProductType
          SET
              Name = 'Vendor Credentialing',
              Description = 'Vendor Credentialing'
        WHERE Name = 'Vendor Services';
    END;

IF EXISTS
(
    SELECT 1
    FROM Enterprise.RightValueType
    WHERE Value = 'Manage Vendor Compliance Product Access'
)
    BEGIN
        UPDATE Enterprise.RightValueType
          SET
              Value = 'Manage Vendor Credentialing Product Access'
        WHERE Value = 'Manage Vendor Compliance Product Access';
    END
GO
IF EXISTS
(
    SELECT 1
    FROM Enterprise.Product
    WHERE NAME = 'Leasing & Rents Conversion Tool'
)
    BEGIN
        UPDATE Enterprise.Product
          SET
              Name = 'L&R Conversion Utility'
        WHERE Name = 'Leasing & Rents Conversion Tool';
    END;
GO
    IF EXISTS
(
    SELECT 1
    FROM Enterprise.RightValueType
    WHERE Value = 'Access to Leasing & Rents Conversion Tool for OneSite users'
)
    BEGIN
        UPDATE Enterprise.RightValueType
          SET
              Value = 'Access to Leasing & Rents Conversion Utility for OneSite users'
        WHERE Value = 'Access to Leasing & Rents Conversion Tool for OneSite users';
    END;
GO
IF EXISTS
(
    SELECT 1
    FROM Enterprise.ProductSetting PS
         INNER JOIN Enterprise.ProductSettingType PT ON PT.ProductSettingTypeId = PS.ProductSettingTypeId
         INNER JOIN Enterprise.Product P ON P.ProductId = PS.ProductId
    WHERE P.NAME = 'Black Book'
          AND PT.Name IN(N'TitleId')--, N'MetatagUniqueId')
)
    BEGIN
        UPDATE PS
          SET
              PS.Value = 'Master Data Management'
        FROM Enterprise.ProductSetting PS
             INNER JOIN Enterprise.ProductSettingType PT ON PT.ProductSettingTypeId = PS.ProductSettingTypeId
             INNER JOIN Enterprise.Product P ON P.ProductId = PS.ProductId
        WHERE P.NAME = 'Black Book'
              AND PT.Name IN(N'TitleId') --, N'MetatagUniqueId');
    END;

IF EXISTS
(
    SELECT 1
    FROM Enterprise.Product
    WHERE NAME = 'Black Book'
)
    BEGIN
        UPDATE Enterprise.Product
          SET
              Name = 'Master Data Management'
        WHERE Name = 'Black Book';
    END;
IF EXISTS
(
    SELECT 1
    FROM Enterprise.ProductType
    WHERE Name = 'Black Book'
)
    BEGIN
        UPDATE Enterprise.ProductType
          SET
              Name = 'Master Data Management',
              Description = 'Master Data Management'
        WHERE Name = 'Black Book';
    END;
GO
IF EXISTS
(
    SELECT 1
    FROM Enterprise.ProductSetting PS
         INNER JOIN Enterprise.ProductSettingType PT ON PT.ProductSettingTypeId = PS.ProductSettingTypeId
         INNER JOIN Enterprise.Product P ON P.ProductId = PS.ProductId
    WHERE P.NAME = 'Financial Suite'
          AND PT.Name IN(N'TitleId', 'Subsolution')--, N'MetatagUniqueId')
)
    BEGIN
        UPDATE PS
          SET
              PS.Value = 'Accounting, Job Cost, Spend Control, Commercial'
        FROM Enterprise.ProductSetting PS
             INNER JOIN Enterprise.ProductSettingType PT ON PT.ProductSettingTypeId = PS.ProductSettingTypeId
             INNER JOIN Enterprise.Product P ON P.ProductId = PS.ProductId
        WHERE P.NAME = 'Financial Suite'
              AND PT.Name IN(N'TitleId', 'Subsolution') --, N'MetatagUniqueId');
    END;

IF EXISTS
(
    SELECT 1
    FROM Enterprise.ProductType
    WHERE Name = 'Financial Suite'
)
    BEGIN
        UPDATE Enterprise.ProductType
          SET
              Description = 'Accounting, Job Cost, Spend Control, Commercial'
        WHERE Name = 'Financial Suite';
    END;
    GO
 
IF NOT EXISTS(SELECT * FROM Enterprise.PartyRole WHERE PartyId = 3 and ROleTypeId = 405)
BEGIN
	INSERT INTO Enterprise.PartyRole (PartyId, RoleTypeId)
		VALUES(3 , 405)
END
GO
INSERT INTO ENterprise.PartyROle
(PartyId,
 RoleTYpeId
)
       SELECT PartyId,
              405
       FROM Enterprise.Organization
       WHERE PartyId NOT IN
(
    SELECT DISTINCT
           O.PartyId
    FROM Enterprise.PartyRole P
         INNER JOIN ENterprise.Organization O ON P.PartyId = O.PartyId
    WHERE RoleTYpeId = 405
	OR O.Name  = 'RealPage Employee'
);
GO


IF NOT EXISTS
(
	SELECT 1
	FROM [Enterprise].[RoleTypeDependency]
	WHERE [ParentRoleTypeId] = 405 AND 
		  [ChildRoleTypeId] = 401 AND 
		  [SortOrder] IS NOT NULL
)
BEGIN
	INSERT INTO [Enterprise].[RoleTypeDependency]( [ParentRoleTypeId], [ChildRoleTypeId], [SortOrder] )
	VALUES( 405, 401, 20 );
END;

IF NOT EXISTS
(
	SELECT 1
	FROM [Enterprise].[RoleTypeDependency]
	WHERE [ParentRoleTypeId] = 405 AND 
		  [ChildRoleTypeId] = 404 AND 
		  [SortOrder] IS NOT NULL
)
BEGIN
	INSERT INTO [Enterprise].[RoleTypeDependency]( [ParentRoleTypeId], [ChildRoleTypeId], [SortOrder] )
	VALUES( 405, 404, 10 );
END;

IF NOT EXISTS
(
	SELECT 1
	FROM [Enterprise].[RoleTypeDependency]
	WHERE [ParentRoleTypeId] = 405 AND 
		  [ChildRoleTypeId] = 405 AND 
		  [SortOrder] IS NOT NULL
)
BEGIN
	INSERT INTO [Enterprise].[RoleTypeDependency]( [ParentRoleTypeId], [ChildRoleTypeId], [SortOrder] )
	VALUES( 405, 405, 30 );
END;