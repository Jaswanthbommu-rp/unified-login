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

