CREATE Procedure [Enterprise].CreateNewProduct  
 @ProductId INT,  
 @LoginURI NVARCHAR(500),
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
 
 @ProductTypeId INT,  
 @TitleUniqueIdSettingValue NVARCHAR(2000),  
 @ParentProductTypeId INT;  
  
SELECT @prdTypeGUID = NEWID()  
SELECT @prdGUID = NEWID()  
SELECT @TitleUniqueIdSettingValue = NEWID();  
SELECT @ParentProductTypeId = ProductTypeId FROM Enterprise.ProductType WHERE Name = @PropertyManagementName AND ParentProductTypeId IS NULL;  
  
SET @ProductTypeId = (SELECT MAX(productTypeid) + 1 'ProductTypeId'  FROM [Enterprise].[ProductType]  WHERE ParentProductTypeId = @ParentProductTypeId)  

  
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
,('IsFavorite','','1', 0)  
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
  
--Setup the product configurations.  
IF NOT EXISTS (SELECT TOP 1 1 FROM Enterprise.ProductSetting WHERE ProductId = @ProductId)  
BEGIN  
       EXEC Enterprise.ProductConfigurationSetup @ProductId, @LoginURI, @SigningCertificateThumbprint, @ProductConfiguration;  
END;  
  
IF NOT EXISTS(SELECT 1 FROM ident.SamlProductSettings WHERE ProductId = @ProductId AND LoginUri = @LoginURL)  
    BEGIN  
        INSERT INTO ident.SamlProductSettings --SamlProductSettingsId - column value is auto-generated  
         (ProductId, LoginUri, SigningCertificateThumbprint, SubjectIdSamlAttribute )  
        VALUES  
   (@ProductId, @LoginURL, @SigningCertificateThumbprint, @SubjectIdSamlAttribute );  
END;  