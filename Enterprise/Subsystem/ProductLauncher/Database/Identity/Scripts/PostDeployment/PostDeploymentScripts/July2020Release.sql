
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

IF NOT EXISTS (SELECT TOP 1 1 FROM[UserManagement].[ProductPageControl] WHERE ProductPageId = @productPageId)
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

GO
declare @now datetime = getutcdate()
;with personastatus (personaid, productid, statusid, rownumber )
as (
 SELECT	p.PersonaID,
			pec.ProductId,
			ps.value,
			Row_Number() Over(Partition by prc.configurationid Order by productconfigurationid DESC) As RN
		FROM	
			Person.Persona p
			INNER JOIN Ident.UserLoginPersona ULP ON ULP.UserLoginPersonaId = p.UserLoginPersonaId
			INNER JOIN Enterprise.PersonaConfiguration pec ON p.PersonaId = pec.PersonaId
			INNER JOIN Enterprise.ProductConfiguration prc ON pec.ConfigurationId = prc.ConfigurationId
			INNER JOIN Enterprise.ProductSetting ps ON prc.ProductSettingId = ps.ProductSettingId
			INNER JOIN Enterprise.ProductSettingType pst ON ps.ProductSettingTypeId = pst.ProductSettingTypeId AND pst.name = 'ProductStatus'
		WHERE
		((@NOW >= p.FromDate AND p.ThruDate IS NULL) OR (@NOW BETWEEN p.FromDate AND p.ThruDate))
        AND     ((@NOW >= pec.FromDate AND pec.ThruDate IS NULL) OR (@NOW BETWEEN pec.FromDate AND pec.ThruDate))
        AND     ((@NOW >= prc.FromDate AND prc.ThruDate IS NULL) OR (@NOW BETWEEN prc.FromDate AND prc.ThruDate))
        AND     ((@NOW >= ps.FromDate AND ps.ThruDate IS NULL) OR (@NOW BETWEEN ps.FromDate AND ps.ThruDate))
)

--update pc
--set pc.statustypeid = ps.statusid
select *
from enterprise.PersonaConfiguration pc
inner join personastatus ps on pc.personaid = ps.personaid and ps.productid = pc.ProductId
where
rownumber = 1
and pc.statustypeid != ps.statusid
and ps.statusid in ( select statustypeid from enterprise.StatusType )

GO

declare @now datetime = getutcdate()
;with personaproductfavourite (personaid, productid, isfav, rownumber )
as (
 SELECT	p.PersonaID,
			pec.ProductId,
			ps.value,
			Row_Number() Over(Partition by prc.configurationid Order by productconfigurationid DESC) As RN
		FROM	
			Person.Persona p
			INNER JOIN Ident.UserLoginPersona ULP ON ULP.UserLoginPersonaId = p.UserLoginPersonaId
			INNER JOIN Enterprise.PersonaConfiguration pec ON p.PersonaId = pec.PersonaId
			INNER JOIN Enterprise.ProductConfiguration prc ON pec.ConfigurationId = prc.ConfigurationId
			INNER JOIN Enterprise.ProductSetting ps ON prc.ProductSettingId = ps.ProductSettingId
			INNER JOIN Enterprise.ProductSettingType pst ON ps.ProductSettingTypeId = pst.ProductSettingTypeId AND pst.name = 'IsFavorite'
		WHERE
		((@NOW >= p.FromDate AND p.ThruDate IS NULL) OR (@NOW BETWEEN p.FromDate AND p.ThruDate))
        AND     ((@NOW >= pec.FromDate AND pec.ThruDate IS NULL) OR (@NOW BETWEEN pec.FromDate AND pec.ThruDate))
        AND     ((@NOW >= prc.FromDate AND prc.ThruDate IS NULL) OR (@NOW BETWEEN prc.FromDate AND prc.ThruDate))
        AND     ((@NOW >= ps.FromDate AND ps.ThruDate IS NULL) OR (@NOW BETWEEN ps.FromDate AND ps.ThruDate))
)

--update pc
--set pc.statustypeid = ps.statusid
select *
from enterprise.PersonaConfiguration pc
inner join personaproductfavourite ps on pc.personaid = ps.personaid and ps.productid = pc.ProductId
where
rownumber = 1
and pc.isfavorite != ps.isfav

GO

