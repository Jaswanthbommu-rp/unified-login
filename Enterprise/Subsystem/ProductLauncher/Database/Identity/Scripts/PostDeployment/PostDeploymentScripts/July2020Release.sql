
DECLARE @UserId bigint,
	@ProductId int =17,
	@Now datetime = GETDATE();

SELECT	@UserId = UserId
FROM	Ident.UserLogin
WHERE	LoginName LIKE 'realpagead@%'

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
