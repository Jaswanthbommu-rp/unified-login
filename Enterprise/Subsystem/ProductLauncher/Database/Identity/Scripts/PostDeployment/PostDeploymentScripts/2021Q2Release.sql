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
DECLARE @UserId bigint
SELECT	@UserId = UserId
FROM	ident.UserLogin
WHERE	LoginName LIKE 'realpagead@%'

if not exists(select top 1 1 from Settings.SettingPicklist where CategoryName = 'CustomFields' and MappingName =  'Alphanumeric')
Begin 
	Insert into Settings.SettingPicklist(CategoryName,MappingKeyName, MappingName, MappingValue, Description, ModifiedBy, ModifiedDate)
	values ('CustomFields','customFieldType', 'Alphanumeric', 1, 'consists of both letters and numerals', @UserId, GETUTCDATE())
End

if not exists(select top 1 1 from Settings.SettingPicklist where CategoryName = 'CustomFields' and MappingName =  'Numeric')
Begin 
	Insert into Settings.SettingPicklist(CategoryName,MappingKeyName, MappingName, MappingValue, Description, ModifiedBy, ModifiedDate)
	values ('CustomFields','customFieldType', 'Numeric', 2, 'consists of only numerals', @UserId, GETUTCDATE())
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
Select FieldId,OrganizationId,Enabled,cf.Name,Description,FieldTypeId,Required,
	ReadOnly,Sequence,MinCharLength,MaxCharLength,CreatedBy
From CustomField.Field cf
Join Enterprise.Organization p on
	p.PartyId = cf.OrganizationId

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
			@CreatedBy bigint, @fieldTypeId smallint

	Select @partyid = OrganizationId,@fieldid = FieldId,
		   @enabled = Enabled,@name = Name,@Description = Description,
		   @ReadOnly = ReadOnly,@Required = Required,@Sequence = Sequence,
		   @MaxCharLength = MaxCharLength, @MinCharLength = MinCharLength,
		   @CreatedBy = CreatedBy, @fieldTypeId = FieldTypeId
	From @CustomFields Where Id = @Current_ID

	Declare @SettingTableId bigint = NULL

	Select @SettingTableId = SettingTableId From  [Settings].[SettingTable] 
	Where PartyId = @partyid 
	AND SettingCategoryTypeId = @SettingCategoryTypeId
	AND TableName = 'CustomFields'

	IF (@SettingTableId IS NULL)
	BEGIN
		INSERT INTO [Settings].[SettingTable]([SettingCategoryTypeId],[PartyId],
				[TableName],[ModifiedBy],[CreatedDate])
		Select @SettingCategoryTypeId,@partyid,'CustomFields',@CreatedBy,@NOW

		set @SettingTableId = SCOPE_IDENTITY();
	END

	IF NOT EXISTS (Select 1 From  [Settings].[SettingTableRow] Where SettingTableId = @SettingTableId and SettingTableRowId = @fieldid)
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
		Where @enabled IS NOT NULL

		INSERT INTO [Settings].[SettingTableColumn](SettingTableRowId,TableColumnName,
			TableColumnValue,ModifiedBy,CreatedDate)
		SELECT @fieldid,'Name',@name,@CreatedBy,@NOW
		Where @name IS NOT NULL

		INSERT INTO [Settings].[SettingTableColumn](SettingTableRowId,TableColumnName,
			TableColumnValue,ModifiedBy,CreatedDate)
		SELECT @fieldid,'FieldTypeId',@fieldTypeId,@CreatedBy,@NOW
		Where @fieldTypeId IS NOT NULL

		INSERT INTO [Settings].[SettingTableColumn](SettingTableRowId,TableColumnName,
			TableColumnValue,ModifiedBy,CreatedDate)
		SELECT @fieldid,'Required',@Required,@CreatedBy,@NOW
		Where @Required IS NOT NULL

		INSERT INTO [Settings].[SettingTableColumn](SettingTableRowId,TableColumnName,
			TableColumnValue,ModifiedBy,CreatedDate)
		SELECT @fieldid,'ReadOnly',@ReadOnly,@CreatedBy,@NOW
		Where @ReadOnly IS NOT NULL

		INSERT INTO [Settings].[SettingTableColumn](SettingTableRowId,TableColumnName,
			TableColumnValue,ModifiedBy,CreatedDate)
		SELECT @fieldid,'Sequence',@Sequence,@CreatedBy,@NOW
		Where @Sequence IS NOT NULL

		INSERT INTO [Settings].[SettingTableColumn](SettingTableRowId,TableColumnName,
			TableColumnValue,ModifiedBy,CreatedDate)
		SELECT @fieldid,'MinCharLength',@MinCharLength,@CreatedBy,@NOW
		Where @MinCharLength IS NOT NULL

		INSERT INTO [Settings].[SettingTableColumn](SettingTableRowId,TableColumnName,
			TableColumnValue,ModifiedBy,CreatedDate)
		SELECT @fieldid,'MaxCharLength',@MaxCharLength,@CreatedBy,@NOW
		Where @MaxCharLength IS NOT NULL
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
  Select UserLoginPersonaId,FieldId,[Value],CreatedBy,cf.CreatedDate
  FROM [CustomField].[FieldValue] cf
  join Settings.SettingTableRow sr on
	sr.SettingTableRowId = cf.FieldId
  except 
	 Select UserLoginPersonaId,SettingTableRowId,Value,ModifiedBy,CreatedDate From [Settings].[SettingTableRowValue]  


	INSERT INTO [Settings].[SettingTableRowValue]([SettingTableRowId],[UserLoginPersonaId],
				[Value],[ModifiedBy],[CreatedDate])
	Select FieldId,UserLoginPersonaId,[Value],CreatedBy,CreatedDate
	From @CustomFieldVlaues

GO
--Use primary properties switch for upfm products
Declare @MasterControlId int,@UPPControlId int,@MaxControlId int,@MaxControlAttributeId int
DECLARE @UserId bigint,
	@ProductId int ,
	@Now datetime = GETDATE()

SELECT	@UserId = UserId
FROM	Ident.UserLogin
WHERE	LoginName LIKE 'realpagead@%'

Select @MasterControlId = ControlId From UserManagement.Control 
Where UIId = 'HAASProductAccessPropertiesTabUIId' AND ControlTypeId = 9

Select @UPPControlId = ControlId From UserManagement.Control 
Where UIId = 'HAASProductAccessUsePrimaryPropertiesSwitchUIId' AND ControlTypeId = 1

IF NOT EXISTS (SELECT TOP 1 1 FROM[UserManagement].[Control] WHERE ControlId = @UPPControlId)
BEGIN
	
	SET IDENTITY_INSERT [UserManagement].[Control] ON 
	SELECT @MaxControlId = max(ControlId) from UserManagement.Control

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
	VALUES (@MaxControlId +1, @MasterControlId, 1, N'HAASProductAccessUsePrimaryPropertiesSwitchUIId', N'Use Primary Properties', N'usePrimaryProperties', 2, @UserId, @Now)

	SET IDENTITY_INSERT [UserManagement].[Control] OFF
END
GO

Declare @MasterControlId int,@UPPControlId int,@MaxControlId int,@MaxControlAttributeId int
DECLARE @UserId bigint,
	@ProductId int ,
	@Now datetime = GETDATE()

SELECT	@UserId = UserId
FROM	Ident.UserLogin
WHERE	LoginName LIKE 'realpagead@%'

Select @MasterControlId = ControlId From UserManagement.Control 
Where UIId = 'IntelligentBuildingWaterAccessPropertiesTabUIId' AND ControlTypeId = 9

Select @UPPControlId = ControlId From UserManagement.Control 
Where UIId = 'IntelligentBuildingWaterProductAccessUsePrimaryPropertiesSwitchUIId' AND ControlTypeId = 1

IF NOT EXISTS (SELECT TOP 1 1 FROM[UserManagement].[Control] WHERE ControlId = @UPPControlId)
BEGIN
	
	SET IDENTITY_INSERT [UserManagement].[Control] ON 
	SELECT @MaxControlId = max(ControlId) from UserManagement.Control

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
	VALUES (@MaxControlId +1, @MasterControlId, 1, N'IntelligentBuildingWaterProductAccessUsePrimaryPropertiesSwitchUIId', N'Use Primary Properties', N'usePrimaryProperties', 2, @UserId, @Now)

	SET IDENTITY_INSERT [UserManagement].[Control] OFF
END
GO

Declare @MasterControlId int,@UPPControlId int,@MaxControlId int,@MaxControlAttributeId int
DECLARE @UserId bigint,
	@ProductId int ,
	@Now datetime = GETDATE()

SELECT	@UserId = UserId
FROM	Ident.UserLogin
WHERE	LoginName LIKE 'realpagead@%'

Select @MasterControlId = ControlId From UserManagement.Control 
Where UIId = 'IntelligentBuildingProductAccessPropertiesTabUIId' AND ControlTypeId = 9

Select @UPPControlId = ControlId From UserManagement.Control 
Where UIId = 'IntelligentBuildingWasteProductAccessUsePrimaryPropertiesSwitchUIId' AND ControlTypeId = 1

IF NOT EXISTS (SELECT TOP 1 1 FROM[UserManagement].[Control] WHERE ControlId = @UPPControlId)
BEGIN
	
	SET IDENTITY_INSERT [UserManagement].[Control] ON 
	SELECT @MaxControlId = max(ControlId) from UserManagement.Control

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
	VALUES (@MaxControlId +1, @MasterControlId, 1, N'IntelligentBuildingWasteProductAccessUsePrimaryPropertiesSwitchUIId', N'Use Primary Properties', N'usePrimaryProperties', 2, @UserId, @Now)

	SET IDENTITY_INSERT [UserManagement].[Control] OFF
END
GO
Declare @MasterControlId int,@UPPControlId int,@MaxControlId int,@MaxControlAttributeId int
DECLARE @UserId bigint,
	@ProductId int ,
	@Now datetime = GETDATE()

SELECT	@UserId = UserId
FROM	Ident.UserLogin
WHERE	LoginName LIKE 'realpagead@%'

Select @MasterControlId = ControlId From UserManagement.Control 
Where UIId = 'SGTProductAccessPropertiesTabUIId' AND ControlTypeId = 9

Select @UPPControlId = ControlId From UserManagement.Control 
Where UIId = 'SGTProductAccessUsePrimaryPropertiesSwitchUIId' AND ControlTypeId = 1

IF NOT EXISTS (SELECT TOP 1 1 FROM[UserManagement].[Control] WHERE ControlId = @UPPControlId)
BEGIN
	
	SET IDENTITY_INSERT [UserManagement].[Control] ON 
	SELECT @MaxControlId = max(ControlId) from UserManagement.Control

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
	VALUES (@MaxControlId +1, @MasterControlId, 1, N'SGTProductAccessUsePrimaryPropertiesSwitchUIId', N'Use Primary Properties', N'usePrimaryProperties', 2, @UserId, @Now)

	SET IDENTITY_INSERT [UserManagement].[Control] OFF
END
GO
Declare @MasterControlId int,@UPPControlId int,@MaxControlId int,@MaxControlAttributeId int
DECLARE @UserId bigint,
	@ProductId int ,
	@Now datetime = GETDATE()

SELECT	@UserId = UserId
FROM	Ident.UserLogin
WHERE	LoginName LIKE 'realpagead@%'

Select @MasterControlId = ControlId From UserManagement.Control 
Where UIId = 'IntelligentBuildingEnergyAccessPropertiesTabUIId' AND ControlTypeId = 9

Select @UPPControlId = ControlId From UserManagement.Control 
Where UIId = 'IntelligentBuildingEnergyProductAccessUsePrimaryPropertiesSwitchUIId' AND ControlTypeId = 1

IF NOT EXISTS (SELECT TOP 1 1 FROM[UserManagement].[Control] WHERE ControlId = @UPPControlId)
BEGIN
	
	SET IDENTITY_INSERT [UserManagement].[Control] ON 
	SELECT @MaxControlId = max(ControlId) from UserManagement.Control

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
	VALUES (@MaxControlId +1, @MasterControlId, 1, N'IntelligentBuildingEnergyProductAccessUsePrimaryPropertiesSwitchUIId', N'Use Primary Properties', N'usePrimaryProperties', 2, @UserId, @Now)

	SET IDENTITY_INSERT [UserManagement].[Control] OFF
END
GO

-- ADD ALLOW DELETE COMPANY SETTING
IF NOT EXISTS (SELECT TOP(1) 1 FROM Enterprise.ProductSettingType WHERE [Name] = 'IsOrganizationRemovalEnabled')
BEGIN
	INSERT INTO Enterprise.ProductSettingType ([Name], [Description], SensitiveData)
	VALUES ('IsOrganizationRemovalEnabled', 'Does the environment support the Organization Removal feature', 0);
END

IF NOT EXISTS (SELECT TOP(1) 1 FROM Enterprise.ProductSettingType WHERE [Name] = 'OrganizationRemovalRetryCount')
BEGIN
	INSERT INTO Enterprise.ProductSettingType ([Name], [Description], SensitiveData)
	VALUES ('OrganizationRemovalRetryCount', 'How many failures should a retry occur before giving up during the organization removal process', 0);
END

IF NOT EXISTS (SELECT TOP(1) 1 FROM Enterprise.ProductSettingType WHERE [Name] = 'OrganizationRemovalBatchSize')
BEGIN
	INSERT INTO Enterprise.ProductSettingType ([Name], [Description], SensitiveData)
	VALUES ('OrganizationRemovalBatchSize', 'How many organizations should be deleted in a batch during the organization removal process', 0);
END

IF NOT EXISTS (SELECT TOP(1) 1 FROM Enterprise.ProductSettingType WHERE [Name] = 'ActivityLogUri')
BEGIN
	INSERT INTO Enterprise.ProductSettingType ([Name], [Description], SensitiveData)
	VALUES ('ActivityLogUri', 'The api uri for ActivityLog', 0);
END

GO

IF NOT EXISTS ( SELECT TOP (1) 1 FROM Maintenance.OrganizationRemovalQueueStatus )
BEGIN
	INSERT INTO Maintenance.OrganizationRemovalQueueStatus
	(
		OrganizationRemovalQueueStatusId,
		Name
	)
	VALUES
	( 0, N'Pending Processing' ),
	( 1, N'Pending Database Removal' ),
	( 2, N'Pending UDMData Removal' ),
	( 3, N'Complete' ),
	( 4, N'Database Removed' ),
	( 5, N'UDMData Removed' ),
	( 6, N'UDMData Not Found' ),
	( 7, N'Database Removal Failed' ),
	( 8, N'UDMData Removal Failed' ),
	( 9, N'ActivityLog Removed' ),
	( 10, N'ActivityLog Removal Failed' )
END
GO

IF NOT EXISTS (SELECT TOP 1 1 FROM Maintenance.OrganizationRemovalQueueStatus WHERE Name = 'ActivityLog Removed' )
BEGIN
	INSERT INTO Maintenance.OrganizationRemovalQueueStatus ( OrganizationRemovalQueueStatusId,Name ) VALUES (9, 'ActivityLog Removed' )
END

IF NOT EXISTS (SELECT TOP 1 1 FROM Maintenance.OrganizationRemovalQueueStatus WHERE Name = 'ActivityLog Removal Failed' )
BEGIN
	INSERT INTO Maintenance.OrganizationRemovalQueueStatus ( OrganizationRemovalQueueStatusId,Name ) VALUES (10, 'ActivityLog Removal Failed' )
END
GO

--START : script for userstory #771257
--AlwaysEnableProductForOrgType 
if not exists ( select top 1 1 from Enterprise.ProductSettingType where name = 'AlwaysEnableProductForOrgType' )
begin
	insert into enterprise.ProductSettingType ( name, Description, SensitiveData ) values ( 'AlwaysEnableProductForOrgType', 'Always Enable Products For OrgType', 0)
end
--EnableProductOnOtherProductsActivation
if not exists ( select top 1 1 from Enterprise.ProductSettingType where name = 'EnableProductOnOtherProductsActivation' )
begin
	insert into enterprise.ProductSettingType ( name, Description, SensitiveData ) values ( 'EnableProductOnOtherProductsActivation', 'Enable Product On other Products Activation', 0)
end

DECLARE @NOW DATETIME = GETUTCDATE(); 
declare @productlist table ( entid int identity, productid int, productsettingtype varchar(500), productsettingvalue varchar(2000))
insert into @productlist values 
	(3, 'AlwaysEnableProductForOrgType', 'Multifamily,Vendor,Other,AppPartner,Supplier'),
	(19, 'AlwaysEnableProductForOrgType', 'Multifamily,Vendor,Other,AppPartner,Supplier'),
	(28, 'AlwaysEnableProductForOrgType', 'Multifamily,Vendor,Other,AppPartner,Supplier'),
	(45, 'AlwaysEnableProductForOrgType', 'Multifamily,Vendor,Other,AppPartner,Supplier'),
	(49, 'AlwaysEnableProductForOrgType', 'Multifamily,Vendor,Other,AppPartner,Supplier'),
	(56, 'AlwaysEnableProductForOrgType', 'Multifamily,Vendor,Other,AppPartner,Supplier'),
	(27, 'AlwaysEnableProductForOrgType', 'Multifamily,Other'),
	(14, 'AlwaysEnableProductForOrgType', 'Multifamily,Other,AppPartner,Supplier'),
	(38, 'AlwaysEnableProductForOrgType', 'Vendor'),

	(39, 'EnableProductOnOtherProductsActivation', '1')
	
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
--END : script for userstory #771257
GO


IF NOT EXISTS
(
	SELECT 1
	FROM Enterprise.CommunicationEmailTemplate CET
	INNER JOin Enterprise.CommunicationEventAudienceType CAT ON CET.CommunicationEventAudienceTypeId = CAT.CommunicationEventAudienceTypeId
	WHERE 
		cat.Description = 'Regular User'
		AND 
		  CommunicationEventPurposeTypeId = 2
)
BEGIN
	INSERT INTO [Enterprise].[CommunicationEmailTemplate]( CommunicationEventAudienceTypeId, CommunicationEventPurposeTypeId, [Subject], [Body] )
	SELECT CommunicationEventAudienceTypeId, 2, 'RealPage Password Reset', '<!DOCTYPE html>
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

                                                                            Your administrator has reset the password on your account. Click the link below to set your new password. You have {EXPIRYDAYS} days to set your new password before the link expires.</span>
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
                                                                                             Your administrator has reset the password on your account. Click the link below to set your new password. You have {EXPIRYDAYS} days to set your new password before the link expires.
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
                                                                                                    <a href="{LINK}" style="font-family: ''Roboto'',''Helvetica Neue'', Helvetica, Arial, sans-serif; color: #ffffff; text-decoration: none; -webkit-border-radius: 25px; -moz-border-radius: 25px; border-radius: 25px; padding: 9px 28px; border: 1px solid #42a5f6; display: inline-block;">Set New Password</a>
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
                                                                                                        If you have trouble accessing your profile, please contact your system administrator.
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
                                                    <span style="color:#757575; font-family: ''Roboto'',''Helvetica Neue'', Helvetica, Arial, sans-serif;">&copy; 2021 RealPage, Inc.</span>
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
FROM
	Enterprise.CommunicationEventAudienceType WHERE Description = 'Regular User'

END;
GO

--START : script for userstory #802024
--ShowInRoleTemplate 
if not exists ( select top 1 1 from Enterprise.ProductSettingType where name = 'ShowInRoleTemplate' )
begin
	insert into enterprise.ProductSettingType ( name, Description, SensitiveData ) values ( 'ShowInRoleTemplate', 'Show Product For RoleTemplate', 0)
end

DECLARE @NOW DATETIME = GETUTCDATE(); 
declare @productlist table ( entid int identity, productid int, productsettingtype varchar(500), productsettingvalue varchar(2000))
insert into @productlist values 
	(1,	 'ShowInRoleTemplate', '1' ),
	(3,	 'ShowInRoleTemplate', '1' ),
	(6,	 'ShowInRoleTemplate', '1' ),
	(8,	 'ShowInRoleTemplate', '1' ),
	(9,	 'ShowInRoleTemplate', '1' ),	
	(13, 'ShowInRoleTemplate', '1' ),
	(14, 'ShowInRoleTemplate', '1' ),
	(15, 'ShowInRoleTemplate', '1' ),
	(16, 'ShowInRoleTemplate', '1' ),
	(17, 'ShowInRoleTemplate', '1' ),
	(18, 'ShowInRoleTemplate', '1' ),
	(20, 'ShowInRoleTemplate', '1' ),	
	(23, 'ShowInRoleTemplate', '1' ),
	(26, 'ShowInRoleTemplate', '1' ),
	(29, 'ShowInRoleTemplate', '1' ),
	(30, 'ShowInRoleTemplate', '1' ),
	(31, 'ShowInRoleTemplate', '1' ),
	(32, 'ShowInRoleTemplate', '1' ),
	(33, 'ShowInRoleTemplate', '1' ),
	(34, 'ShowInRoleTemplate', '1' ),
	(39, 'ShowInRoleTemplate', '1' ),
	(40, 'ShowInRoleTemplate', '1' ),
	(41, 'ShowInRoleTemplate', '1' ),
	(44, 'ShowInRoleTemplate', '1' ),
	(47, 'ShowInRoleTemplate', '1' ),
	(48, 'ShowInRoleTemplate', '1' ),	
	(51, 'ShowInRoleTemplate', '1' ),
	(52, 'ShowInRoleTemplate', '1' ),
	(53, 'ShowInRoleTemplate', '1' ),
	(54, 'ShowInRoleTemplate', '1' ),	
	(57, 'ShowInRoleTemplate', '1' ),
	(58, 'ShowInRoleTemplate', '1' ),
	(59, 'ShowInRoleTemplate', '1' ),
	(60, 'ShowInRoleTemplate', '1' ),
	(63, 'ShowInRoleTemplate', '1' ),
	(65, 'ShowInRoleTemplate', '1' ),
	(66, 'ShowInRoleTemplate', '1' )
	
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
--END : script for userstory #802024

--start : script for userstory #804806


IF NOT EXISTS (SELECT TOP 1 1 FROM[UserManagement].ProductPageType WHERE value = 'RoleTemplate')
BEGIN
Insert into UserManagement.ProductPageType (value, Description)
select 'RoleTemplate', 'Role Template page'
END

GO
Declare @RoleTemplateProductPageTypeId bigint
SELECT @RoleTemplateProductPageTypeId = ProductPageTypeId FROM[UserManagement].ProductPageType WHERE value = 'RoleTemplate'

--OneSite
DECLARE @UserId bigint,
	@ProductId int ,
	@ControlId int,
	@Now datetime = GETDATE()

Declare @ControlAttributeId int
Declare @ProductPageId int
Declare @ProductPageControlId int

SELECT	@UserId = UserId
FROM	Ident.UserLogin
WHERE	LoginName LIKE 'realpagead@%'
SET @ProductId  = 1
IF NOT EXISTS (SELECT TOP 1 1 FROM[UserManagement].[ProductPage] WHERE ProductId = @ProductId and ProductPageTypeId = 3)
BEGIN
	SET IDENTITY_INSERT [UserManagement].[Control] ON 
	Select @ControlId = MAX(controlid) +1 from [UserManagement].[Control]

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
	VALUES (@ControlId, NULL, 8, N'OnesiteRoleTemplateTabGroupUIId', NULL, NULL, 1, @UserId, @Now)

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
	VALUES (@ControlId+1, @ControlId, 9, N'OnesiteRoleTemplateRolesTabUIId', N'Roles', NULL, 1, @UserId, @Now)
	

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
	VALUES (@ControlId+2, @ControlId+1, 3, N'OnesiteRoleTemplateRolesSelectGridUIId', NULL, NULL, 1, @UserId, @Now)

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
	VALUES (@ControlId+3, @ControlId+2, 10, N'OnesiteRoleTemplateCheckboxUIId', NULL, N'isAssigned', 1, @UserId, @Now)	

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
	VALUES (@ControlId+4, @ControlId+2, 5, N'OnesiteRoleTemplateRoleLabelUIId', N'Role', N'name', 2, @UserId, @Now)	

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
	VALUES (@ControlId+5, @ControlId+2, 5, N'OnesiteRoleTemplateRoleTypeLabelUIId', N'Role Type', N'roletype', 3, @UserId, @Now)
	
	SET IDENTITY_INSERT [UserManagement].[Control] OFF

	SET IDENTITY_INSERT [UserManagement].[ControlAttribute] ON 
	
	Select @ControlAttributeId = MAX(ControlAttributeId) +1 from [UserManagement].[ControlAttribute]

	INSERT [UserManagement].[ControlAttribute] ([ControlAttributeId], [ControlId], [Key], [Value], [CreatedBy], [CreatedDate])
	VALUES (@ControlAttributeId, @ControlId+1, N'Default', N'True', @UserId, @Now)

	INSERT [UserManagement].[ControlAttribute] ([ControlAttributeId], [ControlId], [Key], [Value], [CreatedBy], [CreatedDate])
	VALUES (@ControlAttributeId+1, @ControlId+1, N'Hide', N'False', @UserId, @Now)

	INSERT [UserManagement].[ControlAttribute] ([ControlAttributeId], [ControlId], [Key], [Value], [CreatedBy], [CreatedDate])
	VALUES (@ControlAttributeId+2, @ControlId+2, N'ShowSelectAll', N'True', @UserId, @Now)

	SET IDENTITY_INSERT [UserManagement].[ControlAttribute] OFF

	SET IDENTITY_INSERT [UserManagement].[ProductPage] ON 
	
	Select @ProductPageId = MAX(ProductPageId) +1 from [UserManagement].[ProductPage]

	INSERT [UserManagement].[ProductPage] ([ProductPageId], [ProductId], [DisplayName],[ProductPageTypeId], [CreatedBy], [CreatedDate], [IsActive])
	VALUES (@ProductPageId, @ProductId, N'OneSite Roles',@RoleTemplateProductPageTypeId, @UserId, @Now, 1)

	SET IDENTITY_INSERT [UserManagement].[ProductPage] OFF

	SET IDENTITY_INSERT [UserManagement].[ProductPageControl] ON 
	
	Select @ProductPageControlId = MAX(ProductPageControlId) +1 from [UserManagement].[ProductPageControl]

	INSERT [UserManagement].[ProductPageControl] ([ProductPageControlId], [ProductPageId], [ControlId], [CreatedBy], [CreatedDate])
	VALUES (@ProductPageControlId, @ProductPageId, @ControlId, @UserId, @Now)

	SET IDENTITY_INSERT [UserManagement].[ProductPageControl] OFF
END

--Spend Management
SET @ProductId  = 13
IF NOT EXISTS (SELECT TOP 1 1 FROM[UserManagement].[ProductPage] WHERE ProductId = @ProductId and ProductPageTypeId = 3)
BEGIN
	SET IDENTITY_INSERT [UserManagement].[Control] ON 
	Select @ControlId = MAX(controlid) +1 from [UserManagement].[Control]

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
	VALUES (@ControlId, NULL, 8, N'SpendManagementRoleTemplateTabGroupUIId', NULL, NULL, 1, @UserId, @Now)

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
	VALUES (@ControlId+1, @ControlId, 9, N'SpendManagementRoleTemplateRolesTabUIId', N'Roles', NULL, 1, @UserId, @Now)
	

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
	VALUES (@ControlId+2, @ControlId+1, 2, N'SpendManagementRoleTemplateRolesSelectGridUIId', NULL, NULL, 1, @UserId, @Now)

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
	VALUES (@ControlId+3, @ControlId+2, 7, N'SpendManagementRoleTemplateRoleRadioUIId', NULL, N'isAssigned', 1, @UserId, @Now)	

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
	VALUES (@ControlId+4, @ControlId+2, 5, N'SpendManagementRoleTemplateRoleLabelUIId', N'Role', N'name', 2, @UserId, @Now)	
	
	SET IDENTITY_INSERT [UserManagement].[Control] OFF

	SET IDENTITY_INSERT [UserManagement].[ControlAttribute] ON 
	
	Select @ControlAttributeId = MAX(ControlAttributeId) +1 from [UserManagement].[ControlAttribute]

	INSERT [UserManagement].[ControlAttribute] ([ControlAttributeId], [ControlId], [Key], [Value], [CreatedBy], [CreatedDate])
	VALUES (@ControlAttributeId, @ControlId+1, N'Default', N'True', @UserId, @Now)

	INSERT [UserManagement].[ControlAttribute] ([ControlAttributeId], [ControlId], [Key], [Value], [CreatedBy], [CreatedDate])
	VALUES (@ControlAttributeId+1, @ControlId+1, N'Hide', N'False', @UserId, @Now)

	SET IDENTITY_INSERT [UserManagement].[ControlAttribute] OFF

	SET IDENTITY_INSERT [UserManagement].[ProductPage] ON 
	
	Select @ProductPageId = MAX(ProductPageId) +1 from [UserManagement].[ProductPage]

	INSERT [UserManagement].[ProductPage] ([ProductPageId], [ProductId], [DisplayName],[ProductPageTypeId], [CreatedBy], [CreatedDate], [IsActive])
	VALUES (@ProductPageId, @ProductId, N'Spend Management Roles',@RoleTemplateProductPageTypeId, @UserId, @Now, 1)

	SET IDENTITY_INSERT [UserManagement].[ProductPage] OFF

	SET IDENTITY_INSERT [UserManagement].[ProductPageControl] ON 
	
	Select @ProductPageControlId = MAX(ProductPageControlId) +1 from [UserManagement].[ProductPageControl]

	INSERT [UserManagement].[ProductPageControl] ([ProductPageControlId], [ProductPageId], [ControlId], [CreatedBy], [CreatedDate])
	VALUES (@ProductPageControlId, @ProductPageId, @ControlId, @UserId, @Now)

	SET IDENTITY_INSERT [UserManagement].[ProductPageControl] OFF
END

--Unified Platform
SET @ProductId  = 3
IF NOT EXISTS (SELECT TOP 1 1 FROM[UserManagement].[ProductPage] WHERE ProductId = @ProductId and ProductPageTypeId = 3)
BEGIN
	SET IDENTITY_INSERT [UserManagement].[Control] ON 
	Select @ControlId = MAX(controlid) +1 from [UserManagement].[Control]

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
	VALUES (@ControlId, NULL, 8, N'UnifiedPlatformRoleTemplateTabGroupUIId', NULL, NULL, 1, @UserId, @Now)

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
	VALUES (@ControlId+1, @ControlId, 9, N'UnifiedPlatformRoleTemplateRolesTabUIId', N'Roles', NULL, 1, @UserId, @Now)
	

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
	VALUES (@ControlId+2, @ControlId+1, 2, N'UnifiedPlatformRoleTemplateRolesSelectGridUIId', NULL, NULL, 1, @UserId, @Now)

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
	VALUES (@ControlId+3, @ControlId+2, 7, N'UnifiedPlatformRoleTemplateRoleRadioUIId', NULL, N'isAssigned', 1, @UserId, @Now)	

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
	VALUES (@ControlId+4, @ControlId+2, 5, N'UnifiedPlatformRoleTemplateRoleLabelUIId', N'Role', N'name', 2, @UserId, @Now)		

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
	VALUES (@ControlId+5, @ControlId+2, 5, N'UnifiedPlatformRoleTemplateRoleTypeLabelUIId', N'Role Type', N'roletype', 3, @UserId, @Now)
	
	SET IDENTITY_INSERT [UserManagement].[Control] OFF

	SET IDENTITY_INSERT [UserManagement].[ControlAttribute] ON 
	
	Select @ControlAttributeId = MAX(ControlAttributeId) +1 from [UserManagement].[ControlAttribute]

	INSERT [UserManagement].[ControlAttribute] ([ControlAttributeId], [ControlId], [Key], [Value], [CreatedBy], [CreatedDate])
	VALUES (@ControlAttributeId, @ControlId+1, N'Default', N'True', @UserId, @Now)

	INSERT [UserManagement].[ControlAttribute] ([ControlAttributeId], [ControlId], [Key], [Value], [CreatedBy], [CreatedDate])
	VALUES (@ControlAttributeId+1, @ControlId+1, N'Hide', N'False', @UserId, @Now)

	SET IDENTITY_INSERT [UserManagement].[ControlAttribute] OFF

	SET IDENTITY_INSERT [UserManagement].[ProductPage] ON 
	
	Select @ProductPageId = MAX(ProductPageId) +1 from [UserManagement].[ProductPage]

	INSERT [UserManagement].[ProductPage] ([ProductPageId], [ProductId], [DisplayName],[ProductPageTypeId], [CreatedBy], [CreatedDate], [IsActive])
	VALUES (@ProductPageId, @ProductId, N'Unified Platform Roles',@RoleTemplateProductPageTypeId, @UserId, @Now, 1)

	SET IDENTITY_INSERT [UserManagement].[ProductPage] OFF

	SET IDENTITY_INSERT [UserManagement].[ProductPageControl] ON 
	
	Select @ProductPageControlId = MAX(ProductPageControlId) +1 from [UserManagement].[ProductPageControl]

	INSERT [UserManagement].[ProductPageControl] ([ProductPageControlId], [ProductPageId], [ControlId], [CreatedBy], [CreatedDate])
	VALUES (@ProductPageControlId, @ProductPageId, @ControlId, @UserId, @Now)

	SET IDENTITY_INSERT [UserManagement].[ProductPageControl] OFF
END

--MarketingCenter
SET @ProductId  = 9
IF NOT EXISTS (SELECT TOP 1 1 FROM[UserManagement].[ProductPage] WHERE ProductId = @ProductId and ProductPageTypeId = 3)
BEGIN
	SET IDENTITY_INSERT [UserManagement].[Control] ON 
	Select @ControlId = MAX(controlid) +1 from [UserManagement].[Control]

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
	VALUES (@ControlId, NULL, 8, N'MarketingCenterRoleTemplateTabGroupUIId', NULL, NULL, 1, @UserId, @Now)

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
	VALUES (@ControlId+1, @ControlId, 9, N'MarketingCenterRoleTemplateRolesTabUIId', N'Roles', NULL, 1, @UserId, @Now)
	

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
	VALUES (@ControlId+2, @ControlId+1, 2, N'MarketingCenterRoleTemplateRolesSelectGridUIId', NULL, NULL, 1, @UserId, @Now)

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
	VALUES (@ControlId+3, @ControlId+2, 7, N'MarketingCenterRoleTemplateRoleRadioUIId', NULL, N'isAssigned', 1, @UserId, @Now)	

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
	VALUES (@ControlId+4, @ControlId+2, 5, N'MarketingCenterRoleTemplateRoleLabelUIId', N'Role', N'name', 2, @UserId, @Now)	
	
	SET IDENTITY_INSERT [UserManagement].[Control] OFF

	SET IDENTITY_INSERT [UserManagement].[ControlAttribute] ON 
	
	Select @ControlAttributeId = MAX(ControlAttributeId) +1 from [UserManagement].[ControlAttribute]

	INSERT [UserManagement].[ControlAttribute] ([ControlAttributeId], [ControlId], [Key], [Value], [CreatedBy], [CreatedDate])
	VALUES (@ControlAttributeId, @ControlId+1, N'Default', N'True', @UserId, @Now)

	INSERT [UserManagement].[ControlAttribute] ([ControlAttributeId], [ControlId], [Key], [Value], [CreatedBy], [CreatedDate])
	VALUES (@ControlAttributeId+1, @ControlId+1, N'Hide', N'False', @UserId, @Now)

	SET IDENTITY_INSERT [UserManagement].[ControlAttribute] OFF

	SET IDENTITY_INSERT [UserManagement].[ProductPage] ON 
	
	Select @ProductPageId = MAX(ProductPageId) +1 from [UserManagement].[ProductPage]

	INSERT [UserManagement].[ProductPage] ([ProductPageId], [ProductId], [DisplayName],[ProductPageTypeId], [CreatedBy], [CreatedDate], [IsActive])
	VALUES (@ProductPageId, @ProductId, N'Marketing Center Roles',@RoleTemplateProductPageTypeId, @UserId, @Now, 1)

	SET IDENTITY_INSERT [UserManagement].[ProductPage] OFF

	SET IDENTITY_INSERT [UserManagement].[ProductPageControl] ON 
	
	Select @ProductPageControlId = MAX(ProductPageControlId) +1 from [UserManagement].[ProductPageControl]

	INSERT [UserManagement].[ProductPageControl] ([ProductPageControlId], [ProductPageId], [ControlId], [CreatedBy], [CreatedDate])
	VALUES (@ProductPageControlId, @ProductPageId, @ControlId, @UserId, @Now)

	SET IDENTITY_INSERT [UserManagement].[ProductPageControl] OFF
END

--RentersInsurance
SET @ProductId  = 15
IF NOT EXISTS (SELECT TOP 1 1 FROM[UserManagement].[ProductPage] WHERE ProductId = @ProductId and ProductPageTypeId = 3)
BEGIN
	SET IDENTITY_INSERT [UserManagement].[Control] ON 
	Select @ControlId = MAX(controlid) +1 from [UserManagement].[Control]

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
	VALUES (@ControlId, NULL, 8, N'RentersInsuranceRoleTemplateTabGroupUIId', NULL, NULL, 1, @UserId, @Now)

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
	VALUES (@ControlId+1, @ControlId, 9, N'RentersInsuranceRoleTemplateRolesTabUIId', N'Roles', NULL, 1, @UserId, @Now)
	

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
	VALUES (@ControlId+2, @ControlId+1, 2, N'RentersInsuranceRoleTemplateRolesSelectGridUIId', NULL, NULL, 1, @UserId, @Now)

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
	VALUES (@ControlId+3, @ControlId+2, 7, N'RentersInsuranceRoleTemplateRoleRadioUIId', NULL, N'isAssigned', 1, @UserId, @Now)	

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
	VALUES (@ControlId+4, @ControlId+2, 5, N'RentersInsuranceRoleTemplateRoleLabelUIId', N'Role', N'name', 2, @UserId, @Now)	
	
	SET IDENTITY_INSERT [UserManagement].[Control] OFF

	SET IDENTITY_INSERT [UserManagement].[ControlAttribute] ON 
	
	Select @ControlAttributeId = MAX(ControlAttributeId) +1 from [UserManagement].[ControlAttribute]

	INSERT [UserManagement].[ControlAttribute] ([ControlAttributeId], [ControlId], [Key], [Value], [CreatedBy], [CreatedDate])
	VALUES (@ControlAttributeId, @ControlId+1, N'Default', N'True', @UserId, @Now)

	INSERT [UserManagement].[ControlAttribute] ([ControlAttributeId], [ControlId], [Key], [Value], [CreatedBy], [CreatedDate])
	VALUES (@ControlAttributeId+1, @ControlId+1, N'Hide', N'False', @UserId, @Now)

	SET IDENTITY_INSERT [UserManagement].[ControlAttribute] OFF

	SET IDENTITY_INSERT [UserManagement].[ProductPage] ON 
	
	Select @ProductPageId = MAX(ProductPageId) +1 from [UserManagement].[ProductPage]

	INSERT [UserManagement].[ProductPage] ([ProductPageId], [ProductId], [DisplayName],[ProductPageTypeId], [CreatedBy], [CreatedDate], [IsActive])
	VALUES (@ProductPageId, @ProductId, N'Renters Insurance Roles',@RoleTemplateProductPageTypeId, @UserId, @Now, 1)

	SET IDENTITY_INSERT [UserManagement].[ProductPage] OFF

	SET IDENTITY_INSERT [UserManagement].[ProductPageControl] ON 
	
	Select @ProductPageControlId = MAX(ProductPageControlId) +1 from [UserManagement].[ProductPageControl]

	INSERT [UserManagement].[ProductPageControl] ([ProductPageControlId], [ProductPageId], [ControlId], [CreatedBy], [CreatedDate])
	VALUES (@ProductPageControlId, @ProductPageId, @ControlId, @UserId, @Now)

	SET IDENTITY_INSERT [UserManagement].[ProductPageControl] OFF
END

--ResidentPortals
SET @ProductId  = 17
IF NOT EXISTS (SELECT TOP 1 1 FROM[UserManagement].[ProductPage] WHERE ProductId = @ProductId and ProductPageTypeId = 3)
BEGIN
	SET IDENTITY_INSERT [UserManagement].[Control] ON 
	Select @ControlId = MAX(controlid) +1 from [UserManagement].[Control]

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
	VALUES (@ControlId, NULL, 8, N'ResidentPortalsRoleTemplateTabGroupUIId', NULL, NULL, 1, @UserId, @Now)

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
	VALUES (@ControlId+1, @ControlId, 9, N'ResidentPortalsRoleTemplateRolesTabUIId', N'Roles', NULL, 1, @UserId, @Now)
	

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
	VALUES (@ControlId+2, @ControlId+1, 2, N'ResidentPortalsRoleTemplateRolesSelectGridUIId', NULL, NULL, 1, @UserId, @Now)

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
	VALUES (@ControlId+3, @ControlId+2, 7, N'ResidentPortalsRoleTemplateRoleRadioUIId', NULL, N'isAssigned', 1, @UserId, @Now)	

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
	VALUES (@ControlId+4, @ControlId+2, 5, N'ResidentPortalsRoleTemplateRoleLabelUIId', N'Role', N'name', 2, @UserId, @Now)	
	
	SET IDENTITY_INSERT [UserManagement].[Control] OFF

	SET IDENTITY_INSERT [UserManagement].[ControlAttribute] ON 
	
	Select @ControlAttributeId = MAX(ControlAttributeId) +1 from [UserManagement].[ControlAttribute]

	INSERT [UserManagement].[ControlAttribute] ([ControlAttributeId], [ControlId], [Key], [Value], [CreatedBy], [CreatedDate])
	VALUES (@ControlAttributeId, @ControlId+1, N'Default', N'True', @UserId, @Now)

	INSERT [UserManagement].[ControlAttribute] ([ControlAttributeId], [ControlId], [Key], [Value], [CreatedBy], [CreatedDate])
	VALUES (@ControlAttributeId+1, @ControlId+1, N'Hide', N'False', @UserId, @Now)

	SET IDENTITY_INSERT [UserManagement].[ControlAttribute] OFF

	SET IDENTITY_INSERT [UserManagement].[ProductPage] ON 
	
	Select @ProductPageId = MAX(ProductPageId) +1 from [UserManagement].[ProductPage]

	INSERT [UserManagement].[ProductPage] ([ProductPageId], [ProductId], [DisplayName],[ProductPageTypeId], [CreatedBy], [CreatedDate], [IsActive])
	VALUES (@ProductPageId, @ProductId, N'Resident Portals Roles',@RoleTemplateProductPageTypeId, @UserId, @Now, 1)

	SET IDENTITY_INSERT [UserManagement].[ProductPage] OFF

	SET IDENTITY_INSERT [UserManagement].[ProductPageControl] ON 
	
	Select @ProductPageControlId = MAX(ProductPageControlId) +1 from [UserManagement].[ProductPageControl]

	INSERT [UserManagement].[ProductPageControl] ([ProductPageControlId], [ProductPageId], [ControlId], [CreatedBy], [CreatedDate])
	VALUES (@ProductPageControlId, @ProductPageId, @ControlId, @UserId, @Now)

	SET IDENTITY_INSERT [UserManagement].[ProductPageControl] OFF
END

--On-Site 
SET @ProductId  = 23
IF NOT EXISTS (SELECT TOP 1 1 FROM[UserManagement].[ProductPage] WHERE ProductId = @ProductId and ProductPageTypeId = 3)
BEGIN
	SET IDENTITY_INSERT [UserManagement].[Control] ON 
	Select @ControlId = MAX(controlid) +1 from [UserManagement].[Control]

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
	VALUES (@ControlId, NULL, 8, N'OnSiteRoleTemplateTabGroupUIId', NULL, NULL, 1, @UserId, @Now)

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
	VALUES (@ControlId+1, @ControlId, 9, N'OnSiteRoleTemplateRolesTabUIId', N'Roles', NULL, 1, @UserId, @Now)	

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
	VALUES (@ControlId+2, @ControlId+1, 2, N'OnSiteRoleTemplateRolesSelectGridUIId', NULL, NULL, 1, @UserId, @Now)

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
	VALUES (@ControlId+3, @ControlId+2, 7, N'OnSiteRoleTemplateRoleRadioUIId', NULL, N'isAssigned', 1, @UserId, @Now)	

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
	VALUES (@ControlId+4, @ControlId+2, 5, N'OnSiteRoleTemplateRoleLabelUIId', N'Role', N'title', 2, @UserId, @Now)	
	
	SET IDENTITY_INSERT [UserManagement].[Control] OFF

	SET IDENTITY_INSERT [UserManagement].[ControlAttribute] ON 
	
	Select @ControlAttributeId = MAX(ControlAttributeId) +1 from [UserManagement].[ControlAttribute]

	INSERT [UserManagement].[ControlAttribute] ([ControlAttributeId], [ControlId], [Key], [Value], [CreatedBy], [CreatedDate])
	VALUES (@ControlAttributeId, @ControlId+1, N'Default', N'True', @UserId, @Now)

	INSERT [UserManagement].[ControlAttribute] ([ControlAttributeId], [ControlId], [Key], [Value], [CreatedBy], [CreatedDate])
	VALUES (@ControlAttributeId+1, @ControlId+1, N'Hide', N'False', @UserId, @Now)

	SET IDENTITY_INSERT [UserManagement].[ControlAttribute] OFF

	SET IDENTITY_INSERT [UserManagement].[ProductPage] ON 
	
	Select @ProductPageId = MAX(ProductPageId) +1 from [UserManagement].[ProductPage]

	INSERT [UserManagement].[ProductPage] ([ProductPageId], [ProductId], [DisplayName],[ProductPageTypeId], [CreatedBy], [CreatedDate], [IsActive])
	VALUES (@ProductPageId, @ProductId, N'On-Site Roles',@RoleTemplateProductPageTypeId, @UserId, @Now, 1)

	SET IDENTITY_INSERT [UserManagement].[ProductPage] OFF

	SET IDENTITY_INSERT [UserManagement].[ProductPageControl] ON 
	
	Select @ProductPageControlId = MAX(ProductPageControlId) +1 from [UserManagement].[ProductPageControl]

	INSERT [UserManagement].[ProductPageControl] ([ProductPageControlId], [ProductPageId], [ControlId], [CreatedBy], [CreatedDate])
	VALUES (@ProductPageControlId, @ProductPageId, @ControlId, @UserId, @Now)

	SET IDENTITY_INSERT [UserManagement].[ProductPageControl] OFF
END

--UnifiedAmenities
SET @ProductId  = 26
IF NOT EXISTS (SELECT TOP 1 1 FROM[UserManagement].[ProductPage] WHERE ProductId = @ProductId and ProductPageTypeId = 3)
BEGIN
	SET IDENTITY_INSERT [UserManagement].[Control] ON 
	Select @ControlId = MAX(controlid) +1 from [UserManagement].[Control]

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
	VALUES (@ControlId, NULL, 8, N'UnifiedAmenitiesRoleTemplateTabGroupUIId', NULL, NULL, 1, @UserId, @Now)

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
	VALUES (@ControlId+1, @ControlId, 9, N'UnifiedAmenitiesRoleTemplateRolesTabUIId', N'Roles', NULL, 1, @UserId, @Now)
	

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
	VALUES (@ControlId+2, @ControlId+1, 2, N'UnifiedAmenitiesRoleTemplateRolesSelectGridUIId', NULL, NULL, 1, @UserId, @Now)

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
	VALUES (@ControlId+3, @ControlId+2, 7, N'UnifiedAmenitiesRoleTemplateRoleRadioUIId', NULL, N'isAssigned', 1, @UserId, @Now)	

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
	VALUES (@ControlId+4, @ControlId+2, 5, N'UnifiedAmenitiesRoleTemplateRoleLabelUIId', N'Role', N'name', 2, @UserId, @Now)		

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
	VALUES (@ControlId+5, @ControlId+2, 5, N'UnifiedAmenitiesRoleTemplateRoleTypeLabelUIId', N'Role Type', N'roletype', 3, @UserId, @Now)
	
	SET IDENTITY_INSERT [UserManagement].[Control] OFF

	SET IDENTITY_INSERT [UserManagement].[ControlAttribute] ON 
	
	Select @ControlAttributeId = MAX(ControlAttributeId) +1 from [UserManagement].[ControlAttribute]

	INSERT [UserManagement].[ControlAttribute] ([ControlAttributeId], [ControlId], [Key], [Value], [CreatedBy], [CreatedDate])
	VALUES (@ControlAttributeId, @ControlId+1, N'Default', N'True', @UserId, @Now)

	INSERT [UserManagement].[ControlAttribute] ([ControlAttributeId], [ControlId], [Key], [Value], [CreatedBy], [CreatedDate])
	VALUES (@ControlAttributeId+1, @ControlId+1, N'Hide', N'False', @UserId, @Now)

	SET IDENTITY_INSERT [UserManagement].[ControlAttribute] OFF

	SET IDENTITY_INSERT [UserManagement].[ProductPage] ON 
	
	Select @ProductPageId = MAX(ProductPageId) +1 from [UserManagement].[ProductPage]

	INSERT [UserManagement].[ProductPage] ([ProductPageId], [ProductId], [DisplayName],[ProductPageTypeId], [CreatedBy], [CreatedDate], [IsActive])
	VALUES (@ProductPageId, @ProductId, N'Unified Amenities Roles',@RoleTemplateProductPageTypeId, @UserId, @Now, 1)

	SET IDENTITY_INSERT [UserManagement].[ProductPage] OFF

	SET IDENTITY_INSERT [UserManagement].[ProductPageControl] ON 
	
	Select @ProductPageControlId = MAX(ProductPageControlId) +1 from [UserManagement].[ProductPageControl]

	INSERT [UserManagement].[ProductPageControl] ([ProductPageControlId], [ProductPageId], [ControlId], [CreatedBy], [CreatedDate])
	VALUES (@ProductPageControlId, @ProductPageId, @ControlId, @UserId, @Now)

	SET IDENTITY_INSERT [UserManagement].[ProductPageControl] OFF
END

--IntegrationMarketplace
SET @ProductId  = 39
IF NOT EXISTS (SELECT TOP 1 1 FROM[UserManagement].[ProductPage] WHERE ProductId = @ProductId and ProductPageTypeId = 3)
BEGIN
	SET IDENTITY_INSERT [UserManagement].[Control] ON 
	Select @ControlId = MAX(controlid) +1 from [UserManagement].[Control]

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
	VALUES (@ControlId, NULL, 8, N'IntegrationMarketplaceRoleTemplateTabGroupUIId', NULL, NULL, 1, @UserId, @Now)

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
	VALUES (@ControlId+1, @ControlId, 9, N'IntegrationMarketplaceRoleTemplateRolesTabUIId', N'Roles', NULL, 1, @UserId, @Now)
	

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
	VALUES (@ControlId+2, @ControlId+1, 2, N'IntegrationMarketplaceRoleTemplateRolesSelectGridUIId', NULL, NULL, 1, @UserId, @Now)

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
	VALUES (@ControlId+3, @ControlId+2, 7, N'IntegrationMarketplaceRoleTemplateRoleRadioUIId', NULL, N'isAssigned', 1, @UserId, @Now)	

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
	VALUES (@ControlId+4, @ControlId+2, 5, N'IntegrationMarketplaceRoleTemplateRoleLabelUIId', N'Role', N'name', 2, @UserId, @Now)	
	
	SET IDENTITY_INSERT [UserManagement].[Control] OFF

	SET IDENTITY_INSERT [UserManagement].[ControlAttribute] ON 
	
	Select @ControlAttributeId = MAX(ControlAttributeId) +1 from [UserManagement].[ControlAttribute]

	INSERT [UserManagement].[ControlAttribute] ([ControlAttributeId], [ControlId], [Key], [Value], [CreatedBy], [CreatedDate])
	VALUES (@ControlAttributeId, @ControlId+1, N'Default', N'True', @UserId, @Now)

	INSERT [UserManagement].[ControlAttribute] ([ControlAttributeId], [ControlId], [Key], [Value], [CreatedBy], [CreatedDate])
	VALUES (@ControlAttributeId+1, @ControlId+1, N'Hide', N'False', @UserId, @Now)

	SET IDENTITY_INSERT [UserManagement].[ControlAttribute] OFF

	SET IDENTITY_INSERT [UserManagement].[ProductPage] ON 
	
	Select @ProductPageId = MAX(ProductPageId) +1 from [UserManagement].[ProductPage]

	INSERT [UserManagement].[ProductPage] ([ProductPageId], [ProductId], [DisplayName],[ProductPageTypeId], [CreatedBy], [CreatedDate], [IsActive])
	VALUES (@ProductPageId, @ProductId, N'Integration Marketplace Roles',@RoleTemplateProductPageTypeId, @UserId, @Now, 1)

	SET IDENTITY_INSERT [UserManagement].[ProductPage] OFF

	SET IDENTITY_INSERT [UserManagement].[ProductPageControl] ON 
	
	Select @ProductPageControlId = MAX(ProductPageControlId) +1 from [UserManagement].[ProductPageControl]

	INSERT [UserManagement].[ProductPageControl] ([ProductPageControlId], [ProductPageId], [ControlId], [CreatedBy], [CreatedDate])
	VALUES (@ProductPageControlId, @ProductPageId, @ControlId, @UserId, @Now)

	SET IDENTITY_INSERT [UserManagement].[ProductPageControl] OFF
END

--ILMLeadManagement
SET @ProductId  = 40
IF NOT EXISTS (SELECT TOP 1 1 FROM[UserManagement].[ProductPage] WHERE ProductId = @ProductId and ProductPageTypeId = 3)
BEGIN
	SET IDENTITY_INSERT [UserManagement].[Control] ON 
	Select @ControlId = MAX(controlid) +1 from [UserManagement].[Control]

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
	VALUES (@ControlId, NULL, 8, N'ILMLeadManagementRoleTemplateTabGroupUIId', NULL, NULL, 1, @UserId, @Now)

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
	VALUES (@ControlId+1, @ControlId, 9, N'ILMLeadManagementRoleTemplateRolesTabUIId', N'Roles', NULL, 1, @UserId, @Now)
	

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
	VALUES (@ControlId+2, @ControlId+1, 3, N'ILMLeadManagementRoleTemplateRolesSelectGridUIId', NULL, NULL, 1, @UserId, @Now)

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
	VALUES (@ControlId+3, @ControlId+2, 10, N'ILMLeadManagementRoleTemplateRoleCheckboxUIId', NULL, N'isAssigned', 1, @UserId, @Now)	

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
	VALUES (@ControlId+4, @ControlId+2, 5, N'ILMLeadManagementRoleTemplateRoleLabelUIId', N'Role', N'name', 2, @UserId, @Now)
	
	SET IDENTITY_INSERT [UserManagement].[Control] OFF

	SET IDENTITY_INSERT [UserManagement].[ControlAttribute] ON 
	
	Select @ControlAttributeId = MAX(ControlAttributeId) +1 from [UserManagement].[ControlAttribute]

	INSERT [UserManagement].[ControlAttribute] ([ControlAttributeId], [ControlId], [Key], [Value], [CreatedBy], [CreatedDate])
	VALUES (@ControlAttributeId, @ControlId+1, N'Default', N'True', @UserId, @Now)

	INSERT [UserManagement].[ControlAttribute] ([ControlAttributeId], [ControlId], [Key], [Value], [CreatedBy], [CreatedDate])
	VALUES (@ControlAttributeId+1, @ControlId+1, N'Hide', N'False', @UserId, @Now)

	SET IDENTITY_INSERT [UserManagement].[ControlAttribute] OFF

	SET IDENTITY_INSERT [UserManagement].[ProductPage] ON 
	
	Select @ProductPageId = MAX(ProductPageId) +1 from [UserManagement].[ProductPage]

	INSERT [UserManagement].[ProductPage] ([ProductPageId], [ProductId], [DisplayName],[ProductPageTypeId], [CreatedBy], [CreatedDate], [IsActive])
	VALUES (@ProductPageId, @ProductId, N'ILM Lead Management Roles',@RoleTemplateProductPageTypeId, @UserId, @Now, 1)

	SET IDENTITY_INSERT [UserManagement].[ProductPage] OFF

	SET IDENTITY_INSERT [UserManagement].[ProductPageControl] ON 
	
	Select @ProductPageControlId = MAX(ProductPageControlId) +1 from [UserManagement].[ProductPageControl]

	INSERT [UserManagement].[ProductPageControl] ([ProductPageControlId], [ProductPageId], [ControlId], [CreatedBy], [CreatedDate])
	VALUES (@ProductPageControlId, @ProductPageId, @ControlId, @UserId, @Now)

	SET IDENTITY_INSERT [UserManagement].[ProductPageControl] OFF
END

--ILMLeasingAnalytics
SET @ProductId  = 41
IF NOT EXISTS (SELECT TOP 1 1 FROM[UserManagement].[ProductPage] WHERE ProductId = @ProductId and ProductPageTypeId = 3)
BEGIN
	SET IDENTITY_INSERT [UserManagement].[Control] ON 
	Select @ControlId = MAX(controlid) +1 from [UserManagement].[Control]

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
	VALUES (@ControlId, NULL, 8, N'ILMLeasingAnalyticsRoleTemplateTabGroupUIId', NULL, NULL, 1, @UserId, @Now)

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
	VALUES (@ControlId+1, @ControlId, 9, N'ILMLeasingAnalyticsRoleTemplateRolesTabUIId', N'Roles', NULL, 1, @UserId, @Now)	

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
	VALUES (@ControlId+2, @ControlId+1, 3, N'ILMLeasingAnalyticsRoleTemplateRolesSelectGridUIId', NULL, NULL, 1, @UserId, @Now)

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
	VALUES (@ControlId+3, @ControlId+2, 10, N'ILMLeasingAnalyticsRoleTemplateRoleCheckboxUIId', NULL, N'isAssigned', 1, @UserId, @Now)	

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
	VALUES (@ControlId+4, @ControlId+2, 5, N'ILMLeasingAnalyticsRoleTemplateRoleLabelUIId', N'Role', N'name', 2, @UserId, @Now)
	
	SET IDENTITY_INSERT [UserManagement].[Control] OFF

	SET IDENTITY_INSERT [UserManagement].[ControlAttribute] ON 
	
	Select @ControlAttributeId = MAX(ControlAttributeId) +1 from [UserManagement].[ControlAttribute]

	INSERT [UserManagement].[ControlAttribute] ([ControlAttributeId], [ControlId], [Key], [Value], [CreatedBy], [CreatedDate])
	VALUES (@ControlAttributeId, @ControlId+1, N'Default', N'True', @UserId, @Now)

	INSERT [UserManagement].[ControlAttribute] ([ControlAttributeId], [ControlId], [Key], [Value], [CreatedBy], [CreatedDate])
	VALUES (@ControlAttributeId+1, @ControlId+1, N'Hide', N'False', @UserId, @Now)

	SET IDENTITY_INSERT [UserManagement].[ControlAttribute] OFF

	SET IDENTITY_INSERT [UserManagement].[ProductPage] ON 
	
	Select @ProductPageId = MAX(ProductPageId) +1 from [UserManagement].[ProductPage]

	INSERT [UserManagement].[ProductPage] ([ProductPageId], [ProductId], [DisplayName],[ProductPageTypeId], [CreatedBy], [CreatedDate], [IsActive])
	VALUES (@ProductPageId, @ProductId, N'ILM Leasing Analytics Roles',@RoleTemplateProductPageTypeId, @UserId, @Now, 1)

	SET IDENTITY_INSERT [UserManagement].[ProductPage] OFF

	SET IDENTITY_INSERT [UserManagement].[ProductPageControl] ON 
	
	Select @ProductPageControlId = MAX(ProductPageControlId) +1 from [UserManagement].[ProductPageControl]

	INSERT [UserManagement].[ProductPageControl] ([ProductPageControlId], [ProductPageId], [ControlId], [CreatedBy], [CreatedDate])
	VALUES (@ProductPageControlId, @ProductPageId, @ControlId, @UserId, @Now)

	SET IDENTITY_INSERT [UserManagement].[ProductPageControl] OFF
END

--WasteManagementSolution
SET @ProductId  = 57
IF NOT EXISTS (SELECT TOP 1 1 FROM[UserManagement].[ProductPage] WHERE ProductId = @ProductId and ProductPageTypeId = 3)
BEGIN
	SET IDENTITY_INSERT [UserManagement].[Control] ON 
	Select @ControlId = MAX(controlid) +1 from [UserManagement].[Control]

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
	VALUES (@ControlId, NULL, 8, N'WasteManagementSolutionRoleTemplateTabGroupUIId', NULL, NULL, 1, @UserId, @Now)

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
	VALUES (@ControlId+1, @ControlId, 9, N'WasteManagementSolutionRoleTemplateRolesTabUIId', N'Roles', NULL, 1, @UserId, @Now)
	

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
	VALUES (@ControlId+2, @ControlId+1, 2, N'WasteManagementSolutionRoleTemplateRolesSelectGridUIId', NULL, NULL, 1, @UserId, @Now)

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
	VALUES (@ControlId+3, @ControlId+2, 7, N'WasteManagementSolutionRoleTemplateRoleRadioUIId', NULL, N'isAssigned', 1, @UserId, @Now)	

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
	VALUES (@ControlId+4, @ControlId+2, 5, N'WasteManagementSolutionRoleTemplateRoleLabelUIId', N'Role', N'name', 2, @UserId, @Now)			

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
	VALUES (@ControlId+5, @ControlId+2, 5, N'WasteManagementSolutionRoleTemplateRoleTypeLabelUIId', N'Role Type', N'roletype', 3, @UserId, @Now)	
	
	SET IDENTITY_INSERT [UserManagement].[Control] OFF

	SET IDENTITY_INSERT [UserManagement].[ControlAttribute] ON 
	
	Select @ControlAttributeId = MAX(ControlAttributeId) +1 from [UserManagement].[ControlAttribute]

	INSERT [UserManagement].[ControlAttribute] ([ControlAttributeId], [ControlId], [Key], [Value], [CreatedBy], [CreatedDate])
	VALUES (@ControlAttributeId, @ControlId+1, N'Default', N'True', @UserId, @Now)

	INSERT [UserManagement].[ControlAttribute] ([ControlAttributeId], [ControlId], [Key], [Value], [CreatedBy], [CreatedDate])
	VALUES (@ControlAttributeId+1, @ControlId+1, N'Hide', N'False', @UserId, @Now)

	SET IDENTITY_INSERT [UserManagement].[ControlAttribute] OFF

	SET IDENTITY_INSERT [UserManagement].[ProductPage] ON 
	
	Select @ProductPageId = MAX(ProductPageId) +1 from [UserManagement].[ProductPage]

	INSERT [UserManagement].[ProductPage] ([ProductPageId], [ProductId], [DisplayName],[ProductPageTypeId], [CreatedBy], [CreatedDate], [IsActive])
	VALUES (@ProductPageId, @ProductId, N'Smart Waste Roles',@RoleTemplateProductPageTypeId, @UserId, @Now, 1)

	SET IDENTITY_INSERT [UserManagement].[ProductPage] OFF

	SET IDENTITY_INSERT [UserManagement].[ProductPageControl] ON 
	
	Select @ProductPageControlId = MAX(ProductPageControlId) +1 from [UserManagement].[ProductPageControl]

	INSERT [UserManagement].[ProductPageControl] ([ProductPageControlId], [ProductPageId], [ControlId], [CreatedBy], [CreatedDate])
	VALUES (@ProductPageControlId, @ProductPageId, @ControlId, @UserId, @Now)

	SET IDENTITY_INSERT [UserManagement].[ProductPageControl] OFF
END

--IntelligentBuildingEnergy
SET @ProductId  = 58
IF NOT EXISTS (SELECT TOP 1 1 FROM[UserManagement].[ProductPage] WHERE ProductId = @ProductId and ProductPageTypeId = 3)
BEGIN
	SET IDENTITY_INSERT [UserManagement].[Control] ON 
	Select @ControlId = MAX(controlid) +1 from [UserManagement].[Control]

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
	VALUES (@ControlId, NULL, 8, N'IntelligentBuildingEnergyRoleTemplateTabGroupUIId', NULL, NULL, 1, @UserId, @Now)

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
	VALUES (@ControlId+1, @ControlId, 9, N'IntelligentBuildingEnergyRoleTemplateRolesTabUIId', N'Roles', NULL, 1, @UserId, @Now)
	

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
	VALUES (@ControlId+2, @ControlId+1, 2, N'IntelligentBuildingEnergyRoleTemplateRolesSelectGridUIId', NULL, NULL, 1, @UserId, @Now)

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
	VALUES (@ControlId+3, @ControlId+2, 7, N'IntelligentBuildingEnergyRoleTemplateRoleRadioUIId', NULL, N'isAssigned', 1, @UserId, @Now)	

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
	VALUES (@ControlId+4, @ControlId+2, 5, N'IntelligentBuildingEnergyRoleTemplateRoleLabelUIId', N'Role', N'name', 2, @UserId, @Now)		

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
	VALUES (@ControlId+5, @ControlId+2, 5, N'IntelligentBuildingEnergyRoleTemplateRoleTypeLabelUIId', N'Role Type', N'roletype', 3, @UserId, @Now)	
	
	SET IDENTITY_INSERT [UserManagement].[Control] OFF

	SET IDENTITY_INSERT [UserManagement].[ControlAttribute] ON 
	
	Select @ControlAttributeId = MAX(ControlAttributeId) +1 from [UserManagement].[ControlAttribute]

	INSERT [UserManagement].[ControlAttribute] ([ControlAttributeId], [ControlId], [Key], [Value], [CreatedBy], [CreatedDate])
	VALUES (@ControlAttributeId, @ControlId+1, N'Default', N'True', @UserId, @Now)

	INSERT [UserManagement].[ControlAttribute] ([ControlAttributeId], [ControlId], [Key], [Value], [CreatedBy], [CreatedDate])
	VALUES (@ControlAttributeId+1, @ControlId+1, N'Hide', N'False', @UserId, @Now)

	SET IDENTITY_INSERT [UserManagement].[ControlAttribute] OFF

	SET IDENTITY_INSERT [UserManagement].[ProductPage] ON 
	
	Select @ProductPageId = MAX(ProductPageId) +1 from [UserManagement].[ProductPage]

	INSERT [UserManagement].[ProductPage] ([ProductPageId], [ProductId], [DisplayName],[ProductPageTypeId], [CreatedBy], [CreatedDate], [IsActive])
	VALUES (@ProductPageId, @ProductId, N'Smart Energy Roles',@RoleTemplateProductPageTypeId, @UserId, @Now, 1)

	SET IDENTITY_INSERT [UserManagement].[ProductPage] OFF

	SET IDENTITY_INSERT [UserManagement].[ProductPageControl] ON 
	
	Select @ProductPageControlId = MAX(ProductPageControlId) +1 from [UserManagement].[ProductPageControl]

	INSERT [UserManagement].[ProductPageControl] ([ProductPageControlId], [ProductPageId], [ControlId], [CreatedBy], [CreatedDate])
	VALUES (@ProductPageControlId, @ProductPageId, @ControlId, @UserId, @Now)

	SET IDENTITY_INSERT [UserManagement].[ProductPageControl] OFF
END

--SmartWater
SET @ProductId  = 59
IF NOT EXISTS (SELECT TOP 1 1 FROM[UserManagement].[ProductPage] WHERE ProductId = @ProductId and ProductPageTypeId = 3)
BEGIN
	SET IDENTITY_INSERT [UserManagement].[Control] ON 
	Select @ControlId = MAX(controlid) +1 from [UserManagement].[Control]

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
	VALUES (@ControlId, NULL, 8, N'SmartWaterRoleTemplateTabGroupUIId', NULL, NULL, 1, @UserId, @Now)

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
	VALUES (@ControlId+1, @ControlId, 9, N'SmartWaterRoleTemplateRolesTabUIId', N'Roles', NULL, 1, @UserId, @Now)
	

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
	VALUES (@ControlId+2, @ControlId+1, 2, N'SmartWaterRoleTemplateRolesSelectGridUIId', NULL, NULL, 1, @UserId, @Now)

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
	VALUES (@ControlId+3, @ControlId+2, 7, N'SmartWaterRoleTemplateRoleRadioUIId', NULL, N'isAssigned', 1, @UserId, @Now)	

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
	VALUES (@ControlId+4, @ControlId+2, 5, N'SmartWaterRoleTemplateRoleLabelUIId', N'Role', N'name', 2, @UserId, @Now)			

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
	VALUES (@ControlId+5, @ControlId+2, 5, N'SmartWaterRoleTemplateRoleTypeLabelUIId', N'Role Type', N'roletype', 3, @UserId, @Now)
	
	SET IDENTITY_INSERT [UserManagement].[Control] OFF

	SET IDENTITY_INSERT [UserManagement].[ControlAttribute] ON 
	
	Select @ControlAttributeId = MAX(ControlAttributeId) +1 from [UserManagement].[ControlAttribute]

	INSERT [UserManagement].[ControlAttribute] ([ControlAttributeId], [ControlId], [Key], [Value], [CreatedBy], [CreatedDate])
	VALUES (@ControlAttributeId, @ControlId+1, N'Default', N'True', @UserId, @Now)

	INSERT [UserManagement].[ControlAttribute] ([ControlAttributeId], [ControlId], [Key], [Value], [CreatedBy], [CreatedDate])
	VALUES (@ControlAttributeId+1, @ControlId+1, N'Hide', N'False', @UserId, @Now)

	SET IDENTITY_INSERT [UserManagement].[ControlAttribute] OFF

	SET IDENTITY_INSERT [UserManagement].[ProductPage] ON 
	
	Select @ProductPageId = MAX(ProductPageId) +1 from [UserManagement].[ProductPage]

	INSERT [UserManagement].[ProductPage] ([ProductPageId], [ProductId], [DisplayName],[ProductPageTypeId], [CreatedBy], [CreatedDate], [IsActive])
	VALUES (@ProductPageId, @ProductId, N'Smart Water Roles',@RoleTemplateProductPageTypeId, @UserId, @Now, 1)

	SET IDENTITY_INSERT [UserManagement].[ProductPage] OFF

	SET IDENTITY_INSERT [UserManagement].[ProductPageControl] ON 
	
	Select @ProductPageControlId = MAX(ProductPageControlId) +1 from [UserManagement].[ProductPageControl]

	INSERT [UserManagement].[ProductPageControl] ([ProductPageControlId], [ProductPageId], [ControlId], [CreatedBy], [CreatedDate])
	VALUES (@ProductPageControlId, @ProductPageId, @ControlId, @UserId, @Now)

	SET IDENTITY_INSERT [UserManagement].[ProductPageControl] OFF
END

--HomeSharing
SET @ProductId  = 60
IF NOT EXISTS (SELECT TOP 1 1 FROM[UserManagement].[ProductPage] WHERE ProductId = @ProductId and ProductPageTypeId = 3)
BEGIN
	SET IDENTITY_INSERT [UserManagement].[Control] ON 
	Select @ControlId = MAX(controlid) +1 from [UserManagement].[Control]

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
	VALUES (@ControlId, NULL, 8, N'HomeSharingRoleTemplateTabGroupUIId', NULL, NULL, 1, @UserId, @Now)

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
	VALUES (@ControlId+1, @ControlId, 9, N'HomeSharingRoleTemplateRolesTabUIId', N'Roles', NULL, 1, @UserId, @Now)
	

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
	VALUES (@ControlId+2, @ControlId+1, 2, N'HomeSharingRoleTemplateRolesSelectGridUIId', NULL, NULL, 1, @UserId, @Now)

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
	VALUES (@ControlId+3, @ControlId+2, 7, N'HomeSharingRoleTemplateRoleRadioUIId', NULL, N'isAssigned', 1, @UserId, @Now)	

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
	VALUES (@ControlId+4, @ControlId+2, 5, N'HomeSharingRoleTemplateRoleLabelUIId', N'Role', N'name', 2, @UserId, @Now)	
	
	SET IDENTITY_INSERT [UserManagement].[Control] OFF

	SET IDENTITY_INSERT [UserManagement].[ControlAttribute] ON 
	
	Select @ControlAttributeId = MAX(ControlAttributeId) +1 from [UserManagement].[ControlAttribute]

	INSERT [UserManagement].[ControlAttribute] ([ControlAttributeId], [ControlId], [Key], [Value], [CreatedBy], [CreatedDate])
	VALUES (@ControlAttributeId, @ControlId+1, N'Default', N'True', @UserId, @Now)

	INSERT [UserManagement].[ControlAttribute] ([ControlAttributeId], [ControlId], [Key], [Value], [CreatedBy], [CreatedDate])
	VALUES (@ControlAttributeId+1, @ControlId+1, N'Hide', N'False', @UserId, @Now)

	SET IDENTITY_INSERT [UserManagement].[ControlAttribute] OFF

	SET IDENTITY_INSERT [UserManagement].[ProductPage] ON 
	
	Select @ProductPageId = MAX(ProductPageId) +1 from [UserManagement].[ProductPage]

	INSERT [UserManagement].[ProductPage] ([ProductPageId], [ProductId], [DisplayName],[ProductPageTypeId], [CreatedBy], [CreatedDate], [IsActive])
	VALUES (@ProductPageId, @ProductId, N'Home Sharing Roles',@RoleTemplateProductPageTypeId, @UserId, @Now, 1)

	SET IDENTITY_INSERT [UserManagement].[ProductPage] OFF

	SET IDENTITY_INSERT [UserManagement].[ProductPageControl] ON 
	
	Select @ProductPageControlId = MAX(ProductPageControlId) +1 from [UserManagement].[ProductPageControl]

	INSERT [UserManagement].[ProductPageControl] ([ProductPageControlId], [ProductPageId], [ControlId], [CreatedBy], [CreatedDate])
	VALUES (@ProductPageControlId, @ProductPageId, @ControlId, @UserId, @Now)

	SET IDENTITY_INSERT [UserManagement].[ProductPageControl] OFF
END

--HandsOnTrainingSystem
SET @ProductId  = 63
IF NOT EXISTS (SELECT TOP 1 1 FROM[UserManagement].[ProductPage] WHERE ProductId = @ProductId and ProductPageTypeId = 3)
BEGIN
	SET IDENTITY_INSERT [UserManagement].[Control] ON 
	Select @ControlId = MAX(controlid) +1 from [UserManagement].[Control]

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
	VALUES (@ControlId, NULL, 8, N'HandsOnTrainingSystemRoleTemplateTabGroupUIId', NULL, NULL, 1, @UserId, @Now)

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
	VALUES (@ControlId+1, @ControlId, 9, N'HandsOnTrainingSystemRoleTemplateRolesTabUIId', N'Roles', NULL, 1, @UserId, @Now)
	

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
	VALUES (@ControlId+2, @ControlId+1, 2, N'HandsOnTrainingSystemRoleTemplateRolesSelectGridUIId', NULL, NULL, 1, @UserId, @Now)

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
	VALUES (@ControlId+3, @ControlId+2, 7, N'HandsOnTrainingSystemRoleTemplateRoleRadioUIId', NULL, N'isAssigned', 1, @UserId, @Now)	

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
	VALUES (@ControlId+4, @ControlId+2, 5, N'HandsOnTrainingSystemRoleTemplateRoleLabelUIId', N'Role', N'name', 2, @UserId, @Now)	
	
	SET IDENTITY_INSERT [UserManagement].[Control] OFF

	SET IDENTITY_INSERT [UserManagement].[ControlAttribute] ON 
	
	Select @ControlAttributeId = MAX(ControlAttributeId) +1 from [UserManagement].[ControlAttribute]

	INSERT [UserManagement].[ControlAttribute] ([ControlAttributeId], [ControlId], [Key], [Value], [CreatedBy], [CreatedDate])
	VALUES (@ControlAttributeId, @ControlId+1, N'Default', N'True', @UserId, @Now)

	INSERT [UserManagement].[ControlAttribute] ([ControlAttributeId], [ControlId], [Key], [Value], [CreatedBy], [CreatedDate])
	VALUES (@ControlAttributeId+1, @ControlId+1, N'Hide', N'False', @UserId, @Now)

	SET IDENTITY_INSERT [UserManagement].[ControlAttribute] OFF

	SET IDENTITY_INSERT [UserManagement].[ProductPage] ON 
	
	Select @ProductPageId = MAX(ProductPageId) +1 from [UserManagement].[ProductPage]

	INSERT [UserManagement].[ProductPage] ([ProductPageId], [ProductId], [DisplayName],[ProductPageTypeId], [CreatedBy], [CreatedDate], [IsActive])
	VALUES (@ProductPageId, @ProductId, N'Hands-On Training System Roles',@RoleTemplateProductPageTypeId, @UserId, @Now, 1)

	SET IDENTITY_INSERT [UserManagement].[ProductPage] OFF

	SET IDENTITY_INSERT [UserManagement].[ProductPageControl] ON 
	
	Select @ProductPageControlId = MAX(ProductPageControlId) +1 from [UserManagement].[ProductPageControl]

	INSERT [UserManagement].[ProductPageControl] ([ProductPageControlId], [ProductPageId], [ControlId], [CreatedBy], [CreatedDate])
	VALUES (@ProductPageControlId, @ProductPageId, @ControlId, @UserId, @Now)

	SET IDENTITY_INSERT [UserManagement].[ProductPageControl] OFF
END

--SelfGuidedTour
SET @ProductId  = 65
IF NOT EXISTS (SELECT TOP 1 1 FROM[UserManagement].[ProductPage] WHERE ProductId = @ProductId and ProductPageTypeId = 3)
BEGIN
	SET IDENTITY_INSERT [UserManagement].[Control] ON 
	Select @ControlId = MAX(controlid) +1 from [UserManagement].[Control]

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
	VALUES (@ControlId, NULL, 8, N'SelfGuidedTourRoleTemplateTabGroupUIId', NULL, NULL, 1, @UserId, @Now)

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
	VALUES (@ControlId+1, @ControlId, 9, N'SelfGuidedTourRoleTemplateRolesTabUIId', N'Roles', NULL, 1, @UserId, @Now)
	

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
	VALUES (@ControlId+2, @ControlId+1, 2, N'SelfGuidedTourRoleTemplateRolesSelectGridUIId', NULL, NULL, 1, @UserId, @Now)

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
	VALUES (@ControlId+3, @ControlId+2, 7, N'SelfGuidedTourRoleTemplateRoleRadioUIId', NULL, N'isAssigned', 1, @UserId, @Now)	

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
	VALUES (@ControlId+4, @ControlId+2, 5, N'SelfGuidedTourRoleTemplateRoleLabelUIId', N'Role', N'name', 2, @UserId, @Now)			

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
	VALUES (@ControlId+5, @ControlId+2, 5, N'SelfGuidedTourRoleTemplateRoleTypeLabelUIId', N'Role Type', N'roletype', 3, @UserId, @Now)	
	
	SET IDENTITY_INSERT [UserManagement].[Control] OFF

	SET IDENTITY_INSERT [UserManagement].[ControlAttribute] ON 
	
	Select @ControlAttributeId = MAX(ControlAttributeId) +1 from [UserManagement].[ControlAttribute]

	INSERT [UserManagement].[ControlAttribute] ([ControlAttributeId], [ControlId], [Key], [Value], [CreatedBy], [CreatedDate])
	VALUES (@ControlAttributeId, @ControlId+1, N'Default', N'True', @UserId, @Now)

	INSERT [UserManagement].[ControlAttribute] ([ControlAttributeId], [ControlId], [Key], [Value], [CreatedBy], [CreatedDate])
	VALUES (@ControlAttributeId+1, @ControlId+1, N'Hide', N'False', @UserId, @Now)

	SET IDENTITY_INSERT [UserManagement].[ControlAttribute] OFF

	SET IDENTITY_INSERT [UserManagement].[ProductPage] ON 
	
	Select @ProductPageId = MAX(ProductPageId) +1 from [UserManagement].[ProductPage]

	INSERT [UserManagement].[ProductPage] ([ProductPageId], [ProductId], [DisplayName],[ProductPageTypeId], [CreatedBy], [CreatedDate], [IsActive])
	VALUES (@ProductPageId, @ProductId, N'Self-Guided Tour Roles',@RoleTemplateProductPageTypeId, @UserId, @Now, 1)

	SET IDENTITY_INSERT [UserManagement].[ProductPage] OFF

	SET IDENTITY_INSERT [UserManagement].[ProductPageControl] ON 
	
	Select @ProductPageControlId = MAX(ProductPageControlId) +1 from [UserManagement].[ProductPageControl]

	INSERT [UserManagement].[ProductPageControl] ([ProductPageControlId], [ProductPageId], [ControlId], [CreatedBy], [CreatedDate])
	VALUES (@ProductPageControlId, @ProductPageId, @ControlId, @UserId, @Now)

	SET IDENTITY_INSERT [UserManagement].[ProductPageControl] OFF
END

--Lead2Lease
SET @ProductId  = 6
IF NOT EXISTS (SELECT TOP 1 1 FROM[UserManagement].[ProductPage] WHERE ProductId = @ProductId and ProductPageTypeId = 3)
BEGIN
	SET IDENTITY_INSERT [UserManagement].[Control] ON 
	Select @ControlId = MAX(controlid) +1 from [UserManagement].[Control]

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
	VALUES (@ControlId, NULL, 8, N'Lead2LeaseRoleTemplateTabGroupUIId', NULL, NULL, 1, @UserId, @Now)

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
	VALUES (@ControlId+1, @ControlId, 9, N'Lead2LeaseRoleTemplateRolesTabUIId', N'Rights', NULL, 1, @UserId, @Now)
	
	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
	VALUES (@ControlId+2, @ControlId+1, 13, N'Lead2LeaseRoleTemplateSelectaPresetRoleRightsSelectUIId', 'Select a Preset Role', 'roles', 1, @UserId, @Now)

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
	VALUES (@ControlId+3, @ControlId+1, 3, N'Lead2LeaseRoleTemplateRolesSelectGridUIId', NULL, NULL, 1, @UserId, @Now)

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
	VALUES (@ControlId+4, @ControlId+3, 10, N'Lead2LeaseRoleTemplateCheckboxUIId', NULL, N'isAssigned', 1, @UserId, @Now)	

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
	VALUES (@ControlId+5, @ControlId+3, 5, N'Lead2LeaseRoleTemplateRoleLabelUIId', N'Right', N'name', 2, @UserId, @Now)
	
	SET IDENTITY_INSERT [UserManagement].[Control] OFF

	SET IDENTITY_INSERT [UserManagement].[ControlAttribute] ON 
	
	Select @ControlAttributeId = MAX(ControlAttributeId) +1 from [UserManagement].[ControlAttribute]

	INSERT [UserManagement].[ControlAttribute] ([ControlAttributeId], [ControlId], [Key], [Value], [CreatedBy], [CreatedDate])
	VALUES (@ControlAttributeId, @ControlId+1, N'Default', N'True', @UserId, @Now)

	INSERT [UserManagement].[ControlAttribute] ([ControlAttributeId], [ControlId], [Key], [Value], [CreatedBy], [CreatedDate])
	VALUES (@ControlAttributeId+1, @ControlId+1, N'Hide', N'False', @UserId, @Now)

	INSERT [UserManagement].[ControlAttribute] ([ControlAttributeId], [ControlId], [Key], [Value], [CreatedBy], [CreatedDate])
	VALUES (@ControlAttributeId+2, @ControlId+3, N'ShowSelectAll', N'True', @UserId, @Now)

	SET IDENTITY_INSERT [UserManagement].[ControlAttribute] OFF

	SET IDENTITY_INSERT [UserManagement].[ProductPage] ON 
	
	Select @ProductPageId = MAX(ProductPageId) +1 from [UserManagement].[ProductPage]

	INSERT [UserManagement].[ProductPage] ([ProductPageId], [ProductId], [DisplayName],[ProductPageTypeId], [CreatedBy], [CreatedDate], [IsActive])
	VALUES (@ProductPageId, @ProductId, N'Lead2Lease Roles',@RoleTemplateProductPageTypeId, @UserId, @Now, 1)

	SET IDENTITY_INSERT [UserManagement].[ProductPage] OFF

	SET IDENTITY_INSERT [UserManagement].[ProductPageControl] ON 
	
	Select @ProductPageControlId = MAX(ProductPageControlId) +1 from [UserManagement].[ProductPageControl]

	INSERT [UserManagement].[ProductPageControl] ([ProductPageControlId], [ProductPageId], [ControlId], [CreatedBy], [CreatedDate])
	VALUES (@ProductPageControlId, @ProductPageId, @ControlId, @UserId, @Now)

	SET IDENTITY_INSERT [UserManagement].[ProductPageControl] OFF
END

--FinancialSuite
SET @ProductId  = 8
IF NOT EXISTS (SELECT TOP 1 1 FROM[UserManagement].[ProductPage] WHERE ProductId = @ProductId and ProductPageTypeId = 3)
BEGIN
	SET IDENTITY_INSERT [UserManagement].[Control] ON 
	Select @ControlId = MAX(controlid) +1 from [UserManagement].[Control]

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
	VALUES (@ControlId, NULL, 8, N'FinancialSuiteRoleTemplateTabGroupUIId', NULL, NULL, 1, @UserId, @Now)

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate]) 
	VALUES (@ControlId+1, @ControlId, 5, N'FinancialSuiteRoleTemplateOptions:LabelUIId', N'Options:', N'', 1, @UserId, @Now)
	
	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate]) 
	VALUES (@ControlId+2, @ControlId, 1, N'FinancialSuiteRoleTemplateAccesstoSiteSpendManagementonlySwitchUIId', N'Site user', N'hasAccessToSiteSpendManagementOnly', 2, @UserId, @Now)
	
	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate]) 
	VALUES (@ControlId+3, @ControlId, 1, N'FinancialSuiteRoleTemplateAllowaccesstoallcurrentandfutureentitiesSwitchUIId', N'Assign current and new entities automatically', N'hasAccessToAllCurrentFutureProperties', 3, @UserId, @Now)
	
	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate]) 
	VALUES (@ControlId+4, @ControlId, 1, N'FinancialSuiteRoleTemplateAccountingAdminSwitchUIId', N'Accounting Admin', N'isAccountingAdmin', 4, @UserId, @Now)

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
	VALUES (@ControlId+5, @ControlId, 9, N'FinancialSuiteRoleTemplateRolesTabUIId', N'Roles', NULL, 5, @UserId, @Now)	
	
	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
	VALUES (@ControlId+6, @ControlId+5, 3, N'FinancialSuiteRoleTemplateRolesMultiSelectGridUIId', NULL, NULL, 1, @UserId, @Now)

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
	VALUES (@ControlId+7, @ControlId+6, 10, N'FinancialSuiteRoleTemplateCheckboxUIId', NULL, N'isAssigned', 1, @UserId, @Now)	

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
	VALUES (@ControlId+8, @ControlId+6, 5, N'FinancialSuiteRoleTemplateRoleLabelUIId', N'Role', N'name', 2, @UserId, @Now)
	
	SET IDENTITY_INSERT [UserManagement].[Control] OFF

	SET IDENTITY_INSERT [UserManagement].[ControlAttribute] ON 
	
	Select @ControlAttributeId = MAX(ControlAttributeId) +1 from [UserManagement].[ControlAttribute]

	INSERT [UserManagement].[ControlAttribute] ([ControlAttributeId], [ControlId], [Key], [Value], [CreatedBy], [CreatedDate]) 
	VALUES (@ControlAttributeId, @ControlId+2, N'Default', N'True', @UserId, @Now)
	
	INSERT [UserManagement].[ControlAttribute] ([ControlAttributeId], [ControlId], [Key], [Value], [CreatedBy], [CreatedDate]) 
	VALUES (@ControlAttributeId + 1, @ControlId+3, N'Default', N'True', @UserId, @Now)
	
	INSERT [UserManagement].[ControlAttribute] ([ControlAttributeId], [ControlId], [Key], [Value], [CreatedBy], [CreatedDate]) 
	VALUES (@ControlAttributeId +2, @ControlId+4, N'Default', N'True', @UserId, @Now)

	INSERT [UserManagement].[ControlAttribute] ([ControlAttributeId], [ControlId], [Key], [Value], [CreatedBy], [CreatedDate])
	VALUES (@ControlAttributeId + 3 , @ControlId+5, N'Default', N'True', @UserId, @Now)

	INSERT [UserManagement].[ControlAttribute] ([ControlAttributeId], [ControlId], [Key], [Value], [CreatedBy], [CreatedDate])
	VALUES (@ControlAttributeId+4, @ControlId+5, N'Hide', N'False', @UserId, @Now)

	INSERT [UserManagement].[ControlAttribute] ([ControlAttributeId], [ControlId], [Key], [Value], [CreatedBy], [CreatedDate])
	VALUES (@ControlAttributeId+5, @ControlId+6, N'ShowSelectAll', N'True', @UserId, @Now)

	SET IDENTITY_INSERT [UserManagement].[ControlAttribute] OFF

	SET IDENTITY_INSERT [UserManagement].[ProductPage] ON 
	
	Select @ProductPageId = MAX(ProductPageId) +1 from [UserManagement].[ProductPage]

	INSERT [UserManagement].[ProductPage] ([ProductPageId], [ProductId], [DisplayName],[ProductPageTypeId], [CreatedBy], [CreatedDate], [IsActive])
	VALUES (@ProductPageId, @ProductId, N'Financial Suite Roles',@RoleTemplateProductPageTypeId, @UserId, @Now, 1)

	SET IDENTITY_INSERT [UserManagement].[ProductPage] OFF

	SET IDENTITY_INSERT [UserManagement].[ProductPageControl] ON 
	
	Select @ProductPageControlId = MAX(ProductPageControlId) +1 from [UserManagement].[ProductPageControl]

	INSERT [UserManagement].[ProductPageControl] ([ProductPageControlId], [ProductPageId], [ControlId], [CreatedBy], [CreatedDate])
	VALUES (@ProductPageControlId, @ProductPageId, @ControlId, @UserId, @Now)

	SET IDENTITY_INSERT [UserManagement].[ProductPageControl] OFF
END

--DocumentDirector
SET @ProductId  = 20
IF NOT EXISTS (SELECT TOP 1 1 FROM[UserManagement].[ProductPage] WHERE ProductId = @ProductId and ProductPageTypeId = 3)
BEGIN
	SET IDENTITY_INSERT [UserManagement].[Control] ON 
	Select @ControlId = MAX(controlid) +1 from [UserManagement].[Control]

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
	VALUES (@ControlId, NULL, 8, N'DocumentDirectorRoleTemplateTabGroupUIId', NULL, NULL, 1, @UserId, @Now)

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
	VALUES (@ControlId+1, @ControlId, 9, N'DocumentDirectorRoleTemplateRolesTabUIId', N'Roles', NULL, 1, @UserId, @Now)	

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
	VALUES (@ControlId+2, @ControlId+1, 3, N'DocumentDirectorRoleTemplateRolesSelectGridUIId', NULL, NULL, 1, @UserId, @Now)

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
	VALUES (@ControlId+3, @ControlId+2, 10, N'DocumentDirectorRoleTemplateCheckboxUIId', NULL, N'isAssigned', 1, @UserId, @Now)	

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
	VALUES (@ControlId+4, @ControlId+2, 5, N'DocumentDirectorRoleTemplateRoleLabelUIId', N'Role', N'name', 2, @UserId, @Now)

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
	VALUES (@ControlId+5, @ControlId+2, 5, N'DocumentDirectorRoleTemplateRoleTypeLabelUIId', N'Type', N'roletype', 3, @UserId, @Now)
	
	SET IDENTITY_INSERT [UserManagement].[Control] OFF

	SET IDENTITY_INSERT [UserManagement].[ControlAttribute] ON 
	
	Select @ControlAttributeId = MAX(ControlAttributeId) +1 from [UserManagement].[ControlAttribute]

	INSERT [UserManagement].[ControlAttribute] ([ControlAttributeId], [ControlId], [Key], [Value], [CreatedBy], [CreatedDate])
	VALUES (@ControlAttributeId, @ControlId+1, N'Default', N'True', @UserId, @Now)

	INSERT [UserManagement].[ControlAttribute] ([ControlAttributeId], [ControlId], [Key], [Value], [CreatedBy], [CreatedDate])
	VALUES (@ControlAttributeId+1, @ControlId+1, N'Hide', N'False', @UserId, @Now)

	INSERT [UserManagement].[ControlAttribute] ([ControlAttributeId], [ControlId], [Key], [Value], [CreatedBy], [CreatedDate])
	VALUES (@ControlAttributeId+2, @ControlId+2, N'ShowSelectAll', N'True', @UserId, @Now)

	SET IDENTITY_INSERT [UserManagement].[ControlAttribute] OFF

	SET IDENTITY_INSERT [UserManagement].[ProductPage] ON 
	
	Select @ProductPageId = MAX(ProductPageId) +1 from [UserManagement].[ProductPage]

	INSERT [UserManagement].[ProductPage] ([ProductPageId], [ProductId], [DisplayName],[ProductPageTypeId], [CreatedBy], [CreatedDate], [IsActive])
	VALUES (@ProductPageId, @ProductId, N'Document Director Roles',@RoleTemplateProductPageTypeId, @UserId, @Now, 1)

	SET IDENTITY_INSERT [UserManagement].[ProductPage] OFF

	SET IDENTITY_INSERT [UserManagement].[ProductPageControl] ON 
	
	Select @ProductPageControlId = MAX(ProductPageControlId) +1 from [UserManagement].[ProductPageControl]

	INSERT [UserManagement].[ProductPageControl] ([ProductPageControlId], [ProductPageId], [ControlId], [CreatedBy], [CreatedDate])
	VALUES (@ProductPageControlId, @ProductPageId, @ControlId, @UserId, @Now)

	SET IDENTITY_INSERT [UserManagement].[ProductPageControl] OFF
END

--BusinessIntelligence
SET @ProductId  = 29
IF NOT EXISTS (SELECT TOP 1 1 FROM[UserManagement].[ProductPage] WHERE ProductId = @ProductId and ProductPageTypeId = 3)
BEGIN
	SET IDENTITY_INSERT [UserManagement].[Control] ON 
	Select @ControlId = MAX(controlid) +1 from [UserManagement].[Control]

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
	VALUES (@ControlId, NULL, 8, N'BusinessIntelligenceRoleTemplateTabGroupUIId', NULL, NULL, 1, @UserId, @Now)

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
	VALUES (@ControlId+1, @ControlId, 9, N'BusinessIntelligenceRoleTemplateRolesTabUIId', N'Roles', NULL, 1, @UserId, @Now)
	

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
	VALUES (@ControlId+2, @ControlId+1, 3, N'BusinessIntelligenceRoleTemplateRolesSelectGridUIId', NULL, NULL, 1, @UserId, @Now)

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
	VALUES (@ControlId+3, @ControlId+2, 10, N'BusinessIntelligenceRoleTemplateCheckboxUIId', NULL, N'isAssigned', 1, @UserId, @Now)	

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
	VALUES (@ControlId+4, @ControlId+2, 5, N'BusinessIntelligenceRoleTemplateRoleLabelUIId', N'Role', N'name', 2, @UserId, @Now)
	
	SET IDENTITY_INSERT [UserManagement].[Control] OFF

	SET IDENTITY_INSERT [UserManagement].[ControlAttribute] ON 
	
	Select @ControlAttributeId = MAX(ControlAttributeId) +1 from [UserManagement].[ControlAttribute]

	INSERT [UserManagement].[ControlAttribute] ([ControlAttributeId], [ControlId], [Key], [Value], [CreatedBy], [CreatedDate])
	VALUES (@ControlAttributeId, @ControlId+1, N'Default', N'True', @UserId, @Now)

	INSERT [UserManagement].[ControlAttribute] ([ControlAttributeId], [ControlId], [Key], [Value], [CreatedBy], [CreatedDate])
	VALUES (@ControlAttributeId+1, @ControlId+1, N'Hide', N'False', @UserId, @Now)

	INSERT [UserManagement].[ControlAttribute] ([ControlAttributeId], [ControlId], [Key], [Value], [CreatedBy], [CreatedDate])
	VALUES (@ControlAttributeId+2, @ControlId+2, N'ShowSelectAll', N'True', @UserId, @Now)

	SET IDENTITY_INSERT [UserManagement].[ControlAttribute] OFF

	SET IDENTITY_INSERT [UserManagement].[ProductPage] ON 
	
	Select @ProductPageId = MAX(ProductPageId) +1 from [UserManagement].[ProductPage]

	INSERT [UserManagement].[ProductPage] ([ProductPageId], [ProductId], [DisplayName],[ProductPageTypeId], [CreatedBy], [CreatedDate], [IsActive])
	VALUES (@ProductPageId, @ProductId, N'Business Intelligence Roles',@RoleTemplateProductPageTypeId, @UserId, @Now, 1)

	SET IDENTITY_INSERT [UserManagement].[ProductPage] OFF

	SET IDENTITY_INSERT [UserManagement].[ProductPageControl] ON 
	
	Select @ProductPageControlId = MAX(ProductPageControlId) +1 from [UserManagement].[ProductPageControl]

	INSERT [UserManagement].[ProductPageControl] ([ProductPageControlId], [ProductPageId], [ControlId], [CreatedBy], [CreatedDate])
	VALUES (@ProductPageControlId, @ProductPageId, @ControlId, @UserId, @Now)

	SET IDENTITY_INSERT [UserManagement].[ProductPageControl] OFF
END

--InvestmentAnalytics
SET @ProductId  = 31
IF NOT EXISTS (SELECT TOP 1 1 FROM[UserManagement].[ProductPage] WHERE ProductId = @ProductId and ProductPageTypeId = 3)
BEGIN
	SET IDENTITY_INSERT [UserManagement].[Control] ON 
	Select @ControlId = MAX(controlid) +1 from [UserManagement].[Control]

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
	VALUES (@ControlId, NULL, 8, N'InvestmentAnalyticsRoleTemplateTabGroupUIId', NULL, NULL, 1, @UserId, @Now)

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
	VALUES (@ControlId+1, @ControlId, 9, N'InvestmentAnalyticsRoleTemplateRolesTabUIId', N'Roles', NULL, 1, @UserId, @Now)
	

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
	VALUES (@ControlId+2, @ControlId+1, 3, N'InvestmentAnalyticsRoleTemplateRolesSelectGridUIId', NULL, NULL, 1, @UserId, @Now)

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
	VALUES (@ControlId+3, @ControlId+2, 10, N'InvestmentAnalyticsRoleTemplateCheckboxUIId', NULL, N'isAssigned', 1, @UserId, @Now)	

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
	VALUES (@ControlId+4, @ControlId+2, 5, N'InvestmentAnalyticsRoleTemplateRoleLabelUIId', N'Role', N'name', 2, @UserId, @Now)
	
	SET IDENTITY_INSERT [UserManagement].[Control] OFF

	SET IDENTITY_INSERT [UserManagement].[ControlAttribute] ON 
	
	Select @ControlAttributeId = MAX(ControlAttributeId) +1 from [UserManagement].[ControlAttribute]

	INSERT [UserManagement].[ControlAttribute] ([ControlAttributeId], [ControlId], [Key], [Value], [CreatedBy], [CreatedDate])
	VALUES (@ControlAttributeId, @ControlId+1, N'Default', N'True', @UserId, @Now)

	INSERT [UserManagement].[ControlAttribute] ([ControlAttributeId], [ControlId], [Key], [Value], [CreatedBy], [CreatedDate])
	VALUES (@ControlAttributeId+1, @ControlId+1, N'Hide', N'False', @UserId, @Now)

	INSERT [UserManagement].[ControlAttribute] ([ControlAttributeId], [ControlId], [Key], [Value], [CreatedBy], [CreatedDate])
	VALUES (@ControlAttributeId+2, @ControlId+2, N'ShowSelectAll', N'True', @UserId, @Now)

	SET IDENTITY_INSERT [UserManagement].[ControlAttribute] OFF

	SET IDENTITY_INSERT [UserManagement].[ProductPage] ON 
	
	Select @ProductPageId = MAX(ProductPageId) +1 from [UserManagement].[ProductPage]

	INSERT [UserManagement].[ProductPage] ([ProductPageId], [ProductId], [DisplayName],[ProductPageTypeId], [CreatedBy], [CreatedDate], [IsActive])
	VALUES (@ProductPageId, @ProductId, N'Investment Analytics Roles',@RoleTemplateProductPageTypeId, @UserId, @Now, 1)

	SET IDENTITY_INSERT [UserManagement].[ProductPage] OFF

	SET IDENTITY_INSERT [UserManagement].[ProductPageControl] ON 
	
	Select @ProductPageControlId = MAX(ProductPageControlId) +1 from [UserManagement].[ProductPageControl]

	INSERT [UserManagement].[ProductPageControl] ([ProductPageControlId], [ProductPageId], [ControlId], [CreatedBy], [CreatedDate])
	VALUES (@ProductPageControlId, @ProductPageId, @ControlId, @UserId, @Now)

	SET IDENTITY_INSERT [UserManagement].[ProductPageControl] OFF
END

--YieldStar
SET @ProductId  = 32
IF NOT EXISTS (SELECT TOP 1 1 FROM[UserManagement].[ProductPage] WHERE ProductId = @ProductId and ProductPageTypeId = 3)
BEGIN
	SET IDENTITY_INSERT [UserManagement].[Control] ON 
	Select @ControlId = MAX(controlid) +1 from [UserManagement].[Control]

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
	VALUES (@ControlId, NULL, 8, N'YieldStarRoleTemplateTabGroupUIId', NULL, NULL, 1, @UserId, @Now)

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
	VALUES (@ControlId+1, @ControlId, 9, N'YieldStarRoleTemplateRolesTabUIId', N'Roles', NULL, 1, @UserId, @Now)
	

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
	VALUES (@ControlId+2, @ControlId+1, 3, N'YieldStarRoleTemplateRolesSelectGridUIId', NULL, NULL, 1, @UserId, @Now)

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
	VALUES (@ControlId+3, @ControlId+2, 10, N'YieldStarRoleTemplateCheckboxUIId', NULL, N'isAssigned', 1, @UserId, @Now)	

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
	VALUES (@ControlId+4, @ControlId+2, 5, N'YieldStarRoleTemplateRoleLabelUIId', N'Role', N'name', 2, @UserId, @Now)
	
	SET IDENTITY_INSERT [UserManagement].[Control] OFF

	SET IDENTITY_INSERT [UserManagement].[ControlAttribute] ON 
	
	Select @ControlAttributeId = MAX(ControlAttributeId) +1 from [UserManagement].[ControlAttribute]

	INSERT [UserManagement].[ControlAttribute] ([ControlAttributeId], [ControlId], [Key], [Value], [CreatedBy], [CreatedDate])
	VALUES (@ControlAttributeId, @ControlId+1, N'Default', N'True', @UserId, @Now)

	INSERT [UserManagement].[ControlAttribute] ([ControlAttributeId], [ControlId], [Key], [Value], [CreatedBy], [CreatedDate])
	VALUES (@ControlAttributeId+1, @ControlId+1, N'Hide', N'False', @UserId, @Now)

	INSERT [UserManagement].[ControlAttribute] ([ControlAttributeId], [ControlId], [Key], [Value], [CreatedBy], [CreatedDate])
	VALUES (@ControlAttributeId+2, @ControlId+2, N'ShowSelectAll', N'True', @UserId, @Now)

	SET IDENTITY_INSERT [UserManagement].[ControlAttribute] OFF

	SET IDENTITY_INSERT [UserManagement].[ProductPage] ON 
	
	Select @ProductPageId = MAX(ProductPageId) +1 from [UserManagement].[ProductPage]

	INSERT [UserManagement].[ProductPage] ([ProductPageId], [ProductId], [DisplayName],[ProductPageTypeId], [CreatedBy], [CreatedDate], [IsActive])
	VALUES (@ProductPageId, @ProductId, N'YieldStar Roles',@RoleTemplateProductPageTypeId, @UserId, @Now, 1)

	SET IDENTITY_INSERT [UserManagement].[ProductPage] OFF

	SET IDENTITY_INSERT [UserManagement].[ProductPageControl] ON 
	
	Select @ProductPageControlId = MAX(ProductPageControlId) +1 from [UserManagement].[ProductPageControl]

	INSERT [UserManagement].[ProductPageControl] ([ProductPageControlId], [ProductPageId], [ControlId], [CreatedBy], [CreatedDate])
	VALUES (@ProductPageControlId, @ProductPageId, @ControlId, @UserId, @Now)

	SET IDENTITY_INSERT [UserManagement].[ProductPageControl] OFF
END

--Benchmarking
SET @ProductId  = 34
IF NOT EXISTS (SELECT TOP 1 1 FROM[UserManagement].[ProductPage] WHERE ProductId = @ProductId and ProductPageTypeId = 3)
BEGIN
	SET IDENTITY_INSERT [UserManagement].[Control] ON 
	Select @ControlId = MAX(controlid) +1 from [UserManagement].[Control]

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
	VALUES (@ControlId, NULL, 8, N'BenchmarkingRoleTemplateTabGroupUIId', NULL, NULL, 1, @UserId, @Now)

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
	VALUES (@ControlId+1, @ControlId, 9, N'BenchmarkingRoleTemplateRolesTabUIId', N'Roles', NULL, 1, @UserId, @Now)
	

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
	VALUES (@ControlId+2, @ControlId+1, 3, N'BenchmarkingRoleTemplateRolesSelectGridUIId', NULL, NULL, 1, @UserId, @Now)

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
	VALUES (@ControlId+3, @ControlId+2, 10, N'BenchmarkingRoleTemplateCheckboxUIId', NULL, N'isAssigned', 1, @UserId, @Now)	

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
	VALUES (@ControlId+4, @ControlId+2, 5, N'BenchmarkingRoleTemplateRoleLabelUIId', N'Role', N'name', 2, @UserId, @Now)
	
	SET IDENTITY_INSERT [UserManagement].[Control] OFF

	SET IDENTITY_INSERT [UserManagement].[ControlAttribute] ON 
	
	Select @ControlAttributeId = MAX(ControlAttributeId) +1 from [UserManagement].[ControlAttribute]

	INSERT [UserManagement].[ControlAttribute] ([ControlAttributeId], [ControlId], [Key], [Value], [CreatedBy], [CreatedDate])
	VALUES (@ControlAttributeId, @ControlId+1, N'Default', N'True', @UserId, @Now)

	INSERT [UserManagement].[ControlAttribute] ([ControlAttributeId], [ControlId], [Key], [Value], [CreatedBy], [CreatedDate])
	VALUES (@ControlAttributeId+1, @ControlId+1, N'Hide', N'False', @UserId, @Now)

	INSERT [UserManagement].[ControlAttribute] ([ControlAttributeId], [ControlId], [Key], [Value], [CreatedBy], [CreatedDate])
	VALUES (@ControlAttributeId+2, @ControlId+2, N'ShowSelectAll', N'True', @UserId, @Now)

	SET IDENTITY_INSERT [UserManagement].[ControlAttribute] OFF

	SET IDENTITY_INSERT [UserManagement].[ProductPage] ON 
	
	Select @ProductPageId = MAX(ProductPageId) +1 from [UserManagement].[ProductPage]

	INSERT [UserManagement].[ProductPage] ([ProductPageId], [ProductId], [DisplayName],[ProductPageTypeId], [CreatedBy], [CreatedDate], [IsActive])
	VALUES (@ProductPageId, @ProductId, N'Benchmarking Roles',@RoleTemplateProductPageTypeId, @UserId, @Now, 1)

	SET IDENTITY_INSERT [UserManagement].[ProductPage] OFF

	SET IDENTITY_INSERT [UserManagement].[ProductPageControl] ON 
	
	Select @ProductPageControlId = MAX(ProductPageControlId) +1 from [UserManagement].[ProductPageControl]

	INSERT [UserManagement].[ProductPageControl] ([ProductPageControlId], [ProductPageId], [ControlId], [CreatedBy], [CreatedDate])
	VALUES (@ProductPageControlId, @ProductPageId, @ControlId, @UserId, @Now)

	SET IDENTITY_INSERT [UserManagement].[ProductPageControl] OFF
END

--DepositAlternative
SET @ProductId  = 47
IF NOT EXISTS (SELECT TOP 1 1 FROM[UserManagement].[ProductPage] WHERE ProductId = @ProductId and ProductPageTypeId = 3)
BEGIN
	SET IDENTITY_INSERT [UserManagement].[Control] ON 
	Select @ControlId = MAX(controlid) +1 from [UserManagement].[Control]

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
	VALUES (@ControlId, NULL, 8, N'DepositAlternativeRoleTemplateTabGroupUIId', NULL, NULL, 1, @UserId, @Now)

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
	VALUES (@ControlId+1, @ControlId, 9, N'DepositAlternativeRoleTemplateRolesTabUIId', N'Roles', NULL, 1, @UserId, @Now)
	

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
	VALUES (@ControlId+2, @ControlId+1, 3, N'DepositAlternativeRoleTemplateRolesSelectGridUIId', NULL, NULL, 1, @UserId, @Now)

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
	VALUES (@ControlId+3, @ControlId+2, 10, N'DepositAlternativeRoleTemplateCheckboxUIId', NULL, N'isAssigned', 1, @UserId, @Now)	

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
	VALUES (@ControlId+4, @ControlId+2, 5, N'DepositAlternativeRoleTemplateRoleLabelUIId', N'Role', N'name', 2, @UserId, @Now)
	
	SET IDENTITY_INSERT [UserManagement].[Control] OFF

	SET IDENTITY_INSERT [UserManagement].[ControlAttribute] ON 
	
	Select @ControlAttributeId = MAX(ControlAttributeId) +1 from [UserManagement].[ControlAttribute]

	INSERT [UserManagement].[ControlAttribute] ([ControlAttributeId], [ControlId], [Key], [Value], [CreatedBy], [CreatedDate])
	VALUES (@ControlAttributeId, @ControlId+1, N'Default', N'True', @UserId, @Now)

	INSERT [UserManagement].[ControlAttribute] ([ControlAttributeId], [ControlId], [Key], [Value], [CreatedBy], [CreatedDate])
	VALUES (@ControlAttributeId+1, @ControlId+1, N'Hide', N'False', @UserId, @Now)

	INSERT [UserManagement].[ControlAttribute] ([ControlAttributeId], [ControlId], [Key], [Value], [CreatedBy], [CreatedDate])
	VALUES (@ControlAttributeId+2, @ControlId+2, N'ShowSelectAll', N'True', @UserId, @Now)

	SET IDENTITY_INSERT [UserManagement].[ControlAttribute] OFF

	SET IDENTITY_INSERT [UserManagement].[ProductPage] ON 
	
	Select @ProductPageId = MAX(ProductPageId) +1 from [UserManagement].[ProductPage]

	INSERT [UserManagement].[ProductPage] ([ProductPageId], [ProductId], [DisplayName],[ProductPageTypeId], [CreatedBy], [CreatedDate], [IsActive])
	VALUES (@ProductPageId, @ProductId, N'Deposit Alternative Roles',@RoleTemplateProductPageTypeId, @UserId, @Now, 1)

	SET IDENTITY_INSERT [UserManagement].[ProductPage] OFF

	SET IDENTITY_INSERT [UserManagement].[ProductPageControl] ON 
	
	Select @ProductPageControlId = MAX(ProductPageControlId) +1 from [UserManagement].[ProductPageControl]

	INSERT [UserManagement].[ProductPageControl] ([ProductPageControlId], [ProductPageId], [ControlId], [CreatedBy], [CreatedDate])
	VALUES (@ProductPageControlId, @ProductPageId, @ControlId, @UserId, @Now)

	SET IDENTITY_INSERT [UserManagement].[ProductPageControl] OFF
END

--ClickPay
SET @ProductId  = 48
IF NOT EXISTS (SELECT TOP 1 1 FROM[UserManagement].[ProductPage] WHERE ProductId = @ProductId and ProductPageTypeId = 3)
BEGIN
	SET IDENTITY_INSERT [UserManagement].[Control] ON 
	Select @ControlId = MAX(controlid) +1 from [UserManagement].[Control]

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
	VALUES (@ControlId, NULL, 8, N'ClickPayRoleTemplateTabGroupUIId', NULL, NULL, 1, @UserId, @Now)

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
	VALUES (@ControlId+1, @ControlId, 9, N'ClickPayRoleTemplateRolesTabUIId', N'Roles', NULL, 1, @UserId, @Now)
	

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
	VALUES (@ControlId+2, @ControlId+1, 3, N'ClickPayRoleTemplateRolesSelectGridUIId', NULL, NULL, 1, @UserId, @Now)

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
	VALUES (@ControlId+3, @ControlId+2, 10, N'ClickPayRoleTemplateCheckboxUIId', NULL, N'isAssigned', 1, @UserId, @Now)	

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
	VALUES (@ControlId+4, @ControlId+2, 5, N'ClickPayRoleTemplateRoleLabelUIId', N'Role', N'name', 2, @UserId, @Now)
	
	SET IDENTITY_INSERT [UserManagement].[Control] OFF

	SET IDENTITY_INSERT [UserManagement].[ControlAttribute] ON 
	
	Select @ControlAttributeId = MAX(ControlAttributeId) +1 from [UserManagement].[ControlAttribute]

	INSERT [UserManagement].[ControlAttribute] ([ControlAttributeId], [ControlId], [Key], [Value], [CreatedBy], [CreatedDate])
	VALUES (@ControlAttributeId, @ControlId+1, N'Default', N'True', @UserId, @Now)

	INSERT [UserManagement].[ControlAttribute] ([ControlAttributeId], [ControlId], [Key], [Value], [CreatedBy], [CreatedDate])
	VALUES (@ControlAttributeId+1, @ControlId+1, N'Hide', N'False', @UserId, @Now)

	INSERT [UserManagement].[ControlAttribute] ([ControlAttributeId], [ControlId], [Key], [Value], [CreatedBy], [CreatedDate])
	VALUES (@ControlAttributeId+2, @ControlId+2, N'ShowSelectAll', N'True', @UserId, @Now)

	SET IDENTITY_INSERT [UserManagement].[ControlAttribute] OFF

	SET IDENTITY_INSERT [UserManagement].[ProductPage] ON 
	
	Select @ProductPageId = MAX(ProductPageId) +1 from [UserManagement].[ProductPage]

	INSERT [UserManagement].[ProductPage] ([ProductPageId], [ProductId], [DisplayName],[ProductPageTypeId], [CreatedBy], [CreatedDate], [IsActive])
	VALUES (@ProductPageId, @ProductId, N'ClickPay Roles',@RoleTemplateProductPageTypeId, @UserId, @Now, 1)

	SET IDENTITY_INSERT [UserManagement].[ProductPage] OFF

	SET IDENTITY_INSERT [UserManagement].[ProductPageControl] ON 
	
	Select @ProductPageControlId = MAX(ProductPageControlId) +1 from [UserManagement].[ProductPageControl]

	INSERT [UserManagement].[ProductPageControl] ([ProductPageControlId], [ProductPageId], [ControlId], [CreatedBy], [CreatedDate])
	VALUES (@ProductPageControlId, @ProductPageId, @ControlId, @UserId, @Now)

	SET IDENTITY_INSERT [UserManagement].[ProductPageControl] OFF
END

--LRO
SET @ProductId  = 51
IF NOT EXISTS (SELECT TOP 1 1 FROM[UserManagement].[ProductPage] WHERE ProductId = @ProductId and ProductPageTypeId = 3)
BEGIN
	SET IDENTITY_INSERT [UserManagement].[Control] ON 
	Select @ControlId = MAX(controlid) +1 from [UserManagement].[Control]

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
	VALUES (@ControlId, NULL, 8, N'LRORoleTemplateTabGroupUIId', NULL, NULL, 1, @UserId, @Now)

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
	VALUES (@ControlId+1, @ControlId, 9, N'LRORoleTemplateRolesTabUIId', N'Roles', NULL, 1, @UserId, @Now)
	

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
	VALUES (@ControlId+2, @ControlId+1, 3, N'LRORoleTemplateRolesSelectGridUIId', NULL, NULL, 1, @UserId, @Now)

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
	VALUES (@ControlId+3, @ControlId+2, 10, N'LRORoleTemplateCheckboxUIId', NULL, N'isAssigned', 1, @UserId, @Now)	

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
	VALUES (@ControlId+4, @ControlId+2, 5, N'LRORoleTemplateRoleLabelUIId', N'Role', N'name', 2, @UserId, @Now)
	
	SET IDENTITY_INSERT [UserManagement].[Control] OFF

	SET IDENTITY_INSERT [UserManagement].[ControlAttribute] ON 
	
	Select @ControlAttributeId = MAX(ControlAttributeId) +1 from [UserManagement].[ControlAttribute]

	INSERT [UserManagement].[ControlAttribute] ([ControlAttributeId], [ControlId], [Key], [Value], [CreatedBy], [CreatedDate])
	VALUES (@ControlAttributeId, @ControlId+1, N'Default', N'True', @UserId, @Now)

	INSERT [UserManagement].[ControlAttribute] ([ControlAttributeId], [ControlId], [Key], [Value], [CreatedBy], [CreatedDate])
	VALUES (@ControlAttributeId+1, @ControlId+1, N'Hide', N'False', @UserId, @Now)

	INSERT [UserManagement].[ControlAttribute] ([ControlAttributeId], [ControlId], [Key], [Value], [CreatedBy], [CreatedDate])
	VALUES (@ControlAttributeId+2, @ControlId+2, N'ShowSelectAll', N'True', @UserId, @Now)

	SET IDENTITY_INSERT [UserManagement].[ControlAttribute] OFF

	SET IDENTITY_INSERT [UserManagement].[ProductPage] ON 
	
	Select @ProductPageId = MAX(ProductPageId) +1 from [UserManagement].[ProductPage]

	INSERT [UserManagement].[ProductPage] ([ProductPageId], [ProductId], [DisplayName],[ProductPageTypeId], [CreatedBy], [CreatedDate], [IsActive])
	VALUES (@ProductPageId, @ProductId, N'LRO Roles',@RoleTemplateProductPageTypeId, @UserId, @Now, 1)

	SET IDENTITY_INSERT [UserManagement].[ProductPage] OFF

	SET IDENTITY_INSERT [UserManagement].[ProductPageControl] ON 
	
	Select @ProductPageControlId = MAX(ProductPageControlId) +1 from [UserManagement].[ProductPageControl]

	INSERT [UserManagement].[ProductPageControl] ([ProductPageControlId], [ProductPageId], [ControlId], [CreatedBy], [CreatedDate])
	VALUES (@ProductPageControlId, @ProductPageId, @ControlId, @UserId, @Now)

	SET IDENTITY_INSERT [UserManagement].[ProductPageControl] OFF
END

--AmenityOptimization
SET @ProductId  = 52
IF NOT EXISTS (SELECT TOP 1 1 FROM[UserManagement].[ProductPage] WHERE ProductId = @ProductId and ProductPageTypeId = 3)
BEGIN
	SET IDENTITY_INSERT [UserManagement].[Control] ON 
	Select @ControlId = MAX(controlid) +1 from [UserManagement].[Control]

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
	VALUES (@ControlId, NULL, 8, N'AmenityOptimizationRoleTemplateTabGroupUIId', NULL, NULL, 1, @UserId, @Now)

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
	VALUES (@ControlId+1, @ControlId, 9, N'AmenityOptimizationRoleTemplateRolesTabUIId', N'Roles', NULL, 1, @UserId, @Now)
	

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
	VALUES (@ControlId+2, @ControlId+1, 3, N'AmenityOptimizationRoleTemplateRolesSelectGridUIId', NULL, NULL, 1, @UserId, @Now)

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
	VALUES (@ControlId+3, @ControlId+2, 10, N'AmenityOptimizationRoleTemplateCheckboxUIId', NULL, N'isAssigned', 1, @UserId, @Now)	

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
	VALUES (@ControlId+4, @ControlId+2, 5, N'AmenityOptimizationRoleTemplateRoleLabelUIId', N'Role', N'name', 2, @UserId, @Now)
	
	SET IDENTITY_INSERT [UserManagement].[Control] OFF

	SET IDENTITY_INSERT [UserManagement].[ControlAttribute] ON 
	
	Select @ControlAttributeId = MAX(ControlAttributeId) +1 from [UserManagement].[ControlAttribute]

	INSERT [UserManagement].[ControlAttribute] ([ControlAttributeId], [ControlId], [Key], [Value], [CreatedBy], [CreatedDate])
	VALUES (@ControlAttributeId, @ControlId+1, N'Default', N'True', @UserId, @Now)

	INSERT [UserManagement].[ControlAttribute] ([ControlAttributeId], [ControlId], [Key], [Value], [CreatedBy], [CreatedDate])
	VALUES (@ControlAttributeId+1, @ControlId+1, N'Hide', N'False', @UserId, @Now)

	INSERT [UserManagement].[ControlAttribute] ([ControlAttributeId], [ControlId], [Key], [Value], [CreatedBy], [CreatedDate])
	VALUES (@ControlAttributeId+2, @ControlId+2, N'ShowSelectAll', N'True', @UserId, @Now)

	SET IDENTITY_INSERT [UserManagement].[ControlAttribute] OFF

	SET IDENTITY_INSERT [UserManagement].[ProductPage] ON 
	
	Select @ProductPageId = MAX(ProductPageId) +1 from [UserManagement].[ProductPage]

	INSERT [UserManagement].[ProductPage] ([ProductPageId], [ProductId], [DisplayName],[ProductPageTypeId], [CreatedBy], [CreatedDate], [IsActive])
	VALUES (@ProductPageId, @ProductId, N'Amenity Optimization Roles',@RoleTemplateProductPageTypeId, @UserId, @Now, 1)

	SET IDENTITY_INSERT [UserManagement].[ProductPage] OFF

	SET IDENTITY_INSERT [UserManagement].[ProductPageControl] ON 
	
	Select @ProductPageControlId = MAX(ProductPageControlId) +1 from [UserManagement].[ProductPageControl]

	INSERT [UserManagement].[ProductPageControl] ([ProductPageControlId], [ProductPageId], [ControlId], [CreatedBy], [CreatedDate])
	VALUES (@ProductPageControlId, @ProductPageId, @ControlId, @UserId, @Now)

	SET IDENTITY_INSERT [UserManagement].[ProductPageControl] OFF
END

--AIRevenueManagement
SET @ProductId  = 53
IF NOT EXISTS (SELECT TOP 1 1 FROM[UserManagement].[ProductPage] WHERE ProductId = @ProductId and ProductPageTypeId = 3)
BEGIN
	SET IDENTITY_INSERT [UserManagement].[Control] ON 
	Select @ControlId = MAX(controlid) +1 from [UserManagement].[Control]

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
	VALUES (@ControlId, NULL, 8, N'AIRevenueManagementRoleTemplateTabGroupUIId', NULL, NULL, 1, @UserId, @Now)

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
	VALUES (@ControlId+1, @ControlId, 9, N'AIRevenueManagementRoleTemplateRolesTabUIId', N'Roles', NULL, 1, @UserId, @Now)
	

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
	VALUES (@ControlId+2, @ControlId+1, 3, N'AIRevenueManagementRoleTemplateRolesSelectGridUIId', NULL, NULL, 1, @UserId, @Now)

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
	VALUES (@ControlId+3, @ControlId+2, 10, N'AIRevenueManagementRoleTemplateCheckboxUIId', NULL, N'isAssigned', 1, @UserId, @Now)	

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
	VALUES (@ControlId+4, @ControlId+2, 5, N'AIRevenueManagementRoleTemplateRoleLabelUIId', N'Role', N'name', 2, @UserId, @Now)
	
	SET IDENTITY_INSERT [UserManagement].[Control] OFF

	SET IDENTITY_INSERT [UserManagement].[ControlAttribute] ON 
	
	Select @ControlAttributeId = MAX(ControlAttributeId) +1 from [UserManagement].[ControlAttribute]

	INSERT [UserManagement].[ControlAttribute] ([ControlAttributeId], [ControlId], [Key], [Value], [CreatedBy], [CreatedDate])
	VALUES (@ControlAttributeId, @ControlId+1, N'Default', N'True', @UserId, @Now)

	INSERT [UserManagement].[ControlAttribute] ([ControlAttributeId], [ControlId], [Key], [Value], [CreatedBy], [CreatedDate])
	VALUES (@ControlAttributeId+1, @ControlId+1, N'Hide', N'False', @UserId, @Now)

	INSERT [UserManagement].[ControlAttribute] ([ControlAttributeId], [ControlId], [Key], [Value], [CreatedBy], [CreatedDate])
	VALUES (@ControlAttributeId+2, @ControlId+2, N'ShowSelectAll', N'True', @UserId, @Now)

	SET IDENTITY_INSERT [UserManagement].[ControlAttribute] OFF

	SET IDENTITY_INSERT [UserManagement].[ProductPage] ON 
	
	Select @ProductPageId = MAX(ProductPageId) +1 from [UserManagement].[ProductPage]

	INSERT [UserManagement].[ProductPage] ([ProductPageId], [ProductId], [DisplayName],[ProductPageTypeId], [CreatedBy], [CreatedDate], [IsActive])
	VALUES (@ProductPageId, @ProductId, N'AI Revenue Management Roles',@RoleTemplateProductPageTypeId, @UserId, @Now, 1)

	SET IDENTITY_INSERT [UserManagement].[ProductPage] OFF

	SET IDENTITY_INSERT [UserManagement].[ProductPageControl] ON 
	
	Select @ProductPageControlId = MAX(ProductPageControlId) +1 from [UserManagement].[ProductPageControl]

	INSERT [UserManagement].[ProductPageControl] ([ProductPageControlId], [ProductPageId], [ControlId], [CreatedBy], [CreatedDate])
	VALUES (@ProductPageControlId, @ProductPageId, @ControlId, @UserId, @Now)

	SET IDENTITY_INSERT [UserManagement].[ProductPageControl] OFF
END

--RentControl
SET @ProductId  = 54
IF NOT EXISTS (SELECT TOP 1 1 FROM[UserManagement].[ProductPage] WHERE ProductId = @ProductId and ProductPageTypeId = 3)
BEGIN
	SET IDENTITY_INSERT [UserManagement].[Control] ON 
	Select @ControlId = MAX(controlid) +1 from [UserManagement].[Control]

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
	VALUES (@ControlId, NULL, 8, N'RentControlRoleTemplateTabGroupUIId', NULL, NULL, 1, @UserId, @Now)

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
	VALUES (@ControlId+1, @ControlId, 9, N'RentControlRoleTemplateRolesTabUIId', N'Roles', NULL, 1, @UserId, @Now)
	

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
	VALUES (@ControlId+2, @ControlId+1, 3, N'RentControlRoleTemplateRolesSelectGridUIId', NULL, NULL, 1, @UserId, @Now)

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
	VALUES (@ControlId+3, @ControlId+2, 10, N'RentControlRoleTemplateCheckboxUIId', NULL, N'isAssigned', 1, @UserId, @Now)	

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
	VALUES (@ControlId+4, @ControlId+2, 5, N'RentControlRoleTemplateRoleLabelUIId', N'Role', N'name', 2, @UserId, @Now)
	
	SET IDENTITY_INSERT [UserManagement].[Control] OFF

	SET IDENTITY_INSERT [UserManagement].[ControlAttribute] ON 
	
	Select @ControlAttributeId = MAX(ControlAttributeId) +1 from [UserManagement].[ControlAttribute]

	INSERT [UserManagement].[ControlAttribute] ([ControlAttributeId], [ControlId], [Key], [Value], [CreatedBy], [CreatedDate])
	VALUES (@ControlAttributeId, @ControlId+1, N'Default', N'True', @UserId, @Now)

	INSERT [UserManagement].[ControlAttribute] ([ControlAttributeId], [ControlId], [Key], [Value], [CreatedBy], [CreatedDate])
	VALUES (@ControlAttributeId+1, @ControlId+1, N'Hide', N'False', @UserId, @Now)

	INSERT [UserManagement].[ControlAttribute] ([ControlAttributeId], [ControlId], [Key], [Value], [CreatedBy], [CreatedDate])
	VALUES (@ControlAttributeId+2, @ControlId+2, N'ShowSelectAll', N'True', @UserId, @Now)

	SET IDENTITY_INSERT [UserManagement].[ControlAttribute] OFF

	SET IDENTITY_INSERT [UserManagement].[ProductPage] ON 
	
	Select @ProductPageId = MAX(ProductPageId) +1 from [UserManagement].[ProductPage]

	INSERT [UserManagement].[ProductPage] ([ProductPageId], [ProductId], [DisplayName],[ProductPageTypeId], [CreatedBy], [CreatedDate], [IsActive])
	VALUES (@ProductPageId, @ProductId, N'Rent Control Roles',@RoleTemplateProductPageTypeId, @UserId, @Now, 1)

	SET IDENTITY_INSERT [UserManagement].[ProductPage] OFF

	SET IDENTITY_INSERT [UserManagement].[ProductPageControl] ON 
	
	Select @ProductPageControlId = MAX(ProductPageControlId) +1 from [UserManagement].[ProductPageControl]

	INSERT [UserManagement].[ProductPageControl] ([ProductPageControlId], [ProductPageId], [ControlId], [CreatedBy], [CreatedDate])
	VALUES (@ProductPageControlId, @ProductPageId, @ControlId, @UserId, @Now)

	SET IDENTITY_INSERT [UserManagement].[ProductPageControl] OFF
END

--MarketAnalytics
SET @ProductId  = 66
IF NOT EXISTS (SELECT TOP 1 1 FROM[UserManagement].[ProductPage] WHERE ProductId = @ProductId and ProductPageTypeId = 3)
BEGIN
	SET IDENTITY_INSERT [UserManagement].[Control] ON 
	Select @ControlId = MAX(controlid) +1 from [UserManagement].[Control]

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
	VALUES (@ControlId, NULL, 8, N'MarketAnalyticsRoleTemplateTabGroupUIId', NULL, NULL, 1, @UserId, @Now)

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
	VALUES (@ControlId+1, @ControlId, 9, N'MarketAnalyticsRoleTemplateRolesTabUIId', N'Roles', NULL, 1, @UserId, @Now)
	

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
	VALUES (@ControlId+2, @ControlId+1, 3, N'MarketAnalyticsRoleTemplateRolesSelectGridUIId', NULL, NULL, 1, @UserId, @Now)

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
	VALUES (@ControlId+3, @ControlId+2, 10, N'MarketAnalyticsRoleTemplateCheckboxUIId', NULL, N'isAssigned', 1, @UserId, @Now)	

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
	VALUES (@ControlId+4, @ControlId+2, 5, N'MarketAnalyticsRoleTemplateRoleLabelUIId', N'Role', N'name', 2, @UserId, @Now)
	
	SET IDENTITY_INSERT [UserManagement].[Control] OFF

	SET IDENTITY_INSERT [UserManagement].[ControlAttribute] ON 
	
	Select @ControlAttributeId = MAX(ControlAttributeId) +1 from [UserManagement].[ControlAttribute]

	INSERT [UserManagement].[ControlAttribute] ([ControlAttributeId], [ControlId], [Key], [Value], [CreatedBy], [CreatedDate])
	VALUES (@ControlAttributeId, @ControlId+1, N'Default', N'True', @UserId, @Now)

	INSERT [UserManagement].[ControlAttribute] ([ControlAttributeId], [ControlId], [Key], [Value], [CreatedBy], [CreatedDate])
	VALUES (@ControlAttributeId+1, @ControlId+1, N'Hide', N'False', @UserId, @Now)

	INSERT [UserManagement].[ControlAttribute] ([ControlAttributeId], [ControlId], [Key], [Value], [CreatedBy], [CreatedDate])
	VALUES (@ControlAttributeId+2, @ControlId+2, N'ShowSelectAll', N'True', @UserId, @Now)

	SET IDENTITY_INSERT [UserManagement].[ControlAttribute] OFF

	SET IDENTITY_INSERT [UserManagement].[ProductPage] ON 
	
	Select @ProductPageId = MAX(ProductPageId) +1 from [UserManagement].[ProductPage]

	INSERT [UserManagement].[ProductPage] ([ProductPageId], [ProductId], [DisplayName],[ProductPageTypeId], [CreatedBy], [CreatedDate], [IsActive])
	VALUES (@ProductPageId, @ProductId, N'Market Analytics Roles',@RoleTemplateProductPageTypeId, @UserId, @Now, 1)

	SET IDENTITY_INSERT [UserManagement].[ProductPage] OFF

	SET IDENTITY_INSERT [UserManagement].[ProductPageControl] ON 
	
	Select @ProductPageControlId = MAX(ProductPageControlId) +1 from [UserManagement].[ProductPageControl]

	INSERT [UserManagement].[ProductPageControl] ([ProductPageControlId], [ProductPageId], [ControlId], [CreatedBy], [CreatedDate])
	VALUES (@ProductPageControlId, @ProductPageId, @ControlId, @UserId, @Now)

	SET IDENTITY_INSERT [UserManagement].[ProductPageControl] OFF
END

--PerformanceAnalytics
SET @ProductId  = 30
IF NOT EXISTS (SELECT TOP 1 1 FROM[UserManagement].[ProductPage] WHERE ProductId = @ProductId and ProductPageTypeId = 3)
BEGIN
	SET IDENTITY_INSERT [UserManagement].[Control] ON 
	Select @ControlId = MAX(controlid) +1 from [UserManagement].[Control]

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
	VALUES (@ControlId, NULL, 8, N'PerformanceAnalyticsRoleTemplateTabGroupUIId', NULL, NULL, 1, @UserId, @Now)

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
	VALUES (@ControlId+1, @ControlId, 9, N'PerformanceAnalyticsRoleTemplateRolesTabUIId', N'Roles', NULL, 1, @UserId, @Now)
	

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
	VALUES (@ControlId+2, @ControlId+1, 3, N'PerformanceAnalyticsRoleTemplateRolesSelectGridUIId', NULL, NULL, 1, @UserId, @Now)

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
	VALUES (@ControlId+3, @ControlId+2, 10, N'PerformanceAnalyticsRoleTemplateCheckboxUIId', NULL, N'isAssigned', 1, @UserId, @Now)	

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
	VALUES (@ControlId+4, @ControlId+2, 5, N'PerformanceAnalyticsRoleTemplateRoleLabelUIId', N'Role', N'name', 2, @UserId, @Now)
	
	SET IDENTITY_INSERT [UserManagement].[Control] OFF

	SET IDENTITY_INSERT [UserManagement].[ControlAttribute] ON 
	
	Select @ControlAttributeId = MAX(ControlAttributeId) +1 from [UserManagement].[ControlAttribute]

	INSERT [UserManagement].[ControlAttribute] ([ControlAttributeId], [ControlId], [Key], [Value], [CreatedBy], [CreatedDate])
	VALUES (@ControlAttributeId, @ControlId+1, N'Default', N'True', @UserId, @Now)

	INSERT [UserManagement].[ControlAttribute] ([ControlAttributeId], [ControlId], [Key], [Value], [CreatedBy], [CreatedDate])
	VALUES (@ControlAttributeId+1, @ControlId+1, N'Hide', N'False', @UserId, @Now)

	INSERT [UserManagement].[ControlAttribute] ([ControlAttributeId], [ControlId], [Key], [Value], [CreatedBy], [CreatedDate])
	VALUES (@ControlAttributeId+2, @ControlId+2, N'ShowSelectAll', N'True', @UserId, @Now)

	SET IDENTITY_INSERT [UserManagement].[ControlAttribute] OFF

	SET IDENTITY_INSERT [UserManagement].[ProductPage] ON 
	
	Select @ProductPageId = MAX(ProductPageId) +1 from [UserManagement].[ProductPage]

	INSERT [UserManagement].[ProductPage] ([ProductPageId], [ProductId], [DisplayName],[ProductPageTypeId], [CreatedBy], [CreatedDate], [IsActive])
	VALUES (@ProductPageId, @ProductId, N'Performance Analytics Roles',@RoleTemplateProductPageTypeId, @UserId, @Now, 1)

	SET IDENTITY_INSERT [UserManagement].[ProductPage] OFF

	SET IDENTITY_INSERT [UserManagement].[ProductPageControl] ON 
	
	Select @ProductPageControlId = MAX(ProductPageControlId) +1 from [UserManagement].[ProductPageControl]

	INSERT [UserManagement].[ProductPageControl] ([ProductPageControlId], [ProductPageId], [ControlId], [CreatedBy], [CreatedDate])
	VALUES (@ProductPageControlId, @ProductPageId, @ControlId, @UserId, @Now)

	SET IDENTITY_INSERT [UserManagement].[ProductPageControl] OFF
END

--ClientPortal
SET @ProductId  = 14
IF NOT EXISTS (SELECT TOP 1 1 FROM[UserManagement].[ProductPage] WHERE ProductId = @ProductId and ProductPageTypeId = 3)
BEGIN
	SET IDENTITY_INSERT [UserManagement].[Control] ON 
	Select @ControlId = MAX(controlid) +1 from [UserManagement].[Control]

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
	VALUES (@ControlId, NULL, 8, N'ClientPortalRoleTemplateTabGroupUIId', NULL, NULL, 1, @UserId, @Now)

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
	VALUES (@ControlId+1, @ControlId, 9, N'ClientPortalRoleTemplateRolesTabUIId', N'Roles', NULL, 1, @UserId, @Now)
	

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
	VALUES (@ControlId+2, @ControlId+1, 3, N'ClientPortalRoleTemplateRolesSelectGridUIId', NULL, NULL, 1, @UserId, @Now)

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
	VALUES (@ControlId+3, @ControlId+2, 10, N'ClientPortalRoleTemplateCheckboxUIId', NULL, N'isAssigned', 1, @UserId, @Now)	

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
	VALUES (@ControlId+4, @ControlId+2, 5, N'ClientPortalRoleTemplateRoleLabelUIId', N'Role', N'name', 2, @UserId, @Now)	

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
	VALUES (@ControlId+5, @ControlId+2, 5, N'ClientPortalRoleTemplateRoleTypeLabelUIId', N'Role Type', N'roletype', 3, @UserId, @Now)
	
	SET IDENTITY_INSERT [UserManagement].[Control] OFF

	SET IDENTITY_INSERT [UserManagement].[ControlAttribute] ON 
	
	Select @ControlAttributeId = MAX(ControlAttributeId) +1 from [UserManagement].[ControlAttribute]

	INSERT [UserManagement].[ControlAttribute] ([ControlAttributeId], [ControlId], [Key], [Value], [CreatedBy], [CreatedDate])
	VALUES (@ControlAttributeId, @ControlId+1, N'Default', N'True', @UserId, @Now)

	INSERT [UserManagement].[ControlAttribute] ([ControlAttributeId], [ControlId], [Key], [Value], [CreatedBy], [CreatedDate])
	VALUES (@ControlAttributeId+1, @ControlId+1, N'Hide', N'False', @UserId, @Now)

	INSERT [UserManagement].[ControlAttribute] ([ControlAttributeId], [ControlId], [Key], [Value], [CreatedBy], [CreatedDate])
	VALUES (@ControlAttributeId+2, @ControlId+2, N'ShowSelectAll', N'True', @UserId, @Now)

	SET IDENTITY_INSERT [UserManagement].[ControlAttribute] OFF

	SET IDENTITY_INSERT [UserManagement].[ProductPage] ON 
	
	Select @ProductPageId = MAX(ProductPageId) +1 from [UserManagement].[ProductPage]

	INSERT [UserManagement].[ProductPage] ([ProductPageId], [ProductId], [DisplayName],[ProductPageTypeId], [CreatedBy], [CreatedDate], [IsActive])
	VALUES (@ProductPageId, @ProductId, N'Client Portal Roles',@RoleTemplateProductPageTypeId, @UserId, @Now, 1)

	SET IDENTITY_INSERT [UserManagement].[ProductPage] OFF

	SET IDENTITY_INSERT [UserManagement].[ProductPageControl] ON 
	
	Select @ProductPageControlId = MAX(ProductPageControlId) +1 from [UserManagement].[ProductPageControl]

	INSERT [UserManagement].[ProductPageControl] ([ProductPageControlId], [ProductPageId], [ControlId], [CreatedBy], [CreatedDate])
	VALUES (@ProductPageControlId, @ProductPageId, @ControlId, @UserId, @Now)

	SET IDENTITY_INSERT [UserManagement].[ProductPageControl] OFF
END

--VendorCredentialing
SET @ProductId  = 16
IF NOT EXISTS (SELECT TOP 1 1 FROM[UserManagement].[ProductPage] WHERE ProductId = @ProductId and ProductPageTypeId = 3)
BEGIN
	SET IDENTITY_INSERT [UserManagement].[Control] ON 
	Select @ControlId = MAX(controlid) +1 from [UserManagement].[Control]

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
	VALUES (@ControlId, NULL, 8, N'VendorCredentialingRoleTemplateTabGroupUIId', NULL, NULL, 1, @UserId, @Now)

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
	VALUES (@ControlId+1, @ControlId, 9, N'VendorCredentialingRoleTemplateAccessTypeTabUIId', N'Access Type', NULL, 1, @UserId, @Now)

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
	VALUES (@ControlId+2, @ControlId+1, 5, N'VendorCredentialingRoleTemplateAccessTypeRolesLabelUIId', N'Access Type', NULL, 1, @UserId, @Now)

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
	VALUES (@ControlId+3, @ControlId+1, 7, N'VendorCredentialingRoleTemplateSpecificPropertyRolesRadioUIId', N'Specific Property', N'property', 2, @UserId, @Now)

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
	VALUES (@ControlId+4, @ControlId+1, 7, N'VendorCredentialingRoleTemplatePropertyGroupRolesRadioUIId', N'Property Group', N'propertyGroup', 3, @UserId, @Now)

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
	VALUES (@ControlId+5, @ControlId+1, 7, N'VendorCredentialingRoleTemplateAllPropertiesRolesRadioUIId', N'All Properties', N'allProperties', 4, @UserId, @Now)


	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
	VALUES (@ControlId+6, @ControlId, 9, N'VendorCredentialingRoleTemplateRolesTabUIId', N'Roles', NULL, 2, @UserId, @Now)
	

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
	VALUES (@ControlId+7, @ControlId+6, 3, N'VendorCredentialingRoleTemplateRolesSelectGridUIId', NULL, NULL, 1, @UserId, @Now)

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
	VALUES (@ControlId+8, @ControlId+7, 10, N'VendorCredentialingRoleTemplateCheckboxUIId', NULL, N'isAssigned', 1, @UserId, @Now)	

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
	VALUES (@ControlId+9, @ControlId+7, 5, N'VendorCredentialingRoleTemplateRoleLabelUIId', N'Role', N'name', 2, @UserId, @Now)	

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
	VALUES (@ControlId+10, @ControlId+7, 5, N'VendorCredentialingRoleTemplateDescriptionLabelUIId', N'Description', N'description', 3, @UserId, @Now)	

	
	SET IDENTITY_INSERT [UserManagement].[Control] OFF

	SET IDENTITY_INSERT [UserManagement].[ControlAttribute] ON 
	
	Select @ControlAttributeId = MAX(ControlAttributeId) +1 from [UserManagement].[ControlAttribute]

	INSERT [UserManagement].[ControlAttribute] ([ControlAttributeId], [ControlId], [Key], [Value], [CreatedBy], [CreatedDate])
	VALUES (@ControlAttributeId, @ControlId+1, N'Default', N'True', @UserId, @Now)

	INSERT [UserManagement].[ControlAttribute] ([ControlAttributeId], [ControlId], [Key], [Value], [CreatedBy], [CreatedDate])
	VALUES (@ControlAttributeId+1, @ControlId+1, N'Hide', N'False', @UserId, @Now)

	INSERT [UserManagement].[ControlAttribute] ([ControlAttributeId], [ControlId], [Key], [Value], [CreatedBy], [CreatedDate])
	VALUES (@ControlAttributeId+2, @ControlId+6, N'Default', N'True', @UserId, @Now)

	INSERT [UserManagement].[ControlAttribute] ([ControlAttributeId], [ControlId], [Key], [Value], [CreatedBy], [CreatedDate])
	VALUES (@ControlAttributeId+3, @ControlId+6, N'Hide', N'False', @UserId, @Now)	

	INSERT [UserManagement].[ControlAttribute] ([ControlAttributeId], [ControlId], [Key], [Value], [CreatedBy], [CreatedDate])
	VALUES (@ControlAttributeId+4, @ControlId+7, N'ShowSelectAll', N'True', @UserId, @Now)

	SET IDENTITY_INSERT [UserManagement].[ControlAttribute] OFF

	SET IDENTITY_INSERT [UserManagement].[ProductPage] ON 
	
	Select @ProductPageId = MAX(ProductPageId) +1 from [UserManagement].[ProductPage]

	INSERT [UserManagement].[ProductPage] ([ProductPageId], [ProductId], [DisplayName],[ProductPageTypeId], [CreatedBy], [CreatedDate], [IsActive])
	VALUES (@ProductPageId, @ProductId, N'Vendor Credentialing Roles',@RoleTemplateProductPageTypeId, @UserId, @Now, 1)

	SET IDENTITY_INSERT [UserManagement].[ProductPage] OFF

	SET IDENTITY_INSERT [UserManagement].[ProductPageControl] ON 
	
	Select @ProductPageControlId = MAX(ProductPageControlId) +1 from [UserManagement].[ProductPageControl]

	INSERT [UserManagement].[ProductPageControl] ([ProductPageControlId], [ProductPageId], [ControlId], [CreatedBy], [CreatedDate])
	VALUES (@ProductPageControlId, @ProductPageId, @ControlId, @UserId, @Now)

	SET IDENTITY_INSERT [UserManagement].[ProductPageControl] OFF
END

--UtilityManagement
SET @ProductId  = 18
IF NOT EXISTS (SELECT TOP 1 1 FROM[UserManagement].[ProductPage] WHERE ProductId = @ProductId and ProductPageTypeId = 3)
BEGIN
	SET IDENTITY_INSERT [UserManagement].[Control] ON 
	Select @ControlId = MAX(controlid) +1 from [UserManagement].[Control]

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
	VALUES (@ControlId, NULL, 8, N'UtilityManagementRoleTemplateTabGroupUIId', NULL, NULL, 1, @UserId, @Now)

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
	VALUES (@ControlId+1, @ControlId, 9, N'UtilityManagementRoleTemplateAccessTypeTabUIId', N'Access Type', NULL, 1, @UserId, @Now)

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
	VALUES (@ControlId+2, @ControlId+1, 2, N'UtilityManagementRoleTemplateAccessTypeSelectGridUIId', NULL, NULL, 1, @UserId, @Now)

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
	VALUES (@ControlId+3, @ControlId+2, 7, N'UtilityManagementRoleTemplateAccessTypeRadioUIId', NULL, N'isAssigned', 1, @UserId, @Now)	

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
	VALUES (@ControlId+4, @ControlId+2, 5, N'UtilityManagementRoleTemplateAccessTypeLabelUIId', N'Access Type', N'name', 2, @UserId, @Now)
	
	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
	VALUES (@ControlId+5, @ControlId, 9, N'UtilityManagementRoleTemplateRolesTabUIId', N'Roles', NULL, 2, @UserId, @Now)

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
	VALUES (@ControlId+6, @ControlId+5, 3, N'UtilityManagementRoleTemplateRolesSelectGridUIId', NULL, NULL, 1, @UserId, @Now)	

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
	VALUES (@ControlId+7, @ControlId+6, 10, N'UtilityManagementRoleTemplateRolesSelectGridCheckboxUIId', NULL, N'isAssigned', 1, @UserId, @Now)	

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
	VALUES (@ControlId+8, @ControlId+6, 5, N'UtilityManagementRoleTemplateRolesSelectGridRoleLabelUIId', N'Role', N'roleName', 2, @UserId, @Now)	

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
	VALUES (@ControlId+9, @ControlId+6, 5, N'UtilityManagementRoleTemplateRolesSelectGridDescriptionLabelUIId', N'Description', N'roleDescription', 3, @UserId, @Now)	
	
	
	SET IDENTITY_INSERT [UserManagement].[Control] OFF

	SET IDENTITY_INSERT [UserManagement].[ControlAttribute] ON 
	
	Select @ControlAttributeId = MAX(ControlAttributeId) +1 from [UserManagement].[ControlAttribute]

	INSERT [UserManagement].[ControlAttribute] ([ControlAttributeId], [ControlId], [Key], [Value], [CreatedBy], [CreatedDate])
	VALUES (@ControlAttributeId, @ControlId+1, N'Default', N'True', @UserId, @Now)

	INSERT [UserManagement].[ControlAttribute] ([ControlAttributeId], [ControlId], [Key], [Value], [CreatedBy], [CreatedDate])
	VALUES (@ControlAttributeId+1, @ControlId+1, N'Hide', N'False', @UserId, @Now)

	INSERT [UserManagement].[ControlAttribute] ([ControlAttributeId], [ControlId], [Key], [Value], [CreatedBy], [CreatedDate])
	VALUES (@ControlAttributeId+2, @ControlId+5, N'Default', N'True', @UserId, @Now)

	INSERT [UserManagement].[ControlAttribute] ([ControlAttributeId], [ControlId], [Key], [Value], [CreatedBy], [CreatedDate])
	VALUES (@ControlAttributeId+3, @ControlId+5, N'Hide', N'False', @UserId, @Now)

	INSERT [UserManagement].[ControlAttribute] ([ControlAttributeId], [ControlId], [Key], [Value], [CreatedBy], [CreatedDate])
	VALUES (@ControlAttributeId+4, @ControlId+2, N'ShowSelectAll', N'False', @UserId, @Now)

	INSERT [UserManagement].[ControlAttribute] ([ControlAttributeId], [ControlId], [Key], [Value], [CreatedBy], [CreatedDate])
	VALUES (@ControlAttributeId+5, @ControlId+6, N'ShowSelectAll', N'True', @UserId, @Now)

	SET IDENTITY_INSERT [UserManagement].[ControlAttribute] OFF

	SET IDENTITY_INSERT [UserManagement].[ProductPage] ON 
	
	Select @ProductPageId = MAX(ProductPageId) +1 from [UserManagement].[ProductPage]

	INSERT [UserManagement].[ProductPage] ([ProductPageId], [ProductId], [DisplayName],[ProductPageTypeId], [CreatedBy], [CreatedDate], [IsActive])
	VALUES (@ProductPageId, @ProductId, N'Utility Management Roles',@RoleTemplateProductPageTypeId, @UserId, @Now, 1)

	SET IDENTITY_INSERT [UserManagement].[ProductPage] OFF

	SET IDENTITY_INSERT [UserManagement].[ProductPageControl] ON 
	
	Select @ProductPageControlId = MAX(ProductPageControlId) +1 from [UserManagement].[ProductPageControl]

	INSERT [UserManagement].[ProductPageControl] ([ProductPageControlId], [ProductPageId], [ControlId], [CreatedBy], [CreatedDate])
	VALUES (@ProductPageControlId, @ProductPageId, @ControlId, @UserId, @Now)

	SET IDENTITY_INSERT [UserManagement].[ProductPageControl] OFF
END

--Portfolio Management
SET @ProductId  = 44
IF NOT EXISTS (SELECT TOP 1 1 FROM[UserManagement].[ProductPage] WHERE ProductId = @ProductId and ProductPageTypeId = 3)
BEGIN
	SET IDENTITY_INSERT [UserManagement].[Control] ON 
	Select @ControlId = MAX(controlid) +1 from [UserManagement].[Control]

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
	VALUES (@ControlId, NULL, 8, N'PortfolioManagementRoleTemplateTabGroupUIId', NULL, NULL, 1, @UserId, @Now)

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
	VALUES (@ControlId+1, @ControlId, 9, N'PortfolioManagementRoleTemplateEntityRolesTabUIId', N'Entity Roles', NULL, 1, @UserId, @Now)

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
	VALUES (@ControlId+2, @ControlId+1, 3, N'PortfolioManagementRoleTemplateEntityRolesMultiSelectGridUIId', NULL, NULL, 1, @UserId, @Now)

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
	VALUES (@ControlId+3, @ControlId+2, 10, N'PortfolioManagementRoleTemplateCheckboxUIId', NULL, N'isAssigned', 1, @UserId, @Now)	

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
	VALUES (@ControlId+4, @ControlId+2, 5, N'PortfolioManagementRoleTemplateRoleLabelUIId', N'Role', N'name', 2, @UserId, @Now)
	
	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
	VALUES (@ControlId+5, @ControlId, 9, N'PortfolioManagementRoleTemplateGlobalRolesTabUIId', N'Global Roles', NULL, 2, @UserId, @Now)

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
	VALUES (@ControlId+6, @ControlId+5, 3, N'PortfolioManagementRoleTemplateGlobalRolesMultiSelectGridUIId', NULL, NULL, 1, @UserId, @Now)	

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
	VALUES (@ControlId+7, @ControlId+6, 10, N'PortfolioManagementRoleTemplateCheckboxUIId', NULL, N'isAssigned', 1, @UserId, @Now)	

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
	VALUES (@ControlId+8, @ControlId+6, 5, N'PortfolioManagementRoleTemplateRoleLabelUIId', N'Role', N'name', 2, @UserId, @Now)	

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
	VALUES (@ControlId+9, @ControlId+6, 5, N'PortfolioManagementRoleTemplateRoleTypeLabelUIId', N'Role Type', N'roleType', 3, @UserId, @Now)	
	
	
	SET IDENTITY_INSERT [UserManagement].[Control] OFF

	SET IDENTITY_INSERT [UserManagement].[ControlAttribute] ON 
	
	Select @ControlAttributeId = MAX(ControlAttributeId) +1 from [UserManagement].[ControlAttribute]

	INSERT [UserManagement].[ControlAttribute] ([ControlAttributeId], [ControlId], [Key], [Value], [CreatedBy], [CreatedDate])
	VALUES (@ControlAttributeId, @ControlId+1, N'Default', N'True', @UserId, @Now)

	INSERT [UserManagement].[ControlAttribute] ([ControlAttributeId], [ControlId], [Key], [Value], [CreatedBy], [CreatedDate])
	VALUES (@ControlAttributeId+1, @ControlId+1, N'Hide', N'False', @UserId, @Now)

	INSERT [UserManagement].[ControlAttribute] ([ControlAttributeId], [ControlId], [Key], [Value], [CreatedBy], [CreatedDate])
	VALUES (@ControlAttributeId+2, @ControlId+5, N'Default', N'True', @UserId, @Now)

	INSERT [UserManagement].[ControlAttribute] ([ControlAttributeId], [ControlId], [Key], [Value], [CreatedBy], [CreatedDate])
	VALUES (@ControlAttributeId+3, @ControlId+5, N'Hide', N'False', @UserId, @Now)

	INSERT [UserManagement].[ControlAttribute] ([ControlAttributeId], [ControlId], [Key], [Value], [CreatedBy], [CreatedDate])
	VALUES (@ControlAttributeId+4, @ControlId+2, N'ShowSelectAll', N'True', @UserId, @Now)

	INSERT [UserManagement].[ControlAttribute] ([ControlAttributeId], [ControlId], [Key], [Value], [CreatedBy], [CreatedDate])
	VALUES (@ControlAttributeId+5, @ControlId+6, N'ShowSelectAll', N'True', @UserId, @Now)

	SET IDENTITY_INSERT [UserManagement].[ControlAttribute] OFF

	SET IDENTITY_INSERT [UserManagement].[ProductPage] ON 
	
	Select @ProductPageId = MAX(ProductPageId) +1 from [UserManagement].[ProductPage]

	INSERT [UserManagement].[ProductPage] ([ProductPageId], [ProductId], [DisplayName],[ProductPageTypeId], [CreatedBy], [CreatedDate], [IsActive])
	VALUES (@ProductPageId, @ProductId, N'Portfolio Management Roles',@RoleTemplateProductPageTypeId, @UserId, @Now, 1)

	SET IDENTITY_INSERT [UserManagement].[ProductPage] OFF

	SET IDENTITY_INSERT [UserManagement].[ProductPageControl] ON 
	
	Select @ProductPageControlId = MAX(ProductPageControlId) +1 from [UserManagement].[ProductPageControl]

	INSERT [UserManagement].[ProductPageControl] ([ProductPageControlId], [ProductPageId], [ControlId], [CreatedBy], [CreatedDate])
	VALUES (@ProductPageControlId, @ProductPageId, @ControlId, @UserId, @Now)

	SET IDENTITY_INSERT [UserManagement].[ProductPageControl] OFF
END

--END : script for userstory #804806

GO

-- Update default unsupported products for UsePrimaryProperties

BEGIN TRAN;

UPDATE ps SET [Value] = '-1'
FROM Enterprise.GlobalProductConfiguration gpc
	JOIN Enterprise.Product P ON gpc.ProductId = P.ProductId
	JOIN Enterprise.ProductConfiguration pc ON pc.ConfigurationId = gpc.ConfigurationId
	JOIN Enterprise.ProductSetting ps ON ps.ProductSettingId = pc.ProductSettingId
	JOIN Enterprise.ProductSettingType pst ON pst.ProductSettingTypeId = ps.ProductSettingTypeId
WHERE pst.Name = 'UsePrimaryProperties'
	AND ((GETUTCDATE() BETWEEN gpc.FromDate AND gpc.ThruDate) OR (GETUTCDATE() >= gpc.FromDate AND gpc.ThruDate IS NULL))
	AND ((GETUTCDATE() BETWEEN pc.FromDate AND pc.ThruDate) OR (GETUTCDATE() >= pc.FromDate AND pc.ThruDate IS NULL))
	AND ((GETUTCDATE() BETWEEN ps.FromDate AND ps.ThruDate) OR (GETUTCDATE() >= ps.FromDate AND ps.ThruDate IS NULL))
	AND gpc.ProductId NOT IN (3, 4, 40, 41, 6, 9, 1, 23, 15, 17, 16, 56, 58, 57, 59, 65, 60);

DECLARE @NOW DATETIME = GETUTCDATE();
DECLARE @supportedProductList as table (productid int)
insert into @supportedProductList values
(3),(4),(40),(41),(6),(9),(1),(23),(15),(17),(16),(56),(58),(57),(59),(65),(60);

DECLARE @productlist as table (entid int identity, productid int, productsettingtype varchar(500), productsettingvalue varchar(2000))
insert into @productlist
SELECT pl.productid, 'UsePrimaryProperties', '0'
FROM @supportedProductList pl
WHERE NOT EXISTS (SELECT ps.*
	FROM Enterprise.GlobalProductConfiguration gpc
		JOIN Enterprise.Product P ON gpc.ProductId = P.ProductId
		JOIN Enterprise.ProductConfiguration pc ON pc.ConfigurationId = gpc.ConfigurationId
		JOIN Enterprise.ProductSetting ps ON ps.ProductSettingId = pc.ProductSettingId
		JOIN Enterprise.ProductSettingType pst ON pst.ProductSettingTypeId = ps.ProductSettingTypeId
	WHERE pst.Name = 'UsePrimaryProperties'
		AND ((GETUTCDATE() BETWEEN gpc.FromDate AND gpc.ThruDate) OR (GETUTCDATE() >= gpc.FromDate AND gpc.ThruDate IS NULL))
		AND ((GETUTCDATE() BETWEEN pc.FromDate AND pc.ThruDate) OR (GETUTCDATE() >= pc.FromDate AND pc.ThruDate IS NULL))
		AND ((GETUTCDATE() BETWEEN ps.FromDate AND ps.ThruDate) OR (GETUTCDATE() >= ps.FromDate AND ps.ThruDate IS NULL))
		AND gpc.ProductId IN (3, 4, 40, 41, 6, 9, 1, 23, 15, 17, 16, 56, 58, 57, 59, 65, 60)
		AND pl.productid = gpc.ProductId);

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
-- ADD ALLOW Clone users process For HOTS
IF NOT EXISTS (SELECT TOP(1) 1 FROM Enterprise.ProductSettingType WHERE [Name] = 'IsCloneUsersProcessEnabledForHOTS')
BEGIN
	INSERT INTO Enterprise.ProductSettingType ([Name], [Description], SensitiveData)
	VALUES ('IsCloneUsersProcessEnabledForHOTS', 'Does the environment support the Clone Users Process For HOTS', 0);
END

GO
-- HOTS Create user source type
DECLARE @statustypecategoryid INT
DECLARE @ident INT
DECLARE @statusTypeId INT

SELECT @statustypecategoryid = StatusTypeCategoryId
FROM Enterprise.StatusTypeCategory
WHERE name = 'Create User Source';

IF NOT EXISTS
(
    SELECT 1
    FROM Enterprise.StatusType
    WHERE name = 'HOTS'
)
    BEGIN
        INSERT INTO Enterprise.StatusType(name)
        VALUES('HOTS');
        SELECT @ident = @@IDENTITY;
        INSERT INTO Enterprise.StatusTypeCategoryClassification
        (StatusTypeId,
         StatusTypeCategoryId,
         FromDate,
         ThruDate
        )
        VALUES
        (@ident, -- StatusTypeId - int
         @statustypecategoryid, -- StatusTypeCategoryId - int
         GETDATE(), -- FromDate - datetime
         NULL  -- ThruDate - datetime
        );
END;
ELSE
BEGIN
    SELECT @statusTypeId = Enterprise.StatusType.StatusTypeId
    FROM Enterprise.StatusType
            INNER JOIN Enterprise.StatusTypeCategoryClassification ON Enterprise.StatusType.StatusTypeId = Enterprise.StatusTypeCategoryClassification.StatusTypeId
            INNER JOIN Enterprise.StatusTypeCategory ON Enterprise.StatusTypeCategoryClassification.StatusTypeCategoryId = Enterprise.StatusTypeCategory.StatusTypeCategoryId
            INNER JOIN Enterprise.StatusTypeCategoryType ON Enterprise.StatusTypeCategory.StatusTypeCategoryTypeId = Enterprise.StatusTypeCategoryType.StatusTypeCategoryTypeId
    WHERE Enterprise.StatusTypeCategoryType.name = 'Unified Platform'
            AND Enterprise.StatusType.Name = 'HOTS';
	IF NOT EXISTS
	(
		SELECT 1
		FROM Enterprise.StatusTypeCategoryClassification
		WHERE StatusTypeId = @statusTypeId
	)
	BEGIN
		INSERT INTO Enterprise.StatusTypeCategoryClassification
		(StatusTypeId,
			StatusTypeCategoryId,
			FromDate,
			ThruDate
		)
		VALUES
		(@StatusTypeId, -- StatusTypeId - int
			@statustypecategoryid, -- StatusTypeCategoryId - int
			GETDATE(), -- FromDate - datetime
			NULL  -- ThruDate - datetime
		);
	END
END;
GO
BEGIN TRAN

-- Add ProductIcon product settings

IF NOT EXISTS (SELECT TOP 1 1 FROM Enterprise.ProductSettingType WHERE [Name] = 'EnableProductForAdminUserEdit')
BEGIN
	INSERT INTO Enterprise.ProductSettingType ([Name], [Description], SensitiveData)
	VALUES ('EnableProductForAdminUserEdit', 'Enable Product in Product Access Panel For Admin User to Edit', 0);
END

DECLARE @NOW DATETIME = GETUTCDATE();
DECLARE @productlist as table (entid int identity, productid int, productsettingtype varchar(500), productsettingvalue varchar(2000))
insert into @productlist values
(1,  'EnableProductForAdminUserEdit', '0'),
(3,  'EnableProductForAdminUserEdit', '0'),
(4,  'EnableProductForAdminUserEdit', '0'),
(6,  'EnableProductForAdminUserEdit', '0'),
(8,  'EnableProductForAdminUserEdit', '1'),
(9,  'EnableProductForAdminUserEdit', '0'),
(10, 'EnableProductForAdminUserEdit', '0'),
(13, 'EnableProductForAdminUserEdit', '0'),
(14, 'EnableProductForAdminUserEdit', '0'),
(15, 'EnableProductForAdminUserEdit', '0'),
(16, 'EnableProductForAdminUserEdit', '0'),
(17, 'EnableProductForAdminUserEdit', '0'),
(18, 'EnableProductForAdminUserEdit', '0'),
(19, 'EnableProductForAdminUserEdit', '0'),
(20, 'EnableProductForAdminUserEdit', '0'),
(23, 'EnableProductForAdminUserEdit', '0'),
(26, 'EnableProductForAdminUserEdit', '0'),
(29, 'EnableProductForAdminUserEdit', '0'),
(30, 'EnableProductForAdminUserEdit', '0'),
(31, 'EnableProductForAdminUserEdit', '0'),
(32, 'EnableProductForAdminUserEdit', '0'),
(33, 'EnableProductForAdminUserEdit', '0'),
(38, 'EnableProductForAdminUserEdit', '0'),
(39, 'EnableProductForAdminUserEdit', '0'),
(40, 'EnableProductForAdminUserEdit', '0'),
(41, 'EnableProductForAdminUserEdit', '0'),
(44, 'EnableProductForAdminUserEdit', '0'),
(45, 'EnableProductForAdminUserEdit', '0'),
(47, 'EnableProductForAdminUserEdit', '0'),
(48, 'EnableProductForAdminUserEdit', '0'),
(49, 'EnableProductForAdminUserEdit', '0'),
(50, 'EnableProductForAdminUserEdit', '0'),
(51, 'EnableProductForAdminUserEdit', '0'),
(52, 'EnableProductForAdminUserEdit', '0'),
(53, 'EnableProductForAdminUserEdit', '0'),
(54, 'EnableProductForAdminUserEdit', '0'),
(56, 'EnableProductForAdminUserEdit', '0'),
(57, 'EnableProductForAdminUserEdit', '0'),
(58, 'EnableProductForAdminUserEdit', '0'),
(59, 'EnableProductForAdminUserEdit', '0'),
(60, 'EnableProductForAdminUserEdit', '0'),
(63, 'EnableProductForAdminUserEdit', '0');

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
Declare @controlId INT
Declare @accesstabcontrolId INT

select @controlId = ControlId from UserManagement.Control where UIID = 'VendorCredentialiProductAccessRolesTabUIId'
select @accesstabcontrolId = ControlId from UserManagement.Control where UIID = 'VendorCredentialiProductAccessTypeTabUIId'

IF EXISTS (select top 1 1 from UserManagement.ControlAttribute where ControlId = @controlId)
BEGIN
 update UserManagement.ControlAttribute set ControlId = @accesstabcontrolId where ControlId = @controlId
END

IF EXISTS (SELECT TOP 1 1 FROM UserManagement.Control where UIID = 'VendorCredentialiProductAccessTypeTabUIId' AND [Sequence] = 2)
BEGIN
 update UserManagement.Control set Sequence = 1  where UIID = 'VendorCredentialiProductAccessTypeTabUIId' AND [Sequence] = 2
END

IF EXISTS (SELECT TOP 1 1 FROM UserManagement.Control where UIID = 'VendorCredentialiProductAccessRolesTabUIId' AND [Sequence] = 1)
BEGIN
 update UserManagement.Control set Sequence = 4  where UIID = 'VendorCredentialiProductAccessRolesTabUIId' AND [Sequence] = 1
END

IF EXISTS (SELECT TOP 1 1 FROM UserManagement.Control where UIID = 'VendorCredentialiProductAccessPropertiesTabUIId' AND [Sequence] = 3)
BEGIN
 update UserManagement.Control set Sequence = 2  where UIID = 'VendorCredentialiProductAccessPropertiesTabUIId' AND [Sequence] = 3
END

IF EXISTS (SELECT TOP 1 1 FROM UserManagement.Control where UIID = 'VendorCredentialiProductAccessPropertyGroupTabUIId' AND [Sequence] = 4)
BEGIN
 update UserManagement.Control set Sequence = 3  where UIID = 'VendorCredentialiProductAccessPropertyGroupTabUIId' AND [Sequence] = 4
END

Go
--Allowing RegularUser(No Email) for ILM and ILMLA

Declare @NotificationRequiredfoRegularUserNoEmail INT;
Declare @ProductNotAvailableforRegularNoEMail INT;
Select @NotificationRequiredfoRegularUserNoEmail = ProductSettingTypeId from Enterprise.ProductSettingType where Name like 'NotificationEmailRequiredForUserWithNoEmail';
Select @ProductNotAvailableforRegularNoEMail = ProductSettingTypeId from Enterprise.ProductSettingType where Name like 'ProductNotAvailableForRegularUserNoEmail';
IF  EXISTS (SELECT TOP 1 1 FROM  Enterprise.ProductSetting where ProductSettingTypeId =@ProductNotAvailableforRegularNoEMail and ProductId in (40,41))
BEGIN
 UPDATE Enterprise.ProductSetting Set Value=0 where ProductSettingTypeId=@ProductNotAvailableforRegularNoEMail and ProductId in (40,41)
END
IF  EXISTS (SELECT TOP 1 1 FROM  Enterprise.ProductSetting where ProductSettingTypeId =@NotificationRequiredfoRegularUserNoEmail and ProductId in (40,41))
BEGIN
 UPDATE Enterprise.ProductSetting Set Value=0 where ProductSettingTypeId=@NotificationRequiredfoRegularUserNoEmail and ProductId in (40,41)
END

GO

 --Adding Product Setting Type for ILM LM & ILM LA
			   
BEGIN TRAN

-- Add ProductIcon product settings

IF NOT EXISTS (SELECT TOP 1 1 FROM Enterprise.ProductSettingType WHERE [Name] = 'ProductAcceptsUniqueProductUserName')
BEGIN
	INSERT INTO Enterprise.ProductSettingType ([Name], [Description], SensitiveData)
	VALUES ('ProductAcceptsUniqueProductUserName', 'Product Accepts Unique Product UserName', 0);
END

DECLARE @NOW DATETIME = GETUTCDATE();
declare @productlist table ( entid int identity, productid int, productsettingtype varchar(500), productsettingvalue varchar(100))
insert into @productlist values
(40,  'ProductAcceptsUniqueProductUserName','1'),
(41,  'ProductAcceptsUniqueProductUserName','1');

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

UPDATE Enterprise.OrganizationType SET ThruDate = NULL WHERE name IN ( 'AppPartner', 'Supplier' ) AND ThruDate IS NOT NULL
GO

---Script to add KongApiEndPoint configuration
DECLARE @LoginURL NVARCHAR(500) = 'https://internalapi-dev.realpage.com',
@ServerName SYSNAME = @@SERVERNAME
IF @ServerName IN ('RCDUSODBSQL001') --DEV
BEGIN
	SET @LoginURL = 'https://internalapi-dev.realpage.com';
END
IF @ServerName IN ('RCTUSODBSQL001') --QA
BEGIN
	SET @LoginURL = 'https://internalapi-qa.realpage.com';
END
IF @ServerName IN ('RCQUSODBSQL001') --SAT
BEGIN
	SET @LoginURL = 'https://internalapi-sat.realpage.com';
END
IF @ServerName IN ('RCTUSODBSQL001A','RCTUSODBSQL001B') --UAT
BEGIN
	SET @LoginURL = 'https://internalapi-uat.realpage.com';
END
IF @ServerName IN ('RCIUSODBSQL002') --PREPROD
BEGIN
	SET @LoginURL = 'https://internalapi-ppd.realpage.com';
END
IF @ServerName IN ('RCPGBKDBSQL005A', 'RCPGBKDBSQL005B') --PROD
BEGIN
	SET @LoginURL = 'https://internalapi.realpage.com';
END

IF NOT EXISTS ( select top (1) 1 from Enterprise.ProductSettingType where name = 'KongApiEndPoint')
BEGIN
	INSERT INTO Enterprise.ProductSettingType ( name, Description, SensitiveData ) values ( 'KongApiEndPoint', 'The api endpoint for Kong APIs', 0 )
END

IF NOT EXISTS(Select top (1) 1 from Enterprise.ProductSetting ps 
				inner join Enterprise.ProductSettingType pst
				on ps.ProductSettingTypeId = pst.ProductSettingTypeId
				where pst.Name = 'KongApiEndPoint' and ps.ProductId= 3)
BEGIN
	Insert into Enterprise.ProductSetting (ProductId, ProductSettingTypeId, Value, FromDate)
	Select 3, ProductSettingTypeId, @LoginURL, GETUTCDATE()
	from Enterprise.ProductSettingType
	where Name = 'KongApiEndPoint'

	declare @productsettingid int
	select @productsettingid = productsettingid from Enterprise.ProductSetting ps 
				inner join Enterprise.ProductSettingType pst
				on ps.ProductSettingTypeId = pst.ProductSettingTypeId
				where pst.Name = 'KongApiEndPoint' and ps.ProductId= 3

	insert into enterprise.ProductConfiguration ( ConfigurationId, ProductSettingId, FromDate )
				select TOP (1) ConfigurationId, @productsettingid, GETUTCDATE() from enterprise.GlobalProductConfiguration where productid = 3 and thrudate is NULL ORDER BY GlobalProductConfigurationId DESC
END
GO

---Script to add Kong-Vanity-url configuration
DECLARE @kongVanityURL NVARCHAR(500) = '',
@ServerName SYSNAME = @@SERVERNAME
IF @ServerName IN ('RCVGBKDBSQL001') --DEMO
BEGIN
	SET @kongVanityURL = '';
END
IF @ServerName IN ('RCTUSODBTUL001') --TRAINING
BEGIN
	SET @kongVanityURL = '';
END

IF NOT EXISTS ( select top (1) 1 from Enterprise.ProductSettingType where name = 'Kong-Vanity-url')
BEGIN
	INSERT INTO Enterprise.ProductSettingType ( name, Description, SensitiveData ) values ( 'Kong-Vanity-url', 'Kong Vanity url', 0 )
END

IF NOT EXISTS(Select top (1) 1 from Enterprise.ProductSetting ps 
				inner join Enterprise.ProductSettingType pst
				on ps.ProductSettingTypeId = pst.ProductSettingTypeId
				where pst.Name = 'Kong-Vanity-url' and ps.ProductId= 3)
BEGIN
	Insert into Enterprise.ProductSetting (ProductId, ProductSettingTypeId, Value, FromDate)
	Select 3, ProductSettingTypeId, @kongVanityURL, GETUTCDATE()
	from Enterprise.ProductSettingType
	where Name = 'Kong-Vanity-url'

	declare @productsettingid int
	select @productsettingid = productsettingid from Enterprise.ProductSetting ps 
				inner join Enterprise.ProductSettingType pst
				on ps.ProductSettingTypeId = pst.ProductSettingTypeId
				where pst.Name = 'Kong-Vanity-url' and ps.ProductId= 3

	insert into enterprise.ProductConfiguration ( ConfigurationId, ProductSettingId, FromDate )
				select TOP (1) ConfigurationId, @productsettingid, GETUTCDATE() from enterprise.GlobalProductConfiguration where productid = 3 and thrudate is NULL ORDER BY GlobalProductConfigurationId DESC
END
GO

---Script to add Kong Key configuration
DECLARE @kongKey NVARCHAR(500) = '3Bb8btMTLdJqEoVwbfX4H1NudH0GGgd1',
@ServerName SYSNAME = @@SERVERNAME
IF @ServerName IN ('RCDUSODBSQL001') --DEV
BEGIN
	SET @kongKey = '3Bb8btMTLdJqEoVwbfX4H1NudH0GGgd1';
END
IF @ServerName IN ('RCTUSODBSQL001') --QA
BEGIN
	SET @kongKey = 'OllyBPXnfpBW5dDioB6cWFUC7dB8xEeF';
END
IF @ServerName IN ('RCQUSODBSQL001') --SAT
BEGIN
	SET @kongKey = 'gCUWs2pcJ0frVzjHdMQce2PdWAKR03C5';
END
IF @ServerName IN ('RCTUSODBSQL001A','RCTUSODBSQL001B') --UAT
BEGIN
	SET @kongKey = 'JNss4MQgawAUUc9HIunfZKWbYgq1c7a8';
END
IF @ServerName IN ('RCIUSODBSQL002') --PREPROD
BEGIN
	SET @kongKey = '9KVoiVYybJaUdECJiAZ2fEtlSPON6FxZ';
END

IF NOT EXISTS ( select top (1) 1 from Enterprise.ProductSettingType where name = 'KONG_KEY')
BEGIN
	INSERT INTO Enterprise.ProductSettingType ( name, Description, SensitiveData ) values ( 'KONG_KEY', 'Kong key for Unifiedlogin', 0 )
END

IF NOT EXISTS(Select top (1) 1 from Enterprise.ProductSetting ps 
				inner join Enterprise.ProductSettingType pst
				on ps.ProductSettingTypeId = pst.ProductSettingTypeId
				where pst.Name = 'KONG_KEY' and ps.ProductId= 3)
BEGIN
	Insert into Enterprise.ProductSetting (ProductId, ProductSettingTypeId, Value, FromDate)
	Select 3, ProductSettingTypeId, @kongKey, GETUTCDATE()
	from Enterprise.ProductSettingType
	where Name = 'KONG_KEY'

	declare @productsettingid int
	select @productsettingid = productsettingid from Enterprise.ProductSetting ps 
				inner join Enterprise.ProductSettingType pst
				on ps.ProductSettingTypeId = pst.ProductSettingTypeId
				where pst.Name = 'KONG_KEY' and ps.ProductId= 3

	insert into enterprise.ProductConfiguration ( ConfigurationId, ProductSettingId, FromDate )
				select TOP (1) ConfigurationId, @productsettingid, GETUTCDATE() from enterprise.GlobalProductConfiguration where productid = 3 and thrudate is NULL ORDER BY GlobalProductConfigurationId DESC
END
GO

---Script to add international setting API
IF NOT EXISTS ( select top (1) 1 from Enterprise.ProductSettingType where name = 'CompanyInternationalSettingsAPI')
BEGIN
	INSERT INTO Enterprise.ProductSettingType ( name, Description, SensitiveData ) values ( 'CompanyInternationalSettingsAPI', 'API to get International settings from Unified setting', 0 )
END

IF NOT EXISTS(Select top (1) 1 from Enterprise.ProductSetting ps 
				inner join Enterprise.ProductSettingType pst
				on ps.ProductSettingTypeId = pst.ProductSettingTypeId
				where pst.Name = 'CompanyInternationalSettingsAPI' and ps.ProductId= 3)
BEGIN
	Insert into Enterprise.ProductSetting (ProductId, ProductSettingTypeId, Value, FromDate)
	Select 3, ProductSettingTypeId, '/settings/v1/{0}/companies/{1}?category={2}', GETUTCDATE()
	from Enterprise.ProductSettingType
	where Name = 'CompanyInternationalSettingsAPI'

	declare @productsettingid int
	select @productsettingid = productsettingid from Enterprise.ProductSetting ps 
				inner join Enterprise.ProductSettingType pst
				on ps.ProductSettingTypeId = pst.ProductSettingTypeId
				where pst.Name = 'CompanyInternationalSettingsAPI' and ps.ProductId= 3

	insert into enterprise.ProductConfiguration ( ConfigurationId, ProductSettingId, FromDate )
				select TOP (1) ConfigurationId, @productsettingid, GETUTCDATE() from enterprise.GlobalProductConfiguration where productid = 3 and thrudate is NULL ORDER BY GlobalProductConfigurationId DESC
END
GO

-- Navigation Menu seed data

SET IDENTITY_INSERT Enterprise.NavigationMenu ON;

MERGE INTO Enterprise.NavigationMenu t
	USING 
	(
		VALUES
			(1, N'Home', N'home', N'places-home-1', N'/', 10, NULL),

			(2, N'People', N'people', N'user', NULL, 20, NULL),
			(3, N'Users', N'users', NULL, N'/people/users', 30, 2),
			(4, N'User Activity Log', N'activity-log', NULL, N'/people/activity-log', 40, 2),

			(5, N'Reporting', N'reporting', N'file-new-2', N'/reporting', 50, NULL),

			(6, N'Roles & Rights', N'rolesRights', N'key-1', NULL, 60, NULL),
			(7, N'Roles & Rights', N'productsRolesRights', NULL, N'/roles-rights', 70, 6),
			(8, N'Role Templates', N'roleTemplates', NULL, N'/roles-rights/role-templates', 80, 6),

			(9, N'Configurations', N'Configurations', N'wrench-screwdriver', NULL, 90, NULL),
			(10, N'Company Setup', N'company-setup', NULL, N'/company-setup', 100, 9),
			(11, N'Company Setup Activity Log', N'company-setup-activity-log', NULL, N'/company-setup-activity-log', 110, 9),
			(12, N'Notifications', N'notifications', NULL, N'/notifications/configuration', 120, 9),
			(13, N'Products', N'products-configuration', NULL, N'/products-configuration', 130, 9),
			(14, N'Client Settings', N'client-settings', NULL, N'/client-settings', 140, 9),

			--(15, N'Platform Alerts', N'platformalerts', N'alarm-timeout', N'/notifications/platformalerts', 150, NULL),

			(16, N'Settings', N'settings', N'cog-gear-settings', NULL, 160, NULL),
			(17, N'Manage Settings', N'manage-settings', NULL, N'/settings', 170, 16),
			(18, N'Manage Templates', N'manage-templates', NULL, N'/settings/templates', 180, 16),
			(19, N'Settings Activity Log', N'settings-activity', NULL, N'/settings/activity-log', 190, 16)
	) 
	AS 
	s (Id, Title, PageId, Icon, [URL], OrderIndex, ParentId) on t.Id = s.Id
	WHEN MATCHED THEN
		UPDATE SET Title = s.Title,
			PageId = s.PageId,
			Icon = s.Icon,
			[URL] = s.[URL],
			OrderIndex = s.OrderIndex,
			ParentId = s.ParentId
	WHEN NOT MATCHED BY TARGET THEN
		INSERT(Id, Title, PageId, Icon, [URL], OrderIndex, ParentId) VALUES (s.Id, s.Title, s.PageId, s.Icon, s.[URL], s.OrderIndex, s.ParentId)
	WHEN NOT MATCHED BY SOURCE THEN
		DELETE
;

SET IDENTITY_INSERT Enterprise.NavigationMenu OFF;
DECLARE @maxId int = (SELECT MAX(Id) FROM Enterprise.NavigationMenu);
DBCC CHECKIDENT ('Enterprise.NavigationMenu', RESEED, @maxId);

MERGE INTO Enterprise.NavigationMenuRights t
	USING 
	(
		SELECT 1 NavigationMenuId, RightId FROM [Security].[Right] WHERE RightName = 'Dashboard' UNION ALL
		SELECT 2 NavigationMenuId, RightId FROM [Security].[Right] WHERE RightName = 'ViewUsers' UNION ALL
		SELECT 3 NavigationMenuId, RightId FROM [Security].[Right] WHERE RightName = 'ViewUsers' UNION ALL
		SELECT 4 NavigationMenuId, RightId FROM [Security].[Right] WHERE RightName = 'ViewAuditTrailUserData' UNION ALL
		SELECT 5 NavigationMenuId, RightId FROM [Security].[Right] WHERE RightName = 'AccessUnifiedReporting' UNION ALL
		SELECT 5 NavigationMenuId, RightId FROM [Security].[Right] WHERE RightName = 'EmployeeAccessUnifiedReportingAdminConsole' UNION ALL
		SELECT 6 NavigationMenuId, RightId FROM [Security].[Right] WHERE RightName = 'ViewRoleRight' UNION ALL
		SELECT 9 NavigationMenuId, RightId FROM [Security].[Right] WHERE RightName = 'ManageNotifications' UNION ALL
		SELECT 9 NavigationMenuId, RightId FROM [Security].[Right] WHERE RightName = 'EmployeeAccessToCompanySetup' UNION ALL
		SELECT 9 NavigationMenuId, RightId FROM [Security].[Right] WHERE RightName = 'AbilityToAddProducts' UNION ALL
		SELECT 10 NavigationMenuId, RightId FROM [Security].[Right] WHERE RightName = 'EmployeeAccessToCompanySetup' UNION ALL
		SELECT 11 NavigationMenuId, RightId FROM [Security].[Right] WHERE RightName = 'EmployeeAccessToCompanySetup' UNION ALL
		SELECT 12 NavigationMenuId, RightId FROM [Security].[Right] WHERE RightName = 'ManageNotifications' UNION ALL
		SELECT 13 NavigationMenuId, RightId FROM [Security].[Right] WHERE RightName = 'AbilityToAddProducts' UNION ALL
		SELECT 14 NavigationMenuId, RightId FROM [Security].[Right] WHERE RightName = 'EmployeeAccessToCompanySetup' UNION ALL
		SELECT 16 NavigationMenuId, RightId FROM [Security].[Right] WHERE RightName = 'Settings' UNION ALL
		SELECT 16 NavigationMenuId, RightId FROM [Security].[Right] WHERE RightName = 'ViewUnifiedSettings' UNION ALL
		SELECT 18 NavigationMenuId, RightId FROM [Security].[Right] WHERE RightName = 'ManageSettingsTemplates'
	) 
	AS 
	s (NavigationMenuId, RightId) on t.NavigationMenuId = s.NavigationMenuId
		AND t.RightId = s.RightId
	WHEN NOT MATCHED BY TARGET THEN
		INSERT(NavigationMenuId, RightId) VALUES (s.NavigationMenuId, s.RightId)
	WHEN NOT MATCHED BY SOURCE THEN
		DELETE
;

GO