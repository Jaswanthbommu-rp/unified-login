BEGIN TRAN

-- Add ProductIcon product settings

IF NOT EXISTS (SELECT TOP 1 1 FROM Enterprise.ProductSettingType WHERE [Name] = 'ProductIcon')
BEGIN
	INSERT INTO Enterprise.ProductSettingType ([Name], [Description], SensitiveData)
	VALUES ('ProductIcon', 'Defines which product icon to use from the CDN', 0);
END

DECLARE @NOW DATETIME = GETUTCDATE();
DECLARE @productlist as table (entid int identity, productid int, productsettingtype varchar(500), productsettingvalue varchar(2000))
insert into @productlist values
(1,  'ProductIcon', 'onesite'),
(2,  'ProductIcon', 'unified-ui'),
(3,  'ProductIcon', 'unified-platform'),
(4,  'ProductIcon', 'asset-optimization'),
(5,  'ProductIcon', 'propertyware'),
(6,  'ProductIcon', 'lead2lease'),
(7,  'ProductIcon', 'yieldstar'),
(8,  'ProductIcon', 'realpage-accounting'),
(9,  'ProductIcon', 'marketing-center'),
(10, 'ProductIcon', 'prospect-contact-center'),
(11, 'ProductIcon', 'social'),
(12, 'ProductIcon', 'opsbid'),
(13, 'ProductIcon', 'spend-management'),
(14, 'ProductIcon', 'client-portal'),
(15, 'ProductIcon', 'renters-insurance'),
(16, 'ProductIcon', 'vendor-services'),
(17, 'ProductIcon', 'resident-portals'),
(18, 'ProductIcon', 'utility-management'),
(19, 'ProductIcon', 'learning-portal'),
(20, 'ProductIcon', 'realpage-document-management'),
(21, 'ProductIcon', 'leasing-and-rent-conversion-tool'),
(23, 'ProductIcon', 'on-site'),
(24, 'ProductIcon', 'research-application'),
(25, 'ProductIcon', 'self-provisioning-portal'),
(26, 'ProductIcon', 'unified-amenities'),
(27, 'ProductIcon', 'migration-tool'),
(28, 'ProductIcon', 'product-updates'),
(29, 'ProductIcon', 'business-intelligence'),
(30, 'ProductIcon', 'performance-analytics'),
(31, 'ProductIcon', 'investment-analytics'),
(32, 'ProductIcon', 'revenue-management'),
(33, 'ProductIcon', 'axiometrics'),
(35, 'ProductIcon', 'support-tool'),
(36, 'ProductIcon', 'easy-lms'),
(37, 'ProductIcon', 'property-photos'),
(38, 'ProductIcon', 'vendor-marketplace'),
(39, 'ProductIcon', 'integration-marketplace'),
(40, 'ProductIcon', 'intelligent-lead-management'),
(41, 'ProductIcon', 'ilm-leasing-analytics'),
(44, 'ProductIcon', 'portfolio-asset-management'),
(45, 'ProductIcon', 'cimpl'),
(47, 'ProductIcon', 'deposit-iq'),
(48, 'ProductIcon', 'payments'),
(49, 'ProductIcon', 'help-center'),
(50, 'ProductIcon', 'senior-lead-management'),
(51, 'ProductIcon', 'lro'),
(52, 'ProductIcon', 'amenity-analytics'),
(53, 'ProductIcon', 'ai-revenue-management'),
(54, 'ProductIcon', 'rent-control'),
(55, 'ProductIcon', 'renovation-manager'),
(57, 'ProductIcon', 'intelligent-building-trash'),
(58, 'ProductIcon', 'intelligent-building-energy'),
(59, 'ProductIcon', 'intelligent-building-water'),
(60, 'ProductIcon', 'resident-services'),
(62, 'ProductIcon', 'product-updates');

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
--settings data transfer from passwordpolicy table to orgsettings  table
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
	VALUES (@MaxControlId +1, @MCMasterControlId, 1, N'MarketingCenterProductAccessUsePrimaryPropertiesSwitchUIId', N'Use Primary Properties', N'usePrimaryProperties', 2, @UserId, @Now)

	SET IDENTITY_INSERT [UserManagement].[Control] OFF
END
Else
Begin
	Update [UserManagement].[Control] Set DataSource = 'usePrimaryProperties'
	Where ControlId = @MCUPPControlId
End

GO
------Create Product Setting type for IsUnifiedEmailEnabled -------------
DECLARE	@ProductSettingTypeId int,
		@ServerName SYSNAME = @@SERVERNAME

IF @ServerName IN ('RCDUSODBSQL001','rctusodbsql001','RCQUSODBSQL001')
BEGIN
	IF NOT EXISTS(SELECT * FROM Enterprise.ProductSettingType WHERE [NAME]='IsUnifiedEmailEnabled')
	BEGIN
	EXEC [Enterprise].[CreateProductSettingType]
			@ProductSettingTypeName = N'IsUnifiedEmailEnabled',
			@ProductSettingTypeDescription = N'Enable Unified Email to send email to unified platform',
			@ProductSettingTypeSensitiveData = 0,
			@ProductSettingTypeId = @ProductSettingTypeId OUTPUT

	SELECT	@ProductSettingTypeId as N'@ProductSettingTypeId'

	END

	------Create Product Setting type for UnifiedEmailBaseAddress -------------

	IF NOT EXISTS(SELECT * FROM Enterprise.ProductSettingType WHERE [NAME]='UnifiedEmailBaseAddress')
	BEGIN
	EXEC	[Enterprise].[CreateProductSettingType]
			@ProductSettingTypeName = N'UnifiedEmailBaseAddress',
			@ProductSettingTypeDescription = N'Unified Email base address endpoint',
			@ProductSettingTypeSensitiveData = 0,
			@ProductSettingTypeId = @ProductSettingTypeId OUTPUT

	SELECT	@ProductSettingTypeId as N'@ProductSettingTypeId'

	END
	------Create Product Setting type for UnifiedEmailEndPoint -------------

	IF NOT EXISTS(SELECT * FROM Enterprise.ProductSettingType WHERE [NAME]='UnifiedEmailEndPoint')
	BEGIN
	EXEC	[Enterprise].[CreateProductSettingType]
			@ProductSettingTypeName = N'UnifiedEmailEndPoint',
			@ProductSettingTypeDescription = N'Unified Email endpoint',
			@ProductSettingTypeSensitiveData = 0,
			@ProductSettingTypeId = @ProductSettingTypeId OUTPUT

	SELECT	@ProductSettingTypeId as N'@ProductSettingTypeId'
	END

	------Create Product Setting type for UseDefaultTemplate -------------

	IF NOT EXISTS(SELECT * FROM Enterprise.ProductSettingType WHERE [NAME]='UseDefaultTemplate')
	BEGIN
	EXEC	[Enterprise].[CreateProductSettingType]
			@ProductSettingTypeName = N'UseDefaultTemplate',
			@ProductSettingTypeDescription = N' Set to false if you wish to provide your own template/html.',
			@ProductSettingTypeSensitiveData = 0,
			@ProductSettingTypeId = @ProductSettingTypeId OUTPUT

	SELECT	@ProductSettingTypeId as N'@ProductSettingTypeId'
	END
END
GO

--Updating GetUserEndpoint for ILMLA and ILMLA products
DECLARE @ProductSettingTypeId INT
SELECT @ProductSettingTypeId = ProductSettingTypeId FROM Enterprise.ProductSettingType WHERE Name='GetUserEndpoint'
IF EXISTS(SELECT 1 FROM Enterprise.ProductSetting WHERE ProductId IN (40, 41) AND ProductSettingTypeId = @ProductSettingTypeId)
BEGIN
UPDATE Enterprise.ProductSetting SET value = '/{0}/users?loginName={1}'
WHERE ProductId IN (40, 41) AND ProductSettingTypeId = @ProductSettingTypeId
END
GO

-- Fix for some bad product setting data

UPDATE pc
	SET ThruDate = GETUTCDATE()
FROM enterprise.GlobalProductConfiguration gpc INNER JOIN enterprise.ProductConfiguration pc on pc.ConfigurationId = gpc.ConfigurationId INNER JOIN enterprise.ProductSetting ps ON ps.ProductSettingId = pc.ProductSettingId
WHERE gpc.ProductId != ps.ProductId
	AND pc.ThruDate IS NULL;
GO

GO

-- Adding Rights to HomeSharing Product	
DECLARE @UserId INT
SELECT @UserId = UserId FROM Ident.UserLogin WHERE LoginName like 'realpagead@%'

IF NOT EXISTS(SELECT 1 FROM Security.[Right] WHERE RightName = 'PropertyAdmin')
BEGIN
	INSERT INTO Security.[Right](RightName, Description, Value, StatusTypeId, VisibilityStatusId, ProductId, TargetProductId, CreatedBy, CreatedDate)
	VALUES('PropertyAdmin', 'Property Admin', 'Property Admin', 13, 9, 60, 60, @UserId, GETDATE())
END

IF NOT EXISTS(SELECT 1 FROM Security.[Right] WHERE RightName = 'PropertyUser')
BEGIN
	INSERT INTO Security.[Right](RightName, Description, Value, StatusTypeId, VisibilityStatusId, ProductId, TargetProductId, CreatedBy, CreatedDate)
	VALUES('PropertyUser', 'Property User', 'Property User', 13, 9, 60, 60, @UserId, GETDATE())
END

DECLARE @PropertyAdminRoleId INT
DECLARE @PropertyUserRoleId INT		
DECLARE @PropertyAdminRightId INT
DECLARE @PropertyUserRightId INT		

SELECT @PropertyAdminRightId  = RightId FROM Security.[Right] WHERE RightName = 'PropertyAdmin' AND ProductId = 60;
SELECT @PropertyUserRightId = RightId FROM Security.[Right] WHERE RightName='PropertyUser' AND ProductId = 60;

IF NOT EXISTS(SELECT 1 FROM Security.Role WHERE RoleName = 'Property Admin' AND OrgPartyID IS NULL AND ProductId = 60)
BEGIN
	INSERT INTO Security.Role(RoleName, ShortName, Description, RoleTypeID, OrgPartyID, ProductId, CreatedBy, CreatedDate)
	VALUES('Property Admin', 'Property Admin', 'Property Admin', 3, NULL, 60, @UserId, GETDATE())	
END	

SELECT @PropertyAdminRoleId = RoleId FROM Security.Role WHERE RoleName = 'Property Admin' AND OrgPartyID IS NULL AND ProductId = 60

IF NOT EXISTS(SELECT 1 FROM Security.RoleRight WHERE RoleId = @PropertyAdminRoleId AND RightId = @PropertyAdminRightId)
BEGIN
	INSERT INTO Security.RoleRight(RoleId, RightId, CreatedBy, CreatedDate)
	VALUES(@PropertyAdminRoleId, @PropertyAdminRightId, @UserId, GETDATE())
END

IF NOT EXISTS(SELECT 1 FROM Security.Role WHERE RoleName = 'Property User' AND OrgPartyID IS NULL AND ProductId = 60)
BEGIN
	INSERT INTO Security.Role(RoleName, ShortName, Description, RoleTypeID, OrgPartyID, ProductId, CreatedBy, CreatedDate)
	VALUES('Property User', 'Property User', 'Property User', 3, NULL, 60, @UserId, GETDATE())	
END

SELECT @PropertyUserRoleId = RoleId FROM Security.Role WHERE RoleName = 'Property User' AND OrgPartyID IS NULL AND ProductId = 60
	
IF NOT EXISTS(SELECT 1 FROM Security.RoleRight WHERE RoleId = @PropertyUserRoleId AND RightId = @PropertyUserRightId)
BEGIN
	INSERT INTO Security.RoleRight(RoleId, RightId, CreatedBy, CreatedDate)
	VALUES(@PropertyUserRoleId, @PropertyUserRightId, @UserId, GETDATE())
END

BEGIN TRAN

-- Add ProductIntegrationType product settings

IF NOT EXISTS (SELECT TOP 1 1 FROM Enterprise.ProductSettingType WHERE [Name] = 'ProductIntegrationType')
BEGIN
	INSERT INTO Enterprise.ProductSettingType ([Name], [Description], SensitiveData)
	VALUES ('ProductIntegrationType', 'Defines the integration that a specific product is to use', 0);
END

DECLARE @NOW DATETIME = GETUTCDATE();
DECLARE @productlist as table (entid int identity, productid int, productsettingtype varchar(500), productsettingvalue varchar(2000))
insert into @productlist values
(45, 'ProductIntegrationType', 'UPFM'),
(56, 'ProductIntegrationType', 'UPFM'),
(57, 'ProductIntegrationType', 'UPFM'),
(58, 'ProductIntegrationType', 'UPFM'),
(59, 'ProductIntegrationType', 'UPFM'),
(60, 'ProductIntegrationType', 'UPFM'),
(63, 'ProductIntegrationType', 'UPFM'),
(65, 'ProductIntegrationType', 'UPFM'),
(68, 'ProductIntegrationType', 'UPFM');

INSERT INTO @productlist
SELECT p.ProductId, 'ProductIntegrationType', 'Legacy'
FROM Enterprise.Product p
	LEFT JOIN @productlist pl on pl.productid = p.ProductId
WHERE p.ProductId NOT IN (22, 34, 42)
	AND pl.productid IS NULL;

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
 --Adding Product Setting Type for ILM & ILM LA
			   
  BEGIN TRAN

-- Add ProductIcon product settings

IF NOT EXISTS (SELECT TOP 1 1 FROM Enterprise.ProductSettingType WHERE [Name] = 'CreateUpdateMultiCompanyUserRequiresPMC')
BEGIN
	INSERT INTO Enterprise.ProductSettingType ([Name], [Description], SensitiveData)
	VALUES ('CreateUpdateMultiCompanyUserRequiresPMC', 'Create Update MultiCompany User Requires PMC', 0);
END

DECLARE @NOW DATETIME = GETUTCDATE();
declare @productlist table ( entid int identity, productid int, productsettingtype varchar(500), productsettingvalue varchar(100))
insert into @productlist values
(40,  'CreateUpdateMultiCompanyUserRequiresPMC','1'),
(41,  'CreateUpdateMultiCompanyUserRequiresPMC','1');

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
--TFS: 702072 populate UnifiedSettingPicklist table
if not exists(select top 1 1 from Enterprise.SettingPicklist where CategoryName = 'CustomFields' and MappingName =  'Alphanumeric')
Begin 
	Insert into Enterprise.SettingPicklist(CategoryName, MappingName, MappingValue, Description, ModifiedBy, ModifiedDate)
	values ('CustomFields', 'Alphanumeric', 1, 'consists of both letters and numerals', 480, GETDATE())
End

if not exists(select top 1 1 from Enterprise.SettingPicklist where CategoryName = 'CustomFields' and MappingName =  'Numeric')
Begin 
	Insert into Enterprise.SettingPicklist(CategoryName, MappingName, MappingValue, Description, ModifiedBy, ModifiedDate)
	values ('CustomFields', 'Numeric', 2, 'consists of only numerals', 480, GETDATE())
End
Go


--Add Suggest Properties button
BEGIN TRAN

IF NOT EXISTS (SELECT TOP 1 1 FROM Enterprise.ProductSettingType WHERE [Name] = 'SuggestPrimaryProperties')
BEGIN
	INSERT INTO Enterprise.ProductSettingType ([Name], [Description], SensitiveData)
	VALUES ('SuggestPrimaryProperties', 'Used for suggesting primary properties for product', 0);
END

DECLARE @NOW DATETIME = GETUTCDATE();
DECLARE @productlist as table (entid int identity, productid int, productsettingtype varchar(500), productsettingvalue varchar(2000))

INSERT INTO @productlist(productid, productsettingtype, productsettingvalue)
SELECT p.ProductId, 'SuggestPrimaryProperties', '0'
FROM Enterprise.Product p

WHERE p.ProductId NOT IN (2, 5, 7, 11, 12, 19, 21, 22, 27, 36, 55, 64)

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

   --Panel Script for Smart Energy
DECLARE @UserId bigint,
       @ProductId int = 58,
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

		SELECT @MaxControlId = MAX(ControlId) FROM UserManagement.Control

		INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate]) 
		VALUES (@MaxControlId + 1, NULL, 8, N'IntelligentBuildingEnergyUIId', NULL, NULL, 1, @UserId, @Now)
		
		INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate]) 
		VALUES (@MaxControlId + 2, @MaxControlId + 1, 9, N'IntelligentBuildingEnergyAccessRolesTabUIId', N'Roles', NULL, 1, @UserId, @Now)
		

		INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate]) 
		VALUES (@MaxControlId + 3, @MaxControlId + 2, 2, N'IntelligentBuildingEnergyAccessRolesSelectGridUIId', NULL, NULL, 2, @UserId, @Now)

		INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate]) 
		VALUES (@MaxControlId + 4, @MaxControlId + 3, 7, N'IntelligentBuildingEnergyAccessRadioUIId', NULL, N'isAssigned', 1, @UserId, @Now)

		INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate]) 
		VALUES (@MaxControlId + 5, @MaxControlId + 3, 5, N'IntelligentBuildingEnergyAccessRoleLabelUIId', N'Role', N'name', 2, @UserId, @Now)

		INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate]) 
		VALUES (@MaxControlId + 6, @MaxControlId + 3, 5, N'IntelligentBuildingEnergyAccessRoleTypeLabelUIId', N'Role Type', N'roletype', 3, @UserId, @Now)
		
		INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate]) 
		VALUES (@MaxControlId + 7, @MaxControlId + 3, 11, N'IntelligentBuildingEnergyAccessIconUIId', NULL, N'InfoIcon', 4, @UserId, @Now)
		
		INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate]) 
		VALUES (@MaxControlId + 8, @MaxControlId + 1, 9, N'IntelligentBuildingEnergyAccessPropertiesTabUIId', N'Properties', NULL, 2, @UserId, @Now)
		
		INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate]) 
		VALUES (@MaxControlId + 9, @MaxControlId + 8, 1, N'IntelligentBuildingEnergyAccessAllowaccesstoallcurrentandfuturepropertiesPropertiesSwitchUIId', N'Assign access to current and new properties automatically', N'allProperties', 1, @UserId, @Now)

		INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate]) 
		VALUES (@MaxControlId + 10, @MaxControlId + 8, 3, N'IntelligentBuildingEnergyAccessPropertiesMultiSelectGridUIId', NULL, NULL, 2, @UserId, @Now)

		INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate]) 
		VALUES (@MaxControlId + 11, @MaxControlId + 10, 10, N'IntelligentBuildingEnergyAccessCheckboxUIId', NULL, N'isAssigned', 1, @UserId, @Now)

		INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate]) 
		VALUES (@MaxControlId + 12, @MaxControlId + 10, 5, N'IntelligentBuildingEnergyAccessPropertyLabelUIId', N'Property', N'name', 2, @UserId, @Now)

		INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate]) 
		VALUES (@MaxControlId + 13, @MaxControlId + 10, 5, N'IntelligentBuildingEnergyAccessCityLabelUIId', N'City', N'city', 3, @UserId, @Now)

		INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate]) 
		VALUES (@MaxControlId + 14, @MaxControlId + 10, 5, N'IntelligentBuildingEnergyAccessStateLabelUIId', N'State', N'state', 4, @UserId, @Now)

		INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate]) 
		VALUES (@MaxControlId + 15, @MaxControlId + 7, 5, N'IntelligentBuildingEnergyAccessRoleDetailsLabelUIId', N'Role Details', NULL, 1, @UserId, @Now)

		INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate]) 
		VALUES (@MaxControlId + 16, @MaxControlId + 7, 12, N'IntelligentBuildingEnergyAccessGridUIId', N'NULL', NULL, 1, @UserId, @Now)

		INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate]) 
		VALUES (@MaxControlId + 17, @MaxControlId + 16, 5, N'IntelligentBuildingEnergyAccessRightLabelUIId', N'Right', 'description', 1, @UserId, @Now)

		 
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

		DECLARE @MaxProductPageId INT
		SELECT @MaxProductPageId = MAX(ProductPageId) FROM [UserManagement].[ProductPage]

		INSERT [UserManagement].[ProductPage] ([ProductPageId], [ProductId], [DisplayName], [CreatedBy], [CreatedDate], [IsActive], [ProductPageTypeId]) 
		VALUES (@MaxProductPageId + 1, 58, N'Smart Energy Product Access', @UserId, @Now, 1, 1)

		SET IDENTITY_INSERT [UserManagement].[ProductPage] OFF

		SET IDENTITY_INSERT [UserManagement].[ProductPageControl] ON 

		DECLARE @MaxProductPageControlId INT
		SELECT @MaxProductPageControlId = MAX(ProductPageControlId) FROM [UserManagement].[ProductPageControl]

		INSERT [UserManagement].[ProductPageControl] ([ProductPageControlId], [ProductPageId], [ControlId], [CreatedBy], [CreatedDate])
		VALUES (@MaxProductPageControlId + 1, @MaxProductPageId + 1, @MaxControlId + 1, @UserId, @Now)

		SET IDENTITY_INSERT [UserManagement].[ProductPageControl] OFF
            
END

DECLARE @ServerName SYSNAME = @@SERVERNAME;

IF (@ServerName ='RCDUSODBSQL001' AND EXISTS(SELECT 1 FROM Ident.SamlProductSettings where ProductId = 58 and LoginUri ='www.dev-abcenergy.realpage.com'))
BEGIN
	UPDATE Ident.SamlProductSettings SET LoginUri = 'https://dev-boss-energy.realpage.com/' WHERE ProductId = 58 
END

IF (@ServerName ='rctusodbsql001' AND EXISTS(SELECT 1 FROM Ident.SamlProductSettings where ProductId = 58 and LoginUri ='www.qa-abcenergy.realpage.com'))
BEGIN
	UPDATE Ident.SamlProductSettings SET LoginUri = 'https://qa-boss-energy.realpage.com/' WHERE ProductId = 58 
END

IF (@ServerName ='RCQUSODBSQL001' AND EXISTS(SELECT 1 FROM Ident.SamlProductSettings where ProductId = 58 and LoginUri ='www.sat-abcenergy.realpage.com'))
BEGIN
	UPDATE Ident.SamlProductSettings SET LoginUri = ' https://sat-boss-energy.realpage.com/' WHERE ProductId = 58 
END

IF (@ServerName IN ('RCPGBKDBSQL005A', 'RCPGBKDBSQL005B') AND EXISTS(SELECT 1 FROM Ident.SamlProductSettings where ProductId = 58 and LoginUri ='www.abcenergy.realpage.com'))
BEGIN
	UPDATE Ident.SamlProductSettings SET LoginUri = 'https://smart-energy.realpage.com/' WHERE ProductId = 58 
END

IF EXISTS(SELECT 1 FROM Enterprise.Product where ProductId = 58 AND Name = N'Intelligent Building Energy' AND Description=N'Intelligent Building Energy' )
BEGIN
   UPDATE Enterprise.Product SET Name= N'Smart Energy', Description= N'Smart Energy' where ProductId = 58 
END

-- Adding default roles to Smart Energy Product
IF NOT EXISTS(SELECT 1 FROM Security.Role WHERE ProductId = 58)
BEGIN
	INSERT INTO Security.Role(RoleName, ShortName, Description, RoleTypeID, OrgPartyID, ProductId, CreatedBy, CreatedDate )
	VALUES('Portfolio Manager','PortfolioManager','Portfolio Manager', 1, NULL, 58, @UserId, GETDATE())	

	INSERT INTO Security.Role(RoleName, ShortName, Description, RoleTypeID, OrgPartyID, ProductId, CreatedBy, CreatedDate )
	VALUES('Property Manager','PropertyManager','Property Manager', 1, NULL, 58, @UserId,GETDATE())	
END

IF NOT EXISTS(SELECT 1 FROM Security.[Right] WHERE ProductId = 58)
BEGIN
	INSERT INTO Security.[Right](RightName, Description, Value, StatusTypeId, VisibilityStatusId, ProductId, TargetProductId, CreatedBy, CreatedDate)
	VALUES('ReadOnly', NULL, 'Read Only Access',13, 9, 58, 58, @UserId, GETDATE())	
END

DECLARE @PortfolioManagerRoleId INT;
DECLARE @PropertyManagerRoleId INT;
DECLARE @RightId INT;

SELECT @PortfolioManagerRoleId = RoleId FROM Security.Role WHERE RoleName = 'Portfolio Manager' AND ProductId = 58
SELECT @PropertyManagerRoleId = RoleId FROM Security.Role WHERE RoleName = 'Property Manager' AND ProductId = 58
SELECT @RightId = RightId FROm Security.[Right] WHERE RightName = 'ReadOnly' AND ProductId = 58 AND TargetProductId = 58
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

GO

--TFS -716888 : New product setting type
IF NOT EXISTS (select top  1 1 from Enterprise.ProductSettingType WHERE NAME = 'UpdateProductInUDM')
BEGIN
	INSERT INTO Enterprise.ProductSettingType ( Name, Description, SensitiveData)
	VALUES ('UpdateProductInUDM', 'Update product in UDM or not.', 0)
END

GO
DECLARE @settingTypeId INT = 0;
SELECT @settingTypeId = ProductSettingTypeId FROM Enterprise.ProductSettingType WHERE NAME = 'UpdateProductInUDM'
IF @settingTypeId > 0
BEGIN
	CREATE TABLE #Temp(ProductId INT);
	INSERT INTO #Temp
	Values 
		(1),(4),(6),(8),(9),(10),(13),(15),(16),
		(17),(18),(20),(23),(26),(36),(37),(40),
		(41),(44),(45),(47),(48),(50),(56),(57),
		(58),(59),(60),(63),(65),(66),(68),(29),
		(30),(31),(32),(33),(34),(51),(52),(53),
		(54),(66)

	WHILE (Select COUNT(*) FROM #Temp) > 0
	BEGIN
		DECLARE @id int;
		SELECT TOP 1 @id= ProductId from #Temp;
		
		IF NOT EXISTS (
			SELECT TOP 1 1
			FROM Enterprise.ProductSetting PS
			JOIN Enterprise.ProductSettingType PST on PST.ProductSettingTypeId = PS.ProductSettingTypeId
			WHERE ProductId = @id
			AND PST.ProductSettingTypeId = @settingTypeId)
		BEGIN
			EXEC Enterprise.SetProductSetting 0, @id, @settingTypeId, N'1'
		END
		
		DELETE From #Temp WHERE ProductId = @id
	END

	SELECT ProductId INTO #Temp2
	FROM Enterprise.Product
	WHERE ProductId NOT IN (SELECT ProductId FROM #Temp)

	WHILE (Select COUNT(*) FROM #Temp2) > 0
	BEGIN
		DECLARE @id1 int;
		SELECT TOP 1 @id1 = ProductId from #Temp2;
		
		IF NOT EXISTS (
			SELECT TOP 1 1
			FROM Enterprise.ProductSetting PS
			JOIN Enterprise.ProductSettingType PST on PST.ProductSettingTypeId = PS.ProductSettingTypeId
			WHERE ProductId = @id1
			AND PST.ProductSettingTypeId = @settingTypeId)
		BEGIN
			EXEC Enterprise.SetProductSetting 0, @id1, @settingTypeId, N'0'
		END
		
		DELETE From #Temp2 WHERE ProductId = @id1
	END

	DROP Table #Temp
	DROP Table #Temp2
END

--TFS -716888 end
GO
DECLARE @UserId bigint,
	@Now datetime = GETDATE(),
	@RightId int,
	@PartyId int,
	@RightVisibilityStatusId int =9,
	@RouteId int,
	@ServerName SYSNAME = @@SERVERNAME;

SELECT	@UserId = UserId
FROM	Ident.UserLogin
WHERE	LoginName LIKE 'realpagead@%'

Select @RouteId=RouteId 
FROM	[Security].[Route]
Where	RouteValue='SideMenu'

IF NOT EXISTS(select 1 from [Security].[Right] where RightName='ManageCompanyLevelReporting')
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
					'ManageCompanyLevelReporting',
					'Manage company-level reporting',
					'Manage company-level reporting',
					13, 
					@RightVisibilityStatusId,
					3,
					67,
					@UserId,
					@Now
				   )

				
END
 SELECT @RightId=RightId from [Security].[Right] WHERE RightName='ManageCompanyLevelReporting'

  -- Add Route with right
 IF NOT EXISTS (SELECT TOP 1 1 FROM [Security].[RightRoute] WHERE RightId=@RightId and RouteId=@RouteId)
	BEGIN
		INSERT INTO [Security].[RightRoute](
			RightId, 
			RouteId,
			RightName,
			CreatedBy,
			CreatedDate
		)
		VALUES ( 
				@RightId,
				@RouteId,
				'Manage company-level reporting',
				@UserId,
				@Now
				)
	END;

 -- Add Role with right
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


--TFS: 702072 populate UnifiedSettingPicklist table
if not exists(select top 1 1 from Enterprise.SettingPicklist where CategoryName = 'CustomFields' and MappingName =  'Alphanumeric')
Begin 
	Insert into Enterprise.SettingPicklist(CategoryName, MappingName, MappingValue, Description, ModifiedBy, ModifiedDate)
	values ('CustomFields', 'Alphanumeric', 1, 'consists of both letters and numerals', 480, GETDATE())
End

if not exists(select top 1 1 from Enterprise.SettingPicklist where CategoryName = 'CustomFields' and MappingName =  'Numeric')
Begin 
	Insert into Enterprise.SettingPicklist(CategoryName, MappingName, MappingValue, Description, ModifiedBy, ModifiedDate)
	values ('CustomFields', 'Numeric', 2, 'consists of only numerals', 480, GETDATE())
End
Go

if not exists(select top 1 1 from [Settings].[SettingCategoryType] Where Name = 'Security')
Begin 
	Insert into [Settings].[SettingCategoryType](Name)
	values ('Security')
End

if not exists(select top 1 1 from [Settings].[SettingCategoryType] Where Name = 'CustomFields')
Begin 
	Insert into [Settings].[SettingCategoryType](Name)
	values ('CustomFields')
End
Go
--Settings	data conversion
DECLARE @UserId bigint,
	@ProductId int ,
	@SettingCategoryTypeId smallint,
	@Now datetime = GETDATE()

SELECT	@UserId = UserId
FROM	ident.UserLogin
WHERE	LoginName LIKE 'realpagead@%'

Select @SettingCategoryTypeId = SettingCategoryTypeId
From [Settings].[SettingCategoryType]
Where Name = 'Security'

-- 'NumberOfPasswordsToRemember'

	INSERT INTO [Settings].[OrganizationSettings] (PartyId,SettingCategoryTypeId,MappingName,
		MappingValue,Editable,[Hidden],CreatedBy,CreatedDate)
	Select PartyId,@SettingCategoryTypeId,'NumberOfPasswordsToRemember',
		NumberOfPasswordsToRemember,1,0,@UserId,@Now
	From [Ident].[PasswordPolicy]  
	where PartyId  not in (select PartyId From Settings.OrganizationSettings where   MappingName = 'NumberOfPasswordsToRemember')

--'PreventPasswordReuse'

	INSERT INTO [Settings].[OrganizationSettings] (PartyId,SettingCategoryTypeId,MappingName,
		MappingValue,Editable,[Hidden],CreatedBy,CreatedDate)
	Select PartyId,@SettingCategoryTypeId,'PreventPasswordReuse',
		PreventPasswordReuse,1,1,@UserId,@Now
	From [Ident].[PasswordPolicy]  
	where PartyId  not in (select PartyId From Settings.OrganizationSettings where   MappingName = 'PreventPasswordReuse')


--'PasswordExpirationPeriodInDays'

	INSERT INTO [Settings].[OrganizationSettings] (PartyId,SettingCategoryTypeId,MappingName,
		MappingValue,Editable,[Hidden],CreatedBy,CreatedDate)
	Select PartyId,@SettingCategoryTypeId,'PasswordExpirationPeriodInDays',
		PasswordExpirationPeriodInDays,1,0,@UserId,@Now
	From [Ident].[PasswordPolicy]  
	where PartyId  not in (select PartyId From Settings.OrganizationSettings where   MappingName = 'PasswordExpirationPeriodInDays')


-- 'EnablePasswordExpiration'

	INSERT INTO [Settings].[OrganizationSettings] (PartyId,SettingCategoryTypeId,MappingName,
		MappingValue,Editable,[Hidden],CreatedBy,CreatedDate)
	Select PartyId,@SettingCategoryTypeId,'EnablePasswordExpiration',
		EnablePasswordExpiration,1,1,@UserId,@Now
	From [Ident].[PasswordPolicy]  
	where PartyId  not in (select PartyId From Settings.OrganizationSettings where   MappingName = 'EnablePasswordExpiration')


--'AllowUsersToChangeOwnPassword'

	INSERT INTO [Settings].[OrganizationSettings] (PartyId,SettingCategoryTypeId,MappingName,
		MappingValue,Editable,[Hidden],CreatedBy,CreatedDate)
	Select PartyId,@SettingCategoryTypeId,'AllowUsersToChangeOwnPassword',
		AllowUsersToChangeOwnPassword,1,1,@UserId,@Now
	From [Ident].[PasswordPolicy]  
	where PartyId  not in (select PartyId From Settings.OrganizationSettings where   MappingName = 'AllowUsersToChangeOwnPassword')


--'MinimumSpecialCharacter'

	INSERT INTO [Settings].[OrganizationSettings] (PartyId,SettingCategoryTypeId,MappingName,
		MappingValue,Editable,[Hidden],CreatedBy,CreatedDate)
	Select PartyId,@SettingCategoryTypeId,'MinimumSpecialCharacter',
		MinimumSpecialCharacter,1,1,@UserId,@Now
	From [Ident].[PasswordPolicy]
	where PartyId  not in (select PartyId From Settings.OrganizationSettings where   MappingName = 'MinimumSpecialCharacter')

--'MinimumNumeric'

	INSERT INTO [Settings].[OrganizationSettings] (PartyId,SettingCategoryTypeId,MappingName,
		MappingValue,Editable,[Hidden],CreatedBy,CreatedDate)
	Select PartyId,@SettingCategoryTypeId,'MinimumNumeric',
		MinimumNumeric,1,1,@UserId,@Now
	From [Ident].[PasswordPolicy]  
	where PartyId  not in (select PartyId From Settings.OrganizationSettings where   MappingName = 'MinimumNumeric')


--'MinimumUppercase'

	INSERT INTO [Settings].[OrganizationSettings] (PartyId,SettingCategoryTypeId,MappingName,
		MappingValue,Editable,[Hidden],CreatedBy,CreatedDate)
	Select PartyId,@SettingCategoryTypeId,'MinimumUppercase',
		MinimumUppercase,1,1,@UserId,@Now
	From [Ident].[PasswordPolicy]  
	where PartyId  not in (select PartyId From Settings.OrganizationSettings where   MappingName = 'MinimumUppercase')


-- 'MinimumLowercase'

	INSERT INTO [Settings].[OrganizationSettings] (PartyId,SettingCategoryTypeId,MappingName,
		MappingValue,Editable,[Hidden],CreatedBy,CreatedDate)
	Select PartyId,@SettingCategoryTypeId,'MinimumLowercase',
		MinimumLowercase,1,1,@UserId,@Now
	From [Ident].[PasswordPolicy]  
	where PartyId  not in (select PartyId From Settings.OrganizationSettings where   MappingName = 'MinimumLowercase')


--'MaximumLength'

	INSERT INTO [Settings].[OrganizationSettings] (PartyId,SettingCategoryTypeId,MappingName,
		MappingValue,Editable,[Hidden],CreatedBy,CreatedDate)
	Select PartyId,@SettingCategoryTypeId,'MaximumLength',
		MaximumLength,1,1,@UserId,@Now
	From [Ident].[PasswordPolicy]  
	where PartyId  not in (select PartyId From Settings.OrganizationSettings where   MappingName = 'MaximumLength')


--'MinimumLength'

	INSERT INTO [Settings].[OrganizationSettings] (PartyId,SettingCategoryTypeId,MappingName,
		MappingValue,Editable,[Hidden],CreatedBy,CreatedDate)
	Select PartyId,@SettingCategoryTypeId,'MinimumLength',
		MinimumLength,1,0,@UserId,@Now
	From [Ident].[PasswordPolicy]  
	where PartyId  not in (select PartyId From Settings.OrganizationSettings where   MappingName = 'MinimumLength')

	GO

	
	--Data transfer existing custom fields
	
DECLARE @NOW DATETIME = GETUTCDATE();
declare @CustomFields as TABLE(
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[FieldId] [bigint] NOT NULL,
	[OrganizationId] [bigint] NOT NULL,	
	[Enabled] [bit] NOT NULL,
	[Name] [nvarchar](100) NOT NULL,
	[Description] [nvarchar](500) NULL,
	[FieldTypeId] [tinyint] NOT NULL,
	[Required] [bit] NULL,
	[ReadOnly] [bit] NULL,
	[Sequence] [smallint] NOT NULL,	
	[MinCharLength] [int] NULL,
	[MaxCharLength] [int] NULL,
	[CreatedBy] bigint not null
)

insert into @CustomFields(FieldId,OrganizationId,Enabled,Name,Description,FieldTypeId,Required,
	ReadOnly,Sequence,MinCharLength,MaxCharLength,CreatedBy)
Select FieldId,OrganizationId,Enabled,Name,Description,FieldTypeId,Required,
	ReadOnly,Sequence,MinCharLength,MaxCharLength,CreatedBy
From CustomField.Field

declare @MAX_ID INT
declare @Current_ID INT = 1
declare @SettingCategoryTypeId smallint

select @SettingCategoryTypeId = SettingCategoryTypeId
from [Settings].[SettingCategoryType] Where [Name] = 'CustomFields'

select @MAX_ID = max(id) from @CustomFields

SET IDENTITY_INSERT [Settings].[SettingTableRow] ON 

while @Current_ID <= @MAX_ID
begin
	declare @partyid bigint,@fieldid bigint,@enabled bit,@name nvarchar(200),
			@Description nvarchar(200), @Required bit,@ReadOnly bit,
			@Sequence smallint,@MinCharLength int,@MaxCharLength int,
			@CreatedBy bigint, @SettingTableId bigint,@fieldTypeId smallint

	Select @partyid = OrganizationId,@fieldid = FieldId,
		   @enabled = Enabled,@name = Name,@Description = Description,
		   @ReadOnly = ReadOnly,@Required = Required,@Sequence = Sequence,
		   @MaxCharLength = MaxCharLength, @MinCharLength = MinCharLength,
		   @CreatedBy = CreatedBy, @fieldTypeId = FieldTypeId
	From @CustomFields Where Id = @Current_ID

	IF NOT EXISTS (Select 1 From  [Settings].[SettingTable] Where PartyId = @partyid)
	BEGIN
		INSERT INTO [Settings].[SettingTable]([SettingCategoryTypeId],[PartyId],
				[TableName],[ModifiedBy],[CreatedDate])
		Select @SettingCategoryTypeId,@partyid,'Customfields'+ CONVERT(varchar(10),@fieldid),@CreatedBy,@NOW

		set @SettingTableId = SCOPE_IDENTITY();
	END
	ELSE
	BEGIN
		Select @SettingTableId = SettingTableId From  [Settings].[SettingTable] Where PartyId = @partyid
	END

	IF NOT EXISTS (Select 1 From  [Settings].[SettingTableRow] Where SettingTableId = @SettingTableId)
	BEGIN
		INSERT INTO [Settings].[SettingTableRow](SettingTableRowId,SettingTableId,Editable,Deletable,
		IsActive,ModifiedBy,CreatedDate)
		Select @fieldid,@SettingTableId,1,1,1,@CreatedBy,@NOW
	END
	

	IF NOT EXISTS (Select 1 From  [Settings].[SettingTableColumn] Where SettingTableRowId = @fieldid)
	BEGIN
		INSERT INTO [Settings].[SettingTableColumn](SettingTableRowId,TableColumnName,
		TableColumnValue,ModifiedBy,CreatedDate)
		SELECT @fieldid,'Enabled',@enabled,@CreatedBy,@NOW

		INSERT INTO [Settings].[SettingTableColumn](SettingTableRowId,TableColumnName,
			TableColumnValue,ModifiedBy,CreatedDate)
		SELECT @fieldid,'Name',@name,@CreatedBy,@NOW


		INSERT INTO [Settings].[SettingTableColumn](SettingTableRowId,TableColumnName,
			TableColumnValue,ModifiedBy,CreatedDate)
		SELECT @fieldid,'FieldTypeId',@fieldTypeId,@CreatedBy,@NOW

		INSERT INTO [Settings].[SettingTableColumn](SettingTableRowId,TableColumnName,
			TableColumnValue,ModifiedBy,CreatedDate)
		SELECT @fieldid,'Required',@Required,@CreatedBy,@NOW

		INSERT INTO [Settings].[SettingTableColumn](SettingTableRowId,TableColumnName,
			TableColumnValue,ModifiedBy,CreatedDate)
		SELECT @fieldid,'ReadOnly',@ReadOnly,@CreatedBy,@NOW

		INSERT INTO [Settings].[SettingTableColumn](SettingTableRowId,TableColumnName,
			TableColumnValue,ModifiedBy,CreatedDate)
		SELECT @fieldid,'Sequence',@Sequence,@CreatedBy,@NOW

		INSERT INTO [Settings].[SettingTableColumn](SettingTableRowId,TableColumnName,
			TableColumnValue,ModifiedBy,CreatedDate)
		SELECT @fieldid,'MinCharLength',@MinCharLength,@CreatedBy,@NOW

		INSERT INTO [Settings].[SettingTableColumn](SettingTableRowId,TableColumnName,
			TableColumnValue,ModifiedBy,CreatedDate)
		SELECT @fieldid,'MaxCharLength',@MaxCharLength,@CreatedBy,@NOW
	END	

	set @Current_ID = @Current_ID + 1
end

SET IDENTITY_INSERT [Settings].[SettingTableRow] OFF 

GO

Declare @CustomFieldVlaues table(
   Id int identity(1,1),
   UserLoginPersonaId bigint,
   FieldId bigint,
   [Value] nvarchar(max),
   CreatedDate Datetime,
   CreatedBy bigint)

  Insert into @CustomFieldVlaues(UserLoginPersonaId,FieldId,[Value],CreatedBy,CreatedDate)
  Select UserLoginPersonaId,FieldId,[Value],CreatedBy,CreatedDate
  FROM [CustomField].[FieldValue]

  declare @MAX_ID INT
  declare @Current_ID INT = 1
  select @MAX_ID = max(id) from @CustomFieldVlaues
  while @Current_ID <= @MAX_ID
  begin
	declare @UserLoginPersonaId bigint,
			@fieldid bigint,
			@CreatedBy bigint,
			@Value nvarchar(max),
			@CreatedDate datetime

	Select @UserLoginPersonaId = UserLoginPersonaId,
			@fieldid = FieldId,@Value = [value],
			@CreatedBy = CreatedBy,@CreatedDate = CreatedDate
	From @CustomFieldVlaues Where id = @Current_ID

	IF NOT EXISTS (Select 1 From  [Settings].[SettingTableRowValue] 
		Where UserLoginPersonaId = @UserLoginPersonaId 
		AND SettingTableRowId = @fieldid
		AND [Value] = @Value)
	BEGIN
		INSERT INTO [Settings].[SettingTableRowValue]([SettingTableRowId],[UserLoginPersonaId],
				[Value],[ModifiedBy],[CreatedDate])
		Select @fieldid,@UserLoginPersonaId,@Value,@CreatedBy,@CreatedDate
	END
	set @Current_ID = @Current_ID + 1
  end

GO
