
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
      value = 'www.realpage.com/deposit-alternatives'
FROM  Enterprise.ProductSetting ps
	INNER JOIN Enterprise.ProductSettingType pst
		ON ps.ProductSettingTypeId = pst.ProductSettingTypeId
WHERE pst.Name  = 'LearnMore'
      AND ProductId = 47;
GO