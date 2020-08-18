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

if not exists ( select top 1 1 from Enterprise.ProductSettingType where name = 'BooksUseTranslatev2' )
begin
	INSERT INTO Enterprise.ProductSettingType ( name, Description, SensitiveData ) values ( 'BooksUseTranslatev2', 'Use v2 of the books translate endpoint', 0 )
end
GO

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

if not exists(Select top 1 1 from Enterprise.ProductSetting ps 
				inner join Enterprise.ProductSettingType pst
				on ps.ProductSettingTypeId = pst.ProductSettingTypeId
				where pst.Name = 'BooksUseTranslatev2' and ps.ProductId= 3)
Begin
	Insert into Enterprise.ProductSetting (ProductId, ProductSettingTypeId, Value, FromDate)
	Select 3, ProductSettingTypeId, '1', GETUTCDATE()
	from Enterprise.ProductSettingType
	where Name = 'BooksUseTranslatev2'

	declare @productsettingid int
	select @productsettingid = productsettingid from Enterprise.ProductSetting ps 
				inner join Enterprise.ProductSettingType pst
				on ps.ProductSettingTypeId = pst.ProductSettingTypeId
				where pst.Name = 'BooksUseTranslatev2' and ps.ProductId= 3

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
IF @ServerName IN ('RCQUSODBSQL001')
BEGIN
	SET @apiendpoint = 'https://rl-valueadd-sat.herokuapp.com/api/v1/unified-login';
	SET @tokenEndPoint = 'https://www-sat.realpage.com/login/identity/connect/token';
	SET @apisecret = '1E0761CB-FACE-483E-8933-BA8B2C778ABB';
END
IF @ServerName IN ('RCPGBKDBSQL005A', 'RCPGBKDBSQL005B')
BEGIN
	SET @apiendpoint = 'https://reno.rentlytics.com/api/v1/unified-login/';
	SET @tokenEndPoint = 'https://www.realpage.com/login/identity/connect/token';
	SET @apisecret = '981B367E-8F98-47BA-8CC0-CA1B317CAB0E';
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
,('PatchProfileEndpoint','PATCH Profile Endpoint for product API','/users/profiles', 0)
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
IF @ServerName IN ('RCQUSODBSQL001')
BEGIN
	SET @LoginURL = 'https://rl-valueadd-sat.herokuapp.com/auth-callback';
END
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

IF NOT EXISTS (SELECT TOP 1 1 FROM[UserManagement].[ControlAttribute] WHERE ControlId = 504)
BEGIN
	SET IDENTITY_INSERT [UserManagement].[ControlAttribute] ON 

	INSERT [UserManagement].[ControlAttribute] ([ControlAttributeId], [ControlId], [Key], [Value], [CreatedBy], [CreatedDate]) 
	VALUES (138, 504, N'FilterType', N'menu', @UserId, @Now)

	SET IDENTITY_INSERT [UserManagement].[ControlAttribute] OFF
END

IF EXISTS (SELECT TOP 1 1 FROM [UserManagement].[Control] WHERE ControlId = 247)
BEGIN
	DELETE FROM [UserManagement].[Control] WHERE  ControlId = 247
END

IF EXISTS (SELECT TOP 1 1 FROM [UserManagement].[Control] WHERE ControlId = 374)
BEGIN
	DELETE FROM [UserManagement].[Control] WHERE  ControlId = 374
END

IF EXISTS (SELECT TOP 1 1 FROM [UserManagement].[Control] WHERE ControlId = 266)
BEGIN
	DELETE FROM [UserManagement].[Control] WHERE  ControlId = 266
END

IF EXISTS (SELECT TOP 1 1 FROM [UserManagement].[Control] WHERE ControlId = 294)
BEGIN
	DELETE FROM [UserManagement].[Control] WHERE  ControlId = 294
END

IF EXISTS (SELECT TOP 1 1 FROM [UserManagement].[Control] WHERE ControlId = 313)
BEGIN
	DELETE FROM [UserManagement].[Control] WHERE  ControlId = 313
END

IF EXISTS (SELECT TOP 1 1 FROM [UserManagement].[Control] WHERE ControlId = 332)
BEGIN
	DELETE FROM [UserManagement].[Control] WHERE  ControlId = 332
END

IF EXISTS (SELECT TOP 1 1 FROM [UserManagement].[Control] WHERE ControlId = 351)
BEGIN
	DELETE FROM [UserManagement].[Control] WHERE  ControlId = 351
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

IF EXISTS(SELECT TOP 1 1 FROM Enterprise.Role WHERE RoleValueTypeId=@RoleValueTypeId and  DefaultRole=0)
BEGIN

		UPDATE r SET DefaultRole = 1
         FROM enterprise.rolevaluetype rv
              INNER JOIN enterprise.role r ON r.rolevaluetypeid = rv.rolevaluetypeid
         WHERE rv.value = 'view amenities'
               AND r.DefaultRole=0

END

GO
DECLARE @UserId bigint,
	@ProductId int =3,
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

	IF
	(
		SELECT 1
		FROM Enterprise.ProductSettingType
		WHERE Name = 'RolesRightsSchemaName'
	) IS NULL
	BEGIN
		EXEC Enterprise.CreateProductSettingType 'RolesRightsSchemaName', 'Unified Platform RolesRights Schema Name',0, @ProductSettingTypeId OUTPUT;
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
		@Value = 'Enterprise', 
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
		WHERE Name = 'SaveRoleDataInEnterprise'
	) IS NULL
	BEGIN
		EXEC Enterprise.CreateProductSettingType 'SaveRoleDataInEnterprise', 'Save Role Data in Unified Platform Enterprise RolesRights Schema',0, @ProductSettingTypeId OUTPUT;
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
		@Value = '1', 
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

IF EXISTS (SELECT TOP 1 1 FROM Enterprise.RightValueType WHERE [Value] = 'Ability to manage Platform Notifications')
BEGIN
	update Enterprise.RightValueType set [Value] = 'Manage Notifications Configurations' where [Value] = 'Ability to manage Platform Notifications';
END

GO

update Enterprise.[RightValueType] set ShortName = 'AccessHelpCenter' 
where Value = 'Access to Help Center'

GO

--300025-Add new rights for Unified Notifications -Platform Alerts
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
DECLARE @DefaultRightName nvarchar(200);
DECLARE @TargetRoleName nvarchar(100);
DECLARE @RoleId int;
DECLARE @OutputRightId int;
DECLARE @UserActionId int;
DECLARE @RightValueTypeId int;
DECLARE @DependentRightValueTypeId int;

/*SET BLOCK*/
SET @TargetRoleName = 'User Administrator'; --- Role to which the new right will be assinged by default.
SET @RightName = 'Create platform alerts'; -- Name of the right 
SET @RightDescription = 'Create platform alerts'; --Description of the right as stated in story.
SET @RightShortName = 'CreatePlatformAlerts'; --Short name of the right that is being used by the application
SET @ActionName = 'Create platform alerts'; -- This specifically pertains to actions used for routing purposes. 
SET @ActionRouteTarget = 'SideMenu'; -- Where you want this right to show up. other variation is DashBoard.
SET @ActionValueID = 1;
SET @DefaultRightName = 'Default_' + @RightShortName; -- This is used internally for creating right dependency in RightDependency table.

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
	 ON P.PartyId = O.PartyId
WHERE O.Name = 'RealPage Employee'; 

/*SELECT REQUIRED ATTRIBUTES FOR ROLE, RIGHT, AND ACTIONS*/
SELECT @SourceProductId = ProductId
FROM Enterprise.Product
WHERE name = 'Unified Platform';

SELECT @TargetProductId = ProductId
FROM Enterprise.Product
WHERE Name = 'Unified Platform';

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

IF NOT EXISTS (SELECT 1 FROM Enterprise.ACTION WHERE ObjectValue = @ActionName AND ParentActionId IS NULL)
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

IF NOT EXISTS(SELECT 1 FROM Enterprise.ACTION WHERE ObjectValue = @ActionName AND ParentActionID = @ParentActionId)
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

WHILE EXISTS (SELECT 1 FROM #HoldParty WHERE PStatus = 0)
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
	EXECUTE Enterprise.CreateRight @RoleId = -1, @RightName = @DefaultRightName, @ShortName = @RightShortName, @RightCategoryId = @RightCategory, @PartyId = @OrganizationId, @ProductId = @SourceProductId, @Description = '', @TargetProductId = @TargetProductId, @VisibilityStatusId = @VisibilityStatusId, @RightId = @OutputRightId OUTPUT;
	EXEC [Enterprise].[LinkActionToRights] @ActionID = @ActionID, @RightId = @OutputRightId, @StatusId = @VisibilityStatusId, @UserActionId = @UserActionId OUTPUT;
	EXECUTE Enterprise.CreateRight @RoleId = @RoleId, @RightName = @RightName, @RightCategoryId = @RightCategory, @PartyId = @OrganizationId, @ProductId = @SourceProductId, @Shortname = @RightShortName, @Description = @RightDescription, @TargetProductId = @TargetProductId, @VisibilityStatusId = @VisibilityStatusId, @RightId = @OutputRightId OUTPUT;
	EXEC [Enterprise].[LinkActionToRights] @ActionID = @ActionID, @RightId = @OutputRightId, @StatusId = @VisibilityStatusId, @UserActionId = @UserActionId OUTPUT;
	
	UPDATE #HoldParty SET PStatus = 1 WHERE RowNumber = @PartyRowNum;
END;

/*Setup Dependencies for custom roles*/

SELECT @DependentRightValueTypeId = RightValueTypeId
FROM Enterprise.RightValueType
WHERE value = @DefaultRightName;

SELECT @RightValueTypeId = RightValueTypeId
FROM Enterprise.RightValueType
WHERE value = @RightName;

IF NOT EXISTS (SELECT 1 FROM Enterprise.RightDependency 
			   WHERE RightValueTypeId = @RightValueTypeId 
			   AND DependentRightValueTypeId = @DependentRightValueTypeId)
BEGIN
	INSERT INTO Enterprise.RightDependency( RightValueTypeId, DependentRightValueTypeId )
	VALUES( @RightValueTypeId, @DependentRightValueTypeId );
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
DECLARE @DefaultRightName nvarchar(200);
DECLARE @TargetRoleName nvarchar(100);
DECLARE @RoleId int;
DECLARE @OutputRightId int;
DECLARE @UserActionId int;
DECLARE @RightValueTypeId int;
DECLARE @DependentRightValueTypeId int;

/*SET BLOCK*/
SET @TargetRoleName = 'User Administrator'; --- Role to which the new right will be assinged by default.
SET @RightName = 'Approve platform alerts'; -- Name of the right 
SET @RightDescription = 'Approve platform alerts'; --Description of the right as stated in story.
SET @RightShortName = 'ApprovePlatformAlerts'; --Short name of the right that is being used by the application
SET @ActionName = 'Approve platform alerts'; -- This specifically pertains to actions used for routing purposes. 
SET @ActionRouteTarget = 'SideMenu'; -- Where you want this right to show up. other variation is DashBoard.
SET @ActionValueID = 1;
SET @DefaultRightName = 'Default_' + @RightShortName; -- This is used internally for creating right dependency in RightDependency table.

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
	 ON P.PartyId = O.PartyId
WHERE O.Name = 'RealPage Employee'; 

/*SELECT REQUIRED ATTRIBUTES FOR ROLE, RIGHT, AND ACTIONS*/
SELECT @SourceProductId = ProductId
FROM Enterprise.Product
WHERE name = 'Unified Platform';

SELECT @TargetProductId = ProductId
FROM Enterprise.Product
WHERE Name = 'Unified Platform';

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

IF NOT EXISTS (SELECT 1 FROM Enterprise.ACTION WHERE ObjectValue = @ActionName AND ParentActionId IS NULL)
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

IF NOT EXISTS(SELECT 1 FROM Enterprise.ACTION WHERE ObjectValue = @ActionName AND ParentActionID = @ParentActionId)
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

WHILE EXISTS (SELECT 1 FROM #HoldParty WHERE PStatus = 0)
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
	EXECUTE Enterprise.CreateRight @RoleId = -1, @RightName = @DefaultRightName, @ShortName = @RightShortName, @RightCategoryId = @RightCategory, @PartyId = @OrganizationId, @ProductId = @SourceProductId, @Description = '', @TargetProductId = @TargetProductId, @VisibilityStatusId = @VisibilityStatusId, @RightId = @OutputRightId OUTPUT;
	EXEC [Enterprise].[LinkActionToRights] @ActionID = @ActionID, @RightId = @OutputRightId, @StatusId = @VisibilityStatusId, @UserActionId = @UserActionId OUTPUT;
	EXECUTE Enterprise.CreateRight @RoleId = @RoleId, @RightName = @RightName, @RightCategoryId = @RightCategory, @PartyId = @OrganizationId, @ProductId = @SourceProductId, @Shortname = @RightShortName, @Description = @RightDescription, @TargetProductId = @TargetProductId, @VisibilityStatusId = @VisibilityStatusId, @RightId = @OutputRightId OUTPUT;
	EXEC [Enterprise].[LinkActionToRights] @ActionID = @ActionID, @RightId = @OutputRightId, @StatusId = @VisibilityStatusId, @UserActionId = @UserActionId OUTPUT;
	
	UPDATE #HoldParty SET PStatus = 1 WHERE RowNumber = @PartyRowNum;
END;

/*Setup Dependencies for custom roles*/

SELECT @DependentRightValueTypeId = RightValueTypeId
FROM Enterprise.RightValueType
WHERE value = @DefaultRightName;

SELECT @RightValueTypeId = RightValueTypeId
FROM Enterprise.RightValueType
WHERE value = @RightName;

IF NOT EXISTS (SELECT 1 FROM Enterprise.RightDependency 
			   WHERE RightValueTypeId = @RightValueTypeId 
			   AND DependentRightValueTypeId = @DependentRightValueTypeId)
BEGIN
	INSERT INTO Enterprise.RightDependency( RightValueTypeId, DependentRightValueTypeId )
	VALUES( @RightValueTypeId, @DependentRightValueTypeId );
END;

GO
--------------------------------------------OmniChannel----------------------------------------------
--DELETE	OmniChannel Roles and Rights
DECLARE @ProductId int,
	@ProductName nvarchar(50)

SET @ProductName = 'OmniChannel'

DECLARE @Right TABLE (
	RightID int,
	RoleID int,
	RightValueTypeId int,
	PartyId bigint
)

SELECT	@ProductId = ProductId
FROM		Enterprise.Product
WHERE	Name = @ProductName

INSERT INTO @Right (
	RightID,
	RoleID,
	RightValueTypeId,
	PartyId
)
SELECT	eri.RightID,
				eri.RoleID,
				eri.RightValueTypeId,
				eri.PartyId
FROM		Enterprise.RightValueType erivt
				INNER JOIN Enterprise.[Right] eri ON (eri.RightValueTypeId = erivt.RightValueTypeId)
WHERE	ProductId = @ProductId

DELETE	eri
FROM		Enterprise.[Right] eri
				INNER JOIN @Right ri ON (ri.RoleID = eri.RoleID)

DELETE
FROM		Enterprise.RightValueType
WHERE	ProductId = @ProductId

DELETE	epp
FROM	Enterprise.PersonaPrivilege epp
			INNER JOIN @Right ri ON (epp.RoleID = ri.RoleID)

DELETE	ero
FROM		Enterprise.Role ero
				INNER JOIN @Right ri ON (ero.RoleID = ri.RoleID)

DELETE	erovt
FROM		Enterprise.RoleValueType erovt
				INNER JOIN Enterprise.Role ero ON (ero.RoleValueTypeId = erovt.RoleValueTypeId)
				INNER JOIN @Right ri ON (ri.RoleID = ero.RoleID)
GO

--------------------------------------L&R Conversion Utility----------------------------------------
DECLARE @ProductId int,
	@ProductName nvarchar(50)

DECLARE @RightValueType TABLE (
	RightValueTypeId int
)

SET @ProductName = 'L&R Conversion Utility'

SELECT	@ProductId = ProductId
FROM		Enterprise.Product
WHERE	Name = @ProductName

INSERT INTO @RightValueType (
	RightValueTypeId
)
SELECT	RightValueTypeId
FROM		Enterprise.RightValueType
WHERE	Value IN (
	'Access to L&R Conversion Utility for OneSite users'

)
AND		ProductId = @ProductId

DELETE	eri
FROM		Enterprise.[Right] eri
				INNER JOIN @RightValueType rvt ON (eri.RightValueTypeId = rvt.RightValueTypeId)

DELETE	erd
FROM		Enterprise.RightDependency erd
				INNER JOIN @RightValueType rvt ON (erd.RightValueTypeId = rvt.RightValueTypeId)

DELETE	erivt
FROM		Enterprise.RightValueType erivt
				INNER JOIN @RightValueType rvt ON (erivt.RightValueTypeId = rvt.RightValueTypeId)
GO

-----------------------------------------Spend Management-------------------------------------------
DECLARE @ProductId int,
	@ProductName nvarchar(50)

DECLARE @RightValueType TABLE (
	RightValueTypeId int
)

SET @ProductName = 'Spend Management'

SELECT	@ProductId = ProductId
FROM		Enterprise.Product
WHERE	Name = @ProductName

INSERT INTO @RightValueType (
	RightValueTypeId
)
SELECT	RightValueTypeId
FROM		Enterprise.RightValueType
WHERE	Value IN (
	'Manage Vendor Compliance Product Access'
)
AND		ProductId = @ProductId

DELETE	eri
FROM		Enterprise.[Right] eri
				INNER JOIN @RightValueType rvt ON (eri.RightValueTypeId = rvt.RightValueTypeId)

DELETE	erd
FROM		Enterprise.RightDependency erd
				INNER JOIN @RightValueType rvt ON (erd.RightValueTypeId = rvt.RightValueTypeId)

DELETE	erivt
FROM		Enterprise.RightValueType erivt
				INNER JOIN @RightValueType rvt ON (erivt.RightValueTypeId = rvt.RightValueTypeId)
GO

-----------------------------------------Unified Platform-------------------------------------------
DECLARE @ProductId int,
	@ProductName nvarchar(50)

DECLARE @RightValueType TABLE (
	RightValueTypeId int
)

SET @ProductName = 'Unified Platform'

SELECT	@ProductId = ProductId
FROM		Enterprise.Product
WHERE	Name = @ProductName

INSERT INTO @RightValueType (
	RightValueTypeId
)
SELECT	RightValueTypeId
FROM		Enterprise.RightValueType
WHERE	Value IN (
	'Access to Settings Admin for OneSite',
	'Ability to edit password',
	'Ability to Configure Custom Fields for Users',
	'Ability to view Company page',
	'Ability to edit Company information',
	'Ability to view Property page',
	'Ability to edit Property information',
	'Access to Green Book Migration Tool',
	'Access to Amenities Tool',
	'Access to Employee Management',
	'Access to Identity Provider Configuration Page',
	'Access to Leasing & Rents Conversion Tool',
	'Access to Property Hierarchy Tool',
	'Impersonate a User',
	'See All RealPage Products',
	'Access to Settings Admin for OneSite',
	'Manage Unified Platform Security Settings',
	'Manage all Unified Setting',
	'Employee Access to CIMPL: Standard Implementation Access'
)
AND		ProductId = @ProductId

DELETE	eri
FROM		Enterprise.[Right] eri
				INNER JOIN @RightValueType rvt ON (eri.RightValueTypeId = rvt.RightValueTypeId)

DELETE	erd
FROM		Enterprise.RightDependency erd
				INNER JOIN @RightValueType rvt ON (erd.RightValueTypeId = rvt.RightValueTypeId)

DELETE	erivt
FROM		Enterprise.RightValueType erivt
				INNER JOIN @RightValueType rvt ON (erivt.RightValueTypeId = rvt.RightValueTypeId)
GO

----------------------------------------------------------------------------------------------------
--Rename
 Update Enterprise.RightValueType Set Value = 'Access to Unified Settings'
 Where  Value = 'View All Unified Settings'

 GO
 
 if not exists ( select top 1 1 from Enterprise.ProductSettingType where name = 'UsePropertyInstanceUnifiedLogin' )
begin
	INSERT INTO Enterprise.ProductSettingType ( name, Description, SensitiveData ) values ( 'UsePropertyInstanceUnifiedLogin', 'Use property instances for Unified Login property list', 0 )
end
GO


if not exists(Select top 1 1 from Enterprise.ProductSetting ps 
				inner join Enterprise.ProductSettingType pst
				on ps.ProductSettingTypeId = pst.ProductSettingTypeId
				where pst.Name = 'UsePropertyInstanceUnifiedLogin' and ps.ProductId= 3)
Begin
	Insert into Enterprise.ProductSetting (ProductId, ProductSettingTypeId, Value, FromDate)
	Select 3, ProductSettingTypeId, '0', GETUTCDATE()
	from Enterprise.ProductSettingType
	where Name = 'UsePropertyInstanceUnifiedLogin'

	declare @productsettingid int
	select @productsettingid = productsettingid from Enterprise.ProductSetting ps 
				inner join Enterprise.ProductSettingType pst
				on ps.ProductSettingTypeId = pst.ProductSettingTypeId
				where pst.Name = 'UsePropertyInstanceUnifiedLogin' and ps.ProductId= 3

	insert into enterprise.ProductConfiguration ( ConfigurationId, ProductSettingId, FromDate )
		select top 1 ConfigurationId, @productsettingid, GETUTCDATE() from enterprise.GlobalProductConfiguration where productid = 3 and thrudate is null
end
GO


-- migrate data from old table to new
if not exists ( select top 1 1 from Enterprise.PropertyInstanceMapping ) AND exists ( select top 1 1 from enterprise.PropertyInstance )
BEGIN
	insert into enterprise.PropertyInstanceMapping ( PersonaId, PropertyInstanceId, ProductId, fromdate, thrudate, active )
	select personaid, pi1.PropertyInstanceId, productid, fromdate, thrudate, case when pm.ThruDate is null then 1 else 0 end
		from enterprise.PropertyMapping PM 
		inner join enterprise.PropertyInstance pi1 on PM.PropertyId = pi1.CustomerPropertyId
		where pm.ThruDate is null
		and pi1.Domain = 'primary'
	and pm.ProductId NOT IN ( 26 )
END
GO


--Start For Reno product internal settings
GO

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
			Select @roleId = 1
		END
		IF @ServerName IN ('RCQUSODBSQL001')
		BEGIN
			Select @roleId = 1
		END
		IF @ServerName IN ('RCPGBKDBSQL005A', 'RCPGBKDBSQL005B')
		BEGIN
			Select @roleId = 1
		END

		IF
		(
			SELECT 1
			FROM Enterprise.ProductSettingType
			WHERE Name = 'SuperUserRoleId1'
		) IS NULL
		BEGIN
			EXEC Enterprise.CreateProductSettingType 'SuperUserRoleId1', 'The role Id to create admin user in  product', 0,@ProductSettingTypeId OUTPUT;
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
			EXEC Enterprise.CreateProductSettingType 'ClientScope', 'The client scope to get access token', 0,@ProductSettingTypeId OUTPUT;
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
			WHERE Name = 'SuperUserRoleId2'
		) IS NULL
		BEGIN
			EXEC Enterprise.CreateProductSettingType 'SuperUserRoleId2', 'The role Id to create admin user in  product', 0,@ProductSettingTypeId OUTPUT;
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
			EXEC Enterprise.CreateProductSettingType 'ClientScope', 'The client scope to get access token', 0,@ProductSettingTypeId OUTPUT;
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
--End For Reno product internal settings

--START: 305141-Display Top level Properties tab when the users role contains rights to Settings
DECLARE @UserId bigint,	@Now datetime = GETDATE();

SELECT	@UserId = UserId
FROM	Ident.UserLogin
WHERE	LoginName LIKE 'realpagead@%'

SET IDENTITY_INSERT [UserManagement].[ControlDependency] ON 
	
	IF NOT EXISTS(select top 1 1 from [UserManagement].[ControlDependency] where mastercontrolvalue = 'Managecompanylevelsettings')
	BEGIN
		INSERT [UserManagement].[ControlDependency] ([ControlDependencyId], [MasterControlId], [SlaveControlID], [MasterControlValue], [ComparatorId], [CreatedBy], [CreatedDate])
		VALUES (62, 139, 145, N'Managecompanylevelsettings', 1, @UserId, @Now)
	END

	IF NOT EXISTS(select top 1 1 from [UserManagement].[ControlDependency] where mastercontrolvalue = 'Managepropertylevelsettings')
	BEGIN
		INSERT [UserManagement].[ControlDependency] ([ControlDependencyId], [MasterControlId], [SlaveControlID], [MasterControlValue], [ComparatorId], [CreatedBy], [CreatedDate])
		VALUES (63, 139, 145, N'Managepropertylevelsettings', 1, @UserId, @Now)
	END

	IF NOT EXISTS(select top 1 1 from [UserManagement].[ControlDependency] where mastercontrolvalue = 'Viewallcompanylevelsettings')
	BEGIN
		INSERT [UserManagement].[ControlDependency] ([ControlDependencyId], [MasterControlId], [SlaveControlID], [MasterControlValue], [ComparatorId], [CreatedBy], [CreatedDate])
		VALUES (64, 139, 145, N'Viewallcompanylevelsettings', 1, @UserId, @Now)
	END

	IF NOT EXISTS(select top 1 1 from [UserManagement].[ControlDependency] where mastercontrolvalue = 'Viewallpropertylevelsettings')
	BEGIN
		INSERT [UserManagement].[ControlDependency] ([ControlDependencyId], [MasterControlId], [SlaveControlID], [MasterControlValue], [ComparatorId], [CreatedBy], [CreatedDate])
		VALUES (65, 139, 145, N'Viewallpropertylevelsettings', 1, @UserId, @Now)
	END

	IF NOT EXISTS(select top 1 1 from [UserManagement].[ControlDependency] where mastercontrolvalue = 'ManageSettingsTemplates')
	BEGIN
		INSERT [UserManagement].[ControlDependency] ([ControlDependencyId], [MasterControlId], [SlaveControlID], [MasterControlValue], [ComparatorId], [CreatedBy], [CreatedDate])
		VALUES (66, 139, 145, N'ManageSettingsTemplates', 1, @UserId, @Now)
	END

	IF NOT EXISTS(select top 1 1 from [UserManagement].[ControlDependency] where mastercontrolvalue = 'ViewUnifiedSettings')
	BEGIN
		INSERT [UserManagement].[ControlDependency] ([ControlDependencyId], [MasterControlId], [SlaveControlID], [MasterControlValue], [ComparatorId], [CreatedBy], [CreatedDate])
		VALUES (67, 139, 145, N'ViewUnifiedSettings', 1, @UserId, @Now)
	END

	IF NOT EXISTS(select top 1 1 from [UserManagement].[ControlDependency] where mastercontrolvalue = 'AbilitytoanswercompanylevelquestionnairesinCIMPL')
	BEGIN
		INSERT [UserManagement].[ControlDependency] ([ControlDependencyId], [MasterControlId], [SlaveControlID], [MasterControlValue], [ComparatorId], [CreatedBy], [CreatedDate])
		VALUES (68, 139, 145, N'AbilitytoanswercompanylevelquestionnairesinCIMPL', 1, @UserId, @Now)
	END

	IF NOT EXISTS(select top 1 1 from [UserManagement].[ControlDependency] where mastercontrolvalue = 'CIMPLESubmitQuestionnaires')
	BEGIN
		INSERT [UserManagement].[ControlDependency] ([ControlDependencyId], [MasterControlId], [SlaveControlID], [MasterControlValue], [ComparatorId], [CreatedBy], [CreatedDate])
		VALUES (69, 139, 145, N'CIMPLESubmitQuestionnaires', 1, @UserId, @Now)
	END

	IF NOT EXISTS(select top 1 1 from [UserManagement].[ControlDependency] where mastercontrolvalue = 'ManageCIMPLTemplates')
	BEGIN
		INSERT [UserManagement].[ControlDependency] ([ControlDependencyId], [MasterControlId], [SlaveControlID], [MasterControlValue], [ComparatorId], [CreatedBy], [CreatedDate])
		VALUES (70, 139, 145, N'ManageCIMPLTemplates', 1, @UserId, @Now)
	END

SET IDENTITY_INSERT [UserManagement].[ControlDependency] OFF
GO
--END: 305141-Display Top level Properties tab when the users role contains rights to Settings

--IB Product
/*This script is a sample script to create new prodcut in the system.*/

DECLARE @ProductId INT, 
		@LoginURI NVARCHAR(100), 
		@SigningCertificateThumbprint NVARCHAR(50), 
		@ParentProductTypeId INT, 
		@ProductName NVARCHAR(100)= 'Intelligent Building',  -- Produact Name
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
    WHERE Name = 'Intelligent Building'
)
    BEGIN
        EXEC [Enterprise].[CreateProductType] 
             @ProductTypeId = 113, -- Thsi value may change based on the root prodcut type
             @ParentProductTypeId = @ParentProductTypeId, 
             @Name = @ProductName, 
             @Description = @ProductName, 
             @ProductTypeGUID = 'D8DD4D6E-00F6-4453-8AF2-6EFF7E3F87B5'; -- Use newid() to generate new uniqueidentifier.
END;
SET @ProductId = 57; -- Assign new product Id

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
             @ProductGUID = 'D76B2F90-283F-4D58-937C-247A13A82D45', -- Use newid() to generate new uniqueidentifier.
             @Name = @ProductName, 
             @Description = @ProductName, 
             @ProductTypeId = 113;
        
		UPDATE Enterprise.Product
          SET 
              BooksProductCode = 'IB'
        WHERE ProductId = @ProductId;
END;

--The following block picks up all the detail frm Enterprise.ProductSettingType table
--To set up the product, bunch of these settings are required.
SET @apiendpoint = '';
Set @tokenEndPoint = '';
SET @apisecret = '';
IF @ServerName IN ('RCDUSODBSQL001')
BEGIN
	SET @apiendpoint = '';
	SET @tokenEndPoint = '';
	SET @apisecret = '';
END
IF @ServerName IN ('rctusodbsql001')
BEGIN
	SET @apiendpoint = '';
	SET @tokenEndPoint = '';
	SET @apisecret = '';
END
--IF @ServerName IN ('RCQUSODBSQL001')
--BEGIN
--	SET @apiendpoint = '';
--END
IF @ServerName IN ('RCPGBKDBSQL005A', 'RCPGBKDBSQL005B')
BEGIN
	SET @apiendpoint = '';
	SET @tokenEndPoint = '';
	SET @apisecret = '';
END
set nocount on
INSERT INTO @ProductConfiguration
(SettingName, 
 SettingDescription, 
 SettingValue
)
VALUES
 ('ClassName','','intelligentbuilding')
,('ProductUrl','','/product/intelligentbuilding')
,('TitleId','','Intelligent Building')
,('TitleUniqueId','','420A65CD-970B-498D-9CC8-8A861518EC78')
,('IsNewTab','','1')
,('MetatagUniqueId','','Intelligent Building')
,('IsResource','','0')
,('IsFavorite','','1')
,('LearnMore','','https://www.realpage.com/waste/')
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
,('CLIENTID','','bosswasteapi') -- For DEV Environment
,('TOKENENDPOINT','', '') -- For DEV Environment
,('APISECRET','', '')
,('AuthenticationType','Used to determine how to log into the product','Redirect')



SELECT * FROM @ProductConfiguration

SET @LoginURL = '';
IF @ServerName IN ('RCDUSODBSQL001')
BEGIN
	SET @LoginURL = 'www.dev-abctrash.realpage.com';
END
IF @ServerName IN ('rctusodbsql001')
BEGIN
	SET @LoginURL = '';
END
--IF @ServerName IN ('RCQUSODBSQL001')
--BEGIN
--	SET @LoginURL = '';
--END
IF @ServerName IN ('RCPGBKDBSQL005A', 'RCPGBKDBSQL005B')
BEGIN
	SET @LoginURL = '';
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
SET @RightName = 'Manage Intelligent Building Product Access'; -- Name of the right 
SET @RightDescription = 'Manage Intelligent Building Product Access'; --Description of the right as stated in story.
SET @RightShortName = 'ManageIntelligentBuilding'; --Short name of the right that is being used by the application
SET @ActionName = 'Manage Intelligent Building'; -- This specifically pertains to actions used for routing purposes. 
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
WHERE Name = 'Intelligent Building';

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
DECLARE @UserId bigint,
	@ProductId int ,
	@Now datetime = GETDATE()

SELECT	@UserId = UserId
FROM	Ident.UserLogin
WHERE	LoginName LIKE 'realpagead@%'

SELECT @ProductId = 57

IF NOT EXISTS (SELECT TOP 1 1 FROM[UserManagement].[ProductPage] WHERE ProductId = @ProductId)
BEGIN
            SET IDENTITY_INSERT [UserManagement].[Control] ON 
			--Parent
			INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
			VALUES (549, NULL, 8, N'IntelligentBuildingProductAccessTabGroupUIId', NULL, NULL, 1, @UserId, @Now)

			--Roles
			INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate]) 
			VALUES (550, 549, 9, N'IntelligentBuildingProductAccessRolesTabUIId', N'Roles', NULL, 2, @UserId, @Now)

			INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate]) 
			VALUES (551, 550, 2, N'IntelligentBuildingProductAccessRolesSelectGridUIId', NULL, NULL, 1, @UserId, @Now)

			INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate]) 
			VALUES (552, 551, 7, N'IntelligentBuildingProductAccessRadioUIId', NULL, N'isAssigned', 1, @UserId, @Now)

			INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate]) 
			VALUES (553, 551, 5, N'IntelligentBuildingProductAccessRoleLabelUIId', N'Role', N'name', 2, @UserId, @Now)

			INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate]) 
			VALUES (554, 551, 5, N'IntelligentBuildingProductAccessRoleTypeLabelUIId', N'Role Type', N'roletype', 3, @UserId, @Now)

			INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate]) 
			VALUES (555, 551, 11, N'IntelligentBuildingProductAccessIconUIId', NULL, N'InfoIcon', 4, @UserId, @Now)

			INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate]) 
			VALUES (556, 555, 5, N'IntelligentBuildingProductAccessRoleDetailsLabelUIId', N'Role Details', NULL, 1, @UserId, @Now)

			INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate]) 
			VALUES (557, 555, 12, N'IntelligentBuildingProductAccessGridUIId', NULL, NULL, 2, @UserId, @Now)

			INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate]) 
			VALUES (558, 557, 5, N'IntelligentBuildingProductAccessRightLabelUIId', N'Right', N'description', 1, @UserId, @Now)	
				
			SET IDENTITY_INSERT [UserManagement].[Control] OFF
              
            SET IDENTITY_INSERT [UserManagement].[ControlAttribute] ON 
            INSERT [UserManagement].[ControlAttribute] ([ControlAttributeId], [ControlId], [Key], [Value], [CreatedBy], [CreatedDate])
			VALUES (139, 550, N'Default', N'True', @UserId, @Now)

			INSERT [UserManagement].[ControlAttribute] ([ControlAttributeId], [ControlId], [Key], [Value], [CreatedBy], [CreatedDate]) 
			VALUES (140, 555, N'InfoIcon', N'Slide', @UserId, @Now)

            SET IDENTITY_INSERT [UserManagement].[ControlAttribute] OFF

            SET IDENTITY_INSERT [UserManagement].[ProductPage] ON 
              
			INSERT [UserManagement].[ProductPage] ([ProductPageId], [ProductId], [DisplayName], [CreatedBy], [CreatedDate], [IsActive])
			VALUES (34, 57, N'Intelligent Building Product Access', @UserId, @Now, 1)
  
            SET IDENTITY_INSERT [UserManagement].[ProductPage] OFF
              
            SET IDENTITY_INSERT [UserManagement].[ProductPageControl] ON 
            
			INSERT [UserManagement].[ProductPageControl] ([ProductPageControlId], [ProductPageId], [ControlId], [CreatedBy], [CreatedDate]) 
			VALUES (44, 34, 549, @UserId, @Now)
          
            SET IDENTITY_INSERT [UserManagement].[ProductPageControl] OFF

END
GO

DECLARE @ControlId int;
IF EXISTS(SELECT * FROM UserManagement.[Control] WHERE UIId='MarketingCenterProductAccessPropertyGroupTabUIId')
BEGIN
	SELECT @ControlId=ControlId FROM 
	UserManagement.[Control] WHERE UIId='MarketingCenterProductAccessPropertyGroupTabUIId'

    UPDATE UserManagement.[Control] SET ParentControlId=null WHERE ControlId=@ControlId

END
GO
--***************************************************************************************************************************************
--New Roles and Rights data migration
DECLARE @UserId bigint,
	@ProductId int ,
	@Now datetime = GETDATE()

SELECT	@UserId = UserId
FROM	Ident.UserLogin
WHERE	LoginName LIKE 'realpagead@%'

IF NOT EXISTS (SELECT 1 FROM [Security].[Route])
BEGIN
	SET IDENTITY_INSERT [Security].[Route] ON 

	INSERT [Security].[Route] ([RouteId], [RouteValue], [Description], [CreatedBy], [CreatedDate]) VALUES (1, N'ActivityLog', N'ActivityLog',@UserId, @Now)
	INSERT [Security].[Route] ([RouteId], [RouteValue], [Description], [CreatedBy], [CreatedDate]) VALUES (2, N'AddUser', N'AddUser',@UserId, @Now)
	INSERT [Security].[Route] ([RouteId], [RouteValue], [Description], [CreatedBy], [CreatedDate]) VALUES (3, N'CloneUser', N'CloneUser',@UserId, @Now)
	INSERT [Security].[Route] ([RouteId], [RouteValue], [Description], [CreatedBy], [CreatedDate]) VALUES (4, N'DashBoard', N'DashBoard',@UserId, @Now)
	INSERT [Security].[Route] ([RouteId], [RouteValue], [Description], [CreatedBy], [CreatedDate]) VALUES (5, N'EditUser', N'EditUser',@UserId, @Now)
	INSERT [Security].[Route] ([RouteId], [RouteValue], [Description], [CreatedBy], [CreatedDate]) VALUES (6, N'MigrationTool', N'MigrationTool',@UserId, @Now)
	INSERT [Security].[Route] ([RouteId], [RouteValue], [Description], [CreatedBy], [CreatedDate]) VALUES (7, N'RolesAndRights', N'RolesAndRights',@UserId, @Now)
	INSERT [Security].[Route] ([RouteId], [RouteValue], [Description], [CreatedBy], [CreatedDate]) VALUES (8, N'Settings', N'Settings',@UserId, @Now)
	INSERT [Security].[Route] ([RouteId], [RouteValue], [Description], [CreatedBy], [CreatedDate]) VALUES (9, N'SideMenu', N'SideMenu',@UserId, @Now)
	INSERT [Security].[Route] ([RouteId], [RouteValue], [Description], [CreatedBy], [CreatedDate]) VALUES (10, N'SupportTool', N'SupportTool',@UserId, @Now)
	INSERT [Security].[Route] ([RouteId], [RouteValue], [Description], [CreatedBy], [CreatedDate]) VALUES (11, N'Unified Settings', N'Unified Settings',@UserId, @Now)
	INSERT [Security].[Route] ([RouteId], [RouteValue], [Description], [CreatedBy], [CreatedDate]) VALUES (12, N'UsersList', N'UsersList',@UserId, @Now)

	SET IDENTITY_INSERT [Security].[Route] OFF
END

IF NOT EXISTS (SELECT 1 FROM [Security].[Right])
BEGIN
	SET IDENTITY_INSERT [Security].[Right] ON 

	  insert into Security.[Right](RightId,RightName,Description,Value,StatusTypeId,VisibilityStatusId,ProductId,TargetProductId,CreatedBy,CreatedDate)
	  SELECT RightValueTypeId
		  ,[ShortName]
		  ,[Description]
		  ,[Value]
		  ,[StatusTypeId]
		  ,[VisibilityStatusId]
		  ,[ProductId]
		  ,[TargetProductId]
		  ,@UserId
		  ,@Now
	  FROM [Enterprise].[RightValueType]
	  where ShortName is not null and LEN(LTRIM(rtrim(shortname))) > 0
	  and value not like 'Default%'
	  and value not in ('Access to L&R Conversion Utility for OneSite users','Manage Vendor Compliance Product Access','Access to Settings Admin for OneSite','Ability to edit password',
							'Ability to Configure Custom Fields for Users','Ability to view Company page','Ability to edit Company information','Ability to view Property page',
							'Ability to edit Property information','Access to Green Book Migration Tool','Access to Amenities Tool','Access to Employee Management',
							'Access to Identity Provider Configuration Page','Access to Leasing & Rents Conversion Tool','Access to Property Hierarchy Tool',
							'Impersonate a User','See All RealPage Products','Access to Client Portal','Manage all Unified Settings')
	--Accesstoclientportal needs to be removed
	 SET IDENTITY_INSERT [Security].[Right] OFF

	  IF NOT EXISTS(Select 1 From [Security].[Right] Where RightName = 'Dashboard')
	  BEGIN
		  insert into [Security].[Right](Value,RightName,StatusTypeId,Description,CreatedDate,VisibilityStatusId,ProductId,TargetProductId,CreatedBy)
		  select 'Dashboard','Dashboard',11,'Ability to view Dashboard page',GETUTCDATE(),10,3,3,@UserId
	  END
	  IF NOT EXISTS(Select 1 From [Security].[Right] Where RightName = 'People')
	  BEGIN
		insert into [Security].[Right](Value,RightName,StatusTypeId,Description,CreatedDate,VisibilityStatusId,ProductId,TargetProductId,CreatedBy)
		select 'People','People',11,'Ability to view People page',GETUTCDATE(),10,3,3,@UserId
	  END
	  IF NOT EXISTS(Select 1 From [Security].[Right] Where RightName = 'Products')
	  BEGIN
		insert into [Security].[Right](Value,RightName,StatusTypeId,Description,CreatedDate,VisibilityStatusId,ProductId,TargetProductId,CreatedBy)
		select 'Products','Products',11,'Ability to view Products page',GETUTCDATE(),10,3,3,@UserId
	  END
	  IF NOT EXISTS(Select 1 From [Security].[Right] Where RightName = 'Settings')
	  BEGIN
		insert into [Security].[Right](Value,RightName,StatusTypeId,Description,CreatedDate,VisibilityStatusId,ProductId,TargetProductId,CreatedBy)
		select 'Settings','Settings',11,'Ability to view Settings page',GETUTCDATE(),10,3,3,@UserId
	  END
  
	  
	  Update [Security].[Right] Set VisibilityStatusId = 10
	  Where Value IN ('Manage Notifications Configurations',
					  --'Ability to Import users',
					  'View only access to Unified Platform from Support Tool',
					  'Access to Settings Admin',
					  'Manage Settings Templates',
					  'Employee Access to Design Questionnaires in CIMPL',
					  'Employee Access to CIMPL',
					  'Employee Access to add Implementation Records in CIMPL',
					  'Employee Access to Vendor Marketplace',
					  'Manage Custom User fields settings',
					  'Manage Unified Platform Security Settings',
					  'Access to Unified Settings',
					  'Create platform alerts',
					  'Approve platform alerts',
					  'Access to Unified Platform via Support Tool',
					  'Manage help center administrator',
					  'Manage help center knowledge base',
					  'Manage help center videos',
					  'Manage help center online help',
					  'Manage help center product updates')
END

IF NOT EXISTS (SELECT 1 FROM [Security].[RightRoute])
BEGIN
	declare @routeid int
	Select @routeid = RouteId From [Security].[Route] Where RouteValue = 'SideMenu'

	--Side Menu
	insert into Security.RightRoute(RouteId,RightId,RightName,CreatedBy,CreatedDate)
	Select @routeid,RightId,Value,@userId,@now From [Security].[Right] Where Value = 'Dashboard'

	insert into Security.RightRoute(RouteId,RightId,RightName,CreatedBy,CreatedDate)
	Select @routeid,RightId,Value,@userId,@now From [Security].[Right] Where Value = 'Products'

	insert into Security.RightRoute(RouteId,RightId,RightName,CreatedBy,CreatedDate)
	Select @routeid,RightId,'People',@userId,@now From [Security].[Right] Where Value = 'Ability to view users'
	--Select @routeid,RightId,Value,@userId,@now From [Security].[Right] Where Value = 'People'

	insert into Security.RightRoute(RouteId,RightId,RightName,CreatedBy,CreatedDate)
	Select @routeid,RightId,Value,@userId,@now From [Security].[Right] Where Value = 'Settings'

	insert into Security.RightRoute(RouteId,RightId,RightName,CreatedBy,CreatedDate)
	Select @routeid,RightId,'Roles and rights',@userId,@now From [Security].[Right] Where Value = 'Ability to view roles and rights'

	insert into Security.RightRoute(RouteId,RightId,RightName,CreatedBy,CreatedDate)
	Select @routeid,RightId,Value,@userId,@now From [Security].[Right] Where Value = 'Access to Unified Settings'

	--insert into Security.RightRoute(RouteId,RightId,RightName,CreatedBy,CreatedDate)
	--Select @routeid,RightId,Value,@userId,@now From [Security].[Right] Where Value = 'Manage all Unified Settings'

	insert into Security.RightRoute(RouteId,RightId,RightName,CreatedBy,CreatedDate)
	Select @routeid,RightId,'Activity Log',@userId,@now From [Security].[Right] Where Value = 'Ability to view audit trail on user data'

	insert into Security.RightRoute(RouteId,RightId,RightName,CreatedBy,CreatedDate)
	Select @routeid,RightId,Value,@userId,@now From [Security].[Right] Where Value = 'Access Settings Admin'

	insert into Security.RightRoute(RouteId,RightId,RightName,CreatedBy,CreatedDate)
	Select @routeid,RightId,Value,@userId,@now From [Security].[Right] Where Value = 'Manage Settings Templates'

	insert into Security.RightRoute(RouteId,RightId,RightName,CreatedBy,CreatedDate)
	Select @routeid,RightId,'Manage Notifications',@userId,@now From [Security].[Right] Where Value = 'Manage Notifications Configurations'

	insert into Security.RightRoute(RouteId,RightId,RightName,CreatedBy,CreatedDate)
	Select @routeid,RightId,'Access HelpCenter',@userId,@now From [Security].[Right] Where Value = 'Access to Help Center'


	--Users List
	Select @routeid = RouteId From [Security].[Route] Where RouteValue = 'UsersList'
	insert into Security.RightRoute(RouteId,RightId,RightName,CreatedBy,CreatedDate)
	Select @routeid,RightId,'Activate Deactivate User',@userId,@now From [Security].[Right] Where Value = 'Ability to Activate/Deactivate User'

	insert into Security.RightRoute(RouteId,RightId,RightName,CreatedBy,CreatedDate)
	Select @routeid,RightId,'Clone User',@userId,@now From [Security].[Right] Where Value = 'Ability to clone users (all products)'

	insert into Security.RightRoute(RouteId,RightId,RightName,CreatedBy,CreatedDate)
	Select @routeid,RightId,'Create User',@userId,@now From [Security].[Right] Where Value = 'Ability to create user details'

	insert into Security.RightRoute(RouteId,RightId,RightName,CreatedBy,CreatedDate)
	Select @routeid,RightId,'Edit User',@userId,@now From [Security].[Right] Where Value = 'Ability to edit User Details'

	insert into Security.RightRoute(RouteId,RightId,RightName,CreatedBy,CreatedDate)
	Select @routeid,RightId,'Lock/Unlock User',@userId,@now From [Security].[Right] Where Value = 'Ability to lock/unlock users'

	insert into Security.RightRoute(RouteId,RightId,RightName,CreatedBy,CreatedDate)
	Select @routeid,RightId,'Resend Invitation',@userId,@now From [Security].[Right] Where Value = 'Ability to Resend Invite'

	insert into Security.RightRoute(RouteId,RightId,RightName,CreatedBy,CreatedDate)
	Select @routeid,RightId,'View User',@userId,@now From [Security].[Right] Where Value = 'Ability to view users'

	insert into Security.RightRoute(RouteId,RightId,RightName,CreatedBy,CreatedDate)
	Select @routeid,RightId,'Import users',@userId,@now From [Security].[Right] Where Value = 'Ability to Import users'

	--Edit User
	Select @routeid = RouteId From [Security].[Route] Where RouteValue = 'EditUser'

	insert into Security.RightRoute(RouteId,RightId,RightName,CreatedBy,CreatedDate)
	Select @routeid,RightId,'Activate Deactivate User',@userId,@now From [Security].[Right] Where Value = 'Ability to Activate/Deactivate User'

	insert into Security.RightRoute(RouteId,RightId,RightName,CreatedBy,CreatedDate)
	Select @routeid,RightId,'Edit Other User Profile',@userId,@now From [Security].[Right] Where Value = 'Ability to edit profile of other users'

	insert into Security.RightRoute(RouteId,RightId,RightName,CreatedBy,CreatedDate)
	Select @routeid,RightId,'Edit Own Profile',@userId,@now From [Security].[Right] Where Value = 'Ability to edit my own profile'

	insert into Security.RightRoute(RouteId,RightId,RightName,CreatedBy,CreatedDate)
	Select @routeid,RightId,'Manage Resident Portal Users',@userId,@now From [Security].[Right] Where Value = 'Manage Resident Portals Product Access'

	insert into Security.RightRoute(RouteId,RightId,RightName,CreatedBy,CreatedDate)
	Select @routeid,RightId,'View Audit Trail User Data',@userId,@now From [Security].[Right] Where Value = 'Ability to view audit trail on user data'

	insert into Security.RightRoute(RouteId,RightId,RightName,CreatedBy,CreatedDate)
	Select @routeid,RightId,'Resend Invitation',@userId,@now From [Security].[Right] Where Value = 'Ability to Resend Invite'

	insert into Security.RightRoute(RouteId,RightId,RightName,CreatedBy,CreatedDate)
	Select @routeid,RightId,'View User',@userId,@now From [Security].[Right] Where Value = 'Ability to view users'


	--Add User
	Select @routeid = RouteId From [Security].[Route] Where RouteValue = 'AddUser'

	insert into Security.RightRoute(RouteId,RightId,RightName,CreatedBy,CreatedDate)
	Select @routeid,RightId,'Activate Deactivate User',@userId,@now From [Security].[Right] Where Value = 'Ability to Activate/Deactivate User'

	insert into Security.RightRoute(RouteId,RightId,RightName,CreatedBy,CreatedDate)
	Select @routeid,RightId,'Create User',@userId,@now From [Security].[Right] Where Value = 'Ability to create user details'

	insert into Security.RightRoute(RouteId,RightId,RightName,CreatedBy,CreatedDate)
	Select @routeid,RightId,'Manage Resident Portal Users',@userId,@now From [Security].[Right] Where Value = 'Manage Resident Portals Product Access'


	--Clone User
	Select @routeid = RouteId From [Security].[Route] Where RouteValue = 'CloneUser'

	insert into Security.RightRoute(RouteId,RightId,RightName,CreatedBy,CreatedDate)
	Select @routeid,RightId,'Edit User',@userId,@now From [Security].[Right] Where Value = 'Ability to edit User Details'


	--RolesAndRights
	Select @routeid = RouteId From [Security].[Route] Where RouteValue = 'RolesAndRights'

	insert into Security.RightRoute(RouteId,RightId,RightName,CreatedBy,CreatedDate)
	Select @routeid,RightId,'Manage Role Right',@userId,@now From [Security].[Right] Where Value = 'Ability to manage roles and rights'

	insert into Security.RightRoute(RouteId,RightId,RightName,CreatedBy,CreatedDate)
	Select @routeid,RightId,'View Role Right',@userId,@now From [Security].[Right] Where Value = 'Ability to view roles and rights'

	--ActivityLog
	Select @routeid = RouteId From [Security].[Route] Where RouteValue = 'ActivityLog'

	insert into Security.RightRoute(RouteId,RightId,RightName,CreatedBy,CreatedDate)
	Select @routeid,RightId,'View Audit Trail',@userId,@now From [Security].[Right] Where Value = 'Ability to view audit trail on user data'

	--SupportTool
	Select @routeid = RouteId From [Security].[Route] Where RouteValue = 'SupportTool'

	insert into Security.RightRoute(RouteId,RightId,RightName,CreatedBy,CreatedDate)
	Select @routeid,RightId,'Access to Unified Platform',@userId,@now From [Security].[Right] Where Value = 'Access to Unified Platform via Support Tool'

	insert into Security.RightRoute(RouteId,RightId,RightName,CreatedBy,CreatedDate)
	Select @routeid,RightId,'View Only Support Tool Access',@userId,@now From [Security].[Right] Where Value = 'View only access to Unified Platform from Support Tool'

END

IF NOT EXISTS (SELECT 1 FROM [Security].[RoleType])
BEGIN
	SET IDENTITY_INSERT [Security].[RoleType] ON
	Insert Into [Security].[RoleType] (RoleTypeId,ParentRoleTypeId, Value,Description,CreatedBy,CreatedDate)
	Select 1,NULL, 'System','System Role',@UserId,@now

	Insert Into [Security].[RoleType] (RoleTypeId,ParentRoleTypeId,Value,Description,CreatedBy,CreatedDate)
	Select 2,NULL,'Custom','Custom Role',@UserId,@now

	Insert Into [Security].[RoleType] (RoleTypeId,ParentRoleTypeId,Value,Description,CreatedBy,CreatedDate)
	Select 3,NULL,'Product','Product Role',@UserId,@now
	SET IDENTITY_INSERT [Security].[RoleType] OFF
END

IF NOT EXISTS (SELECT 1 FROM [Security].[Role])
BEGIN
	SET IDENTITY_INSERT [Security].[Role] ON 
	Insert Into [Security].[Role] (RoleId, RoleName,Description,ShortName,RoleTypeID,OrgPartyID,ProductId,CreatedBy,CreatedDate)
	Select 1,'User Administrator','User Administrator','SuperUser',1,NULL,3,@userId,@now

	Insert Into [Security].[Role] (RoleId,RoleName,Description,ShortName,RoleTypeID,OrgPartyID,ProductId,CreatedBy,CreatedDate)
	Select 2,'Basic End User','Basic End User','User',1,NULL,3,@userId,@now

	Insert Into [Security].[Role] (RoleId,RoleName,Description,ShortName,RoleTypeID,OrgPartyID,ProductId,CreatedBy,CreatedDate)
	Select 3,'Read only for Unified Platform','Read only for Unified Platform','ROForUnifiedPlatform',1,NULL,3,@userId,@now

	 --custom roles for product 3
	  INSERT INTO [Security].[Role](RoleId,RoleName,ShortName,Description,RoleTypeID,OrgPartyID,ProductId,CreatedBy,CreatedDate)
	  SELECT r.RoleID ,RV.Value,RV.ShortName,RV.Description,2,r.PartyID,3,@userId,@now
	  From [Enterprise].[Role] R
		JOIN [Enterprise].[RoleValueType] RV ON
			R.RoleValueTypeId = RV.RoleValueTypeId
	    Join Enterprise.Organization O ON
			o.PartyId = R.PartyID
	  where RoleTypeID IN (400,402)
	  and RV.Value  not in ('SystemRight','User Administrator','Basic End User','Read only for Unified Platform')	 
	  and rv.StatusTypeId = 14

	  --Basic End User & CIMPL product 3
	   INSERT INTO [Security].[Role](RoleId,RoleName,ShortName,Description,RoleTypeID,OrgPartyID,ProductId,CreatedBy,CreatedDate)
	  SELECT r.RoleID ,RV.Value,RV.ShortName,RV.Description,2,r.PartyID,3,@userId,@now
	  From [Enterprise].[Role] R
		JOIN [Enterprise].[RoleValueType] RV ON
		R.RoleValueTypeId = RV.RoleValueTypeId
	  where RoleTypeID IN (400,402)
	  and RV.Value  not in ('SystemRight','User Administrator','Basic End User','Read only for Unified Platform')	 
	  and rv.StatusTypeId = 13

	  --product role
	  INSERT INTO [Security].[Role](RoleId,RoleName,ShortName,Description,RoleTypeID,OrgPartyID,ProductId,CreatedBy,CreatedDate)
	  SELECT r.RoleID ,RV.Value,RV.ShortName,RV.Description,1,r.PartyID,24,@userId,@now
	  From [Enterprise].[Role] R
		JOIN [Enterprise].[RoleValueType] RV ON
		R.RoleValueTypeId = RV.RoleValueTypeId
	  where RoleTypeID  = 500
	  and rv.Value = 'Black-Book Director'
	  and rv.StatusTypeId = 13

	  INSERT INTO [Security].[Role](RoleId,RoleName,ShortName,Description,RoleTypeID,OrgPartyID,ProductId,CreatedBy,CreatedDate)
	  SELECT r.RoleID ,RV.Value,RV.ShortName,RV.Description,1,r.PartyID,24,@userId,@now
	  From [Enterprise].[Role] R
		JOIN [Enterprise].[RoleValueType] RV ON
		R.RoleValueTypeId = RV.RoleValueTypeId
	  where RoleTypeID = 500
	   and rv.Value = 'CSM Read-Only'
	  and rv.StatusTypeId = 13

	  INSERT INTO [Security].[Role](RoleId,RoleName,ShortName,Description,RoleTypeID,OrgPartyID,ProductId,CreatedBy,CreatedDate)
	  SELECT r.RoleID ,RV.Value,RV.ShortName,RV.Description,1,r.PartyID,24,@userId,@now
	  From [Enterprise].[Role] R
		JOIN [Enterprise].[RoleValueType] RV ON
		R.RoleValueTypeId = RV.RoleValueTypeId
	  where RoleTypeID = 500	 
	  and rv.Value = 'Executive'
	  and rv.StatusTypeId = 13

	  INSERT INTO [Security].[Role](RoleId,RoleName,ShortName,Description,RoleTypeID,OrgPartyID,ProductId,CreatedBy,CreatedDate)
	  SELECT r.RoleID ,RV.Value,RV.ShortName,RV.Description,1,r.PartyID,24,@userId,@now
	  From [Enterprise].[Role] R
		JOIN [Enterprise].[RoleValueType] RV ON
		R.RoleValueTypeId = RV.RoleValueTypeId
	  where RoleTypeID = 500 
	  and rv.Value = 'Implementation'
	  and rv.StatusTypeId = 13

	  INSERT INTO [Security].[Role](RoleId,RoleName,ShortName,Description,RoleTypeID,OrgPartyID,ProductId,CreatedBy,CreatedDate)
	  SELECT r.RoleID ,RV.Value,RV.ShortName,RV.Description,1,r.PartyID,24,@userId,@now
	  From [Enterprise].[Role] R
		JOIN [Enterprise].[RoleValueType] RV ON
		R.RoleValueTypeId = RV.RoleValueTypeId
	  where RoleTypeID = 500
	  and rv.Value = 'Read Only Research'
	  and rv.StatusTypeId = 13
  
	  INSERT INTO [Security].[Role](RoleId,RoleName,ShortName,Description,RoleTypeID,OrgPartyID,ProductId,CreatedBy,CreatedDate)
	  SELECT r.RoleID ,RV.Value,RV.ShortName,RV.Description,1,r.PartyID,24,@userId,@now
	  From [Enterprise].[Role] R
		JOIN [Enterprise].[RoleValueType] RV ON
		R.RoleValueTypeId = RV.RoleValueTypeId
	  where RoleTypeID = 500	  
	  and rv.Value = 'Research Analyst'
	  and rv.StatusTypeId = 13

	  INSERT INTO [Security].[Role](RoleId,RoleName,ShortName,Description,RoleTypeID,OrgPartyID,ProductId,CreatedBy,CreatedDate)
	  SELECT r.RoleID ,RV.Value,RV.ShortName,RV.Description,1,r.PartyID,24,@userId,@now
	  From [Enterprise].[Role] R
		JOIN [Enterprise].[RoleValueType] RV ON
		R.RoleValueTypeId = RV.RoleValueTypeId
	  where RoleTypeID = 500	  
	  and rv.Value = 'Research Manager'
	  and rv.StatusTypeId = 13

	  INSERT INTO [Security].[Role](RoleId,RoleName,ShortName,Description,RoleTypeID,OrgPartyID,ProductId,CreatedBy,CreatedDate)
	  SELECT r.RoleID ,RV.Value,RV.ShortName,RV.Description,1,r.PartyID,24,@userId,@now
	  From [Enterprise].[Role] R
		JOIN [Enterprise].[RoleValueType] RV ON
		R.RoleValueTypeId = RV.RoleValueTypeId
	  where RoleTypeID = 500 
	  and rv.Value = 'Research QA'
	  and rv.StatusTypeId = 13

	  INSERT INTO [Security].[Role](RoleId,RoleName,ShortName,Description,RoleTypeID,OrgPartyID,ProductId,CreatedBy,CreatedDate)
	  SELECT r.RoleID ,RV.Value,RV.ShortName,RV.Description,1,r.PartyID,24,@userId,@now
	  From [Enterprise].[Role] R
		JOIN [Enterprise].[RoleValueType] RV ON
		R.RoleValueTypeId = RV.RoleValueTypeId
	  where RoleTypeID = 500 
	  and rv.Value = 'White Space'
	  and rv.StatusTypeId = 13

	  INSERT INTO [Security].[Role](RoleId,RoleName,ShortName,Description,RoleTypeID,OrgPartyID,ProductId,CreatedBy,CreatedDate)
	  SELECT r.RoleID ,RV.Value,RV.ShortName,RV.Description,1,r.PartyID,24,@userId,@now
	  From [Enterprise].[Role] R
		JOIN [Enterprise].[RoleValueType] RV ON
		R.RoleValueTypeId = RV.RoleValueTypeId
	  where RoleTypeID = 500	  
	  and rv.Value = 'White Space Campaign SME'
	  and rv.StatusTypeId = 13

	SET IDENTITY_INSERT [Security].[Role] OFF

	Insert Into Security.Role(RoleName,ShortName,Description,RoleTypeID,OrgPartyID,ProductId,CreatedBy,CreatedDate)
	SELECT 'Manage Amenity Status','Amenity.Status','Ability to inactivate master amenities',1,NULL,26,@userId,GETDATE()

	Insert Into Security.Role(RoleName,ShortName,Description,RoleTypeID,OrgPartyID,ProductId,CreatedBy,CreatedDate)
	SELECT 'Manage Amenity No Pricing','Amenity.No.Pricing','Create and update master amenity marketing content but not pricing',1,NULL,26,@userId,GETDATE()

	Insert Into Security.Role(RoleName,ShortName,Description,RoleTypeID,OrgPartyID,ProductId,CreatedBy,CreatedDate)
	SELECT 'Manage Amenity With Pricing','Amenity.With.Pricing','Create and update master amenity pricing content',1,NULL,26,@userId,GETDATE()

	Insert Into Security.Role(RoleName,ShortName,Description,RoleTypeID,OrgPartyID,ProductId,CreatedBy,CreatedDate)
	SELECT 'Manage Property Amenity No Pricing','Prop.Amenity.No.Pricing','Create and update property amenity marketing content but not pricing',1,NULL,26,@userId,GETDATE()

	Insert Into Security.Role(RoleName,ShortName,Description,RoleTypeID,OrgPartyID,ProductId,CreatedBy,CreatedDate)
	SELECT 'Manage Property Amenity With Pricing','Prop.Amenity.With.Pricing','Create and update property amenity pricing',1,NULL,26,@userId,GETDATE()

	Insert Into Security.Role(RoleName,ShortName,Description,RoleTypeID,OrgPartyID,ProductId,CreatedBy,CreatedDate)
	SELECT 'View Amenities','View.Amenities','View master amenities (default access to application - only allows users to view (read-only) amenities list)',1,NULL,26,@userId,GETDATE()
END

IF NOT EXISTS (SELECT 1 FROM [Security].[RoleRight])
BEGIN
	Declare @adminRole int,@basicUserRole int,@ROUserRole int
	Declare @UAProductRole1 int,@UAProductRole2 int,@UAProductRole3 int,@UAProductRole4 int,@UAProductRole5 int,@UAProductRole6 int
	DECLARE @dashBoardRightId int;	
	DECLARE @peopleRightId int;	
	DECLARE @settingsRightId int;

	  Select @adminRole = RoleId from [Security].Role where ShortName = 'SuperUser'
	  Select @basicUserRole = RoleId from [Security].Role where ShortName = 'User'
	  Select @ROUserRole = RoleId from [Security].Role where ShortName = 'ROForUnifiedPlatform'
	  Select @UAProductRole1 = RoleId from [Security].Role where ShortName = 'Amenity.Status'
	  Select @UAProductRole2 = RoleId from [Security].Role where ShortName = 'Amenity.No.Pricing'
	  Select @UAProductRole3 = RoleId from [Security].Role where ShortName = 'Amenity.With.Pricing'
	  Select @UAProductRole4 = RoleId from [Security].Role where ShortName = 'Prop.Amenity.No.Pricing'
	  Select @UAProductRole5 = RoleId from [Security].Role where ShortName = 'Prop.Amenity.With.Pricing'
	  Select @UAProductRole6 = RoleId from [Security].Role where ShortName = 'View.Amenities'
	  Select @dashBoardRightId = [RightId] from Security.[Right] Where Value = 'Dashboard'
	  Select @peopleRightId = [RightId] from Security.[Right] Where Value = 'People'
	  Select @settingsRightId = [RightId] from Security.[Right] Where Value = 'Settings'

	  Insert into [Security].RoleRight(RoleId,RightId,CreatedBy,CreatedDate)
	  Select @adminRole,RightId,@UserId,@Now  from [Security].[Right]
	  where   productid = 3 
	  and Value NOT IN ('View only access to Unified Platform from Support Tool','Approve platform alerts','Create platform alerts','Manage Notifications Configurations')
	

	  --Basic user role
	  Insert into [Security].RoleRight(RoleId,RightId,CreatedBy,CreatedDate)
	  Select @basicUserRole,RightId,@UserId,@Now From [Security].[Right] 
	  Where ProductId = 3 
	  And Value  in ('Access to Vendor Marketplace','Ability to edit my own profile','Access to Product Learning Portal','Dashboard','Products','People')

	  --Read only Role
	  Insert into [Security].RoleRight(RoleId,RightId,CreatedBy,CreatedDate)
	  Select @ROUserRole,RightId,@UserId,@Now From [Security].[Right] 
	  Where ProductId = 3 
	  And Value  in ('Ability to edit my own profile','Ability to view audit trail on user data','Ability to view roles and rights',
	  'Ability to view users','Access to Product Learning Portal','Access to Vendor Marketplace','Dashboard','Products','People')

	  --All
	  Insert into [Security].RoleRight(RoleId,RightId,CreatedBy,CreatedDate)
	  SELECT R.RoleId,SR.RightId,@UserId,@Now
	  FROM [Security].[Role] R
	  Join [Enterprise].[Right] RI On
		R.RoleId = RI.RoleID and
		R.OrgPartyID = RI.PartyId
	  Join [Security].[Right] SR On
		RI.RightValueTypeId = SR.RightId
	  Where  R.RoleId NOT IN (@adminRole,@basicUserRole,@ROUserRole)

	  --dashboard
	   Insert into [Security].RoleRight(RoleId,RightId,CreatedBy,CreatedDate)
	  SELECT Distinct R.RoleId,@dashBoardRightId,@UserId,@Now
	  FROM [Security].[Role] R
	  Join [Enterprise].[Right] RI On
		R.RoleId = RI.RoleID and
		R.OrgPartyID = RI.PartyId
	  Join [Security].[Right] SR On
		RI.RightValueTypeId = SR.RightId
	   Where  R.RoleId NOT IN (@adminRole,@basicUserRole,@ROUserRole)
	 --people
	  Insert into [Security].RoleRight(RoleId,RightId,CreatedBy,CreatedDate)
	  SELECT Distinct R.RoleId,@peopleRightId,@UserId,@Now
	  FROM [Security].[Role] R
	  Join [Enterprise].[Right] RI On
		R.RoleId = RI.RoleID and
		R.OrgPartyID = RI.PartyId
	  Join [Security].[Right] SR On
		RI.RightValueTypeId = SR.RightId
	   Where  R.RoleId NOT IN (@adminRole,@basicUserRole,@ROUserRole)
	  and sr.Value = 'Ability to view users'
--settings
	  Insert into [Security].RoleRight(RoleId,RightId,CreatedBy,CreatedDate)
	  SELECT Distinct R.RoleId,@settingsRightId,@UserId,@Now
	  FROM [Security].[Role] R
	  Join [Enterprise].[Right] RI On
		R.RoleId = RI.RoleID and
		R.OrgPartyID = RI.PartyId
	  Join [Security].[Right] SR On
		RI.RightValueTypeId = SR.RightId
	   Where  R.RoleId NOT IN (@adminRole,@basicUserRole,@ROUserRole)
	  and (sr.RightName = 'ViewUnifiedSettings' or sr.RightName = 'ManageCustomFields' or sr.RightName ='AccessSettingsAdmin')

	  --UA Product Role and rights mapping
	  Insert into [Security].RoleRight(RoleId,RightId,CreatedBy,CreatedDate)
	  Select @UAProductRole1,RightId,@UserId,@Now
	  From [Security].[Right]
	  Where ProductId = 26
	  AND RightName IN ('assign.unit','unassign.unit','assign.floorplan','unassign.floorplan', 'ca.master.view','ca.property.view', 'fpu.master.view','fpu.property.view','activity.view')

	  Insert into [Security].RoleRight(RoleId,RightId,CreatedBy,CreatedDate)
	  Select @UAProductRole2,RightId,@UserId,@Now
	  From [Security].[Right]
	  Where ProductId = 26
	  AND RightName IN ('assign.unit','unassign.unit','assign.floorplan','unassign.floorplan','ca.add','ca.edit','ca.delete','fpu.add','fpu.edit','fpu.delete',
		'ca.merge','fpu.merge','ca.export','fpu.export','assignto.prop','prop.ca.add','prop.ca.edit','prop.ca.delete', 'prop.fpu.add','prop.fpu.edit',
		'prop.fpu.delete','prop.ca.merge','prop.fpu.merge','prop.ca.export','prop.fpu.export','ca.master.view','ca.property.view','fpu.master.view','fpu.property.view','activity.view')

	 Insert into [Security].RoleRight(RoleId,RightId,CreatedBy,CreatedDate)
	 Select @UAProductRole3,RightId,@UserId,@Now
	 From [Security].[Right] Where ProductId = 26 and VisibilityStatusId = 9

	 Insert into [Security].RoleRight(RoleId,RightId,CreatedBy,CreatedDate)
	  Select @UAProductRole4,RightId,@UserId,@Now
	  From [Security].[Right]
	  Where ProductId = 26
	  AND RightName IN ('assign.unit','unassign.unit','assign.floorplan','unassign.floorplan','prop.ca.add','prop.ca.edit','prop.ca.delete',
		   'prop.fpu.add','prop.fpu.edit','prop.fpu.delete','prop.ca.merge','prop.fpu.merge','prop.ca.export','prop.fpu.export',
		   'ca.master.view','ca.property.view','fpu.master.view','fpu.property.view','activity.view')

	Insert into [Security].RoleRight(RoleId,RightId,CreatedBy,CreatedDate)
	  Select @UAProductRole5,RightId,@UserId,@Now
	  From [Security].[Right]
	  Where ProductId = 26
	  AND RightName IN ('assign.unit','unassign.unit','assign.floorplan','unassign.floorplan','prop.ca.add','prop.ca.edit','prop.ca.delete','prop.fpu.add',
			'prop.fpu.edit','prop.fpu.delete','prop.ca.merge','prop.fpu.merge','prop.ca.export','prop.fpu.export', 'price.point.add', 'price.point.edit',
			 'price.point.delete', 'depreciation.add','depreciation.edit','ca.master.view','ca.property.view','fpu.master.view','fpu.property.view','activity.view')

	  Insert into [Security].RoleRight(RoleId,RightId,CreatedBy,CreatedDate)
	  Select @UAProductRole6,RightId,@UserId,@Now
	  From [Security].[Right]
	  Where ProductId = 26
	  AND RightName IN ('ca.master.view','ca.property.view', 'fpu.master.view','fpu.property.view','activity.view')
END

IF NOT EXISTS (SELECT 1 FROM [Security].[OrganizationDefaultRole])
BEGIN
	  Declare @basic1RoleID int,@UAPRole int
	  Select @basic1RoleID = RoleId From [Security].[Role] Where ShortName = 'User'
	  Select @UAPRole = RoleId from [Security].Role where ShortName = 'View.Amenities'
	  -- --Default Role
	  INSERT INTO [Security].[OrganizationDefaultRole](RoleId,OrgPartyId,CreatedBy,CreatedDate)
	  SELECT @basic1RoleID ,r.PartyID,@userId,@now
	   from [Enterprise].[Role] R 
	   JOIN [Enterprise].[RoleValueType] RV ON
			R.RoleValueTypeId = RV.RoleValueTypeId	
	  where Rv.value  = 'Basic End User' 
	  And DefaultRole = 1

	   INSERT INTO [Security].[OrganizationDefaultRole](RoleId,OrgPartyId,CreatedBy,CreatedDate)
	  SELECT @basic1RoleID ,r.PartyID,@userId,@now
	   from [Enterprise].[Role] R 
	   JOIN [Enterprise].[RoleValueType] RV ON
			R.RoleValueTypeId = RV.RoleValueTypeId	
	  where Rv.value  = 'Read only for Unified Platform' 
	  And DefaultRole = 1

	  INSERT INTO [Security].[OrganizationDefaultRole](RoleId,OrgPartyId,CreatedBy,CreatedDate)
	  Select R.RoleID, PartyID ,@userId,@now 
	  from [Enterprise].[Role] R 
	   JOIN [Enterprise].[RoleValueType] RV ON
			R.RoleValueTypeId = RV.RoleValueTypeId	
	 where Rv.value  not in ('Basic End User','Read only for Unified Platform' ,'View Amenities')
	  And DefaultRole = 1

	   --UA product default
	  INSERT INTO [Security].[OrganizationDefaultRole](RoleId,OrgPartyId,CreatedBy,CreatedDate)
	  Select @UAPRole, o.PartyId ,@userId,@now from Enterprise.Organization o
	  join Enterprise.OrganizationProduct op on
		o.PartyId = op.PartyId
	  and ProductId = 26 
	  and ThruDate is null
END

IF NOT EXISTS (SELECT 1 FROM [Security].[PersonaRole])
BEGIN
	Declare @UARole1 int,@UARole2 int,@UARole3 int,@UARole4 int,@UARole5 int,@UARole6 int
	Declare @adminRoleID int,@basicRoleID int,@RORoleID int
    Select @adminRoleID = RoleId From [Security].[Role] Where ShortName = 'SuperUser'	 
	Select @basicRoleID = RoleId From [Security].[Role] Where ShortName = 'User'	
	Select @RORoleID = RoleId From [Security].[Role] Where ShortName = 'ROForUnifiedPlatform'
	Select @UARole1 = RoleId from [Security].Role where ShortName = 'Amenity.Status'
	Select @UARole2 = RoleId from [Security].Role where ShortName = 'Amenity.No.Pricing'
	Select @UARole3 = RoleId from [Security].Role where ShortName = 'Amenity.With.Pricing'
	Select @UARole4 = RoleId from [Security].Role where ShortName = 'Prop.Amenity.No.Pricing'
	Select @UARole5 = RoleId from [Security].Role where ShortName = 'Prop.Amenity.With.Pricing'
	Select @UARole6 = RoleId from [Security].Role where ShortName = 'View.Amenities'
	--admin user
	INSERT INTO [Security].[PersonaRole] (PersonaId,RoleId,FromDate,ThruDate,CreatedBy,CreatedDate) 
	Select PersonaId,@adminRoleID,FromDate,ThruDate,@userId,@now 
	from [Enterprise].[PersonaPrivilege] P
	Join [Enterprise].[Role] R On
		R.RoleID = P.RoleID
	Join  [Enterprise].[RoleValueType] RV ON
		RV.RoleValueTypeId = R.RoleValueTypeId
	Where RV.Value = 'User Administrator'

  --Basic user Role
	
	INSERT INTO [Security].[PersonaRole] (PersonaId,RoleId,FromDate,ThruDate,CreatedBy,CreatedDate)  
	Select PersonaId,@basicRoleID,FromDate,ThruDate,@userId,@now 
	from [Enterprise].[PersonaPrivilege] P
	Join [Enterprise].[Role] R On
		R.RoleID = P.RoleID
	Join  [Enterprise].[RoleValueType] RV ON
		RV.RoleValueTypeId = R.RoleValueTypeId
	Where RV.Value = 'Basic End User'
	and PersonaId is not null

	--Read only user Role
	
	INSERT INTO [Security].[PersonaRole] (PersonaId,RoleId,FromDate,ThruDate,CreatedBy,CreatedDate)  
	Select PersonaId,@basicRoleID,FromDate,ThruDate,@userId,@now 
	from [Enterprise].[PersonaPrivilege] P
	Join [Enterprise].[Role] R On
		R.RoleID = P.RoleID
	Join  [Enterprise].[RoleValueType] RV ON
		RV.RoleValueTypeId = R.RoleValueTypeId
	Where RV.Value ='ROForUnifiedPlatform'
	and PersonaId is not null

	  --all other Role
	  INSERT INTO [Security].[PersonaRole] (PersonaId,RoleId,FromDate,ThruDate,CreatedBy,CreatedDate)  
	  Select PersonaId,P.RoleID,FromDate,ThruDate,@userId,@now 
	  from [Enterprise].[PersonaPrivilege] P
		Join [Enterprise].[Role] R On
			R.RoleID = P.RoleID
		Join [Enterprise].[RoleValueType] RV ON
			R.RoleValueTypeId = RV.RoleValueTypeId
		join [Security].[Role] SR on
			r.RoleID = sr.RoleId
	  Where  RV.Value  not in ('User Administrator','Basic End User','Read only for Unified Platform','Manage Amenity Status','Manage Amenity No Pricing','Manage Amenity With Pricing',
	  'Manage Property Amenity No Pricing','Manage Property Amenity With Pricing','View Amenities')	
	  and PersonaId is not null

	INSERT INTO [Security].[PersonaRole] (PersonaId,RoleId,FromDate,ThruDate,CreatedBy,CreatedDate)  
	Select PersonaId,@UARole1,FromDate,ThruDate,@userId,@now 
	from [Enterprise].[PersonaPrivilege] P
	Join [Enterprise].[Role] R On
		R.RoleID = P.RoleID
	Join  [Enterprise].[RoleValueType] RV ON
		RV.RoleValueTypeId = R.RoleValueTypeId
	Where RV.Value ='Manage Amenity Status'
	and PersonaId is not null

	INSERT INTO [Security].[PersonaRole] (PersonaId,RoleId,FromDate,ThruDate,CreatedBy,CreatedDate)  
	Select PersonaId,@UARole1,FromDate,ThruDate,@userId,@now 
	from [Enterprise].[PersonaPrivilege] P
	Join [Enterprise].[Role] R On
		R.RoleID = P.RoleID
	Join  [Enterprise].[RoleValueType] RV ON
		RV.RoleValueTypeId = R.RoleValueTypeId
	Where RV.Value ='Manage Amenity No Pricing'
	and PersonaId is not null

	INSERT INTO [Security].[PersonaRole] (PersonaId,RoleId,FromDate,ThruDate,CreatedBy,CreatedDate)  
	Select PersonaId,@UARole1,FromDate,ThruDate,@userId,@now 
	from [Enterprise].[PersonaPrivilege] P
	Join [Enterprise].[Role] R On
		R.RoleID = P.RoleID
	Join  [Enterprise].[RoleValueType] RV ON
		RV.RoleValueTypeId = R.RoleValueTypeId
	Where RV.Value ='Manage Amenity With Pricing'
	and PersonaId is not null

	INSERT INTO [Security].[PersonaRole] (PersonaId,RoleId,FromDate,ThruDate,CreatedBy,CreatedDate)  
	Select PersonaId,@UARole1,FromDate,ThruDate,@userId,@now 
	from [Enterprise].[PersonaPrivilege] P
	Join [Enterprise].[Role] R On
		R.RoleID = P.RoleID
	Join  [Enterprise].[RoleValueType] RV ON
		RV.RoleValueTypeId = R.RoleValueTypeId
	Where RV.Value ='Manage Property Amenity No Pricing'
	and PersonaId is not null

	INSERT INTO [Security].[PersonaRole] (PersonaId,RoleId,FromDate,ThruDate,CreatedBy,CreatedDate)  
	Select PersonaId,@UARole1,FromDate,ThruDate,@userId,@now 
	from [Enterprise].[PersonaPrivilege] P
	Join [Enterprise].[Role] R On
		R.RoleID = P.RoleID
	Join  [Enterprise].[RoleValueType] RV ON
		RV.RoleValueTypeId = R.RoleValueTypeId
	Where RV.Value ='Manage Property Amenity With Pricing'
	and PersonaId is not null

	INSERT INTO [Security].[PersonaRole] (PersonaId,RoleId,FromDate,ThruDate,CreatedBy,CreatedDate)  
	Select PersonaId,@UARole1,FromDate,ThruDate,@userId,@now 
	from [Enterprise].[PersonaPrivilege] P
	Join [Enterprise].[Role] R On
		R.RoleID = P.RoleID
	Join  [Enterprise].[RoleValueType] RV ON
		RV.RoleValueTypeId = R.RoleValueTypeId
	Where RV.Value ='View Amenities'
	and PersonaId is not null
END

IF NOT EXISTS (SELECT 1 FROM [Security].[OrganizationOverRideRight])
BEGIN
	Declare @RightId int
	Declare @OrgId int

	  select @OrgId = PartyId from [Enterprise].[Organization] where Name = 'RealPage Employee'

	  --Notification turned on only for employee comp
	 Insert Into [Security].[OrganizationOverRideRight](RightId,OrgPartyId,VisibilityStatusId,CreatedBy,CreatedDate)
	 Select RightId, @OrgId ,9,@userId,@now
	 FROM [Security].[Right]
	 Where Value IN ('Manage Notifications Configurations',
					  'Ability to Import users',
					  'View only access to Unified Platform from Support Tool',
					  'Access to Settings Admin',
					  'Manage Settings Templates',
					  'Employee Access to Design Questionnaires in CIMPL',
					  'Employee Access to CIMPL',
					  'Employee Access to add Implementation Records in CIMPL',
					  'Employee Access to Vendor Marketplace',
					  'Manage Custom User fields settings',
					  'Manage Unified Platform Security Settings',
					  'Access to Unified Settings',
					  'Access to Unified Platform via Support Tool',
					  'Manage help center administrator',
					  'Manage help center knowledge base',
					  'Manage help center videos',
					  'Manage help center online help',
					  'Manage help center product updates',
					  'Approve platform alerts',
					  'Create platform alerts')

	 select @OrgId = PartyId from [Enterprise].[Organization] where Name = 'RP Northstar Management Demo'
	 Insert Into [Security].[OrganizationOverRideRight](RightId,OrgPartyId,VisibilityStatusId,CreatedBy,CreatedDate)
	 Select RightId, @OrgId ,9,@userId,@now
	 FROM [Security].[Right]
	 Where Value IN ( 'Manage Settings Templates',
					  'Manage Unified Platform Security Settings',
					  'Access to Unified Settings')
	
END

--****************************************************************************************************************************************
--FIX for 375675
GO
DECLARE @CreateAlertId INT, @ApproveAlertId INT, @UserId BIGINT, @OrganizationPartyId INT, @Now datetime = GETDATE();
SELECT	@UserId = UserId
FROM	Ident.UserLogin
WHERE	LoginName LIKE 'realpagead@%';

SELECT  @OrganizationPartyId = o.PartyId
FROM Enterprise.Organization AS o
	 INNER JOIN
	 Enterprise.Party AS p
	 ON P.PartyId = O.PartyId
WHERE O.Name = 'RealPage Employee';

select @CreateAlertId = RightId from [security].[right] where rightname ='CreatePlatformAlerts';
select @ApproveAlertId = RightId from [security].[right] where rightname ='ApprovePlatformAlerts';

update [security].[right] set VisibilityStatusId = 10 where RightId in (@CreateAlertId,@ApproveAlertId);

IF EXISTS(SELECT TOP 1 1 FROM [security].[roleright] where roleid = 1 and RightId in (@CreateAlertId,@ApproveAlertId))
BEGIN
	delete [security].[roleright] where roleid = 1 and RightId in (@CreateAlertId,@ApproveAlertId);
END

IF NOT EXISTS(SELECT TOP 1 1 FROM [security].[organizationoverrideright] where RightId = @CreateAlertId)
BEGIN
	insert into [security].[organizationoverrideright](RightId,OrgPartyId,VisibilityStatusId,CreatedBy,CreatedDate)
	values(@CreateAlertId,@OrganizationPartyId,9,@UserId,@Now);
END

IF NOT EXISTS(SELECT TOP 1 1 FROM [security].[organizationoverrideright] where RightId = @ApproveAlertId)
BEGIN
	insert into [security].[organizationoverrideright](RightId,OrgPartyId,VisibilityStatusId,CreatedBy,CreatedDate)
	values(@ApproveAlertId,@OrganizationPartyId,9,@UserId,@Now);
END
GO

--Fix for 381988
DECLARE @ManageNotificationRightId INT, @UserId BIGINT, @OrganizationPartyId INT, @Now datetime = GETDATE();
SELECT	@UserId = UserId
FROM	Ident.UserLogin
WHERE	LoginName LIKE 'realpagead@%';

SELECT  @OrganizationPartyId = o.PartyId
FROM Enterprise.Organization AS o
	 INNER JOIN
	 Enterprise.Party AS p
	 ON P.PartyId = O.PartyId
WHERE O.Name = 'RealPage Employee';

select @ManageNotificationRightId = RightId from [security].[right] where rightname ='ManageNotifications';

update [security].[right] set VisibilityStatusId = 10 where RightId in (@ManageNotificationRightId);

IF EXISTS(SELECT TOP 1 1 FROM [security].[roleright] where roleid = 1 and RightId in (@ManageNotificationRightId))
BEGIN
	delete [security].[roleright] where roleid = 1 and RightId in (@ManageNotificationRightId);
END

IF NOT EXISTS(SELECT TOP 1 1 FROM [security].[organizationoverrideright] where RightId = @ManageNotificationRightId)
BEGIN
	insert into [security].[organizationoverrideright](RightId,OrgPartyId,VisibilityStatusId,CreatedBy,CreatedDate)
	values(@ManageNotificationRightId,@OrganizationPartyId,9,@UserId,@Now);
END

GO
IF NOT EXISTS(SELECT TOP 1 1 FROM [Auth].[Claim]  where ClaimName = 'reno-username')
BEGIN
	INSERT INTO [Auth].[Claim]([ClaimName], [SAMLAttributeName], [ProductId])
	VALUES ('reno-username','productUsername', 55);
END
GO
IF NOT EXISTS(SELECT TOP 1 1 FROM [Auth].[Claim]  where ClaimName = 'reno-pmcid')
BEGIN
	INSERT INTO [Auth].[Claim]([ClaimName], [SAMLAttributeName], [ProductId])
	VALUES ('reno-pmcid','PMCID', 55);
END
GO

IF NOT EXISTS(SELECT TOP 1 1 FROM [Enterprise].[ProductRight] where ProductId = 56 AND RightShortName = 'Managecompanylevelsettings')
BEGIN
	INSERT INTO Enterprise.ProductRight(ProductId,RightShortName)
	VALUES (56,'Managecompanylevelsettings');
END
GO

IF NOT EXISTS(SELECT TOP 1 1 FROM [Enterprise].[ProductRight] where ProductId = 56 AND RightShortName = 'Managepropertylevelsettings')
BEGIN
	INSERT INTO Enterprise.ProductRight(ProductId,RightShortName)
	VALUES (56,'Managepropertylevelsettings');
END
GO

IF NOT EXISTS(SELECT TOP 1 1 FROM [Enterprise].[ProductRight] where ProductId = 56 AND RightShortName = 'Viewallcompanylevelsettings')
BEGIN
	INSERT INTO Enterprise.ProductRight(ProductId,RightShortName)
	VALUES (56,'Viewallcompanylevelsettings');
END
GO

IF NOT EXISTS(SELECT TOP 1 1 FROM [Enterprise].[ProductRight] where ProductId = 56 AND RightShortName = 'Viewallpropertylevelsettings')
BEGIN
	INSERT INTO Enterprise.ProductRight(ProductId,RightShortName)
	VALUES (56,'Viewallpropertylevelsettings');
END
GO

IF NOT EXISTS(SELECT TOP 1 1 FROM [Enterprise].[ProductRight] where ProductId = 56 AND RightShortName = 'ViewUnifiedSettings')
BEGIN
	INSERT INTO Enterprise.ProductRight(ProductId,RightShortName)
	VALUES (56,'ViewUnifiedSettings');
END
GO

IF NOT EXISTS(SELECT TOP 1 1 FROM [Enterprise].[ProductRight] where ProductId = 39 AND RightShortName = 'AccessIntegrationMarketplace')
BEGIN
	INSERT INTO Enterprise.ProductRight(ProductId,RightShortName)
	VALUES (39,'AccessIntegrationMarketplace');
END
GO

IF EXISTS (SELECT TOP 1 1 FROM UserManagement.[Control] WHERE ControlId = 519)
BEGIN
	update UserManagement.[Control] set DisplayName = 'Assign current and new entities automatically' where ControlId = 519;
END
GO