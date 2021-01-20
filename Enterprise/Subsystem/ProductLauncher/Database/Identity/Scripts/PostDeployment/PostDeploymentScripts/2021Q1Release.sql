GO
--Begin Renter Engagement Product Resource tile
DECLARE @ProductId INT, 
		@LoginURI NVARCHAR(100), 
		@SigningCertificateThumbprint NVARCHAR(50), 
		@ParentProductTypeId INT, 
		@ProductName NVARCHAR(100)= 'P2 Engagement Queue', 
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
    WHERE Name = 'P2 Engagement Queue'
)
    BEGIN
        EXEC [Enterprise].[CreateProductType] 
             @ProductTypeId = 509, 
             @ParentProductTypeId = @ParentProductTypeId, 
             @Name = @ProductName, 
             @Description = 'P2 Engagement Queue', 
             @ProductTypeGUID = '876FB21D-8810-45AD-9572-9EEA2746EA2C';
END;
SET @ProductId = 64;
IF NOT EXISTS
(
    SELECT 1
    FROM Enterprise.Product
    WHERE Name = @ProductName
)
    BEGIN
        EXEC Enterprise.CreateProduct 
             @ProductId = @ProductId, 
             @ProductGUID = 'BB35CC3D-D64F-4C59-8556-F5185D818046', 
             @Name = @ProductName, 
             @Description = @ProductName, 
             @ProductTypeId = 509;
        UPDATE Enterprise.Product
          SET 
              BooksProductCode = 'RE'
        WHERE ProductId = @ProductId;
END;


IF @ServerName IN('RCDUSODBSQL001')
    BEGIN
        SET @ProductUrl = 'https://p2-qa.realpage.com/renter-engagement';
END;
IF @ServerName = 'RCTUSODBSQL001'
    BEGIN
        SET @ProductUrl = 'https://p2-qa.realpage.com/renter-engagement';
END;
IF @ServerName IN('RCQUSODBSQL001', 'RCVEUSODBSQL001', 'RCDUSODBSQL001A', 'RCIUSODBSQL002', 'RCTUSODBSQL001A') -- Need to chnage
    BEGIN
        SET @ProductUrl = '';
END;
IF @ServerName IN('RCPGBKDBSQL005A', 'RCPGBKDBSQL005B') -- Need to change
    BEGIN
        SET @ProductUrl = '';
END;
INSERT INTO @ProductConfiguration
(SettingName, 
 SettingDescription, 
 SettingValue
)
VALUES
('ClassName',  '',  'p2engagementqueue'),
('ProductUrl',  '',  @ProductUrl),
('TitleId',  '',  'P2 Engagement Queue'),
('TitleUniqueId',  '',  '69F7A6D7-CA8B-4E7B-BD46-340AE45BCF11'),
('IsNewTab',  '', '1'),
('MetatagUniqueId',  '',  'P2 Engagement Queue'),
('IsResource',  '',  '1'),
('IsFavorite',  '',  '0'),
('LearnMore',  '',  'https://www.realpage.com/'),
('ProductStatus',  'Show if the external application was configured for the dashboard user.',  '8'),
('ShowInUserDetails',  'Should the product show in the New/Edit user pages',  '0'),
('ShowInRolesAndRights',  'Should the product show in the Role/Rights page',  '0'),
('ShowInAppSwitcher',  'Should the product show in the application switcher',  '0'),
('ShowInUserListFilter',  'Should the product show in the user list product pick list',  '0'),
('ProductAPIRequiresUser',  'Does the product require a user for api calls',  '0'),
('LockOnProductAccess',  '',  '0'),
('ProductNotAvailableForRegularUserNoEmail',  'Product Attribute for Product Not Available for Regular User No Email.',  '0');

SELECT * FROM @ProductConfiguration

IF @ServerName IN('RCDUSODBSQL001')
    BEGIN
        SET @LoginURL = 'https://p2-qa.realpage.com/renter-engagement';
END;
IF @ServerName = 'RCTUSODBSQL001'
    BEGIN
        SET @LoginURL = 'https://p2-qa.realpage.com/renter-engagement';
END;
IF @ServerName IN('RCQUSODBSQL001', 'RCVEUSODBSQL001', 'RCDUSODBSQL001A', 'RCIUSODBSQL002', 'RCTUSODBSQL001A') -- Need to change
    BEGIN
        SET @LoginURL = '';
END;
IF @ServerName IN('RCPGBKDBSQL005A', 'RCPGBKDBSQL005B') -- Need to change
    BEGIN
        SET @LoginURL = '';
END;
SET @ProductID = 64;
SET @LoginURI = @LoginURL;
SET @SigningCertificateThumbprint = NULL;
EXEC Enterprise.ProductConfigurationSetup 
     @ProductId, 
     @LoginURI, 
     @SigningCertificateThumbprint, 
     @ProductConfiguration;


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
-- To set Roles and Rights for product
DECLARE @RightName nvarchar(200),
              @RightDescription nvarchar(200),
              @RightValue nvarchar(200),
              @StatusTypeId int,
              @OrgVisibilityStatusId INT = NULL,
              @RightVisibilityStatusId INT =NULL,
              @ProductId INT,
              @TargetProductId int,
              @UserId bigint,
              @Now datetime = GETDATE(),
              @RightId int,
              @RoleId INT,
              @OrgPartyId INT,
              @SuperUserRoleId Int,
              @ServerName SYSNAME = @@SERVERNAME;

DECLARE @TargetRoleName TABLE (RoleName nvarchar(100))
DECLARE @TargetOrganization TABLE (PartyId INT)
DECLARE @HoldRoleId TABLE (RoleId int)
DECLARE @HoldOrgPartyId TABLE (PartyId INT)
DECLARE @HoldRouteId TABLE (RouteId INT)

       SET @RightName = 'AccessP2EngagementQueue'; 
       SET @RightDescription = 'Access to P2 Engagement Queue';
       SET @RightValue = 'Access to P2 Engagement Queue';
       SET @StatusTypeId = 13;
       SET @RightVisibilityStatusId = 9;
       SET @ProductId =3;
       SET @TargetProductId = 3;

       SELECT @UserId = UserId
       FROM   Ident.UserLogin
       WHERE  LoginName LIKE 'realpagead@%'
       
    IF NOT EXISTS (Select 1 From [Security].[Right] Where RightName = @RightName)
    BEGIN
        INSERT INTO [Security].[Right](  RightName,Description, Value,StatusTypeId,VisibilityStatusId,ProductId,TargetProductId,     CreatedBy,CreatedDate)
           VALUES ( @RightName,@RightDescription,@RightValue,@StatusTypeId, @RightVisibilityStatusId,@ProductId,@TargetProductId,@UserId,@Now)
    END
       
    Select @SuperUserRoleId = RoleId from Security.Role Where ShortName = 'SuperUser'
    Select @RightId = RightId From [Security].[Right] Where RightName = 'AccessP2EngagementQueue'

    IF NOT EXISTS (Select 1 From [Security].[RoleRight] Where RoleId = @SuperUserRoleId AND RightId = @RightId)
    BEGIN
        INSERT INTO [Security].[RoleRight]( RoleId,RightId,CreatedBy,CreatedDate)
           VALUES ( @SuperUserRoleId,@RightId,@UserId,@Now)
    END

   
GO
	IF NOT EXISTS (Select 1 From Enterprise.ProductRight Where ProductId = 64)
	BEGIN
		Insert into Enterprise.ProductRight(ProductId,RightShortName,DependantProductId)
		Select 64,'AccessP2EngagementQueue',NULL
	END
--End Script

--Stroed Proc to create a new product
GO
Create Procedure [Enterprise].CreateNewProduct
	@LoginURI NVARCHAR(100), 
	@SigningCertificateThumbprint NVARCHAR(50), 
	@PropertyManagementName Nvarchar(50),
	@ProductName NVARCHAR(100),
	@LoginURL NVARCHAR(500),
	@apiendpoint NVARCHAR(1000), 
	@tokenEndPoint NVARCHAR(1000), 
	@apisecret NVARCHAR(1000),
	@LearnMoreSettingValue nvarchar(2000),
	@BooksProductCode NVARCHAR(10),	
	@ClassNameSettingValue NVARCHAR(2000),
	@ProductUrlSettingValue NVARCHAR(2000),
	@TitleIdSettingValue NVARCHAR(2000),
	@IsNewTabSettingValue NVARCHAR(2000),
	@IsResourceSettingValue NVARCHAR(2000),
	@IsFavoriteSettingValue NVARCHAR(2000),
	@ProductStatus1SettingValue NVARCHAR(2000),
	@ProductStatus2SettingValue NVARCHAR(2000),
	@ProductStatus3SettingValue NVARCHAR(2000),
	@ProductStatus4SettingValue NVARCHAR(2000),
	@ShowInUserDetailsSettingValue NVARCHAR(2000),
	@ShowInRolesAndRightsSettingValue NVARCHAR(2000),
	@ShowInAppSwitcherSettingValue NVARCHAR(2000),
	@ShowInUserListFilterSettingValue NVARCHAR(2000),
	@ProductAPIRequiresUserSettingValue NVARCHAR(2000),
	@LockOnProductAccessSettingValue NVARCHAR(2000),
	@ProductNotAvailableForRegularUserNoEmailSettingValue NVARCHAR(2000),
	@CLIENTIDSettingValue NVARCHAR(2000),
	@TOKENENDPOINTSettingValue NVARCHAR(2000),
	@APISECRETSensitiveData INT,
	@ClientScopeSettingValue NVARCHAR(2000),
	@AuthenticationTypeSettingValue NVARCHAR(2000),
	@SubjectIdSamlAttribute NVARCHAR(50)
AS

DECLARE 
	@ServerName SYSNAME = @@SERVERNAME,
	@prdTypeGUID NVARCHAR(50), 	
	@prdGUID NVARCHAR(50),
	@ProductConfiguration AS PRODUCTCONFIGURATIONTYPE,
	@ProductId INT,
	@ProductTypeId INT,
	@TitleUniqueIdSettingValue NVARCHAR(2000),
	@ParentProductTypeId INT;

SELECT @prdTypeGUID = NEWID()
SELECT @prdGUID = NEWID()
SELECT @TitleUniqueIdSettingValue = NEWID();
SELECT @ParentProductTypeId = ProductTypeId FROM Enterprise.ProductType WHERE Name = @PropertyManagementName AND ParentProductTypeId IS NULL;

SET @ProductTypeId = (SELECT MAX(productTypeid) + 1 'ProductTypeId'  FROM [Enterprise].[ProductType]  WHERE ParentProductTypeId = @ParentProductTypeId)
SET @ProductId = (SELECT MAX(pr.ProductId)+1 FROM Enterprise.Product pr)

IF NOT EXISTS(SELECT TOP 1 1 FROM enterprise.ProductType WHERE Name = @ProductName)
BEGIN
        EXEC [Enterprise].[CreateProductType]  @ProductTypeId = @ProductTypeId, @ParentProductTypeId = @ParentProductTypeId, @Name = @ProductName, @Description = @ProductName, @ProductTypeGUID = @prdTypeGUID
END;

IF NOT EXISTS (SELECT 1 FROM Enterprise.Product WHERE Name = @ProductName)
    BEGIN
        EXEC Enterprise.CreateProduct @ProductId = @ProductId, @ProductGUID = @prdGUID, @Name = @ProductName, @Description = @ProductName, @ProductTypeId = @ProductTypeId
        
        UPDATE Enterprise.Product
        SET BooksProductCode = @BooksProductCode
		WHERE ProductId = @ProductId;
END;

SET NOCOUNT ON

INSERT INTO @ProductConfiguration(SettingName,  SettingDescription,  SettingValue,SettingSensitiveData)
VALUES
('ClassName','',@ClassNameSettingValue, 0)
,('ProductUrl','',@ProductUrlSettingValue, 0)
,('TitleId','',@TitleIdSettingValue, 0)
,('TitleUniqueId','',@TitleUniqueIdSettingValue, 0)
,('IsNewTab','',@IsNewTabSettingValue, 0)
,('MetatagUniqueId','',@ProductName, 0)
,('IsResource','',@IsResourceSettingValue, 0)
,('IsFavorite','',@IsFavoriteSettingValue, 0)
,('LearnMore','',@LearnMoreSettingValue, 0)
,('ApiEndPoint','',@apiendpoint, 0)
,('ProductStatus','Show if the external application was configured for the dashboard user.',@ProductStatus1SettingValue, 0)
,('ProductStatus','Show if the external application was configured for the dashboard user.',@ProductStatus2SettingValue, 0)
,('ProductStatus','Show if the external application was configured for the dashboard user.',@ProductStatus3SettingValue, 0)
,('ProductStatus','Show if the external application was configured for the dashboard user.',@ProductStatus4SettingValue, 0)
,('ShowInUserDetails','Should the product show in the New/Edit user pages',@ShowInUserDetailsSettingValue, 0)
,('ShowInRolesAndRights','Should the product show in the Role/Rights page',@ShowInRolesAndRightsSettingValue, 0)
,('ShowInAppSwitcher','Should the product show in the application switcher',@ShowInAppSwitcherSettingValue, 0)
,('ShowInUserListFilter','Should the product show in the user list product pick list',@ShowInUserListFilterSettingValue, 0)
,('ProductAPIRequiresUser','Does the product require a user for api calls',@ProductAPIRequiresUserSettingValue, 0)
,('LockOnProductAccess', '', @LockOnProductAccessSettingValue, 0)
,('ProductNotAvailableForRegularUserNoEmail','Product Attribute for Product Not Available for Regular User No Email.',@ProductNotAvailableForRegularUserNoEmailSettingValue, 0)
,('CLIENTID','',@CLIENTIDSettingValue, 0)
,('TOKENENDPOINT','', @tokenEndPoint, 0) 
,('APISECRET','', @apisecret, 1)
,('ClientScope', 'The client scope to get access token',@ClientScopeSettingValue,0)
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
,('GetUserExistEndpoint','Get User Exist Endpoint for product API','/userexists?loginName={0}', 0) 
,('AuthenticationType','Used to determine how to log into the product','Redirect', 0)

IF NOT EXISTS (SELECT TOP 1 1 FROM Enterprise.ProductSetting WHERE ProductId = @ProductId)
BEGIN
       EXEC Enterprise.ProductConfigurationSetup @ProductId, @LoginURI, @SigningCertificateThumbprint, @ProductConfiguration;
END;

IF NOT EXISTS(SELECT 1 FROM ident.SamlProductSettings WHERE ProductId = @ProductId AND LoginUri = @LoginURL)
    BEGIN
        INSERT INTO ident.SamlProductSettings 
         (ProductId, LoginUri, SigningCertificateThumbprint, SubjectIdSamlAttribute )
        VALUES
		 (@ProductId, @LoginURL, @SigningCertificateThumbprint, @SubjectIdSamlAttribute );
END;

GO


--execute proc to create new product Reporting
exec [Enterprise].CreateNewProduct  
	@LoginURI = '', @SigningCertificateThumbprint = '', 	
	@PropertyManagementName = 'Administration',
	@ProductName = 'Reporting',
    @LoginURL ='',
	@apiendpoint ='',
	@tokenEndPoint ='',	
	@apisecret ='',
	@LearnMoreSettingValue ='',	
	@BooksProductCode ='RPT',
	@ClassNameSettingValue ='reporting',
	@ProductUrlSettingValue ='',
	@TitleIdSettingValue ='Reporting',
	@IsNewTabSettingValue ='0',
	@IsResourceSettingValue ='0',
	@ProductStatus1SettingValue ='',
	@ProductStatus2SettingValue ='',
	@ProductStatus3SettingValue ='',
	@ProductStatus4SettingValue ='',
	@ShowInUserDetailsSettingValue ='0',
	@ShowInRolesAndRightsSettingValue ='0',
	@ShowInAppSwitcherSettingValue ='0',
	@ShowInUserListFilterSettingValue ='0',
	@ProductAPIRequiresUserSettingValue ='0',
	@LockOnProductAccessSettingValue ='0',
	@ProductNotAvailableForRegularUserNoEmailSettingValue ='0',
	@CLIENTIDSettingValue ='',
	@TOKENENDPOINTSettingValue ='',
	@APISECRETSensitiveData = 1,
	@ClientScopeSettingValue ='',
	@AuthenticationTypeSettingValue ='',
	@SubjectIdSamlAttribute =''

GO