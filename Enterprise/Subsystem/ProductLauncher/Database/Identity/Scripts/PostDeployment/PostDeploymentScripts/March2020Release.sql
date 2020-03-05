
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
VALUES( 1, 'Access to Help Center', 'Access to Help Center', 'AccessHelpCenter' );

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
	WHERE ObjectValue = 'Access HelpCenter' AND 
		  ParentActionId IS NULL
)
BEGIN
	EXEC Enterprise.CreateAction @ProductID = @ProductId, @Action = N'Access HelpCenter', @ActionTarget = N'Right', @ActionbValueTypeId = 1, @Description = '', @ActionID = @ActionID OUTPUT;
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
	WHERE ObjectValue = 'Access HelpCenter' AND 
		  ParentActionID = @ParentActionId
)
BEGIN
	EXEC [Enterprise].[CreateAction] @ProductID = @ProductId, @Action = N'Access HelpCenter', @ActionTarget = N'Right', @ActionbValueTypeId = 1, @Description = '', @ParentActionID = @ParentActionId, @ActionID = @ActionID OUTPUT;
	SELECT @ActionID AS N'@ActionID';
END;

SELECT DISTINCT 
	   IDENTITY(int, 1, 1) AS RowNumber, o.PartyId OrganizationPartyID, 0 AS PStatus
INTO #HoldPartyForUnifiedSettings
FROM Enterprise.Organization o
	 INNER JOIN Enterprise.Party p
	 ON P.partyid  = O.PartyId
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
		EXECUTE Enterprise.CreateRight @RoleId = -1, @RightName = 'Default_AccessHelpCenter', @ShortName = 'AccessHelpCenter', @RightCategoryId = @RightCategory, @PartyId = @PartyId, @ProductId = @ProductId, @Description = '',  @TargetProductId = @TargetProductId, @VisibilityStatusId = @VisibilityStatusId, @RightId = @RightId OUTPUT;
		EXECUTE Enterprise.CreateRight @RoleId = @RoleId, @RightName = @TRightName, @RightCategoryId = @RightCategory, @PartyId = @PartyId, @ProductId = @ProductId, @Shortname = @TRightShortName, @Description = @TRightDesc, @TargetProductId = @TargetProductId, @VisibilityStatusId = @VisibilityStatusId, @RightId = @RightId OUTPUT;


		SELECT @ActionID = ActionID
		FROM Enterprise.ACTION
		WHERE ObjectValue = 'Access HelpCenter'  AND 
			  ObjectType = 'Right' AND 
			  ParentActionId IS NULL;
		EXEC [Enterprise].[LinkActionToRights] @ActionID = @ActionID, @RightId = @RightId, @StatusId = @Status, @UserActionId = @UserActionId OUTPUT;

		SELECT @RightId = RightId
		FROM Enterprise.[Right] AS R
			 INNER JOIN
			 Enterprise.RightValueType AS RR
			 ON RR.RightValueTypeId = R.RightValueTypeId
		WHERE RR.Value = 'Default_AccessHelpCenter';
		SELECT @ActionID = ActionID
		FROM Enterprise.ACTION
		WHERE ObjectValue = 'Access HelpCenter'   AND 
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
WHERE value = 'Default_AccessHelpCenter';

SELECT @RightValueTypeId = RightValueTypeId
FROM Enterprise.RightValueType
WHERE value IN( 'Access to Help Center' );

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
VALUES( 1, 'Access to Help Center', 'Access to Help Center', 'AccessHelpCenter' );

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
	WHERE ObjectValue = 'Access HelpCenter' AND 
		  ParentActionId IS NULL
)
BEGIN
	EXEC Enterprise.CreateAction @ProductID = @ProductId, @Action = N'Access HelpCenter', @ActionTarget = N'Right', @ActionbValueTypeId = 1, @Description = '', @ActionID = @ActionID OUTPUT;
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
	WHERE ObjectValue = 'Access HelpCenter' AND 
		  ParentActionID = @ParentActionId
)
BEGIN
	EXEC [Enterprise].[CreateAction] @ProductID = @ProductId, @Action = N'Access HelpCenter', @ActionTarget = N'Right', @ActionbValueTypeId = 1, @Description = '', @ParentActionID = @ParentActionId, @ActionID = @ActionID OUTPUT;
	SELECT @ActionID AS N'@ActionID';
END;

SELECT DISTINCT 
	   IDENTITY(int, 1, 1) AS RowNumber, o.PartyId OrganizationPartyID, 0 AS PStatus
INTO #HoldPartyForUnifiedSettings
FROM Enterprise.Organization o
	 INNER JOIN Enterprise.Party p

	 ON P.partyid  = O.PartyId
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
	WHERE RR.Value = 'Basic End User' AND 
		  R.PartyId = @PartyId;
	DECLARE Rights CURSOR
	FOR SELECT RightId, Name, Description, ShortName
		FROM #RightsUnifiedSettings;
	OPEN Rights;
	FETCH Rights INTO @TRightId, @TRightName, @TRightDesc, @TRightShortName;
	WHILE @@FETCH_STATUS = 0
	BEGIN
		EXECUTE Enterprise.CreateRight @RoleId = -1, @RightName = 'Default_AccessHelpCenter', @ShortName = 'AccessHelpCenter', @RightCategoryId = @RightCategory, @PartyId = @PartyId, @ProductId = @ProductId, @Description = '',  @TargetProductId = @TargetProductId, @VisibilityStatusId = @VisibilityStatusId, @RightId = @RightId OUTPUT;
		EXECUTE Enterprise.CreateRight @RoleId = @RoleId, @RightName = @TRightName, @RightCategoryId = @RightCategory, @PartyId = @PartyId, @ProductId = @ProductId, @Shortname = @TRightShortName, @Description = @TRightDesc, @TargetProductId = @TargetProductId, @VisibilityStatusId = @VisibilityStatusId, @RightId = @RightId OUTPUT;


		SELECT @ActionID = ActionID
		FROM Enterprise.ACTION
		WHERE ObjectValue = 'Access HelpCenter'  AND 
			  ObjectType = 'Right' AND 
			  ParentActionId IS NULL;
		EXEC [Enterprise].[LinkActionToRights] @ActionID = @ActionID, @RightId = @RightId, @StatusId = @Status, @UserActionId = @UserActionId OUTPUT;

		SELECT @RightId = RightId
		FROM Enterprise.[Right] AS R
			 INNER JOIN
			 Enterprise.RightValueType AS RR
			 ON RR.RightValueTypeId = R.RightValueTypeId
		WHERE RR.Value = 'Default_AccessHelpCenter';
		SELECT @ActionID = ActionID
		FROM Enterprise.ACTION
		WHERE ObjectValue = 'Access HelpCenter'   AND 
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
WHERE value = 'Default_AccessHelpCenter';

SELECT @RightValueTypeId = RightValueTypeId
FROM Enterprise.RightValueType
WHERE value IN( 'Access to Help Center' );

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
VALUES( 1, 'Access to Help Center', 'Access to Help Center', 'AccessHelpCenter' );

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
	WHERE ObjectValue = 'Access HelpCenter' AND 
		  ParentActionId IS NULL
)
BEGIN
	EXEC Enterprise.CreateAction @ProductID = @ProductId, @Action = N'Access HelpCenter', @ActionTarget = N'Right', @ActionbValueTypeId = 1, @Description = '', @ActionID = @ActionID OUTPUT;
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
	WHERE ObjectValue = 'Access HelpCenter' AND 
		  ParentActionID = @ParentActionId
)
BEGIN
	EXEC [Enterprise].[CreateAction] @ProductID = @ProductId, @Action = N'Access HelpCenter', @ActionTarget = N'Right', @ActionbValueTypeId = 1, @Description = '', @ParentActionID = @ParentActionId, @ActionID = @ActionID OUTPUT;
	SELECT @ActionID AS N'@ActionID';
END;

SELECT DISTINCT 
	   IDENTITY(int, 1, 1) AS RowNumber, o.PartyId OrganizationPartyID, 0 AS PStatus
INTO #HoldPartyForUnifiedSettings
FROM Enterprise.Organization o
	 INNER JOIN Enterprise.Party p

	 ON P.partyid  = O.PartyId
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
	WHERE RR.Value = 'Read only for Unified Platform' AND 
		  R.PartyId = @PartyId;
	DECLARE Rights CURSOR
	FOR SELECT RightId, Name, Description, ShortName
		FROM #RightsUnifiedSettings;
	OPEN Rights;
	FETCH Rights INTO @TRightId, @TRightName, @TRightDesc, @TRightShortName;
	WHILE @@FETCH_STATUS = 0
	BEGIN
		EXECUTE Enterprise.CreateRight @RoleId = -1, @RightName = 'Default_AccessHelpCenter', @ShortName = 'AccessHelpCenter', @RightCategoryId = @RightCategory, @PartyId = @PartyId, @ProductId = @ProductId, @Description = '',  @TargetProductId = @TargetProductId, @VisibilityStatusId = @VisibilityStatusId, @RightId = @RightId OUTPUT;
		EXECUTE Enterprise.CreateRight @RoleId = @RoleId, @RightName = @TRightName, @RightCategoryId = @RightCategory, @PartyId = @PartyId, @ProductId = @ProductId, @Shortname = @TRightShortName, @Description = @TRightDesc, @TargetProductId = @TargetProductId, @VisibilityStatusId = @VisibilityStatusId, @RightId = @RightId OUTPUT;


		SELECT @ActionID = ActionID
		FROM Enterprise.ACTION
		WHERE ObjectValue = 'Access HelpCenter'  AND 
			  ObjectType = 'Right' AND 
			  ParentActionId IS NULL;
		EXEC [Enterprise].[LinkActionToRights] @ActionID = @ActionID, @RightId = @RightId, @StatusId = @Status, @UserActionId = @UserActionId OUTPUT;

		SELECT @RightId = RightId
		FROM Enterprise.[Right] AS R
			 INNER JOIN
			 Enterprise.RightValueType AS RR
			 ON RR.RightValueTypeId = R.RightValueTypeId
		WHERE RR.Value = 'Default_AccessHelpCenter';
		SELECT @ActionID = ActionID
		FROM Enterprise.ACTION
		WHERE ObjectValue = 'Access HelpCenter'   AND 
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
WHERE value = 'Default_AccessHelpCenter';

SELECT @RightValueTypeId = RightValueTypeId
FROM Enterprise.RightValueType
WHERE value IN( 'Access to Help Center' );

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
UPDATE Enterprise.Product
  SET 
      Description = 'DepositIQ and Deposit Insurance are Deposit Alternatives that allow you to offer residents a low-cost move-in solution, generate ancillary revenue and reduce your financial risk.'
WHERE ProductId = 47;
GO
UPDATE ps
  SET 
      value = 'https://www.realpage.com/deposit-alternatives'
FROM  Enterprise.ProductSetting ps
	INNER JOIN Enterprise.ProductSettingType pst
		ON ps.ProductSettingTypeId = pst.ProductSettingTypeId
WHERE pst.Name  = 'LearnMore'
      AND ProductId = 47;
GO
--NEW ROLE WITH CIMPL Rights
SET NOCOUNT ON
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
DECLARE @TRightShortName NVARCHAR(100);
DECLARE @TargetProductId INT;
DECLARE @VisibilityStatusId INT;
DECLARE @Status_Role INT;
DECLARE @Status_Right INT;
IF OBJECT_ID('tempdb..#RightsUnifiedSettings') IS NOT NULL
BEGIN
	DROP TABLE #RightsUnifiedSettings;
END;

IF OBJECT_ID('tempdb..#HoldPartyForUnifiedSettings') IS NOT NULL
BEGIN
	DROP TABLE #HoldPartyForUnifiedSettings;
END;

SELECT @ProductId = ProductId
FROM Enterprise.Product
WHERE name = 'Unified Platform';

--select * from enterprise.product where name like '%lead%'

SELECT @TargetProductId = ProductId
FROM Enterprise.Product
WHERE Name = 'Unified Platform';

SET @ActionValueID = 1;

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

SELECT @Status_Role = ST.StatusTypeId
FROM Enterprise.StatusTypeCategoryType AS STCT
	 INNER JOIN
	 Enterprise.StatusTypeCategory AS STC
	 ON STCT.StatusTypeCategoryTypeId = STC.StatusTypeCategoryTypeId
	 INNER JOIN
	 Enterprise.StatusTypeCategoryClassification AS STCC
	 ON STCC.StatusTypeCategoryId = STC.StatusTypeCategoryId
	 INNER JOIN
	 Enterprise.StatusType AS ST
	 ON ST.StatusTypeId = STCC.StatusTypeId
WHERE STC.Name = 'Role Type' AND 
	  ST.Name = 'System';

SELECT @Status_Right = ST.StatusTypeId
FROM Enterprise.StatusTypeCategoryType AS STCT
	 INNER JOIN
	 Enterprise.StatusTypeCategory AS STC
	 ON STCT.StatusTypeCategoryTypeId = STC.StatusTypeCategoryTypeId
	 INNER JOIN
	 Enterprise.StatusTypeCategoryClassification AS STCC
	 ON STCC.StatusTypeCategoryId = STC.StatusTypeCategoryId
	 INNER JOIN
	 Enterprise.StatusType AS ST
	 ON ST.StatusTypeId = STCC.StatusTypeId
WHERE STC.Name = 'Right Type' AND 
	  ST.Name = 'System';

SELECT DISTINCT 
	   IDENTITY(int, 1, 1) AS RowNumber, o.PartyId AS OrganizationPartyID, 0 AS PStatus
INTO #HoldPartyForUnifiedSettings
FROM Enterprise.Organization AS o
	 INNER JOIN
	 Enterprise.Party AS p
	 ON P.partyid = O.PartyId
--WHERE O.Name = 'RealPage Employee';

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
	IF NOT EXISTS
	(
		SELECT 1
		FROM Enterprise.Role AS R
			 INNER JOIN
			 Enterprise.RoleValueType AS RVT
			 ON RVT.RoleValueTypeId = R.RoleValueTypeId
		WHERE Value = 'Basic End User & CIMPL' AND 
			  PartyID = @PartyId
	)
	BEGIN
		EXEC Enterprise.CreateRole @RoleName = N'Basic End User & CIMPL', @Description = N'', @RoleTypeID = 400, @PartyID = @PartyId, @RoleCategoryId = @Status_Role, @RoleID = @RoleId OUTPUT;
		SET @RoleName = 'Basic End User & CIMPL';
		SELECT @RoleId = RoleID
		FROM Enterprise.Role AS R
			 INNER JOIN
			 Enterprise.RoleValueType AS RVT
			 ON RVT.RoleValueTypeId = R.RoleValueTypeId
		WHERE Value = @RoleName AND 
			  PartyID = @PartyId;
		EXECUTE Enterprise.CreateRight @RoleID = @RoleId, @PartyId = @PartyId, @ProductId = @ProductId, @RightName = 'Ability to edit my own profile', @RightCategoryId = @Status_Right, @RightID = @RightID OUTPUT, @Description = '', @TargetProductId = @TargetProductId, @VisibilityStatusId = @VisibilityStatusId;
		EXECUTE Enterprise.CreateRight @RoleID = @RoleId, @PartyId = @PartyId, @ProductId = @ProductId, @RightName = 'Access to Product Learning Portal', @RightCategoryId = @Status_Right, @RightID = @RightID OUTPUT, @Description = '', @TargetProductId = @TargetProductId, @VisibilityStatusId = @VisibilityStatusId;
		EXECUTE Enterprise.CreateRight @RoleID = @RoleId, @PartyId = @PartyId, @ProductId = @ProductId, @RightName = 'Access to Help Center', @RightCategoryId = @Status_Right, @RightID = @RightID OUTPUT, @Description = '', @TargetProductId = @TargetProductId, @VisibilityStatusId = @VisibilityStatusId;
		EXECUTE Enterprise.CreateRight @RoleID = @RoleId, @PartyId = @PartyId, @ProductId = @ProductId, @RightName = 'Default_SideMenu_Users', @RightCategoryId = @Status_Right, @RightID = @RightID OUTPUT, @Description = '';
		EXECUTE Enterprise.CreateRight @RoleID = @RoleId, @PartyId = @PartyId, @ProductId = @ProductId, @RightName = 'Default_Dashboard_Users', @RightCategoryId = @Status_Right, @RightID = @RightID OUTPUT, @Description = '', @TargetProductId = @TargetProductId, @VisibilityStatusId = @VisibilityStatusId;
		SELECT @TargetProductId = ProductId
		FROM Enterprise.Product
		WHERE Name = 'Vendor Marketplace';
		EXECUTE Enterprise.CreateRight @RoleID = @RoleId, @PartyId = @PartyId, @ProductId = @ProductId,  @RightName = 'Access to Vendor Marketplace', @RightCategoryId = @Status_Right, @RightID = @RightID OUTPUT, @Description = '', @TargetProductId = @TargetProductId, @VisibilityStatusId = @VisibilityStatusId;
		SELECT @TargetProductId = ProductId
		FROM Enterprise.Product
		WHERE Name = 'CIMPL';
		EXECUTE Enterprise.CreateRight @RoleID = @RoleId, @PartyId = @PartyId, @ProductId = @ProductId,  @RightName = 'View CIMPL Implementation Questions', @RightCategoryId = @Status_Right, @RightID = @RightID OUTPUT, @Description = '', @TargetProductId = @TargetProductId, @VisibilityStatusId = @VisibilityStatusId;
		EXECUTE Enterprise.CreateRight @RoleID = @RoleId, @PartyId = @PartyId, @ProductId = @ProductId,  @RightName = 'Ability to Answer Questions for CIMPL', @RightCategoryId = @Status_Right, @RightID = @RightID OUTPUT, @Description = '', @TargetProductId = @TargetProductId, @VisibilityStatusId = @VisibilityStatusId;
		EXECUTE Enterprise.CreateRight @RoleID = @RoleId, @PartyId = @PartyId, @ProductId = @ProductId,  @RightName = 'Manage Sensitive Financial Data in CIMPL', @RightCategoryId = @Status_Right, @RightID = @RightID OUTPUT, @Description = '', @TargetProductId = @TargetProductId, @VisibilityStatusId = @VisibilityStatusId;
		EXECUTE Enterprise.CreateRight @RoleID = @RoleId, @PartyId = @PartyId, @ProductId = @ProductId,  @RightName = 'Manage Personally Identifiable Information (PII) in CIMPL', @RightCategoryId = @Status_Right, @RightID = @RightID OUTPUT, @Description = '', @TargetProductId = @TargetProductId, @VisibilityStatusId = @VisibilityStatusId;
		SELECT @ActionID = ActionID
		FROM Enterprise.ACTION
		WHERE ObjectValue = 'SideMenu' AND 
			  ObjectType = 'ROUTE' AND 
			  Description = 'User';
		SELECT @RoleId = RoleID
		FROM Enterprise.Role AS R
			 INNER JOIN
			 Enterprise.RoleValueType AS RVT
			 ON RVT.RoleValueTypeId = R.RoleValueTypeId
		WHERE Value = 'Basic End User & CIMPL' AND 
			  PartyID = @PartyId;
		SELECT @RightID = RightID
		FROM Enterprise.[Right] AS R
			 INNER JOIN
			 Enterprise.RightValueType AS RVT
			 ON RVT.RightValueTypeId = R.RightValueTypeId
		WHERE Value = 'Default_SideMenu_Users' AND 
			  RoleID = @RoleId;
		EXEC Enterprise.LinkActionToRights @ActionID = @ActionID, @RightId = @RightID, @StatusId = @Status_Right, @UserActionId = @UserActionID OUTPUT;
		SELECT @ActionID = ActionID
		FROM Enterprise.ACTION
		WHERE ObjectValue = 'Dashboard' AND 
			  ObjectType = 'ROUTE' AND 
			  Description = 'User';
		SELECT @RoleId = RoleID
		FROM Enterprise.Role AS R
			 INNER JOIN
			 Enterprise.RoleValueType AS RVT
			 ON RVT.RoleValueTypeId = R.RoleValueTypeId
		WHERE Value = 'Basic End User & CIMPL' AND 
			  PartyID = @PartyId;
		SELECT @RightID = RightID
		FROM Enterprise.[Right] AS R
			 INNER JOIN
			 Enterprise.RightValueType AS RVT
			 ON RVT.RightValueTypeId = R.RightValueTypeId
		WHERE Value = 'Default_Dashboard_Users' AND 
			  RoleID = @RoleId;
		EXEC Enterprise.LinkActionToRights @ActionID = @ActionID, @RightId = @RightID, @StatusId = @Status_Right, @UserActionId = @UserActionID OUTPUT;
		SELECT @ActionID = ActionID
		FROM Enterprise.ACTION
		WHERE ObjectValue = 'EditUser' AND 
			  ObjectType = 'ROUTE' AND 
			  Description = 'User';
		SELECT @RoleId = RoleID
		FROM Enterprise.Role AS R
			 INNER JOIN
			 Enterprise.RoleValueType AS RVT
			 ON RVT.RoleValueTypeId = R.RoleValueTypeId
		WHERE Value = 'Basic End User & CIMPL' AND 
			  PartyID = @PartyId;
		SELECT @RightID = RightID
		FROM Enterprise.[Right] AS R
			 INNER JOIN
			 Enterprise.RightValueType AS RVT
			 ON RVT.RightValueTypeId = R.RightValueTypeId
		WHERE Value = 'Ability to edit my own profile' AND 
			  RoleID = @RoleId;
		EXEC Enterprise.LinkActionToRights @ActionID = @ActionID, @RightId = @RightID, @StatusId = @Status_Right, @UserActionId = @UserActionID OUTPUT;
	END;
	UPDATE #HoldPartyForUnifiedSettings
	  SET PStatus = 1
	WHERE RowNumber = @PartyRowNum;
END;

go

declare @oldgoogletypeid int
		,@newgoogletypeid int
		,@oldgooglecontactid int
		,@newgooglecontactid int

select @oldgoogletypeid = IdentityProviderTypeId, @oldgooglecontactid = ContactMechanismId
from ident.IdentityProviderType where name = 'google'

select @newgoogletypeid = IdentityProviderTypeId, @newgooglecontactid = ContactMechanismId
from ident.IdentityProviderType where name = 'oidcgoogle'

if exists ( select top 1 1 from ident.userlogin where IdentityProviderTypeId = @oldgoogletypeid )
begin
	update ident.userlogin set IdentityProviderTypeId = @newgoogletypeid where IdentityProviderTypeId = @oldgoogletypeid
end

if exists ( select top 1 1 from enterprise.Organization where IdentityProviderTypeId = @oldgoogletypeid )
begin
	update enterprise.Organization set IdentityProviderTypeId = @newgoogletypeid where IdentityProviderTypeId = @oldgoogletypeid
end

if exists ( select top 1 1 from enterprise.PartyContactMechanism pcm inner join enterprise.Organization o on pcm.PartyId = o.PartyId where ContactMechanismId = @oldgooglecontactid )
begin
	update pcm set ContactMechanismId = @newgooglecontactid
	from enterprise.PartyContactMechanism pcm inner join enterprise.Organization o on pcm.PartyId = o.PartyId where ContactMechanismId = @oldgooglecontactid
end
go
DECLARE @ProductId INT, 
		@LoginURI NVARCHAR(100), 
		@SigningCertificateThumbprint NVARCHAR(50), 
		@ParentProductTypeId INT, 
		@ProductName NVARCHAR(100)= 'Help Center', 
		@LoginURL NVARCHAR(500), 
		@ProductUrl NVARCHAR(256), 
		@ServerName SYSNAME = @@SERVERNAME;

DECLARE @ProductConfiguration AS PRODUCTCONFIGURATIONTYPE;

SELECT @ParentProductTypeId = ProductTypeId
FROM Enterprise.ProductType
WHERE Name = 'Administration'
      AND ParentProductTypeId IS NULL;
IF NOT EXISTS
(
    SELECT TOP 1 1
    FROM enterprise.ProductType
    WHERE Name = 'Help Center'
)
    BEGIN
        EXEC [Enterprise].[CreateProductType] 
             @ProductTypeId = 507, 
             @ParentProductTypeId = @ParentProductTypeId, 
             @Name = @ProductName, 
             @Description = 'Unified Help Product KnowledgeBase', 
             @ProductTypeGUID = '72306C85-7BDC-460B-B96E-C0F0AD95F64A';
END;
SET @ProductId = 49;
IF NOT EXISTS
(
    SELECT 1
    FROM Enterprise.Product
    WHERE Name = @ProductName
)
    BEGIN
        EXEC Enterprise.CreateProduct 
             @ProductId = @ProductId, 
             @ProductGUID = 'C8B1B954-F868-4132-9ECF-0CD3D8B7F09B', 
             @Name = @ProductName, 
             @Description = @ProductName, 
             @ProductTypeId = 507;
        UPDATE Enterprise.Product
          SET 
              BooksProductCode = 'HLP'
        WHERE ProductId = @ProductId;
END;


IF @ServerName IN('RCDUSODBSQL001')
    BEGIN
        SET @ProductUrl = 'https://helpcenterdev.realpage.com';
END;
IF @ServerName = 'RCTUSODBSQL001'
    BEGIN
        SET @ProductUrl = 'https://helpcenterqa.realpage.com';
END;
IF @ServerName IN('RCQUSODBSQL001', 'RCVEUSODBSQL001', 'RCDUSODBSQL001A', 'RCIUSODBSQL002', 'RCTUSODBSQL001A') -- Need to chnage
    BEGIN
        SET @ProductUrl = 'https://helpcenterqa.realpage.com';
END;
IF @ServerName IN('RCPGBKDBSQL005A', 'RCPGBKDBSQL005B') -- Need to change
    BEGIN
        SET @ProductUrl = 'https://helpcenterqa.realpage.com';
END;
INSERT INTO @ProductConfiguration
(SettingName, 
 SettingDescription, 
 SettingValue
)
VALUES
('ClassName',  '',  'helpcenter'),
('ProductUrl',  '',  @ProductUrl),
('TitleId',  '',  'Help Center'),
('TitleUniqueId',  '',  '60C1DDA5-6DA2-40B7-A5BA-AD2412853336'),
('IsNewTab',  '', '1'),
('MetatagUniqueId',  '',  'Help Center'),
('IsResource',  '',  '1'),
('IsFavorite',  '',  '0'),
('LearnMore',  '',  'https://www.realpage.com/'),
('ProductStatus',  'Show if the external application was configured for the dashboard user.',  '8'),
('ShowInUserDetails',  'Should the product show in the New/Edit user pages',  '0'),
('ShowInRolesAndRights',  'Should the product show in the Role/Rights page',  '1'),
('ShowInAppSwitcher',  'Should the product show in the application switcher',  '0'),
('ShowInUserListFilter',  'Should the product show in the user list product pick list',  '0'),
('ProductAPIRequiresUser',  'Does the product require a user for api calls',  '0'),
('LockOnProductAccess',  '',  '0'),
('ProductNotAvailableForRegularUserNoEmail',  'Product Attribute for Product Not Available for Regular User No Email.',  '0');

SELECT * FROM @ProductConfiguration

IF @ServerName IN('RCDUSODBSQL001')
    BEGIN
        SET @LoginURL = 'https://helpcenterdev.realpage.com/';
END;
IF @ServerName = 'RCTUSODBSQL001'
    BEGIN
        SET @LoginURL = 'https://helpcenterqa.realpage.com';
END;
IF @ServerName IN('RCQUSODBSQL001', 'RCVEUSODBSQL001', 'RCDUSODBSQL001A', 'RCIUSODBSQL002', 'RCTUSODBSQL001A') -- Need to change
    BEGIN
        SET @LoginURL = 'https://helpcenterqa.realpage.com';
END;
IF @ServerName IN('RCPGBKDBSQL005A', 'RCPGBKDBSQL005B') -- Need to change
    BEGIN
        SET @LoginURL = 'https://helpcenterqa.realpage.com';
END;
SET @ProductID = 49;
SET @LoginURI = @LoginURL;
SET @SigningCertificateThumbprint = NULL;
EXEC Enterprise.ProductConfigurationSetup 
     @ProductId, 
     @LoginURI, 
     @SigningCertificateThumbprint, 
     @ProductConfiguration;
GO
DECLARE @ClientId INT;
IF NOT EXISTS
(
    SELECT 1
    FROM [Auth].[Clients]
    WHERE ClientCode = 'helpcenter-ui'
)
    BEGIN
        INSERT INTO [Auth].[Clients]
        ([ClientCode], 
         [ClientName], 
         [ClientUri], 
         [LogoUri], 
         [Flow], 
         [LogoutUri], 
         [IdentityTokenLifetime], 
         [AccessTokenLifetime], 
         [AuthorizationCodeLifetime], 
         [AbsoluteRefreshTokenLifetime], 
         [SlidingRefreshTokenLifetime], 
         [RefreshTokenUsage], 
         [RefreshTokenExpiration], 
         [AccessTokenType], 
         [UpdateAccessTokenOnRefresh], 
         [Enabled], 
         [LogoutSessionRequired], 
         [RequireSignOutPrompt], 
         [AllowAccessToAllScopes], 
         [AllowClientCredentialsOnly], 
         [RequireConsent], 
         [AllowRememberConsent], 
         [EnableLocalLogin], 
         [IncludeJwtId], 
         [AlwaysSendClientClaims], 
         [PrefixClientClaims], 
         [AllowAccessToAllGrantTypes]
        )
        VALUES
        (N'helpcenter-ui', 
         N'Unified Help Product KnowledgeBase', 
         NULL, 
         NULL, 
         1, 
         NULL, 
         36000, 
         36000, 
         36000, 
         0, 
         0, 
         0, 
         0, 
         1, 
         0, 
         1, 
         1, 
         0, 
         1, 
         0, 
         0, 
         1, 
         1, 
         1, 
         1, 
         1, 
         1
        );
        SELECT @ClientId = SCOPE_IDENTITY();
END;
IF @ClientId IS NOT NULL
   AND NOT EXISTS
(
    SELECT 1
    FROM [Auth].[ClientScopes]
    WHERE ClientId = @ClientId
          AND [Scope] = 'openid'
)
    BEGIN
        INSERT INTO [Auth].[ClientScopes]
        ([ClientId], 
         [Scope]
        )
        VALUES
        (@ClientId, 
         N'openid'
        );
END;
IF @ClientId IS NOT NULL
   AND NOT EXISTS
(
    SELECT 1
    FROM [Auth].[ClientScopes]
    WHERE ClientId = @ClientId
          AND [Scope] = 'profile'
)
    BEGIN
        INSERT INTO [Auth].[ClientScopes]
        ([ClientId], 
         [Scope]
        )
        VALUES
        (@ClientId, 
         N'profile'
        );
END;
IF @ClientId IS NOT NULL
   AND NOT EXISTS
(
    SELECT 1
    FROM [Auth].[ClientScopes]
    WHERE ClientId = @ClientId
          AND [Scope] = 'userinfoapi'
)
    BEGIN
        INSERT INTO [Auth].[ClientScopes]
        ([ClientId], 
         [Scope]
        )
        VALUES
        (@ClientId, 
         N'userinfoapi'
        );
END;
IF @ClientId IS NOT NULL
   AND NOT EXISTS
(
    SELECT 1
    FROM [Auth].[ClientRedirectUris]
    WHERE ClientId = @ClientId
)
    BEGIN
        INSERT INTO [Auth].[ClientRedirectUris]
        ([ClientId], 
         Uri
        )
        VALUES
        (@ClientId, 
         'https://aim.realpage.com/pim-unitycallback/test'
        );
END;
GO