
DECLARE @UserId bigint,
	@ProductId int =17,
	@Now datetime = GETDATE();

SELECT	@UserId = UserId
FROM	Ident.UserLogin
WHERE	LoginName LIKE 'realpagead@%'

IF NOT EXISTS (SELECT TOP 1 1 FROM [UserManagement].[Control] WHERE ControlId = 399 )
BEGIN

	SET IDENTITY_INSERT [UserManagement].[Control] ON 

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate]) 
	VALUES (399, NULL, 8, N'ResidentPortalTabGroupUIId', NULL, NULL, 1, @UserId, @Now)

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate]) 
	VALUES (400, 399, 9, N'ResidentPortalPropertiesTabUIId', N'Properties', NULL, 1, @UserId, @Now)

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate]) 
	VALUES (401, 400, 3, N'ResidentPortalMultiSelectUIId', NULL, N'ResPortalMultiSelectDataSource', 1, @UserId, @Now)

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate]) 
	VALUES (402, 401, 10, N'ResidentPortalPropertyLabelforColumnUIId', NULL, N'isAssigned', 1, @UserId, @Now)

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate]) 
	VALUES (403, 401, 5, N'ResidentPortalMultiselectColumnUIId', N'Property', N'name', 2, @UserId, @Now)

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate]) 
	VALUES (404, 401, 5, N'ResidentPortalMultiselectColumnUIId', N'State', N'state', 3, @UserId, @Now)

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate]) 
	VALUES (405, 399, 9, N'ResidentPortalRolesTabUIId', N'Roles', NULL, 2, @UserId, @Now)

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate]) 
	VALUES (406, 405, 2, N'ResidentPortalRoleSelectGrid', NULL, N'RoleSelectGridDataSource', 1, @UserId, @Now)

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate]) 
	VALUES (407, 406, 7, N'ResidentPortalRoleSelectColumnUIId', NULL, N'isAssigned', 2, @UserId, @Now)

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate]) 
	VALUES (408, 406, 5, N'ResidentPortalRoleSelectColumnLabel', N'Role', N'name', 3, @UserId, @Now)

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate]) 
	VALUES (409, 405, 1, N'ResidentPortalRolesSwitchUIId', N'Assign access to current and new properties automatically', N'allProperties', 1, @UserId, @Now)

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate]) 
	VALUES (410, 399, 9, N'ResidentPortalMessagingTabUIId', N'Messaging Groups', NULL, 3, @UserId, @Now)

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate]) 
	VALUES (411, 410, 3, N'ResidentPortalMessagingGroupMultiselectGridUIId', NULL, N'MessagingGroupMuliselectGridDataSource', 1, @UserId, @Now)

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate]) 
	VALUES (412, 411, 10, N'ResidentPortalMessagingGroupColumn', NULL, N'isAssigned', 1, @UserId, @Now)

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate]) 
	VALUES (413, 411, 5, N'ResidentPortalMessagingGroupLabel', N'Group', N'name', 2, @UserId, @Now)

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate]) 
	VALUES (414, 399, 9, N'ResidentPortalNotificationTabUIId', N'Notifications', NULL, 4, @UserId, @Now)

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate]) 
	VALUES (415, 414, 1, N'ResidentPortalNotificationFrontDeskToggleUIId', N'Front desk instructions', N'managerFdiViaEmail', 1, @UserId, @Now)

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate]) 
	VALUES (416, 414, 1, N'ResidentPortalNotificationNewAmenityToggleUIId', N'New amernity reservation', N'amenitiesViaEmail', 2, @UserId, @Now)

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate]) 
	VALUES (417, 414, 1, N'ResidentPortalNotificationServiceRequestUIId', N'Service request submission & updates', N'managerMrViaEmail', 3, @UserId, @Now)

	SET IDENTITY_INSERT [UserManagement].[Control] OFF

	SET IDENTITY_INSERT [UserManagement].[ControlAttribute] ON 

	INSERT [UserManagement].[ControlAttribute] ([ControlAttributeId], [ControlId], [Key], [Value], [CreatedBy], [CreatedDate]) 
	VALUES (88, 405, N'Default', N'True', @UserId, @Now)

	INSERT [UserManagement].[ControlAttribute] ([ControlAttributeId], [ControlId], [Key], [Value], [CreatedBy], [CreatedDate]) 
	VALUES (89, 401, N'ShowSelectAll', N'True', @UserId, @Now)

	INSERT [UserManagement].[ControlAttribute] ([ControlAttributeId], [ControlId], [Key], [Value], [CreatedBy], [CreatedDate]) 
	VALUES (90, 411, N'ShowSelectAll', N'True', @UserId, @Now)

	INSERT [UserManagement].[ControlAttribute] ([ControlAttributeId], [ControlId], [Key], [Value], [CreatedBy], [CreatedDate]) 
	VALUES (91, 400, N'Hide', N'False', @UserId, @Now)

	INSERT [UserManagement].[ControlAttribute] ([ControlAttributeId], [ControlId], [Key], [Value], [CreatedBy], [CreatedDate]) 
	VALUES (92, 410, N'Hide', N'True', @UserId, @Now)

	INSERT [UserManagement].[ControlAttribute] ([ControlAttributeId], [ControlId], [Key], [Value], [CreatedBy], [CreatedDate]) 
	VALUES (93, 414, N'Hide', N'True', @UserId, @Now)

	INSERT [UserManagement].[ControlAttribute] ([ControlAttributeId], [ControlId], [Key], [Value], [CreatedBy], [CreatedDate]) 
	VALUES (94, 409, N'Default', N'True', @UserId, @Now)

	SET IDENTITY_INSERT [UserManagement].[ControlAttribute] OFF

	SET IDENTITY_INSERT [UserManagement].[ControlDependency] ON 

	INSERT [UserManagement].[ControlDependency] ([ControlDependencyId], [MasterControlId], [SlaveControlID], [MasterControlValue], [ComparatorId], [CreatedBy], [CreatedDate]) 
	VALUES (28, 408, 400, N'Staff Limited', 1, @UserId, @Now)

	INSERT [UserManagement].[ControlDependency] ([ControlDependencyId], [MasterControlId], [SlaveControlID], [MasterControlValue], [ComparatorId], [CreatedBy], [CreatedDate]) 
	VALUES (29, 408, 405, N'Staff Limited', 1, @UserId, @Now)

	INSERT [UserManagement].[ControlDependency] ([ControlDependencyId], [MasterControlId], [SlaveControlID], [MasterControlValue], [ComparatorId], [CreatedBy], [CreatedDate]) 
	VALUES (30, 408, 410, N'Staff Limited', 1, @UserId, @Now)

	INSERT [UserManagement].[ControlDependency] ([ControlDependencyId], [MasterControlId], [SlaveControlID], [MasterControlValue], [ComparatorId], [CreatedBy], [CreatedDate]) 
	VALUES (31, 408, 414, N'Staff Limited', 1, @UserId, @Now)

	INSERT [UserManagement].[ControlDependency] ([ControlDependencyId], [MasterControlId], [SlaveControlID], [MasterControlValue], [ComparatorId], [CreatedBy], [CreatedDate]) 
	VALUES (32, 408, 405, N'Enterprise Admin', 1, @UserId, @Now)

	INSERT [UserManagement].[ControlDependency] ([ControlDependencyId], [MasterControlId], [SlaveControlID], [MasterControlValue], [ComparatorId], [CreatedBy], [CreatedDate]) 
	VALUES (33, 408, 400, N'Enterprise Standard', 1, @UserId, @Now)

	INSERT [UserManagement].[ControlDependency] ([ControlDependencyId], [MasterControlId], [SlaveControlID], [MasterControlValue], [ComparatorId], [CreatedBy], [CreatedDate]) 
	VALUES (34, 408, 405, N'Enterprise Standard', 1, @UserId, @Now)

	INSERT [UserManagement].[ControlDependency] ([ControlDependencyId], [MasterControlId], [SlaveControlID], [MasterControlValue], [ComparatorId], [CreatedBy], [CreatedDate]) 
	VALUES (35, 408, 400, N'Staff Admin', 1, @UserId, @Now)

	INSERT [UserManagement].[ControlDependency] ([ControlDependencyId], [MasterControlId], [SlaveControlID], [MasterControlValue], [ComparatorId], [CreatedBy], [CreatedDate]) 
	VALUES (36, 408, 405, N'Staff Admin', 1, @UserId, @Now)

	INSERT [UserManagement].[ControlDependency] ([ControlDependencyId], [MasterControlId], [SlaveControlID], [MasterControlValue], [ComparatorId], [CreatedBy], [CreatedDate]) 
	VALUES (37, 408, 410, N'Staff Admin', 1, @UserId, @Now)

	INSERT [UserManagement].[ControlDependency] ([ControlDependencyId], [MasterControlId], [SlaveControlID], [MasterControlValue], [ComparatorId], [CreatedBy], [CreatedDate]) 
	VALUES (38, 408, 414, N'Staff Admin', 1, @UserId, @Now)

	INSERT [UserManagement].[ControlDependency] ([ControlDependencyId], [MasterControlId], [SlaveControlID], [MasterControlValue], [ComparatorId], [CreatedBy], [CreatedDate]) 
	VALUES (39, 408, 400, N'Staff Standard', 1, @UserId, @Now)

	INSERT [UserManagement].[ControlDependency] ([ControlDependencyId], [MasterControlId], [SlaveControlID], [MasterControlValue], [ComparatorId], [CreatedBy], [CreatedDate]) 
	VALUES (40, 408, 405, N'Staff Standard', 1, @UserId, @Now)

	INSERT [UserManagement].[ControlDependency] ([ControlDependencyId], [MasterControlId], [SlaveControlID], [MasterControlValue], [ComparatorId], [CreatedBy], [CreatedDate]) 
	VALUES (41, 408, 410, N'Staff Standard', 1, @UserId, @Now)

	INSERT [UserManagement].[ControlDependency] ([ControlDependencyId], [MasterControlId], [SlaveControlID], [MasterControlValue], [ComparatorId], [CreatedBy], [CreatedDate]) 
	VALUES (42, 408, 414, N'Staff Standard', 1, @UserId, @Now)

	INSERT [UserManagement].[ControlDependency] ([ControlDependencyId], [MasterControlId], [SlaveControlID], [MasterControlValue], [ComparatorId], [CreatedBy], [CreatedDate]) 
	VALUES (43, 408, 409, N'Enterprise Standard', 1, @UserId, @Now)

	SET IDENTITY_INSERT [UserManagement].[ControlDependency] OFF
END

IF NOT EXISTS (SELECT TOP 1 1 FROM[UserManagement].[ProductPage] WHERE ProductId = @ProductId)
BEGIN
	SET IDENTITY_INSERT [UserManagement].[ProductPage] ON 

	INSERT [UserManagement].[ProductPage] ([ProductPageId], [ProductId], [DisplayName], [CreatedBy], [CreatedDate], [IsActive]) 
	VALUES (25, 17, N'Resident Portals Product Access', @UserId, @Now, 1)

	SET IDENTITY_INSERT [UserManagement].[ProductPage] OFF​
END

declare @productPageId int;
SELECT @productPageId = ProductPageId FROM [UserManagement].[ProductPage] WHERE ProductId = @ProductId

IF NOT EXISTS (SELECT TOP 1 1 FROM[UserManagement].[ProductPageControl] WHERE ControlId = 399)
BEGIN

	SET IDENTITY_INSERT [UserManagement].[ProductPageControl] ON 

	INSERT [UserManagement].[ProductPageControl] ([ProductPageControlId], [ProductPageId], [ControlId], [CreatedBy], [CreatedDate]) 
	VALUES (14, 5, 399, @UserId, @Now)

	SET IDENTITY_INSERT [UserManagement].[ProductPageControl] OFF
END
ELSE
BEGIN
	UPDATE [UserManagement].[ProductPageControl] SET ControlId = 399 WHERE ProductPageId = @productPageId;
END

SELECT @ProductId = 26

IF EXISTS (SELECT TOP 1 1 FROM[UserManagement].[ProductPage] WHERE ProductId = @ProductId)
BEGIN
	IF NOT EXISTS (SELECT TOP 1 1 FROM [UserManagement].[ControlDependency] WHERE MasterControlValue IN ('Manage Amenity No Pricing','Manage Amenity With Pricing'))
	BEGIN
		SET IDENTITY_INSERT [UserManagement].[ControlDependency] ON 

			INSERT [UserManagement].[ControlDependency] ([ControlDependencyId], [MasterControlId], [SlaveControlID], [MasterControlValue], [ComparatorId], [CreatedBy], [CreatedDate]) 
			VALUES (20, 199, 196, N'Manage Amenity No Pricing', 1, @UserId, @Now)
			INSERT [UserManagement].[ControlDependency] ([ControlDependencyId], [MasterControlId], [SlaveControlID], [MasterControlValue], [ComparatorId], [CreatedBy], [CreatedDate]) 
			VALUES (21, 199, 196, N'Manage Amenity With Pricing', 1, @UserId, @Now)

		 SET IDENTITY_INSERT [UserManagement].[ControlDependency] OFF
	 END
END

SELECT @ProductId = 18

IF NOT EXISTS (SELECT TOP 1 1 FROM[UserManagement].[ProductPage] WHERE ProductId = @ProductId)
BEGIN
       
SET IDENTITY_INSERT [UserManagement].[Control] ON 

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
	VALUES (380, NULL, 8, N'UtilityManagementProductAccessTabGroupUIId', NULL, NULL, 1, @UserId, @Now)
	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate]) 
	VALUES (381, 380, 9, N'UtilityManagementProductAccessRolesTabUIId', N'Roles', NULL, 1, @UserId, @Now)
	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
	VALUES (382, 381, 2, N'UtilityManagementProductAccessRolesSelectGridUIId', NULL, NULL, 1, @UserId, @Now)
	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
	VALUES (383, 382, 7, N'UtilityManagementProductAccessRadioUIId', NULL, N'isAssigned', 1, @UserId, @Now)
	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
	VALUES (384, 382, 5, N'UtilityManagementProductAccessRoleLabelUIId', N'Role', N'name', 2, @UserId, @Now)
	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
	VALUES (385, 380, 9, N'UtilityManagementProductAccessPropertiesTabUIId', N'Properties', NULL, 2, @UserId, @Now)
	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
	VALUES (386, 385, 3, N'UtilityManagementProductAccessPropertiesMultiSelectGridUIId', NULL, NULL, 1, @UserId, @Now)
	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
	VALUES (387, 386, 10, N'UtilityManagementProductAccessCheckboxUIId', NULL, N'isAssigned', 1, @UserId, @Now)
	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
	VALUES (388, 386, 5, N'UtilityManagementProductAccessPropertyLabelUIId', N'Property', N'name', 2, @UserId, @Now)
	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
	VALUES (389, 386, 5, N'UtilityManagementProductAccessStateLabelUIId', N'State', N'state', 3, @UserId, @Now)
	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
	VALUES (390, 380, 9, N'UtilityManagementProductAccessPropertyGroupTabUIId', N'Property Group', NULL, 3, @UserId, @Now)
	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
	VALUES (391, 390, 3, N'UtilityManagementProductAccessPropertyGroupMultiSelectGridUIId', NULL, NULL, 1, @UserId, @Now)
	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
	VALUES (392, 391, 10, N'UtilityManagementProductAccessCheckboxUIId', NULL, N'isAssigned', 1, @UserId, @Now)
	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
	VALUES (393, 391, 5, N'UtilityManagementProductAccessPropertyGroupLabelUIId', N'Property Group', N'name', 2, @UserId, @Now)
	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
	VALUES (394, 380, 9, N'UtilityManagementProductAccessAdditionalRightsTabUIId', N'Additional Rights', NULL, 4, @UserId, @Now)
	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
	VALUES (395, 394, 3, N'UtilityManagementProductAccessAdditionalRightsMultiSelectGridUIId', NULL, NULL, 1, @UserId, @Now)
	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
	VALUES (396, 395, 10, N'UtilityManagementProductAccessCheckboxUIId', NULL, N'isAssigned', 1, @UserId, @Now)
	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate]) 
	VALUES (397, 395, 5, N'UtilityManagementProductAccessRightLabelUIId', N'Right', N'roleName', 2, @UserId, @Now)
	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
	VALUES (398, 395, 5, N'UtilityManagementProductAccessDescriptionLabelUIId', N'Description', N'roleDescription', 3, @UserId, @Now)

	SET IDENTITY_INSERT [UserManagement].[Control] OFF

	SET IDENTITY_INSERT [UserManagement].[ControlAttribute] ON 
	INSERT [UserManagement].[ControlAttribute] ([ControlAttributeId], [ControlId], [Key], [Value], [CreatedBy], [CreatedDate])
	VALUES (81, 381, N'Hide', N'False', @UserId, @Now)
	INSERT [UserManagement].[ControlAttribute] ([ControlAttributeId], [ControlId], [Key], [Value], [CreatedBy], [CreatedDate])
	VALUES (82, 385, N'Hide', N'True', @UserId, @Now)
	INSERT [UserManagement].[ControlAttribute] ([ControlAttributeId], [ControlId], [Key], [Value], [CreatedBy], [CreatedDate])
	VALUES (83, 386, N'ShowSelectAll', N'True', @UserId, @Now)
	INSERT [UserManagement].[ControlAttribute] ([ControlAttributeId], [ControlId], [Key], [Value], [CreatedBy], [CreatedDate])
	VALUES (84, 390, N'Hide', N'True', @UserId, @Now)
	INSERT [UserManagement].[ControlAttribute] ([ControlAttributeId], [ControlId], [Key], [Value], [CreatedBy], [CreatedDate])
	VALUES (85, 391, N'ShowSelectAll', N'True', @UserId, @Now)
	INSERT [UserManagement].[ControlAttribute] ([ControlAttributeId], [ControlId], [Key], [Value], [CreatedBy], [CreatedDate])
	VALUES (86, 395, N'ShowSelectAll', N'True', @UserId, @Now)
	INSERT [UserManagement].[ControlAttribute] ([ControlAttributeId], [ControlId], [Key], [Value], [CreatedBy], [CreatedDate])
	VALUES (87, 381, N'Default', N'True', @UserId, @Now)
 
	SET IDENTITY_INSERT [UserManagement].[ControlAttribute] OFF
	SET IDENTITY_INSERT [UserManagement].[ControlDependency] ON 

	INSERT [UserManagement].[ControlDependency] ([ControlDependencyId], [MasterControlId], [SlaveControlID], [MasterControlValue], [ComparatorId], [CreatedBy], [CreatedDate]) 
	VALUES (22, 384, 381, N'Property Manager', 1, @UserId, @Now)
	INSERT [UserManagement].[ControlDependency] ([ControlDependencyId], [MasterControlId], [SlaveControlID], [MasterControlValue], [ComparatorId], [CreatedBy], [CreatedDate]) 
	VALUES (23, 384, 385, N'Property Manager', 1, @UserId, @Now)
	INSERT [UserManagement].[ControlDependency] ([ControlDependencyId], [MasterControlId], [SlaveControlID], [MasterControlValue], [ComparatorId], [CreatedBy], [CreatedDate]) 
	VALUES (24, 384, 394, N'Property Manager', 1, @UserId, @Now)
	INSERT [UserManagement].[ControlDependency] ([ControlDependencyId], [MasterControlId], [SlaveControlID], [MasterControlValue], [ComparatorId], [CreatedBy], [CreatedDate])
	VALUES (25, 384, 381, N'Group Manager', 1, @UserId, @Now)
	INSERT [UserManagement].[ControlDependency] ([ControlDependencyId], [MasterControlId], [SlaveControlID], [MasterControlValue], [ComparatorId], [CreatedBy], [CreatedDate])
	VALUES (26, 384, 390, N'Group Manager', 1, @UserId, @Now)
	INSERT [UserManagement].[ControlDependency] ([ControlDependencyId], [MasterControlId], [SlaveControlID], [MasterControlValue], [ComparatorId], [CreatedBy], [CreatedDate])
	VALUES (27, 384, 394, N'Group Manager', 1, @UserId, @Now)

	SET IDENTITY_INSERT [UserManagement].[ControlDependency] OFF
	SET IDENTITY_INSERT [UserManagement].[ProductPage] ON 

	INSERT [UserManagement].[ProductPage] ([ProductPageId], [ProductId], [DisplayName], [CreatedBy], [CreatedDate], [IsActive])
	VALUES (25, 18, N'Utility Management Product Access', @UserId, @Now, 1)
 
	SET IDENTITY_INSERT [UserManagement].[ProductPage] OFF
	SET IDENTITY_INSERT [UserManagement].[ProductPageControl] ON 

	INSERT [UserManagement].[ProductPageControl] ([ProductPageControlId], [ProductPageId], [ControlId], [CreatedBy], [CreatedDate]) 
	VALUES (34, 25, 380, @UserId, @Now)

	SET IDENTITY_INSERT [UserManagement].[ProductPageControl] OFF    

END

SELECT @ProductId = 13

IF NOT EXISTS (SELECT TOP 1 1 FROM[UserManagement].[ProductPage] WHERE ProductId = @ProductId)
BEGIN
    SET IDENTITY_INSERT [UserManagement].[Control] ON 

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate]) 
	VALUES (418, NULL, 8, N'SpendManagementProductAccessTabGroupUIId', NULL, NULL, 1, @UserId, @Now)

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate]) 
	VALUES (419, 418, 9, N'SpendManagementProductAccessRolesTabUIId', N'Roles', NULL, 1, @UserId, @Now)

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate]) 
	VALUES (420, 419, 2, N'SpendManagementProductAccessRolesSelectGridUIId', NULL, NULL, 1, @UserId, @Now)

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate]) 
	VALUES (421, 420, 7, N'SpendManagementProductAccessRadioUIId', NULL, N'isAssigned', 1, @UserId, @Now)

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate]) 
	VALUES (422, 420, 5, N'SpendManagementProductAccessRoleLabelUIId', N'Role', N'name', 2, @UserId, @Now)

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate]) 
	VALUES (423, 418, 9, N'SpendManagementProductAccessPropertyGroupsTabUIId', N'Property Group', NULL, 2, @UserId, @Now)

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate]) 
	VALUES (424, 423, 2, N'SpendManagementProductAccessPropertyGroupsSelectGridUIId', NULL, NULL, 1, @UserId, @Now)

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate]) 
	VALUES (425, 424, 7, N'SpendManagementProductAccessRadioUIId', NULL, N'isAssigned', 1, @UserId, @Now)

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate]) 
	VALUES (426, 424, 5, N'SpendManagementProductAccessPropertyGroupLabelUIId', N'Property Group', N'name', 2, @UserId, @Now)

	SET IDENTITY_INSERT [UserManagement].[Control] OFF

	SET IDENTITY_INSERT [UserManagement].[ControlAttribute] ON 

	INSERT [UserManagement].[ControlAttribute] ([ControlAttributeId], [ControlId], [Key], [Value], [CreatedBy], [CreatedDate]) 
	VALUES (95, 420, N'ShowSelectAll', N'False', @UserId, @Now)

	INSERT [UserManagement].[ControlAttribute] ([ControlAttributeId], [ControlId], [Key], [Value], [CreatedBy], [CreatedDate]) 
	VALUES (96, 424, N'ShowSelectAll', N'False', @UserId, @Now)

	INSERT [UserManagement].[ControlAttribute] ([ControlAttributeId], [ControlId], [Key], [Value], [CreatedBy], [CreatedDate]) 
	VALUES (97, 419, N'Default', N'True', @UserId, @Now)

	SET IDENTITY_INSERT [UserManagement].[ControlAttribute] OFF

	SET IDENTITY_INSERT [UserManagement].[ProductPage] ON 

	INSERT [UserManagement].[ProductPage] ([ProductPageId], [ProductId], [DisplayName], [CreatedBy], [CreatedDate], [IsActive]) 
	VALUES (26, 13, N'Spend Management Product Access', @UserId, @Now, 1)

	SET IDENTITY_INSERT [UserManagement].[ProductPage] OFF

	SET IDENTITY_INSERT [UserManagement].[ProductPageControl] ON 

	INSERT [UserManagement].[ProductPageControl] ([ProductPageControlId], [ProductPageId], [ControlId], [CreatedBy], [CreatedDate]) 
	VALUES (35, 26, 418, @UserId, @Now)

	SET IDENTITY_INSERT [UserManagement].[ProductPageControl] OFF    

END

SELECT @ProductId = 16

IF NOT EXISTS (SELECT TOP 1 1 FROM[UserManagement].[ProductPage] WHERE ProductId = @ProductId)
BEGIN
		SET IDENTITY_INSERT [UserManagement].[Control] ON 

    	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate]) 
		VALUES (427, NULL, 8, N'VendorCredentialiProductAccessTabGroupUIId', NULL, NULL, 1, @UserId, @Now)

		INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate]) 
		VALUES (428, 427, 9, N'VendorCredentialiProductAccessRolesTabUIId', N'Roles', NULL, 1, @UserId, @Now)

		INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate]) 
		VALUES (429, 428, 5, N'VendorCredentialiProductAccessAccessTypeRolesLabelUIId', N'Access Type', NULL, 1, @UserId, @Now)

		INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate]) 
		VALUES (430, 428, 7, N'VendorCredentialiProductAccessSpecificPropertyRolesRadioUIId', N'Specific Property', N'property', 2, @UserId, @Now)

		INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate]) 
		VALUES (431, 428, 7, N'VendorCredentialiProductAccessPropertyGroupRolesRadioUIId', N'Property Group', N'propertyGroup', 3, @UserId, @Now)

		INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate]) 
		VALUES (432, 428, 7, N'VendorCredentialiProductAccessAllPropertiesRolesRadioUIId', N'All Properties', N'allProperties', 4, @UserId, @Now)

		INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate]) 
		VALUES (433, 428, 3, N'VendorCredentialiProductAccessRolesMultiSelectGridUIId', NULL, NULL, 5, @UserId, @Now)

		INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate]) 
		VALUES (434, 433, 10, N'VendorCredentialiProductAccessCheckboxUIId', NULL, N'isAssigned', 1, @UserId, @Now)

		INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate]) 
		VALUES (435, 433, 5, N'VendorCredentialiProductAccessRoleLabelUIId', N'Role', N'name', 2, @UserId, @Now)

		INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate]) 
		VALUES (436, 433, 5, N'VendorCredentialiProductAccessDescriptionLabelUIId', N'Description', N'description', 3, @UserId, @Now)

		INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate]) 
		VALUES (437, 427, 9, N'VendorCredentialiProductAccessPropertiesTabUIId', N'Properties', NULL, 2, @UserId, @Now)

		INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate]) 
		VALUES (438, 437, 3, N'VendorCredentialiProductAccessPropertiesMultiSelectGridUIId', NULL, NULL, 1, @UserId, @Now)

		INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate]) 
		VALUES (439, 438, 10, N'VendorCredentialiProductAccessCheckboxUIId', NULL, N'isAssigned', 1, @UserId, @Now)

		INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate]) 
		VALUES (440, 438, 5, N'VendorCredentialiProductAccessPropertyLabelUIId', N'Property', N'name', 2, @UserId, @Now)

		INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate]) 
		VALUES (441, 438, 5, N'VendorCredentialiProductAccessStateLabelUIId', N'State', N'state', 3, @UserId, @Now)

		INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate]) 
		VALUES (442, 427, 9, N'VendorCredentialiProductAccessPropertyGroupTabUIId', N'Property Group', NULL, 3, @UserId, @Now)

		INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate]) 
		VALUES (443, 442, 2, N'VendorCredentialiProductAccessPropertyGroupSelectGridUIId', NULL, NULL, 1, @UserId, @Now)

		INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate]) 
		VALUES (444, 443, 7, N'VendorCredentialiProductAccessRadioUIId', NULL, N'isAssigned', 1, @UserId, @Now)

		INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate]) 
		VALUES (445, 443, 5, N'VendorCredentialiProductAccessPropertyGroupLabelUIId', N'Property Group', N'name', 2, @UserId, @Now)

		INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate]) 
		VALUES (446, 443, 5, N'VendorCredentialiProductAccessGroupTypeLabelUIId', N'Group Type', N'accessLevel', 3, @UserId, @Now)

		INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate]) 
		VALUES (447, 427, 9, N'VendorCredentialiProductAccessNotificationsTabUIId', N'Notifications', NULL, 4, @UserId, @Now)

		INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate]) 
		VALUES (448, 447, 12, N'VendorCredentialiProductAccessNotificationsGridUIId', NULL, NULL, 1, @UserId, @Now)

		INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate]) 
		VALUES (449, 448, 1, N'VendorCredentialiProductAccessNotifybyemailwhenanyvendor''sinsuranceisabouttoexpireSwitchUIId', N'Notify by email when any vendor''s insurance is about to expire', N'isInsuranceExpired', 1, @UserId, @Now)

		INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate]) 
		VALUES (450, 448, 1, N'VendorCredentialiProductAccessNotifybyemailwhenanyvendor''srecommendationchangesSwitchUIId', N'Notify by email when any vendor''s recommendation changes', N'isVendorRecommendationChanges', 2, @UserId, @Now)

		INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate]) 
		VALUES (451, 448, 1, N'VendorCredentialiProductAccessNotifybyemailwhenavendorisnotlinkedtoanyproperties.SwitchUIId', N'Notify by email when a vendor is not linked to any properties.', N'isVendorNotLinkedToAnyProperty', 3, @UserId, @Now)


		SET IDENTITY_INSERT [UserManagement].[Control] OFF

		SET IDENTITY_INSERT [UserManagement].[ControlAttribute] ON 

		INSERT [UserManagement].[ControlAttribute] ([ControlAttributeId], [ControlId], [Key], [Value], [CreatedBy], [CreatedDate]) 
		VALUES (98, 428, N'Default', N'True', @UserId, @Now)

		INSERT [UserManagement].[ControlAttribute] ([ControlAttributeId], [ControlId], [Key], [Value], [CreatedBy], [CreatedDate]) 
		VALUES (99, 433, N'ShowSelectAll', N'False', @UserId, @Now)

		INSERT [UserManagement].[ControlAttribute] ([ControlAttributeId], [ControlId], [Key], [Value], [CreatedBy], [CreatedDate]) 
		VALUES (100, 437, N'Default', N'False', @UserId, @Now)

		INSERT [UserManagement].[ControlAttribute] ([ControlAttributeId], [ControlId], [Key], [Value], [CreatedBy], [CreatedDate]) 
		VALUES (101, 438, N'ShowSelectAll', N'True', @UserId, @Now)

		INSERT [UserManagement].[ControlAttribute] ([ControlAttributeId], [ControlId], [Key], [Value], [CreatedBy], [CreatedDate]) 
		VALUES (102, 448, N'ShowSelectAll', N'False', @UserId, @Now)

		INSERT [UserManagement].[ControlAttribute] ([ControlAttributeId], [ControlId], [Key], [Value], [CreatedBy], [CreatedDate]) 
		VALUES (103, 442, N'Hide', N'True', @UserId, @Now)

		SET IDENTITY_INSERT [UserManagement].[ControlAttribute] OFF
		SET IDENTITY_INSERT [UserManagement].[ControlDependency] ON 

		INSERT [UserManagement].[ControlDependency] ([ControlDependencyId], [MasterControlId], [SlaveControlID], [MasterControlValue], [ComparatorId], [CreatedBy], [CreatedDate]) 
		VALUES (54, 430, 442, N'Specific Property', 1, @UserId, @Now)

		INSERT [UserManagement].[ControlDependency] ([ControlDependencyId], [MasterControlId], [SlaveControlID], [MasterControlValue], [ComparatorId], [CreatedBy], [CreatedDate]) 
		VALUES (55, 431, 437, N'Property Group', 1, @UserId, @Now)

		INSERT [UserManagement].[ControlDependency] ([ControlDependencyId], [MasterControlId], [SlaveControlID], [MasterControlValue], [ComparatorId], [CreatedBy], [CreatedDate]) 
		VALUES (56, 432, 442, N'All Properties', 1, @UserId, @Now)

		SET IDENTITY_INSERT [UserManagement].[ControlDependency] OFF
		
		SET IDENTITY_INSERT [UserManagement].[ProductPage] ON 

		INSERT [UserManagement].[ProductPage] ([ProductPageId], [ProductId], [DisplayName], [CreatedBy], [CreatedDate], [IsActive]) 
		VALUES (27, 16, N'Vendor Credentialing Product Access', @UserId, @Now, 1)

		SET IDENTITY_INSERT [UserManagement].[ProductPage] OFF

		SET IDENTITY_INSERT [UserManagement].[ProductPageControl] ON 

		INSERT [UserManagement].[ProductPageControl] ([ProductPageControlId], [ProductPageId], [ControlId], [CreatedBy], [CreatedDate]) 
		VALUES (36, 27, 427, @UserId, @Now)

		SET IDENTITY_INSERT [UserManagement].[ProductPageControl] OFF    

END

SELECT @ProductId = 44

IF NOT EXISTS (SELECT TOP 1 1 FROM[UserManagement].[ProductPage] WHERE ProductId = @ProductId)
BEGIN

	SET IDENTITY_INSERT [UserManagement].[ControlType] ON 
	INSERT [UserManagement].[ControlType] ([ControlTypeId], [Name], [Description], [CreatedBy], [CreatedDate]) 
	VALUES (14, N'Link', N'HyperLink', @UserId, @Now)
	SET IDENTITY_INSERT [UserManagement].[ControlType] OFF


	SET IDENTITY_INSERT [UserManagement].[Control] ON 
	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate]) 
	VALUES (494, NULL, 8, N'PortfolioManagementProductAccessTabGroupUIId', NULL, NULL, 1, @UserId, @Now)

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate]) 
	VALUES (495, 494, 9, N'PortfolioManagementProductAccessEntityRolesTabUIId', N'Entity Roles', NULL, 1, @UserId, @Now)

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate]) 
	VALUES (496, 495, 3, N'PortfolioManagementProductAccessEntityRolesMultiSelectGridUIId', NULL, NULL, 1, @UserId, @Now)

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate]) 
	VALUES (497, 496, 10, N'PortfolioManagementProductAccessCheckboxUIId', NULL, N'isAssigned', 1, @UserId, @Now)

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate]) 
	VALUES (498, 496, 5, N'PortfolioManagementProductAccessRoleLabelUIId', N'Role', N'name', 2, @UserId, @Now)

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate]) 
	VALUES (499, 496, 14, N'PortfolioManagementProductAccessAssignedEntitiesLinkLabelUIId', N'Assigned Entities', N'assignedProperties', 3, @UserId, @Now)

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate]) 
	VALUES (500, 499, 5, N'PortfolioManagementProductAccessAssignedEntitiesAssignedEntitiesLabelUIId', N'Entities', NULL, 1, @UserId, @Now)

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate]) 
	VALUES (501, 499, 12, N'PortfolioManagementProductAccessAssignedEntitiesMultiSelectGridUIId', NULL, NULL, 2, @UserId, @Now)

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate]) 
	VALUES (502, 501, 10, N'PortfolioManagementProductAccessCheckboxUIId', NULL, N'isAssigned', 1, @UserId, @Now)

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate]) 
	VALUES (503, 501, 5, N'PortfolioManagementProductAccessEntityLabelUIId', N'Entity', N'name', 2, @UserId, @Now)

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate]) 
	VALUES (504, 501, 5, N'PortfolioManagementProductAccessTypeLabelUIId', N'Type', N'propertyType', 3, @UserId, @Now)

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate]) 
	VALUES (505, 494, 9, N'PortfolioManagementProductAccessGlobalRolesTabUIId', N'Global Roles', NULL, 2, @UserId, @Now)

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate]) 
	VALUES (506, 505, 3, N'PortfolioManagementProductAccessGlobalRolesMultiSelectGridUIId', NULL, NULL, 1, @UserId, @Now)

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate]) 
	VALUES (507, 506, 10, N'PortfolioManagementProductAccessCheckboxUIId', NULL, N'isAssigned', 1, @UserId, @Now)

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate]) 
	VALUES (508, 506, 5, N'PortfolioManagementProductAccessRoleLabelUIId', N'Role', N'name', 2, @UserId, @Now)

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate]) 
	VALUES (509, 506, 5, N'PortfolioManagementProductAccessRoleTypeLabelUIId', N'Role Type', N'roleType', 3, @UserId, @Now)
	SET IDENTITY_INSERT [UserManagement].[Control] OFF

	SET IDENTITY_INSERT [UserManagement].[ControlAttribute] ON 
	INSERT [UserManagement].[ControlAttribute] ([ControlAttributeId], [ControlId], [Key], [Value], [CreatedBy], [CreatedDate]) 
	VALUES (122, 495, N'Default', N'True', @UserId, @Now)

	INSERT [UserManagement].[ControlAttribute] ([ControlAttributeId], [ControlId], [Key], [Value], [CreatedBy], [CreatedDate]) 
	VALUES (123, 496, N'ShowSelectAll', N'False', @UserId, @Now)

	INSERT [UserManagement].[ControlAttribute] ([ControlAttributeId], [ControlId], [Key], [Value], [CreatedBy], [CreatedDate]) 
	VALUES (124, 499, N'AssignedProperties', N'Slide', @UserId, @Now)

	INSERT [UserManagement].[ControlAttribute] ([ControlAttributeId], [ControlId], [Key], [Value], [CreatedBy], [CreatedDate]) 
	VALUES (125, 501, N'ShowSelectAll', N'True', @UserId, @Now)

	INSERT [UserManagement].[ControlAttribute] ([ControlAttributeId], [ControlId], [Key], [Value], [CreatedBy], [CreatedDate]) 
	VALUES (126, 506, N'ShowSelectAll', N'True', @UserId, @Now)
	SET IDENTITY_INSERT [UserManagement].[ControlAttribute] OFF

	SET IDENTITY_INSERT [UserManagement].[ProductPage] ON 
	INSERT [UserManagement].[ProductPage] ([ProductPageId], [ProductId], [DisplayName], [CreatedBy], [CreatedDate], [IsActive]) 
	VALUES (31, 44, N'Portfolio Management Product Access', @UserId, @Now, 1)
	SET IDENTITY_INSERT [UserManagement].[ProductPage] OFF

	SET IDENTITY_INSERT [UserManagement].[ProductPageControl] ON 
	INSERT [UserManagement].[ProductPageControl] ([ProductPageControlId], [ProductPageId], [ControlId], [CreatedBy], [CreatedDate]) 
	VALUES (40, 31, 494, @UserId, @Now)
	SET IDENTITY_INSERT [UserManagement].[ProductPageControl] OFF

END


SELECT @ProductId = 24

IF NOT EXISTS (SELECT TOP 1 1 FROM[UserManagement].[ProductPage] WHERE ProductId = @ProductId)
BEGIN


	SET IDENTITY_INSERT [UserManagement].[Control] ON 
	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate]) 
	VALUES (510, NULL, 8, N'MasterDataManagementProductAccessTabGroupUIId', NULL, NULL, 1, @UserId, @Now)

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate]) 
	VALUES (511, 510, 9, N'MasterDataManagementProductAccessRolesTabUIId', N'Roles', NULL, 2, @UserId, @Now)

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate]) 
	VALUES (512, 511, 2, N'MasterDataManagementProductAccessRolesSelectGridUIId', NULL, NULL, 1, @UserId, @Now)

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate]) 
	VALUES (513, 512, 7, N'MasterDataManagementProductAccessRadioUIId', NULL, N'isAssigned', 1, @UserId, @Now)

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate]) 
	VALUES (514, 512, 5, N'MasterDataManagementProductAccessRoleLabelUIId', N'Role', N'name', 2, @UserId, @Now)

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate]) 
	VALUES (515, 512, 5, N'MasterDataManagementProductAccessRoleTypeLabelUIId', N'Role Type', N'roletype', 3, @UserId, @Now)
	

	SET IDENTITY_INSERT [UserManagement].[Control] OFF

	SET IDENTITY_INSERT [UserManagement].[ControlAttribute] ON 
	
	INSERT [UserManagement].[ControlAttribute] ([ControlAttributeId], [ControlId], [Key], [Value], [CreatedBy], [CreatedDate]) 
	VALUES (127, 511, N'Default', N'True', @UserId, @Now)

	SET IDENTITY_INSERT [UserManagement].[ControlAttribute] OFF

	SET IDENTITY_INSERT [UserManagement].[ProductPage] ON 
	
	INSERT [UserManagement].[ProductPage] ([ProductPageId], [ProductId], [DisplayName], [CreatedBy], [CreatedDate], [IsActive]) 
	VALUES (32, 24, N'Master Data Management Product Access', @UserId, @Now, 1)
	
	SET IDENTITY_INSERT [UserManagement].[ProductPage] OFF

	SET IDENTITY_INSERT [UserManagement].[ProductPageControl] ON 
	
	INSERT [UserManagement].[ProductPageControl] ([ProductPageControlId], [ProductPageId], [ControlId], [CreatedBy], [CreatedDate]) 
	VALUES (41, 32, 510, @UserId, @Now)
	
	SET IDENTITY_INSERT [UserManagement].[ProductPageControl] OFF

END

IF EXISTS (SELECT TOP 1 1 FROM[UserManagement].[Control] WHERE ControlId = 179)
BEGIN
	UPDATE [UserManagement].[Control] SET DataSource = 'title' WHERE ControlId = 179
END
ELSE
BEGIN
	SET IDENTITY_INSERT [UserManagement].[Control] ON

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
	VALUES (179, 177, 5, N'On-SiteProductAccessRoleLabelUIId', N'Role', N'title', 2, @UserId, @Now)

	SET IDENTITY_INSERT [UserManagement].[Control] OFF
END

--Financial Suite Product Access panel
SELECT @ProductId = 8

IF NOT EXISTS (SELECT TOP 1 1 FROM[UserManagement].[ProductPage] WHERE ProductId = @ProductId)
BEGIN
	SET IDENTITY_INSERT [UserManagement].[Control] ON 

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate]) 
	VALUES (516, NULL, 8, N'FinancialSuiteProductAccessTabGroupUIId', NULL, NULL, 1, @UserId, @Now)
	
	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate]) 
	VALUES (517, 516, 5, N'FinancialSuiteProductAccessOptions:LabelUIId', N'Options:', N'', 1, @UserId, @Now)
	
	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate]) 
	VALUES (518, 516, 1, N'FinancialSuiteProductAccessAccesstoSiteSpendManagementonlySwitchUIId', N'Access to Site Spend Management only', N'hasAccessToSiteSpendManagementOnly', 2, @UserId, @Now)
	
	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate]) 
	VALUES (519, 516, 1, N'FinancialSuiteProductAccessAllowaccesstoallcurrentandfutureentitiesSwitchUIId', N'Allow access to all current and future entities', N'hasAccessToAllCurrentFutureProperties', 3, @UserId, @Now)
	
	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate]) 
	VALUES (520, 516, 1, N'FinancialSuiteProductAccessAccountingAdminSwitchUIId', N'Accounting Admin', N'isAccountingAdmin', 4, @UserId, @Now)
	
	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate]) 
	VALUES (521, 516, 9, N'FinancialSuiteProductAccessCompaniesTabUIId', N'Companies', NULL, 5, @UserId, @Now)
	
	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate]) 
	VALUES (522, 521, 3, N'FinancialSuiteProductAccessCompaniesMultiSelectGridUIId', NULL, NULL, 1, @UserId, @Now)
	
	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate]) 
	VALUES (523, 522, 10, N'FinancialSuiteProductAccessCheckboxUIId', NULL, N'isAssigned', 1, @UserId, @Now)
	
	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate]) 
	VALUES (524, 522, 5, N'FinancialSuiteProductAccessIDLabelUIId', N'ID', N'id', 2, @UserId, @Now)
	
	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate]) 
	VALUES (525, 522, 5, N'FinancialSuiteProductAccessNameLabelUIId', N'Name', N'name', 3, @UserId, @Now)
	
	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate]) 
	VALUES (526, 516, 9, N'FinancialSuiteProductAccessEntitiesTabUIId', N'Entities', NULL, 6, @UserId, @Now)
	
	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate]) 
	VALUES (527, 526, 3, N'FinancialSuiteProductAccessEntitiesMultiSelectGridUIId', NULL, NULL, 1, @UserId, @Now)
	
	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate]) 
	VALUES (528, 527, 10, N'FinancialSuiteProductAccessCheckboxUIId', NULL, N'isAssigned', 1, @UserId, @Now)
	
	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate]) 
	VALUES (529, 527, 5, N'FinancialSuiteProductAccessEntityIDLabelUIId', N'Entity ID', N'propertyId', 2, @UserId, @Now)
	
	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate]) 
	VALUES (530, 527, 5, N'FinancialSuiteProductAccessEntityNameLabelUIId', N'Entity Name', N'propertyName', 3, @UserId, @Now)
	
	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate]) 
	VALUES (531, 527, 5, N'FinancialSuiteProductAccessCompanyIDLabelUIId', N'Company ID', N'companyId', 4, @UserId, @Now)
	
	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate]) 
	VALUES (532, 527, 5, N'FinancialSuiteProductAccessCompanyNameLabelUIId', N'Company Name', N'companyName', 5, @UserId, @Now)
	
	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate]) 
	VALUES (533, 516, 9, N'FinancialSuiteProductAccessRolesTabUIId', N'Roles', NULL, 7, @UserId, @Now)
	
	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate]) 
	VALUES (534, 533, 3, N'FinancialSuiteProductAccessRolesMultiSelectGridUIId', NULL, NULL, 1, @UserId, @Now)
	
	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate]) 
	VALUES (535, 534, 10, N'FinancialSuiteProductAccessCheckboxUIId', NULL, N'isAssigned', 1, @UserId, @Now)
	
	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate]) 
	VALUES (536, 534, 5, N'FinancialSuiteProductAccessRoleLabelUIId', N'Role', N'name', 2, @UserId, @Now)
	
	SET IDENTITY_INSERT [UserManagement].[Control] OFF
	
	SET IDENTITY_INSERT [UserManagement].[ControlAttribute] ON 	
	
	INSERT [UserManagement].[ControlAttribute] ([ControlAttributeId], [ControlId], [Key], [Value], [CreatedBy], [CreatedDate]) 
	VALUES (128, 518, N'Default', N'True', @UserId, @Now)
	
	INSERT [UserManagement].[ControlAttribute] ([ControlAttributeId], [ControlId], [Key], [Value], [CreatedBy], [CreatedDate]) 
	VALUES (129, 519, N'Default', N'True', @UserId, @Now)
	
	INSERT [UserManagement].[ControlAttribute] ([ControlAttributeId], [ControlId], [Key], [Value], [CreatedBy], [CreatedDate]) 
	VALUES (130, 520, N'Default', N'True', @UserId, @Now)
	
	INSERT [UserManagement].[ControlAttribute] ([ControlAttributeId], [ControlId], [Key], [Value], [CreatedBy], [CreatedDate]) 
	VALUES (131, 521, N'Default', N'True', @UserId, @Now)
	
	INSERT [UserManagement].[ControlAttribute] ([ControlAttributeId], [ControlId], [Key], [Value], [CreatedBy], [CreatedDate]) 
	VALUES (132, 522, N'ShowSelectAll', N'True', @UserId, @Now)
	
	INSERT [UserManagement].[ControlAttribute] ([ControlAttributeId], [ControlId], [Key], [Value], [CreatedBy], [CreatedDate]) 
	VALUES (133, 527, N'ShowSelectAll', N'True', @UserId, @Now)
	
	INSERT [UserManagement].[ControlAttribute] ([ControlAttributeId], [ControlId], [Key], [Value], [CreatedBy], [CreatedDate]) 
	VALUES (134, 534, N'ShowSelectAll', N'False', @UserId, @Now)
	
	SET IDENTITY_INSERT [UserManagement].[ControlAttribute] OFF
	
	SET IDENTITY_INSERT [UserManagement].[ProductPage] ON 
	
	INSERT [UserManagement].[ProductPage] ([ProductPageId], [ProductId], [DisplayName], [CreatedBy], [CreatedDate], [IsActive]) 
	VALUES (33, 8, N'Financial Suite Product Access', @UserId, @Now, 1)
	
	SET IDENTITY_INSERT [UserManagement].[ProductPage] OFF
	
	SET IDENTITY_INSERT [UserManagement].[ProductPageControl] ON 
	
	INSERT [UserManagement].[ProductPageControl] ([ProductPageControlId], [ProductPageId], [ControlId], [CreatedBy], [CreatedDate]) 
	VALUES (42, 33, 516, @UserId, @Now)
	
	SET IDENTITY_INSERT [UserManagement].[ProductPageControl] OFF
	
END

SELECT @ProductId = 55
IF NOT EXISTS (SELECT TOP 1 1 FROM[UserManagement].[ProductPage] WHERE ProductId = @ProductId)
BEGIN
            SET IDENTITY_INSERT [UserManagement].[Control] ON 
			--Parent
			INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
			VALUES (452, NULL, 8, N'RenovationManagerProductAccessTabGroupUIId', NULL, NULL, 1, @UserId, @Now)

			--Roles
			INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
			VALUES (453, 452, 9, N'RenovationManagerProductAccessRolesTabUIId', N'Roles', NULL, 1, @UserId, @Now)

			INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
			VALUES (454, 453, 3, N'RenovationManagerProductAccessRolesSelectGridUIId', NULL, NULL, 1, @UserId, @Now)

			INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
			VALUES (455, 454, 10, N'RenovationManagerProductAccessRadioUIId', NULL, N'isAssigned', 1, @UserId, @Now)

			INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
			VALUES (456, 454, 5, N'RenovationManagerProductAccessRoleLabelUIId', N'Role', N'name', 2, @UserId, @Now)

							
			--Properties
			INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate]) 
			VALUES (457, 452, 9, N'RenovationManagerProductAccessPropertiesTabUIId', N'Properties', NULL, 3, @UserId, @Now)
				
			INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
			VALUES (458, 457, 3, N'RenovationManagerProductAccessPropertiesMultiSelectGridUIId', NULL, NULL, 1, @UserId, @Now)
				
			INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
			VALUES (459, 458, 10, N'RenovationManagerProductAccessCheckboxUIId', NULL, N'isAssigned', 1, @UserId, @Now)
				
			INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
			VALUES (460, 458, 5, N'RenovationManagerProductAccessPropertyLabelUIId', N'Property', N'name', 2, @UserId, @Now)
				
			INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate]) 
			VALUES (461, 458, 5, N'RenovationManagerProductAccessStateLabelUIId', N'State', N'state', 3, @UserId, @Now)	
				
			SET IDENTITY_INSERT [UserManagement].[Control] OFF
              
            SET IDENTITY_INSERT [UserManagement].[ControlAttribute] ON 
            INSERT [UserManagement].[ControlAttribute] ([ControlAttributeId], [ControlId], [Key], [Value], [CreatedBy], [CreatedDate])
			VALUES (104, 453, N'Default', N'True', @UserId, @Now)

			INSERT [UserManagement].[ControlAttribute] ([ControlAttributeId], [ControlId], [Key], [Value], [CreatedBy], [CreatedDate]) 
			VALUES (105, 454, N'ShowSelectAll', N'False', @UserId, @Now)
    
			INSERT [UserManagement].[ControlAttribute] ([ControlAttributeId], [ControlId], [Key], [Value], [CreatedBy], [CreatedDate]) 
			VALUES (106, 458, N'ShowSelectAll', N'True', @UserId, @Now)

            SET IDENTITY_INSERT [UserManagement].[ControlAttribute] OFF

            SET IDENTITY_INSERT [UserManagement].[ProductPage] ON 
              
			INSERT [UserManagement].[ProductPage] ([ProductPageId], [ProductId], [DisplayName], [CreatedBy], [CreatedDate], [IsActive])
			VALUES (28, 55, N'Renovation Manager Product Access', @UserId, @Now, 1)
  
            SET IDENTITY_INSERT [UserManagement].[ProductPage] OFF
              
            SET IDENTITY_INSERT [UserManagement].[ProductPageControl] ON 
            
			INSERT [UserManagement].[ProductPageControl] ([ProductPageControlId], [ProductPageId], [ControlId], [CreatedBy], [CreatedDate]) 
			VALUES (37, 28, 452, @UserId, @Now)
          
            SET IDENTITY_INSERT [UserManagement].[ProductPageControl] OFF

END

SELECT @ProductId = 47

IF NOT EXISTS (SELECT TOP 1 1 FROM[UserManagement].[ProductPage] WHERE ProductId = @ProductId)
BEGIN

	SET IDENTITY_INSERT [UserManagement].[Control] ON 

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate]) 
	VALUES (475, NULL, 8, N'DepositAlternativeProductAccessTabGroupUIId', NULL, NULL, 1, @UserId, @Now)

	
	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate]) 
	VALUES (476, 475, 9, N'DepositAlternativeProductAccessRolesTabUIId', N'Roles', NULL, 1, @UserId, @Now)

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate]) 
	VALUES (477, 476, 2, N'DepositAlternativeProductAccessRoleSelectGrid', NULL, N'RoleSelectGridDataSource', 1, @UserId, @Now)

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate]) 
	VALUES (478, 477, 7, N'DepositAlternativeProductAccessRoleSelectColumnUIId', NULL, N'isAssigned', 2, @UserId, @Now)

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate]) 
	VALUES (479, 477, 5, N'DepositAlternativeProductAccessRoleSelectColumnLabel', N'Role', N'name', 3, @UserId, @Now)

	--Properties
	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate]) 
	VALUES (480, 475, 9, N'DepositAlternativeProductAccessPropertiesTabUIId', N'Properties', NULL, 2, @UserId, @Now)

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate]) 
	VALUES (481, 480, 3, N'DepositAlternativeProductAccessMultiSelectUIId', NULL, N'ResPortalMultiSelectDataSource', 1, @UserId, @Now)

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate]) 
	VALUES (482, 481, 10, N'DepositAlternativeProductAccessPropertyLabelforColumnUIId', NULL, N'isAssigned', 1, @UserId, @Now)

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate]) 
	VALUES (483, 481, 5, N'DepositAlternativeProductAccessMultiselectColumnUIId', N'Property', N'name', 2, @UserId, @Now)

	--Areas
	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate]) 
	VALUES (484, 475, 9, N'DepositAlternativeProductAccessMessagingTabUIId', N'Areas', NULL, 3, @UserId, @Now)

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate]) 
	VALUES (485, 484, 3, N'DepositAlternativeProductAccessMessagingGroupMultiselectGridUIId', NULL, N'MessagingGroupMuliselectGridDataSource', 1, @UserId, @Now)

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate]) 
	VALUES (486, 485, 10, N'DepositAlternativeProductAccessMessagingGroupColumn', NULL, N'isAssigned', 1, @UserId, @Now)

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate]) 
	VALUES (487, 485, 5, N'DepositAlternativeProductAccessMessagingGroupLabel', N'Area', N'name', 2, @UserId, @Now)

	--Regions
	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate]) 
	VALUES (488, 475, 9, N'DepositAlternativeProductAccessMessagingTabUIId', N'Regions', NULL, 4, @UserId, @Now)

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate]) 
	VALUES (489, 488, 3, N'DepositAlternativeProductAccessMessagingGroupMultiselectGridUIId', NULL, N'MessagingGroupMuliselectGridDataSource', 1, @UserId, @Now)

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate]) 
	VALUES (490, 489, 10, N'DepositAlternativeProductAccessMessagingGroupColumn', NULL, N'isAssigned', 1, @UserId, @Now)

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate]) 
	VALUES (491, 489, 5, N'DepositAlternativeProductAccessMessagingGroupLabel', N'Group', N'name', 2, @UserId, @Now)

	--Notifications
	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate]) 
	VALUES (492, 475, 9, N'DepositAlternativeProductAccessNotificationTabUIId', N'Notifications', NULL, 5, @UserId, @Now)

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate]) 
	VALUES (493, 492, 1, N'DepositAlternativeProductAccessNotificationFrontDeskToggleUIId', N'Receive Monthly Activity Summary Report', N'canReceiveMonthlyReport', 1, @UserId, @Now)	

	SET IDENTITY_INSERT [UserManagement].[Control] OFF

	SET IDENTITY_INSERT [UserManagement].[ControlAttribute] ON 

	INSERT [UserManagement].[ControlAttribute] ([ControlAttributeId], [ControlId], [Key], [Value], [CreatedBy], [CreatedDate]) 
	VALUES (114, 476, N'Default', N'True', @UserId, @Now)

	INSERT [UserManagement].[ControlAttribute] ([ControlAttributeId], [ControlId], [Key], [Value], [CreatedBy], [CreatedDate]) 
	VALUES (115, 477, N'ShowSelectAll', N'False', @UserId, @Now)

	INSERT [UserManagement].[ControlAttribute] ([ControlAttributeId], [ControlId], [Key], [Value], [CreatedBy], [CreatedDate]) 
	VALUES (116, 481, N'ShowSelectAll', N'True', @UserId, @Now)

	INSERT [UserManagement].[ControlAttribute] ([ControlAttributeId], [ControlId], [Key], [Value], [CreatedBy], [CreatedDate]) 
	VALUES (117, 485, N'ShowSelectAll', N'True', @UserId, @Now)

	INSERT [UserManagement].[ControlAttribute] ([ControlAttributeId], [ControlId], [Key], [Value], [CreatedBy], [CreatedDate]) 
	VALUES (118, 489, N'ShowSelectAll', N'True', @UserId, @Now)

	INSERT [UserManagement].[ControlAttribute] ([ControlAttributeId], [ControlId], [Key], [Value], [CreatedBy], [CreatedDate]) 
	VALUES (119, 484, N'Hide', N'True', @UserId, @Now)

	INSERT [UserManagement].[ControlAttribute] ([ControlAttributeId], [ControlId], [Key], [Value], [CreatedBy], [CreatedDate]) 
	VALUES (120, 488, N'Hide', N'True', @UserId, @Now)

	INSERT [UserManagement].[ControlAttribute] ([ControlAttributeId], [ControlId], [Key], [Value], [CreatedBy], [CreatedDate]) 
	VALUES (121, 492, N'Hide', N'True', @UserId, @Now)

	SET IDENTITY_INSERT [UserManagement].[ControlAttribute] OFF

	SET IDENTITY_INSERT [UserManagement].[ControlDependency] ON 
	INSERT [UserManagement].[ControlDependency] ([ControlDependencyId], [MasterControlId], [SlaveControlID], [MasterControlValue], [ComparatorId], [CreatedBy], [CreatedDate]) 
	VALUES (46, 479, 492, N'Regional Manager', 1, @UserId, @Now)

	INSERT [UserManagement].[ControlDependency] ([ControlDependencyId], [MasterControlId], [SlaveControlID], [MasterControlValue], [ComparatorId], [CreatedBy], [CreatedDate]) 
	VALUES (47, 479, 492, N'Company Management', 1, @UserId, @Now)	

	INSERT [UserManagement].[ControlDependency] ([ControlDependencyId], [MasterControlId], [SlaveControlID], [MasterControlValue], [ComparatorId], [CreatedBy], [CreatedDate]) 
	VALUES (48, 479, 476, N'Leasing Agent', 1, @UserId, @Now)

	INSERT [UserManagement].[ControlDependency] ([ControlDependencyId], [MasterControlId], [SlaveControlID], [MasterControlValue], [ComparatorId], [CreatedBy], [CreatedDate]) 
	VALUES (49, 479, 476, N'Property Manager', 1, @UserId, @Now)

	INSERT [UserManagement].[ControlDependency] ([ControlDependencyId], [MasterControlId], [SlaveControlID], [MasterControlValue], [ComparatorId], [CreatedBy], [CreatedDate]) 
	VALUES (50, 479, 476, N'Area Manager', 1, @UserId, @Now)

	INSERT [UserManagement].[ControlDependency] ([ControlDependencyId], [MasterControlId], [SlaveControlID], [MasterControlValue], [ComparatorId], [CreatedBy], [CreatedDate]) 
	VALUES (51, 479, 476, N'Regional Manager', 1, @UserId, @Now)

	INSERT [UserManagement].[ControlDependency] ([ControlDependencyId], [MasterControlId], [SlaveControlID], [MasterControlValue], [ComparatorId], [CreatedBy], [CreatedDate]) 
	VALUES (52, 479, 476, N'Company Management', 1, @UserId, @Now)	

	INSERT [UserManagement].[ControlDependency] ([ControlDependencyId], [MasterControlId], [SlaveControlID], [MasterControlValue], [ComparatorId], [CreatedBy], [CreatedDate]) 
	VALUES (53, 479, 480, N'Leasing Agent', 1, @UserId, @Now)

	INSERT [UserManagement].[ControlDependency] ([ControlDependencyId], [MasterControlId], [SlaveControlID], [MasterControlValue], [ComparatorId], [CreatedBy], [CreatedDate]) 
	VALUES (57, 479, 480, N'Property Manager', 1, @UserId, @Now)

	INSERT [UserManagement].[ControlDependency] ([ControlDependencyId], [MasterControlId], [SlaveControlID], [MasterControlValue], [ComparatorId], [CreatedBy], [CreatedDate]) 
	VALUES (58, 479, 484, N'Area Manager', 1, @UserId, @Now)

	INSERT [UserManagement].[ControlDependency] ([ControlDependencyId], [MasterControlId], [SlaveControlID], [MasterControlValue], [ComparatorId], [CreatedBy], [CreatedDate]) 
	VALUES (59, 479, 488, N'Regional Manager', 1, @UserId, @Now)

	INSERT [UserManagement].[ControlDependency] ([ControlDependencyId], [MasterControlId], [SlaveControlID], [MasterControlValue], [ComparatorId], [CreatedBy], [CreatedDate]) 
	VALUES (60, 479, 492, N'Property Manager', 1, @UserId, @Now)

	INSERT [UserManagement].[ControlDependency] ([ControlDependencyId], [MasterControlId], [SlaveControlID], [MasterControlValue], [ComparatorId], [CreatedBy], [CreatedDate]) 
	VALUES (61, 479, 492, N'Area Manager', 1, @UserId, @Now)


	SET IDENTITY_INSERT [UserManagement].[ControlDependency] OFF

	SET IDENTITY_INSERT [UserManagement].[ProductPage] ON 
              
	INSERT [UserManagement].[ProductPage] ([ProductPageId], [ProductId], [DisplayName], [CreatedBy], [CreatedDate], [IsActive])
	VALUES (30, 47, N'Deposit Alternative Product Access', @UserId, @Now, 1)
  
    SET IDENTITY_INSERT [UserManagement].[ProductPage] OFF
              
    SET IDENTITY_INSERT [UserManagement].[ProductPageControl] ON 
            
	INSERT [UserManagement].[ProductPageControl] ([ProductPageControlId], [ProductPageId], [ControlId], [CreatedBy], [CreatedDate]) 
	VALUES (39, 30, 475, @UserId, @Now)
          
    SET IDENTITY_INSERT [UserManagement].[ProductPageControl] OFF
END
--SLM
--Select @ProductId = 50
--IF NOT EXISTS (SELECT TOP 1 1 FROM[UserManagement].[ProductPage] WHERE ProductId = @ProductId)
--BEGIN
	
--	SET IDENTITY_INSERT [UserManagement].[Control] ON 
--	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate]) 
--	VALUES (516, NULL, 8, N'SeniorLeadManagementProductAccessTabGroupUIId', NULL, NULL, 1, @UserId, @Now)

--	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate]) 
--	VALUES (517, 516, 9, N'SeniorLeadManagementProductAccessPropertiesTabUIId', N'Properties', NULL, 1, @UserId, @Now)

--	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate]) 
--	VALUES (518, 517, 3, N'SeniorLeadManagementProductAccessPropertiesMultiSelectGridUIId', NULL, NULL, 2, @UserId, @Now)

--	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate]) 
--	VALUES (519, 518, 10, N'SeniorLeadManagementProductAccessCheckboxUIId', NULL, N'isAssigned', 1, @UserId, @Now)

--	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate]) 
--	VALUES (520, 518, 5, N'SeniorLeadManagementProductAccessPropertyLabelUIId', N'Property', N'name', 2, @UserId, @Now)

--	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate]) 
--	VALUES (521, 518, 5, N'SeniorLeadManagementProductAccessStateLabelUIId', N'State', N'state', 3, @UserId, @Now)

--	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate]) 
--	VALUES (522, 516, 9, N'SeniorLeadManagementProductAccessRightsTabUIId', N'Rights', NULL, 2, @UserId, @Now)

--	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate]) 
--	VALUES (523, 522, 13, N'SeniorLeadManagementProductAccessSelectaPresetRoleRightsSelectUIId', N'Select a Preset Role', N'roles', 1, @UserId, @Now)

--	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate]) 
--	VALUES (524, 522, 3, N'SeniorLeadManagementProductAccessRightsMultiSelectGridUIId', NULL, NULL, 1, @UserId, @Now)

--	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate]) 
--	VALUES (525, 524, 10, N'SeniorLeadManagementProductAccessCheckboxUIId', NULL, N'isAssigned', 1, @UserId, @Now)

--	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate]) 
--	VALUES (526, 524, 5, N'SeniorLeadManagementProductAccessRightLabelUIId', N'Right', N'name', 2, @UserId, @Now)

--	SET IDENTITY_INSERT [UserManagement].[Control] OFF

--	SET IDENTITY_INSERT [UserManagement].[ControlAttribute] ON 
--	INSERT [UserManagement].[ControlAttribute] ([ControlAttributeId], [ControlId], [Key], [Value], [CreatedBy], [CreatedDate]) 
--	VALUES (128, 517, N'Default', N'True', @UserId, @Now)

--	INSERT [UserManagement].[ControlAttribute] ([ControlAttributeId], [ControlId], [Key], [Value], [CreatedBy], [CreatedDate]) 
--	VALUES (129, 518, N'ShowSelectAll', N'True', @UserId, @Now)

--	INSERT [UserManagement].[ControlAttribute] ([ControlAttributeId], [ControlId], [Key], [Value], [CreatedBy], [CreatedDate]) 
--	VALUES (130, 524, N'ShowSelectAll', N'True', @UserId, @Now)

--	SET IDENTITY_INSERT [UserManagement].[ControlAttribute] OFF

--	SET IDENTITY_INSERT [UserManagement].[ProductPage] ON 
--	INSERT [UserManagement].[ProductPage] ([ProductPageId], [ProductId], [DisplayName], [CreatedBy], [CreatedDate], [IsActive]) 
--	VALUES (33, 50, N'Senior Lead Management Product Access', @UserId, @Now, 1)
--	SET IDENTITY_INSERT [UserManagement].[ProductPage] OFF

--	SET IDENTITY_INSERT [UserManagement].[ProductPageControl] ON 
--	INSERT [UserManagement].[ProductPageControl] ([ProductPageControlId], [ProductPageId], [ControlId], [CreatedBy], [CreatedDate]) 
--	VALUES (42, 33, 516, @UserId, @Now)
--	SET IDENTITY_INSERT [UserManagement].[ProductPageControl] OFF
--END


IF EXISTS (SELECT TOP 1 1 FROM[UserManagement].[Control] WHERE ControlId = 104)
BEGIN
	UPDATE [UserManagement].[Control] SET DisplayName = 'Assign current and new properties automatically' WHERE ControlId = 104
END

Go
--Add Unified Settings Product
DECLARE @ProductID int = 56,
	@ProductTypeID int = 702,
	@ProductName nvarchar(50) = N'Unified Settings',
	@ProductGUID nvarchar(50) = N'62E1BA40-5ECD-4F11-9936-7327D6B5F7BB',
	@ProductTypeGUID nvarchar(50) = N'8068DCF5-443A-4E48-8C86-CAAA765B0ADA',
	@ParentProductTypeId int

SELECT @ParentProductTypeId = ProductTypeId
FROM	Enterprise.ProductType
WHERE	Name = 'Administration'
AND		ParentProductTypeId IS NULL;

IF NOT EXISTS (SELECT TOP 1 1 FROM enterprise.ProductType WHERE Name = @ProductName)
BEGIN
	EXEC [Enterprise].[CreateProductType] 
		@ProductTypeId = @ProductTypeID,
		@ParentProductTypeId = @ParentProductTypeId, 
		@Name = @ProductName, 
		@Description = @ProductName, 
		@ProductTypeGUID = @ProductTypeGUID
END;

--Following block will create the new prodcut in the database
IF NOT EXISTS (SELECT TOP 1 1 FROM Enterprise.Product WHERE Name = @ProductName)
BEGIN
	EXEC Enterprise.CreateProduct 
		@ProductId = @ProductId, 
		@ProductGUID = @ProductGUID,
		@Name = @ProductName, 
		@Description = @ProductName,
		@ProductTypeId = @ProductTypeID,
		@BooksProductCode = N'SET';
END
GO

DECLARE @Now DATETIME = GETUTCDATE(),
	@OrganizationPartyId bigint,
	@ProductId int,
	@TargetProductId int,
	@ActionId int,
	@RoleId int,
	@OutputRightId int,
	@UserActionId int,
	@RightValueTypeValue nvarchar(200),
	@DetaulRightName nvarchar(200),
	@RightShortName nvarchar(50),
	@RightCategory int,
	@VisibilityStatusTypeId int,
	@ConfigurationId int,
	@RoleName nvarchar(200) = N'User Administrator'

DECLARE @NewRightValueType TABLE (
	Value nvarchar(200)
)

SELECT @RightCategory = TypeId
FROM	Enterprise.RoleRightStatus
WHERE	CategoryName = 'Right Type'
AND			TypeName = 'System'

SELECT	@VisibilityStatusTypeId = TypeId
FROM	Enterprise.RoleRightStatus AS rrs
WHERE	TypeName = 'ALL'
AND			CategoryType = 'Security'

INSERT INTO @NewRightValueType (
	Value
)
VALUES (
	'View all Unified Settings'
),
(
	'Manage company-level settings'
),
(
	'Manage property-level settings'
),
(
	'View all company-level settings'
),
(
	'View all property- level settings'
)

SELECT	@OrganizationPartyId = PartyId
FROM	Enterprise.Organization
WHERE	Name = N'RealPage Employee'

SELECT	@ProductId = ProductId
FROM	Enterprise.Product
WHERE	Name = N'Unified Platform'

SELECT	@TargetProductId = ProductId
FROM	Enterprise.Product
WHERE	Name = N'Unified Settings'

IF NOT EXISTS(SELECT TOP 1 1 FROM Enterprise.GlobalProductConfiguration WHERE ProductId = @TargetProductId)
BEGIN
	EXEC Enterprise.CreateProductConfiguration @ConfigurationId OUTPUT

	EXEC Enterprise.LinkGlobalConfigurationToProduct
		@ConfigurationId = @ConfigurationId,
		@ProductId = @TargetProductId,
		@FromDate = @Now,
		@ThruDate = NULL		

	EXEC Enterprise.CreateOrganizationProduct
		@PartyId = @OrganizationPartyId,
		@ConfigurationID = @ConfigurationId,
		@ProductId = @TargetProductId,
		@FromDate = @Now,
		@ThruDate = NULL
END

DECLARE curNewRightValueType CURSOR FOR
SELECT Value
FROM @NewRightValueType

OPEN curNewRightValueType
FETCH NEXT FROM curNewRightValueType INTO @RightValueTypeValue

WHILE @@FETCH_STATUS = 0
BEGIN
	IF NOT EXISTS (SELECT TOP 1 1 FROM Enterprise.ACTION WHERE ObjectValue = @RightValueTypeValue AND ParentActionId IS NULL)
	BEGIN
		EXEC Enterprise.CreateAction
			@ProductID = @ProductId, 
			@Action = @RightValueTypeValue, 
			@ActionTarget = N'Right', 
			@ActionbValueTypeId = 1, 
			@Description = '', 
			@ActionID = @ActionId OUTPUT;
	END;
	FETCH NEXT FROM curNewRightValueType INTO @RightValueTypeValue
END
CLOSE curNewRightValueType
DEALLOCATE curNewRightValueType

DECLARE curOrganizationRight CURSOR FOR
SELECT eo.PartyId,
			x.Value
FROM	Enterprise.Organization eo
			CROSS JOIN (
				SELECT Value
				FROM @NewRightValueType
			) x
WHERE	Name = N'RealPage Employee'

OPEN curOrganizationRight
FETCH NEXT FROM curOrganizationRight INTO @OrganizationPartyId, @RightValueTypeValue

WHILE @@FETCH_STATUS = 0
BEGIN
	SET @ActionID = NULL

	SELECT @RoleId = RoleId
	FROM	Enterprise.Role ero
				INNER JOIN Enterprise.RoleValueType AS erovt 	ON (ero.RoleValueTypeId = erovt.RoleValueTypeId)
	WHERE	erovt.Value = @RoleName
	AND			ero.PartyId = @OrganizationPartyId;

	SELECT @DetaulRightName = 'Default_' + @RightValueTypeValue,
		@RightShortName = REPLACE(REPLACE(@RightValueTypeValue, ' ', ''), '-', '')

	EXEC Enterprise.CreateRight
		@RoleId = -1,
		@RightName = @DetaulRightName,
		@ShortName = @RightShortName,
		@RightCategoryId = @RightCategory,
		@PartyId = @OrganizationPartyId,
		@ProductId = @ProductId,
		@Description = '',
		@TargetProductId = @TargetProductId,
		@VisibilityStatusId = @VisibilityStatusTypeId,
		@RightId = @OutputRightId OUTPUT;

	SELECT @ActionID = ActionId
	FROM	Enterprise.Action
	WHERE	ParentActionId IS NULL 
	AND		ObjectValue = @RightValueTypeValue
	AND		ObjectType = 'Right'

	EXEC [Enterprise].[LinkActionToRights]
		@ActionID = @ActionID,
		@RightId = @OutputRightId,
		@StatusId = @VisibilityStatusTypeId,
		@UserActionId = @UserActionId OUTPUT;

	EXEC Enterprise.CreateRight
		@RoleId = @RoleId,
		@RightName = @RightValueTypeValue,
		@RightCategoryId = @RightCategory,
		@PartyId = @OrganizationPartyId,
		@ProductId = @ProductId,
		@Shortname = @RightShortName,
		@Description = @RightValueTypeValue,
		@TargetProductId = @TargetProductId,
		@VisibilityStatusId = @VisibilityStatusTypeId,
		@RightId = @OutputRightId OUTPUT;

	FETCH NEXT FROM curOrganizationRight INTO @OrganizationPartyId, @RightValueTypeValue
END
CLOSE curOrganizationRight
DEALLOCATE curOrganizationRight

UPDATE	Enterprise.RightValueType
SET			TargetProductId = @TargetProductId
WHERE		Value = 'View all Unified Settings'
AND			TargetProductId = @ProductId
GO

--Add 2 additional CIMPL Rights and Assign to Unified Platform
DECLARE @Now DATETIME = GETUTCDATE(),
	@RoleValueTypeId int,
	@OrganizationPartyId bigint,
	@ProductId int,
	@TargetProductId int,
	@ActionId int,
	@RoleId int,
	@OutputRightId int,
	@UserActionId int,
	@RightValueTypeValue nvarchar(200),
	@DetaulRightName nvarchar(200),
	@RightShortName nvarchar(50),
	@RightCategory int,
	@VisibilityStatusTypeId int,
	@ConfigurationId int,
	@RoleName nvarchar(200) = N'User Administrator'

DECLARE @NewRightValueType TABLE (
	Value nvarchar(200)
)

DECLARE @Organization TABLE (
	PartyId bigint
)

SELECT @RightCategory = TypeId
FROM	Enterprise.RoleRightStatus
WHERE	CategoryName = 'Right Type'
AND			TypeName = 'System'

SELECT	@VisibilityStatusTypeId = TypeId
FROM	Enterprise.RoleRightStatus AS rrs
WHERE	TypeName = 'ALL'
AND			CategoryType = 'Security'

INSERT INTO @NewRightValueType (
	Value
)
VALUES (
	'Ability to answer company-level questionnaires in CIMPL'
),
(
	'Manage CIMPL Templates'
)

SELECT	@ProductId = ProductId
FROM	Enterprise.Product
WHERE	Name = N'Unified Platform'

SELECT	@TargetProductId = ProductId
FROM	Enterprise.Product
WHERE	Name = N'CIMPL'

SELECT	@RoleValueTypeId = RoleValueTypeId
FROM	Enterprise.RoleValueType
WHERE	Value = @RoleName

DECLARE curNewRightValueType CURSOR FOR
SELECT Value
FROM @NewRightValueType

OPEN curNewRightValueType
FETCH NEXT FROM curNewRightValueType INTO @RightValueTypeValue

WHILE @@FETCH_STATUS = 0
BEGIN
	IF NOT EXISTS (SELECT TOP 1 1 FROM Enterprise.ACTION WHERE ObjectValue = @RightValueTypeValue AND ParentActionId IS NULL)
	BEGIN
		EXEC Enterprise.CreateAction
			@ProductID = @ProductId, 
			@Action = @RightValueTypeValue, 
			@ActionTarget = N'Right', 
			@ActionbValueTypeId = 1, 
			@Description = '', 
			@ActionID = @ActionId OUTPUT;
	END;
	FETCH NEXT FROM curNewRightValueType INTO @RightValueTypeValue
END
CLOSE curNewRightValueType
DEALLOCATE curNewRightValueType

--Companies CIMPL Enabled
INSERT INTO @Organization (
	PartyId
)
SELECT	eo.PartyId
FROM	Enterprise.OrganizationProduct eop
			INNER JOIN Enterprise.Organization eo ON (eop.PartyId = eo.PartyId)
WHERE	eop.ProductId = @TargetProductId
AND			((@NOW >= eop.FromDate AND eop.ThruDate IS NULL) OR (@NOW BETWEEN eop.FromDate AND eop.ThruDate))

DECLARE curOrganizationRight CURSOR FOR
SELECT o.PartyId,
			x.Value
FROM	@Organization o
			CROSS JOIN (
				SELECT Value
				FROM @NewRightValueType
				WHERE Value IN ('Ability to answer company-level questionnaires in CIMPL', 'Manage CIMPL Templates')
			) x

OPEN curOrganizationRight
FETCH NEXT FROM curOrganizationRight INTO @OrganizationPartyId, @RightValueTypeValue

WHILE @@FETCH_STATUS = 0
BEGIN
	SELECT @RoleId = RoleId
	FROM	Enterprise.Role ero
				INNER JOIN Enterprise.RoleValueType AS erovt 	ON (ero.RoleValueTypeId = erovt.RoleValueTypeId)
	WHERE	erovt.Value = @RoleName
	AND			ero.PartyId = @OrganizationPartyId;

	SELECT @DetaulRightName = 'Default_' + @RightValueTypeValue,
		@RightShortName = REPLACE(REPLACE(@RightValueTypeValue, ' ', ''), '-', '')

	EXEC Enterprise.CreateRight
		@RoleId = -1,
		@RightName = @DetaulRightName,
		@ShortName = @RightShortName,
		@RightCategoryId = @RightCategory,
		@PartyId = @OrganizationPartyId,
		@ProductId = @ProductId,
		@Description = '',
		@TargetProductId = @TargetProductId,
		@VisibilityStatusId = @VisibilityStatusTypeId,
		@RightId = @OutputRightId OUTPUT;

	SELECT @ActionID = ActionId
	FROM	Enterprise.Action
	WHERE	ParentActionId IS NULL 
	AND		ObjectValue = @RightValueTypeValue
	AND		ObjectType = 'Right'

	EXEC [Enterprise].[LinkActionToRights]
		@ActionID = @ActionID,
		@RightId = @OutputRightId,
		@StatusId = @VisibilityStatusTypeId,
		@UserActionId = @UserActionId OUTPUT;

	EXEC Enterprise.CreateRight
		@RoleId = @RoleId,
		@RightName = @RightValueTypeValue,
		@RightCategoryId = @RightCategory,
		@PartyId = @OrganizationPartyId,
		@ProductId = @ProductId,
		@Shortname = @RightShortName,
		@Description = @RightValueTypeValue,
		@TargetProductId = @TargetProductId,
		@VisibilityStatusId = @VisibilityStatusTypeId,
		@RightId = @OutputRightId OUTPUT;

	FETCH NEXT FROM curOrganizationRight INTO @OrganizationPartyId, @RightValueTypeValue
END
CLOSE curOrganizationRight
DEALLOCATE curOrganizationRight
GO

IF EXISTS (SELECT TOP 1 1 FROM[UserManagement].[Control] WHERE ControlId = 409)
BEGIN
	UPDATE [usermanagement].[control] SET displayname = 'Assign current and new properties automatically' WHERE ControlId =409
END

GO
--Renovation Manager Product
/*This script is a sample script to create new prodcut in the system.*/

DECLARE @ProductId INT, 
		@LoginURI NVARCHAR(100), 
		@SigningCertificateThumbprint NVARCHAR(50), 
		@ParentProductTypeId INT, 
		@ProductName NVARCHAR(100)= 'Renovation Manager',  -- Produact Name
		@LoginURL NVARCHAR(500), 
		@ProductUrl NVARCHAR(256), 
		@apiendpoint NVARCHAR(1000), 
		@tokenEndPoint NVARCHAR(1000), 
		@apisecret NVARCHAR(1000),
		@ServerName SYSNAME = @@SERVERNAME;

DECLARE @ProductConfiguration AS PRODUCTCONFIGURATIONTYPE;

/*Validate what product type ths new product belongs to. 'Administration' in the following block 
need to be chnanged to desired prodcut type. You can query Enterprise.ProductType table for more details.
*/

SELECT @ParentProductTypeId = ProductTypeId
FROM Enterprise.ProductType
WHERE Name = 'Property Management'
      AND ParentProductTypeId IS NULL;
IF NOT EXISTS
(
    SELECT TOP 1 1
    FROM enterprise.ProductType
    WHERE Name = 'Renovation Manager'
)
    BEGIN
        EXEC [Enterprise].[CreateProductType] 
             @ProductTypeId = 112, -- Thsi value may change based on the root prodcut type
             @ParentProductTypeId = @ParentProductTypeId, 
             @Name = @ProductName, 
             @Description = @ProductName, 
             @ProductTypeGUID = '08C7E2B6-3F6F-47A2-A409-653973299C26'; -- Use newid() to generate new uniqueidentifier.
END;
SET @ProductId = 55; -- Assign new product Id

--Following block will create the new prodcut in the database
IF NOT EXISTS
(
    SELECT 1
    FROM Enterprise.Product
    WHERE Name = @ProductName
)
    BEGIN
        EXEC Enterprise.CreateProduct 
             @ProductId = @ProductId, 
             @ProductGUID = '11EA2CEF-944B-4F3B-AD2F-E2E6E1B890F4', -- Use newid() to generate new uniqueidentifier.
             @Name = @ProductName, 
             @Description = @ProductName, 
             @ProductTypeId = 112;
        
		UPDATE Enterprise.Product
          SET 
              BooksProductCode = 'RENO'
        WHERE ProductId = @ProductId;
END;

--The following block picks up all the detail frm Enterprise.ProductSettingType table
--To set up the product, bunch of these settings are required.
SET @apiendpoint = '';
Set @tokenEndPoint = '';
SET @apisecret = '';
IF @ServerName IN ('RCDUSODBSQL001')
BEGIN
	SET @apiendpoint = 'https://rl-valueadd-dev.herokuapp.com/api/v1/unified-login';
	SET @tokenEndPoint = 'https://www-dev.realpage.com/login/identity/connect/token';
	SET @apisecret = '941C57A6-2B37-4C6E-951F-672D6E2364BF';
END
IF @ServerName IN ('rctusodbsql001')
BEGIN
	SET @apiendpoint = 'https://rl-valueadd-qa.herokuapp.com/api/v1/unified-login';
	SET @tokenEndPoint = 'https://www-qa.realpage.com/login/identity/connect/token';
	SET @apisecret = 'CA6527EE-FB3C-4D15-8A82-735CAAD3E8E4';
END
--IF @ServerName IN ('RCQUSODBSQL001')
--BEGIN
--	SET @apiendpoint = '';
--END
IF @ServerName IN ('RCPGBKDBSQL005A', 'RCPGBKDBSQL005B')
BEGIN
	SET @apiendpoint = 'https://reno.rentlytics.com/api/v1/unified-login/';
	SET @tokenEndPoint = 'https://www.realpage.com/login/identity/connect/token';
	SET @apisecret = '';
END
set nocount on
INSERT INTO @ProductConfiguration
(SettingName, 
 SettingDescription, 
 SettingValue
)
VALUES
 ('ClassName','','renovationmanager')
,('ProductUrl','','/product/renovationmanager')
,('TitleId','','Renovation Manager')
,('TitleUniqueId','','4167CF48-B169-4F4F-A510-0CEB73365334')
,('IsNewTab','','1')
,('MetatagUniqueId','','Renovation Manager')
,('IsResource','','0')
,('IsFavorite','','1')
,('LearnMore','','https://www.realpage.com/reno/')
,('ApiEndPoint','',@apiendpoint)
,('ProductStatus','Show if the external application was configured for the dashboard user.','8')
,('ProductStatus','Show if the external application was configured for the dashboard user.','7')
,('ProductStatus','Show if the external application was configured for the dashboard user.','10')
,('ProductStatus','Show if the external application was configured for the dashboard user.','19')
,('ShowInUserDetails','Should the product show in the New/Edit user pages','1')
,('ShowInRolesAndRights','Should the product show in the Role/Rights page','0')
,('ShowInAppSwitcher','Should the product show in the application switcher','1')
,('ShowInUserListFilter','Should the product show in the user list product pick list','1')
,('ProductAPIRequiresUser','Does the product require a user for api calls','0')
,('LockOnProductAccess', '', '0')
,('ProductNotAvailableForRegularUserNoEmail','Product Attribute for Product Not Available for Regular User No Email.','0')

,('CLIENTID','','ulrenoapi') -- For DEV Environment
,('TOKENENDPOINT','', @tokenEndPoint) -- For DEV Environment
,('APISECRET','', @apisecret)

,('GetRoleEndpoint','Role End point for product API','/{0}/roles?isIncludeRights={1}')
,('GetRightEndpoint','Right End point for product API','/roleRights/{0}')
,('GetPropertyEndpoint','Property End point for product API','/{0}/properties')
,('GetUserEndpoint','GET User Endpoint for product API','/users?companyId={0}&loginname={1}')
,('GetListUsersEndpoint','','/{0}/users?filter={1}&startRow={2}&resultsperpage={3}')
,('PostUserEndpoint','POST User Endpoint for product API','/users')
,('PutUserEndpoint','PUT User Endpoint for product API','/users')
,('DeleteUserEndpoint','DELETE User Endpoint for product API','/{0}/users?loginName={0}') 
,('PatchMigrateUsersEndpoint','Patch Migrate Users Endpoint', '/users/{0}/migrate')
,('PatchProfileEndpoint','PATCH Profile Endpoint for product API','/userprofile')
,('GetUserExistEndpoint','Get User Exist Endpoint for product API','/userexists?loginName={0}') -- Made New Setting
,('AuthenticationType','Used to determine how to log into the product','Redirect')



SELECT * FROM @ProductConfiguration

SET @LoginURL = '';
IF @ServerName IN ('RCDUSODBSQL001')
BEGIN
	SET @LoginURL = 'https://rl-valueadd-dev.herokuapp.com/auth-callback';
END
IF @ServerName IN ('rctusodbsql001')
BEGIN
	SET @LoginURL = 'https://rl-valueadd-qa.herokuapp.com/auth-callback';
END
--IF @ServerName IN ('RCQUSODBSQL001')
--BEGIN
--	SET @LoginURL = '';
--END
IF @ServerName IN ('RCPGBKDBSQL005A', 'RCPGBKDBSQL005B')
BEGIN
	SET @LoginURL = 'https://reno.rentlytics.com/auth-callback';
END

SET @LoginURI = @LoginURL;
SET @SigningCertificateThumbprint = NULL;

--Setup the product configurations.
if not exists (select top 1 1 from Enterprise.ProductSetting where ProductId = @ProductId)
begin

	EXEC Enterprise.ProductConfigurationSetup 
		 @ProductId, 
		 @LoginURI, 
		 @SigningCertificateThumbprint, 
		 @ProductConfiguration;
end;

IF NOT EXISTS
(
    SELECT 1
    FROM ident.SamlProductSettings
    WHERE ProductId = @ProductId
          AND LoginUri = @LoginURL
)
    BEGIN
        INSERT INTO ident.SamlProductSettings
        (
        --SamlProductSettingsId - column value is auto-generated
        ProductId, 
        LoginUri, 
        SigningCertificateThumbprint, 
        SubjectIdSamlAttribute
        )
        VALUES
        (
        -- SamlProductSettingsId - int
        @ProductId, -- ProductId - int
        @LoginURL, -- LoginUri - nvarchar
        N'NA', -- SigningCertificateThumbprint - nvarchar
        N'productUserName' -- SubjectIdSamlAttribute - nvarchar
        );
END;
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
SET @RightName = 'Manage Renovation Manager Product Access'; -- Name of the right 
SET @RightDescription = 'Manage Renovation Manager Product Access'; --Description of the right as stated in story.
SET @RightShortName = 'ManageRenovationManager'; --Short name of the right that is being used by the application
SET @ActionName = 'Manage Renovation Manager'; -- This specifically pertains to actions used for routing purposes. 
SET @ActionRouteTarget = 'SideMenu'; -- Where you want this right to show up. other variation is DashBoard.
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
WHERE O.Name <> 'RealPage Employee'; 
--1. If rigths need in all organization then no condition 
--2. If needed in all except RP Employee company then O.Name <> 'RealPage Employee'
--3. If needed in just RP Employee and not in any other company, then  O.Name = 'RealPage Employee'

/*SELECT REQUIRED ATTRIBUTES FOR ROLE, RIGHT, AND ACTIONS*/
SELECT @SourceProductId = ProductId
FROM Enterprise.Product
WHERE name = 'Unified Platform';

SELECT @TargetProductId = ProductId
FROM Enterprise.Product
WHERE Name = 'Renovation Manager';

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

GO