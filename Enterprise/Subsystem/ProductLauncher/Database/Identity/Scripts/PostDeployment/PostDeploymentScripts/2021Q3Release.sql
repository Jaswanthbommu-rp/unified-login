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
WHERE LoginName LIKE 'realpagead@%'

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

--Product Center Seed Data


MERGE INTO Enterprise.ProductCenter pc
	USING 
	(
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
	)
	AS 
	src (ProductCenterSourceId, Name, Source) ON src.ProductCenterSourceId = pc.ProductCenterSourceId AND src.Source = pc.Source
	WHEN MATCHED THEN
		UPDATE SET
			Name = src.Name,
			ModifiedDate = GETUTCDATE()
	WHEN NOT MATCHED BY TARGET THEN
		INSERT (ProductCenterSourceId, Source, Name) 
		VALUES (src.ProductCenterSourceId, src.Source, src.Name)
	;

DECLARE @OneSiteProductId INT;
SELECT @OneSiteProductId = Productid FROM Enterprise.Product WHERE BooksProductCode = 'OS'

MERGE INTO Enterprise.ProductProductCenter ppc
	USING (SELECT ProductCenterId FROM Enterprise.ProductCenter WHERE Source = 'OS') AS src (ProductCenterId) 
		ON ppc.ProductId = @OneSiteProductId AND ppc.ProductCenterId = src.ProductCenterId
WHEN MATCHED THEN
	UPDATE SET
		ModifiedDate = GETUTCDATE()
WHEN NOT MATCHED BY TARGET THEN
	INSERT (ProductId, ProductCenterId)
	VALUES (@OneSiteProductId, src.ProductCenterId);

DECLARE @MCProductId INT;
SELECT @MCProductId = Productid FROM Enterprise.Product WHERE BooksProductCode = 'LS'

MERGE INTO Enterprise.ProductProductCenter ppc
	USING (SELECT ProductCenterId FROM Enterprise.ProductCenter WHERE Source = 'LS') AS src (ProductCenterId) 
		ON ppc.ProductId = @MCProductId AND ppc.ProductCenterId = src.ProductCenterId
WHEN MATCHED THEN
	UPDATE SET
		ModifiedDate = GETUTCDATE()
WHEN NOT MATCHED BY TARGET THEN
	INSERT (ProductId, ProductCenterId)
	VALUES (@MCProductId, src.ProductCenterId);


-- Add Status Type Category Classification and Types for Sync Statuses
DECLARE @StatusTypeCategoryTypeId INT, @StatusTypeCategoryId INT;
SELECT @StatusTypeCategoryTypeId = StatusTypeCategoryTypeId FROM Enterprise.StatusTypeCategoryType WHERE Name = 'Status'
SELECT @StatusTypeCategoryId = StatusTypeCategoryId FROM Enterprise.StatusTypeCategory WHERE Name = 'Sync Status'
IF @StatusTypeCategoryId IS NULL
BEGIN
	INSERT INTO Enterprise.StatusTypeCategory (StatusTypeCategoryTypeId, Name)
	VALUES (@StatusTypeCategoryTypeId, 'Sync Status')
	SELECT @StatusTypeCategoryId = SCOPE_IDENTITY()
END

MERGE INTO Enterprise.StatusTypeCategoryClassification as stcc
USING (
	SELECT * FROM Enterprise.StatusType WHERE Name in ('Pending', 'Error', 'Success')
) AS src ON src.StatusTypeId = stcc.statusTypeId AND stcc.StatusTypeCategoryId = @StatusTypeCategoryId
WHEN NOT MATCHED BY TARGET THEN
	INSERT(StatusTypeId, StatusTypeCategoryId)
	VALUES(src.StatusTypeId, @StatusTypeCategoryId)
;

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
IF NOT EXISTS (SELECT 1 FROM [Batch].[BatchProcessConfigurationType] Where Name = 'EnterpriseRoleProcessApiEndpoint')
Begin
    Insert Into [Batch].[BatchProcessConfigurationType](BatchProcessConfigurationTypeId,[Name],[Description])
	Select 2,'EnterpriseRoleProcessApiEndpoint', 'API Endpoint to be invoked by batch processor'
End
GO
IF NOT EXISTS(Select 1 From [Batch].[BatchProcessConfiguration] Where BatchProcessConfigurationId = 2)
  BEGIN
  
	DECLARE @serverName VARCHAR(50);
	SELECT @serverName = @@SERVERNAME 
	
	DECLARE @endpoint VARCHAR(100)
	SET @endpoint = '';

	IF(@serverName = 'RCDUSODBSQL001') -- dev
	BEGIN 
		Set @endpoint = 'https://my2dev.realpage.com/api/erpbatchprocessor';
	END
	IF(@serverName = 'RCTUSODBSQL001') -- qa
	BEGIN 
		Set @endpoint = 'https://my2qa.realpage.com/api/erpbatchprocessor';
	END
	IF @ServerName IN ('RCAUSODBSQL001') --SAT
	BEGIN
		Set @endpoint = 'https://my2sat.realpage.com/api/erpbatchprocessor';
	END
	IF @ServerName IN ('RCTUSODBSQL001A','RCTUSODBSQL001B') --UAT
	BEGIN
		Set @endpoint = 'https://my2uat.realpage.com/api/erpbatchprocessor';
	END
	IF @ServerName IN ('RCIUSODBSQL002') --PREPROD
	BEGIN
		Set @endpoint = 'https://my2preprod.realpage.com/api/erpbatchprocessor';
	END

	IF @ServerName IN ('RCVGBKDBSQL001') --DEMO
	BEGIN
		Set @endpoint = 'https://my2demo.realpage.com/api/erpbatchprocessor';
	END

	IF @ServerName IN ('RCTUSODBTUL001') --TRAINING
	BEGIN
		Set @endpoint = 'https://my2qa.realpage.com/api/erpbatchprocessor';
	END

	IF @ServerName IN ('RCPGBKDBSQL005A', 'RCPGBKDBSQL005B') --PROD
	BEGIN
		Set @endpoint = 'https://my2.realpage.com/api/erpbatchprocessor';
	END

	IF @ServerName IN ('reagbkdbsql001') --EUSAT
	BEGIN
		SET @endpoint = 'https://my2sat.realpage.co.uk/api/erpbatchprocessor';
	END

	IF @ServerName IN ('repgbkdbsql001a','repgbkdbsql001b') --EUPROD
	BEGIN
		SET @endpoint = 'https://my2.realpage.co.uk/api/erpbatchprocessor';
	END
	Insert Into Batch.BatchProcessConfiguration(BatchProcessConfigurationId,BatchProcessConfigurationTypeId,Value)
	Select 2,2,@endpoint
  END
  GO

DECLARE @ControlId BIGINT, @GridControlId BIGINT
SELECT @ControlId = ControlId FROM UserManagement.Control WHERE UIId='ClientPortalRoleTemplateCheckboxUIId'
SELECT @GridControlId = ControlId FROM UserManagement.Control WHERE UIId='ClientPortalRoleTemplateRolesSelectGridUIId'
UPDATE UserManagement.Control SET ControlTypeId = 7 WHERE ControlId = @ControlId
UPDATE UserManagement.Control SET ControlTypeId = 2 WHERE ControlId = @GridControlId
UPDATE UserManagement.ControlAttribute SET Value = 'False'	WHERE ControlId = @GridControlId

GO


  --UserStory 546918
Declare @R1 varchar(100),@R2 varchar(100),@R3 varchar(100),@R4 varchar(100),@RoleId int,@UserId int;

SELECT @UserId = UserId
FROM Ident.UserLogin
WHERE LoginName LIKE 'realpagead@%';

Select @RoleId =RoleId from Security.Role where ShortName ='ROForUnifiedPlatform';
Select @R1 = RightId from Security.[Right] where RightName = 'ViewUnifiedSettings';
Select @R2 = RightId from Security.[Right] where RightName = 'Viewallcompanylevelsettings';
Select @R3 = RightId from Security.[Right] where RightName = 'Viewallpropertylevelsettings';
Select @R4 = RightId from Security.[Right] where RightName = 'ViewCIMPLQuestions';

IF NOT EXISTS(Select Top 1 1 from Security.RoleRight where RoleId = @RoleId and RightId in (@R1,@R2,@R3,@R4))
BEGIN TRY
Insert into Security.RoleRight(RoleId,RightId,CreatedBy,CreatedDate) Values(@RoleId,@R1,@UserId,GETDATE());

Insert into Security.RoleRight(RoleId,RightId,CreatedBy,CreatedDate) Values(@RoleId,@R2,@UserId,GETDATE());

Insert into Security.RoleRight(RoleId,RightId,CreatedBy,CreatedDate) Values(@RoleId,@R3,@UserId,GETDATE());

Insert into Security.RoleRight(RoleId,RightId,CreatedBy,CreatedDate) Values(@RoleId,@R4,@UserId,GETDATE());
END TRY
Begin Catch 
 
      Declare @ErrorMessage nvarchar(250);
      Declare @ErrorSeverity int;
      Declare @ErrorState int;
      Declare @ErrorLine int;
      Select @ErrorMessage = ERROR_Message()
	         ,@ErrorSeverity = ERROR_SEVERITY()
			 ,@ErrorState = ERROR_STATE()
			 ,@ErrorLine = ERROR_LINE();
			 
		 RAISERROR(@ErrorMessage,@ErrorSeverity,@ErrorState,@ErrorLine); 
 END Catch 
 
 GO

 

-- Add Employee Access to Manage Settings Templates right to RealPage Employee Company
DECLARE @CreatedById bigint,
		@RouteId bigint,
		@RightId bigint,
		@Now datetime = GETDATE(),
		@PartyId bigint,
		@RoleId bigint

SELECT @CreatedById = UserId
FROM Ident.UserLogin
WHERE LoginName LIKE 'realpagead@%'; 


IF NOT EXISTS (SELECT 1 FROM [Security].[Right] WHERE RightName = 'EmployeeAccesstoManageSettingsTemplates')
BEGIN
	INSERT INTO [Security].[Right](	RightName,Description, Value,StatusTypeId,VisibilityStatusId,ProductId,TargetProductId,	CreatedBy,CreatedDate)
    VALUES ('EmployeeAccesstoManageSettingsTemplates', 'Employee Access to Manage Settings Templates','Employee Access to Manage Settings Templates', 13,10, 3, 56, @CreatedById, @Now)
END

--OrganizationOverRideRight
SELECT @RightId = RightId
FROM [Security].[Right]
WHERE RightName = 'EmployeeAccesstoManageSettingsTemplates'

SELECT @PartyId = O.PartyId
FROM [Enterprise].[Organization] O
    INNER JOIN [Enterprise].[Party] P ON P.PartyId = O.PartyId
WHERE p.RealPageId = '0D018E46-C20E-477D-ADED-4E5A35FB8F99'

IF NOT EXISTS (SELECT Top 1 1 FROM [Security].[OrganizationOverRideRight]  WHERE RightId = @RightId AND OrgPartyId = @PartyId)
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


-- sync new support admin table
;WITH companyadminusers ( 	OrganizationPartyId, UserLoginPersonaId )
AS (
	SELECT O.PartyId,
				   UL.UserLoginPersonaId
			 FROM Enterprise.Organization O
				  INNER JOIN Enterprise.Party P ON O.PartyId = P.PartyId
				  INNER JOIN Enterprise.VW_DataImportMapping D ON O.PartyId = D.PartyId
				  INNER JOIN Enterprise.OrganizationDomain OD on O.OrganizationDomainId = OD.OrganizationDomainId
				  INNER JOIN Enterprise.MasterConfiguration MC ON MC.AttributeId = O.PartyId
				  INNER JOIN Enterprise.MasterConfigurationSetting MCS ON MC.MasterConfigurationId = MCS.MasterConfigurationId
				  INNER JOIN Enterprise.MasterSetting MS ON MCS.MasterSettingId = MS.MasterSettingId
				  INNER JOIN Enterprise.MasterSettingType MST ON MST.MasterSettingTypeId = MS.MasterSettingTypeId
				  INNER JOIN Enterprise.MasterConfigurationType MCT ON MCT.MasterConfigurationTypeId = MST.MasterConfigurationTypeId
				  INNER JOIN
					(
						SELECT P.RealPageId,
							   UL.LoginName,
							   ul.UserId,
							   ulp.UserLoginPersonaId
						FROM 
							Ident.UserLogin UL
							INNER JOIN Enterprise.Party P ON UL.PersonPartyId = P.PartyId
							INNER JOIN Ident.UserLoginPersona ULP ON ULP.UserLoginId = UL.UserId AND ULP.PrimaryOrganization = 1
					) UL ON UL.RealPageId = MS.Value
			 WHERE MCT.Name = 'Organization'
				   AND MST.Name = N'RealPageEmployeeAccessID'
)

INSERT INTO Enterprise.OrganizationAdminUser (OrganizationPartyId, UserLoginPersonaId)
SELECT CA.OrganizationPartyId, ca.UserLoginPersonaId 
FROM companyadminusers CA 
	LEFT OUTER JOIN Enterprise.OrganizationAdminUser OAU ON CA.OrganizationPartyId = OAU.OrganizationPartyId
WHERE OAU.OrganizationPartyId IS NULL
-- sync new support admin table
GO

 GO
 IF NOT EXISTS (SELECT 1 FROM [Batch].[BatchProcessType] WHERE Name = 'EnterpriseRoleCreateUpdateProductUser')
  BEGIN
	INSERT INTO [Batch].[BatchProcessType]
	SELECT 10,1,'Batch to create EnterpriseRole Create-Update User','EnterpriseRoleCreateUpdateProductUser'
  END
  IF NOT EXISTS (SELECT 1 FROM [Batch].[BatchProcessType] WHERE Name = 'EnterpriseRoleBulkUpdateProductUser')
  BEGIN
	INSERT INTO [Batch].[BatchProcessType]
	SELECT 11,2,'Batch to create EnterpriseRole Bulk Update Users','EnterpriseRoleBulkUpdateProductUser'
  END
  IF NOT EXISTS (SELECT 1 FROM [Batch].[BatchProcessType] WHERE Name = 'CreateEnterpriseRoleFromUserProduct')
  BEGIN
	INSERT INTO [Batch].[BatchProcessType]
	SELECT 12,2,'Batch to create EnterpriseRole based on User Products','CreateEnterpriseRoleFromUserProduct'
  END
GO

--User Story 860157

-- Add Settings Internal Administrator right to RealPage Employee Company
DECLARE @CreatedById bigint,
		@RouteId bigint,
		@RightId bigint,
		@Now datetime = GETDATE(),
		@PartyId bigint,
		@RoleId bigint

SELECT @CreatedById = UserId
FROM Ident.UserLogin
WHERE LoginName LIKE 'realpagead@%'; 


IF NOT EXISTS (SELECT 1 FROM [Security].[Right] WHERE RightName = 'InternalAdminaccessToUnifiedSettings')
BEGIN
	INSERT INTO [Security].[Right](	RightName,Description, Value,StatusTypeId,VisibilityStatusId,ProductId,TargetProductId,	CreatedBy,CreatedDate)
    VALUES ('InternalAdminaccessToUnifiedSettings', 'Internal Admin Access to Unified Settings','Internal Admin Access to Unified Settings', 13,10, 3, 56, @CreatedById, @Now)
END

--OrganizationOverRideRight
SELECT @RightId = RightId
FROM [Security].[Right]
WHERE RightName = 'InternalAdminaccessToUnifiedSettings'

SELECT @PartyId = O.PartyId
FROM [Enterprise].[Organization] O
    INNER JOIN [Enterprise].[Party] P ON P.PartyId = O.PartyId
WHERE p.RealPageId = '0D018E46-C20E-477D-ADED-4E5A35FB8F99'

IF NOT EXISTS (SELECT Top 1 1 FROM [Security].[OrganizationOverRideRight]  WHERE RightId = @RightId AND OrgPartyId = @PartyId)
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

-- User Story 898054 Smart Waste Commercial Role's and Right
Declare @UserId bigint;
SELECT @UserId = UserId
FROM   Ident.UserLogin
WHERE  LoginName LIKE 'realpagead@%';
-- Adding default roles to Smart Waste Commercial Product
IF NOT EXISTS(SELECT 1 FROM Security.Role WHERE ProductId = 70)
BEGIN
	INSERT INTO Security.Role(RoleName, ShortName, Description, RoleTypeID, OrgPartyID, ProductId, CreatedBy, CreatedDate )
	VALUES('Portfolio Manager','PortfolioManager','Portfolio Manager', 1, NULL, 70, @UserId, GETDATE())	
	INSERT INTO Security.Role(RoleName, ShortName, Description, RoleTypeID, OrgPartyID, ProductId, CreatedBy, CreatedDate )
	VALUES('Property Manager','PropertyManager','Property Manager', 1, NULL, 70, @UserId,GETDATE())	
END
IF NOT EXISTS(SELECT 1 FROM Security.[Right] WHERE ProductId = 70)
BEGIN
	INSERT INTO Security.[Right](RightName, Description, Value, StatusTypeId, VisibilityStatusId, ProductId, TargetProductId, CreatedBy, CreatedDate)
	VALUES('ReadOnly', NULL, 'Read Only Access',13, 9, 70, 70, @UserId, GETDATE())	
END
DECLARE @PortfolioManagerRoleId INT;
DECLARE @PropertyManagerRoleId INT;
DECLARE @RightId INT;
SELECT @PortfolioManagerRoleId = RoleId FROM Security.Role WHERE RoleName = 'Portfolio Manager' AND ProductId = 70
SELECT @PropertyManagerRoleId = RoleId FROM Security.Role WHERE RoleName = 'Property Manager' AND ProductId = 70
SELECT @RightId = RightId FROm Security.[Right] WHERE RightName = 'ReadOnly' AND ProductId = 70 AND TargetProductId = 70
IF NOT EXISTS(SELECT 1 FROM Security.RoleRight WHERE RightId  = @RightId AND RoleId = @PortfolioManagerRoleId)
BEGIN
	INSERT INTO Security.RoleRight(RoleId, RightId, CreatedBy, CreatedDate)
	VALUES(@PortfolioManagerRoleId, @RightId, @UserId, GETDATE())
END
IF NOT EXISTS(SELECT 1 FROM Security.RoleRight WHERE RightId  = @RightId AND RoleId = @PropertyManagerRoleId)
BEGIN
	INSERT INTO Security.RoleRight(RoleId, RightId, CreatedBy, CreatedDate)
	VALUES(@PropertyManagerRoleId, @RightId, @UserId, GETDATE())
END
GO

DECLARE @UserId bigint

SELECT	@UserId = UserId
FROM	Ident.UserLogin
WHERE	LoginName LIKE 'realpagead@%'

IF NOT EXISTS ( SELECT TOP(1) 1 FROM security.Role WHERE ProductId = 39 AND RoleName = 'Integration Viewer' AND ShortName = 'Role-IntVwr' )
BEGIN
	INSERT INTO security.Role ( RoleName,ShortName,Description,RoleTypeID,OrgPartyID,ProductId,CreatedBy,CreatedDate)
	VALUES ( N'Integration Viewer', 'Role-IntVwr', 'Integration Viewer', 3, NULL, 39, @UserId, GETUTCDATE() )
END
IF NOT EXISTS ( SELECT TOP(1) 1 FROM security.Role WHERE ProductId = 39 AND RoleName = 'Integration Manager' AND ShortName = 'Role-IntMgr' )
BEGIN
	INSERT INTO security.Role ( RoleName,ShortName,Description,RoleTypeID,OrgPartyID,ProductId,CreatedBy,CreatedDate)
		VALUES ( N'Integration Manager', 'Role-IntMgr', 'Integration Manager', 3, NULL, 39, @UserId, GETUTCDATE() )
END

IF NOT EXISTS (SELECT 1 FROM [Security].[Right] WHERE RightName = 'Role-IntVwr')
BEGIN
	INSERT INTO [Security].[Right](	RightName,Description, Value,StatusTypeId,VisibilityStatusId,ProductId,TargetProductId,	CreatedBy,CreatedDate)
    VALUES ('Role-IntVwr', 'Integration Viewer','Integration Viewer', 13,9, 39, 39, @UserId, GETUTCDATE())
END

IF NOT EXISTS (SELECT 1 FROM [Security].[Right] WHERE RightName = 'Role-IntMgr')
BEGIN
	INSERT INTO [Security].[Right](	RightName,Description, Value,StatusTypeId,VisibilityStatusId,ProductId,TargetProductId,	CreatedBy,CreatedDate)
    VALUES ('Role-IntMgr', 'Integration Manager','Integration Manager', 13,9, 39, 39, @UserId, GETUTCDATE())
END

IF NOT EXISTS ( SELECT 1 FROM [Security].RoleRight RR 
					INNER JOIN Security.Role R ON R.RoleId = RR.RoleId 
					INNER JOIN security.[Right] R2 ON R2.RightId = RR.RightId 
				WHERE R.ShortName = 'Role-IntVwr' AND R.ProductId = 39 AND R.OrgPartyID IS NULL AND r2.RightName = 'Role-IntVwr' AND R2.ProductId = 39 )
BEGIN
	INSERT INTO SECURITY.RoleRight ( RoleId, RightId, CreatedBy, CreatedDate )
	SELECT r.RoleId, R2.RightId, @UserId, GETUTCDATE() 
		FROM Security.Role R CROSS JOIN security.[Right] R2 
	WHERE R.ShortName = 'Role-IntVwr' AND R.ProductId = 39 AND R.OrgPartyID IS NULL AND r2.RightName = 'Role-IntVwr' AND R2.ProductId = 39
END

IF NOT EXISTS ( SELECT 1 FROM [Security].RoleRight RR 
					INNER JOIN Security.Role R ON R.RoleId = RR.RoleId 
					INNER JOIN security.[Right] R2 ON R2.RightId = RR.RightId 
				WHERE R.ShortName = 'Role-IntMgr' AND R.ProductId = 39 AND R.OrgPartyID IS NULL AND r2.RightName = 'Role-IntMgr' AND R2.ProductId = 39 )
BEGIN
	INSERT INTO SECURITY.RoleRight ( RoleId, RightId, CreatedBy, CreatedDate )
	SELECT r.RoleId, R2.RightId, @UserId, GETUTCDATE() 
		FROM Security.Role R CROSS JOIN security.[Right] R2 
	WHERE R.ShortName = 'Role-IntMgr' AND R.ProductId = 39 AND R.OrgPartyID IS NULL AND r2.RightName = 'Role-IntMgr' AND R2.ProductId = 39
END

GO

;WITH oldimrole ( personaid, oldvalue ) AS (
	SELECT personaid, CASE WHEN value = 'Role-IntMgr' THEN 'Role-IntMgr' ELSE CASE WHEN value = 'Role-IntVwr' THEN 'Role-IntVwr' ELSE 'Role-IntVwr' end end FROM ident.SamlUserAttribute sua INNER JOIN ident.SamlAttribute sa ON sa.SamlAttributeId = sua.SamlAttributeId WHERE sa.Name = 'RoleCode' AND sua.ProductId = 39 
)
, newpersonarole ( personaid, roleid )
	AS ( SELECT personaid, roleid FROM oldimrole om INNER JOIN security.Role R ON om.oldvalue = r.ShortName AND r.ProductId = 39 )
INSERT INTO Security.PersonaRole (PersonaId,RoleId,FromDate,ThruDate,CreatedBy,CreatedDate)
	SELECT np.personaid, np.roleid, GETUTCDATE(), NULL, 480, GETUTCDATE() FROM newpersonarole np LEFT JOIN security.PersonaRole pr ON pr.RoleId = np.roleid AND pr.PersonaId = np.personaid
		WHERE pr.PersonaId IS NULL AND pr.RoleId IS NULL

GO

-- UPDATE THE PRODUCT SETTING
IF NOT EXISTS (
	SELECT TOP (1) 1 FROM enterprise.GlobalProductConfiguration gpc 
		INNER JOIN enterprise.ProductConfiguration pc ON pc.ConfigurationId = gpc.ConfigurationId
		INNER JOIN enterprise.ProductSetting ps ON ps.ProductSettingId = pc.ProductSettingId 
		INNER JOIN enterprise.ProductSettingType pst ON pst.ProductSettingTypeId = ps.ProductSettingTypeId
	WHERE gpc.ProductId = 39
	AND gpc.ThruDate IS NULL
	AND pc.ThruDate IS NULL
	AND ps.ThruDate IS null
	AND pst.Name = 'ProductIntegrationType' AND ps.VALUE = 'UPFM' )
BEGIN
	IF EXISTS (
		SELECT TOP (1) 1 FROM enterprise.GlobalProductConfiguration gpc 
			INNER JOIN enterprise.ProductConfiguration pc ON pc.ConfigurationId = gpc.ConfigurationId
			INNER JOIN enterprise.ProductSetting ps ON ps.ProductSettingId = pc.ProductSettingId 
			INNER JOIN enterprise.ProductSettingType pst ON pst.ProductSettingTypeId = ps.ProductSettingTypeId
		WHERE gpc.ProductId = 39
		AND gpc.ThruDate IS NULL
		AND pc.ThruDate IS NULL
		AND ps.ThruDate IS null
		AND pst.Name = 'ProductIntegrationType' AND ps.VALUE = 'Legacy')
	BEGIN
		DECLARE @ProductSettingId INT
		SELECT TOP (1) @ProductSettingId = PS.ProductSettingId FROM enterprise.GlobalProductConfiguration gpc 
			INNER JOIN enterprise.ProductConfiguration pc ON pc.ConfigurationId = gpc.ConfigurationId
			INNER JOIN enterprise.ProductSetting ps ON ps.ProductSettingId = pc.ProductSettingId 
			INNER JOIN enterprise.ProductSettingType pst ON pst.ProductSettingTypeId = ps.ProductSettingTypeId
			WHERE gpc.ProductId = 39
			AND gpc.ThruDate IS NULL
			AND pc.ThruDate IS NULL
			AND ps.ThruDate IS null
			AND pst.Name = 'ProductIntegrationType' AND ps.VALUE = 'Legacy'
		ORDER BY PC.ProductConfigurationId DESC
        UPDATE Enterprise.ProductSetting SET value = 'UPFM' WHERE ProductSettingId = @ProductSettingId AND value = 'Legacy'
	END
END

GO

IF NOT EXISTS ( SELECT TOP (1) 1 FROM Auth.Claim WHERE ClaimName = 'rolealias~im-role' AND ProductId = 39 )
BEGIN
	INSERT INTO auth.Claim ( ClaimName, SAMLAttributeName, ProductId )
VALUES
	( N'rolealias~im-role', '', 39 )
END

GO
IF EXISTS ( SELECT TOP (1) 1 FROM AUTH.ClientUserClaim CC INNER JOIN auth.clients c ON c.ClientId = CC.ClientId INNER JOIN auth.claim c1 ON c1.ClaimId = CC.ClaimId 
	WHERE c.ClientCode = 'IntegrationMarketplace' AND c1.SAMLAttributeName = 'RoleCode' and c1.ProductId = 39 )
BEGIN
	DELETE CC FROM AUTH.ClientUserClaim CC INNER JOIN auth.clients c ON c.ClientId = CC.ClientId INNER JOIN auth.claim c1 ON c1.ClaimId = CC.ClaimId 
		WHERE c.ClientCode = 'IntegrationMarketplace' AND c1.SAMLAttributeName = 'RoleCode' and c1.ProductId = 39
END
GO

IF NOT EXISTS (SELECT TOP (1) 1 FROM AUTH.ClientUserClaim CC INNER JOIN auth.clients c ON c.ClientId = CC.ClientId INNER JOIN auth.claim c1 ON c1.ClaimId = CC.ClaimId 
	WHERE c.ClientCode = 'IntegrationMarketplace' AND c1.ClaimName = 'rolealias~im-role' and c1.ProductId = 39 )
BEGIN
	INSERT INTO Auth.ClientUserClaim ( ClientId, ClaimId )
	SELECT C.ClientId, c1.ClaimId FROM Auth.Clients C CROSS JOIN Auth.Claim c1 WHERE c.ClientCode = 'IntegrationMarketplace' AND c1.ClaimName = 'rolealias~im-role' and c1.ProductId = 39 
END

GO

-- Add LockOnProductAccessRight product settings
IF NOT EXISTS (SELECT TOP 1 1 FROM Enterprise.ProductSettingType WHERE [Name] = 'LockOnProductAccessRight')
BEGIN
	INSERT INTO Enterprise.ProductSettingType ([Name], [Description], SensitiveData)
	VALUES ('LockOnProductAccessRight', 'Defines which right to use for the LockOnProductAccess solution property', 0);
END

GO

-- Add the Manage Relate 24/7 Product Access right
 DECLARE @RightValue nvarchar(200),
		 @UserId bigint,
		 @Now datetime = GETDATE(),
		 @RightId int,
		 @RoleId INT,
		 @ProductId int = 3,
		 @TargetProductId int = 60,
		 @RoleName nvarchar(100),
		 @OrgVisibilityStatusId INT = 9,
		 @RightVisibilityStatusId INT =9,
		 @StatusTypeId int=13;
SELECT	@UserId = UserId
	FROM	Ident.UserLogin
	WHERE	LoginName LIKE 'realpagead@%'
IF NOT EXISTS(SELECT TOP 1 1 FROM [Security].[Right] WHERE [Value] ='Manage Relate 24/7 Product Access')
BEGIN 
		INSERT INTO Security.[Right] (RightName,Description,Value,StatusTypeId,VisibilityStatusId,ProductId,TargetProductId,CreatedBy,CreatedDate)
		VALUES('ManageRelate247ProductAccess','Manage Relate 24/7 Product Access','Manage Relate 24/7 Product Access',@StatusTypeId,@RightVisibilityStatusId,@ProductId ,@TargetProductId,@UserId,@Now);
END
SELECT @RoleId = RoleId from [Security].[Role] where RoleName='User Administrator';
SELECT @RightId =  RightId from [Security].[Right] where [Value] = 'Manage Relate 24/7 Product Access';
IF NOT EXISTS(SELECT TOP 1 1 FROM [Security].[RoleRight] WHERE [RightId]= @RightId)
BEGIN
	INSERT INTO Security.RoleRight (RoleId,RightId,CreatedBy,CreatedDate) 
	VALUES(@RoleId,@RightId,@UserId,@Now);
END
GO
UPDATE Auth.Claim
SET ClaimName = 'roleid'
WHERE ClaimName = 'role' AND productid = 3
GO

--Roles
declare @productid bigint, @UserId bigint
select @productid = 3
SELECT	@UserId = UserId FROM	Ident.UserLogin WHERE	LoginName LIKE 'realpagead@%'
if not exists(select 1  from Security.Role where RoleName='Contributor to CIMPL property-level' and ProductId =@productid and OrgPartyID IS NULL)
begin 
insert into Security.Role(RoleName,ShortName,Description,RoleTypeID,OrgPartyID,ProductId,CreatedBy,CreatedDate)
select 'Contributor to CIMPL property-level','ContributorToCIMPLPropertyLevel','Contributor to CIMPL property-level',1,NULL,@productid, @UserId, getUTCDate()
end
if not exists(select 1  from Security.Role where RoleName='Manage ALL Implementations in CIMPL' and ProductId =@productid  and OrgPartyID IS NULL)
begin 
insert into Security.Role(RoleName,ShortName,Description,RoleTypeID,OrgPartyID,ProductId,CreatedBy,CreatedDate)
select 'Manage ALL Implementations in CIMPL','ManageALLImplementationsInCIMPL','Manage ALL Implementations in CIMPL',1,NULL,@productid, @UserId, getUTCDate()
end
if not exists(select 1  from Security.Role where RoleName='Manage Unified Settings property-level' and ProductId =@productid  and OrgPartyID IS NULL)
begin 
insert into Security.Role(RoleName,ShortName,Description,RoleTypeID,OrgPartyID,ProductId,CreatedBy,CreatedDate)
select 'Manage Unified Settings property-level','ManageUnifiedSettingsPropertyLevel','Manage Unified Settings property-level',1,NULL,@productid, @UserId, getUTCDate()
end
if not exists(select 1  from Security.Role where RoleName='Manage ALL Unified Settings' and ProductId =@productid  and OrgPartyID IS NULL)
begin 
insert into Security.Role(RoleName,ShortName,Description,RoleTypeID,OrgPartyID,ProductId,CreatedBy,CreatedDate)
select 'Manage ALL Unified Settings','ManageALLUnifiedSettings','Manage ALL Unified Settings',1,NULL,@productid, @UserId, getUTCDate()
end
if not exists(select 1  from Security.Role where RoleName='Standard Reporting' and ProductId =@productid  and OrgPartyID IS NULL)
begin 
insert into Security.Role(RoleName,ShortName,Description,RoleTypeID,OrgPartyID,ProductId,CreatedBy,CreatedDate)
select 'Standard Reporting','StandardReporting','Standard Reporting',1,NULL,@productid, @UserId, getUTCDate()
end
if not exists(select 1  from Security.Role where RoleName='Manage ALL Reporting' and ProductId =@productid  and OrgPartyID IS NULL)
begin 
insert into Security.Role(RoleName,ShortName,Description,RoleTypeID,OrgPartyID,ProductId,CreatedBy,CreatedDate)
select 'Manage ALL Reporting','ManageALLReporting','Manage ALL Reporting',1,NULL,@productid, @UserId, getUTCDate()
end

--Link rights to roles above
declare @roleId int
declare @rightId int

select @roleId = RoleId  from Security.Role where RoleName='Contributor to CIMPL property-level' and ProductId = @productid AND OrgPartyID IS NULL

select @rightId = RightId from Security.[Right] WHERE Value = 'Ability to Answer Questions for CIMPL' AND ProductId = 3
if not exists(select 1 from Security.RoleRight where RoleId=@roleId and RightId = @rightId)
begin
insert into Security.RoleRight(RoleId,RightId,CreatedBy,CreatedDate)
select @roleId,@rightId,@UserId,getUTCDate()
end

select @rightId = RightId from Security.[Right] WHERE Value = 'Access to Submit questionnaires within CIMPL'  AND ProductId = 3
if not exists(select 1 from Security.RoleRight where RoleId=@roleId and RightId = @rightId)
begin
insert into Security.RoleRight(RoleId,RightId,CreatedBy,CreatedDate)
select @roleId,@rightId,@UserId,getUTCDate()
end

select @rightId = RightId from Security.[Right] WHERE Value = 'View CIMPL Implementation Questions' AND ProductId = 3
if not exists(select 1 from Security.RoleRight where RoleId=@roleId and RightId = @rightId)
begin
insert into Security.RoleRight(RoleId,RightId,CreatedBy,CreatedDate)
select @roleId,@rightId,@UserId,getUTCDate()
end


select @roleId = RoleId  from Security.Role where RoleName='Manage ALL Implementations in CIMPL' and ProductId = @productid AND OrgPartyID IS NULL

select @rightId = RightId from Security.[Right] WHERE Value = 'Ability to Answer Questions for CIMPL' AND ProductId = 3
if not exists(select 1 from Security.RoleRight where RoleId=@roleId and RightId = @rightId)
begin
insert into Security.RoleRight(RoleId,RightId,CreatedBy,CreatedDate)
select @roleId,@rightId,@UserId,getUTCDate()
end

select @rightId = RightId from Security.[Right] WHERE Value = 'Access to Company-level questionnaires and Portfolio Views in CIMPL' AND ProductId = 3
if not exists(select 1 from Security.RoleRight where RoleId=@roleId and RightId = @rightId)
begin
insert into Security.RoleRight(RoleId,RightId,CreatedBy,CreatedDate)
select @roleId,@rightId,@UserId,getUTCDate()
end

select @rightId = RightId from Security.[Right] WHERE Value = 'Access to Submit questionnaires within CIMPL' AND ProductId = 3
if not exists(select 1 from Security.RoleRight where RoleId=@roleId and RightId = @rightId)
begin
insert into Security.RoleRight(RoleId,RightId,CreatedBy,CreatedDate)
select @roleId,@rightId,@UserId,getUTCDate()
end

select @rightId = RightId from Security.[Right] WHERE Value = 'Manage CIMPL Templates' AND ProductId = 3
if not exists(select 1 from Security.RoleRight where RoleId=@roleId and RightId = @rightId)
begin
insert into Security.RoleRight(RoleId,RightId,CreatedBy,CreatedDate)
select @roleId,@rightId,@UserId,getUTCDate()
end

select @rightId = RightId from Security.[Right] WHERE Value = 'Manage Personally Identifiable Information (PII) in CIMPL' AND ProductId = 3
if not exists(select 1 from Security.RoleRight where RoleId=@roleId and RightId = @rightId)
begin
insert into Security.RoleRight(RoleId,RightId,CreatedBy,CreatedDate)
select @roleId,@rightId,@UserId,getUTCDate()
end

select @rightId = RightId from Security.[Right] WHERE Value = 'View CIMPL Implementation Questions' AND ProductId = 3
if not exists(select 1 from Security.RoleRight where RoleId=@roleId and RightId = @rightId)
begin
insert into Security.RoleRight(RoleId,RightId,CreatedBy,CreatedDate)
select @roleId,@rightId,@UserId,getUTCDate()
end


select @roleId = RoleId  from Security.Role where RoleName='Manage Unified Settings property-level' and ProductId = @productid AND OrgPartyID IS NULL

select @rightId = RightId from Security.[Right] WHERE Value = 'Access to Unified Settings' AND ProductId = 3
if not exists(select 1 from Security.RoleRight where RoleId=@roleId and RightId = @rightId)
begin
insert into Security.RoleRight(RoleId,RightId,CreatedBy,CreatedDate)
select @roleId,@rightId,@UserId,getUTCDate()
end

select @rightId = RightId from Security.[Right] WHERE Value = 'View property-level settings' AND ProductId = 3
if not exists(select 1 from Security.RoleRight where RoleId=@roleId and RightId = @rightId)
begin
insert into Security.RoleRight(RoleId,RightId,CreatedBy,CreatedDate)
select @roleId,@rightId,@UserId,getUTCDate()
end

select @rightId = RightId from Security.[Right] WHERE Value = 'Manage property-level settings' AND ProductId = 3
if not exists(select 1 from Security.RoleRight where RoleId=@roleId and RightId = @rightId)
begin
insert into Security.RoleRight(RoleId,RightId,CreatedBy,CreatedDate)
select @roleId,@rightId,@UserId,getUTCDate()
end


select @roleId = RoleId  from Security.Role where RoleName='Manage ALL Unified Settings' and ProductId = @productid AND OrgPartyID IS NULL

select @rightId = RightId from Security.[Right] WHERE Value = 'Access to Unified Settings' AND ProductId = 3
if not exists(select 1 from Security.RoleRight where RoleId=@roleId and RightId = @rightId)
begin
insert into Security.RoleRight(RoleId,RightId,CreatedBy,CreatedDate)
select @roleId,@rightId,@UserId,getUTCDate()
end

select @rightId = RightId from Security.[Right] WHERE Value = 'View property-level settings' AND ProductId = 3
if not exists(select 1 from Security.RoleRight where RoleId=@roleId and RightId = @rightId)
begin
insert into Security.RoleRight(RoleId,RightId,CreatedBy,CreatedDate)
select @roleId,@rightId,@UserId,getUTCDate()
end

select @rightId = RightId from Security.[Right] WHERE Value = 'Manage property-level settings' AND ProductId = 3
if not exists(select 1 from Security.RoleRight where RoleId=@roleId and RightId = @rightId)
begin
insert into Security.RoleRight(RoleId,RightId,CreatedBy,CreatedDate)
select @roleId,@rightId,@UserId,getUTCDate()
end

select @rightId = RightId from Security.[Right] WHERE Value = 'View all company-level settings & templates' AND ProductId = 3
if not exists(select 1 from Security.RoleRight where RoleId=@roleId and RightId = @rightId)
begin
insert into Security.RoleRight(RoleId,RightId,CreatedBy,CreatedDate)
select @roleId,@rightId,@UserId,getUTCDate()
end

select @rightId = RightId from Security.[Right] WHERE Value = 'Manage company-level settings' AND ProductId = 3
if not exists(select 1 from Security.RoleRight where RoleId=@roleId and RightId = @rightId)
begin
insert into Security.RoleRight(RoleId,RightId,CreatedBy,CreatedDate)
select @roleId,@rightId,@UserId,getUTCDate()
end

select @rightId = RightId from Security.[Right] WHERE Value = 'Manage Settings Templates' AND ProductId = 3
if not exists(select 1 from Security.RoleRight where RoleId=@roleId and RightId = @rightId)
begin
insert into Security.RoleRight(RoleId,RightId,CreatedBy,CreatedDate)
select @roleId,@rightId,@UserId,getUTCDate()
end


select @roleId = RoleId  from Security.Role where RoleName='Standard Reporting' and ProductId = @productid AND OrgPartyID IS NULL

select @rightId = RightId from Security.[Right] WHERE Value = 'Access to Unified Reporting' AND ProductId = 3
if not exists(select 1 from Security.RoleRight where RoleId=@roleId and RightId = @rightId)
begin
insert into Security.RoleRight(RoleId,RightId,CreatedBy,CreatedDate)
select @roleId,@rightId,@UserId,getUTCDate()
end


select @roleId = RoleId  from Security.Role where RoleName='Manage ALL Reporting' and ProductId = @productid AND OrgPartyID IS NULL

select @rightId = RightId from Security.[Right] WHERE Value = 'Access to Unified Reporting' AND ProductId = 3
if not exists(select 1 from Security.RoleRight where RoleId=@roleId and RightId = @rightId)
begin
insert into Security.RoleRight(RoleId,RightId,CreatedBy,CreatedDate)
select @roleId,@rightId,@UserId,getUTCDate()
end

select @rightId = RightId from Security.[Right] WHERE Value = 'Manage company-level reporting' AND ProductId = 3
if not exists(select 1 from Security.RoleRight where RoleId=@roleId and RightId = @rightId)
begin
insert into Security.RoleRight(RoleId,RightId,CreatedBy,CreatedDate)
select @roleId,@rightId,@UserId,getUTCDate()
end

UPDATE Enterprise.ProductSettingType 
SET SensitiveData=1 WHERE Name='Kong_key'
GO