-- August 2020 changes
GO
--Renovation Manager Product
/*This script is a sample script to create new prodcut in the system.*/

DECLARE @ProductId INT, 
		@LoginURI NVARCHAR(100), 
		@SigningCertificateThumbprint NVARCHAR(50), 
		@ParentProductTypeId INT, 
		@ProductName NVARCHAR(100)= 'Renovation Manager',  -- Produact Name
		@LoginURL NVARCHAR(500), 
		@ProductUrl NVARCHAR(256), 
		@apiendpoint NVARCHAR(1000), 
		@tokenEndPoint NVARCHAR(1000), 
		@apisecret NVARCHAR(1000),
		@ServerName SYSNAME = @@SERVERNAME;

DECLARE @ProductConfiguration AS PRODUCTCONFIGURATIONTYPE;

/*Validate what product type ths new product belongs to. 'Administration' in the following block 
need to be chnanged to desired prodcut type. You can query Enterprise.ProductType table for more details.
*/

SELECT @ParentProductTypeId = ProductTypeId
FROM Enterprise.ProductType
WHERE Name = 'Property Management'
      AND ParentProductTypeId IS NULL;
IF NOT EXISTS
(
    SELECT TOP 1 1
    FROM enterprise.ProductType
    WHERE Name = 'Renovation Manager'
)
    BEGIN
        EXEC [Enterprise].[CreateProductType] 
             @ProductTypeId = 112, -- Thsi value may change based on the root prodcut type
             @ParentProductTypeId = @ParentProductTypeId, 
             @Name = @ProductName, 
             @Description = @ProductName, 
             @ProductTypeGUID = '08C7E2B6-3F6F-47A2-A409-653973299C26'; -- Use newid() to generate new uniqueidentifier.
END;
SET @ProductId = 55; -- Assign new product Id

--Following block will create the new prodcut in the database
IF NOT EXISTS
(
    SELECT 1
    FROM Enterprise.Product
    WHERE Name = @ProductName
)
    BEGIN
        EXEC Enterprise.CreateProduct 
             @ProductId = @ProductId, 
             @ProductGUID = '11EA2CEF-944B-4F3B-AD2F-E2E6E1B890F4', -- Use newid() to generate new uniqueidentifier.
             @Name = @ProductName, 
             @Description = @ProductName, 
             @ProductTypeId = 112;
        
		UPDATE Enterprise.Product
          SET 
              BooksProductCode = 'RENO'
        WHERE ProductId = @ProductId;
END;

--The following block picks up all the detail frm Enterprise.ProductSettingType table
--To set up the product, bunch of these settings are required.
SET @apiendpoint = '';
Set @tokenEndPoint = '';
SET @apisecret = '';
IF @ServerName IN ('RCDUSODBSQL001')
BEGIN
	SET @apiendpoint = 'https://rl-valueadd-dev.herokuapp.com/api/v1/unified-login';
	SET @tokenEndPoint = 'https://www-dev.realpage.com/login/identity/connect/token';
	SET @apisecret = '941C57A6-2B37-4C6E-951F-672D6E2364BF';
END
IF @ServerName IN ('rctusodbsql001')
BEGIN
	SET @apiendpoint = 'https://rl-valueadd-qa.herokuapp.com/api/v1/unified-login';
	SET @tokenEndPoint = 'https://www-qa.realpage.com/login/identity/connect/token';
	SET @apisecret = 'CA6527EE-FB3C-4D15-8A82-735CAAD3E8E4';
END
--IF @ServerName IN ('RCQUSODBSQL001')
--BEGIN
--	SET @apiendpoint = '';
--END
IF @ServerName IN ('RCPGBKDBSQL005A', 'RCPGBKDBSQL005B')
BEGIN
	SET @apiendpoint = 'https://reno.rentlytics.com/api/v1/unified-login/';
	SET @tokenEndPoint = 'https://www.realpage.com/login/identity/connect/token';
	SET @apisecret = '';
END
set nocount on
INSERT INTO @ProductConfiguration
(SettingName, 
 SettingDescription, 
 SettingValue
)
VALUES
 ('ClassName','','renovationmanager')
,('ProductUrl','','/product/renovationmanager')
,('TitleId','','Renovation Manager')
,('TitleUniqueId','','4167CF48-B169-4F4F-A510-0CEB73365334')
,('IsNewTab','','1')
,('MetatagUniqueId','','Renovation Manager')
,('IsResource','','0')
,('IsFavorite','','1')
,('LearnMore','','https://www.realpage.com/reno/')
,('ApiEndPoint','',@apiendpoint)
,('ProductStatus','Show if the external application was configured for the dashboard user.','8')
,('ProductStatus','Show if the external application was configured for the dashboard user.','7')
,('ProductStatus','Show if the external application was configured for the dashboard user.','10')
,('ProductStatus','Show if the external application was configured for the dashboard user.','19')
,('ShowInUserDetails','Should the product show in the New/Edit user pages','1')
,('ShowInRolesAndRights','Should the product show in the Role/Rights page','0')
,('ShowInAppSwitcher','Should the product show in the application switcher','1')
,('ShowInUserListFilter','Should the product show in the user list product pick list','1')
,('ProductAPIRequiresUser','Does the product require a user for api calls','0')
,('LockOnProductAccess', '', '0')
,('ProductNotAvailableForRegularUserNoEmail','Product Attribute for Product Not Available for Regular User No Email.','0')

,('CLIENTID','','ulrenoapi') -- For DEV Environment
,('TOKENENDPOINT','', @tokenEndPoint) -- For DEV Environment
,('APISECRET','', @apisecret)

,('GetRoleEndpoint','Role End point for product API','/{0}/roles?isIncludeRights={1}')
,('GetRightEndpoint','Right End point for product API','/roleRights/{0}')
,('GetPropertyEndpoint','Property End point for product API','/{0}/properties')
,('GetUserEndpoint','GET User Endpoint for product API','/users?companyId={0}&loginName={1}')
,('GetListUsersEndpoint','','/{0}/users?filter={1}&startRow={2}&resultsperpage={3}')
,('PostUserEndpoint','POST User Endpoint for product API','/users')
,('PutUserEndpoint','PUT User Endpoint for product API','/users')
,('DeleteUserEndpoint','DELETE User Endpoint for product API','/{0}/users?loginName={0}') 
,('PatchMigrateUsersEndpoint','Patch Migrate Users Endpoint', '/users/{0}/migrate')
,('PatchProfileEndpoint','PATCH Profile Endpoint for product API','/userprofile')
,('GetUserExistEndpoint','Get User Exist Endpoint for product API','/userexists?loginName={0}') -- Made New Setting
,('AuthenticationType','Used to determine how to log into the product','Redirect')



SELECT * FROM @ProductConfiguration

SET @LoginURL = '';
IF @ServerName IN ('RCDUSODBSQL001')
BEGIN
	SET @LoginURL = 'https://rl-valueadd-dev.herokuapp.com/auth-callback';
END
IF @ServerName IN ('rctusodbsql001')
BEGIN
	SET @LoginURL = 'https://rl-valueadd-qa.herokuapp.com/auth-callback';
END
--IF @ServerName IN ('RCQUSODBSQL001')
--BEGIN
--	SET @LoginURL = '';
--END
IF @ServerName IN ('RCPGBKDBSQL005A', 'RCPGBKDBSQL005B')
BEGIN
	SET @LoginURL = 'https://reno.rentlytics.com/auth-callback';
END

SET @LoginURI = @LoginURL;
SET @SigningCertificateThumbprint = NULL;

--Setup the product configurations.
if not exists (select top 1 1 from Enterprise.ProductSetting where ProductId = @ProductId)
begin

	EXEC Enterprise.ProductConfigurationSetup 
		 @ProductId, 
		 @LoginURI, 
		 @SigningCertificateThumbprint, 
		 @ProductConfiguration;
end;

IF NOT EXISTS
(
    SELECT 1
    FROM ident.SamlProductSettings
    WHERE ProductId = @ProductId
          AND LoginUri = @LoginURL
)
    BEGIN
        INSERT INTO ident.SamlProductSettings
        (
        --SamlProductSettingsId - column value is auto-generated
        ProductId, 
        LoginUri, 
        SigningCertificateThumbprint, 
        SubjectIdSamlAttribute
        )
        VALUES
        (
        -- SamlProductSettingsId - int
        @ProductId, -- ProductId - int
        @LoginURL, -- LoginUri - nvarchar
        N'NA', -- SigningCertificateThumbprint - nvarchar
        N'productUserName' -- SubjectIdSamlAttribute - nvarchar
        );
END;
GO


/*ASSIGN VALUES*/

DECLARE @OrganizationId int;
DECLARE @PartyRowNum int;
DECLARE @RightName nvarchar(200);
DECLARE @RightDescription nvarchar(200);
DECLARE @RightShortName nvarchar(200);
DECLARE @ActionName nvarchar(100);
DECLARE @ActionRouteTarget nvarchar(100);
DECLARE @ActionValueId int;
DECLARE @SourceProductId int;
DECLARE @TargetProductId int;
DECLARE @RoleCategory int;
DECLARE @RightCategory int;
DECLARE @VisibilityStatusId int;
DECLARE @ActionId int;
DECLARE @ParentActionId int;
DECLARE @DetaulRightName nvarchar(200);
DECLARE @TargetRoleName nvarchar(100);
DECLARE @RoleId int;
DECLARE @OutputRightId int;
DECLARE @UserActionId int;
DECLARE @RightValueTypeId int;
DECLARE @DependentRightValueTypeId int;

/*SET BLOCK*/
SET @TargetRoleName = 'User Administrator'; --- Role to which the new right will be assinged by default.
SET @RightName = 'Manage Renovation Manager Product Access'; -- Name of the right 
SET @RightDescription = 'Manage Renovation Manager Product Access'; --Description of the right as stated in story.
SET @RightShortName = 'ManageRenovationManager'; --Short name of the right that is being used by the application
SET @ActionName = 'Manage Renovation Manager'; -- This specifically pertains to actions used for routing purposes. 
SET @ActionRouteTarget = 'SideMenu'; -- Where you want this right to show up. other variation is DashBoard.
SET @ActionValueID = 1;
SET @DetaulRightName = 'Default_' + @RightShortName; -- This is used internally for creating right dependency in RightDependency table.

/*CLEANUP  AND LOAD TEMPORARY TABLE FOR ORG LIST*/

IF OBJECT_ID('tempdb..#HoldParty') IS NOT NULL
BEGIN
	DROP TABLE #HoldParty;
END;

SELECT DISTINCT 
	   IDENTITY(int, 1, 1) AS RowNumber, o.PartyId AS OrganizationPartyID, 0 AS PStatus
INTO #HoldParty
FROM Enterprise.Organization AS o
	 INNER JOIN
	 Enterprise.Party AS p
	 ON P.partyid = O.PartyId
WHERE O.Name <> 'RealPage Employee'; 
--1. If rigths need in all organization then no condition 
--2. If needed in all except RP Employee company then O.Name <> 'RealPage Employee'
--3. If needed in just RP Employee and not in any other company, then  O.Name = 'RealPage Employee'

/*SELECT REQUIRED ATTRIBUTES FOR ROLE, RIGHT, AND ACTIONS*/
SELECT @SourceProductId = ProductId
FROM Enterprise.Product
WHERE name = 'Unified Platform';

SELECT @TargetProductId = ProductId
FROM Enterprise.Product
WHERE Name = 'Renovation Manager';

SELECT @RoleCategory = TypeId
FROM Enterprise.RoleRightStatus AS rrs
WHERE CategoryName = 'Role Type' AND 
	  TypeName = 'System';

SELECT @RightCategory = TypeId
FROM Enterprise.RoleRightStatus AS rrs
WHERE CategoryName = 'Right Type' AND 
	  TypeName = 'System';

SELECT @VisibilityStatusId = TypeId
FROM Enterprise.RoleRightStatus AS rrs
WHERE TypeName = 'ALL' AND 
	  CategoryType = 'Security';

IF NOT EXISTS
(
	SELECT 1
	FROM Enterprise.ACTION
	WHERE ObjectValue = @ActionName AND 
		  ParentActionId IS NULL
)
BEGIN
	EXEC Enterprise.CreateAction 
     @ProductID = @SourceProductId, 
     @Action = @ActionName, 
     @ActionTarget = N'Right', 
     @ActionbValueTypeId = 1, 
     @Description = '', 
     @ActionID = @ActionID OUTPUT;
SELECT @ActionID AS N'@ActionID';
END;

SELECT @ParentActionId = ActionId
FROM Enterprise.ACTION
WHERE ObjectValue = @ActionRouteTarget AND 
	  ObjectType = 'Route' AND 
	  Description = 'SuperUser';

IF NOT EXISTS
(
	SELECT 1
	FROM Enterprise.ACTION
	WHERE ObjectValue = @ActionName AND 
		  ParentActionID = @ParentActionId
)
BEGIN
EXEC [Enterprise].[CreateAction] 
     @ProductID = @SourceProductId, 
     @Action = @ActionName, 
     @ActionTarget = N'Right', 
     @ActionbValueTypeId = 1, 
     @Description = '', 
     @ParentActionID = @ParentActionId, 
     @ActionID = @ActionID OUTPUT;
SELECT @ActionID AS N'@ActionID';
END;

SELECT @ActionID = ActionID
FROM Enterprise.ACTION
WHERE ObjectValue = @ActionName AND 
	  ObjectType = 'Right' AND 
	  ParentActionId IS NULL;

WHILE EXISTS
(
	SELECT 1
	FROM #HoldParty
	WHERE PStatus = 0
)
BEGIN
	SELECT TOP 1 @PartyRowNum = Rownumber, @OrganizationId = OrganizationPartyID
	FROM #HoldParty
	WHERE PStatus = 0;
	SELECT @RoleId = RoleId
	FROM Enterprise.Role AS R
		 INNER JOIN
		 Enterprise.RoleValueType AS RR
		 ON RR.RoleValueTypeId = R.RoleValueTypeId
	WHERE RR.Value = @TargetRoleName AND 
		  R.PartyId = @OrganizationId;
	EXECUTE Enterprise.CreateRight @RoleId = -1, @RightName = @DetaulRightName, @ShortName = @RightShortName, @RightCategoryId = @RightCategory, @PartyId = @OrganizationId, @ProductId = @SourceProductId, @Description = '', @TargetProductId = @TargetProductId, @VisibilityStatusId = @VisibilityStatusId, @RightId = @OutputRightId OUTPUT;
	EXEC [Enterprise].[LinkActionToRights] @ActionID = @ActionID, @RightId = @OutputRightId, @StatusId = @VisibilityStatusId, @UserActionId = @UserActionId OUTPUT;
	EXECUTE Enterprise.CreateRight @RoleId = @RoleId, @RightName = @RightName, @RightCategoryId = @RightCategory, @PartyId = @OrganizationId, @ProductId = @SourceProductId, @Shortname = @RightShortName, @Description = @RightDescription, @TargetProductId = @TargetProductId, @VisibilityStatusId = @VisibilityStatusId, @RightId = @OutputRightId OUTPUT;
	EXEC [Enterprise].[LinkActionToRights] @ActionID = @ActionID, @RightId = @OutputRightId, @StatusId = @VisibilityStatusId, @UserActionId = @UserActionId OUTPUT;
	UPDATE #HoldParty
	  SET PStatus = 1
	WHERE RowNumber = @PartyRowNum;
END;

/*Setup Dependencies for custom roles*/

SELECT @DependentRightValueTypeId = RightValueTypeId
FROM Enterprise.RightValueType
WHERE value = @DetaulRightName;

SELECT @RightValueTypeId = RightValueTypeId
FROM Enterprise.RightValueType
WHERE value = @RightName;

IF NOT EXISTS
(
	SELECT 1
	FROM Enterprise.RightDependency
	WHERE RightValueTypeId = @RightValueTypeId AND 
		  DependentRightValueTypeId = @DependentRightValueTypeId
)
BEGIN
	INSERT INTO Enterprise.RightDependency( RightValueTypeId, DependentRightValueTypeId )
	VALUES( @RightValueTypeId, @DependentRightValueTypeId );
END;

GO

---- 99688 [GB-6736] Unified Login integration with Senior Lead Management
---- 207273 Add text for the Senior Lead Management tile text and "Learn More" URL

/*This script is a sample script to create new prodcut in the system.*/

DECLARE @ProductId INT, 
		@LoginURI NVARCHAR(100), 
		@SigningCertificateThumbprint NVARCHAR(50), 
		@ParentProductTypeId INT, 
		@ProductName NVARCHAR(100)= 'Senior Lead Management',  -- Produact Name
		@LoginURL NVARCHAR(500), 
		@ProductUrl NVARCHAR(256), 
		@ServerName SYSNAME = @@SERVERNAME;

DECLARE @ProductConfiguration AS PRODUCTCONFIGURATIONTYPE;

/*Validate what product type ths new product belongs to. 'Administration' in the following block 
need to be chnanged to desired prodcut type. You can query Enterprise.ProductType table for more details.
*/

SELECT @ParentProductTypeId = ProductTypeId
FROM Enterprise.ProductType
WHERE Name = 'Lease Management'
      AND ParentProductTypeId IS NULL;
IF NOT EXISTS
(
    SELECT TOP 1 1
    FROM enterprise.ProductType
    WHERE Name = 'Senior Lead Management'
)
    BEGIN
        EXEC [Enterprise].[CreateProductType] 
             @ProductTypeId = 311, -- Thsi value may change based on the root prodcut type
             @ParentProductTypeId = @ParentProductTypeId, 
             @Name = @ProductName, 
             @Description = @ProductName, 
             @ProductTypeGUID = '39BECAD7-19C7-4B07-86A6-EDD424BE12BF'; -- Use newid() to generate new uniqueidentifier.
END;
SET @ProductId = 50; -- Assign new product Id

--Following block will create the new prodcut in the database
IF NOT EXISTS
(
    SELECT 1
    FROM Enterprise.Product
    WHERE Name = @ProductName
)
    BEGIN
        EXEC Enterprise.CreateProduct 
             @ProductId = @ProductId, 
             @ProductGUID = '97EED28D-93DC-4435-9A26-07B33A162839', -- Use newid() to generate new uniqueidentifier.
             @Name = @ProductName, 
             @Description = 'Senior Lead Manager is a lead and referrer management platform designed to help sales agents and leasing staff to manage leads from a variety of sources. With Senior Lead Manager, users can do lead capture, lead and prospect nurturing, follow-up tasks, reminders, reports and referrer management in one convenient location. This is an internal tool designed for the on-site staff.', 
             @ProductTypeId = 311;
        UPDATE Enterprise.Product
          SET 
              BooksProductCode = 'SLM'
        WHERE ProductId = @ProductId;
END;

--The following block picks up all the detail frm Enterprise.ProductSettingType table
--To set up the product, bunch of these settings are required.
set nocount on
INSERT INTO @ProductConfiguration
(SettingName, 
 SettingDescription, 
 SettingValue
)
VALUES
('ClientId','','1')
,('ClassName','','seniorleadmanagement')
,('ProductUrl','','/product/seniorleadmanagement')
,('TitleId','','Senior Lead Management')
,('TitleUniqueId','','EDFB27F1-6335-4297-B44F-F265204B0538')
,('IsNewTab','','1')
,('MetatagUniqueId','','Senior Lead Management')
,('IsResource','','0')
,('IsFavorite','','1')
,('LearnMore','','https://www.realpage.com/senior/')
,('ApiEndPoint','','https://dev-spmadminbff.realpage.com/api/v1')
,('ProductStatus','Show if the external application was configured for the dashboard user.','8')
,('ShowInUserDetails','Should the product show in the New/Edit user pages','1')
,('ShowInRolesAndRights','Should the product show in the Role/Rights page','0')
,('ShowInAppSwitcher','Should the product show in the application switcher','1')
,('ShowInUserListFilter','Should the product show in the user list product pick list','1')
,('ProductAPIRequiresUser','Does the product require a user for api calls','0')
,('LockOnProductAccess', '', '0')
,('ProductNotAvailableForRegularUserNoEmail','Product Attribute for Product Not Available for Regular User No Email.','0')

--,('ApiUserName','','test+rfqarpunity@novelpay.com')
--,('ApiPassword','','7mgp43EIvc8c!@')
,('ApiKey','','53448358-FC1C-4B30-8C45-1171B06D84D1') -- For DEV Environment

,('GetRoleEndpoint','Role End point for product API','/{0}/Roles?isIncludeRights={1}')
,('GetRightEndpoint','Right End point for product API','/roleRights/{0}')

,('GetPropertyEndpoint','Property End point for product API','/properties/{0}')
,('GetUserEndpoint','GET User Endpoint for product API','/users?loginName={0}')
,('GetListUsersEndpoint','','/users/{0}?filter={1}&pageNumber={2}&PageSize={3}')
,('PostUserEndpoint','POST User Endpoint for product API','/users')
,('PutUserEndpoint','PUT User Endpoint for product API','/users')
,('DeleteUserEndpoint','DELETE User Endpoint for product API','/users?loginName={0}') 
,('PatchMigrateUsersEndpoint','Patch Migrate Users Endpoint', '/users/{0}/migrate')
,('PatchProfileEndpoint','PATCH Profile Endpoint for product API','/userprofile')
,('GetUserExistEndpoint','Get User Exist Endpoint for product API','/userexists?loginName={0}') -- Made New Setting
,('AuthenticationType','Used to determine how to log into the product','Redirect')
--- Not sure about below ones -------
--,('GetCompanyEndpoint','GET Company Endpoint for API','/orgs/{0}')
--,('GetParentCompanyEndpoint','GET Company Endpoint for API','/orgs?parentOrgId={0}')
--,('GetProfileEndpoint','GET User Profile Endpoint for product API','/users/{0}/profile')


SELECT * FROM @ProductConfiguration

SET @LoginURL = 'http://dev-spm.realpage.com/oidcHanlder';

SET @LoginURI = @LoginURL;
SET @SigningCertificateThumbprint = NULL;

--Setup the product configurations.
if not exists (select top 1 1 from Enterprise.ProductSetting where ProductId = @ProductId)
begin

	EXEC Enterprise.ProductConfigurationSetup 
		 @ProductId, 
		 @LoginURI, 
		 @SigningCertificateThumbprint, 
		 @ProductConfiguration;
end;

IF NOT EXISTS
(
    SELECT 1
    FROM ident.SamlProductSettings
    WHERE ProductId = @ProductId
          AND LoginUri = @LoginURL
)
    BEGIN
        INSERT INTO ident.SamlProductSettings
        (
        --SamlProductSettingsId - column value is auto-generated
        ProductId, 
        LoginUri, 
        SigningCertificateThumbprint, 
        SubjectIdSamlAttribute
        )
        VALUES
        (
        -- SamlProductSettingsId - int
        @ProductId, -- ProductId - int
        @LoginURL, -- LoginUri - nvarchar
        N'NA', -- SigningCertificateThumbprint - nvarchar
        N'productUserName' -- SubjectIdSamlAttribute - nvarchar
        );
END;
GO


/*ASSIGN VALUES*/

DECLARE @OrganizationId int;
DECLARE @PartyRowNum int;
DECLARE @RightName nvarchar(200);
DECLARE @RightDescription nvarchar(200);
DECLARE @RightShortName nvarchar(200);
DECLARE @ActionName nvarchar(100);
DECLARE @ActionRouteTarget nvarchar(100);
DECLARE @ActionValueId int;
DECLARE @SourceProductId int;
DECLARE @TargetProductId int;
DECLARE @RoleCategory int;
DECLARE @RightCategory int;
DECLARE @VisibilityStatusId int;
DECLARE @ActionId int;
DECLARE @ParentActionId int;
DECLARE @DetaulRightName nvarchar(200);
DECLARE @TargetRoleName nvarchar(100);
DECLARE @RoleId int;
DECLARE @OutputRightId int;
DECLARE @UserActionId int;
DECLARE @RightValueTypeId int;
DECLARE @DependentRightValueTypeId int;

/*SET BLOCK*/
SET @TargetRoleName = 'User Administrator'; --- Role to which the new right will be assinged by default.
SET @RightName = 'Manage Senior Lead Management Product Access'; -- Name of the right 
SET @RightDescription = 'Manage Senior Lead Management Product Access'; --Description of the right as stated in story.
SET @RightShortName = 'ManageSeniorLeadManagement'; --Short name of the right that is being used by the application
SET @ActionName = 'Manage Senior Lead Management'; -- This specifically pertains to actions used for routing purposes. 
SET @ActionRouteTarget = 'SideMenu'; -- Where you want this right to show up. other variation is DashBoard.
SET @ActionValueID = 1;
SET @DetaulRightName = 'Default_' + @RightShortName; -- This is used internally for creating right dependency in RightDependency table.

/*CLEANUP  AND LOAD TEMPORARY TABLE FOR ORG LIST*/

IF OBJECT_ID('tempdb..#HoldParty') IS NOT NULL
BEGIN
	DROP TABLE #HoldParty;
END;

SELECT DISTINCT 
	   IDENTITY(int, 1, 1) AS RowNumber, o.PartyId AS OrganizationPartyID, 0 AS PStatus
INTO #HoldParty
FROM Enterprise.Organization AS o
	 INNER JOIN
	 Enterprise.Party AS p
	 ON P.partyid = O.PartyId
WHERE O.Name <> 'RealPage Employee'; 
--1. If rigths need in all organization then no condition 
--2. If needed in all except RP Employee company then O.Name <> 'RealPage Employee'
--3. If needed in just RP Employee and not in any other company, then  O.Name = 'RealPage Employee'

/*SELECT REQUIRED ATTRIBUTES FOR ROLE, RIGHT, AND ACTIONS*/
SELECT @SourceProductId = ProductId
FROM Enterprise.Product
WHERE name = 'Unified Platform';

SELECT @TargetProductId = ProductId
FROM Enterprise.Product
WHERE Name = 'Senior Lead Management';

SELECT @RoleCategory = TypeId
FROM Enterprise.RoleRightStatus AS rrs
WHERE CategoryName = 'Role Type' AND 
	  TypeName = 'System';

SELECT @RightCategory = TypeId
FROM Enterprise.RoleRightStatus AS rrs
WHERE CategoryName = 'Right Type' AND 
	  TypeName = 'System';

SELECT @VisibilityStatusId = TypeId
FROM Enterprise.RoleRightStatus AS rrs
WHERE TypeName = 'ALL' AND 
	  CategoryType = 'Security';

IF NOT EXISTS
(
	SELECT 1
	FROM Enterprise.ACTION
	WHERE ObjectValue = @ActionName AND 
		  ParentActionId IS NULL
)
BEGIN
	EXEC Enterprise.CreateAction 
     @ProductID = @SourceProductId, 
     @Action = @ActionName, 
     @ActionTarget = N'Right', 
     @ActionbValueTypeId = 1, 
     @Description = '', 
     @ActionID = @ActionID OUTPUT;
SELECT @ActionID AS N'@ActionID';
END;

SELECT @ParentActionId = ActionId
FROM Enterprise.ACTION
WHERE ObjectValue = @ActionRouteTarget AND 
	  ObjectType = 'Route' AND 
	  Description = 'SuperUser';

IF NOT EXISTS
(
	SELECT 1
	FROM Enterprise.ACTION
	WHERE ObjectValue = @ActionName AND 
		  ParentActionID = @ParentActionId
)
BEGIN
EXEC [Enterprise].[CreateAction] 
     @ProductID = @SourceProductId, 
     @Action = @ActionName, 
     @ActionTarget = N'Right', 
     @ActionbValueTypeId = 1, 
     @Description = '', 
     @ParentActionID = @ParentActionId, 
     @ActionID = @ActionID OUTPUT;
SELECT @ActionID AS N'@ActionID';
END;

SELECT @ActionID = ActionID
FROM Enterprise.ACTION
WHERE ObjectValue = @ActionName AND 
	  ObjectType = 'Right' AND 
	  ParentActionId IS NULL;

WHILE EXISTS
(
	SELECT 1
	FROM #HoldParty
	WHERE PStatus = 0
)
BEGIN
	SELECT TOP 1 @PartyRowNum = Rownumber, @OrganizationId = OrganizationPartyID
	FROM #HoldParty
	WHERE PStatus = 0;
	SELECT @RoleId = RoleId
	FROM Enterprise.Role AS R
		 INNER JOIN
		 Enterprise.RoleValueType AS RR
		 ON RR.RoleValueTypeId = R.RoleValueTypeId
	WHERE RR.Value = @TargetRoleName AND 
		  R.PartyId = @OrganizationId;
	EXECUTE Enterprise.CreateRight @RoleId = -1, @RightName = @DetaulRightName, @ShortName = @RightShortName, @RightCategoryId = @RightCategory, @PartyId = @OrganizationId, @ProductId = @SourceProductId, @Description = '', @TargetProductId = @TargetProductId, @VisibilityStatusId = @VisibilityStatusId, @RightId = @OutputRightId OUTPUT;
	EXEC [Enterprise].[LinkActionToRights] @ActionID = @ActionID, @RightId = @OutputRightId, @StatusId = @VisibilityStatusId, @UserActionId = @UserActionId OUTPUT;
	EXECUTE Enterprise.CreateRight @RoleId = @RoleId, @RightName = @RightName, @RightCategoryId = @RightCategory, @PartyId = @OrganizationId, @ProductId = @SourceProductId, @Shortname = @RightShortName, @Description = @RightDescription, @TargetProductId = @TargetProductId, @VisibilityStatusId = @VisibilityStatusId, @RightId = @OutputRightId OUTPUT;
	EXEC [Enterprise].[LinkActionToRights] @ActionID = @ActionID, @RightId = @OutputRightId, @StatusId = @VisibilityStatusId, @UserActionId = @UserActionId OUTPUT;
	UPDATE #HoldParty
	  SET PStatus = 1
	WHERE RowNumber = @PartyRowNum;
END;

/*Setup Dependencies for custom roles*/

SELECT @DependentRightValueTypeId = RightValueTypeId
FROM Enterprise.RightValueType
WHERE value = @DetaulRightName;

SELECT @RightValueTypeId = RightValueTypeId
FROM Enterprise.RightValueType
WHERE value = @RightName;

IF NOT EXISTS
(
	SELECT 1
	FROM Enterprise.RightDependency
	WHERE RightValueTypeId = @RightValueTypeId AND 
		  DependentRightValueTypeId = @DependentRightValueTypeId
)
BEGIN
	INSERT INTO Enterprise.RightDependency( RightValueTypeId, DependentRightValueTypeId )
	VALUES( @RightValueTypeId, @DependentRightValueTypeId );
END;

GO
--For Reno product internal settings
DECLARE @UserId bigint,
	@ProductId int =55,
	@Now datetime = GETDATE(),
	@CurrentProductConfigurationID INT,
	@ProductSettingTypeId INT,
	@ProductSettingId INT,
	@roleId INT,
	@ServerName SYSNAME = @@SERVERNAME;
		
		SELECT TOP 1 @CurrentProductConfigurationID = ConfigurationId
		FROM Enterprise.GlobalProductConfiguration AS gpc
		WHERE gpc.ProductId = @ProductId AND 
				( ( @NOW BETWEEN gpc.FromDate AND gpc.ThruDate
				) OR 
				( @NOW >= gpc.FromDate AND 
					gpc.ThruDate IS NULL
				)
				)
		ORDER BY GlobalProductConfigurationId DESC;

		IF @ServerName IN ('RCTUSODBSQL001', 'RCDUSODBSQL001')
		BEGIN
			Select @roleId = 34
		END
		IF @ServerName IN ('RCQUSODBSQL001')
		BEGIN
			Select @roleId = 34
		END
		IF @ServerName IN ('RCPGBKDBSQL005A', 'RCPGBKDBSQL005B')
		BEGIN
			Select @roleId = 34
		END

		IF
		(
			SELECT 1
			FROM Enterprise.ProductSettingType
			WHERE Name = 'SuperUserRoleId'
		) IS NULL
		BEGIN
			EXEC Enterprise.CreateProductSettingType 'SuperUserRoleId', 'The role Id to create admin user in  product', @ProductSettingTypeId OUTPUT;
		END;

		IF @ProductSettingTypeId IS NOT NULL AND 
			   NOT EXISTS
			(
				SELECT TOP 1 1
				FROM Enterprise.ProductSetting
				WHERE ProductID = @productId AND 
					  ProductSettingTypeId = @ProductSettingTypeId AND 
					  ThruDate IS NULL
			)
			BEGIN
	
				-- Create the Value and assign it to the Product and ProductSettingType
				EXEC Enterprise.CreateProductSetting @ProductId = @ProductId, -- int
				@ProductSettingTypeId = @ProductSettingTypeId, -- int
				@Value = @roleId, 
				@FromDate = @NOW, -- datetime
				@ThruDate = NULL, -- datetime
				@ProductSettingId = @ProductSettingId OUTPUT; -- int

				-- Link the Product Setting to an actual configuration
				EXEC Enterprise.LinkProductSettingToConfiguration @ConfigurationId = @CurrentProductConfigurationID, -- int
				@ProductSettingId = @ProductSettingId, -- int
				@FromDate = @NOW, -- datetime
				@ThruDate = NULL;   -- datetime
			END;


		IF
		(
			SELECT 1
			FROM Enterprise.ProductSettingType
			WHERE Name = 'ClientScope'
		) IS NULL
		BEGIN
			EXEC Enterprise.CreateProductSettingType 'ClientScope', 'The client scope to get access token', @ProductSettingTypeId OUTPUT;
		END;

		IF @ProductSettingTypeId IS NOT NULL AND 
			   NOT EXISTS
			(
				SELECT TOP 1 1
				FROM Enterprise.ProductSetting
				WHERE ProductID = @productId AND 
					  ProductSettingTypeId = @ProductSettingTypeId AND 
					  ThruDate IS NULL
			)
			BEGIN
	
				-- Create the Value and assign it to the Product and ProductSettingType
				EXEC Enterprise.CreateProductSetting @ProductId = @ProductId, -- int
				@ProductSettingTypeId = @ProductSettingTypeId, -- int
				@Value = 'renouserapi', 
				@FromDate = @NOW, -- datetime
				@ThruDate = NULL, -- datetime
				@ProductSettingId = @ProductSettingId OUTPUT; -- int

				-- Link the Product Setting to an actual configuration
				EXEC Enterprise.LinkProductSettingToConfiguration @ConfigurationId = @CurrentProductConfigurationID, -- int
				@ProductSettingId = @ProductSettingId, -- int
				@FromDate = @NOW, -- datetime
				@ThruDate = NULL;   -- datetime
			END;
GO

DECLARE @UserId bigint,
	@ProductId int =17,
	@Now datetime = GETDATE();

SELECT	@UserId = UserId
FROM	Ident.UserLogin
WHERE	LoginName LIKE 'realpagead@%'

--SLM
Select @ProductId = 50
IF NOT EXISTS (SELECT TOP 1 1 FROM[UserManagement].[ProductPage] WHERE ProductId = @ProductId)
BEGIN	
	SET IDENTITY_INSERT [UserManagement].[Control] ON 
	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate]) 
	VALUES (537, NULL, 8, N'SeniorLeadManagementProductAccessTabGroupUIId', NULL, NULL, 1, @UserId, @Now)

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate]) 
	VALUES (538, 537, 9, N'SeniorLeadManagementProductAccessPropertiesTabUIId', N'Properties', NULL, 1, @UserId, @Now)

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate]) 
	VALUES (539, 538, 3, N'SeniorLeadManagementProductAccessPropertiesMultiSelectGridUIId', NULL, NULL, 2, @UserId, @Now)

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate]) 
	VALUES (540, 539, 10, N'SeniorLeadManagementProductAccessCheckboxUIId', NULL, N'isAssigned', 1, @UserId, @Now)

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate]) 
	VALUES (541, 539, 5, N'SeniorLeadManagementProductAccessPropertyLabelUIId', N'Property', N'name', 2, @UserId, @Now)

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate]) 
	VALUES (542, 539, 5, N'SeniorLeadManagementProductAccessStateLabelUIId', N'State', N'state', 3, @UserId, @Now)

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate]) 
	VALUES (543, 537, 9, N'SeniorLeadManagementProductAccessRightsTabUIId', N'Rights', NULL, 2, @UserId, @Now)

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate]) 
	VALUES (545, 543, 13, N'SeniorLeadManagementProductAccessSelectaPresetRoleRightsSelectUIId', N'Select a Preset Role', N'roles', 1, @UserId, @Now)

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate]) 
	VALUES (546, 543, 3, N'SeniorLeadManagementProductAccessRightsMultiSelectGridUIId', NULL, NULL, 1, @UserId, @Now)

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate]) 
	VALUES (547, 546, 10, N'SeniorLeadManagementProductAccessCheckboxUIId', NULL, N'isAssigned', 1, @UserId, @Now)

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate]) 
	VALUES (548, 546, 5, N'SeniorLeadManagementProductAccessRightLabelUIId', N'Right', N'name', 2, @UserId, @Now)	

	SET IDENTITY_INSERT [UserManagement].[Control] OFF

	SET IDENTITY_INSERT [UserManagement].[ControlAttribute] ON 
	INSERT [UserManagement].[ControlAttribute] ([ControlAttributeId], [ControlId], [Key], [Value], [CreatedBy], [CreatedDate]) 
	VALUES (135, 538, N'Default', N'True', @UserId, @Now)

	INSERT [UserManagement].[ControlAttribute] ([ControlAttributeId], [ControlId], [Key], [Value], [CreatedBy], [CreatedDate]) 
	VALUES (136, 539, N'ShowSelectAll', N'True', @UserId, @Now)

	INSERT [UserManagement].[ControlAttribute] ([ControlAttributeId], [ControlId], [Key], [Value], [CreatedBy], [CreatedDate]) 
	VALUES (137, 546, N'ShowSelectAll', N'True', @UserId, @Now)	

	SET IDENTITY_INSERT [UserManagement].[ControlAttribute] OFF


	IF EXISTS (SELECT TOP 1 1 FROM[UserManagement].[ProductPage] WHERE ProductPageId = 33)
	BEGIN
		SET IDENTITY_INSERT [UserManagement].[ProductPage] ON 
		INSERT [UserManagement].[ProductPage] ([ProductPageId], [ProductId], [DisplayName], [CreatedBy], [CreatedDate], [IsActive]) 
		VALUES (33, 50, N'Senior Lead Management Product Access', @UserId, @Now, 1)
		SET IDENTITY_INSERT [UserManagement].[ProductPage] OFF
	END
	

	SET IDENTITY_INSERT [UserManagement].[ProductPageControl] ON 
	INSERT [UserManagement].[ProductPageControl] ([ProductPageControlId], [ProductPageId], [ControlId], [CreatedBy], [CreatedDate]) 
	VALUES (43, 33, 537, @UserId, @Now)
	SET IDENTITY_INSERT [UserManagement].[ProductPageControl] OFF

	SET IDENTITY_INSERT [UserManagement].[ProductPage] ON 
	INSERT [UserManagement].[ProductPage] ([ProductPageId], [ProductId], [DisplayName], [CreatedBy], [CreatedDate], [IsActive]) 
	VALUES (33, 50, N'Senior Lead Management Product Access', @UserId, @Now, 1)
	SET IDENTITY_INSERT [UserManagement].[ProductPage] OFF
	
END

SELECT @ProductId = 55
IF NOT EXISTS (SELECT TOP 1 1 FROM[UserManagement].[ProductPage] WHERE ProductId = @ProductId)
BEGIN
            SET IDENTITY_INSERT [UserManagement].[Control] ON 
			--Parent
			INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
			VALUES (452, NULL, 8, N'RenovationManagerProductAccessTabGroupUIId', NULL, NULL, 1, @UserId, @Now)

			--Roles
			INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
			VALUES (453, 452, 9, N'RenovationManagerProductAccessRolesTabUIId', N'Roles', NULL, 1, @UserId, @Now)

			INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
			VALUES (454, 453, 3, N'RenovationManagerProductAccessRolesSelectGridUIId', NULL, NULL, 1, @UserId, @Now)

			INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
			VALUES (455, 454, 10, N'RenovationManagerProductAccessRadioUIId', NULL, N'isAssigned', 1, @UserId, @Now)

			INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
			VALUES (456, 454, 5, N'RenovationManagerProductAccessRoleLabelUIId', N'Role', N'name', 2, @UserId, @Now)

							
			--Properties
			INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate]) 
			VALUES (457, 452, 9, N'RenovationManagerProductAccessPropertiesTabUIId', N'Properties', NULL, 3, @UserId, @Now)
				
			INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
			VALUES (458, 457, 3, N'RenovationManagerProductAccessPropertiesMultiSelectGridUIId', NULL, NULL, 1, @UserId, @Now)
				
			INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
			VALUES (459, 458, 10, N'RenovationManagerProductAccessCheckboxUIId', NULL, N'isAssigned', 1, @UserId, @Now)
				
			INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
			VALUES (460, 458, 5, N'RenovationManagerProductAccessPropertyLabelUIId', N'Property', N'name', 2, @UserId, @Now)
				
			INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate]) 
			VALUES (461, 458, 5, N'RenovationManagerProductAccessStateLabelUIId', N'State', N'state', 3, @UserId, @Now)	
				
			SET IDENTITY_INSERT [UserManagement].[Control] OFF
              
            SET IDENTITY_INSERT [UserManagement].[ControlAttribute] ON 
            INSERT [UserManagement].[ControlAttribute] ([ControlAttributeId], [ControlId], [Key], [Value], [CreatedBy], [CreatedDate])
			VALUES (104, 453, N'Default', N'True', @UserId, @Now)

			INSERT [UserManagement].[ControlAttribute] ([ControlAttributeId], [ControlId], [Key], [Value], [CreatedBy], [CreatedDate]) 
			VALUES (105, 454, N'ShowSelectAll', N'False', @UserId, @Now)
    
			INSERT [UserManagement].[ControlAttribute] ([ControlAttributeId], [ControlId], [Key], [Value], [CreatedBy], [CreatedDate]) 
			VALUES (106, 458, N'ShowSelectAll', N'True', @UserId, @Now)

            SET IDENTITY_INSERT [UserManagement].[ControlAttribute] OFF

            SET IDENTITY_INSERT [UserManagement].[ProductPage] ON 
              
			INSERT [UserManagement].[ProductPage] ([ProductPageId], [ProductId], [DisplayName], [CreatedBy], [CreatedDate], [IsActive])
			VALUES (28, 55, N'Renovation Manager Product Access', @UserId, @Now, 1)
  
            SET IDENTITY_INSERT [UserManagement].[ProductPage] OFF
              
            SET IDENTITY_INSERT [UserManagement].[ProductPageControl] ON 
            
			INSERT [UserManagement].[ProductPageControl] ([ProductPageControlId], [ProductPageId], [ControlId], [CreatedBy], [CreatedDate]) 
			VALUES (37, 28, 452, @UserId, @Now)
          
            SET IDENTITY_INSERT [UserManagement].[ProductPageControl] OFF

END
GO