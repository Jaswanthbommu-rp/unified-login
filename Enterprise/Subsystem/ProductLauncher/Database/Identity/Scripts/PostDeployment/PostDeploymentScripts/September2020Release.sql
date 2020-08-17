GO
IF NOT EXISTS (SELECT 1 FROM [UserManagement].[ProductPageType])
BEGIN
	SET IDENTITY_INSERT [UserManagement].[ProductPageType] ON
	Insert Into [UserManagement].[ProductPageType] (ProductPageTypeId, Value,Description)
	Select 1,'ProductAccess','Product Access Page'

	Insert Into [UserManagement].[ProductPageType] (ProductPageTypeId,Value,Description)
	Select 2,'RolesAndRights','Roles And Rights Page'

	SET IDENTITY_INSERT [UserManagement].[ProductPageType] OFF
END

GO
DECLARE @UserId bigint,
	@ProductId int ,
	@Now datetime = GETDATE();

SELECT	@UserId = UserId
FROM	Ident.UserLogin
WHERE	LoginName LIKE 'realpagead@%'

IF NOT EXISTS (SELECT 1 FROM [UserManagement].[ControlType] WHERE Name = 'Button')
BEGIN
	Insert into [UserManagement].[ControlType](name,Description,CreatedBy,CreatedDate)
	Select 'Button','Command Button',@UserId,@Now
END

IF NOT EXISTS (SELECT 1 FROM [UserManagement].[ControlType] WHERE Name = 'ActionMenu')
BEGIN
Insert into [UserManagement].[ControlType](name,Description,CreatedBy,CreatedDate)
Select 'ActionMenu','Action Menu',@UserId,@Now
END

IF NOT EXISTS (SELECT 1 FROM [UserManagement].[ControlType] WHERE Name = 'TextBox')
BEGIN
	Insert into [UserManagement].[ControlType](name,Description,CreatedBy,CreatedDate)
	Select 'TextBox','TextBox',@UserId,@Now
END

GO
--Unified Platform Product Access Data
DECLARE @UserId bigint,
	@ProductId int = 3,
	@ControlId int,
	@Now datetime = GETDATE()

SELECT	@UserId = UserId
FROM	Ident.UserLogin
WHERE	LoginName LIKE 'realpagead@%'

IF NOT EXISTS (SELECT TOP 1 1 FROM[UserManagement].[ProductPage] WHERE ProductId = @ProductId and ProductPageTypeId = 2)
BEGIN
	SET IDENTITY_INSERT [UserManagement].[Control] ON 
	Select @ControlId = MAX(controlid) +1 from [UserManagement].[Control]

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
	VALUES (@ControlId, NULL, 8, N'UnifiedPlatformRolesAndRightsTabGroupUIId', NULL, NULL, 1, @UserId, @Now)

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
	VALUES (@ControlId+1, @ControlId, 9, N'UnifiedPlatformRolesAndRightsRolesTabUIId', N'Roles', NULL, 1, @UserId, @Now)

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
	VALUES (@ControlId+2, @ControlId+1, 15, N'UnifiedPlatformRolesAndRightsRolesTabUIId', N'New Role', NULL, 1, @UserId, @Now)

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
	VALUES (@ControlId+3, @ControlId+1, 2, N'UnifiedPlatformRolesAndRightsRolesSelectGridUIId', NULL, NULL, 1, @UserId, @Now)

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
	VALUES (@ControlId+4, @ControlId+3, 5, N'UnifiedPlatformRolesAndRightsRoleLabelUIId', N'Role', N'name', 1, @UserId, @Now)

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
	VALUES (@ControlId+5, @ControlId+3, 14, N'UnifiedPlatformRolesAndRightsRadioUIId', N'Right', N'rightsAssigned', 2, @UserId, @Now)

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
	VALUES (@ControlId+6, @ControlId+3, 5, N'UnifiedPlatformRolesAndRightsRoleTypeLabelUIId', N'Role Type', N'roletype', 3, @UserId, @Now)

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
	VALUES (@ControlId+7, @ControlId+3, 5, N'UnifiedPlatformRolesAndRightsRoleTypeLabelUIId', NULL, N'defaultRole', 4, @UserId, @Now)

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
	VALUES (@ControlId+8, @ControlId+3, 16, N'UnifiedPlatformRolesAndRightsIconUIId', NULL, N'more', 5, @UserId, @Now)

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
	VALUES (@ControlId+9, @ControlId+5, 5, N'UnifiedPlatformRolesAndRightsRoleDetailsLabelUIId', N'Edit Custom Role', NULL, 1, @UserId, @Now)

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
	VALUES (@ControlId+10, @ControlId+9, 17, N'UnifiedPlatformRolesAndRightsRoleDetailsLabelUIId', N'Role Name', N'name', 2, @UserId, @Now)

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
	VALUES (@ControlId+11, @ControlId+9, 3, N'UnifiedPlatformRolesAndRightsGridUIId', NULL, NULL, 3, @UserId, @Now)

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
	VALUES (@ControlId+12, @ControlId+11, 10, N'UnifiedPlatformRolesAndRightsCheckboxUIId', NULL, N'isAssigned', 1, @UserId, @Now)

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
	VALUES (@ControlId+13, @ControlId+11, 5, N'UnifiedPlatformRolesAndRightsRightLabelUIId', N'Right', N'description', 2, @UserId, @Now)

	--tab 2

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
	VALUES (@ControlId+14, @ControlId, 9, N'UnifiedPlatformRolesAndRightsPropertiesTabUIId', N'Rights', NULL, 2, @UserId, @Now)

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
	VALUES (@ControlId+15, @ControlId+14, 12, N'UnifiedPlatformRolesAndRightsPropertiesMultiSelectGridUIId', NULL, NULL, 1, @UserId, @Now)

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
	VALUES (@ControlId+16, @ControlId+15, 5, N'UnifiedPlatformRolesAndRightsPropertyLabelUIId', N'Right', N'description', 1, @UserId, @Now)

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
	VALUES (@ControlId+17, @ControlId+15, 14, N'UnifiedPlatformRolesAndRightsCityLabelUIId', N'Role', N'rolesAssigned', 2, @UserId, @Now)

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
	VALUES (@ControlId+18, @ControlId+15, 16, N'UnifiedPlatformRolesAndRightsStateLabelUIId', NULL, N'more', 3, @UserId, @Now)

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
	VALUES (@ControlId+19, @ControlId+17, 5, N'UnifiedPlatformRolesAndRightsRoleDetailsLabelUIId', N'Assign Roles', NULL, 1, @UserId, @Now)

	--INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
	--VALUES (@ControlId+10, @ControlId+19, 17, N'UnifiedPlatformRolesAndRightsRoleDetailsLabelUIId', N'Role Name', N'name', 2, @UserId, @Now)

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
	VALUES (@ControlId+20, @ControlId+19, 3, N'UnifiedPlatformRolesAndRightsGridUIId', NULL, NULL, 3, @UserId, @Now)

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
	VALUES (@ControlId+21, @ControlId+20, 10, N'UnifiedPlatformRolesAndRightsCheckboxUIId', NULL, N'isAssigned', 1, @UserId, @Now)

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
	VALUES (@ControlId+22, @ControlId+20, 5, N'UnifiedPlatformRolesAndRightsRightLabelUIId', N'Right', N'description', 2, @UserId, @Now)

	SET IDENTITY_INSERT [UserManagement].[Control] OFF

	SET IDENTITY_INSERT [UserManagement].[ControlAttribute] ON 
	Declare @ControlAttributeId int
	Select @ControlAttributeId = MAX(ControlAttributeId) +1 from [UserManagement].[ControlAttribute]

	INSERT [UserManagement].[ControlAttribute] ([ControlAttributeId], [ControlId], [Key], [Value], [CreatedBy], [CreatedDate])
	VALUES (@ControlAttributeId, @ControlId+1, N'Default', N'True', @UserId, @Now)

	INSERT [UserManagement].[ControlAttribute] ([ControlAttributeId], [ControlId], [Key], [Value], [CreatedBy], [CreatedDate])
	VALUES (@ControlAttributeId+1, @ControlId+1, N'Hide', N'False', @UserId, @Now)

	INSERT [UserManagement].[ControlAttribute] ([ControlAttributeId], [ControlId], [Key], [Value], [CreatedBy], [CreatedDate])
	VALUES (@ControlAttributeId+2, @ControlId+5, N'Link', N'Slide', @UserId, @Now)

	INSERT [UserManagement].[ControlAttribute] ([ControlAttributeId], [ControlId], [Key], [Value], [CreatedBy], [CreatedDate])
	VALUES (@ControlAttributeId+3, @ControlId+8, N'Menu', N'ActionMenu', @UserId, @Now)

	INSERT [UserManagement].[ControlAttribute] ([ControlAttributeId], [ControlId], [Key], [Value], [CreatedBy], [CreatedDate])
	VALUES (@ControlAttributeId+4, @ControlId+11, N'ShowSelectAll', N'True', @UserId, @Now)
	
	INSERT [UserManagement].[ControlAttribute] ([ControlAttributeId], [ControlId], [Key], [Value], [CreatedBy], [CreatedDate])
	VALUES (@ControlAttributeId+5, @ControlId+17, N'Link', N'Slide', @UserId, @Now)

	INSERT [UserManagement].[ControlAttribute] ([ControlAttributeId], [ControlId], [Key], [Value], [CreatedBy], [CreatedDate])
	VALUES (@ControlAttributeId+6, @ControlId+18, N'ActionMenu', N'ActionMenu', @UserId, @Now)

	INSERT [UserManagement].[ControlAttribute] ([ControlAttributeId], [ControlId], [Key], [Value], [CreatedBy], [CreatedDate])
	VALUES (@ControlAttributeId+7, @ControlId+20, N'ShowSelectAll', N'True', @UserId, @Now)

	SET IDENTITY_INSERT [UserManagement].[ControlAttribute] OFF

	SET IDENTITY_INSERT [UserManagement].[ProductPage] ON 
	Declare @ProductPageId int
	Select @ProductPageId = MAX(ProductPageId) +1 from [UserManagement].[ProductPage]

	INSERT [UserManagement].[ProductPage] ([ProductPageId], [ProductId], [DisplayName],[ProductPageTypeId], [CreatedBy], [CreatedDate], [IsActive])
	VALUES (@ProductPageId, 3, N'Unified Platform Roles And Rights Access',2, @UserId, @Now, 1)

	SET IDENTITY_INSERT [UserManagement].[ProductPage] OFF

	SET IDENTITY_INSERT [UserManagement].[ProductPageControl] ON 
	Declare @ProductPageControlId int
	Select @ProductPageControlId = MAX(ProductPageControlId) +1 from [UserManagement].[ProductPageControl]

	INSERT [UserManagement].[ProductPageControl] ([ProductPageControlId], [ProductPageId], [ControlId], [CreatedBy], [CreatedDate])
	VALUES (@ProductPageControlId, @ProductPageId, @ControlId, @UserId, @Now)

	SET IDENTITY_INSERT [UserManagement].[ProductPageControl] OFF
END




