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

--Userstory - 795122
-- Employee Access to Internal Client Settings
DECLARE @CreatedById bigint,
		@RouteId bigint,
		@RightId bigint,
		@Now datetime = GETDATE(),
		@PartyId bigint,
		@RoleId bigint

SELECT @CreatedById = UserId
FROM Ident.UserLogin
WHERE LoginName like 'realpagead@%'

IF NOT EXISTS (SELECT 1 FROM [Security].[Right] WHERE RightName = 'EmployeeAccessToInternalClientSettings')
BEGIN
	INSERT INTO [Security].[Right](	RightName,Description, Value,StatusTypeId,VisibilityStatusId,ProductId,TargetProductId,	CreatedBy,CreatedDate)
    VALUES ('EmployeeAccessToInternalClientSettings', 'Employee Access to Internal Client Settings','Employee Access to Internal Client Settings', 13,10, 3, 3, @CreatedById, @Now)
END

--RightRoute
SELECT @RightId = RightId
FROM [Security].[Right]
WHERE RightName = 'EmployeeAccessToInternalClientSettings'

SELECT @RouteId = RouteId
FROM [Security].[Route]
WHERE RouteValue = 'SideMenu'

IF NOT EXISTS (SELECT 1 FROM [Security].[RightRoute] WHERE RightId = @RightId AND RouteId = @RouteId)
BEGIN
	INSERT INTO [Security].[RightRoute] (RightId,RouteId,RightName,CreatedBy,CreatedDate)
	VALUES (@RightId, @RouteId, 'Employee Access to Internal Client Settings', @CreatedById, @Now)
END
--RoleRight
SELECT @RoleId = RoleId 
FROM [Security].[Role]
WHERE RoleName = 'User Administrator' AND ShortName = 'SuperUser' and OrgPartyID IS NULL

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

 declare @NavigationMenuId bigint, @previousRightId bigint
 select @previousRightId = RightId from Security.RightRoute  where RightName='Employee Access to Company Setup'
 select @NavigationMenuId=id from Enterprise.NavigationMenu where PageId='client-settings' and url='/home/client-settings'
 update Enterprise.NavigationMenuRights set rightid = @RightId where  navigationmenuid=@NavigationMenuId and RightId = @previousRightId
GO

IF NOT EXISTS (SELECT TOP 1 1 FROM Enterprise.ProductSettingType where Name = 'NotificationCategoryCode')
BEGIN
	INSERT INTO Enterprise.ProductSettingType (Name, Description, SensitiveData) values ('NotificationCategoryCode', 'Category code that needs to be passed to notifications', 0)
END

IF NOT EXISTS (SELECT TOP 1 1 FROM Enterprise.ProductSetting ps	
	JOIN Enterprise.ProductSettingType pst on pst.ProductSettingTypeId = ps.ProductSettingTypeId	
	WHERE ProductId = 3 	
	AND pst.Name = 'NotificationCategoryCode')
BEGIN 
	DECLARE @typeId int;
	SELECT @typeId = ProductSettingTypeId from Enterprise.ProductSettingType
	WHERE NAME = 'NotificationCategoryCode'
	
	EXEC Enterprise.SetProductSetting @ProductSettingId=0,  @ProductId =3,  @ProductSettingTypeId = @typeId,  @Value = 'ULUUS'
End
GO

DECLARE @PartyId INT
select @PartyId = PartyId from Enterprise.Organization where Name = 'RealPage Employee'

IF NOT EXISTS (select TOP 1 1 from Enterprise.PartyRole where PartyId = @PartyId AND RoleTypeId = 405)
BEGIN
 INSERT INTO Enterprise.PartyRole VALUES(@PartyId, 405)
END

GO