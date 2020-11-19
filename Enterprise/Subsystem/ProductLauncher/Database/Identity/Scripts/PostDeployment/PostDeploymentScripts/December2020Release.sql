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

/*	This script to store Address and ContractedName in Enterprise.CompanyAddress and  Enterprise.CompanyContractedName tables */

GO
DECLARE @tempCompany table(
		UPFMID [nvarchar](50) NULL,
		CompanyMasterId bigint,
		[ContractedName] [nvarchar](200) NOT NULL,
		[Address] [nvarchar](400) NULL,
		[City] [nvarchar](120) NULL,
		[State] [nvarchar](40) NULL,
		[PostalCode] [nvarchar](50) NULL,
		[county] [nvarchar](120) NULL,
		[country] [nvarchar](50) NULL
	)
INSERT INTO @tempCompany(
		UPFMID,
		CompanyMasterId ,
		[ContractedName],
		[Address],
		[City] ,
		[State],
		[PostalCode],
		[county],
		[country]
		)

SELECT '9f9dbea4-b4ea-4477-97bd-0bc13fe1de8b','6406','100 CHADWICK AVE, LLC','100 CHADWICK AVE','NEWARK','NJ','07108-1571','','UNITED STATES' UNION
SELECT '684382d3-f2f8-4f42-8d29-935f834c6888','775','121 7TH STREET, LLC','121 7TH ST','PITTSBURGH','PA','15222-3403','ALLEGHENY COUNTY','UNITED STATES' UNION
SELECT '9ff3595a-6ac7-4231-ad1e-936393f7fe62','5343','','','','','','','' UNION
SELECT 'cef7d224-bb63-4179-8584-44d1898a79d3','1379957','','','','','','','' UNION
SELECT 'dc0bbd41-d2ed-497f-92cc-ae7e98806197','1367077','15534 CHASE LLC','15534 CHASE ST','NORTH HILLS','CA','91343-6567','LOS ANGELES COUNTY','UNITED STATES' UNION
SELECT '8e14a003-1d7b-4309-a3ce-cff6f99fbbb3','16248','180 MULTIFAMILY MGMT, LLC','101 E RANDOL MILL RD SUITE 106','ARLINGTON','TX','76011','TARRANT COUNTY','UNITED STATES' UNION
SELECT '745ba4f9-6f3b-4259-8e7d-db6509c95087','1378076','1805 APARTMENTS LLC','731 GAIL CHAMBERS RD','JACKSON','NJ','08527-3951','OCEAN COUNTY','UNITED STATES' UNION
SELECT '7eb1bfe7-e91e-464b-bc6d-2fb9d77f07a2','997','1ST CITY, LLC','28 W ADAMS AVE STE 900  PAINIA DEVELOPMENT CORP','DETROIT','MI','48226-1664','WAYNE COUNTY','UNITED STATES' UNION
SELECT '15634071-bc0c-4005-875f-6e1a6c38421d','22927','2755 E LEDBETTER HIVE PARTNERS LLC','2755 E LEDBETTER DR','DALLAS','TX','75216-7570','DALLAS COUNTY','UNITED STATES' UNION
SELECT '183588e2-8041-4ce3-a487-a0de2ed5a774','14988','29TH STREET CAPITAL','343 W ERIE ST STE 300','CHICAGO','IL','60654-5735','COOK COUNTY','UNITED STATES' UNION
SELECT 'b4c921dd-0436-4b9e-8dad-b8c22265e3de','6928','','','','','','','' UNION
SELECT 'b960775d-7b90-4fe0-b8ca-7d024d9f575e','1374647','3612 CUMING STREET LP','3612 CUMING ST','OMAHA','NE','68131-1952','DOUGLAS COUNTY','UNITED STATES' UNION
SELECT 'bdc1f027-3379-44c2-988e-e67d6fb7c61e','6882','5305 MARQUIS, INC','4445 BUENA VISTA ST','DALLAS','TX','75205-4118','DALLAS COUNTY','UNITED STATES' UNION
SELECT 'a6705fce-9b0c-4457-8333-ac409b28d8a7','5469','','','','','','','' UNION
SELECT '7215a2a7-f920-4461-bc10-691ef216c3f1','26689','601 WEST MANAGEMENT CORP.','601 W 26TH ST RM 1275','NEW YORK','NY','10001-1120','NEW YORK COUNTY','UNITED STATES' UNION
SELECT 'b109aa7c-fa64-4b06-b18e-2fc065133a13','8959','722 SUNSET BLVD LLC','1425 ORLANDO DR','ARCADIA','CA','91006-2108','LOS ANGELES COUNTY','UNITED STATES' UNION
SELECT '1789133d-5aeb-4201-96cd-398a49595eec','5512','750 IMHOFF, LLC','750 W IMHOFF RD','NORMAN','OK','73072-7536','CLEVELAND COUNTY','UNITED STATES' UNION
SELECT '6b3c45d3-d637-427e-976a-d2d1aba4560f','1368825','8TH STREET DEVELOPERS, LLC.','1370 E 8TH ST STE 1','TEMPE','AZ','85281-4383','MARICOPA COUNTY','UNITED STATES' UNION
SELECT '554892d2-d8bb-41b3-b4c5-f9303baed903','7492','A R BUILDING COMPANY','310 SEVEN FIELDS BLVD STE 350','SEVEN FIELDS','PA','16046-4343','','UNITED STATES' UNION
SELECT 'a1188730-8ea1-47aa-8bf1-1d94683a44a2','8177','A&G MANAGEMENT','7779 NEW YORK LN','GLEN BURNIE','MD','21061-5616','ANNE ARUNDEL COUNTY','UNITED STATES' UNION
SELECT 'b5ce5a1c-b271-43f5-bd91-f1125b522cd4','1367','ABACUS CAPITAL GROUP LLC','420 LEXINGTON AVE STE 2821','NEW YORK','NY','10170-0002','NEW YORK COUNTY','UNITED STATES' UNION
SELECT '7daa620a-a214-4706-b17f-6e43510a2cd8','5993','ABBEY RESIDENTIAL SERVICES INC','1930 STONEGATE DR','BIRMINGHAM','AL','35242-2523','JEFFERSON COUNTY','UNITED STATES' UNION
SELECT '6c0ee973-c113-4db3-b929-31f9a5b3d5ac','5464','ABCDE REALTY MANAGEMENT LLC','2800 W PARKER RD STE 110','PLANO','TX','75075-9190','COLLIN COUNTY','UNITED STATES' UNION
SELECT 'a5192c7e-934f-4ae4-92d4-47518d3cff08','1379360','ABODE WELL REALTY, INC','127 CRAIGHEAD ST','PITTSBURGH','PA','15211-2400','ALLEGHENY COUNTY','UNITED STATES' UNION
SELECT '6a8e171d-fc2f-4201-a018-84f1d394353b','1379960','','','','','','','' UNION
SELECT '17043e8f-452e-4c8d-8e47-8fafe5391bad','8195','ACC MANAGEMENT GROUP, INC.','2375 STATE ROAD 44 STE A','OSHKOSH','WI','54904-6333','WINNEBAGO COUNTY','UNITED STATES' UNION
SELECT '83dac9d6-bc7c-45f0-b0d3-fe27ac309de8','786','','','','','','','' UNION
SELECT '8067a833-4d83-4cba-9d5f-09fc0e99a57d','6718','','','','','','','' UNION
SELECT '96a60c16-9f01-4e06-a1a3-da089d0465c0','14701','ACCESSIBLE APARTMENTS OF NEWPORT NEWS INC','12750 NETTLES DR','NEWPORT NEWS','VA','23606-1874','NEWPORT NEWS CITY','UNITED STATES' UNION
SELECT 'a8985816-b84e-47f6-b4a1-64a185ee1986','7916','ACCORD MANAGEMENT LLC','1263 E BROAD ST','COLUMBUS','OH','43205-1429','FRANKLIN COUNTY','UNITED STATES' UNION
SELECT '545e04c0-2904-4790-a6d7-614ec9329962','1260','ACENTO REAL ESTATE PARTNERS, LLC','8120 WOODMONT AVE STE 520','BETHESDA','MD','20814-2760','MONTGOMERY COUNTY','UNITED STATES' UNION
SELECT '30808eb8-be61-4725-9a59-cd0a5ab34df1','1245','ADA S. MCKINLEY COMMUNITY SERVICES INC.','1359 W WASHINGTON BLVD','CHICAGO','IL','60607-1905','COOK COUNTY','UNITED STATES' UNION
SELECT '8efe3c79-1d61-48d4-8e9d-830fae6b4fa6','8791','ADS REALTY INVESTMENTS LLC','3928 AMBASSADOR DR','PALM HARBOR','FL','34685-1071','PINELLAS COUNTY','UNITED STATES' UNION
SELECT 'eb829005-a62f-44a6-b693-3e403dc9e1f7','6659','','','','','','','' UNION
SELECT '2c2fff00-6f88-48d5-bb2d-dce28218a339','1299','ADVENIR LIVING','17501 BISCAYNE BLVD STE 300','AVENTURA','FL','33160-4809','MIAMI-DADE COUNTY','UNITED STATES' UNION
SELECT 'b577873b-3e23-4b88-9f76-165006705202','5739','AD-WEST REALTY SERVICES, INC.','545 RAINIER BLVD N STE 9','ISSAQUAH','WA','98027-2806','KING COUNTY','UNITED STATES' UNION
SELECT 'c3c65d12-4988-4518-b54f-857f9acd70af','9393','AFFORDABLE PROPERTY MANAGEMENT SPECIALISTS, LLC','9300 S ASHLAND AVE','CHICAGO','IL','60620-5119','COOK COUNTY','UNITED STATES' UNION
SELECT '952ae256-ece6-4d78-b791-3ff70cf3d5a2','1190','AG613, LLC DBA EIGHTEEN CAPITAL GROUP','11615 ROSEWOOD ST STE 100','LEAWOOD','KS','66211-2000','JOHNSON COUNTY','UNITED STATES' UNION
SELECT 'ffde0339-c65c-4c79-a417-279688bc551e','6545','AIMCO/BETHESDA HOLDINGS, INC','4582 S ULSTER ST STE 1100','DENVER','CO','80237-2662','DENVER COUNTY','UNITED STATES' UNION
SELECT '8d0af75e-d1ae-44c9-b30a-f0cd4656a969','1378243','N/A','N/A','N/A','N/A','N/A','N/A','N/A' UNION
SELECT 'c51c1688-275c-44cc-bfed-5cac6a554db7','246','AJH MANAGEMENT LLC','101 CHASE AVE STE 101','LAKEWOOD','NJ','08701-4761','OCEAN COUNTY','UNITED STATES' UNION
SELECT '70d5897d-6002-43ec-9762-d3f8086e0e2f','1379958','','','','','','','' UNION
SELECT '4d27b8a8-0417-42ce-925d-6c48e09ffe82','5616','AJO REALTY GROUP, LLC','711 GORMAN AVE STE A','LAUREL','MD','20707-3911','PRINCE GEORGE`S COUNTY','UNITED STATES' UNION
SELECT '49eaa06e-c6d6-4adb-9774-d08acb05547f','1379959','','','','','','','' UNION
SELECT 'd6415142-7bc0-4f80-9ceb-8faec44027e3','7303','AKELIUS REAL ESTATE MANAGEMENT','101 FEDERAL ST STE 1900','BOSTON','MA','02110-1861','SUFFOLK COUNTY','UNITED STATES' UNION
SELECT '9640c8cc-13c5-4e05-89d2-cc29c23879a5','1377136','AKIVA HOLDINGS, LLC','1661 COUNTY ROAD 313','JARRELL','TX','76537-1813','WILLIAMSON COUNTY','UNITED STATES' UNION
SELECT 'cd7cc4e1-b26c-4465-a140-d57dd046b1d8','6060','ALCOLE PROPERTIES INC.','2321 ROSECRANS AVE STE 4250','EL SEGUNDO','CA','90245-4962','','UNITED STATES' UNION
SELECT 'e5633c8a-be61-4f6b-9949-a7165c280e08','18016','ALDON MANAGEMENT CORPORATION','8180 WISCONSIN AVE','BETHESDA','MD','20814-3624','MONTGOMERY COUNTY','UNITED STATES' UNION
SELECT '635a98e9-dbcc-4011-9ae1-9cdd0197f49f','5224','ALLAN LTD','350 N HARRISON RD','TUCSON','AZ','85748-3235','PIMA COUNTY','UNITED STATES' UNION
SELECT '4da7a432-7ff2-45e3-8490-2fec2af1021e','1348','ALLEN & ROCKS, INC','1960 GALLOWS RD STE 300','VIENNA','VA','22182-3827','FAIRFAX COUNTY','UNITED STATES' UNION
SELECT 'ada9f2bf-04f6-4b23-a643-217c70826b0f','6546','ALLEN HARRISON PROPERTY MANAGEMENT, LLC','1800 AUGUSTA DR STE 170','HOUSTON','TX','77057-3130','HARRIS COUNTY','UNITED STATES' UNION
SELECT '52a5d3de-864b-46b5-9664-23df19722d56','6693','ALLIANCE RESIDENTIAL COMPANY','2525 E CAMELBACK RD STE 500','PHOENIX','AZ','85016-4227','MARICOPA COUNTY','UNITED STATES' UNION
SELECT '656e61c9-4019-4c8b-a16f-d4c7eb154be2','8082','ALLTRADE SERVICE SOLUTIONS LLC','710 BARRET AVE','LOUISVILLE','KY','40204-1750','JEFFERSON COUNTY','UNITED STATES' UNION
SELECT '650409bb-1fac-403c-b6bd-838294d6e86e','17348','ALPERT GROUP LLC','1 PARKER PLZ STE 6','FORT LEE','NJ','07024-2920','','UNITED STATES' UNION
SELECT '7528c90a-5223-48fa-a3d5-7ace5e77c505','781','ALPHA CAPITAL PARTNERS LLC','44 ABELE RD  BEACON 1, SUITE 304','BRIDGEVILLE','PA','15017-3458','ALLEGHENY COUNTY','UNITED STATES' UNION
SELECT '6d335e9b-7e67-47a5-b355-e68c06efda04','5530','ALPHA MANAGEMENT AND MAINTENANCE','2069 SAN BERNARDINO AVE','COLTON','CA','92324-7413','ORANGE COUNTY','United States' UNION
SELECT '4468b7ec-067a-466d-a6e8-74417a0782f8','5687','ALPHA MANAGEMENT PARTNERS, LLC','2851 JOHNSTON ST STE 328','LAFAYETTE','LA','70503-3243','LAFAYETTE PARISH','UNITED STATES' UNION
SELECT '64e3d2f7-8c4b-45e1-80fa-c05ad9e9e996','6557','ALPHA-BARNES REAL ESTATE SERVICES II, LLC','12720 HILLCREST ROAD  STE 400','DALLAS','TX','75230-2087','','UNITED STATES' UNION
SELECT 'bcba10b4-3d6a-45c8-bb93-8eff09d8534b','15291','ALPOR PROPERTIES INC','1500 LAWNDALE PLZ','HOUSTON','TX','77023-4215','HARRIS COUNTY','UNITED STATES' UNION
SELECT '3bb1f45c-044c-41cf-97ac-b3d9a75433d1','1380394','ALTA VISTA HEALTHCARE, INC','9635 MONTE VISTA AVE STE 201','MONTCLAIR','CA','91763-2235','SAN BERNARDINO COUNTY','UNITED STATES' UNION
SELECT 'f28f87aa-23db-400b-89b4-f396cb9e47da','1259','ALTON MANAGEMENT CORPORATION','5530 ALTON AVE','DALLAS','TX','75214-5105','DALLAS COUNTY','UNITED STATES' UNION
SELECT '1e82dc65-f8c5-4604-a2b7-e5a0c861d7fa','20712','','','','','','','' UNION
SELECT '07067037-e4f5-4e23-b4b1-b93aa941e67e','6289','AMBLING PROPERTY INVESTMENTS, LLC','348 ENTERPRISE DR','VALDOSTA','GA','31601-5169','LOWNDES COUNTY','UNITED STATES' UNION
SELECT '247aa13a-aa78-47bf-ad41-3b12b2be0229','1380095','AMBO PROPERTIES LLC','101 CASE RD STE 105','LAKEWOOD','NJ','8701','OCEAN COUNTY','UNITED STATES' UNION
SELECT '9e812063-7be3-4e6e-b9a1-4e47fd095acc','6509','AMERICAN CAMPUS COMMUNITIES OPERATING PARTNERSHIP LP','12700 HILL COUNTRY BLVD STE T-200','AUSTIN','TX','78738-6307','TRAVIS COUNTY','UNITED STATES' UNION
SELECT '84dcd689-651d-4139-be18-1c4c9474f1d2','31574','AMERICAN COMMUNITY MANAGEMENT','7484 CANDLEWOOD RD STE H','HANOVER','MD','21076-3103','ANNE ARUNDEL COUNTY','UNITED STATES' UNION
SELECT 'bd19e2fb-bdd5-4c7e-9baa-7b302676f0b0','29311','AMERICAN HOUSING REALTY LLC','3015 BRIGHTON 13TH ST APT 1D','BROOKLYN','NY','11235-5413','KINGS COUNTY','UNITED STATES' UNION
SELECT '0332f5b9-4941-4bec-9c7f-56338d381ddf','813','AMERICAN LANDMARK MANAGEMENT LLC','4890 W KENNEDY BLVD STE 240','TAMPA','FL','33609-2587','HILLSBOROUGH COUNTY','UNITED STATES' UNION
SELECT 'e8d8d459-4204-4f4b-ba62-b2dc8c4b7948','1930','AMERICAN PROPERTY MANAGEMENT, INC.','110 110TH AVE NE STE 550','BELLEVUE','WA','98004-5854','KING COUNTY','UNITED STATES' UNION
SELECT '330add31-08a9-4407-aad9-1b46226f91ab','6778','AMERICAN REALTY INVESTORS','11811 NORTH FWY STE 300','HOUSTON','TX','77060-3238','HARRIS COUNTY','UNITED STATES' UNION
SELECT '34fcaf56-2df1-45cb-8fd0-94c9af75bd45','5787','AMERICAN RESIDENTIAL INVESTMENT MANAGEMENT, LLC','2409 COUNTRYWOOD RD','RALEIGH','NC','27615-1218','WAKE COUNTY','UNITED STATES' UNION
SELECT 'c068f69a-6c2f-4b21-9a45-700c9511e8aa','5421','AMERILAND REALTY INC','800 BRICKELL AVE','MIAMI','FL','33131-2902','','UNITED STATES' UNION
SELECT '5799074c-50d0-4afe-9a1e-02ae7180c03c','278','AMERIMAR ENTERPRISES','50 S 16TH ST  TWO LIBERTY PL, STE 3500','PHILADELPHIA','PA','19102-2513','PHILADELPHIA COUNTY','UNITED STATES' UNION
SELECT 'ff7af14d-359b-4ba3-b566-97fd176a3e5a','6725','AMESBURY MANAGEMENT, LLC','400 CONVENTION ST STE 1050','BATON ROUGE','LA','70802-5638','EAST BATON ROUGE PARISH','UNITED STATES' UNION
SELECT '8df1e0e0-da65-4a3f-957c-cd578266a462','14','AMHERST APARTMENTS','10440 VALLEY FORGE DR APT 9','HOUSTON','TX','77042-1918','HARRIS COUNTY','UNITED STATES' UNION
SELECT '29fd38a5-18db-463a-939e-e7475deec4cd','5209','AMLI MANAGEMENT COMPANY','141 W JACKSON BLVD STE 300','CHICAGO','IL','60604-3123','COOK COUNTY','UNITED STATES' UNION
SELECT '98876812-60a5-4a09-8ed3-7f54838a6cd8','656','AMP RESIDENTIAL MANAGEMENT LLC','920 N SHADELAND AVE STE G1','INDIANAPOLIS','IN','46219-4817','MARION COUNTY','UNITED STATES' UNION
SELECT '8712494d-5fff-4874-a33e-4c617787be0f','6361','AMURCON REALTY CO','1111 E MAIN ST STE 1100','RICHMOND','VA','23219-3520','RICHMOND CITY','UNITED STATES' UNION
SELECT 'b27c5e46-25b5-45df-aaea-a0b33a44aaf3','3357','ANCHOR GROUP MANAGEMENT, INC','630 N CHURCH ST STE 101','ROCKFORD','IL','61103-7203','WINNEBAGO COUNTY','UNITED STATES' UNION
SELECT '84636eef-cf24-456f-ab21-e83b9022cdd1','1063','ANCHOR INVESTMENTS WEST, INC.','5583 GATLIN AVE','ORLANDO','FL','32812-7735','ORANGE COUNTY','UNITED STATES' UNION
SELECT '1fbc47bd-3234-448e-8115-c8add36a6f77','1378625','ANDMARK MANAGEMENT COMPANY, LLC','10990 WILSHIRE BLVD STE 420','LOS ANGELES','CA','90024-3929','LOS ANGELES COUNTY','UNITED STATES' UNION
SELECT '2ba79ba8-7a98-485d-8b49-1b06732c52ce','1372124','','','','','','','' UNION
SELECT '6cda0901-2574-4371-bcd2-acc3cfb800a1','1371966','','','','','','','' UNION
SELECT 'd28ded93-79fd-48e4-8c18-433c22198056','3720','ANDREW DURKIN','4318 SOMERVILLE BAY','SAN ANTONIO','TX','78244-1350','BEXAR COUNTY','UNITED STATES' UNION
SELECT 'e4524036-6713-44e6-b726-d2910baa60e4','2003','ANTELOPE EXCLUSIVE MANAGEMENT','3131 TURTLE CREEK BLVD STE 907','DALLAS','TX','75219-5406','DALLAS COUNTY','UNITED STATES' UNION
SELECT 'f8e36fa1-9a0a-487d-9e15-a456d4682574','5764','','','','','','','' UNION
SELECT '34e37af5-8e66-4b9d-b6d1-11856a1111f6','623','AON WAKEFOREST, INC','307 AYLMER','PETERBOROUGH','ON','K9J 7M4','','CANADA' UNION
SELECT 'fc9b111f-39d4-4b00-8031-aa535c29e795','1778','APARTMENT AND COMMERCIAL SERVICES, INC','4418 OCEAN DR','CORPUS CHRISTI','TX','78412-2535','NUECES COUNTY','UNITED STATES' UNION
SELECT '685e2dcf-47ed-4311-ac9e-803c471aec06','1823','APARTMENT MANAGING PROFESSIONALS, LTD','5900 SOUTHWEST PKWY BLDG 2 STE 210','AUSTIN','TX','78735-6204','TRAVIS COUNTY','UNITED STATES' UNION
SELECT 'f0a5f57c-3173-4ce2-b1fc-0bf5d0d678f4','6616','APARTMENT RENTAL  ASSISTANCE II, INC','737 S GENESEE AVE','LOS ANGELES','CA','90036-4544','LOS ANGELES COUNTY','UNITED STATES' UNION
SELECT '04935f55-1899-4c92-b4f0-8a4c032eb640','2650','APERTO MANAGEMENT COMPANY INC','23461 S POINTE DR STE 180','LAGUNA HILLS','CA','92653-1523','ORANGE COUNTY','UNITED STATES' UNION
SELECT 'bf6440f4-3450-4f4e-99f5-5c5af1828807','1371990','APEX 2015 LLC','1 CENTER POINTE DR STE 445','LA PALMA','CA','90623-2501','','UNITED STATES' UNION
SELECT '6a00840c-3a3b-442a-a7e3-87ba155a5397','1379961','','','','','','','' UNION
SELECT 'cd898b40-4c5c-4732-889f-6c9c7e228398','4495','APPLETON CORPORATION','800 KELLY WAY STE 200','HOLYOKE','MA','1040','HAMPDEN COUNTY','UNITED STATES' UNION
SELECT '2b8a5abd-fd3b-4ca3-a0d7-43cdc4434a0f','4797','APT MANAGEMENT INC','500 W CUMMINGS PARK STE 6050','WOBURN','MA','01801-6547','MIDDLESEX COUNTY','UNITED STATES' UNION
SELECT '287adc77-2fa2-441f-a7cb-67906b48dd44','8105','ARAINO VENTURES INC.','15310 AMBERLY DR STE 250','TAMPA','FL','33647-1642','HILLSBOROUGH COUNTY','UNITED STATES' UNION
SELECT '0e13aa7a-2411-487f-b8f4-e3e70a9ec188','5079','ARB PROPERTIES LLC','365 PARAGON MILLS RD','NASHVILLE','TN','37211-3500','DAVIDSON COUNTY','UNITED STATES' UNION
SELECT '78280dcd-0cfb-43aa-a68f-665d9dafc1e3','15072','','','','','','','' UNION
SELECT 'baa56424-0c72-42c7-8ad5-59d6992d1144','14933','','','','','','',''  UNION
SELECT '0fd87630-a812-4fa8-9e47-2b909178d5f3','30516','ARCADIAN PROPERTY GROUP LLC','1200 PROSPECT ST STE 320','LA JOLLA','CA','92037-3660','SAN DIEGO COUNTY','UNITED STATES' UNION
SELECT '62b31da3-eb22-4c31-831e-5b4e233f2391','1372888','ARCH ASSET MANAGEMENT LLC','524 BROADWAY RM 405','NEW YORK','NY','10012-4408','NEW YORK COUNTY','UNITED STATES' UNION
SELECT '75cf787d-3a5c-40ca-a494-5f460f4395f4','19050','ARCH MANAGEMENT LLC','150 SAINT CLAIR LN','BECKLEY','WV','25801-7900','RALEIGH COUNTY','UNITED STATES' UNION
SELECT 'b18dd12e-152c-4997-96a5-7a034f34663f','6204','ARDENBROOK INC','4725 THORNTON AVE','FREMONT','CA','94536-6408','ALAMEDA COUNTY','UNITED STATES' UNION
SELECT '734a56f9-f708-436c-a4b3-d78fa80e95bf','8471','ARDENT RESIDENTIAL LLC','5555 GLENRIDGE CONNECTOR STE 200','SANDY SPRINGS','GA','30342-4740','FULTON COUNTY','UNITED STATES' UNION
SELECT '035ba8ac-d094-4425-ae8c-0cfad94152a1','14996','','','','','','','' UNION
SELECT '8e094cac-5687-4bd5-a3ff-4313df39a1b3','14848','ARIA APARTMENTS LLC','2791 W 52ND AVE STE 1-103','DENVER','CO','80221','','UNITED STATES' UNION
SELECT 'd9ec1eba-7148-4b76-9f81-c11eafd35e88','6775','ARIAM PARTNERS, LLC','8000 AVALON BLVD STE 100','ALPHARETTA','GA','30009-2469','FULTON COUNTY','UNITED STATES' UNION
SELECT 'cf8f9984-7efc-46cf-bba6-8a3300080160','4371','ARIZONA DISCIPLES HOMES, INC.','5325 W BUTLER DR','GLENDALE','AZ','85302-4854','MARICOPA COUNTY','UNITED STATES' UNION
SELECT 'aeb4cef5-1d50-4f7e-9e15-302905c4f72b','22842','ARIZONA STUDENT HOUSING MANAGEMENT, LLC','20701 N SCOTTSDALE RD STE 107','SCOTTSDALE','AZ','85255-6413','MARICOPA COUNTY','UNITED STATES' UNION
SELECT '066a7020-5ef4-42e4-bdd8-7a15f6076be9','399','','','','','','','' UNION
SELECT '13cd5eaa-1f52-494f-b7d0-eb28748c55fb','1232','ARNOLD-GROUNDS APARTMENT MANAGEMENT & AFFORDABLE HOUSING SPECIALISTS, LLC','920 S MAIN ST STE 200','GRAPEVINE','TX','76051-7517','TARRANT COUNTY','UNITED STATES' UNION
SELECT '575e868b-3c33-4ce3-b31c-32388bd770b5','9207','ARROWHEAD HOUSING, INC. A CALIFORNIA CORPORATION','1401 EL CAMINO AVE STE 300','SACRAMENTO','CA','95815-2747','SACRAMENTO COUNTY','UNITED STATES' UNION
SELECT '49893daa-9f83-42d7-92d2-06e73c348b80','7286','ARTHUR PARTNERS LLC DBA TREPLUS COMMUNITIES','1515 LAKE SHORE DR STE 225','COLUMBUS','OH','43204-3896','FRANKLIN COUNTY','UNITED STATES' UNION
SELECT 'ed78977b-59dd-4f64-a2ef-0fbb4fcff40a','408','ASCENSION COMMERCIAL REAL ESTATE, LLC','4615 SOUTHWEST FWY STE 700','HOUSTON','TX','77027-7106','HARRIS COUNTY','UNITED STATES' UNION
SELECT 'fc4ae5dd-7425-4af7-ba5f-975562fec83b','1282','ASPEN SQUARE MANAGEMENT','380 UNION ST STE 300','WEST SPRINGFIELD','MA','01089-4128','HAMPDEN COUNTY','UNITED STATES' UNION
SELECT '9c552f7d-b079-428a-ab76-564b507034e0','6729','ASSET MANAGEMENT AND CONSULTING SERVICES, INC','2409 MALL DR STE A','N CHARLESTON','SC','29406','','UNITED STATES' UNION
SELECT '1ff59db6-3fb4-4cbd-9536-4777010bf05b','268','ASSET PLUS CORPORATION','950 CORBINDALE RD STE 300','HOUSTON','TX','77024-2849','HARRIS COUNTY','UNITED STATES' UNION
SELECT '61bd37b4-273d-4700-9e64-c68f828044a3','1379962','','','','','','','' UNION
SELECT '7bf4199e-454c-4b79-b172-b60f2cbb95b7','5204','ASSOCIATED CATHOLIC CHARITIES INC','2300 DULANEY VALLEY RD APT B','TIMONIUM','MD','21093-2739','BALTIMORE COUNTY','UNITED STATES' UNION
SELECT 'b60ebb7b-deef-4047-8458-120e2466f7d5','637','ASSOCIATED MANAGEMENT CO','33067 SCHOOLCRAFT RD','LIVONIA','MI','48150-1618','WAYNE COUNTY','UNITED STATES' UNION
SELECT '22db6e78-1e11-4faf-848b-e07fab2b983d','6175','ASSOCIATES OF TRIANGLE INC','1712 N MERIDIAN ST STE 300','INDIANAPOLIS','IN','46202-6402','MARION COUNTY','UNITED STATES' UNION
SELECT '2af659d0-0b83-455b-b4ce-e4f61e2962d2','9092','','','','','','','' UNION
SELECT '22578a4b-97f9-4af7-a9cd-b4f476f6a91a','5418','ATLANTIC HOUSING MANAGEMENT','5910 N CENTRAL EXPY STE 1310','DALLAS','TX','75206-1103','DALLAS COUNTY','UNITED STATES' UNION
SELECT 'a3001ea8-b01c-43b7-b743-ffe23995f194','6597','ATLANTIC AND PACIFIC WEST COAST','11075 CARMEL MOUNTAIN RD STE 200','SAN DIEGO','CA','92129-1600','SAN DIEGO COUNTY','UNITED STATES' UNION
SELECT 'b38a8478-e96f-42b6-82dd-017f6a5fc270','6694','ATLANTIC REALTY MANAGEMENT, INC.','11426 YORK RD','COCKEYSVILLE','MD','21030-1834','BALTIMORE COUNTY','UNITED STATES' UNION
SELECT 'c191afe7-2e48-4c31-be14-0ba49b5816fc','18730','ATLANTIS REAL ESTATE, INC.','10 JACKSON ST STE 107','LOS GATOS','CA','95030-7168','SANTA CLARA COUNTY','UNITED STATES' UNION
SELECT 'dba85ce8-3b77-4066-b16e-445308986505','2060','ATLAS APARTMENT HOLDINGS, LLC','55 E MONROE ST STE 3610','CHICAGO','IL','60603-5713','COOK COUNTY','UNITED STATES' UNION
SELECT '591d5f39-bf17-48d7-9f0b-9893a27c41fa','6702','ATLAS REALTY','1360 EISENHOWER BLVD STE 704','JOHNSTOWN','PA','15904-3341','CAMBRIA COUNTY','UNITED STATES' UNION
SELECT 'f910e971-5445-49db-91cd-c4fca0aed6d6','1379091','AU MANAGEMENT, LLC.','159 OLD GEORGETOWN ST','LEXINGTON','KY','40508-1297','FAYETTE COUNTY','UNITED STATES' UNION
SELECT '64aa6881-114e-45cc-a8f1-d9e27e2ea63f','9097','','','','','','','' UNION
SELECT 'c6143a36-ab09-44f3-b66d-1c328435ce10','18678','AURA CORPORATION','1300 ASHFORD AVE. PENTHOUSE SUITE','SAN JUAN','PR','907','SAN JUAN MUNICIPIO','UNITED STATES' UNION
SELECT '8d7268b9-11ed-4c3b-b315-9c99b24bd4d1','513','AURORA PROPERTY RESOURCES LLC','2251 CHARLES DR','RALEIGH','NC','27612-4083','WAKE COUNTY','UNITED STATES' UNION
SELECT '4fbbe218-8760-48c4-b3e4-bfebec41a732','5094','AVALONBAY COMMUNITIES','4040 WILSON BLVD STE 1000','ARLINGTON','VA','22203-1924','ARLINGTON COUNTY','UNITED STATES' UNION
SELECT 'f1c72bda-9e02-40f8-a6bd-6e858d1e2c75','1371969','AVAN PROPERTY MANAGEMENT LLC','1200 BLALOCK RD STE 380','HOUSTON','TX','77055-6455','HARRIS COUNTY','UNITED STATES' UNION
SELECT '3a46a19a-06b6-4960-a1c6-9957d8bcfe79','15449','AVENIDA MANAGEMENT SERVICES, LLC','130 NEWPORT CENTER DR STE 220','NEWPORT BEACH','CA','92660-6924','ORANGE COUNTY','UNITED STATES' UNION
SELECT '3c116575-1934-49a4-9a53-2f07440cae38','8214','AVENUE5 RESIDENTIAL, LLC','901 5TH AVE STE 3000','SEATTLE','WA','98164-2066','KING COUNTY','UNITED STATES' UNION
SELECT 'a3333dec-296a-4c20-b225-a52a38ae4bb1','23447','','','','','','','' UNION
SELECT '58886dc0-2d85-40d7-8587-791776b29826','14719','','','','','','','' UNION
SELECT '82c709cd-a5fe-40e1-aa8c-61d1df09520d','1329','','','','','','','' UNION
SELECT '91245807-751e-4cbf-9199-2ec98cf855c2','643','B & R PROPERTY MANAGEMENT, INC.','8966 SPANISH RIDGE AVE STE 100','LAS VEGAS','NV','89148-1302','','UNITED STATES' UNION
SELECT 'bdb1f47e-ee0c-4ac4-a25b-b1daa1d373b9','5832','B AND M MANAGEMENT','7020 FAIN PARK DR STE 5','MONTGOMERY','AL','36117-7813','MONTGOMERY COUNTY','UNITED STATES' UNION
SELECT '48f594af-ef27-4ce0-8530-222324a7b691','1379720','','','','','','','' UNION
SELECT 'c7742424-a4de-48c0-8539-53b287ce65a4','7144','B/T WASHINGTON LLC DBA BLANTON TURNER','614 1ST AVE STE 200','SEATTLE','WA','98104-2233','KING COUNTY','UNITED STATES' UNION
SELECT 'b06dfaf2-4d16-40c2-ac56-e2fc63bc1fbb','1307','BACH CORPORATION','11650 S STATE ST STE 300','DRAPER','UT','84020-9455','SALT LAKE COUNTY','UNITED STATES' UNION
SELECT 'be0f3b10-ce53-41a4-9ef9-76108fad9f97','1195','BAINBRIDGE MANAGEMENT GROUP LLC','12765 FOREST HILL BLVD STE 1307','WELLINGTON','FL','33414-4781','PALM BEACH COUNTY','UNITED STATES' UNION
SELECT 'fc5aec39-1d07-46d4-8d84-0af5932ed6af','250','BAKKE DEVELOPMENT','3511 BROADWAY ST','SAN ANTONIO','TX','78209-6513','BEXAR COUNTY','UNITED STATES' UNION
SELECT '65930d63-437e-47fe-bd4e-6178b4d2c00a','1374873','BALDWIN ACRES, INC.','4 BALDWIN AVE','NORWOOD','NY','13668-1202','ST. LAWRENCE COUNTY','UNITED STATES' UNION
SELECT '8f78ba47-a032-4c89-a03d-bcc8ab53d55c','24399','BALDWIN ASSET MANAGEMENT INC','610 W ASH ST STE 1500','SAN DIEGO','CA','92101-3367','SAN DIEGO COUNTY','UNITED STATES' UNION
SELECT '1afef587-0f5d-4677-b801-dbc9c87bb5af','216','','','','','','','' UNION
SELECT '279d0797-b153-4840-95f9-cf6d9a751701','6538','BANYAN EQUITY MANAGEMENT, LLC','2800 NASA PKWY # 1','SEABROOK','TX','77586-3247','HARRIS COUNTY','UNITED STATES' UNION
SELECT 'ef80c30d-6e27-4951-a199-5afb5a083257','398','','','','','','','' UNION
SELECT 'b44341a6-d935-4f53-8703-9dd15335fccf','2059','BARKER MANAGEMENT INC','1101 E ORANGEWOOD AVE STE 200','ANAHEIM','CA','92805-6809','ORANGE COUNTY','UNITED STATES' UNION
SELECT 'b047770f-664d-4675-9b14-90c8c6ba1087','1380017','','','','','','','' UNION
SELECT 'd88d81b2-2ceb-4135-987a-027850803f08','2814','BARON PROPERTY SERVICES LLC','1401 17TH ST STE 700','DENVER','CO','80202-1241','DENVER COUNTY','UNITED STATES' UNION
SELECT '4a1c9aeb-cc5b-4b9c-9bff-8f73832f15a5','5294','BARRETT AND STOKELY MANAGEMENT, LLC','3755 E 82ND ST STE 300','INDIANAPOLIS','IN','46240-4343','MARION COUNTY','UNITED STATES' UNION
SELECT 'cccff560-0a6f-4b21-9153-a48f31bd0791','5320','BARRINGTON RESIDENTIAL','3375 BRIGHTON HENRIETTA TOWN LINE RD','ROCHESTER','NY','14623-2842','MONROE COUNTY','UNITED STATES' UNION
SELECT 'efa04ac8-33ba-492f-bbb7-e851687d96d4','9006','','','','','','','' UNION
SELECT 'bf249180-3ad2-47a9-8deb-cd2a95de6003','1380716','BAY CITY HOUSING COMMISSION','315 14TH ST','BAY CITY','MI','48708-7148','BAY COUNTY','UNITED STATES' UNION
SELECT '669cbfea-7eaa-42b5-a254-d8d2b9e940cb','29677','BAYVIEW MANAGEMENT LLC','17 W PENNSYLVANIA AVE','TOWSON','MD','21204-5016','BALTIMORE COUNTY','UNITED STATES' UNION
SELECT 'c8f85c14-8a36-465e-940e-b6255d077c8a','9303','BEACH CLUB CONDOMINIUMS','14721 WHITECAP BLVD','CORPUS CHRISTI','TX','78418-7712','NUECES COUNTY','UNITED STATES' UNION
SELECT '6b821702-d716-42f9-b096-022b72454db1','654','BEACON 360 MANAGEMENT','3409 E BROAD ST','COLUMBUS','OH','43213-1064','FRANKLIN COUNTY','UNITED STATES' UNION
SELECT '54731c9a-d8b5-4b0c-bc21-f98e5432f132','5261','BEACON RESIDENTIAL MANAGEMENT LIMITED PARTNERSHIP.','2 CENTER PLZ STE 700','BOSTON','MA','02108-1906','SUFFOLK COUNTY','UNITED STATES' UNION
SELECT '93b71661-809e-4e08-910a-a587cb21af41','1377942','BEAR VENTURES GROUP, LLC','4900 PLAZA DR','MONTGOMERY','AL','36116-2629','MONTGOMERY COUNTY','UNITED STATES' UNION
SELECT '1e87c75e-34c9-4101-9b01-8191d70b9d89','1379963','','','','','','','' UNION
SELECT 'dc5db0da-f1c8-44c0-8fa3-ed70fb8a65c1','4569','','','','','','','' UNION
SELECT 'a00c50f8-315f-4510-bed4-b27e8adb2bfe','3344','BEECHWOOD PORTOFINO,LLC','200 ROBBINS LN UNIT D1','JERICHO','NY','11753-2341','NASSAU COUNTY','UNITED STATES' UNION
SELECT '44dd7f0e-8cc1-440f-a094-cef67f75c7e1','1379491','BEECHWOOD PROPERTY HOLDINGS MANAGEMENT, LLC','2118 DELANCEY ST','PHILADELPHIA','PA','19103-6512','PHILADELPHIA COUNTY','UNITED STATES' UNION
SELECT '95a7edeb-b850-4629-b5da-8ce4d95e5469','5740','BELCHER MANAGEMENT','9771 JEFFERSON HWY','BATON ROUGE','LA','70809-7207','EAST BATON ROUGE PARISH','UNITED STATES' UNION
SELECT '80e05390-b3ab-4b85-b8c9-7a28102cf1d5','661','BELCO EQUITIES','505 E HUNTLAND DR STE 530','AUSTIN','TX','78752-3760','TRAVIS COUNTY','United States' UNION
SELECT '294d46b5-73f1-4afc-9d33-8cd51cbebdb0','1378206','BELL ASSET MANAGEMENT LLC','1420 WASHINGTON BLVD # 480','DETROIT','MI','48226-1718','WAYNE COUNTY','UNITED STATES' UNION
SELECT '131ac8a8-73ad-4e70-bc19-5a04f9f27c40','734','BELL PARTNERS INC.','300 N GREENE ST STE 1000','GREENSBORO','NC','27401-2173','GUILFORD COUNTY','UNITED STATES' UNION
SELECT '581a241d-25cf-40ed-9bf0-1740868a632f','8084','BELLEVUE REALTY MANAGEMENT, LLC','3710 RAWLINS ST STE 1375','DALLAS','TX','75219-4217','DALLAS COUNTY','UNITED STATES' UNION
SELECT '9eeef22b-7fb4-4ff2-9128-ded1cc2af282','666','BELMONT MANAGEMENT COMPANY, INC','215 BROADWAY ST','BUFFALO','NY','14204-1471','ERIE COUNTY','UNITED STATES' UNION
SELECT 'f05aaf21-3c63-4817-b22d-e7f43f91182c','6640','BENCHMARK PROPERTY MGMT','4053 MAPLE RD STE 200','BUFFALO','NY','14226-1058','ERIE COUNTY','UNITED STATES' UNION
SELECT 'acac0a95-56e6-45a2-a22b-0ec59e99822a','17455','BENOIT MANAGEMENT, LLC','2180 IMMOKALEE RD STE 313','NAPLES','FL','34110-1407','COLLIER COUNTY','UNITED STATES' UNION
SELECT 'adbb62ab-8a75-4404-b9d3-6660f18295f5','5443','BENSON INVESTMENTS','1502 W 6TH ST # B','AUSTIN','TX','78703-5134','TRAVIS COUNTY','United States' UNION
SELECT 'b353dfba-81be-4597-b900-4c00e7c455f0','2941','BERGER PROPERTIES','2140 S 10TH ST','PHILADELPHIA','PA','19148-3034','PHILADELPHIA COUNTY','UNITED STATES' UNION
SELECT 'a4b496d1-36a4-4ea0-b05c-4f3ef8d45b14','1327','BERKADIA COMMERCIAL MORTGAGE L','1250 E COPELAND RD STE 250','ARLINGTON','TX','76011-1345','TARRANT COUNTY','UNITED STATES' UNION
SELECT 'd2c44b2b-2e9d-430a-b5f0-4e8f10d9436b','8086','BERKSHIRE COMMUNITIES, L.L.C.','1 BEACON ST STE 2400','BOSTON','MA','02108-3107','SUFFOLK COUNTY','UNITED STATES' UNION
SELECT 'd7685a3c-7dec-44e3-8f37-cdfd1df2d315','1794','','','','','','','' UNION
SELECT '500f45a9-f191-4328-a054-5519b605a662','30770','BEST BAY APARTMENTS INC., DBA RIAZ CAPITAL','2744 E 11TH ST','OAKLAND','CA','94601-1429','ALAMEDA COUNTY','UNITED STATES' UNION
SELECT 'bf50329f-d8a3-4cd0-91e3-c30f120de7e3','6108','BET INVESTMENTS','200 DRYDEN RD E STE 2000','DRESHER','PA','19025-1048','MONTGOMERY COUNTY','UNITED STATES' UNION
SELECT '0d342aca-a10a-4855-842a-9eaa63abf2e3','25694','BEYMER BROOK II DBA CRIMSON COURT TOWNHOUSES','230 S CARPENTER AVE','INDIANA','PA','15701-4057','INDIANA COUNTY','UNITED STATES' UNION
SELECT '48f88c01-5cbc-46a0-88f5-065ad9a64a64','251','','','','','','','' UNION
SELECT 'a2a7fda7-f496-46c0-9e89-1c7907d3fd76','1379149','BFIT INVESTORS LLC','2040 AUDREY LN','SHREVEPORT','LA','71107-4801','CADDO PARISH','UNITED STATES' UNION
SELECT 'caa77e21-982e-42c0-a906-c97765e89403','917','BH MANAGEMENT SERVICES, LLC','400 LOCUST ST STE 790','DES MOINES','IA','50309-2347','POLK COUNTY','UNITED STATES' UNION
SELECT 'dc8eef64-20a6-40da-a930-10783b745ac1','4888','BHC PROPERTY MANAGEMENT, LLC','13621 PERDIDO KEY DR UNIT W2002','PENSACOLA','FL','32507-5204','ESCAMBIA COUNTY','UNITED STATES' UNION
SELECT '5134ee4c-1b74-4475-94e2-76a12c2a1032','41','','','','','','','' UNION
SELECT '2e54af98-5043-4bf2-94ff-c1ea3a1abbd7','1379900','BINGHAMTON NORTHSIDE LP','35 EXCHANGE ST','BINGHAMTON','NY','13901','BROOME COUNTY','UNITED STATES' UNION
SELECT '09e33a6d-9914-4451-b334-c0b8b12e4e21','1379964','','','','','','','' UNION
SELECT 'f0ab8c34-bf46-47f8-8674-ad10ef198157','6109','BIRGE AND HELD ASSET MANAGEMENT, LLC','8902 N MERIDIAN ST STE 205','INDIANAPOLIS','IN','46260-5307','MARION COUNTY','UNITED STATES' UNION
SELECT '81cd2de1-ed2a-410d-a81d-124e6dfb58fd','6726','BI-STATE MANAGEMENT, LLC','626 DARTMOUTH DR','CLARKSVILLE','IN','47129-6626','CLARK COUNTY','UNITED STATES' UNION
SELECT '1e1aaf6d-b0b6-44fa-83a8-70e2ca452f1a','5889','BLACKHAWK REALTY MANAGEMENT, LLC','190 S LA SALLE ST STE 510','CHICAGO','IL','60603-6601','COOK COUNTY','UNITED STATES' UNION
SELECT '629f0328-b3f6-41fa-bf81-b6f4370d458a','5210','BLACKPOINT MANAGEMENT, INC.','235 POSADA DEL SOL','NOVATO','CA','94949-6388','MARIN COUNTY','UNITED STATES' UNION
SELECT 'fa42df52-b110-4714-b99c-6f1d781e578b','19509','','','','','','','' UNION
SELECT '3c2e03a2-01c0-4b5d-b3b5-454d4df62e48','6601','BLAKE CAPITAL CORP','731 N JACKSON ST STE 400','MILWAUKEE','WI','53202-4697','MILWAUKEE COUNTY','UNITED STATES' UNION
SELECT '21b439da-b61f-49c2-a8d5-ec22ddf51074','1379089','','','','','','','' UNION
SELECT '87ec2ec4-6c12-4813-b32f-b100bf73f15b','6435','BLAZER REAL ESTATE SERVICES LLC','4001 W SAM HOUSTON PKWY N STE 100','HOUSTON','TX','77043-1236','HARRIS COUNTY','UNITED STATES' UNION
SELECT '2fb47e7d-cb70-4429-bb5c-421aa5a607c3','5581','BLEAKLEY DEVELOPMENT COMPANY','1074 W SANTA FE ST','OLATHE','KS','66061-3173','JOHNSON COUNTY','UNITED STATES' UNION
SELECT '90cb29e4-ee9c-41af-ab6c-b02b93b3932a','1380303','','','','','','','' UNION
SELECT '6908fcb7-23f5-4ea6-82f2-03c173784512','6721','BLOCK MULTIFAMILY GROUP','4501 COLLEGE BLVD STE 260','LEAWOOD','KS','66211-2328','JOHNSON COUNTY','UNITED STATES' UNION
SELECT 'bb6e73cf-6cb4-46de-a5ce-dde351a04808','1371190','','','','','','','' UNION
SELECT '7204a30b-6eee-4175-8cb2-692272ce183f','6742','BLUE RIDGE PROPERTY MANAGEMENT, LLC','5826 SAMET DR STE 105','HIGH POINT','NC','27265-3661','GUILFORD COUNTY','UNITED STATES' UNION
SELECT 'd60824e9-89f1-477e-8f94-50cb59430c4d','1375092','BLUE RIVER SERVICES, INC.','156 AUTUMN RIDGE DR','CORYDON','IN','47112','','UNITED STATES' UNION
SELECT '7e6bc8c3-6be9-40ab-85ef-53e9b8e8a6ef','88','BLUE SKY LAND CO, LLC','400 SOUTH AVE STE 8','MIDDLESEX','NJ','08846-2567','MIDDLESEX COUNTY','UNITED STATES' UNION
SELECT '9421ed36-41c7-4248-8185-212aa3bda8c1','6738','BLUELINE PROPERTIES','3322 W 13TH ST','GREELEY','CO','80634-6310','WELD COUNTY','UNITED STATES' UNION
SELECT 'a72f02b0-1184-4775-9994-430e2b01f078','9051','BLUESTONE & HOCKLEY','9320 SW BARBUR BLVD STE 300','PORTLAND','OR','97219-5405','MULTNOMAH COUNTY','UNITED STATES' UNION
SELECT '2eb10e35-7a01-484f-b210-b5b82b43403d','3419','BLVD RESIDENTIAL INC.','4080 CAMPBELL AVE','MENLO PARK','CA','94025-1007','SAN MATEO COUNTY','UNITED STATES' UNION
SELECT '9842a7ce-f61f-4df9-a610-706195aa4b10','6567','BMI MANAGEMENT, INC','161 W SHADOWBEND AVE STE 100','FRIENDSWOOD','TX','77546-3940','GALVESTON COUNTY','UNITED STATES' UNION
SELECT '67b1fa5d-984b-4583-9e4e-6b3a97d316dc','1292','BNC EQUITIES LLC','13151 EMILY RD STE 250','DALLAS','TX','75240-8941','DALLAS COUNTY','UNITED STATES' UNION
SELECT '0303aaf7-4b5a-49e9-984e-c43c001e3e94','6888','BOHANNON DEVELOPMENT CORP','5915 SILVER SPRINGS DR BLDG 2','EL PASO','TX','79912-4126','EL PASO COUNTY','UNITED STATES' UNION
SELECT '7d2e5291-6cce-4215-876d-38f7f68ec4f4','15799','BONANNI DEVELOPMENT','5500 BOLSA AVE STE 120','HUNTINGTON BEACH','CA','92649-1188','ORANGE COUNTY','UNITED STATES' UNION
SELECT 'da278d02-4ce4-462b-a9f9-948a833b99ce','17991','','','','','','','' UNION
SELECT '202c1cf2-91a2-47a1-9765-4e0bd804911d','873','BOWEN PROPERTY MANAGEMENT CO.','1455 SW BROADWAY STE 1700','PORTLAND','OR','97201-3412','MULTNOMAH COUNTY','UNITED STATES' UNION
SELECT 'b36e56e0-8d84-4e2a-855a-c40d9afb2ebb','7171','BOWMAN STATION, LLC','5235 BOWMAN RD','MACON','GA','31210','BIBB COUNTY','UNITED STATES' UNION
SELECT '4e2b2df6-4a5c-43b2-b00d-71fadfe2f45f','14178','BOYD A JARRELL & CO. INC.','441 DIXIE HWY APT A10','CHICAGO HEIGHTS','IL','60411-1779','COOK COUNTY','UNITED STATES' UNION
SELECT 'e5f91dd9-a1f6-4170-a3af-e66726c90189','1201','','','','','','','' UNION
SELECT '75e02f26-2c7e-4465-9e99-b8be635b3966','1251','BOYD WILSON, LLC','600 OLDE HICKORY RD','LANCASTER','PA','17601-4959','LANCASTER COUNTY','UNITED STATES' UNION
SELECT '75738036-1d7f-4502-b1b7-7633b10dbb14','4424','BOYER HILL MILITARY HOUSING','4114 CHARLESTOWN LOOP # B','HILL AFB','UT','84056-1104','DAVIS COUNTY','UNITED STATES' UNION
SELECT '7ec77195-5614-464e-8b9d-2c06f699b0be','6800','BOZZUTO MANAGEMENT CO','6406 IVY LN','GREENBELT','MD','20770-1441','PRINCE GEORGE''S COUNTY','UNITED STATES' UNION
SELECT '68d7c6e6-a094-41b3-b6c3-6634a3af47e2','1368062','','','','','','','' UNION
SELECT '7c486c16-b99b-4aee-9a56-e0f861608d27','2961','BRACKENRIDGE PARK/BROADWAY DEVELOPMENTS LTD DBA BROADWAY DEVELOPMENTS','2632 BROADWAY ST STE 101S','SAN ANTONIO','TX','78215-1138','BEXAR COUNTY','UNITED STATES' UNION
SELECT 'ce5b3745-543c-4c4d-804b-b186c5dfbe3d','5097','BREAKING GROUND MANAGEMENT','505 8TH AVE 5TH FL','NEW YORK','NY','10018-6505','NEW YORK COUNTY','UNITED STATES' UNION
SELECT 'a8777fad-49db-4bf5-bc7a-768ff7ff7eb9','1371916','BRICK AND VINE LLC','370 HIGHLAND AVE STE 200','PIEDMONT','CA','94611-4018','ALAMEDA COUNTY','UNITED STATES' UNION
SELECT '820cbbaa-b711-48e8-9389-e50e4e488e87','24774','BRICK LANE PM LLC','3506 CONNECTICUT AVE NW FL 2','WASHINGTON','DC','20008-2401','DISTRICT OF COLUMBIA','UNITED STATES' UNION
SELECT '0dd38b1d-b670-4550-b9cf-33e1cf6dcebf','1374876','BRICKWORK PROPERTIES, L.P.','2040 SANDY DR STE C','STATE COLLEGE','PA','16803-2524','CENTRE COUNTY','UNITED STATES' UNION
SELECT '20f40bee-259a-4ff6-91df-f9b41b648b16','7267','BROADMOOR GROUP INC.','5634R COLUMBIA AVE','SAINT LOUIS','MO','63139-1624','ST. LOUIS CITY','UNITED STATES' UNION
SELECT '66028b43-281b-44b0-a63f-88cc1cc03b8d','6451','BROADWAY MANAGEMENT COMPANY','8 CROW CANYON CT STE 800','SAN RAMON','CA','94583-1971','','United States' UNION
SELECT '596ac846-3f12-43c8-8963-09a8e589da7d','5449','BROADWAY REAL ESTATE SERVICES','100 S KENTUCKY AVE STE 290','LAKELAND','FL','33801-5088','POLK COUNTY','UNITED STATES' UNION
SELECT 'a5082cb3-1b2c-46e5-8094-b31d7e85d265','1835','','','','','','','' UNION
SELECT '222661eb-73be-43c9-9772-cd27f54bfaac','788','BROOKFIELD PROPERTIES MULTIFAMILY LLC','127 PUBLIC SQ STE 2500','CLEVELAND','OH','44114-1216','CUYAHOGA COUNTY','UNITED STATES' UNION
SELECT '00efc58c-4eb1-41de-9420-533452c220d6','171','BROOKSIDE PROPERTIES','2002 RICHARD JONES RD STE C200','NASHVILLE','TN','37215-2963','DAVIDSON COUNTY','UNITED STATES' UNION
SELECT '8adedacd-b330-41d4-bee9-923cf72a3d79','3495','BROOKSTONE REALTY MANAGEMENT','2802 JEFFERSON AVE','MIDLAND','MI','48640-4506','MIDLAND COUNTY','UNITED STATES' UNION
SELECT '7ce27ad6-7763-4419-bc0a-7dae20d50cb6','5833','BROTHERS MANAGEMENT','400 LASALLE AVE','WACO','TX','76710','MCLENNAN COUNTY','UNITED STATES' UNION
SELECT 'f2a378ce-29a3-4fd3-9a5a-c7a54b9c7953','5437','BROTHERS PROPERTY MANAGEMENT, INC.','2250 EATON ST STE B','DENVER','CO','80214-1276','JEFFERSON COUNTY','UNITED STATES' UNION
SELECT '504a0190-3423-4a2c-8954-4f098cdd4024','679','','','','','','','' UNION
SELECT '567c3737-87f5-4b28-a52f-08c8f28991bc','8558','BROWNSTONE SHILOH CROSSING','6517 MAPLERIDGE ST','HOUSTON','TX','77081-4610','HARRIS COUNTY','UNITED STATES' UNION
SELECT '031c220e-7e62-4eee-bea7-0131c27bc6b2','3727','BRUTGER EQUITIES INC','100 4TH AVE S','SAINT CLOUD','MN','56301-3615','STEARNS COUNTY','United States' UNION
SELECT 'b02c7785-c63b-4e15-bb93-78217bac90f8','2278','BUCK PHILADELPHIA DEVELOPMENT LLC C/O THE JOHN BUCK COMPANY','1 N WACKER DR STE 2400','CHICAGO','IL','60606-2866','COOK COUNTY','UNITED STATES' UNION
SELECT '42b93410-93aa-4e8d-b105-3c94a7df84ec','1321','BUCKINGHAM MANAGEMENT LLC','941 N MERIDIAN ST','INDIANAPOLIS','IN','46204-1012','MARION COUNTY','UNITED STATES' UNION
SELECT '0359f755-3caf-4f02-8881-ccb4bc7705a0','405','','','','','','','' UNION
SELECT '40d2b6f9-5fd0-4828-b1ee-016b86ac033c','3480','BURKENTINE & SONS','1454 BALTIMORE ST STE A','HANOVER','PA','17331-9816','YORK COUNTY','UNITED STATES' UNION
SELECT '05cd8474-debc-4bd1-8e1e-e019f3f75514','1296','BURLINGTON CAPITAL PROPERTIES, LLC','1004 FARNAM ST STE 400','OMAHA','NE','68102-1885','DOUGLAS COUNTY','UNITED STATES' UNION
SELECT '7716acc8-0bc5-4192-aa22-1fd053955275','19390','BURTON & ASSOCIATES LLC','18877 W 10 MILE RD STE 250H','SOUTHFIELD','MI','48075-2647','OAKLAND COUNTY','UNITED STATES' UNION
SELECT '5f934d61-6885-46bd-9cd9-5af2a49cd101','6384','BURTON CAROL MANAGEMENT, LLC','4832 RICHMOND RD STE 200','CLEVELAND','OH','44128-5993','CUYAHOGA COUNTY','UNITED STATES' UNION
SELECT 'a33d8c66-c410-472a-9b6e-7c2b7113cfdd','812','C & C REALTY MANAGEMENT, LLC','526 WESTERN AVE','AUGUSTA','ME','04330-7739','KENNEBEC COUNTY','UNITED STATES' UNION
SELECT '46326293-9dd3-41c8-9e14-088a299c827f','733','','','','','','','' UNION
SELECT 'd1a8bd6b-3054-46ff-95ee-49235ac5bb28','6441','C STAR MANAGEMENT, LLC','3124 NW 23RD ST','OKLAHOMA CITY','OK','73107-1902','OKLAHOMA COUNTY','UNITED STATES' UNION
SELECT '995ab12a-ab3b-4524-ae82-94a7f6872558','3219','','','','','','','' UNION
SELECT 'a338a9f7-18cd-4fc6-8eed-b8c9bc58af98','5965','','','','','','','' UNION
SELECT '0699375e-b77c-4981-8757-b99121dba302','2020','CAF MANAGEMENT LLC','2600 NETWORK BLVD STE 590','FRISCO','TX','75034-6037','COLLIN COUNTY','UNITED STATES' UNION
SELECT '2d293c4a-4174-4737-b0f3-8ce25feb2574','2245','CALDWELL HOUSING AUTHORITY','22730 FARMWAY RD','CALDWELL','ID','83607-8888','CANYON COUNTY','UNITED STATES' UNION
SELECT '88f18d48-6f18-4448-ab67-9aedff69492f','5558','CALHOUN-BARRE DEVELOPMENT CORP DBA SPECTRUM MANAGEMENT CO','2662 E JOYCE BLVD STE 4','FAYETTEVILLE','AR','72703-4867','WASHINGTON COUNTY','UNITED STATES' UNION
SELECT 'd5ae3897-2ec0-4992-8398-c3e9c360b7f7','684','CALIBRATE','677 120TH AVE NE STE 2A106','BELLEVUE','WA','98005-3045','KING COUNTY','UNITED STATES' UNION
SELECT '26764d00-9759-460a-921f-d1b50b7e7af9','5461','CALIFORNIA HUMAN DEVELOPMENT CORP','3315 AIRWAY DR','SANTA ROSA','CA','95403-2005','SONOMA COUNTY','UNITED STATES' UNION
SELECT 'f2a452bc-9832-4270-8316-d7130e35932b','14740','CAMBRIDGE MANAGEMENT INC','1916 64TH AVE W','TACOMA','WA','98466-6203','PIERCE COUNTY','UNITED STATES' UNION
SELECT 'c8efb793-200d-4cd0-81c3-183269f20f72','259','CAMBRIDGE MANAGEMENT SERVICES INC','650 NORTHLAKE BLVD STE 450','ALTAMONTE SPRINGS','FL','32701-6175','SEMINOLE COUNTY','UNITED STATES' UNION
SELECT '96162a5b-3b61-4eeb-9b6a-200f4a03cf09','6706','CAMDEN DEVELOPMENT, INC.','11 GREENWAY PLZ STE 2400','HOUSTON','TX','77046-1124','HARRIS COUNTY','UNITED STATES' UNION
SELECT 'ec91a3d1-71d7-4e7b-b404-628596355336','5880','CAMILLO PROPERTIES','6707 GESSNER RD','HOUSTON','TX','77040-4017','HARRIS COUNTY','UNITED STATES' UNION
SELECT 'acfdbca5-6477-4b24-940f-a92638058276','4489','','','','','','','' UNION
SELECT '3db2474d-490f-4601-964d-871527d6681a','1379965','','','','','','','' UNION
SELECT '55fd9e6e-ecc0-4733-8a93-14fc481186e5','9218','','','','','','','' UNION
SELECT '62524d6b-d986-4b45-9c70-c0f9f2b25ed4','189','CAMPUS ADVANTAGE, INC','110 WILD BASIN RD STE 365','WEST LAKE HILLS','TX','78746-3352','TRAVIS COUNTY','UNITED STATES' UNION
SELECT '7a84ae08-5130-4bca-ba6f-182c98a18d3a','20584','CAMPUS LOFTS LLC','1305 BOARDMAN CANFIELD RD','YOUNGSTOWN','OH','44512-4034','MAHONING COUNTY','UNITED STATES' UNION
SELECT 'c73f559c-441f-4cea-8407-19855be8e3dd','1223','CANNON MANAGEMENT','6349 RIVERSIDE AVE 2ND FL','RIVERSIDE','CA','92506-3163','RIVERSIDE COUNTY','UNITED STATES' UNION
SELECT '8be091b3-8e95-4734-9c22-da1f5ff9eac2','6739','CANNON PROPERTIES','763 MADISON RD STE 205','CULPEPER','VA','22701-3342','CULPEPER COUNTY','UNITED STATES' UNION
SELECT '09b7d7b9-f132-4220-9b64-ae68be86a654','614','CANYON VIEW CAPITAL INC.','331 SOQUEL AVE STE 100','SANTA CRUZ','CA','95062-2330','SANTA CRUZ COUNTY','UNITED STATES' UNION
SELECT 'f059e72d-031b-4f84-998c-866ac03c8e54','14900','','','','','','','' UNION
SELECT '6a6670cc-0a20-45f3-ace7-c5155728602d','3227','CAPITAL ASSOCIATES MANAGEMENT, LLC','5400 TRINITY RD STE 105','RALEIGH','NC','27607-6001','WAKE COUNTY','UNITED STATES' UNION
SELECT 'aa7c8d10-7e3f-462e-ba78-35a264fa3a32','1028','CAPITAL REALTY GROUP','86 ROUTE 59','SPRING VALLEY','NY','10977-5214','ROCKLAND COUNTY','UNITED STATES' UNION
SELECT '5608f5a4-bec4-4993-a452-121501b64642','5761','CAPREIT RESIDENTIAL MANAGEMENT LLC','11200 ROCKVILLE PIKE STE 100','ROCKVILLE','MD','20852-3152','MONTGOMERY COUNTY','UNITED STATES' UNION
SELECT '505c214a-df94-4179-b244-bec7e515fe52','5960','','','','','','','' UNION
SELECT '94407e59-d993-4d55-a56f-3447119922b0','6608','CAPSTONE REAL ESTATE SERVICES INC','210 BARTON SPRINGS RD STE 300','AUSTIN','TX','78704-1251','TRAVIS COUNTY','UNITED STATES' UNION
SELECT '20c378bb-1663-4ece-bfff-2bf466946ee7','6377','CARBON THOMPSON MULTIFAMILY MANAGEMENT, LLC','1600 N COLLINS BLVD STE 1500','RICHARDSON','TX','75080-3692','DALLAS COUNTY','UNITED STATES' UNION
SELECT 'e69fd2de-ba08-4270-b740-f654cc53c435','6040','CARDINAL CAPITAL MGMT INC','901 S 70TH ST','WEST ALLIS','WI','53214','MILWAUKEE COUNTY','UNITED STATES' UNION
SELECT 'b75c3ebb-49d0-4e79-b34b-bc4e4b86d8d2','1379050','CAREFREE DEVELOPMENT LLC','25 ELGIN AVE STE A','FOREST PARK','IL','60130-1118','COOK COUNTY','UNITED STATES' UNION
SELECT 'b78ce753-c14a-4c6a-a087-725436c2bd8a','23182','','','','','','','' UNION
SELECT 'e0a7a279-c2c3-4136-98bc-fdeac33547b3','15163','','','','','','','' UNION
SELECT 'cfd7c09c-ef81-404e-9a71-678a78825b4d','87','CARLETON MANAGEMENT SERVICES, LLC','3301 AIRPORT FWY STE 210','BEDFORD','TX','76021-6034','TARRANT COUNTY','UNITED STATES' UNION
SELECT 'b88c2e44-9fce-4834-a05f-083b1a583ced','5962','','','','','','','' UNION
SELECT '51359110-7d87-4715-96f8-9280b1d303ba','1378874','CARLSBAD MANAGEMENT GROUP LLC','100 48TH AVE NW','NORMAN','OK','73072-4442','CLEVELAND COUNTY','UNITED STATES' UNION
SELECT '1c6a97fb-9f3b-4ff0-addb-6e15cb206520','1380121','CARLYLE MANAGEMENT, INC. DBA WOODBRANCH MANAGEMENT, INC.','4265 SAN FELIPE ST STE 550','HOUSTON','TX','77027-3019','HARRIS COUNTY','UNITED STATES' UNION
SELECT 'af2a9df3-049f-41c3-8656-aa2cfe406e58','406','CAROLINA COMMERCIAL CONTRACTORS LLC','1600 COLON RD','SANFORD','NC','27330-9577','LEE COUNTY','UNITED STATES' UNION
SELECT '3466711c-f251-49a6-8153-78e768bda82a','7085','CAROLINA COMMUNITY MANAGEMENT LLC','307 AVALON RD STE K','GREENSBORO','NC','27401-4394','GUILFORD COUNTY','UNITED STATES' UNION
SELECT 'a0a27511-fb3a-4173-be1f-452a3339cf55','455','CARROLL MANAGEMENT GROUP','3340 PEACHTREE RD NE STE 2250','ATLANTA','GA','30326-1037','FULTON COUNTY','UNITED STATES' UNION
SELECT 'fa6da310-59b2-40b9-ac36-0a40bb9b2448','8196','CARTER HASTON REAL ESTATE SVCS INC','3301 W END AVE STE 200','NASHVILLE','TN','37203-6897','DAVIDSON COUNTY','UNITED STATES' UNION
SELECT '807a840b-5e19-4538-8099-33fb8754b0d5','5296','CARTERET MANAGEMENT','5300 W CYPRESS ST STE 200','TAMPA','FL','33607-1757','HILLSBOROUGH COUNTY','UNITED STATES' UNION
SELECT 'd723d2cf-56c1-4da0-938b-b535a36b08d0','6407','CARUSO MANAGEMENT COMPANY, LTD','101 THE GROVE DR','LOS ANGELES','CA','90036-6221','LOS ANGELES COUNTY','UNITED STATES' UNION
SELECT '9006a61a-81ca-4281-a402-e63474a883f9','1380025','','','','','','','' UNION
SELECT 'fe6be352-f44c-41f0-af39-38f41e405200','22901','CASPIAN ENTERPRISES INC.','101 CRAWFORD ST STE 100','HOUSTON','TX','77002-2198','HARRIS COUNTY','UNITED STATES' UNION
SELECT '5f6e59ed-acf5-4500-90d9-5e88d658b5f0','16529','CASTLE RESIDENTIAL MANAGEMENT, INC.','12270 SW 3RD ST STE 200','PLANTATION','FL','33325-2811','BROWARD COUNTY','UNITED STATES' UNION
SELECT '6a82b80d-5d60-4445-a9d9-6c31612c4c3d','418','CASTLE ROCK REALTY MANAGEMENT, LLC','40 S DUNDALK AVE STE 501','BALTIMORE','MD','21222-4272','BALTIMORE COUNTY','UNITED STATES' UNION
SELECT 'fc8253a6-b1e1-4da0-8d72-b36b4bcfe3e0','29965','CASTLE SENIOR LIVING, INC.','13050 W CLEVELAND AVE','NEW BERLIN','WI','53151-4013','WAUKESHA COUNTY','UNITED STATES' UNION
SELECT 'd7e61fba-c7c6-47c2-a008-bd371e36aee8','15063','CASTLEGATE APT OWNERS ASSOCIATION INC','4174 SILVER PEAK PKWY','SUWANEE','GA','30024-4095','GWINNETT COUNTY','UNITED STATES' UNION
SELECT 'c14db2b2-1ced-4413-8d03-015a354a9bb2','9204','CASTLEWOOD NB LLC','301 CASTLEWOOD DR','NEW BRAUNFELS','TX','78130-8150','COMAL COUNTY','UNITED STATES' UNION
SELECT '162d3098-f34d-42d1-80dc-da60eaa04949','5223','CATHOLIC CHARITIES HOUSING DEVELOPMENT CORPORATION','721 N LA SALLE DR 5TH FL','CHICAGO','IL','60654-3503','COOK COUNTY','UNITED STATES' UNION
SELECT 'b38d70a2-99d9-4e46-89a5-cbe9c62139a3','22212','CATHOLIC CHARITIES OF CENTRAL FLORIDA, INC','1819 N SEMORAN BLVD','ORLANDO','FL','32807-3546','ORANGE COUNTY','UNITED STATES' UNION
SELECT 'd2de5f50-fbbf-465e-b06d-71d0be0bab6a','7142','CATHOLIC HOUSING FOR THE ELDERLY AND HANDICAPPED INC. D/B/A CATHOLIC HOUSING MANAGEMENT','11410 N KENDALL DR STE 306','MIAMI','FL','33176-1031','MIAMI-DADE COUNTY','UNITED STATES' UNION
SELECT 'af29b0d7-5149-4837-9749-4adedb1e0a89','8984','CATHOLIC HOUSING INITIATIVES, INC.','2401 LAKE PARK DR SE','SMYRNA','GA','30080-8862','COBB COUNTY','UNITED STATES' UNION
SELECT '57358d1a-9735-42fd-8765-0e518010e90b','18841','CATRANEL REALTY CO.','901 ELIZABETH ST','PITTSBURGH','PA','15221-3991','ALLEGHENY COUNTY','UNITED STATES' UNION
SELECT 'e4e318be-33a0-436b-be38-a8dd74a29759','6497','','','','','','','' UNION
SELECT 'a163a6d1-bc5d-4d3a-8cac-ef129476a899','21444','CEABOS MANAGEMENT COMPANY LLC','2501 GRIFFIN ST','LAKE CHARLES','LA','70601-2015','CALCASIEU PARISH','UNITED STATES' UNION
SELECT '8a92d510-3ba8-4f19-8c0b-d2eb8808051d','15164','CELADON PROPERTIES INC','403 BUTTERCUP CREEK BLVD','CEDAR PARK','TX','78613-3709','WILLIAMSON COUNTY','UNITED STATES' UNION
SELECT '3b2eb1f5-fec6-4358-b5cf-3fe20331b7c7','3045','CENTENNIAL VILLAGE CORP','130 W BROWN RD','MESA','AZ','85201-3445','MARICOPA COUNTY','UNITED STATES' UNION
SELECT '854c6eca-5bc1-43d9-9057-a14a3f0fee41','4889','CENTRA ASSET PARTNERS','1980 POST OAK BLVD STE 1200','HOUSTON','TX','77056-3970','HARRIS COUNTY','UNITED STATES' UNION
SELECT '480eb70b-71ba-4cd3-b3d0-a7fe2fba278d','6343','CENTRAL MANAGEMENT INC','820 GESSNER RD STE 1525','HOUSTON','TX','77024-4472','HARRIS COUNTY','UNITED STATES' UNION
SELECT 'cd958ec6-333e-4c1a-a897-65732b1aa907','7179','CENTRAL PLAINS DEVELOPMENT, LLC','3713 E ROSS PKWY STE 200','WICHITA','KS','67210-1359','SEDGWICK COUNTY','UNITED STATES' UNION
SELECT 'b0c668cc-ca48-42a5-8002-fd66a59d2144','4891','CENTRAL STATES DEVELOPMENT, LLC','740 S 75TH ST','OMAHA','NE','68114-4621','DOUGLAS COUNTY','UNITED STATES' UNION
SELECT '482259b0-a4b2-4d76-a431-934c1cc3d02b','5508','CENTURY SALES AND MANAGEMENT','2855 S 70TH ST STE 200','LINCOLN','NE','68506-3700','LANCASTER COUNTY','UNITED STATES' UNION
SELECT 'f9dc3775-d00d-436c-8d23-67c5cd096185','14827','','','','','','','' UNION
SELECT 'd4589352-35f9-44c0-8a48-f7a097ead0e3','691','CESAR E CHAVEZ FOUNDATION','3270 NACOGDOCHES RD','SAN ANTONIO','TX','78217-3349','BEXAR COUNTY','UNITED STATES' UNION
SELECT 'f5c090fa-78ab-452f-b504-98aafee09121','379','CF REAL ESTATE SERVICES','710 PEACHTREE ST NE STE 100','ATLANTA','GA','30308-1200','FULTON COUNTY','UNITED STATES' UNION
SELECT '36c2039d-dfd5-48fc-8628-6ba805966c1f','980','CHAMPLAIN HOUSING TRUST','88 KING ST','BURLINGTON','VT','05401-5089','CHITTENDEN COUNTY','UNITED STATES' UNION
SELECT '8e89843f-fe37-42f7-84bd-a3b1ba57a6d3','6455','CHANDLER MANAGEMENT CORPORATION','11719B JEFFERSON AVE STE 103','NEWPORT NEWS','VA','23606-2070','NEWPORT NEWS CITY','UNITED STATES' UNION
SELECT 'fd3499ca-4baa-4764-b9bb-a78b63eda360','5471','CHAPMAN ACRES LLC','75 HOCKANUM BLVD','VERNON','CT','06066-4056','','UNITED STATES' UNION
SELECT '35578ba9-3950-43fb-adee-5a44cc998fb2','7203','CHARGER DEVELOPMENT LLC','1302 SAYLES BLVD','ABILENE','TX','79605-4206','TAYLOR COUNTY','UNITED STATES' UNION
SELECT '7dd3f98c-e9d7-4372-b996-85531b02bc12','2227','CHARLES WILLIAMS REAL ESTATE INVESTMENT CORPORATION','5 E 6TH AVE','ROME','GA','30161-6001','FLOYD COUNTY','UNITED STATES' UNION
SELECT '20842c47-db5c-476c-a3de-240a64761699','23483','CHARLOTTE MANAGEMENT COMPANY LLC','7388 CABOT DR','NASHVILLE','TN','37209-4348','DAVIDSON COUNTY','UNITED STATES' UNION
SELECT 'b1495943-62e0-4fb3-8d0a-7f6d349d923d','11812','CHESTNUT CROSSING, LLC','6253 GRAND RIVER RD STE 700','BRIGHTON','MI','48114-7714','LIVINGSTON COUNTY','UNITED STATES' UNION
SELECT 'fec85ade-b616-4ebb-b5a1-8d6b46cd67c7','15574','CHG SENIOR LIVING, LLC','2200 ROSS AVE STE 5400','DALLAS','TX','75201-7918','DALLAS COUNTY','UNITED STATES' UNION
SELECT 'a5728767-8ebb-40bd-be1c-741cd9117bd0','7074','CHRISTOPHER COMMERCIAL INC','12918 SHOPS PKWY STE 600','AUSTIN','TX','78738-6630','TRAVIS COUNTY','UNITED STATES' UNION
SELECT '2dea6f47-5ebb-4f6b-8aa2-7450927d9f0f','6578','CHRISTOPHER COMMUNITY, INC.','990 JAMES ST','SYRACUSE','NY','13203-2879','ONONDAGA COUNTY','UNITED STATES' UNION
SELECT 'ffd1d674-b608-4978-8443-e620ce882874','1379966','','','','','','','' UNION
SELECT '6587bd85-0960-458b-b331-d289c9b4711e','8197','CIM GROUP LP','4700 WILSHIRE BLVD','LOS ANGELES','CA','90010-3853','LOS ANGELES COUNTY','UNITED STATES' UNION
SELECT '84793a0d-4fdd-407f-b160-66acce25550c','22588','','','','','','','' UNION
SELECT 'e3856890-036f-4ce5-9746-57655476b07d','23414','CIRCLE K PROPERTY MANAGEMENT LLC','13311 231ST ST E','GRAHAM','WA','98338-8937','PIERCE COUNTY','UNITED STATES' UNION
SELECT '3d17cd22-eddb-4e22-b661-8b68d1b38363','5445','CIS MANAGEMENT INC','1970 BRUNSWICK AVE, STE 100','LAWRENCEVILLE','NJ','08648-4667','','UNITED STATES' UNION
SELECT 'cd8f8c99-3c24-4dd0-974a-ada81ef38f37','20956','CITY CARE, INC.','2400 GENERAL PERSHING BLVD','OKLAHOMA CITY','OK','73107-6400','OKLAHOMA COUNTY','UNITED STATES' UNION
SELECT 'cfe8e141-b613-4c29-a868-ed06a362ee96','1379967','','','','','','','' UNION
SELECT 'ce87fa3c-5f9f-443b-93bb-36979e7b6ecf','571','CIVIC PROPERTIES, LLC','60 W BROAD ST STE 102','BETHLEHEM','PA','18018-5721','NORTHAMPTON COUNTY','UNITED STATES' UNION
SELECT '0c51ba21-7034-422a-bde6-643fc4a3406c','1893','CIVITAS SENIOR HEALTHCARE','777 MAIN ST STE 2300','FORT WORTH','TX','76102-5304','TARRANT COUNTY','UNITED STATES' UNION
SELECT '25e4fbd8-1d3a-46e4-8e89-3c8059e6fd8b','1379631','CJEHN PARTNERS LLC','201 SYLVIA DR','YORKTOWN','VA','23693-2208','YORK COUNTY','UNITED STATES' UNION
SELECT 'caac8529-a9fc-45c8-9a99-bfc117bca826','1371767','CLARIDGE HOUSE ASSOCIATES LIMITED PARTNERSHIP, LLP','301 N RIPLEY ST','ALEXANDRIA','VA','22304-5701','ALEXANDRIA CITY','UNITED STATES' UNION
SELECT '54b8a516-3dce-4bbe-be67-d9bca4864a42','1379602','CLARION MANAGEMENT INC.','101 PACIFICA STE 260','IRVINE','CA','92618-7342','ORANGE COUNTY','UNITED STATES' UNION
SELECT 'f9635e54-23aa-4a24-bf59-85c2c53541a4','6553','','','','','','','' UNION
SELECT '4414cc1e-d4c2-45d2-b3c9-87221116dfb2','8392','CLASS A MANAGEMENT','1187 ROWLETT RD','GARLAND','TX','75043-2826','DALLAS COUNTY','UNITED STATES' UNION
SELECT '787ee640-1a09-43c6-8a69-f89a3815ec54','1379130','CLEAR MOUNTAIN PROPERTIES','50 UPPER MONTCLAIR PLZ STE 210','MONTCLAIR','NJ','07043-1346','ESSEX COUNTY','UNITED STATES' UNION
SELECT '7f122e3e-022b-4526-b738-b28a348378df','7088','CLEAR PROPERTY MANAGEMENT LLC','9050 N CAPITAL OF TEXAS HWY STE 320  BLDG 3','AUSTIN','TX','78759-7288','TRAVIS COUNTY','UNITED STATES' UNION
SELECT '9a9a33be-f7b6-47e9-8930-7542fed2bf41','4874','CLEVELAND MANOR INC','2200 CLEVELAND AVE','MIDLAND','MI','48640-5597','MIDLAND COUNTY','UNITED STATES' UNION
SELECT 'a8a20b67-4036-4206-92e9-2f42a0c262ef','1379721','','','','','','','' UNION
SELECT '74c63d17-6d09-4a63-8975-016a6a93462d','1297','CLK MULTI-FAMILY MANAGEMENT, LLC','5545 MURRAY AVE','MEMPHIS','TN','38119-3806','SHELBY COUNTY','UNITED STATES' UNION
SELECT 'ea5440c7-bbef-477f-9a23-2a11c54e2dfb','5106','CLOUD PEAK INVESTMENTS, INC.','721 BIG HORN AVE','WORLAND','WY','82401-2619','WASHAKIE COUNTY','UNITED STATES' UNION
SELECT '283a0095-27c8-4a83-8b02-f998ca4dd4be','3285','CNC MANAGEMENT INC','1223 SW 4TH ST','MIAMI','FL','33135-2407','MIAMI-DADE COUNTY','UNITED STATES' UNION
SELECT '2219c4a5-f519-48a4-87fe-db77103aae55','1379654','COAST EQUITY PARTNERS III, LLC','2829 RUCKER AVE','EVERETT','WA','98201-3456','SNOHOMISH COUNTY','UNITED STATES' UNION
SELECT '6f42169a-b6c4-41d5-a100-32440db3e299','1338','COAST REAL ESTATE SERVICES','2829 RUCKER AVE STE 100','EVERETT','WA','98201-3456','SNOHOMISH COUNTY','UNITED STATES' UNION
SELECT '9acb8684-9d2a-4a30-b4dd-caf73778f198','5584','COASTAL RIDGE MANAGEMENT, LLC','80 E RICH ST','COLUMBUS','OH','43215-5249','FRANKLIN COUNTY','UNITED STATES' UNION
SELECT 'dff18d06-0a6e-4114-b233-178d666856ba','2018','COASTLINE MANAGEMENT SERVICES LP','17 E BAR LE DOC DR','CORPUS CHRISTI','TX','78414-6156','NUECES COUNTY','UNITED STATES' UNION
SELECT '052f9c97-e133-41f1-9cf5-13181c873269','6169','COASTLINE REAL ESTATE ADVISORS, INC.','134 LOMITA ST','EL SEGUNDO','CA','90245-4113','LOS ANGELES COUNTY','UNITED STATES' UNION
SELECT '0389381d-6991-402b-80f2-90b0bf77722c','3300','COGIC MEMORIAL HOME FOR THE ELDERLY','2412 E VIRGINIA BEACH BLVD','NORFOLK','VA','23504-3628','NORFOLK CITY','UNITED STATES' UNION
SELECT '6342e54a-86e2-4b70-97e6-95bb4051a366','7337','COHEN ESREY, LLC','8500 SHAWNEE MISSION PKWY STE 150','MERRIAM','KS','66202-2960','JOHNSON COUNTY','UNITED STATES' UNION
SELECT 'ba52f0b0-e6bd-45c5-a302-69ee06519f1f','8850','','','','','','','' UNION
SELECT '7f90e8d3-c595-4d09-862a-9d45fbd8cbd0','6532','COLONY APARTMENT HOMES','9211 QUIOCCASIN RD','RICHMOND','VA','23229-5425','HENRICO COUNTY','UNITED STATES' UNION
SELECT '46611da1-be9c-4919-8170-fae2a0347434','667','COMCAP MANAGEMENT LLC','7200 S ALTON WAY STE A310','CENTENNIAL','CO','80112-2253','ARAPAHOE COUNTY','UNITED STATES' UNION
SELECT 'a3dd017e-2f07-410c-a576-00780b214fcd','2671','COMCAPP, LLC','421 6TH ST','SAN ANTONIO','TX','78215-1805','BEXAR COUNTY','UNITED STATES' UNION
SELECT '2fec7bec-e710-436e-ade3-6c963c3453b2','16838','COMMERCIAL INVESTMENT PROPERTIES (CIP)','7211 S 27TH ST','LINCOLN','NE','68512-4831','LANCASTER COUNTY','UNITED STATES' UNION
SELECT '83476c0c-392d-43dc-8eac-866e8d3364a0','428','COMMERCIAL PROPERTY MANAGEMENT INC','12021 WILSHIRE BLVD STE 646','LOS ANGELES','CA','90025-1206','LOS ANGELES COUNTY','UNITED STATES' UNION
SELECT '08e02749-1112-496f-a20f-4427a76bc4eb','6201','COMMONSPACE PROPERTIES, INC','201 E JEFFERSON ST STE 200','SYRACUSE','NY','13202-2646','ONONDAGA COUNTY','UNITED STATES' UNION
SELECT '4f055c8f-9674-4e37-a9d6-f72d7b9899d8','1371930','','','','','','','' UNION
SELECT '7ec48756-9e19-4b89-b9e3-b907bfa43f89','5082','COMMUNITY HOUSING SERVICES INC','649 E SOUTH TEMPLE','SALT LAKE CITY','UT','84102-1153','SALT LAKE COUNTY','UNITED STATES' UNION
SELECT '463811ff-1338-4c60-af86-c389fc1ee6cb','9354','COMMUNITY MANAGEMENT ASSOCIATES, INC.','1465 NORTHSIDE DR NW STE 128','ATLANTA','GA','30318-4220','FULTON COUNTY','UNITED STATES' UNION
SELECT 'cbc17b3b-e0bd-49ad-9e3a-aff264c36e41','1379968','','','','','','','' UNION
SELECT '4627c461-0c7e-4f35-9581-76f3c1d7c48b','865','COMMUNITY REALTY MGMT INC','36 S MAIN ST','PLEASANTVILLE','NJ','08232-2728','ATLANTIC COUNTY','UNITED STATES' UNION
SELECT '11ef99be-94cf-4560-b153-808ca6808a97','15738','COMMUNITY SUPPORT PROGRAMS, INC.','2924 KNIGHT ST STE 326','SHREVEPORT','LA','71105-2413','CADDO PARISH','UNITED STATES' UNION
SELECT 'e9dc8a87-30f5-4424-b606-31c53054622f','4808','COMPANION ASSOCIATES, INC.','2 WHARFSIDE ST STE 3-O','CHARLESTON','SC','29401-1658','','UNITED STATES' UNION
SELECT '43195648-fa3d-4983-b5f3-4d58f5f865ed','1379090','COMUNILIFE, INC.','462 SEVENTH AVENUE 3RD FLOOR','NEW YORK','NY','10018-7445','','UNITED STATES' UNION
SELECT 'c9807148-e535-4a45-ba7c-e09c1cdd4bcd','339','CONAM MANAGEMENT CORPORATION','3990 RUFFIN RD STE 100','SAN DIEGO','CA','92123-4805','SAN DIEGO COUNTY','UNITED STATES' UNION
SELECT '7f3880a7-862a-4ec9-bbdb-02da5f0eabdc','1237','CONCORD MANAGEMENT LIMITED','2605 MAITLAND CENTER PKWY STE A','MAITLAND','FL','32751-7139','ORANGE COUNTY','UNITED STATES' UNION
SELECT 'bf175f5a-f5ca-44f5-9c65-0dba934c5a35','13251','CONCRETE VENTURES LLC','1009 W BOSTON POST RD','MAMARONECK','NY','10543-3329','WESTCHESTER COUNTY','UNITED STATES' UNION
SELECT '0faae547-b91d-407d-97ee-bde3e98f3e9f','6035','CONNECTICUT MANAGEMENT LIMITED LIABILITY COMPANY','418 CLIFTON AVE STE 205','LAKEWOOD','NJ','08701-3749','OCEAN COUNTY','UNITED STATES' UNION
SELECT 'c059e78a-7378-47d9-bc6f-4f424c96a424','23137','CONNECTICUT REAL ESTATE MANAGEMENT LLC','50 SOUTHWICK CT 2ND FL','CHESHIRE','CT','06410-3499','NEW HAVEN COUNTY','UNITED STATES' UNION
SELECT '51fa32e8-4a01-4bd4-8608-3ff952ae6835','25254','CONREX PROPERTY MANAGEMENT LLC','1505 KING STREET EXT STE 100','CHARLESTON','SC','29405-9442','CHARLESTON COUNTY','UNITED STATES' UNION
SELECT 'd314d685-c218-4573-86e6-bae933c7774b','174','CONTINENTAL MANAGEMENT','32600 TELEGRAPH RD','BINGHAM FARMS','MI','48025-2411','OAKLAND COUNTY','UNITED STATES' UNION
SELECT '07ade7cb-093d-4556-bc11-557512633e92','9386','','','','','','','' UNION
SELECT '897d1f26-c24b-4ca6-b030-704ee0dc6bcc','340','CONTINENTAL PROPERTIES COMPANY, INC.','W134N8675 EXECUTIVE PKWY','MENOMONEE FALLS','WI','53051-3310','WAUKESHA COUNTY','UNITED STATES' UNION
SELECT 'c2a5532c-c137-4bf2-a00d-34d5ff0d9a55','1185','CONTINENTAL PROPERTY SERVICES INC','444 SEABREEZE BLVD STE 600','DAYTONA BEACH','FL','32118-3951','','UNITED STATES' UNION
SELECT '8e74c03e-ee9d-4b21-8e8d-dd6fbbfda4a7','8180','','','','','','','' UNION
SELECT 'b65dafd8-ce2f-429b-a0bd-1f5e3ef2725b','1153','CONTRAVEST MANAGEMENT COMPANY','237 S WESTMONTE DR STE 140','ALTAMONTE SPRINGS','FL','32714-4263','','UNITED STATES' UNION
SELECT '1422bdca-166c-482b-9772-5fa6a4858681','1376715','COOGAN TERRACE LDHA LP','3501 OAKWOOD BLVD','MELVINDALE','MI','48122-1181','WAYNE COUNTY','UNITED STATES' UNION
SELECT 'faa3cd54-9893-4d6c-99a7-e9e700fc4581','6477','CORCORAN JENNISON MGMT LLC','150 MOUNT VERNON ST STE 520','DORCHESTER','MA','02125-3115','SUFFOLK COUNTY','UNITED STATES' UNION
SELECT '2abf2e06-0299-4a2b-8714-120f62ea7db7','1330','CORCORAN MANAGEMENT','100 GRANDVIEW RD STE 205','BRAINTREE','MA','02184-2686','NORFOLK COUNTY','UNITED STATES' UNION
SELECT 'c72e9c0b-c11e-4eae-999b-d7d29736f188','6624','CORE DESIGN & BUILD LLC DBA CORE MANAGEMENT GROUP','6919 PORTWEST DR STE 150','HOUSTON','TX','77024-8010','HARRIS COUNTY','UNITED STATES' UNION
SELECT '712a04df-8117-4029-bdc6-f65a770d17f1','604','CORE REDEVELOPMENT','549 N SENATE AVE','INDIANAPOLIS','IN','46204-1270','MARION COUNTY','UNITED STATES' UNION
SELECT '4d5a1b76-f29a-46a2-b6b2-d30a299ae490','803','CORNERSTONE MANAGED PROPERTIES','2147 E 28TH ST','LORAIN','OH','44055-1932','LORAIN COUNTY','UNITED STATES' UNION
SELECT '81d53d5f-4fe4-4340-82fd-bae62f852a15','1334','CORNERSTONE RESIDENTIAL','1525 N MAIN ST STE 105','BOUNTIFUL','UT','84010-6167','DAVIS COUNTY','UNITED STATES' UNION
SELECT '683438ec-a77d-4080-8b4d-b98c99cc7123','1217','','','','','','','' UNION
SELECT '872ac166-5b82-4deb-8b89-139b177e61ec','363','CORTLAND MANAGEMENT, LLC','3424 PEACHTREE RD NE STE 300','ATLANTA','GA','30326-2818','FULTON COUNTY','UNITED STATES' UNION
SELECT '79bda49c-3ca6-4c04-9977-9923f8e141d9','6053','CORVIAS GROUP, LLC','1405 S COUNTY TRL STE 530','EAST GREENWICH','RI','02818-5081','KENT COUNTY','UNITED STATES' UNION
SELECT '6d357c11-fe89-4f71-9a8f-491375cb5f2d','1380378','CORVUS PROPERTY INTELLIGENCE, LLC','1707 N CHARLES ST APT 200A','BALTIMORE','MD','21201-5835','BALTIMORE CITY','UNITED STATES' UNION
SELECT '7cde5adc-78f0-4865-b4dd-e6dd5b8defd9','1376877','COUGHLIN AND COMPANY','140 E 19TH AVE STE 700','DENVER','CO','80203-1035','DENVER COUNTY','UNITED STATES' UNION
SELECT '3e96065d-41f7-4ce3-aafb-1630fde2718d','3678','COUGHLIN MGMT CO','206 1ST AVE','ASBURY PARK','NJ','07712-6204','MONMOUTH COUNTY','UNITED STATES' UNION
SELECT '15fe20d4-8dab-4cfd-bdc7-3f45f5da1d0f','3284','COUNCIL FOR JEWISH ELDERLY','3101 W TOUHY AVE','CHICAGO','IL','60645-2801','COOK COUNTY','UNITED STATES' UNION
SELECT '8d1e96e1-6316-4952-8248-3f1f0a67f019','29945','COUNCIL GARDENS','2501 N TAYLOR RD','CLEVELAND HEIGHTS','OH','44118-1391','CUYAHOGA COUNTY','UNITED STATES' UNION
SELECT 'a4fd767f-ef2e-48f3-bbca-82a37d308d50','24710','COVINGTON HOUSING AUTHORITY','5160 ALCOVY RD NE','COVINGTON','GA','30014-1358','NEWTON COUNTY','UNITED STATES' UNION
SELECT 'eddeb50c-f24d-478c-a2df-8eb91d176af3','23477','CPMC INC DBA WESTHOME','1901 AVENUE OF THE STARS','LOS ANGELES','CA','90067-6001','LOS ANGELES COUNTY','UNITED STATES' UNION
SELECT '18a372f6-ca83-4a3d-a3cf-6e1f07dcc161','1374676','CRAIN COMMERCIAL REAL ESTATE, LLC','1 CHENAL VILLAGE CIR','LITTLE ROCK','AR','72223-3003','PULASKI COUNTY','UNITED STATES' UNION
SELECT '28a9d824-5cc4-46e2-a57e-60336a7454bd','6355','CRAWFORD COMMUNITIES, LLC','6640 RIVERSIDE DR STE 500','DUBLIN','OH','43017-9503','FRANKLIN COUNTY','UNITED STATES' UNION
SELECT '408b94af-3c82-4c89-8d08-7e9ae2dcc545','5582','CRC ASSET MANAGEMENT SERVICES LLC','4401 WILSON BLVD STE 600','ARLINGTON','VA','22203-4195','ARLINGTON COUNTY','UNITED STATES' UNION
SELECT 'd94841d6-be4c-4ea8-9a33-68247bbb3c06','6566','CREATIVE PROPERTIES','8323 SOUTHWEST FWY STE 330','HOUSTON','TX','77074-1616','HARRIS COUNTY','UNITED STATES' UNION
SELECT 'aaa0c8bd-5f8b-4f79-8008-bdaf0b90b919','1379969','','','','','','','' UNION
SELECT '9e327a09-d857-4081-9697-dd087c84c810','6238','CRES MANAGEMENT','9201 WARD PKWY STE 200','KANSAS CITY','MO','64114-3309','','UNITED STATES' UNION
SELECT '47026a07-c023-4fe1-a262-b11d4e5a9f82','6615','CREST ASSET MGMT','1400 CIVIC PL STE 225','SOUTHLAKE','TX','76092-7644','','UNITED STATES' UNION
SELECT '0ac1415d-4e1c-4dd6-8292-008b2aeed1bf','264','CRESTLINE COMMUNITIES','1333 E 86TH ST','INDIANAPOLIS','IN','46240-1909','MARION COUNTY','UNITED STATES' UNION
SELECT '7c88832e-c26c-4406-a205-c5d13aa683b3','5198','CRESTWOOD MANAGEMENT','2600 NONESUCH RD','ABILENE','TX','79606-1567','','UNITED STATES' UNION
SELECT 'dac3308c-2d6f-4a64-9afb-5e3e797a5e49','6081','CRF AFFORDABLE HOUSING, INC.','44 N GIRLS SCHOOL RD','INDIANAPOLIS','IN','46214-3960','MARION COUNTY','UNITED STATES' UNION
SELECT 'd2a57e4b-566a-4dec-bcd5-93cdc186905e','1380354','CRGP, LLC','211 N BROADWAY STE 1900','SAINT LOUIS','MO','63102-2748','ST. LOUIS CITY','UNITED STATES' UNION
SELECT '78bb36e3-c735-4182-89ea-ab44c0b4affe','1809','CRIMSON MANAGEMENT (FORMERLY BAYROCK CORPORATION)','6000 LAKE FORREST DR STE 430','SANDY SPRINGS','GA','30328-3896','FULTON COUNTY','UNITED STATES' UNION
SELECT '2b36b007-84b7-4ef7-b151-6dee6c3f77a6','23011','','','','','','','' UNION
SELECT '55c51598-61a7-464f-ba0a-41703abeb647','557','CROFT RESIDENTIAL LLC','1832 SECOND AVE','DECATUR','GA','30032-3970','DEKALB COUNTY','UNITED STATES' UNION
SELECT 'f16a3bb3-5ace-4e41-b67b-6878e554eb6c','1791','','','','','','','' UNION
SELECT 'ee1d9823-0090-43ae-903e-02a43d191f12','5972','CROW FAMILY, INC.','3819 MAPLE AVE','DALLAS','TX','75219-3913','DALLAS COUNTY','UNITED STATES' UNION
SELECT '5aaf6600-93d0-4ca0-b6c2-a6c40071f275','1374874','CROWN RETAIL SERVICES LLC','667 MADISON AVE 12TH FL','NEW YORK','NY','10065-8029','NEW YORK COUNTY','UNITED STATES' UNION
SELECT '7fd6ead0-f769-4d4a-9d11-863327e2b0d5','1371935','N/A','N/A','N/A','N/A','N/A','N/A','N/A' UNION
SELECT '6ac64843-4e2d-4d11-a60c-7254a8214c6f','1371936','N/A','N/A','N/A','N/A','N/A','N/A','N/A' UNION
SELECT '5ff48f0a-23e5-4e3b-af5c-8e4c3d2f510c','1374855','CRP OSBOURNE, LLC','122 E 42ND ST RM 1903','NEW YORK','NY','10168-1999','NEW YORK COUNTY','UNITED STATES' UNION
SELECT 'f6211548-37d4-4ac3-8c1a-bff2f4f7c9a8','15315','CRV INVESTMENTS LLC','801 BRIARWOOD ST','WEATHERFORD','TX','76087-9398','PARKER COUNTY','UNITED STATES' UNION
SELECT '8d2855b7-20ea-4c99-89b6-76f339136b9c','1378712','CSC MANAGEMENT LLC','1280 S UTE AVE STE 10','ASPEN','CO','81611-2259','PITKIN COUNTY','UNITED STATES' UNION
SELECT '687af131-0d9b-4eb1-8f6f-1200c2bb06ec','5614','CSL WOODWARD MASTER OPERATOR, LLC','1450 W LONG LAKE RD STE 300','TROY','MI','48098-6330','OAKLAND COUNTY','UNITED STATES' UNION
SELECT '8637f32d-2e19-4b5b-ad1e-fc274095409a','14940','','','','','','','' UNION
SELECT '76f1369d-c1c6-424d-bd70-1a5069dc9f44','6188','','','','','','','' UNION
SELECT '7b7531ad-26da-4481-8f17-a425b3b8d7d3','709','CTM LLC','399 PERRY ST STE 203','CASTLE ROCK','CO','80104-4007','DOUGLAS COUNTY','UNITED STATES' UNION
SELECT 'a41c3c17-3730-4e5f-a567-636aff45f3a1','5622','CVAL GROUP, LLC','21 BRIAR HOLLOW LN UNIT 408','HOUSTON','TX','77027-2807','','United States' UNION
SELECT 'a446a2e1-21dd-49f4-8c18-dcb252ea60f3','6931','CWS APARTMENT HOMES LLC','9606 N MOPAC EXPY','AUSTIN','TX','78759-5932','TRAVIS COUNTY','UNITED STATES' UNION
SELECT '40da3a81-f831-4e92-98f7-b5e88e3c00f7','84','CYPRESS POINT MANAGEMENT LLC','55 WAUGH DR STE 600','HOUSTON','TX','77007-5837','HARRIS COUNTY','UNITED STATES' UNION
SELECT '65ee8b1e-4cf4-4041-81cf-641e86599b72','22786','CYPRESSBROOK MULTIFAMILY MANAGEMENT, LP','1776 WOODSTEAD CT STE 218','THE WOODLANDS','TX','77380-1480','MONTGOMERY COUNTY','UNITED STATES' UNION
SELECT '6853b14b-5546-428f-a96c-989cff57e66d','431','D & K MANAGEMENT, INC.','19222 HIGHWAY 11 E','LENOIR CITY','TN','37772-5452','LOUDON COUNTY','UNITED STATES' UNION
SELECT '00be0855-f477-4f2a-a0ac-25ebe4e00431','6842','D.P. PREISS COMPANY, INC DBA THE PREISS COMPANY','1700 HILLSBOROUGH ST','RALEIGH','NC','27605-1641','WAKE COUNTY','UNITED STATES' UNION
SELECT '8cf6c162-b00a-449c-a6d7-8d35f3f1f9ab','5909','DAKOTA ENTERPRISES LLC DBA DAKOTA PROPERTY MGMT','2100 WEST LOOP S STE 780','HOUSTON','TX','77027-3553','HARRIS COUNTY','UNITED STATES' UNION
SELECT '2dddcca1-91f9-47ad-a64e-a4fcc0b2dda9','5293','DALMARK MANAGEMENT GROUP LLC','12220 STATE LINE RD','LEAWOOD','KS','66209-1217','JOHNSON COUNTY','UNITED STATES' UNION
SELECT 'fc2a3a83-2894-4bb1-8c62-3c3e0bf8b748','1379970','','','','','','','' UNION
SELECT 'b4b5eb15-57ea-4830-808a-c4a8feb2ab88','1380018','','','','','','','' UNION
SELECT 'e8861182-ae45-47a4-9bc8-2550efdc37df','1377210','DANTIN BRUCE MANAGEMENT LLC','4469 BLUEBONNET BLVD','BATON ROUGE','LA','70809-9639','EAST BATON ROUGE PARISH','UNITED STATES' UNION
SELECT '66414cd7-3fdf-43ee-afe9-02308cb41b15','6367','DARBY DEVELOPMENT','4142 DORCHESTER RD','CHARLESTON','SC','29405-7462','CHARLESTON COUNTY','UNITED STATES' UNION
SELECT '0f21535d-0065-45db-9d8b-ba5d9fc9ab8e','266','','','','','','','' UNION
SELECT 'd36378a3-1743-4678-93fc-7f847923b249','4287','DAVID NICKLAS ORGAN DONOR AWARENESS FOUNDATION, INC','2935 S BELT LINE RD','GRAND PRAIRIE','TX','75052-5999','DALLAS COUNTY','UNITED STATES' UNION
SELECT 'f871f571-3b5c-42bf-b0d3-d69a6866af0a','7014','DAYRISE RESIDENTIAL, LLC','1700 WEST LOOP S STE 350','HOUSTON','TX','77027-3005','HARRIS COUNTY','UNITED STATES' UNION
SELECT '3978610a-1d40-4bbd-a995-5990e4869d7d','6618','DECRON PROPERTIES CORP.','6222 WILSHIRE BLVD STE 400','LOS ANGELES','CA','90048-5100','LOS ANGELES COUNTY','UNITED STATES' UNION
SELECT 'b5ce6cd6-46d5-451c-a7cf-b0aba608a91b','1375107','DEL REY TERRACE','2698 JUNIPERO AVE STE 101A','SIGNAL HILL','CA','90755-2145','LOS ANGELES COUNTY','UNITED STATES' UNION
SELECT '2d2c772e-73bf-437d-a33b-7f5549f2d938','19297','DEMCO MANAGEMENT, INC.','1551 ATLANTIC BLVD STE 300','JACKSONVILLE','FL','32207-3368','DUVAL COUNTY','UNITED STATES' UNION
SELECT '9b4f94fe-5646-4886-a7a5-184f935a2108','1853','DEMMON PARTNERS','601 UNIVERSITY AVE STE 110','SACRAMENTO','CA','95825-6706','SACRAMENTO COUNTY','UNITED STATES' UNION
SELECT 'd4be1f0c-487b-46ca-ad1d-19fb258e727c','182','DENSTOCK MANAGEMENT LLC','1430 ROLKIN CT STE 201','CHARLOTTESVILLE','VA','22911-3582','ALBEMARLE COUNTY','UNITED STATES' UNION
SELECT '0a166c51-2156-454d-b495-c37d5ab0fa1d','996','DEPAUL HOUSING','41 N MAIN AVE','ALBANY','NY','12203-1414','ALBANY COUNTY','UNITED STATES' UNION
SELECT 'db01c73c-0042-4a17-b0fa-29247278ca3d','18726','DEVON PROPERTIES LTD.','990 FORT ST STE 100','VICTORIA','BC','V8V 3K2','CAPITAL','CANADA' UNION
SELECT '83ba9089-fe4e-4b75-ac0d-38001ef05521','1168','DEVONSHIRE REAL ESTATE AND ASSET MTG','17304 PRESTON RD STE 340','DALLAS','TX','75252-5649','COLLIN COUNTY','United States' UNION
SELECT 'afe7c2b4-988c-4fc8-8425-96b708c81750','6295','DHD VENTURES MANAGEMENT INC.','401 E SOUTH MAIN ST UNIT 125A','WAXHAW','NC','28173-0235','UNION COUNTY','UNITED STATES' UNION
SELECT '35f56fc7-872a-4f58-8d4c-15f328d22864','5300','DIAL EQUITIES, INC., D/B/A HALEY RESIDENTIAL','10703 J ST','OMAHA','NE','68127-1023','DOUGLAS COUNTY','UNITED STATES' UNION
SELECT '9509f29d-4417-4c07-9f42-06481e614380','5615','DIAMOND MANAGEMENT','10735 MESQUITE FLT','HELOTES','TX','78023-4212','BEXAR COUNTY','UNITED STATES' UNION
SELECT '299e8e6a-895d-4119-a3b7-1a61baeeb10e','5971','DIETZ PROPERTY GROUP','2075 W BIG BEAVER RD STE 100','TROY','MI','48084-3437','OAKLAND COUNTY','UNITED STATES' UNION
SELECT '2228cb00-0d27-4ea7-a8d3-c94b730c37a4','7077','DIME MANAGEMENT CORPORATION','2071 FLATBUSH AVE # 48','BROOKLYN','NY','11234-4340','KINGS COUNTY','UNITED STATES' UNION
SELECT 'a6ee27ee-ee2b-4203-aef6-4139feeb1a80','463','DIOCESAN HOUSING SERVICE CORP OF THE DIOCESE OF CAMDEN','1845 HADDON AVE','CAMDEN','NJ','08103-3008','CAMDEN COUNTY','UNITED STATES' UNION
SELECT '755a99de-002e-4841-9e5b-cdc27fda09ba','1379311','DISCIPLES HOUSE OF GLENDALE, INC.','7987 N 53RD AVE OFC','GLENDALE','AZ','85301-8663','MARICOPA COUNTY','UNITED STATES' UNION
SELECT '9b1e23d5-5e37-402d-98be-a7737a5568ad','1375261','DISRUPT EQUITY, LLC','11010 NEESHAW DR BLDG B','HOUSTON','TX','77065-5274','HARRIS COUNTY','UNITED STATES' UNION
SELECT 'b4adb7cb-0965-47aa-a9e4-92099c0a2bf8','1378080','','','','','','','' UNION
SELECT 'f3998162-f1fc-4115-abf2-849f129fb00d','1378131','DJE PROPERTIES LLC','7770 PIPERS LN','SAN ANTONIO','TX','78251-1660','BEXAR COUNTY','UNITED STATES' UNION
SELECT 'b7b5b0f0-934f-40f2-b86f-fb399dc64f70','822','DKD PROPERTY MGMT COMP','255 W JULIAN ST STE 301','SAN JOSE','CA','95110','','UNITED STATES' UNION
SELECT 'aa8445b6-3ca2-4eab-ba2d-b787b3c9374f','6327','DMA PROPERTIES LLC','4101 PARKSTONE HEIGHTS DR STE 310','AUSTIN','TX','78746-7375','TRAVIS COUNTY','UNITED STATES' UNION
SELECT '800cf720-c0e8-4c37-aa68-ad52c7945e24','653','DOMINION REALTY, INC','8355 ROCKVILLE RD','INDIANAPOLIS','IN','46234-2722','MARION COUNTY','UNITED STATES' UNION
SELECT '58df7f74-3d0f-489e-8f2d-e278ab0dd098','1370','DORCHESTER MANAGEMENT LLC','6780 ROSWELL RD STE 200','ATLANTA','GA','30328-2589','FULTON COUNTY','UNITED STATES' UNION
SELECT '08546c0c-ad9c-43b3-8061-2c0e5a189a48','744','DORVIDOR','25 SE 2ND AVE STE 900','MIAMI','FL','33131-1604','MIAMI-DADE COUNTY','UNITED STATES' UNION
SELECT '113c8732-8898-4d7c-9409-3b936fdfc5a2','601','DOUGLAS ALLRED COMPANY','11452 EL CAMINO REAL STE 200','SAN DIEGO','CA','92130-2080','SAN DIEGO COUNTY','UNITED STATES' UNION
SELECT 'a620e5a1-fcec-46b2-a780-ea5c93788cae','1784','DOUGLAS EMMETT MANAGEMENT LLC','1299 OCEAN AVE STE 1000','SANTA MONICA','CA','90401-1063','LOS ANGELES COUNTY','UNITED STATES' UNION
SELECT 'b47613ed-8fc6-44d4-9eb6-56a8aee84d85','5298','DOUGLASS PROPERTIES, LLC','815 E ROSEWOOD AVE','SPOKANE','WA','99208-5507','SPOKANE COUNTY','UNITED STATES' UNION
SELECT '4f5ba502-fd8f-4598-800b-113efa903e5b','1375209','DOWNTOWN EMERGENCY SERVICE CENTER','515 3RD AVE','SEATTLE','WA','98104-2304','KING COUNTY','UNITED STATES' UNION
SELECT 'cebd1ec1-519f-4e84-857a-038a37c89e3d','6607','DRAGAS MANAGEMENT','4538 BONNEY RD STE B','VIRGINIA BCH','VA','23462-3874','VIRGINIA BEACH CITY','UNITED STATES' UNION
SELECT '80542508-f005-40ee-b066-3e1b98c7034a','5802','DRAPER AND KRAMER, INCORPORATED, AN ILLINOIS CORPORATION','55 E MONROE, SUITE 3900','CHICAGO','IL','60603-5300','COOK COUNTY','UNITED STATES' UNION
SELECT '35a8b301-d60b-44c5-91fa-890fdf7357d2','7157','DREVER MANAGEMENT COMPANY','2900 PARADISE DR','TIBURON','CA','94920-1211','MARIN COUNTY','UNITED STATES' UNION
SELECT 'ad636744-2629-4f99-99ee-f1b8861db863','817','DREYFUSS MANAGEMENT LLC','4800 MONTGOMERY LN','BETHESDA','MD','20814-3429','MONTGOMERY COUNTY','UNITED STATES' UNION
SELECT '35e714eb-7e83-41e3-94ec-ce31ca6aa9da','411','DRUCKER AND FALK LLC','11824 FISHING POINT DR','NEWPORT NEWS','VA','23606-2679','NEWPORT NEWS CITY','UNITED STATES' UNION
SELECT '8cf0b036-4bc7-402f-a982-6deaf8db816a','3690','DSTR#1, LLC DBA SPRUCE STREET REAL ESTATE COMPANY','1520 SPRUCE ST','PHILADELPHIA','PA','19102-4511','PHILADELPHIA COUNTY','UNITED STATES' UNION
SELECT '91ed77f5-0b48-43f5-a968-592d46590fa2','749','DTN MANAGEMENT COMPANY','2502 LAKE LANSING RD STE C','LANSING','MI','48912-3661','INGHAM COUNTY','UNITED STATES' UNION
SELECT 'be63d037-f06f-4b85-bcda-8f328a66d574','4388','DUNLAVY DEVELOPMENT PARTNERS','4310 DUNLAVY ST','HOUSTON','TX','77006-5221','HARRIS COUNTY','UNITED STATES' UNION
SELECT '8ff2bb71-b45d-47af-8176-a5dda502ebf8','8199','DYNAMIC DESIGN BUILD INC DBA DYNAMIC PROPERTIES','11777 SAN VICENTE BLVD STE 800','LOS ANGELES','CA','90049-5011','LOS ANGELES COUNTY','UNITED STATES' UNION
SELECT 'e8ecb170-2239-4434-bd01-c1703b179120','1999','E AND S RING MANAGEMENT','11050 SANTA MONICA BLVD 2ND FL','LOS ANGELES','CA','90025-7571','LOS ANGELES COUNTY','UNITED STATES' UNION
SELECT 'ab6439e4-5e87-46ed-8a21-2307884740b1','5216','E/L PROPERTIES, LLC DBA URBAN PHENIX','1001 S DAHLIA ST STE 101','GLENDALE','CO','80246-8204','DENVER COUNTY','UNITED STATES' UNION
SELECT 'a2dd0fd4-9ddf-4c44-b558-95485689d339','284','EAGLE MANAGEMENT RE, LLC','33601 N ROYAL OAK LN','GRAYSLAKE','IL','60030-2864','LAKE COUNTY','UNITED STATES' UNION
SELECT '2ec74695-a808-40fd-b030-f3d181d704e0','6233','EAGLE POINT MANAGEMENT LLC','125 JOHN ROBERTS RD STE 12','SOUTH PORTLAND','ME','04106-3295','CUMBERLAND COUNTY','UNITED STATES' UNION
SELECT '9ac2c252-a0bb-4f3a-acad-2b3f25c2cdde','315','','','','','','','' UNION
SELECT '82698637-f14d-4281-9201-c6373653a963','5905','','','','','','','' UNION
SELECT 'e2b59cf9-0b1b-4edb-9aef-3662c698d06e','2263','EBENEZER MANAGEMENT SERVICES INC','2722 PARK AVE','MINNEAPOLIS','MN','55407-1009','HENNEPIN COUNTY','UNITED STATES' UNION
SELECT 'bd7e4eb1-9c60-4fb9-8d42-dbf686c2dc9d','7025','ECHELON PROPERTY GROUP LLC','7600 E ORCHARD RD STE 200N','GREENWOOD VILLAGE','CO','80111-2520','ARAPAHOE COUNTY','UNITED STATES' UNION
SELECT '0f1fbabf-13bc-48cf-acb7-41621fd565ed','5918','','','','','','','' UNION
SELECT '30e46690-1fb8-4c1f-afc3-f4bded3c3e93','5911','EDGEWATER MANAGEMENT','950 PENINSULA CORPORATE CIR STE 30','BOCA RATON','FL','33487-1378','PALM BEACH COUNTY','UNITED STATES' UNION
SELECT '9356bf45-7869-4482-953f-1c27111840c6','5927','EDGEWOOD MANAGEMENT CORPORATION','9711 WASHINGTONIAN BLVD STE 200','GAITHERSBURG','MD','20878-7365','MONTGOMERY COUNTY','UNITED STATES' UNION
SELECT '0b12207f-4a61-41e2-aecc-2777f2d60cfc','6490','EDWARDS STUDENT HOUSING MANAGEMENT COMPANY','495 S HIGH ST STE 150','COLUMBUS','OH','43215-5695','FRANKLIN COUNTY','UNITED STATES' UNION
SELECT '5f6665b7-e556-48f7-ad7c-b8ab514455f4','6328','EHDOC','1580 SAWGRASS CORPORATE PKWY STE 100','SUNRISE','FL','33323-2860','BROWARD COUNTY','UNITED STATES' UNION
SELECT '4ccd9d31-4cc1-4ebd-8d6c-5c4d1aac2cd3','3531','ELEMENT NATIONAL MANAGEMENT','1515 S FEDERAL HWY STE 302','BOCA RATON','FL','33432-7451','PALM BEACH COUNTY','UNITED STATES' UNION
SELECT '56880ac5-fb18-45a3-9bbd-2ecfc01e7bf2','4438','ELEOS PROPERTY MANAGEMENT, LLC','2100 NE 140TH ST','EDMOND','OK','73013-5539','OKLAHOMA COUNTY','UNITED STATES' UNION
SELECT 'bc0a8c46-7f78-40b6-9137-8c1a60b83af5','7062','ELEVATE MULTIFAMILY LLC','73 W MONROE ST','CHICAGO','IL','60603-4910','COOK COUNTY','UNITED STATES' UNION
SELECT '528bf2c8-3995-4cc5-9ae3-79f8d2b892a0','6454','ELEVATE ROI LLC','4201 WINGREN DR STE 210','IRVING','TX','75062-2763','DALLAS COUNTY','UNITED STATES' UNION
SELECT '0b2bb0b6-bb0a-4f55-b5c6-e44225fd5d7c','6163','ELEVATION PROPERTY MANAGEMENT LLC','201 E PINE ST STE 200','ORLANDO','FL','32801-2715','ORANGE COUNTY','UNITED STATES' UNION
SELECT '6cfacd60-ec0f-44b0-8d78-064f291fd784','5631','ELITE PROPERTY MANAGEMENT, LLC','16250 NORTHLAND DR STE 301','SOUTHFIELD','MI','48075-5228','OAKLAND COUNTY','UNITED STATES' UNION
SELECT '0e33fbec-22bb-4b3f-9740-37613c311c08','25492','ELITE SIGNATURE PROPERTIES, LLC','2220 UNIVERSITY BLVD','TUSCALOOSA','AL','35401-1542','TUSCALOOSA COUNTY','UNITED STATES' UNION
SELECT '078e29a7-0f09-47d5-b6ae-9e9591139c9a','6239','ELLER RESIDENTIAL LIVING','1350 ENVIRON WAY','CHAPEL HILL','NC','27517-4430','ORANGE COUNTY','UNITED STATES' UNION
SELECT '9627cabf-8d70-4c4d-9852-19832a9fb40b','15115','ELLIOTT ENTERPRISES OF VERO BEACH, INC.','835 20TH PL','VERO BEACH','FL','32960-5357','INDIAN RIVER COUNTY','UNITED STATES' UNION
SELECT '184fc251-d559-4d1e-94b8-4ac689be1197','6670','ELLIS AND ELLIS, PLLC','500 VIRGINIA ST E STE 1000','CHARLESTON','WV','25301-2156','KANAWHA COUNTY','UNITED STATES' UNION
SELECT '6908b08e-af03-4220-af30-371c254315ad','714','ELMINGTON PROPERTY MANAGEMENT LLC','118 16TH AVE S STE 200','NASHVILLE','TN','37203-3135','DAVIDSON COUNTY','UNITED STATES' UNION
SELECT '5bed4f9b-3046-4db2-a166-451a67a7da90','5214','EMBRACE LIVING COMMUNITIES','1900 SPRING RD STE 300','OAK BROOK','IL','60523-1480','DUPAGE COUNTY','UNITED STATES' UNION
SELECT 'a693666e-23d7-4c3f-8682-808eb803ccb7','6621','EMBREY MANAGEMENT SERVICES LTD','1020 NE LOOP 410','SAN ANTONIO','TX','78209-1204','BEXAR COUNTY','UNITED STATES' UNION
SELECT '2d972f1e-c81d-4fac-b4ca-6b284a6edf54','715','EMERALD FUND, INC.','235 MONTGOMERY ST 27TH FL','SAN FRANCISCO','CA','94104-2902','SAN FRANCISCO COUNTY','UNITED STATES' UNION
SELECT '2a7b0706-b574-48ff-9caf-99d731334199','9319','EMMETT OCHS INVESTMENTS, INC','11812 SAN VICENTE BLVD STE 210','LOS ANGELES','CA','90049-6622','LOS ANGELES COUNTY','UNITED STATES' UNION
SELECT 'fe3f1d30-cc0f-4161-a868-87f259c83fb3','18145','EMPOWER CHEROKEE OF GA, INC.','133 UNIVETER RD','CANTON','GA','30114-9089','CHEROKEE COUNTY','UNITED STATES' UNION
SELECT '67fe5ca8-f05a-494f-9611-e1369dcc0014','9330','ENCHANTA','4619 DIETRICH RD','SAN ANTONIO','TX','78219-2868','BEXAR COUNTY','UNITED STATES' UNION
SELECT 'a4c07b1b-322c-4662-b270-217885fc3e9b','5936','ENFIELD MANAGEMENT COMPANY, LLC','1222 16TH AVE S STE 300','NASHVILLE','TN','37212-2926','DAVIDSON COUNTY','UNITED STATES' UNION
SELECT '1b035844-88f6-4d0e-a978-52a28ac4ad37','31754','','','','','','','' UNION
SELECT '248184ef-0ade-4c4a-ad78-325e8e8e9855','1229','ENGLERT MGMT','1426 HUNTERS POINT DR','ZIONSVILLE','IN','46077-3831','BOONE COUNTY','UNITED STATES' UNION
SELECT '644ce91f-ddfc-4b30-b441-2e429ddb0e5d','2990','ENLIVANT MASTER MGMT CO LLC','330 N WABASH AVE STE 3700','CHICAGO','IL','60611-7605','COOK COUNTY','UNITED STATES' UNION
SELECT '559f1136-44a5-4181-be38-e894b1912f32','1379857','ENON TOLAND APARTMENTS','245 W QUEEN LN','PHILADELPHIA','PA','19144-4065','PHILADELPHIA COUNTY','UNITED STATES' UNION
SELECT '424a1d04-3ea4-4563-8d03-0b897785930e','1379971','','','','','','','' UNION
SELECT 'a22abb1d-d00e-4b27-8ee6-fac2a0ad66b3','1226','ENVOLVE CLIENT SERVICES GROUP LLC','555 PERKINS EXT STE 200','MEMPHIS','TN','38117-4410','SHELBY COUNTY','UNITED STATES' UNION
SELECT '73b340aa-511e-4e82-9ed2-a4ee1b36cae8','6304','EOS REAL ESTATE MANAGEMENT GROUP','2 W WASHINGTON ST STE 401','GREENVILLE','SC','29601-2774','GREENVILLE COUNTY','UNITED STATES' UNION
SELECT '88c46a35-4d7a-415b-8f29-74e5061832e0','6733','EP MANAGEMENT, LLC','1260 STELTON RD','PISCATAWAY','NJ','08854-5282','MIDDLESEX COUNTY','UNITED STATES' UNION
SELECT 'd245030d-c079-4f34-9010-aaeeb3c55adc','986','EPARTMENTS COMMUNITIES','8001 METCALF AVE STE 300','OVERLAND PARK','KS','66204-3844','JOHNSON COUNTY','UNITED STATES' UNION
SELECT 'cbdc6283-1542-4aab-83ec-b10e8857a06f','1374527','EPO DEVELOPMENT LLC','4140 GADDI DR APT 3','SACRAMENTO','CA','95824-2743','SACRAMENTO COUNTY','UNITED STATES' UNION
SELECT '061cb251-4b74-4c36-9492-20fce4d27fc0','6216','EQUITY PROPERTY MANAGEMENT LLC','200 W 75TH PL','MERRILLVILLE','IN','46410-4702','LAKE COUNTY','UNITED STATES' UNION
SELECT '3aa3c907-317a-4493-94a6-6ab57251ad92','3550','EQUITY RESIDENTIAL MANAGEMENT LLC','2 N RIVERSIDE PLZ STE 400','CHICAGO','IL','60606-2624','COOK COUNTY','UNITED STATES' UNION
SELECT '875b8a29-7641-4528-a54e-a00ce40131b5','8763','ESKAY MANAGEMENT CORP.','11 TORI DR','LAKEWOOD','NJ','08701-2358','OCEAN COUNTY','UNITED STATES' UNION
SELECT '449d33ea-3cc9-459f-9c42-c27ce6a9e0a3','16302','','','','','','','' UNION
SELECT '65263411-e6fd-48e2-8319-67ef98eb50bd','3713','ESSEX PLAZA MANAGEMENT','1060 BROAD ST','NEWARK','NJ','07102-2321','ESSEX COUNTY','UNITED STATES' UNION
SELECT '76a26879-db74-4421-9170-32578d97c4e5','752','ESSEX PORTFOLIO, L.P.','1100 PARK PL STE 200','SAN MATEO','CA','94403-7107','SAN MATEO COUNTY','UNITED STATES' UNION
SELECT 'fb0096d8-b0cc-4f94-92f2-f79c5d09ea62','5387','EUCALYPTUS REAL ESTATE LLC','1300 SOVEREIGN ROW','OKLAHOMA CITY','OK','73108-1825','OKLAHOMA COUNTY','UNITED STATES' UNION
SELECT '930bbfe8-b423-4d72-b856-30c93b54e94e','6366','EUREKA MULTI FAMILY GROUP','1108 LAVACA ST STE 110-348','AUSTIN','TX','78701-2110','TRAVIS COUNTY','UNITED STATES' UNION
SELECT '56e11431-d1e5-4473-ae3f-ac830d49ddf0','1379972','','','','','','','' UNION
SELECT '07c15801-4dff-482c-8c69-c076f1e104fb','6421','EUSTON MGMT','910 CAMINO DEL MAR STE A','DEL MAR','CA','92014-2800','SAN DIEGO COUNTY','UNITED STATES' UNION
SELECT 'f6c9261b-4e00-450f-a2d6-5304c1dc9206','1378191','EVERCORE COMPANIES LLC','2727 KIRBY DRIVE, SUITE 15C','HOUSTON','TX','77098','','UNITED STATES' UNION
SELECT 'b40c7994-1cf7-45ab-ab6a-b6b5d354d13d','6584','EVEREST CAMPUS SERVICES COMPANY LLC','2970 CLAIRMONT RD NE STE 310','BROOKHAVEN','GA','30329-4514','DEKALB COUNTY','UNITED STATES' UNION
SELECT 'ef4d9f2a-faf6-4a5a-8274-085b59709da9','1367716','EVEREST EQUITIES GROUP LLC','1999 FLATBUSH AVE','BROOKLYN','NY','11234-3524','KINGS COUNTY','UNITED STATES' UNION
SELECT 'b7425f83-238c-4c7d-9d50-4b51a1265509','6225','EVERGREEN REAL ESTATE SERVICES LLC','566 W LAKE ST STE 400','CHICAGO','IL','60661-1410','COOK COUNTY','UNITED STATES' UNION
SELECT '237e780d-6f56-48d9-a188-676eab602c01','3739','EVOLUTION MULTIFAMILY LLC','176 CHURCH ST STE 1','LOWELL','MA','01852-2685','MIDDLESEX COUNTY','UNITED STATES' UNION
SELECT '947d3222-ae36-4ea8-87c7-2df6dbb840b0','1374511','FAIR SHARE HOUSING DEVELOPMENT, INC.','1 ETHEL R LAWRENCE BLVD','MOUNT LAUREL','NJ','08054-6999','BURLINGTON COUNTY','UNITED STATES' UNION
SELECT 'a4e7ceff-e94b-4a39-8b6f-f0d4d2b8bd18','1377835','FAIRFIELD PROPERTIES','538 BROADHOLLOW RD 3RD FL','MELVILLE','NY','11747-3676','SUFFOLK COUNTY','UNITED STATES' UNION
SELECT '1843a941-fb3a-4770-bf53-db37e4ed052c','860','FAIRMONT MANAGEMENT INC.','2421 FOOTHILL BLVD APT 12G','LA VERNE','CA','91750-3032','LOS ANGELES COUNTY','UNITED STATES' UNION
SELECT 'd04405c5-f831-4c79-bf94-2750a4ab7d01','9311','FAIRVIEW VENTURES','257 W BIG SPRINGS RD','RIVERSIDE','CA','92507-4776','RIVERSIDE COUNTY','UNITED STATES' UNION
SELECT 'cd5d8921-a41c-4580-b393-cc35eb5a35d6','6804','FAIRVILLE MANAGEMENT COMPANY, LLC','726 YORKLYN RD STE 200','HOCKESSIN','DE','19707-8701','NEW CASTLE COUNTY','UNITED STATES' UNION
SELECT '4dfe2898-7b11-44f8-b800-b9da4c0c3730','6226','FAIRWAY MANAGEMENT, INC','206 PEACH WAY','COLUMBIA','MO','65203-4905','BOONE COUNTY','UNITED STATES' UNION
SELECT '52da457d-e545-45d5-b869-f855615ed1d9','5305','FAITH ASSET MANAGEMENT, LLC','2080 SILAS DEANE HWY STE 102B','ROCKY HILL','CT','06067-2334','HARTFORD COUNTY','UNITED STATES' UNION
SELECT 'f53e23c8-9f01-4eda-847a-7023bf2b8e12','1379973','','','','','','','' UNION
SELECT '3b594f17-7819-4a93-841d-e40ccdc2b38d','6973','FALKIN PLATNICK REALTY GROUP','70 JACKSON DR','CRANFORD','NJ','07016-3510','UNION COUNTY','UNITED STATES' UNION
SELECT '70efb007-4e35-4f62-9969-4365e7d772be','866','FALLS MANAGEMENT COMPANY','13651 NW 4TH ST','PEMBROKE PINES','FL','33028-2224','BROWARD COUNTY','UNITED STATES' UNION
SELECT '06a174ae-22a8-4eb0-b49a-6db493db7936','27495','FAME HOUSING CORPORATION','1968 W ADAMS BLVD','LOS ANGELES','CA','90018-3515','LOS ANGELES COUNTY','UNITED STATES' UNION
SELECT '5ebc90e1-f306-4b8f-91bd-590f0de9fcda','4839','FAMILY HOUSING MANAGEMENT COMPANY, INC','134 E CHURCH ST','JACKSONVILLE','FL','32202-3130','DUVAL COUNTY','UNITED STATES' UNION
SELECT '9105e594-d034-41e7-9d34-cd6debc3ceb3','1035','FATH PROPERTIES','10602 STONE CANYON RD','DALLAS','TX','75230-4481','DALLAS COUNTY','UNITED STATES' UNION
SELECT '242bf380-ff5a-4f3c-88bd-03279072de36','6966','FCA MANAGEMENT, LLC','300 S TRYON ST # 420','CHARLOTTE','NC','28202-1914','MECKLENBURG COUNTY','UNITED STATES' UNION
SELECT '5e5ffd8a-9b36-4e53-880d-d887cb61d9d0','5322','FCI RESIDENTIAL CORPORATION','2199 PONCE DE LEON BLVD STE 201','CORAL GABLES','FL','33134-5233','MIAMI-DADE COUNTY','UNITED STATES' UNION
SELECT 'e71829be-6704-4d19-bb4c-45f49c96c364','5527','FDI MANAGEMENT','26303 OAK RIDGE DR','SPRING','TX','77380-1962','MONTGOMERY COUNTY','UNITED STATES' UNION
SELECT 'c64190e7-0aa5-4376-a833-c1cf190433dc','877','FELSON COMPANIES INC','1290 B ST STE 212','HAYWARD','CA','94541-2996','ALAMEDA COUNTY','UNITED STATES' UNION
SELECT 'f1e06495-0d58-4803-a9a7-3f49be28e143','6641','FEMCO INC DBA THE FINGER COMPANIES','99 DETERING ST STE 200','HOUSTON','TX','77007-8259','HARRIS COUNTY','UNITED STATES' UNION
SELECT '5c81ba4e-6832-4849-bd27-10bba59798bf','882','FEREBEE PROPERTIES OPERATING COMPANY LLC','701 TREYBROOKE CIR','GREENVILLE','NC','27834-7845','PITT COUNTY','UNITED STATES' UNION
SELECT 'c2a85ac7-72ae-4ae0-93be-8af384612090','6549','FF PROPERTIES L.P.','5355 MIRA SORRENTO PL STE 100','SAN DIEGO','CA','92121-3812','SAN DIEGO COUNTY','UNITED STATES' UNION
SELECT 'e02583dc-5e9a-4491-9513-567ab620e59a','3570','FIELDSTONE PROPERTIES I, LLC','1719 STATE RT 10 STE 220','PARSIPPANY','NJ','07054-4537','MORRIS COUNTY','UNITED STATES' UNION
SELECT '4630eb0a-8286-44dd-81ef-fc32425b8cbc','9195','','','','','','','' UNION
SELECT 'b2391ee3-3a32-4723-a3df-01680f7f5029','15030','FINE ASSOCIATES LLC','80 S 8TH ST STE 4916','MINNEAPOLIS','MN','55402-2226','HENNEPIN COUNTY','UNITED STATES' UNION
SELECT 'e14e1a54-75fc-49be-97c3-68cf2d8f8b64','6828','FIRST CHOICE MGMT GRP','11124 WURZBACH RD STE 304','SAN ANTONIO','TX','78230-2445','BEXAR COUNTY','UNITED STATES' UNION
SELECT '7f278869-020f-4c8b-b76a-696f26bebe33','288','FIRST CHOICE PROPERTY SOLUTIONS LLC','4001 PELHAM RD','GREER','SC','29650-4300','GREENVILLE COUNTY','UNITED STATES' UNION
SELECT '6a871b61-bd2a-442f-9f8f-240a7ee8995b','1182','FIRST MONTGOMERY MANAGEMENT GROUP','222 HADDON AVE STE 100','HADDON TOWNSHIP','NJ','08108-2827','CAMDEN COUNTY','UNITED STATES' UNION
SELECT '5a909cef-c739-409e-b66d-8cff02e0b169','784','FIRST NJ ASSET MANAGEMENT','1608 ROUTE 88 STE 200','BRICK','NJ','08724-3009','OCEAN COUNTY','UNITED STATES' UNION
SELECT 'cd868fdc-40bf-4aec-bbb4-5ac1ac76196d','1369','FIRST RANGE MANAGEMENT CO.','6255 HABITAT DR APT 3005','BOULDER','CO','80301-3219','BOULDER COUNTY','UNITED STATES' UNION
SELECT '3b0f33d4-aa5a-47ef-a035-98ee9b735c78','1408','FIRST REALTY MGMT CORP','151 TREMONT ST','BOSTON','MA','02111-1125','SUFFOLK COUNTY','UNITED STATES' UNION
SELECT '11688b87-783c-4b34-9a3a-36333a1445fc','5568','FIRST SERVICE RESIDENTIAL NEW YORK, INC','622 3RD AVE','NEW YORK','NY','10017-6707','NEW YORK COUNTY','UNITED STATES' UNION
SELECT '8e1a9c49-237b-4b2d-908c-f53567716d60','21042','','','','','','','' UNION
SELECT '5a7fa4c2-25ca-439f-83e5-2fb148d2bc2d','22993','','','','','','','' UNION
SELECT '4fe0c29d-caa7-49e2-b4e1-710ea9b3eed3','6236','FLAHERTY AND COLLINS INC','1 INDIANA SQ STE 3000','INDIANAPOLIS','IN','46204-2063','MARION COUNTY','UNITED STATES' UNION
SELECT '17fe9eeb-ab15-4274-8d94-a58dded2c42d','1380124','FLAT CREEK PROPERTY MANAGEMENT LLC','4332 TERAVISTA CLUB DR UNIT 13','ROUND ROCK','TX','78665-1377','WILLIAMSON COUNTY','UNITED STATES' UNION
SELECT '1c314f17-f3db-400e-989e-9bf51e561729','289','','','','','','',''  UNION
SELECT '2f0a541c-c54d-496e-bf40-8da3a6c2b372','6110','FLOURNOY PROPERTIES LLC','900 BROOKSTONE CENTRE PKWY','COLUMBUS','GA','31904-2987','MUSCOGEE COUNTY','UNITED STATES' UNION
SELECT 'c09e95dc-f24f-4ddf-9a6b-fa42b2ebf496','926','FM SHORT MANAGEMENT','201 TEXAS CENTRAL PKWY','WACO','TX','76712-6503','MCLENNAN COUNTY','UNITED STATES' UNION
SELECT 'c0110bd4-2f81-4a27-84a0-e84bc04d24e5','1375286','FOCUS VENTURES LLC','1517 GREENWOOD ALY','BOWLING GREEN','KY','42101-2936','WARREN COUNTY','UNITED STATES' UNION
SELECT '59be9e30-2755-4c81-8ee6-d56553afe9dc','6231','FOGELMAN MANAGEMENT','6060 POPLAR AVE STE 200','MEMPHIS','TN','38119-3959','SHELBY COUNTY','UNITED STATES' UNION
SELECT '99cd8809-bd9a-4c0c-a997-066724faa0dc','3105','FORD HEIGHTS COOPERATIVE','1105 DREXEL AVE','FORD HEIGHTS','IL','60411-2212','COOK COUNTY','UNITED STATES' UNION
SELECT '1d9d6af3-055b-4fdc-ab94-62897a3d2100','9602','FORESIGHT ASSET MANAGEMENT LLC','7334 BLANCO RD STE 200','SAN ANTONIO','TX','78216-4933','BEXAR COUNTY','UNITED STATES' UNION
SELECT '8485b3ae-dadf-428d-accd-653dc355be3d','6303','FOREST PROPERTIES MANAGEMENT, INC','625 MOUNT AUBURN ST STE 210','CAMBRIDGE','MA','02138-4555','MIDDLESEX COUNTY','UNITED STATES' UNION
SELECT 'c3101e04-9b3c-4740-a3aa-cd107a7d136a','279','FORT DEFIANCE HOUSING CORP DBA. SANDSTONE HOUSING','8500 MENAUL BLVD NE STE A420','ALBUQUERQUE','NM','87112-2250','BERNALILLO COUNTY','UNITED STATES' UNION
SELECT 'e9374487-28af-4792-a632-7c091d624247','5087','','','','','','','' UNION
SELECT 'cff51bd2-429f-462d-9429-bf18115cec93','6508','FORTY-TWO, LLC','6 E GERMANTOWN PIKE','PLYMOUTH MEETING','PA','19462-1531','MONTGOMERY COUNTY','UNITED STATES' UNION
SELECT 'e445c60e-a40e-46e6-9fb8-e6cdf153329c','6213','','','','','','','' UNION
SELECT 'e40754f1-d6df-4481-bcde-5a44e991c139','6208','FOSHEE MANAGEMENT COMPANY, LLC','44 MARKET PLZ STE 801','MONTGOMERY','AL','36104-3646','MONTGOMERY COUNTY','UNITED STATES' UNION
SELECT 'd624698c-2c6b-4d02-8e53-d30751e9df70','6605','FOUNDATION COMMUNITIES','3000 S INTERSTATE 35','AUSTIN','TX','78704-6513','TRAVIS COUNTY','UNITED STATES' UNION
SELECT '21061f0b-3e8d-48b1-849d-9df33dae211b','6249','FOUNDATION PROPERTY MANAGEMENT, INC.','911 N STUDEBAKER RD','LONG BEACH','CA','90815-4900','LOS ANGELES COUNTY','UNITED STATES' UNION
SELECT '6a367d2a-5a3f-4d48-9823-bf69098595fd','6224','FPI MANAGEMENT','800 IRON POINT RD','FOLSOM','CA','95630-9004','SACRAMENTO COUNTY','UNITED STATES' UNION
SELECT 'bdd8a03c-40cc-4169-8ca8-51191339d805','1379974','','','','','','','' UNION
SELECT '643be92d-951a-4fd0-9b9f-fdde76673de7','20971','FRANKLIN APARTMENT MANAGEMENT, LTD. ','21260 GATHERING OAK STE 101','SAN ANTONIO','TX','78260-3387','BEXAR COUNTY','UNITED STATES' UNION
SELECT '9f4a923b-e5a0-486d-895a-278cc42aa93c','1380369','FRANKLIN ASSET MANAGEMENT INC','2509 PLANTSIDE DR','LOUISVILLE','KY','40299-2529','JEFFERSON COUNTY','UNITED STATES' UNION
SELECT '3b813463-b5fa-49e2-a023-ac9e6795b91f','9350','FRANKLIN HOUSING COLLABORATIVE ','200 SPRING ST','FRANKLIN','TN','37064-3337','WILLIAMSON COUNTY','UNITED STATES' UNION
SELECT '0a90c91d-6131-4315-9e61-67a8bc48821f','514','FRANKLIN STREET RESIDENTIAL SERVICES','600 N WEST SHORE BLVD STE 600','TAMPA','FL','33609-1137','HILLSBOROUGH COUNTY','UNITED STATES' UNION
SELECT '285699a6-0205-42f5-8be8-97d8c45f4fb1','6389','FREEMAN WEBB COMPANY, REALTORS','3810 BEDFORD AVE STE 300','NASHVILLE','TN','37215-2515','DAVIDSON COUNTY','UNITED STATES' UNION
SELECT 'b56b62ba-840a-40e9-b294-9314b13ef273','6286','FRESH PROPERTIES','4300 CHURCH ROAD','SAUGET','IL','62207','','UNITED STATES' UNION
SELECT '807c1f0e-634b-4fb6-8af0-c38fdaef6726','6426','FRONTIER REALTY','25700 SCIENCE PARK DR STE 360','BEACHWOOD','OH','44122-7326','CUYAHOGA COUNTY','UNITED STATES' UNION
SELECT 'be800262-7121-43cb-af90-a80c18ec90bd','6590','FRYE PROPERTIES','300 W FREEMASON ST','NORFOLK','VA','23510-1208','NORFOLK CITY','UNITED STATES' UNION
SELECT '07c0f560-279c-4465-842c-a3eb690a85d9','3575','FSL MANAGEMENT','1201 E THOMAS RD','PHOENIX','AZ','85014-5734','MARICOPA COUNTY','UNITED STATES' UNION
SELECT '49ad92d6-c127-4bcd-b905-632be62557e7','5658','FULL LIFE MANAGEMENT','2186 JACKSON KELLER RD','SAN ANTONIO','TX','78213-2723','BEXAR COUNTY','UNITED STATES' UNION
SELECT '4f2f70d3-03a5-4f8c-a8f5-61100524750f','5235','','','','','','','' UNION
SELECT '0d879860-ac9c-40da-9690-c2f421cc75c2','8200','FUSION PROPERTY MANAGEMENT CO., LLC','4425 ATLANTIC AVE STE B20','LONG BEACH','CA','90807-2277','LOS ANGELES COUNTY','UNITED STATES' UNION
SELECT 'af6cdddd-8675-4f60-a57d-7e7dd9807c52','6271','GABLES RESIDENTIAL SERVICES, INC.','3399 PEACHTREE RD NE STE 600','ATLANTA','GA','30326-2832','FULTON COUNTY','UNITED STATES' UNION
SELECT '3a80da35-4298-4e7d-a1f7-03546d8bbcda','1367439','GALLAHER HOMES MANAGEMENT GROUP LLC','9240 OLD REDWOOD HWY UNIT 200','WINDSOR','CA','95492-9349','SONOMA COUNTY','UNITED STATES' UNION
SELECT 'eb401648-e8f0-4780-aabb-26f28a5473e7','5953','GARDNER MANAGEMENT COMPANY','5770 VENTURE PARK DR','KALAMAZOO','MI','49009-1846','KALAMAZOO COUNTY','UNITED STATES' UNION
SELECT '11a60525-d1d4-45b3-8a71-d780d2bcb189','1380019','','','','','','','' UNION
SELECT '711b1c6b-9b4f-44f7-95d8-434103c684ef','73','GASPAR PROPERTIES INC','50 DAVIS BLVD #1','TAMPA','FL','33606-3485','HILLSBOROUGH COUNTY','UNITED STATES' UNION
SELECT '2260af22-3736-4b3b-b7f2-81390d3f0f1f','911','','','','','','','' UNION
SELECT '72c5d4c6-87ff-4010-9a9a-afc457faffbd','5700','GATEWAY MANAGEMENT COMPANY LLC DBA THE GATEWAY COMPANIES INC','22 INVERNESS CENTER PKWY # 222','BIRMINGHAM','AL','35242-4814','SHELBY COUNTY','UNITED STATES' UNION
SELECT '8e383141-9f85-4d7a-905c-6c9a1f31db7b','754','GATEWAY MANAGEMENT SERVICES LLC','350C FORTUNE TER STE 202','POTOMAC','MD','20854-2981','MONTGOMERY COUNTY','UNITED STATES' UNION
SELECT 'dc30a512-7bcb-40b0-89d0-95d10189d04f','8448','','','','','','','' UNION
SELECT 'd7e942ef-62bf-454c-9688-b1d9c94d0edf','5324','GDC PROPERTIES, LLC','245 SAW MILL RIVER RD','HAWTHORNE','NY','10532-1526','WESTCHESTER COUNTY','UNITED STATES' UNION
SELECT 'c03aca21-f612-426f-b9b3-47f85601e787','1786','GELLER ASSOCIATES, INC.','101 EISENHOWER PKWY STE 304','ROSELAND','NJ','07068-1054','ESSEX COUNTY','UNITED STATES' UNION
SELECT '39790de1-d087-4220-94b5-193cde587df5','1379894','GENESIS IPV PROPERTIES LLC','3936 POMODORO CIR','CAPE CORAL','FL','33909-5128','LEE COUNTY','UNITED STATES' UNION
SELECT '2c3aa951-ad71-43c4-9bff-81e2bc89beea','22082','GENEXUS LIVING','4906 OTTER RUN CT NE','SALEM','OR','97305-2800','MARION COUNTY','UNITED STATES' UNION
SELECT '3408777f-4a51-4f2d-b697-871816c1f08a','1379646','GENTRY MANOR, LLC','382 PENNSYLVANIA AVE','GLEN ELLYN','IL','60137-4350','DUPAGE COUNTY','UNITED STATES' UNION
SELECT 'eec000f6-e955-43ef-9b3a-5d470a2becc5','1379975','','','','','','','' UNION
SELECT '35ff0540-1764-4451-ab8f-fbeef81e76d2','3544','GEORGIA INFIRMARY NPHC','1900 LINCOLN ST','SAVANNAH','GA','31401-8144','CHATHAM COUNTY','UNITED STATES' UNION
SELECT 'cf5d404c-5be2-4c59-92b2-0acae673a1e6','1380457','GEORGICA PROPERTY MANAGEMENT LLC','50 JERICHO QUADRANGLE STE 118','JERICHO','NY','11753-2726','NASSAU COUNTY','UNITED STATES' UNION
SELECT '2c1178c1-083e-4ef9-8e3e-95df4f1d851d','291','GF PROPERTIES GROUP LLC','320 S TELLER ST STE 220','LAKEWOOD','CO','80226-7392','JEFFERSON COUNTY','UNITED STATES' UNION
SELECT 'e79b0ead-8645-4429-b1ca-29955fa6cd53','3323','GHERTNER AND COMPANY','50 VANTAGE WAY STE 100','NASHVILLE','TN','37228-1553','DAVIDSON COUNTY','UNITED STATES' UNION
SELECT '82cf15a4-9e7a-461f-bbdf-420b6ddce3f4','793','GHP MANAGEMENT CORPORATION','1082 W 7TH ST','LOS ANGELES','CA','90017-2504','LOS ANGELES COUNTY','UNITED STATES' UNION
SELECT 'b8fe608c-c24d-46eb-b748-1b386d9f9cb1','5310','GIBRALTAR PROPERTY MANAGEMENT','6201 W 26TH AVE','WHEAT RIDGE','CO','80214-8239','JEFFERSON COUNTY','UNITED STATES' UNION
SELECT '0747fcd9-efa6-40ed-9234-cdca716129d6','916','GILBERT CAMPBELL REAL ESTATE','176 CHURCH ST','LOWELL','MA','01852-2685','MIDDLESEX COUNTY','UNITED STATES' UNION
SELECT 'd913f75d-ecf6-4c09-9f3f-7783f23ffc74','8726','GILLESPIE COMPANY, LLC','329 S WASHINGTON SQ STE I','LANSING','MI','48933-2114','INGHAM COUNTY','UNITED STATES' UNION
SELECT '639a387f-0b8b-41e8-b9da-e020c4e0b4c0','7300','GINKGO RESIDENTIAL LLC','200 S COLLEGE ST STE 200','CHARLOTTE','NC','28202-2007','MECKLENBURG COUNTY','UNITED STATES' UNION
SELECT '1c7da0f2-5d2b-4b3d-881b-5e2649df33ad','19814','GINOSKO DEVELOPMENT COMPANY','41800 W 11 MILE RD STE 209','NOVI','MI','48375-1818','OAKLAND COUNTY','UNITED STATES' UNION
SELECT '5c06c1d1-5ca0-4bfc-8919-1b3c940c35d3','811','GK MANAGEMENT CO. INC.','5150 OVERLAND AVE','CULVER CITY','CA','90230-4914','LOS ANGELES COUNTY','UNITED STATES' UNION
SELECT 'f062a911-7e5f-455a-86b9-9442d9b96b46','5326','GL HOUSING GROUP','1020 HURON RD E STE 100','CLEVELAND','OH','44115-1700','CUYAHOGA COUNTY','UNITED STATES' UNION
SELECT '139d4391-06ce-45a5-8bbf-9142ed977996','14739','GLENN HOWARD MANOR','12007 N LAMAR BLVD','AUSTIN','TX','78753-1700','TRAVIS COUNTY','UNITED STATES' UNION
SELECT '08324f0a-52b6-4e94-bff2-b06775501d5d','5236','GLOBAL REALTY CORPORATION','1875 CENTURY PARK E STE 1800','LOS ANGELES','CA','90067-2519','LOS ANGELES COUNTY','UNITED STATES' UNION
SELECT 'ec4cbb4d-a421-4e60-8bbe-f245efff2f9f','15086','','','','','','','' UNION
SELECT 'd70be152-8127-4e43-90b3-fb91b2a0dcf1','5645','GOLD CROWN MANAGEMENT','7400 E CRESTLINE CIR STE 200','GREENWOOD VILLAGE','CO','80111-3653','ARAPAHOE COUNTY','UNITED STATES' UNION
SELECT '89b64651-faab-4696-9972-498de9953cf5','4731','GOLD CROWN PROPERTIES, INC','7400 W 110TH ST STE 550','OVERLAND PARK','KS','66210-2371','JOHNSON COUNTY','UNITED STATES' UNION
SELECT '60769598-1820-4c28-91aa-671b220dc763','2053','GOLD STANDARD ASSET MANAGEMENT','5155 W ROSECRANS AVE STE 238','HAWTHORNE','CA','90250-6650','LOS ANGELES COUNTY','UNITED STATES' UNION
SELECT '4b91969c-21cd-4b95-a891-484c57a13913','5311','GOOD HARBOR MANAGEMENT LLC','1225 17TH ST STE 1400','DENVER','CO','80202-5506','DENVER COUNTY','UNITED STATES' UNION
SELECT '48ea0fb0-eb3e-4722-ab25-137f048274ee','4627','GOOD SAMARITAN HOSPITAL OF MD, INC','1651 E BELVEDERE AVE','BALTIMORE','MD','21239-3003','BALTIMORE CITY','UNITED STATES' UNION
SELECT 'f595f028-c733-4d21-8cee-45153ff35dbc','14843','GOODMAN REAL ESTATE, INC.','2801 ALASKAN WAY STE 310','SEATTLE','WA','98121-1135','KING COUNTY','UNITED STATES' UNION
SELECT 'fef6d39b-76e1-431f-aef7-112442caa3e1','1378000','GOODWILL INDUSTRIES OF ACADIANA, INC.','2435 W CONGRESS ST','LAFAYETTE','LA','70506-5548','LAFAYETTE PARISH','UNITED STATES' UNION
SELECT '3355dd72-2761-4029-86b1-9543804ef716','6189','GORMAN AND COMPANY INC.','200 N MAIN ST','OREGON','WI','53575-1447','DANE COUNTY','UNITED STATES' UNION
SELECT '440b55e9-28b0-46b3-a526-534bb3d125fd','5392','GORMAN MANAGEMENT','398060 W 2200 RD','BARTLESVILLE','OK','74006-0265','WASHINGTON COUNTY','UNITED STATES' UNION
SELECT '2423da31-9a23-47f8-bce4-d8d17f05e103','1367795','','','','','','','' UNION
SELECT '840a2e48-e76c-496e-af6b-a65f661222cc','7127','GRADY MGMT INC','8630 FENTON ST','SILVER SPRING','MD','20910-3806','MONTGOMERY COUNTY','UNITED STATES' UNION
SELECT 'bb6a40c8-7671-4917-9325-9fcfba382837','6039','GRAND ATLAS PROPERTY MANAGEMENT LLC (014)','1080 PITTSFORD VICTOR RD 1ST FL','PITTSFORD','NY','14534-3805','MONROE COUNTY','UNITED STATES' UNION
SELECT 'bb346f9e-75d4-4d29-9756-b642cc8cac59','2202','GRAND AVENUE ECONOMIC COMMUNITY DEVELOPMENT CORP','3200 W COLONIAL DR','ORLANDO','FL','32808-8023','ORANGE COUNTY','UNITED STATES' UNION
SELECT 'bfdae489-c2c1-449f-a087-5c0332880947','1331','GRAND CAMPUS LIVING INC','1301 S CAPITAL OF TEXAS HWY STE B201','AUSTIN','TX','78746-6502','','UNITED STATES' UNION
SELECT '6d3b915c-6d93-4279-85d5-18a0e33459be','6284','GRAND PEAKS PROPERTY MANAGEMENT','4582 S ULSTER ST STE 1200','DENVER','CO','80237-2632','DENVER COUNTY','UNITED STATES' UNION
SELECT 'e5504a52-9a7a-4cf7-b89b-432b006fed70','1247','GRANGER HANSELL & COMPANY, LLC.','12600 DEERFIELD PKWY STE 100','ALPHARETTA','GA','30004-6130','FULTON COUNTY','UNITED STATES' UNION
SELECT 'a428a52b-c08a-4fba-8089-e739377e2c10','8738','GRANITE MANAGEMENT GROUP INC','133 W DE LA GUERRA ST','SANTA BARBARA','CA','93101-3225','SANTA BARBARA COUNTY','UNITED STATES' UNION
SELECT 'd6015d5e-4555-4ac5-be81-943457fc1bb6','3598','GRAVES BROTHERS COMPANY','2770 INDIAN RIVER BLVD STE 201','VERO BEACH','FL','32960-4230','INDIAN RIVER COUNTY','UNITED STATES' UNION
SELECT 'cc90f3d7-f813-4457-9dd4-417a3dda2365','6403','GRAYCO PARTNERS MANAGEMENT, LLC','55 WAUGH DR STE 500','HOUSTON','TX','77007-5840','HARRIS COUNTY','UNITED STATES' UNION
SELECT '632b6c5f-5d71-46cc-a01d-2d5affb7e413','561','GREAT LAKES MANAGEMENT COMPANY (CORPORATE)','12755 HIGHWAY 55 STE 125','PLYMOUTH','MN','55441-3837','HENNEPIN COUNTY','UNITED STATES' UNION
SELECT '33e5a201-0c14-44ea-a1d0-2e35a5e4e29b','2191','GREAT SOUTHERN INVEST & ASSET MGMT','3600 NW 43RD ST STE E4','GAINESVILLE','FL','32606-8134','ALACHUA COUNTY','UNITED STATES' UNION
SELECT '5755be2a-9b30-4f35-ba2d-311f144e251d','1374111','GREATER SOUTHWEST DEVELOPMENT CORPORATION','2601 W 63RD ST','CHICAGO','IL','60629-1619','COOK COUNTY','UNITED STATES' UNION
SELECT 'fc90d734-c728-42a5-a5ac-f15d0000535b','1380326','GREEN ACRES APARTMENTS, L.L.C.','415 W SANILAC RD','SANDUSKY','MI','48471-1072','SANILAC COUNTY','UNITED STATES' UNION
SELECT '6146d969-5853-41d7-948f-b2012907776d','5420','GREEN LEAF PARTNERS MANAGEMENT, INC','127 SPRING ST STE 200','PLEASANTON','CA','94566-6623','ALAMEDA COUNTY','UNITED STATES' UNION
SELECT '02619905-a87b-4483-af50-e64aff3f968d','14704','GREENLAND PROPERTY SERVICES LLC DBA GREEN NATIONAL','913 OLD LIVERPOOL RD STE H','LIVERPOOL','NY','13088-5571','ONONDAGA COUNTY','UNITED STATES' UNION
SELECT '4b7223f0-ee0e-49d3-9a15-a08b2ad39e52','7408','GREENWICH PREMIER SERVICES COMPANY','2 DEARFIELD DR','GREENWICH','CT','06831-5300','FAIRFIELD COUNTY','UNITED STATES' UNION
SELECT '04c4e78b-0968-454e-971a-e99c3fe1893f','7411','GREER MANAGEMENT, LLC','20 MARKET SQ','ROGERSVILLE','AL','35652-8008','LAUDERDALE COUNTY','UNITED STATES' UNION
SELECT 'd4d0ba11-534c-4a06-b563-4263c7be04c5','8419','','','','','','','' UNION
SELECT '59b56f19-fd91-486d-ac2a-64176a2a6c91','6177','GREYSTAR MANAGEMENT SERVICES LP','600 LAS COLINAS BLVD E STE 2100','IRVING','TX','75039-5628','DALLAS COUNTY','UNITED STATES' UNION
SELECT 'cc6237f7-5960-4050-9d61-bc5a7a509eb5','6056','GREYSTONE ASSET MANAGEMENT','950 CORBINDALE RD STE 300','HOUSTON','TX','77024-2849','HARRIS COUNTY','UNITED STATES' UNION
SELECT '99ad49fc-089c-47db-b975-0379529817e4','770','GREYSTONE CAPITAL LLC','35 KENT AVE','PITTSFIELD','MA','01201-4001','BERKSHIRE COUNTY','UNITED STATES' UNION
SELECT '9670d26b-8e49-4f44-98b8-7e2a8ca8d8ab','5279','GREYSTONE PROPERTIES, LLC','7206 SCHOMBURG RD','COLUMBUS','GA','31909','','UNITED STATES' UNION
SELECT '44a36d90-e6f1-4d94-b84b-c1ca00c8bce4','1379749','GRIFFIN HOUSING AUTHORITY','327 S 9TH ST','GRIFFIN','GA','30224-4111','SPALDING COUNTY','UNITED STATES' UNION
SELECT '2af990a4-8ba6-40d8-a0dc-7fb704e7f584','1770','GRIFFIS BLESSING, INC','102 N CASCADE AVE','COLORADO SPRINGS','CO','80903-1409','EL PASO COUNTY','UNITED STATES' UNION
SELECT '67685c20-5a24-4be3-8793-2b637a2e4ddf','6004','GRIFFIS GROUP RESIDENTIAL LLC','6400 S FIDDLERS GREEN CIR','GREENWOOD VILLAGE','CO','80111-4950','ARAPAHOE COUNTY','UNITED STATES' UNION
SELECT 'b5b233a4-20ce-4d7d-9c77-771109f0b3ee','5376','GRINDSTAFF PROPERTY MANAGEMENT, LLC','133 BIRCHWOOD LN','CROSSVILLE','TN','38555-4189','CUMBERLAND COUNTY','UNITED STATES' UNION
SELECT 'eba3c64f-02cc-42b9-b6c9-e025802fa867','1074','GROSSE & QUADE MANAGEMENT CO','762 E MAIN ST 2ND FL','LANSDALE','PA','19446-3004','MONTGOMERY COUNTY','UNITED STATES' UNION
SELECT 'ebae2075-eb81-464d-b3c1-9a0d3858b163','1179','GRUBB PROPERTIES','2626 GLENWOOD AVE STE 450','RALEIGH','NC','27608-1043','WAKE COUNTY','UNITED STATES' UNION
SELECT '82da7910-5381-45ff-ab9e-8c00b62c3d95','9206','','','','','','','' UNION
SELECT '66b4e6c1-7d7e-4709-a46d-eb56fb14764c','6774','','','','','','','' UNION
SELECT 'a3db79c4-a863-4b0e-ba38-26fa6bf140dc','6306','GULF COAST HOUSING PARTNERSHIP, INC.','1626 ORETHA CASTLE HALEY BLVD STE A','NEW ORLEANS','LA','70113-1311','ORLEANS PARISH','UNITED STATES' UNION
SELECT 'ef0894ac-1d30-4970-8b15-9afd9cc769d9','1171','GUNDLE HOLDINGS INC D/B/A PROTEA REAL ESTATE','6210 CAMPBELL RD STE 140','DALLAS','TX','75248-1380','DALLAS COUNTY','UNITED STATES' UNION
SELECT '39deaefa-2e94-4540-9ae5-c0ba121951fb','3635','GUPTA PARTNERS LLC','13331 REECK RD','SOUTHGATE','MI','48195','WAYNE COUNTY','UNITED STATES' UNION
SELECT 'd5ca808b-f183-49c1-b368-1c5245aff488','2800','GWR MANAGEMENT, LLC','2000 WEST LOOP S STE 1050','HOUSTON','TX','77027-3516','HARRIS COUNTY','UNITED STATES' UNION
SELECT '9bb023b4-3321-4a11-88ce-53cf344a5890','1379976','','','','','','','' UNION
SELECT '82c7c7ea-47c5-47d1-964e-1e62919c776f','1378602','H. O. P. E. THROUGH DIVINE INTERVENTION INC','385 HOLLY ST NW','ATLANTA','GA','30318-8194','FULTON COUNTY','UNITED STATES' UNION
SELECT '8d125e0c-261c-4475-bfd2-8f015084132b','546','','','','','','','' UNION
SELECT '482e9712-c8fb-4e41-ac66-2d0111d1e8e8','941','HAGAN PROPERTIES INC','12911 REAMERS RD','LOUISVILLE','KY','40245-2741','JEFFERSON COUNTY','UNITED STATES' UNION
SELECT '1a7eb41d-5277-4c8a-ba80-17c4ef92069d','5656','HALE MAHAOLU','200 HINA AVE','KAHULUI','HI','96732-1821','MAUI COUNTY','UNITED STATES' UNION
SELECT 'cae86522-6fde-4e62-aa9f-c172d51d7c5b','5799','HALLKEEN MANAGEMENT','1400 BOSTON PROVIDENCE TPKE STE 1000','NORWOOD','MA','02062-5032','NORFOLK COUNTY','UNITED STATES' UNION
SELECT 'f43ea18a-f939-4378-be8f-bb1482e6d162','5723','HALLMARK MGMT','3111 PACES MILL RD SE # A-250','ATLANTA','GA','30339-5704','COBB COUNTY','UNITED STATES' UNION
SELECT 'cb8803df-a10c-480d-b1e7-44161d982055','9158','HAMID H AHWAZI','8600 RESEARCH BLVD APT 130','AUSTIN','TX','78758-7165','TRAVIS COUNTY','UNITED STATES' UNION
SELECT 'b0ebd891-cf92-4a23-b248-36963ec4e9b7','1374668','HAMILTON BROTHERS INVESTMENTS L.L.C.','3556 S CULPEPPER CIR STE 4','SPRINGFIELD','MO','65804-4252','GREENE COUNTY','UNITED STATES' UNION
SELECT 'afbfaba7-8df3-4393-9a63-e1299d22b9e2','1379977','','','','','','','' UNION
SELECT '4bf2d3b8-33b6-498a-bc60-c24c25515f2b','20685','HAMPSHIRE ASSETS LLC','2329 NOSTRAND AVE STE 500','BROOKLYN','NY','11210-3948','KINGS COUNTY','UNITED STATES' UNION
SELECT 'ad4c063e-eefb-4207-9e8a-8f4d5f224d2e','1379978','','','','','','','' UNION
SELECT 'ab1323d9-9731-4ee3-9486-6fcda658fced','5497','HARBOR GROUP MANAGEMENT CO. LLC','999 WATERSIDE DR STE 2300','NORFOLK','VA','23510-3324','NORFOLK CITY','UNITED STATES' UNION
SELECT '641b5796-a555-4ff0-b160-e80b1c8c9ef6','21065','HARBOR HOUSE FOUNDATION, INC.','2707 HARPER RD','CHOCTAW','OK','73020-8666','OKLAHOMA COUNTY','UNITED STATES' UNION
SELECT '616d5b46-bce4-4dc0-8a5d-3c8543c10066','18211','','','','','','','' UNION
SELECT 'fb55d82e-f517-4abe-99ef-7c0a10608ba1','6316','HARMONY HOSPITALITY, INC','1300 DIAMOND SPRINGS RD','VIRGINIA BEACH','VA','23455-3645','VIRGINIA BEACH CITY','UNITED STATES' UNION
SELECT 'b5025169-d878-425d-8633-8882efcbee40','2794','HARPER REALTY GROUP LLC','7151 QUEBEC ST','COMMERCE CITY','CO','80022-5804','ADAMS COUNTY','UNITED STATES' UNION
SELECT '5552db2d-5fe4-4693-92c1-1fa69a133e30','19625','HARRISONBURG UNIVERSITY PLACE CONDOS','165 MASSANETTA SPRINGS RD','ROCKINGHAM','VA','22801-3575','ROCKINGHAM COUNTY','UNITED STATES' UNION
SELECT 'f3d4d8c2-130c-4f35-897e-a52be994a3ab','14689','HARVEST INVESTMENTS','4090 N HIGHWAY 16','DENVER','NC','28037-7908','LINCOLN COUNTY','UNITED STATES' UNION
SELECT '0b905595-bd9f-4f64-804f-de9db4bb6d6a','1375185','HATTERAS SKY','999 PEACHTREE ST NE STE 1120','ATLANTA','GA','30309-4471','FULTON COUNTY','UNITED STATES' UNION
SELECT 'd6d8edfc-c4c7-4a72-86ea-5897f81f6217','441','HAVEN COMMUNITIES','9211 GARLAND RD','DALLAS','TX','75218-3627','DALLAS COUNTY','UNITED STATES' UNION
SELECT 'df2cfa30-bb99-47d4-849a-d37785dab7da','6052','HAVENS PROPERTIES','9020 QUIOCCASIN RD STE J','RICHMOND','VA','23229-5515','HENRICO COUNTY','UNITED STATES' UNION
SELECT '86718687-e412-49fa-ac56-b4ada78ec98e','1298','HAVERKAMP PROPERTIES','4720 MORTENSEN RD STE 105','AMES','IA','50014-6217','','UNITED STATES' UNION
SELECT '2f103197-7931-42b1-ad3d-30fc564f349c','2418','HAVRE EAGLES MANOR','20 3RD ST W','HAVRE','MT','59501-3570','HILL COUNTY','UNITED STATES' UNION
SELECT '16720abf-0909-4627-beff-6ba58824f206','1154','HAWTHORNE RESIDENTIAL PARTNERS','806 GREEN VALLEY RD STE 311','GREENSBORO','NC','27408-7076','GUILFORD COUNTY','UNITED STATES' UNION
SELECT '19cfb9ab-00fc-4f47-a7f9-37c82cd5ea34','21102','HAYES ENTERPRISES TAMPA, LLC','13412 GRAND PRIX WAY OFC 23','TAMPA','FL','33612-3613','HILLSBOROUGH COUNTY','UNITED STATES' UNION
SELECT '447762bd-df70-47f8-af30-af4424077543','6398','HAYMAN COMPANY','29100 NORTHWESTERN HWY STE 410','SOUTHFIELD','MI','48034-1081','OAKLAND COUNTY','UNITED STATES' UNION
SELECT 'f0da28bc-ae85-455c-9f88-337e5862c880','7145','HEARTLAND REALTY INVESTORS INC','4802 NICOLLET AVE','MINNEAPOLIS','MN','55419-5511','HENNEPIN COUNTY','UNITED STATES' UNION
SELECT '253e3e1e-04f7-4b46-b40d-54aab5ebd1f0','6559','HEF MANAGEMENT LLC','437 SW 4TH AVE','FORT LAUDERDALE','FL','33315-1007','BROWARD COUNTY','UNITED STATES' UNION
SELECT '0bea13e7-871a-406e-80b5-9f6a77c8af2d','943','HEKEMIAN & CO., INC.','505 MAIN ST STE 400','HACKENSACK','NJ','07601-5925','BERGEN COUNTY','UNITED STATES' UNION
SELECT 'af6a604f-6373-4ef6-88f2-964c1be9ff73','7296','HENDERSON BEHAVIORAL HEALTH','4717 N STATE ROAD 7 BLDG C','LAUDERDALE LAKES','FL','33319-5859','BROWARD COUNTY','UNITED STATES' UNION
SELECT '311ed360-f3fe-4efe-a044-91136c8c95d6','1379979','','','','','','','' UNION
SELECT '2338c076-0582-4625-a221-be3a30b2bf23','1235','HERCULES REAL ESTATE SERVICES INC','168 BUSINESS PARK DR STE 103','VIRGINIA BEACH','VA','23462-6532','VIRGINIA BEACH CITY','UNITED STATES' UNION
SELECT '4d31d669-9d57-4e5f-95b8-c695fe253894','19361','HERINGTON HEIGHTS DEVELOPMENT LP','201 E HELEN ST','HERINGTON','KS','67449-1660','DICKINSON COUNTY','UNITED STATES' UNION
SELECT 'a6f6532a-42d7-4032-ab0a-4570959fc65f','14825','HERITAGE CIRCLE APTS, LLC','101 PERSON CT','ROXBORO','NC','27573-5861','PERSON COUNTY','UNITED STATES' UNION
SELECT '27dc1015-18f3-482a-b3d3-c4355f7b885c','3732','HERITAGE HOUSING PARTNERS CORP','9770 PATUXENT WOODS DR STE 305','COLUMBIA','MD','21046-3385','HOWARD COUNTY','UNITED STATES' UNION
SELECT '34758c92-c9b3-4a59-87e3-b6c15d4b2656','8272','HERITAGE PROPERTY MANAGEMENT SERVICES, INC.','500 SUGAR MILL RD BLDG B  STE 200','ATLANTA','GA','30350-2865','FULTON COUNTY','UNITED STATES' UNION
SELECT 'cdd44878-31be-4c00-a27e-25f6d1f67cb9','1379980','','','','','','','' UNION
SELECT 'cf752409-cc7a-4dc9-aa94-546e0e593ee6','731','HERMAN AND KITTLE PROPERTIES','500 E 96TH ST STE 300','INDIANAPOLIS','IN','46240-3778','HAMILTON COUNTY','UNITED STATES' UNION
SELECT '577f2dd2-5c4f-4831-ab4a-c036ce33069c','444','','','','','','','' UNION
SELECT 'c017ed29-fd25-47dd-bdf9-62bd49b09a51','7370','HICKOK-DIBLE COMPANIES','9000 W 64TH TER','MERRIAM','KS','66202-4718','JOHNSON COUNTY','UNITED STATES' UNION
SELECT 'dc40ae34-fc96-4dae-ab04-36d10a824480','669','HIGHLAND PROPERTY MGMT INC','283 W FRONT ST STE 1','MISSOULA','MT','59802-4328','MISSOULA COUNTY','UNITED STATES' UNION
SELECT '384a6792-1613-4094-88ef-5e464af9a37c','1379981','','','','','','','' UNION
SELECT 'f52b2349-29a1-40cf-b8f9-2e570bd8b4b2','1379982','','','','','','','' UNION
SELECT 'd60c3cd3-4ae0-4853-966a-386400a28e83','6272','HILAND MANAGEMENT CO, LLC','400 E MILL PLAIN BLVD STE 500','VANCOUVER','WA','98660-3492','CLARK COUNTY','UNITED STATES' UNION
SELECT '4c793536-a99d-460b-89f5-9c59f05e0b30','5982','HILLCREST MANAGEMENT LLC','459 SHASTA DR','BRIDGEWATER','NJ','08807-3730','SOMERSET COUNTY','UNITED STATES' UNION
SELECT '94ea6d3f-c090-4b1c-8751-08ce831770a5','1369891','HILLPOINTE LLC','1031 W MORSE BLVD STE 240','WINTER PARK','FL','32789-3774','ORANGE COUNTY','UNITED STATES' UNION
SELECT '19da8078-2064-4a48-b563-dc7ca398be1d','9391','HILLTOP RESIDENTIAL, LLC','9 GREENWAY PLZ STE 2050','HOUSTON','TX','77046-0947','HARRIS COUNTY','UNITED STATES' UNION
SELECT '6edf3934-b34a-4b65-a9c8-7b2e6bd311c2','1922','HILLWOOD PROPERTIES','13600 HERITAGE PKWY STE 200','FORT WORTH','TX','76177-4320','TARRANT COUNTY','UNITED STATES' UNION
SELECT '96be7993-c538-46e9-a0a5-a3f9e82517af','5562','HK CAPITAL','6718 DE MOSS DR','HOUSTON','TX','77074-4957','HARRIS COUNTY','UNITED STATES' UNION
SELECT 'ad500755-4cd6-45f3-a674-f4d7393fd4eb','22534','HOCK MANAGEMENT COMPANY INC.','2807 YULUPA AVE','SANTA ROSA','CA','95405-8653','SONOMA COUNTY','UNITED STATES' UNION
SELECT 'f50f0146-dd97-4062-9d6e-d19d7e1e459f','1962','HOLABIRD MANAGEMENT','1705 HOLAVIEW RD APT A1','BALTIMORE','MD','21222-2043','BALTIMORE COUNTY','UNITED STATES' UNION
SELECT '4cbdcaf6-140a-4824-be99-b21d82455f0c','1377877','HOLIDAY ISLES PROPERTY MANAGEMENT, INC.','11350 66TH ST STE 124','LARGO','FL','33773-5525','PINELLAS COUNTY','UNITED STATES' UNION
SELECT '37cf176a-0c9a-4ff2-ab8e-05892bd73ea0','2024','HOLLAND RESIDENTIAL','1111 MAIN ST STE 700','VANCOUVER','WA','98660-2991','','UNITED STATES' UNION
SELECT '0abab1d6-a16c-4da8-94a1-267853a66a38','894','HOLLYHAND COMPANY','527 MAIN AVE STE B','NORTHPORT','AL','35476-4418','TUSCALOOSA COUNTY','UNITED STATES' UNION
SELECT '87b664f5-4d1e-4aae-b66b-867d71a3c993','5399','HOLSTEN REAL ESTATE DEVELOPMENT CORPORATION','1020 W MONTROSE AVE','CHICAGO','IL','60613-1323','COOK COUNTY','UNITED STATES' UNION
SELECT '4c39a065-52af-4a6e-880a-00b753b0769a','7459','HOME BASE PROPERTY MANAGEMENT LLC','114 19TH ST','ROCK ISLAND','IL','61201-8008','ROCK ISLAND COUNTY','UNITED STATES' UNION
SELECT 'e3e400b8-aacd-4633-915e-87daed3dfd22','1375406','','','','','','','' UNION
SELECT '3874e985-559e-4b9d-a5bf-5d6393e79b3f','21113','HOMEFIRST SERVICES OF SANTA CLARA','507 VALLEY WAY','MILPITAS','CA','95035-4105','SANTA CLARA COUNTY','UNITED STATES' UNION
SELECT '1446bf26-d9c7-44fc-8e4f-b1fbb4054714','1379983','','','','','','','' UNION
SELECT 'b0906c86-165a-4924-b602-0d9fcc1e579a','14781','','','','','','','' UNION
SELECT '41a7e950-66fc-4e16-9820-f36010c01cd8','1656','','','','','','','' UNION
SELECT '511b4f53-7afb-4438-93e5-f0c11323da5c','6781','HOMEWOOD PROPERTY MANAGEMENT, LLC','274 MADISON AVE RM 1401','NEW YORK','NY','10016-0701','NEW YORK COUNTY','UNITED STATES' UNION
SELECT '3634e7b2-d5b5-4767-9b81-81852943df6b','8236','HORIZON MANAGEMENT SERVICES INC','5201 E TERRACE DR STE 300','MADISON','WI','53718-8362','DANE COUNTY','UNITED STATES' UNION
SELECT 'f604a509-14ac-404c-acf1-143081fa4084','1266','HORIZON REALTY ADVISORS','601 UNION ST STE 1401','SEATTLE','WA','98101-2341','KING COUNTY','UNITED STATES' UNION
SELECT 'ecc0a5e5-50c0-4f76-89b1-c4df1bb6fde1','446','HOUSE AUTHORITY OF THE CITY OF EL PASO','5300 E PAISANO DR','EL PASO','TX','79905','','UNITED STATES' UNION
SELECT '672eb207-f1f2-42b3-8822-0f3d1106da65','1379526','HOUSING & REDEVELOPMENT AUTHORITY OF HUTCHINSON, MN','133 3RD AVE SW','HUTCHINSON','MN','55350-2400','MCLEOD COUNTY','UNITED STATES' UNION
SELECT 'e54d9505-aee2-4f6b-aa72-e82a8c4f2992','1380509','HOUSING AUTHORITY OF ELGIN','130 S STATE ST','ELGIN','IL','60123-6444','KANE COUNTY','UNITED STATES' UNION
SELECT '4f50e047-c277-422e-a869-f4fc570f30ef','24057','HOUSING AUTHORITY OF THE BIRMINGHAM DISTRICT','1826 3RD AVE S','BIRMINGHAM','AL','35233-1905','JEFFERSON COUNTY','UNITED STATES' UNION
SELECT '9b19e5ad-ac35-490f-b35a-5ed44aa24817','568','HOUSING HOPE','5830 EVERGREEN WAY','EVERETT','WA','98203-3748','SNOHOMISH COUNTY','UNITED STATES' UNION
SELECT '98b20fa8-de0f-4a68-85d5-b8fdd3b8701f','26624','','','','','','','' UNION
SELECT 'b4e08649-d15e-4d9f-96b2-4d51c291dc82','4950','HOUSING OPPORTUNITIES, INC.','600 E 43RD ST','TEXARKANA','AR','71854-1470','MILLER COUNTY','UNITED STATES' UNION
SELECT 'd22e88ee-9ef4-47b5-93fd-a91b8111f312','3681','HOUSTON HOUSING MANAGEMENT CORPORATION','2211 NORFOLK ST STE 614','HOUSTON','TX','77098-4044','HARRIS COUNTY','UNITED STATES' UNION
SELECT '5d6d0d63-72c3-4af7-929e-75d8b4f9d3ec','6462','HPMA, CORP.','1100 S MCCASLIN BLVD STE 102','SUPERIOR','CO','80027-8614','BOULDER COUNTY','UNITED STATES' UNION
SELECT 'b85a8846-53fe-4417-ba3f-966ceb7acbda','5453','HSL PROPERTIES INC.','3901 E BROADWAY BLVD','TUCSON','AZ','85711-3452','PIMA COUNTY','UNITED STATES' UNION
SELECT 'b2c46ed2-b80f-4ee2-b423-614d71c90059','2854','HSR PROPERTY SERVICES, LLC','7601 W. 191ST STREET, SUITE 1E','TINLEY PARK','IL','60487','WILL COUNTY','United States' UNION
SELECT '3dc1e4a7-00a4-4d74-ab4c-311d8e1d55a5','1243','HTG MANAGEMENT, LLC.','3225 AVIATION AVE STE 602','COCONUT GROVE','FL','33133-4741','MIAMI-DADE COUNTY','UNITED STATES' UNION
SELECT '15cb7ee7-b9c5-4577-9622-4ea9a7497e16','7575','HUDSON HENLEY PROPERTIES','2520 FAIRMOUNT ST STE 200','DALLAS','TX','75201-1902','DALLAS COUNTY','UNITED STATES' UNION
SELECT '4b0828a9-02c2-4c72-b3ef-e3c69b35d9c7','2948','HUDSON PROPERTY MANAGEMENT SERVICES, LLC','2450 SHENANGO VALLEY FWY','HERMITAGE','PA','16148-2572','MERCER COUNTY','UNITED STATES' UNION
SELECT '7c0500c0-64f2-4bf3-8d48-108fdac8b6c9','3672','HUDSON REAL ESTATE','1200 28TH ST','BOULDER','CO','80303-1924','BOULDER COUNTY','UNITED STATES' UNION
SELECT 'e4307082-ec01-45dc-8ad7-1a756a6cbf68','4807','HUFFJONES MANAGEMENT CO., LLC DBA ESTAGE PROPERTY MANAGEMENT','4611 BEE CAVES RD STE 203','AUSTIN','TX','78746-5283','','UNITED STATES' UNION
SELECT 'e987a302-cd59-4209-a1e2-038461699b79','5436','HUMANGOOD AFFORDABLE HOUSING','6120 STONERIDGE MALL RD','PLEASANTON','CA','94588-3296','ALAMEDA COUNTY','UNITED STATES' UNION
SELECT 'e8db5f47-b4e2-4059-8ab9-1d35abf6fb99','6061','I AND MJ GROSS COMPANY','14300 RIDGE RD','NORTH ROYALTON','OH','44133-4936','CUYAHOGA COUNTY','UNITED STATES' UNION
SELECT '946cb941-91ad-4f92-b22a-4981d8188d15','982','IA MANAGEMENT, LLC','810 CARDINAL LN STE 100','HARTLAND','WI','53029-2390','WAUKESHA COUNTY','UNITED STATES' UNION
SELECT 'b503d20c-fa69-473f-b36b-9dfbe4b6fcb7','5674','I ASSET MANAGEMENT','9801 IRVINE CENTER DR','IRVINE','CA','92618-4307','ORANGE COUNTY','UNITED STATES' UNION
SELECT 'd990850c-b2c8-4856-881a-b0c56cf41e01','5686','ICON INVESTMENT GROUP II LLC','8837 CHAPELSQUARE LN UNIT 1A','CINCINNATI','OH','45249-4706','HAMILTON COUNTY','UNITED STATES' UNION
SELECT 'be8cce98-e32f-4115-9d47-72035fccba46','7212','IDYLWOOD GARDEN ASSOCIATES','2206 PIMMIT RUN LN APT 3','FALLS CHURCH','VA','22043-3817','FAIRFAX COUNTY','UNITED STATES' UNION
SELECT 'cc5f771b-dad7-4a95-8f4d-10a0110dbc32','6540','','','','','','','' UNION
SELECT '411dc0ed-f787-4681-9875-1a9ed065ca8c','6162','IN GOOD COMPANY LLC','320 E 90TH DR STE A','MERRILLVILLE','IN','46410-7351','LAKE COUNTY','UNITED STATES' UNION
SELECT 'fddd08fc-bae2-4eff-b244-cdb6cc6b6b9f','17595','INCOME PROPERTY INVESTMENTS, INC','10807 LAUREL ST STE 100','RANCHO CUCAMONGA','CA','91730-3866','SAN BERNARDINO COUNTY','UNITED STATES' UNION
SELECT '40d66b05-585e-4cf7-a73c-7fa7e33764f2','1384','INCOME PROPERTY MANAGEMENT','1800 SW 1ST AVE STE 220','PORTLAND','OR','97201-5333','MULTNOMAH COUNTY','UNITED STATES' UNION
SELECT 'ff740e5a-b0fd-4f41-8837-fd5eebe2f9eb','25098','INDEPENDENCE MANOR I &  II, INC','1501 PRUITT HILL DR','NACOGDOCHES','TX','75961-4672','NACOGDOCHES COUNTY','UNITED STATES' UNION
SELECT 'c426797e-2bfa-4548-a390-97104d9a0d03','5668','INLAND RESIDENTIAL REAL ESTATE SERVICES, LLC','2901 BUTTERFIELD RD','OAK BROOK','IL','60523-1106','DUPAGE COUNTY','UNITED STATES' UNION
SELECT '8a0b77c1-3d29-471e-91c0-fd388ed6f37b','21109','','','','','','','' UNION
SELECT 'dbb5f47b-fc49-41c1-a4ec-87b9333f8753','1371280','INSPIRICA, INC.','141 FRANKLIN ST','STAMFORD','CT','06901-1014','FAIRFIELD COUNTY','UNITED STATES' UNION
SELECT 'eb8af7ed-35ed-42ae-978e-4b8b172c0a5a','7268','INTEGRA AFFORDABLE MANAGEMENT, LLC','3555 W PETERSON AVE STE 303','CHICAGO','IL','60659-3265','COOK COUNTY','UNITED STATES' UNION
SELECT '718a2402-3923-43c0-ade2-5068822f4c6d','6527','INTEGRAL SENIOR LIVING MANAGEMENT, LLC','2333 STATE ST STE 300','CARLSBAD','CA','92008-1691','SAN DIEGO COUNTY','UNITED STATES' UNION
SELECT '49c0bbef-c280-4212-96be-4f89d0f47524','5503','INTERMARK MANAGEMENT','808-B LADY ST','COLUMBIA','SC','29201-3104','','UNITED STATES' UNION
SELECT 'f6825bb2-6304-4d94-8dbc-159219d15cd5','23381','INTERVEST CORPORATION','7538 OLD CANTON RD','MADISON','MS','39110-8967','MADISON COUNTY','UNITED STATES' UNION
SELECT 'ed658b27-d317-443d-9808-0990bc1010e2','538','INVESCO REALTY, INC. A DELAWARE CORPORATION','2001 ROSS AVE STE 3400','DALLAS','TX','75201-2966','DALLAS COUNTY','UNITED STATES' UNION
SELECT '0db90462-f91f-4594-a748-5b8cd754f665','1386','INVESTMENT CONCEPTS INC.','1667 E LINCOLN AVE','ORANGE','CA','92865-1929','ORANGE COUNTY','UNITED STATES' UNION
SELECT 'f942e8cb-a1d2-4670-9772-052d2a317be4','8508','INVESTMENT PROPERTY ADVISORS','11237 CORNELL PARK DR STE 200','BLUE ASH','OH','45242-1811','HAMILTON COUNTY','UNITED STATES' UNION
SELECT '154f53a4-7109-4377-8f91-aae6071938c6','1387','INVESTORS MANAGEMENT TRUST REAL ESTATE GROUP, INC','15303 VENTURA BLVD STE 200','SHERMAN OAKS','CA','91403-3126','','UNITED STATES' UNION
SELECT 'f675a8cc-6958-494a-b92e-673a188beb17','1379647','','','','','','','' UNION
SELECT '2643824c-8230-4630-a488-52d432fb7496','4563','IPM COLORADO,LLC','8137 ZANG ST','ARVADA','CO','80005-5190','JEFFERSON COUNTY','UNITED STATES' UNION
SELECT '01cb0d90-1f2d-4d62-8aa9-30058dcc3fe3','8189','IRON RIVER MANAGEMENT, LLC','155 S MADISON ST STE 214','DENVER','CO','80209-3011','DENVER COUNTY','UNITED STATES' UNION
SELECT 'ff084b7a-e063-4f52-a2a1-4ec0eb0a0938','7175','IRVINE MANAGEMENT COMPANY','101 INNOVATION DR','IRVINE','CA','92617-3040','ORANGE COUNTY','UNITED STATES' UNION
SELECT '8da000d4-19c4-4ba4-ac22-0b85966be458','5526','IRWIN R ROSE AND COMPANY LLC','9245 N MERIDIAN ST STE 215','INDIANAPOLIS','IN','46260-1812','MARION COUNTY','UNITED STATES' UNION
SELECT '3fd36796-cf82-4acc-958b-4d8c29bf8f72','1371487','IVY RIDGE REALTY ASSOCIATES LLC','589 YALE ST','HARRISBURG','PA','17111-3560','DAUPHIN COUNTY','UNITED STATES' UNION
SELECT '5b3f84fa-9dcd-4b26-b360-7668a17b687c','6887','J CYRIL INVESTMENT CORP','125 WILLOW RD','MENLO PARK','CA','94025-2750','','UNITED STATES' UNION
SELECT '1deb1033-2b75-4e5d-a719-51e7ce6d3433','5495','J HESTER PROPERTIES','2500 NE GREEN OAKS BLVD STE 203','ARLINGTON','TX','76006-3028','TARRANT COUNTY','United States' UNION
SELECT '66ec01fb-561c-4448-8e02-c3eb433944da','1385','J. ALLEN MANAGEMENT','1390 BROADWAY ST','BEAUMONT','TX','77701-2108','JEFFERSON COUNTY','UNITED STATES' UNION
SELECT 'ff62e5ec-bbc4-4f83-b749-1888b88b5a6e','6290','J.P. THAYER COMPANIES, INC.','32 BRICKYARD RD','PHENIX CITY','AL','36869-3662','RUSSELL COUNTY','UNITED STATES' UNION
SELECT 'd07ce89e-aa6a-45c4-b52c-472225ecbe99','1187','J.T. BURNS MANAGEMENT CORP','1732 WESTERN AVE','ALBANY','NY','12203-4478','ALBANY COUNTY','UNITED STATES' UNION
SELECT 'e4740063-d6c8-41f3-b39d-8900b6b0acf1','6689','','','','','','','' UNION
SELECT '61668338-0c46-4dc6-b54d-f9cf8729498e','20132','','','','','','','' UNION
SELECT 'cc1097a7-28dc-409f-ba91-ca8a1af5c67d','1380172','JADON PROPERTIES, INC.','4225 MURRAY PL','LYNCHBURG','VA','24501-5007','LYNCHBURG CITY','UNITED STATES' UNION
SELECT 'd0c327cf-ea17-4c88-9ae7-dbc7254228ad','5538','JAG MANAGEMENT COMPANY LLC','1420 SPRING HILL RD STE 420','MCLEAN','VA','22102-3028','','UNITED STATES' UNION
SELECT '640e28b4-888c-4d72-a0c4-58fb88a73697','23558','JALEE, INC.','4505 12TH AVE S','GREAT FALLS','MT','59405-8033','CASCADE COUNTY','UNITED STATES' UNION
SELECT '536cd832-4834-449e-8f6d-b36aa757e019','1956','JALSON CO., INC. DBA GERSON BAKAR AND ASSOCIATES','201 FILBERT ST STE 700','SAN FRANCISCO','CA','94133-3242','SAN FRANCISCO COUNTY','UNITED STATES' UNION
SELECT 'cb0c274c-b615-41c8-b4af-211cfeb63389','1380020','','','','','','','' UNION
SELECT '672fc8d5-6e24-4f1d-a2c4-55321c97190a','3748','JAMESTOWN YMCA','101 E 4TH ST','JAMESTOWN','NY','14701-5380','CHAUTAUQUA COUNTY','UNITED STATES' UNION
SELECT '1a91390e-c8d0-4fed-b98f-3c599df12e97','30675','JANKOW MANAGEMENT, INC','1200 WESTERN AVE','ALBANY','NY','12203-3326','ALBANY COUNTY','UNITED STATES' UNION
SELECT '516069f1-e2d4-4e90-b7ac-bf4c80cf3363','6097','JARS MANAGEMENT LLC','209 CORNWALL ST NW','LEESBURG','VA','20176-2702','LOUDOUN COUNTY','UNITED STATES' UNION
SELECT '88c9e606-15bf-46fd-a864-6dc098beaf5f','5961','JBT PROPERTY MANAGEMENT COMPANY, INC.','1520 W KETTLEMAN LN STE A1','LODI','CA','95242-9290','SAN JOAQUIN COUNTY','UNITED STATES' UNION
SELECT '0b0c0561-20af-4624-8498-c8d23f19b8fd','2954','JCC HOUSING CORPORATION','1635 RAOUL WALLENBERG BLVD','CHARLESTON','SC','29407-3539','CHARLESTON COUNTY','UNITED STATES' UNION
SELECT 'ffd7a64a-a5d5-46ca-b979-f00ccbd3b3b0','6568','JCHE SERVICES, INC','30 WALLINGFORD RD','BRIGHTON','MA','02135-4708','SUFFOLK COUNTY','UNITED STATES' UNION
SELECT '273f74f2-efc0-4fc4-8c89-0f75e6817b5b','1372646','JCL MANAGEMENT L.L.C.','19 PELL PL','BRONX','NY','10464-1515','BRONX COUNTY','UNITED STATES' UNION
SELECT '6106f1cd-a922-42de-92b0-15fb0cdbddb3','8603','JD REAL ESTATE, LLC','139 ONEIL CT','COLUMBIA','SC','29223-7608','RICHLAND COUNTY','UNITED STATES' UNION
SELECT '17f0aca4-e170-419a-b6ba-02b30c64880e','1003','JDC MANAGEMENT LLC','474 WANDO PARK BLVD STE 102','MT PLEASANT','SC','29464-7933','','United States' UNION
SELECT '2699cb78-d8ab-454b-8463-06e470edb681','8366','JEFFERSON SHADOWS APTS PARTNERSHIP','9771 JEFFERSON HWY','BATON ROUGE','LA','70809-2718','','UNITED STATES' UNION
SELECT '2fc62853-d42d-4bd4-8346-15357e1f1b81','303','JEFFREY CHARLES AND ASSOCIATES, INC.','6422 GROVEDALE DR','ALEXANDRIA','VA','22310-2570','','UNITED STATES' UNION
SELECT '2a8e4ec2-0755-4f6f-9c96-8505e39112eb','15041','JEFFREY R. ANDERSON REAL ESTATE','3825 EDWARDS RD','CINCINNATI','OH','45209-1287','HAMILTON COUNTY','UNITED STATES' UNION
SELECT '93b0af51-02c3-429f-a6f0-1cb931fe807b','8018','JGV MANAGEMENT CORP','1325 SOUTHERN BLVD','BRONX','NY','10459-3269','BRONX COUNTY','UNITED STATES' UNION
SELECT '9559b3a5-8d6d-4f5a-aede-07cba162fdc4','1379984','','','','','','','' UNION
SELECT 'c2a87eac-ca77-4ca1-9492-93def8ec2018','5374','JLB RESIDENTIAL LLC','3890 W NORTHWEST HWY STE 700','DALLAS','TX','75220-8109','DALLAS COUNTY','UNITED STATES' UNION
SELECT '86a0da7a-d2d8-496e-a92e-549856f9ddb2','6247','JMG REALTY, INC.','5605 GLENRIDGE DR STE 1010','ATLANTA','GA','30342-1381','FULTON COUNTY','UNITED STATES' UNION
SELECT 'fce86008-b50d-4680-b086-fad03677e95d','4809','JMK INVESTMENTS, INC','100 SARATOGA AVE STE 300','SANTA CLARA','CA','95051-7337','SANTA CLARA COUNTY','UNITED STATES' UNION
SELECT 'efd9e6b8-3ac6-44a8-ac2f-45ae76180a71','5989','JN MANAGEMENT','1 STEVENS RD','WALLINGTON','NJ','07057-1724','BERGEN COUNTY','UNITED STATES' UNION
SELECT 'fe0d5eb0-d0c5-495e-bd72-013aa3c74c5a','1380021','','','','','','','' UNION
SELECT '3725eff9-a294-4ab2-9032-047128647e2c','1379985','','','','','','','' UNION
SELECT 'ed7cf1b1-39da-494b-b58a-188bbb7a02ed','1370113','JOWERS, INC.','6565A E INTERSTATE 30','FATE','TX','75189-8548','ROCKWALL COUNTY','UNITED STATES' UNION
SELECT '8bbf720a-0718-43df-a097-6a82a1609d1a','1379750','JOY JEM COMMUNITY DEVELOPMENT CORPORATION','2632 S ROCHESTER RD UNIT 70531','ROCHESTER HILLS','MI','48307-7924','OAKLAND COUNTY','UNITED STATES' UNION
SELECT '5f1c13cc-ec25-4328-84a7-22c63046dcd7','187','JRK RESIDENTIAL GROUP INC A NEVADA CORPORATION','11766 WILSHIRE BLVD 15TH FL','LOS ANGELES','CA','90025-6538','LOS ANGELES COUNTY','UNITED STATES' UNION
SELECT 'd1022779-584e-436c-8fb6-dbd8d79bc193','1679','JS PROPERTY MANAGEMENT, LLC','11 N 2ND ST','ST CHARLES','IL','60174-1869','KANE COUNTY','UNITED STATES' UNION
SELECT '88621144-43bb-4d36-a544-32b480dffa05','1375087','JSL MANAGEMENT SOLUTIONS, LLC','2636 FOREST LAKE WAY','MARYVILLE','TN','37803-8419','BLOUNT COUNTY','UNITED STATES' UNION
SELECT 'fd26f357-19b8-40cd-bf66-b11ab81c6efd','6067','JUSTUS RENTAL PROPERTIES, INC','1398 N SHADELAND AVE','INDIANAPOLIS','IN','46219-3652','MARION COUNTY','UNITED STATES' UNION
SELECT '7e52666c-9737-4406-b144-ad1530ba18f0','6478','JVM REALTY CORPORATION','903 COMMERCE DR STE 250','OAK BROOK','IL','60523-8825','DUPAGE COUNTY','UNITED STATES' UNION
SELECT 'd4ec853c-a6f9-4ccc-9d3e-20e998aa3688','1375669','KA HALE A KE OLA HOMELESS RESOURCE CENTERS, INC.','670 WAIALE RD','WAILUKU','HI','96793-2375','MAUI COUNTY','UNITED STATES' UNION
SELECT '9e30aeb5-624d-4799-8ec2-eaf39db5d91f','5894','KAIROI MANAGEMENT, LLC','1900 NW LOOP 410 STE 320','SAN ANTONIO','TX','78213-2330','BEXAR COUNTY','UNITED STATES' UNION
SELECT '5e035af8-c98a-4c03-b067-4b336fd9ebc7','1379986','','','','','','','' UNION
SELECT '02015ef4-30a7-4d19-add7-d1a776dc97c3','1371726','KANAKA MANAGEMENT COMPANY LLC','2150 WEHRLE DR STE 400','WILLIAMSVILLE','NY','14221-7099','ERIE COUNTY','UNITED STATES' UNION
SELECT 'df26fd3f-de99-4fe5-8ccb-c7b7c3caed8e','566','KANE RESIDENTIAL, LLC','4321 LASSITER AT NORTH HILLS AVE STE 250','RALEIGH','NC','27609-5782','WAKE COUNTY','UNITED STATES' UNION
SELECT '5738f09c-73f7-4024-b122-c59384be5558','1371218','KAPLAN MANAGEMENT COMPANY, INC.','777 POST OAK BLVD STE 850','HOUSTON','TX','77056-3273','HARRIS COUNTY','UNITED STATES' UNION
SELECT 'fef915a5-b05f-415f-9bd6-670e83bd2368','6803','KARYA PROPERTY MANAGEMENT LLC','8901 GAYLORD DR STE 100','HOUSTON','TX','77024-3042','HARRIS COUNTY','UNITED STATES' UNION
SELECT '4069939e-6229-4030-9bf0-79462b25b368','17662','KAY-KAY MANAGEMENT SERVICES UT, INC.','6908 E THOMAS RD STE 300','SCOTTSDALE','AZ','85251-6898','MARICOPA COUNTY','UNITED STATES' UNION
SELECT '84322e4a-3436-4a1c-9ca5-937e3a4e87d3','1378150','','','','','','','' UNION
SELECT '852d77d0-a0e5-438f-a5ff-80f60481ff8d','1371137','KEASEY PROPERTIES','9800 208TH AVE NE','REDMOND','WA','98053-5217','KING COUNTY','UNITED STATES' UNION
SELECT '99c10ce1-fdec-4191-9133-f040aa7f3d7c','2886','KELI MANAGEMENT, INC.','6324 N CHATHAM AVE # 123','KANSAS CITY','MO','64151-2473','PLATTE COUNTY','UNITED STATES' UNION
SELECT '07edfdbd-3d31-48fc-b38c-6d7ad2be83c8','3815','KELLER INVESTMENTS','500 N MARKET PLACE DR STE 101','CENTERVILLE','UT','84014-1711','DAVIS COUNTY','UNITED STATES' UNION
SELECT '28629116-ebcd-42af-b66a-e5f98864600a','8283','KEN TEMPLETON REALTY AND INVESTMENTS INC.','3311 S RAINBOW BLVD STE 225','LAS VEGAS','NV','89146-6210','CLARK COUNTY','UNITED STATES' UNION
SELECT '316e2306-2e88-4923-a457-0208647bd220','1379987','','','','','','','' UNION
SELECT '8e432105-44d3-411d-9749-3ed25a1f031b','5549','KENNEDY - WILSON LTD','151 EL CAMINO DR','BEVERLY HILLS','CA','90212-2704','LOS ANGELES COUNTY','UNITED STATES' UNION
SELECT '88908047-c9e0-492c-be10-8b4bb9aa25dc','3276','KENNETH WALLACE','1030 W MAIN ST','MOUNT VERNON','KY','40456-2561','ROCKCASTLE COUNTY','UNITED STATES' UNION
SELECT '9bd8eb07-67f7-4341-b06a-9d0ea4fd8d11','545','','','','','','','' UNION
SELECT '222cfac5-4dcb-4103-a98f-672e131241b5','6573','KETTLER MANAGEMENT CO.','8255 GREENSBORO DR STE 200','MC LEAN','VA','22102-4944','FAIRFAX COUNTY','UNITED STATES' UNION
SELECT '7cdbf360-1ada-499a-926f-26460f286bcf','1231','KEY MANAGEMENT COMPANY','7701 E KELLOGG DR STE 250','WICHITA','KS','67207-1707','SEDGWICK COUNTY','UNITED STATES' UNION
SELECT 'd5e4b6e4-1d33-4447-a881-1ec68de12398','5606','KEY MANAGEMENT COMPANY','643 MAGAZINE ST STE 306','NEW ORLEANS','LA','70130-3433','ORLEANS PARISH','UNITED STATES' UNION
SELECT 'de8e781e-55e7-4023-86e6-cb787c3fc67f','14674','KIMCO REALTY CORPORATION','500 N BROADWAY STE 201','JERICHO','NY','11753-2128','NASSAU COUNTY','UNITED STATES' UNION
SELECT 'dc45d550-dfe4-4c1f-82e8-be6b3ca74ed0','6610','KING PROPERTIES, INC','1400 LAUREL SPRINGS DR','DURHAM','NC','27713-6212','DURHAM COUNTY','UNITED STATES' UNION
SELECT '942a40c0-e7e2-44a7-a6e1-0c97b0ccbec1','5231','KIRKLAND MGMT CO','605 STEED RD','RIDGELAND','MS','39157-9482','MADISON COUNTY','UNITED STATES' UNION
SELECT '1036942f-e1bc-4ed4-af67-18b7c4f80cde','659','KIRKPATRICK MGMT','5702 KIRKPATRICK WAY','INDIANAPOLIS','IN','46220-3925','MARION COUNTY','UNITED STATES' UNION
SELECT '65df36e3-82dd-491d-b554-e65937a437b2','22340','','','','','','','' UNION
SELECT '3418f1fb-0283-4d7f-a80e-a7b08ce79a01','6333','KLINGBEIL CAPITAL','200 CALIFORNIA ST STE 300','SAN FRANCISCO','CA','94111-4340','SAN FRANCISCO COUNTY','UNITED STATES' UNION
SELECT '756e395f-7856-4909-93db-8a557f9af2bd','9236','KLMN PROPERTY MANAGEMENT, INC','734 W POLK ST','PHOENIX','AZ','85007-2539','MARICOPA COUNTY','UNITED STATES' UNION
SELECT '4f6398c7-4278-4b7e-bc67-986062cdd7f8','6153','KMG PRESTIGE, INC','102 S MAIN ST','MT PLEASANT','MI','48858-2336','ISABELLA COUNTY','UNITED STATES' UNION
SELECT '536f8e16-8b67-4155-8caa-54360c2bd728','6074','KNIGHTVEST MANAGEMENT LLC','5400 LYNDON B JOHNSON FWY STE 450','DALLAS','TX','75240-1059','DALLAS COUNTY','UNITED STATES' UNION
SELECT 'd06a6217-4e4d-4622-9646-985caa2d2720','6475','KOLE MANAGEMENT COMPANY','1719 ABERCORN ST','SAVANNAH','GA','31401-7523','CHATHAM COUNTY','UNITED STATES' UNION
SELECT '09a3f6a6-8c03-4521-b8ca-6c2e8e7c2c16','8244','KONOVER RESIDENTIAL CORPORATION','342 N MAIN ST STE 200','WEST HARTFORD','CT','06117-2507','HARTFORD COUNTY','UNITED STATES' UNION
SELECT 'c8b77e2e-9f5c-4a25-8a4c-431735ce9816','1371029','KOZ PROPERTIES LLC','1830 BICKFORD AVE STE 201','SNOHOMISH','WA','98290-1750','SNOHOMISH COUNTY','UNITED STATES' UNION
SELECT '852006e5-0cb6-4d1a-8bdb-03eb116b8d7a','22389','KPC PROPERTIES, LLC','6400 N ANDREWS AVE STE 490','FT LAUDERDALE','FL','33309-9105','BROWARD COUNTY','UNITED STATES' UNION
SELECT 'c7a3a948-75b5-4f9e-8a6d-ed35d0dd34ac','22475','KRAUSE GROUP, LTD.','1459 GRAND AVE','DES MOINES','IA','50309-3005','POLK COUNTY','UNITED STATES' UNION
SELECT '6c0803dc-3cf8-4fe9-8471-39a1e586e4db','1376586','','','','','','','' UNION
SELECT '13a06858-131e-4659-82ea-9322f89dc8ea','6379','L. I. COMBS MANAGEMENT CORPORATION','1152 MARSH ST STE F','VALPARAISO','IN','46385-6295','PORTER COUNTY','UNITED STATES' UNION
SELECT '411fa67d-3e31-4171-8a98-65d2d4e46f1f','6755','L.E.A. PROPERTIES LLC DBA BEACON PROPERTIES','1244 S 4TH ST','LOUISVILLE','KY','40203-3051','JEFFERSON COUNTY','UNITED STATES' UNION
SELECT '3f1340ad-58d5-4281-85d2-989267e50ca3','8665','L3 MANAGEMENT LLC','3461 SW 2ND AVE','GAINESVILLE','FL','32607-2810','ALACHUA COUNTY','UNITED STATES' UNION
SELECT '0add35b4-fc24-4de0-9215-84b58a06f0e5','6757','LA CASA DE ESPERANZA INC','1431 BIG BEND RD','WAUKESHA','WI','53189-2133','WAUKESHA COUNTY','United States' UNION
SELECT '4b246b30-3d34-46cb-b025-b3938f98ec39','6924','LA FORTIS PROPERTY MANAGEMENT, LLC','5015 SHED RD STE 300','BOSSIER CITY','LA','71111-5585','BOSSIER PARISH','UNITED STATES' UNION
SELECT '132df7c4-ec63-4036-9dcb-8d2958755b3e','5722','LABORERS HOME DEVELOPMENT CORP','200 LABOR DR','JACKSONVILLE','IL','62650-3508','MORGAN COUNTY','UNITED STATES' UNION
SELECT '5104d622-0ab5-4921-9d51-499f7586c715','8740','','','','','','','' UNION
SELECT '5a195046-7491-4739-a483-fa506deb32f2','1371817','LAKE HAVASU INDEPENDENT LIVING LLC, AN ARIZONA LIMITED LIABILITY COMPANY','1845 MCCULLOCH BLVD N STE A6','LAKE HAVASU CITY','AZ','86403-5722','MOHAVE COUNTY','UNITED STATES' UNION
SELECT '25a65519-ab20-459c-adb2-b5f93cb0da21','2369','LAKEWOOD MANAGEMENT COMPANY','4200 W CENTRE AVE','PORTAGE','MI','49024-4640','KALAMAZOO COUNTY','UNITED STATES' UNION
SELECT '24dc4102-cdc0-48a8-8d4c-fccecafc3b39','1379988','','','','','','','' UNION
SELECT 'e3f43925-4287-44aa-8dc5-796077cf63da','1379989','','','','','','','' UNION
SELECT 'b0c99917-7b80-4d45-a3f6-0144cbf87c41','6299','LANDMARK MANAGEMENT GROUP','113 PARK CIRCLE DR','FLOWOOD','MS','39232-8878','RANKIN COUNTY','UNITED STATES' UNION
SELECT '77f84885-2819-4e50-a9b6-2cd28d3b3c11','5659','LANDMARK PROPERTY MANAGEMENT COMPANY, INC','406 E 4TH ST','WINSTON SALEM','NC','27101-4112','FORSYTH COUNTY','UNITED STATES' UNION
SELECT '3a79d106-0fa1-4615-9cf7-59b7c905ddda','818','LANDMARK PROPERTY SERVICES INC','4901 DICKENS RD STE 119','RICHMOND','VA','23230-1952','HENRICO COUNTY','UNITED STATES' UNION
SELECT '0c44b0bb-0faa-422f-9777-97ad1fa2e29f','1379990','','','','','','','' UNION
SELECT '12d81951-c945-4871-a135-466accb3586e','1379991','','','','','','','' UNION
SELECT 'd6a5c958-0ea4-4e19-b88c-8c64db077192','2026','LANTOWER LUXURY LIVING LLC, A DELAWARE LIMITED LIABILITY COMPANY','2218 BRYAN ST STE 400','DALLAS','TX','75201-2633','DALLAS COUNTY','UNITED STATES' UNION
SELECT 'b980143e-7055-4bb9-bc4f-009ae720f36b','855','LARRIN MANAGEMENT','300 FRANK W BURR BLVD STE 26','TEANECK','NJ','07666-6703','BERGEN COUNTY','UNITED STATES' UNION
SELECT 'bc028667-6229-45e7-8a89-a4f7d03f2b58','7377','LARRY PEEL DEVELOPMENT','1006 MO PAC CIR STE 201','AUSTIN','TX','78746-6806','TRAVIS COUNTY','UNITED STATES' UNION
SELECT '627ec0ef-4af4-4ca9-aa8c-08d29ad3fc7a','2730','LARRYMORE ORGANIZATION INC','6477 COLLEGE SQ STE 306','VIRGINIA BEACH','VA','23464','','UNITED STATES' UNION
SELECT '6519af8d-26d0-4966-9ccc-eb573b938ac8','1749','LASALLE INVESTMENT MANAGEMENT INC.','8910 UNIVERSITY CENTER LN STE 100','SAN DIEGO','CA','92122-1016','SAN DIEGO COUNTY','UNITED STATES' UNION
SELECT 'bfa31fab-0c79-4900-980b-824ee3173494','7392','LAUNCH DEVELOPMENT, INC','9744 NW CONANT AVE','KANSAS CITY','MO','64153-1832','PLATTE COUNTY','UNITED STATES' UNION
SELECT '9aa104da-2f10-4bf2-8947-92cff13d0eb0','5733','','','','','','','' UNION
SELECT '453aefd8-5a25-4ef7-980a-2e39208dc149','6734','LAURENZ PLACE, LLC','203 W 90TH ST APT 2H','NEW YORK','NY','10024-1220','NEW YORK COUNTY','UNITED STATES' UNION
SELECT '282eeeca-f839-49da-8e0b-81afa9eb9a56','961','LAWSON REALTY CORPORATION','373 EDWIN DR','VIRGINIA BEACH','VA','23462-4522','VIRGINIA BEACH CITY','UNITED STATES' UNION
SELECT '30114a69-0957-47bd-86bb-32fa5b6169cd','6180','LBK MANAGEMENT SERVICES','1320 GREENWAY DR STE 720','IRVING','TX','75038-2546','DALLAS COUNTY','UNITED STATES' UNION
SELECT '5061abf9-e8f3-4eb2-a424-9ed1102cb335','1782','LCL MANAGEMENT','199 BALDWIN RD','PARSIPPANY','NJ','07054-2043','MORRIS COUNTY','UNITED STATES' UNION
SELECT 'bc1f00ec-0150-4032-8e1c-c9f53b8439a3','8280','','','','','','','' UNION
SELECT '9173d58b-9099-404c-84b7-73ad71f1965a','4739','LEGACY PROPERTIES MANAGEMENT COMPANY, LLC.','707 WHITLOCK AVE SW STE F1','MARIETTA','GA','30064-4653','COBB COUNTY','UNITED STATES' UNION
SELECT '8a9025c4-1a61-489e-9a5a-2426d51a412e','1973','LEGACY PROPERTY MANAGEMENT GROUP LLC','1610 16TH AVE S','NASHVILLE','TN','37212-2908','DAVIDSON COUNTY','UNITED STATES' UNION
SELECT '4a9dcb54-76f7-4ab8-b3b2-fc7887a9cf2c','6466','LEGEND ASSET MGMT','2415 AVENUE J STE 111','ARLINGTON','TX','76006-6119','TARRANT COUNTY','UNITED STATES' UNION
SELECT '51a40a81-db9b-445c-8f90-64c384520484','1236','LEGEND MANAGEMENT GROUP, LLC','1355 BEVERLY RD STE 105','MCLEAN','VA','22101-3623','FAIRFAX COUNTY','UNITED STATES' UNION
SELECT '1eaa56b7-f14c-48c2-a39d-9993ccd67a78','1375773','LEGEND MANAGEMENT SERVICES, LLC','1000 FAYETTE ST','CONSHOHOCKEN','PA','19428-1562','MONTGOMERY COUNTY','UNITED STATES' UNION
SELECT 'e789d5e0-79ba-4f18-a1da-5e7418d4fc93','2455','LEGOW MANAGEMENT COMPANY, LLC','160 S LIVINGSTON AVE','LIVINGSTON','NJ','07039-3033','','United States' UNION
SELECT '58ffbc1f-c31d-43af-a34d-b6b249a305ac','5504','LENNOX COMPANIES','5384 POPLAR AVE STE 400','MEMPHIS','TN','38119-0605','SHELBY COUNTY','UNITED STATES' UNION
SELECT '6543fdf2-3646-4e89-a2d4-ea9d6ac6f5af','1038','LETTER9 MANAGEMENT COMPANY LLC','745 5TH AVE 29TH FL','NEW YORK','NY','10151-2999','NEW YORK COUNTY','UNITED STATES' UNION
SELECT 'd9ebc312-8550-4bc3-998e-b6fd6459d7d3','1375313','LEUMAS RESIDENTIAL','4343 PLANK RD STE 115','FREDERICKSBURG','VA','22407-4807','SPOTSYLVANIA COUNTY','UNITED STATES' UNION
SELECT 'bdfa2094-05a0-488c-bb14-6da6ecf1aab1','4865','','','','','','','' UNION
SELECT '9a73b894-cb6c-40ff-9e63-0a062cbd0ceb','6620','LEWIS MANAGEMENT CORP','1156 N MOUNTAIN AVE','UPLAND','CA','91786-3633','SAN BERNARDINO COUNTY','UNITED STATES' UNION
SELECT 'e023ebd0-f325-4689-8c49-42e91832fc2b','5331','LEX GROUP, INC','10411 CLAYTON RD STE 300','SAINT LOUIS','MO','63131-2913','ST. LOUIS COUNTY','UNITED STATES' UNION
SELECT '429cf905-34fd-4835-a259-900d22261b60','9297','','','','','','','' UNION
SELECT 'e52ba7f3-62ae-4cdb-8607-7524ff490031','17967','LEXINGTON PROPERTY MANAGEMENT, LLC','30 LEWIS ST','HARTFORD','CT','06103-2501','HARTFORD COUNTY','UNITED STATES' UNION
SELECT '80fd186f-267e-4be4-89f4-ebea22d512c0','1375339','LIFE TIME MIXED USE MANAGEMENT SERVICES LLC','2900 CORPORATE PL','CHANHASSEN','MN','55317-4560','CARVER COUNTY','UNITED STATES' UNION
SELECT '76d85b75-2988-41c6-838a-6b649b98d434','7466','LIFT DEVELOPMENT','625 N SPRING ST','MIDDLETOWN','PA','17057-1926','DAUPHIN COUNTY','UNITED STATES' UNION
SELECT 'fb2797f4-08bb-4e9f-bb06-c6459386804f','8193','LINDEMANN MULTIFAMILY MANAGEMENT, LLC','4500 BISCAYNE BLVD','MIAMI','FL','33137-3254','MIAMI-DADE COUNTY','UNITED STATES' UNION
SELECT '5b65333f-8188-4548-86ec-c2c40e47a015','5848','LINDY PROPERTY MGMT','309 YORK RD STE 211','JENKINTOWN','PA','19046-3270','MONTGOMERY COUNTY','UNITED STATES' UNION
SELECT '693f35a3-ad0f-497f-8a2c-879755839736','15052','LINK REAL ESTATE GROUP, LLC','5000 ARLINGTON CENTRE BLVD STE 2165','UPPER ARLINGTON','OH','43220','FRANKLIN COUNTY','UNITED STATES' UNION
SELECT '8378fa31-d821-4af1-bbed-98422dfb57e2','2611','','','','','','','' UNION
SELECT '0e94837f-4998-4eee-b78e-558a92435557','5745','LISA MANAGEMENT, INC.','826 BROADWAY 11TH FL','NEW YORK','NY','10003-4826','NEW YORK COUNTY','UNITED STATES' UNION
SELECT '65e8ba88-b0e3-48ec-8b4d-1e8596482ce6','8361','','','','','','','' UNION
SELECT '3a11bc7c-a2a5-475a-bb80-00d9c7dd6c35','2656','LIV COMMUNITIES LLC','8950 S 52ND ST STE 115','TEMPE','AZ','85284-1042','MARICOPA COUNTY','UNITED STATES' UNION
SELECT 'c7a82bc9-3ce3-4adf-974e-40344dc74c55','5302','LIVCOR, LLC','233 S WACKER DR STE 4200','CHICAGO','IL','60606-6310','COOK COUNTY','UNITED STATES' UNION
SELECT '0063262b-35ec-41e1-8644-41eb919eb20c','16538','LIVING REAL ESTATE GROUP LLC','225 W 35TH ST','NEW YORK','NY','10001-1904','NEW YORK COUNTY','UNITED STATES' UNION
SELECT 'c0fbf37e-4b3b-481f-925c-c7a3488a5df1','9394','LLV1, LP','18025 GLENVILLE CV','AUSTIN','TX','78738-7651','TRAVIS COUNTY','UNITED STATES' UNION
SELECT '7975a101-8dab-44bc-b575-6bcb48963993','1771','LMC LIVING, LLC','433 LAS COLINAS BLVD E STE 450','IRVING','TX','75039-5581','DALLAS COUNTY','UNITED STATES' UNION
SELECT 'fd4f6ae8-6484-4775-b3c2-be24cc73e4f7','6663','LOCKWOOD MANAGEMENT LLC','27777 FRANKLIN RD STE 1410','SOUTHFIELD','MI','48034-8209','OAKLAND COUNTY','UNITED STATES' UNION
SELECT 'a7c450ec-baad-4d78-8403-7d7eb53c7e2e','1029','','','','','','','' UNION
SELECT '1e13a73c-0ee4-4c1d-8f4d-1cd93993f3fb','6070','LOFTY ASSET MANAGEMENT INC.','4025 SUNBEAM RD','JACKSONVILLE','FL','32257-6025','DUVAL COUNTY','UNITED STATES' UNION
SELECT '222c1055-dc72-4d0d-b6c1-db3482b70f10','2070','LOGAN PROPERTY MANAGEMENT, INC.','1927 ADAMS AVE STE 200','SAN DIEGO','CA','92116-1211','SAN DIEGO COUNTY','UNITED STATES' UNION
SELECT 'f2ea6ee4-1eb1-4a82-a8f9-e5d987bb96c2','14876','LONG DRAGON PROPERTIES MGMT INC','2633 S BALDWIN AVE','ARCADIA','CA','91007-8325','LOS ANGELES COUNTY','UNITED STATES' UNION
SELECT '274e4297-8cb2-43b2-8495-d3f22abe10b9','8144','LONG REACH ASSOCIATES LIMITED AND LLC','2323 LONG REACH DR','SUGAR LAND','TX','77478-4190','FORT BEND COUNTY','UNITED STATES' UNION
SELECT 'd1050e05-fdd8-4f42-95cc-468839f8eb2a','1795','LONGBOAT PROPERTY MANAGEMENT, LLC','700 COCOANUT AVE','SARASOTA','FL','34236-4916','SARASOTA COUNTY','UNITED STATES' UNION
SELECT 'b682b7aa-8dd0-414e-97dc-4781dca95453','5942','LPC RESIDENT SERVICES LLC','2000 MCKINNEY AVE STE 1000','DALLAS','TX','75201-1858','DALLAS COUNTY','UNITED STATES' UNION
SELECT '9cbbdce7-11db-4d58-8edb-9f03102cd965','5335','LRC CAMPUS MANAGEMENT LLC','1585 FREDERICK BLVD','AKRON','OH','44320-4000','SUMMIT COUNTY','UNITED STATES' UNION
SELECT 'dc6a33a4-a432-40c2-9f98-d8456143b7ba','1380353','LUCERN LIVING, LLC','29 UNION AVE','LAKEHURST','NJ','08733-3023','OCEAN COUNTY','UNITED STATES' UNION
SELECT '437622c2-73d4-45b2-a121-e807fe32b6a9','1024','LUDWIG AND COMPANY','4081 RYAN RD STE 106','GURNEE','IL','60031-1267','LAKE COUNTY','UNITED STATES' UNION
SELECT '0af82cc1-80f6-4efd-b8db-a772ea809da6','7532','LURIN PROPERTY MANAGEMENT, LLC','2101 CEDAR SPRINGS RD STE 1250','DALLAS','TX','75201-1597','DALLAS COUNTY','UNITED STATES' UNION
SELECT '20a62fac-4e0a-4f6a-8936-8f72cd4708f4','3883','LUTHERAN SENIOR SERVICES','1150 HANLEY INDUSTRIAL CT','SAINT LOUIS','MO','63144-1910','ST. LOUIS COUNTY','UNITED STATES' UNION
SELECT '893b94c6-167b-4c5e-946d-c02577e76407','3875','LUTHERAN SENIOR SERVICES INC','1201 N HARRISON ST','WILMINGTON','DE','19806-3534','NEW CASTLE COUNTY','UNITED STATES' UNION
SELECT '3bfe91e8-7e1a-4746-b02c-4eff076eec61','1380221','LUXE PROPERTY MANAGEMENT, INC.','1769 E WALNUT ST UNIT 3018','PASADENA','CA','91106-1650','LOS ANGELES COUNTY','UNITED STATES' UNION
SELECT '0d6948d1-860a-47b7-abbb-d3f368afe80d','1002','LYON MANAGEMENT GROUP, INC. D/B/A LYON LIVING','4901 BIRCH ST','NEWPORT BEACH','CA','92660-2108','','UNITED STATES' UNION
SELECT '8012f6a6-0cc6-496b-9296-cdcd28c12b6b','22518','M & T PROPERTY MANAGEMENT','1910 FARMERVILLE HWY','RUSTON','LA','71270-3008','LINCOLN PARISH','UNITED STATES' UNION
SELECT '78f9a433-d27d-43eb-9225-685e489b8382','1036','M AND R MANAGEMENT CO INC','1501 AVENUE V','BROOKLYN','NY','11229-4432','','UNITED STATES' UNION
SELECT 'a66b7bf2-7a4d-4b44-9d81-12e8a62088f2','23397','M. OSTREICHER FAMILY PARTNERSHIP LLC','1500 S WATERFORD DR','FLORISSANT','MO','63033-6115','ST. LOUIS COUNTY','UNITED STATES' UNION
SELECT '345e451a-31a2-439f-86e9-011683877b37','974','MACDONALD PROPERTY MANAGEMENT, L.L.C','2951 FALL CREEK RD','KERRVILLE','TX','78028-9240','KERR COUNTY','UNITED STATES' UNION
SELECT '51c62838-c4d2-44cc-a337-a7780cd13143','3753','MACK MANAGEMENT','80 LONGFELLOW ST','WESTBROOK','ME','04092-2628','CUMBERLAND COUNTY','UNITED STATES' UNION
SELECT '4d75cdd3-937c-4747-9487-9d968f4f8839','19393','MACK PROPERTY MANAGEMENT LIMITED PARTNERSHIP','2415 E CAMELBACK RD STE 920','PHOENIX','AZ','85016-4288','MARICOPA COUNTY','UNITED STATES' UNION
SELECT '09b2777a-20da-4833-adfd-8233ddeb4c28','2645','MADISON APARTMENT GROUP LP','3843 W CHESTER PIKE  ELLIS PRESERVE','NEWTOWN SQUARE','PA','19073-2304','DELAWARE COUNTY','UNITED STATES' UNION
SELECT '8c63d61a-2ca4-4d2a-be9b-462dc18d01ac','14775','','','','','','','' UNION
SELECT '48f8a62b-f091-4f87-8d20-3af272e3dc3b','29081','MADISON MARQUETTE REAL ESTATE SERVICES LLC','1000 MAIN ST STE 2400','HOUSTON','TX','77002-6359','HARRIS COUNTY','UNITED STATES' UNION
SELECT 'eca168b4-894c-4f0a-b44c-e66609e68813','1044','MAESTRI-MURRELL PROPERTY MGMT LLC','9018 JEFFERSON HWY STE B','BATON ROUGE','LA','70809-2437','EAST BATON ROUGE PARISH','UNITED STATES' UNION
SELECT '976715ff-8d7e-4ca9-8342-1f0db2f36afb','1370805','','','','','','','' UNION
SELECT 'bc6727f9-392a-468d-a502-a9614c2adde9','4851','MAGELLAN PROPERTY MANAGEMENT LLC','225 N COLUMBUS DR STE 100','CHICAGO','IL','60601-7981','COOK COUNTY','UNITED STATES' UNION
SELECT '21afc608-ce0b-4239-bf1e-9c7b5415fb16','8353','MAGNOLIA CAPITAL MANAGEMENT COMPANY, INC.','1 N WACKER DR STE 1905','CHICAGO','IL','60606-2807','COOK COUNTY','UNITED STATES' UNION
SELECT 'c16071cf-346c-4d3c-8472-3f013c4822c9','5195','MAGNOLIA SPECIALIZED SVCS INC','1616 N VINE ST','MAGNOLIA','AR','71753-9740','COLUMBIA COUNTY','UNITED STATES' UNION
SELECT 'a6e995da-5f3a-4275-a004-bd8199d0dd68','1403','MAIN STREET COMMONS LLC','1103 W MAIN ST','PEORIA','IL','61606-1252','PEORIA COUNTY','UNITED STATES' UNION
SELECT '7844378f-fa4b-433a-9ed4-19b47a042b8f','1371510','MAINE DEVELOPMENT ASSOCIATES','43 HIGH ST','BANGOR','ME','04401-6146','PENOBSCOT COUNTY','UNITED STATES' UNION
SELECT '68877f99-e15b-41a0-87aa-27f0c7560ac2','1374496','MALON D. MIMMS COMPANY','85A MILL ST STE 100','ROSWELL','GA','30075-4979','FULTON COUNTY','UNITED STATES' UNION
SELECT '62f995db-be1f-4768-b09f-3541a2ce7d81','1048','MANAGEMENT AND MARKETING CONCEPTS,INC','2911 MIDDLE TENNESSEE BLVD','MURFREESBORO','TN','37130-2635','','United States' UNION
SELECT '30a597cf-cc67-49ff-a733-90644cefe1ec','5853','MANAGEMENT SERVICES CORP','780 MADISON AVE','CHARLOTTESVILLE','VA','22903-2103','CHARLOTTESVILLE CITY','UNITED STATES' UNION
SELECT '84f16b59-9d69-487a-a611-b8fa296d91a5','6344','MANAGEMENT SUPPORT','1800 E DEERE AVE','SANTA ANA','CA','92705-5721','ORANGE COUNTY','UNITED STATES' UNION
SELECT 'c0f44842-cd9a-4d3b-9b1a-479f557fe690','7024','MANDEL PROPERTY SERVICES, INC.','330 E KILBOURN AVE STE 600  SOUTH TOWER','MILWAUKEE','WI','53202-3144','MILWAUKEE COUNTY','UNITED STATES' UNION
SELECT '530d3d8c-cce5-49bf-af87-6c5171aa8e50','3903','MANGO SHADOW LLC','14020 N BLACK CANYON HWY','PHOENIX','AZ','85053-5800','MARICOPA COUNTY','United States' UNION
SELECT 'ef1b1880-3575-4f5e-95fb-38a70a4ede75','1380114','','','','','','','' UNION
SELECT '4d6ae0d1-a63a-4951-bc49-34802886f017','1374958','MANOR LIVING MANAGEMENT, LLC','845 15TH ST STE 103','SAN DIEGO','CA','92101-8098','SAN DIEGO COUNTY','UNITED STATES' UNION
SELECT '6bd95d76-080c-4d92-97f6-9570a50d35e7','3915','MANOR MANAGEMENT SERVICES','100 RIVIERA DR','HENDERSONVILLE','TN','37075-7414','SUMNER COUNTY','UNITED STATES' UNION
SELECT '9a16789e-f75e-48fb-a51c-10ac35880819','2569','MANOR MGMT SERVICES INC','1625 SE LAFAYETTE ST','PORTLAND','OR','97202-3862','MULTNOMAH COUNTY','UNITED STATES' UNION
SELECT 'b6ff5c19-b40a-4dac-b2e6-623592e52e10','14853','','','','','','','' UNION
SELECT '62e0ea01-f077-4e6e-9191-500b63cb749b','1380449','MAPLE LEAF HOUSING, INC.','30 UNO ST','NORTH BANGOR','NY','12966-2129','FRANKLIN COUNTY','UNITED STATES' UNION
SELECT 'b4245c94-4fb2-4ae1-82d8-38becab79ade','15202','','','','','','','' UNION
SELECT 'ea39c857-839b-4767-837c-4194b918402c','6820','MARCRUM MANAGEMENT COMPANY','200 CAHABA PARK CIR STE 210','BIRMINGHAM','AL','35242-8082','SHELBY COUNTY','UNITED STATES' UNION
SELECT '690b4f7a-3109-4e55-bc5a-361cdfc67bcb','6369','MARIMAN AND COMPANY','500 NEWPORT CENTER DR STE 200','NEWPORT BEACH','CA','92660-7001','ORANGE COUNTY','UNITED STATES' UNION
SELECT 'e33504ca-4645-423c-acb3-f35acf1dac61','4608','MARINA BAY MANAGEMENT SERVICES LLC','500 VICTORY RD','NORTH QUINCY','MA','02171-3139','NORFOLK COUNTY','UNITED STATES' UNION
SELECT 'fb0d538e-398a-4c8e-a709-7378382c9b5d','9324','MARK RUBIN','3200 MANGUM RD','HOUSTON','TX','77092-6738','HARRIS COUNTY','UNITED STATES' UNION
SELECT '3d5bf93f-bc94-4924-8db0-b24de73b2724','1399','MARK TAYLOR RESIDENTIAL','6623 N SCOTTSDALE RD','SCOTTSDALE','AZ','85250-4421','MARICOPA COUNTY','UNITED STATES' UNION
SELECT 'defb676b-c71f-459e-844e-fe78d0bcc5f3','3433','MARQUARDT MANAGEMENT SERVICES, INC.','1045 HILL ST','WATERTOWN','WI','53098-3015','DODGE COUNTY','UNITED STATES' UNION
SELECT '004831be-efda-4c10-a545-578998c805e3','665','MARQUETTE MGMT INC','135 WATER ST 3RD FL','NAPERVILLE','IL','60540-5338','DUPAGE COUNTY','UNITED STATES' UNION
SELECT '787b9bd4-4a4e-4d1d-ac93-c28e24d460f2','1350','MARSH PROPERTIES','2401 PARK RD','CHARLOTTE','NC','28203-5926','MECKLENBURG COUNTY','UNITED STATES' UNION
SELECT 'a1f1fb94-b59a-4d4f-8022-c02eab5a6e91','1379630','MARSHALL PARK INVESTMENTS LLC','11520 E CREEK RD','DARIEN','WI','53114-1111','ROCK COUNTY','UNITED STATES' UNION
SELECT '0edfb29b-d73e-4cb2-a12e-73fc646facf7','19977','MARTINAL MANAGEMENT CORPORATION','701 AVE PONCE DE LEON STE 309  EDIF. CENTRO DE SEGUROS','SAN JUAN','PR','00907-3247','SAN JUAN MUNICIPIO','UNITED STATES' UNION
SELECT 'e2d86c58-476d-4121-bb3f-240e54d38de9','3908','MARY LEE PROPERTY MANAGEMENT','1339 LAMAR SQUARE DR','AUSTIN','TX','78704-2209','TRAVIS COUNTY','UNITED STATES' UNION
SELECT 'fb331fb0-7a48-4659-98c1-fdb3db5da6cb','1643','','','','','','','' UNION
SELECT '3cac2203-4ee5-4f21-865e-8147c136cfd9','92','MATHER CAPITAL CORP','119 PICABO ST','KETCHUM','ID','83340','BLAINE COUNTY','UNITED STATES' UNION
SELECT '1fee418f-4863-42e7-98ea-d47e9ce71bc3','22576','MAURICE I. SCHWARTZ TOWERS INC.','90 NORTH ST','AUBURN','NY','13021-2669','CAYUGA COUNTY','UNITED STATES' UNION
SELECT 'd2f220a8-fc1e-4a34-b28b-c72a44d4d925','985','MAXIMUS REAL ESTATE PARTNERS, LLC','1 MARITIME PLZ STE 1900','SAN FRANCISCO','CA','94111-3509','SAN FRANCISCO COUNTY','UNITED STATES' UNION
SELECT '3c12cc6f-845b-4466-a317-f114df1186bc','6473','MAXUS PROPERTIES LLC','104 ARMOUR RD','NORTH KANSAS CITY','MO','64116-3503','','UNITED STATES' UNION
SELECT 'b278e47f-e479-406f-8d07-c24b40232f81','6073','MAXX PROPERTIES','600 MAMARONECK AVE','HARRISON','NY','10528-1635','WESTCHESTER COUNTY','UNITED STATES' UNION
SELECT '21355e9a-fdf7-4211-9585-a744fd43cf06','1225','MBP TEXAS, LLC DBA CATALYST MULTIFAMILY','2460 W 26TH AVE STE 355C','DENVER','CO','80211-5334','DENVER COUNTY','UNITED STATES' UNION
SELECT '9e0ae963-6e02-4f84-895f-4cc82d474fdb','1051','MBS PROPERTY MANAGEMENT','3950 INDUSTRIAL BLVD STE 100','WEST SACRAMENTO','CA','95691-6508','YOLO COUNTY','UNITED STATES' UNION
SELECT '027b94d2-0827-469d-8129-4796337d839b','22661','MCCALEB MANAGEMENT, LLC','2412 CLIO ST','NEW ORLEANS','LA','70113-2146','ORLEANS PARISH','UNITED STATES' UNION
SELECT 'a2eab377-7f9c-4500-852b-ba8b28e680bb','1061','MCCALL RESIDENTIAL, LLC.','531 S MAIN ST','GREENVILLE','SC','29601-2500','GREENVILLE COUNTY','UNITED STATES' UNION
SELECT 'cac9fd8b-0c5b-41e2-85d6-5fe85390b31a','6350','MCDOUGAL PROPERTY MANAGEMENT, L.C.','6800 PARK TEN BLVD STE 184W','SAN ANTONIO','TX','78213-4204','BEXAR COUNTY','UNITED STATES' UNION
SELECT 'ab7b1597-19b6-416f-b94e-6bc9a8f43b6f','6589','MCR PROPERTY MANAGEMENT INC','2319 WHITNEY AVE STE 1A','HAMDEN','CT','06518-3534','NEW HAVEN COUNTY','UNITED STATES' UNION
SELECT '940fb42a-aaed-4435-98d5-b504ab7e54dd','21110','MDG PROPERTIES LLC','3691 E 69TH AVE','MERRILLVILLE','IN','46410-3961','LAKE COUNTY','UNITED STATES' UNION
SELECT '43d95e1a-61b6-4d2c-8cd8-0eb8d3454cb8','6422','MERCURY METROPLEX REAL ESTATE','1307 SYLVAN CT','ARLINGTON','TX','76012-2400','TARRANT COUNTY','UNITED STATES' UNION
SELECT 'da2ce0e8-393f-4d19-aa59-6829f5952afe','5383','MERCY HOUSING, INC','1600 BROADWAY STE 2000','DENVER','CO','80202-4929','DENVER COUNTY','UNITED STATES' UNION
SELECT 'f627f3b6-a9fc-484a-b31f-5d6eb92febf6','1009','MERGE MANAGEMENT','270 N DENTON TAP RD STE 100','COPPELL','TX','75019-2133','DALLAS COUNTY','UNITED STATES' UNION
SELECT '6962def0-b8b1-47ae-9579-cb6b8de37f13','7760','MERIDIAN HOUSING PROPERTIES, LLC','501 CAMBRIDGE CT','ALPHARETTA','GA','30005-4216','FULTON COUNTY','UNITED STATES' UNION
SELECT '6ab7f78f-7621-44a0-8a25-6c5ab664eb3c','6012','MERIDIAN RESIDENTIAL GROUP, LLC','125 REGIONAL PKWY','ORANGEBURG','SC','29118-8607','ORANGEBURG COUNTY','UNITED STATES' UNION
SELECT 'a1e9a8c8-0d5a-4e19-9526-23c58668db8c','20791','MGP IX SAC II PROPERTIES, LLC','425 CALIFORNIA ST 10TH FL','SAN FRANCISCO','CA','94104-2102','SAN FRANCISCO COUNTY','UNITED STATES' UNION
SELECT 'a2689485-d4d5-4280-96db-aba958576266','1374691','MERRILL AREA HOUSING AUTHORITY','701 E 1ST ST','MERRILL','WI','54452-2402','LINCOLN COUNTY','UNITED STATES' UNION
SELECT '41544bb3-38b0-44bc-ac25-47fbe0b73da3','1379024','','','','','','','' UNION
SELECT '132c7ec1-0da0-4c82-a6db-60b1ef80625f','18614','MESIROW FINANCIAL INVESTMENT MANAGEMENT, INC','353 N CLARK ST','CHICAGO','IL','60654-4704','COOK COUNTY','UNITED STATES' UNION
SELECT 'bd3f3488-d427-4e64-b415-749f2c2613a0','1371315','METONIC REAL ESTATE SOLUTIONS LLC','12149 W CENTER RD','OMAHA','NE','68144-3955','DOUGLAS COUNTY','UNITED STATES' UNION
SELECT 'bcdc08d0-6d0f-4dfa-8b48-89a807f3da78','6918','METROPOLITAN MANAGEMENT CORPORATION','4111 E MADISON ST # 490','SEATTLE','WA','98112-3241','KING COUNTY','UNITED STATES' UNION
SELECT '9355cf31-2234-4d8c-8a31-5be5069642f7','93','METROPOLITAN PROPERTY MANAGEMENT, INC.','327 N MARKET ST','WASHINGTON','NC','27889-4933','BEAUFORT COUNTY','UNITED STATES' UNION
SELECT 'e57953c5-6088-4133-a793-8c40dcb40355','1377922','MGIK MANAGEMENT INC','1500 PAERDEGAT AVE N','BROOKLYN','NY','11236-4100','KINGS COUNTY','UNITED STATES' UNION
SELECT '57bee467-101e-42df-bda7-3e19e426a6fd','5921','MGM ENTERPRISES DBA THE APARTMENT GALLERY','1 WATERFORD PROFESSIONAL CTR','YORK','PA','17402-9280','YORK COUNTY','UNITED STATES' UNION
SELECT '9f419eca-40a0-4029-ba9b-876ee56db379','7947','MHANY MANAGEMENT INC','470 VANDERBILT AVE 9TH FL','BROOKLYN','NY','11238-2208','KINGS COUNTY','UNITED STATES' UNION
SELECT '282ad0b3-6ed5-4776-a843-81c7bf838ff5','1375589','','','','','','','' UNION
SELECT '4e910f20-ced7-4165-ab40-591f5d6c9203','8237','MICHAELS MANAGEMENT-AFFORDABLE, LLC','2 COOPER ST 14TH FL','CAMDEN','NJ','08102-2348','CAMDEN COUNTY','UNITED STATES' UNION
SELECT '7c701b32-57e2-4b07-97a1-41d47e7baeb8','1379993','','','','','','','' UNION
SELECT 'dd0b5133-799b-41f9-ba40-b8a9992300ef','1227','MID AMERICA MANAGEMENT CORPORATION','3333 RICHMOND RD STE 350','BEACHWOOD','OH','44122-4196','CUYAHOGA COUNTY','UNITED STATES' UNION
SELECT 'acb12598-9d44-4b25-815c-7eb62260d284','5786','','','','','','','' UNION
SELECT 'c811c53c-35f7-400e-8e13-9216691df6b7','7193','MID-AMERICA APARTMENTS, L.P.','6815 POPLAR AVE STE 500','GERMANTOWN','TN','38138-3606','SHELBY COUNTY','UNITED STATES' UNION
SELECT '425be319-4630-4cdd-b74a-fd23750b6d2a','5728','MIDDLEBURG MANAGEMENT LLC','1921 GALLOWS RD STE 700','VIENNA','VA','22182-3994','FAIRFAX COUNTY','UNITED STATES' UNION
SELECT 'da4e22ee-fabe-4e3c-9902-24e907ffaca7','20222','MIDDLETON MEYERS MANAGEMENT, LLC','11111 CARMEL COMMONS BLVD STE 100','CHARLOTTE','NC','28226-4492','MECKLENBURG COUNTY','UNITED STATES' UNION
SELECT '06844920-4011-4c2a-b443-68ee730fd12e','1067','MIDPEN PROPERTY MANAGEMENT CORP','303 VINTAGE PARK DR STE 250','FOSTER CITY','CA','94404-1135','SAN MATEO COUNTY','UNITED STATES' UNION
SELECT 'c3d8c173-480e-4b5a-a15b-5e89345e8469','5534','MILES REALTY CO INC','2401 BLUERIDGE AVE STE 308','WHEATON','MD','20902-4517','MONTGOMERY COUNTY','UNITED STATES' UNION
SELECT '4c684169-8544-4d26-a530-bd56512b3a8f','1379131','MILESTONE INVESTMENTS, LLC','8333 DOUGLAS AVE STE 1600','DALLAS','TX','75225-5882','DALLAS COUNTY','UNITED STATES' UNION
SELECT 'a0cef16b-ac55-4374-b5f3-5301765c268e','6155','','','','','','','' UNION
SELECT '6737987a-7885-4e6f-a64e-8974c6c62337','7198','MILHAUS MANAGEMENT LLC','460 VIRGINIA AVE','INDIANAPOLIS','IN','46203-1700','MARION COUNTY','UNITED STATES' UNION
SELECT '43bc4767-39fc-4b68-a0e4-a91f41305a6f','22631','MILL STREET VILLAGE LLC','186 MILL ST','ATHENS','OH','45701-2609','ATHENS COUNTY','UNITED STATES' UNION
SELECT '2b3d0504-76c2-4917-b459-ab9838f64f86','6583','MILLENNIA HOUSING MANAGEMENT','127 PUBLIC SQ  1300 KEY TOWER','CLEVELAND','OH','44114-1217','CUYAHOGA COUNTY','UNITED STATES' UNION
SELECT '6e3bad8c-3663-4249-9a09-f8d97e9e1579','8541','MILLENNIAL APARTMENTS, LLC','2930 COMMERCE ST','DALLAS','TX','75226-1609','DALLAS COUNTY','UNITED STATES' UNION
SELECT '696ad369-21f0-46c3-b5ed-e32cd1c28493','125508','MILLER INVESTMENT COMPANY, LLC','10850 E TRAVERSE HWY STE 5595','TRAVERSE CITY','MI','49684-1325','GRAND TRAVERSE COUNTY','UNITED STATES' UNION
SELECT 'd7b89bef-e7e8-435e-bb67-367197ec2f69','1379994','','','','','','','' UNION
SELECT 'd74f25fd-884b-47ad-8338-dd3bc1e662a8','4510','MILMO INVESTMENTS','514 EL PASO ST','SAN ANTONIO','TX','78207-5006','BEXAR COUNTY','UNITED STATES' UNION
SELECT 'a8465d31-665d-45f6-8487-cea7a77293e9','6565','MINKIN MANAGEMENT INC.','5511 W NATIONAL AVE # 221A','MILWAUKEE','WI','53214-3414','MILWAUKEE COUNTY','UNITED STATES' UNION
SELECT 'fb12a83c-adbc-4fe3-9cdc-ebbd78a590ba','1985','MIRACLE MILE REALTY GROUP','1455 E 27TH ST','BROOKLYN','NY','11210-5308','KINGS COUNTY','UNITED STATES' UNION
SELECT '55d50936-ceb7-409d-b859-13e338cc3fd9','6003','MISSION ROCK RESIDENTIAL, LLC','1355 S COLORADO BLVD STE C710','DENVER','CO','80222-3305','DENVER COUNTY','UNITED STATES' UNION
SELECT '6f8b8be5-9c8b-4892-a590-fab8f80f31a6','1379465','','','','','','','' UNION
SELECT '014a385f-6aca-4b6b-b7e7-664dbb698d12','1379995','','','','','','','' UNION
SELECT 'ba959054-d813-4f33-a5f0-3e9f64f8a44a','7923','MKF MANAGEMENT LLC','950 3RD AVE 28TH FL','NEW YORK','NY','10022-2705','NEW YORK COUNTY','UNITED STATES' UNION
SELECT 'e9adf2c0-89af-489c-b1e0-b21bd5f0cf9b','5460','MMM HOUSING','21059 BLAIR RD','CONROE','TX','77385-7309','','UNITED STATES' UNION
SELECT 'c743f9ef-9dfa-4feb-8cce-616e9fa6aa2a','125','MONARCH MGMT AND REALTY INC','3003 E 98TH ST STE 141','INDIANAPOLIS','IN','46280-1973','HAMILTON COUNTY','UNITED STATES' UNION
SELECT '359040fd-001b-4f62-849b-88cffb2ff819','4487','MONARCH ON THE MERRIMACK LLC','210 MERRIMACK ST','LAWRENCE','MA','01843-1772','ESSEX COUNTY','UNITED STATES' UNION
SELECT 'f299e7a3-7ab1-494d-874f-33fec6f5907a','6265','MONARCH PROPERTIES, INC.','1720 LOUISIANA BLVD NE STE 402','ALBUQUERQUE','NM','87110-7020','BERNALILLO COUNTY','UNITED STATES' UNION
SELECT '0d9a82c5-ef82-4af0-8073-c251efaea48d','1379648','','','','','','','' UNION
SELECT 'c8845124-0c2b-4b81-a0b5-df6ab4670097','15161','MONTERREY ASSET MANAGEMENT, LTD.','9615 SIMS DR','EL PASO','TX','79925-7243','EL PASO COUNTY','UNITED STATES' UNION
SELECT 'db4fbffc-bfd9-481a-afb1-8080e166769d','5251','MONTFORD MANAGEMENT','2591 DALLAS PKWY STE 300','FRISCO','TX','75034-8563','COLLIN COUNTY','UNITED STATES' UNION
SELECT 'eb90a03b-3782-4a8c-8da4-783552be4b05','5567','MONTICELLO ASSET MGMT, INC','12750 MERIT DR STE 800','DALLAS','TX','75251-1251','DALLAS COUNTY','UNITED STATES' UNION
SELECT '6763ba14-17d8-4377-86f4-5b84ccdf7932','9024','MONUMENT PROPERTY MANAGEMENT, LLC','11777 KATY FWY STE 567  SOUTH ATRIUM','HOUSTON','TX','77079-1703','HARRIS COUNTY','UNITED STATES' UNION
SELECT '14ca19b5-57e5-4ab6-99ac-665808a46a37','868','MONUMENT REAL ESTATE SERVICES','5200 BLUE LAGOON DR STE 400','MIAMI','FL','33126-7001','MIAMI-DADE COUNTY','UNITED STATES' UNION
SELECT 'f9d4878c-3fda-45c7-859b-e2023e1470b7','15421','MOORE ASSET MANAGEMENT','211 E SHEPHERD AVE STE 202','LUFKIN','TX','75901-3062','ANGELINA COUNTY','UNITED STATES' UNION
SELECT '5dbfe024-5fec-40a4-a0fa-84739c327e74','9306','','','','','','','' UNION
SELECT '360e46fc-06ff-44f2-9851-ec47523f2d26','6312','MORGAN GROUP PROPERTY MANAGEMENT, LLC','5606 S RICE AVE','HOUSTON','TX','77081-2118','HARRIS COUNTY','UNITED STATES' UNION
SELECT '0606790f-243a-4933-9dd8-737a4b704f4f','5858','MORGAN PROPERTIES MANAGEMENT COMPANY LLC','160 CLUBHOUSE RD STE 9910','KING OF PRUSSIA','PA','19406-3307','','UNITED STATES' UNION
SELECT 'fbf2d6e1-0d33-4af3-b855-5ea286b1bc38','7937','MORRIS CORAL SPRINGS ASSOCIATES LLC','10890 W SAMPLE RD','CORAL SPRINGS','FL','33065-2632','BROWARD COUNTY','UNITED STATES' UNION
SELECT '0fdd4028-9a51-4d29-9137-5a1595faba8f','5783','MORRISON EKRE AND BART MANAGEMENT SERVICES, INC','11201 N TATUM BLVD STE 260','PHOENIX','AZ','85028-6044','MARICOPA COUNTY','UNITED STATES' UNION
SELECT '605a1f12-22a1-4948-9330-fa4262032458','1261','MOSAIC RESIDENTIAL INC','15021 KATY FWY STE 580','HOUSTON','TX','77094-1900','HARRIS COUNTY','UNITED STATES' UNION
SELECT '20ba5bb9-b8d7-4e71-832b-268bed1551fb','3049','MOSS & COMPANY','15300 VENTURA BLVD STE 405','SHERMAN OAKS','CA','91403-5856','LOS ANGELES COUNTY','UNITED STATES' UNION
SELECT 'e9920df7-1a61-4884-bb84-350d7631e885','3955','MOTHER OF MERCY','230 CHURCH AVE','ALBANY','MN','56307','','UNITED STATES' UNION
SELECT '792173d0-f202-4d70-8c7e-3fab2896c8c9','26408','MOUNTAINVIEW MANAGMENT NJ LLC','701 CROSS ST STE 105','LAKEWOOD','NJ','08701-4029','OCEAN COUNTY','UNITED STATES' UNION
SELECT '3d616df3-908d-46c8-8cfd-4f74bff2f832','6582','MPC MAGNOLIA PROPERTY MANAGEMENT, LLC','2435 E SOUTHLAKE BLVD STE 150','SOUTHLAKE','TX','76092-6681','TARRANT COUNTY','UNITED STATES' UNION
SELECT '3fba5337-0e0a-4357-abe7-933accfc6877','1062','MPMS, INC','1225 N GROVE ST','ANAHEIM','CA','92806-2114','ORANGE COUNTY','UNITED STATES' UNION
SELECT 'ed979334-d646-48ad-bf0e-1f970f45a151','3210','MRC DEVELOPMENT, LLC','637 HIGHWAY 51 STE J','RIDGELAND','MS','39157-2144','MADISON COUNTY','UNITED STATES' UNION
SELECT 'c411c62f-8d24-4a8d-896e-d5080c9e8948','5712','MRM RESIDENTIAL HOLDINGS, LLC DBA MERION RESIDENTIAL','300 E LANCASTER AVE STE 310','WYNNEWOOD','PA','19096-2106','MONTGOMERY COUNTY','UNITED STATES' UNION
SELECT 'aea35e7e-9b02-430b-addd-b18481775bd4','95','MSC PROPERTIES LLC','6660 MABLETON PKWY SE','MABLETON','GA','30126-5302','','UNITED STATES' UNION
SELECT '0d9fcac1-d3f2-4af1-bca6-6c4b536b7775','8963','MSCO','16550 VANOWEN ST','VAN NUYS','CA','91406-4756','LOS ANGELES COUNTY','UNITED STATES' UNION
SELECT '1cc46f93-36df-4f25-b9b3-5869823e1f30','14741','MSR MANAGEMENT INC','8000 N ARMENIA AVE','TAMPA','FL','33604-2758','HILLSBOROUGH COUNTY','UNITED STATES' UNION
SELECT '7fd3514b-3021-49d8-a046-ae98a386f996','24388','MT. CARMEL HOMES, INC.','372 N LINCOLN ST','DAYTONA BEACH','FL','32114-3088','VOLUSIA COUNTY','UNITED STATES' UNION
SELECT '0cf92408-522e-4285-add4-81194c19dd67','5810','MUESING MANAGEMENT CO.','3755 E 82ND ST STE 350','INDIANAPOLIS','IN','46240-7335','MARION COUNTY','UNITED STATES' UNION
SELECT '58e39f03-240d-44fb-a842-91b3ed694889','15000','MULLOY COMMERCIAL REAL ESTATE','8303 SHELBYVILLE RD','LOUISVILLE','KY','40222-5544','JEFFERSON COUNTY','UNITED STATES' UNION
SELECT 'c038e8fa-d482-42f7-90c6-e2ecf139e93a','901','STONE CREEK LIVING','398 ENCHANTED PKWY','MANCHESTER','MO','63021-5501','ST. LOUIS COUNTY','UNITED STATES' UNION
SELECT 'cf3e87e7-7e87-4bb2-b01c-114bbae8f3d5','1076','MULTIFAMILY MANAGEMENT SERVICES, LLC','4 EXECUTIVE BLVD STE 100','SUFFERN','NY','10901-4176','ROCKLAND COUNTY','UNITED STATES' UNION
SELECT '90393848-e7db-4010-a173-9b0ed0844ba4','5825','MULTIFAMILY MANAGEMENT, INC','758 SAINT MICHAEL ST','MOBILE','AL','36602-1327','','UNITED STATES' UNION
SELECT '78ac4480-72cf-43fd-9e78-790ecd1b899b','6479','MULTIFAMILY SELECT INC','300 E JOHN ST STE 138','MATTHEWS','NC','28105-4939','MECKLENBURG COUNTY','UNITED STATES' UNION
SELECT '66d8cabe-db15-48cf-a8a3-4d45b5f47d9d','5632','MUTUAL HOUSING ASSOCIATION OF GREATER HARTFORD, INC','95 NILES ST','HARTFORD','CT','06105-2305','HARTFORD COUNTY','United States' UNION
SELECT '92169451-4033-4e01-8b9f-8e71fcbfe8e5','1372079','MUTUAL HOUSING ASSOCIATION OF SOUTHWESTERN CONNECTICUT, INC.','1235 HUNTINGTON TPKE','TRUMBULL','CT','06611-5362','FAIRFIELD COUNTY','UNITED STATES' UNION
SELECT '6371fc3d-6866-4d77-9fc0-afe778979622','11404','MUTUAL PROPERTY MANAGEMENT, LLC','6 PARKLANE BLVD STE 545','DEARBORN','MI','48126-2618','WAYNE COUNTY','UNITED STATES' UNION
SELECT 'bc1eb5d5-34f7-4f18-b86b-3157de01d576','1371927','MVHP, LLC','950 N WASHINGTON ST STE 212','ALEXANDRIA','VA','22314-1534','ALEXANDRIA CITY','UNITED STATES' UNION
SELECT '118e053e-e88a-4436-862f-180a103ba69a','6185','MW HOLDINGS, LLC','3708 ALLIANCE DR','GREENSBORO','NC','27407-2016','GUILFORD COUNTY','UNITED STATES' UNION
SELECT '39645e92-61b5-4931-b1df-7c0612cb6bb7','5964','MYAN MANAGEMENT GROUP, LLC','520 SILICON DR','SOUTHLAKE','TX','76092-9162','TARRANT COUNTY','UNITED STATES' UNION
SELECT '49799b65-e457-410f-b3ba-8198d4d5852e','1092','NATIONAL DEVELOPMENT','2310 WASHINGTON ST','NEWTON LOWER FALLS','MA','02462-1449','MIDDLESEX COUNTY','UNITED STATES' UNION
SELECT 'b7830577-7a49-4436-93da-d427d51dac00','7553','NATIONAL MANAGEMENT CORP DBA TRUVERSE MANAGEMENT','12289 STRATFORD DR','CLIVE','IA','50325-8167','POLK COUNTY','UNITED STATES' UNION
SELECT '6e58309c-ce2c-4bb4-8ffd-fced34c50680','22838','NATIONAL PRESERVATION PROPERTY MANAGEMENT CO','500 S GRAND AVE 22ND FL','LOS ANGELES','CA','90071-2609','LOS ANGELES COUNTY','UNITED STATES' UNION
SELECT '86582d93-8e8a-433d-bb47-727470b2e673','7559','NATIONS PREMIER PROPERTIES LLC','2201 LAKESIDE BLVD','RICHARDSON','TX','75082-4305','DALLAS COUNTY','UNITED STATES' UNION
SELECT '42b274e3-7098-42cc-a685-ae92a9a1c430','24041','','','','','','','' UNION
SELECT '1a212e7e-768d-4b78-9d0d-8ecb453c6334','2453','NATIONWIDE HOUSING MANAGEMENT LLC','601 WHITE HORSE PIKE','HADDON TOWNSHIP','NJ','08107-1269','','UNITED STATES' UNION
SELECT 'aa8c01ea-d4eb-44bb-b284-b2b5c8099201','6038','NATIONWIDE REALTY MANAGEMENT, LLC','375 N FRONT ST STE 200','COLUMBUS','OH','43215-2258','FRANKLIN COUNTY','UNITED STATES' UNION
SELECT '096f85a8-f300-4c39-baeb-574a3c127ecf','1852','NAVARINO PROPERTY MANAGEMENT','1023 MAIN ST 2ND FL','BRIDGEPORT','CT','06604-4221','FAIRFIELD COUNTY','UNITED STATES' UNION
SELECT '822529b3-3711-46a1-a68d-0d854d8df372','2086','NCM MANAGEMENT LTD','2656 BRIDGEWAY STE 201','SAUSALITO','CA','94965-1400','MARIN COUNTY','UNITED STATES' UNION
SELECT '4af22a8a-c1bf-4a9a-a386-a7c2be559eea','329','ND & S MANAGEMENT COMPANY, LLC','1425 S 18TH ST','SAINT LOUIS','MO','63104-2501','ST. LOUIS CITY','UNITED STATES' UNION
SELECT '4d70bbba-faf9-4eaa-989a-ff6fc13d6586','6242','NDC REAL ESTATE MANAGEMENT','437 GRANT ST','PITTSBURGH','PA','15219-6002','ALLEGHENY COUNTY','UNITED STATES' UNION
SELECT '4091525a-1214-4847-9cdd-fbc5c2a8d0ca','5245','NDI MANAGEMENT, LLC','1421 OREAD WEST ST STE B','LAWRENCE','KS','66049-3844','DOUGLAS COUNTY','UNITED STATES' UNION
SELECT '705db516-8eed-453b-bbd9-aa8cc8aa8f57','1356','NE PROPERTY MANAGEMENT, LLC','116 HILLSIDE DR STE 100','LEWISVILLE','TX','75057-3175','DENTON COUNTY','UNITED STATES' UNION
SELECT '29cc324b-eec6-4ced-9c7c-8f46a46cd5b2','20852','NEIGHBORHOOD PROPERTY MANAGEMENT INC','720 W HAWES AVE','FRESNO','CA','93706-2830','FRESNO COUNTY','UNITED STATES' UNION
SELECT '7ade28cb-daf8-47d1-b4a7-d02af0ab4ef1','897','NEIGHBORWORKS NEW HORIZONS','235 GRAND AVE','NEW HAVEN','CT','06513-3722','NEW HAVEN COUNTY','UNITED STATES' UNION
SELECT 'd13ab960-ba2b-4671-9094-b5b544ee1028','6468','NELSON AND ASSOCIATES, INC.','5181 NATORP BLVD STE 120','MASON','OH','45040-7987','WARREN COUNTY','UNITED STATES' UNION
SELECT '0f490f8d-7eb7-45b5-a167-6f0af66550f1','4985','NELSON-MINAHAN REALTORS, INC','2611 LIBAL ST','GREEN BAY','WI','54301-2865','','UNITED STATES' UNION
SELECT 'bab39994-ad7b-45f9-8cfd-6acef2445af0','1379996','','','','','','','' UNION
SELECT '4ac736d3-fdeb-4d7d-a5db-92082c425606','6644','NEVINS ADAMS LEWBEL SCHELL','920 GARDEN ST STE A','SANTA BARBARA','CA','93101-7465','SANTA BARBARA COUNTY','UNITED STATES' UNION
SELECT 'ca7bae81-505b-45b2-8739-3cbafe1c1420','14776','NEW CITY MANAGEMENT','2940 E BROADWAY RD','MESA','AZ','85204-1751','MARICOPA COUNTY','UNITED STATES' UNION
SELECT '1a7fb4a5-f2ac-4ce7-b104-356407fcd299','6112','NEW COLUMBIA RESIDENTIAL PROPERTY MANAGEMENT, LLC','1718 PEACHTREE ST NW STE 684','ATLANTA','GA','30309-2405','','UNITED STATES' UNION
SELECT '79014e7a-cf11-4ba1-b7b2-de28d201da23','8414','','','','','','','' UNION
SELECT '5642bcb7-1758-4c93-b0ba-a6c2893a8877','476','NEW HAVEN JEWISH COMMUNITY COUNCIL HOUSING CORP.','18 TOWER LN','NEW HAVEN','CT','06519-1764','NEW HAVEN COUNTY','UNITED STATES' UNION
SELECT 'a060e382-0e9d-40cc-aca2-1d24e2517370','1379351','NEW HORIZONS HEALTHY LIFESTYLES, LLC','317 N CENTRAL AVE','SUPERIOR','NE','68978-1737','NUCKOLLS COUNTY','UNITED STATES' UNION
SELECT '8f4b2814-92b3-4793-a2f1-b9ae68b9c723','9289','NEW NEIGHBORHOODS, INC.','76 PROGRESS DR STE 140','STAMFORD','CT','06902-3696','FAIRFIELD COUNTY','UNITED STATES' UNION
SELECT 'f90d5095-4e5b-481a-99db-acaab1ad3d7b','1380481','NEW YORK BUILDING SOLUTIONS, LLC','80 GUION PL L5','NEW ROCHELLE','NY','10801','WESTCHESTER COUNTY','UNITED STATES' UNION
SELECT '21bfe5f9-dc4a-41d5-b637-4f36d30697cb','2261','NEWBURY MANAGEMENT COMP','3408 WOODLAND AVE STE 504','WEST DES MOINES','IA','50266-6505','POLK COUNTY','UNITED STATES' UNION
SELECT '86d2ca93-4d0c-4b56-82a6-227f91cab40f','20626','NEWCASTLE LTD','150 N MICHIGAN AVE STE 3610','CHICAGO','IL','60601-7569','COOK COUNTY','UNITED STATES' UNION
SELECT '4d2814b4-f0d7-4f2e-b144-57aaa3393657','6198','NFI APARTMENT MANAGEMENT, LLC','7031 W 97TH TER','OVERLAND PARK','KS','66212-1505','JOHNSON COUNTY','UNITED STATES' UNION
SELECT '03586e9f-519c-42a1-b4d1-018cd0791981','5346','NHE INC','5 LEGACY PARK RD STE A','GREENVILLE','SC','29607-3941','GREENVILLE COUNTY','UNITED STATES' UNION
SELECT 'cba5958d-f2c5-4a75-a743-f739976d0445','3021','NHE MANAGEMENT ASSOC., LLC','90-11 160TH ST STE 100','JAMAICA','NY','11432','','UNITED STATES' UNION
SELECT '771f059b-24d7-4c43-b6a3-456642859d36','1369547','NHJUSTIN, LLC','14302 FNB PKWY','OMAHA','NE','68154-5212','DOUGLAS COUNTY','UNITED STATES' UNION
SELECT '6e5b26b3-16eb-4a89-afb0-1d750ec1e04c','5795','NLR PROPERTY MANAGEMENT LLC','380 FRANKLIN PARK UNIT 104','HARBOR SPRINGS','MI','49740-8669','EMMET COUNTY','UNITED STATES' UNION
SELECT 'fe3ba18f-22a8-4f9e-a6e4-8bffb5fd77e1','6190','NMI PROPERTY MANAGEMENT','7921 JONES BRANCH DR STE 250','MCLEAN','VA','22102-3353','FAIRFAX COUNTY','UNITED STATES' UNION
SELECT 'c74269c4-2177-4590-be34-6c185df659e1','34','NOBLE REALTY ADVISORS, LLC','1707 N CHARLES ST STE 200','BALTIMORE','MD','21201-5829','BALTIMORE CITY','UNITED STATES' UNION
SELECT '48364041-1bc8-4c18-abdf-9b0a476c3a65','7604','NOEL MANAGEMENT COMPANY','1605 LYNDON B JOHNSON FWY STE 250','DALLAS','TX','75234-6802','DALLAS COUNTY','UNITED STATES' UNION
SELECT '8b8b2e91-88d3-43d3-aef5-b86906e80a20','1352','NOI PROPERTY MANAGEMENT LLC','9950 CYPRESSWOOD DR STE 150','HOUSTON','TX','77070-3413','HARRIS COUNTY','UNITED STATES' UNION
SELECT '1111d82f-bd1d-48ea-86a5-7698d87d77b7','5987','NOLAN REAL ESTATE SERVICES','2020 W 89TH ST STE 320','LEAWOOD','KS','66206-1946','JOHNSON COUNTY','UNITED STATES' UNION
SELECT 'c686d70e-fee4-4a80-8485-352cfbb84485','20973','NORRFOSS MANAGEMENT, LLC','5637 FORBES AVE','PITTSBURGH','PA','15217-1574','','UNITED STATES' UNION
SELECT '82c265bc-258f-4469-907e-5994c0c7876c','1109','NORTH AMERICAN PROPERTIES','212 E 3RD ST STE 300','CINCINNATI','OH','45202-5500','HAMILTON COUNTY','UNITED STATES' UNION
SELECT '5ad7248a-caf8-48dd-a870-962e5e41f622','6654','NORTH BAY GROUP LLC','2129 GENERAL BOOTH BLVD STE 103-189','VIRGINIA BEACH','VA','23454-5899','VIRGINIA BEACH CITY','UNITED STATES' UNION
SELECT 'dda49aa5-bb37-480b-bd54-460be680eef8','5957','NORTHLAND INVESTMENT CORPORATION','2150 WASHINGTON ST','NEWTON','MA','02462-1498','MIDDLESEX COUNTY','UNITED STATES' UNION
SELECT '134a0c06-1b5c-4b09-8c34-92da0a9f2a0c','4878','NORTHPOINT DEVELOPMENT, LLC','4825 NW 41ST ST STE 500','RIVERSIDE','MO','64150-7806','PLATTE COUNTY','UNITED STATES' UNION
SELECT '9ccda7d5-027e-4fa2-8b29-fc62725f4f49','1082','NORTHRIDGE PROPERTIES LLC','1551 28TH AVE S','GRAND FORKS','ND','58201-6782','GRAND FORKS COUNTY','UNITED STATES' UNION
SELECT 'db9fabdb-be97-48fa-94a5-9335d1c89b5f','1378550','NORTHSTAR EQUITY SERVICES, LLC','80 S 8TH ST','MINNEAPOLIS','MN','55402-2100','HENNEPIN COUNTY','UNITED STATES' UNION
SELECT 'b98cb629-9bfe-4146-ad4f-1264cfdedf02','797','NORTHSTAR RESIDENTIAL','12708 WAYZATA BLVD STE 450','MINNETONKA','MN','55305-1942','HENNEPIN COUNTY','UNITED STATES' UNION
SELECT 'fce6a0fb-c6a2-430d-a9d7-5bea7d5683cb','6376','NOVA PROPERTY MANAGEMENT','1200 BLALOCK RD STE 210','HOUSTON','TX','77055-6441','HARRIS COUNTY','UNITED STATES' UNION
SELECT '2c64222e-3013-49c6-b807-310b2ee08d2b','871','NWR MANAGEMENT, LLC','558 E STONEWALL ST STE 120','CHARLOTTE','NC','28202-3396','MECKLENBURG COUNTY','UNITED STATES' UNION
SELECT 'f6e1efb3-ec6a-496b-a090-f4c041ec5a7c','26419','O.C.E.A.N. INC.','40 WASHINGTON ST','TOMS RIVER','NJ','08753-7669','OCEAN COUNTY','UNITED STATES' UNION
SELECT 'ef7a4bb1-528a-46ab-aa08-9547ce8b6616','6682','OAKMONT MANAGEMENT GROUP','9240 OLD REDWOOD HWY STE 200','WINDSOR','CA','95492-9282','SONOMA COUNTY','UNITED STATES' UNION
SELECT '0fa912cb-b725-4878-b3e5-d9633b3fb565','1804','OAKRIDGE NEIGHBORHOOD ASSOCIATES LP','1401 CENTER ST','DES MOINES','IA','50314-2285','POLK COUNTY','UNITED STATES' UNION
SELECT 'bbea457c-619c-40af-a565-5f23ad2ac8ab','6577','OAKS PROPERTIES LLC','3550 E 46TH ST APT 120','MINNEAPOLIS','MN','55406-3965','HENNEPIN COUNTY','UNITED STATES' UNION
SELECT 'da04f0c9-27c0-41c5-88ad-e1c0f529129e','6788','OAKWOOD APARTMENT','1 WORLD TRADE CTR 24TH FL','LONG BEACH','CA','90831-0002','LOS ANGELES COUNTY','UNITED STATES' UNION
SELECT 'c466acfe-3d33-4215-a4c3-9d553f4e8b0b','1379997','','','','','','','' UNION
SELECT 'ab3080fe-6141-4768-b93d-b78b53e30186','1349','ODEN HUGHES, LLC','901 S MO PAC EXPY STE 200','AUSTIN','TX','78746-5759','TRAVIS COUNTY','UNITED STATES' UNION
SELECT 'a705261f-ef6c-4eee-a3a1-b8b282aeb9e7','9212','OHIO CITY','1452 ASHFORD GLEN LN','SAGAMORE HILLS','OH','44067-1682','SUMMIT COUNTY','UNITED STATES' UNION
SELECT '9d779ed0-064a-4966-99d2-cdcf5ea86079','1374495','OHM PROPERTY MANAGEMENT, LLC DBA THEORY PROPERTY MANAGEMENT','6509 VALLEY FORGE DR','ROWLETT','TX','75089-5106','DALLAS COUNTY','UNITED STATES' UNION
SELECT 'fe48aace-3584-42ba-8965-85fb8de62918','3977','OLD DUTCHTOWN APARTMENT LLC','7543 N JEFFERSON PLACE CIR APT A','BATON ROUGE','LA','70809-8620','EAST BATON ROUGE PARISH','UNITED STATES' UNION
SELECT '5a237871-d6c3-4ec1-aca7-98d2aa671b5b','5859','OLDHAM GOODWIN GROUP LLC','2800 S TEXAS AVE STE 401','BRYAN','TX','77802-5361','BRAZOS COUNTY','UNITED STATES' UNION
SELECT '4119ea3f-361d-4e33-b0b3-9601ba6190aa','1367772','ONE PROPERTY MANAGEMENT LLC','3330 SOUTHGATE CT SW STE 100','CEDAR RAPIDS','IA','52404-5416','LINN COUNTY','UNITED STATES' UNION
SELECT 'e4ae36f6-2410-480c-a760-b2c9e48b5fad','5344','ONE STREET RESIDENTIAL SERVICES, LLC','2000 RIVEREDGE PKWY STE 450','ATLANTA','GA','30328-4659','FULTON COUNTY','UNITED STATES' UNION
SELECT '349b3b32-6f1b-428e-b07e-b2c3cc44da89','22061','ONNI GROUP','5055 N 32ND ST STE 200','PHOENIX','AZ','85018-1401','MARICOPA COUNTY','UNITED STATES' UNION
SELECT 'bf814d9f-c125-45e0-8586-13b62aa8de8c','23256','','','','','','','' UNION
SELECT '861131c3-af29-4ae1-84a0-bbe293d7ad23','1379998','','','','','','','' UNION
SELECT 'a6d1fd19-b3fc-42b4-ad71-d425a567c551','6456','ORBACH AFFORDABLE MANAGEMENT LLC','980 SYLVAN AVE','ENGLEWOOD CLIFFS','NJ','07632-3301','BERGEN COUNTY','UNITED STATES' UNION
SELECT 'c1fc7847-228b-431a-b5f8-dec3d560dc05','5807','ORION OPERATING CORPORATION','2930 N SHARTEL AVE','OKLAHOMA CITY','OK','73103-1034','OKLAHOMA COUNTY','UNITED STATES' UNION
SELECT 'f2c18cbf-c884-4939-89f4-2440549aa261','5842','ORION REAL ESTATE','2051 GREENHOUSE RD STE 300','HOUSTON','TX','77084-7859','HARRIS COUNTY','UNITED STATES' UNION
SELECT 'c30272e0-2ae1-475a-836a-dfc9f4ea248d','6203','ORION RESIDENTIAL MANAGEMENT LLC','770 LAKE COOK RD STE 350','DEERFIELD','IL','60015-4940','LAKE COUNTY','UNITED STATES' UNION
SELECT 'cf12fd27-dbac-417a-ae78-3c7d005c513f','8325','ORISON MANAGEMENT, LLC','240 E 15TH ST','EDMOND','OK','73013-5049','OKLAHOMA COUNTY','UNITED STATES' UNION
SELECT 'a73ff042-113f-416c-98d9-58e6510736da','22557','OSD PROPERTY MANAGEMENT INC','875 WYOMING ST STE 101','MISSOULA','MT','59801-1788','MISSOULA COUNTY','UNITED STATES' UNION
SELECT 'd05eea91-092c-45ae-89d3-85c8363b3030','1376130','OSPM LLC','2144 AGLER RD','COLUMBUS','OH','43224-4586','FRANKLIN COUNTY','UNITED STATES' UNION
SELECT '97a967c2-ecf7-4e53-a662-68d3f711326d','7616','OTEYS REAL ESTATE AGENCY, INC','2701 JEFFERSON ST STE 205','NASHVILLE','TN','37208-2865','DAVIDSON COUNTY','UNITED STATES' UNION
SELECT '02c16523-9ff4-467e-9eda-5fd9efe94907','1772','OVATION PROPERTY MANAGEMENT','6021 S FORT APACHE RD STE 100','LAS VEGAS','NV','89148-5562','CLARK COUNTY','UNITED STATES' UNION
SELECT '96575725-9583-412b-8fda-43fad1bb0846','4047','OVER THE RAINBOW ASSOCIATION','2040 BROWN AVE','EVANSTON','IL','60201-3373','COOK COUNTY','UNITED STATES' UNION
SELECT 'f47e5669-1845-46e6-9661-4aa01f181fe9','1371200','OWNERS AGENCY, LLC','8713 CREST LN','FORT MYERS','FL','33907-4205','LEE COUNTY','UNITED STATES' UNION
SELECT '2381d953-9d6e-4dc7-8cdc-9b608bf9bb5c','6678','OWNERS MANAGEMENT COMPANY','25250 ROCKSIDE RD','BEDFORD HEIGHTS','OH','44146-1900','CUYAHOGA COUNTY','UNITED STATES' UNION
SELECT 'b41b3f4e-6dfa-4aae-b237-67feb929925b','1374685','OXFORD PARTNERS','2900 CHARLEVOIX DR SE STE 160','GRAND RAPIDS','MI','49546-7085','KENT COUNTY','UNITED STATES' UNION
SELECT 'a3eef627-e6d5-4014-84f3-eeb52ce8b802','693','P.F.C. ENTERPRISES, INC. DBA ALLEN PROPERTIES','25531 COMMERCENTRE DR STE 150','LAKE FOREST','CA','92630-8891','ORANGE COUNTY','UNITED STATES' UNION
SELECT 'c53971b5-5a34-422a-976c-73790bc4fc75','1152','PA MANAGEMENT AND MNTC CORP','1275 DRUMMERS LN STE 220','WAYNE','PA','19087-1571','CHESTER COUNTY','UNITED STATES' UNION
SELECT 'ebfec86b-a87a-43b2-9a3d-dd76812565bd','6498','PAC CARVEOUT, LLC','3284 NORTHSIDE PKWY NW STE 150','ATLANTA','GA','30327-2282','FULTON COUNTY','UNITED STATES' UNION
SELECT '151a2984-c7f4-417c-9312-6a819750dff1','125351','PAC HOUSING GROUP, LLC','65 GERMANTOWN CT STE 409','CORDOVA','TN','38018-4258','SHELBY COUNTY','UNITED STATES' UNION
SELECT 'b085ddb8-72c1-41cd-a12d-66a17327a6d3','6736','PACE REALTY','5429 LYNDON B JOHNSON FWY STE 500','DALLAS','TX','75240-2607','DALLAS COUNTY','UNITED STATES' UNION
SELECT 'eca79470-735e-46b7-a6cb-d50c320b08de','2999','PACIFIC CREST REAL ESTATE, LLC','200 1ST AVE W STE 520','SEATTLE','WA','98119-4298','KING COUNTY','UNITED STATES' UNION
SELECT 'f1e2c8b3-b116-400b-bdbc-71dc4627f847','8224','PACIFIC CREST REALTY','11911 SAN VICENTE BLVD STE 265','LOS ANGELES','CA','90049-6634','LOS ANGELES COUNTY','UNITED STATES' UNION
SELECT 'aa06f0ab-eb97-4d74-b3ac-e11dbd7d1022','15050','','','','','','','' UNION
SELECT '4baf3290-ade6-4d86-a1c7-b961f74cb2a1','1027','PACIFIC NORTHWEST PROPERTY MANAGEMENT, LLC','10580 SW MCDONALD ST STE 204','KING CITY','OR','97224-4816','WASHINGTON COUNTY','UNITED STATES' UNION
SELECT 'b1252ac8-84f8-427b-ae67-0a65d6c7df67','479','PACIFIC PROPERTIES MANAGEMENT','333 S HOPE ST','LOS ANGELES','CA','90071-1406','LOS ANGELES COUNTY','UNITED STATES' UNION
SELECT 'ca8c4140-15c9-4743-91d4-1aa7975a21d1','5447','PACIFIC WESTERN CORPORATION','9948 HIBERT ST STE 210','SAN DIEGO','CA','92131-1034','SAN DIEGO COUNTY','UNITED STATES' UNION
SELECT '2414b8a2-0d21-46d4-8e2f-3f8aff5a24b2','8874','PACIFICA KESSLER LLC','2522 FORT WORTH AVE','DALLAS','TX','75211-1703','DALLAS COUNTY','UNITED STATES' UNION
SELECT '4c7d8430-e6b2-4c6a-83a7-f7dceeb02fb9','6386','PACIFICAP PROPERTIES GROUP','412 NW 5TH AVE STE 200','PORTLAND','OR','97209-3816','MULTNOMAH COUNTY','UNITED STATES' UNION
SELECT 'a9e1d3e7-f906-48e9-accd-52592e12a0e4','2157','PALLADIAN MANAGEMENT LLC','1071 POST RD E STE 205','WESTPORT','CT','06880-5361','FAIRFIELD COUNTY','UNITED STATES' UNION
SELECT '7f77e473-6666-4289-8544-b9e4373426ee','26694','PALM COMMUNITIES','301 W PLATT ST # 425','TAMPA','FL','33606-2292','HILLSBOROUGH COUNTY','UNITED STATES' UNION
SELECT '31d42b96-1efa-4f18-b56a-3c2f0d701ee1','480','PALM GARDENS RENTAL APARTMENTS, LLC','19098 NW 57TH AVE','HIALEAH','FL','33015-5014','MIAMI-DADE COUNTY','UNITED STATES' UNION
SELECT '706ac4fc-ab5a-432b-b4a8-dd4410153b67','4538','PALOMAR PROPERTIES LLC','12001 BELCHER RD S','LARGO','FL','33773-5000','PINELLAS COUNTY','UNITED STATES' UNION
SELECT 'd3dc60ff-3d77-4c56-a7b0-484479c1bcaa','14684','PAMA MANAGEMENT','4900 SANTA ANITA AVE STE 2C','EL MONTE','CA','91731-1490','LOS ANGELES COUNTY','UNITED STATES' UNION
SELECT '1e8264a5-9755-482f-9b52-90b0957e2c67','24300','PANADIUM MANAGEMENT LLC','9997 ALLISONVILLE RD','FISHERS','IN','46038-2074','HAMILTON COUNTY','UNITED STATES' UNION
SELECT '6b68223a-b32d-4d22-9257-c0ad8d3f4909','1374','PANCO MANAGEMENT OF NEW JERSEY LLC','395 W PASSAIC ST STE 251','ROCHELLE PARK','NJ','07662-3016','BERGEN COUNTY','UNITED STATES' UNION
SELECT '841d773a-5f1c-4d64-b95a-c5a7794ec96f','2291','PANGEA VENTURES, LLC','549 W RANDOLPH ST 2ND FL','CHICAGO','IL','60661-2208','COOK COUNTY','UNITED STATES' UNION
SELECT 'c2b8bf0e-40d3-4376-8dd7-41dc4d2cafcc','9390','PANORAMA TOWER MANAGEMENT LLC','100 S BISCAYNE BLVD STE 900','MIAMI','FL','33131-2031','MIAMI-DADE COUNTY','UNITED STATES' UNION
SELECT '1a288050-f86a-42cf-a142-a996234636f7','6164','PANTHER RESIDENTIAL MANAGEMENT LLC','300 TRADECENTER STE 7700','WOBURN','MA','01801-7419','MIDDLESEX COUNTY','UNITED STATES' UNION
SELECT 'b6a7ebbe-2db8-48d4-8ef0-716c192b8b79','1614','','','','','','','' UNION
SELECT '7a79ae51-e52f-4a9a-b377-5d3aadabd912','8322','PARAGON MANAGEMENT LLC','120 ALBANY ST STE 800','NEW BRUNSWICK','NJ','08901-2069','MIDDLESEX COUNTY','UNITED STATES' UNION
SELECT '0a02d367-ad62-4c5b-b265-7798660493ea','1253','PARAM MANAGEMENT, LLC','1637 S MICHIGAN AVE','VILLA PARK','IL','60181-4101','DUPAGE COUNTY','UNITED STATES' UNION
SELECT '28e72b97-5a36-477f-af6a-6f11ee04e00e','5958','PARAWEST COMMUNITY DEVELOPMENT, LLC DBA PARAWEST MANAGEMENT','7975 N HAYDEN RD STE D263','SCOTTSDALE','AZ','85258-3246','MARICOPA COUNTY','UNITED STATES' UNION
SELECT 'c186f052-bf94-43dc-a6ef-4a2678c308f6','2215','PARC COMMUNITIES, LLC','3350 RIVERWOOD PKWY SE STE 1580','ATLANTA','GA','30339-3358','COBB COUNTY','UNITED STATES' UNION
SELECT 'fc6112b2-ab33-493d-91d8-6a55a6c1f45e','5813','PAREDIM PARTNERS','45 KNOLLWOOD RD STE 305','ELMSFORD','NY','10523-2833','WESTCHESTER COUNTY','UNITED STATES' UNION
SELECT '6f6ef1a8-5913-49cb-9117-ccfbe2eac3c2','8309','PARK GROVE REALTY, LLC','46 PRINCE ST STE 2003','ROCHESTER','NY','14607-1023','MONROE COUNTY','UNITED STATES' UNION
SELECT '0678026c-ef42-4090-9ea9-00dfe0e2302a','5317','PARKVIEW MANAGEMENT CORP','5101 PARK AVE STE 1  1ST FLOOR MGMT OFFICE','WEST NEW YORK','NJ','07093-2510','HUDSON COUNTY','UNITED STATES' UNION
SELECT '137b4bf4-c9d4-444a-86e1-c0b9be3cf0de','4065','BESSE, CARMONA, AND PARTIN PC','9831 WHITHORN DR','HOUSTON','TX','77095-5027','HARRIS COUNTY','UNITED STATES' UNION
SELECT 'cb4e58f7-ded6-490a-8eb3-65ccd60f85cb','19573','PARTNERSHIP FOR URBAN HOUSING DEVELOPMENT, INCORPORATION','1700 W SUSQUEHANNA AVE','PHILADELPHIA','PA','19121-1640','PHILADELPHIA COUNTY','UNITED STATES' UNION
SELECT '8f8ca035-3ca1-47ca-98e2-2edb1f7f6da7','8597','','','','','','','' UNION
SELECT '26a5ddbf-6ad3-4df5-9ca7-d9bcc30ac7cc','1199','PATRICIAN MANAGEMENT LLC','8027 JEFFERSON HWY','BATON ROUGE','LA','70809-7651','EAST BATON ROUGE PARISH','UNITED STATES' UNION
SELECT '99ced1fa-608b-4ecb-94fa-1824b03a003a','8914','','','','','','','' UNION
SELECT 'a646246d-825c-4d22-ad44-a1dfe148835c','652','PAULS CAPITAL, LLC DBA PAULSCORP, LLC','100 SAINT PAUL ST STE 300','DENVER','CO','80206-5136','DENVER COUNTY','UNITED STATES' UNION
SELECT '1679bc15-7057-4e3f-9142-ba14b2ee4308','5805','PEABODY PROPERTIES','536 GRANITE ST','BRAINTREE','MA','02184-3952','NORFOLK COUNTY','UNITED STATES' UNION
SELECT '336561a8-af9f-4391-8f1c-72f84ee08527','6919','PEAK LIVING LLC','2174 W GROVE PKWY STE 100','PLEASANT GROVE','UT','84062-6737','UTAH COUNTY','UNITED STATES' UNION
SELECT '4d6917e5-fc5b-4dc1-9817-e8dad2c04309','5551','PEAK REAL ESTATE MANAGEMENT LLC','16107 KENSINGTON DR # 120','SUGAR LAND','TX','77479-4224','FORT BEND COUNTY','UNITED STATES' UNION
SELECT '22fce62c-8618-4395-a749-db683dc4c0ac','5992','','','','','','','' UNION
SELECT 'cf1fac30-0562-49c4-9410-fbb8919bbdb8','6076','PEGASUS RESIDENTIAL LLC','1750 FOUNDERS PKWY STE 180','ALPHARETTA','GA','30009-7603','FULTON COUNTY','UNITED STATES' UNION
SELECT '5f3a1aea-338c-408d-a9ef-f9508e5d65ff','1007','PEM REAL ESTATE GROUP','14822 N 73RD ST STE 101','SCOTTSDALE','AZ','85260-3142','MARICOPA COUNTY','UNITED STATES' UNION
SELECT 'cf3e2046-baf8-4378-96a0-191087267cc5','1379999','','','','','','','' UNION
SELECT 'd558997f-8311-43cc-8bdd-fbdbf55db0f3','1378081','PEOPLE INCORPORATED OF VIRGINIA','1173 W MAIN ST','ABINGDON','VA','24210-4703','WASHINGTON COUNTY','UNITED STATES' UNION
SELECT '174c2a77-9f61-4b39-8625-94101f0cf33a','14969','PEREGRINE PLACE APARTMENTS LLC','4100 PRESERVE PKWY N','GREENWOOD VILLAGE','CO','80121-3948','ARAPAHOE COUNTY','UNITED STATES' UNION
SELECT '0eec7e30-4091-4d3e-8d27-f3cacba85857','6193','PERFORMANCE PROPERTIES, LLC','4144 N CENTRAL EXPY STE 1150','DALLAS','TX','75204-2107','DALLAS COUNTY','UNITED STATES' UNION
SELECT '3ad37de4-6d2f-4bd9-b7d9-e0f10f20b191','9727','PERRY GUEST MANAGEMENT LLC','2517 THOMAS AVE','DALLAS','TX','75201-2039','DALLAS COUNTY','UNITED STATES' UNION
SELECT 'e6d3442a-7fb7-4fc3-8f49-c3a402d0dddd','1136','PERRY REID','9200 ANDERMATT DR','LINCOLN','NE','68526-9639','LANCASTER COUNTY','UNITED STATES' UNION
SELECT 'c5b3be4a-be5e-40d1-acf4-5dd24c555f23','5746','PETERSON GROUP MANAGEMENT CORP','7340 GALLAGHER DR','EDINA','MN','55435-4503','HENNEPIN COUNTY','UNITED STATES' UNION
SELECT 'cf035f1f-525a-4c38-9be4-edf0416abb04','6221','PETTINARO MANAGEMENT, LLC','234 N JAMES ST','NEWPORT','DE','19804-3132','NEW CASTLE COUNTY','UNITED STATES' UNION
SELECT '8939afad-fa0e-4295-9487-45afc5b75a48','14878','PFALZGRAF ASSET MANAGEMENT INC.','5417 ALBEMARLE RD','CHARLOTTE','NC','28212-3608','MECKLENBURG COUNTY','UNITED STATES' UNION
SELECT '721e1991-8f5b-4d78-ab65-1f2aaf3dd0b3','22671','PHASE HOUSING MANAGEMENT, LLC','4920 ATLANTA HWY STE 345','ALPHARETTA','GA','30004-2921','FORSYTH COUNTY','UNITED STATES' UNION
SELECT '941d3b9a-cd2c-4d2c-ae19-a9f097d03f02','1367678','PHILLIPS DEVELOPMENT CORP DBA PDC COMPANIES','1501 N UNIVERSITY AVE STE 740','LITTLE ROCK','AR','72207-5236','PULASKI COUNTY','UNITED STATES' UNION
SELECT 'da1bcee9-ac38-478e-a25d-e3d7dd42cd18','6122','PHILLIPS MANAGEMENT GROUP','1400 BATTLEGROUND AVE STE 201','GREENSBORO','NC','27408-8028','GUILFORD COUNTY','UNITED STATES' UNION
SELECT 'a83dc227-f72a-4097-8ad1-853f59e2fe7d','6353','PHOENIX PROPERTY MANAGEMENT','1038 W SOUTHERN AVE','TEMPE','AZ','85282-4514','MARICOPA COUNTY','UNITED STATES' UNION
SELECT '3b8b9b4a-1fa1-4d64-bcd2-2882f6687877','20777','PIERCE CONTRACTING INC','461 N ORANGE AVE','SARASOTA','FL','34236-5003','SARASOTA COUNTY','UNITED STATES' UNION
SELECT '7fe2d2e6-9c19-4e42-9729-6031559007be','5885','PILLAR INCOME ASSET MANAGEMENT','1603 LYNDON B JOHNSON FWY','DALLAS','TX','75234-6034','DALLAS COUNTY','UNITED STATES' UNION
SELECT 'd7dd0bd9-d69a-4e02-ae8d-f7b639f9fac8','8795','PINK STONE CAPITAL GROUP LLC','54 THOMPSON ST 5TH FL','NEW YORK','NY','10012-4308','NEW YORK COUNTY','UNITED STATES' UNION
SELECT 'acadd528-2e03-4853-b506-48d6ded6b825','4509','PINNACLE INTERNATIONAL DEVELOPMENT, INC.','424 15TH ST STE 100','SAN DIEGO','CA','92101-7525','SAN DIEGO COUNTY','UNITED STATES' UNION
SELECT '9061f79d-642b-48d5-8b21-3b4321c66a32','6958','PINNACLE PROPERTY MANAGEMENT SERVICES, LLC','5055 KELLER SPRINGS RD STE 400','ADDISON','TX','75001-6208','DALLAS COUNTY','UNITED STATES' UNION
SELECT '10f5c218-b1cd-4a43-b213-8eb82be796e7','22562','PIONEER PROPERTY MANAGEMENT, INC.','65 N ELM ST','PLATTEVILLE','WI','53818-2542','GRANT COUNTY','UNITED STATES' UNION
SELECT 'af974283-7f54-4bfe-8dcb-0f017c3adf1d','992','PK MANAGEMENT, LLC','15301 VENTURA BLVD STE B570','SHERMAN OAKS','CA','91403-6650','LOS ANGELES COUNTY','UNITED STATES' UNION
SELECT 'c0b8285b-5f2a-40e0-b4ff-7b50b13d172a','1130','PLATINUM MANAGEMENT SERVICES','1547 E 8TH ST','BROOKLYN','NY','11230-7005','KINGS COUNTY','UNITED STATES' UNION
SELECT '19483edf-1350-4585-9e23-5609739324e0','5603','','','','','','','' UNION
SELECT '33bdcfc2-b514-4e12-83aa-d295c5199db6','2638','PM RESIDENTIAL MANAGEMENT LLC','26361 CROWN VALLEY PKWY STE 210','MISSION VIEJO','CA','92691-6305','ORANGE COUNTY','UNITED STATES' UNION
SELECT '94ecb719-3125-4b5c-b78f-06b73571af77','7813','POINT REYES RESIDENTIAL, LLC','8787 HAMMERLY BLVD','HOUSTON','TX','77080-6644','HARRIS COUNTY','UNITED STATES' UNION
SELECT '39d1c7e3-3f79-45e9-b192-7b2394776dc1','7812','POINTE MANAGEMENT CORPORATION','7777 GOLDEN TRIANGLE DR','EDEN PRAIRIE','MN','55344-3736','HENNEPIN COUNTY','UNITED STATES' UNION
SELECT '84c6b1a2-db98-4225-997b-0198faa432fd','7811','','','','','','','' UNION
SELECT '6912cfcf-fed4-449c-97a8-2178c7439c24','8575','POMAJEST LLC','6064 LUCKETT CT','EL PASO','TX','79932-1806','EL PASO COUNTY','UNITED STATES' UNION
SELECT '4774a398-e799-426f-9d4c-af74810a9c8e','5483','PORTICO PROPERTY MANAGEMENT LLC','6119 GREENVILLE AVE STE 134','DALLAS','TX','75206-1910','DALLAS COUNTY','UNITED STATES' UNION
SELECT 'ae67a22a-a072-4c85-9634-4add887ea33f','15012','POSTGRADUATE CENTER FOR MENTAL HEALTH','158 E 35TH ST','NEW YORK','NY','10016-4102','NEW YORK COUNTY','UNITED STATES' UNION
SELECT '5b4c3d81-9930-4ca4-9ec2-5eabe799d79f','1370747','POWER PLAY PARTNERS, LLC','5823 TRANSIT RD STE 8','DEPEW','NY','14043-2800','ERIE COUNTY','UNITED STATES' UNION
SELECT '7e53784a-5554-442a-a18a-eb78ff2e52e0','193','','','','','','','' UNION
SELECT 'bc0f96b0-a382-44f3-9750-afa1d28c3c97','25275','PRAXM MANAGEMENT LLC','8335 KEYSTONE XING STE 220','INDIANAPOLIS','IN','46240-2695','MARION COUNTY','UNITED STATES' UNION
SELECT 'b2f5854f-d866-4166-9980-36dbd25649c7','14798','','','','','','','' UNION
SELECT '82c13fc9-c940-4dcd-ba8c-07b58da1bdb6','8190','PREMIER REALTY','544 NEWTOWN RD STE 128','VIRGINIA BEACH','VA','23462-5603','VIRGINIA BEACH CITY','UNITED STATES' UNION
SELECT 'a664ed13-34b0-4ffe-8114-74002fc08d71','5256','PRESBYTERIAN HOMES AND HOUSING FOUNDATION','1050 BURLINGTON AVE N','SAINT PETERSBURG','FL','33705-1537','PINELLAS COUNTY','UNITED STATES' UNION
SELECT 'adf47241-9c8b-4971-b7fc-f39698899dd0','2917','PRESBYTERIAN VILLAGES OF MICHIGAN','26200 LAHSER RD STE 300','SOUTHFIELD','MI','48033-7157','OAKLAND COUNTY','UNITED STATES' UNION
SELECT '51d8e334-a52d-4613-9ffc-7d96b94d09e7','6459','PRESERVATION MGMT INC','261 GORHAM RD','PORTLAND','ME','04106-2408','CUMBERLAND COUNTY','UNITED STATES' UNION
SELECT 'e9c5d8dc-a850-4a5d-8b39-7ce024b44b07','6547','PRICE REALTY CORPORATION','4125 CENTURION WAY # 200','ADDISON','TX','75001-4347','DALLAS COUNTY','UNITED STATES' UNION
SELECT 'd05af69b-b3a2-4187-a5c6-f24c628960ad','6210','PRIME ADMINISTRATION LLC DBA PRIME GROUP','321 S BURNSIDE AVE','LOS ANGELES','CA','90036-3269','LOS ANGELES COUNTY','UNITED STATES' UNION
SELECT '2c4e9b74-82e7-4ed2-8cfc-4c1b33c91694','345','PRIME COMPANIES INC','621 COLUMBIA STREET EXT','COHOES','NY','12047-3876','ALBANY COUNTY','UNITED STATES' UNION
SELECT '45531882-a5c4-4089-b37c-21b077ed6ec4','3650','PRIME REAL ESTATE, LLC','1 ELMCROFT RD STE 500','STAMFORD','CT','6902','FAIRFIELD COUNTY','UNITED STATES' UNION
SELECT 'ab68504a-0994-425c-9e57-f5a35f20b1df','19014','HARKINS BUILDERS INC','10490 LITTLE PATUXENT PKWY STE 400','COLUMBIA','MD','21044-4929','HOWARD COUNTY','UNITED STATES' UNION
SELECT '2c78d81e-5d11-41f8-a996-4327ef6784e1','6195','PRINCE PARTNERS, INC.','600 WHISPERING HILLS DR','NASHVILLE','TN','37211-5234','DAVIDSON COUNTY','UNITED STATES' UNION
SELECT '5689a875-f214-4852-af46-0d11e5a8d0d5','1976','PRINCETON PROPERTY MANAGEMENT INC','7831 SE LAKE RD STE 200','PORTLAND','OR','97267-2193','CLACKAMAS COUNTY','UNITED STATES' UNION
SELECT '2f3ded83-c501-4e53-9e14-61e63bdc0249','1790','PRINCIPAL MANAGEMENT GROUP','12700 PARK CENTRAL DR STE 600','DALLAS','TX','75251-1537','DALLAS COUNTY','UNITED STATES' UNION
SELECT '38e04086-eedd-4c53-bf51-eff4a9753d97','6026','PRISM REAL ESTATE SERVICES, LLC','8826 SANTA FE DR STE 300','OVERLAND PARK','KS','66212-3672','JOHNSON COUNTY','UNITED STATES' UNION
SELECT '55e9cfc6-a911-42d4-b354-28c5679ea6c9','122','','','','','','','' UNION
SELECT '7404fefe-a722-439d-856d-e41a210f49f8','598','PROFESSIONAL MANAGEMENT, INC.','9095 SW 87TH AVE STE 777','MIAMI','FL','33176-2310','MIAMI-DADE COUNTY','UNITED STATES' UNION
SELECT '58586202-1245-4f89-8ae1-0281ba24a400','1380026','','','','','','','' UNION
SELECT '3dd52557-28bf-4dad-9ae4-2f327df4d686','4077','PROGRESS OF PEOPLES MANAGEMENT','191 JORALEMON ST 8TH FL','BROOKLYN','NY','11201-4306','KINGS COUNTY','UNITED STATES' UNION
SELECT 'a5df704a-8fc6-4d7d-a4de-72167b442954','2456','PROJECT LIVE, INC','465-475 BROADWAY','NEWARK','NJ','07104-4177','ESSEX COUNTY','UNITED STATES' UNION
SELECT '6d534ca0-b170-4596-8ae8-1a8b00b6b240','1379249','','','','','','','' UNION
SELECT 'd942f0a6-2d02-4700-a6e1-5b02fa735ab7','6161','PROMETHEUS REAL ESTATE GROUP, INC.','1900 S NORFOLK ST STE 150','SAN MATEO','CA','94403-1161','SAN MATEO COUNTY','UNITED STATES' UNION
SELECT '75ad6a0c-c0fb-4125-953b-12fbe08e9517','50','PROMEX MIDWEST MGMT INC DEV','800 S MILWAUKEE AVE STE 170','LIBERTYVILLE','IL','60048-3200','LAKE COUNTY','UNITED STATES' UNION
SELECT '17f486ab-4dac-47c7-985c-0675baac19c5','1375627','PROPER MANAGEMENT','6050 HOLLIS ST','EMERYVILLE','CA','94608-2030','ALAMEDA COUNTY','UNITED STATES' UNION
SELECT 'd35f9ba5-a88b-4f98-9ba3-0a5c837e45fa','487','PROPERTIES UNLIMITED - GRB PARTNERS','15335 MORRISON ST STE 203','SHERMAN OAKS','CA','91403-6714','LOS ANGELES COUNTY','UNITED STATES' UNION
SELECT 'df21e216-735c-495a-9607-c8f202a4cd72','6723','PROPERTY INVESTMENT MANAGEMENT INC','6 S SUMMIT ST','FAIRHOPE','AL','36532-2332','BALDWIN COUNTY','United States' UNION
SELECT 'f6577ede-ec0c-48b6-9833-b7bc64e3a8ef','6077','','','','','','','' UNION
SELECT 'f0e1eb6d-6283-44f2-8f27-a63b2d0dded6','1380001','','','','','','','' UNION
SELECT 'd048f63a-e5bc-46dd-a53d-72f5e0a7ac80','1374927','PROPERTY SERVICES GROUP LLC','7200 WISCONSIN AVE STE 700','BETHESDA','MD','20814-4891','MONTGOMERY COUNTY','UNITED STATES' UNION
SELECT '45509954-97a9-490b-bd44-a9a6b9019aa6','7905','PROPERTY TECHNICIANS, INC. DBA AS UNITED PROPERTY MANAGEMENT COMPANY','300 BROADWAY STE 202','LORAIN','OH','44052-1639','LORAIN COUNTY','UNITED STATES' UNION
SELECT '7c02c354-0164-4cc2-b849-f95375d044ed','6354','PROVENCE REAL ESTATE, LLC','3300 NORTHEAST EXPY NE BLDG 6','ATLANTA','GA','30341-3937','DEKALB COUNTY','UNITED STATES' UNION
SELECT '92412b0f-2ef7-4944-a393-5c3865a305ba','1191','PRYZANT MGMT CO','6060 GULFTON ST','HOUSTON','TX','77081-2401','HARRIS COUNTY','UNITED STATES' UNION
SELECT '1590e503-0720-455a-a19f-b1b623a5e467','1377705','PSC SIGNAL HILL, LLC','1930 NEW HAMPSHIREAVENUE, NW #14','WASHINGTON','DC','20009','DISTRICT OF COLUMBIA','UNITED STATES' UNION
SELECT 'd6d49988-aee6-43fe-85eb-fe7d5c31c2d1','24288','','','','','','','' UNION
SELECT 'fdb30a4c-9710-4fec-aaa3-9b59a82f81e1','5533','PURE US APARTMENTS REIT INC.','5810 TENNYSON PKWY STE 450','PLANO','TX','75024','','UNITED STATES' UNION
SELECT 'f3ebd377-197d-4315-a42e-44e41dd08e56','27344','PV ASSET MANAGEMENT, INC.','1505 RUSTICWOOD DR','DESOTO','TX','75115-7775','DALLAS COUNTY','UNITED STATES' UNION
SELECT 'ea3285c0-2913-47ee-8e68-fb36229bf948','346','PYRAMID DEVELOPMENT LLC','1300 VIRGINIA DR STE 215','FORT WASHINGTON','PA','19034-3249','MONTGOMERY COUNTY','UNITED STATES' UNION
SELECT 'fc62a2c1-26c7-4cab-8fc3-9b7b92adc7bd','8943','QEPROP','3600 W T C JESTER BLVD','HOUSTON','TX','77018-5076','HARRIS COUNTY','UNITED STATES' UNION
SELECT 'bb671375-c644-41b7-8280-4414c747a78b','17459','QUADREAL PROPERTY GROUP','666 BURRARD ST STE 1238','VANCOUVER','BC','V6C 2X8','','CANADA' UNION
SELECT '87936359-ad61-452f-a789-d614905997a4','9216','','','','','','','' UNION
SELECT '1f0e126d-e50b-4b4c-b3d1-6670f9caba80','5714','QUANTUM REAL ESTATE MGMT., LLC','5101 RIVER RD STE 101','BETHESDA','MD','20816-1560','MONTGOMERY COUNTY','UNITED STATES' UNION
SELECT '4270c098-a960-4a7d-84f5-c045600518e2','5948','QUINTUS CORPORATION','1827 POWERS FERRY RD SE STE 2-200','ATLANTA','GA','30339-5686','COBB COUNTY','UNITED STATES' UNION
SELECT '9abbff69-bad6-4814-94b0-b0b9eaaa0db3','4161','R. W. DAY & ASSOCIATES, INC.','4005 NICHOLSON DR','BATON ROUGE','LA','70808-8402','EAST BATON ROUGE PARISH','UNITED STATES' UNION
SELECT 'bef060fb-0d86-4c7c-8f05-844fc2948b54','44','','','','','','','' UNION
SELECT '6f79a93b-7fa8-4a89-aa2d-c95e641b8e0a','1178','R.A. SNYDER PROPERTIES, INC.','2399 CAMINO DEL RIO S','SAN DIEGO','CA','92108-3606','SAN DIEGO COUNTY','UNITED STATES' UNION
SELECT '3c92f479-9f2d-4a5c-9597-5b0169fad1ef','4448','RADIANT PROPERTY MANAGEMENT','691 ELIZABETH AVE APT 2','NEWARK','NJ','07112-2363','ESSEX COUNTY','UNITED STATES' UNION
SELECT '436987ea-5959-4004-b3b2-2bb224cbd7fc','958','RADIUS PROPERTIES, LLC','2115 LINWOOD AVE STE 430','FORT LEE','NJ','07024-5022','BERGEN COUNTY','UNITED STATES' UNION
SELECT '58444d6c-b893-447a-b260-fd458d4b83d1','5771','RAFANELLI & NAHAS','3697 MT DIABLO BLVD, STE 250','LAFAYETTE','CA','94549','CONTRA COSTA COUNTY','UNITED STATES' UNION
SELECT '802cba1a-1cf1-4afc-b60d-84c2c2cbf7d9','1371929','','','','','','','' UNION
SELECT 'eea84887-58db-4715-94c5-5c90261e36c5','1378220','RAINEY PROPERTY MANAGEMENT, LLC','3300 N INTERSTATE 35 STE 700','AUSTIN','TX','78705-1874','TRAVIS COUNTY','UNITED STATES' UNION
SELECT '575bbeda-9fa0-4843-9c26-686a26bb0b13','6423','RAINIER MANAGEMENT, LTD.','4505 DUVAL ST','AUSTIN','TX','78751-3241','TRAVIS COUNTY','UNITED STATES' UNION
SELECT 'cf4d3d4b-cca0-4841-9b55-256186647a1b','348','RAINTREE PARTNERS','34052 LA PLZ STE 201','DANA POINT','CA','92629-2572','ORANGE COUNTY','UNITED STATES' UNION
SELECT 'ee2f4bf3-0a1e-417a-ad2f-00418ebdb354','1300','RAM PARTNERS LLC','1100 CIRCLE 75 PKWY SE STE 1200','ATLANTA','GA','30339-3081','COBB COUNTY','UNITED STATES' UNION
SELECT 'b1c7fb60-8518-4161-9ccc-227b5165c9f4','19567','RAMPART PROPERTIES, INC','16830 VENTURA BLVD STE 360','ENCINO','CA','91436-1711','LOS ANGELES COUNTY','UNITED STATES' UNION
SELECT 'dffea5fa-0e55-4f15-9dcb-d040b0a4a8bc','6416','RANE PROPERTY MANAGEMENT LLC','5360 GENESEE ST STE 201','BOWMANSVILLE','NY','14026-1044','ERIE COUNTY','UNITED STATES' UNION
SELECT 'a3496c3f-171f-43d1-b991-bb9c9e2af9c6','1281','RANGEWATER RESIDENTIAL, LLC','5605 GLENRIDGE DR NE  ONE PREMIER PLAZA','ATLANTA','GA','30342-1333','FULTON COUNTY','UNITED STATES' UNION
SELECT '007b3dfb-2cc5-4049-b94e-f1a2b6ca2a90','9389','RAS MANAGEMENT GROUP LLC','6001 BROKEN SOUND PKWY NW STE 504','BOCA RATON','FL','33487-2766','PALM BEACH COUNTY','UNITED STATES' UNION
SELECT '10139c7e-dca7-4b52-99e1-1deb10b89047','4092','RASSIER PROPERTIES','321 HARTZ AVE STE 7','DANVILLE','CA','94526-3336','CONTRA COSTA COUNTY','UNITED STATES' UNION
SELECT '4cc567d8-1c07-4a62-896b-d9c5a7eff771','6309','RAY DOUGLAS ANN & PATRICK, INC DBA RPM COMPANY','1420 S MILLS AVE','LODI','CA','95242','','UNITED STATES' UNION
SELECT '161bcd8a-93e5-4c2b-b369-94539029e734','5861','RC MANAGEMENT, INC','4702 WEST AVE STE 2','SAN ANTONIO','TX','78213-2727','BEXAR COUNTY','UNITED STATES' UNION
SELECT '9e196529-fccc-466f-8589-9f3c114d1296','493','RC RESIDENTIAL LLC','9001 HWY 21','PORT WENTWORTH','GA','31407','CHATHAM COUNTY','UNITED STATES' UNION
SELECT 'f9b707ac-657d-4115-b54d-7e42bae178ac','6095','RCF MANAGEMENT, LP','5950 SHERRY LN STE 205','DALLAS','TX','75225-6557','DALLAS COUNTY','UNITED STATES' UNION
SELECT '96a97f37-edb0-453b-b524-c15dbf4a1374','9373','RCG SE, LLC','10 S BROAD ST','BREVARD','NC','28712-5677','TRANSYLVANIA COUNTY','UNITED STATES' UNION
SELECT 'bc9a3bc7-270b-49a7-aee3-72775ab29a16','4','REAL AMERICA DEVELOPMENT','10501 HAGUE RD','FISHERS','IN','46038-2522','HAMILTON COUNTY','UNITED STATES' UNION
SELECT 'a27e5b78-9d2a-41c4-abc9-4f36b8860cb7','22549','REAL ESTATE ASSET PARTNERS','309 E PACES FERRY RD NE','ATLANTA','GA','30305-2367','FULTON COUNTY','UNITED STATES' UNION
SELECT '9354c040-1170-4df9-ad2b-cc69160eee57','2376','REAL ESTATE EQUITIES DEVELOPMENT LLC','1400 CORPORATE CENTER CURV STE 100','EAGAN','MN','55121-1380','DAKOTA COUNTY','UNITED STATES' UNION 
SELECT '24000757-7073-4385-b04d-bf40265a735a','21199','N/A','N/A','N/A','N/A','N/A','N/A','N/A' UNION
SELECT '6c594cbd-62c2-4fbc-9c44-5c7e4f0ee971','7702','RealPage QA Company','2201 Lakeside Blvd','Richardson','TX','75082','','UNITED STATES' UNION
SELECT 'f81c2945-bf05-42c1-8f7e-7abdd207e46f','1379352','N/A','N/A','N/A','N/A','N/A','N/A','N/A' UNION
SELECT '467d8b42-df59-42d9-ab9b-db1ec66e787a','1379348','N/A','N/A','N/A','N/A','N/A','N/A','N/A' UNION
SELECT 'c220a83c-3e2a-4650-a166-741297522e0c','1379336','N/A','N/A','N/A','N/A','N/A','N/A','N/A' UNION
SELECT '8e9c1c75-5632-4c7b-a978-d192266a6971','20735','N/A','N/A','N/A','N/A','N/A','N/A','N/A' UNION
SELECT 'd43576b7-bf94-4495-9ce8-fd5539726c7d','22524','N/A','N/A','N/A','N/A','N/A','N/A','N/A' UNION
SELECT 'f7254053-a261-4f15-8c95-9ef8793ed667','3373','REALTEX HOUSING MANAGEMENT LLC','1101 S CAPITAL OF TEXAS HWY BLDG F STE 200','WEST LAKE HILLS','TX','78746-6445','TRAVIS COUNTY','UNITED STATES' UNION
SELECT '19bb8870-88fd-4fb8-b44b-155acb0b8d34','879','REALTY SERVICES CORP','7 CORPORATE PLAZA DR','NEWPORT BEACH','CA','92660-7904','ORANGE COUNTY','UNITED STATES' UNION
SELECT 'd636d92a-5f39-4d5c-93e9-90446755a004','496','RED CURB INVESTMENTS','1600 CABRILLO AVE','TORRANCE','CA','90501-2819','LOS ANGELES COUNTY','UNITED STATES' UNION
SELECT 'c1c3ca26-c238-4a53-a9da-21614c7e6769','7691','RED DIRT MANAGEMENT, LLC','200 LANDCREST','FAIRVIEW','OK','73737-9736','MAJOR COUNTY','UNITED STATES' UNION
SELECT '029a0d34-bbbe-470b-890d-b2aa210c50c9','5413','RED STONE PROPERTIES, LLC','1100 WAYNE AVE STE 1010','SILVER SPRING','MD','20910-5632','MONTGOMERY COUNTY','UNITED STATES' UNION
SELECT 'f3b7e7bd-4340-4c7c-8239-04ed543db628','133','REDWOOD RESIDENTIAL LLC','1 E WACKER DR','CHICAGO','IL','60601-1474','COOK COUNTY','UNITED STATES' UNION
SELECT 'cdb9138a-fb8b-4619-ba49-358494705109','1374979','REFORMED PRESBYTERIAN HOME','2344 PERRYSVILLE AVE','PITTSBURGH','PA','15214-3560','ALLEGHENY COUNTY','UNITED STATES' UNION
SELECT '349bcad2-0b31-412e-9255-48c6e166b478','17778','REGENCY CONSOLIDATED RESIDENTIAL LLC','2417 FIELDS SOUTH DR','CHAMPAIGN','IL','61822-9391','CHAMPAIGN COUNTY','UNITED STATES' UNION
SELECT 'f4f53825-64d5-40df-990c-a610f2b0d63a','6','REGENT WEST CORP','1109 WESTWOOD BLVD','LOS ANGELES','CA','90024-3411','LOS ANGELES COUNTY','UNITED STATES' UNION
SELECT '91ca8f91-6d68-4b28-8521-b9fa96bcca5e','1176','REGION NINE HOUSING CORPORATION','88 HUNTINGTON ST','NEW BRUNSWICK','NJ','08901-1003','MIDDLESEX COUNTY','UNITED STATES' UNION
SELECT '2d75ec39-3775-4201-ac00-725675095eb6','4800','REI CAPITAL LLC','115 PENN WARREN DR STE 300 PMB 385','BRENTWOOD','TN','37027-5054','WILLIAMSON COUNTY','UNITED STATES' UNION
SELECT 'a730c690-ab55-4514-8df9-27690bfa2800','886','RELATED MANAGEMENT COMPANY, L.P.','423 W 55TH ST 9TH FL','NEW YORK','NY','10019-4460','NEW YORK COUNTY','UNITED STATES' UNION
SELECT '5021d504-7cb1-4cb4-8514-676b16aace8f','6080','RELIANT GROUP MANAGEMENT, LLC','601 CALIFORNIA ST STE 1150','SAN FRANCISCO','CA','94108-2816','SAN FRANCISCO COUNTY','UNITED STATES' UNION
SELECT 'd9dd6311-2bfa-4dd2-bafa-2d7b47c6d6b2','6062','RELIANT REAL ESTATE MANAGEMENT, INC. DBA THE REMM GROUP','15991 RED HILL AVE STE 200','TUSTIN','CA','92780-7320','ORANGE COUNTY','UNITED STATES' UNION
SELECT 'cae7eb60-056d-424d-a38d-8fb831ba1ec0','1379675','RELY PROPERTIES LLC','1015 WHITEHEAD ROAD EXT','EWING','NJ','08638-2431','MERCER COUNTY','UNITED STATES' UNION
SELECT 'bd14bd6e-6717-4245-ad47-89918711947f','9278','REM PROP LLC','2815 PEAVY RD','DALLAS','TX','75228-4800','DALLAS COUNTY','UNITED STATES' UNION
SELECT 'cc9ce007-5afb-4f67-a3bc-06a48d598309','1376240','REMERTON MILL APARTMENTS, LLC','1853 W GORDON ST','VALDOSTA','GA','31601-3044','LOWNDES COUNTY','UNITED STATES' UNION
SELECT '173630e7-b597-40eb-8ed5-b9cb6f5632e7','31784','RENAISSANCE MANAGEMENT GROUP INC.','1 CHESTER CIR','NEW BRUNSWICK','NJ','08901-1518','MIDDLESEX COUNTY','UNITED STATES' UNION
SELECT 'f6dd2c04-b49a-4d1e-9597-616883f2d916','1216','RENAISSANCE PROPERTY MGMT CORP','2111 CHAMPA ST','DENVER','CO','80205-2529','DENVER COUNTY','UNITED STATES' UNION
SELECT 'b4f78528-68a4-44b8-8c9d-739a7532df49','1375466','RENEE LEWIS REALTY LLC','6600 W MAIN ST STE 1','BELLEVILLE','IL','62223-3037','ST. CLAIR COUNTY','UNITED STATES' UNION
SELECT '7d646dd3-d6cd-4d73-a36c-d7a5f1ca6a91','4774','RENTAL MANAGEMENT, INC.','17 N 6TH ST','FORT SMITH','AR','72901-2101','SEBASTIAN COUNTY','UNITED STATES' UNION
SELECT 'c78a1e27-3937-4ecf-b27b-c0b9149d34fa','6270','RES ICD MANAGEMENT, L.P. DBA INTEGRATED PROPERTY MANAGEMENT','3110 W SOUTHLAKE BLVD STE 120','SOUTHLAKE','TX','76092-6731','TARRANT COUNTY','UNITED STATES' UNION
SELECT 'ae63906f-9d4d-40fa-9e44-643514fa362c','4551','RESIDENCE MANAGEMENT INC','209 TOWN CENTER BLVD','DAVENPORT','FL','33896-5226','','UNITED STATES' UNION
SELECT 'a9a1ee47-af66-4b3c-a826-94bb4b24c6e1','14968','RESIDENT FIRST MANAGEMENT','2909 FREDERICKSBURG RD','SAN ANTONIO','TX','78201-4712','BEXAR COUNTY','UNITED STATES' UNION
SELECT '56582653-a9cc-4bd4-aa0f-e12cd1f774fb','18369','RESIDENTIAL PROPERTIES MANAGEMENT, INC.','1105 BROOKSTOWN AVE','WINSTON SALEM','NC','27101-2524','FORSYTH COUNTY','UNITED STATES' UNION
SELECT 'e4afb51b-b1b1-468b-b575-d388ca7a3206','947','','','','','','','' UNION
SELECT '30b27ce6-e24a-4ce7-ae25-5b637eaae985','23193','','','','','','','' UNION
SELECT '5adf4dde-6b8a-43fa-8751-f678cb06d39c','17718','','','','','','','' UNION
SELECT '4afd66e1-99e3-4dbc-8d27-60f332b53950','6425','RICH MANAGEMENT LLC','1420 WALNUT ST STE 1011','PHILADELPHIA','PA','19102-4010','PHILADELPHIA COUNTY','UNITED STATES' UNION
SELECT '61cf4038-1dfd-4c98-aa44-385be06a280a','5945','RICH PROPERTIES LLC','2552 PETERS RD STE B','FORT PIERCE','FL','34945-2635','ST. LUCIE COUNTY','UNITED STATES' UNION
SELECT '836f83cd-af85-4b95-91e8-857844d56f22','616','RICHARDSON PROPERTIES LLC','9800 MAUMELLE BLVD','NORTH LITTLE ROCK','AR','72113-7027','PULASKI COUNTY','UNITED STATES' UNION
SELECT '6dc51237-1357-455b-bfb9-660cd553edbf','6402','RICHLAND RESIDENTIAL INC','7055 ENGLE RD STE 302','MIDDLEBURG HEIGHTS','OH','44130-8403','CUYAHOGA COUNTY','UNITED STATES' UNION
SELECT 'a978777d-55ec-4d51-ad17-3122614d5ae3','5576','RICHMAN PROPERTY SERVICES, INC','340 PEMBERWICK RD','GREENWICH','CT','06831-4240','FAIRFIELD COUNTY','UNITED STATES' UNION
SELECT '4fe6515c-77b9-4b60-bf66-8fb5295c7759','6132','RICHSMITH MANAGEMENT, LLC','9800 MAUMELLE BLVD','NORTH LITTLE ROCK','AR','72113-7027','PULASKI COUNTY','UNITED STATES' UNION
SELECT 'e5864a1f-5fe6-4e30-adf8-448da47d01a2','1380022','','','','','','','' UNION
SELECT '5911f830-85ae-477e-8977-90acb7515438','1371815','','','','','','','' UNION
SELECT '41aac51d-0703-4529-8b50-669d06eb17db','6860','RIDGELINE MANAGEMENT COMPANY (CORPORATE)','1914 WILLAMETTE FALLS DR STE 280','WEST LINN','OR','97068-4690','CLACKAMAS COUNTY','UNITED STATES' UNION
SELECT '7d883ed7-edc3-4008-8912-f64c638e465b','1379722','','','','','','','' UNION
SELECT '66e59652-e990-4c4b-aedb-8b8190319a20','6448','RINNIER MANAGEMENT LLC','218 E MAIN ST','SALISBURY','MD','21801-4923','WICOMICO COUNTY','UNITED STATES' UNION
SELECT '180a8dfb-a15a-4f31-857c-ddfc76cca684','6400','RIPLEY HEATWOLE CO INC','808 NEWTOWN RD','VIRGINIA BEACH','VA','23462-1116','VIRGINIA BEACH CITY','UNITED STATES' UNION
SELECT '3443d997-a69d-45d2-bace-4df2288d3770','19400','RISE ASSOCIATION MANAGEMENT GROUP','2500 WILCREST DR','HOUSTON','TX','77042-2752','HARRIS COUNTY','UNITED STATES' UNION
SELECT '3885191b-b7af-4f62-a8d1-c7d9b7ffa09b','6332','RISE RESIDENTIAL, LLC - STUDENT','129 N PATTERSON ST','VALDOSTA','GA','31601-5538','LOWNDES COUNTY','UNITED STATES' UNION
SELECT 'f2f74e44-7fc2-44b4-9f59-1757623eee27','2962','RITTER MANAGEMENT, INC','580 DECKER CT STE 203','IRVING','TX','75062','DALLAS COUNTY','UNITED STATES' UNION
SELECT 'f6eb2d8b-be48-48ee-af67-35a747c13cbf','4996','RIVER PARK APARTMENTS, LTD','731 VASSAR ST','ORLANDO','FL','32804-4920','ORANGE COUNTY','UNITED STATES' UNION
SELECT 'f0e94280-8917-4323-a0fc-9df1961e288e','498','','','','','','','' UNION
SELECT '89f065f3-e136-4863-931e-4ec05f4aad04','5363','RIVERDALE MANAGEMENT','2021 CUNNINGHAM DR STE 100','HAMPTON','VA','23666-3368','HAMPTON CITY','UNITED STATES' UNION
SELECT '3eaed3ab-db7b-46a0-a6ce-8b8e93e95fab','6728','RIVERGATE KW MANAGEMENT DBA RKW RESIDENTIAL','8200 NW 33RD ST STE 303','DORAL','FL','33122-1901','MIAMI-DADE COUNTY','UNITED STATES' UNION
SELECT '538dab44-81b9-4afa-b37e-bc9d55b7a2fa','14847','RIVEROAKS MANAGEMENT, INC.','17742 IRVINE BLVD STE 205','TUSTIN','CA','92780-3239','ORANGE COUNTY','UNITED STATES' UNION
SELECT 'cfd680ef-9d7b-4266-b487-97a83b5fc0d3','5915','RIVERSTONE RESIDENTIAL','1201 ELM ST STE 1600','DALLAS','TX','75270-2038','DALLAS COUNTY','UNITED STATES' UNION
SELECT '64048e78-8858-4caf-9974-1f42bb768a4c','5470','RKM MANAGEMENT','500 NE SPANISH RIVER BLVD STE 108','BOCA RATON','FL','33431-4559','PALM BEACH COUNTY','UNITED STATES' UNION
SELECT '7fb81adb-1c1e-4f03-8886-05d455deef2d','14860','RLDM REAL ESTATE LLP','10840 OTIS CIR','WESTMINSTER','CO','80020-3164','JEFFERSON COUNTY','UNITED STATES' UNION
SELECT '23a5f1b9-284d-422a-9cc9-ac9faa6f7c51','8526','ROBBINS PROPERTY ASSOCIATES, LLC','4890 W KENNEDY BLVD STE 270','TAMPA','FL','33609-1851','HILLSBOROUGH COUNTY','UNITED STATES' UNION
SELECT '59434356-d517-4448-ba4b-6544fb69ced6','8182','ROBERT RUSSELL EASTERN STAR MASONIC RETIREMENT CAMPUS','2445 S QUEBEC ST','DENVER','CO','80231-6030','DENVER COUNTY','UNITED STATES' UNION
SELECT 'b2281a6c-4c14-4ea6-beb5-39fb3a7adef4','3594','ROCK MANAGEMENT','6400 TELEGRAPH RD STE 2500','BLOOMFIELD HILLS','MI','48301-1728','OAKLAND COUNTY','UNITED STATES' UNION
SELECT '2f55a955-660f-46fc-ab57-9d1a571b068e','6865','ROCKSTAR CAPITAL MANAGEMENT LLC','720 N POST OAK RD STE 650','HOUSTON','TX','77024-3899','HARRIS COUNTY','UNITED STATES' UNION
SELECT 'c82f1d95-03cc-466b-a8f0-b2221331a33a','20629','ROCKWELL PARTNERS LLC','1000 N HALSTED ST','CHICAGO','IL','60642-4251','COOK COUNTY','UNITED STATES' UNION
SELECT '35cb3615-2a34-4827-939b-2d05e5889033','6686','ROCKWOOD CAPITAL','2100 LAKESIDE BLVD STE 425','RICHARDSON','TX','75082-4350','DALLAS COUNTY','UNITED STATES' UNION
SELECT 'c1d4c4ac-d6ef-4572-9e07-4f1f459695c2','1375619','ROCKY MOUNTAIN COMMUNITY WORKS LLC','5940 QUAIL ST','ARVADA','CO','80004-4757','JEFFERSON COUNTY','UNITED STATES' UNION
SELECT '21a7087a-96c0-41c5-b368-575639468c47','5433','ROCO REAL ESTATE','33 BLOOMFIELD HILLS PKWY STE 135','BLOOMFIELD HILLS','MI','48304-2945','OAKLAND COUNTY','UNITED STATES' UNION
SELECT '332d4e75-eb0b-4bd1-9d23-fd27946808a5','6411','ROHDIE MANAGEMENT LLC','52 VANDERBILT AVE SUITE 2007','NEW YORK','NY','10017','NEW YORK COUNTY','UNITED STATES' UNION
SELECT '6934c042-c393-4855-b33b-e8bd0c9b90a6','6098','','','','','','','' UNION
SELECT 'b8217fe9-2178-4515-9e13-7dfe30c12846','141','ROM INVESTMENTS INC','6464 W SUNSET BLVD STE 850','LOS ANGELES','CA','90028-8026','LOS ANGELES COUNTY','UNITED STATES' UNION
SELECT 'f3dfb002-0721-4b2c-a1f2-53f51e42e9fe','14736','ROMA COMMERCIAL','5870 ONIX DR STE A1','EL PASO','TX','79912-5566','EL PASO COUNTY','UNITED STATES' UNION
SELECT '72a8f7e0-9446-4d52-897f-22b097843ae7','6562','','','','','','','' UNION
SELECT 'a9427f26-75b3-4b75-9d54-128b3f1736bd','6212','ROOSTER PHILBEN INC DBA STOUT MANAGEMENT','10151 PARK RUN DR','LAS VEGAS','NV','89145-8859','ARAPAHOE COUNTY','UNITED STATES' UNION
SELECT '9e9767f9-0daa-4dca-9a58-c2ee7b9a6532','5922','ROSCOE PROPERTIES GP LLC','5508 PARKCREST DR STE 320','AUSTIN','TX','78731-5049','TRAVIS COUNTY','UNITED STATES' UNION
SELECT 'a5a2a01b-267d-40ba-b5c5-5e81cfbb7590','6710','ROSE COMMUNITY MANAGEMENT, LLC','551 5TH AVE 23RD FL','NEW YORK','NY','10176-2604','NEW YORK COUNTY','UNITED STATES' UNION
SELECT 'ee733506-1999-42a2-a41a-3195d68bc239','6042','ROSELAND MANAGEMENT CO','210 HUDSON ST STE 400','JERSEY CITY','NJ','07311-1206','HUDSON COUNTY','UNITED STATES' UNION
SELECT '99bd5ed6-8579-43c3-bf1b-fbff9fbf7a5b','706','ROYAL AMERICAN MANAGEMENT','1002 W 23RD ST STE 400','PANAMA CITY','FL','32405-3648','BAY COUNTY','UNITED STATES' UNION
SELECT '8d902863-a103-41ff-ac5d-38ba8785aa62','5307','ROYAL MANAGEMENT','60 SPRINGDALE BLVD STE C','MOBILE','AL','36606-3904','MOBILE COUNTY','UNITED STATES' UNION
SELECT '1d4cb45b-da61-4e14-ab53-8715bcf57f1c','142','ROYAL T MANAGEMENT','7419 N CEDAR AVE STE 102','FRESNO','CA','93720-3640','FRESNO COUNTY','UNITED STATES' UNION
SELECT 'd6ab8dd1-a60f-49dd-b100-d250e7c74926','1374714','N/A','N/A','N/A','N/A','N/A','N/A','N/A' UNION
SELECT 'e087788f-0765-4d00-9b8b-47663370f701','8135','N/A','N/A','N/A','N/A','N/A','N/A','N/A' UNION
SELECT '6c53283e-3d68-4b14-828f-920256a9c14a','145','RREAF RESIDENTIAL, LLC','1909 WOODALL RODGERS FWY 3RD FL','DALLAS','TX','75201-2232','DALLAS COUNTY','UNITED STATES' UNION
SELECT '0395c5b2-eee1-4f51-b8a2-0d43ee4027b1','1373885','RRG MV APARTMENTS LLC','114 OLD YORK RD STE 100','JENKINTOWN','PA','19046','MONTGOMERY COUNTY','UNITED STATES' UNION
SELECT 'b89f216d-1a0d-479e-b366-bb8ab50952ee','1378436','RUSH HOMES','1721 MONSVIEW PL','LYNCHBURG','VA','24504-1065','LYNCHBURG CITY','UNITED STATES' UNION
SELECT 'd0a52854-d001-4520-8b3e-4061ebd316e0','501','RUSSO PROPERTY MANAGEMENT, LLC','570 COMMERCE BLVD','CARLSTADT','NJ','07072-3013','BERGEN COUNTY','UNITED STATES' UNION
SELECT 'b4c92fe9-4ff2-4f6f-a185-82bfc14bccd0','1978','RUTHERFORD MANAGEMENT COMPANY','770 TAMALPAIS DR STE 202','CORTE MADERA','CA','94925-1736','MARIN COUNTY','UNITED STATES' UNION
SELECT '2a0b429d-0422-4fbc-be4b-b4a8bb84a1b3','1380588','RYBAK MANAGEMENT CORP','1817 EMMONS AVE STE 1','BROOKLYN','NY','11235-2715','KINGS COUNTY','UNITED STATES' UNION
SELECT '64910946-c9bd-453b-a16a-139def65ead6','5767','S & S PROP MGMT','129 S 11TH ST','NASHVILLE','TN','37206-2954','DAVIDSON COUNTY','UNITED STATES' UNION
SELECT '7dfa1f61-68d5-4cf5-a8d4-2cd4c9a5bd91','5222','S.L. NUSBAUM REALTY CO.','1700 WELLS FARGO CENTER  440 MONTICELLO AVE','NORFOLK','VA','23510-2571','','UNITED STATES' UNION
SELECT '7ff9a601-2b5b-49e6-8a2a-0c2d8664173a','134','SA LOOKOUT ROAD MF LP','7319 N LOOP 1604 E','LIVE OAK','TX','78233-5662','BEXAR COUNTY','UNITED STATES' UNION
SELECT 'd7c67f03-c50c-4745-b35d-be83c3bd6638','1379209','SABIN COMMUNITY DEVELOPMENT CORP','1488 NE ALBERTA ST','PORTLAND','OR','97211-5062','MULTNOMAH COUNTY','UNITED STATES' UNION
SELECT '69de842f-d2b1-4ebe-849b-1504cdfbe977','27275','','','','','','','' UNION
SELECT '9a994b1c-7c07-46a4-b99c-7eba133ce990','20953','SALEM CREST APARTMENTS LLC','790 SALEM CREST LN','WINSTON SALEM','NC','27103-6627','FORSYTH COUNTY','UNITED STATES' UNION
SELECT '7dfd62f0-80ca-46ad-9be9-d076259301fd','1379723','','','','','','','' UNION
SELECT 'e77f9b80-ab9c-4792-8895-4194b12336c9','14902','SALINAS PROPERTY MANAGEMENT','6406 MCPHERSON RD STE 4','LAREDO','TX','78041-6258','WEBB COUNTY','UNITED STATES' UNION
SELECT '5fd6e9e7-0f7f-4cb3-b6ff-cd9f76e2a6eb','1052','SAMARITAN PROPERTY MANAGEMENT, INC. DBA SAMARITAN COMPANIES','170 N PERRY RD STE 198','PLAINFIELD','IN','46168-9025','HENDRICKS COUNTY','UNITED STATES' UNION
SELECT 'bccbc80f-f1f4-460c-8184-26095b9f376e','1380002','','','','','','','' UNION
SELECT '3bab6bed-e574-4c9f-905f-1c83789f7985','1380023','','','','','','','' UNION
SELECT '0e783552-e319-4ce5-952e-4a0340f3f057','5556','SAMUELS & ASSOCIATES MANAGEMENT LLC','136 BROOKLINE AVE','BOSTON','MA','02215-3907','SUFFOLK COUNTY','UNITED STATES' UNION
SELECT '5d5335d7-49a3-43f6-8c99-0edf4545d9cf','31585','SAN ANTONIO PEDIATRICS CENTER INC','2020 BABCOCK RD STE 10','SAN ANTONIO','TX','78229-4437','BEXAR COUNTY','UNITED STATES' UNION
SELECT '178737d2-7525-4b7e-a025-32924d528fcf','222','SAN DIEGO SUNRISE MANAGEMENT COMPANY','7837 CONVOY CT STE 100','SAN DIEGO','CA','92111-1209','SAN DIEGO COUNTY','UNITED STATES' UNION
SELECT '8988df3b-34ba-4bd1-a946-d3e2f5075b9b','60','SAN FRANCISCO STATE UNIVERSITY','800 FONT BLVD','SAN FRANCISCO','CA','94132-1723','SAN FRANCISCO COUNTY','UNITED STATES' UNION
SELECT '16232639-2df4-4831-8cf9-006728d0c5fe','1921','SAN MIGUEL MANAGEMENT LP','2222 RIO GRANDE ST STE D200','AUSTIN','TX','78705-5110','TRAVIS COUNTY','UNITED STATES' UNION
SELECT 'cd4ccff7-2a60-4033-88cf-d192b06ae3c7','1242','SANSONE GROUP','120 S CENTRAL AVE STE 500','SAINT LOUIS','MO','63105-1717','','UNITED STATES' UNION
SELECT 'd5f81670-0c8d-469a-ae6e-006d2db93870','9315','SANTA FE APARTMENT DEVELOPMENT, LLC','8181 NW 39TH AVE STE 220','GAINESVILLE','FL','32606-5611','ALACHUA COUNTY','UNITED STATES' UNION
SELECT 'e1c727cf-2d04-4141-9bac-5598c84ff03e','8282','SANTA FE CIVIC HOUSING AUTHORITY INC.','664 ALTA VISTA ST','SANTA FE','NM','87505-4149','SANTA FE COUNTY','UNITED STATES' UNION
SELECT '777dadf6-0489-40d5-b32b-44fc18c55934','20898','SARAHS CIRCLE','4838 N SHERIDAN RD','CHICAGO','IL','60640-3704','COOK COUNTY','UNITED STATES' UNION
SELECT '8e385410-8492-48d8-bb0f-875ecfcb5fb6','6382','SARATOGA CAPITAL INC','485 ALBERTO WAY STE 200','LOS GATOS','CA','95032-5476','SANTA CLARA COUNTY','UNITED STATES' UNION
SELECT 'cde09ec8-5a2c-40a2-9f35-5365c747d030','1364','SARES REGIS MANAGEMENT COMPANY, L.P. A DELAWARE LIMITED PARTNERSHIP','3501 JAMBOREE RD STE 3000 NORTH TOWER','NEWPORT BEACH','CA','92660','','UNITED STATES' UNION
SELECT '7551b2e3-4276-4b69-a080-0ec13325f331','1816','SAWYER PROPERTY MANAGEMENT OF MASSACHUSETTS LLC','1215 CHESTNUT ST','NEWTON','MA','02464-1308','MIDDLESEX COUNTY','UNITED STATES' UNION
SELECT 'e425849e-dffc-4e00-8846-2df24a126f9b','6695','SC BODNER COMPANY, INC','9075 N MERIDIAN ST STE 250','INDIANAPOLIS','IN','46260-2738','MARION COUNTY','UNITED STATES' UNION
SELECT '56abdf58-335c-4722-af04-01768078af92','1013','SCG GLOBAL HOLDINGS, L.L.C.','591 W PUTNAM AVE','GREENWICH','CT','06830-6005','FAIRFIELD COUNTY','UNITED STATES' UNION
SELECT '6696e8ec-86c0-4ea9-b7c2-3c0594d96b98','178','SCHRADER RENTAL PROPERTIES','5862 FARINGDON PL','RALEIGH','NC','27609-4481','WAKE COUNTY','UNITED STATES' UNION
SELECT 'a702837b-1537-4da4-b4ce-d149672026d4','821','SCOTT BROTHERS INVESTMENT CORP INC','100 BIG RIVER DR','LAKE ST LOUIS','MO','63367-2158','ST. CHARLES COUNTY','UNITED STATES' UNION
SELECT 'afccabb8-336b-496b-8935-6063ef591300','5422','SCOTT MANAGEMENT INC.','300 N LEE ST STE 200','ALEXANDRIA','VA','22314-2640','ALEXANDRIA CITY','UNITED STATES' UNION
SELECT '673f0166-8f1e-4b88-ace9-2504470dae00','5238','SCULLY COMPANY','801 OLD YORK RD','JENKINTOWN','PA','19046-1608','MONTGOMERY COUNTY','UNITED STATES' UNION
SELECT 'ee2ab506-7c06-4fa4-9d5d-fc88140baca1','5408','SDK APARTMENTS LLC','1124 E RIDGEWOOD AVE STE 101','RIDGEWOOD','NJ','07450-3915','BERGEN COUNTY','UNITED STATES' UNION
SELECT 'e26ad86e-a6d3-40b6-9f4a-b461817ebb48','14761','SE ASSET MANAGEMENT, LLC','4276 GREAT OAKS LN','JACKSONVILLE','FL','32207-6220','DUVAL COUNTY','UNITED STATES' UNION
SELECT 'fb458ac2-9570-44f4-99d8-b939beacc522','20379','','','','','','','' UNION
SELECT '2ff98d36-861c-4757-bf14-f31a975f3d73','1374513','SECOND STONE RIDGE COOPERATIVE CORPORATION','169 YAREMICH DR','BRIDGEPORT','CT','06606-2570','FAIRFIELD COUNTY','UNITED STATES' UNION
SELECT '298b6f92-1e47-44e8-9b1c-a41626e1a9eb','6405','SECURITY PROPERTIES','106 POTOMAC ST','BOGALUSA','LA','70427-3032','WASHINGTON PARISH','UNITED STATES' UNION
SELECT '298b6f92-1e47-44e8-9b1c-a41626e1a9eb','6405','SECURITY PROPERTIES RESIDENTIAL LLC','701 5TH AVE STE 5700','SEATTLE','WA','98104-7097','KING COUNTY','UNITED STATES' UNION
SELECT '6d412f90-2479-4c1c-b082-d9a2a31e95db','6526','SELDIN COMPANY','16910 FRANCES ST','OMAHA','NE','68130-2398','DOUGLAS COUNTY','UNITED STATES' UNION
SELECT '3a6c7a3b-bd42-4762-8846-063b07aac93d','5244','SENIOR CARE NETWORK INC.','1215 HULTON RD','OAKMONT','PA','15139-1135','ALLEGHENY COUNTY','UNITED STATES' UNION
SELECT '1987137d-f34c-4743-99a1-60976aaaa999','689','SENTRY MANAGEMENT SERVICES','8425 KEYSTONE XING STE 108','INDIANAPOLIS','IN','46240-4322','MARION COUNTY','UNITED STATES' UNION
SELECT '8842ca63-651c-4c04-b114-4f4aec665732','1383','SEQUOIA EQUITIES INC','1777 BOTELHO DR STE 300','WALNUT CREEK','CA','94596-5041','CONTRA COSTA COUNTY','UNITED STATES' UNION
SELECT '4b809bea-e996-4194-88a8-31beedd7b2e3','1375354','SHAIKH DEVELOPMENT, LLC','45 HIGHMEADOW RD','ROCKY HILL','CT','06067-1251','HARTFORD COUNTY','UNITED STATES' UNION
SELECT 'e72f9467-20db-4859-bb11-d33b9c3a8f46','2821','SHAMCO MANAGEMENT','505 THORNALL ST STE 403','EDISON','NJ','08837-2260','MIDDLESEX COUNTY','UNITED STATES' UNION
SELECT '7688cbed-c524-4508-996e-0be12a92b5c9','903','SHAPELL PROPERTIES, INC','1990 S BUNDY DR STE 500','LOS ANGELES','CA','90025-5245','LOS ANGELES COUNTY','UNITED STATES' UNION
SELECT '30a36559-b165-45c3-94ee-d31c3b4bd2d8','1374681','SHARKEY AND ASSOCIATES, INC','3821 NE 16TH AVE','OAKLAND PARK','FL','33334-5469','BROWARD COUNTY','UNITED STATES' UNION
SELECT 'e06e4617-6135-405b-aebe-a6cac53003ab','15197','SHARP WESTOVER HILLS','6200 PERSHING AVE','FT WORTH','TX','76116-2608','TARRANT COUNTY','UNITED STATES' UNION
SELECT '20ecfdca-8e53-4bb2-a337-6ccaf420e9aa','6705','SHEEHAN PROPERTY MGMT INC','6930 ATRIUM BOARDWALK S STE 100','INDIANAPOLIS','IN','46250-2179','MARION COUNTY','UNITED STATES' UNION
SELECT '6d0e0da5-81da-4664-9fd2-80afb9d42884','27167','SHELTER ASSET MANAGEMENT LLC','10100 SANTA MONICA BLVD STE 2480','LOS ANGELES','CA','90067-4132','LOS ANGELES COUNTY','UNITED STATES' UNION
SELECT '36fa9957-64d8-4924-b996-1639289e8775','6846','SHELTON RESIDENTIAL, INC.','2850 E CAMELBACK RD STE 300','PHOENIX','AZ','85016-4367','MARICOPA COUNTY','UNITED STATES' UNION
SELECT '66bf7b0b-67e8-4df1-9c80-a4b0311d4107','826','SHERMAN ASSOCIATES','233 PARK AVE STE 201A','MINNEAPOLIS','MN','55415-1132','HENNEPIN COUNTY','UNITED STATES' UNION
SELECT '6bdd5151-fa1c-49e7-a62a-28a5baeb1b8f','21118','','','','','','','' UNION
SELECT '26cf0a82-a546-4470-a3de-1364b96c43d0','9246','SHERRY AITKEN','5403 WILLIAM HOLLAND AVE','AUSTIN','TX','78756-2039','TRAVIS COUNTY','UNITED STATES' UNION
SELECT 'a06bf2fc-c146-411f-800b-b9c6398470b2','1271','SHK MANAGEMENT DBA KORMAN COMMUNITIES','220 W GERMANTOWN PIKE STE 250','PLYMOUTH MEETING','PA','19462-1437','MONTGOMERY COUNTY','UNITED STATES' UNION
SELECT '115ea96f-12a8-4997-8455-a11fe61cdb3a','1379148','SHOMA MANAGEMENT, LLC','201 SEVILLA AVE','CORAL GABLES','FL','33134-6616','MIAMI-DADE COUNTY','UNITED STATES' UNION
SELECT '65610c57-0035-4fc3-8f8e-1460fe12805a','454','SHORE TO SHORE PROPERTIES, LLC','1001 BRIDGEWAY # 170','SAUSALITO','CA','94965-2104','MARIN COUNTY','UNITED STATES' UNION
SELECT '18a21c7c-f949-4fd7-8fd2-d5fabf76ffd4','6885','SHORELINE CORP','130 PROSPECT ST','CAMBRIDGE','MA','02139-1844','MIDDLESEX COUNTY','UNITED STATES' UNION
SELECT '151d187f-d678-40fb-9c4c-98252f3e76ea','976','SHOWE MANAGEMENT CORP','45 N 4TH ST STE 200','COLUMBUS','OH','43215-3602','FRANKLIN COUNTY','UNITED STATES' UNION
SELECT 'a13ef1d7-992e-44fa-9ed3-f1d96c308783','787','SHP MANAGEMENT CORP','7 THOMAS DR','CUMBERLAND FORESIDE','ME','04110-1318','CUMBERLAND COUNTY','UNITED STATES' UNION
SELECT '1529892f-feeb-449f-8e75-b83a9d1b6d7a','1379724','','','','','','','' UNION
SELECT '005f119b-bf4f-47b9-a462-dd857af68d12','5202','SIDAL REALTY COMPANY','7201 WALKER ST STE 20','ST LOUIS PARK','MN','55426-4280','HENNEPIN COUNTY','UNITED STATES' UNION
SELECT '4aacb84d-e7fb-40e7-a800-cb82fc20c6e4','5259','SIERRA DEVELOPMENT','401 CHERRY ST STE 600','MACON','GA','31201-3496','BIBB COUNTY','UNITED STATES' UNION
SELECT '2fef86ad-95b7-4008-baa4-3852b02a76ad','5384','SIGNATURE MANAGEMENT','5201 BROOK HOLLOW PKWY STE A','NORCROSS','GA','30071-3640','GWINNETT COUNTY','UNITED STATES' UNION
SELECT 'a4951e19-0c4f-4824-8237-233a4dd354d8','6731','SIGNATURE MANAGEMENT CORP','244 MUSTANG TRL STE 6','VIRGINIA BEACH','VA','23452-7510','VIRGINIA BEACH CITY','UNITED STATES' UNION
SELECT '32af85bf-0beb-40c7-841b-a29788ec1b06','1004','SIGNATURE PROPERTY GROUP, INC','305 BLANDWOOD AVE','GREENSBORO','NC','27401-6029','','UNITED STATES' UNION
SELECT '13d0964c-0052-4920-b3d2-1a0dbbb9b00f','4757','','','','','','','' UNION
SELECT '2a50a60c-2596-47d6-8e60-89b6a4e79972','466','SILVA-MARKHAM PARTNERS, LLC','1325 S COLORADO BLVD STE B601','DENVER','CO','80222-3348','DENVER COUNTY','UNITED STATES' UNION
SELECT '953271ed-1bae-475d-ac8f-817bba06dc02','5268','SILVER STAR REAL ESTATE','1 CENTERPOINTE DR STE 400','LA PALMA','CA','90623-2530','ORANGE COUNTY','UNITED STATES' UNION
SELECT '4e99df05-b969-4d09-b031-6ef40e2ea3e9','1380508','SILVERIE PROPERTIES LLC','3160 OCEAN TERRACE','MARINA','CA','93933-3291','','UNITED STATES' UNION
SELECT 'f13d7909-08b6-400b-9e37-8296c3a13fb4','1375482','SILVERPOINT SENIOR LIVING, LLC','302 CROSS ST','NEW BRAUNFELS','TX','78130-5644','COMAL COUNTY','UNITED STATES' UNION
SELECT 'b45e3fd6-ad36-49ab-8117-881e77870ab5','8300','SILVERSTEIN PROPERTIES, INC.','7 WORLD TRADE CTR','NEW YORK','NY','10007-2140','NEW YORK COUNTY','UNITED STATES' UNION
SELECT '4df36990-5522-4d4d-9a89-93f638a0ca10','2572','SIMMONS & KOURTIS, LLC','12150 E 96TH ST N','OWASSO','OK','74055-5338','TULSA COUNTY','UNITED STATES' UNION
SELECT 'e03c8e8e-2f78-4367-961d-7412e20a5d57','138','','','','','','','' UNION
SELECT '5c9f7ed2-ae1e-4deb-acb2-7d4827663cb3','17','SIMPLY BETTER MANAGEMENT','60 COLUMBUS CIR','NEW YORK','NY','10023','','UNITED STATES' UNION
SELECT '62c2a83d-7359-4ede-bbee-c78d7cdf7486','5228','SIMPSON PROPERTY GROUP LP','7601 E TECHNOLOGY WAY STE 600','DENVER','CO','80237-3190','DENVER COUNTY','UNITED STATES' UNION
SELECT '19f2ffe0-1e97-4d2e-a634-7f4ab0d7d796','150','','','','','','','' UNION
SELECT 'ef7971a7-5223-4d79-92f9-ae434624554c','15130','SINGH MANAGEMENT','10206 FAIRBANKS N HOUSTON RD','HOUSTON','TX','77064-3406','HARRIS COUNTY','UNITED STATES' UNION
SELECT '469af7a1-4ce1-40ac-a548-5b7a3ef6aa68','1375179','','','','','','','' UNION
SELECT '5b570f65-e2a0-406e-94d5-d258f2345842','1380003','','','','','','','' UNION
SELECT '9718af73-ff9a-446b-822c-17013eb4cba5','4737','SKYLINE TOWERS INC','300 E COLLINS DR','CASPER','WY','82601-2813','NATRONA COUNTY','UNITED STATES' UNION
SELECT '7167c93d-77c7-4950-97c5-eea58f07a179','1379530','SKYVIEW MANAGEMENT LLC','4024 COLLEGE POINT BLVD STE F-6','FLUSHING','NY','11354-5111','QUEENS COUNTY','UNITED STATES' UNION
SELECT 'c0b2190e-b210-4207-b3ee-aa7a00f994c3','9403','SLATE PROPERTY GROUP','38 E 29TH ST 9TH FL','NEW YORK','NY','10016-7911','NEW YORK COUNTY','UNITED STATES' UNION
SELECT '5d00b999-d5a2-493c-b6fa-8b08bd3dbf78','14907','SLATEHOUSE GROUP PROPERTY MANAGEMENT, LLC','841 FLORY MILL RD STE 2','LANCASTER','PA','17601-2747','LANCASTER COUNTY','UNITED STATES' UNION
SELECT 'bf2a9d13-6ee2-4112-8e6f-cb51feb8a8ed','1394','SME MULTIFAMILY DBA IMPLICITY MANAGEMENT COMPANY','14855 BLANCO RD STE 309','SAN ANTONIO','TX','78216-7731','BEXAR COUNTY','UNITED STATES' UNION
SELECT '8de5b28c-5928-4695-b10e-2e269c6eaec1','1380004','','','','','','','' UNION
SELECT '0fe54b3f-ad4e-425e-8f86-d7d2fe30f08c','1380005','','','','','','','' UNION
SELECT 'deb1773d-0896-494f-8053-692f1d24b40d','1378286','SOJURN','700 PENNSYLVANIA AVE SE # 2097','WASHINGTON','DC','20003-2493','DISTRICT OF COLUMBIA','UNITED STATES' UNION
SELECT 'd2dfeaf9-a8a7-4b0f-aa85-581e5474a343','1374659','SOLLENBERGER PROPERTIES, LLC','5415 ROMA VALLEY CT','FORT COLLINS','CO','80525-6739','LARIMER COUNTY','UNITED STATES' UNION
SELECT 'dd928fde-a6b1-41ac-a7a6-bc29230dc4b0','1414','SOME INC., DBA SO OTHERS MIGHT EAT','60 O ST NW','WASHINGTON','DC','20001-1259','DISTRICT OF COLUMBIA','UNITED STATES' UNION
SELECT '1b277b8c-d43a-4323-8508-4f4e33cb1dce','20670','','','','','','','' UNION
SELECT '4b5bb00d-82da-4fa4-8091-560bb1d0f242','1967','SOMERSET MANAGEMENT SERVICES, INC.','308 VINE ST','SOMERSET','KY','42503','PULASKI COUNTY','United States' UNION
SELECT 'b8dad909-c555-4ca2-a07f-0641b4874e2c','1381','SOMERSET PACIFIC MANAGEMENT','4481 N DRESDEN PL','GARDEN CITY','ID','83714-5091','ADA COUNTY','UNITED STATES' UNION
SELECT '0d4ad6cd-c34a-4e19-afdf-4f4b653702ed','1371255','SORIMAR, INC.','4840 NW 7TH ST','MIAMI','FL','33126-2583','MIAMI-DADE COUNTY','UNITED STATES' UNION
SELECT '1d2ee5cf-2a62-49e1-a5c6-031e6dea89a4','2203','','','','','','','' UNION
SELECT '87bb69d1-86e3-48c1-921a-2d788d5627e5','14858','SOUTH MIDDLESEX OPPORTUNITY COUNCIL, INC.','7 BISHOP ST','FRAMINGHAM','MA','01702-8323','MIDDLESEX COUNTY','UNITED STATES' UNION
SELECT '1002cb87-381e-49d6-af2b-5dcad8ea1de1','1377868','SOUTH PARKWAY MANAGEMENT INCORPORATED','440 S LA SALLE ST STE 1900','CHICAGO','IL','60605-5010','COOK COUNTY','UNITED STATES' UNION
SELECT '1b575833-4ba6-4fe1-a021-ac5d41d9095e','525','','','','','','','' UNION
SELECT '602a321c-6807-41a2-85ed-ee937bc613e5','1437','','','','','','','' UNION
SELECT '60acb993-61e4-40a3-b0b4-2187c9bc456e','1371958','SOUTHWEST BEHAVIORAL HEALTH SERVICES, INC','3450 N 3RD ST','PHOENIX','AZ','85012-2331','MARICOPA COUNTY','UNITED STATES' UNION
SELECT 'f517212b-a575-4d38-b5c9-fe308d30fac9','17038','','','','','','','' UNION
SELECT 'c9f97bc4-8dd8-4fa6-b9db-9dc359f192f4','29753','SP MANAGEMENT CORP.','419 AVE PONCE DE LEON STE 112','SAN JUAN','PR','918','','UNITED STATES' UNION
SELECT '33ee40ac-f825-4ce9-8fb9-9bdf7a1bc43d','8437','SPECTRUM PROPERTIES, LTD','2518 CONVERSE ST','DALLAS','TX','75207-5904','DALLAS COUNTY','UNITED STATES' UNION
SELECT 'a5a7fe82-b123-49d8-8698-29b7742ede30','6430','SPECTRUM REALTY GROUP','1831 W LOUISIANA ST','MCKINNEY','TX','75069-7859','COLLIN COUNTY','UNITED STATES' UNION
SELECT '2183c787-cc52-4024-b752-a6de5bcc5c41','5423','SPM INC','1103 RICHARD ARRINGTON JR BLVD S','BIRMINGHAM','AL','35205-2809','JEFFERSON COUNTY','UNITED STATES' UNION
SELECT '806e07c3-80d3-4e40-b4be-81788a20d980','8319','SPOKANE HOUSING VENTURES','2001 N DIVISION ST STE 100','SPOKANE','WA','99207-2255','SPOKANE COUNTY','UNITED STATES' UNION
SELECT '6154b50c-d406-4183-979e-d3650d33604d','1374878','SPRING LEASING AND MANAGEMENT, LLC','111 GREAT NECK RD STE 308','GREAT NECK','NY','11021-5403','NASSAU COUNTY','UNITED STATES' UNION
SELECT 'c98cd5c0-b22e-4733-ba57-353617f3ad9f','28171','SPRINGPOINT SENIOR LIVING','4814 OUTLOOK DR STE 201','WALL TOWNSHIP','NJ','07753-6812','MONMOUTH COUNTY','UNITED STATES' UNION
SELECT '348af585-85a3-4f70-a868-1f12e24413ad','547','SQUIRETOWN PROPERTIES, LLC','34 WOODLAND RD','ROSELAND','NJ','07068-1237','ESSEX COUNTY','UNITED STATES' UNION
SELECT '29ebecfa-df5c-4521-86c7-d1c9e5609501','5869','SRA MANAGEMENT LLC DBA OLYMPUS PROPERTY','500 THROCKMORTON ST STE 300','FORT WORTH','TX','76102-3745','TARRANT COUNTY','UNITED STATES' UNION
SELECT 'a65aafc5-5914-4640-9180-50d49ef2f81e','1380377','SRAA MANAGEMENT, LLC','1070 FLORENCE RD','SAVANNAH','TN','38372-3441','HARDIN COUNTY','UNITED STATES' UNION
SELECT 'a1de0a6b-46df-4fc7-bfca-7881cbc0a0ca','5260','SSMG, LLC','3651 PEACHTREE PKWY','SUWANEE','GA','30024-6034','FORSYTH COUNTY','UNITED STATES' UNION
SELECT 'c438cb12-d275-4f22-ac82-63d4530e94cd','7962','ST AMBROSE HOUSING AID CENTER','321 E 25TH ST','BALTIMORE','MD','21218-5303','BALTIMORE CITY','UNITED STATES' UNION
SELECT '4a336049-88a1-4d07-a242-158f7ffd5c32','2273','ST FRANCIS CATHOLIC OF BEARDSTOWN NFP','800 S 5TH ST','SPRINGFIELD','IL','62703-2308','SANGAMON COUNTY','UNITED STATES' UNION
SELECT 'ec57f3f5-a762-4652-81b5-4e5d50fccb01','2576','ST VINCENT DE PAUL OF LANE COUNTY','2890 CHAD DR','EUGENE','OR','97408-7336','LANE COUNTY','UNITED STATES' UNION
SELECT 'e7b3c299-4ce5-4475-b0ed-aaa9d7d484da','1374834','STAN RESIDENTIAL LLC','94 SPRUCE ST','CEDARHURST','NY','11516-2018','NASSAU COUNTY','UNITED STATES' UNION
SELECT 'cfd1d5f1-264e-4425-a359-64ed917dda28','1326','STANDARD ENTERPRISES, INC.','3104 BREARD ST','MONROE','LA','71201-5109','OUACHITA PARISH','UNITED STATES' UNION
SELECT '67a42976-eda4-48ec-8e55-3f9dfeeb6a9e','5552','STANFORD MANAGEMENT','245 COMMERCIAL ST','PORTLAND','ME','04101-4606','CUMBERLAND COUNTY','UNITED STATES' UNION
SELECT 'd655844e-778d-419e-a5d4-3bf63d56e0a3','6836','STEADFAST MANAGEMENT COMPANY INC','18100 VON KARMAN AVE STE 500','IRVINE','CA','92612-0196','ORANGE COUNTY','UNITED STATES' UNION
SELECT 'bd5465c4-8424-4ddc-ad4b-7223c329a11c','5821','STEELHEAD MANAGEMENT LLC','1310 ROSENEATH RD STE 200','RICHMOND','VA','23230-4632','RICHMOND CITY','UNITED STATES' UNION
SELECT 'daa5a9cf-e528-4908-836d-2cda9f32d3ee','2842','STEPHENSON & MOORE, INC.','706 TURNBULL AVE STE 103','ALTAMONTE SPRINGS','FL','32701-6476','ORANGE COUNTY','UNITED STATES' UNION
SELECT '9e2bab17-bae6-46ec-8460-c58e896f457e','1380006','','','','','','','' UNION
SELECT '9e37b87f-033f-4292-8b1e-d6369110baa5','8008','STERLING RESIDENTIAL SERVICES','820 SHADES CREEK PKWY STE 2300','BIRMINGHAM','AL','35209-4528','JEFFERSON COUNTY','UNITED STATES' UNION
SELECT '71dba31a-9cf3-44fc-a2b1-436ac420ea3a','1380007','','','','','','','' UNION
SELECT 'd7d49039-787c-4720-990d-614611c99b86','14820','STILES FAMILY PROPERTIES, L.P.','5405 E LOOP 281 S','LONGVIEW','TX','75602-6807','HARRISON COUNTY','UNITED STATES' UNION
SELECT 'd21a32a8-194d-491e-9af4-f30d063a3124','17586','STOA GROUP LLC','1250 SW RAILROAD AVE STE 100A-2','HAMMOND','LA','70403-5001','TANGIPAHOA PARISH','UNITED STATES' UNION
SELECT '758347a4-8460-4590-8679-bb4dac69fc24','15158','STOLTZ MANAGEMENT OF DELAWARE, INC.','725 CONSHOHOCKEN STATE RD','BALA CYNWYD','PA','19004-2122','MONTGOMERY COUNTY','UNITED STATES' UNION
SELECT '928e3625-712e-4f94-942a-712c060f2da8','9314','STONE CREEK REALTY LLC','2700 TRIMMIER RD','KILLEEN','TX','76652','HARRIS COUNTY','UNITED STATES' UNION
SELECT 'd932601d-0610-4316-939d-5430bf14f69d','5877','WATERFORD RESIDENTIAL LLC','760 W MAIN ST STE 140','BARRINGTON','IL','60010-4124','LAKE COUNTY','UNITED STATES' UNION
SELECT 'f493e85b-b3b4-49d6-8781-adcc8304f304','5243','STONEMARK MANAGEMENT, LLC','400 INTERSTATE NORTH PKWY SE STE 1100','ATLANTA','GA','30339-5054','COBB COUNTY','UNITED STATES' UNION
SELECT '1f693f6f-c1b4-4515-86dd-08cde89cb29d','5354','STONEWALL ASSOCIATES, LLC','5116 ELM ST','HOUSTON','TX','77081-2929','HARRIS COUNTY','UNITED STATES' UNION
SELECT 'b56f3ea9-6695-42a2-9226-f80dc8fe376b','6296','','','','','','','' UNION
SELECT 'e2a8b897-ef00-484a-85b0-406ac1448d41','14695','','','','','','','' UNION
SELECT '2b99cc8f-a3d8-4ae5-970a-11584c3c53ca','5868','STUART SHAW FAMILY MANAGEMENT, LLC DBA BONNER CARRINGTON, LLC','901 S MO PAC EXPY BLDG V  STE 100','AUSTIN','TX','78746-5776','TRAVIS COUNTY','UNITED STATES' UNION

SELECT '6fa40335-4627-49f1-9e80-695bd251524d','2085','STUBBLEFIELD CONSTRUCTION CO','2258 BRADFORD AVE','HIGHLAND','CA','92346-2204','SAN BERNARDINO COUNTY','UNITED STATES' UNION
SELECT '77d0fde4-4840-44c2-999a-6b055682788a','370','STUHO LLC','2595 S HOOVER ST STE C','LOS ANGELES','CA','90007-4800','LOS ANGELES COUNTY','UNITED STATES' UNION
SELECT '72401def-6a19-41b4-a5c8-b0ad1fa24f7f','20293','SUDBERRY PROPERTIES, INC.','5465 MOREHOUSE DR STE 260','SAN DIEGO','CA','92121-4714','SAN DIEGO COUNTY','UNITED STATES' UNION
SELECT '4222eeac-1570-4e86-a552-3dd2e2660e49','15170','SUGARHILL','8207 CALLAGHAN RD STE 400','SAN ANTONIO','TX','78230-4799','BEXAR COUNTY','UNITED STATES' UNION
SELECT '6cce83a4-90dd-42f3-87c0-0f433ea0e6e0','2009','SUMAR ENTERPRISES INC.','3838 N SAM HOUSTON PKWY E STE 290','HOUSTON','TX','77032-3400','HARRIS COUNTY','UNITED STATES' UNION
SELECT 'd991f33c-9e84-4529-903a-5b4a72fa2557','2074','SUMMIT MANAGEMENT, LLC','6770 STILLWATER BLVD N STE 110','STILLWATER','MN','55082-5440','WASHINGTON COUNTY','UNITED STATES' UNION
SELECT '2e1187a9-d00f-4c5b-b721-3f84bde589e7','20933','SUMMIT POINTE CONDOMINIUM ASSOC','1604 SUMMIT POINTE','SCRANTON','PA','18508-1035','LACKAWANNA COUNTY','UNITED STATES' UNION
SELECT '43844632-05ca-425d-bf63-f733ab206f73','1380606','SUNBELT MULTIFAMILY PROPERTIES, LLC','11640 ARBOR ST STE 202','OMAHA','NE','68144-5007','DOUGLAS COUNTY','UNITED STATES' UNION
SELECT 'a3024a80-eb0c-4e02-8daf-2c975fc31524','1371924','SUNBELT PROPERTIES','212 HEATHER RIDGE DR','FAYETTEVILLE','NC','28311-7026','CUMBERLAND COUNTY','UNITED STATES' UNION
SELECT 'fc9a4f0e-c7da-40f7-94c0-8653c0cd0861','859','SUNCHASE AMERICAN LTD','3514 DRAWBRIDGE PKWY STE A','GREENSBORO','NC','27410-8584','GUILFORD COUNTY','UNITED STATES' UNION
SELECT '7a8af59e-f51b-42ad-9d6c-c97c07e1f7a3','16257','SUNDANCE PROPERTY MANAGEMENT, LLC','9918 CARVER RD STE 110','BLUE ASH','OH','45242-5530','HAMILTON COUNTY','UNITED STATES' UNION
SELECT '91f42955-9641-419b-9fa3-2907a754af97','6690','SUNQUEST PROPERTIES','3 BANCROFT CIR','MONROE','LA','71201-5101','OUACHITA PARISH','UNITED STATES' UNION
SELECT '67678dc1-d603-4e9a-ba8f-a74bbe6e0973','18543','SUNREX MANAGEMENT','1209-21 ROSLYN RD','WINNIPEG','MB','R3L2S8','','CANADA' UNION
SELECT '7d97a6b3-4e3f-4ea7-8eb5-60cda2d84eb6','5875','SUNRIDGE MANAGEMENT GROUP, INC.','1605 LYNDON B JOHNSON FWY STE 250','DALLAS','TX','75234-6802','DALLAS COUNTY','UNITED STATES' UNION
SELECT '7de53df9-8d5d-45cd-9de2-0d1d9cc2df19','2580','SUNSHINE RETIREMENT LIVING LLC','1080 SW MT BACHELOR DR SUITE #200','BEND','OR','97702','','UNITED STATES' UNION
SELECT '5ced9d2f-6379-4841-aefe-0fda68fc05b4','6698','SUNSTATES MANAGEMENT','723 HOWARD AVE','BILOXI','MS','39530-4305','HARRISON COUNTY','UNITED STATES' UNION
SELECT 'e67c79a7-0e19-4590-aa19-368718f3141b','532','','','','','','','' UNION
SELECT '8274e477-83d9-4416-9ea5-92f82d0c4c5a','1380008','','','','','','','' UNION
SELECT '42252cdf-535d-4c13-bd76-9e3481df3beb','1374829','SVRE ACQUISITION, LLC','216 QUINOBEQUIN RD','WABAN','MA','02468-1817','MIDDLESEX COUNTY','UNITED STATES' UNION
SELECT '032daee0-a9c8-4bec-af23-e1964179e0e5','8078','SWEETWATER MANAGEMENT, LLC','10 WALNUT LN','NORTH AUGUSTA','SC','29860-9386','AIKEN COUNTY','UNITED STATES' UNION
SELECT '43f5183f-1afd-46c0-849f-8b7ff48ca5dc','25346','SWEZY REALTY','7735 NW 146TH ST STE 306','MIAMI LAKES','FL','33016-1583','MIAMI-DADE COUNTY','UNITED STATES' UNION
SELECT '5f49efad-e448-43a0-a464-356c3a688cc5','30778','SWORDS TO PLOWSHARES: VETERANS RIGHTS ORGANIZATION','1060 HOWARD ST','SAN FRANCISCO','CA','94103-2820','SAN FRANCISCO COUNTY','UNITED STATES' UNION
SELECT '4db2b4ea-1e58-4556-bd98-2845cc39723b','1379277','','','','','','','' UNION
SELECT 'b1f01eaf-3ab4-4727-b475-b17265fc5138','20651','','','','','','','' UNION
SELECT '3cd9668e-2f01-4aa5-ac41-543c00b52577','6292','T AND L PROPERTIES LLC','1909 SYCAMORE ST','GRANGER','IA','50109-9733','DALLAS COUNTY','UNITED STATES' UNION
SELECT '414a0153-8f0b-47f6-a523-28fe8eaec726','1479','TACOLCY PROPERTY MANAGEMENT INC','5900 NW 7TH AVE STE 102','MIAMI','FL','33127-1267','MIAMI-DADE COUNTY','UNITED STATES' UNION
SELECT 'f53b264b-db49-4531-9f49-f0daf55bd46e','152','TAILWIND MANAGEMENT INC','530 S FRONT ST STE 100','MANKATO','MN','56001-3850','BLUE EARTH COUNTY','UNITED STATES' UNION
SELECT 'f6078541-e5e9-4146-92e6-6441051778ba','4205','TANNER PROPERTIES','4420 S 18TH PL','PHOENIX','AZ','85040-2467','MARICOPA COUNTY','UNITED STATES' UNION
SELECT 'fbd69dfa-21ef-4d15-8e2e-e42fefe45492','5901','TARANTINO PROPERTIES','7887 SAN FELIPE ST STE 237','HOUSTON','TX','77063-1621','HARRIS COUNTY','UNITED STATES' UNION
SELECT '3f357eb7-9f79-4e2c-b2ee-2229f33e5eb4','5623','TARGA REAL ESTATE','720 S 348TH ST # A-2','FEDERAL WAY','WA','98003-7000','KING COUNTY','UNITED STATES' UNION
SELECT 'e38455fc-9d6c-41b7-9fcd-bdc864dc5d4c','6500','TARRAGON PROPERTY SERVICES','1302 PUYALLUP ST','SUMNER','WA','98390-1604','PIERCE COUNTY','UNITED STATES' UNION
SELECT '7561aaa7-e85a-4e34-a508-b1fca2f67e9c','20717','TAURUS INVESTMENT HOLDINGS, LLC','2 INTERNATIONAL PL FL 27','BOSTON','MA','02110-4104','SUFFOLK COUNTY','UNITED STATES' UNION
SELECT '9103ca20-70b2-4bd2-b431-bb20406bee0d','2672','TAYLOR - HILL PROPERTIES LLC','333 WASHINGTON BLVD','ABILENE','TX','79601-5465','','United States' UNION
SELECT '6180a8d9-74b3-4a5f-b614-9818b5838aed','23','TAYLOR MANAGEMENT COMPANY','80 S JEFFERSON RD 2ND FL','WHIPPANY','NJ','07981-1056','MORRIS COUNTY','UNITED STATES' UNION
SELECT '80d3b1f2-345d-473c-a93c-8533be3b24a2','1377689','TAYLOR REAL ESTATE & MANAGEMENT, LLC','876 WELCOME WAY SE STE 200','SALEM','OR','97302-4092','MARION COUNTY','UNITED STATES' UNION
SELECT 'a7902e2e-3a11-4efc-81b3-d1eb3b42a1fc','4249','TB AND HE MANAGEMENT COMPANY, INC','19 SHELTER COVE LN STE 300','HILTON HEAD ISLAND','SC','29928-3549','BEAUFORT COUNTY','UNITED STATES' UNION
SELECT '70ad2d15-48c7-4d09-aa00-d5bdaafe5e21','1353','TDC MANAGEMENT CO., LTD','3411 RICHMOND AVE STE 500','HOUSTON','TX','77046-3409','HARRIS COUNTY','UNITED STATES' UNION
SELECT '9bcbda96-ad8a-4535-a781-0d31f400d1ab','5303','TECTON CORPORATION','900 SW 16TH ST STE 210','RENTON','WA','98057-2631','KING COUNTY','UNITED STATES' UNION
SELECT 'bcb4a45f-15d9-4cdd-958b-5e9fbf1b3117','1241','TELACU RESIDENTIAL MGMT','1248 GOODRICH BLVD','LOS ANGELES','CA','90022-5107','LOS ANGELES COUNTY','UNITED STATES' UNION
SELECT 'ed6b24fc-743e-4ca9-9dc1-81dce58cca34','1358','TENANTS DEVELOPMENT CORP.','1 APPLETON ST STE 301','BOSTON','MA','02116-5223','SUFFOLK COUNTY','UNITED STATES' UNION
SELECT '40c34c8c-62b8-4fd3-9e59-599524f0d1bf','1379651','TENEMENT MANAGING GROUP LLC','7726 SAINT LAWRENCE AVE','PITTSBURGH','PA','15218-2139','ALLEGHENY COUNTY','UNITED STATES' UNION
SELECT 'bbd0c157-49d4-453b-a169-2a8c8121f044','1262','TERRA WEST, LLC','5091 BAUMGARTNER RD','SAINT LOUIS','MO','63129-2821','ST. LOUIS COUNTY','UNITED STATES' UNION
SELECT '816d550d-644b-402a-acab-23ab68db09d6','2831','TERRACORP FINANCIAL INC','1043 STUART ST STE 220','LAFAYETTE','CA','94549-4019','CONTRA COSTA COUNTY','United States' UNION
SELECT '03bd581e-7641-432d-8938-60f8c2753163','1175','TESCO PROPERTIES','2171 JUDICIAL DR STE 200','GERMANTOWN','TN','38138-3800','SHELBY COUNTY','UNITED STATES' UNION
SELECT 'ce9556b9-5d38-46c5-97f5-068a87f96b9d','383','TEXAS HOUSING FOUNDATION','1110 BROADWAY ST','MARBLE FALLS','TX','78654-5504','BURNET COUNTY','UNITED STATES' UNION
SELECT '0601a649-d3f5-4841-acf4-132c70c040f2','384','','','','','','','' UNION
SELECT '1a94e5ce-2e5f-4c6e-a457-40f5fc806138','5271','TEXLA HOUSING PARTNERS INC.','113 HEYMANN BLVD STE 7-2','LAFAYETTE','LA','70503-2398','LAFAYETTE PARISH','UNITED STATES' UNION
SELECT '50ff03d9-11a8-4710-8837-d38997cee774','1114','THALHIMER','11100 W BROAD ST','GLEN ALLEN','VA','23060-5813','HENRICO COUNTY','UNITED STATES' UNION
SELECT 'a2df23c2-f804-486c-9a25-3202dd597905','5518','THE ABEL-BISHOP AND CLARKE REALTY CO DBA ABC MANAGEMENT','23925 COMMERCE PARK','BEACHWOOD','OH','44122-5821','CUYAHOGA COUNTY','UNITED STATES' UNION
SELECT '65c134fd-383e-4135-baec-d53cb1712fc6','2298','THE ABRAMS MGMT CO INC','621 COLUMBUS AVE','BOSTON','MA','02118-1058','SUFFOLK COUNTY','UNITED STATES' UNION
SELECT '8285f917-1162-4b20-bf13-1a729354b4ba','5560','THE ALEXANDER COMPANY','345 W WASHINGTON AVE STE 301','MADISON','WI','53703-3007','DANE COUNTY','UNITED STATES' UNION
SELECT '2fd6c2b3-82ca-4c05-bc62-3a38a9786a63','8950','THE APARTMENTS AT ABERDEEN STATION','1000 CENTRAL AVE','ABERDEEN','NJ','07747-1061','MONMOUTH COUNTY','UNITED STATES' UNION
SELECT '8cb397ef-7e45-4310-8799-e3ec5f07d5c6','1315','THE ARDIZZONE GROUP MANAGEMENT COMPANY INC.','4101 CASHARD AVE STE 100','INDIANAPOLIS','IN','46203-6040','MARION COUNTY','UNITED STATES' UNION
SELECT '211ff61c-93e7-408a-b610-cf13b7cbe7c6','1942','THE BRIDGE INC.','248 W 108TH ST','NEW YORK','NY','10025-2956','NEW YORK COUNTY','UNITED STATES' UNION
SELECT 'a83cce6c-a4ab-4f79-aae8-b370589cfbc7','6593','THE CABOT GROUP','130 LINDEN OAKS','ROCHESTER','NY','14625-2834','','UNITED STATES' UNION
SELECT '5c6543d6-0208-4f7e-ad2c-fdeba67e0f3a','1377885','THE CAPPER FOUNDATION','1500 E 8TH AVE STE 201','WINFIELD','KS','67156-3104','COWLEY COUNTY','UNITED STATES' UNION
SELECT '519b58f6-e14b-4e31-862a-fea43db2a5f1','20675','','','','','','','' UNION
SELECT '2427c08e-02a5-4275-973a-2232422e8d25','5221','THE CARSWELL GROUP, INC DBA INDEPENDENT MANAGEMENT SERVICES','14381 NORTH RD','FENTON','MI','48430-1336','GENESEE COUNTY','UNITED STATES' UNION
SELECT '3725dddf-5480-4871-9e21-04b9f5cf28a3','1367656','THE CENTER FOR DEVELOPMENT AND PROPERTY SOLUTIONS','3245 E LIVINGSTON AVE STE 202','COLUMBUS','OH','43227-1943','FRANKLIN COUNTY','UNITED STATES' UNION
SELECT '4adb32e0-a797-4bf2-8215-e254f91f03e9','4674','THE CITY MISSION SOCIETY INC','100 E TUPPER ST','BUFFALO','NY','14203-1315','ERIE COUNTY','UNITED STATES' UNION
SELECT '4a6fd2ea-bdd1-4a16-a34c-2e7200b840fe','6391','THE COLUMBIA PROPERTY GROUP','701 BIENVILLE BLVD','OCEAN SPRINGS','MS','39564-2842','JACKSON COUNTY','UNITED STATES' UNION
SELECT '98706a90-a29e-4008-9dac-a8057cdace47','1172','THE CONNOR GROUP','10510 SPRINGBORO PIKE','MIAMISBURG','OH','45342-4956','MONTGOMERY COUNTY','UNITED STATES' UNION
SELECT 'fe553ff2-d518-4612-abc9-16de0f282fd1','2782','','','','','','','' UNION
SELECT 'c0ca8878-1c5e-4f05-813b-9a3f47e1b068','1252','THE CYPRESS COMPANIES','12221 MERIT DR STE 900','DALLAS','TX','75251-2212','DALLAS COUNTY','UNITED STATES' UNION
SELECT 'c9797c5a-c9c7-4560-8667-a164609d9a1d','25256','THE DEBRULER COMPANY','131 E PARK AVE','LIBERTYVILLE','IL','60048-2800','LAKE COUNTY','UNITED STATES' UNION
SELECT 'd9eacd6f-c495-4fb9-b2dc-392d93cd7bfa','20221','THE DISPATCH PRINTING COMPANY','34 S 3RD ST','COLUMBUS','OH','43215-4201','FRANKLIN COUNTY','UNITED STATES' UNION
SELECT 'cdc4ec37-887d-49db-8b39-71c477b190eb','219','THE DOLBEN COMPANY, INC., MANAGING AGENT','150 PRESIDENTIAL WAY STE 220','WOBURN','MA','01801-1121','MIDDLESEX COUNTY','UNITED STATES' UNION
SELECT '1cf0760d-851c-49ce-8096-8727c9f0c488','6511','THE DONALDSON GROUP','15245 SHADY GROVE RD STE 160','ROCKVILLE','MD','20850-6232','','UNITED STATES' UNION
SELECT '976e614a-6bf8-4af2-ad7d-9ded8c69c410','6645','THE EMBASSY GROUP','382A ROUTE 59 STE 101','AIRMONT','NY','10952-3440','ROCKLAND COUNTY','UNITED STATES' UNION
SELECT 'f94b9378-e3ad-42e6-99b8-5fe67d865a81','8263','THE FERNDALE PROPERTY MANAGEMENT GROUP, LLC','6650 GUNPARK DR STE 100','BOULDER','CO','80301-7003','BOULDER COUNTY','UNITED STATES' UNION
SELECT 'd4658a6a-c2d2-4546-b88d-ac0528cedb56','2402','THE GOODMAN GROUP, LLC','1107 HAZELTINE BLVD STE 200','CHASKA','MN','55318-1070','CARVER COUNTY','UNITED STATES' UNION
SELECT '2cbf84c6-87b6-4407-a8cb-5689907950e8','6317','THE GRIFFIN COMPANY','920 WOODMONT BLVD','NASHVILLE','TN','37204-3300','DAVIDSON COUNTY','UNITED STATES' UNION
SELECT 'bebd4c58-540f-4849-93b8-1912e8c5e45c','4609','THE GROVE STUDENT PROPERTIES, LLC','2100 REXFORD RD STE 414','CHARLOTTE','NC','28211-3484','','UNITED STATES' UNION
SELECT '9956efa7-79d7-4af4-9e32-9491349ea2e8','35','THE HAMILTON COMPANY','39 BRIGHTON AVE','ALLSTON','MA','02134-2301','SUFFOLK COUNTY','UNITED STATES' UNION
SELECT 'dc3165f2-2df3-41eb-a911-fbb5ebddfd70','22388','THE HOUSING AUTHORITY OF THE CITY OF MCCOMB, MS','1002 SEDGEWICK ST','MCCOMB','MS','39648-5058','PIKE COUNTY','UNITED STATES' UNION
SELECT 'd4170d30-8d37-48ed-a1e6-5638dd2f7548','1379538','THE HOUSING AUTHORITY OF THE CITY OF WINTER PARK','718 MARGARET SQ','WINTER PARK','FL','32789-1932','ORANGE COUNTY','UNITED STATES' UNION
SELECT '6e356c95-48c6-4f9f-8942-94ec068461a5','8281','THE HOUSING COMPANY','565 W MYRTLE ST STE 250','BOISE','ID','83702-7684','ADA COUNTY','UNITED STATES' UNION
SELECT '51478776-1c2a-4227-a828-9b3d34a3aacd','1375568','','','','','','','' UNION
SELECT '481602ba-6344-42fb-84f7-c9e80db22607','24433','','','','','','','' UNION
SELECT '67c26b75-1398-4b5e-b784-14e01849b188','6665','THE LIGHTHOUSE GROUP','881 ALMA REAL DR STE 300','PACIFIC PALISADES','CA','90272-5044','LOS ANGELES COUNTY','UNITED STATES' UNION
SELECT 'a0ab5166-a909-4da1-aebd-d7d7c3464821','6748','THE LIGHTSTONE GROUP, LLC','1985 CEDAR BRIDGE AVE STE 1','LAKEWOOD','NJ','08701-7031','OCEAN COUNTY','UNITED STATES' UNION
SELECT '9e5e0370-c1d8-4230-9d07-759eec0f69a6','6140','THE LYND COMPANY','4499 POND HILL RD','SAN ANTONIO','TX','78231-1292','BEXAR COUNTY','UNITED STATES' UNION
SELECT '0fbe33ea-5fe6-476e-b539-5ccb502a460a','14760','THE MAHAFFEY APARTMENT COMPANY','147 2ND AVE S STE 300','ST PETERSBURG','FL','33701-4393','PINELLAS COUNTY','UNITED STATES' UNION
SELECT '6bb7fde4-e0e6-4561-b023-fcb1b328d407','2652','THE MARTINO GROUP','525 S LOOP 288 STE 105','DENTON','TX','76205-4508','DENTON COUNTY','UNITED STATES' UNION
SELECT '4d5f52ef-46db-405e-9ed2-112d276cc556','6334','THE MEDVE GROUP  INC','1411 W WALNUT HILL LN','IRVING','TX','75038-3010','DALLAS COUNTY','UNITED STATES' UNION
SELECT '022034a0-b79b-4850-bb5d-1d609818e136','7978','THE MERKLE LLC','1930 NEW HAMPSHIRE AVE NW APT 14','WASHINGTON','DC','20009-3348','DISTRICT OF COLUMBIA','UNITED STATES' UNION
SELECT '51bc618f-e3a4-4193-9b3b-4428e174c789','5774','THE MITCHELL COMPANY, LLC','41 W I65 SERVICE RD N STE 300','MOBILE','AL','36608-1206','MOBILE COUNTY','UNITED STATES' UNION
SELECT 'c93cb625-3857-4c2c-8cea-cd2bd6b1dfac','6339','THE PAUL SACK PROPERTIES II INC.','111 PINE ST STE 1600','SAN FRANCISCO','CA','94111-5618','SAN FRANCISCO COUNTY','UNITED STATES' UNION
SELECT '3a342d14-fa0f-460b-8271-9f28c32def8f','15729','','','','','','','' UNION
SELECT '5c784e5d-2fd0-4b08-ae3c-d7639fdd7578','1380733','THE PROPERTY CENTER, L.L.C.','2905 NW 156TH ST','EDMOND','OK','73013-2101','OKLAHOMA COUNTY','UNITED STATES' UNION
SELECT '15fe74f0-56c1-4932-baaf-e1d593909164','1380009','','','','','','','' UNION
SELECT 'da4a7863-ba1c-41c5-bc30-4e3fa95e2bba','6871','RADCO','400 GALLERIA PKWY SE STE 400','ATLANTA','GA','30339','','UNITED STATES' UNION
SELECT '3e2fa4a6-5a63-417f-8a50-5784e8a6d379','1375418','','','','','','','' UNION
SELECT '370e9c38-b816-4a54-8a85-e70d1762ab13','1380010','','','','','','','' UNION
SELECT 'ad75860a-3ca6-4ceb-85e1-3312fa956891','6872','THE RICHDALE GROUP','10040 REGENCY CIR','OMAHA','NE','68114-3723','DOUGLAS COUNTY','UNITED STATES' UNION
SELECT '34f32243-1239-45e0-bd8d-56a34446586c','154','THE ROBERTS CO.','2886 COLORADO AVE','SANTA MONICA','CA','90404-3661','LOS ANGELES COUNTY','UNITED STATES' UNION
SELECT '9ea95ee6-5e4f-41e0-8d7e-69b6a7f068f1','9353','','','','','','','' UNION
SELECT '5b065be1-d0b0-4626-b462-345973eeef83','3380','THE SALVATION ARMY','1221 RIVER BEND DR','DALLAS','TX','75247-4911','DALLAS COUNTY','UNITED STATES' UNION
SELECT 'c6386cca-6a00-4e98-9121-2afbf3788518','1272','THE SIMON COMPANIES','639 GRANITE ST','BRAINTREE','MA','02184-5366','NORFOLK COUNTY','UNITED STATES' UNION
SELECT '67930bfc-5e6f-4356-bc01-eca88d219bfe','1276','THE SOLOMON ORGANIZATION, LLC','92 RIVER RD','SUMMIT','NJ','07901-1443','','UNITED STATES' UNION
SELECT '3fd1b8c2-8306-42f6-9bbe-7ba293e04220','802','THE SOUTHERN GROUP LLC','109 LYON ST','MCMINNVILLE','TN','37110-2545','WARREN COUNTY','UNITED STATES' UNION
SELECT 'f9f675a1-e79d-43bf-935c-d76f74b17fb9','4288','THE STREAMWOOD COMPANY','30 WASHINGTON AVE STE A','HADDONFIELD','NJ','08033-3341','CAMDEN COUNTY','UNITED STATES' UNION
SELECT '82c80415-a33e-4446-9a1c-0839880a0975','1379649','THE UNIVERSITY OF NORTH TEXAS HEALTH SCIENCE CENTER AT FORT WORTH','3500 CAMP BOWIE BLVD','FORT WORTH','TX','76107-2644','TARRANT COUNTY','UNITED STATES' UNION
SELECT '058919b7-6473-4719-9d28-684f6bd00886','2451','','','','','','','' UNION
SELECT 'b4ba5be8-5a8c-4dd1-b024-894261a5b118','1378627','THIEN THAN LLC','10111 BISSONNET ST OFC','HOUSTON','TX','77036-7827','HARRIS COUNTY','UNITED STATES' UNION
SELECT '9848aede-c372-4bc7-9bdd-6c94f257d5da','6089','THIES & TALLE MANAGEMENT INC','470 W 78TH ST STE 260','CHANHASSEN','MN','55317-9745','CARVER COUNTY','UNITED STATES' UNION
SELECT 'faac60bc-6c9b-42de-9ef8-37b3160e2182','1280','THOMAS C MERCER','3653 BRIARGROVE LN APT 1211','DALLAS','TX','75287-6131','DENTON COUNTY','UNITED STATES' UNION
SELECT 'cedcbc4e-7160-40be-b414-cc244bdb566f','1380210','THOMAS VENTURES, LLC','4900 PLAZA DR','MONTGOMERY','AL','36116-2629','MONTGOMERY COUNTY','UNITED STATES' UNION
SELECT 'd5708845-07f4-4e44-8325-d8ef8848186f','6926','THOMPSON THRIFT DEVELOPMENT INC','901 WABASH AVE STE 300','TERRE HAUTE','IN','47807-3233','VIGO COUNTY','UNITED STATES' UNION
SELECT 'b4b11918-fab6-49e1-9c6a-3d1d94f0d2d9','15165','THORNTON APARTMENTS LP','2003 S 1ST ST','AUSTIN','TX','78704-5141','TRAVIS COUNTY','UNITED STATES' UNION
SELECT '74fe97a4-d7d2-4225-b7ae-56c2cf88fd25','798','TIG MANAGEMENT, LLC','265 N JOY ST STE 200','CORONA','CA','92879-0600','RIVERSIDE COUNTY','UNITED STATES' UNION
SELECT '0e84ce10-324a-496b-8f6c-a4d408609d73','1380565','TILDEN HOUSING DEVELOPMENT CORPORATION','600 S GILES CREEK LN','TILDEN','NE','68781-4776','ANTELOPE COUNTY','UNITED STATES' UNION
SELECT 'fd35ce06-791d-4a84-8469-a67a70f9ec36','1073','TIMBERLAND PARTNERS','8500 NORMANDALE LAKE BLVD STE 700','MINNEAPOLIS','MN','55437-3813','HENNEPIN COUNTY','UNITED STATES' UNION
SELECT 'b61ff34d-5b91-47f1-abf9-1263d09fbdfc','5916','TIPTON GROUP, THE','6529 PRESTON RD STE 100','PLANO','TX','75024-2610','COLLIN COUNTY','UNITED STATES' UNION
SELECT '8e558813-6e9b-4da4-b116-aea710b569f4','5801','TISHAU GP, LLC','405 EL CAMINO REAL # 220','MENLO PARK','CA','94025-5240','SAN MATEO COUNTY','UNITED STATES' UNION
SELECT '2f84da26-1282-451a-87c7-cf219ffe4ca5','20780','','','','','','','' UNION
SELECT '4ca6ea0d-4c4c-45de-871a-7a9d98344327','5874','TM REALTY SERVICES, LLC','135 SAN LORENZO AVE STE 770','CORAL GABLES','FL','33146-1878','MIAMI-DADE COUNTY','UNITED STATES' UNION
SELECT '9a6c273f-3451-4c4a-a4e5-5bd4a663664f','2585','TN DEVELOPMENT CORP','1225 LADY ST STE 201','COLUMBIA','SC','29201-3347','RICHLAND COUNTY','UNITED STATES' UNION
SELECT '8c085e78-f3ea-4cbf-92d1-5611ce15e5bf','8147','TOLL BROS INC','250 GIBRALTAR RD','HORSHAM','PA','19044-2323','MONTGOMERY COUNTY','UNITED STATES' UNION
SELECT '814fe164-cf0e-458d-9ffb-a3236e02ba36','5796','TOP CHOICE PROPERTIES, LLC DBA ALLIED PROPERTY MANAGEMENT','1900 COUNTRY CLUB DR STE 120','MANSFIELD','TX','76063-2630','TARRANT COUNTY','UNITED STATES' UNION
SELECT 'caabd342-6f37-402d-a198-fb550bb3ff25','212','TOPOROVSKY & SONS REALTY CORP','179 CEDAR LN STE H','TEANECK','NJ','07666-4304','BERGEN COUNTY','UNITED STATES' UNION
SELECT '74d2a743-99c1-412d-9350-d8750b9b985b','5273','TOUCHMARK LLC DBA TOUCHMARK','5150 SW GRIFFITH DR','BEAVERTON','OR','97005-2935','WASHINGTON COUNTY','UNITED STATES' UNION
SELECT '0d0d7eb9-03da-4cfd-acc7-9bfe5f3de1c5','1380011','','','','','','','' UNION
SELECT '9b23d112-6640-4b5a-84c2-a54bc5240017','6847','TPG MANAGEMENT, LLC','735 STATE ST STE 416','SANTA BARBARA','CA','93101-5552','','UNITED STATES' UNION
SELECT 'cfa01f11-8974-4df3-8f94-a93df690de44','25045','TRANSITIONAL HOUSING CORPORATION DBA HOUSING UP','5101 16TH ST NW','WASHINGTON','DC','20011-3847','DISTRICT OF COLUMBIA','UNITED STATES' UNION
SELECT 'd374e7bf-fae3-4b1e-8e58-68f11952f4c4','1376938','TRASON GLOBAL REALTY & HOTEL MANAGEMENT, LLC','24 MAPLE ST','MARCELLUS','NY','13108-1257','ONONDAGA COUNTY','UNITED STATES' UNION
SELECT '21c28740-0bfd-4e6e-9995-d51f5dc357cf','641','TREK DEVELOPMENT GROUP','130 7TH ST STE 300','PITTSBURGH','PA','15222-3409','ALLEGHENY COUNTY','UNITED STATES' UNION
SELECT '44addfce-bba8-4f7d-8dd5-b23fcaa65aea','1380526','TREYSTAR','241 E MICHIGAN AVE STE 135','KALAMAZOO','MI','49007-3909','KALAMAZOO COUNTY','UNITED STATES' UNION
SELECT '89a51959-d38b-4098-a923-ef7cc5d8f745','6219','TRG MANAGEMENT COMPANY LLP','2200 N COMMERCE PKWY STE 100','WESTON','FL','33326-3258','BROWARD COUNTY','UNITED STATES' UNION
SELECT 'f3d4c348-c053-4eb1-9a53-4aa588bcadb0','1897','TRIANGLE MANAGEMENT LLC','102 FIRST CHOICE DR','MADISON','MS','39110-7067','MADISON COUNTY','UNITED STATES' UNION
SELECT '59c5b82e-3989-4db9-aa01-9e5d1dead5d2','29588','TRIBECA MDR MANAGEMENT, LLC','4100 DEL REY AVE','MARINA DEL REY','CA','90292-5604','LOS ANGELES COUNTY','UNITED STATES' UNION
SELECT '074562ab-3141-4a45-9f57-536c7636f723','5229','TRICAP CHICAGO, LLC','171 N ABERDEEN ST STE 400','CHICAGO','IL','60607-1669','COOK COUNTY','UNITED STATES' UNION
SELECT '52328503-61f0-4cad-bf21-2f2f5919eac4','2027','TRICAP MANAGEMENT INC','8383 WILSHIRE BLVD STE 214','BEVERLY HILLS','CA','90211-2432','LOS ANGELES COUNTY','UNITED STATES' UNION
SELECT 'b21461de-32d9-4060-aeaa-747fca16b831','533','','','','','','','' UNION
SELECT 'f352530b-8588-4839-9f1e-acf45fe19db9','1375471','TRI-KO, INC.','301 1ST ST','OSAWATOMIE','KS','66064-1810','MIAMI COUNTY','UNITED STATES' UNION
SELECT '4a60930a-ba0f-4d5d-a9cf-0c7add369e4a','1970','TRILAR MANAGEMENT GROUP','2101 CAMINO VIDA ROBLE STE A','CARLSBAD','CA','92009-1446','SAN DIEGO COUNTY','UNITED STATES' UNION
SELECT 'eb91826c-188b-47fe-a0fa-ed79086fcb25','8433','TRILLIST MANAGEMENT, LLC','1360 PEACHTREE ST NE STE 750','ATLANTA','GA','30309-3233','FULTON COUNTY','UNITED STATES' UNION
SELECT 'bd717a97-550c-4e74-9a3f-e73d740230d0','412','TRILOGY REAL ESTATE GROUP','520 W ERIE ST STE 100','CHICAGO','IL','60654-5700','COOK COUNTY','UNITED STATES' UNION
SELECT 'd4c42c62-efd4-4aeb-9a90-da699274b4e8','1212','','','','','','','' UNION
SELECT '3efe95d6-d4c0-49c0-ab0f-7bc066402891','5955','TRIO PROPERTIES, LLC','2461 MAIN ST STE 2','GLASTONBURY','CT','06033-2083','HARTFORD COUNTY','UNITED STATES' UNION
SELECT '438cb901-61c1-4652-8288-a97f671bdf74','1367090','TRIP REALTY MANAGEMENT CORPORATION','415 RIVER ST. 3RD FLOOR','TROY','NY','12180-2834','','UNITED STATES' UNION
SELECT '11d143ea-6dfe-4a77-aa17-7a498e2c189d','23695','','','','','','','' UNION
SELECT 'ecf8cfbe-376c-4055-9321-a54b51e56d62','1117','TRIUMPH HOUSING MANAGEMENT, LLC','5920 ODELL ST STE 201','CUMMING','GA','30040-5703','FORSYTH COUNTY','UNITED STATES' UNION
SELECT '8c390c1b-46c2-4ef1-aad2-d3565b2efad2','1974','TRIVEST MCNEIL REAL ESTATE LLC','1901 N CENTRAL EXPY STE 225','RICHARDSON','TX','75080-3780','DALLAS COUNTY','UNITED STATES' UNION
SELECT '4bdcddef-5dc3-48e9-9cd0-102b875c6ad9','17620','TRUAMERICA','10100 SANTA MONICA BLVD STE 400','LOS ANGELES','CA','90067-4108','LOS ANGELES COUNTY','UNITED STATES' UNION
SELECT '831dd942-5ced-4e0c-b074-0785f647ff21','22926','TRUE GRIT ASSET ADMINISTRATION, LLC','1201 HAMPTON ST STE 3A','COLUMBIA','SC','29201-2865','RICHLAND COUNTY','UNITED STATES' UNION
SELECT '8f3de495-f615-47c7-b5f9-046ea7d4ae78','1371643','TULSA PROPERTY GROUP LEASING AND MANAGEMENT, INC.','5401 S SHERIDAN RD STE 108','TULSA','OK','74145-7524','TULSA COUNTY','UNITED STATES' UNION
SELECT 'a7119f84-b94f-45e9-a4ab-25a19c767828','23132','TUNIC GROUP LLC','7 GLENWOOD AVE','ORANGE','NJ','07050-4003','ESSEX COUNTY','UNITED STATES' UNION
SELECT '387c5e66-62e4-48d8-9a0a-9616bc99913a','1379156','TURNING POINT COMMONS, INC.','25 VIA LA PAZ','CHICO','CA','95928-4192','BUTTE COUNTY','UNITED STATES' UNION
SELECT '675a85ca-c415-46d9-8149-0bf8a5197c2b','1365','TVP NON PROFIT CORP','151 S PRADO RD','EL PASO','TX','79907-6164','EL PASO COUNTY','UNITED STATES' UNION
SELECT '5cdac964-5edf-4d35-be08-66035e3840b0','966','TWG MANAGEMENT, LLC','1301 E WASHINGTON ST STE 100','INDIANAPOLIS','IN','46202-3849','MARION COUNTY','UNITED STATES' UNION
SELECT '702470c6-3a50-425c-ab53-eca0fad8d679','6951','TWINROCK MANAGEMENT, INC','180 NEWPORT CENTER DR STE 230','NEWPORT BEACH','CA','92660-0903','ORANGE COUNTY','UNITED STATES' UNION
SELECT '871c4453-f56d-442f-bc24-0c272819ed67','6166','UAG VENTURES AC, L.P. DBA UNITED APARTMENT GROUP','7334 BLANCO RD STE 300','SAN ANTONIO','TX','78216-4350','','UNITED STATES' UNION
SELECT '3618d773-6190-4037-83f2-8af1516b7f98','6627','','','','','','','' UNION
SELECT '07fc8b6c-c775-46ed-a97d-5e3406203e68','1019','','','','','','','' UNION
SELECT '0fd0805b-f604-4ca4-aaa0-5b187fd9a4a4','647','','','','','','','' UNION
SELECT 'ac676ec2-5afb-41c4-b332-f3eb7e3b0a27','5308','UDR, INC','1745 SHEA CENTER DR STE 200','HIGHLANDS RANCH','CO','80129-1540','DOUGLAS COUNTY','UNITED STATES' UNION
SELECT 'cec8905b-cda6-462e-8964-e92d7bd47cec','751','UHM PROPERTIES LLC','530 WARREN ST','DORCHESTER','MA','02121-1821','SUFFOLK COUNTY','UNITED STATES' UNION
SELECT 'de118441-5d00-4c9c-89e1-89efc1f482f9','1374457','UNIFIED INVESTMENTS LLC','609 8TH AVE','GREELEY','CO','80631-3970','WELD COUNTY','UNITED STATES' UNION
SELECT '8b47e496-10b5-4cb1-9418-656715a2c41b','5095','UNITED HOUSING ASSOCIATES','1345 GARNER LN STE 103','COLUMBIA','SC','29210-8301','RICHLAND COUNTY','UNITED STATES' UNION
SELECT 'e6df0f30-2614-4418-919c-ef477696307b','7887','UNITED NEIGHBORHOOD COMMUNITY DEV. CORP.','777 KEYSTONE INDUSTRIAL PARK','DUNMORE','PA','18512-1530','LACKAWANNA COUNTY','UNITED STATES' UNION
SELECT '8bff9871-890d-4e00-a6c7-cd2c6215932a','6461','UNIVERSAL DEVELOPMENT','1607 MOTOR INN DR','GIRARD','OH','44420-2486','TRUMBULL COUNTY','UNITED STATES' UNION
SELECT '645c2360-d0a3-4a11-8d24-523ca0c2fe69','1380410','UNIVERSAL DEVELOPMENT & CONSTRUCTION, LLC','2304 SILVERDALE DR STE 300','JOHNSON CITY','TN','37601-2590','WASHINGTON COUNTY','UNITED STATES' UNION
SELECT '49b95959-0d11-4a93-aa04-8387c3a965e4','5684','UNIVERSAL MANAGEMENT SERVICES, INC.','8201 183RD ST','TINLEY PARK','IL','60487-9248','','UNITED STATES' UNION
SELECT 'bcfda522-9e3a-4596-abeb-0f7999d51d60','20899','UNIVERSITY OF CALIFORNIA, SAN FRANCISCO DBA UCSF','1855 FOLSOM ST # 0815','SAN FRANCISCO','CA','94143','','UNITED STATES' UNION
SELECT '53a7164c-bb78-409d-b455-2e6a2c1d8c0c','9312','UNIVERSITY OF PITTSBURGH OF THE COMMONWEALTH SYSTEM OF HIGHER EDUCATION','4200 5TH AVE','PITTSBURGH','PA','15260-0001','ALLEGHENY COUNTY','UNITED STATES' UNION
SELECT '796fc055-3ad2-4c7e-b233-76152399663c','1355','','','','','','','' UNION
SELECT 'cbf53bfa-4545-4ac3-8ec0-7b7173af844f','2287','UPGRADE SERVICE COOP','1720 N GREAT OAK RD','PEORIA','IL','61604-3860','PEORIA COUNTY','United States' UNION
SELECT 'ff66c81b-3697-470e-999f-f11b5317f794','1373513','UPLIFT MANOR LLC','801 NE SILKWOOD ST','GRIMES','IA','50111-2079','POLK COUNTY','UNITED STATES' UNION
SELECT '8e21bf49-ffe4-4257-aeb2-d5710fae832c','1367735','UPSTREAM PROPERTIES LLC','70 WEST ST','DANBURY','CT','06810-6555','FAIRFIELD COUNTY','UNITED STATES' UNION
SELECT '8482a0b0-0f57-4ac4-ac0a-b6ae7f062701','5485','U.S. RESIDENTIAL GROUP','6404 INTERNATIONAL PKWY STE 1010','PLANO','TX','75093','','UNITED STATES' UNION
SELECT '94086e8a-f6b6-4b63-bdab-62ae936cda44','1538','USAA REAL ESTATE, A DELAWARE CORPORATION','9830 COLONNADE BLVD STE 600','SAN ANTONIO','TX','78230-2209','BEXAR COUNTY','UNITED STATES' UNION
SELECT '53115468-dc36-4a5d-a0a4-4d7b211467ad','1379663','USAI MANAGEMENT INC.','16610 DALLAS PKWY STE 1600','DALLAS','TX','75248-2685','DALLAS COUNTY','UNITED STATES' UNION
SELECT '7bc8fe4f-df82-410b-876d-cbdb16652072','24981','','','','','','','' UNION
SELECT '76199228-055c-41df-84c8-14d22dbaa2c1','9247','VALEO GROUPE US LLC','1001 MOREHEAD SQUARE DR STE 320','CHARLOTTE','NC','28203-4253','MECKLENBURG COUNTY','UNITED STATES' UNION
SELECT 'bc574080-1834-4aa9-92b6-2d737fae8494','7206','VALIANT ENTERPRISES LLC','8750 N CENTRAL EXPY STE 1010','DALLAS','TX','75231-6409','DALLAS COUNTY','UNITED STATES' UNION
SELECT '6fffe5d5-e563-4a29-b01d-e089d44e579e','5304','VAN METRE MANAGEMENT COMPANY','9900 MAIN ST STE 500','FAIRFAX','VA','22031-3907','FAIRFAX CITY','UNITED STATES' UNION
SELECT 'c6391332-86d1-4fff-985d-1fe7f7571dd9','5506','VAN ROOY PROPERTIES INC','1030 N COLLEGE AVE','INDIANAPOLIS','IN','46202-2726','MARION COUNTY','UNITED STATES' UNION
SELECT '55872f49-a5d3-42cd-82b5-9e3590a5ec7d','1380012','','','','','','','' UNION
SELECT 'afb9fb80-c316-453d-aa92-786eb0226ca0','1379242','VANDENBERGHE PROPERTIES, INC.','525 SYCAMORE VALLEY RD W','DANVILLE','CA','94526-3900','CONTRA COSTA COUNTY','UNITED STATES' UNION
SELECT '3b6bb209-6646-4fca-915e-cfab4c21515a','6625','VANTAGE MANAGEMENT, LLC','1544 MAIN ST','FYFFE','AL','35971-3484','','UNITED STATES' UNION
SELECT 'e6dd9538-55b4-4a8f-b284-68f26bed3872','23147','VASIL MGMT COMPANY, INC D/B/A VILLAGE MGMT COMPANY','860 E 86TH ST STE 5','INDIANAPOLIS','IN','46240-6860','MARION COUNTY','UNITED STATES' UNION
SELECT '8ee31039-a7ae-4168-9ff4-5caf9c80eaaa','6792','VASONA MANAGEMENT INC','1500 E HAMILTON AVE STE 210','CAMPBELL','CA','95008-0835','SANTA CLARA COUNTY','UNITED STATES' UNION
SELECT '51797e6e-91fe-447a-bd02-1c82aff4d3c8','9392','','','','','','','' UNION
SELECT '2c0d1e10-1357-401e-99d0-e013ab170840','22992','VENICE COMMUNITY HOUSING CORPORATION','720 ROSE AVE','VENICE','CA','90291-2710','LOS ANGELES COUNTY','UNITED STATES' UNION
SELECT 'e557415d-1934-46ca-9653-eeab1cc26f1d','8206','','','','','','','' UNION
SELECT '55858436-c4b4-4dfa-8a3a-172ad6dcbde3','7934','VENTERRA REALTY MANAGEMENT COMPANY INC','38 LEEK CRES SUITE 202','RICHMOND HILL','ON','L4B 4C6','','CANADA' UNION
SELECT '541c3108-365b-4ea3-89e8-10f5d0d5eaae','559','VENTRON MANAGEMENT LLC','2500 N MILITARY TRL STE 285','BOCA RATON','FL','33431-6322','PALM BEACH COUNTY','UNITED STATES' UNION
SELECT 'bf0faaf0-65cc-4a2d-8842-41e700859a29','1380013','','','','','','','' UNION
SELECT '07d8609a-1d63-446a-87d2-2df21bfc0fec','1368912','VESPAIO, LLC','1510 S BASCOM AVE','CAMPBELL','CA','95008-0626','SANTA CLARA COUNTY','UNITED STATES' UNION
SELECT '5257c19a-e856-44df-b1c6-2829d3fbd211','6942','VESTECO REAL ESTATE MANAGEMENT SERVICES, LLC','2000 VILLAGE LN','WINTER PARK','FL','32792-3451','ORANGE COUNTY','UNITED STATES' UNION
SELECT '89b16cb4-a5d7-4cab-bd74-cd1d2e86b4cd','1379528','VICINIA REAL ESTATE LLC','1297 S BARNES DR','BLOOMINGTON','IN','47401-8632','MONROE COUNTY','UNITED STATES' UNION
SELECT '98f58640-97d7-4b4d-96ee-26e18d04fff2','17262','VICKERY DEVELOPMENT, INC.','1417 E HWY 30 STE 1','GARLAND','TX','75043-1142','DALLAS COUNTY','UNITED STATES' UNION
SELECT 'ca50b664-079a-44f4-915c-dcaaae12a171','808','VIDALTA PROPERTY MANAGEMENT, LLC','2600 S DOUGLAS RD','CORAL GABLES','FL','33134-6127','MIAMI-DADE COUNTY','UNITED STATES' UNION
SELECT '234247b0-ae21-44c6-89d0-2f775cc08b69','3482','VIKING RESIDENTIAL','4 EXECUTIVE CAMPUS STE 200','CHERRY HILL','NJ','08002-4105','CAMDEN COUNTY','UNITED STATES' UNION
SELECT '14eaab46-46c5-4462-8214-00daf7b14069','9355','VILLA DIJON CONDOMINIUM ASSOCIATION','100 LORENZ RD','SAN ANTONIO','TX','78209-2567','BEXAR COUNTY','UNITED STATES' UNION
SELECT '8aba0aa9-f2d9-4bfa-a715-890f04c6273a','20773','','','','','','','' UNION
SELECT '86768728-e3ee-4c8e-ac5d-963ad3359b1d','1935','VILLAGE GATE PARTNERS LLC','3311 TURTLE POINT DRIVE','FAYETTEVILLE','NC','28304-3819','CUMBERLAND COUNTY','UNITED STATES' UNION
SELECT '05a5670d-b2f5-41a0-b2df-919b0e8bca84','8217','VILLAGE GREEN MANAGEMENT COMPANY','28411 NORTHWESTERN HWY STE 400','SOUTHFIELD','MI','48034-5544','OAKLAND COUNTY','UNITED STATES' UNION
SELECT '6ed98b76-f71f-4fb6-acba-6851b95ebe4a','1378171','VILLAS MANAGEMENT COMPANY, LLC','2111 RIO GRANDE ST','AUSTIN','TX','78705-5504','TRAVIS COUNTY','UNITED STATES' UNION
SELECT '6b742bc8-224d-49cd-80ae-5ddfddb5b8fa','2732','VIRGINIA BEACH COMM DEV CORP','2400 POTTERS RD','VIRGINIA BEACH','VA','23454-4377','','UNITED STATES' UNION
SELECT '4f72ece1-44b2-4881-b02a-8dc0e113a5d9','4368','VIRGINIA SUPPORTIVE HOUSING','8002 DISCOVERY DR STE 201','RICHMOND','VA','23229-8601','HENRICO COUNTY','UNITED STATES' UNION
SELECT 'c1a0bf50-c65b-480e-a836-908a74cbafc9','6612','VIRTU INVESTMENTS','5973 AVENIDA ENCINAS','CARLSBAD','CA','92008-4476','SAN DIEGO COUNTY','UNITED STATES' UNION
SELECT 'a8e3ac9e-4e73-4f65-b4db-d7e61c1fcbd2','1374614','VIRTUS PROPERTY GROUP LLC DBA SIGNIFICANLC LLC','11090 SERENBE LN STE 200B','CHATT HILLS','GA','30268-2464','FULTON COUNTY','UNITED STATES' UNION
SELECT '7d9a71f1-1ae6-48df-87ee-bf91b6525212','1380014','','','','','','','' UNION
SELECT 'bb4b9d3f-fc09-4a81-9361-4aa3ad74feef','1151','VITA PROPERTY MANAGEMENT GROUP','3333 ASPEN GROVE DR STE 130','FRANKLIN','TN','37067-4874','WILLIAMSON COUNTY','UNITED STATES' UNION
SELECT '7b632952-ba64-468e-820f-ae34ce16f6f2','6950','','','','','','','' UNION
SELECT '84407e0c-cd9a-433b-a499-06a3e2e9a722','17467','VIZION MANAGEMENT, INC.','710 OLD STAGE RD','AUBURN','AL','36830-4968','LEE COUNTY','UNITED STATES' UNION
SELECT 'd944ce0d-9da4-4466-ae36-33e7a7f9b252','5351','VOA NATIONAL SERVICES','1660 DUKE ST','ALEXANDRIA','VA','22314-3473','ALEXANDRIA CITY','UNITED STATES' UNION
SELECT '1612c367-9821-4a21-8633-c32a96131e50','2860','VOA OF AMERICA NORTHERN  NEW ENGLAND','14 MAINE ST','BRUNSWICK','ME','04011-2049','CUMBERLAND COUNTY','UNITED STATES' UNION
SELECT 'a39b2187-42b0-48cf-b2e8-1e425fd44e7c','6088','VORNADO/CHARLES E. SMITH','2345 CRYSTAL DR STE 1100','ARLINGTON','VA','22202-4801','ARLINGTON COUNTY','UNITED STATES' UNION
SELECT 'f70664ad-a63c-4a97-9e6c-0d2bb1a04f0b','6227','VTT MANAGEMENT INC.','100 CONCORD ST 3RD FL','FRAMINGHAM','MA','01702-8328','MIDDLESEX COUNTY','UNITED STATES' UNION
SELECT 'ff9ab282-9cd0-4a49-826b-eab1ef46adb6','2694','SMITH MANAGEMENT INC','707 N WALNUT AVE STE 104','NEW BRAUNFELS','TX','78130-7951','COMAL COUNTY','UNITED STATES' UNION
SELECT '71d1a203-84b2-4b1f-89ed-2dbf759088ad','700','W H LONG RENTALS','301 N 5TH ST STE 570','LAFAYETTE','IN','47901-1118','TIPPECANOE COUNTY','UNITED STATES' UNION
SELECT 'feb5974b-d0fb-4ff0-9d89-8bae7a14d35b','2750','WA SIMKINS REALTY INC','4705 CHEVERLY CT','VIRGINIA BEACH','VA','23464-5821','VIRGINIA BEACH CITY','UNITED STATES' UNION
SELECT '93b9ecc5-721f-4384-a806-8f43c368ec84','1374928','WAAHE CAPITAL, LLC','1625 E PRINCE RD','TUCSON','AZ','85719-1950','PIMA COUNTY','UNITED STATES' UNION
SELECT 'e4cb6fe6-8b3c-46e4-ae87-41d67a9fc780','6943','WALDRON ENTERPRISES, LLC','125 RIVER ROCK BLVD STE A','MURFREESBORO','TN','37128-4875','RUTHERFORD COUNTY','UNITED STATES' UNION
SELECT 'ca96fbe7-078e-4695-b65b-9335a050f260','18609','WARMINSTER HEIGHTS HOME OWNERSHIP ASSOCIATION, INC.','75 DOWNEY DR','WARMINSTER','PA','18974-4966','BUCKS COUNTY','UNITED STATES' UNION
SELECT 'bc9edb71-7228-40ec-8642-a7bf1c028b9e','5991','WARREN PROPERTIES, INC','6134 LA GRANADA','RANCHO SANTA FE','CA','92067','SAN DIEGO COUNTY','UNITED STATES' UNION
SELECT '9bff3f38-7610-4fd5-a6d3-fc7845a8a938','20669','','','','','','','' UNION
SELECT '5ee1c603-4544-4cab-956e-15d13aeae912','1287','WASATCH PROPERTY MANAGEMENT, INC','595 S 80 E STE 400','LOGAN','UT','84321-6845','CACHE COUNTY','UNITED STATES' UNION
SELECT '618c5d74-13f9-4fd5-93eb-54fe749a41e3','5876','WASHINGTON REAL ESTATE INVESTMENT TRUST','1775 I ST NW STE 1000','WASHINGTON','DC','20006-2404','DISTRICT OF COLUMBIA','UNITED STATES' UNION
SELECT '3bbdc2ed-01ad-4dbb-ba74-2e33b94b9d57','20775','WATERFORD PROPERTIES','16630 N DALE MABRY HWY','TAMPA','FL','33618-1400','HILLSBOROUGH COUNTY','UNITED STATES' UNION
SELECT 'c85e0eae-039c-475d-b7ae-d537827c122b','6476','WATERTON RESIDENTIAL, LLC','30 S WACKER DR STE 3600','CHICAGO','IL','60606-7462','COOK COUNTY','UNITED STATES' UNION
SELECT 'd20ecd1e-a1bd-42dd-a699-d5147b6cb292','7435','WATKINS GLEN HOUSING AUTHORITY','222 E 2ND ST','WATKINS GLEN','NY','14891-1254','SCHUYLER COUNTY','UNITED STATES' UNION
SELECT '07b11122-cee5-4401-b3cf-813c2a93b742','8321','WAYFINDER MANAGEMENT, INC.','3535 COLONEL VANDERHORST CIR','MT PLEASANT','SC','29466-8032','CHARLESTON COUNTY','UNITED STATES' UNION
SELECT '34c5028c-e9aa-4ed2-ac15-99f57fb685a4','6617','WAYPOINT MANAGEMENT SERVICES, LLC','3475 PIEDMONT RD NE STE 1640','ATLANTA','GA','30305-2992','FULTON COUNTY','UNITED STATES' UNION
SELECT 'bb91f5b5-2ac4-45dc-af91-52201a924c1f','8569','WAYPOINT STUDENT LIVING, LLC.','3475 PIEDMONT RD NE STE 1640','ATLANTA','GA','30305-2992','FULTON COUNTY','UNITED STATES' UNION
SELECT '693dc6f8-3519-4fb3-9e88-87a621176c33','1377137','WEBB & BROOKER, INC.','2534 ADAM CLAYTON POWELL JR BLVD','NEW YORK','NY','10039-3805','NEW YORK COUNTY','UNITED STATES' UNION
SELECT 'd81953df-226e-482a-83b4-25d989d224b6','5839','WEDGE MANAGEMENT, INC. DBA PROSPERA PROPERTY MANAGEMENT','3419 NACOGDOCHES RD','SAN ANTONIO','TX','78217','BEXAR COUNTY','UNITED STATES' UNION
SELECT '341e71c1-e91f-496d-9a04-ff27de2e7730','848','WEIDNER PROPERTY MANAGEMENT LLC','9757 NE JUANITA DR STE 300','KIRKLAND','WA','98034-4291','KING COUNTY','UNITED STATES' UNION
SELECT 'dbbb21f8-ebeb-4311-a571-737f416edc1c','5361','WEIGAND-OMEGA MANAGEMENT, INC.','333 S BROADWAY ST STE 105','WICHITA','KS','67202-4325','SEDGWICK COUNTY','UNITED STATES' UNION
SELECT '4b689ce8-bce0-4a48-8790-6b1ebe1743b1','5941','WELLINGTON ADVISORS, LLC','2121 EASTCHESTER DR','HIGH POINT','NC','27265-1534','GUILFORD COUNTY','UNITED STATES' UNION
SELECT '1eaec0d6-7c7e-42ea-b40c-d6e1f3d42891','6719','WESCAP REAL ESTATE SERVICES, INC','4745 N 7TH ST STE 110','PHOENIX','AZ','85014-3666','MARICOPA COUNTY','UNITED STATES' UNION
SELECT '6b18ae1f-2b4c-45bf-81cf-4307906f0031','1379725','','','','','','','' UNION
SELECT '504923fa-6dd4-49b5-8c51-9b7af18c8906','3406','WEST ARDEN MANAGEMENT','502 S KOENIGHEIM ST STE 3B','SAN ANGELO','TX','76903-6769','TOM GREEN COUNTY','UNITED STATES' UNION
SELECT '64f82ea5-b8d2-4d5f-abc0-da2a636f6ffb','24397','','','','','','','' UNION
SELECT '86ef75f4-0a3e-4260-be57-95623384165b','24769','WEST SIDE MANAGEMENT OF SARATOGA, LLC','18 DIVISION ST STE 401','SARATOGA SPRINGS','NY','12866-3154','SARATOGA COUNTY','UNITED STATES' UNION
SELECT '2373d197-a029-43f7-9040-8a6b8a902e2e','8866','WESTBANK MANAGEMENT','4515 MANCHACA RD STE 100','AUSTIN','TX','78745-1645','TRAVIS COUNTY','UNITED STATES' UNION
SELECT '711a5e5a-7ffe-4d50-8514-32a0ea99adb7','6410','WESTCAL MANAGEMENT','6056 RUTLAND DR STE 1','CARMICHAEL','CA','95608-0514','SACRAMENTO COUNTY','UNITED STATES' UNION
SELECT '217a042e-23ef-4ed1-b3e2-0335d13f1654','5934','WESTCORP MANAGEMENT GROUP ONE INC.','6655 S EASTERN AVE STE 200','LAS VEGAS','NV','89119-3915','CLARK COUNTY','UNITED STATES' UNION
SELECT 'f20a2d03-a66b-4616-98ca-86c111e92ef3','6000','WESTDALE ASSET MGMT','3100 MONTICELLO AVE STE 600','DALLAS','TX','75205-3439','DALLAS COUNTY','UNITED STATES' UNION
SELECT '3f6b65b9-8d4e-45bb-ac7c-91f3e8916346','6502','WESTERN SECURITIES (USA) LIMITED','613 NW LOOP 410','SAN ANTONIO','TX','78216-5507','BEXAR COUNTY','UNITED STATES' UNION
SELECT 'a6d9c0cd-13a6-4b19-87b6-99c5fc5e5856','6418','WESTGATE MANAGEMENT CO, INC','133 FRANKLIN CORNER RD','LAWRENCEVILLE','NJ','08648-2531','MERCER COUNTY','UNITED STATES' UNION
SELECT 'a9825f1d-fb5b-4103-abae-c43dcd7c41ff','1273','WESTLAKE HOUSING LP','515 S CAPITAL OF TEXAS HWY STE 100','WEST LAKE HILLS','TX','78746-4305','TRAVIS COUNTY','UNITED STATES' UNION
SELECT 'd0024414-e201-4d2b-a1a3-2d6f08702591','1375112','','','','','','','' UNION
SELECT '7e92c47e-e065-40f4-81ce-5531d99c8569','6255','WESTMINSTER COMPANY','3859 BATTLEGROUND AVE STE 100','GREENSBORO','NC','27410-9580','GUILFORD COUNTY','UNITED STATES' UNION
SELECT 'cc647d64-e11d-4df4-84d1-33b9f136dc6e','1309','WESTON ASSOCIATES MANAGEMENT COMPANY, INC.','170 NEWBURY ST','BOSTON','MA','02116-2873','SUFFOLK COUNTY','UNITED STATES' UNION
SELECT 'aa3a73e6-fdeb-4dee-9704-950a5d4bfd70','6011','WESTWOOD RESIDENTIAL COMPANY','3198 PARKWOOD BLVD APT 11076','FRISCO','TX','75034-9518','COLLIN COUNTY','UNITED STATES' UNION
SELECT 'fa92cd85-0af9-4055-96d3-9710eae31b52','162','WHITE OAK PARTNERS LLC','5150 E DUBLIN GRANVILLE RD','WESTERVILLE','OH','43081-8701','FRANKLIN COUNTY','UNITED STATES' UNION
SELECT 'a468eee1-14cc-413b-8b12-3361141e21cc','2269','WHITECO RESIDENTIAL MANAGEMENT, LLC','9800 CONNECTICUT DR','CROWN POINT','IN','46307-7840','LAKE COUNTY','UNITED STATES' UNION
SELECT 'a29d732e-f37a-4f15-9895-17afbf44da88','8551','','','','','','','' UNION
SELECT '25c5eee2-a86a-44c0-8ecf-087318314115','1380024','','','','','','','' UNION
SELECT '2be17ea7-b851-439d-a7dd-ae31ae5a0f4f','5201','WHITNEY MANAGEMENT CORP','9575 KATY FWY STE 330','HOUSTON','TX','77024-1420','HARRIS COUNTY','UNITED STATES' UNION
SELECT 'e3cca192-b694-4f1d-acc4-3668dcad2c1b','6368','WILDWOOD PROPERTY MANAGEMENT, LLC','162 WEST ST STE D','CROMWELL','CT','06416-4405','MIDDLESEX COUNTY','UNITED STATES' UNION
SELECT 'be3096b0-f953-49b8-bf95-d4dffc0fb97e','978','WILHOIT PROPERTIES INC','1329 E LARK ST','SPRINGFIELD','MO','65804','GREENE COUNTY','UNITED STATES' UNION
SELECT '055aba14-4213-424f-99c5-de46002fe0ec','6814','WILKINSON CORPORATION','212 N NACHES AVE','YAKIMA','WA','98901-2438','YAKIMA COUNTY','UNITED STATES' UNION
SELECT 'd6178c4d-c39e-4a5b-999d-5383f4b22f29','6393','','','','','','','' UNION
SELECT '52d6a0c7-ef65-4cdc-bbc6-2d3b1b8b201c','5496','WILLIAM C SMITH AND CO INC','1100 NEW JERSEY AVE SE STE 1000','WASHINGTON','DC','20003-3302','DISTRICT OF COLUMBIA','UNITED STATES' UNION
SELECT 'b6092376-10eb-4719-99fe-ea583c94bfff','1380015','','','','','','','' UNION
SELECT '57d96c2e-ee39-4bd2-916e-826f10dd3a93','24563','','','','','','','' UNION
SELECT '1b2568a0-e0c9-4c38-bbad-4ae64fdc6cde','816','WILLNER REALTY AND DEVELOPMENT CO','123 COULTER AVE STE 200','ARDMORE','PA','19003-2425','MONTGOMERY COUNTY','UNITED STATES' UNION
SELECT '94c68865-1eb7-449a-828f-2b68780d5a6a','22519','WILLOW CREEK PARTNERS, LLC','3200 GLEN ROYAL RD STE 111','RALEIGH','NC','27617-7419','WAKE COUNTY','UNITED STATES' UNION
SELECT '727099a4-6304-4648-a696-5c67a9b54ef0','5907','','','','','','','' UNION
SELECT '74350d12-079c-4d18-8436-fe97297b3085','1367182','WILMORITE MANAGEMENT GROUP, LLC','1265 SCOTTSVILLE RD','ROCHESTER','NY','14624-5104','MONROE COUNTY','UNITED STATES' UNION
SELECT 'b1deaaa6-c3cd-4219-84d8-805df3b33404','5836','WIMSATT MANAGEMENT COMPANY, INC.','3101 BRECKENRIDGE LN # 4A','LOUISVILLE','KY','40220-2742','JEFFERSON COUNTY','UNITED STATES' UNION
SELECT '5db11e94-b24d-4ed6-b174-d70a7c1167e2','2958','WIND RIVER MANAGEMENT CORPORATION','1600 S MAIN ST','DUNCANVILLE','TX','75137-4004','','UNITED STATES' UNION
SELECT 'e57ae5c4-870b-4e4b-ba23-8bd978253471','6534','WINDMARK GROUP','5001 S HULEN ST STE 106','FORT WORTH','TX','76132-1913','TARRANT COUNTY','UNITED STATES' UNION
SELECT '8da014df-cd53-4fa1-95bc-37c91808b24f','59','WINDRIVER MANAGEMENT','4733 COLLEGE PARK','SAN ANTONIO','TX','78249-4053','BEXAR COUNTY','UNITED STATES' UNION
SELECT '87963d6a-e4d9-43f6-b11d-fcc86616128a','7991','WINDSOR MANAGEMENT LLC','601 W DELAWARE AVE','IMMOKALEE','FL','34142-4066','COLLIER COUNTY','UNITED STATES' UNION
SELECT '37f0ff83-232a-46ce-9b22-69a494a07b22','23394','WINDWARD COMMUNITIES, LLC','238 REHOBOTH AVE','REHOBOTH BEACH','DE','19971-2134','SUSSEX COUNTY','UNITED STATES' UNION
SELECT 'b1897bb1-ec01-447c-94e2-faaa30bd725d','5713','WINGATE MGMT CO','100 WELLS AVE','NEWTON','MA','02459-3210','MIDDLESEX COUNTY','UNITED STATES' UNION
SELECT '5dd3f485-a4b9-45fd-8d92-61665d6531ce','1314','WINN FAMILY OFFICE, LLC','2201 LAKESIDE BLVD STE 200  ATTN SABRINA KING','RICHARDSON','TX','75082-4305','DALLAS COUNTY','UNITED STATES' UNION
SELECT '62297c43-7548-4512-9c8d-b00a29e2f8b7','8260','WINNRESIDENTIAL LLC','1 WASHINGTON MALL STE 500','BOSTON','MA','02108-2603','SUFFOLK COUNTY','UNITED STATES' UNION
SELECT 'eb1f2d05-4f7c-41ab-a31b-6c3aa430896b','1972','WINTERWOOD, INC.','1390 OLIVIA LN STE 100','LEXINGTON','KY','40511-1391','FAYETTE COUNTY','UNITED STATES' UNION
SELECT 'e41df9e9-1e8b-4b8f-bda8-9d8de4807b6d','5762','WINTHER INVESTMENT, INC.','1919 POST OAK PARK DR  STE 3101','HOUSTON','TX','77027-3343','','UNITED STATES' UNION
SELECT '67dcbb8b-d734-4bff-b924-fde71159623b','5677','WINTON CORPORATION','131 ASHLEY AVE STE A1','WEST SPRINGFIELD','MA','01089-1332','HAMPDEN COUNTY','UNITED STATES' UNION
SELECT 'e08c2395-0daf-43d4-876e-94afb0ae4149','1378007','WISCONSIN HOUSING PRESERVATION CORP.','150 E GILMAN ST STE 1500','MADISON','WI','53703-1499','DANE COUNTY','UNITED STATES' UNION
SELECT '1d7380b2-e470-4d4b-a911-18dab6785f7c','1380016','','','','','','','' UNION
SELECT '77a40a3b-2334-49c4-b1b1-3e77c8f22b1a','2666','WOODBURY MANAGEMENT','260 E BROWN ST STE 200','BIRMINGHAM','MI','48009-6231','OAKLAND COUNTY','UNITED STATES' UNION
SELECT '8f16138e-542e-4858-b9b4-2caf75f1b16e','6908','WOODFIELD INVESTMENTS, LLC','19583 SARATOGA SPRINGS PL','ASHBURN','VA','20147-5217','LOUDOUN COUNTY','UNITED STATES' UNION
SELECT 'b4f138d9-dd4e-4eef-b337-e9058625e2df','6724','WOODLARK ASSET MANAGEMENT, LLC','80 SW 8TH ST STE 2500','MIAMI','FL','33130-3032','MIAMI-DADE COUNTY','UNITED STATES' UNION
SELECT 'd1599ac0-b34d-4f5f-b28d-4fd6f3354df9','8044','WOODMONT REAL ESTATE SERVICES','1050 RALSTON AVE','BELMONT','CA','94002-2210','SAN MATEO COUNTY','UNITED STATES' UNION
SELECT '442aafdb-c1ec-4726-8938-226452f738cd','1312','WP SOUTH ACQUISITIONS, LLC','3715 NORTHSIDE PKWY NW STE 4-600','ATLANTA','GA','30327-2808','FULTON COUNTY','UNITED STATES' UNION
SELECT '793b3f5f-37a7-4580-abdc-1f5327f26f7a','5670','WRH REALTY SERVICES, INC','100 3RD ST S STE 300','ST PETERSBURG','FL','33701-4250','PINELLAS COUNTY','UNITED STATES' UNION
SELECT '8c2503a1-65b6-40bc-aba0-ad6079c09919','1150','WRPS III, LP','2505 N STATE HIGHWAY 360 STE 800','GRAND PRAIRIE','TX','75050-7803','TARRANT COUNTY','UNITED STATES' UNION
SELECT 'ff45680f-2c73-4dfc-b10e-693a3e12f276','15939','WWC OPERATIONS CORP','1270 E BROADWAY RD STE 109','TEMPE','AZ','85282-1516','MARICOPA COUNTY','UNITED STATES' UNION
SELECT '6d7900a8-b318-4625-9fc5-ae556dab647e','6713','YAMAOKA ASSOCIATES, INC.','1307 S MARY AVE STE 110','SUNNYVALE','CA','94087-3071','SANTA CLARA COUNTY','UNITED STATES' UNION
SELECT '3256d8da-86fc-4962-bda2-2110a417b6c8','30677','YAMPA VALLEY HOUSING AUTHORITY','2100 ELK RIVER RD','STEAMBOAT SPRINGS','CO','80487-5262','ROUTT COUNTY','UNITED STATES' UNION
SELECT '7a4be14b-84f0-4142-92ea-2e99e0abc3b9','5822','YARCO COMPANY, INC','7920 WARD PKWY','KANSAS CITY','MO','64114-2017','','UNITED STATES' UNION
SELECT '7d52ae7a-ba31-4094-b7fb-0d0fed831ab6','19373','YARR PROPERTIES, LLC','1075 16TH AVE','VERO BEACH','FL','32960-3742','INDIAN RIVER COUNTY','UNITED STATES' UNION
SELECT '6ffb9b50-26b5-445a-a9dc-d0532729689a','1373803','YMCA OF THE NORTH SHORE','245 CABOT ST','BEVERLY','MA','01915-4511','ESSEX COUNTY','UNITED STATES' UNION
SELECT 'e73b6e43-947c-4e8a-a6bb-8e9c3408ca93','1374509','YMP CREST OPCO LLC','5100 CRESTHAVEN BLVD','WEST PALM BEACH','FL','33415-8618','PALM BEACH COUNTY','UNITED STATES' UNION
SELECT 'aaa731ca-6cd3-4aed-a84c-2b45ba8817bc','6600','YOPP PROPERTIES LLC','1095 FIELDWOOD LN','WINSTON SALEM','NC','27106-5863','FORSYTH COUNTY','UNITED STATES' UNION
SELECT '6cedcc40-a175-48f1-a59f-fb6a1a50f436','5829','YORK PROPERTIES, INC.','28411 NORTHWESTERN HWY STE 900','SOUTHFIELD','MI','48034-5519','OAKLAND COUNTY','UNITED STATES' UNION
SELECT '359100cc-daf5-4dca-ab45-a47653bbd642','6556','YOUNG MANAGEMENT','22602 STATE LINE RD','BUCYRUS','KS','66013-9733','MIAMI COUNTY','UNITED STATES' UNION
SELECT 'b354c0ee-cbc7-40b6-b7e2-34c9a61b3c23','1379707','YW HOMES, INC.','309 23RD ST N','BIRMINGHAM','AL','35203-3820','JEFFERSON COUNTY','UNITED STATES' UNION
SELECT '27b2a7a8-b13a-4681-98e0-15577e36ff19','6720','YWCA APARTMENTS, INC.','940 POWELL ST','SAN FRANCISCO','CA','94108-2014','SAN FRANCISCO COUNTY','UNITED STATES' UNION
SELECT '0105b780-0878-4c7e-85b3-1a3d6c7e8027','1369601','YWCA MADISON, INC.','101 E MIFFLIN ST','MADISON','WI','53703-2824','DANE COUNTY','UNITED STATES' UNION
SELECT '458aa3fc-1d23-45f4-a2f7-72e0d279ed66','20659','YWCA OF HAMILTON APARTMENTS','244 DAYTON ST','HAMILTON','OH','45011-1634','BUTLER COUNTY','UNITED STATES' UNION
SELECT 'e31655cf-9074-4b54-82b1-3595935a7ac2','1379359','','','','','','','' UNION
SELECT '6269ccd7-9a2b-4cc2-ac20-b55dc9cdb9b4','4005','ZEHMAN-WOLF MANAGEMENT, LLC','25700 SCIENCE PARK DR STE 325','BEACHWOOD','OH','44122','','UNITED STATES' UNION
SELECT '1dcb87ee-307e-4241-b296-50853fc88db3','6345','','','','','','','' UNION
SELECT '64256881-d0be-44f6-aaea-202e317a5cf4','1169','ZIDELL PROPERTIES','5421 ALPHA RD STE 100','DALLAS','TX','75240-2518','DALLAS COUNTY','UNITED STATES' UNION
SELECT '35172afb-6b54-42e3-a6aa-e1598344084c','6223','ZIMMERMAN','1201 DUBLIN RD STE 400','COLUMBUS','OH','43215-3140','FRANKLIN COUNTY','UNITED STATES' UNION
SELECT '1ae23315-373e-42cb-bc90-8fcadc8377c6','5860','ZOCALO COMMUNITY DEVELOPMENT','455 N SHERMAN ST STE 205','DENVER','CO','80203-4404','DENVER COUNTY','UNITED STATES' UNION
SELECT 'a97ab122-8c96-4a94-8be5-58b6432250b9','6795','ZRS MANAGEMENT LLC','2001 SUMMIT PARK DR STE 300','ORLANDO','FL','32810-5998','','UNITED STATES' UNION
SELECT 'f5c6e8ad-42d1-4b40-b902-735b13b21d8b','6816','ZUCKERMAN GRAVELY MGMT','2 WISCONSIN CIR STE 1050','CHEVY CHASE','MD','20815-7003','MONTGOMERY COUNTY','UNITED STATES' 


--Insert Company Address
INSERT INTO  [Enterprise].[CompanyAddress]
(
	[PartyId], 
	[Address],
	[City],
	[State],
	[PostalCode],
	[County],
	[Country],
	[Latitude],
	[Longitude]
 )

SELECT 
	O.PartyId,
	BooksAddress.Address,
	BooksAddress.City,
	BooksAddress.State,
	BooksAddress.PostalCode,
	BooksAddress.county,
	BooksAddress.country,
	NULL,
	NULL
 FROM Enterprise.Organization O 
	inner join Enterprise.Party P on o.PartyId = p.PartyId
	INNER JOIN @tempCompany BooksAddress ON BooksAddress.UPFMID = p.RealPageId 
	WHERE O.PartyId NOT IN (SELECT PartyId FROM Enterprise.CompanyAddress)

--Insert Contracted Name
INSERT INTO [Enterprise].[CompanyContractedName]
(
	[PartyId],
	[ContractedName]
)

SELECT 
	O.PartyId,
	BooksAddress.ContractedName
 FROM Enterprise.Organization O 
	inner join Enterprise.Party P on o.PartyId = p.PartyId
	INNER JOIN @tempCompany BooksAddress ON BooksAddress.UPFMID = p.RealPageId
	WHERE O.PartyId NOT IN (SELECT PartyId FROM Enterprise.CompanyContractedName)

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
       SET @RightDescription = 'Access to Help Center';
       SET @RightValue = 'Access to Help Center';
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