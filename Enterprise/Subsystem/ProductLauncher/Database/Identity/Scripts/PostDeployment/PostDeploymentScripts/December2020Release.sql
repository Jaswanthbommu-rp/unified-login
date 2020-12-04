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
WHERE LoginName LIKE 'realpagead@%'

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
    WHERE ProductTypeId = 114
)
    BEGIN
        EXEC [Enterprise].[CreateProductType] 
             @ProductTypeId = 114, -- Thsi value may change based on the root prodcut type
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
             @ProductTypeId = 114;
        
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
