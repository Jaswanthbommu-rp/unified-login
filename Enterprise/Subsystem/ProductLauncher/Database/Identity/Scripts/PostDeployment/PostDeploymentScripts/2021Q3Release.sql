-- Navigation Menu data

SET IDENTITY_INSERT Enterprise.NavigationMenu ON;

MERGE INTO Enterprise.NavigationMenu t
	USING 
	(
		VALUES
			(8, N'Enterprise Roles', N'enterpriseRoles', NULL, N'/home/roles-rights/enterprise-roles', 80, 6, 'unified-login')
	) 
	AS 
	s (Id, Title, PageId, Icon, [URL], OrderIndex, ParentId, Origin) on t.Id = s.Id
	WHEN MATCHED THEN
		UPDATE SET Title = s.Title,
			PageId = s.PageId,
			Icon = s.Icon,
			[URL] = s.[URL],
			OrderIndex = s.OrderIndex,
			ParentId = s.ParentId,
			Origin = s.Origin
	WHEN NOT MATCHED BY TARGET THEN
		INSERT(Id, Title, PageId, Icon, [URL], OrderIndex, ParentId, Origin) VALUES (s.Id, s.Title, s.PageId, s.Icon, s.[URL], s.OrderIndex, s.ParentId, s.Origin)
;

SET IDENTITY_INSERT Enterprise.NavigationMenu OFF;
DECLARE @maxId int = (SELECT MAX(Id) FROM Enterprise.NavigationMenu);
DBCC CHECKIDENT ('Enterprise.NavigationMenu', RESEED, @maxId);

MERGE INTO Enterprise.NavigationMenuRights t
	USING 
	(
		SELECT 8 NavigationMenuId, RightId FROM [Security].[Right] WHERE RightName = 'ViewRoleRight'
	) 
	AS 
	s (NavigationMenuId, RightId) on t.NavigationMenuId = s.NavigationMenuId
		AND t.RightId = s.RightId
	WHEN NOT MATCHED BY TARGET THEN
		INSERT(NavigationMenuId, RightId) VALUES (s.NavigationMenuId, s.RightId);

GO

--Rename UsePrimaryProperties to 
UPDATE Enterprise.MasterSettingType  SET NAME='EnablePrimaryPropertiesAndEnterpriseRoles' WHERE name ='UsePrimaryProperties'
GO

-- Add user sync integration URL
IF NOT EXISTS (SELECT TOP 1 1 FROM Enterprise.ProductSettingType WHERE [Name] = 'UserSyncIntegrationURL')
BEGIN
	INSERT INTO Enterprise.ProductSettingType ([Name], [Description], SensitiveData)
	VALUES ('UserSyncIntegrationURL', 'The URL to fetch user data from when the UserSyncIntegrationMethod setting is set to Pull', 0);
END

-- Add user sync integration method setting
IF NOT EXISTS (SELECT TOP 1 1 FROM Enterprise.ProductSettingType WHERE [Name] = 'UserSyncIntegrationMethod')
BEGIN
	INSERT INTO Enterprise.ProductSettingType ([Name], [Description], SensitiveData)
	VALUES ('UserSyncIntegrationMethod', 'The method to use for syncing user data', 0);
END

GO
---Insert records to Enterpirse.ProductRule as part of Userstroy:840776

IF NOT EXISTS (SELECT 1 FROM Enterprise.ProductRuleType WHERE ProductRuleType = 'Product')
BEGIN
	INSERT INTO Enterprise.ProductRuleType(ProductRuleType, Description)
	VALUES( 'Product','Product')
END
IF NOT EXISTS (SELECT 1 FROM Enterprise.ProductRuleType WHERE ProductRuleType = 'Role')
BEGIN
	INSERT INTO Enterprise.ProductRuleType(ProductRuleType, Description)
	VALUES( 'Role','Role')
END
IF NOT EXISTS (SELECT 1 FROM Enterprise.ProductRuleType WHERE ProductRuleType = 'AccessType')
BEGIN
	INSERT INTO Enterprise.ProductRuleType(ProductRuleType, Description)
	VALUES( 'AccessType','AccessType')
END

DECLARE @ProductRuleAccessTypeId int, @UserId bigint,	
	@Now datetime = GETUTCDATE()	
select @ProductRuleAccessTypeId = productRuleTypeId from Enterprise.ProductRuleType where productRuleType = 'AccessType'

DECLARE @ProductRuleRoleTypeId int
select @ProductRuleRoleTypeId = productRuleTypeId from Enterprise.ProductRuleType where productRuleType = 'Role'

DECLARE @ProductRuleProductTypeId int
select @ProductRuleProductTypeId = productRuleTypeId from Enterprise.ProductRuleType where productRuleType = 'Product'

SELECT	@UserId = UserId
FROM	Ident.UserLogin
WHERE	LoginName LIKE 'realpagead@%'

IF NOT EXISTS(SELECT 1 FROM Enterprise.ProductValidationRule WHERE ProductId = 34 AND ProductRuleTypeId = @ProductRuleProductTypeId)
BEGIN
INSERT INTO Enterprise.ProductValidationRule(ProductId,ProductRuleTypeId,RuleValue,ValidationMessage,CreatedBy,CreatedDate)
SELECT 34, @ProductRuleProductTypeId, 30, 'Performance Analytics Role Required', @UserId, @Now
END
IF NOT EXISTS(SELECT 1 FROM Enterprise.ProductValidationRule WHERE ProductId = 3 AND ProductRuleTypeId = @ProductRuleRoleTypeId)
BEGIN
INSERT INTO Enterprise.ProductValidationRule(ProductId,ProductRuleTypeId,RuleValue,ValidationMessage,CreatedBy,CreatedDate)
SELECT 3, @ProductRuleRoleTypeId, 1, 'At least one role is required for Unified Platform', @UserId, @Now
END
IF NOT EXISTS(SELECT 1 FROM Enterprise.ProductValidationRule WHERE ProductId = 16 AND ProductRuleTypeId = @ProductRuleAccessTypeId)
BEGIN
INSERT INTO Enterprise.ProductValidationRule(ProductId,ProductRuleTypeId,RuleValue,ValidationMessage,CreatedBy,CreatedDate)
SELECT 16, @ProductRuleAccessTypeId, 1, 'Access type is required for Vendor Credentialing', @UserId, @Now
END
IF NOT EXISTS(SELECT 1 FROM Enterprise.ProductValidationRule WHERE ProductId = 18 AND ProductRuleTypeId = @ProductRuleAccessTypeId)
BEGIN
INSERT INTO Enterprise.ProductValidationRule(ProductId,ProductRuleTypeId,RuleValue,ValidationMessage,CreatedBy,CreatedDate)
SELECT 18, @ProductRuleAccessTypeId, 1, 'Access type is required for Utility Management', @UserId, @Now
END
IF NOT EXISTS(SELECT 1 FROM Enterprise.ProductValidationRule WHERE ProductId = 44 AND ProductRuleTypeId = @ProductRuleRoleTypeId)
BEGIN
INSERT INTO Enterprise.ProductValidationRule(ProductId,ProductRuleTypeId,RuleValue,ValidationMessage,CreatedBy,CreatedDate)
SELECT 44, @ProductRuleRoleTypeId, 1, 'At least one Entity role is required for Portfolio Management', @UserId, @Now
END

GO

-- Add the Employee Access to Login Page Setup right
DECLARE @CreatedById bigint,
		@RouteId bigint,
		@RightId bigint,
		@Now datetime = GETDATE(),
		@PartyId bigint,
		@RoleId bigint

SELECT @CreatedById = UserId
FROM Ident.UserLogin
WHERE LoginName = 'RealPageAd@test.com'

IF NOT EXISTS (SELECT 1 FROM [Security].[Right] WHERE RightName = 'EmployeeAccessToLoginPageSetup')
BEGIN
	INSERT INTO [Security].[Right](	RightName,Description, Value,StatusTypeId,VisibilityStatusId,ProductId,TargetProductId,	CreatedBy,CreatedDate)
    VALUES ('EmployeeAccessToLoginPageSetup', 'Allow an authorized RealPage employee the ability to navigate to Login Page Setup','Employee Access to Login Page Setup', 13,10, 3, 3, @CreatedById, @Now)
END

--RightRoute
SELECT @RightId = RightId
FROM [Security].[Right]
WHERE RightName = 'EmployeeAccessToLoginPageSetup'

SELECT @RouteId = RouteId
FROM [Security].[Route]
WHERE RouteValue = 'SideMenu'

IF NOT EXISTS (SELECT 1 FROM [Security].[RightRoute] WHERE RightId = @RightId AND RouteId = @RouteId)
BEGIN
	INSERT INTO [Security].[RightRoute] (RightId,RouteId,RightName,CreatedBy,CreatedDate)
	VALUES (@RightId, @RouteId, 'Employee Access to Login Page Setup', @CreatedById, @Now)
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

-- Create side menu navigation entry for Login Page Setup

IF NOT EXISTS(SELECT TOP 1 1 FROM Enterprise.NavigationMenu WHERE PageId = 'login-page-setup')
BEGIN 
	BEGIN TRAN

	DECLARE @parentId int;
	SELECT TOP 1 @parentId = Id FROM Enterprise.NavigationMenu WHERE PageId = N'Configurations';

	DECLARE @menuEntryId int;
	INSERT INTO Enterprise.NavigationMenu(Title, PageId, Icon, [URL], OrderIndex, ParentId, Origin)
	VALUES (N'Login Page Setup', N'login-page-setup', NULL, '/home/login-page-setup', 141, @parentId, 'unified-login');

	SET @menuEntryId = SCOPE_IDENTITY();

	INSERT INTO Enterprise.NavigationMenuRights(NavigationMenuId, RightId)
	SELECT @menuEntryId, RightId FROM [Security].[Right] WHERE RightName = 'EmployeeAccessToLoginPageSetup'

	COMMIT TRAN
END

GO
 --AAdding Role for System Admin for RUM Product
			   
  BEGIN TRAN

-- Add ProductIcon product settings

IF NOT EXISTS (SELECT TOP 1 1 FROM Enterprise.ProductSettingType WHERE [Name] = 'UtilitySuperuser')
BEGIN
	INSERT INTO Enterprise.ProductSettingType ([Name], [Description], SensitiveData)
	VALUES ('UtilitySuperUser', 'Role for System Admin for Utility Management ', 0);
END

DECLARE @NOW DATETIME = GETUTCDATE();
declare @productlist table ( entid int identity, productid int, productsettingtype varchar(500), productsettingvalue varchar(100))
insert into @productlist values
(18,  'UtilitySuperUser','UtilitySuperuser');


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

COMMIT TRAN;
GO

  
IF NOT EXISTS (SELECT TOP (1) 1 FROM Enterprise.ProductSettingType WHERE Name = 'DirectUDMTranslateProperty' )
BEGIN
    INSERT INTO Enterprise.ProductSettingType
    (
        Name,
        Description,
        SensitiveData
    )
    VALUES
    (   N'DirectUDMTranslateProperty',    -- Name - nvarchar(50)
        'Should the product use direct translation when getting property data from UDM',   -- Description - nvarchar(100)
        0 -- SensitiveData - tinyint
    )

END
GO

-- CIMPL

DECLARE @NOW DATETIME = GETUTCDATE()

if NOT EXISTS (
	select TOP (1) 1 
		FROM Enterprise.GlobalProductConfiguration gpc  
		JOIN Enterprise.ProductConfiguration pc ON pc.ConfigurationId = gpc.ConfigurationId  
		JOIN Enterprise.ProductSetting ps ON ps.ProductSettingId = pc.ProductSettingId  
		JOIN Enterprise.ProductSettingType pst ON pst.ProductSettingTypeId = ps.ProductSettingTypeId  
			WHERE  gpc.ProductId = 45  
		AND ((@NOW BETWEEN gpc.FromDate AND gpc.ThruDate) OR (@NOW >= gpc.FromDate AND gpc.ThruDate IS NULL))  
		AND ((@NOW BETWEEN pc.FromDate AND pc.ThruDate) OR (@NOW >= pc.FromDate AND pc.ThruDate IS NULL))  
		AND ((@NOW BETWEEN ps.FromDate AND ps.ThruDate) OR (@NOW >= ps.FromDate AND ps.ThruDate IS NULL))  
		AND pst.Name = 'DirectUDMTranslateProperty'
		AND ps.Value = '1'
	)
	BEGIN
		declare @currentproductconfigurationid INT
		select distinct TOP (1) @currentproductconfigurationid = pc.configurationid
			FROM Enterprise.GlobalProductConfiguration gpc  
			JOIN Enterprise.ProductConfiguration pc ON pc.ConfigurationId = gpc.ConfigurationId  
			JOIN Enterprise.ProductSetting ps ON ps.ProductSettingId = pc.ProductSettingId  
			JOIN Enterprise.ProductSettingType pst ON pst.ProductSettingTypeId = ps.ProductSettingTypeId  
				WHERE  gpc.ProductId = 45
			AND ((@NOW BETWEEN gpc.FromDate AND gpc.ThruDate) OR (@NOW >= gpc.FromDate AND gpc.ThruDate IS NULL))  
			AND ((@NOW BETWEEN pc.FromDate AND pc.ThruDate) OR (@NOW >= pc.FromDate AND pc.ThruDate IS NULL))  
			AND ((@NOW BETWEEN ps.FromDate AND ps.ThruDate) OR (@NOW >= ps.FromDate AND ps.ThruDate IS NULL))  
		order by pc.ConfigurationId DESC

		if (@currentproductconfigurationid is not null)
		begin
			insert into enterprise.ProductSetting ( productid, ProductSettingTypeId, value, FromDate )
				select 45, productsettingtypeid, '1', GETUTCDATE()
					from enterprise.ProductSettingType where name = 'DirectUDMTranslateProperty'
			insert into enterprise.ProductConfiguration ( ConfigurationId, ProductSettingId, FromDate, ThruDate )
				values ( @currentproductconfigurationid, SCOPE_IDENTITY(), GETUTCDATE(), null )
		end
	END

GO

-- SETTINGS
DECLARE @NOW DATETIME = GETUTCDATE()
if NOT EXISTS (
	select TOP (1) 1 
		FROM Enterprise.GlobalProductConfiguration gpc  
		JOIN Enterprise.ProductConfiguration pc ON pc.ConfigurationId = gpc.ConfigurationId  
		JOIN Enterprise.ProductSetting ps ON ps.ProductSettingId = pc.ProductSettingId  
		JOIN Enterprise.ProductSettingType pst ON pst.ProductSettingTypeId = ps.ProductSettingTypeId  
			WHERE  gpc.ProductId = 56  
		AND ((@NOW BETWEEN gpc.FromDate AND gpc.ThruDate) OR (@NOW >= gpc.FromDate AND gpc.ThruDate IS NULL))  
		AND ((@NOW BETWEEN pc.FromDate AND pc.ThruDate) OR (@NOW >= pc.FromDate AND pc.ThruDate IS NULL))  
		AND ((@NOW BETWEEN ps.FromDate AND ps.ThruDate) OR (@NOW >= ps.FromDate AND ps.ThruDate IS NULL))  
		AND pst.Name = 'DirectUDMTranslateProperty'
		AND ps.Value = '1'
	)
	BEGIN
		declare @currentproductconfigurationid INT
		select distinct TOP (1) @currentproductconfigurationid = pc.configurationid
			FROM Enterprise.GlobalProductConfiguration gpc  
			JOIN Enterprise.ProductConfiguration pc ON pc.ConfigurationId = gpc.ConfigurationId  
			JOIN Enterprise.ProductSetting ps ON ps.ProductSettingId = pc.ProductSettingId  
			JOIN Enterprise.ProductSettingType pst ON pst.ProductSettingTypeId = ps.ProductSettingTypeId  
				WHERE  gpc.ProductId = 56
			AND ((@NOW BETWEEN gpc.FromDate AND gpc.ThruDate) OR (@NOW >= gpc.FromDate AND gpc.ThruDate IS NULL))  
			AND ((@NOW BETWEEN pc.FromDate AND pc.ThruDate) OR (@NOW >= pc.FromDate AND pc.ThruDate IS NULL))  
			AND ((@NOW BETWEEN ps.FromDate AND ps.ThruDate) OR (@NOW >= ps.FromDate AND ps.ThruDate IS NULL))  
		order by pc.ConfigurationId DESC

		if (@currentproductconfigurationid is not null)
		begin
			insert into enterprise.ProductSetting ( productid, ProductSettingTypeId, value, FromDate )
				select 56, productsettingtypeid, '1', GETUTCDATE()
					from enterprise.ProductSettingType where name = 'DirectUDMTranslateProperty'
			insert into enterprise.ProductConfiguration ( ConfigurationId, ProductSettingId, FromDate, ThruDate )
				values ( @currentproductconfigurationid, SCOPE_IDENTITY(), GETUTCDATE(), null )
		end
	END
GO
 DECLARE @ProductsettingTypeId int;
 Select @ProductsettingTypeId= ProductSettingTypeId from Enterprise.ProductSettingTYpe where Name = 'UtilitySuperUser';

 IF EXISTS ( Select Top 1 1 from ENterprise.ProductSetting where ProductSettingTypeId = @ProductsettingTypeId and ProductId = 18)
 BEGIN
     Update ENterprise.ProductSetting Set Value ='Utility Superuser' where ProductSettingTypeId = @ProductsettingTypeId and ProductId =18;
 END
 Go
GO

  IF NOT EXISTS (SELECT 1 FROM [Batch].[BatchProcessType] WHERE Name = 'EnterpriseRoleCreateUpdateProductUser')
  BEGIN
	INSERT INTO [Batch].[BatchProcessType]
	SELECT 10,1,'Batch to create EnterpriseRole Create-Update User','EnterpriseRoleCreateUpdateProductUser'
  END
GO

-- Seed Status Types  --TODO:  Insert Categories and Classifications
INSERT INTO Enterprise.StatusType ([Name])
VALUES 
('Pending Sync'),
('Sync Successful'),
('Sync Failed')

-- Seed Product Centers
INSERT INTO [Enterprise].[ProductCenter] ([ProductCenterSourceId], [Name], [Source])
VALUES
( N'N', N'Concierge', N'AB' ), 
( N'Y', N'ActiveBuilding', N'AB' ), 
( N'bbqa_2020.8', N'bbqa_2020.8', N'AB' ), 
( N'2', N'Commercial', N'ACCT' ), 
( N'1', N'Property Management', N'ACCT' ), 
( N'AX', N'Axiometrics', N'AO' ), 
( N'BI', N'Business Intelligence', N'AO' ), 
( N'MA', N'Investment Analytics', N'AO' ), 
( N'PA', N'Performance Analytics', N'AO' ), 
( N'PO', N'YieldStar', N'AO' ), 
( N'RF', N'Revenue Forcaster', N'AO' ), 
( N'BBQA', N'BBQA', N'BBQA' ), 
( N'BBQA1', N'BBQA1', N'BBQA' ), 
( N'3', N'Lead2Lease', N'CIMPL' ), 
( N'4', N'Community Website', N'CIMPL' ), 
( N'5', N'Classifieds', N'CIMPL' ), 
( N'6', N'Syndication', N'CIMPL' ), 
( N'7', N'Online Renewals', N'CIMPL' ), 
( N'10', N'Online Leasing', N'CIMPL' ), 
( N'12', N'Smart Leasing Tablet', N'CIMPL' ), 
( N'13', N'Social', N'CIMPL' ), 
( N'14', N'Resident Portals (WelcomeHome)', N'CIMPL' ), 
( N'15', N'Senior Prospect Management', N'CIMPL' ), 
( N'17', N'Resident Portals (ActiveBuilding)', N'CIMPL' ), 
( N'18', N'Digital Marketing', N'CIMPL' ), 
( N'19', N'Resident Portals (ActiveBuilding Concierge)', N'CIMPL' ), 
( N'29', N'Interactive Site Map', N'CIMPL' ), 
( N'30', N'Corporate Website', N'CIMPL' ), 
( N'35', N'Ellipse', N'CIMPL' ), 
( N'36', N'Contact Center Leasing', N'CIMPL' ), 
( N'37', N'Contact Center Maintenance', N'CIMPL' ), 
( N'38', N'Contact Center Smart Answer Automation', N'CIMPL' ), 
( N'63', N'ILM Lead Manager', N'CIMPL' ), 
( N'64', N'ILM Leasing Analytics', N'CIMPL' ), 
( N'78', N'Relate 24/7', N'CIMPL' ), 
( N'80', N'Contact Center Message on Hold', N'CIMPL' ), 
( N'83', N'Copy Services', N'CIMPL' ), 
( N'85', N'MyNewPlace', N'CIMPL' ), 
( N'88', N'Check Scanner', N'CIMPL' ), 
( N'89', N'Payments', N'CIMPL' ), 
( N'101', N'YieldStar', N'CIMPL' ), 
( N'102', N'LRO', N'CIMPL' ), 
( N'1', N'Document Director', N'DOC' ), 
( N'1', N'EasyLMS', N'ELMS' ), 
( N'1', N'Hands On Training System', N'HOTS' ), 
( N'SMS-T', N'Intelligent Building Waste', N'IB' ), 
( N'SMS-W', N'Intelligent Building Water', N'IB' ), 
( N'LM', N'LeadMgmt', N'ILMLA' ), 
( N'LA', N'LeasingAnalytics', N'ILMLA' ), 
( N'1', N'Lead2Lease', N'L2L' ), 
( N'6164', N'LeaseStar Pricing and Availability', N'LS' ), 
( N'6133', N'LeaseStar Mobile Community Website', N'LS' ), 
( N'6244', N'Relate 24/7', N'LS' ), 
( N'6241', N'PropPhotos', N'LS' ), 
( N'6224', N'Interactive Siteplan', N'LS' ), 
( N'6243', N'Contact Center', N'LS' ), 
( N'6247', N'Self-Guided Tour', N'LS' ), 
( N'6242', N'Appointment Widget', N'LS' ), 
( N'6245', N'Virtual Tour Leasing', N'LS' ), 
( N'6246', N'Prospect Validation', N'LS' ), 
( N'6238', N'GoDirect', N'LS' ), 
( N'6231', N'RentJoy', N'LS' ), 
( N'6193', N'Quoting Preferences', N'LS' ), 
( N'6240', N'Email Marketing', N'LS' ), 
( N'6233', N'RentJungle', N'LS' ), 
( N'6239', N'Web2Print', N'LS' ), 
( N'6227', N'LeaseStar PSTO (RS)', N'LS' ), 
( N'6178', N'Places', N'LS' ), 
( N'6230', N'PropertyLinkOnline', N'LS' ), 
( N'6117', N'Lease Match Program', N'LS' ), 
( N'6129', N'LeaseStar SEO Option - Advanced', N'LS' ), 
( N'6236', N'Check Availability', N'LS' ), 
( N'6237', N'RealPage SEO/SEM', N'LS' ), 
( N'6235', N'Flex Demand Optimizer', N'LS' ), 
( N'6174', N'LeaseStar Community Websites -- Std ILF', N'LS' ), 
( N'6228', N'LeaseStar Campaign Full (RS)', N'LS' ), 
( N'6152', N'Campaign Service Ongoing Tune-Up', N'LS' ), 
( N'6171', N'Posting Suite Tool Only ILF', N'LS' ), 
( N'6121', N'Mobile Portal - Silver', N'LS' ), 
( N'6122', N'Mobile Portal - Gold', N'LS' ), 
( N'6135', N'LeaseStar SEO ongoing service (Standard)', N'LS' ), 
( N'6137', N'LeaseStar SEO ongoing service (Advanced)', N'LS' ), 
( N'6141', N'Marketing Kiosk', N'LS' ), 
( N'6126', N'LeaseStar Community Websites - Standard', N'LS' ), 
( N'6127', N'LeaseStar Community Websites - Advanced', N'LS' ), 
( N'6131', N'LeaseStar Community Website', N'LS' ), 
( N'6184', N'Leasing Tablet', N'LS' ), 
( N'6128', N'LeaseStar SEO Option - Standard', N'LS' ), 
( N'6157', N'1 to 5 Toll-Free Tracking Numbers', N'LS' ), 
( N'6147', N'Custom 3D Floor Plan Animated', N'LS' ), 
( N'6130', N'LeaseStar Syndication License Fee', N'LS' ), 
( N'6158', N'6 to 10 Toll-Free Tracking Numbers', N'LS' ), 
( N'6159', N'11 to 15 Toll-Free Tracking Numbers', N'LS' ), 
( N'6160', N'16 to 20 Toll-Free Tracking Numbers', N'LS' ), 
( N'6155', N'Lead2Lease Lead Management', N'LS' ), 
( N'6162', N'Yardi; MRI and AMSI Data Interfaces', N'LS' ), 
( N'6177', N'LeaseStar Campaign Service - LEGACY ILF', N'LS' ), 
( N'6167', N'Community Photography - Standard Shoot', N'LS' ), 
( N'6168', N'Community Photography - Twilight Shoot', N'LS' ), 
( N'6132', N'LeaseStar Community Website ILF', N'LS' ), 
( N'6134', N'LeaseStar Mobile Community Website ILF', N'LS' ), 
( N'6136', N'LeaseStar SEO ongoing service (Std) ILF', N'LS' ), 
( N'6138', N'LeaseStar SEO ongoing service (Adv) ILF', N'LS' ), 
( N'6139', N'Unique Theme', N'LS' ), 
( N'6140', N'Custom Website Design', N'LS' ), 
( N'6142', N'Marketing Kiosk ILF', N'LS' ), 
( N'6143', N'Corporate Design', N'LS' ), 
( N'6144', N'Gallery Theme Change Fee', N'LS' ), 
( N'6151', N'Campaign Service Setup Only', N'LS' ), 
( N'6153', N'Campaign Service Ongoing Tune-Up ILF', N'LS' ), 
( N'6154', N'Campaign Service Remediation', N'LS' ), 
( N'6189', N'RentSentinel Prospector', N'LS' ), 
( N'6170', N'LeaseStar Social ILF', N'LS' ), 
( N'6175', N'LeaseStar Community Websites SEO ILF', N'LS' ), 
( N'6176', N'LeaseStar SEO option -- Standard ILF', N'LS' ), 
( N'6145', N'Custom 3D Floor Plan', N'LS' ), 
( N'6146', N'Custom 3D Floor Plan Hi Res', N'LS' ), 
( N'6148', N'Custom 3D Floor Plan Animated ILF', N'LS' ), 
( N'6149', N'Custom 3D Floor Plan Change Fee', N'LS' ), 
( N'6150', N'2D CAD Floor Plan Drawing', N'LS' ), 
( N'6156', N'Lead2Lease Lead Management ILF', N'LS' ), 
( N'6161', N'eMail Campaigns', N'LS' ), 
( N'6163', N'Custom HTML Response Template', N'LS' ), 
( N'6165', N'LeaseStar Pricing and Availability ILF', N'LS' ), 
( N'6166', N'LeaseStar Pricing and Availability Cust', N'LS' ), 
( N'6190', N'RentSentinel Reputation Radar', N'LS' ), 
( N'6173', N'Community Search ILF', N'LS' ), 
( N'6181', N'Content Updater', N'LS' ), 
( N'6232', N'Reputation Management', N'LS' ), 
( N'6234', N'Surveys and Referrals', N'LS' ), 
( N'6182', N'Property Website - Legacy', N'LS' ), 
( N'6120', N'LeaseStar Posting Suite Tool Only', N'LS' ), 
( N'6114', N'Featured Property', N'LS' ), 
( N'6116', N'Flat Fee Program', N'LS' ), 
( N'6115', N'LeaseStar Classified Tool & Services', N'LS' ), 
( N'6123', N'Lease Match - Alt Lead Program', N'LS' ), 
( N'6110', N'Video Tour - Custom', N'LS' ), 
( N'6111', N'Video Tour - Standard', N'LS' ), 
( N'6112', N'Video Tour - Basic', N'LS' ), 
( N'6124', N'Lease Match - Alt Flat Fee Program', N'LS' ), 
( N'6194', N'MNP Premium Listing with Guarantee', N'LS' ), 
( N'6105', N'MyNewSite', N'LS' ), 
( N'6172', N'Community Search', N'LS' ), 
( N'6169', N'LeaseStar Social', N'LS' ), 
( N'6108', N'Featured Property - Top 50', N'LS' ), 
( N'6109', N'Featured Property - 2nd Tier', N'LS' ), 
( N'6101', N'MNP Listing - PFP', N'LS' ), 
( N'6102', N'MNP Listing - MLF', N'LS' ), 
( N'6103', N'MNP Listing - CPA', N'LS' ), 
( N'6104', N'Lead Program--', N'LS' ), 
( N'6125', N'LeaseStar Syndication', N'LS' ), 
( N'6113', N'MNP Listing - Subscription', N'LS' ), 
( N'6119', N'Lead Program', N'LS' ), 
( N'6191', N'Community Search 3rd Party', N'LS' ), 
( N'6192', N'Complimentary Listing', N'LS' ), 
( N'6185', N'Corporate Website', N'LS' ), 
( N'6225', N'LeaseStar SEM', N'LS' ), 
( N'6229', N'Online Leasing', N'LS' ), 
( N'6195', N'MNP Premium Lead Program', N'LS' ), 
( N'6118', N'Lease Match Program - Custom', N'LS' ), 
( N'6180', N'MyNewSite Setup', N'LS' ), 
( N'6183', N'Community Photography - Verified Shoot', N'LS' ), 
( N'6179', N'Places with Chat', N'LS' ), 
( N'6186', N'RentSentinel Marketer Prime', N'LS' ), 
( N'6187', N'RentSocial', N'LS' ), 
( N'6106', N'Photos - Standard', N'LS' ), 
( N'6107', N'Photos - Basic', N'LS' ), 
( N'6188', N'RentSentinel Marketer', N'LS' ), 
( N'3', N'Click2Chat', N'LVL1' ), 
( N'5', N'LeasingSuite', N'LVL1' ), 
( N'4', N'MessageOnHold', N'LVL1' ), 
( N'1', N'LeasingCalls', N'LVL1' ), 
( N'2', N'Maintenance', N'LVL1' ), 
( N'1', N'OPS', N'OPS' ), 
( N'OPSI', N'OpsInv', N'OPSI' ), 
( N'20', N'CrossFire Call Resident Support', N'OS' ), 
( N'71', N'Kiosk touchscreen with marketing functions only', N'OS' ), 
( N'77', N'Community Management', N'OS' ), 
( N'24', N'CrossFire Prospects', N'OS' ), 
( N'32', N'Concierge Services', N'OS' ), 
( N'44', N'CrossFire Leads Virtual Agent and auto-response system', N'OS' ), 
( N'3', N'Learning', N'OS' ), 
( N'43', N'Document management', N'OS' ), 
( N'48', N'CrossFire Leads inbound/outbound email and phone follow-up tracking', N'OS' ), 
( N'5', N'Privatized Military', N'OS' ), 
( N'51', N'Paperless Purchasing', N'OS' ), 
( N'68', N'Care Management', N'OS' ), 
( N'7', N'Rents', N'OS' ), 
( N'72', N'Affordable 50058 processing', N'OS' ), 
( N'49', N'Velocity Invoice Processing', N'OS' ), 
( N'50', N'Velocity Submetering', N'OS' ), 
( N'54', N'Insurance Services', N'OS' ), 
( N'29', N'Unified UI Alpha', N'OS' ), 
( N'40', N'Affordable Waitlist', N'OS' ), 
( N'42', N'Commercial', N'OS' ), 
( N'12', N'Leasing and Rents', N'OS' ), 
( N'25', N'CrossFire Residents', N'OS' ), 
( N'101', N'Online Leasing Workflow Platform - Esignature', N'OS' ), 
( N'100', N'Online Leasing Workflow Platform - Screening', N'OS' ), 
( N'14', N'Portfolio Access', N'OS' ), 
( N'1', N'Core setup and maintenance', N'OS' ), 
( N'16', N'CrossFire Pricing and Availability', N'OS' ), 
( N'19', N'Site Data Exchange', N'OS' ), 
( N'2', N'Applicant screening', N'OS' ), 
( N'21', N'Resident Access', N'OS' ), 
( N'22', N'Resident Awards', N'OS' ), 
( N'23', N'Central Reporting - Basic', N'OS' ), 
( N'26', N'Student living', N'OS' ), 
( N'33', N'Affordable CA Net', N'OS' ), 
( N'34', N'Affordable Tax Credits', N'OS' ), 
( N'35', N'Rent Stabilization', N'OS' ), 
( N'36', N'Velocity', N'OS' ), 
( N'37', N'Payments', N'OS' ), 
( N'38', N'CrossFire Online Leasing Web Service', N'OS' ), 
( N'4', N'Property', N'OS' ), 
( N'55', N'Crossfire Online Renewals', N'OS' ), 
( N'56', N'OneSite Budgeting', N'OS' ), 
( N'57', N'History Storage and Access', N'OS' ), 
( N'58', N'Crossfire OnDemand CallCenter', N'OS' ), 
( N'59', N'CrossFire Content Management', N'OS' ), 
( N'6', N'Affordable HUD', N'OS' ), 
( N'60', N'VES', N'OS' ), 
( N'62', N'ODM', N'OS' ), 
( N'63', N'OpsBuyer', N'OS' ), 
( N'65', N'CrossFire Callcenter Leasing', N'OS' ), 
( N'66', N'CrossFire Callcenter Service', N'OS' ), 
( N'67', N'CrossFire Callcenter Rollover', N'OS' ), 
( N'69', N'Mobile Prospect Portal', N'OS' ), 
( N'70', N'Mobile Resident Portal', N'OS' ), 
( N'73', N'Inventory management', N'OS' ), 
( N'75', N'Facilities mobile app', N'OS' ), 
( N'76', N'Level One Contact Center', N'OS' ), 
( N'78', N'Affordable Tax Credit Basic', N'OS' ), 
( N'79', N'Velocity Advanced Energy Reporting', N'OS' ), 
( N'8', N'Facilities', N'OS' ), 
( N'80', N'Facilities Premium Mobile', N'OS' ), 
( N'81', N'Facilities Inspections', N'OS' ), 
( N'84', N'Vendor Management', N'OS' ), 
( N'86', N'LeaseStar', N'OS' ), 
( N'87', N'Facilities Turn Caddy', N'OS' ), 
( N'88', N'Welcome Home', N'OS' ), 
( N'9', N'Accounting & budgeting', N'OS' ), 
( N'90', N'OnlineLeasing Basic', N'OS' ), 
( N'91', N'OnlineLeasing Basic Plus', N'OS' ), 
( N'92', N'OnlineLeasing E-signature', N'OS' ), 
( N'93', N'Welcome Home Widgets', N'OS' ), 
( N'17', N'Revenue Management', N'OS' ), 
( N'46', N'BETA version of CrossFire Email Campaigns feature', N'OS' ), 
( N'61', N'Military Managed Housing', N'OS' ), 
( N'64', N'CrossFire Ad Source Tracking', N'OS' ), 
( N'83', N'Facilities Workforce Management', N'OS' ), 
( N'85', N'eMAR', N'OS' ), 
( N'94', N'Property Content & Unit Availability for Zillow Export', N'OS' ), 
( N'95', N'Welcome Home Affordable', N'OS' ), 
( N'96', N'RPX Third Party Pricing', N'OS' ), 
( N'99', N'Online Leasing Workflow Platform - Search', N'OS' ), 
( N'28', N'Central Reporting - Advanced', N'OS' ), 
( N'41', N'Affordable Rural Housing', N'OS' ), 
( N'47', N'CrossFire Leads reporting', N'OS' ), 
( N'13', N'CrossFire Call Center', N'OS' ), 
( N'10', N'Purchasing', N'OS' ), 
( N'11', N'CrossFire Online Service Requests', N'OS' ), 
( N'45', N'Basic Subscription', N'PP' ), 
( N'52', N'Enterprise Subscription', N'PP' ), 
( N'48', N'Premium Subscription', N'PP' ), 
( N'1', N'Email', N'R247' ), 
( N'2', N'Text', N'R247' ), 
( N'1', N'Renovation Manager', N'RENO' ), 
( N'37', N'Property Photos', N'UPFM' ), 
( N'55', N'Renovation Manager', N'UPFM' ), 
( N'63', N'HOTS', N'UPFM' ), 
( N'12', N'Ops Bid', N'UPFM' ), 
( N'4', N'Asset Optimization', N'UPFM' ), 
( N'65', N'Self-Guided Tour', N'UPFM' ), 
( N'66', N'Market Analytics', N'UPFM' ), 
( N'29', N'Business Intelligence', N'UPFM' ), 
( N'31', N'Investment Analytics', N'UPFM' ), 
( N'30', N'Performance Analytics', N'UPFM' ), 
( N'54', N'Rent Control', N'UPFM' ), 
( N'32', N'YieldStar', N'UPFM' ), 
( N'53', N'AI Revenue Management', N'UPFM' ), 
( N'52', N'Amenity Optimization', N'UPFM' ), 
( N'33', N'Axiometrics', N'UPFM' ), 
( N'56', N'Unified Settings', N'UPFM' ), 
( N'2', N'UnifiedUI', N'UPFM' ), 
( N'3', N'Unified Platform', N'UPFM' ), 
( N'11', N'Social', N'UPFM' ), 
( N'24', N'Unified Data Management', N'UPFM' ), 
( N'35', N'Support Tool', N'UPFM' ), 
( N'42', N'SalesForce', N'UPFM' ), 
( N'43', N'Settings Management', N'UPFM' ), 
( N'46', N'Site Spend Management Portal', N'UPFM' ), 
( N'50', N'Senior Lead Management', N'UPFM' ), 
( N'51', N'LRO', N'UPFM' ), 
( N'58', N'Intelligent Building Energy', N'UPFM' ), 
( N'60', N'Home Sharing', N'UPFM' ), 
( N'62', N'PME Dashboard', N'UPFM' ), 
( N'67', N'Reporting', N'UPFM' ), 
( N'68', N'LeaseLabs', N'UPFM' ), 
( N'45', N'CIMPL', N'UPFM' ), 
( N'38', N'Vendor Marketplace', N'UPFM' ), 
( N'64', N'P2 Engagement Queue', N'UPFM' ), 
( N'21', N'L&R Conversion Utility', N'UPFM' ), 
( N'40', N'ILM Lead Management', N'UPFM' ), 
( N'59', N'Smart Water', N'UPFM' ), 
( N'34', N'Benchmarking', N'UPFM' ), 
( N'16', N'Vendor Credentialing', N'UPFM' ), 
( N'49', N'Simon Help Center', N'UPFM' ), 
( N'26', N'Unified Amenities', N'UPFM' ), 
( N'57', N'Waste Management Solution', N'UPFM' ), 
( N'10', N'Prospect Contact Center', N'UPFM' ), 
( N'13', N'Spend Management', N'UPFM' ), 
( N'17', N'Resident Portals', N'UPFM' ), 
( N'9', N'Marketing Center', N'UPFM' ), 
( N'8', N'Financial Suite', N'UPFM' ), 
( N'48', N'Payments', N'UPFM' ), 
( N'47', N'Deposit Alternative', N'UPFM' ), 
( N'20', N'Document Director', N'UPFM' ), 
( N'1', N'OneSite', N'UPFM' ), 
( N'6', N'Lead2Lease', N'UPFM' ), 
( N'15', N'Renters Insurance', N'UPFM' ), 
( N'18', N'Utility Management', N'UPFM' ), 
( N'36', N'EasyLMS', N'UPFM' ), 
( N'41', N'ILM Leasing Analytics', N'UPFM' ), 
( N'44', N'Portfolio Management', N'UPFM' ), 
( N'23', N'On-Site', N'UPFM' ), 
( N'39', N'Integration Marketplace', N'UPFM' ), 
( N'19', N'Product Learning Portal', N'UPFM' ), 
( N'27', N'Migration Tool Application', N'UPFM' ), 
( N'28', N'Product Updates', N'UPFM' ), 
( N'25', N'Self-provisioning portal', N'UPFM' ), 
( N'14', N'Client Portal', N'UPFM' )


--Point OS Product Centers to the OneSite Product ID
DECLARE @OneSiteProductId INT;
SELECT @OneSiteProductId = Productid FROM Enterprise.Product WHERE BooksProductCode = 'OS'
UPDATE [Enterprise].ProductCenter
SET ProductId = @OneSiteProductId
WHERE Source = 'OS'

--Point LS Product Centers to the Marketing Center Product ID
DECLARE @MCProductId INT;
SELECT @MCProductId = Productid FROM Enterprise.Product WHERE BooksProductCode = 'LS'
UPDATE [Enterprise].ProductCenter
SET ProductId = @MCProductId
WHERE Source = 'LS'