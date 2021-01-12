
GO
DECLARE @ProductTypeId INT= 505, 
@ProductId INT, 
@LoginURI NVARCHAR(100), 
@SigningCertificateThumbprint NVARCHAR(50), 
@ParentProductTypeId INT, 
@ProductName NVARCHAR(100)= 'CIMPL';
DECLARE @ProductConfiguration AS PRODUCTCONFIGURATIONTYPE;

--Create root product type
IF NOT EXISTS
(
    SELECT TOP 1 1
    FROM Enterprise.ProductType
    WHERE Name = 'CIMPL'
)
    BEGIN
        EXEC [Enterprise].[CreateProductType]
             @ProductTypeId = 506,
             @ParentProductTypeId = 500,
             @Name = 'CIMPL',
             @Description = 'CIMPL',
             @ProductTypeGUID = '58388758-3DA6-4CB6-9BB0-7AB44F57D97C';
    END;

SET @ProductId = 45;

IF NOT EXISTS
(
    SELECT 1
    FROM Enterprise.Product
    WHERE Name = @ProductName
)
    BEGIN
        EXEC Enterprise.CreateProduct
             @ProductId = @ProductId,
             @ProductGUID = 'EC6B2C46-7F23-4D7E-980E-AA6703B6846E',
             @Name = @ProductName,
             @Description = @ProductName,
             @ProductTypeId = 506;
        UPDATE Enterprise.Product
          SET
              BooksProductCode = 'CIMPL'
        WHERE ProductId = @ProductId;
    END;

SET @ProductId = 45;
DECLARE @ServerName SYSNAME
SELECT @ServerName = @@SERVERNAME

-- ALWAYS CHECK TO MAKE SURE THE SETTINGS ARE NOT ALREADY THERE!
IF NOT EXISTS ( SELECT TOP 1 PRODUCTID FROM Ident.SamlProductSettings WHERE ProductId = @ProductId )
BEGIN
	INSERT INTO @ProductConfiguration
	(SettingName,
	SettingDescription,
	SettingValue
	)
	VALUES
	('ClientId','','')
	,('ClassName','','')
	,('ProductUrl','','product/cimpl')
	,('TitleId','','')
	,('TitleUniqueId','','')
	,('IsNewTab','','1')
	,('MetatagUniqueId','','')
	,('IsResource','','1')
	,('ProductStatus','Show if the external application was configured for the dashboard user.','8')
	,('ShowInUserDetails','Should the product show in the New/Edit user pages','0')
	,('ShowInRolesAndRights','Should the product show in the Role/Rights page','0')
	,('ShowInAppSwitcher','Should the product show in the application switcher','1')
	,('ShowInUserListFilter','Should the product show in the user list product pick list','0')
	,('ProductAPIRequiresUser','Does the product require a user for api calls','0')
	,('ProductNotAvailableForRegularUserNoEmail','Product Attribute for Product Not Available for Regular User No Email.','0')
	,('LockOnProductAccess', '', '')
	,('ApiEndPoint','','')
	,('ProductStatus', '', '')

	SET @ProductID = 45
	SET @SigningCertificateThumbprint = 'NA'
	SET @ProductID = 45
	SET @SigningCertificateThumbprint = 'NA'

	SET @LoginURI = 'https://cimpl-dev.realpage.com'
	if @ServerName = 'RCDUSODBSQL001' -- (DEV)
	begin
		IF DB_NAME() = 'UPDEV'
		begin
			SET @LoginURI = 'https://cimpl-dev.realpage.com'
		end

		IF DB_NAME() = 'UPLocal'
		begin
			SET @LoginURI = 'https://cimpl-dev.realpage.com'
		end

	end

	if @ServerName = 'RCTUSODBSQL001' -- (QA)
	begin
		SET @LoginURI = 'https://cimpl-qa.realpage.com'
	end

	if @ServerName = 'RCQUSODBSQL001' -- (SAT)
	begin
		SET @LoginURI = 'https://cimpl-sat.realpage.com'
	end

	if @ServerName = 'RCTUSODBSQL001A' OR @ServerName = 'RCTUSODBSQL001B' -- (UAT)
	begin
		SET @LoginURI = 'https://cimpl-uat.realpage.com'
	end

	if @ServerName = 'RCVGBKDBSQL001' -- (DEMO)
	begin
		SET @LoginURI = 'https://cimpl-demo.realpage.com'
	end

	if @ServerName = 'WIN-H1CS7HBGS9V' -- (PREPROD)
	begin
		SET @LoginURI = 'https://cimpl-preprod.realpage.com'
	end

	if @ServerName = 'RCPGBKDBSQL005A' OR @ServerName = 'RCPGBKDBSQL005B'-- (PROD)
	begin
		SET @LoginURI = 'https://cimpl.realpage.com'
	end

	SET @LoginURI = 'https://cimpl-dev.realpage.com'
	if @ServerName = 'RCDUSODBSQL001' -- (DEV)
	begin
		IF DB_NAME() = 'UPDEV'
		begin
			SET @LoginURI = 'https://cimpl-dev.realpage.com'
		end

		IF DB_NAME() = 'UPLocal'
		begin
			SET @LoginURI = 'https://cimpl-dev.realpage.com'
		end

	end

	if @ServerName = 'RCTUSODBSQL001' -- (QA)
	begin
		SET @LoginURI = 'https://cimpl-qa.realpage.com'
	end

	if @ServerName = 'RCQUSODBSQL001' -- (SAT)
	begin
		SET @LoginURI = 'https://cimpl-sat.realpage.com'
	end

	if @ServerName = 'RCTUSODBSQL001A' OR @ServerName = 'RCTUSODBSQL001B' -- (UAT)
	begin
		SET @LoginURI = 'https://cimpl-uat.realpage.com'
	end

	if @ServerName = 'RCVGBKDBSQL001' -- (DEMO)
	begin
		SET @LoginURI = 'https://cimpl-demo.realpage.com'
	end

	if @ServerName = 'WIN-H1CS7HBGS9V' -- (PREPROD)
	begin
		SET @LoginURI = 'https://cimpl-preprod.realpage.com'
	end

	if @ServerName = 'RCPGBKDBSQL005A' OR @ServerName = 'RCPGBKDBSQL005B'-- (PROD)
	begin
		SET @LoginURI = 'https://cimpl.realpage.com'
	end

	EXEC Enterprise.ProductConfigurationSetup @ProductId, @LoginURI, @SigningCertificateThumbprint, @ProductConfiguration;
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
VALUES( 1, 'View CIMPL Implementation Questions', 'View CIMPL Implementation Questions', 'ViewCIMPLQuestions' );

SELECT @ProductId = ProductId
FROM Enterprise.Product
WHERE name = 'Unified Platform';

--select * from enterprise.product where name like '%lead%'

SELECT @TargetProductId = ProductId
FROM Enterprise.Product
WHERE Name = 'CIMPL';

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
	WHERE ObjectValue = 'View CIMPL Questions' AND 
		  ParentActionId IS NULL
)
BEGIN
	EXEC Enterprise.CreateAction @ProductID = @ProductId, @Action = N'View CIMPL Questions', @ActionTarget = N'Right', @ActionbValueTypeId = 1, @Description = '', @ActionID = @ActionID OUTPUT;
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
	WHERE ObjectValue = 'View CIMPL Questions' AND 
		  ParentActionID = @ParentActionId
)
BEGIN
	EXEC [Enterprise].[CreateAction] @ProductID = @ProductId, @Action = N'View CIMPL Questions', @ActionTarget = N'Right', @ActionbValueTypeId = 1, @Description = '', @ParentActionID = @ParentActionId, @ActionID = @ActionID OUTPUT;
	SELECT @ActionID AS N'@ActionID';
END;

SELECT DISTINCT 
	   IDENTITY(int, 1, 1) AS RowNumber, OrganizationPartyID, 0 AS PStatus
INTO #HoldPartyForUnifiedSettings
FROM Person.Persona AS P
	 INNER JOIN
	 Enterprise.Organization AS O
	 ON P.OrganizationPartyId = O.PartyId
--WHERE O.Name = 'RealPage Employee'

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
		EXECUTE Enterprise.CreateRight @RoleId = -1, @RightName = 'Default_ViewCIMPLQuestions', @ShortName = 'ViewCIMPLQuestions', @RightCategoryId = @RightCategory, @PartyId = @PartyId, @ProductId = @ProductId, @Description = '',  @TargetProductId = @TargetProductId, @VisibilityStatusId = @VisibilityStatusId, @RightId = @RightId OUTPUT;
		EXECUTE Enterprise.CreateRight @RoleId = @RoleId, @RightName = @TRightName, @RightCategoryId = @RightCategory, @PartyId = @PartyId, @ProductId = @ProductId, @Shortname = @TRightShortName, @Description = @TRightDesc, @TargetProductId = @TargetProductId, @VisibilityStatusId = @VisibilityStatusId, @RightId = @RightId OUTPUT;


		SELECT @ActionID = ActionID
		FROM Enterprise.ACTION
		WHERE ObjectValue = 'View CIMPL Questions'  AND 
			  ObjectType = 'Right' AND 
			  ParentActionId IS NULL;
		EXEC [Enterprise].[LinkActionToRights] @ActionID = @ActionID, @RightId = @RightId, @StatusId = @Status, @UserActionId = @UserActionId OUTPUT;

		SELECT @RightId = RightId
		FROM Enterprise.[Right] AS R
			 INNER JOIN
			 Enterprise.RightValueType AS RR
			 ON RR.RightValueTypeId = R.RightValueTypeId
		WHERE RR.Value = 'Default_ViewCIMPLQuestions';
		SELECT @ActionID = ActionID
		FROM Enterprise.ACTION
		WHERE ObjectValue = 'View CIMPL Questions'   AND 
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
WHERE value = 'Default_ViewCIMPLQuestions';

SELECT @RightValueTypeId = RightValueTypeId
FROM Enterprise.RightValueType
WHERE value IN( 'View CIMPL Implementation Questions' );

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
VALUES( 1, 'Ability to Manage Questions  for CIMPL', 'Ability to Manage Questions  for CIMPL', 'ManageCIMPLQuestions' );

SELECT @ProductId = ProductId
FROM Enterprise.Product
WHERE name = 'Unified Platform';

--select * from enterprise.product where name like '%lead%'

SELECT @TargetProductId = ProductId
FROM Enterprise.Product
WHERE Name = 'CIMPL';

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
	WHERE ObjectValue = 'Manage CIMPL Questions' AND 
		  ParentActionId IS NULL
)
BEGIN
	EXEC Enterprise.CreateAction @ProductID = @ProductId, @Action = N'Manage CIMPL Questions', @ActionTarget = N'Right', @ActionbValueTypeId = 1, @Description = '', @ActionID = @ActionID OUTPUT;
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
	WHERE ObjectValue = 'Manage CIMPL Questions' AND 
		  ParentActionID = @ParentActionId
)
BEGIN
	EXEC [Enterprise].[CreateAction] @ProductID = @ProductId, @Action = N'Manage CIMPL Questions', @ActionTarget = N'Right', @ActionbValueTypeId = 1, @Description = '', @ParentActionID = @ParentActionId, @ActionID = @ActionID OUTPUT;
	SELECT @ActionID AS N'@ActionID';
END;

SELECT DISTINCT 
	   IDENTITY(int, 1, 1) AS RowNumber, OrganizationPartyID, 0 AS PStatus
INTO #HoldPartyForUnifiedSettings
FROM Person.Persona AS P
	 INNER JOIN
	 Enterprise.Organization AS O
	 ON P.OrganizationPartyId = O.PartyId
--WHERE O.Name = 'RealPage Employee'

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
		EXECUTE Enterprise.CreateRight @RoleId = -1, @RightName = 'Default_ManageCIMPLQuestions', @ShortName = 'ManageCIMPLQuestions', @RightCategoryId = @RightCategory, @PartyId = @PartyId, @ProductId = @ProductId, @Description = '',  @TargetProductId = @TargetProductId, @VisibilityStatusId = @VisibilityStatusId, @RightId = @RightId OUTPUT;
		EXECUTE Enterprise.CreateRight @RoleId = @RoleId, @RightName = @TRightName, @RightCategoryId = @RightCategory, @PartyId = @PartyId, @ProductId = @ProductId, @Shortname = @TRightShortName, @Description = @TRightDesc, @TargetProductId = @TargetProductId, @VisibilityStatusId = @VisibilityStatusId, @RightId = @RightId OUTPUT;


		SELECT @ActionID = ActionID
		FROM Enterprise.ACTION
		WHERE ObjectValue = 'Manage CIMPL Questions'  AND 
			  ObjectType = 'Right' AND 
			  ParentActionId IS NULL;
		EXEC [Enterprise].[LinkActionToRights] @ActionID = @ActionID, @RightId = @RightId, @StatusId = @Status, @UserActionId = @UserActionId OUTPUT;

		SELECT @RightId = RightId
		FROM Enterprise.[Right] AS R
			 INNER JOIN
			 Enterprise.RightValueType AS RR
			 ON RR.RightValueTypeId = R.RightValueTypeId
		WHERE RR.Value = 'Default_ManageCIMPLQuestions';
		SELECT @ActionID = ActionID
		FROM Enterprise.ACTION
		WHERE ObjectValue = 'Manage CIMPL Questions'   AND 
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
WHERE value = 'Default_ManageCIMPLQuestions';

SELECT @RightValueTypeId = RightValueTypeId
FROM Enterprise.RightValueType
WHERE value IN( 'Ability to Manage Questions  for CIMPL' );

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



DECLARE @ProductTypeId INT= 505, 
@ProductId INT, 
@LoginURI NVARCHAR(100), 
@SigningCertificateThumbprint NVARCHAR(50), 
@ParentProductTypeId INT, 
@ProductName NVARCHAR(100)= 'Site Spend Management Portal';
DECLARE @ProductConfiguration AS PRODUCTCONFIGURATIONTYPE;
select newid()
--Create root product type
--IF NOT EXISTS
--(
--    SELECT TOP 1 1
--    FROM Enterprise.ProductType
--    WHERE Name = 'CIMPL'
--)
--    BEGIN
--        EXEC [Enterprise].[CreateProductType]
--             @ProductTypeId = 506,
--             @ParentProductTypeId = 500,
--             @Name = 'CIMPL',
--             @Description = 'CIMPL',
--             @ProductTypeGUID = '58388758-3DA6-4CB6-9BB0-7AB44F57D97C';
--    END;

SET @ProductId = 46;

IF NOT EXISTS
(
    SELECT 1
    FROM Enterprise.Product
    WHERE Name = @ProductName
)
    BEGIN
        EXEC Enterprise.CreateProduct
             @ProductId = @ProductId,
             @ProductGUID = '4ABA5994-2809-48AE-B668-5FA03D969F9F',
             @Name = @ProductName,
             @Description = @ProductName,
             @ProductTypeId = 102;
        UPDATE Enterprise.Product
          SET
              BooksProductCode = 'SSM'
        WHERE ProductId = @ProductId;
    END;



SET @ProductId = 46;
-- ALWAYS CHECK TO MAKE SURE THE SETTINGS ARE NOT ALREADY THERE!
IF NOT EXISTS ( SELECT TOP 1 PRODUCTID FROM Ident.SamlProductSettings WHERE ProductId = @ProductId )
BEGIN
	INSERT INTO @ProductConfiguration
	(SettingName,
	SettingDescription,
	SettingValue
	)
	VALUES
	('ClientId','','')
	,('ClassName','','')
	,('ProductUrl','','')
	,('TitleId','','')
	,('TitleUniqueId','','')
	,('IsNewTab','','0')
	,('MetatagUniqueId','','')
	,('IsResource','','0')
	,('ProductStatus','Show if the external application was configured for the dashboard user.','8')
	,('ShowInUserDetails','Should the product show in the New/Edit user pages','0')
	,('ShowInRolesAndRights','Should the product show in the Role/Rights page','0')
	,('ShowInAppSwitcher','Should the product show in the application switcher','0')
	,('ShowInUserListFilter','Should the product show in the user list product pick list','0')
	,('ProductAPIRequiresUser','Does the product require a user for api calls','0')
	,('ProductNotAvailableForRegularUserNoEmail','Product Attribute for Product Not Available for Regular User No Email.','0')
	,('LockOnProductAccess', '', '')
	,('ApiEndPoint','','')
	,('ProductStatus', '', '')


	SET @ProductID = 46
	SET @LoginURI = ''
	SET @SigningCertificateThumbprint = 'EF26FEC08C554976572E8A9767DDA437AC452CF6'
	EXEC Enterprise.ProductConfigurationSetup @ProductId, @LoginURI, @SigningCertificateThumbprint, @ProductConfiguration;
END

GO


GO
IF EXISTS(SELECT 1 FROM Enterprise.Product WHERE Name IN ('Accounting', 'RealPage Accounting'))
BEGIN
	UPDATE Enterprise.Product
		SET Name = 'Financial Suite',  Description = 'Financial Suite is a feature-rich, web-based property management accounting solution designed for corporate operations of any size.'
	WHERE Name IN ('Accounting', 'RealPage Accounting')
END

GO


DECLARE @ServerName SYSNAME
SELECT @ServerName = @@SERVERNAME

declare @OLDULURL VARCHAR(200) = ''
,@NEWULURLONLY VARCHAR(200) = ''
,@NEWULURL VARCHAR(200) = ''
,@RUNUPDATE tinyint = 1

if @ServerName = 'RCDUSODBSQL001' -- (DEV)
begin
	IF DB_NAME() = 'UPDEV'
	begin
		SET	@OLDULURL = 'mydev.corp.realpage.com'
		SET @NEWULURLONLY = 'www-dev.realpage.com'
		SET @NEWULURL = 'www-dev.realpage.com/home/'
	end

	IF DB_NAME() = 'UPLocal'
	begin
		SET	@OLDULURL = 'mylocal.corp.realpage.com'
		SET @NEWULURLONLY = 'www-local.realpage.com'
		SET @NEWULURL = 'www-local.realpage.com/home/'
	end

end

if @ServerName = 'RCTUSODBSQL001' -- (QA)
begin
	SET	@OLDULURL = 'myqa.realpage.com'
	SET @NEWULURLONLY = 'www-qa.realpage.com'
	SET @NEWULURL = 'www-qa.realpage.com/home/'
end

if @ServerName = 'RCQUSODBSQL001' -- (SAT)
begin
	SET	@OLDULURL = 'mysat.realpage.com'
	SET @NEWULURLONLY = 'www-sat.realpage.com'
	SET @NEWULURL = 'www-sat.realpage.com/home/'
end

if @ServerName = 'RCTUSODBSQL001A' OR @ServerName = 'RCTUSODBSQL001B' -- (UAT)
begin
	SET	@OLDULURL = 'myuat.realpage.com'
	SET @NEWULURLONLY = 'www-uat.realpage.com'
	SET @NEWULURL = 'www-uat.realpage.com/home/'
end

if @ServerName = 'RCVGBKDBSQL001' -- (DEMO)
begin
	SET	@OLDULURL = 'mydemo.realpage.com'
	SET @NEWULURLONLY = 'www-demo.realpage.com'
	SET @NEWULURL = 'www-demo.realpage.com/home/'
end

if @ServerName = 'WIN-H1CS7HBGS9V' -- (PREPROD)
begin
	SET	@OLDULURL = 'mypreprod.realpage.com'
	SET @NEWULURLONLY = 'www-preprod.realpage.com'
	SET @NEWULURL = 'www-preprod.realpage.com/home/'
end

if @ServerName = 'RCPGBKDBSQL005A' OR @ServerName = 'RCPGBKDBSQL005B'-- (PROD)
begin
	SET	@OLDULURL = 'my.realpage.com'
	SET @NEWULURLONLY = 'www.realpage.com'
	SET @NEWULURL = 'www.realpage.com/home/'
end

--select @OLDULURL, @NEWULURLONLY, @NEWULURL

IF (@OLDULURL != '')
begin
	
	select replace(value, '/product/', 'product/') from Enterprise.ProductSetting where ProductSettingTypeId = 4 and Value like '%product%'
	update enterprise.productsetting set value = replace(value, '/product/', 'product/') where ProductSettingTypeId = 4 and Value like '%product%'

	select '#/employee-access' from enterprise.productsetting where productid = 35 and productsettingtypeid = 4 and value = '/#/employee-access'
	update enterprise.productsetting set value = '#/employee-access' where productid = 35 and productsettingtypeid = 4 and value = '/#/employee-access'

	select replace(Uri, @OLDULURL,@NEWULURL) from Auth.ClientRedirectUris where Uri LIKE '%'+@OLDULURL+'%'
	update Auth.ClientRedirectUris set Uri = replace(Uri, @OLDULURL,@NEWULURL) where Uri LIKE '%'+@OLDULURL+'%'
	
	select replace(value, @OLDULURL,@NEWULURLONLY) from Enterprise.MasterSetting where MasterSettingTypeId in ( 8,9) and Value like '%'+@OLDULURL+'%' 
	update Enterprise.MasterSetting set Value = replace(value, @OLDULURL,@NEWULURLONLY) where MasterSettingTypeId in ( 8,9) and Value like '%'+@OLDULURL+'%'
		
	select replace(uri, @OLDULURL,@NEWULURL) from Auth.ClientPostLogoutRedirectUris where Uri like '%'+@OLDULURL+'%' and not exists (select 1 from auth.ClientPostLogoutRedirectUris where Uri = replace(uri, @OLDULURL,@NEWULURL) )
	update Auth.ClientPostLogoutRedirectUris set Uri = replace(uri, @OLDULURL,@NEWULURL) where Uri like '%'+@OLDULURL+'%' and not exists (select 1 from auth.ClientPostLogoutRedirectUris where Uri = replace(uri, @OLDULURL,@NEWULURL) )

	select replace(value, @OLDULURL+'/',@NEWULURL) from Ident.IdentityProviderSetting where Value like '%'+@OLDULURL+'%' and not exists ( select 1 from Ident.IdentityProviderSetting where Value = replace(value, @OLDULURL+'/',@NEWULURL))
	update Ident.IdentityProviderSetting set Value = replace(value, @OLDULURL+'/',@NEWULURL) where Value like '%'+@OLDULURL+'%' and not exists ( select 1 from Ident.IdentityProviderSetting where Value = replace(value, @OLDULURL+'/',@NEWULURL))
end

GO

IF NOT EXISTS
(
    SELECT 1
    FROM Ident.UserLogin
    WHERE StatusId > 0
)
BEGIN

   
 ;WITH CTEFDate
	    AS (
	    SELECT USerId,
			 [1],
			 [2],
			 [3]
	    FROM
		    (
			    SELECT ul.UserId,
					  us.StatusTypeId,
					  us.FromDate
			    FROM Ident.UserLogin AS ul
					INNER JOIN Person.Persona PE ON Ul.UserId = PE.Userid
					INNER JOIN Enterprise.Party AS p ON p.PartyId = pe.PersonPartyId
					LEFT JOIN Ident.UserCurrentStatus AS us ON us.UserId = ul.UserId
		    ) AS T PIVOT(MAX(FromDate) FOR StatusTypeId IN([1],
										 [2],
										 [3])) AS P1),
	    CTETDate
	    AS (
	    SELECT USerId,
			 [1],
			 [2],
			 [3]
	    FROM
		    (
			    SELECT ul.UserId,
					  us.StatusTypeId,
					  us.ThruDate
			    FROM Ident.UserLogin AS ul
					INNER JOIN Person.Persona PE ON Ul.UserId = PE.Userid
					INNER JOIN Enterprise.Party AS p ON p.PartyId = pe.PersonPartyId
					LEFT JOIN Ident.UserCurrentStatus AS us ON us.UserId = ul.UserId
		    ) AS T PIVOT(MAX(THruDate) FOR StatusTypeId IN([1],
										 [2],
										 [3])) AS P1)
    
	    SELECT UL.UserId,
			 P.PersonaId,
			 PA.Name,
			 PE.FIrstName,
			 PE.LastNAme,
			 UL.LoginName,
			 UL.LastLoginDate,
			 UL.FromDate AS ULActiveDate,
			 UL.ThruDate AS ULThrudate,
			 A.[1] AS ActiveFdate,
			 A.[2] AS PendingFDate,
			 A.[3] LockedFDate,
			 B.[1] AS ActiveTdate,
			 B.[2] AS PendingTDate,
			 B.[3] LockedTDate,
			 CAST('NoStatus' AS NVARCHAR(50)) AS PStatus
	    INTO #Temp
	    FROM CTEFDate A
		    INNER JOIN CTETDate B ON B.UserId = A.UserId
		    INNER JOIN Ident.UserLogin UL ON A.UserId = UL.UserId
		    RIGHT JOIN Person.Persona P ON P.UserId = UL.UserId
		    INNER JOIN Enterprise.Organization PA ON PA.PartyId = P.OrganizationPartyId
		    INNER JOIN Person.Person PE ON PE.PartyId = P.PersonPartyID
	    ORDER BY UserId;
		ALTER TABLE #Temp ADD StatusThruDate DATETIME NULL
		select * from #temp
    UPDATE #Temp
	   SET
		  PStatus = CASE
				    WHEN U.IdentityProviderTypeId != 4
				    THEN 'Active'
				END
    FROM #Temp A
		  INNER JOIN Ident.UserLogin U ON U.USerId = A.USerId;
    UPDATE #Temp
	   SET
		  PStatus = 'Active'
    FROM #Temp A
		  INNER JOIN Ident.UserLogin U ON U.USerId = A.USerId
    WHERE ActiveFDate IS NOT NULL
		  AND PendingFDate IS NOT NULL
		  AND LockedFDate IS NULL
		  AND ActiveTdate IS NULL
		  AND PendingTDate IS NULL
		  AND LockedTDate IS NULL
		  AND PStatus IS NULL;
    UPDATE #Temp
	   SET
		  PStatus = 'Active'
    FROM #Temp A
		  INNER JOIN Ident.UserLogin U ON U.USerId = A.USerId
    WHERE ActiveFDate IS NOT NULL
		  AND PendingFDate IS NOT NULL
		  AND LockedFDate IS NOT NULL
		  AND ActiveTdate IS NULL
		  AND PendingTDate IS NULL
		  AND LockedTDate IS NULL
		  AND PStatus IS NULL;
    UPDATE #Temp
	   SET
		  PStatus = 'Active'
    FROM #Temp A
		  INNER JOIN Ident.UserLogin U ON U.USerId = A.USerId
    WHERE ActiveFDate IS NOT NULL
		  AND PendingFDate IS NOT NULL
		  AND LockedFDate IS NOT NULL
		  AND ActiveTdate IS NULL
		  AND PendingTDate IS NULL
		  AND LockedTDate < GETUTCDATE()
		  AND PStatus IS NULL;

    UPDATE #Temp
	   SET
		  PStatus = 'Active'
    FROM #Temp A
		  INNER JOIN Ident.UserLogin U ON U.USerId = A.USerId
    WHERE ULActiveDate IS NOT NULL 
		    AND ULThruDate IS NULL
		    AND ActiveFDate IS  NULL
		  AND PendingFDate IS  NULL
		  AND LockedFDate IS  NULL
		  AND ActiveTdate IS NULL
		  AND PendingTDate IS NULL
		  AND LockedTDate IS NULL
		  AND PStatus IS NULL;

    UPDATE #Temp
	   SET
		  PStatus = 'Active'
    FROM #Temp A
		  INNER JOIN Ident.UserLogin U ON U.USerId = A.USerId
    WHERE ULActiveDate IS NOT NULL 
		    AND ULThruDate > GETUTCDATE()
		    AND ActiveFDate IS  NULL
		  AND PendingFDate IS  NULL
		  AND LockedFDate IS  NULL
		  AND ActiveTdate IS NULL
		  AND PendingTDate IS NULL
		  AND LockedTDate IS NULL
		  AND PStatus IS NULL;

    UPDATE #Temp
	   SET
		  PStatus = 'Active'
    FROM #Temp A
		  INNER JOIN Ident.UserLogin U ON U.USerId = A.USerId
    WHERE ActiveFDate IS NOT NULL
		  AND PendingFDate IS NULL
		  AND (LockedFDate IS NULL
			 OR LockedFDate < GETUTCDATE())
		  AND ActiveTdate IS NULL
		  AND PendingTDate IS NULL
		  AND (LockedTDate IS NULL
			 OR LockedFDate < GETUTCDATE() - 1)
		  AND PStatus IS NULL;

    UPDATE #Temp
	 SET
		PStatus = 'Pending',
		StatusThruDate = A.PendingTDate

	FROM #Temp A
	    INNER JOIN Ident.UserLogin U ON U.USerId = A.USerId
    WHERE ActiveFDate IS NOT NULL
		AND (PendingFDate IS NOT NULL
			OR PendingFDate < GETUTCDATE())
		AND (ActiveTdate IS NULL
			OR ActiveTDate > GETUTCDATE())
		AND PendingTDate > DATEADD(HH, 72, PendingFDate);



    UPDATE #Temp
	   SET
		  PStatus = 'Expired',
		StatusThruDate = PendingTDate
	FROM #Temp A
		  INNER JOIN Ident.UserLogin U ON U.USerId = A.USerId
    WHERE ActiveFDate IS NOT NULL
		  AND PendingFDate IS NOT NULL
		  AND LockedFDate IS NULL
		  AND ActiveTdate IS NULL
		  AND PendingTDate IS NOT NULL
		  AND LockedTDate IS NULL
		  AND (PendingFDate < PendingTDate OR PendingTDate < GETUTCDATE())
		  AND PStatus IS NULL;
    
	UPDATE #Temp
	   SET
		  PStatus = 'Expired',
		StatusThruDate = PendingTDate
	FROM #Temp A
		  INNER JOIN Ident.UserLogin U ON U.USerId = A.USerId
    WHERE A.LastLoginDate IS NULL
		  AND ActiveFDate IS NOT NULL
		  AND ULThruDate IS NULL
		  AND PendingFDate < GETUTCDATE()
		  AND LockedFDate IS NULL
		  AND ActiveTdate IS NULL
		  AND PEndingTDate IS NOT NULL
		 AND PendingTDate < GETUTCDATE()
		  AND LockedTDate IS NULL
		  AND PSTatus != 'Expired';
    UPDATE #Temp
	   SET
		  PStatus = 'Expired',
		StatusThruDate = PendingTDate
    FROM #Temp A
		  INNER JOIN Ident.UserLogin U ON U.USerId = A.USerId
    WHERE ActiveFDate IS NOT NULL
		  AND PendingFDate IS NOT NULL
		  AND LockedFDate IS NOT NULL
		  AND ActiveTdate IS NULL
		  AND PendingTDate IS NOT NULL
		  AND LockedTDate IS NULL
		  AND (PendingFDate < PendingTDate
			    OR PendingTDate < GETUTCDATE());

    UPDATE #Temp
	   SET
		  PStatus = 'Expired',
		StatusThruDate = PendingTDate
	FROM #Temp A
		  INNER JOIN Ident.UserLogin U ON U.USerId = A.USerId
    WHERE ActiveFDate IS NOT NULL
		  AND ULThruDate IS NULL
		  AND PendingFDate < GETUTCDATE()
		  AND LockedFDate IS NULL
		  AND ActiveTdate IS NULL
		  AND (PEndingTDate IS NOT NULL
			 AND PendingTDate < GETUTCDATE())
		  AND LockedTDate IS NULL
		  AND PSTatus != 'Expired';
    UPDATE #Temp
	   SET
		  PStatus = 'DISABLED'
    FROM #Temp A
		  INNER JOIN Ident.UserLogin U ON U.USerId = A.USerId
    WHERE(ActiveFDate < ActiveTDate
		  OR ActiveTDate < ActiveFDAte);
    UPDATE #Temp
	   SET
		  PStatus = 'DISABLED'
    FROM #Temp A
		  INNER JOIN Ident.UserLogin U ON U.USerId = A.USerId
    WHERE(ULActiveDate IS NOT NULL
		  AND ULThruDate < GETUTCDATE());
    UPDATE #Temp
	   SET
		  PStatus = 'Locked',
		StatusThruDate = LockedTDate
    FROM #Temp A
		  INNER JOIN Ident.UserLogin U ON U.USerId = A.USerId
    WHERE ActiveFDate IS NOT NULL
		  AND PendingFDate IS NOT NULL
		  AND LockedFDate IS NOT NULL
		  AND LockedTDate > GETUTCDATE();

    UPDATE #Temp
	   SET
		  PStatus = 'Locked',
		StatusThruDate = LockedTDate
    FROM #Temp A
		  INNER JOIN Ident.UserLogin U ON U.USerId = A.USerId
    WHERE ActiveFDate IS NOT NULL
		    AND ActiveTDate IS NULL
		  AND PendingFDate IS NOT NULL
		    AND PendingTDate IS NOT NULL
		  AND LockedFDate IS NOT NULL
		  AND LockedTDate IS NOT NULL
		    AND PStatus IS NULL;



    UPDATE UL
        SET
            UL.CreateDate = P.FromDate
    FROM Ident.UserLogin UL
            INNER JOIN Person.Persona P ON P.UserId = UL.UserId;
    UPDATE UL
        SET
            UL.StatusId = S.StatusTypeId,
			UL.StatusThruDate = T.StatusThruDate
    FROM #Temp T
            INNER JOIN Ident.UserLogin UL ON UL.UserId = T.UserId
            INNER JOIN Enterprise.StatusType S ON S.Name = T.PStatus;

	


END;
GO

DECLARE @StatusTypeCategoryType INT;
DECLARE @StatusTypeCategory INT;
IF NOT EXISTS
(
	SELECT	1
	FROM	[Enterprise].[StatusTypeCategoryType]
	WHERE	Name = 'Status'
)
BEGIN
	INSERT INTO [Enterprise].[StatusTypeCategoryType]
	(
		Name
	)
	VALUES
	(
		'Status'
	);
	SELECT	@StatusTypeCategoryType = SCOPE_IDENTITY();
END;
ELSE
BEGIN
	SELECT	@StatusTypeCategoryType = StatusTypeCategoryTypeId
	FROM	[Enterprise].[StatusTypeCategoryType]
	WHERE	Name = 'Status';
END;
IF NOT EXISTS
(
	SELECT	1
	FROM	[Enterprise].[StatusTypeCategory]
	WHERE	Name = 'User Status'
)
BEGIN
	INSERT INTO [Enterprise].[StatusTypeCategory]
	(
		StatusTypeCategoryTypeId,
		Name
	)
	VALUES
	(
		@StatusTypeCategoryType,
		'User Status'
	);
	SELECT @StatusTypeCategory = SCOPE_IDENTITY();
END;
ELSE
BEGIN
	SELECT	@StatusTypeCategory = StatusTypeCategoryId
	FROM	[Enterprise].[StatusTypeCategory]
	WHERE	Name = 'User Status';
END;
IF NOT EXISTS
(
	SELECT	1
	FROM	[Enterprise].[StatusTypeCategoryClassification] A
				INNER JOIN Enterprise.StatusType B ON A.StatusTypeId = B.StatusTypeId
	WHERE	A.StatusTypeCategoryId = @StatusTypeCategory
	AND		B.Name IN (N'Active', N'Pending', N'Locked', N'Expired', N'Disabled')
)
BEGIN
	INSERT INTO [Enterprise].[StatusTypeCategoryClassification]
	(
		StatusTypeId,
		StatusTypeCategoryId
	)
	SELECT	StatusTypeId,
				@StatusTypeCategory
	FROM	Enterprise.StatusType
	WHERE Name IN (N'Active', N'Pending', N'Locked', N'Expired', N'Disabled');
END;
GO

DECLARE @StatusTypeCategoryType INT;
DECLARE @StatusTypeCategory INT;
IF NOT EXISTS
(
	SELECT	1
	FROM	[Enterprise].[StatusTypeCategoryType]
	WHERE	Name = 'Status'
)
BEGIN
	INSERT INTO [Enterprise].[StatusTypeCategoryType]
	(
		Name
	)
	VALUES
	(
		'Status'
	);
	SELECT	@StatusTypeCategoryType = SCOPE_IDENTITY();
END;
ELSE
BEGIN
	SELECT	@StatusTypeCategoryType = StatusTypeCategoryTypeId
	FROM	[Enterprise].[StatusTypeCategoryType]
	WHERE	Name = 'Status';
END;
IF NOT EXISTS
(
	SELECT	1
	FROM	[Enterprise].[StatusTypeCategory]
	WHERE	Name = 'User Status'
)
BEGIN
	INSERT INTO [Enterprise].[StatusTypeCategory]
	(
		StatusTypeCategoryTypeId,
		Name
	)
	VALUES
	(
		@StatusTypeCategoryType,
		'User Status'
	);
	SELECT @StatusTypeCategory = SCOPE_IDENTITY();
END;
ELSE
BEGIN
	SELECT	@StatusTypeCategory = StatusTypeCategoryId
	FROM	[Enterprise].[StatusTypeCategory]
	WHERE	Name = 'User Status';
END;
IF NOT EXISTS
(
	SELECT	1
	FROM	[Enterprise].[StatusTypeCategoryClassification] A
				INNER JOIN Enterprise.StatusType B ON A.StatusTypeId = B.StatusTypeId
	WHERE	A.StatusTypeCategoryId = @StatusTypeCategory
	AND		B.Name IN (N'Active', N'Pending', N'Locked', N'Expired', N'Disabled')
)
BEGIN
	INSERT INTO [Enterprise].[StatusTypeCategoryClassification]
	(
		StatusTypeId,
		StatusTypeCategoryId
	)
	SELECT	StatusTypeId,
				@StatusTypeCategory
	FROM	Enterprise.StatusType
	WHERE Name IN (N'Active', N'Pending', N'Locked', N'Expired', N'Disabled');
END;
GO

IF EXISTS (SELECT 1 FROM Enterprise.Product WHERE Name = 'Document Management')
BEGIN
	UPDATE Enterprise.Product 
		SET Name = 'Document Director'
	WHERE Name = 'Document Management'
END	

IF EXISTS (SELECT 1 FROM Enterprise.ProductType WHERE Name = 'Document Management')
BEGIN
	UPDATE Enterprise.ProductType 
		SET Name = 'Document Director', Description = 'Document Director'
	WHERE Name = 'Document Management'
END	
GO
IF EXISTS (SELECT 1 FROM Enterprise.RightValueType WHERE value = 'Manage Document Management Product Access')
BEGIN
	UPDATE Enterprise.RightValueType
		SET value = 'Manage Document Director Product Access'
	WHERE value = 'Manage Document Management Product Access'
END	
GO



DECLARE @StatusTypeCategoryType INT;
DECLARE @StatusTypeCategory INT;
IF NOT EXISTS
(
    SELECT 1
    FROM [Enterprise].[StatusTypeCategoryType]
    WHERE Name = 'Status'
)
    BEGIN
        INSERT INTO [Enterprise].[StatusTypeCategoryType](Name)
    VALUES('Status');
        SELECT @StatusTypeCategoryType = SCOPE_IDENTITY();
    END;
    ELSE
    BEGIN
        SELECT @StatusTypeCategoryType = StatusTypeCategoryTypeId
        FROM [Enterprise].[StatusTypeCategoryType]
        WHERE Name = 'Status';
    END;
IF NOT EXISTS
(
    SELECT 1
    FROM [Enterprise].[StatusTypeCategory]
    WHERE Name = 'User Status'
)
    BEGIN
        INSERT INTO [Enterprise].[StatusTypeCategory]
(StatusTypeCategoryTypeId,
 Name
)
        VALUES
(@StatusTypeCategoryType,
 'User Status'
);
        SELECT @StatusTypeCategory = SCOPE_IDENTITY();
    END;
    ELSE
    BEGIN
        SELECT @StatusTypeCategory = StatusTypeCategoryId
        FROM [Enterprise].[StatusTypeCategory]
        WHERE Name = 'User Status';
    END;
IF NOT EXISTS
(
    SELECT 1
    FROM [Enterprise].[StatusTypeCategoryClassification] A
         INNER JOIN Enterprise.StatusType B ON A.StatusTypeId = B.StatusTypeId
    WHERE A.StatusTypeCategoryId = @StatusTypeCategory
          AND B.Name IN(N'Active', N'Pending', N'Locked', N'ForceResetPassword', N'Expired', N'Disabled')
)
    BEGIN
        INSERT INTO [Enterprise].[StatusTypeCategoryClassification]
	   (StatusTypeId,
	    StatusTypeCategoryId
	   )
               SELECT StatusTypeId,
                      @StatusTypeCategory
               FROM Enterprise.StatusType
               WHERE Name IN(N'Active', N'Pending', N'Locked', N'ForceResetPassword', N'Expired', N'Disabled');
    END;

GO


IF EXISTS(SELECT * FROM Enterprise.RightValueType WHERE Value = 'Manage Accounting Product Access')
BEGIN
	UPDATE ENterprise.RightValueType 
		SET Value = 'Manage Financial Suite Product Access'
	WHERE VALUE = 'Manage Accounting Product Access'
END
GO
IF EXISTS (SELECT 1 FROM Enterprise.ProductType WHERE Name = 'Accounting')
BEGIN
	UPDATE Enterprise.ProductType 
		SET Name = 'Financial Suite', Description = 'Financial Suite (Property, Corporate, Job Cost)'
	WHERE Name = 'Accounting'
END	
GO
UPDATE P2
	SET Value = 'Document Director'
 FROM Enterprise.ProductSettingType P1
INNER JOIN [Enterprise].[ProductSetting] P2
	ON P1.ProductSettingTypeId = P2.ProductSettingTypeId
  WHERE ProductId = 20 
	AND P1.Name = 'TitleId'
GO
UPDATE P2
	SET Value = 'Accounting, Job Cost, Spend Control Management, Commercial'
 FROM Enterprise.ProductSettingType P1
INNER JOIN [Enterprise].[ProductSetting] P2
	ON P1.ProductSettingTypeId = P2.ProductSettingTypeId
  WHERE ProductId = 8 
	AND P1.Name IN ('TitleId', 'Subsolution')

GO

DECLARE @RightId INT,
		@RouteId INT,
        @BasicEndUserRoleId INT,
        @UserAdminRoleId INT,
        @UPRoleId INT,
        @UserId bigint,
        @Now datetime = GETDATE(),
		@partyId INT
		

SELECT    @UserId = UserId
            FROM    Ident.UserLogin
            WHERE    LoginName LIKE 'realpagead@%'


IF NOT EXISTS (SELECT TOP 1 1 FROM Security.[Right] where  RightName = 'AccessUnifiedReporting')
BEGIN
    --insert into right table
	Insert into [Security].[right](RightName, [Description], [Value], [StatusTypeId], [VisibilityStatusId], [ProductId], [TargetProductId], CreatedDate, CreatedBy)
	values ('AccessUnifiedReporting', 'Access to Unified Reporting', 'Access to Unified Reporting', 13, 9, 3, 3, @Now, @UserId)
    
	--get newly inserted right's ID
	select @RightId = RightId from Security.[Right] where  RightName = 'AccessUnifiedReporting'
	print(@RightId)

	--get route id
	select @RouteId = RouteId from [Security].Route where RouteValue = 'sidemenu'
	print(@RouteId)

    --insert into rightroute
    Insert into Security.[RightRoute] (RightId, RouteId, RightName, CreatedBy, CreatedDate)
	values (@RightId, @RouteId, 'Access to Unified Reporting', @UserId, @Now)

    select @UserAdminRoleId = RoleId from security.role where rolename = 'User Administrator' and OrgPartyID IS NULL
    
	select @partyId=  PartyId 	from Enterprise.[Organization]	where Name = 'RealPage Employee'

    IF NOT EXISTS (SELECT TOP 1 1 FROM Security.RoleRight WHERE RoleId = @UserAdminRoleId AND @RightId = RightId)
    BEGIN
         INSERT INTO SECURITY.[RoleRight] (RoleId,RightId,CreatedBy,CreatedDate) 
         VALUES(@UserAdminRoleId,@RightId,@UserId,@Now)
    END

	IF NOT EXISTS (SELECT TOP 1 1 FROM Security.[OrganizationOverRideRight] WHERE RightId = @RightId)
    BEGIN
         INSERT INTO SECURITY.[OrganizationOverRideRight] (RightId, OrgPartyId, VisibilityStatusId,CreatedBy,CreatedDate) 
         VALUES(@RightId, @partyId, 10,@UserId,@Now)
    END
END

GO


DECLARE @RightId INT,
		@RouteId INT,
        @BasicEndUserRoleId INT,
        @UserAdminRoleId INT,
        @UPRoleId INT,
        @UserId bigint,
        @Now datetime = GETDATE(),
		@partyId INT
		

SELECT    @UserId = UserId
            FROM    Ident.UserLogin
            WHERE    LoginName LIKE 'realpagead@%'


IF NOT EXISTS (SELECT TOP 1 1 FROM Security.[Right] where  RightName = 'EmployeeAccessUnifiedReportingAdminConsole')
BEGIN
    --insert into right table
	Insert into [Security].[right](RightName, [Description], [Value], [StatusTypeId], [VisibilityStatusId], [ProductId], [TargetProductId], CreatedDate, CreatedBy)
	values ('EmployeeAccessUnifiedReportingAdminConsole', 'Employee Access to Unified Reporting Admin Console', 'Employee Access to Unified Reporting Admin Console', 13, 10, 3, 3, @Now, @UserId)
    
	--get newly inserted right's ID
	select @RightId = RightId from Security.[Right] where  RightName = 'EmployeeAccessUnifiedReportingAdminConsole'
	print(@RightId)

	--get route id
	select @RouteId = RouteId from [Security].Route where RouteValue = 'sidemenu'
	print(@RouteId)

    --insert into rightroute
    Insert into Security.[RightRoute] (RightId, RouteId, RightName, CreatedBy, CreatedDate)
	values (@RightId, @RouteId, 'Employee Access to Unified Reporting Admin Console', @UserId, @Now)

    select @UserAdminRoleId = RoleId from security.role where rolename = 'User Administrator' and OrgPartyID IS NULL

	select @partyId=  PartyId from Enterprise.[Organization] where Name = 'RealPage Employee'
    
    IF NOT EXISTS (SELECT TOP 1 1 FROM Security.RoleRight WHERE RoleId = @UserAdminRoleId AND @RightId = RightId)
    BEGIN
         INSERT INTO SECURITY.[RoleRight] (RoleId,RightId,CreatedBy,CreatedDate) 
         VALUES(@UserAdminRoleId,@RightId,@UserId,@Now)
    END

	IF NOT EXISTS (SELECT TOP 1 1 FROM Security.[OrganizationOverRideRight] WHERE RightId = @RightId)
    BEGIN
         INSERT INTO SECURITY.[OrganizationOverRideRight] (RightId, OrgPartyId, VisibilityStatusId,CreatedBy,CreatedDate) 
         VALUES(@RightId, @partyId, 9,@UserId,@Now)
    END
END


GO