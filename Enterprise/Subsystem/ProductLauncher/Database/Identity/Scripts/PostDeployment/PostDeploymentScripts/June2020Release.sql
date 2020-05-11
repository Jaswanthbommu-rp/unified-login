GO

UPDATE	Enterprise.Product
SET			BooksProductCode = 'UPFM'
WHERE	ProductId = 3
AND			BooksProductCode = 'UL'

GO

--Unified Platform Product Access Data
DECLARE @UserId bigint,
	@ProductId int =15,
	@Now datetime = GETDATE()

SELECT	@UserId = UserId
FROM	Ident.UserLogin
WHERE	LoginName LIKE 'realpagead@%'

IF NOT EXISTS (SELECT TOP 1 1 FROM[UserManagement].[ProductPage] WHERE ProductId = @ProductId)
BEGIN
	SET IDENTITY_INSERT [UserManagement].[Control] ON 

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate]) 
	VALUES (180, NULL, 8, N'RentersInsuranceProductAccessTabGroupUIId', NULL, NULL, 1, @UserId, @Now)

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate]) 
	VALUES (181, 180, 9, N'RentersInsuranceProductAccessPropertiesTabUIId', N'Properties', NULL, 1, @UserId, @Now)

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate]) 
	VALUES (182, 181, 3, N'RentersInsuranceProductAccessPropertiesMultiSelectGridUIId', NULL, NULL, 2, @UserId, @Now)

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
	VALUES (183, 182, 10, N'RentersInsuranceProductAccessCheckboxUIId', NULL, N'isAssigned', 1, @UserId, @Now)

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate]) 
	VALUES (184, 182, 5, N'RentersInsuranceProductAccessPropertyLabelUIId', N'Property', N'name', 2, @UserId, @Now)

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
	VALUES (185, 182, 5, N'RentersInsuranceProductAccessStateLabelUIId', N'State', N'state', 3, @UserId, @Now)

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
	VALUES (186, 180, 9, N'RentersInsuranceProductAccessRolesTabUIId', N'Roles', NULL, 2, @UserId, @Now)

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate]) 
	VALUES (187, 186, 2, N'RentersInsuranceProductAccessRolesSelectGridUIId', NULL, NULL, 1, @UserId, @Now)

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
	VALUES (188, 187, 7, N'RentersInsuranceProductAccessRadioUIId', NULL, N'isAssigned', 1, @UserId, @Now)

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
	VALUES (189, 187, 5, N'RentersInsuranceProductAccessRoleLabelUIId', N'Role', N'name', 2, @UserId, @Now)
	
	SET IDENTITY_INSERT [UserManagement].[Control] OFF
	
	SET IDENTITY_INSERT [UserManagement].[ControlAttribute] ON 
	
	INSERT [UserManagement].[ControlAttribute] ([ControlAttributeId], [ControlId], [Key], [Value], [CreatedBy], [CreatedDate]) 
	VALUES (29, 181, N'Default', N'True', @UserId, CAST(N'2020-04-27T09:02:50.730' AS DateTime))

	INSERT [UserManagement].[ControlAttribute] ([ControlAttributeId], [ControlId], [Key], [Value], [CreatedBy], [CreatedDate]) 
	VALUES (30, 182, N'ShowSelectAll', N'True', @UserId, CAST(N'2020-04-27T09:02:50.730' AS DateTime))

	SET IDENTITY_INSERT [UserManagement].[ControlAttribute] OFF

	SET IDENTITY_INSERT [UserManagement].[ProductPage] ON 
	
	INSERT [UserManagement].[ProductPage] ([ProductPageId], [ProductId], [DisplayName], [CreatedBy], [CreatedDate], [IsActive]) 
	VALUES (12, 15, N'Renters Insurance Product Access', @UserId, CAST(N'2020-04-27T09:02:50.730' AS DateTime), 1)
	
	SET IDENTITY_INSERT [UserManagement].[ProductPage] OFF
	
	SET IDENTITY_INSERT [UserManagement].[ProductPageControl] ON 
	
	INSERT [UserManagement].[ProductPageControl] ([ProductPageControlId], [ProductPageId], [ControlId], [CreatedBy], [CreatedDate]) 
	VALUES (21, 12, 180, @UserId, CAST(N'2020-04-27T09:02:50.730' AS DateTime))
	
	SET IDENTITY_INSERT [UserManagement].[ProductPageControl] OFF

END

SELECT @ProductId = 26

IF NOT EXISTS (SELECT TOP 1 1 FROM[UserManagement].[ProductPage] WHERE ProductId = @ProductId)
BEGIN
	SET IDENTITY_INSERT [UserManagement].[Control] ON 

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate]) 
	VALUES (190, NULL, 8, N'UnifiedAmenitiesProductAccessTabGroupUIId', NULL, NULL, 1, @UserId, @Now)

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate]) 
	VALUES (191, 190, 9, N'UnifiedAmenitiesProductAccessPropertiesTabUIId', N'Properties', NULL, 1, @UserId, @Now)

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate]) 
	VALUES (192, 191, 3, N'UnifiedAmenitiesProductAccessPropertiesMultiSelectGridUIId', NULL, NULL, 1, @UserId, @Now)

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate]) 
	VALUES (193, 192, 10, N'UnifiedAmenitiesProductAccessCheckboxUIId', NULL, N'isAssigned', 1, @UserId, @Now)

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate]) 
	VALUES (194, 192, 5, N'UnifiedAmenitiesProductAccessPropertyLabelUIId', N'Property', N'name', 2, @UserId, @Now)

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate]) 
	VALUES (195, 192, 5, N'UnifiedAmenitiesProductAccessStateLabelUIId', N'State', N'state', 3, @UserId, @Now)

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate]) 
	VALUES (196, 190, 9, N'UnifiedAmenitiesProductAccessRolesTabUIId', N'Roles', NULL, 2, @UserId, @Now)

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate]) 
	VALUES (197, 196, 2, N'UnifiedAmenitiesProductAccessRolesSelectGridUIId', NULL, NULL, 1, @UserId, @Now)

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate]) 
	VALUES (198, 197, 7, N'UnifiedAmenitiesProductAccessRadioUIId', NULL, N'isAssigned', 1, @UserId, @Now)

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate]) 
	VALUES (199, 197, 5, N'UnifiedAmenitiesProductAccessRoleLabelUIId', N'Role', N'name', 2, @UserId, @Now)

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate]) 
	VALUES (200, 197, 5, N'UnifiedAmenitiesProductAccessRoleTypeLabelUIId', N'Role Type', N'roletype', 3, @UserId, @Now)

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate]) 
	VALUES (201, 197, 11, N'UnifiedAmenitiesProductAccessIconUIId', NULL, N'InfoIcon', 4, @UserId, @Now)

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate]) 
	VALUES (202, 201, 5, N'UnifiedAmenitiesProductAccessRoleDetailsLabelUIId', N'Role Details', NULL, 1, @UserId, @Now)

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate]) 
	VALUES (203, 201, 12, N'UnifiedAmenitiesProductAccessGridUIId', NULL, NULL, 2, @UserId, @Now)

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate]) 
	VALUES (204, 203, 5, N'UnifiedAmenitiesProductAccessRightLabelUIId', N'Right', N'description', 1, @UserId, @Now)

	
	SET IDENTITY_INSERT [UserManagement].[Control] OFF
	
	SET IDENTITY_INSERT [UserManagement].[ControlAttribute] ON 
	
	INSERT [UserManagement].[ControlAttribute] ([ControlAttributeId], [ControlId], [Key], [Value], [CreatedBy], [CreatedDate]) 
	VALUES (32, 192, N'ShowSelectAll', N'True', @UserId, @Now)

	INSERT [UserManagement].[ControlAttribute] ([ControlAttributeId], [ControlId], [Key], [Value], [CreatedBy], [CreatedDate]) 
	VALUES (33, 196, N'Default', N'True', @UserId, @Now)

	INSERT [UserManagement].[ControlAttribute] ([ControlAttributeId], [ControlId], [Key], [Value], [CreatedBy], [CreatedDate]) 
	VALUES (34, 197, N'Default', N'False', @UserId, @Now)

	INSERT [UserManagement].[ControlAttribute] ([ControlAttributeId], [ControlId], [Key], [Value], [CreatedBy], [CreatedDate]) 
	VALUES (35, 201, N'InfoIcon', N'Slide', @UserId, @Now)

	SET IDENTITY_INSERT [UserManagement].[ControlAttribute] OFF

	SET IDENTITY_INSERT [UserManagement].[ProductPage] ON 
	
	INSERT [UserManagement].[ProductPage] ([ProductPageId], [ProductId], [DisplayName], [CreatedBy], [CreatedDate], [IsActive]) 
	VALUES (13, 26, N'Unified Amenities Product Access', @UserId, @Now, 1)
	
	SET IDENTITY_INSERT [UserManagement].[ProductPage] OFF
	
	SET IDENTITY_INSERT [UserManagement].[ProductPageControl] ON 
	
	INSERT [UserManagement].[ProductPageControl] ([ProductPageControlId], [ProductPageId], [ControlId], [CreatedBy], [CreatedDate]) 
	VALUES (22, 13, 190, @UserId, @Now)
	
	SET IDENTITY_INSERT [UserManagement].[ProductPageControl] OFF

END