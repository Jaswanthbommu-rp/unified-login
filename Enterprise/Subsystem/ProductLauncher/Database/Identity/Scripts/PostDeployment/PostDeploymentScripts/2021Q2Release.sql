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
GO
-- Move Password policy settings data in to new organization settings table
DECLARE @UserId bigint,
	@ProductId int ,
	@SettingCategoryTypeId smallint,
	@Now datetime = GETDATE()

SELECT	@UserId = UserId
FROM	Ident.UserLogin
WHERE	LoginName LIKE 'realpagead@%'

Select @SettingCategoryTypeId = SettingCategoryTypeId
From [Ident].[SettingCategoryType]
Where Name = 'Security'

-- 'NumberOfPasswordsToRemember'

	INSERT INTO [Ident].[OrganizationSettings] (PartyId,SettingCategoryTypeId,MappingName,
		MappingValue,Editable,[Hidden],CreatedBy,CreatedDate)
	Select PartyId,@SettingCategoryTypeId,'NumberOfPasswordsToRemember',
		NumberOfPasswordsToRemember,1,0,@UserId,@Now
	From [Ident].[PasswordPolicy]  
	where PartyId  not in (select PartyId From Ident.OrganizationSettings where   MappingName = 'NumberOfPasswordsToRemember')

--'PreventPasswordReuse'

	INSERT INTO [Ident].[OrganizationSettings] (PartyId,SettingCategoryTypeId,MappingName,
		MappingValue,Editable,[Hidden],CreatedBy,CreatedDate)
	Select PartyId,@SettingCategoryTypeId,'PreventPasswordReuse',
		PreventPasswordReuse,1,1,@UserId,@Now
	From [Ident].[PasswordPolicy]  
	where PartyId  not in (select PartyId From Ident.OrganizationSettings where   MappingName = 'PreventPasswordReuse')


--'PasswordExpirationPeriodInDays'

	INSERT INTO [Ident].[OrganizationSettings] (PartyId,SettingCategoryTypeId,MappingName,
		MappingValue,Editable,[Hidden],CreatedBy,CreatedDate)
	Select PartyId,@SettingCategoryTypeId,'PasswordExpirationPeriodInDays',
		PasswordExpirationPeriodInDays,1,0,@UserId,@Now
	From [Ident].[PasswordPolicy]  
	where PartyId  not in (select PartyId From Ident.OrganizationSettings where   MappingName = 'PasswordExpirationPeriodInDays')


-- 'EnablePasswordExpiration'

	INSERT INTO [Ident].[OrganizationSettings] (PartyId,SettingCategoryTypeId,MappingName,
		MappingValue,Editable,[Hidden],CreatedBy,CreatedDate)
	Select PartyId,@SettingCategoryTypeId,'EnablePasswordExpiration',
		EnablePasswordExpiration,1,1,@UserId,@Now
	From [Ident].[PasswordPolicy]  
	where PartyId  not in (select PartyId From Ident.OrganizationSettings where   MappingName = 'EnablePasswordExpiration')


--'AllowUsersToChangeOwnPassword'

	INSERT INTO [Ident].[OrganizationSettings] (PartyId,SettingCategoryTypeId,MappingName,
		MappingValue,Editable,[Hidden],CreatedBy,CreatedDate)
	Select PartyId,@SettingCategoryTypeId,'AllowUsersToChangeOwnPassword',
		AllowUsersToChangeOwnPassword,1,1,@UserId,@Now
	From [Ident].[PasswordPolicy]  
	where PartyId  not in (select PartyId From Ident.OrganizationSettings where   MappingName = 'AllowUsersToChangeOwnPassword')


--'MinimumSpecialCharacter'

	INSERT INTO [Ident].[OrganizationSettings] (PartyId,SettingCategoryTypeId,MappingName,
		MappingValue,Editable,[Hidden],CreatedBy,CreatedDate)
	Select PartyId,@SettingCategoryTypeId,'MinimumSpecialCharacter',
		MinimumSpecialCharacter,1,1,@UserId,@Now
	From [Ident].[PasswordPolicy]
	where PartyId  not in (select PartyId From Ident.OrganizationSettings where   MappingName = 'MinimumSpecialCharacter')


--'MinimumNumeric'

	INSERT INTO [Ident].[OrganizationSettings] (PartyId,SettingCategoryTypeId,MappingName,
		MappingValue,Editable,[Hidden],CreatedBy,CreatedDate)
	Select PartyId,@SettingCategoryTypeId,'MinimumNumeric',
		MinimumNumeric,1,1,@UserId,@Now
	From [Ident].[PasswordPolicy]  
	where PartyId  not in (select PartyId From Ident.OrganizationSettings where   MappingName = 'MinimumNumeric')


--'MinimumUppercase'

	INSERT INTO [Ident].[OrganizationSettings] (PartyId,SettingCategoryTypeId,MappingName,
		MappingValue,Editable,[Hidden],CreatedBy,CreatedDate)
	Select PartyId,@SettingCategoryTypeId,'MinimumUppercase',
		MinimumUppercase,1,1,@UserId,@Now
	From [Ident].[PasswordPolicy]  
	where PartyId  not in (select PartyId From Ident.OrganizationSettings where   MappingName = 'MinimumUppercase')


-- 'MinimumLowercase'

	INSERT INTO [Ident].[OrganizationSettings] (PartyId,SettingCategoryTypeId,MappingName,
		MappingValue,Editable,[Hidden],CreatedBy,CreatedDate)
	Select PartyId,@SettingCategoryTypeId,'MinimumLowercase',
		MinimumLowercase,1,1,@UserId,@Now
	From [Ident].[PasswordPolicy]  
	where PartyId  not in (select PartyId From Ident.OrganizationSettings where   MappingName = 'MinimumLowercase')


--'MaximumLength'

	INSERT INTO [Ident].[OrganizationSettings] (PartyId,SettingCategoryTypeId,MappingName,
		MappingValue,Editable,[Hidden],CreatedBy,CreatedDate)
	Select PartyId,@SettingCategoryTypeId,'MaximumLength',
		MaximumLength,1,1,@UserId,@Now
	From [Ident].[PasswordPolicy]  
	where PartyId  not in (select PartyId From Ident.OrganizationSettings where   MappingName = 'MaximumLength')


--'MinimumLength'

	INSERT INTO [Ident].[OrganizationSettings] (PartyId,SettingCategoryTypeId,MappingName,
		MappingValue,Editable,[Hidden],CreatedBy,CreatedDate)
	Select PartyId,@SettingCategoryTypeId,'MinimumLength',
		MinimumLength,1,0,@UserId,@Now
	From [Ident].[PasswordPolicy]  
	where PartyId  not in (select PartyId From Ident.OrganizationSettings where   MappingName = 'MinimumLength')



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
WHERE p.ProductId NOT IN (22, 34)
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

 --Adding Product Setting Type for ILM & ILM LA
			   
			   IF NOT EXISTS (Select Top 1 1 From Enterprise.ProductSettingType Where Name='CreateUpdateMultiCompanyUserRequiresPMC')
	              BEGIN
		              INSERT INTO Enterprise.ProductSettingType ( name, Description)
                      VALUES ('CreateUpdateMultiCompanyUserRequiresPMC','Create Update MultiComapny User Required PMC')   
	              END
			      
			     DECLARE @ProductSttingTypeId bigint,@ConfigurationId INT,@ProductSettingILM INT,@ProductSettingILMLA INT;
				  
                 DECLARE @ConfigurationIDTable TABLE (ID INT);
				 DECLARE @ProductSettingILMTable TABLE (ID INT);
				 DECLARE @ProductSettingILMLATable TABLE (ID INT);
			   
			     SELECT @ProductSttingTypeId = ProductSettingTypeId From Enterprise.ProductSettingType Where Name='CreateUpdateMultiCompanyUserRequiresPMC';  
				 
				 IF NOT EXISTS (Select TOP 1 1 From Enterprise.ProductSetting where ProductId = 40 and ProductSettingTypeId = @ProductSttingTypeId)
				 BEGIN
				    Insert into Enterprise.Configuration(CreateDate)   Output   Inserted.ConfigurationId INTO  @ConfigurationIDTable
                    Values(GetUTCDATE())
                    -- GET A NEW CONFIGURATION ID
                     SELECT @ConfigurationId = ID FROM @ConfigurationIDTable;
					 
					
		          INSERT INTO Enterprise.ProductSetting (ProductId,ProductSettingTypeId,VALUE,FromDate,ThruDate) Output   Inserted.ProductSettingId INTO  @ProductSettingILMTable
				  VALUES(40,@ProductSttingTypeId,1,GETUTCDATE(),NULL)
										   
                      -- GET A NEW PRODUCTSETTING ID FOR ILM
                     SELECT @ProductSettingILM  = ID FROM @ProductSettingILMTable;
	             END
				 
				 IF NOT EXISTS (Select TOP 1 1 From Enterprise.ProductSetting where ProductId = 41 and ProductSettingTypeId = @ProductSttingTypeId)
				 BEGIN
		         INSERT INTO Enterprise.ProductSetting (ProductId,ProductSettingTypeId,VALUE,FromDate,ThruDate) Output   Inserted.ProductSettingId INTO @ProductSettingILMLATable
					                       VALUES(41,@ProductSttingTypeId,1,GETUTCDATE(),NULL)
                     -- GET A NEW PRODUCTSETTING ID FOR ILMLA
                     SELECT @ProductSettingILMLA  = ID FROM @ProductSettingILMLATable;
	             END
				 
				 IF NOT EXISTS (SELECT TOP 1 1 FROM Enterprise.ProductConfiguration WHERE  ConfigurationId= @ConfigurationId and ProductSettingId =
				 @ProductSettingILM)
				 BEGIN
				 INSERT INTO Enterprise.ProductConfiguration(ConfigurationId,ProductSettingId,FromDate,ThruDate)
				 VALUES(@ConfigurationId,@ProductSettingILM,GETUTCDATE(),NULL)
				 END
				  IF NOT EXISTS (SELECT TOP 1 1 FROM Enterprise.ProductConfiguration WHERE  ConfigurationId= @ConfigurationId and ProductSettingId =
				 @ProductSettingILMLA)
				 BEGIN
				 INSERT INTO Enterprise.ProductConfiguration(ConfigurationId,ProductSettingId,FromDate,ThruDate)
				 VALUES(@ConfigurationId,@ProductSettingILMLA,GETUTCDATE(),NULL)
				 END
			     GO

				