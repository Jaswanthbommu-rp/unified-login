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
GO


--Begin Sql scripts for ticket numbeer 668019 and 668119

DECLARE @RightId INT,
		@RouteId INT,
        @BasicEndUserRoleId INT,
        @UserAdminRoleId INT,
        @UPRoleId INT,
        @UserId bigint,
        @Now datetime = GETDATE(),
		@partyId INT
		

SELECT    @UserId = UserId
            FROM    Ident.UserLogin
            WHERE    LoginName LIKE 'realpagead@%'


IF NOT EXISTS (SELECT TOP 1 1 FROM Security.[Right] where  RightName = 'AccessUnifiedReporting')
BEGIN
    --insert into right table
	Insert into [Security].[right](RightName, [Description], [Value], [StatusTypeId], [VisibilityStatusId], [ProductId], [TargetProductId], CreatedDate, CreatedBy)
	values ('AccessUnifiedReporting', 'Access to Unified Reporting', 'Access to Unified Reporting', 13, 9, 3, 3, @Now, @UserId)
    
	--get newly inserted right's ID
	select @RightId = RightId from Security.[Right] where  RightName = 'AccessUnifiedReporting'
	print(@RightId)

	--get route id
	select @RouteId = RouteId from [Security].Route where RouteValue = 'sidemenu'
	print(@RouteId)

    --insert into rightroute
    Insert into Security.[RightRoute] (RightId, RouteId, RightName, CreatedBy, CreatedDate)
	values (@RightId, @RouteId, 'Access to Unified Reporting', @UserId, @Now)

    select @UserAdminRoleId = RoleId from security.role where rolename = 'User Administrator' and OrgPartyID IS NULL
    
	select @partyId=  PartyId 	from Enterprise.[Organization]	where Name = 'RealPage Employee'

    IF NOT EXISTS (SELECT TOP 1 1 FROM Security.RoleRight WHERE RoleId = @UserAdminRoleId AND @RightId = RightId)
    BEGIN
         INSERT INTO SECURITY.[RoleRight] (RoleId,RightId,CreatedBy,CreatedDate) 
         VALUES(@UserAdminRoleId,@RightId,@UserId,@Now)
    END

	IF NOT EXISTS (SELECT TOP 1 1 FROM Security.[OrganizationOverRideRight] WHERE RightId = @RightId)
    BEGIN
         INSERT INTO SECURITY.[OrganizationOverRideRight] (RightId, OrgPartyId, VisibilityStatusId,CreatedBy,CreatedDate) 
         VALUES(@RightId, @partyId, 10,@UserId,@Now)
    END
END

GO


DECLARE @RightId INT,
		@RouteId INT,
        @BasicEndUserRoleId INT,
        @UserAdminRoleId INT,
        @UPRoleId INT,
        @UserId bigint,
        @Now datetime = GETDATE(),
		@partyId INT
		

SELECT    @UserId = UserId
            FROM    Ident.UserLogin
            WHERE    LoginName LIKE 'realpagead@%'


IF NOT EXISTS (SELECT TOP 1 1 FROM Security.[Right] where  RightName = 'EmployeeAccessUnifiedReportingAdminConsole')
BEGIN
    --insert into right table
	Insert into [Security].[right](RightName, [Description], [Value], [StatusTypeId], [VisibilityStatusId], [ProductId], [TargetProductId], CreatedDate, CreatedBy)
	values ('EmployeeAccessUnifiedReportingAdminConsole', 'Employee Access to Unified Reporting Admin Console', 'Employee Access to Unified Reporting Admin Console', 13, 10, 3, 3, @Now, @UserId)
    
	--get newly inserted right's ID
	select @RightId = RightId from Security.[Right] where  RightName = 'EmployeeAccessUnifiedReportingAdminConsole'
	print(@RightId)

	--get route id
	select @RouteId = RouteId from [Security].Route where RouteValue = 'sidemenu'
	print(@RouteId)

    --insert into rightroute
    Insert into Security.[RightRoute] (RightId, RouteId, RightName, CreatedBy, CreatedDate)
	values (@RightId, @RouteId, 'Employee Access to Unified Reporting Admin Console', @UserId, @Now)

    select @UserAdminRoleId = RoleId from security.role where rolename = 'User Administrator' and OrgPartyID IS NULL

	select @partyId=  PartyId from Enterprise.[Organization] where Name = 'RealPage Employee'
    
    IF NOT EXISTS (SELECT TOP 1 1 FROM Security.RoleRight WHERE RoleId = @UserAdminRoleId AND @RightId = RightId)
    BEGIN
         INSERT INTO SECURITY.[RoleRight] (RoleId,RightId,CreatedBy,CreatedDate) 
         VALUES(@UserAdminRoleId,@RightId,@UserId,@Now)
    END

	IF NOT EXISTS (SELECT TOP 1 1 FROM Security.[OrganizationOverRideRight] WHERE RightId = @RightId)
    BEGIN
         INSERT INTO SECURITY.[OrganizationOverRideRight] (RightId, OrgPartyId, VisibilityStatusId,CreatedBy,CreatedDate) 
         VALUES(@RightId, @partyId, 9,@UserId,@Now)
    END
END

-- End Sql scripts for ticket numbeer 668019 and 668119
GO

-- ShowInAuditProductPage, ShowInNewCompanySetup

if not exists ( select top 1 1 from Enterprise.ProductSettingType where name = 'ShowInAuditProductPage' )
begin
	insert into enterprise.ProductSettingType ( name, Description, SensitiveData ) values ( 'ShowInAuditProductPage', 'Should the product be shown in the UPFM property audit page', 0)
end

if not exists ( select top 1 1 from Enterprise.ProductSettingType where name = 'ShowInNewCompanySetup' )
begin
	insert into enterprise.ProductSettingType ( name, Description, SensitiveData ) values ( 'ShowInNewCompanySetup', 'Should the product be shown in the company product setup page', 0)
end

DECLARE @NOW DATETIME = GETUTCDATE(); 
declare @productlist table ( entid int identity, productid int, productsettingtype varchar(500), productsettingvalue varchar(2000))
insert into @productlist values 
	(1, 'ShowInNewCompanySetup', '1' ),
	(4, 'ShowInNewCompanySetup', '1' ),
	(6, 'ShowInNewCompanySetup', '1' ),
	(8, 'ShowInNewCompanySetup', '1' ),
	(9, 'ShowInNewCompanySetup', '1' ),
	(10, 'ShowInNewCompanySetup', '1' ),
	(13, 'ShowInNewCompanySetup', '1' ),
	(14, 'ShowInNewCompanySetup', '1' ),
	(15, 'ShowInNewCompanySetup', '1' ),
	(16, 'ShowInNewCompanySetup', '1' ),
	(17, 'ShowInNewCompanySetup', '1' ),
	(18, 'ShowInNewCompanySetup', '1' ),
	(20, 'ShowInNewCompanySetup', '1' ),
	(21, 'ShowInNewCompanySetup', '1' ),
	(23, 'ShowInNewCompanySetup', '1' ),
	(26, 'ShowInNewCompanySetup', '1' ),
	(27, 'ShowInNewCompanySetup', '1' ),
	(36, 'ShowInNewCompanySetup', '1' ),
	(37, 'ShowInNewCompanySetup', '1' ),
	(38, 'ShowInNewCompanySetup', '1' ),
	(39, 'ShowInNewCompanySetup', '1' ),
	(40, 'ShowInNewCompanySetup', '1' ),
	(41, 'ShowInNewCompanySetup', '1' ),
	(44, 'ShowInNewCompanySetup', '1' ),
	(45, 'ShowInNewCompanySetup', '1' ),
	(46, 'ShowInNewCompanySetup', '1' ),
	(47, 'ShowInNewCompanySetup', '1' ),
	(48, 'ShowInNewCompanySetup', '1' ),
	(49, 'ShowInNewCompanySetup', '1' ),
	(50, 'ShowInNewCompanySetup', '1' ),
	(55, 'ShowInNewCompanySetup', '1' ),
	(56, 'ShowInNewCompanySetup', '1' ),
	(57, 'ShowInNewCompanySetup', '1' ),
	(58, 'ShowInNewCompanySetup', '1' ),
	(59, 'ShowInNewCompanySetup', '1' ),
	(60, 'ShowInNewCompanySetup', '1' ),
	(63, 'ShowInNewCompanySetup', '1' ),

	(1, 'ShowInAuditProductPage', '1' ),
	(3, 'ShowInAuditProductPage', '1' ),
	(4, 'ShowInAuditProductPage', '0' ),
	(6, 'ShowInAuditProductPage', '1' ),
	(8, 'ShowInAuditProductPage', '0' ),
	(9, 'ShowInAuditProductPage', '1' ),
	(10, 'ShowInAuditProductPage', '0' ),
	(13, 'ShowInAuditProductPage', '0' ),
	(14, 'ShowInAuditProductPage', '0' ),
	(15, 'ShowInAuditProductPage', '0' ),
	(16, 'ShowInAuditProductPage', '0' ),
	(17, 'ShowInAuditProductPage', '0' ),
	(18, 'ShowInAuditProductPage', '0' ),
	(20, 'ShowInAuditProductPage', '0' ),
	(21, 'ShowInAuditProductPage', '0' ),
	(23, 'ShowInAuditProductPage', '0' ),
	(26, 'ShowInAuditProductPage', '0' ),
	(27, 'ShowInAuditProductPage', '0' ),
	(36, 'ShowInAuditProductPage', '0' ),
	(37, 'ShowInAuditProductPage', '0' ),
	(38, 'ShowInAuditProductPage', '0' ),
	(39, 'ShowInAuditProductPage', '0' ),
	(40, 'ShowInAuditProductPage', '0' ),
	(41, 'ShowInAuditProductPage', '0' ),
	(44, 'ShowInAuditProductPage', '0' ),
	(45, 'ShowInAuditProductPage', '0' ),
	(46, 'ShowInAuditProductPage', '0' ),
	(47, 'ShowInAuditProductPage', '0' ),
	(48, 'ShowInAuditProductPage', '0' ),
	(49, 'ShowInAuditProductPage', '0' ),
	(50, 'ShowInAuditProductPage', '0' ),
	(55, 'ShowInAuditProductPage', '0' ),
	(56, 'ShowInAuditProductPage', '0' ),
	(57, 'ShowInAuditProductPage', '0' ),
	(58, 'ShowInAuditProductPage', '0' ),
	(59, 'ShowInAuditProductPage', '0' ),
	(60, 'ShowInAuditProductPage', '0' ),
	(63, 'ShowInAuditProductPage', '0' )
	
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

DECLARE @ProductId INT, 
		@LoginURI NVARCHAR(100), 
		@SigningCertificateThumbprint NVARCHAR(50), 
		@ParentProductTypeId INT, 
		@ProductName NVARCHAR(100)= 'Market Analytics',  -- Produact Name
		@LoginURL NVARCHAR(500), 
		@ProductUrl NVARCHAR(256), 
		@apiendpoint NVARCHAR(1000), 
		@ServerName SYSNAME = @@SERVERNAME;

DECLARE @ProductConfiguration AS PRODUCTCONFIGURATIONTYPE;

/*Validate what product type ths new product belongs to. 'Administration' in the following block 
need to be chnanged to desired prodcut type. You can query Enterprise.ProductType table for more details.
*/

SELECT @ParentProductTypeId = ProductTypeId
FROM Enterprise.ProductType
WHERE Name = 'Asset Optimization'
      AND ParentProductTypeId IS NULL;
IF NOT EXISTS
(
    SELECT TOP 1 1
    FROM enterprise.ProductType
    WHERE Name = 'Market Analytics'
)
    BEGIN
        EXEC [Enterprise].[CreateProductType] 
             @ProductTypeId = 411, -- Thsi value may change based on the root prodcut type
             @ParentProductTypeId = @ParentProductTypeId, 
             @Name = @ProductName, 
             @Description = 'Market Analytics provides unparalleled visibility into market conditions sourced through actual lease transactions, plus precision forecasts, detailed rent comps down to the floorplan level, Real Capital Analytics sales transactions, and analyst-written commentaries.', 
             @ProductTypeGUID = '964535EE-1A47-4906-8274-233DFC36B462'; -- Use newid() to generate new uniqueidentifier.
END;
SET @ProductId = 66; -- Assign new product Id

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
             @ProductGUID = '2848A410-8CA5-49F4-B490-D3D74A5A8E6C', -- Use newid() to generate new uniqueidentifier.
             @Name = @ProductName, 
             @Description = 'Market Analytics provides unparalleled visibility into market conditions sourced through actual lease transactions, plus precision forecasts, detailed rent comps down to the floorplan level, Real Capital Analytics sales transactions, and analyst-written commentaries.', 
             @ProductTypeId = 411;
        
		UPDATE Enterprise.Product
          SET 
              BooksProductCode = 'RMA'
        WHERE ProductId = @ProductId;
END;

--The following block picks up all the detail frm Enterprise.ProductSettingType table
--To set up the product, bunch of these settings are required.
SET @apiendpoint = '';
IF @ServerName IN ('RCDUSODBSQL001')
BEGIN
	SET @apiendpoint = 'https://aodev.realpage.com/ma/a/';
END
IF @ServerName IN ('rctusodbsql001')
BEGIN
	SET @apiendpoint = 'https://aoqa.realpage.com/ma/a/';
END
IF @ServerName IN ('RCQUSODBSQL001')
BEGIN
	SET @apiendpoint = 'https://aosat.realpage.com/ma/a/';
END
IF @ServerName IN ('RCPGBKDBSQL005A', 'RCPGBKDBSQL005B')
BEGIN
	SET @apiendpoint = 'https://ao.realpage.com/ma/a/';
END
set nocount on
INSERT INTO @ProductConfiguration
(SettingName, 
 SettingDescription, 
 SettingValue
)
VALUES
('ClientId','','1')
,('ClassName','','marketanalytics')
,('ProductUrl','','/product/marketanalytics')
,('TitleId','','Market Analytics')
,('TitleUniqueId','','71665D14-0CD0-46CF-86A1-6EE6435D8ABD')
,('IsNewTab','','1')
,('MetatagUniqueId','','Market Analytics')
,('IsResource','','0')
,('IsFavorite','','1')
,('LearnMore','','https://www.realpage.com/asset-optimization/market-analytics/')
,('ApiEndPoint','',@apiendpoint)
,('ApiUserName','','wsuser')
,('ApiPassword','','cGdAIXcyM3Jn')
,('ProductSuperUserLoginName','','amungale')
,('ProductStatus','Show if the external application was configured for the dashboard user.','8')
,('ProductStatus','Show if the external application was configured for the dashboard user.','10')
,('ProductStatus','Show if the external application was configured for the dashboard user.','19')
,('ShowInUserDetails','Should the product show in the New/Edit user pages','1')
,('ShowInRolesAndRights','Should the product show in the Role/Rights page','0')
,('ShowInAppSwitcher','Should the product show in the application switcher','1')
,('ShowInUserListFilter','Should the product show in the user list product pick list','1')
,('ProductAPIRequiresUser','Does the product require a user for api calls','0')
,('LockOnProductAccess', '', '0')
,('ProductNotAvailableForRegularUserNoEmail','Product Attribute for Product Not Available for Regular User No Email.','0')
,('NotificationEmailRequiredForUserWithNoEmail', '', '1')
,('AuthenticationType','Used to determine how to log into the product','Redirect')



SELECT * FROM @ProductConfiguration

SET @LoginURL = '';
IF @ServerName IN ('RCDUSODBSQL001')
BEGIN
	SET @LoginURL = 'https://aodev.realpage.com/ysconfig/sso/oauth?Product=RMA';
END
IF @ServerName IN ('rctusodbsql001')
BEGIN
	SET @LoginURL = 'https://aoqa.realpage.com/ysconfig/sso/oauth?Product=RMA';
END
IF @ServerName IN ('RCQUSODBSQL001')
BEGIN
	SET @LoginURL = 'https://aosat.realpage.com/ysconfig/sso/oauth?Product=RMA';
END
IF @ServerName IN ('RCPGBKDBSQL005A', 'RCPGBKDBSQL005B')
BEGIN
	SET @LoginURL = 'https://ao.realpage.com/ysconfig/sso/oauth?Product=RMA';
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
IF NOT EXISTS (SELECT TOP 1 1 FROM UserManagement.Control where UIID like '%MarketAnalyticsProductAccessTabGroupUIId%')
BEGIN
	SET IDENTITY_INSERT [UserManagement].[Control] ON 

	Declare @MaxControlId INT
	select @MaxControlId = MAX(controlId) from UserManagement.Control 
	
	insert into UserManagement.Control (ControlId, ParentControlId, ControlTypeId, UIId, DisplayName,DataSource, [Sequence], CreatedBy,CreatedDate)
	values(@MaxControlId + 1,NULL,8,'MarketAnalyticsProductAccessTabGroupUIId',NULL,NULL,1,480,GETDATE())

	insert into UserManagement.Control(ControlId, ParentControlId, ControlTypeId, UIId, DisplayName,DataSource, [Sequence], CreatedBy,CreatedDate)
	values(@MaxControlId + 2,@MaxControlId + 1,9,'MarketAnalyticsProductAccessRolesTabUIId','Roles',NULL,1,480,GETDATE())

	insert into UserManagement.Control(ControlId, ParentControlId, ControlTypeId, UIId, DisplayName,DataSource, [Sequence], CreatedBy,CreatedDate)
	values(@MaxControlId + 3,@MaxControlId + 2,3,'MarketAnalyticsProductAccessRolesMultiSelectGridUIId',NULL,NULL,1,480,GETDATE())

	insert into UserManagement.Control(ControlId, ParentControlId, ControlTypeId, UIId, DisplayName,DataSource, [Sequence], CreatedBy,CreatedDate)
	values(@MaxControlId + 4,@MaxControlId + 3,10,'MarketAnalyticsProductAccessCheckboxUIId',NULL,'isAssigned',1,480,GETDATE())

	insert into UserManagement.Control(ControlId, ParentControlId, ControlTypeId, UIId, DisplayName,DataSource, [Sequence], CreatedBy,CreatedDate)
	values(@MaxControlId + 5,@MaxControlId + 3,5,'MarketAnalyticsProductAccessRoleLabelUIId','Role','name',2,480,GETDATE())

	insert into UserManagement.Control(ControlId, ParentControlId, ControlTypeId, UIId, DisplayName,DataSource, [Sequence], CreatedBy,CreatedDate)
	values(@MaxControlId + 6,@MaxControlId + 1,9,'MarketAnalyticsProductAccessMarketsTabUIId','Markets',NULL,1,480,GETDATE())

	insert into UserManagement.Control(ControlId, ParentControlId, ControlTypeId, UIId, DisplayName,DataSource, [Sequence], CreatedBy,CreatedDate)
	values(@MaxControlId + 7,@MaxControlId + 6,3,'MarketAnalyticsProductAccessMarketsMultiSelectGridUIId',NULL,NULL,1,480,GETDATE())

	insert into UserManagement.Control(ControlId, ParentControlId, ControlTypeId, UIId, DisplayName,DataSource, [Sequence], CreatedBy,CreatedDate)
	values(@MaxControlId + 8,@MaxControlId + 7,10,'MarketAnalyticsProductAccessCheckboxUIId',NULL,'isAssigned',1,480,GETDATE())

	insert into UserManagement.Control(ControlId, ParentControlId, ControlTypeId, UIId, DisplayName,DataSource, [Sequence], CreatedBy,CreatedDate)
	values(@MaxControlId + 9,@MaxControlId + 7,5,'MarketAnalyticsProductAccessMarketsLabelUIId','Market','name',2,480,GETDATE())

	SET IDENTITY_INSERT [UserManagement].[Control] OFF

	SET IDENTITY_INSERT [UserManagement].ControlAttribute ON 

	Declare @MaxControlAttributeId INT
	select @MaxControlAttributeId = MAX(ControlAttributeId) from UserManagement.ControlAttribute 

	insert into UserManagement.ControlAttribute(ControlAttributeId, ControlId, [Key], [Value],[CreatedBy],[CreatedDate])
	values(@MaxControlAttributeId + 1,@MaxControlId + 2,'Default','True',480,GETDATE())

	insert into UserManagement.ControlAttribute(ControlAttributeId, ControlId, [Key], [Value],[CreatedBy],[CreatedDate])
	values(@MaxControlAttributeId + 2,@MaxControlId + 7,'ShowSelectAll','True',480,GETDATE())

	SET IDENTITY_INSERT [UserManagement].ControlAttribute OFF 

	SET IDENTITY_INSERT [UserManagement].ProductPage ON 
	
	Declare @MaxProductPageId INT
	select @MaxProductPageId = MAX(ProductPageId) from UserManagement.ProductPage

	INSERT INTO [UserManagement].ProductPage([ProductPageId],ProductId,DisplayName,CreatedBy,CreatedDate,IsActive,ProductPageTypeId)
	VALUES(@MaxProductPageId + 1 ,66,'Market Analytics Product Access',480,GETDATE(),1,1)

	SET IDENTITY_INSERT [UserManagement].ProductPage OFF 

	SET IDENTITY_INSERT [UserManagement].ProductPageControl ON 

	Declare @MaxProductPageControlId INT
	select @MaxProductPageControlId = MAX(ProductPageControlId) from UserManagement.ProductPageControl

	INSERT INTO [UserManagement].ProductPageControl(ProductPageControlId,ProductPageId,ControlId,CreatedBy,CreatedDate)
	VALUES(@MaxProductPageControlId + 1,@MaxProductPageId + 1,@MaxControlId + 1,480,GETDATE())

	SET IDENTITY_INSERT [UserManagement].ProductPageControl OFF
END
GO
IF EXISTS(SELECT TOP 1 1 FROM Enterprise.Product where ProductId = 66 and UDMSourceCode IS NULL)
BEGIN
 UPDATE Enterprise.Product SET UDMSourceCode = 'AO' where ProductId = 66 
END
GO

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
