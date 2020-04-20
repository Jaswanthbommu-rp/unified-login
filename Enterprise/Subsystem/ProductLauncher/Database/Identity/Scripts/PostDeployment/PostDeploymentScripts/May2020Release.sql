GO

--Unified Platform Product Access Data
DECLARE @UserId bigint,
	@ProductId int = 3,
	@Now datetime = GETDATE()

SELECT	@UserId = UserId
FROM	Ident.UserLogin
WHERE	LoginName LIKE 'realpagead@%'

IF NOT EXISTS (SELECT TOP 1 1 FROM[UserManagement].[ProductPage] WHERE ProductId = @ProductId)
BEGIN
	SET IDENTITY_INSERT [UserManagement].[Control] ON 

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
	VALUES (135, NULL, 8, N'UnifiedPlatformProductAccessTabGroupUIId', NULL, NULL, 1, @UserId, @Now)

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
	VALUES (136, 135, 9, N'UnifiedPlatformProductAccessRolesTabUIId', N'Roles', NULL, 1, @UserId, @Now)

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
	VALUES (137, 136, 2, N'UnifiedPlatformProductAccessRolesSelectGridUIId', NULL, NULL, 1, @UserId, @Now)

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
	VALUES (138, 137, 7, N'UnifiedPlatformProductAccessRadioUIId', NULL, N'isAssigned', 1, @UserId, @Now)

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
	VALUES (139, 137, 5, N'UnifiedPlatformProductAccessRoleLabelUIId', N'Role', N'name', 2, @UserId, @Now)

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
	VALUES (140, 137, 5, N'UnifiedPlatformProductAccessRoleTypeLabelUIId', N'Role Type', N'roletype', 3, @UserId, @Now)

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
	VALUES (141, 137, 11, N'UnifiedPlatformProductAccessIconUIId', NULL, N'InfoIcon', 4, @UserId, @Now)

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
	VALUES (142, 141, 5, N'UnifiedPlatformProductAccessRoleDetailsLabelUIId', N'Role Details', NULL, 1, @UserId, @Now)

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
	VALUES (143, 141, 12, N'UnifiedPlatformProductAccessGridUIId', NULL, NULL, 2, @UserId, @Now)

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
	VALUES (144, 143, 5, N'UnifiedPlatformProductAccessRightLabelUIId', N'Right', N'description', 2, @UserId, @Now)

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
	VALUES (145, 135, 9, N'UnifiedPlatformProductAccessPropertiesTabUIId', N'Properties', NULL, 2, @UserId, @Now)

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
	VALUES (146, 145, 1, N'UnifiedPlatformProductAccessAllowaccesstoallcurrentandfuturepropertiesPropertiesSwitchUIId', N'Allow access to all current and future properties', N'allProperties', 1, @UserId, @Now)

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
	VALUES (147, 145, 3, N'UnifiedPlatformProductAccessPropertiesMultiSelectGridUIId', NULL, NULL, 2, @UserId, @Now)

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
	VALUES (148, 147, 10, N'UnifiedPlatformProductAccessCheckboxUIId', NULL, N'isAssigned', 1, @UserId, @Now)

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
	VALUES (149, 147, 5, N'UnifiedPlatformProductAccessPropertyLabelUIId', N'Property', N'name', 2, @UserId, @Now)

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
	VALUES (150, 147, 5, N'UnifiedPlatformProductAccessCityLabelUIId', N'City', N'city', 3, @UserId, @Now)

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
	VALUES (151, 147, 5, N'UnifiedPlatformProductAccessStateLabelUIId', N'State', N'state', 4, @UserId, @Now)

	SET IDENTITY_INSERT [UserManagement].[Control] OFF

	SET IDENTITY_INSERT [UserManagement].[ControlAttribute] ON 

	INSERT [UserManagement].[ControlAttribute] ([ControlAttributeId], [ControlId], [Key], [Value], [CreatedBy], [CreatedDate])
	VALUES (18, 136, N'Default', N'True', @UserId, @Now)

	INSERT [UserManagement].[ControlAttribute] ([ControlAttributeId], [ControlId], [Key], [Value], [CreatedBy], [CreatedDate])
	VALUES (19, 136, N'Hide', N'False', @UserId, @Now)

	INSERT [UserManagement].[ControlAttribute] ([ControlAttributeId], [ControlId], [Key], [Value], [CreatedBy], [CreatedDate])
	VALUES (20, 141, N'InfoIcon', N'Slide', @UserId, @Now)

	INSERT [UserManagement].[ControlAttribute] ([ControlAttributeId], [ControlId], [Key], [Value], [CreatedBy], [CreatedDate])
	VALUES (21, 145, N'Hide', N'True', @UserId, @Now)

	INSERT [UserManagement].[ControlAttribute] ([ControlAttributeId], [ControlId], [Key], [Value], [CreatedBy], [CreatedDate])
	VALUES (22, 147, N'ShowSelectAll', N'True', @UserId, @Now)

	SET IDENTITY_INSERT [UserManagement].[ControlAttribute] OFF

	SET IDENTITY_INSERT [UserManagement].[ControlDependency] ON 
	
	INSERT [UserManagement].[ControlDependency] ([ControlDependencyId], [MasterControlId], [SlaveControlID], [MasterControlValue], [ComparatorId], [CreatedBy], [CreatedDate])
	VALUES (16, 139, 145, N'ManageCIMPLQuestions', 1, @UserId, @Now)

	INSERT [UserManagement].[ControlDependency] ([ControlDependencyId], [MasterControlId], [SlaveControlID], [MasterControlValue], [ComparatorId], [CreatedBy], [CreatedDate])
	VALUES (17, 139, 145, N'CIMPLManagePII', 1, @UserId, @Now)

	INSERT [UserManagement].[ControlDependency] ([ControlDependencyId], [MasterControlId], [SlaveControlID], [MasterControlValue], [ComparatorId], [CreatedBy], [CreatedDate])
	VALUES (18, 139, 145, N'CIMPLManageSensitiveFinancialData', 1, @UserId, @Now)

	INSERT [UserManagement].[ControlDependency] ([ControlDependencyId], [MasterControlId], [SlaveControlID], [MasterControlValue], [ComparatorId], [CreatedBy], [CreatedDate])
	VALUES (19, 139, 145, N'ViewCIMPLQuestions', 1, @UserId, @Now)

	SET IDENTITY_INSERT [UserManagement].[ControlDependency] OFF

	SET IDENTITY_INSERT [UserManagement].[ProductPage] ON 

	INSERT [UserManagement].[ProductPage] ([ProductPageId], [ProductId], [DisplayName], [CreatedBy], [CreatedDate], [IsActive])
	VALUES (9, 3, N'Unified Platform Product Access', @UserId, @Now, 1)

	SET IDENTITY_INSERT [UserManagement].[ProductPage] OFF

	SET IDENTITY_INSERT [UserManagement].[ProductPageControl] ON 

	INSERT [UserManagement].[ProductPageControl] ([ProductPageControlId], [ProductPageId], [ControlId], [CreatedBy], [CreatedDate])
	VALUES (18, 9, 135, @UserId, @Now)

	SET IDENTITY_INSERT [UserManagement].[ProductPageControl] OFF
END

GO

DECLARE @ProductId INT, 
		@ParentProductTypeId INT, 
		@ProductName NVARCHAR(100)= 'Vendor Marketplace',  -- Produact Name
		@ServerName SYSNAME = @@SERVERNAME;

DECLARE @ProductConfiguration AS PRODUCTCONFIGURATIONTYPE;

/*Validate what product type ths new product belongs to. 'Administration' in the following block 
need to be chnanged to desired prodcut type. You can query Enterprise.ProductType table for more details.
*/

INSERT INTO @ProductConfiguration(SettingName,  SettingDescription,  SettingValue) VALUES('AlternateLoginURL','Alternate URL that can be used for Product Login','https://dev.realpagevendormarketplace.com')

IF @ServerName IN ( 'RCTUSODBSQL001')
BEGIN
	Update @ProductConfiguration set SettingValue = 'https://qa.realpagevendormarketplace.com' where SettingName = 'AlternateLoginURL'
END
IF @ServerName IN ( 'RCQUSODBSQL001')
BEGIN
	Update @ProductConfiguration set SettingValue = 'https://sat.realpagevendormarketplace.com' where SettingName = 'AlternateLoginURL'
END
IF @ServerName IN ( 'RCPGBKDBSQL005A', 'RCPGBKDBSQL005B')
BEGIN
	Update @ProductConfiguration set SettingValue = 'https://www.realpagevendormarketplace.com' where SettingName = 'AlternateLoginURL'
END

--The following block picks up all the detail frm Enterprise.ProductSettingType table
--To set up the product, bunch of these settings are required.
set nocount on
--SELECT * FROM @ProductConfiguration

if not exists(Select top 1 1 from Enterprise.ProductSettingType where Name = 'AlternateLoginURL')
Begin
	Insert into Enterprise.ProductSettingType (Name, Description) Values ('AlternateLoginURL', 'Alternate URL that can be used for Product Login')
End

if not exists(Select top 1 * from Enterprise.ProductSetting ps 
				inner join Enterprise.ProductSettingType pst
				on ps.ProductSettingTypeId = pst.ProductSettingTypeId
				where pst.Name = 'AlternateLoginURL' and ps.ProductId= 38)
Begin
	Insert into Enterprise.ProductSetting (ProductId, ProductSettingTypeId, Value, FromDate)
	Select 38, ProductSettingTypeId, SettingValue, GETUTCDATE()
	from @ProductConfiguration cross join Enterprise.ProductSettingType
	where Name = 'AlternateLoginURL'

	declare @productsettingid int
	select @productsettingid = productsettingid from Enterprise.ProductSetting ps 
				inner join Enterprise.ProductSettingType pst
				on ps.ProductSettingTypeId = pst.ProductSettingTypeId
				where pst.Name = 'AlternateLoginURL' and ps.ProductId= 38

	insert into enterprise.ProductConfiguration ( ConfigurationId, ProductSettingId, FromDate )
		select top 1 ConfigurationId, @productsettingid, GETUTCDATE() from enterprise.GlobalProductConfiguration where productid = 38 and thrudate is null
End

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
SET @RightName = 'Employee Access to Vendor Marketplace'; -- Name of the right 
SET @RightDescription = 'Employee Access to Vendor Marketplace'; --Description of the right as stated in story.
SET @RightShortName = 'EmployeeAccessVendorMarketPlace'; --Short name of the right that is being used by the application
SET @ActionName = 'Employee Vendor MarketPlace'; -- This specifically pertains to actions used for routing purposes. 
SET @ActionRouteTarget = 'DashBoard'; -- Where you want this right to show up. other variation is DashBoard.
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
WHERE O.Name= 'RealPage Employee'; 
--1. If rigths ne ed in all organization then no condition 
--2. If needed in all except RP Employee company then O.Name <> 'RealPage Employee'
--3. If needed in just RP Employee and not in any other company, then  O.Name = 'RealPage Employee'

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

Go



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

,('GetRoleEndpoint','Role End point for product API','/roles/{0}')
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



