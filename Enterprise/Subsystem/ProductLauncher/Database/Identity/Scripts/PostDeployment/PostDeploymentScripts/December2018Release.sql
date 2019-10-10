

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
DECLARE @TRightShortName NVARCHAR(100);
DECLARE @TargetProductId INT;
DECLARE @VisibilityStatusID INT;


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
VALUES( 1, 'Ability to Import users', 
'This right provides access to the new Import Users feature on the User list page.  Ability to view users right is also required. An Import Users tab is also visible in the Migration Tool if the user also has the Ability to Import Users right and the Ability to Migrate Users right.', 
'AbilityToImportUsers' );

SELECT @ProductId = ProductId
FROM Enterprise.Product
WHERE name = 'Unified Platform';

--select * from enterprise.product where name like '%spend%'

SELECT @TargetProductId = ProductId
FROM Enterprise.Product
WHERE Name = 'Migration Tool Application';

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
	WHERE ObjectValue = 'Import users' AND 
		  ParentActionId IS NULL
)
BEGIN
	EXEC Enterprise.CreateAction @ProductID = @ProductId, @Action = N'Import users', @ActionTarget = N'Right', @ActionbValueTypeId = 1, @Description = '', @ActionID = @ActionID OUTPUT;
	SELECT @ActionID AS N'@ActionID';
END;

SELECT @ParentActionId = ActionId
FROM Enterprise.ACTION
WHERE ObjectValue = 'UsersList' AND 
	  ObjectType = 'Route' AND 
	  Description = 'SuperUser';

IF NOT EXISTS
(
	SELECT 1
	FROM Enterprise.ACTION
	WHERE ObjectValue = 'Import users' AND 
		  ParentActionID = @ParentActionId
)
BEGIN
	EXEC [Enterprise].[CreateAction] @ProductID = @ProductId, @Action = N'Import users', @ActionTarget = N'Right', @ActionbValueTypeId = 1, @Description = '', @ParentActionID = @ParentActionId, @ActionID = @ActionID OUTPUT;
	SELECT @ActionID AS N'@ActionID';
END;

SELECT DISTINCT 
	   IDENTITY(int, 1, 1) AS RowNumber, OrganizationPartyID, 0 AS PStatus
INTO #HoldPartyForUnifiedSettings
FROM Person.Persona AS P
	 INNER JOIN
	 Enterprise.Organization AS O
	 ON P.OrganizationPartyId = O.PartyId
	 AND O.Name = 'Realpage Employee'


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
		EXECUTE Enterprise.CreateRight @RoleId = -1, @RightName = 'Default_AbilityToImportUsers', @ShortName = 'AbilityToImportUsers', @RightCategoryId = @RightCategory, @PartyId = @PartyId, @ProductId = @ProductId, @Description = '',  @TargetProductId = @TargetProductId, @VisibilityStatusId = @VisibilityStatusId, @RightId = @RightId OUTPUT;
		EXECUTE Enterprise.CreateRight @RoleId = @RoleId, @RightName = @TRightName, @RightCategoryId = @RightCategory, @PartyId = @PartyId, @ProductId = @ProductId, @Shortname = @TRightShortName, @Description = @TRightDesc, @TargetProductId = @TargetProductId, @VisibilityStatusId = @VisibilityStatusId, @RightId = @RightId OUTPUT;


		SELECT @ActionID = ActionID
		FROM Enterprise.ACTION
		WHERE ObjectValue = 'Import users' AND 
			  ObjectType = 'Right' AND 
			  ParentActionId IS NULL;
		EXEC [Enterprise].[LinkActionToRights] @ActionID = @ActionID, @RightId = @RightId, @StatusId = @Status, @UserActionId = @UserActionId OUTPUT;

		SELECT @RightId = RightId
		FROM Enterprise.[Right] AS R
			 INNER JOIN
			 Enterprise.RightValueType AS RR
			 ON RR.RightValueTypeId = R.RightValueTypeId
		WHERE RR.Value = 'Default_AbilityToImportUsers';
		SELECT @ActionID = ActionID
		FROM Enterprise.ACTION
		WHERE ObjectValue = 'Import users' AND 
			  ObjectType = 'Right' AND 
			  ParentActionId IS NULL;
		EXEC [Enterprise].[LinkActionToRights] @ActionID = @ActionID, @RightId = @RightId, @StatusId = @Status, @UserActionId = @UserActionId OUTPUT;

		SELECT @ActionID = ActionID
            FROM Enterprise.Action
            WHERE ObjectValue = 'Userslist'
                  AND ObjectType = 'ROUTE'
                  AND Description = 'SuperUser';
            SELECT @RoleId = RoleID
            FROM Enterprise.Role AS R
                INNER JOIN Enterprise.RoleValueType AS RVT
                    ON RVT.RoleValueTypeId = R.RoleValueTypeId
            WHERE RVT.Value = 'User Administrator'
                  AND R.PartyID = @PartyId;
            SELECT @RightID = R.RightID
            FROM Enterprise.[Right] AS R
                INNER JOIN Enterprise.RightValueType AS RVT
                    ON RVT.RightValueTypeId = R.RightValueTypeId
            WHERE RVT.Value = 'Ability to Import users'
                  AND R.RoleID = @RoleId;
            EXEC Enterprise.LinkActionToRights @ActionID = @ActionID,
                                               @RightId = @RightID,
                                               @StatusId = @Status,
                                               @UserActionId = @UserActionID OUTPUT;

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
WHERE value = 'Default_AbilityToImportUsers';

SELECT @RightValueTypeId = RightValueTypeId
FROM Enterprise.RightValueType
WHERE value IN( 'Ability to Import users' );

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
SET NOCOUNT ON;


DECLARE @ProductId INT , 
		 @CurrentProductConfigurationID INT , 
		 @ProductSettingTypeId INT , 
		 @ProductSettingId INT , 
		 @ProductNotAvailableForRegularUserNoEmail INT 
		 

IF
(
	SELECT 1
	FROM Enterprise.ProductSettingType
	WHERE Name = 'ProductNotAvailableForRegularUserNoEmail'
) IS NULL
BEGIN
	EXEC Enterprise.CreateProductSettingType 'ProductNotAvailableForRegularUserNoEmail', 'Product Attribute for Product Not Available for Regular User No Email.', @ProductSettingTypeId OUTPUT;
END;


DECLARE @ProductSetting TABLE
( 
 ProductId int, ProductNotAvailableForRegularUserNoEmail int
);

INSERT INTO @ProductSetting( ProductId, ProductNotAvailableForRegularUserNoEmail )
VALUES	( 1,  0 ), -- OneSite
		( 3,  0 ), -- Unified Platform
		( 4,  1 ), -- Asset Optimization
		( 6,  0 ), -- Lead2Lease
		( 7,  0 ), -- NOT USED
		( 8,  0 ), -- RealPage Accounting
		( 9,  0 ), -- Marketing Center
		( 10, 0 ), -- Prospect Contact Center
		( 13, 0 ), -- Spend Management
		( 14, 1 ), -- Client Portal
		( 15, 0 ), -- Renters Insurance
		( 16, 0 ), -- Vendor Services
		( 17, 0 ), -- Resident Portals
		( 18, 0 ), -- Utility Management
		( 19, 0 ), -- Product Learning Portal
		( 20, 0 ), -- RealPage Document Management
		( 21, 0 ), -- OneSite Conversions
		( 23, 0 ), -- On-Site
		( 24, 0 ), -- Research Application
		( 25, 0 ), -- Self-provisioning portal
		( 26, 0 ), -- Unified Amenities
		( 27, 0 ), -- Migration Tool Application
		( 28, 0 ), -- Product Updates
		( 29, 1 ), -- Business Intelligence
		( 30, 1 ), -- Performance Analytics
		( 31, 1 ), -- Investment Analytics
		( 32, 1 ), -- Revenue Management
		( 33, 0 ), -- Axiometrics
		( 34, 0 ), -- Benchmarking
		( 35, 0 ), -- Support Tool
		( 36, 0 ), -- EasyLMS
		( 37, 0 ), -- PropertyPhotos
		( 38, 0 ), -- VendorMarketplace
		( 39, 0 ), -- Integrations Marketplace
		( 40, 0 ), -- Intelligent Lead Management
		( 41, 1 ), -- Intelligent Lead Management-Leasing Analytics 
		( 42, 0 ), -- SalesForce
		( 43, 0 ), -- Settings Management
		( 44, 0 )  -- Portfolio Management


DECLARE @NOW datetime= GETUTCDATE();

DECLARE Products CURSOR
FOR SELECT ProductId, ProductNotAvailableForRegularUserNoEmail
	FROM @ProductSetting;

OPEN Products;

FETCH Products INTO @ProductId, @ProductNotAvailableForRegularUserNoEmail;

WHILE @@FETCH_STATUS = 0
BEGIN
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

	
	--- ProductNotAvailableForRegularUserNoEmail 
	SET @ProductSettingTypeId = NULL;
	EXEC Enterprise.GetProductSettingType @Name = 'ProductNotAvailableForRegularUserNoEmail', @ProductSettingTypeId = @ProductSettingTypeId OUTPUT;

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
		@Value = @ProductNotAvailableForRegularUserNoEmail, 
		@FromDate = @NOW, -- datetime
		@ThruDate = NULL, -- datetime
		@ProductSettingId = @ProductSettingId OUTPUT; -- int

		-- Link the Product Setting to an actual configuration
		EXEC Enterprise.LinkProductSettingToConfiguration @ConfigurationId = @CurrentProductConfigurationID, -- int
		@ProductSettingId = @ProductSettingId, -- int
		@FromDate = @NOW, -- datetime
		@ThruDate = NULL;   -- datetime
	END;
	--- ProductNotAvailableForRegularUserNoEmail 

	FETCH Products INTO @ProductId, @ProductNotAvailableForRegularUserNoEmail;
END;
GO

IF EXISTS (SELECT 1 FROM Enterprise.RightValueType WHERE Value = 'Access to Vendor MarketPlace')
BEGIN
	UPDATE Enterprise.RightValueType 
	SET Value = 'Access to GetWork'
	WHERE Value = 'Access to Vendor MarketPlace'
END
GO


--Monte's PME Fix
DECLARE @OLDRoleID BIGINT
DECLARE @NEWRoleId BIGINT

SELECT @OLDRoleID = RoleID 
FROM Enterprise.Role
INNER JOIN Enterprise.RoleValueType on Role.RoleValueTypeId = RoleValueType.RoleValueTypeId
WHERE RoleValueType.Value = 'Migration Analyst'

SELECT @NEWRoleID = RoleID 
FROM Enterprise.Role
INNER JOIN Enterprise.RoleValueType on Role.RoleValueTypeId = RoleValueType.RoleValueTypeId
WHERE RoleValueType.Value = 'Implementation Analyst'

UPDATE Enterprise.PersonaPrivilege 
SET RoleID = @NEWRoleId
WHERE RoleID = @OLDRoleID



-- GB3628
DECLARE @OrganizationId INT;
DECLARE @RightValueTypeId INT;
DECLARE @ROleId INT;
DECLARE @ProductId INT;
DECLARE ListOrgnaizations CURSOR FOR
SELECT DISTINCT
       R.PartyId,
       R.RoleID
FROM Enterprise.RightValueType RVT
    INNER JOIN Enterprise.[Right] R
        ON R.RightValueTypeId = RVT.RightValueTypeId
    INNER JOIN Enterprise.Role RL
        ON RL.RoleID = R.RoleID
	INNER JOIN Enterprise.RoleValueType RVT1
		ON RVT1.RoleValueTypeId = RL.RoleValueTypeId
WHERE RVT1.Value = 'User Administrator'
ORDER BY R.PartyId;
OPEN ListOrgnaizations;
FETCH ListOrgnaizations
INTO @OrganizationId,
     @ROleId;
WHILE @@FETCH_STATUS = 0
BEGIN
    SELECT @RightValueTypeId = RightValueTypeId
    FROM Enterprise.RightValueType
    WHERE Value = 'Access to Leasing & Rents Conversion Tool for OneSite users';

	EXECUTE Enterprise.AssignRightToRole @OrganizationId = @OrganizationId,
                                             @RightValueTypeId = @RightValueTypeId,
                                             @ROleId = @ROleId,
                                             @AssignRightToRole = 1;
    FETCH ListOrgnaizations
    INTO @OrganizationId,
         @ROleId;
END;
CLOSE ListOrgnaizations;
DEALLOCATE ListOrgnaizations;
GO

DECLARE @OrganizationId INT;
DECLARE @RightValueTypeId INT;
DECLARE @ROleId INT;
DECLARE @ProductId INT;
DECLARE ListOrgnaizations CURSOR FOR
SELECT DISTINCT
       R.PartyId,
       R.RoleID
FROM Enterprise.RightValueType RVT
    INNER JOIN Enterprise.[Right] R
        ON R.RightValueTypeId = RVT.RightValueTypeId
    INNER JOIN Enterprise.Role RL
        ON RL.RoleID = R.RoleID
	INNER JOIN Enterprise.RoleValueType RVT1
		ON RVT1.RoleValueTypeId = RL.RoleValueTypeId
WHERE RVT1.Value = 'User Administrator'
ORDER BY R.PartyId;
OPEN ListOrgnaizations;
FETCH ListOrgnaizations
INTO @OrganizationId,
     @ROleId;
WHILE @@FETCH_STATUS = 0
BEGIN
    SELECT @RightValueTypeId = RightValueTypeId
    FROM Enterprise.RightValueType
    WHERE Value = 'Access to Property Photos';

	EXECUTE Enterprise.AssignRightToRole @OrganizationId = @OrganizationId,
                                             @RightValueTypeId = @RightValueTypeId,
                                             @ROleId = @ROleId,
                                             @AssignRightToRole = 1;
    FETCH ListOrgnaizations
    INTO @OrganizationId,
         @ROleId;
END;
CLOSE ListOrgnaizations;
DEALLOCATE ListOrgnaizations;
GO


DECLARE @OrganizationId INT;
DECLARE @RightValueTypeId INT;
DECLARE @ROleId INT;
DECLARE @ProductId INT;
DECLARE ListOrgnaizations CURSOR FOR
SELECT DISTINCT
       R.PartyId,
       R.RoleID
FROM Enterprise.RightValueType RVT
    INNER JOIN Enterprise.[Right] R
        ON R.RightValueTypeId = RVT.RightValueTypeId
    INNER JOIN Enterprise.Role RL
        ON RL.RoleID = R.RoleID
	INNER JOIN Enterprise.RoleValueType RVT1
		ON RVT1.RoleValueTypeId = RL.RoleValueTypeId
WHERE RVT1.Value = 'User Administrator'
ORDER BY R.PartyId;
OPEN ListOrgnaizations;
FETCH ListOrgnaizations
INTO @OrganizationId,
     @ROleId;
WHILE @@FETCH_STATUS = 0
BEGIN
    SELECT @RightValueTypeId = RightValueTypeId
    FROM Enterprise.RightValueType
    WHERE Value IN  ('Access to Vendor Marketplace', 'Access to GetWork');

	EXECUTE Enterprise.AssignRightToRole @OrganizationId = @OrganizationId,
                                             @RightValueTypeId = @RightValueTypeId,
                                             @ROleId = @ROleId,
                                             @AssignRightToRole = 1;
    FETCH ListOrgnaizations
    INTO @OrganizationId,
         @ROleId;
END;
CLOSE ListOrgnaizations;
DEALLOCATE ListOrgnaizations;
GO

--Fix target prodcutId for bunch of rights that were missed in the release and testing.
DECLARE @ProductId INT

IF EXISTS (SELECT 1 FROM Enterprise.RightValueTYpe WHERE ShortName = 'MigrationTool')
BEGIN
	SELECT @ProductId = ProductId FROM Enterprise.Product WHERE Name = 'Migration Tool Application'
	UPDATE Enterprise.RightValueType SET TargetProductId = @ProductId WHERE ShortName = 'MigrationTool'
END


IF EXISTS (SELECT 1 FROM Enterprise.RightValueTYpe WHERE ShortName = 'AccessOneSiteConversions')
BEGIN
	SELECT @ProductId = ProductId FROM Enterprise.Product WHERE Name = 'Leasing & Rents Conversion Tool'
	UPDATE Enterprise.RightValueType SET TargetProductId = @ProductId WHERE ShortName = 'AccessOneSiteConversions'
END

IF EXISTS (SELECT 1 FROM Enterprise.RightValueTYpe WHERE ShortName = 'AccessVendorMarketplace')
BEGIN
	SELECT @ProductId = ProductId FROM Enterprise.Product WHERE Name = 'GetWork'
	UPDATE Enterprise.RightValueType SET TargetProductId = @ProductId WHERE ShortName = 'AccessVendorMarketplace'
END

IF EXISTS (SELECT 1 FROM Enterprise.RightValueTYpe WHERE ShortName = 'AccessPropertyPhotos')
BEGIN
	SELECT @ProductId = ProductId FROM Enterprise.Product WHERE Name = 'Property Photos'
	UPDATE Enterprise.RightValueType SET TargetProductId = @ProductId WHERE ShortName = 'AccessPropertyPhotos'
END
GO
--DECLARE @OrganizationId INT;
--DECLARE @RightValueTypeId INT;
--DECLARE @ROleId INT;
--DECLARE @ProductId INT;
--DECLARE ListOrgnaizations CURSOR FOR
--SELECT DISTINCT
--       R.PartyId,
--       R.RoleID
--FROM Enterprise.RightValueType RVT
--    INNER JOIN Enterprise.[Right] R
--        ON R.RightValueTypeId = RVT.RightValueTypeId
--    INNER JOIN Enterprise.Role RL
--        ON RL.RoleID = R.RoleID
--	INNER JOIN Enterprise.RoleValueType RVT1
--		ON RVT1.RoleValueTypeId = RL.RoleValueTypeId
--WHERE RVT1.Value = 'User Administrator'
--ORDER BY R.PartyId;
--OPEN ListOrgnaizations;
--FETCH ListOrgnaizations
--INTO @OrganizationId,
--     @ROleId;
--WHILE @@FETCH_STATUS = 0
--BEGIN
--    SELECT @RightValueTypeId = RightValueTypeId
--    FROM Enterprise.RightValueType
--    WHERE Value = 'Ability to Migrate Users';

--	EXECUTE Enterprise.AssignRightToRole @OrganizationId = @OrganizationId,
--                                             @RightValueTypeId = @RightValueTypeId,
--                                             @ROleId = @ROleId,
--                                             @AssignRightToRole = 1;
--    FETCH ListOrgnaizations
--    INTO @OrganizationId,
--         @ROleId;
--END;
--CLOSE ListOrgnaizations;
--DEALLOCATE ListOrgnaizations;
--GO

DECLARE @ProductTypeId INT= 701, 
@ProductId INT, 
@LoginURI NVARCHAR(100), 
@SigningCertificateThumbprint NVARCHAR(50), 
@ParentProductTypeId INT, 
@ProductName NVARCHAR(100)= 'Portfolio Management';
DECLARE @ProductConfiguration AS PRODUCTCONFIGURATIONTYPE;

--Create root product type
IF NOT EXISTS
(
    SELECT TOP 1 1
    FROM Enterprise.ProductType
    WHERE Name = 'Asset and Investment Management'
)
    BEGIN
        EXEC [Enterprise].[CreateProductType]
             @ProductTypeId = 700,
             @ParentProductTypeId = NULL,
             @Name = 'Asset and Investment Management',
             @Description = 'Asset and Investment Management',
             @ProductTypeGUID = '9A480BD2-D0E8-4D43-AD26-9A2437B517AA';
    END;

SELECT @ParentProductTypeId = ProductTypeId
FROM Enterprise.ProductType
WHERE Name = 'Asset and Investment Management'
      AND ParentProductTypeId IS NULL;

IF NOT EXISTS
(
    SELECT TOP 1 1
    FROM enterprise.ProductType
    WHERE Name = 'Portfolio Management'
)
    BEGIN
        EXEC [Enterprise].[CreateProductType]
             @ProductTypeId = @ProductTypeId,
             @ParentProductTypeId = @ParentProductTypeId,
             @Name = @ProductName,
             @Description = 'Portfolio Management',
             @ProductTypeGUID = 'B7402864-0FBE-41CD-A232-C575E7C04758';
    END;

SET @ProductId = 44;

IF NOT EXISTS
(
    SELECT 1
    FROM Enterprise.Product
    WHERE Name = @ProductName
)
    BEGIN
        EXEC Enterprise.CreateProduct
             @ProductId = @ProductId,
             @ProductGUID = 'A9ECAD0F-9374-42AC-975C-CB46431F9D9F',
             @Name = @ProductName,
             @Description = @ProductName,
             @ProductTypeId = 701;
        UPDATE Enterprise.Product
          SET
              BooksProductCode = 'RPM'
        WHERE ProductId = @ProductId;
    END;



INSERT INTO @ProductConfiguration
(SettingName,
 SettingDescription,
 SettingValue
)
VALUES
('ClientId','','1')
,('ClassName','','portfoliomanagement')
,('ProductUrl','','/product/portfoliomanagement')
,('TitleId','','Portfolio Management')
,('TitleUniqueId','','8798E17D-249F-4C7D-89D5-72523437D71D')
,('IsNewTab','','1')
,('MetatagUniqueId','','PortfolioManagement')
,('IsResource','','0')
,('IsFavorite','','1')
,('LearnMore','','https://www.realpage.com/')
,('ApiEndPoint','','https://wmu-books.asseteye.net')
,('ProductStatus','Show if the external application was configured for the dashboard user.','8')
,('ShowInUserDetails','Should the product show in the New/Edit user pages','1')
,('ShowInRolesAndRights','Should the product show in the Role/Rights page','0')
,('ShowInAppSwitcher','Should the product show in the application switcher','1')
,('ShowInUserListFilter','Should the product show in the user list product pick list','1')
,('ProductAPIRequiresUser','Does the product require a user for api calls','0')
,('GetRoleEndpoint','Role End point for product API','/api/{0}/Roles?isGlobalRoles={1}')
,('GetUserEndpoint','GET User Endpoint for product API','/api/Users?companyId={0}&loginname={1}')
,('PostUserEndpoint','POST User Endpoint for product API','/api/users')
,('PutUserEndpoint','PUT User Endpoint for product API','/api/users')
,('DeleteUserEndpoint','DELETE User Endpoint for product API','/api/{companyId}/users?loginName={0}')
,('PutProfileEndpoint','PUT Profile Endpoint for product API','/api/users/profile')
,('GetPropertyEndpoint','GET Property Endpoint for API','/api/{0}/Properties')
,('TokenClientId','','apiuser')
,('TokenClientSecret','','apiU$3r')
,('GetLoginUrlEndpoint','GET Product Login URL Endpoint','/api/{0}/LoginURL')
,('ProductNotAvailableForRegularUserNoEmail','Product Attribute for Product Not Available for Regular User No Email.','0')
,('GetListUsersEndpoint','','/api/{0}/users?filter={1}&startRow={2}&resultsperpage={3}')
,('PatchMigrateUsersEndpoint','', '/api/{0}/migrate-users')
,('LockOnProductAccess', '', 0)



SET @ProductID = 44
SET @LoginURI = 'https://qa-books.asseteye.net/api/{{orgid}}/LoginURL'
SET @SigningCertificateThumbprint = 'EF26FEC08C554976572E8A9767DDA437AC452CF6'



EXEC Enterprise.ProductConfigurationSetup @ProductId, @LoginURI, @SigningCertificateThumbprint, @ProductConfiguration;

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
DECLARE @TRightShortName NVARCHAR(100);
DECLARE @TargetProductId INT;
DECLARE @VisibilityStatusID INT;


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
VALUES( 1, 'Manage Portfolio Management Product Access', 
'For  Portfolio Management, this right unlocks the ability to edit the Product assignment and Product Access Details for a user, assuming that the user can access the page because of Ability to view users.  All of the Product Rights are included in the System Role for the User Administrator, if the customer has the product.', 
'ManagePortfolioManagementProductAccess' );

SELECT @ProductId = ProductId
FROM Enterprise.Product
WHERE name = 'Unified Platform';

--select * from enterprise.product where name like '%lead%'

SELECT @TargetProductId = ProductId
FROM Enterprise.Product
WHERE Name = 'Portfolio Management';

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
	WHERE ObjectValue = 'Portfolio Management' AND 
		  ParentActionId IS NULL
)
BEGIN
	EXEC Enterprise.CreateAction @ProductID = @ProductId, @Action = N'Portfolio Management', @ActionTarget = N'Right', @ActionbValueTypeId = 1, @Description = '', @ActionID = @ActionID OUTPUT;
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
	WHERE ObjectValue = 'Portfolio Management' AND 
		  ParentActionID = @ParentActionId
)
BEGIN
	EXEC [Enterprise].[CreateAction] @ProductID = @ProductId, @Action = N'Portfolio Management', @ActionTarget = N'Right', @ActionbValueTypeId = 1, @Description = '', @ParentActionID = @ParentActionId, @ActionID = @ActionID OUTPUT;
	SELECT @ActionID AS N'@ActionID';
END;

SELECT DISTINCT 
	   IDENTITY(int, 1, 1) AS RowNumber, OrganizationPartyID, 0 AS PStatus
INTO #HoldPartyForUnifiedSettings
FROM Person.Persona AS P
	 INNER JOIN
	 Enterprise.Organization AS O
	 ON P.OrganizationPartyId = O.PartyId
--WHERE O.PartyId = 350;

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
		EXECUTE Enterprise.CreateRight @RoleId = -1, @RightName = 'Default_ManagePortfolioManagementProductAccess', @ShortName = 'ManagePortfolioManagementProductAccess', @RightCategoryId = @RightCategory, @PartyId = @PartyId, @ProductId = @ProductId, @Description = '',  @TargetProductId = @TargetProductId, @VisibilityStatusId = @VisibilityStatusId, @RightId = @RightId OUTPUT;
		EXECUTE Enterprise.CreateRight @RoleId = @RoleId, @RightName = @TRightName, @RightCategoryId = @RightCategory, @PartyId = @PartyId, @ProductId = @ProductId, @Shortname = @TRightShortName, @Description = @TRightDesc, @TargetProductId = @TargetProductId, @VisibilityStatusId = @VisibilityStatusId, @RightId = @RightId OUTPUT;


		SELECT @ActionID = ActionID
		FROM Enterprise.ACTION
		WHERE ObjectValue = 'Portfolio Management' AND 
			  ObjectType = 'Right' AND 
			  ParentActionId IS NULL;
		EXEC [Enterprise].[LinkActionToRights] @ActionID = @ActionID, @RightId = @RightId, @StatusId = @Status, @UserActionId = @UserActionId OUTPUT;

		SELECT @RightId = RightId
		FROM Enterprise.[Right] AS R
			 INNER JOIN
			 Enterprise.RightValueType AS RR
			 ON RR.RightValueTypeId = R.RightValueTypeId
		WHERE RR.Value = 'Default_ManagePortfolioManagementProductAccess';
		SELECT @ActionID = ActionID
		FROM Enterprise.ACTION
		WHERE ObjectValue = 'Portfolio Management'  AND 
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
WHERE value = 'Default_ManagePortfolioManagementProductAccess';

SELECT @RightValueTypeId = RightValueTypeId
FROM Enterprise.RightValueType
WHERE value IN( 'Manage Portfolio Management Product Access' );

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
IF NOT EXISTS(SELECT 1 FROM Enterprise.StatusType WHERE Name = 'Stop')
BEGIN
	SET IDENTITY_INSERT Enterprise.StatusTYpe ON
	INSERT INTO Enterprise.StatusType (StatusTypeId, Name)
	VALUES (20, 'Stop')
	SET IDENTITY_INSERT Enterprise.StatusTYpe OFF
END
IF NOT EXISTS(SELECT 1 FROM Enterprise.StatusType WHERE Name = 'Pause')
BEGIN
	SET IDENTITY_INSERT Enterprise.StatusTYpe ON
	INSERT INTO Enterprise.StatusType (StatusTypeId, Name)
	VALUES (21, 'Pause')
	SET IDENTITY_INSERT Enterprise.StatusTYpe OFF
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
DECLARE @TRightShortName NVARCHAR(100);
DECLARE @TargetProductId INT;
DECLARE @VisibilityStatusID INT;


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
VALUES( 1, 'Ability to Import users', 
'This right provides access to the new Import Users feature on the User list page.  Ability to view users right is also required. An Import Users tab is also visible in the Migration Tool if the user also has the Ability to Import Users right and the Ability to Migrate Users right.', 
'AbilityToImportUsers' );

SELECT @ProductId = ProductId
FROM Enterprise.Product
WHERE name = 'Unified Platform';

--select * from enterprise.product where name like '%spend%'

SELECT @TargetProductId = ProductId
FROM Enterprise.Product
WHERE Name = 'Unified Platform';

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
	WHERE ObjectValue = 'Import users' AND 
		  ParentActionId IS NULL
)
BEGIN
	EXEC Enterprise.CreateAction @ProductID = @ProductId, @Action = N'Import users', @ActionTarget = N'Right', @ActionbValueTypeId = 1, @Description = '', @ActionID = @ActionID OUTPUT;
	SELECT @ActionID AS N'@ActionID';
END;

SELECT @ParentActionId = ActionId
FROM Enterprise.ACTION
WHERE ObjectValue = 'UsersList' AND 
	  ObjectType = 'Route' AND 
	  Description = 'SuperUser';

IF NOT EXISTS
(
	SELECT 1
	FROM Enterprise.ACTION
	WHERE ObjectValue = 'Import users' AND 
		  ParentActionID = @ParentActionId
)
BEGIN
	EXEC [Enterprise].[CreateAction] @ProductID = @ProductId, @Action = N'Import users', @ActionTarget = N'Right', @ActionbValueTypeId = 1, @Description = '', @ParentActionID = @ParentActionId, @ActionID = @ActionID OUTPUT;
	SELECT @ActionID AS N'@ActionID';
END;

SELECT DISTINCT 
	   IDENTITY(int, 1, 1) AS RowNumber, OrganizationPartyID, 0 AS PStatus
INTO #HoldPartyForUnifiedSettings
FROM Person.Persona AS P
	 INNER JOIN
	 Enterprise.Organization AS O
	 ON P.OrganizationPartyId = O.PartyId
WHERE O.name  = 'RealPage Employee';

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
		EXECUTE Enterprise.CreateRight @RoleId = -1, @RightName = 'Default_AbilityToImportUsers', @ShortName = 'AbilityToImportUsers', @RightCategoryId = @RightCategory, @PartyId = @PartyId, @ProductId = @ProductId, @Description = '',  @TargetProductId = @TargetProductId, @VisibilityStatusId = @VisibilityStatusId, @RightId = @RightId OUTPUT;
		EXECUTE Enterprise.CreateRight @RoleId = @RoleId, @RightName = @TRightName, @RightCategoryId = @RightCategory, @PartyId = @PartyId, @ProductId = @ProductId, @Shortname = @TRightShortName, @Description = @TRightDesc, @TargetProductId = @TargetProductId, @VisibilityStatusId = @VisibilityStatusId, @RightId = @RightId OUTPUT;


		SELECT @ActionID = ActionID
		FROM Enterprise.ACTION
		WHERE ObjectValue = 'Import users' AND 
			  ObjectType = 'Right' AND 
			  ParentActionId IS NULL;
		EXEC [Enterprise].[LinkActionToRights] @ActionID = @ActionID, @RightId = @RightId, @StatusId = @Status, @UserActionId = @UserActionId OUTPUT;

		SELECT @RightId = RightId
		FROM Enterprise.[Right] AS R
			 INNER JOIN
			 Enterprise.RightValueType AS RR
			 ON RR.RightValueTypeId = R.RightValueTypeId
		WHERE RR.Value = 'Default_AbilityToImportUsers';
		SELECT @ActionID = ActionID
		FROM Enterprise.ACTION
		WHERE ObjectValue = 'Import users' AND 
			  ObjectType = 'Right' AND 
			  ParentActionId IS NULL;
		EXEC [Enterprise].[LinkActionToRights] @ActionID = @ActionID, @RightId = @RightId, @StatusId = @Status, @UserActionId = @UserActionId OUTPUT;

		SELECT @ActionID = ActionID
            FROM Enterprise.Action
            WHERE ObjectValue = 'Userslist'
                  AND ObjectType = 'ROUTE'
                  AND Description = 'SuperUser';
            SELECT @RoleId = RoleID
            FROM Enterprise.Role AS R
                INNER JOIN Enterprise.RoleValueType AS RVT
                    ON RVT.RoleValueTypeId = R.RoleValueTypeId
            WHERE RVT.Value = 'User Administrator'
                  AND R.PartyID = @PartyId;
            SELECT @RightID = R.RightID
            FROM Enterprise.[Right] AS R
                INNER JOIN Enterprise.RightValueType AS RVT
                    ON RVT.RightValueTypeId = R.RightValueTypeId
            WHERE RVT.Value = 'Ability to Import users'
                  AND R.RoleID = @RoleId;
            EXEC Enterprise.LinkActionToRights @ActionID = @ActionID,
                                               @RightId = @RightID,
                                               @StatusId = @Status,
                                               @UserActionId = @UserActionID OUTPUT;

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
WHERE value = 'Default_AbilityToImportUsers';

SELECT @RightValueTypeId = RightValueTypeId
FROM Enterprise.RightValueType
WHERE value IN( 'Ability to Import users' );

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
SET NOCOUNT ON;


DECLARE @ProductId INT , 
		 @CurrentProductConfigurationID INT , 
		 @ProductSettingTypeId INT , 
		 @ProductSettingId INT , 
		 @ProductNotAvailableForRegularUserNoEmail INT 
		 

IF
(
	SELECT 1
	FROM Enterprise.ProductSettingType
	WHERE Name = 'ProductNotAvailableForRegularUserNoEmail'
) IS NULL
BEGIN
	EXEC Enterprise.CreateProductSettingType 'ProductNotAvailableForRegularUserNoEmail', 'Product Attribute for Product Not Available for Regular User No Email.', @ProductSettingTypeId OUTPUT;
END;


DECLARE @ProductSetting TABLE
( 
 ProductId int, ProductNotAvailableForRegularUserNoEmail int
);

INSERT INTO @ProductSetting( ProductId, ProductNotAvailableForRegularUserNoEmail )
VALUES	( 1,  0 ), -- OneSite
		( 3,  0 ), -- Unified Platform
		( 4,  1 ), -- Asset Optimization
		( 6,  0 ), -- Lead2Lease
		( 7,  0 ), -- NOT USED
		( 8,  0 ), -- RealPage Accounting
		( 9,  0 ), -- Marketing Center
		( 10, 0 ), -- Prospect Contact Center
		( 13, 0 ), -- Spend Management
		( 14, 1 ), -- Client Portal
		( 15, 0 ), -- Renters Insurance
		( 16, 0 ), -- Vendor Services
		( 17, 0 ), -- Resident Portals
		( 18, 0 ), -- Utility Management
		( 19, 0 ), -- Product Learning Portal
		( 20, 0 ), -- RealPage Document Management
		( 21, 0 ), -- OneSite Conversions
		( 23, 0 ), -- On-Site
		( 24, 0 ), -- Research Application
		( 25, 0 ), -- Self-provisioning portal
		( 26, 0 ), -- Unified Amenities
		( 27, 0 ), -- Migration Tool Application
		( 28, 0 ), -- Product Updates
		( 29, 1 ), -- Business Intelligence
		( 30, 1 ), -- Performance Analytics
		( 31, 1 ), -- Investment Analytics
		( 32, 1 ), -- Revenue Management
		( 33, 0 ), -- Axiometrics
		( 34, 0 ), -- Benchmarking
		( 35, 0 ), -- Support Tool
		( 36, 0 ), -- EasyLMS
		( 37, 0 ), -- PropertyPhotos
		( 38, 0 ), -- VendorMarketplace
		( 39, 0 ), -- Integrations Marketplace
		( 40, 0 ), -- Intelligent Lead Management
		( 41, 1 ), -- Intelligent Lead Management-Leasing Analytics 
		( 42, 0 ), -- SalesForce
		( 43, 0 ), -- Settings Management
		( 44, 0 )  -- Portfolio Management


DECLARE @NOW datetime= GETUTCDATE();

DECLARE Products CURSOR
FOR SELECT ProductId, ProductNotAvailableForRegularUserNoEmail
	FROM @ProductSetting;

OPEN Products;

FETCH Products INTO @ProductId, @ProductNotAvailableForRegularUserNoEmail;

WHILE @@FETCH_STATUS = 0
BEGIN
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

	
	--- ProductNotAvailableForRegularUserNoEmail 
	SET @ProductSettingTypeId = NULL;
	EXEC Enterprise.GetProductSettingType @Name = 'ProductNotAvailableForRegularUserNoEmail', @ProductSettingTypeId = @ProductSettingTypeId OUTPUT;

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
		@Value = @ProductNotAvailableForRegularUserNoEmail, 
		@FromDate = @NOW, -- datetime
		@ThruDate = NULL, -- datetime
		@ProductSettingId = @ProductSettingId OUTPUT; -- int

		-- Link the Product Setting to an actual configuration
		EXEC Enterprise.LinkProductSettingToConfiguration @ConfigurationId = @CurrentProductConfigurationID, -- int
		@ProductSettingId = @ProductSettingId, -- int
		@FromDate = @NOW, -- datetime
		@ThruDate = NULL;   -- datetime
	END;
	--- ProductNotAvailableForRegularUserNoEmail 

	FETCH Products INTO @ProductId, @ProductNotAvailableForRegularUserNoEmail;
END;
GO

IF EXISTS (SELECT 1 FROM Enterprise.RightValueType WHERE Value = 'Access to Vendor MarketPlace')
BEGIN
	UPDATE Enterprise.RightValueType 
	SET Value = 'Access to GetWork'
	WHERE Value = 'Access to Vendor MarketPlace'
END
GO


--Monte's PME Fix
DECLARE @OLDRoleID BIGINT
DECLARE @NEWRoleId BIGINT

SELECT @OLDRoleID = RoleID 
FROM Enterprise.Role
INNER JOIN Enterprise.RoleValueType on Role.RoleValueTypeId = RoleValueType.RoleValueTypeId
WHERE RoleValueType.Value = 'Migration Analyst'

SELECT @NEWRoleID = RoleID 
FROM Enterprise.Role
INNER JOIN Enterprise.RoleValueType on Role.RoleValueTypeId = RoleValueType.RoleValueTypeId
WHERE RoleValueType.Value = 'Implementation Analyst'

UPDATE Enterprise.PersonaPrivilege 
SET RoleID = @NEWRoleId
WHERE RoleID = @OLDRoleID



-- GB3628
DECLARE @OrganizationId INT;
DECLARE @RightValueTypeId INT;
DECLARE @ROleId INT;
DECLARE @ProductId INT;
DECLARE ListOrgnaizations CURSOR FOR
SELECT DISTINCT
       R.PartyId,
       R.RoleID
FROM Enterprise.RightValueType RVT
    INNER JOIN Enterprise.[Right] R
        ON R.RightValueTypeId = RVT.RightValueTypeId
    INNER JOIN Enterprise.Role RL
        ON RL.RoleID = R.RoleID
	INNER JOIN Enterprise.RoleValueType RVT1
		ON RVT1.RoleValueTypeId = RL.RoleValueTypeId
WHERE RVT1.Value = 'User Administrator'
ORDER BY R.PartyId;
OPEN ListOrgnaizations;
FETCH ListOrgnaizations
INTO @OrganizationId,
     @ROleId;
WHILE @@FETCH_STATUS = 0
BEGIN
    SELECT @RightValueTypeId = RightValueTypeId
    FROM Enterprise.RightValueType
    WHERE Value = 'Access to Leasing & Rents Conversion Tool for OneSite users';

	EXECUTE Enterprise.AssignRightToRole @OrganizationId = @OrganizationId,
                                             @RightValueTypeId = @RightValueTypeId,
                                             @ROleId = @ROleId,
                                             @AssignRightToRole = 1;
    FETCH ListOrgnaizations
    INTO @OrganizationId,
         @ROleId;
END;
CLOSE ListOrgnaizations;
DEALLOCATE ListOrgnaizations;
GO

DECLARE @OrganizationId INT;
DECLARE @RightValueTypeId INT;
DECLARE @ROleId INT;
DECLARE @ProductId INT;
DECLARE ListOrgnaizations CURSOR FOR
SELECT DISTINCT
       R.PartyId,
       R.RoleID
FROM Enterprise.RightValueType RVT
    INNER JOIN Enterprise.[Right] R
        ON R.RightValueTypeId = RVT.RightValueTypeId
    INNER JOIN Enterprise.Role RL
        ON RL.RoleID = R.RoleID
	INNER JOIN Enterprise.RoleValueType RVT1
		ON RVT1.RoleValueTypeId = RL.RoleValueTypeId
WHERE RVT1.Value = 'User Administrator'
ORDER BY R.PartyId;
OPEN ListOrgnaizations;
FETCH ListOrgnaizations
INTO @OrganizationId,
     @ROleId;
WHILE @@FETCH_STATUS = 0
BEGIN
    SELECT @RightValueTypeId = RightValueTypeId
    FROM Enterprise.RightValueType
    WHERE Value = 'Access to Property Photos';

	EXECUTE Enterprise.AssignRightToRole @OrganizationId = @OrganizationId,
                                             @RightValueTypeId = @RightValueTypeId,
                                             @ROleId = @ROleId,
                                             @AssignRightToRole = 1;
    FETCH ListOrgnaizations
    INTO @OrganizationId,
         @ROleId;
END;
CLOSE ListOrgnaizations;
DEALLOCATE ListOrgnaizations;
GO


DECLARE @OrganizationId INT;
DECLARE @RightValueTypeId INT;
DECLARE @ROleId INT;
DECLARE @ProductId INT;
DECLARE ListOrgnaizations CURSOR FOR
SELECT DISTINCT
       R.PartyId,
       R.RoleID
FROM Enterprise.RightValueType RVT
    INNER JOIN Enterprise.[Right] R
        ON R.RightValueTypeId = RVT.RightValueTypeId
    INNER JOIN Enterprise.Role RL
        ON RL.RoleID = R.RoleID
	INNER JOIN Enterprise.RoleValueType RVT1
		ON RVT1.RoleValueTypeId = RL.RoleValueTypeId
WHERE RVT1.Value = 'User Administrator'
ORDER BY R.PartyId;
OPEN ListOrgnaizations;
FETCH ListOrgnaizations
INTO @OrganizationId,
     @ROleId;
WHILE @@FETCH_STATUS = 0
BEGIN
    SELECT @RightValueTypeId = RightValueTypeId
    FROM Enterprise.RightValueType
    WHERE Value IN  ('Access to Vendor Marketplace', 'Access to GetWork');

	EXECUTE Enterprise.AssignRightToRole @OrganizationId = @OrganizationId,
                                             @RightValueTypeId = @RightValueTypeId,
                                             @ROleId = @ROleId,
                                             @AssignRightToRole = 1;
    FETCH ListOrgnaizations
    INTO @OrganizationId,
         @ROleId;
END;
CLOSE ListOrgnaizations;
DEALLOCATE ListOrgnaizations;
GO

--Fix target prodcutId for bunch of rights that were missed in the release and testing.
DECLARE @ProductId INT

IF EXISTS (SELECT 1 FROM Enterprise.RightValueTYpe WHERE ShortName = 'MigrationTool')
BEGIN
	SELECT @ProductId = ProductId FROM Enterprise.Product WHERE Name = 'Migration Tool Application'
	UPDATE Enterprise.RightValueType SET TargetProductId = @ProductId WHERE ShortName = 'MigrationTool'
END


IF EXISTS (SELECT 1 FROM Enterprise.RightValueTYpe WHERE ShortName = 'AccessOneSiteConversions')
BEGIN
	SELECT @ProductId = ProductId FROM Enterprise.Product WHERE Name = 'Leasing & Rents Conversion Tool'
	UPDATE Enterprise.RightValueType SET TargetProductId = @ProductId WHERE ShortName = 'AccessOneSiteConversions'
END

IF EXISTS (SELECT 1 FROM Enterprise.RightValueTYpe WHERE ShortName = 'AccessVendorMarketplace')
BEGIN
	SELECT @ProductId = ProductId FROM Enterprise.Product WHERE Name = 'GetWork'
	UPDATE Enterprise.RightValueType SET TargetProductId = @ProductId WHERE ShortName = 'AccessVendorMarketplace'
END

IF EXISTS (SELECT 1 FROM Enterprise.RightValueTYpe WHERE ShortName = 'AccessPropertyPhotos')
BEGIN
	SELECT @ProductId = ProductId FROM Enterprise.Product WHERE Name = 'Property Photos'
	UPDATE Enterprise.RightValueType SET TargetProductId = @ProductId WHERE ShortName = 'AccessPropertyPhotos'
END
GO
--DECLARE @OrganizationId INT;
--DECLARE @RightValueTypeId INT;
--DECLARE @ROleId INT;
--DECLARE @ProductId INT;
--DECLARE ListOrgnaizations CURSOR FOR
--SELECT DISTINCT
--       R.PartyId,
--       R.RoleID
--FROM Enterprise.RightValueType RVT
--    INNER JOIN Enterprise.[Right] R
--        ON R.RightValueTypeId = RVT.RightValueTypeId
--    INNER JOIN Enterprise.Role RL
--        ON RL.RoleID = R.RoleID
--	INNER JOIN Enterprise.RoleValueType RVT1
--		ON RVT1.RoleValueTypeId = RL.RoleValueTypeId
--WHERE RVT1.Value = 'User Administrator'
--ORDER BY R.PartyId;
--OPEN ListOrgnaizations;
--FETCH ListOrgnaizations
--INTO @OrganizationId,
--     @ROleId;
--WHILE @@FETCH_STATUS = 0
--BEGIN
--    SELECT @RightValueTypeId = RightValueTypeId
--    FROM Enterprise.RightValueType
--    WHERE Value = 'Ability to Migrate Users';

--	EXECUTE Enterprise.AssignRightToRole @OrganizationId = @OrganizationId,
--                                             @RightValueTypeId = @RightValueTypeId,
--                                             @ROleId = @ROleId,
--                                             @AssignRightToRole = 1;
--    FETCH ListOrgnaizations
--    INTO @OrganizationId,
--         @ROleId;
--END;
--CLOSE ListOrgnaizations;
--DEALLOCATE ListOrgnaizations;
--GO

DECLARE @ProductTypeId INT= 701, 
@ProductId INT, 
@LoginURI NVARCHAR(100), 
@SigningCertificateThumbprint NVARCHAR(50), 
@ParentProductTypeId INT, 
@ProductName NVARCHAR(100)= 'Portfolio Management';
DECLARE @ProductConfiguration AS PRODUCTCONFIGURATIONTYPE;

--Create root product type
IF NOT EXISTS
(
    SELECT TOP 1 1
    FROM Enterprise.ProductType
    WHERE Name = 'Asset and Investment Management'
)
    BEGIN
        EXEC [Enterprise].[CreateProductType]
             @ProductTypeId = 700,
             @ParentProductTypeId = NULL,
             @Name = 'Asset and Investment Management',
             @Description = 'Asset and Investment Management',
             @ProductTypeGUID = '9A480BD2-D0E8-4D43-AD26-9A2437B517AA';
    END;

SELECT @ParentProductTypeId = ProductTypeId
FROM Enterprise.ProductType
WHERE Name = 'Asset and Investment Management'
      AND ParentProductTypeId IS NULL;

IF NOT EXISTS
(
    SELECT TOP 1 1
    FROM enterprise.ProductType
    WHERE Name = 'Portfolio Management'
)
    BEGIN
        EXEC [Enterprise].[CreateProductType]
             @ProductTypeId = @ProductTypeId,
             @ParentProductTypeId = @ParentProductTypeId,
             @Name = @ProductName,
             @Description = 'Portfolio Management',
             @ProductTypeGUID = 'B7402864-0FBE-41CD-A232-C575E7C04758';
    END;

SET @ProductId = 44;

IF NOT EXISTS
(
    SELECT 1
    FROM Enterprise.Product
    WHERE Name = @ProductName
)
    BEGIN
        EXEC Enterprise.CreateProduct
             @ProductId = @ProductId,
             @ProductGUID = 'A9ECAD0F-9374-42AC-975C-CB46431F9D9F',
             @Name = @ProductName,
             @Description = @ProductName,
             @ProductTypeId = 701;
        UPDATE Enterprise.Product
          SET
              BooksProductCode = 'RPM'
        WHERE ProductId = @ProductId;
    END;



INSERT INTO @ProductConfiguration
(SettingName,
 SettingDescription,
 SettingValue
)
VALUES
('ClientId','','1')
,('ClassName','','portfoliomanagement')
,('ProductUrl','','/product/portfoliomanagement')
,('TitleId','','Portfolio Management')
,('TitleUniqueId','','8798E17D-249F-4C7D-89D5-72523437D71D')
,('IsNewTab','','1')
,('MetatagUniqueId','','PortfolioManagement')
,('IsResource','','0')
,('IsFavorite','','1')
,('LearnMore','','https://www.realpage.com/')
,('ApiEndPoint','','https://wmu-books.asseteye.net')
,('ProductStatus','Show if the external application was configured for the dashboard user.','8')
,('ShowInUserDetails','Should the product show in the New/Edit user pages','1')
,('ShowInRolesAndRights','Should the product show in the Role/Rights page','0')
,('ShowInAppSwitcher','Should the product show in the application switcher','1')
,('ShowInUserListFilter','Should the product show in the user list product pick list','1')
,('ProductAPIRequiresUser','Does the product require a user for api calls','0')
,('GetRoleEndpoint','Role End point for product API','/api/{0}/Roles?isGlobalRoles={1}')
,('GetUserEndpoint','GET User Endpoint for product API','/api/Users?companyId={0}&loginname={1}')
,('PostUserEndpoint','POST User Endpoint for product API','/api/users')
,('PutUserEndpoint','PUT User Endpoint for product API','/api/users')
,('DeleteUserEndpoint','DELETE User Endpoint for product API','/api/{companyId}/users?loginName={0}')
,('PutProfileEndpoint','PUT Profile Endpoint for product API','/api/users/profile')
,('GetPropertyEndpoint','GET Property Endpoint for API','/api/{0}/Properties')
,('TokenClientId','','apiuser')
,('TokenClientSecret','','apiU$3r')
,('GetLoginUrlEndpoint','GET Product Login URL Endpoint','/api/{0}/LoginURL')
,('ProductNotAvailableForRegularUserNoEmail','Product Attribute for Product Not Available for Regular User No Email.','0')
,('GetListUsersEndpoint','','/api/{0}/users?filter={1}&startRow={2}&resultsperpage={3}')
,('PatchMigrateUsersEndpoint','', '/api/{0}/migrate-users')



SET @ProductID = 44
SET @LoginURI = 'https://qa-books.asseteye.net/api/{{orgid}}/LoginURL'
SET @SigningCertificateThumbprint = 'EF26FEC08C554976572E8A9767DDA437AC452CF6'



EXEC Enterprise.ProductConfigurationSetup @ProductId, @LoginURI, @SigningCertificateThumbprint, @ProductConfiguration;

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
DECLARE @TRightShortName NVARCHAR(100);
DECLARE @TargetProductId INT;
DECLARE @VisibilityStatusID INT;


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
VALUES( 1, 'Manage Portfolio Management Product Access', 
'For  Portfolio Management, this right unlocks the ability to edit the Product assignment and Product Access Details for a user, assuming that the user can access the page because of Ability to view users.  All of the Product Rights are included in the System Role for the User Administrator, if the customer has the product.', 
'ManagePortfolioManagementProductAccess' );

SELECT @ProductId = ProductId
FROM Enterprise.Product
WHERE name = 'Unified Platform';

--select * from enterprise.product where name like '%lead%'

SELECT @TargetProductId = ProductId
FROM Enterprise.Product
WHERE Name = 'Portfolio Management';

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
	WHERE ObjectValue = 'Portfolio Management' AND 
		  ParentActionId IS NULL
)
BEGIN
	EXEC Enterprise.CreateAction @ProductID = @ProductId, @Action = N'Portfolio Management', @ActionTarget = N'Right', @ActionbValueTypeId = 1, @Description = '', @ActionID = @ActionID OUTPUT;
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
	WHERE ObjectValue = 'Portfolio Management' AND 
		  ParentActionID = @ParentActionId
)
BEGIN
	EXEC [Enterprise].[CreateAction] @ProductID = @ProductId, @Action = N'Portfolio Management', @ActionTarget = N'Right', @ActionbValueTypeId = 1, @Description = '', @ParentActionID = @ParentActionId, @ActionID = @ActionID OUTPUT;
	SELECT @ActionID AS N'@ActionID';
END;

SELECT DISTINCT 
	   IDENTITY(int, 1, 1) AS RowNumber, OrganizationPartyID, 0 AS PStatus
INTO #HoldPartyForUnifiedSettings
FROM Person.Persona AS P
	 INNER JOIN
	 Enterprise.Organization AS O
	 ON P.OrganizationPartyId = O.PartyId
--WHERE O.PartyId = 350;

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
		EXECUTE Enterprise.CreateRight @RoleId = -1, @RightName = 'Default_ManagePortfolioManagementProductAccess', @ShortName = 'ManagePortfolioManagementProductAccess', @RightCategoryId = @RightCategory, @PartyId = @PartyId, @ProductId = @ProductId, @Description = '',  @TargetProductId = @TargetProductId, @VisibilityStatusId = @VisibilityStatusId, @RightId = @RightId OUTPUT;
		EXECUTE Enterprise.CreateRight @RoleId = @RoleId, @RightName = @TRightName, @RightCategoryId = @RightCategory, @PartyId = @PartyId, @ProductId = @ProductId, @Shortname = @TRightShortName, @Description = @TRightDesc, @TargetProductId = @TargetProductId, @VisibilityStatusId = @VisibilityStatusId, @RightId = @RightId OUTPUT;


		SELECT @ActionID = ActionID
		FROM Enterprise.ACTION
		WHERE ObjectValue = 'Portfolio Management' AND 
			  ObjectType = 'Right' AND 
			  ParentActionId IS NULL;
		EXEC [Enterprise].[LinkActionToRights] @ActionID = @ActionID, @RightId = @RightId, @StatusId = @Status, @UserActionId = @UserActionId OUTPUT;

		SELECT @RightId = RightId
		FROM Enterprise.[Right] AS R
			 INNER JOIN
			 Enterprise.RightValueType AS RR
			 ON RR.RightValueTypeId = R.RightValueTypeId
		WHERE RR.Value = 'Default_ManagePortfolioManagementProductAccess';
		SELECT @ActionID = ActionID
		FROM Enterprise.ACTION
		WHERE ObjectValue = 'Portfolio Management'  AND 
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
WHERE value = 'Default_ManagePortfolioManagementProductAccess';

SELECT @RightValueTypeId = RightValueTypeId
FROM Enterprise.RightValueType
WHERE value IN( 'Manage Portfolio Management Product Access' );

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
IF NOT EXISTS(SELECT 1 FROM Enterprise.StatusType WHERE Name = 'Stop')
BEGIN
	SET IDENTITY_INSERT Enterprise.StatusTYpe ON
	INSERT INTO Enterprise.StatusType (StatusTypeId, Name)
	VALUES (20, 'Stop')
	SET IDENTITY_INSERT Enterprise.StatusTYpe OFF
END
IF NOT EXISTS(SELECT 1 FROM Enterprise.StatusType WHERE Name = 'Pause')
BEGIN
	SET IDENTITY_INSERT Enterprise.StatusTYpe ON
	INSERT INTO Enterprise.StatusType (StatusTypeId, Name)
	VALUES (21, 'Pause')
	SET IDENTITY_INSERT Enterprise.StatusTYpe OFF
END
IF NOT EXISTS(SELECT 1 FROM Enterprise.StatusType WHERE Name = 'Force Close')
BEGIN
	SET IDENTITY_INSERT Enterprise.StatusTYpe ON
	INSERT INTO Enterprise.StatusType (StatusTypeId, Name)
	VALUES (22, 'Force Close')
	SET IDENTITY_INSERT Enterprise.StatusTYpe OFF
END
GO
GO

DECLARE @ClientId INT;
DECLARE @ServerName SYSNAME

SELECT @ServerName = @@SERVERNAME
IF NOT EXISTS
(
    SELECT 1
    FROM [Auth].[Clients]
    WHERE ClientCode = 'ops-buyer'
          AND [ClientName] = 'Ops Mobile App'
)
    BEGIN
        INSERT INTO [Auth].[Clients]
		([ClientCode],
		 [ClientName],
		 [ClientUri],
		 [LogoUri],
		 [Flow],
		 [LogoutUri],
		 [IdentityTokenLifetime],
		 [AccessTokenLifetime],
		 [AuthorizationCodeLifetime],
		 [AbsoluteRefreshTokenLifetime],
		 [SlidingRefreshTokenLifetime],
		 [RefreshTokenUsage],
		 [RefreshTokenExpiration],
		 [AccessTokenType],
		 [UpdateAccessTokenOnRefresh],
		 [Enabled],
		 [LogoutSessionRequired],
		 [RequireSignOutPrompt],
		 [AllowAccessToAllScopes],
		 [AllowClientCredentialsOnly],
		 [RequireConsent],
		 [AllowRememberConsent],
		 [EnableLocalLogin],
		 [IncludeJwtId],
		 [AlwaysSendClientClaims],
		 [PrefixClientClaims],
		 [AllowAccessToAllGrantTypes]
		)
        VALUES
		(N'ops-buyer',
	 N'Ops Mobile App',
	 NULL,
	 NULL,
	 2,
	 NULL,
	 36000,
	 36000,
	 36000,
	 86400,
	 36000,
	 1,
	 37000,
	 0,
	 1,
	 1,
	 1,
	 0,
	 1,
	 0,
	 0,
	 1,
	 1,
	 1,
	 1,
	 1,
     1
	);
        SELECT @ClientId = SCOPE_IDENTITY();

	INSERT INTO Auth.ClientSecrets( ClientId, Value, Description, Expiration )
	VALUES( @ClientId, 'QwscCGLe7efLZ959e11yxJCZ93bTubOJqwq96NfP6U8=',  'ops-buyer', '2099-12-31 00:00:00.0000000 -06:00' );

	INSERT INTO [Auth].[ClientPostLogoutRedirectUris]
           ([ClientId]
           ,[Uri])
     VALUES
           (@ClientId, 
		   'com.realpage.opsbuyer.enterprise://')

	INSERT INTO [Auth].[ClientPostLogoutRedirectUris]
           ([ClientId]
           ,[Uri])
     VALUES
           (@ClientId, 
		   'com.realpage.opstechnology.opsbuyer.internal://')

    END;
IF @ClientId IS NOT NULL
   AND NOT EXISTS
		(
		SELECT 1
		FROM [Auth].[ClientScopes]
		WHERE ClientId = @ClientId
				AND [Scope] = 'openid'
		)
    BEGIN
        INSERT INTO [Auth].[ClientScopes]
		([ClientId],
		 [Scope]
		)
        VALUES
		(@ClientId,
		 N'openid'
		);
    END;

IF @ClientId IS NOT NULL
   AND NOT EXISTS
		(
		SELECT 1
		FROM [Auth].[ClientScopes]
		WHERE ClientId = @ClientId
				AND [Scope] = 'email'
		)
    BEGIN
        INSERT INTO [Auth].[ClientScopes]
		([ClientId],
		 [Scope]
		)
        VALUES
		(@ClientId,
		 N'email'
		);
    END;

	IF @ClientId IS NOT NULL
   AND NOT EXISTS
		(
		SELECT 1
		FROM [Auth].[ClientScopes]
		WHERE ClientId = @ClientId
				AND [Scope] = 'profie'
		)
    BEGIN
        INSERT INTO [Auth].[ClientScopes]
		([ClientId],
		 [Scope]
		)
        VALUES
		(@ClientId,
		 N'profie'
		);
    END;

	IF @ClientId IS NOT NULL
   AND NOT EXISTS
		(
		SELECT 1
		FROM [Auth].[ClientScopes]
		WHERE ClientId = @ClientId
				AND [Scope] = 'userinfoapi'
		)
    BEGIN
        INSERT INTO [Auth].[ClientScopes]
		([ClientId],
		 [Scope]
		)
        VALUES
		(@ClientId,
		 N'userinfoapi'
		);
    END;

	

IF @ServerName = 'RCDUSODBSQL001'
BEGIN
IF @ClientId IS NOT NULL
   AND NOT EXISTS
	(
		SELECT 1
		FROM [Auth].[ClientRedirectUris]
		WHERE ClientId = @ClientId
	)
    BEGIN
        INSERT INTO [Auth].[ClientRedirectUris]
		([ClientId],
		 Uri
		)
        VALUES
		(@ClientId,
		 N'http://rcdoptwwomk02.onesitedev.realpage.com/unifiedlogin/authorize.php'
		);
    END;
END

IF @ServerName = 'RCTUSODBSQL001'
BEGIN
IF @ClientId IS NOT NULL
   AND NOT EXISTS
	(
		SELECT 1
		FROM [Auth].[ClientRedirectUris]
		WHERE ClientId = @ClientId
	)
    BEGIN
        INSERT INTO [Auth].[ClientRedirectUris]
		([ClientId],
		 Uri
		)
        VALUES
		(@ClientId,
		 N'https://qamarket.opstechnology.com/unifiedlogin/authorize.php'
		);
    END;
END

IF @ServerName = 'RCQUSODBSQL001'
BEGIN
IF @ClientId IS NOT NULL
   AND NOT EXISTS
	(
		SELECT 1
		FROM [Auth].[ClientRedirectUris]
		WHERE ClientId = @ClientId
	)
    BEGIN
        INSERT INTO [Auth].[ClientRedirectUris]
		([ClientId],
		 Uri
		)
        VALUES
		(@ClientId,
		 N'https://preview.opstechnology.com/unifiedlogin/authorize.php'
		);
    END;
END


IF @ServerName = 'RCVGBKDBSQL001'
BEGIN
IF @ClientId IS NOT NULL
   AND NOT EXISTS
	(
		SELECT 1
		FROM [Auth].[ClientRedirectUris]
		WHERE ClientId = @ClientId
	)
    BEGIN
        INSERT INTO [Auth].[ClientRedirectUris]
		([ClientId],
		 Uri
		)
        VALUES
		(@ClientId,
		 N'https://demomarket.opstechnology.com/unifiedlogin/authorize.php'
		);
    END;
END


IF @ServerName IN ('RCPGBKDBSQL005B', 'RCPGBKDBSQL005A')
BEGIN
IF @ClientId IS NOT NULL
   AND NOT EXISTS
	(
		SELECT 1
		FROM [Auth].[ClientRedirectUris]
		WHERE ClientId = @ClientId
	)
    BEGIN
        INSERT INTO [Auth].[ClientRedirectUris]
		([ClientId],
		 Uri
		)
        VALUES
		(@ClientId,
		 N'https://market.opstechnology.com/unifiedlogin/authorize.php'
		);
    END;
END
GO
IF NOT EXISTS(SELECT 1 FROM Enterprise.StatusType WHERE Name = 'Force Close')
BEGIN
	SET IDENTITY_INSERT Enterprise.StatusTYpe ON
	INSERT INTO Enterprise.StatusType (StatusTypeId, Name)
	VALUES (22, 'Force Close')
	SET IDENTITY_INSERT Enterprise.StatusTYpe OFF
END
GO
GO
IF (SELECT COUNT(*) FROM [Batch].[BatchProcessConfigurationType]) = 0
BEGIN
	INSERT [Batch].[BatchProcessConfigurationType] ([BatchProcessConfigurationTypeId], [Name], [Description]) VALUES (1, N'ProcessApiEndpoint', N'API Endpoint to be invoked by batch processor')
END

IF (SELECT COUNT(*) FROM [Batch].[BatchProcessConfiguration]) = 0
BEGIN
	INSERT [Batch].[BatchProcessConfiguration] ([BatchProcessConfigurationId], [BatchProcessConfigurationTypeId], [Value]) VALUES (1, 1, N'https://my2dev.corp.realpage.com/api/batchprocessor')
END 
IF (SELECT COUNT(*) FROM [Batch].[BatchProcessType]) = 0
BEGIN
	INSERT [Batch].[BatchProcessType] ([BatchProcessTypeId], [BatchProcessConfigurationId], [Description], [Name]) VALUES (1, 1, N'Batch to create-update user', N'CreateUpdateProductUser')
	INSERT [Batch].[BatchProcessType] ([BatchProcessTypeId], [BatchProcessConfigurationId], [Description], [Name]) VALUES (2, 1, N'Profile Update', N'ProfileUpdate')
	INSERT [Batch].[BatchProcessType] ([BatchProcessTypeId], [BatchProcessConfigurationId], [Description], [Name]) VALUES (3, 1, N'Deactivate Product User', N'DeactivateProductUser')
	INSERT [Batch].[BatchProcessType] ([BatchProcessTypeId], [BatchProcessConfigurationId], [Description], [Name]) VALUES (4, 1, N'Activate Product User', N'ActivateProductUser')
	INSERT [Batch].[BatchProcessType] ([BatchProcessTypeId], [BatchProcessConfigurationId], [Description], [Name]) VALUES (5, 1, N'User Type changed from Regular To Admin', N'UserTypeRegularToAdmin')
	INSERT [Batch].[BatchProcessType] ([BatchProcessTypeId], [BatchProcessConfigurationId], [Description], [Name]) VALUES (6, 1, N'User Type changed from Admin To Regular', N'UserTypeAdminToRegular')
	INSERT [Batch].[BatchProcessType] ([BatchProcessTypeId], [BatchProcessConfigurationId], [Description], [Name]) VALUES (7, 1, N'Un-assign user from product', N'UnassignUser')
END

IF NOT EXISTS (SELECT 1 FROM ENterprise.GlobalControl where COntrolName = 'IsNewBatchService')
BEGIN
	INSERT INTO Enterprise.GlobalControl(GlobalControlId, ControlName, ControlValue, Description, CreateDateTime)
	VALUES (1, 'IsNewBatchService', 1, '0-Old 1 New', GETUTCDATE())
END
GO

update [Enterprise].[CommunicationEmailTemplate] set Body = 
  '<!DOCTYPE html>
<html dir="ltr" lang="en">
<body>
    <table border="0" cellspacing="0" cellpadding="0" width="100%" style="table-layout:fixed; font-size:16px;">
        <tbody>
            <tr>
                <td>
                    <center>
                        <table border="0" cellspacing="0" cellpadding="0" width="600" style="margin:0 auto; max-width:535px; width:inherit;">
                            <tbody>
                                <tr>
                                    <td align="left">
                                        <table border="0" cellspacing="0" cellpadding="0" width="100%">
                                            <tbody>
                                                <tr>
                                                    <td width="100%">
                                                        <table border="0" cellspacing="0" cellpadding="0" width="100%">
                                                            <tbody>
                                                                <tr>
                                                                    <td style="padding:18px 0 0 0;">
                                                            <tbody>
                                                                <tr>
                                                                    <td style="padding:0 10px" align="center">
                                                                        <div style="display:none;color:#fff;line-height:1px;max-height:0px;max-width:0px;opacity:0;overflow:hidden;">                                                                            
                                                                            <span>Hi {FIRST NAME}, 

                                                                            Your RealPage Unified Platform Administrator account is ready! Your email is your user name. Please click the button below to set your password. For your security, you have 7 days to log in before the link below expires.</span>
                                                                        </div>
                                                                        <a href="https://www.realpage.com" style="text-decoration:none;">
                                                                            <img src="{IMAGES}/RealPage-Logo.png" alt="RealPage" width="270" height="80" style="margin: 0; border: 0; padding: 0; display: block;" />
                                                                        </a>
                                                                    </td>
                                                                </tr>
                                                            </tbody>
                                                    </td>
                                                </tr>
                                            </tbody>
                                        </table>
                                        <table border="0" cellspacing="0" cellpadding="0" width="100%">
                                            <tbody>
                                                <tr>
                                                    <td width="100%" style="padding:24px 24px 32px 24px; border-style:none;">
                                                        <table border="0" cellspacing="0" cellpadding="0" width="100%">
                                                            <tbody>
                                                                <tr>
                                                                    <td width="100%" style="padding:18px 0 0 0">
                                                                        <table border="0" cellspacing="0" cellpadding="0" width="100%">
                                                                            <tbody>
                                                                                <tr>
                                                                                    <td style="padding:0 10px; color:#757575; font-family: ''Roboto'',''Helvetica Neue'', Helvetica, Arial, sans-serif;">
                                                                                        <span>Hello {FIRST NAME},</span>
                                                                                    </td>
                                                                                </tr>
                                                                            </tbody>
                                                                        </table>
                                                                    </td>
                                                                </tr>
                                                                <tr>
                                                                    <td width="100%" style="padding:18px 0 0 0">
                                                                        <table border="0" cellspacing="0" cellpadding="0" width="100%">
                                                                            <tbody>
                                                                                <tr>
                                                                                    <td style="padding:0 10px; line-height:27px; color:#757575; font-family: ''Roboto'',''Helvetica Neue'', Helvetica, Arial, sans-serif;">
                                                                                        <span>
                                                                                             Your RealPage Unified Platform Administrator account is ready! Your email is your user name. Please click the button below to set your password. For your security, you have 7 days to log in before the link below expires.
                                                                                        </span>
                                                                                    </td>
                                                                                </tr>
                                                                            </tbody>
                                                                        </table>
                                                                    </td>
                                                                </tr>                                                                
                                                                <tr>
                                                                    <td align="center" style="padding:18px 0 0 0">
                                                                        <table border="0" cellpadding="0" cellspacing="0" align="center">
                                                                            <tbody>
                                                                                <tr>
                                                                                    <td>
                                                                                        <table width="100%" border="0" cellspacing="0" cellpadding="0">
                                                                                            <tr>
                                                                                                <td align="center" style="-webkit-border-radius: 25px; -moz-border-radius: 25px; border-radius: 25px;" bgcolor="#42a5f6">
                                                                                                    <a href="{LINK}" style="font-family: ''Roboto'',''Helvetica Neue'', Helvetica, Arial, sans-serif; color: #ffffff; text-decoration: none; -webkit-border-radius: 25px; -moz-border-radius: 25px; border-radius: 25px; padding: 9px 28px; border: 1px solid #42a5f6; display: inline-block;">Set Your Password Now</a>
                                                                                                </td>
                                                                                            </tr>
                                                                                        </table>
                                                                                    </td>
                                                                                </tr>
                                                                            </tbody>
                                                                        </table>
                                                                    </td>
                                                                </tr>                                                                
                                                                <tr>
                                                                    <td align="center" style="padding:18px 0 0 0">
                                                                        <table border="0" cellpadding="0" cellspacing="0" align="center">
                                                                            <tbody>
                                                                                <tr>
                                                                                    <td>
                                                                                        <table width="100%" border="0" cellspacing="0" cellpadding="0">
                                                                                            <tr>
                                                                                                <td style="padding:0 10px; line-height:27px; color:#757575; font-family: ''Roboto'',''Helvetica Neue'', Helvetica, Arial, sans-serif;">
                                                                                                    <span>
                                                                                                        If you have trouble accessing your profile, please contact your internal help desk or <a href="https://www.realpage.com/support/" style="color:#42A5F5; font-family: ''Roboto'',''Helvetica Neue'', Helvetica, Arial, sans-serif;">RealPage Support</a> for assistance. For additional information, please read our <a href="{UNIFIED}/RealPage Unified Platform Quick Steps.pdf" style="font-family: ''Roboto'',''Helvetica Neue'', Helvetica, Arial, sans-serif; color:#42A5F5;">Quick Start Guide</a>
                                                                                                    </span>
                                                                                                </td>                   
                                                                                            </tr>          
                                                                                        </table>
                                                                                    </td>
                                                                                </tr>
                                                                            </tbody>
                                                                        </table>
                                                                    </td>
                                                                </tr>
                                                                <tr>
                                                                    <td width="100%" style="padding:18px 0 0 0">
                                                                        <table border="0" cellspacing="0" cellpadding="0" width="100%">
                                                                            <tbody>
                                                                                <tr>
                                                                                    <td style="padding:0 10px; line-height:27px; color:#757575; font-family: ''Roboto'',''Helvetica Neue'', Helvetica, Arial, sans-serif; font-size:9px;">

                                                                                        This email and any files transmitted with it are confidential and intended solely for the use of the individual or entity to whom they are addressed.  If you’ve received this email in error, please notify <a href="https://www.realpage.com/support/" style="color:#42A5F5; font-family: ''Roboto'',''Helvetica Neue'', Helvetica, Arial, sans-serif;">RealPage Support</a> by forwarding this email to <a href="mailto:support@realpage.com?subject=support">support@realpage.com</a>.  This message contains confidential information and is intended only for the individual named.
                                                                                    </td>
                                                                                </tr>
                                                                            </tbody>
                                                                        </table>
                                                                    </td>
                                                                </tr>
                                                            </tbody>
                                                        </table>
                                                    </td>
                                                </tr>
                                            </tbody>
                                        </table>
                                    </td>
                                </tr>
                            </tbody>
                        </table>
                </td>
            </tr>
            <tr>
                <td align="left">
                    <table border="0" cellspacing="0" cellpadding="0" width="100%" style="padding:0 24px;">
                        <tbody>
                            <tr>
                                <td align="center" width="100%">
                                    <table border="0" cellspacing="0" cellpadding="0" width="100%">
                                        <tbody>
                                            <tr>
                                                <td align="center" width="100%" style="border-top:1px solid #757575; padding:16px 0;font-size:11px;">
                                                    <a href="https://www.realpage.com/privacy-policy" style="color:#757575;text-decoration:none; font-family: ''Roboto'',''Helvetica Neue'', Helvetica, Arial, sans-serif;"><span>Privacy Policy</span></a>
                                                    <span style="color:#757575; font-family: ''Roboto'',''Helvetica Neue'', Helvetica, Arial, sans-serif;">|</span>
                                                    <a href="https://www.realpage.com/" style="color:#757575;text-decoration:none; font-family: ''Roboto'',''Helvetica Neue'', Helvetica, Arial, sans-serif;"><span>Contact Us</span></a>
                                                    <span style="color:#757575; font-family: ''Roboto'',''Helvetica Neue'', Helvetica, Arial, sans-serif;">|</span>
                                                    <span style="color:#757575; font-family: ''Roboto'',''Helvetica Neue'', Helvetica, Arial, sans-serif;">&copy; 2019 RealPage, Inc.</span>
                                                </td>
                                            </tr>                                           
                                        </tbody>
                                    </table>
                                </td>
                            </tr>
                        </tbody>
                    </table>
                </td>
            </tr>
        </tbody>
    </table>
    </center>
    </td>
    </tr>
    </tbody>
    </table>
</body>
</html>'
where CommunicationEmailTemplateID = 1
GO

update [Enterprise].[CommunicationEmailTemplate] set Body = 
  '<!DOCTYPE html>
<html dir="ltr" lang="en">
<body>
    <table border="0" cellspacing="0" cellpadding="0" width="100%" style="table-layout:fixed; font-size:16px;">
        <tbody>
            <tr>
                <td>
                    <center>
                        <table border="0" cellspacing="0" cellpadding="0" width="600" style="margin:0 auto; max-width:535px; width:inherit;">
                            <tbody>
                                <tr>
                                    <td align="left">
                                        <table border="0" cellspacing="0" cellpadding="0" width="100%">
                                            <tbody>
                                                <tr>
                                                    <td width="100%">
                                                        <table border="0" cellspacing="0" cellpadding="0" width="100%">
                                                            <tbody>
                                                                <tr>
                                                                    <td style="padding:18px 0 0 0;">
                                                            <tbody>
                                                                <tr>
                                                                    <td style="padding:0 10px" align="center">
                                                                        <div style="display:none;color:#fff;line-height:1px;max-height:0px;max-width:0px;opacity:0;overflow:hidden;">
                                                                            <span>Hi {FIRST NAME}, 
                                                                            Your RealPage Unified Platform account is ready! Your email is your user name. Please click the button below to set your password. For your security, you have 7 days to log in before the link below expires.</span>
                                                                        </div>
                                                                        <a href="https://www.realpage.com" style="text-decoration:none;">
                                                                            <img src="{IMAGES}/RealPage-Logo.png" alt="RealPage" width="270" height="80" style="margin: 0; border: 0; padding: 0; display: block;" />
                                                                        </a>
                                                                    </td>
                                                                </tr>
                                                            </tbody>
                                                    </td>
                                                </tr>
                                            </tbody>
                                        </table>
                                        <table border="0" cellspacing="0" cellpadding="0" width="100%">
                                            <tbody>
                                                <tr>
                                                    <td width="100%" style="padding:24px 24px 32px 24px; border-style:none;">
                                                        <table border="0" cellspacing="0" cellpadding="0" width="100%">
                                                            <tbody>
                                                                <tr>
                                                                    <td width="100%" style="padding:18px 0 0 0">
                                                                        <table border="0" cellspacing="0" cellpadding="0" width="100%">
                                                                            <tbody>
                                                                                <tr>
                                                                                    <td style="padding:0 10px; color:#757575; font-family: ''Roboto'',''Helvetica Neue'', Helvetica, Arial, sans-serif;">
                                                                                        <span>Hi {FIRST NAME},</span>
                                                                                    </td>
                                                                                </tr>
                                                                            </tbody>
                                                                        </table>
                                                                    </td>
                                                                </tr>
                                                                <tr>
                                                                    <td width="100%" style="padding:18px 0 0 0">
                                                                        <table border="0" cellspacing="0" cellpadding="0" width="100%">
                                                                            <tbody>
                                                                                <tr>
                                                                                    <td style="padding:0 10px; line-height:27px; color:#757575; font-family: ''Roboto'',''Helvetica Neue'', Helvetica, Arial, sans-serif;">
                                                                                        <span>
                                                                                             Your RealPage Unified Platform account is ready! Your email is your user name. Please click the button below to set your password. For your security, you have 7 days to log in before the link below expires.
                                                                                        </span>
                                                                                    </td>
                                                                                </tr>
                                                                            </tbody>
                                                                        </table>
                                                                    </td>
                                                                </tr>                                                                                
                                                                <tr>
                                                                    <td align="center" style="padding:18px 0 0 0">
                                                                        <table border="0" cellpadding="0" cellspacing="0" align="center">
                                                                            <tbody>
                                                                                <tr>
                                                                                    <td>
                                                                                        <table width="100%" border="0" cellspacing="0" cellpadding="0">
                                                                                            <tr>
                                                                                                <td align="center" style="-webkit-border-radius: 25px; -moz-border-radius: 25px; border-radius: 25px;" bgcolor="#42a5f6">
                                                                                                    <a href="{LINK}" style="font-family: ''Roboto'',''Helvetica Neue'', Helvetica, Arial, sans-serif; color: #ffffff; text-decoration: none; -webkit-border-radius: 25px; -moz-border-radius: 25px; border-radius: 25px; padding: 9px 28px; border: 1px solid #42a5f6; display: inline-block;">Set Your Password Now</a>
                                                                                                </td>
                                                                                            </tr>
                                                                                        </table>
                                                                                    </td>
                                                                                </tr>
                                                                            </tbody>
                                                                        </table>
                                                                    </td>
                                                                </tr>                                                               
                                                                <tr>
                                                                    <td align="center" style="padding:18px 0 0 0">
                                                                        <table border="0" cellpadding="0" cellspacing="0" align="center">        
                                                                             <tbody>
                                                                                <tr>
                                                                                    <td>
                                                                                        <table width="100%" border="0" cellspacing="0" cellpadding="0">
                                                                                            <tr>
                                                                                                <td style="padding:0 10px; line-height:27px; color:#757575; font-family: ''Roboto'',''Helvetica Neue'', Helvetica, Arial, sans-serif;">
                                                                                                    <span>
                                                                                                        If you have trouble accessing your profile, please contact your internal help desk or <a href="https://www.realpage.com/support/" style="color:#42A5F5; font-family: ''Roboto'',''Helvetica Neue'', Helvetica, Arial, sans-serif;">RealPage Support</a> for assistance. For additional information, please read our <a href="{UNIFIED}/RealPage Unified Platform Quick Steps.pdf" style="font-family: ''Roboto'',''Helvetica Neue'', Helvetica, Arial, sans-serif; color:#42A5F5;">Quick Start Guide</a>
                                                                                                    </span>
                                                                                                </td>                   
                                                                                            </tr>          
                                                                                        </table>
                                                                                    </td>
                                                                                </tr>
                                                                            </tbody>
                                                                        </table>
                                                                    </td>
                                                                </tr>                                                              
                                                                <tr>
                                                                    <td width="100%" style="padding:18px 0 0 0">
                                                                        <table border="0" cellspacing="0" cellpadding="0" width="100%">
                                                                            <tbody>
                                                                                <tr>
                                                                                    <td style="padding:0 10px; line-height:27px; color:#757575; font-family: ''Roboto'',''Helvetica Neue'', Helvetica, Arial, sans-serif; font-size:9px;">

                                                                                        This email and any files transmitted with it are confidential and intended solely for the use of the individual or entity to whom they are addressed.  If you’ve received this email in error, please notify <a href="https://www.realpage.com/support/" style="color:#42A5F5; font-family: ''Roboto'',''Helvetica Neue'', Helvetica, Arial, sans-serif;">RealPage Support</a> by forwarding this email to <a href="mailto:support@realpage.com?subject=support">support@realpage.com</a>.  This message contains confidential information and is intended only for the individual named.
                                                                                    </td>
                                                                                </tr>
                                                                            </tbody>
                                                                        </table>
                                                                    </td>
                                                                </tr>
                                                            </tbody>
                                                        </table>
                                                    </td>
                                                </tr>
                                            </tbody>
                                        </table>
                                    </td>
                                </tr>
                            </tbody>
                        </table>
                </td>
            </tr>
            <tr>
                <td align="left">
                    <table border="0" cellspacing="0" cellpadding="0" width="100%" style="padding:0 24px;">
                        <tbody>
                            <tr>
                                <td align="center" width="100%">
                                    <table border="0" cellspacing="0" cellpadding="0" width="100%">
                                        <tbody>
                                            <tr>
                                                <td align="center" width="100%" style="border-top:1px solid #757575; padding:16px 0;font-size:11px;">
                                                    <a href="https://www.realpage.com/privacy-policy" style="color:#757575;text-decoration:none; font-family: ''Roboto'',''Helvetica Neue'', Helvetica, Arial, sans-serif;"><span>Privacy Policy</span></a>
                                                    <span style="color:#757575; font-family: ''Roboto'',''Helvetica Neue'', Helvetica, Arial, sans-serif;">|</span>
                                                    <a href="https://www.realpage.com/" style="color:#757575;text-decoration:none; font-family: ''Roboto'',''Helvetica Neue'', Helvetica, Arial, sans-serif;"><span>Contact Us</span></a>
                                                    <span style="color:#757575; font-family: ''Roboto'',''Helvetica Neue'', Helvetica, Arial, sans-serif;">|</span>
                                                    <span style="color:#757575; font-family: ''Roboto'',''Helvetica Neue'', Helvetica, Arial, sans-serif;">&copy; 2019 RealPage, Inc.</span>
                                                </td>
                                            </tr>                                          
                                        </tbody>
                                    </table>
                                </td>
                            </tr>
                        </tbody>
                    </table>
                </td>
            </tr>
        </tbody>
    </table>
    </center>
    </td>
    </tr>
    </tbody>
    </table>
</body>
</html>'
where CommunicationEmailTemplateID = 2
GO
DECLARE @RvtId INT
DECLARE @DRvtId INT

SELECT @RvtID = RightValueTYpeId FROM Enterprise.RIghtValueType WHERE Value = 'Ability to manage roles and rights'
SELECT @DRvtId = RightValueTYpeId FROM Enterprise.RIghtValueType WHERE Value = 'Default_RoleRightRight'

DELETE FROM Enterprise.RightDependency WHERE RightvalueTypeId = @RvtID
	AND DependentRightValueTypeId = @DRvtId
GO
UPDATE Ident.UserLogin 
	SET ThruDate = NULL
WHERE ThruDate = '9999-12-31 00:00:00.000'
GO

DECLARE @ClientId INT;
DECLARE @ServerName SYSNAME

SELECT @ServerName = @@SERVERNAME
IF NOT EXISTS
(
    SELECT 1
    FROM [Auth].[Clients]
    WHERE ClientCode = 'ops-buyer'
          AND [ClientName] = 'Ops Mobile App'
)
    BEGIN
        INSERT INTO [Auth].[Clients]
		([ClientCode],
		 [ClientName],
		 [ClientUri],
		 [LogoUri],
		 [Flow],
		 [LogoutUri],
		 [IdentityTokenLifetime],
		 [AccessTokenLifetime],
		 [AuthorizationCodeLifetime],
		 [AbsoluteRefreshTokenLifetime],
		 [SlidingRefreshTokenLifetime],
		 [RefreshTokenUsage],
		 [RefreshTokenExpiration],
		 [AccessTokenType],
		 [UpdateAccessTokenOnRefresh],
		 [Enabled],
		 [LogoutSessionRequired],
		 [RequireSignOutPrompt],
		 [AllowAccessToAllScopes],
		 [AllowClientCredentialsOnly],
		 [RequireConsent],
		 [AllowRememberConsent],
		 [EnableLocalLogin],
		 [IncludeJwtId],
		 [AlwaysSendClientClaims],
		 [PrefixClientClaims],
		 [AllowAccessToAllGrantTypes]
		)
        VALUES
		(N'ops-buyer',
	 N'Ops Mobile App',
	 NULL,
	 NULL,
	 2,
	 NULL,
	 36000,
	 36000,
	 36000,
	 86400,
	 36000,
	 1,
	 37000,
	 0,
	 1,
	 1,
	 1,
	 1,
	 1,
	 0,
	 0,
	 1,
	 1,
	 1,
	 1,
	 1,
     1
	);
        SELECT @ClientId = SCOPE_IDENTITY();

	INSERT INTO Auth.ClientSecrets( ClientId, Value, Description, Expiration )
	VALUES( @ClientId, 'QwscCGLe7efLZ959e11yxJCZ93bTubOJqwq96NfP6U8=',  'ops-buyer', '2099-12-31 00:00:00.0000000 -06:00' );

	INSERT INTO [Auth].[ClientPostLogoutRedirectUris]
           ([ClientId]
           ,[Uri])
     VALUES
           (@ClientId, 
		   'com.realpage.opsbuyer.enterprise://')

	INSERT INTO [Auth].[ClientPostLogoutRedirectUris]
           ([ClientId]
           ,[Uri])
     VALUES
           (@ClientId, 
		   'com.realpage.opstechnology.opsbuyer.internal://')

    END;
IF @ClientId IS NOT NULL
   AND NOT EXISTS
		(
		SELECT 1
		FROM [Auth].[ClientScopes]
		WHERE ClientId = @ClientId
				AND [Scope] = 'openid'
		)
    BEGIN
        INSERT INTO [Auth].[ClientScopes]
		([ClientId],
		 [Scope]
		)
        VALUES
		(@ClientId,
		 N'openid'
		);
    END;

IF @ClientId IS NOT NULL
   AND NOT EXISTS
		(
		SELECT 1
		FROM [Auth].[ClientScopes]
		WHERE ClientId = @ClientId
				AND [Scope] = 'email'
		)
    BEGIN
        INSERT INTO [Auth].[ClientScopes]
		([ClientId],
		 [Scope]
		)
        VALUES
		(@ClientId,
		 N'email'
		);
    END;

	IF @ClientId IS NOT NULL
   AND NOT EXISTS
		(
		SELECT 1
		FROM [Auth].[ClientScopes]
		WHERE ClientId = @ClientId
				AND [Scope] = 'profie'
		)
    BEGIN
        INSERT INTO [Auth].[ClientScopes]
		([ClientId],
		 [Scope]
		)
        VALUES
		(@ClientId,
		 N'profie'
		);
    END;

	IF @ClientId IS NOT NULL
   AND NOT EXISTS
		(
		SELECT 1
		FROM [Auth].[ClientScopes]
		WHERE ClientId = @ClientId
				AND [Scope] = 'userinfoapi'
		)
    BEGIN
        INSERT INTO [Auth].[ClientScopes]
		([ClientId],
		 [Scope]
		)
        VALUES
		(@ClientId,
		 N'userinfoapi'
		);
    END;

	

IF @ServerName = 'RCDUSODBSQL001'
BEGIN
IF @ClientId IS NOT NULL
   AND NOT EXISTS
	(
		SELECT 1
		FROM [Auth].[ClientRedirectUris]
		WHERE ClientId = @ClientId
	)
    BEGIN
        INSERT INTO [Auth].[ClientRedirectUris]
		([ClientId],
		 Uri
		)
        VALUES
		(@ClientId,
		 N'http://rcdoptwwomk02.onesitedev.realpage.com/unifiedlogin/authorize.php'
		);
    END;
END

IF @ServerName = 'RCTUSODBSQL001'
BEGIN
IF @ClientId IS NOT NULL
   AND NOT EXISTS
	(
		SELECT 1
		FROM [Auth].[ClientRedirectUris]
		WHERE ClientId = @ClientId
	)
    BEGIN
        INSERT INTO [Auth].[ClientRedirectUris]
		([ClientId],
		 Uri
		)
        VALUES
		(@ClientId,
		 N'https://qamarket.opstechnology.com/unifiedlogin/authorize.php'
		);
    END;
END

IF @ServerName = 'RCQUSODBSQL001'
BEGIN
IF @ClientId IS NOT NULL
   AND NOT EXISTS
	(
		SELECT 1
		FROM [Auth].[ClientRedirectUris]
		WHERE ClientId = @ClientId
	)
    BEGIN
        INSERT INTO [Auth].[ClientRedirectUris]
		([ClientId],
		 Uri
		)
        VALUES
		(@ClientId,
		 N'https://preview.opstechnology.com/unifiedlogin/authorize.php'
		);
    END;
END


IF @ServerName = 'RCVGBKDBSQL001'
BEGIN
IF @ClientId IS NOT NULL
   AND NOT EXISTS
	(
		SELECT 1
		FROM [Auth].[ClientRedirectUris]
		WHERE ClientId = @ClientId
	)
    BEGIN
        INSERT INTO [Auth].[ClientRedirectUris]
		([ClientId],
		 Uri
		)
        VALUES
		(@ClientId,
		 N'https://demomarket.opstechnology.com/unifiedlogin/authorize.php'
		);
    END;
END


IF @ServerName IN ('RCPGBKDBSQL005B', 'RCPGBKDBSQL005A')
BEGIN
IF @ClientId IS NOT NULL
   AND NOT EXISTS
	(
		SELECT 1
		FROM [Auth].[ClientRedirectUris]
		WHERE ClientId = @ClientId
	)
    BEGIN
        INSERT INTO [Auth].[ClientRedirectUris]
		([ClientId],
		 Uri
		)
        VALUES
		(@ClientId,
		 N'https://market.opstechnology.com/unifiedlogin/authorize.php'
		);
    END;
END
GO
--Populate ActivityType Table
IF (SELECT COUNT(*) FROM [Ident].[ActivityType]) = 0
BEGIN
	INSERT [Ident].[ActivityType] ([ActivityTypeId], [ActivityCode], [Description]) 
	VALUES (1, N'Login', N'Login Activity')
	, (2, N'ForgotPassword', N'Forgot Password')
	, (3, N'ForcedLock', N'User has forced lock')
	, (4, N'BruteForce', N'Brute Force Activity')
	, (5, N'QuestionAttempts', N'Question Attempts')
	, (6, N'VerifyAnswers', N'Verify Security Question Answers are correct')
	, (7, N'LoginSuccess', N'Succesful log in activity')
	, (8, N'NewUserRegistration', N'New User Registration, Expires in 7 days')
	, (9, N'NewUserRegistrationVerification', N'New User Registration Verification, Expires in 7 days')
	, (10, N'UnlockUser', N'Unlock User')
END


--Populate ActivityConfiguration Table
--DECLARE @orgid int;
--DECLARE OrgList CURSOR
--FOR SELECT O.partyid
--	FROM enterprise.organization O
--		LEFT OUTER JOIN Ident.ActivityConfiguration A
--	ON A.PartyId = O.PartyId
--	WHERE A.PartyId IS NULL

--OPEN OrgList;

--FETCH OrgList INTO @orgid;

--WHILE @@fetch_status = 0
--	BEGIN
--		INSERT INTO Ident.ActivityConfiguration( PartyId, ActivityTypeId, MaxActivityAttemptCount, ActivityTokenExpirationMinutes )
--		VALUES( @OrgId, 1, 4, 30 ), ( @OrgId, 2, 3, 30 ), ( @OrgId, 3, 3, 30 ), ( @OrgId, 4, 3, 30 ), ( @OrgId, 5, 3, 30 ), ( @OrgId, 6, 2, 5 ), ( @OrgId, 7, 0, 30 ), ( @OrgId, 8, 5, 10080 ), ( @OrgId, 9, 5, 10080 ), ( @OrgId, 10, 0, 0 );
--		FETCH OrgList INTO @orgid;
--	END;
--CLOSE OrgList;
--DEALLOCATE OrgList;
--GO
----Load data in other table
--DECLARE @StartCnt INT;
--DECLARE @EndCnt INT;
--SELECT @StartCnt = COUNT(*)
--FROM [Ident].[ActivityAttempts_Old];
--SELECT @EndCnt = COUNT(*)
--FROM [Ident].[ActivityAttempts];
--IF(@Endcnt = 0)
--	AND EXISTS (SELECT 1 FROM sys.tables Where name = 'ActivityAttempts_old' and Schema_Name(schema_id) = 'Ident')
--    BEGIN
--        SET IDENTITY_INSERT [Ident].[ActivityAttempts] ON;
--        INSERT INTO [Ident].[ActivityAttempts]
--([ActivityAttemptsId],
-- [ActivityConfigurationId],
-- [EnterpriseUserName],
-- [AuthenticationServiceId],
-- [AttemptCount],
-- [IpAddress],
-- [BrowserType],
-- [BrowserName],
-- [Version],
-- [Platform],
-- [IsMobile],
-- [DeviceType],
-- [LastAttemptDateTime],
-- [Timezone]
--)
--               SELECT [ActivityAttemptsId],
--                      [ActivityId],
--                      [EnterpriseUserName],
--                      [AuthenticationServiceId],
--                      [AttemptCount],
--                      [IpAddress],
--                      [BrowserType],
--                      [BrowserName],
--                      [Version],
--                      [Platform],
--                      [IsMobile],
--                      [DeviceType],
--                      [LastAttemptDateTime],
--                      [Timezone]
--               FROM [Ident].[ActivityAttempts_Old];
--        SET IDENTITY_INSERT [Ident].[ActivityAttempts] OFF;
--    END;
--GO
--DECLARE @StartCnt INT;
--DECLARE @EndCnt INT;
--SELECT @StartCnt = COUNT(*)
--FROM [Ident].[ActivityToken_Old];
--SELECT @EndCnt = COUNT(*)
--FROM [Ident].[ActivityToken];
--IF(@Endcnt = 0)
--AND EXISTS (SELECT 1 FROM sys.tables Where name = 'ActivityToken_Old' and Schema_Name(schema_id) = 'Ident')
--    BEGIN
--        SET IDENTITY_INSERT [Ident].[ActivityToken] ON;
--        INSERT INTO [Ident].[ActivityToken]
--([ActivityTokenId],
-- [ActivityConfigurationId],
-- [RealPageId],
-- [ActivityToken],
-- [IsActive],
-- [CreateDateTime],
-- [ExpireDateTime]
--)
--               SELECT [ActivityTokenId],
--                      [ActivityId],
--                      [RealPageId],
--                      [ActivityToken],
--                      [IsActive],
--                      [CreateDateTime],
--                      [ExpireDateTime]
--               FROM [Ident].[ActivityToken_Old];
--        SET IDENTITY_INSERT [Ident].[ActivityToken] OFF;
--    END;
--GO
--DECLARE @StartCnt INT;
--DECLARE @EndCnt INT;
--SELECT @StartCnt = COUNT(*)
--FROM [Ident].[PasswordHistory_Old];
--SELECT @EndCnt = COUNT(*)
--FROM [Ident].[PasswordHistory];
--IF(@Endcnt = 0)
--AND EXISTS (SELECT 1 FROM sys.tables Where name = 'PasswordHistory_Old' and Schema_Name(schema_id) = 'Ident')

--    BEGIN
--        SET IDENTITY_INSERT [Ident].[PasswordHistory] ON;
--        INSERT INTO [Ident].[PasswordHistory]
--([PasswordHistoryId],
-- [UserId],
-- [ActivityConfigurationId],
-- [ChangedPasswordHash],
-- [ChangedPasswordSalt],
-- [ChangedPasswordDateTime]
--)
--               SELECT [PasswordHistoryId],
--                      [UserId],
--                      [ActivityId],
--                      [ChangedPasswordHash],
--                      [ChangedPasswordSalt],
--                      [ChangedPasswordDateTime]
--               FROM [Ident].[PasswordHistory_old];
--        SET IDENTITY_INSERT [Ident].[PasswordHistory] OFF;
--    END;
--GO
----Update exiisring mappings

--CREATE TABLE #temp(
--	[ActivityId] [int] NOT NULL,
--	[ActivityCode] [varchar](50) NOT NULL,
--	[Description] [varchar](100) NULL,
--	[MaxActivityAttemptCount] [tinyint] NOT NULL,
--	[ActivityTokenExpirationMinutes] [int] NOT NULL
--)


--GO
--INSERT #temp ([ActivityId], [ActivityCode], [Description], [MaxActivityAttemptCount], [ActivityTokenExpirationMinutes]) 
--VALUES (1, N'Login', N'Login Activity', 4, 30)
--,(2, N'ForgotPassword', N'Forgot Password', 3, 30)
--,(3, N'ForcedLock', N'User has forced lock', 3, 30)
--,(4, N'BruteForce', N'Brute Force Activity', 3, 30)
--,(5, N'QuestionAttempts', N'Question Attempts', 3, 30)
--,(6, N'VerifyAnswers', N'Verify Security Question Answers are correct', 2, 5)
--,(7, N'LoginSuccess', N'Succesful log in activity', 0, 30)
--,(8, N'NewUserRegistration', N'New User Registration, Expires in 7 days', 5, 10080)
--,(9, N'NewUserRegistrationVerification', N'New User Registration Verification, Expires in 7 days', 5, 10080)
--,(10, N'UnlockUser', N'Unlock User', 0, 0)


----Update ActityAttempts

--;WITH CTE AS
--	(SELECT T.Activityid 'TAId' , AT.ActivityCode, AC.ActivityConfigurationid, AC.PartyId FROM #Temp T
--		INNER JOIN Ident.ActivityType AT
--			ON T.ActivityCOde = AT.ActivityCOde	
--		INNER JOIN Ident.ActivityCOnfiguration AC
--			On AC.ActivityTypeId = AT.ActivityTypeId
--	)
--UPDATE AA
--SET ActivityCOnfigurationId = C.ActivityCOnfigurationId
--	FROM Ident.ActivityAttempts AA
--	INNER JOIN Ident.UserLogin UL
--		ON UL.LoginName = AA.EnterpriseUserName
--	INNER JOIN Person.Persona P 
--		ON P.UserId = UL.UserId
--	INNER JOIN CTE C ON C.PartyId = P.OrganizationPartyId AND C.TAID = AA.ActivityConfigurationid

----Update ActivityToken
--;WITH CTE AS
--	(SELECT T.Activityid 'TAId' , AT.ActivityCode, AC.ActivityConfigurationid, AC.PartyId FROM #Temp T
--		INNER JOIN Ident.ActivityType AT
--			ON T.ActivityCOde = AT.ActivityCOde	
--		INNER JOIN Ident.ActivityCOnfiguration AC
--			On AC.ActivityTypeId = AT.ActivityTypeId
--	)
--UPDATE A SET ActivityConfigurationId = C.ActivityConfigurationId
----SELECT * 
-- FROM Ident.ActivityToken A
--	INNER JOIN Enterprise.Party P
--		ON P.RealPageId = A.RealPageId
--	INNER JOIN Person.Persona PE
--		ON P.PartyId = PE.PersonPartyId
--	INNER JOIN CTE C ON C.PartyId = PE.OrganizationPartyId
--		AND C.TAID = A.ActivityConfigurationId

----Update PasswordHistory
--;WITH CTE AS
--	(SELECT T.Activityid 'TAId' , AT.ActivityCode, AC.ActivityConfigurationid, AC.PartyId FROM #Temp T
--		INNER JOIN Ident.ActivityType AT
--			ON T.ActivityCOde = AT.ActivityCOde	
--		INNER JOIN Ident.ActivityCOnfiguration AC
--			On AC.ActivityTypeId = AT.ActivityTypeId
--	)
--UPDATE PH
--SET ActivityConfigurationId = C.ActivityConfigurationId
--	FROM Ident.PasswordHistory PH
--		INNER JOIN Ident.UserLogin UL
--			ON UL.UserId = PH.UserId
--		INNER JOIN Person.Persona P
--			ON P.UserId = UL.UserId
--		INNER JOIN CTE C ON C.PartyId = P.OrganizationPartyId
--			AND C.TAID = PH.ActivityConfigurationId

--GO
update ident.userlogin set thrudate = null where datepart(yyyy, thrudate) = 9999
GO