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
 
GO
  IF NOT EXISTS (SELECT 1 FROM [Batch].[BatchProcessType] WHERE Name = 'EnterpriseRoleCreateUpdateProductUser')
  BEGIN
	INSERT INTO [Batch].[BatchProcessType]
	SELECT 10,1,'Batch to create EnterpriseRole Create-Update User','EnterpriseRoleCreateUpdateProductUser'
  END
  IF NOT EXISTS (SELECT 1 FROM [Batch].[BatchProcessType] WHERE Name = 'EnterpriseRoleBulkUpdateProductUser')
  BEGIN
	INSERT INTO [Batch].[BatchProcessType]
	SELECT 11,1,'Batch to create EnterpriseRole Bulk Update Users','EnterpriseRoleBulkUpdateProductUser'
  END
  IF NOT EXISTS (SELECT 1 FROM [Batch].[BatchProcessType] WHERE Name = 'CreateEnterpriseRoleFromUserProduct')
  BEGIN
	INSERT INTO [Batch].[BatchProcessType]
	SELECT 12,1,'Batch to create EnterpriseRole based on User Products','CreateEnterpriseRoleFromUserProduct'
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


DECLARE @AdminUserId INT
SELECT @AdminUserId = UserId FROM Ident.Userlogin WHERE LoginName LIKE 'realpagead@%'

-- RIGHTS
IF NOT EXISTS ( SELECT TOP(1) 1 FROM Security.[Right] WHERE RightName = 'CustomerSupportManager' AND ProductId = 60 )
BEGIN
	INSERT INTO security.[Right] ( RightName, Description, Value, StatusTypeId, VisibilityStatusId, ProductId, TargetProductId, CreatedBy, CreatedDate )
	VALUES
		(   N'CustomerSupportManager', 'Customer Support Manager', 'Customer Support Manager', 13, 9, 60, 60, @AdminUserId, GETUTCDATE() )
END

IF NOT EXISTS ( SELECT TOP(1) 1 FROM Security.[Right] WHERE RightName = 'CustomerSupportRepresentative' AND ProductId = 60 )
BEGIN
	INSERT INTO security.[Right] ( RightName, Description, Value, StatusTypeId, VisibilityStatusId, ProductId, TargetProductId, CreatedBy, CreatedDate )
	VALUES
		(   N'CustomerSupportRepresentative', 'Customer Support Representative', 'Customer Support Representative', 13, 9, 60, 60, @AdminUserId, GETUTCDATE() )
END

IF NOT EXISTS ( SELECT TOP(1) 1 FROM Security.[Right] WHERE RightName = 'Implementations' AND ProductId = 60 )
BEGIN
	INSERT INTO security.[Right] ( RightName, Description, Value, StatusTypeId, VisibilityStatusId, ProductId, TargetProductId, CreatedBy, CreatedDate )
	VALUES
		(   N'Implementations', 'Implementations', 'Implementations', 13, 9, 60, 60, @AdminUserId, GETUTCDATE() )
END

IF NOT EXISTS ( SELECT TOP(1) 1 FROM Security.[Right] WHERE RightName = 'SystemsAdmin' AND ProductId = 60 )
BEGIN
	INSERT INTO security.[Right] ( RightName, Description, Value, StatusTypeId, VisibilityStatusId, ProductId, TargetProductId, CreatedBy, CreatedDate )
	VALUES
		(   N'SystemsAdmin', 'Systems Admin', 'Systems Admin', 13, 9, 60, 60, @AdminUserId, GETUTCDATE() )
END

-- ROLERIGHTS
IF EXISTS ( SELECT TOP(1) 1 FROM Security.Role r WHERE r.RoleName = 'Customer Support Manager' AND r.ProductId = 60 )
BEGIN
	IF NOT EXISTS ( SELECT TOP(1) 1 FROM Security.Role R INNER JOIN Security.RoleRight RR ON RR.RoleId = R.RoleId INNER JOIN Security.[Right] R2 ON R2.RightId = RR.RightId
		WHERE r.RoleName = 'Customer Support Manager' AND r2.RightName = 'CustomerSupportManager' AND r.ProductId = 60 AND r2.ProductId = 60 )
	BEGIN
		INSERT INTO Security.RoleRight (RoleId, RightId, CreatedBy, CreatedDate )
		SELECT R.RoleId, R2.RightId, @AdminUserId, GETUTCDATE() FROM Security.Role R CROSS JOIN Security.[Right] R2 
			WHERE r.RoleName = 'Customer Support Manager' AND R2.RightName = 'CustomerSupportManager' AND r.ProductId = 60 AND r2.ProductId = 60
	END
END

IF EXISTS ( SELECT TOP(1) 1 FROM Security.Role r WHERE r.RoleName = 'Customer Support Representative' AND r.ProductId = 60 )
BEGIN
	IF NOT EXISTS ( SELECT TOP(1) 1 FROM Security.Role R INNER JOIN Security.RoleRight RR ON RR.RoleId = R.RoleId INNER JOIN Security.[Right] R2 ON R2.RightId = RR.RightId
		WHERE r.RoleName = 'Customer Support Representative' AND r2.RightName = 'CustomerSupportRepresentative' AND r.ProductId = 60 AND r2.ProductId = 60 )
	BEGIN
		INSERT INTO Security.RoleRight (RoleId, RightId, CreatedBy, CreatedDate )
		SELECT R.RoleId, R2.RightId, @AdminUserId, GETUTCDATE() FROM Security.Role R CROSS JOIN Security.[Right] R2 
			WHERE r.RoleName = 'Customer Support Representative' AND R2.RightName = 'CustomerSupportRepresentative' AND r.ProductId = 60 AND r2.ProductId = 60
	END
END

IF EXISTS ( SELECT TOP(1) 1 FROM Security.Role r WHERE r.RoleName = 'Implementations' AND r.ProductId = 60 )
BEGIN
	IF NOT EXISTS ( SELECT TOP(1) 1 FROM Security.Role R INNER JOIN Security.RoleRight RR ON RR.RoleId = R.RoleId INNER JOIN Security.[Right] R2 ON R2.RightId = RR.RightId
		WHERE r.RoleName = 'Implementations' AND r2.RightName = 'Implementations' AND r.ProductId = 60 AND r2.ProductId = 60 )
	BEGIN
		INSERT INTO Security.RoleRight (RoleId, RightId, CreatedBy, CreatedDate )
		SELECT R.RoleId, R2.RightId, @AdminUserId, GETUTCDATE() FROM Security.Role R CROSS JOIN Security.[Right] R2 
			WHERE r.RoleName = 'Implementations' AND R2.RightName = 'Implementations' AND r.ProductId = 60 AND r2.ProductId = 60
	END
END

IF EXISTS ( SELECT TOP(1) 1 FROM Security.Role r WHERE r.RoleName = 'Systems Admin' AND r.ProductId = 60 )
BEGIN
	IF NOT EXISTS ( SELECT TOP(1) 1 FROM Security.Role R INNER JOIN Security.RoleRight RR ON RR.RoleId = R.RoleId INNER JOIN Security.[Right] R2 ON R2.RightId = RR.RightId
		WHERE r.RoleName = 'Systems Admin' AND r2.RightName = 'SystemsAdmin' AND r.ProductId = 60 AND r2.ProductId = 60 )
	BEGIN
		INSERT INTO Security.RoleRight (RoleId, RightId, CreatedBy, CreatedDate )
		SELECT R.RoleId, R2.RightId, @AdminUserId, GETUTCDATE() FROM Security.Role R CROSS JOIN Security.[Right] R2 
			WHERE r.RoleName = 'Systems Admin' AND R2.RightName = 'SystemsAdmin' AND r.ProductId = 60 AND r2.ProductId = 60
	END
END

GO
IF NOT EXISTS (SELECT TOP 1 1 FROM Enterprise.ProductSettingType WHERE [Name] = 'GetUserProductCenterEndPoint')
BEGIN
	INSERT INTO Enterprise.ProductSettingType ([Name], [Description], SensitiveData)
	VALUES ('GetUserProductCenterEndPoint', 'Get user product center end point', 0);
END

IF NOT EXISTS (SELECT TOP 1 1 FROM Enterprise.ProductSettingType WHERE [Name] = 'GetUserProductCenterSyncType')
BEGIN
	INSERT INTO Enterprise.ProductSettingType ([Name], [Description], SensitiveData)
	VALUES ('GetUserProductCenterSyncType', 'Get user product center sync type (push or pull)', 0);
END

IF NOT EXISTS (SELECT TOP 1 1 FROM Enterprise.ProductSettingType WHERE [Name] = 'GetUserProductCenterEnabled')
BEGIN
	INSERT INTO Enterprise.ProductSettingType ([Name], [Description], SensitiveData)
	VALUES ('GetUserProductCenterEnabled', 'Get user product center enabled (0 or 1)', 0);
END

DECLARE @NOW DATETIME = GETUTCDATE();
DECLARE @productlist as table (entid int identity, productid int, productsettingtype varchar(500), productsettingvalue varchar(2000))
insert into @productlist values
(1,  'GetUserProductCenterEndPoint', '/os/core/common/user/v2/product-centers?loginName={0}'),
(2,  'GetUserProductCenterEndPoint', '/user/product-centers?loginName={}'),
(3,  'GetUserProductCenterEndPoint', '/user/product-centers?loginName={}'),
(4,  'GetUserProductCenterEndPoint', '/user/product-centers?loginName={}'),
(6,  'GetUserProductCenterEndPoint', '/user/product-centers?loginName={}'),
(8,  'GetUserProductCenterEndPoint', '/user/product-centers?loginName={}'),
(9,  'GetUserProductCenterEndPoint', '/user/product-centers?loginName={}'),
(10, 'GetUserProductCenterEndPoint', '/user/product-centers?loginName={}'),
(11, 'GetUserProductCenterEndPoint', '/user/product-centers?loginName={}'),
(13, 'GetUserProductCenterEndPoint', '/user/product-centers?loginName={}'),
(14, 'GetUserProductCenterEndPoint', '/user/product-centers?loginName={}'),
(15, 'GetUserProductCenterEndPoint', '/user/product-centers?loginName={}'),
(16, 'GetUserProductCenterEndPoint', '/user/product-centers?loginName={}'),
(17, 'GetUserProductCenterEndPoint', '/user/product-centers?loginName={}'),
(18, 'GetUserProductCenterEndPoint', '/user/product-centers?loginName={}'),
(19, 'GetUserProductCenterEndPoint', '/user/product-centers?loginName={}'),
(20, 'GetUserProductCenterEndPoint', '/user/product-centers?loginName={}'),
(21, 'GetUserProductCenterEndPoint', '/user/product-centers?loginName={}'),
(23, 'GetUserProductCenterEndPoint', '/user/product-centers?loginName={}'),
(24, 'GetUserProductCenterEndPoint', '/user/product-centers?loginName={}'),
(25, 'GetUserProductCenterEndPoint', '/user/product-centers?loginName={}'),
(26, 'GetUserProductCenterEndPoint', '/user/product-centers?loginName={}'),
(27, 'GetUserProductCenterEndPoint', '/user/product-centers?loginName={}'),
(28, 'GetUserProductCenterEndPoint', '/user/product-centers?loginName={}'),
(29, 'GetUserProductCenterEndPoint', '/user/product-centers?loginName={}'),
(30, 'GetUserProductCenterEndPoint', '/user/product-centers?loginName={}'),
(31, 'GetUserProductCenterEndPoint', '/user/product-centers?loginName={}'),
(32, 'GetUserProductCenterEndPoint', '/user/product-centers?loginName={}'),
(33, 'GetUserProductCenterEndPoint', '/user/product-centers?loginName={}'),
(35, 'GetUserProductCenterEndPoint', '/user/product-centers?loginName={}'),
(36, 'GetUserProductCenterEndPoint', '/user/product-centers?loginName={}'),
(37, 'GetUserProductCenterEndPoint', '/user/product-centers?loginName={}'),
(38, 'GetUserProductCenterEndPoint', '/user/product-centers?loginName={}'),
(39, 'GetUserProductCenterEndPoint', '/user/product-centers?loginName={}'),
(40, 'GetUserProductCenterEndPoint', '/user/product-centers?loginName={}'),
(41, 'GetUserProductCenterEndPoint', '/user/product-centers?loginName={}'),
(44, 'GetUserProductCenterEndPoint', '/user/product-centers?loginName={}'),
(45, 'GetUserProductCenterEndPoint', '/user/product-centers?loginName={}'),
(47, 'GetUserProductCenterEndPoint', '/user/product-centers?loginName={}'),
(48, 'GetUserProductCenterEndPoint', '/user/product-centers?loginName={}'),
(49, 'GetUserProductCenterEndPoint', '/user/product-centers?loginName={}'),
(50, 'GetUserProductCenterEndPoint', '/user/product-centers?loginName={}'),
(51, 'GetUserProductCenterEndPoint', '/user/product-centers?loginName={}'),
(52, 'GetUserProductCenterEndPoint', '/user/product-centers?loginName={}'),
(53, 'GetUserProductCenterEndPoint', '/user/product-centers?loginName={}'),
(54, 'GetUserProductCenterEndPoint', '/user/product-centers?loginName={}'),
(57, 'GetUserProductCenterEndPoint', '/user/product-centers?loginName={}'),
(58, 'GetUserProductCenterEndPoint', '/user/product-centers?loginName={}'),
(59, 'GetUserProductCenterEndPoint', '/user/product-centers?loginName={}'),
(60, 'GetUserProductCenterEndPoint', '/user/product-centers?loginName={}'),
(62, 'GetUserProductCenterEndPoint', '/user/product-centers?loginName={}'),
(63, 'GetUserProductCenterEndPoint', '/user/product-centers?loginName={}'),
(64, 'GetUserProductCenterEndPoint', '/user/product-centers?loginName={}'),
(65, 'GetUserProductCenterEndPoint', '/user/product-centers?loginName={}'),
(66, 'GetUserProductCenterEndPoint', '/user/product-centers?loginName={}'),
(67, 'GetUserProductCenterEndPoint', '/user/product-centers?loginName={}'),
(68, 'GetUserProductCenterEndPoint', '/user/product-centers?loginName={}'),
(69, 'GetUserProductCenterEndPoint', '/user/product-centers?loginName={}')

insert into @productlist values
(1,  'GetUserProductCenterSyncType', 'Push'),
(2,  'GetUserProductCenterSyncType', 'Push'),
(3,  'GetUserProductCenterSyncType', 'Push'),
(4,  'GetUserProductCenterSyncType', 'Push'),
(6,  'GetUserProductCenterSyncType', 'Push'),
(8,  'GetUserProductCenterSyncType', 'Push'),
(9,  'GetUserProductCenterSyncType', 'Push'),
(10, 'GetUserProductCenterSyncType', 'Push'),
(11, 'GetUserProductCenterSyncType', 'Push'),
(13, 'GetUserProductCenterSyncType', 'Push'),
(14, 'GetUserProductCenterSyncType', 'Push'),
(15, 'GetUserProductCenterSyncType', 'Push'),
(16, 'GetUserProductCenterSyncType', 'Push'),
(17, 'GetUserProductCenterSyncType', 'Push'),
(18, 'GetUserProductCenterSyncType', 'Push'),
(19, 'GetUserProductCenterSyncType', 'Push'),
(20, 'GetUserProductCenterSyncType', 'Push'),
(21, 'GetUserProductCenterSyncType', 'Push'),
(23, 'GetUserProductCenterSyncType', 'Push'),
(24, 'GetUserProductCenterSyncType', 'Push'),
(25, 'GetUserProductCenterSyncType', 'Push'),
(26, 'GetUserProductCenterSyncType', 'Push'),
(27, 'GetUserProductCenterSyncType', 'Push'),
(28, 'GetUserProductCenterSyncType', 'Push'),
(29, 'GetUserProductCenterSyncType', 'Push'),
(30, 'GetUserProductCenterSyncType', 'Push'),
(31, 'GetUserProductCenterSyncType', 'Push'),
(32, 'GetUserProductCenterSyncType', 'Push'),
(33, 'GetUserProductCenterSyncType', 'Push'),
(35, 'GetUserProductCenterSyncType', 'Push'),
(36, 'GetUserProductCenterSyncType', 'Push'),
(37, 'GetUserProductCenterSyncType', 'Push'),
(38, 'GetUserProductCenterSyncType', 'Push'),
(39, 'GetUserProductCenterSyncType', 'Push'),
(40, 'GetUserProductCenterSyncType', 'Push'),
(41, 'GetUserProductCenterSyncType', 'Push'),
(44, 'GetUserProductCenterSyncType', 'Push'),
(45, 'GetUserProductCenterSyncType', 'Push'),
(47, 'GetUserProductCenterSyncType', 'Push'),
(48, 'GetUserProductCenterSyncType', 'Push'),
(49, 'GetUserProductCenterSyncType', 'Push'),
(50, 'GetUserProductCenterSyncType', 'Push'),
(51, 'GetUserProductCenterSyncType', 'Push'),
(52, 'GetUserProductCenterSyncType', 'Push'),
(53, 'GetUserProductCenterSyncType', 'Push'),
(54, 'GetUserProductCenterSyncType', 'Push'),
(57, 'GetUserProductCenterSyncType', 'Push'),
(58, 'GetUserProductCenterSyncType', 'Push'),
(59, 'GetUserProductCenterSyncType', 'Push'),
(60, 'GetUserProductCenterSyncType', 'Push'),
(62, 'GetUserProductCenterSyncType', 'Push'),
(63, 'GetUserProductCenterSyncType', 'Push'),
(64, 'GetUserProductCenterSyncType', 'Push'),
(65, 'GetUserProductCenterSyncType', 'Push'),
(66, 'GetUserProductCenterSyncType', 'Push'),
(67, 'GetUserProductCenterSyncType', 'Push'),
(68, 'GetUserProductCenterSyncType', 'Push'),
(69, 'GetUserProductCenterSyncType', 'Push')

insert into @productlist values
(1,  'GetUserProductCenterEnabled', '0'),
(2,  'GetUserProductCenterEnabled', '0'),
(3,  'GetUserProductCenterEnabled', '0'),
(4,  'GetUserProductCenterEnabled', '0'),
(6,  'GetUserProductCenterEnabled', '0'),
(8,  'GetUserProductCenterEnabled', '0'),
(9,  'GetUserProductCenterEnabled', '0'),
(10, 'GetUserProductCenterEnabled', '0'),
(11, 'GetUserProductCenterEnabled', '0'),
(13, 'GetUserProductCenterEnabled', '0'),
(14, 'GetUserProductCenterEnabled', '0'),
(15, 'GetUserProductCenterEnabled', '0'),
(16, 'GetUserProductCenterEnabled', '0'),
(17, 'GetUserProductCenterEnabled', '0'),
(18, 'GetUserProductCenterEnabled', '0'),
(19, 'GetUserProductCenterEnabled', '0'),
(20, 'GetUserProductCenterEnabled', '0'),
(21, 'GetUserProductCenterEnabled', '0'),
(23, 'GetUserProductCenterEnabled', '0'),
(24, 'GetUserProductCenterEnabled', '0'),
(25, 'GetUserProductCenterEnabled', '0'),
(26, 'GetUserProductCenterEnabled', '0'),
(27, 'GetUserProductCenterEnabled', '0'),
(28, 'GetUserProductCenterEnabled', '0'),
(29, 'GetUserProductCenterEnabled', '0'),
(30, 'GetUserProductCenterEnabled', '0'),
(31, 'GetUserProductCenterEnabled', '0'),
(32, 'GetUserProductCenterEnabled', '0'),
(33, 'GetUserProductCenterEnabled', '0'),
(35, 'GetUserProductCenterEnabled', '0'),
(36, 'GetUserProductCenterEnabled', '0'),
(37, 'GetUserProductCenterEnabled', '0'),
(38, 'GetUserProductCenterEnabled', '0'),
(39, 'GetUserProductCenterEnabled', '0'),
(40, 'GetUserProductCenterEnabled', '0'),
(41, 'GetUserProductCenterEnabled', '0'),
(44, 'GetUserProductCenterEnabled', '0'),
(45, 'GetUserProductCenterEnabled', '0'),
(47, 'GetUserProductCenterEnabled', '0'),
(48, 'GetUserProductCenterEnabled', '0'),
(49, 'GetUserProductCenterEnabled', '0'),
(50, 'GetUserProductCenterEnabled', '0'),
(51, 'GetUserProductCenterEnabled', '0'),
(52, 'GetUserProductCenterEnabled', '0'),
(53, 'GetUserProductCenterEnabled', '0'),
(54, 'GetUserProductCenterEnabled', '0'),
(57, 'GetUserProductCenterEnabled', '0'),
(58, 'GetUserProductCenterEnabled', '0'),
(59, 'GetUserProductCenterEnabled', '0'),
(60, 'GetUserProductCenterEnabled', '0'),
(62, 'GetUserProductCenterEnabled', '0'),
(63, 'GetUserProductCenterEnabled', '0'),
(64, 'GetUserProductCenterEnabled', '0'),
(65, 'GetUserProductCenterEnabled', '0'),
(66, 'GetUserProductCenterEnabled', '0'),
(67, 'GetUserProductCenterEnabled', '0'),
(68, 'GetUserProductCenterEnabled', '0'),
(69, 'GetUserProductCenterEnabled', '0')

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
	IF EXISTS ( SELECT TOP(1) 1 FROM Enterprise.Product WHERE ProductId = @CurrentProductId )
	BEGIN    
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
	END	
	set @Current_ID = @Current_ID + 1
end
go
DECLARE @PartyId INT
select @PartyId = PartyId from Enterprise.Organization where Name = 'RealPage Employee'

IF NOT EXISTS (select TOP 1 1 from Enterprise.PartyRole where PartyId = @PartyId AND RoleTypeId = 405)
BEGIN
 INSERT INTO Enterprise.PartyRole VALUES(@PartyId, 405)
END

GO

IF NOT EXISTS (SELECT TOP 1 1 FROM Enterprise.ProductSettingType where Name = 'HOTSCloneUserCallBackEnpoint')
BEGIN
	INSERT INTO Enterprise.ProductSettingType (Name, Description, SensitiveData) values ('HOTSCloneUserCallBackEnpoint', 'Endpoint to be called by UL once HOTS clone user request is completed.', 0)
END

IF NOT EXISTS (SELECT TOP 1 1 FROM Enterprise.ProductSetting ps	
	JOIN Enterprise.ProductSettingType pst on pst.ProductSettingTypeId = ps.ProductSettingTypeId	
	WHERE ProductId = 3 	
	AND pst.Name = 'HOTSCloneUserCallBackEnpoint')
BEGIN 
	DECLARE @typeId int;
	SELECT @typeId = ProductSettingTypeId from Enterprise.ProductSettingType
	WHERE NAME = 'HOTSCloneUserCallBackEnpoint'
	
	DECLARE @serverName VARCHAR(50);
	SELECT @serverName = @@SERVERNAME 
	
	DECLARE @endpoint VARCHAR(100)

	IF(@serverName = 'RCDUSODBSQL001') -- dev
	BEGIN 
		Set @endpoint = 'https://training-api-dev.realpage.com/v1/cloning/userclone';
	END
	ELSE IF(@serverName = 'RCTUSODBSQL001') -- qa
	BEGIN 
		Set @endpoint = 'https://training-api-qa.realpage.com/v1/cloning/userclone';
	END
	ELSE IF(@serverName = 'RCTUSODBTUL001') -- training 
	BEGIN 
		Set @endpoint = 'https://training-api.realpage.com/v1/cloning/userclone';
	END

	EXEC Enterprise.SetProductSetting @ProductSettingId=0,  @ProductId =3,  @ProductSettingTypeId = @typeId,  @Value = @endpoint
End
GO
