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

SET @ProductUrl = '';

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

SET @LoginURL = ''

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
            WHERE  LoginName LIKE 'realpagead@%'


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
	(60, 'ShowInAuditProductPage', '0' )
	
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
--Accounting Location Group
--Declare @FSMasterControlId int,@FSLocationGroupControlId int,@MaxControlId int,@MaxControlAttributeId int
--DECLARE @UserId bigint,
--	@ProductId int ,
--	@Now datetime = GETDATE()

--SELECT	@UserId = UserId
--FROM	Ident.UserLogin
--WHERE	LoginName LIKE 'realpagead@%'

--Select @FSMasterControlId = ControlId From UserManagement.Control 
--Where UIId = 'FinancialSuiteProductAccessTabGroupUIId' AND ControlTypeId = 8

--Select @FSLocationGroupControlId = ControlId From UserManagement.Control 
--Where UIId = 'FinancialSuiteProductAccessLocationGroupTabUIId' AND ControlTypeId = 9

--update [UserManagement].[Control] set Sequence = 8
--where uiid = 'FinancialSuiteProductAccessEntitiesTabUIId' and ControlTypeId = 9

--IF NOT EXISTS (SELECT TOP 1 1 FROM[UserManagement].[Control] WHERE ControlId = @FSLocationGroupControlId)
--BEGIN
	
--	SET IDENTITY_INSERT [UserManagement].[Control] ON 
--	SELECT @MaxControlId = max(ControlId) from UserManagement.Control

--	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
--	VALUES (@MaxControlId +1, @FSMasterControlId, 9, N'FinancialSuiteProductAccessLocationGroupTabUIId', N'Location Group', NULL, 7, @UserId, @Now)

--	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
--	VALUES (@MaxControlId +2, @MaxControlId +1, 3, N'FinancialSuiteProductAccessLocationGroupMultiSelectGridUIId', NULL, NULL, 1, @UserId, @Now)

--	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
--	VALUES (@MaxControlId +3, @MaxControlId +2, 10, N'FinancialSuiteProductAccessLocationGroupCheckboxUIId', NULL, N'isAssigned', 2, @UserId, @Now)

--	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
--	VALUES (@MaxControlId +4, @MaxControlId +2, 5, N'FinancialSuiteProductAccessLocationGroupLabelUIId', N'Location Group', N'name', 3, @UserId, @Now)

--	--PGSlide
--			INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
--			VALUES (@MaxControlId +5, @MaxControlId +2, 11, N'FinancialSuiteProductAccessLocationGroupIconUIId', NULL, N'InfoIcon', 4, @UserId, @Now)

--			INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
--			VALUES (@MaxControlId +6, @MaxControlId +5, 5, N'FinancialSuiteProductAccessLocationGroupDetailsLabelUIId', N'Location Group Details', NULL, 1, @UserId, @Now)

--			INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
--			VALUES (@MaxControlId +7, @MaxControlId +5, 12, N'FinancialSuiteProductAccessLocationGroupDetailsGridUIId', NULL, NULL, 2, @UserId, @Now)

--			INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
--			VALUES (@MaxControlId +8, @MaxControlId +7, 5, N'FinancialSuiteProductAccessLocationGroupDetailsPropertyLabelUIId', N'Entity', N'name', 2, @UserId, @Now)
			
--			SET IDENTITY_INSERT [UserManagement].[Control] OFF
--END

--		SET IDENTITY_INSERT [UserManagement].[ControlAttribute] ON 
            
--			SELECT @MaxControlAttributeId = max(ControlAttributeId) from UserManagement.ControlAttribute
--			Declare @FSLGSelallID int,@FSLGinfoiconID int

--			Select @FSLGSelallID = ControlId From UserManagement.Control 
--            Where UIId = 'FinancialSuiteProductAccessLocationGroupMultiSelectGridUIId' AND ControlTypeId = 3

--            Select @FSLGinfoiconID = ControlId From UserManagement.Control 
--            Where UIId = 'FinancialSuiteProductAccessLocationGroupIconUIId' AND ControlTypeId = 11

--			IF NOT EXISTS ( SELECT TOP 1 1 FROM [UserManagement].[ControlAttribute] WHERE ControlId = @FSLGSelallID)
--			BEGIN
--				INSERT [UserManagement].[ControlAttribute] ([ControlAttributeId], [ControlId], [Key], [Value], [CreatedBy], [CreatedDate]) 
--				VALUES (@MaxControlAttributeId + 1, @FSLGSelallID, N'ShowSelectAll', N'False', @UserId, @Now)
--			END

--			--SELECT @MaxControlAttributeId = max(ControlAttributeId) from UserManagement.ControlAttribute
--			IF NOT EXISTS ( SELECT TOP 1 1 FROM [UserManagement].[ControlAttribute] WHERE ControlId = @FSLGinfoiconID)
--			BEGIN
--				INSERT [UserManagement].[ControlAttribute] ([ControlAttributeId], [ControlId], [Key], [Value], [CreatedBy], [CreatedDate]) 
--				VALUES (@MaxControlAttributeId + 2, @FSLGinfoiconID, N'InfoIcon', N'Slide', @UserId, @Now)		
--			END
			
--            SET IDENTITY_INSERT [UserManagement].[ControlAttribute] OFF

GO

--CREATE new product called Reporting
exec [Enterprise].CreateNewProduct 
	@ProductId = 67,
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
/***********************************
Add UsePrimaryProperties ORG Level
***********************************/
DECLARE @ProductSettingTypeId INT
DECLARE @ProductSettingId INT
DECLARE @PRoductId INT
DECLARE @COnfigurationId INT
DECLARE @MasterConFigurationTypeId VARCHAR(100);

DECLARE @NOW DATETIME = GETUTCDATE(); 

SELECT @MasterConFigurationTypeId = MasterConfigurationTypeId
FROM Enterprise.MasterConfigurationType
WHERE Name = 'Organization';

IF NOT EXISTS(SELECT 1 FROM Enterprise.MasterSettingType WHERE Name = 'UsePrimaryProperties' AND MasterConfigurationTypeId = @MasterConFigurationTypeId)
BEGIN
INSERT INTO Enterprise.MasterSettingType
(Name,
 MasterConfigurationTypeId
)
VALUES
('UsePrimaryProperties',
 @MasterConFigurationTypeId
);
END

GO
if not exists ( select top 1 1 from Enterprise.ProductSettingType where name = 'UsePrimaryProperties' )
begin
	insert into enterprise.ProductSettingType ( name, Description, SensitiveData ) values ( 'UsePrimaryProperties', 'Should the product consume UPFM property', 0)
end


DECLARE @NOW DATETIME = GETUTCDATE(); 
declare @productlist table ( entid int identity, productid int, productsettingtype varchar(500), productsettingvalue varchar(2000))
insert into @productlist (productid, productsettingtype,productsettingvalue)
Select productid,'UsePrimaryProperties','1' From enterprise.product
	
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
--insert userprimaryproperties data for orgs
DECLARE @MasterConfigurationId BIGINT;
DECLARE @MasterConfigurationTypeId BIGINT;
DECLARE @MasterSettingId INT;
Declare @MasterSettingTypeId INT;
DECLARE @NOW DATETIME = GETUTCDATE(); 

 SELECT @MasterConfigurationTypeId = MasterConfigurationTypeId
 FROM Enterprise.MasterConfigurationType
 WHERE Name = 'Organization';

 
	Select @MasterSettingTypeId = MasterSettingTypeId from Enterprise.MasterSettingType where Name = 'UsePrimaryProperties'

				IF NOT EXISTS (
                    SELECT 1
                    FROM Enterprise.MasterSetting
                    WHERE Value = '0'
					AND MasterSettingTypeId = @MasterSettingTypeId
                )
                    BEGIN
                        INSERT INTO Enterprise.MasterSetting
                        (MasterSettingTypeId, 
                         Value, 
                         FromDate
                        )
                        VALUES
                        (@MasterSettingTypeId, 
                         '0', 
                         @NOW
                        );                      
                END;
      

				IF NOT EXISTS
                (
                    SELECT 1
                    FROM Enterprise.MasterSetting
                    WHERE Value = '1'
					AND MasterSettingTypeId = @MasterSettingTypeId
                )
                BEGIN
                        INSERT INTO Enterprise.MasterSetting
                        (MasterSettingTypeId, 
                         Value, 
                         FromDate
                        )
                        VALUES
                        (@MasterSettingTypeId, 
                         '1', 
                         @NOW
                        );
				END;

 
declare @orglist table ( entid int identity, partyid int)
insert into @orglist (partyid)
Select PartyId From enterprise.Organization
	
--select * from @productlist
SELECT @MasterSettingId = MasterSettingId
 FROM Enterprise.MasterSetting AS MS
 INNER JOIN Enterprise.MasterSettingType AS MST ON MST.MasterSettingTypeId = MS.MasterSettingTypeId
 WHERE MST.MasterConfigurationTypeId = @MasterConfigurationTypeId
 AND MST.Name = 'UsePrimaryProperties'
 AND MS.Value = '0'

declare @MAX_ID INT
declare @Current_ID INT = 1
declare @Currentpartyid INT = 1

select @MAX_ID = max(entid) from @orglist

while @Current_ID <= @MAX_ID
begin
	select @Currentpartyid = partyid
		from @orglist where entid = @Current_ID

		IF NOT EXISTS ( SELECT 1 FROM Enterprise.MasterConfiguration
					WHERE MasterConfigurationTypeId = @MasterConfigurationTypeId
					AND AttributeId = @Currentpartyid)
                 BEGIN
                     INSERT INTO Enterprise.MasterConfiguration
						(MasterConfigurationTypeId,
						 AttributeId,
						 FromDate
						)
                     VALUES
						(@MasterConfigurationTypeId,
						 @Currentpartyid,
						 @NOW
						);
                     SELECT @MasterConfigurationId = SCOPE_IDENTITY();
                 END;
				 ELSE
				BEGIN
				SELECT @MasterConfigurationId = MasterConfigurationId FROM Enterprise.MasterConfiguration
					WHERE MasterConfigurationTypeId = @MasterConfigurationTypeId
					AND AttributeId = @Currentpartyid
				END

             IF NOT EXISTS
				(
					SELECT 1
					FROM Enterprise.MasterConfigurationSetting
					WHERE MasterConfigurationId = @MasterConfigurationId
						  AND MasterSettingId = @MasterSettingId
				)
                 BEGIN
                     INSERT INTO Enterprise.MasterConfigurationSetting
						(MasterConfigurationId,
						 MasterSettingId
						)
                     VALUES
						(@MasterConfigurationId,
						 @MasterSettingId
						);
                 END;
	set @Current_ID = @Current_ID + 1
end

GO
--Accounting Location Group
Declare @MCMasterControlId int,@MCUPPControlId int,@MaxControlId int,@MaxControlAttributeId int
DECLARE @UserId bigint,
	@ProductId int ,
	@Now datetime = GETDATE()

SELECT	@UserId = UserId
FROM	Ident.UserLogin
WHERE	LoginName LIKE 'realpagead@%'

Select @MCMasterControlId = ControlId From UserManagement.Control 
Where UIId = 'MarketingCenterProductAccessPropertiesTabUIId' AND ControlTypeId = 9

Select @MCUPPControlId = ControlId From UserManagement.Control 
Where UIId = 'MarketingCenterProductAccessUsePrimaryPropertiesSwitchUIId' AND ControlTypeId = 1

IF NOT EXISTS (SELECT TOP 1 1 FROM[UserManagement].[Control] WHERE ControlId = @MCUPPControlId)
BEGIN
	
	SET IDENTITY_INSERT [UserManagement].[Control] ON 
	SELECT @MaxControlId = max(ControlId) from UserManagement.Control

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
	VALUES (@MaxControlId +1, @MCMasterControlId, 1, N'MarketingCenterProductAccessUsePrimaryPropertiesSwitchUIId', N'Use Primary Properties', N'UsePrimaryProperties', 2, @UserId, @Now)

	SET IDENTITY_INSERT [UserManagement].[Control] OFF
END

		
GO
   --Panel Script for Sustain:Water
DECLARE @UserId bigint,
       @ProductId int = 59,
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

IF NOT EXISTS (SELECT TOP 1 1 FROM [UserManagement].[ProductPage] WHERE ProductId = @ProductId)
BEGIN
		SET IDENTITY_INSERT [UserManagement].[Control] ON 

		SELECT @MaxControlId = max(ControlId) from UserManagement.Control

		INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate]) 
		VALUES (@MaxControlId + 1, NULL, 8, N'IntelligentBuildingWaterUIId', NULL, NULL, 1, @UserId, @Now)
		
		INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate]) 
		VALUES (@MaxControlId + 2, @MaxControlId + 1, 9, N'IntelligentBuildingWaterAccessRolesTabUIId', N'Roles', NULL, 1, @UserId, @Now)
		

		INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate]) 
		VALUES (@MaxControlId + 3, @MaxControlId + 2, 2, N'IntelligentBuildingWaterAccessRolesSelectGridUIId', NULL, NULL, 2, @UserId, @Now)

		INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate]) 
		VALUES (@MaxControlId + 4, @MaxControlId + 3, 7, N'IntelligentBuildingWaterAccessRadioUIId', NULL, N'isAssigned', 1, @UserId, @Now)

		INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate]) 
		VALUES (@MaxControlId + 5, @MaxControlId + 3, 5, N'IntelligentBuildingWaterAccessRoleLabelUIId', N'Role', N'name', 2, @UserId, @Now)

		INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate]) 
		VALUES (@MaxControlId + 6, @MaxControlId + 3, 5, N'IntelligentBuildingWaterAccessRoleTypeLabelUIId', N'Role Type', N'roletype', 3, @UserId, @Now)
		
		INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate]) 
		VALUES (@MaxControlId + 7, @MaxControlId + 3, 11, N'IntelligentBuildingWaterAccessIconUIId', NULL, N'InfoIcon', 4, @UserId, @Now)
		
		INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate]) 
		VALUES (@MaxControlId + 8, @MaxControlId + 1, 9, N'IntelligentBuildingWaterAccessPropertiesTabUIId', N'Properties', NULL, 2, @UserId, @Now)
		
		INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate]) 
		VALUES (@MaxControlId + 9, @MaxControlId + 8, 1, N'IntelligentBuildingWaterAccessAllowaccesstoallcurrentandfuturepropertiesPropertiesSwitchUIId', N'Assign access to current and new properties automatically', N'allProperties', 1, @UserId, @Now)

		INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate]) 
		VALUES (@MaxControlId + 10, @MaxControlId + 8, 3, N'IntelligentBuildingWaterAccessPropertiesMultiSelectGridUIId', NULL, NULL, 2, @UserId, @Now)

		INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate]) 
		VALUES (@MaxControlId + 11, @MaxControlId + 10, 10, N'IntelligentBuildingWaterAccessCheckboxUIId', NULL, N'isAssigned', 1, @UserId, @Now)

		INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate]) 
		VALUES (@MaxControlId + 12, @MaxControlId + 10, 5, N'IntelligentBuildingWaterAccessPropertyLabelUIId', N'Property', N'name', 2, @UserId, @Now)

		INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate]) 
		VALUES (@MaxControlId + 13, @MaxControlId + 10, 5, N'IntelligentBuildingWaterAccessCityLabelUIId', N'City', N'city', 3, @UserId, @Now)

		INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate]) 
		VALUES (@MaxControlId + 14, @MaxControlId + 10, 5, N'IntelligentBuildingWaterAccessStateLabelUIId', N'State', N'state', 4, @UserId, @Now)

		INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate]) 
		VALUES (@MaxControlId + 15, @MaxControlId + 7, 5, N'IntelligentBuildingWaterAccessRoleDetailsLabelUIId', N'Role Details', NULL, 1, @UserId, @Now)

		INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate]) 
		VALUES (@MaxControlId + 16, @MaxControlId + 7, 12, N'IntelligentBuildingWaterAccessGridUIId', N'NULL', NULL, 1, @UserId, @Now)

		INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate]) 
		VALUES (@MaxControlId + 17, @MaxControlId + 16, 5, N'IntelligentBuildingWaterAccessRightLabelUIId', N'Right', 'description', 1, @UserId, @Now)

		 
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
		VALUES (43, 59, N'Sustain: Water Product Access', @UserId, @Now, 1, 1)

		SET IDENTITY_INSERT [UserManagement].[ProductPage] OFF

		SET IDENTITY_INSERT [UserManagement].[ProductPageControl] ON 

		INSERT [UserManagement].[ProductPageControl] ([ProductPageControlId], [ProductPageId], [ControlId], [CreatedBy], [CreatedDate])
		VALUES (53, 43, @MaxControlId + 1, @UserId, @Now)

		SET IDENTITY_INSERT [UserManagement].[ProductPageControl] OFF
            
END
GO

Declare @ServerName SYSNAME = @@SERVERNAME;

IF @ServerName IN ('RCPGBKDBSQL005A', 'RCPGBKDBSQL005B')
BEGIN
	IF EXISTS(SELECT 1 FROM Ident.SamlProductSettings where ProductId = 59 and LoginUri ='www.abcwater.realpage.com')
   BEGIN
          UPDATE Ident.SamlProductSettings SET LoginUri = 'https://sustain-water.realpage.com/' where ProductId = 59 
   END
END
GO
IF EXISTS(SELECT 1 FROM Enterprise.Product where ProductId = 59 AND Name = N'Intelligent Building Water' AND Description=N'Intelligent Building Water' )
BEGIN
   UPDATE Enterprise.Product SET Name= N'Sustain: Water', Description= N'Sustain: Water' where ProductId = 59 
END
GO


-- defect 701779
update [Security].[Right]
set TargetProductId = (select ProductId from Enterprise.Product where Name = 'Reporting' and BooksProductCode = 'RPT')
where RightName = 'AccessUnifiedReporting'


-- Update UDMSourceCode for ILMLM and ILMLA Products
IF EXISTS(SELECT * FROM Enterprise.Product WHERE ProductId = 40 AND UDMSourceCode IS NULL)
	UPDATE Enterprise.Product SET UDMSourceCode = 'ILMLA' WHERE ProductId = 40
IF EXISTS(SELECT * FROM Enterprise.Product WHERE ProductId = 41 AND UDMSourceCode IS NULL)
	UPDATE Enterprise.Product SET UDMSourceCode = 'ILMLA' WHERE ProductId = 41

GO
--LeaseLabs
/*This script is a sample script to create new prodcut in the system.*/

DECLARE @ProductId INT, 
		@LoginURI NVARCHAR(100), 
		@SigningCertificateThumbprint NVARCHAR(50), 
		@ParentProductTypeId INT, 
		@ProductName NVARCHAR(100)= 'LeaseLabs',  -- Produact Name
		@LoginURL NVARCHAR(500), 
		@ProductUrl NVARCHAR(256), 
		@apiendpoint NVARCHAR(1000), 
		@tokenEndPoint NVARCHAR(1000), 
		@apisecret NVARCHAR(1000),
		@ServerName SYSNAME = @@SERVERNAME,
		@ProductDescription NVARCHAR(MAX) =''

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
    WHERE Name = 'LeaseLabs'
)
    BEGIN
        EXEC [Enterprise].[CreateProductType] 
             @ProductTypeId = 314, -- Thsi value may change based on the root prodcut type
             @ParentProductTypeId = @ParentProductTypeId, 
             @Name = @ProductName, 
             @Description = @ProductName, 
             @ProductTypeGUID = '3B85E000-D339-40DC-89A7-D695D4F48082'; -- Use Select newid() to generate new uniqueidentifier.
END;
SET @ProductId = 68; -- Assign new product Id

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
             @ProductGUID = '70267DB2-902F-4113-88F5-5A1646E65564', -- Use Select newid() to generate new uniqueidentifier.
             @Name = @ProductName, 
             @Description = @ProductDescription, 
             @ProductTypeId = 314;
        
		UPDATE Enterprise.Product
          SET 
              BooksProductCode = 'LeaseLabs'
        WHERE ProductId = @ProductId;
END;

--The following block picks up all the detail frm Enterprise.ProductSettingType table
--To set up the product, bunch of these settings are required.
SET @apiendpoint = '';
Set @tokenEndPoint = '';
SET @apisecret = '';
IF @ServerName IN ('RCDUSODBSQL001')
BEGIN
	SET @apiendpoint = 'https://admin.dev-ws.realpage.com/';
	SET @tokenEndPoint = '';
	SET @apisecret = '';
END
IF @ServerName IN ('rctusodbsql001')
BEGIN
	SET @apiendpoint = 'https://admin.qa-ws.realpage.com/';
	SET @tokenEndPoint = '';
	SET @apisecret = '';
END
IF @ServerName IN ('RCQUSODBSQL001') 
BEGIN
	SET @apiendpoint = 'https://admin.sat-ws.realpage.com/';
END
IF @ServerName IN ('RCPGBKDBSQL005A', 'RCPGBKDBSQL005B')
BEGIN
	SET @apiendpoint = 'https://admin.ws.realpage.com/';
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
 ('ClassName','','leaselabs')
,('ProductUrl','','/product/leaselabs')
,('TitleId','','LeaseLabs')
,('TitleUniqueId','','F94C333F-EDE2-497D-9109-9631BE29ACC1')
,('IsNewTab','','1')
,('MetatagUniqueId','','LeaseLabs')
,('IsResource','','0')
,('IsFavorite','','1')
,('LearnMore','','')
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
,('NotificationEmailRequiredForUserWithNoEmail', '', '0')



SELECT * FROM @ProductConfiguration

SET @LoginURL = '';
IF @ServerName IN ('RCDUSODBSQL001')
BEGIN
	SET @LoginURL = 'https://admin.dev-ws.realpage.com/';
END
IF @ServerName IN ('rctusodbsql001')
BEGIN
	SET @LoginURL = 'https://admin.qa-ws.realpage.com/';
END
IF @ServerName IN ('RCQUSODBSQL001')
BEGIN
	SET @LoginURL = 'https://admin.sat-ws.realpage.com/';
END
IF @ServerName IN ('RCPGBKDBSQL005A', 'RCPGBKDBSQL005B')
BEGIN
	SET @LoginURL = 'https://admin.ws.realpage.com/';
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
--LeaseLabs ROles and Rights

DECLARE @RightValue nvarchar(200),
		 @UserId bigint,
		 @Now datetime = GETDATE(),
		 @RightId int,
		 @RoleId INT,
		 @OrgPartyId int,
		 @RoleTypeId int,
		 @ProductId int =68,
		 @RoleName nvarchar(100),
		 @OrgVisibilityStatusId INT = 9,
		 @RightVisibilityStatusId INT = 9,
		 @StatusTypeId int=13,
		 @ServerName SYSNAME = @@SERVERNAME;

		DECLARE @TargetRoleValue TABLE (RoleName nvarchar(100))

		INSERT INTO @TargetRoleValue VALUES ('Developer'),('Implementation'),('Product'),
										('SEO');

	
			--UserId
			SELECT	@UserId = UserId
			FROM	Ident.UserLogin
			WHERE	LoginName LIKE 'realpagead@%'
        SELECT @OrgPartyId=PartyId FROM Enterprise.Organization WHERE [Name]='Realpage Employee'
	

				SELECT @RoleTypeId=RoleTypeId from [Security].RoleType WHERE [Value]='Product'
 
					--Cursor Mapping Role with Right
						DECLARE curCreateNewRole CURSOR FOR
						SELECT RoleName
						FROM @TargetRoleValue

						OPEN curCreateNewRole
						FETCH NEXT FROM curCreateNewRole INTO @RoleName

						WHILE @@FETCH_STATUS = 0
						BEGIN
							IF NOT EXISTS (SELECT TOP 1 1 FROM [Security].[Role] WHERE RoleName = @RoleName and OrgPartyID = @OrgPartyId and ProductId=@ProductId)
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
							IF NOT EXISTS (SELECT TOP 1 1 FROM [Security].[Right] WHERE [Value] = @RoleName and VisibilityStatusId = 9 and ProductId=@ProductId)
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

GO

-- Create manage LeaseLabs right access
DECLARE @UserId bigint,
	@Now datetime = GETDATE(),
	@RightId int;
	
SELECT	@UserId = UserId
FROM	Ident.UserLogin
WHERE	LoginName LIKE 'realpagead@%'

IF NOT EXISTS(select 1 from [Security].[Right] where RightName='ManageLeaseLabsProductAccess')
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
					'ManageLeaseLabsProductAccess',
					'Manage LeaseLabs Product Access',
					'Manage LeaseLabs Product Access',
					13, 
					9,
					3,
					68,
					@UserId,
					@Now
				   )

				
END
 SELECT @RightId=RightId from [Security].[Right] WHERE RightName='ManageLeaseLabsProductAccess'

	IF NOT EXISTS (SELECT TOP 1 1 FROM [Security].[RoleRight] WHERE RoleId = 1 AND RightId=@RightId)
	BEGIN
		INSERT INTO [Security].[RoleRight]
		(	RoleId,
			RightId, 
			CreatedBy,
			CreatedDate
		)
		VALUES ( 
				1,
				@RightId,
				@UserId,
				@Now
				)
	END;
GO

--LeaseLabs Product Panel Script
DECLARE @MaxControlId INT, 
		@UserId bigint,
		@Now datetime = GETDATE(), 
		@MaxControlAttributeId INT, 
		@MaxProductPageId INT,
		@MaxProductPageControlId INT,
		@ProductId INT = 68;
SELECT	@UserId = UserId
FROM	Ident.UserLogin
WHERE	LoginName LIKE 'realpagead@%'
IF NOT EXISTS (SELECT TOP 1 1 FROM[UserManagement].[ProductPage] WHERE ProductId = @ProductId)
BEGIN
	SET IDENTITY_INSERT [UserManagement].[Control] ON 
	SELECT @MaxControlId = max(ControlId) from UserManagement.Control
	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate]) 
	VALUES (@MaxControlId+1, NULL, 8, N'LeaseLabsProductAccessTabGroupUIId', NULL, NULL, 1, @UserId, @Now)
	
	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate]) 
	VALUES (@MaxControlId+2, @MaxControlId+1, 9, N'LeaseLabsProductAccessRolesTabUIId', N'Roles', NULL, 1, @UserId, @Now)
	
	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate]) 
	VALUES (@MaxControlId+3, @MaxControlId+2, 2, N'LeaseLabsProductAccessRolesSelectGridUIId', NULL, NULL, 2, @UserId, @Now)
	
	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate]) 
	VALUES (@MaxControlId+4, @MaxControlId+3, 7, N'LeaseLabsProductAccessRadioUIId', NULL, N'isAssigned', 1, @UserId, @Now)
	
	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate]) 
	VALUES (@MaxControlId+5, @MaxControlId+3, 5, N'LeaseLabsProductAccessRoleLabelUIId', N'Role', N'name', 2, @UserId, @Now)
	
	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate]) 
	VALUES (@MaxControlId+6, @MaxControlId+3, 5, N'LeaseLabsProductAccessRoleTypeLabelUIId', N'Role Type', N'roletype', 3, @UserId, @Now)
	
	SET IDENTITY_INSERT [UserManagement].[Control] OFF
	SET IDENTITY_INSERT [UserManagement].[ControlAttribute] ON
	 
	SELECT @MaxControlAttributeId = max(ControlAttributeId) from [UserManagement].[ControlAttribute]

	INSERT [UserManagement].[ControlAttribute] ([ControlAttributeId], [ControlId], [Key], [Value], [CreatedBy], [CreatedDate]) 
	VALUES (@MaxControlAttributeId + 1, @MaxControlId + 2, N'Default', N'True', @UserId, @Now)
	INSERT [UserManagement].[ControlAttribute] ([ControlAttributeId], [ControlId], [Key], [Value], [CreatedBy], [CreatedDate]) 
	VALUES (@MaxControlAttributeId + 2, @MaxControlId + 3, N'ShowSelectAll', N'False', @UserId, @Now)
	SET IDENTITY_INSERT [UserManagement].[ControlAttribute] OFF

	SELECT @MaxProductPageId=max(ProductPageId) from [UserManagement].[ProductPage]
	SET IDENTITY_INSERT [UserManagement].[ProductPage] ON 

	INSERT [UserManagement].[ProductPage] ([ProductPageId], [ProductId], [DisplayName], [CreatedBy], [CreatedDate], [IsActive], [ProductPageTypeId]) VALUES 
	(@MaxProductPageId + 1, @ProductId, N'LeaseLabs Product Access', @UserId, @Now, 1, 1)
	SET IDENTITY_INSERT [UserManagement].[ProductPage] OFF

	SELECT @MaxProductPageControlId=max(ProductPageControlId) from [UserManagement].[ProductPageControl]

	SET IDENTITY_INSERT [UserManagement].[ProductPageControl] ON 
	INSERT [UserManagement].[ProductPageControl] ([ProductPageControlId], [ProductPageId], [ControlId], [CreatedBy], [CreatedDate]) 
	VALUES (@MaxProductPageControlId + 1, @MaxProductPageId + 1, @MaxControlId + 1, @UserId, @Now)
	SET IDENTITY_INSERT [UserManagement].[ProductPageControl] OFF
End
GO

--ADD RIGHT - AbilityToAddProducts
GO
DECLARE @CreatedById bigint,
		@RouteId bigint,
		@RightId bigint,
		@Now datetime = GETDATE(),
		@PartyId bigint,
		@RoleId bigint

SELECT @CreatedById = UserId
FROM Ident.UserLogin
WHERE LoginName like 'realpagead@%'

IF NOT EXISTS (SELECT 1 FROM [Security].[Right] WHERE RightName = 'AbilityToAddProducts')
BEGIN
	INSERT INTO [Security].[right](RightName, [Description], [Value], [StatusTypeId], [VisibilityStatusId], [ProductId], [TargetProductId], CreatedBy, CreatedDate)
	VALUES ('AbilityToAddProducts', 'Allow an authorized RealPage employee the ability to add products to Unified Platform','Ability to add products to Unified Platform', 13,10, 3, 3, @CreatedById, @Now)	
END

--RightRoute
SELECT @RightId = RightId
FROM [Security].[Right]
WHERE RightName = 'AbilityToAddProducts'

SELECT @RouteId = RouteId
FROM [Security].[Route]
WHERE RouteValue = 'SideMenu'

IF NOT EXISTS (SELECT 1 FROM [Security].[RightRoute] WHERE RightId = @RightId AND RouteId = @RouteId)
BEGIN
	INSERT INTO [Security].[RightRoute] (RightId, RouteId, RightName, CreatedBy, CreatedDate)
	VALUES (@RightId, @RouteId, 'Ability To Add Products', @CreatedById, @Now)
END
--RoleRight
SELECT @RoleId = RoleId 
FROM [Security].[Role]
WHERE RoleName = 'User Administrator' AND ShortName = 'SuperUser'

IF NOT EXISTS (SELECT 1 FROM [Security].[RoleRight] WHERE RoleId = @RoleId AND RightId = @RightId)
BEGIN
	INSERT INTO [Security].[RoleRight] (RoleId, RightId, CreatedBy, CreatedDate)
	VALUES	(@RoleId, @RightId, @CreatedById, @Now)
END

--OrganizationOverRideRight
SELECT @PartyId = PartyId
FROM [Enterprise].[Organization] 
WHERE [Name] = 'RealPage Employee'

IF NOT EXISTS (SELECT 1 FROM [Security].[OrganizationOverRideRight]  WHERE RightId = @RightId AND OrgPartyId = @PartyId)
BEGIN
	INSERT INTO [Security].[OrganizationOverRideRight](RightId, OrgPartyId, VisibilityStatusId, CreatedBy, CreatedDate) 
	VALUES	(@RightId, @PartyId, 9, @CreatedById, @Now)
END
GO

-- Updating the Unified Amenities roles to be sentence-cased
IF EXISTS(SELECT 1 FROM Security.Role WHERE ProductId = 26 AND RoleName='Manage Amenity Status')
BEGIN
UPDATE Security.Role SET RoleName='Manage amenity status'
WHERE ProductId = 26 AND RoleName='Manage Amenity Status'
END

IF EXISTS(SELECT 1 FROM Security.Role WHERE ProductId = 26 AND RoleName='Manage Amenity No Pricing') 
BEGIN
UPDATE Security.Role SET RoleName='Manage amenity no pricing'
WHERE ProductId = 26 AND RoleName='Manage Amenity No Pricing'
END

IF EXISTS(SELECT 1 FROM Security.Role WHERE ProductId = 26 AND RoleName='Manage Amenity With Pricing')
BEGIN
UPDATE Security.Role SET RoleName='Manage amenity with pricing'
WHERE ProductId = 26 AND RoleName='Manage Amenity With Pricing'
END

IF EXISTS(SELECT 1 FROM Security.Role WHERE ProductId = 26 AND RoleName='Manage Property Amenity No Pricing')
BEGIN
UPDATE Security.Role SET RoleName='Manage property amenity no pricing'
WHERE ProductId = 26 AND RoleName='Manage Property Amenity No Pricing'
END

IF EXISTS(SELECT 1 FROM Security.Role WHERE ProductId = 26 AND RoleName='Manage Property Amenity With Pricing')
BEGIN
UPDATE Security.Role SET RoleName='Manage property amenity with pricing'
WHERE ProductId = 26 AND RoleName='Manage Property Amenity With Pricing'
END

IF EXISTS(SELECT 1 FROM Security.Role WHERE ProductId = 26 AND RoleName='View Amenities')
BEGIN
UPDATE Security.Role SET RoleName='View amenities'
WHERE ProductId = 26 AND RoleName='View Amenities'
END

IF NOT EXISTS (SELECT TOP 1 1 FROM [UserManagement].[Control] 
WHERE UIId = 'UnifiedPlatformRolesAndRightsRightLabelTypeUIId')
BEGIN
	DECLARE @ParentControlId INT, @UserId BIGINT, @Now DATETIME = GETDATE();

	SELECT	@UserId = UserId
	FROM	Ident.UserLogin
	WHERE	LoginName LIKE 'realpagead@%'

	SELECT @ParentControlId = ParentControlId FROM [UserManagement].[Control] 
	WHERE UIId = 'UnifiedPlatformRolesAndRightsRightLabelUIId' AND DataSource = 'name';

	INSERT INTO [UserManagement].[Control](ParentControlId,ControlTypeId,UIId,DisplayName,DataSource,Sequence,CreatedBy,CreatedDate)
	VALUES(@ParentControlId,5,'UnifiedPlatformRolesAndRightsRightLabelTypeUIId', 'Type','roletype',3,@UserId,@Now);
END

GO
--Renaming Right
IF EXISTS(SELECT TOP 1 1 FROM [Security].[Right] where Value ='View all company-level settings')
BEGIN
   DECLARE @RightID INT;
   SELECT @RightID = RightId from [Security].[Right] where Value ='View all company-level settings'
   UPDATE [Security].[Right] SET Value='View all company-level settings & templates' where RightId=@RightID
END
GO

IF EXISTS(SELECT TOP 1 1 FROM [Security].[Right] where Value ='Access to Company-level questionnaires and Summary Views in CIMPL')
BEGIN
   DECLARE @RightID INT;
   SELECT @RightID = RightId from [Security].[Right] where Value ='Access to Company-level questionnaires and Summary Views in CIMPL'
   UPDATE [Security].[Right] SET Value='Access to Company-level questionnaires and Portfolio Views in CIMPL' where RightId=@RightID
END
GO

DECLARE @ProductSettingTypeId INT
select @ProductSettingTypeId = ProductSettingTypeId from Enterprise.ProductSettingType where Name='IsNewTab'

IF EXISTS ( select TOP 1 1 from Enterprise.ProductSetting where ProductId = 26 and  ProductSettingTypeId = @ProductSettingTypeId and Value = 0)
BEGIN
 UPDATE Enterprise.ProductSetting SET Value = 1 where ProductId = 26 and  ProductSettingTypeId = @ProductSettingTypeId
END
GO

---Script to add SettingsApiEndPoint configuration
DECLARE @LoginURL NVARCHAR(500) = 'https://settingsapi-dev.realpage.com',
@ServerName SYSNAME = @@SERVERNAME
IF @ServerName IN ('RCDUSODBSQL001') --DEV
BEGIN
	SET @LoginURL = 'https://settingsapi-dev.realpage.com';
END
IF @ServerName IN ('RCTUSODBSQL001') --QA
BEGIN
	SET @LoginURL = 'https://settingsapi-qa.realpage.com';
END
IF @ServerName IN ('RCQUSODBSQL001') --SAT
BEGIN
	SET @LoginURL = 'https://settingsapi-sat.realpage.com';
END
IF @ServerName IN ('RCTUSODBSQL001A','RCTUSODBSQL001B') --UAT
BEGIN
	SET @LoginURL = 'https://settingsapi-uat.realpage.com';
END
IF @ServerName IN ('RCVGBKDBSQL001') --DEMO
BEGIN
	SET @LoginURL = 'https://settingsapi-demo.realpage.com';
END
IF @ServerName IN ('RCTUSODBTUL001') --TRAINING
BEGIN
	SET @LoginURL = 'https://settingsapi-training.realpage.com';
END
IF @ServerName IN ('RCIUSODBSQL002') --PREPROD
BEGIN
	SET @LoginURL = 'https://settingsapi-preprod.realpage.com';
END
IF @ServerName IN ('RCPGBKDBSQL005A', 'RCPGBKDBSQL005B') --PROD
BEGIN
	SET @LoginURL = 'https://settingsapi.realpage.com';
END

IF NOT EXISTS ( select top (1) 1 from Enterprise.ProductSettingType where name = 'SettingsApiEndPoint')
BEGIN
	INSERT INTO Enterprise.ProductSettingType ( name, Description, SensitiveData ) values ( 'SettingsApiEndPoint', 'The api endpoint for Unified Settings', 0 )
END

IF NOT EXISTS(Select top (1) 1 from Enterprise.ProductSetting ps 
				inner join Enterprise.ProductSettingType pst
				on ps.ProductSettingTypeId = pst.ProductSettingTypeId
				where pst.Name = 'SettingsApiEndPoint' and ps.ProductId= 3)
BEGIN
	Insert into Enterprise.ProductSetting (ProductId, ProductSettingTypeId, Value, FromDate)
	Select 3, ProductSettingTypeId, @LoginURL, GETUTCDATE()
	from Enterprise.ProductSettingType
	where Name = 'SettingsApiEndPoint'

	declare @productsettingid int
	select @productsettingid = productsettingid from Enterprise.ProductSetting ps 
				inner join Enterprise.ProductSettingType pst
				on ps.ProductSettingTypeId = pst.ProductSettingTypeId
				where pst.Name = 'SettingsApiEndPoint' and ps.ProductId= 3

	insert into enterprise.ProductConfiguration ( ConfigurationId, ProductSettingId, FromDate )
				select TOP (1) ConfigurationId, @productsettingid, GETUTCDATE() from enterprise.GlobalProductConfiguration where productid = 3 and thrudate is NULL ORDER BY GlobalProductConfigurationId DESC
END
GO

---- 679135
--UPDATE enterprise.product SET Description = 'Axiometrics is the industry leader in providing multifamily and student housing data at individual properties down to the floorplan level. Covering markets of all sizes across the country, Axiometrics delivers detailed reports and analytics on tens of thousands of assets every month.' WHERE productid = 33
--GO

--UPDATE ps
--SET ps.Value = 'https://www.realpage.com/asset-optimization/market-analytics/'
--FROM enterprise.ProductSetting ps 
--INNER JOIN enterprise.ProductSettingType pst ON pst.ProductSettingTypeId = ps.ProductSettingTypeId 
--WHERE ProductId = 33 AND pst.Name = 'LearnMore'
---- 679135

--GO

-- ILMLM/ILMLA Authentication for GB API
DECLARE @ServerName SYSNAME = @@SERVERNAME
IF @ServerName IN ('RCDUSODBSQL001','RCTUSODBSQL001') --DEV And QA
BEGIN
	IF NOT EXISTS (SELECT 1 FROM Enterprise.ProductSetting WHERE ProductId IN (40,41) AND ProductSettingTypeId = 1010)
	BEGIN
		INSERT INTO Enterprise.ProductSetting VALUES(40,1010,'dW5pZmllZC1sb2dpbkByZWFscGFnZS5jb20=',GETDATE(),NULL)
		INSERT INTO Enterprise.ProductSetting VALUES(41,1010,'dW5pZmllZC1sb2dpbkByZWFscGFnZS5jb20=',GETDATE(),NULL)
	END	

	IF NOT EXISTS (SELECT 1 FROM Enterprise.ProductSetting WHERE ProductId IN (40,41) AND ProductSettingTypeId = 1011)
	BEGIN
		INSERT INTO Enterprise.ProductSetting VALUES(40,1011,'WHZkanhYV01DT2Y1akZ6NA==',GETDATE(),NULL)
		INSERT INTO Enterprise.ProductSetting VALUES(41,1011,'WHZkanhYV01DT2Y1akZ6NA==',GETDATE(),NULL)
	END			

END
IF @ServerName IN ('RCQUSODBSQL001') --SAT
BEGIN
	IF NOT EXISTS (SELECT 1 FROM Enterprise.ProductSetting WHERE ProductId IN (40,41) AND ProductSettingTypeId = 1010)
	BEGIN
		INSERT INTO Enterprise.ProductSetting VALUES(40,1010,'dW5pZmllZC1sb2dpbkByZWFscGFnZS5jb20=',GETDATE(),NULL)
		INSERT INTO Enterprise.ProductSetting VALUES(41,1010,'dW5pZmllZC1sb2dpbkByZWFscGFnZS5jb20=',GETDATE(),NULL)
	END	

	IF NOT EXISTS (SELECT 1 FROM Enterprise.ProductSetting WHERE ProductId IN (40,41) AND ProductSettingTypeId = 1011)
	BEGIN
		INSERT INTO Enterprise.ProductSetting VALUES(40,1011,'ZVVjVXR5YlI3eXlWRmpJRA==',GETDATE(),NULL)
		INSERT INTO Enterprise.ProductSetting VALUES(41,1011,'ZVVjVXR5YlI3eXlWRmpJRA==',GETDATE(),NULL)
	END	
END
IF @ServerName IN ('RCPGBKDBSQL005A', 'RCPGBKDBSQL005B') --PROD
BEGIN
	IF NOT EXISTS (SELECT 1 FROM Enterprise.ProductSetting WHERE ProductId IN (40,41) AND ProductSettingTypeId = 1010)
	BEGIN
		INSERT INTO Enterprise.ProductSetting VALUES(40,1010,'dW5pZmllZC1sb2dpbkByZWFscGFnZS5jb20=',GETDATE(),NULL)
		INSERT INTO Enterprise.ProductSetting VALUES(41,1010,'dW5pZmllZC1sb2dpbkByZWFscGFnZS5jb20=',GETDATE(),NULL)
	END	

	IF NOT EXISTS (SELECT 1 FROM Enterprise.ProductSetting WHERE ProductId IN (40,41) AND ProductSettingTypeId = 1011)
	BEGIN
		INSERT INTO Enterprise.ProductSetting VALUES(40,1011,'ZUpNSFlpeHgzMXk0dTBnUQ==',GETDATE(),NULL)
		INSERT INTO Enterprise.ProductSetting VALUES(41,1011,'ZUpNSFlpeHgzMXk0dTBnUQ==',GETDATE(),NULL)
	END
END

IF EXISTS (SELECT 1 ProductSettingId FROM Enterprise.ProductSetting WHERE ProductId = 40 and ProductSettingTypeId = 1010)
BEGIN
INSERT INTO Enterprise.ProductConfiguration(ConfigurationId, ProductSettingId, FromDate, ThruDate) VALUES (
(SELECT TOP 1 ConfigurationId FROM Enterprise.GlobalProductConfiguration WHERE ProductId = 40 AND ThruDate IS NULL),
(SELECT TOP 1 ProductSettingId FROM Enterprise.ProductSetting WHERE ProductId = 40 and ProductSettingTypeId = 1010),
GETDATE(),NULL)
END

IF EXISTS (SELECT 1 ProductSettingId FROM Enterprise.ProductSetting WHERE ProductId = 40 and ProductSettingTypeId = 1011)
BEGIN
INSERT INTO Enterprise.ProductConfiguration(ConfigurationId, ProductSettingId, FromDate, ThruDate) VALUES (
(SELECT TOP 1 ConfigurationId FROM Enterprise.GlobalProductConfiguration WHERE ProductId = 40 AND ThruDate IS NULL),
(SELECT ProductSettingId FROM Enterprise.ProductSetting WHERE ProductId = 40 and ProductSettingTypeId = 1011),
GETDATE(),NULL)
END

IF EXISTS (SELECT 1 ProductSettingId FROM Enterprise.ProductSetting WHERE ProductId = 41 and ProductSettingTypeId = 1010)
BEGIN
INSERT INTO Enterprise.ProductConfiguration(ConfigurationId, ProductSettingId, FromDate, ThruDate) VALUES (
(SELECT TOP 1 ConfigurationId FROM Enterprise.GlobalProductConfiguration WHERE ProductId = 41 AND ThruDate IS NULL),
(SELECT ProductSettingId FROM Enterprise.ProductSetting WHERE ProductId = 41 and ProductSettingTypeId = 1010),
GETDATE(),NULL)
END

IF EXISTS (SELECT 1 ProductSettingId FROM Enterprise.ProductSetting WHERE ProductId = 41 and ProductSettingTypeId = 1011)
BEGIN
INSERT INTO Enterprise.ProductConfiguration(ConfigurationId, ProductSettingId, FromDate, ThruDate) VALUES (
(SELECT TOP 1 ConfigurationId FROM Enterprise.GlobalProductConfiguration WHERE ProductId = 41 AND ThruDate IS NULL),
(SELECT ProductSettingId FROM Enterprise.ProductSetting WHERE ProductId = 41 and ProductSettingTypeId = 1011),
GETDATE(),NULL)
END

GO
IF NOT EXISTS(SELECT TOP 1 1 FROM [Ident].[SettingCategoryType] Where Name = 'Security')
BEGIN
	INSERT INTO [Ident].[SettingCategoryType](Name)
	VALUES('Security')
END
GO

-- Updating DisplayName of UnifiedPlatformRolesAndRightsRightLabelUIId for name DataSource
GO
DECLARE @ControlId INT
SELECT @ControlId = ControlId FROM [UserManagement].[Control] WHERE UIId='UnifiedPlatformRolesAndRightsRightLabelUIId' AND DataSource='name'
IF (@ControlId <> '')
BEGIN
UPDATE [UserManagement].[Control] SET DisplayName='Role' WHERE ControlId = @ControlId
END
GO