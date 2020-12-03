GO
UPDATE [Enterprise].[Product] 
  SET [Name] =  'Unified Data Management' WHERE [Name] = 'Master Data Management' AND ProductId = 24
UPDATE [UserManagement].[ProductPage]
  SET DisplayName = 'Unified Data Management Product Access' WHERE DisplayName = 'Master Data Management Product Access' 
GO

/*This script is to update 'Access to Site Spend Management only', to 'Site user'*/
UPDATE [UserManagement].[Control]
SET DisplayName = 'Site user' WHERE DisplayName = 'Access to Site Spend Management only'
GO
/*This script is to add EmployeeAccessToCompanySetup right.*/
GO
DECLARE @CreatedById bigint,
		@RouteId bigint,
		@RightId bigint,
		@Now datetime = GETDATE(),
		@PartyId bigint,
		@RoleId bigint

SELECT @CreatedById = UserId
FROM Ident.UserLogin
WHERE LoginName = 'RealPageAd@test.com'

IF NOT EXISTS (SELECT 1 FROM [Security].[Right] WHERE RightName = 'EmployeeAccessToCompanySetup')
BEGIN
	INSERT INTO [Security].[Right](	RightName,Description, Value,StatusTypeId,VisibilityStatusId,ProductId,TargetProductId,	CreatedBy,CreatedDate)
    VALUES ('EmployeeAccessToCompanySetup', 'Allow an authorized RealPage employee the ability to navigate the Configurations icon','Employee Access to Company Setup', 13,10, 3, 3, @CreatedById, @Now)
END

--RightRoute
SELECT @RightId = RightId
FROM [Security].[Right]
WHERE RightName = 'EmployeeAccessToCompanySetup'

SELECT @RouteId = RouteId
FROM [Security].[Route]
WHERE RouteValue = 'SideMenu'

IF NOT EXISTS (SELECT 1 FROM [Security].[RightRoute] WHERE RightId = @RightId AND RouteId = @RouteId)
BEGIN
	INSERT INTO [Security].[RightRoute] (RightId,RouteId,RightName,CreatedBy,CreatedDate)
	VALUES (@RightId, @RouteId, 'Employee Access to Company Setup', @CreatedById, @Now)
END
--RoleRight
SELECT @RoleId = RoleId 
FROM [Security].[Role]
WHERE RoleName = 'User Administrator' AND ShortName = 'SuperUser'

IF NOT EXISTS (SELECT 1 FROM [Security].[RoleRight] WHERE RoleId = @RoleId AND RightId = @RightId)
BEGIN
	INSERT INTO [Security].[RoleRight]( RoleId,RightId,CreatedBy,CreatedDate)
	VALUES (@RoleId, @RightId, @CreatedById, @Now)
END

--OrganizationOverRideRight
SELECT @PartyId = O.PartyId
FROM [Enterprise].[Organization] O
    INNER JOIN [Enterprise].[Party] P ON P.PartyId = O.PartyId
WHERE p.RealPageId = '0D018E46-C20E-477D-ADED-4E5A35FB8F99'

IF NOT EXISTS (SELECT 1 FROM [Security].[OrganizationOverRideRight]  WHERE RightId = @RightId AND OrgPartyId = @PartyId)
BEGIN
	INSERT INTO [Security].[OrganizationOverRideRight]
           ([RightId]
           ,[OrgPartyId]
           ,[VisibilityStatusId]
           ,[CreatedBy]
           ,[CreatedDate]) 
           VALUES	(@RightId, @PartyId, 9, @CreatedById, @Now)
END
GO
-- Add UDM Source Code for ILM products
Update Enterprise.Product SET UDMSourceCode = NULL
Where ProductId in (40,41)

Update Enterprise.Product SET UDMSourceCode = 'AO'
Where ProductId in (29,30,31,32,33,34,51,52,53,54)

Update Enterprise.Product SET UDMSourceCode = 'IB',BooksProductCode = 'SMS-T'
Where ProductId in (57)

-- update roles and rights search datasource
  update [UserManagement].[Control] set DataSource = 'name'
  where UIId = 'OnesiteRolesAndRightsRightLabelUIId'
  And DisplayName = 'Role'
GO

--IB Product Energy
/*This script is a sample script to create new prodcut in the system.*/
GO
DECLARE @ProductId INT, 
		@LoginURI NVARCHAR(100), 
		@SigningCertificateThumbprint NVARCHAR(50), 
		@ParentProductTypeId INT, 
		@ProductName NVARCHAR(100)= 'Intelligent Building Energy',  -- Produact Name
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
    WHERE Name = 'Intelligent Building Energy'
)
    BEGIN
        EXEC [Enterprise].[CreateProductType] 
             @ProductTypeId = 113, -- Thsi value may change based on the root prodcut type
             @ParentProductTypeId = @ParentProductTypeId, 
             @Name = @ProductName, 
             @Description = @ProductName, 
             @ProductTypeGUID = 'E94DF650-C9A6-46B5-974D-99127DA46A3D'; -- Use newid() to generate new uniqueidentifier.
END;
SET @ProductId = 58; -- Assign new product Id

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
             @ProductGUID = '88DE7EF1-70F4-492C-AA14-D3020ED406D9', -- Use newid() to generate new uniqueidentifier.
             @Name = @ProductName, 
             @Description = @ProductName, 
             @ProductTypeId = 113;
        
		UPDATE Enterprise.Product
          SET 
              BooksProductCode = 'SMS-E',
              UDMSourceCode = 'IB'
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
IF @ServerName IN ('RCQUSODBSQL001')
BEGIN
	SET @apiendpoint = '';
END
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
 ('ClassName','','intelligentbuildingenergy')
,('ProductUrl','','/product/intelligentbuildingenergy')
,('TitleId','','Intelligent Building Energy')
,('TitleUniqueId','','30B79EE4-C79A-43B7-B3B4-CAE58C86791A')
,('IsNewTab','','1')
,('MetatagUniqueId','','Intelligent Building Energy')
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
	SET @LoginURL = 'www.dev-abcenergy.realpage.com';
END
IF @ServerName IN ('rctusodbsql001')
BEGIN
	SET @LoginURL = 'www.qa-abcenergy.realpage.com';
END
IF @ServerName IN ('RCQUSODBSQL001')
BEGIN
	SET @LoginURL = 'www.sat-abcenergy.realpage.com';
END
IF @ServerName IN ('RCPGBKDBSQL005A', 'RCPGBKDBSQL005B')
BEGIN
	SET @LoginURL = 'www.abcenergy.realpage.com';
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

--IB Product water
/*This script is a sample script to create new prodcut in the system.*/

DECLARE @ProductId INT, 
		@LoginURI NVARCHAR(100), 
		@SigningCertificateThumbprint NVARCHAR(50), 
		@ParentProductTypeId INT, 
		@ProductName NVARCHAR(100)= 'Intelligent Building Water',  -- Produact Name
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
    WHERE Name = 'Intelligent Building Water'
)
    BEGIN
        EXEC [Enterprise].[CreateProductType] 
             @ProductTypeId = 113, -- Thsi value may change based on the root prodcut type
             @ParentProductTypeId = @ParentProductTypeId, 
             @Name = @ProductName, 
             @Description = @ProductName, 
             @ProductTypeGUID = 'DC3177C8-534E-47B7-B2E3-730107382D23'; -- Use newid() to generate new uniqueidentifier.
END;
SET @ProductId = 59; -- Assign new product Id

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
             @ProductGUID = '1DB5E6B6-F634-417C-BC8B-83546EB1421F', -- Use newid() to generate new uniqueidentifier.
             @Name = @ProductName, 
             @Description = @ProductName, 
             @ProductTypeId = 113;
        
		UPDATE Enterprise.Product
          SET 
              BooksProductCode = 'SMS-W',
              UDMSourceCode = 'IB'
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
 ('ClassName','','intelligentbuildingwater')
,('ProductUrl','','/product/intelligentbuildingwater')
,('TitleId','','Intelligent Building Water')
,('TitleUniqueId','','E8BB4376-7105-4E8D-B365-E00E58CC4832')
,('IsNewTab','','1')
,('MetatagUniqueId','','Intelligent Building Water')
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
	SET @LoginURL = 'www.dev-abcwater.realpage.com';
END
IF @ServerName IN ('rctusodbsql001')
BEGIN
	SET @LoginURL = 'www.qa-abcwater.realpage.com';
END
IF @ServerName IN ('RCQUSODBSQL001')
BEGIN
	SET @LoginURL = 'www.sat-abcwater.realpage.com';
END
IF @ServerName IN ('RCPGBKDBSQL005A', 'RCPGBKDBSQL005B')
BEGIN
	SET @LoginURL = 'www.abcwater.realpage.com';
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

--HospitalityService
/*This script is a sample script to create new prodcut in the system.*/

DECLARE @ProductId INT, 
		@LoginURI NVARCHAR(100), 
		@SigningCertificateThumbprint NVARCHAR(50), 
		@ParentProductTypeId INT, 
		@ProductName NVARCHAR(100)= 'Hospitality As A Service',  -- Produact Name
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
    WHERE Name = 'HospitalityService'
)
    BEGIN
        EXEC [Enterprise].[CreateProductType] 
             @ProductTypeId = 113, -- Thsi value may change based on the root prodcut type
             @ParentProductTypeId = @ParentProductTypeId, 
             @Name = @ProductName, 
             @Description = @ProductName, 
             @ProductTypeGUID = '0FCD4571-EDF2-4040-B985-61F709EFA7C7'; -- Use newid() to generate new uniqueidentifier.
END;
SET @ProductId = 60; -- Assign new product Id

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
             @ProductGUID = '7ED03C61-670D-4692-97A4-8F6D4E94BAF8', -- Use newid() to generate new uniqueidentifier.
             @Name = @ProductName, 
             @Description = @ProductName, 
             @ProductTypeId = 113;
        
		UPDATE Enterprise.Product
          SET 
              BooksProductCode = 'HAAS'
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
 ('ClassName','','hospitalityservice')
,('ProductUrl','','/product/HospitalityService')
,('TitleId','','Hospitality As A Service')
,('TitleUniqueId','','F98C3B10-3753-4438-B3F1-24A3878D8FD9')
,('IsNewTab','','1')
,('MetatagUniqueId','','HospitalityService')
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
	SET @LoginURL = 'https://dev-haas-admin.realpage.com/';
END
IF @ServerName IN ('rctusodbsql001')
BEGIN
	SET @LoginURL = 'https://qa-haas-admin.realpage.com/';
END
IF @ServerName IN ('RCQUSODBSQL001')
BEGIN
	SET @LoginURL = 'https://dev-haas-admin.realpage.com/';
END
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

	SET @RightName = 'ManageIntelligentBuildingEnergyProductAccess'; 
	SET @RightDescription = 'Manage Energy Management Solution Product Access';
	SET @RightValue = 'Manage Energy Management Solution Product Access';
	SET @StatusTypeId = 13;
	SET @RightVisibilityStatusId = 9;
	SET @ProductId =3;
	SET @TargetProductId = 58;

	SELECT	@UserId = UserId
	FROM	Ident.UserLogin
	WHERE	LoginName LIKE 'realpagead@%'
	
    IF NOT EXISTS (Select 1 From [Security].[Right] Where RightName = @RightName)
    BEGIN
        INSERT INTO [Security].[Right](	RightName,Description, Value,StatusTypeId,VisibilityStatusId,ProductId,TargetProductId,	CreatedBy,CreatedDate)
	    VALUES ( @RightName,@RightDescription,@RightValue,@StatusTypeId, @RightVisibilityStatusId,@ProductId,@TargetProductId,@UserId,@Now)
    END
	

	SET @RightName = 'ManageIntelligentBuildingWaterProductAccess'; 
	SET @RightDescription = 'Manage Water Management Solution Product Access';
	SET @RightValue = 'Manage Water Management Solution Product Access';
	SET @TargetProductId = 59;

    IF NOT EXISTS (Select 1 From [Security].[Right] Where RightName = @RightName)
    BEGIN
        INSERT INTO [Security].[Right](	RightName,Description, Value,StatusTypeId,VisibilityStatusId,ProductId,TargetProductId,	CreatedBy,CreatedDate)
	    VALUES ( @RightName,@RightDescription,@RightValue,@StatusTypeId, @RightVisibilityStatusId,@ProductId,@TargetProductId,@UserId,@Now)
    END
	

	SET @RightName = 'ManageHospitalityServiceProductAccess'; 
	SET @RightDescription = 'Manage Hospitality As A Service Solution Product Access';
	SET @RightValue = 'Manage Hospitality As A Service Solution Product Access';
	SET @TargetProductId = 60;
    IF NOT EXISTS (Select 1 From [Security].[Right] Where RightName = @RightName)
    BEGIN
        INSERT INTO [Security].[Right](	RightName,Description, Value,StatusTypeId,VisibilityStatusId,ProductId,TargetProductId,	CreatedBy,CreatedDate)
	    VALUES ( @RightName,@RightDescription,@RightValue,@StatusTypeId, @RightVisibilityStatusId,@ProductId,@TargetProductId,@UserId,@Now)
    END
	
    Select @SuperUserRoleId = RoleId from Security.Role Where ShortName = 'SuperUser'
    Select @RightId = RightId From [Security].[Right] Where RightName = 'ManageIntelligentBuildingEnergyProductAccess'

    IF NOT EXISTS (Select 1 From [Security].[RoleRight] Where RoleId = @SuperUserRoleId AND RightId = @RightId)
    BEGIN
        INSERT INTO [Security].[RoleRight]( RoleId,RightId,CreatedBy,CreatedDate)
	    VALUES ( @SuperUserRoleId,@RightId,@UserId,@Now)
    END

    Select @RightId = RightId From [Security].[Right] Where RightName = 'ManageIntelligentBuildingWaterProductAccess'

    IF NOT EXISTS (Select 1 From [Security].[RoleRight] Where RoleId = @SuperUserRoleId AND RightId = @RightId)
    BEGIN
        INSERT INTO [Security].[RoleRight]( RoleId,RightId,CreatedBy,CreatedDate)
	    VALUES ( @SuperUserRoleId,@RightId,@UserId,@Now)

    END

    Select @RightId = RightId From [Security].[Right] Where RightName = 'ManageHospitalityServiceProductAccess'

    IF NOT EXISTS (Select 1 From [Security].[RoleRight] Where RoleId = @SuperUserRoleId AND RightId = @RightId)
    BEGIN
        INSERT INTO [Security].[RoleRight]( RoleId,RightId,CreatedBy,CreatedDate)
	    VALUES ( @SuperUserRoleId,@RightId,@UserId,@Now)
    END
GO

GO
DECLARE @UserId bigint,
       @ProductId int = 60,
       @productSettingId INT,
       @productSettingTypeId INT,
       @productGroupSettingTypeId INT,
       @ConfigurationId INT,
       @ParentControlID INT,
       @ControlID INT,
       @MaxControlId INT,
       @MaxControlAttributeId INT,
       @Now datetime = GETDATE();

SELECT @UserId = UserId
FROM   Ident.UserLogin
WHERE  LoginName LIKE 'realpagead@%'

IF NOT EXISTS (SELECT TOP 1 1 FROM[UserManagement].[ProductPage] WHERE ProductId = @ProductId)
BEGIN
		SET IDENTITY_INSERT [UserManagement].[Control] ON 

		SELECT @MaxControlId = max(ControlId) from UserManagement.Control

		INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate]) 
		VALUES (@MaxControlId + 1, NULL, 8, N'HAASProductAccessTabGroupUIId', NULL, NULL, 1, @UserId, @Now)

		INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate]) 
		VALUES (@MaxControlId + 2, @MaxControlId + 1, 9, N'HAASProductAccessRolesTabUIId', N'Roles', NULL, 1, @UserId, @Now)

		INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate]) 
		VALUES (@MaxControlId + 3, @MaxControlId + 2, 2, N'HAASProductAccessRolesSelectGridUIId', NULL, NULL, 2, @UserId, @Now)

		INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate]) 
		VALUES (@MaxControlId + 4, @MaxControlId + 3, 7, N'HAASProductAccessRadioUIId', NULL, N'isAssigned', 1, @UserId, @Now)

		INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate]) 
		VALUES (@MaxControlId + 5, @MaxControlId + 3, 5, N'HAASProductAccessRoleLabelUIId', N'Role', N'name', 2, @UserId, @Now)

		INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate]) 
		VALUES (@MaxControlId + 6, @MaxControlId + 3, 5, N'HAASProductAccessRoleTypeLabelUIId', N'Role Type', N'roletype', 3, @UserId, @Now)
		
		INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate]) 
		VALUES (@MaxControlId + 7, @MaxControlId + 3, 11, N'HAASProductAccessIconUIId', NULL, N'InfoIcon', 4, @UserId, @Now)
		
		INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate]) 
		VALUES (@MaxControlId + 8, @MaxControlId + 1, 9, N'HAASProductAccessPropertiesTabUIId', N'Properties', NULL, 2, @UserId, @Now)
		
		INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate]) 
		VALUES (@MaxControlId + 9, @MaxControlId + 8, 1, N'HAASProductAccessAllowaccesstoallcurrentandfuturepropertiesPropertiesSwitchUIId', N'Assign access to current and new properties automatically', N'allProperties', 1, @UserId, @Now)

		INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate]) 
		VALUES (@MaxControlId + 10, @MaxControlId + 8, 3, N'HAASProductAccessPropertiesMultiSelectGridUIId', NULL, NULL, 2, @UserId, @Now)

		INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate]) 
		VALUES (@MaxControlId + 11, @MaxControlId + 10, 10, N'HAASProductAccessCheckboxUIId', NULL, N'isAssigned', 1, @UserId, @Now)

		INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate]) 
		VALUES (@MaxControlId + 12, @MaxControlId + 10, 5, N'HAASProductAccessPropertyLabelUIId', N'Property', N'name', 2, @UserId, @Now)

		INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate]) 
		VALUES (@MaxControlId + 13, @MaxControlId + 10, 5, N'HAASProductAccessCityLabelUIId', N'City', N'city', 3, @UserId, @Now)

		INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate]) 
		VALUES (@MaxControlId + 14, @MaxControlId + 10, 5, N'HAASProductAccessStateLabelUIId', N'State', N'state', 4, @UserId, @Now)

		INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate]) 
		VALUES (@MaxControlId + 15, @MaxControlId + 7, 5, N'HAASProductAccessRoleDetailsLabelUIId', N'Role Details', NULL, 1, @UserId, @Now)

		INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate]) 
		VALUES (@MaxControlId + 16, @MaxControlId + 7, 12, N'HAASProductAccessGridUIId', N'NULL', NULL, 1, @UserId, @Now)

		INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate]) 
		VALUES (@MaxControlId + 17, @MaxControlId + 16, 5, N'HAASProductAccessRightLabelUIId', N'Right', 'description', 1, @UserId, @Now)

		 
		SET IDENTITY_INSERT [UserManagement].[Control] OFF
		
		SET IDENTITY_INSERT [UserManagement].[ControlAttribute] ON 

		SELECT @MaxControlAttributeId = max(ControlAttributeId) from [UserManagement].[ControlAttribute]

		INSERT [UserManagement].[ControlAttribute] ([ControlAttributeId], [ControlId], [Key], [Value], [CreatedBy], [CreatedDate]) 
		VALUES (@MaxControlAttributeId + 1, @MaxControlId + 2, N'Default', N'True', @UserId, @Now)

		INSERT [UserManagement].[ControlAttribute] ([ControlAttributeId], [ControlId], [Key], [Value], [CreatedBy], [CreatedDate]) 
		VALUES (@MaxControlAttributeId + 2, @MaxControlId + 3, N'ShowSelectAll', N'False', @UserId, @Now)

		INSERT [UserManagement].[ControlAttribute] ([ControlAttributeId], [ControlId], [Key], [Value], [CreatedBy], [CreatedDate]) 
		VALUES (@MaxControlAttributeId + 3, @MaxControlId + 7, N'InfoIcon', N'Slide', @UserId, @Now)

		SET IDENTITY_INSERT [UserManagement].[ControlAttribute] OFF

		SET IDENTITY_INSERT [UserManagement].[ProductPage] ON 

		INSERT [UserManagement].[ProductPage] ([ProductPageId], [ProductId], [DisplayName], [CreatedBy], [CreatedDate], [IsActive], [ProductPageTypeId]) 
		VALUES (39, 60, N'Home Sharing Product Access', @UserId, @Now, 1, 1)

		SET IDENTITY_INSERT [UserManagement].[ProductPage] OFF

		SET IDENTITY_INSERT [UserManagement].[ProductPageControl] ON 

		INSERT [UserManagement].[ProductPageControl] ([ProductPageControlId], [ProductPageId], [ControlId], [CreatedBy], [CreatedDate])
		VALUES (49, 39, @MaxControlId + 1, @UserId, @Now)

		SET IDENTITY_INSERT [UserManagement].[ProductPageControl] OFF
            
END
GO
DECLARE @RightValue nvarchar(200),
		 @UserId bigint,
		 @Now datetime = GETDATE(),
		 @RightId int,
		 @RoleId INT,
		 @OrgPartyId int,
		 @RoleTypeId int,
		 @ProductId int =60,
		 @RoleName nvarchar(100),
		 @OrgVisibilityStatusId INT = 9,
		 @RightVisibilityStatusId INT = 10,
		 @StatusTypeId int=13,
		 @ServerName SYSNAME = @@SERVERNAME;

IF @ServerName IN ('RCDUSODBSQL001','rctusodbsql001','RCQUSODBSQL001')
BEGIN		

		DECLARE @TargetRoleValue TABLE (RoleName nvarchar(100))

		INSERT INTO @TargetRoleValue VALUES('Customer Support Manager'),('Customer Support Representative'),
												('Implementations'),('Systems Admin');

	
			--UserId
			SELECT	@UserId = UserId
			FROM	Ident.UserLogin
			WHERE	LoginName LIKE 'realpagead@%'

		IF NOT EXISTS
		(
			SELECT TOP 1 1 FROM [Security].[Role]
			WHERE [RoleName] IN (SELECT RoleName FROM @TargetRoleValue)
		)
		BEGIN

				SELECT @RoleTypeId=RoleTypeId from [Security].RoleType WHERE [Value]='Product'
				SELECT @OrgPartyId=PartyId FROM Enterprise.Organization WHERE [Name]='Realpage Employee'

					--Cursor Mapping Role with Right
						DECLARE curCreateNewRole CURSOR FOR
						SELECT RoleName
						FROM @TargetRoleValue

						OPEN curCreateNewRole
						FETCH NEXT FROM curCreateNewRole INTO @RoleName

						WHILE @@FETCH_STATUS = 0
						BEGIN
							IF NOT EXISTS (SELECT TOP 1 1 FROM [Security].[Role] WHERE RoleName = @RoleName)
							BEGIN
								INSERT INTO [Security].[Role]
								(	RoleName,
									Shortname, 
									Description,
									RoleTypeID,
									OrgPartyID,
									ProductId,
									CreatedBy,
									createdDate
								)
								VALUES ( 
										@RoleName,
										@RoleName,
										@RoleName,
										@RoleTypeId,
										@OrgPartyId,
										@ProductId,
										@UserId,
										@Now
									   )

								IF NOT EXISTS(SELECT 1 FROM [Security].[Right] WHERE [Value]=@RoleName)
								BEGIN
										---Create Right
										INSERT INTO [Security].[Right]
											(	RightName,
												Description, 
												Value,
												StatusTypeId,
												VisibilityStatusId,
												ProductId,
												TargetProductId,
												CreatedBy,
												CreatedDate
											)
											VALUES ( 
													REPLACE(@RoleName, ' ', ''),
													@RoleName,
													@RoleName,
													@StatusTypeId, 
													@RightVisibilityStatusId,
													@ProductId,
													@ProductId,
													@UserId,
													@Now
												   )
											
								END
								SELECT @RoleId=RoleId FROM [Security].[Role] WHERE RoleName=@RoleName
								SELECT @RightId=RightId FROM [Security].[Right] WHERE [Value]=@RoleName

								IF NOT EXISTS (SELECT TOP 1 1 FROM [Security].[ORGANIZATIONOVERRIDERIGHT] WHERE RightId = @RightId AND OrgPartyId=@OrgPartyId)
								BEGIN
								INSERT INTO Security.organizationoverrideright(
											RightId,
											OrgPartyId,
											VisibilityStatusId,
											CreatedBy,
											CreatedDate
											)
											VALUES(
											@RightId,
											@OrgPartyId,
											@OrgVisibilityStatusId,
											@UserId,
											@Now
											)
								END

								IF NOT EXISTS (SELECT TOP 1 1 FROM [Security].[RoleRight] WHERE RoleId = @RoleId AND RightId=@RightId)
								BEGIN
									INSERT INTO [Security].[RoleRight]
									(	RoleId,
										RightId, 
										CreatedBy,
										CreatedDate
									)
									VALUES ( 
											@RoleId,
											@RightId,
											@UserId,
											@Now
										   )
								END;

								

							END;
							FETCH NEXT FROM curCreateNewRole INTO @RoleName
						END
						CLOSE curCreateNewRole
						DEALLOCATE curCreateNewRole


				
		END
END
GO

--ClickPay UI Panels
DECLARE @MaxControlId INT, 
		@UserId bigint,
		@Now datetime = GETDATE(), 
		@MaxControlAttributeId INT, 
		@ProductId INT = 48;

SELECT	@UserId = UserId
FROM	Ident.UserLogin
WHERE	LoginName LIKE 'realpagead@%'

IF NOT EXISTS (SELECT TOP 1 1 FROM[UserManagement].[ProductPage] WHERE ProductId = @ProductId)
BEGIN
	SET IDENTITY_INSERT [UserManagement].[Control] ON 

	SELECT @MaxControlId = max(ControlId) from UserManagement.Control

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate]) 
	VALUES (@MaxControlId + 1, NULL, 8, N'ClickPayProductAccessTabGroupUIId', NULL, NULL, 1, @UserId, @Now)

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate]) 
	VALUES (@MaxControlId + 2, @MaxControlId + 1, 9, N'ClickPayProductAccessRolesTabUIId', N'Roles', NULL, 1, @UserId, @Now)

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate]) 
	VALUES (@MaxControlId + 3, @MaxControlId + 2, 3, N'ClickPayProductAccessRolesMultiSelectGridUIId', NULL, NULL, 1, @UserId, @Now)

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate]) 
	VALUES (@MaxControlId + 4, @MaxControlId + 3, 7, N'ClickPayProductAccessRoleRadioLabelUIId', NULL, N'isAssigned', 1, @UserId, @Now)

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate]) 
	VALUES (@MaxControlId + 5, @MaxControlId + 3, 5, N'ClickPayProductAccessRoleLabelUIId', N'Role', N'name', 2, @UserId, @Now)

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate]) 
	VALUES (@MaxControlId + 6, @MaxControlId + 3, 5, N'ClickPayProductAccessOrgTypeLabelUIId', N'Org Type', N'orgType', 3, @UserId, @Now)

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate]) 
	VALUES (@MaxControlId + 7, @MaxControlId + 3, 14, N'ClickPayProductAccesAssignedToIdLinkLabelUIId', N'Assign To', N'orgsAssigned', 4, @UserId, @Now)

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate]) 
	VALUES (@MaxControlId + 8, @MaxControlId + 3, 14, N'ClickPayProductAccessAssignedPropertiesLinkLabelUIId', NULL, N'orgsAssignedName', 5, @UserId, @Now)

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate]) 
	VALUES (@MaxControlId + 9, @MaxControlId + 8, 5, N'ClickPayProductAccessAssignCompaniesLabelUIId', N'Assign Companies', N'assignedCompanies', 1, @UserId, @Now)

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate]) 
	VALUES (@MaxControlId + 10, @MaxControlId + 9, 3, N'ClickPayProductAccessAssignedCompaniesMultiSelectGridUIId', NULL, NULL, 2, @UserId, @Now)

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate]) 
	VALUES (@MaxControlId + 11, @MaxControlId + 10, 10, N'ClickPayProductAccessCompaniesCheckboxUIId', NULL, N'isAssigned', 1, @UserId, @Now)

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate]) 
	VALUES (@MaxControlId + 12, @MaxControlId + 10, 5, N'ClickPayProductAccessCompaniesLabelUIId', N'Company', N'name', 2, @UserId, @Now)

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate]) 
	VALUES (@MaxControlId + 13, @MaxControlId + 8, 5, N'ClickPayProductAccessAssignPropertiesLabelUIId', N'Assign Properties', N'assignedProperties', 1, @UserId, @Now)

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate]) 
	VALUES (@MaxControlId + 14, @MaxControlId + 13, 3, N'ClickPayProductAccessAssignedPropertiesMultiSelectGridUIId', NULL, NULL, 2, @UserId, @Now)

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate]) 
	VALUES (@MaxControlId + 15, @MaxControlId + 14, 10, N'ClickPayProductAccessPropertiesCheckboxUIId', NULL, N'isAssigned', 1, @UserId, @Now)

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate]) 
	VALUES (@MaxControlId + 16, @MaxControlId + 14, 5, N'ClickPayProductAccessPropertiesLabelUIId', N'Property', N'name', 2, @UserId, @Now)

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate]) 
	VALUES (@MaxControlId + 17, @MaxControlId + 14, 5, N'ClickPayProductAccessPropertiesLLCLabelUIId', N'LLC', N'llcName', 3, @UserId, @Now)

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate]) 
	VALUES (@MaxControlId + 18, @MaxControlId + 8, 5, N'ClickPayProductAccessAssignedLLCLabelUIId', N'Assigned LLCs', N'assignedLLCs', 1, @UserId, @Now)

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate]) 
	VALUES (@MaxControlId + 19, @MaxControlId + 18, 3, N'ClickPayProductAccessAssignedLLCMultiSelectGridUIId', NULL, NULL, 2, @UserId, @Now)

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate]) 
	VALUES (@MaxControlId + 20, @MaxControlId + 19, 11, N'ClickPayProductAccessIconLLCUIId', NULL, N'InfoIcon', 3, @UserId, @Now)

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate]) 
	VALUES (@MaxControlId + 21, @MaxControlId + 19, 10, N'ClickPayProductAccessLLCCheckboxUIId', NULL, N'isAssigned', 1, @UserId, @Now)

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate]) 
	VALUES (@MaxControlId + 22, @MaxControlId + 19, 5, N'ClickPayProductAccessLLCLabelUIId', N'LLC', N'name', 2, @UserId, @Now)

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate]) 
	VALUES (@MaxControlId + 23, @MaxControlId + 20, 5, N'ClickPayProductAccessAssignedLLCPropertiesMultiSelectGridUIId', N'Assigned Properties', N'siteList', 1, @UserId, @Now)

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate]) 
	VALUES (@MaxControlId + 24, @MaxControlId + 23, 5, N'ClickPayProductAccessAssignedLLCPropertiesNameGridUIId', N'Properties', N'name', 1, @UserId, @Now)

	SET IDENTITY_INSERT [UserManagement].[Control] OFF
                             
	SET IDENTITY_INSERT [UserManagement].[ControlAttribute] ON 

	SELECT @MaxControlAttributeId = max(ControlAttributeId) from [UserManagement].[ControlAttribute]

	INSERT [UserManagement].[ControlAttribute] ([ControlAttributeId], [ControlId], [Key], [Value], [CreatedBy], [CreatedDate]) 
	VALUES (@MaxControlAttributeId + 1, @MaxControlId + 1, N'Default', N'True', @UserId, @Now)

	INSERT [UserManagement].[ControlAttribute] ([ControlAttributeId], [ControlId], [Key], [Value], [CreatedBy], [CreatedDate]) 
	VALUES (@MaxControlAttributeId + 2, @MaxControlId + 7, N'AssignedTo', N'Slide', @UserId, @Now)

	INSERT [UserManagement].[ControlAttribute] ([ControlAttributeId], [ControlId], [Key], [Value], [CreatedBy], [CreatedDate]) 
	VALUES (@MaxControlAttributeId + 3, @MaxControlId + 9, N'AssignedProperties', N'Slide', @UserId, @Now)

	INSERT [UserManagement].[ControlAttribute] ([ControlAttributeId], [ControlId], [Key], [Value], [CreatedBy], [CreatedDate]) 
	VALUES (@MaxControlAttributeId + 4, @MaxControlId + 13, N'ShowSelectAll', N'True', @UserId, @Now)

	INSERT [UserManagement].[ControlAttribute] ([ControlAttributeId], [ControlId], [Key], [Value], [CreatedBy], [CreatedDate]) 
	VALUES (@MaxControlAttributeId + 5, @MaxControlId + 18, N'ShowSelectAll', N'True', @UserId, @Now)

                             
	SET IDENTITY_INSERT [UserManagement].[ControlAttribute] OFF

	SET IDENTITY_INSERT [UserManagement].[ProductPage] ON 

	INSERT [UserManagement].[ProductPage] ([ProductPageId], [ProductId], [DisplayName], [CreatedBy], [CreatedDate], [IsActive], [ProductPageTypeId]) 
	VALUES (40, @ProductId, N'ClickPay Product Access', @UserId, @Now, 1, 1)

	SET IDENTITY_INSERT [UserManagement].[ProductPage] OFF

	SET IDENTITY_INSERT [UserManagement].[ProductPageControl] ON 

	INSERT [UserManagement].[ProductPageControl] ([ProductPageControlId], [ProductPageId], [ControlId], [CreatedBy], [CreatedDate])
	VALUES (50, 40, @MaxControlId + 1, @UserId, @Now)

	SET IDENTITY_INSERT [UserManagement].[ProductPageControl] OFF
END

GO
IF EXISTS(select top 1 1 from enterprise.product where productid=48 and name = 'Payments')
BEGIN
	update enterprise.product set name = 'ClickPay', Description = 'ClickPay'  where productid=48
END

GO
IF EXISTS (select 1 from Enterprise.Product where ProductId = 60 and Name ='Hospitality As A Service')
BEGIN
	DECLARE @ProductSettingId INT
	update Enterprise.Product set Name = 'Home Sharing', Description='Home Sharing is a business initiative that creates a new and incremental revenue opportunity for RealPage clients (property owners), their residents and for RealPage by listing available residential units on platforms such as Airbnb and enabling short term rental bookings.' where ProductId = 60 and Name ='Hospitality As A Service'
	select @ProductSettingId = ProductSettingId from Enterprise.ProductSetting where ProductId = 60 and ProductSettingTypeId in (select ProductSettingTypeId from Enterprise.ProductSettingType where ProductSettingTypeId = 5) 
	update Enterprise.ProductSetting set Value ='Home Sharing' where ProductSettingId = @ProductSettingId
END
GO
DECLARE @ProductSettingTypeId INT
select @ProductSettingTypeId = ProductSettingTypeId from Enterprise.ProductSettingType where Name = 'Learnmore'

IF EXISTS (SELECT TOP 1 * FROM Enterprise.ProductSetting where ProductId = 60 and ProductSettingTypeId = @ProductSettingTypeId and [Value] !='')
BEGIN
 UPDATE ENTERPRISE.ProductSetting set Value = '' where ProductId = 60 and ProductSettingTypeId = @ProductSettingTypeId 
END
GO

IF EXISTS(select top 1 1 from enterprise.ProductType where ProductTypeGuid = '52169FDA-5C23-495D-B626-8C78BE1CD11C')
BEGIN
	update Enterprise.ProductType set Name = 'ClickPay' where ProductTypeGuid = '52169FDA-5C23-495D-B626-8C78BE1CD11C'
END

GO
DECLARE @ProductSettingTypeId INT
select @ProductSettingTypeId = ProductSettingTypeId from Enterprise.ProductSettingType where Name = 'TitleId'

IF EXISTS (SELECT TOP 1 * FROM Enterprise.ProductSetting where ProductId = 48 and ProductSettingTypeId = @ProductSettingTypeId and [Value] !='')
BEGIN
 UPDATE ENTERPRISE.ProductSetting set Value = 'ClickPay' where ProductId = 48 and ProductSettingTypeId = @ProductSettingTypeId 
END
GO
DECLARE @ProductSettingTypeId INT
select @ProductSettingTypeId = ProductSettingTypeId from Enterprise.ProductSettingType where Name = 'MetatagUniqueId'

IF EXISTS (SELECT TOP 1 * FROM Enterprise.ProductSetting where ProductId = 48 and ProductSettingTypeId = @ProductSettingTypeId and [Value] !='')
BEGIN
 UPDATE ENTERPRISE.ProductSetting set Value = 'ClickPay' where ProductId = 48 and ProductSettingTypeId = @ProductSettingTypeId 
END

GO

IF EXISTS (SELECT TOP 1 * FROM usermanagement.control where uiid = 'ClickPayProductAccessRoleRadioLabelUIId')
BEGIN
 UPDATE usermanagement.control set controltypeid = 10 where uiid = 'ClickPayProductAccessRoleRadioLabelUIId'
END

GO

DECLARE @ProductId INT, 
		@LoginURI NVARCHAR(100), 
		@SigningCertificateThumbprint NVARCHAR(50), 
		@ParentProductTypeId INT, 
		@ProductName NVARCHAR(100)= 'PME Dashboard', 
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
    WHERE Name = 'PME Dashboard'
)
    BEGIN
        EXEC [Enterprise].[CreateProductType] 
             @ProductTypeId = 508, 
             @ParentProductTypeId = @ParentProductTypeId, 
             @Name = @ProductName, 
             @Description = 'PME Dashboard', 
             @ProductTypeGUID = '31F4F399-177E-4BD6-8C05-EDB50B0C0A91';
END;
SET @ProductId = 62;
IF NOT EXISTS
(
    SELECT 1
    FROM Enterprise.Product
    WHERE Name = @ProductName
)
    BEGIN
        EXEC Enterprise.CreateProduct 
             @ProductId = @ProductId, 
             @ProductGUID = 'C2441CBB-F51B-47E1-B8DF-29612117B0C2', 
             @Name = @ProductName, 
             @Description = @ProductName, 
             @ProductTypeId = 507;
        UPDATE Enterprise.Product
          SET 
              BooksProductCode = 'PME'
        WHERE ProductId = @ProductId;
END;


IF @ServerName IN('RCDUSODBSQL001')
    BEGIN
        SET @ProductUrl = 'https://dashboard.realpage.com/';
END;
IF @ServerName = 'RCTUSODBSQL001'
    BEGIN
        SET @ProductUrl = 'https://dashboard.realpage.com/';
END;
IF @ServerName IN('RCQUSODBSQL001', 'RCVEUSODBSQL001', 'RCDUSODBSQL001A', 'RCIUSODBSQL002', 'RCTUSODBSQL001A') -- Need to chnage
    BEGIN
        SET @ProductUrl = 'https://dashboard.realpage.com/';
END;
IF @ServerName IN('RCPGBKDBSQL005A', 'RCPGBKDBSQL005B') -- Need to change
    BEGIN
        SET @ProductUrl = 'https://dashboard.realpage.com/';
END;
INSERT INTO @ProductConfiguration
(SettingName, 
 SettingDescription, 
 SettingValue
)
VALUES
('ClassName',  '',  'pmedashboard'),
('ProductUrl',  '',  @ProductUrl),
('TitleId',  '',  'PME DashBoard'),
('TitleUniqueId',  '',  '0A3D5C5D-B16E-4DAE-87EA-EDA0E9639FBF'),
('IsNewTab',  '', '1'),
('MetatagUniqueId',  '',  'PME Dashboard'),
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
        SET @LoginURL = 'https://dashboard.realpage.com/';
END;
IF @ServerName = 'RCTUSODBSQL001'
    BEGIN
        SET @LoginURL = 'https://dashboard.realpage.com/';
END;
IF @ServerName IN('RCQUSODBSQL001', 'RCVEUSODBSQL001', 'RCDUSODBSQL001A', 'RCIUSODBSQL002', 'RCTUSODBSQL001A') -- Need to change
    BEGIN
        SET @LoginURL = 'https://dashboard.realpage.com/';
END;
IF @ServerName IN('RCPGBKDBSQL005A', 'RCPGBKDBSQL005B') -- Need to change
    BEGIN
        SET @LoginURL = 'https://dashboard.realpage.com/';
END;
SET @ProductID = 62;
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

       SET @RightName = 'AccessPMEDashboard'; 
       SET @RightDescription = 'Access to PME Dashboard';
       SET @RightValue = 'Access to PME Dashboard';
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
    Select @RightId = RightId From [Security].[Right] Where RightName = 'AccessPMEDashboard'

    IF NOT EXISTS (Select 1 From [Security].[RoleRight] Where RoleId = @SuperUserRoleId AND RightId = @RightId)
    BEGIN
        INSERT INTO [Security].[RoleRight]( RoleId,RightId,CreatedBy,CreatedDate)
           VALUES ( @SuperUserRoleId,@RightId,@UserId,@Now)
    END

   
GO
	IF NOT EXISTS (Select 1 From Enterprise.ProductRight Where ProductId = 62)
	BEGIN
		Insert into Enterprise.ProductRight(ProductId,RightShortName,DependantProductId)
		Select 62,'AccessPMEDashboard',NULL
	END

GO
DECLARE @RightValue nvarchar(200),
		 @UserId bigint,
		 @Now datetime = GETDATE(),
		 @RightId int,
		 @RoleId INT,
		 @OrgPartyId int,
		 @RoleTypeId int,
		 @ProductId int =60,
		 @RoleName nvarchar(100),
		 @OrgVisibilityStatusId INT = 9,
		 @RightVisibilityStatusId INT = 9,
		 @StatusTypeId int=13,
		 @ServerName SYSNAME = @@SERVERNAME;

IF @ServerName IN ('RCDUSODBSQL001','rctusodbsql001','RCQUSODBSQL001')
BEGIN		

		DECLARE @TargetRoleValue TABLE (RoleName nvarchar(100))

		INSERT INTO @TargetRoleValue VALUES('Customer Support Manager'),('Customer Support Representative'),
												('Implementations'),('Systems Admin');

	
			--UserId
			SELECT	@UserId = UserId
			FROM	Ident.UserLogin
			WHERE	LoginName LIKE 'realpagead@%'
        SELECT @OrgPartyId=PartyId FROM Enterprise.Organization WHERE [Name]='CF Real Estate Services'
		IF NOT EXISTS
		(
			SELECT TOP 1 1 FROM [Security].[Role]
			WHERE OrgPartyId = @OrgPartyId AND [RoleName] IN (SELECT RoleName FROM @TargetRoleValue)
		)
		BEGIN

				SELECT @RoleTypeId=RoleTypeId from [Security].RoleType WHERE [Value]='Product'
 
					--Cursor Mapping Role with Right
						DECLARE curCreateNewRole CURSOR FOR
						SELECT RoleName
						FROM @TargetRoleValue

						OPEN curCreateNewRole
						FETCH NEXT FROM curCreateNewRole INTO @RoleName

						WHILE @@FETCH_STATUS = 0
						BEGIN
							IF NOT EXISTS (SELECT TOP 1 1 FROM [Security].[Role] WHERE RoleName = @RoleName and OrgPartyID = @OrgPartyId)
							BEGIN
								INSERT INTO [Security].[Role]
								(	RoleName,
									Shortname, 
									Description,
									RoleTypeID,
									OrgPartyID,
									ProductId,
									CreatedBy,
									createdDate
								)
								VALUES ( 
										@RoleName,
										@RoleName,
										@RoleName,
										@RoleTypeId,
										@OrgPartyId,
										@ProductId,
										@UserId,
										@Now
								)
							END
							IF NOT EXISTS (SELECT TOP 1 1 FROM [Security].[Right] WHERE [Value] = @RoleName and VisibilityStatusId = 9)
							BEGIN	
								INSERT INTO [Security].[Right]
											(	RightName,
												Description, 
												Value,
												StatusTypeId,
												VisibilityStatusId,
												ProductId,
												TargetProductId,
												CreatedBy,
												CreatedDate
											)
											VALUES ( 
													REPLACE(@RoleName, ' ', ''),
													@RoleName,
													@RoleName,
													@StatusTypeId, 
													@RightVisibilityStatusId,
													@ProductId,
													@ProductId,
													@UserId,
													@Now
								)
							END
								 
							SELECT @RoleId = RoleId FROM [Security].[Role] WHERE RoleName=@RoleName and OrgPartyID = @OrgPartyId
							SELECT @RightId = RightId FROM [Security].[Right] WHERE [Value]=@RoleName and VisibilityStatusId = 9
							SELECT @RoleId, @RightId
 							IF NOT EXISTS (SELECT TOP 1 1 FROM [Security].[RoleRight] WHERE RoleId = @RoleId AND RightId=@RightId)
							BEGIN
									INSERT INTO [Security].[RoleRight]
									(	RoleId,
										RightId, 
										CreatedBy,
										CreatedDate
									)
									VALUES ( 
											@RoleId,
											@RightId,
											@UserId,
											@Now
										   )
								END
										
							
							FETCH NEXT FROM curCreateNewRole INTO @RoleName
						END
						CLOSE curCreateNewRole
						DEALLOCATE curCreateNewRole


				
		END
 END
 GO
GO

--Add New product setting
GO
if not exists ( select top 1 1 from Enterprise.ProductSettingType where name = 'DisableUsersOnProductCancel' )
begin
	insert into enterprise.ProductSettingType ( name, Description ) values ( 'DisableUsersOnProductCancel', 'Disable all users on product provisioning cancel for a company')
end
-- AuthenticationType , Redirect, SAML, OpenIdCustom, NA

DECLARE @NOW DATETIME = GETUTCDATE(); 
declare @productlist table ( entid int identity, productid int, productsettingtype varchar(500), productsettingvalue varchar(2000))
insert into @productlist values 
	(1,     'DisableUsersOnProductCancel',   '0'),
	(2,     'DisableUsersOnProductCancel',   '0'),
	(3,     'DisableUsersOnProductCancel',   '0'),
	(4,     'DisableUsersOnProductCancel',   '0'),
	(5,     'DisableUsersOnProductCancel',   '0'),
	(6,     'DisableUsersOnProductCancel',   '0'),
	(7,     'DisableUsersOnProductCancel',   '0'),
	(8,     'DisableUsersOnProductCancel',   '0'),
	(9,     'DisableUsersOnProductCancel',   '0'),
	(10,     'DisableUsersOnProductCancel',   '0'),
	(11,     'DisableUsersOnProductCancel',   '0'),
	(12,     'DisableUsersOnProductCancel',   '0'),
	(13,     'DisableUsersOnProductCancel',   '0'),
	(14,     'DisableUsersOnProductCancel',   '0'),
	(15,     'DisableUsersOnProductCancel',   '0'),
	(16,     'DisableUsersOnProductCancel',   '0'),
	(17,     'DisableUsersOnProductCancel',   '0'),
	(18,     'DisableUsersOnProductCancel',   '0'),
	(19,     'DisableUsersOnProductCancel',   '0'),
	(20,     'DisableUsersOnProductCancel',   '0'),
	(21,     'DisableUsersOnProductCancel',   '0'),
	(22,     'DisableUsersOnProductCancel',   '0'),
	(23,     'DisableUsersOnProductCancel',   '0'),
	(24,     'DisableUsersOnProductCancel',   '0'),
	(25,     'DisableUsersOnProductCancel',   '0'),
	(26,     'DisableUsersOnProductCancel',   '0'),
	(27,     'DisableUsersOnProductCancel',   '0'),
	(28,     'DisableUsersOnProductCancel',   '0'),
	(29,     'DisableUsersOnProductCancel',   '0'),
	(30,     'DisableUsersOnProductCancel',   '0'),
	(31,     'DisableUsersOnProductCancel',   '0'),
	(32,     'DisableUsersOnProductCancel',   '0'),
	(33,     'DisableUsersOnProductCancel',   '0'),
	(34,     'DisableUsersOnProductCancel',   '0'),
	(35,     'DisableUsersOnProductCancel',   '0'),
	(36,     'DisableUsersOnProductCancel',   '0'),
	(37,     'DisableUsersOnProductCancel',   '0'),
	(38,     'DisableUsersOnProductCancel',   '0'),
	(39,     'DisableUsersOnProductCancel',   '0'),
	(40,     'DisableUsersOnProductCancel',   '0'),
	(41,     'DisableUsersOnProductCancel',   '0'),
	(42,     'DisableUsersOnProductCancel',   '0'),
	(43,     'DisableUsersOnProductCancel',   '0'),
	(44,     'DisableUsersOnProductCancel',   '0'),
	(45,     'DisableUsersOnProductCancel',   '0'),
	(46,     'DisableUsersOnProductCancel',   '0'),
	(47,     'DisableUsersOnProductCancel',   '0'),
	(48,     'DisableUsersOnProductCancel',   '0'),
	(49,     'DisableUsersOnProductCancel',   '0'),
	(50,     'DisableUsersOnProductCancel',   '0'),
	(51,     'DisableUsersOnProductCancel',   '0'),
	(52,     'DisableUsersOnProductCancel',   '0'),
	(53,     'DisableUsersOnProductCancel',   '0'),
	(54,     'DisableUsersOnProductCancel',   '0'),
	(55,     'DisableUsersOnProductCancel',   '0'),
	(56,     'DisableUsersOnProductCancel',   '0'),
	(57,     'DisableUsersOnProductCancel',   '0'),
	(58,     'DisableUsersOnProductCancel',   '0'),
	(59,     'DisableUsersOnProductCancel',   '0'),
	(60,     'DisableUsersOnProductCancel',   '0')


--select * from @productlist

declare @MAX_ID INT
declare @Current_ID INT = 1
declare @CurrentProductId INT = 1

select @MAX_ID = max(entid) from @productlist

while @Current_ID <= @MAX_ID
begin
	declare @currentSettingType varchar(500)
	declare @currentsettingValue varchar(2000)

	select @CurrentProductId = productid , @currentSettingType = productsettingtype, @currentSettingValue = productsettingvalue
		from @productlist where entid = @Current_ID

	--print 'productid = ' + convert(varchar,@currentproductid)

	if not exists (
	select top 1 1 
		FROM Enterprise.GlobalProductConfiguration gpc  
		JOIN Enterprise.ProductConfiguration pc ON pc.ConfigurationId = gpc.ConfigurationId  
		JOIN Enterprise.ProductSetting ps ON ps.ProductSettingId = pc.ProductSettingId  
		JOIN Enterprise.ProductSettingType pst ON pst.ProductSettingTypeId = ps.ProductSettingTypeId  
			WHERE  gpc.ProductId = @CurrentProductId  
		AND ((@NOW BETWEEN gpc.FromDate AND gpc.ThruDate) OR (@NOW >= gpc.FromDate AND gpc.ThruDate IS NULL))  
		AND ((@NOW BETWEEN pc.FromDate AND pc.ThruDate) OR (@NOW >= pc.FromDate AND pc.ThruDate IS NULL))  
		AND ((@NOW BETWEEN ps.FromDate AND ps.ThruDate) OR (@NOW >= ps.FromDate AND ps.ThruDate IS NULL))  
		AND pst.Name = @currentSettingType
		AND ps.Value = @currentsettingValue
	)
	begin
		declare @currentproductconfigurationid INT
		select distinct top 1 @currentproductconfigurationid = pc.configurationid
			FROM Enterprise.GlobalProductConfiguration gpc  
			JOIN Enterprise.ProductConfiguration pc ON pc.ConfigurationId = gpc.ConfigurationId  
			JOIN Enterprise.ProductSetting ps ON ps.ProductSettingId = pc.ProductSettingId  
			JOIN Enterprise.ProductSettingType pst ON pst.ProductSettingTypeId = ps.ProductSettingTypeId  
				WHERE  gpc.ProductId = @CurrentProductId
			AND ((@NOW BETWEEN gpc.FromDate AND gpc.ThruDate) OR (@NOW >= gpc.FromDate AND gpc.ThruDate IS NULL))  
			AND ((@NOW BETWEEN pc.FromDate AND pc.ThruDate) OR (@NOW >= pc.FromDate AND pc.ThruDate IS NULL))  
			AND ((@NOW BETWEEN ps.FromDate AND ps.ThruDate) OR (@NOW >= ps.FromDate AND ps.ThruDate IS NULL))  
		order by pc.ConfigurationId desc

		if (@currentproductconfigurationid is not null)
		begin
			insert into enterprise.ProductSetting ( productid, ProductSettingTypeId, value, FromDate )
				select @CurrentProductId, productsettingtypeid, @currentSettingValue, GETUTCDATE()
					from enterprise.ProductSettingType where name = @currentSettingType
			insert into enterprise.ProductConfiguration ( ConfigurationId, ProductSettingId, FromDate, ThruDate )
				values ( @currentproductconfigurationid, @@IDENTITY, GETUTCDATE(), null )
		end
	end
	
	set @Current_ID = @Current_ID + 1
end
GO
 DECLARE @RightValue nvarchar(200),
		 @UserId bigint,
		 @Now datetime = GETDATE(),
		 @RightId int,
		 @RoleId INT,
		 @ProductId int = 3,
		 @TargetProductId int = 60,
		 @RoleName nvarchar(100),
		 @OrgVisibilityStatusId INT = 9,
		 @RightVisibilityStatusId INT =9,
		 @StatusTypeId int=13;
		
 
	
	--UserId
	SELECT	@UserId = UserId
	FROM	Ident.UserLogin
	WHERE	LoginName LIKE 'realpagead@%'
IF NOT EXISTS(SELECT TOP 1 1 FROM [Security].[Right] WHERE [Value] ='Manage Home Sharing Product Access')
BEGIN 
		INSERT INTO Security.[Right] (RightName,Description,Value,StatusTypeId,VisibilityStatusId,ProductId,TargetProductId,CreatedBy,CreatedDate)
		VALUES('ManageHomeSharingProductAccess','Manage Home Sharing Product Access','Manage Home Sharing Product Access',@StatusTypeId,@RightVisibilityStatusId,@ProductId ,@TargetProductId,@UserId,@Now)
		SELECT @RoleId = RoleId from [Security].[Role] where RoleName='User Administrator'
		SELECT @RightId =  RightId from [Security].[Right] where [Value] = 'Manage Home Sharing Product Access' 
		IF NOT EXISTS(SELECT TOP 1 1 FROM [Security].[RoleRight] WHERE [RightId]= @RightId)
		BEGIN
		 INSERT INTO Security.RoleRight (RoleId,RightId,CreatedBy,CreatedDate) 
		 VALUES(@RoleId,@RightId,@UserId,@Now)
		END
END
GO

DECLARE @ControlId int,
        @Now Datetime = GETDate();

IF EXISTS(SELECT 1 FROM [UserManagement].[Control] WHERE UIId ='ILMLeadManagementProductAccessPropertiesTabUIId')
BEGIN
  SELECT @ControlId=ControlId FROM [UserManagement].[Control] WHERE UIId ='ILMLeadManagementProductAccessPropertiesTabUIId'
  select @ControlId
  IF NOT EXISTS(SELECT 1 FROM [UserManagement].[Control] WHERE UIId ='ILMLeadManagementProductAccessAllowCurrentandFuturePropertiesSwitchUIId')
  BEGIN
	   INSERT INTO [UserManagement].[Control] Values(@ControlId,1,'ILMLeadManagementProductAccessAllowCurrentandFuturePropertiesSwitchUIId',
	   'Assign current and new properties automatically','allProperties',1, 480, @Now)
	  select @ControlId
	  UPDATE  UserManagement.[Control] SET  Sequence=2 WHERE UIId='ILMLeadManagementProductAccessPropertiesMultiSelectGridUIId'
  END
 
END

IF EXISTS(SELECT 1 FROM [UserManagement].[Control] WHERE UIId ='ILMLeasingAnalyticsProductAccessPropertiesTabUIId')
BEGIN
  SELECT @ControlId=ControlId FROM [UserManagement].[Control] WHERE UIId ='ILMLeasingAnalyticsProductAccessPropertiesTabUIId'
  IF NOT EXISTS(SELECT 1 FROM [UserManagement].[Control] WHERE UIId ='ILMLeasingAnalyticsProductAccessAllowCurrentandFuturePropertiesSwitchUIId')
  BEGIN
	   INSERT INTO [UserManagement].[Control] Values(@ControlId,1,'ILMLeasingAnalyticsProductAccessAllowCurrentandFuturePropertiesSwitchUIId',
	   'Assign current and new properties automatically','allProperties',1, 480, @Now)
	  select @ControlId
	  UPDATE  UserManagement.[Control] SET  Sequence=2 WHERE UIId='ILMLeasingAnalyticsProductAccessPropertiesMultiSelectGridUIId'
  END
 
END
GO
DECLARE @ProductSettingTypeId INT
select @ProductSettingTypeId = ProductSettingTypeId from Enterprise.ProductSettingType where Name = 'Learnmore'
IF EXISTS (SELECT TOP 1 * FROM Enterprise.ProductSetting where ProductId = 48 and ProductSettingTypeId = @ProductSettingTypeId)
BEGIN
 UPDATE ENTERPRISE.ProductSetting set Value = 'https://site.clickpay.com/for-property-managers/' where ProductId = 48 and ProductSettingTypeId = @ProductSettingTypeId 
END
GO
IF EXISTS(select top 1 1 from enterprise.product where productid=48)
BEGIN
	update enterprise.product set  Description = 'Payments - Open Market Solution'  where productid=48
END
GO

IF EXISTS(SELECT * FROM [Security].[Right] WHERE RightName ='HelpCenterContactSupport')
BEGIN
  ---RENAME Right name
	  IF NOT EXISTS(SELECT * FROM [Security].[Right] WHERE [Value] ='Simon Help Center Contact Support')
		BEGIN  
		   UPDATE [Security].[Right] SET [Value] ='Simon Help Center Contact Support'
			WHERE RightName ='HelpCenterContactSupport'
		END
	-- Visiable across All PMCs
	UPDATE [Security].[Right] SET VisibilityStatusId = 9
    WHERE RightName ='HelpCenterContactSupport'
END
GO

DECLARE @RightId INT;
SELECT @RightId = RightId FROM [Security].[Right] WHERE rightname = 'ManageIntelligentBuildingProductAccess';

IF EXISTS(SELECT TOP 1 1 FROM [Security].[Right] WHERE rightname = 'ManageIntelligentBuildingProductAccess')
BEGIN
	UPDATE [Security].[Right] SET [Value] = 'Manage Waste Management Solution Product Access', 
								  [Description] = 'Manage Waste Management Solution Product Access',
								  [RightName] = 'ManageIntelligentBuildingTrashProductAccess'
							  WHERE RightId = @RightId;
END

GO
DECLARE @RightValue nvarchar(200),
		 @UserId bigint,
		 @Now datetime = GETDATE(),
		 @RightId int,
		 @RoleId INT,
		 @OrgPartyId int,
		 @RoleTypeId int,
		 @ProductId int =60,
		 @RoleName nvarchar(100),
		 @OrgVisibilityStatusId INT = 9,
		 @RightVisibilityStatusId INT = 9,
		 @StatusTypeId int=13,
		 @ServerName SYSNAME = @@SERVERNAME;
IF @ServerName IN ('RCDUSODBSQL001','rctusodbsql001','RCQUSODBSQL001')
BEGIN		
		DECLARE @TargetRoleValue TABLE (RoleName nvarchar(100))
		INSERT INTO @TargetRoleValue VALUES('Property Admin'),('Property User');
	
			--UserId
			SELECT	@UserId = UserId
			FROM	Ident.UserLogin
			WHERE	LoginName LIKE 'realpagead@%'
        SELECT @OrgPartyId=PartyId FROM Enterprise.Organization WHERE [Name]='CAMDEN DEVELOPMENT, INC.'
		IF NOT EXISTS
		(
			SELECT TOP 1 1 FROM [Security].[Role]
			WHERE OrgPartyId = @OrgPartyId AND [RoleName] IN (SELECT RoleName FROM @TargetRoleValue)
		)
		BEGIN
				SELECT @RoleTypeId=RoleTypeId from [Security].RoleType WHERE [Value]='Product'
 
					--Cursor Mapping Role with Right
						DECLARE curCreateNewRole CURSOR FOR
						SELECT RoleName
						FROM @TargetRoleValue
						OPEN curCreateNewRole
						FETCH NEXT FROM curCreateNewRole INTO @RoleName
						WHILE @@FETCH_STATUS = 0
						BEGIN
							IF NOT EXISTS (SELECT TOP 1 1 FROM [Security].[Role] WHERE RoleName = @RoleName and OrgPartyID = @OrgPartyId)
							BEGIN
								INSERT INTO [Security].[Role]
								(	RoleName,
									Shortname, 
									Description,
									RoleTypeID,
									OrgPartyID,
									ProductId,
									CreatedBy,
									createdDate
								)
								VALUES ( 
										@RoleName,
										@RoleName,
										@RoleName,
										@RoleTypeId,
										@OrgPartyId,
										@ProductId,
										@UserId,
										@Now
								)
							END
							IF NOT EXISTS (SELECT TOP 1 1 FROM [Security].[Right] WHERE [Value] = @RoleName and VisibilityStatusId = 9)
							BEGIN	
								INSERT INTO [Security].[Right]
											(	RightName,
												Description, 
												Value,
												StatusTypeId,
												VisibilityStatusId,
												ProductId,
												TargetProductId,
												CreatedBy,
												CreatedDate
											)
											VALUES ( 
													REPLACE(@RoleName, ' ', ''),
													@RoleName,
													@RoleName,
													@StatusTypeId, 
													@RightVisibilityStatusId,
													@ProductId,
													@ProductId,
													@UserId,
													@Now
								)
							END
								 
							SELECT @RoleId = RoleId FROM [Security].[Role] WHERE RoleName=@RoleName and OrgPartyID = @OrgPartyId
							SELECT @RightId = RightId FROM [Security].[Right] WHERE [Value]=@RoleName and VisibilityStatusId = 9
							SELECT @RoleId, @RightId
 							IF NOT EXISTS (SELECT TOP 1 1 FROM [Security].[RoleRight] WHERE RoleId = @RoleId AND RightId=@RightId)
							BEGIN
									INSERT INTO [Security].[RoleRight]
									(	RoleId,
										RightId, 
										CreatedBy,
										CreatedDate
									)
									VALUES ( 
											@RoleId,
											@RightId,
											@UserId,
											@Now
										   )
								END
										
							
							FETCH NEXT FROM curCreateNewRole INTO @RoleName
						END
						CLOSE curCreateNewRole
						DEALLOCATE curCreateNewRole
				
		END
 END
 GO
 DECLARE @RightId INT,
		@BasicEndUserRoleId INT,
		@UserAdminRoleId INT,
		@UPRoleId INT,
		@UserId bigint,
		@Now datetime = GETDATE()
SELECT	@UserId = UserId
			FROM	Ident.UserLogin
			WHERE	LoginName LIKE 'realpagead@%'
IF EXISTS (SELECT TOP 1 1 FROM Security.[Right] where [Value] = 'Access to Help Center' AND RightName = 'AccessHelpCenter')
BEGIN
	select @RightId = RightId from Security.[Right] where [Value] = 'Access to Help Center' AND RightName = 'AccessHelpCenter'
	UPDATE Security.[Right] SET [Value] = 'Access to Simon Help Center' WHERE RightId = @RightId
	
	select @BasicEndUserRoleId = RoleId from security.role where rolename = 'Basic End User' and OrgPartyID IS NULL
	
	IF NOT EXISTS (SELECT TOP 1 1 FROM Security.RoleRight WHERE RoleId = @BasicEndUserRoleId AND @RightId = RightId)
	BEGIN
	 INSERT INTO SECURITY.[RoleRight] (RoleId,RightId,CreatedBy,CreatedDate) 
	 VALUES(@BasicEndUserRoleId,@RightId,@UserId,@Now)
	END
	select @UPRoleId = RoleId from security.role where rolename = 'Read only for Unified Platform' and OrgPartyID IS NULL
	
	IF NOT EXISTS (SELECT TOP 1 1 FROM Security.RoleRight WHERE RoleId = @UPRoleId AND @RightId = RightId)
	BEGIN
		 INSERT INTO SECURITY.[RoleRight] (RoleId,RightId,CreatedBy,CreatedDate) 
		 VALUES(@UPRoleId,@RightId,@UserId,@Now)
	END
	select @UserAdminRoleId = RoleId from security.role where rolename = 'User Administrator' and OrgPartyID IS NULL
	
	IF NOT EXISTS (SELECT TOP 1 1 FROM Security.RoleRight WHERE RoleId = @UserAdminRoleId AND @RightId = RightId)
	BEGIN
		 INSERT INTO SECURITY.[RoleRight] (RoleId,RightId,CreatedBy,CreatedDate) 
		 VALUES(@UserAdminRoleId,@RightId,@UserId,@Now)
	END
END
GO
IF EXISTS (SELECT TOP 1 1 FROM Security.[Right] WHERE RightName = 'AccessPMEDashboard' AND VALUE = 'Access to Help Center')
BEGIN
    UPDATE Security.[Right] SET Description='Access to PME Dashboard',VALUE = 'Access to PME Dashboard' WHERE RightName = 'AccessPMEDashboard' AND VALUE = 'Access to Help Center'
END
GO