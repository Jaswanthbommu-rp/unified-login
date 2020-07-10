-- August 2020 changes
GO

update enterprise.ProductSettingType set sensitivedata = 1 where name in (
'ApiSecret'
,'MTClientSecret'
,'UnifiedLoginResearchApplicationClientSecret'
,'TokenClientSecret'
,'TiboWebHookSigningSecret'
,'ApiPassword'
,'IntactPassword'

)
GO

if not exists ( select top 1 1 from Enterprise.ProductSettingType where name = 'BooksUseDomains' )
begin
	INSERT INTO Enterprise.ProductSettingType ( name, Description, SensitiveData ) values ( 'BooksUseDomains', 'Use domains for books api calls', 0 )
end
GO

if not exists ( select top 1 1 from Enterprise.ProductSettingType where name = 'BooksUseUPFMId' )
begin
	INSERT INTO Enterprise.ProductSettingType ( name, Description, SensitiveData ) values ( 'BooksUseUPFMId', 'Use UPFM instance for books api calls', 0 )
end
go

if not exists(Select top 1 1 from Enterprise.ProductSetting ps 
				inner join Enterprise.ProductSettingType pst
				on ps.ProductSettingTypeId = pst.ProductSettingTypeId
				where pst.Name = 'BooksUseDomains' and ps.ProductId= 3)
Begin
	Insert into Enterprise.ProductSetting (ProductId, ProductSettingTypeId, Value, FromDate)
	Select 3, ProductSettingTypeId, '1', GETUTCDATE()
	from Enterprise.ProductSettingType
	where Name = 'BooksUseDomains'

	declare @productsettingid int
	select @productsettingid = productsettingid from Enterprise.ProductSetting ps 
				inner join Enterprise.ProductSettingType pst
				on ps.ProductSettingTypeId = pst.ProductSettingTypeId
				where pst.Name = 'BooksUseDomains' and ps.ProductId= 3

	insert into enterprise.ProductConfiguration ( ConfigurationId, ProductSettingId, FromDate )
		select top 1 ConfigurationId, @productsettingid, GETUTCDATE() from enterprise.GlobalProductConfiguration where productid = 3 and thrudate is null
end
GO

if not exists(Select top 1 1 from Enterprise.ProductSetting ps 
				inner join Enterprise.ProductSettingType pst
				on ps.ProductSettingTypeId = pst.ProductSettingTypeId
				where pst.Name = 'BooksUseUPFMId' and ps.ProductId= 3)
Begin
	Insert into Enterprise.ProductSetting (ProductId, ProductSettingTypeId, Value, FromDate)
	Select 3, ProductSettingTypeId, '1', GETUTCDATE()
	from Enterprise.ProductSettingType
	where Name = 'BooksUseUPFMId'

	declare @productsettingid int
	select @productsettingid = productsettingid from Enterprise.ProductSetting ps 
				inner join Enterprise.ProductSettingType pst
				on ps.ProductSettingTypeId = pst.ProductSettingTypeId
				where pst.Name = 'BooksUseUPFMId' and ps.ProductId= 3

	insert into enterprise.ProductConfiguration ( ConfigurationId, ProductSettingId, FromDate )
		select top 1 ConfigurationId, @productsettingid, GETUTCDATE() from enterprise.GlobalProductConfiguration where productid = 3 and thrudate is null
end
GO

-- sync up the employee and external company guids
update enterprise.party set realpageid = '0D018E46-C20E-477D-ADED-4E5A35FB8F99' where partyid = (select top 1 partyid from enterprise.DataImportMapping where sourceid = '-1')
update enterprise.party set realpageid = 'EEFACE50-9F75-4DCE-B133-A97EE0E0D723' where partyid = (select top 1 partyid from enterprise.DataImportMapping where sourceid = '-2')

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
 SettingValue,
 SettingSensitiveData
)
VALUES
 ('ClassName','','renovationmanager', 0)
,('ProductUrl','','/product/renovationmanager', 0)
,('TitleId','','Renovation Manager', 0)
,('TitleUniqueId','','4167CF48-B169-4F4F-A510-0CEB73365334', 0)
,('IsNewTab','','1', 0)
,('MetatagUniqueId','','Renovation Manager', 0)
,('IsResource','','0', 0)
,('IsFavorite','','1', 0)
,('LearnMore','','https://www.realpage.com/reno/', 0)
,('ApiEndPoint','',@apiendpoint, 0)
,('ProductStatus','Show if the external application was configured for the dashboard user.','8', 0)
,('ProductStatus','Show if the external application was configured for the dashboard user.','7', 0)
,('ProductStatus','Show if the external application was configured for the dashboard user.','10', 0)
,('ProductStatus','Show if the external application was configured for the dashboard user.','19', 0)
,('ShowInUserDetails','Should the product show in the New/Edit user pages','1', 0)
,('ShowInRolesAndRights','Should the product show in the Role/Rights page','0', 0)
,('ShowInAppSwitcher','Should the product show in the application switcher','1', 0)
,('ShowInUserListFilter','Should the product show in the user list product pick list','1', 0)
,('ProductAPIRequiresUser','Does the product require a user for api calls','0', 0)
,('LockOnProductAccess', '', '0', 0)
,('ProductNotAvailableForRegularUserNoEmail','Product Attribute for Product Not Available for Regular User No Email.','0', 0)

,('CLIENTID','','ulrenoapi', 0) -- For DEV Environment
,('TOKENENDPOINT','', @tokenEndPoint, 0) -- For DEV Environment
,('APISECRET','', @apisecret, 1)

,('GetRoleEndpoint','Role End point for product API','/{0}/roles?isIncludeRights={1}', 0)
,('GetRightEndpoint','Right End point for product API','/roleRights/{0}', 0)
,('GetPropertyEndpoint','Property End point for product API','/{0}/properties', 0)
,('GetUserEndpoint','GET User Endpoint for product API','/users?companyId={0}&loginName={1}', 0)
,('GetListUsersEndpoint','','/{0}/users?filter={1}&startRow={2}&resultsperpage={3}', 0)
,('PostUserEndpoint','POST User Endpoint for product API','/users', 0)
,('PutUserEndpoint','PUT User Endpoint for product API','/users', 0)
,('DeleteUserEndpoint','DELETE User Endpoint for product API','/{0}/users?loginName={0}', 0)
,('PatchMigrateUsersEndpoint','Patch Migrate Users Endpoint', '/users/{0}/migrate', 0)
,('PatchProfileEndpoint','PATCH Profile Endpoint for product API','/userprofile', 0)
,('GetUserExistEndpoint','Get User Exist Endpoint for product API','/userexists?loginName={0}', 0) -- Made New Setting
,('AuthenticationType','Used to determine how to log into the product','Redirect', 0)

--SELECT * FROM @ProductConfiguration

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
 SettingValue,
 SettingSensitiveData
)
VALUES
('ClientId','','1', 0)
,('ClassName','','seniorleadmanagement', 0)
,('ProductUrl','','/product/seniorleadmanagement', 0)
,('TitleId','','Senior Lead Management', 0)
,('TitleUniqueId','','EDFB27F1-6335-4297-B44F-F265204B0538', 0)
,('IsNewTab','','1', 0)
,('MetatagUniqueId','','Senior Lead Management', 0)
,('IsResource','','0', 0)
,('IsFavorite','','1', 0)
,('LearnMore','','https://www.realpage.com/senior/', 0)
,('ApiEndPoint','','https://dev-spmadminbff.realpage.com/api/v1', 0)
,('ProductStatus','Show if the external application was configured for the dashboard user.','8', 0)
,('ShowInUserDetails','Should the product show in the New/Edit user pages','1', 0)
,('ShowInRolesAndRights','Should the product show in the Role/Rights page','0', 0)
,('ShowInAppSwitcher','Should the product show in the application switcher','1', 0)
,('ShowInUserListFilter','Should the product show in the user list product pick list','1', 0)
,('ProductAPIRequiresUser','Does the product require a user for api calls','0', 0)
,('LockOnProductAccess', '', '0', 0)
,('ProductNotAvailableForRegularUserNoEmail','Product Attribute for Product Not Available for Regular User No Email.','0', 0)

--,('ApiUserName','','test+rfqarpunity@novelpay.com', 0)
--,('ApiPassword','','7mgp43EIvc8c!@', 1)
,('ApiKey','','53448358-FC1C-4B30-8C45-1171B06D84D1', 0) -- For DEV Environment

,('GetRoleEndpoint','Role End point for product API','/{0}/Roles?isIncludeRights={1}', 0)
,('GetRightEndpoint','Right End point for product API','/roleRights/{0}', 0)

,('GetPropertyEndpoint','Property End point for product API','/properties/{0}', 0)
,('GetUserEndpoint','GET User Endpoint for product API','/users?loginName={0}', 0)
,('GetListUsersEndpoint','','/users/{0}?filter={1}&pageNumber={2}&PageSize={3}', 0)
,('PostUserEndpoint','POST User Endpoint for product API','/users', 0)
,('PutUserEndpoint','PUT User Endpoint for product API','/users', 0)
,('DeleteUserEndpoint','DELETE User Endpoint for product API','/users?loginName={0}', 0)
,('PatchMigrateUsersEndpoint','Patch Migrate Users Endpoint', '/users/{0}/migrate', 0)
,('PatchProfileEndpoint','PATCH Profile Endpoint for product API','/userprofile', 0)
,('GetUserExistEndpoint','Get User Exist Endpoint for product API','/userexists?loginName={0}', 0) -- Made New Setting
,('AuthenticationType','Used to determine how to log into the product','Redirect', 0)
--- Not sure about below ones -------
--,('GetCompanyEndpoint','GET Company Endpoint for API','/orgs/{0}', 0)
--,('GetParentCompanyEndpoint','GET Company Endpoint for API','/orgs?parentOrgId={0}', 0)
--,('GetProfileEndpoint','GET User Profile Endpoint for product API','/users/{0}/profile', 0)


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

-- Unified Amenities rights in Sentence case instead of Title Case format
	IF EXISTS(SELECT * FROM Enterprise.RightValueType WHERE TargetProductId = 26)
	BEGIN
		UPDATE [Enterprise].[RightValueType]
		SET [VALUE]= UPPER(LEFT([VALUE],1))+LOWER(SUBSTRING([VALUE],2,LEN([VALUE]))),
		  [Description] = UPPER(LEFT([Description],1))+LOWER(SUBSTRING([Description],2,LEN([Description])))
		WHERE TargetProductId = 26
	END

	IF EXISTS(select * from enterprise.RightValueType where TargetProductId = 26 and [Value] like'%Un-Assign%')
	BEGIN
		UPDATE [Enterprise].[RightValueType]
		SET [VALUE]= REPLACE([VALUE],  'Un-Assign',  'Unassign' ),
		  [Description] = REPLACE([Description],  'Un-Assign',  'Unassign' )
		WHERE TargetProductId = 26 and  [Value] like'%Un-Assign%'
	END

	IF EXISTS(select * from enterprise.RightValueType where TargetProductId = 26 and [Value] like'%Depriciation%')
	BEGIN
		UPDATE [Enterprise].[RightValueType]
		SET [VALUE]= REPLACE([VALUE],  'Depriciation',  'depreciation' ),
		  [Description] = REPLACE([Description],  'Depriciation',  'depreciation' )
		WHERE TargetProductId = 26 and [Value] like'%Depriciation%'
	END

	IF EXISTS( select * from enterprise.RightValueType where TargetProductId = 26 and [Value] like'%Amenites%')
	BEGIN
		UPDATE [Enterprise].[RightValueType]
		SET [VALUE]= REPLACE([VALUE],  'Amenites',  'amenities' ),
		  [Description] = REPLACE([Description],  'Amenites',  'amenities' )
		WHERE TargetProductId = 26 and [Value] like'%Amenites%'
	END

-- Unified Amenities: "View Amenities" Role is selected as default
DECLARE @RoleValueTypeId int;
SELECT	@RoleValueTypeId = RoleValueTypeId
FROM	Enterprise.RoleValueType
WHERE	Value='View Amenities'

IF EXISTS(SELECT TOP 1 1 FROM Enterprise.Role WHERE RoleValueTypeId=@RoleValueTypeId and PartyId in (1566,132790,82532) and DefaultRole=0)
BEGIN

		UPDATE r SET DefaultRole = 1
         FROM enterprise.rolevaluetype rv
              INNER JOIN enterprise.role r ON r.rolevaluetypeid = rv.rolevaluetypeid
         WHERE rv.value = 'view amenities'
               AND R.PartyId = 1566;

		UPDATE r SET DefaultRole = 1
         FROM enterprise.rolevaluetype rv
              INNER JOIN enterprise.role r ON r.rolevaluetypeid = rv.rolevaluetypeid
         WHERE rv.value = 'view amenities'
               AND R.PartyId = 82532;

		UPDATE r SET DefaultRole = 1
         FROM enterprise.rolevaluetype rv
              INNER JOIN enterprise.role r ON r.rolevaluetypeid = rv.rolevaluetypeid
         WHERE rv.value = 'view amenities'
               AND R.PartyId = 132790;
END

/******   StoredProcedure [Enterprise].[SetupUnifiedAmenities] Sentence case instead of Title Case format   ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE [Enterprise].[SetupUnifiedAmenities](@PartyId BIGINT)
AS
     BEGIN
         SET NOCOUNT ON;
         DECLARE @PersonaId INT;
         DECLARE @FromDate DATETIME;
         DECLARE @TRoleId INT;
         DECLARE @TRoleName NVARCHAR(500);
         DECLARE @TRoleDesc NVARCHAR(500);
         DECLARE @TRoleShortName NVARCHAR(100);
         DECLARE @TRightShortName NVARCHAR(100);
         DECLARE @TRightId INT;
         DECLARE @TRightName NVARCHAR(500);
         DECLARE @TRightDesc NVARCHAR(500);
         DECLARE @RoleId INT;
         DECLARE @RightId INT;
         DECLARE @RightCategory INT;
         DECLARE @RoleCategory INT;
         DECLARE @ProductId INT;
         DECLARE @RoleName NVARCHAR(500);
         DECLARE @RightName NVARCHAR(500);
         DECLARE @RoleTypeID INT;
         DECLARE @PerosonaP INT;
         DECLARE @PartyRowNum INT;
         SET @FromDate = GETUTCDATE();

         SELECT @RoleTypeId = PartyROleTypeId
         FROM enterprise.roletype
         WHERE Name = 'Product Role';
         SELECT @ProductId = ProductId
         FROM Enterprise.Product
         WHERE name = 'Unified Amenities';
         SELECT @RoleCategory = ST.StatusTypeId
         FROM Enterprise.StatusTypeCategoryType AS STCT
              JOIN Enterprise.StatusTypeCategory AS STC ON STCT.StatusTypeCategoryTypeId = STC.StatusTypeCategoryTypeId
              JOIN Enterprise.StatusTypeCategoryClassification AS STCC ON STCC.StatusTypeCategoryId = STC.StatusTypeCategoryId
              JOIN Enterprise.StatusType AS ST ON ST.StatusTypeId = STCC.StatusTypeId
         WHERE STC.Name = 'Role Type'
               AND ST.Name = 'Default';
         SELECT @RightCategory = ST.StatusTypeId
         FROM Enterprise.StatusTypeCategoryType AS STCT
              JOIN Enterprise.StatusTypeCategory AS STC ON STCT.StatusTypeCategoryTypeId = STC.StatusTypeCategoryTypeId
              JOIN Enterprise.StatusTypeCategoryClassification AS STCC ON STCC.StatusTypeCategoryId = STC.StatusTypeCategoryId
              JOIN Enterprise.StatusType AS ST ON ST.StatusTypeId = STCC.StatusTypeId
         WHERE STC.Name = 'Right Type'
               AND ST.Name = 'Default';
         DECLARE @UARole TABLE
			(RoleID      INT,
			 Name        NVARCHAR(500),
			 ShortName   VARCHAR(100),
			 Description NVARCHAR(500)
			);
         INSERT INTO @UARole
			(RoleId,
			 Name,
			 ShortName,
			 Description
			)
         VALUES
			(1,
			 'Manage Amenity Status',
			 'amenity.status',
			 'Ability to inactivate master amenities '
			),
			(2,
			 'Manage Amenity No Pricing',
			 'amenity.no.pricing',
			 'Create and update master amenity marketing content but not pricing '
			),
			(3,
			 'Manage Amenity With Pricing',
			 'amenity.with.pricing',
			 'Create and update master amenity pricing content'
			),
			(4,
			 'Manage Property Amenity No Pricing',
			 'prop.amenity.no.pricing',
			 'Create and update property amenity marketing content but not pricing '
			),
			(5,
			 'Manage Property Amenity With Pricing',
			 'prop.amenity.with.pricing',
			 'Create and update property amenity pricing '
			),
			(6,
			 'View Amenities',
			 'view.amenities',
			 'View master amenities (default access to application - only allows users to view (read-only) amenities list)'
			);
         DECLARE @UARight TABLE
			(RightId     INT,
			 Name        NVARCHAR(500),
			 ShortName   NVARCHAR(100),
			 description NVARCHAR(500)
			);
         INSERT INTO @UARight
			(rightid,
			 name,
			 shortname,
			 description
			)
         VALUES
			(1,
			 'Assign unit amenities',
			 'assign.unit',
			 'Ability to assign unit amenities'
			),
			(2,
			 'Unassign unit amenities',
			 'unassign.unit',
			 'Ability to unassign unit amenities'
			),
			(3,
			 'Assign floorplan amenities',
			 'assign.floorplan',
			 'Ability to assign floorplan amenities'
			),
			(4,
			 'Unassign floorplan amenities',
			 'unassign.floorplan',
			 'Ability to unassign floorplan amenities'
			),
			(5,
			 'Add common area amenity',
			 'ca.add',
			 'Ability to add common area amenity'
			),
			(6,
			 'Edit common area amenity',
			 'ca.edit',
			 'Ability to edit common area amenity'
			),
			(7,
			 'Delete common area amenity',
			 'ca.delete',
			 'Ability to delete common area amenity'
			),
			(8,
			 'Add floorplan unit amenity',
			 'fpu.add',
			 'Ability to add floorplan unit amenity'
			),
			(9,
			 'Edit floorplan unit amenity',
			 'fpu.edit',
			 'Ability to edit floorplan unit amenity'
			),
			(10,
			 'Delete floorplan unit amenity',
			 'fpu.delete',
			 'Ability to delete floorplan unit amenity'
			),
			(11,
			 'Merge common area amenities',
			 'ca.merge',
			 'Ability to merge common area amenities'
			),
			(12,
			 'Merge floorplan unit amenities',
			 'fpu.merge',
			 'Ability to merge floorplan unit amenities'
			),
			(13,
			 'Export common area amenities',
			 'ca.export',
			 'Ability to export common area amenities'
			),
			(14,
			 'Export floorplan unit amenities',
			 'fpu.export',
			 'Ability to export floorplan unit amenities'
			),
			(15,
			 'Add default pricing',
			 'assignto.prop',
			 'Ability to add default pricing'
			),
			(16,
			 'Add Default Pricing',
			 'defaultpricing.add',
			 'Ability to Add Default Pricing'
			),
			(17,
			 'Edit default pricing',
			 'defaultpricing.edit',
			 'Ability to edit default pricing'
			),
			(18,
			 'Delete default pricing',
			 'defaultpricing.delete',
			 'Ability to delete default pricing'
			),
			(19,
			 'Turn on depreciation setting',
			 'depriciation.on',
			 'Ability to turn on depreciation setting'
			),
			(20,
			 'Turn off depreciation setting',
			 'depriciation.off',
			 'Ability to turn off depreciation setting'
			),
			(21,
			 'Add property common area amenity',
			 'prop.ca.add',
			 'Add property common area amenity'
			),
			(22,
			 'Edit property common area amenity',
			 'prop.ca.edit',
			 'Ability to edit property common area amenity'
			),
			(23,
			 'Delete property common area amenity',
			 'prop.ca.delete',
			 'Ability to delete property common area amenity'
			),
			(24,
			 'Add property floorplan unit amenity',
			 'prop.fpu.add',
			 'Ability to add property floorplan unit amenity'
			),
			(25,
			 'Edit property floorplan unit amenity',
			 'prop.fpu.edit',
			 'Ability to edit property floorplan unit amenity'
			),
			(26,
			 'Delete property floorplan unit amenity',
			 'prop.fpu.delete',
			 'Ability to delete property floorplan unit amenity'
			),
			(27,
			 'Merge property common area amenities',
			 'prop.ca.merge',
			 'Ability to merge property common area amenities'
			),
			(28,
			 'Merge property floorplan unit amenities',
			 'prop.fpu.merge',
			 'Ability to merge property floorplan unit amenities'
			),
			(29,
			 'Export property common area amenities',
			 'prop.ca.export',
			 'Ability to export property common area amenities'
			),
			(30,
			 'Export property floorplan unit amenities',
			 'prop.fpu.export',
			 'Ability to export property floorplan unit amenities'
			),
			(31,
			 'Add price points',
			 'price.point.add',
			 'Ability to add price points'
			),
			(32,
			 'Edit price points',
			 'price.point.edit',
			 'Ability to edit price points'
			),
			(33,
			 'Delete price points',
			 'price.point.delete',
			 'Ability to delete price points'
			),
			(34,
			 'Add depreciation schedule',
			 'depreciation.add',
			 'Ability to add depreciation schedule'
			),
			(35,
			 'Edit depreciation schedule',
			 'depreciation.edit',
			 'Ability to edit depreciation schedule'
			),
			(36,
			 'View common area amenities master list',
			 'ca.master.view',
			 'Ability to view common area amenities master list'
			),
			(37,
			 'View common area amenities by property',
			 'ca.property.view',
			 'Ability to view common area amenities by property'
			),
			(38,
			 'View floorplan unit amenities master list',
			 'fpu.master.view',
			 'Ability to view floorplan unit amenities master list'
			),
			(39,
			 'View floorplan unit amenities by property',
			 'fpu.property.view',
			 'Ability to view floorplan unit amenities by property'
			),
			(40,
			 'View activity page',
			 'activity.view',
			 'Ability to view activity page'
			);
         DECLARE @UAMapping TABLE
			(RoleId  INT,
			 RightId INT
			);

INSERT INTO @UAMapping( RoleId, RightId )
VALUES( 1, 1 ), ( 1, 2 ), ( 1, 3 ), ( 1, 4 ), ( 2, 1 ), ( 2, 2 ), ( 2, 3 ), ( 2, 4 ), ( 3, 1 ), ( 3, 2 ), ( 3, 3 ), ( 3, 4 ), ( 4, 1 ), ( 4, 2 ), ( 4, 3 ), ( 4, 4 ), ( 5, 1 ), ( 5, 2 ), ( 5, 3 ), ( 5, 4 ), ( 2, 5 ), ( 2, 6 ), ( 2, 7 ), ( 2, 8 ), ( 2, 9 ), ( 2, 10 ), ( 2, 11 ), ( 2, 12 ), ( 2, 13 ), ( 2, 14 ), ( 2, 15 ), ( 3, 5 ), ( 3, 6 ), ( 3, 7 ), ( 3, 8 ), ( 3, 9 ), ( 3, 10 ), ( 3, 11 ), ( 3, 12 ), ( 3, 13 ), ( 3, 14 ), ( 3, 15 ), ( 3, 16 ), ( 3, 17 ), ( 3, 18 ), ( 3, 19 ), ( 3, 20 ), ( 2, 21 ), ( 2, 22 ), ( 2, 23 ), ( 2, 24 ), ( 2, 25 ), ( 2, 26 ), ( 2, 27 ), ( 2, 28 ), ( 2, 29 ), ( 2, 30 ), ( 3, 21 ), ( 3, 22 ), ( 3, 23 ), ( 3, 24 ), ( 3, 25 ), ( 3, 26 ), ( 3, 27 ), ( 3, 28 ), ( 3, 29 ), ( 3, 30 ), ( 4, 21 ), ( 4, 22 ), ( 4, 23 ), ( 4, 24 ), ( 4, 25 ), ( 4, 26 ), ( 4, 27 ), ( 4, 28 ), ( 4, 29 ), ( 4, 30 ), ( 5, 21 ), ( 5, 22 ), ( 5, 23 ), ( 5, 24 ), ( 5, 25 ), ( 5, 26 ), ( 5, 27 ), ( 5, 28 ), ( 5, 29 ), ( 5, 30 ), ( 3, 31 ), ( 3, 32 ), ( 3, 33 ), ( 3, 34 ), ( 3, 35 ), ( 5, 31 ), ( 5, 32 ), ( 5, 33 ), ( 5, 34 ), ( 5, 35 ), ( 1, 36 ), ( 1, 37 ), ( 1, 38 ), ( 1, 39 ), ( 1, 40 ), ( 2, 36 ), ( 2, 37 ), ( 2, 38 ), ( 2, 39 ), ( 2, 40 ), ( 3, 36 ), ( 3, 37 ), ( 3, 38 ), ( 3, 39 ), ( 3, 40 ), ( 4, 36 ), ( 4, 37 ), ( 4, 38 ), ( 4, 39 ), ( 4, 40 ), ( 5, 36 ), ( 5, 37 ), ( 5, 38 ), ( 5, 39 ), ( 5, 40 ), ( 6, 36 ), ( 6, 37 ), ( 6, 38 ), ( 6, 39 ), ( 6, 40 );
         DECLARE @HoldPartyid TABLE
			(ROwnumber           INT IDENTITY(1, 1),
			 OrganizationPartyId INT,
			 PStatus             BIT DEFAULT 0
			);
         INSERT INTO @HoldPartyid(OrganizationPartyId)
         VALUES(@PartyId);


--select * from enterprise.organization where name  In (N'American Landmark Management LLC')
--select * from enterprise.dataimportmapping where sourceid in (2416 , 1076 )


         WHILE EXISTS
		(
			SELECT 1
			FROM @HoldPartyid
			WHERE PStatus = 0
		)
             BEGIN
                 SELECT TOP 1 @PartyRowNum = Rownumber,
                              @PartyId = OrganizationPartyID
                 FROM @HoldPartyid
                 WHERE PStatus = 0;
                 DECLARE Roles CURSOR
                 FOR SELECT RoleId,
                            Name,
                            ShortName,
                            Description
                     FROM @UARole;
                 OPEN Roles;
                 FETCH Roles INTO @TRoleId, @TRoleName, @TRoleShortName, @TRoleDesc;
                 WHILE @@FETCH_STATUS = 0
                     BEGIN
                         EXECUTE Enterprise.CreateRole
                                 @RoleName = @TRoleName,
                                 @Shortname = @TRoleShortName,
                                 @Description = @TRoleDesc,
                                 @RoleTypeId = @RoleTypeId,
                                 @RoleCategoryId = @RoleCategory,
                                 @PartyId = @PartyId,
                                 @RoleId = @RoleId OUTPUT;
                         DECLARE RightsL CURSOR
                         FOR SELECT Name,
                                    Description,
                                    ShortName,
                                    b.RightId
                             FROM @UARight AS a
                                  INNER JOIN @UAMapping AS b ON a.RightId = b.RightId
                                                                AND b.RoleID = @TRoleId;
                         OPEN RightsL;
                         FETCH RightsL INTO @TRightName, @TRightDesc, @TRightShortName, @TRightId;
                         WHILE @@FETCH_STATUS = 0
                             BEGIN
                                 EXECUTE Enterprise.CreateRight
                                         @RoleId = @RoleId,
                                         @RightName = @TRightName,
                                         @RightCategoryId = @RightCategory,
                                         @PartyId = @PartyId,
                                         @ProductId = @ProductId,
                                         @Shortname = @TRightShortName,
                                         @Description = @TRightDesc,
                                         @targetProductid = @productId,
                                         @RightId = @RightId OUTPUT;
                                 FETCH RightsL INTO @TRightName, @TRightDesc, @TRightShortName, @TRightId;
                             END;
                         CLOSE RightsL;
                         DEALLOCATE RightsL;
                         FETCH Roles INTO @TRoleId, @TRoleName, @TRoleShortName, @TRoleDesc;
                     END;
                 CLOSE Roles;
                 DEALLOCATE Roles;
                 UPDATE @HoldPartyid
                   SET
                       PStatus = 1
                 WHERE RowNumber = @PartyRowNum;
             END;
         UPDATE r
           SET
               DefaultRole = 1
         FROM enterprise.rolevaluetype rv
              INNER JOIN enterprise.role r ON r.rolevaluetypeid = rv.rolevaluetypeid
         WHERE rv.value = 'view amenities'
               AND R.PartyId = @PartyId;
   END;

