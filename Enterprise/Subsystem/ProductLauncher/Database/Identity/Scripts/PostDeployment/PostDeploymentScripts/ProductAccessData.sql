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
	VALUES (146, 145, 1, N'UnifiedPlatformProductAccessAllowaccesstoallcurrentandfuturepropertiesPropertiesSwitchUIId', N'Allow access to all current and future properties', N'allProperties', 1, @UserId, @Now)

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
GO