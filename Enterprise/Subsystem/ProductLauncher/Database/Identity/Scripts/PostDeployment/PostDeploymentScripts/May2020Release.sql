GO

--Unified Platform Product Access Data
DECLARE @UserId bigint,
	@ProductId int = 3,
	@Now datetime = GETDATE()

SELECT	@UserId = UserId
FROM	Ident.UserLogin
WHERE	LoginName LIKE 'realpagead@%'

IF NOT EXISTS (SELECT TOP 1 1 FROM[UserManagement].[ProductPage] WHERE ProductId = @ProductId)
BEGIN
	SET IDENTITY_INSERT [UserManagement].[Control] ON 

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
	VALUES (135, NULL, 8, N'UnifiedPlatformProductAccessTabGroupUIId', NULL, NULL, 1, @UserId, @Now)

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
	VALUES (136, 135, 9, N'UnifiedPlatformProductAccessRolesTabUIId', N'Roles', NULL, 1, @UserId, @Now)

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
	VALUES (137, 136, 2, N'UnifiedPlatformProductAccessRolesSelectGridUIId', NULL, NULL, 1, @UserId, @Now)

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
	VALUES (138, 137, 7, N'UnifiedPlatformProductAccessRadioUIId', NULL, N'isAssigned', 1, @UserId, @Now)

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
	VALUES (139, 137, 5, N'UnifiedPlatformProductAccessRoleLabelUIId', N'Role', N'name', 2, @UserId, @Now)

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
	VALUES (140, 137, 5, N'UnifiedPlatformProductAccessRoleTypeLabelUIId', N'Role Type', N'roletype', 3, @UserId, @Now)

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
	VALUES (141, 137, 11, N'UnifiedPlatformProductAccessIconUIId', NULL, N'InfoIcon', 4, @UserId, @Now)

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
	VALUES (142, 141, 5, N'UnifiedPlatformProductAccessRoleDetailsLabelUIId', N'Role Details', NULL, 1, @UserId, @Now)

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
	VALUES (143, 141, 12, N'UnifiedPlatformProductAccessGridUIId', NULL, NULL, 2, @UserId, @Now)

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
	VALUES (144, 143, 5, N'UnifiedPlatformProductAccessRightLabelUIId', N'Right', N'description', 2, @UserId, @Now)

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
	VALUES (145, 135, 9, N'UnifiedPlatformProductAccessPropertiesTabUIId', N'Properties', NULL, 2, @UserId, @Now)

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
	VALUES (146, 145, 1, N'UnifiedPlatformProductAccessAllowaccesstoallcurrentandfuturepropertiesPropertiesSwitchUIId', N'Allow access to current and future properties', N'allProperties', 1, @UserId, @Now)

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
	VALUES (147, 145, 3, N'UnifiedPlatformProductAccessPropertiesMultiSelectGridUIId', NULL, NULL, 2, @UserId, @Now)

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
	VALUES (148, 147, 10, N'UnifiedPlatformProductAccessCheckboxUIId', NULL, N'isAssigned', 1, @UserId, @Now)

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
	VALUES (149, 147, 5, N'UnifiedPlatformProductAccessPropertyLabelUIId', N'Property', N'name', 2, @UserId, @Now)

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
	VALUES (150, 147, 5, N'UnifiedPlatformProductAccessCityLabelUIId', N'City', N'city', 3, @UserId, @Now)

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
	VALUES (151, 147, 5, N'UnifiedPlatformProductAccessStateLabelUIId', N'State', N'state', 4, @UserId, @Now)

	SET IDENTITY_INSERT [UserManagement].[Control] OFF

	SET IDENTITY_INSERT [UserManagement].[ControlAttribute] ON 

	INSERT [UserManagement].[ControlAttribute] ([ControlAttributeId], [ControlId], [Key], [Value], [CreatedBy], [CreatedDate])
	VALUES (18, 136, N'Default', N'True', @UserId, @Now)

	INSERT [UserManagement].[ControlAttribute] ([ControlAttributeId], [ControlId], [Key], [Value], [CreatedBy], [CreatedDate])
	VALUES (19, 136, N'Hide', N'False', @UserId, @Now)

	INSERT [UserManagement].[ControlAttribute] ([ControlAttributeId], [ControlId], [Key], [Value], [CreatedBy], [CreatedDate])
	VALUES (20, 141, N'InfoIcon', N'Slide', @UserId, @Now)

	INSERT [UserManagement].[ControlAttribute] ([ControlAttributeId], [ControlId], [Key], [Value], [CreatedBy], [CreatedDate])
	VALUES (21, 145, N'Hide', N'True', @UserId, @Now)

	INSERT [UserManagement].[ControlAttribute] ([ControlAttributeId], [ControlId], [Key], [Value], [CreatedBy], [CreatedDate])
	VALUES (22, 147, N'ShowSelectAll', N'True', @UserId, @Now)

	SET IDENTITY_INSERT [UserManagement].[ControlAttribute] OFF

	SET IDENTITY_INSERT [UserManagement].[ControlDependency] ON 
	
	INSERT [UserManagement].[ControlDependency] ([ControlDependencyId], [MasterControlId], [SlaveControlID], [MasterControlValue], [ComparatorId], [CreatedBy], [CreatedDate])
	VALUES (16, 139, 145, N'ManageCIMPLQuestions', 1, @UserId, @Now)

	INSERT [UserManagement].[ControlDependency] ([ControlDependencyId], [MasterControlId], [SlaveControlID], [MasterControlValue], [ComparatorId], [CreatedBy], [CreatedDate])
	VALUES (17, 139, 145, N'CIMPLManagePII', 1, @UserId, @Now)

	INSERT [UserManagement].[ControlDependency] ([ControlDependencyId], [MasterControlId], [SlaveControlID], [MasterControlValue], [ComparatorId], [CreatedBy], [CreatedDate])
	VALUES (18, 139, 145, N'CIMPLManageSensitiveFinancialData', 1, @UserId, @Now)

	INSERT [UserManagement].[ControlDependency] ([ControlDependencyId], [MasterControlId], [SlaveControlID], [MasterControlValue], [ComparatorId], [CreatedBy], [CreatedDate])
	VALUES (19, 139, 145, N'ViewCIMPLQuestions', 1, @UserId, @Now)

	SET IDENTITY_INSERT [UserManagement].[ControlDependency] OFF

	SET IDENTITY_INSERT [UserManagement].[ProductPage] ON 

	INSERT [UserManagement].[ProductPage] ([ProductPageId], [ProductId], [DisplayName], [CreatedBy], [CreatedDate], [IsActive])
	VALUES (9, 3, N'Unified Platform Product Access', @UserId, @Now, 1)

	SET IDENTITY_INSERT [UserManagement].[ProductPage] OFF

	SET IDENTITY_INSERT [UserManagement].[ProductPageControl] ON 

	INSERT [UserManagement].[ProductPageControl] ([ProductPageControlId], [ProductPageId], [ControlId], [CreatedBy], [CreatedDate])
	VALUES (18, 9, 135, @UserId, @Now)

	SET IDENTITY_INSERT [UserManagement].[ProductPageControl] OFF
END

IF NOT EXISTS(SELECT TOP 1 1 FROM[UserManagement].[Control] WHERE [ControlId] = 86)
BEGIN
	SET IDENTITY_INSERT [UserManagement].[Control] ON
	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
	VALUES (86, 147, 5, N'UnifiedPlatformProductAccessAddressLabelUIId', N'Address', N'street1', 3, @UserId, @Now)
	SET IDENTITY_INSERT [UserManagement].[Control] OFF
END

IF NOT EXISTS(SELECT TOP 1 1 FROM[UserManagement].[Control] WHERE [ControlId] = 87)
BEGIN
	SET IDENTITY_INSERT [UserManagement].[Control] ON
	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
	VALUES (87, 147, 5, N'UnifiedPlatformProductAccessZipLabelUIId', N'Zip Code', N'zip', 6, @UserId, @Now)
	SET IDENTITY_INSERT [UserManagement].[Control] OFF
END

UPDATE	[UserManagement].[Control]
SET			[DisplayName] = N'Assign current and new properties automatically'
WHERE	[ControlId] = 146
AND			[DisplayName] = N'Allow access to all current and future properties'

--City
UPDATE	[UserManagement].[Control]
SET			[Sequence] = 4
WHERE	[ControlId] = 150
AND			[Sequence] = 3

--State
UPDATE	[UserManagement].[Control]
SET			[Sequence] = 5
WHERE	[ControlId] = 151
AND			[Sequence] = 4

GO

--Unified Platform Product Access Data
DECLARE @UserId bigint,
	@ProductId int ,
	@Now datetime = GETDATE()

SELECT	@UserId = UserId
FROM	Ident.UserLogin
WHERE	LoginName LIKE 'realpagead@%'

--L2L
Select @ProductId = 6
IF NOT EXISTS (SELECT TOP 1 1 FROM[UserManagement].[ProductPage] WHERE ProductId = @ProductId)
BEGIN
	SET IDENTITY_INSERT [UserManagement].[ControlType] ON
	INSERT [UserManagement].[ControlType] ([ControlTypeId], [Name], [Description], [CreatedBy], [CreatedDate]) VALUES (13, N'Select', N'DropDown Select List', @UserId, @Now)
	SET IDENTITY_INSERT [UserManagement].[ControlType] OFF

	SET IDENTITY_INSERT [UserManagement].[Control] ON 
	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate]) VALUES (153, NULL, 8, N'Lead2LeaseProductAccessTabGroupUIId', NULL, NULL, 1, @UserId, @Now)
	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate]) VALUES (154, 153, 9, N'Lead2LeaseProductAccessPropertiesTabUIId', N'Properties', NULL, 1, @UserId, @Now)
	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate]) VALUES (155, 154, 3, N'Lead2LeaseProductAccessPropertiesMultiSelectGridUIId', NULL, NULL, 2, @UserId, @Now)
	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate]) VALUES (156, 155, 10, N'Lead2LeaseProductAccessCheckboxUIId', NULL, N'isAssigned', 1, @UserId, @Now)
	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate]) VALUES (157, 155, 5, N'Lead2LeaseProductAccessPropertyLabelUIId', N'Property', N'name', 2, @UserId, @Now)
	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate]) VALUES (158, 155, 5, N'Lead2LeaseProductAccessStateLabelUIId', N'State', N'state', 3, @UserId, @Now)
	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate]) VALUES (159, 153, 9, N'Lead2LeaseProductAccessRightsTabUIId', N'Rights', NULL, 2, @UserId, @Now)
	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate]) VALUES (160, 159, 13, N'Lead2LeaseProductAccessSelectaPresetRoleRightsSelectUIId', N'Select a Preset Role', N'roles', 1, @UserId, @Now)
	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate]) VALUES (161, 159, 3, N'Lead2LeaseProductAccessRightsMultiSelectGridUIId', NULL, NULL, 1, @UserId, @Now)
	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate]) VALUES (162, 161, 10, N'Lead2LeaseProductAccessCheckboxUIId', NULL, N'isAssigned', 1, @UserId, @Now)
	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate]) VALUES (163, 161, 5, N'Lead2LeaseProductAccessRightLabelUIId', N'Right', N'name', 2, @UserId, @Now)

	SET IDENTITY_INSERT [UserManagement].[Control] OFF

	SET IDENTITY_INSERT [UserManagement].[ControlAttribute] ON 
	INSERT [UserManagement].[ControlAttribute] ([ControlAttributeId], [ControlId], [Key], [Value], [CreatedBy], [CreatedDate]) VALUES (23, 154, N'Default', N'True', @UserId, @Now)
	INSERT [UserManagement].[ControlAttribute] ([ControlAttributeId], [ControlId], [Key], [Value], [CreatedBy], [CreatedDate]) VALUES (24, 155, N'ShowSelectAll', N'True', @UserId, @Now)
	INSERT [UserManagement].[ControlAttribute] ([ControlAttributeId], [ControlId], [Key], [Value], [CreatedBy], [CreatedDate]) VALUES (25, 161, N'ShowSelectAll', N'True', @UserId, @Now)

	SET IDENTITY_INSERT [UserManagement].[ControlAttribute] OFF

	SET IDENTITY_INSERT [UserManagement].[ProductPage] ON 
	INSERT [UserManagement].[ProductPage] ([ProductPageId], [ProductId], [DisplayName], [CreatedBy], [CreatedDate], [IsActive]) VALUES (10, 6, N'Lead2Lease Product Access', @UserId, @Now, 0)
	SET IDENTITY_INSERT [UserManagement].[ProductPage] OFF

	SET IDENTITY_INSERT [UserManagement].[ProductPageControl] ON 
	INSERT [UserManagement].[ProductPageControl] ([ProductPageControlId], [ProductPageId], [ControlId], [CreatedBy], [CreatedDate]) VALUES (19, 10, 153, @UserId, @Now)
	SET IDENTITY_INSERT [UserManagement].[ProductPageControl] OFF
END

--On-Site
Select @ProductId = 23
IF NOT EXISTS (SELECT TOP 1 1 FROM[UserManagement].[ProductPage] WHERE ProductId = @ProductId)
BEGIN
	
	SET IDENTITY_INSERT [UserManagement].[Control] ON 
	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
	VALUES (164, NULL, 8, N'On-SiteProductAccessTabGroupUIId', NULL, NULL, 1, @UserId, @Now)

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
	VALUES (165, 164, 9, N'On-SiteProductAccessPropertyGroupTabUIId', N'Property Group', NULL, 1, @UserId, @Now)

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
	VALUES (166, 165, 3, N'On-SiteProductAccessPropertyGroupMultiSelectGridUIId', NULL, NULL, 1, @UserId, @Now)

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
	VALUES (167, 166, 10, N'On-SiteProductAccessCheckboxUIId', NULL, N'isAssigned', 1, @UserId, @Now)

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
	VALUES (168, 166, 5, N'On-SiteProductAccessPropertyGroupLabelUIId', N'Property Group', N'name', 2, @UserId, @Now)

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
	VALUES (169, 164, 9, N'On-SiteProductAccessPropertiesTabUIId', N'Properties', NULL, 2, @UserId, @Now)

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
	VALUES (170, 169, 1, N'On-SiteProductAccessAssignnewpropertiesautomaticallyPropertiesSwitchUIId', N'Assign new properties automatically', 'allProperties', 1, @UserId, @Now)

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
	VALUES (171, 169, 3, N'On-SiteProductAccessPropertiesMultiSelectGridUIId', NULL, NULL, 2, @UserId, @Now)

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
	VALUES (172, 171, 10, N'On-SiteProductAccessCheckboxUIId', NULL, N'isAssigned', 1, @UserId, @Now)

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
	VALUES (173, 171, 5, N'On-SiteProductAccessPropertyLabelUIId', N'Property', N'name', 2, @UserId, @Now)

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
	VALUES (174, 171, 5, N'On-SiteProductAccessStateLabelUIId', N'City', N'city', 3, @UserId, @Now)

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
	VALUES (175, 171, 5, N'On-SiteProductAccessStateLabelUIId', N'State', N'state', 3, @UserId, @Now)

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
	VALUES (176, 164, 9, N'On-SiteProductAccessRolesTabUIId', N'Roles', NULL, 3, @UserId, @Now)

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
	VALUES (177, 176, 2, N'On-SiteProductAccessRolesSelectGridUIId', NULL, NULL, 1, @UserId, @Now)

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
	VALUES (178, 177, 7, N'On-SiteProductAccessRadioUIId', NULL, N'isAssigned', 1, @UserId, @Now)

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
	VALUES (179, 177, 5, N'On-SiteProductAccessRoleLabelUIId', N'Role', N'name', 2, @UserId, @Now)
	
	SET IDENTITY_INSERT [UserManagement].[Control] OFF

	SET IDENTITY_INSERT [UserManagement].[ControlAttribute] ON 

	INSERT [UserManagement].[ControlAttribute] ([ControlAttributeId], [ControlId], [Key], [Value], [CreatedBy], [CreatedDate])
	VALUES (26, 166, N'ShowSelectAll', N'True', @UserId, @Now)

	INSERT [UserManagement].[ControlAttribute] ([ControlAttributeId], [ControlId], [Key], [Value], [CreatedBy], [CreatedDate])
	VALUES (27, 171, N'ShowSelectAll', N'True', @UserId, @Now)
	
	INSERT [UserManagement].[ControlAttribute] ([ControlAttributeId], [ControlId], [Key], [Value], [CreatedBy], [CreatedDate]) 
	VALUES (28, 169, N'Default', N'True', @UserId, @Now)

	SET IDENTITY_INSERT [UserManagement].[ControlAttribute] OFF

	SET IDENTITY_INSERT [UserManagement].[ProductPage] ON 	
	INSERT [UserManagement].[ProductPage] ([ProductPageId], [ProductId], [DisplayName], [CreatedBy], [CreatedDate],[IsActive])
	VALUES (11, 23, N'On-site Product Access', @UserId, @Now,0)
	
	SET IDENTITY_INSERT [UserManagement].[ProductPage] OFF

	SET IDENTITY_INSERT [UserManagement].[ProductPageControl] ON 
	INSERT [UserManagement].[ProductPageControl] ([ProductPageControlId], [ProductPageId], [ControlId], [CreatedBy], [CreatedDate])
	VALUES (20, 11, 164, @UserId, @Now)
	SET IDENTITY_INSERT [UserManagement].[ProductPageControl] OFF
END
GO

DECLARE @ProductId INT, 
		@ParentProductTypeId INT, 
		@ProductName NVARCHAR(100)= 'Vendor Marketplace',  -- Produact Name
		@ServerName SYSNAME = @@SERVERNAME;

DECLARE @ProductConfiguration AS PRODUCTCONFIGURATIONTYPE;

/*Validate what product type ths new product belongs to. 'Administration' in the following block 
need to be chnanged to desired prodcut type. You can query Enterprise.ProductType table for more details.
*/

INSERT INTO @ProductConfiguration(SettingName,  SettingDescription,  SettingValue) VALUES('AlternateLoginURL','Alternate URL that can be used for Product Login','https://dev.realpagevendormarketplace.com/login')

IF @ServerName IN ( 'RCTUSODBSQL001')
BEGIN
	Update @ProductConfiguration set SettingValue = 'https://qa.realpagevendormarketplace.com/login' where SettingName = 'AlternateLoginURL'
END
IF @ServerName IN ( 'RCQUSODBSQL001')
BEGIN
	Update @ProductConfiguration set SettingValue = 'https://sat.realpagevendormarketplace.com/login' where SettingName = 'AlternateLoginURL'
END
IF @ServerName IN ( 'RCPGBKDBSQL005A', 'RCPGBKDBSQL005B')
BEGIN
	Update @ProductConfiguration set SettingValue = 'https://www.realpagevendormarketplace.com/login' where SettingName = 'AlternateLoginURL'
END

--The following block picks up all the detail frm Enterprise.ProductSettingType table
--To set up the product, bunch of these settings are required.
set nocount on
--SELECT * FROM @ProductConfiguration

if not exists(Select top 1 1 from Enterprise.ProductSettingType where Name = 'AlternateLoginURL')
Begin
	Insert into Enterprise.ProductSettingType (Name, Description) Values ('AlternateLoginURL', 'Alternate URL that can be used for Product Login')
End

if not exists(Select top 1 * from Enterprise.ProductSetting ps 
				inner join Enterprise.ProductSettingType pst
				on ps.ProductSettingTypeId = pst.ProductSettingTypeId
				where pst.Name = 'AlternateLoginURL' and ps.ProductId= 38)
Begin
	Insert into Enterprise.ProductSetting (ProductId, ProductSettingTypeId, Value, FromDate)
	Select 38, ProductSettingTypeId, SettingValue, GETUTCDATE()
	from @ProductConfiguration cross join Enterprise.ProductSettingType
	where Name = 'AlternateLoginURL'

	declare @productsettingid int
	select @productsettingid = productsettingid from Enterprise.ProductSetting ps 
				inner join Enterprise.ProductSettingType pst
				on ps.ProductSettingTypeId = pst.ProductSettingTypeId
				where pst.Name = 'AlternateLoginURL' and ps.ProductId= 38

	insert into enterprise.ProductConfiguration ( ConfigurationId, ProductSettingId, FromDate )
		select top 1 ConfigurationId, @productsettingid, GETUTCDATE() from enterprise.GlobalProductConfiguration where productid = 38 and thrudate is null
End

GO

/*ASSIGN VALUES*/

DECLARE @OrganizationId int;
DECLARE @PartyRowNum int;
DECLARE @RightName nvarchar(200);
DECLARE @RightDescription nvarchar(200);
DECLARE @RightShortName nvarchar(200);
DECLARE @ActionName nvarchar(100);
DECLARE @ActionRouteTarget nvarchar(100);
DECLARE @ActionValueId int;
DECLARE @SourceProductId int;
DECLARE @TargetProductId int;
DECLARE @RoleCategory int;
DECLARE @RightCategory int;
DECLARE @VisibilityStatusId int;
DECLARE @ActionId int;
DECLARE @ParentActionId int;
DECLARE @DetaulRightName nvarchar(200);
DECLARE @TargetRoleName nvarchar(100);
DECLARE @RoleId int;
DECLARE @OutputRightId int;
DECLARE @UserActionId int;
DECLARE @RightValueTypeId int;
DECLARE @DependentRightValueTypeId int;

/*SET BLOCK*/
SET @TargetRoleName = 'User Administrator'; --- Role to which the new right will be assinged by default.
SET @RightName = 'Employee Access to Vendor Marketplace'; -- Name of the right 
SET @RightDescription = 'Employee Access to Vendor Marketplace'; --Description of the right as stated in story.
SET @RightShortName = 'EmployeeAccessVendorMarketPlace'; --Short name of the right that is being used by the application
SET @ActionName = 'Employee Vendor MarketPlace'; -- This specifically pertains to actions used for routing purposes. 
SET @ActionRouteTarget = 'DashBoard'; -- Where you want this right to show up. other variation is DashBoard.
SET @ActionValueID = 1;
SET @DetaulRightName = 'Default_' + @RightShortName; -- This is used internally for creating right dependency in RightDependency table.

/*CLEANUP  AND LOAD TEMPORARY TABLE FOR ORG LIST*/

IF OBJECT_ID('tempdb..#HoldParty') IS NOT NULL
BEGIN
	DROP TABLE #HoldParty;
END;

SELECT DISTINCT 
	   IDENTITY(int, 1, 1) AS RowNumber, o.PartyId AS OrganizationPartyID, 0 AS PStatus
INTO #HoldParty
FROM Enterprise.Organization AS o
	 INNER JOIN
	 Enterprise.Party AS p
	 ON P.partyid = O.PartyId
WHERE O.Name= 'RealPage Employee'; 
--1. If rigths ne ed in all organization then no condition 
--2. If needed in all except RP Employee company then O.Name <> 'RealPage Employee'
--3. If needed in just RP Employee and not in any other company, then  O.Name = 'RealPage Employee'

/*SELECT REQUIRED ATTRIBUTES FOR ROLE, RIGHT, AND ACTIONS*/
SELECT @SourceProductId = ProductId
FROM Enterprise.Product
WHERE name = 'Unified Platform';

SELECT @TargetProductId = ProductId
FROM Enterprise.Product
WHERE Name = 'Unified Platform';

SELECT @RoleCategory = TypeId
FROM Enterprise.RoleRightStatus AS rrs
WHERE CategoryName = 'Role Type' AND 
	  TypeName = 'System';

SELECT @RightCategory = TypeId
FROM Enterprise.RoleRightStatus AS rrs
WHERE CategoryName = 'Right Type' AND 
	  TypeName = 'System';

SELECT @VisibilityStatusId = TypeId
FROM Enterprise.RoleRightStatus AS rrs
WHERE TypeName = 'ALL' AND 
	  CategoryType = 'Security';

IF NOT EXISTS
(
	SELECT 1
	FROM Enterprise.ACTION
	WHERE ObjectValue = @ActionName AND 
		  ParentActionId IS NULL
)
BEGIN
	EXEC Enterprise.CreateAction 
     @ProductID = @SourceProductId, 
     @Action = @ActionName, 
     @ActionTarget = N'Right', 
     @ActionbValueTypeId = 1, 
     @Description = '', 
     @ActionID = @ActionID OUTPUT;
SELECT @ActionID AS N'@ActionID';
END;

SELECT @ParentActionId = ActionId
FROM Enterprise.ACTION
WHERE ObjectValue = @ActionRouteTarget AND 
	  ObjectType = 'Route' AND 
	  Description = 'SuperUser';

IF NOT EXISTS
(
	SELECT 1
	FROM Enterprise.ACTION
	WHERE ObjectValue = @ActionName AND 
		  ParentActionID = @ParentActionId
)
BEGIN
EXEC [Enterprise].[CreateAction] 
     @ProductID = @SourceProductId, 
     @Action = @ActionName, 
     @ActionTarget = N'Right', 
     @ActionbValueTypeId = 1, 
     @Description = '', 
     @ParentActionID = @ParentActionId, 
     @ActionID = @ActionID OUTPUT;
SELECT @ActionID AS N'@ActionID';
END;

SELECT @ActionID = ActionID
FROM Enterprise.ACTION
WHERE ObjectValue = @ActionName AND 
	  ObjectType = 'Right' AND 
	  ParentActionId IS NULL;

WHILE EXISTS
(
	SELECT 1
	FROM #HoldParty
	WHERE PStatus = 0
)
BEGIN
	SELECT TOP 1 @PartyRowNum = Rownumber, @OrganizationId = OrganizationPartyID
	FROM #HoldParty
	WHERE PStatus = 0;
	SELECT @RoleId = RoleId
	FROM Enterprise.Role AS R
		 INNER JOIN
		 Enterprise.RoleValueType AS RR
		 ON RR.RoleValueTypeId = R.RoleValueTypeId
	WHERE RR.Value = @TargetRoleName AND 
		  R.PartyId = @OrganizationId;
	EXECUTE Enterprise.CreateRight @RoleId = -1, @RightName = @DetaulRightName, @ShortName = @RightShortName, @RightCategoryId = @RightCategory, @PartyId = @OrganizationId, @ProductId = @SourceProductId, @Description = '', @TargetProductId = @TargetProductId, @VisibilityStatusId = @VisibilityStatusId, @RightId = @OutputRightId OUTPUT;
	EXEC [Enterprise].[LinkActionToRights] @ActionID = @ActionID, @RightId = @OutputRightId, @StatusId = @VisibilityStatusId, @UserActionId = @UserActionId OUTPUT;
	EXECUTE Enterprise.CreateRight @RoleId = @RoleId, @RightName = @RightName, @RightCategoryId = @RightCategory, @PartyId = @OrganizationId, @ProductId = @SourceProductId, @Shortname = @RightShortName, @Description = @RightDescription, @TargetProductId = @TargetProductId, @VisibilityStatusId = @VisibilityStatusId, @RightId = @OutputRightId OUTPUT;
	EXEC [Enterprise].[LinkActionToRights] @ActionID = @ActionID, @RightId = @OutputRightId, @StatusId = @VisibilityStatusId, @UserActionId = @UserActionId OUTPUT;
	UPDATE #HoldParty
	  SET PStatus = 1
	WHERE RowNumber = @PartyRowNum;
END;

/*Setup Dependencies for custom roles*/

SELECT @DependentRightValueTypeId = RightValueTypeId
FROM Enterprise.RightValueType
WHERE value = @DetaulRightName;

SELECT @RightValueTypeId = RightValueTypeId
FROM Enterprise.RightValueType
WHERE value = @RightName;

IF NOT EXISTS
(
	SELECT 1
	FROM Enterprise.RightDependency
	WHERE RightValueTypeId = @RightValueTypeId AND 
		  DependentRightValueTypeId = @DependentRightValueTypeId
)
BEGIN
	INSERT INTO Enterprise.RightDependency( RightValueTypeId, DependentRightValueTypeId )
	VALUES( @RightValueTypeId, @DependentRightValueTypeId );
END;

Go

Declare @PartyId int;
Declare @RoleId int;
Declare @RightValueTypeId int;

select @PartyId = PartyId from Enterprise.Organization
where name = 'RealPage Employee'

select @RoleId = r.RoleID from Enterprise.Role r
inner join Enterprise.RoleValueType rovt
on rovt.RoleValueTypeId = r.RoleValueTypeId
where  r.PartyID = @PartyId
and rovt.Value = 'User Administrator'

select  @RightValueTypeId = r.RightValueTypeId from Enterprise.[Right] r 
inner join Enterprise.RightValueType rvt
on rvt.RightValueTypeId = r.RightValueTypeId
where PartyId = @PartyId
and r.RoleID = @RoleId
and ShortName = 'AccessVendorMarketplace'

select * from Enterprise.[Right]
where RightValueTypeId = @RightValueTypeId
and PartyId = @PartyId

if exists(select * from Enterprise.[Right] where  RightValueTypeId = @RightValueTypeId and PartyId = @PartyId)
BEGIN
	DELETE from Enterprise.[Right] where  RightValueTypeId = @RightValueTypeId and PartyId = @PartyId
END;

Go

DECLARE @ProductId INT = 3,
		 @CurrentProductConfigurationID INT,
		 @ProductSettingTypeId INT,
		 @ProductSettingId INT,
		 @Now DATETIME = GETUTCDATE()

SELECT TOP 1 @CurrentProductConfigurationID = ConfigurationId
FROM Enterprise.GlobalProductConfiguration AS gpc
WHERE gpc.ProductId = @ProductId AND 
		( ( @NOW BETWEEN gpc.FromDate AND gpc.ThruDate
		) OR 
		( @NOW >= gpc.FromDate AND 
			gpc.ThruDate IS NULL
		)
		)
ORDER BY GlobalProductConfigurationId DESC;

IF
(
	SELECT 1
	FROM Enterprise.ProductSettingType
	WHERE Name = 'TiboWebHookSigningSecret'
) IS NULL
BEGIN
	EXEC Enterprise.CreateProductSettingType 'TiboWebHookSigningSecret', 'Used to verify Tibco WebHook requests', @ProductSettingTypeId OUTPUT;
END;

IF @ProductSettingTypeId IS NOT NULL AND 
	   NOT EXISTS
	(
		SELECT TOP 1 1
		FROM Enterprise.ProductSetting
		WHERE ProductID = @productId AND 
			  ProductSettingTypeId = @ProductSettingTypeId AND 
			  ThruDate IS NULL
	)
	BEGIN
	
		-- Create the Value and assign it to the Product and ProductSettingType
		EXEC Enterprise.CreateProductSetting @ProductId = @ProductId, -- int
		@ProductSettingTypeId = @ProductSettingTypeId, -- int
		@Value = '7EFE59F38D17D83721E241D27638FED0', 
		@FromDate = @NOW, -- datetime
		@ThruDate = NULL, -- datetime
		@ProductSettingId = @ProductSettingId OUTPUT; -- int

		-- Link the Product Setting to an actual configuration
		EXEC Enterprise.LinkProductSettingToConfiguration @ConfigurationId = @CurrentProductConfigurationID, -- int
		@ProductSettingId = @ProductSettingId, -- int
		@FromDate = @NOW, -- datetime
		@ThruDate = NULL;   -- datetime
	END;

GO

DECLARE
	@NOW DATETIME = GETUTCDATE()

DECLARE @PropertyMapping TABLE (
	PersonaId bigint,
	PropertyId bigint,
	ProductId int,
	FromDate datetime,
	ThruDate datetime
)

--List Roles that have CIMPL OR Settings Rights
--Filter Enterprise.PersonaPrivilege to these Roles
;With cteRightValueType (
	RightValueTypeId
)
AS
(
	SELECT	RightValueTypeId
	FROM	Enterprise.RightValueType
	WHERE	Value IN ('Ability to Answer Questions for CIMPL', 'Access to Submit questionnaires within CIMPL', 'Manage Personally Identifiable Information (PII) in CIMPL', 'Manage Sensitive Financial Data in CIMPL', 'View CIMPL Implementation Questions', 'Manage All Unified Settings', 'Manage Settings Templates', 'View all Unified Settings')
)

INSERT INTO @PropertyMApping (
	PersonaId,
	PropertyId,
	ProductId,
	FromDate,
	ThruDate
)
SELECT	DISTINCT
				epp.PersonaId,
				'-1' AS 'PropertyId',
				erivt.ProductId,
				@Now,
				NULL
FROM		Enterprise.[Right] eri
				INNER JOIN Enterprise.Role  ero ON (eri.RoleID = ero.RoleID)
				INNER JOIN Enterprise.RoleValueType erovt ON (erovt.RoleValueTypeId = ero.RoleValueTypeId)
				INNER JOIN Enterprise.RightValueType erivt ON (erivt.RightValueTypeId = eri.RightValueTypeId)
				INNER JOIN Enterprise.StatusType est ON (erovt.StatusTypeId = est.StatusTypeId)
				INNER JOIN Enterprise.PersonaPrivilege epp ON (epp.RoleID = ero.RoleID)
WHERE	eri.RightValueTypeId IN (
	SELECT	RightValueTypeId
	FROM	cteRightValueType
)

--UnAssign existing Properties by Setting the ThruDate By PersoanId and ProductID
--SELECT		epm.*
UPDATE	epm
SET			epm.ThruDate = @Now
FROM		Enterprise.PropertyMapping epm
				INNER JOIN @PropertyMapping pm ON (epm.PersonaId = pm.PersonaId AND	epm.ProductId = pm.ProductId)
WHERE	((@NOW >= epm.FromDate AND epm.ThruDate IS NULL) OR (@NOW BETWEEN epm.FromDate AND epm.ThruDate))
AND			epm.PropertyId != -1

--Assign All properties (-1) to each PersonaId by adding a records to Enterprise.PropertyMapping
INSERT INTO Enterprise.PropertyMapping (
	PersonaId,
	PropertyId,
	ProductId,
	FromDate,
	ThruDate
)
SELECT	pm.PersonaId,
				pm.PropertyId,
				pm.ProductId,
				pm.FromDate,
				pm.ThruDate
FROM	@PropertyMApping pm
			LEFT OUTER JOIN Enterprise.PropertyMapping epm ON (epm.PersonaId = pm.PersonaId AND epm.ProductId = pm.ProductId AND epm.PropertyId = pm.PropertyId)
WHERE	epm.PersonaId IS NULL
AND			epm.ProductId IS NULL

GO
