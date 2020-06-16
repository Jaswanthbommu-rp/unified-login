GO

UPDATE	Enterprise.Product
SET			BooksProductCode = 'UPFM'
WHERE	ProductId = 3
AND			BooksProductCode = 'UL'

GO

--Unified Platform Product Access Data
DECLARE @UserId bigint,
	@ProductId int =15,
	@Now datetime = GETDATE(),
	@CurrentProductConfigurationID INT,
	@ProductSettingTypeId INT,
	@ProductSettingId INT,
	@ServerName SYSNAME = @@SERVERNAME;

SELECT	@UserId = UserId
FROM	Ident.UserLogin
WHERE	LoginName LIKE 'realpagead@%'

	SET IDENTITY_INSERT [UserManagement].[ControlDependency] ON 
	
  IF NOT EXISTS (SELECT TOP 1 1 FROM[UserManagement].[ControlDependency]  WHERE ControlDependencyId = 16)
  BEGIN
	  INSERT [UserManagement].[ControlDependency] ([ControlDependencyId], [MasterControlId], [SlaveControlID], [MasterControlValue], [ComparatorId], [CreatedBy], [CreatedDate])
	  VALUES (16, 139, 145, N'ManageCIMPLQuestions', 1, @UserId, @Now)
  END

  IF NOT EXISTS (SELECT TOP 1 1 FROM[UserManagement].[ControlDependency]  WHERE ControlDependencyId = 17)
  BEGIN
	INSERT [UserManagement].[ControlDependency] ([ControlDependencyId], [MasterControlId], [SlaveControlID], [MasterControlValue], [ComparatorId], [CreatedBy], [CreatedDate])
	VALUES (17, 139, 145, N'CIMPLManagePII', 1, @UserId, @Now)
  END
 
  IF NOT EXISTS (SELECT TOP 1 1 FROM[UserManagement].[ControlDependency]  WHERE ControlDependencyId = 18)
  BEGIN
  	INSERT [UserManagement].[ControlDependency] ([ControlDependencyId], [MasterControlId], [SlaveControlID], [MasterControlValue], [ComparatorId], [CreatedBy], [CreatedDate])
	VALUES (18, 139, 145, N'CIMPLManageSensitiveFinancialData', 1, @UserId, @Now)
  END
  
  IF NOT EXISTS (SELECT TOP 1 1 FROM[UserManagement].[ControlDependency]  WHERE ControlDependencyId = 19)
  BEGIN  
	INSERT [UserManagement].[ControlDependency] ([ControlDependencyId], [MasterControlId], [SlaveControlID], [MasterControlValue], [ComparatorId], [CreatedBy], [CreatedDate])
	VALUES (19, 139, 145, N'ViewCIMPLQuestions', 1, @UserId, @Now)
  END

	SET IDENTITY_INSERT [UserManagement].[ControlDependency] OFF

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
	VALUES (30, 181, N'Default', N'True', @UserId, @Now)

	INSERT [UserManagement].[ControlAttribute] ([ControlAttributeId], [ControlId], [Key], [Value], [CreatedBy], [CreatedDate]) 
	VALUES (31, 182, N'ShowSelectAll', N'True', @UserId, @Now)

	SET IDENTITY_INSERT [UserManagement].[ControlAttribute] OFF

	SET IDENTITY_INSERT [UserManagement].[ProductPage] ON 
	
	INSERT [UserManagement].[ProductPage] ([ProductPageId], [ProductId], [DisplayName], [CreatedBy], [CreatedDate], [IsActive]) 
	VALUES (12, 15, N'Renters Insurance Product Access', @UserId, @Now, 1)
	
	SET IDENTITY_INSERT [UserManagement].[ProductPage] OFF
	
	SET IDENTITY_INSERT [UserManagement].[ProductPageControl] ON 
	
	INSERT [UserManagement].[ProductPageControl] ([ProductPageControlId], [ProductPageId], [ControlId], [CreatedBy], [CreatedDate]) 
	VALUES (21, 12, 180, @UserId, @Now)
	
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

SELECT @ProductId = 41

IF NOT EXISTS (SELECT TOP 1 1 FROM[UserManagement].[ProductPage] WHERE ProductId = @ProductId)
BEGIN
            SET IDENTITY_INSERT [UserManagement].[Control] ON 

			INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
			VALUES (205, NULL, 8, N'ILMLeasingAnalyticsProductAccessTabGroupUIId', NULL, NULL, 1, @UserId, @Now)
			
			INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
			VALUES (206, 205, 9, N'ILMLeasingAnalyticsProductAccessPropertyGroupsTabUIId', N'Property Group', NULL, 1, @UserId, @Now)
			
			INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
			VALUES (207, 206, 3, N'ILMLeasingAnalyticsProductAccessPropertyGroupsMultiSelectGridUIId', NULL, NULL, 1, @UserId, @Now)
				
			INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate]) 
			VALUES (208, 207, 10, N'ILMLeasingAnalyticsProductAccessCheckboxUIId', NULL, N'isAssigned', 1, @UserId, @Now)
				
			INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate]) 
			VALUES (209, 207, 5, N'ILMLeasingAnalyticsProductAccessPropertyGroupLabelUIId', N'Property Group', N'name', 2, @UserId, @Now)
				
			INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate]) 
			VALUES (210, 205, 9, N'ILMLeasingAnalyticsProductAccessPropertiesTabUIId', N'Properties', NULL, 2, @UserId, @Now)
				
			INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
			VALUES (211, 210, 3, N'ILMLeasingAnalyticsProductAccessPropertiesMultiSelectGridUIId', NULL, NULL, 1, @UserId, @Now)
				
			INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
			VALUES (212, 211, 10, N'ILMLeasingAnalyticsProductAccessCheckboxUIId', NULL, N'isAssigned', 1, @UserId, @Now)
				
			INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
			VALUES (213, 211, 5, N'ILMLeasingAnalyticsProductAccessPropertyLabelUIId', N'Property', N'name', 2, @UserId, @Now)
				
			INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate]) 
			VALUES (214, 211, 5, N'ILMLeasingAnalyticsProductAccessStateLabelUIId', N'State', N'state', 3, @UserId, @Now)
				
			INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
			VALUES (215, 205, 9, N'ILMLeasingAnalyticsProductAccessRolesTabUIId', N'Roles', NULL, 3, @UserId, @Now)
				
			INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate]) 
			VALUES (216, 215, 3, N'ILMLeasingAnalyticsProductAccessRolesMultiSelectGridUIId', NULL, NULL, 1, @UserId, @Now)
				
			INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
			VALUES (217, 216, 10, N'ILMLeasingAnalyticsProductAccessCheckboxUIId', NULL, N'isAssigned', 1, @UserId, @Now)
				
			INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
			VALUES (218, 216, 5, N'ILMLeasingAnalyticsProductAccessRoleLabelUIId', N'Role', N'name', 2, @UserId, @Now)
				
			SET IDENTITY_INSERT [UserManagement].[Control] OFF
              
            SET IDENTITY_INSERT [UserManagement].[ControlAttribute] ON 
                
			INSERT [UserManagement].[ControlAttribute] ([ControlAttributeId], [ControlId], [Key], [Value], [CreatedBy], [CreatedDate]) 
			VALUES (36, 207, N'ShowSelectAll', N'True', @UserId, @Now)

			INSERT [UserManagement].[ControlAttribute] ([ControlAttributeId], [ControlId], [Key], [Value], [CreatedBy], [CreatedDate]) 
			VALUES (37, 211, N'ShowSelectAll', N'True', @UserId, @Now)

			INSERT [UserManagement].[ControlAttribute] ([ControlAttributeId], [ControlId], [Key], [Value], [CreatedBy], [CreatedDate])
			VALUES (38, 216, N'ShowSelectAll', N'False', @UserId, @Now)

            SET IDENTITY_INSERT [UserManagement].[ControlAttribute] OFF

            SET IDENTITY_INSERT [UserManagement].[ProductPage] ON 
              
			INSERT [UserManagement].[ProductPage] ([ProductPageId], [ProductId], [DisplayName], [CreatedBy], [CreatedDate], [IsActive])
			VALUES (14, 41, N'ILM Leasing Analytics Product Access', @UserId, @Now, 1)
  
            SET IDENTITY_INSERT [UserManagement].[ProductPage] OFF
              
            SET IDENTITY_INSERT [UserManagement].[ProductPageControl] ON 
            
			INSERT [UserManagement].[ProductPageControl] ([ProductPageControlId], [ProductPageId], [ControlId], [CreatedBy], [CreatedDate]) 
			VALUES (23, 14, 205, @UserId, @Now)
          
            SET IDENTITY_INSERT [UserManagement].[ProductPageControl] OFF

END

SELECT @ProductId = 40

IF NOT EXISTS (SELECT TOP 1 1 FROM[UserManagement].[ProductPage] WHERE ProductId = @ProductId)
BEGIN
              SET IDENTITY_INSERT [UserManagement].[Control] ON 

              INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
			  VALUES (219, NULL, 8, N'ILMLeadManagementProductAccessTabGroupUIId', NULL, NULL, 1, @UserId, @Now)

			  INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate]) 
			  VALUES (220, 219, 9, N'ILMLeadManagementProductAccessPropertiesTabUIId', N'Properties', NULL, 1, @UserId, @Now)

			  INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
			  VALUES (221, 220, 3, N'ILMLeadManagementProductAccessPropertiesMultiSelectGridUIId', NULL, NULL, 1, @UserId, @Now)

			  INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate]) 
			  VALUES (222, 221, 10, N'ILMLeadManagementProductAccessCheckboxUIId', NULL, N'isAssigned', 1, @UserId, @Now)

			  INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
			  VALUES (223, 221, 5, N'ILMLeadManagementProductAccessPropertyLabelUIId', N'Property', N'name', 2, @UserId, @Now)

			  INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
			  VALUES (224, 221, 5, N'ILMLeadManagementProductAccessStateLabelUIId', N'State', N'state', 3, @UserId, @Now)

			  INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate]) 
			  VALUES (225, 219, 9, N'ILMLeadManagementProductAccessRolesTabUIId', N'Roles', NULL, 2, @UserId, @Now)

			  INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate]) 
			  VALUES (226, 225, 3, N'ILMLeadManagementProductAccessRolesMultiSelectGridUIId', NULL, NULL, 1, @UserId, @Now)

			  INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
			  VALUES (227, 226, 10, N'ILMLeadManagementProductAccessCheckboxUIId', NULL, N'isAssigned', 1, @UserId, @Now)

			  INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
			  VALUES (228, 226, 5, N'ILMLeadManagementProductAccessRoleLabelUIId', N'Role', N'name', 2, @UserId, @Now)

              
              SET IDENTITY_INSERT [UserManagement].[Control] OFF
              
              SET IDENTITY_INSERT [UserManagement].[ControlAttribute] ON 
              
              INSERT [UserManagement].[ControlAttribute] ([ControlAttributeId], [ControlId], [Key], [Value], [CreatedBy], [CreatedDate])
			  VALUES (39, 221, N'ShowSelectAll', N'True', @UserId, @Now)

			  INSERT [UserManagement].[ControlAttribute] ([ControlAttributeId], [ControlId], [Key], [Value], [CreatedBy], [CreatedDate])
			  VALUES (40, 226, N'ShowSelectAll', N'False', @UserId, @Now)

              SET IDENTITY_INSERT [UserManagement].[ControlAttribute] OFF

              SET IDENTITY_INSERT [UserManagement].[ProductPage] ON 
              
              INSERT [UserManagement].[ProductPage] ([ProductPageId], [ProductId], [DisplayName], [CreatedBy], [CreatedDate], [IsActive]) 
			  VALUES (15, 40, N'ILM Lead Management Product Access', @UserId, @Now, 1)
              
              SET IDENTITY_INSERT [UserManagement].[ProductPage] OFF
              
              SET IDENTITY_INSERT [UserManagement].[ProductPageControl] ON 
              
              INSERT [UserManagement].[ProductPageControl] ([ProductPageControlId], [ProductPageId], [ControlId], [CreatedBy], [CreatedDate])
			  VALUES (24, 15, 219, @UserId, @Now)
              
              SET IDENTITY_INSERT [UserManagement].[ProductPageControl] OFF

END
SELECT @ProductId = 39

IF NOT EXISTS (SELECT TOP 1 1 FROM[UserManagement].[ProductPage] WHERE ProductId = @ProductId)
BEGIN
              SET IDENTITY_INSERT [UserManagement].[Control] ON 
			  INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate]) 
			  VALUES (229, NULL, 8, N'IntegrationMarketplaceProductAccessTabGroupUIId', NULL, NULL, 1, @UserId, @Now)

			  INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate]) 
			  VALUES (230, 229, 9, N'IntegrationMarketplaceProductAccessRolesTabUIId', N'Roles', NULL, 2, @UserId, @Now)

			  INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate]) 
			  VALUES (231, 230, 2, N'IntegrationMarketplaceProductAccessRolesSelectGridUIId', NULL, NULL, 1, @UserId, @Now)

			  INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate]) 
			  VALUES (232, 231, 7, N'IntegrationMarketplaceProductAccessRadioUIId', NULL, N'isAssigned', 1, @UserId, @Now)

			  INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate]) 
			  VALUES (233, 231, 5, N'IntegrationMarketplaceProductAccessRoleLabelUIId', N'Role', N'name', 2, @UserId, @Now)
              
              SET IDENTITY_INSERT [UserManagement].[Control] OFF
              
              SET IDENTITY_INSERT [UserManagement].[ControlAttribute] ON 
              
			  INSERT [UserManagement].[ControlAttribute] ([ControlAttributeId], [ControlId], [Key], [Value], [CreatedBy], [CreatedDate]) 
			  VALUES (41, 231, N'Default', N'True', @UserId, @Now)
              
              SET IDENTITY_INSERT [UserManagement].[ControlAttribute] OFF

              SET IDENTITY_INSERT [UserManagement].[ProductPage] ON 
              
			  INSERT [UserManagement].[ProductPage] ([ProductPageId], [ProductId], [DisplayName], [CreatedBy], [CreatedDate], [IsActive]) 
			  VALUES (16, 39, N'Integration Marketplace Product Access', @UserId, @Now, 1)
              
              SET IDENTITY_INSERT [UserManagement].[ProductPage] OFF
              
              SET IDENTITY_INSERT [UserManagement].[ProductPageControl] ON 
              
			  INSERT [UserManagement].[ProductPageControl] ([ProductPageControlId], [ProductPageId], [ControlId], [CreatedBy], [CreatedDate]) 
			  VALUES (25, 16, 229, @UserId, @Now)
              
              SET IDENTITY_INSERT [UserManagement].[ProductPageControl] OFF

END
SELECT @ProductId = 32

IF NOT EXISTS (SELECT TOP 1 1 FROM[UserManagement].[ProductPage] WHERE ProductId = @ProductId)
BEGIN
            SET IDENTITY_INSERT [UserManagement].[Control] ON 
			--Parent
			INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
			VALUES (234, NULL, 8, N'YieldStarProductAccessTabGroupUIId', NULL, NULL, 1, @UserId, @Now)

			--Roles
			INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
			VALUES (235, 234, 9, N'YieldStarProductAccessRolesTabUIId', N'Roles', NULL, 1, @UserId, @Now)

			INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate]) 
			VALUES (236, 235, 3, N'YieldStarProductAccessRolesMultiSelectGridUIId', NULL, NULL, 1, @UserId, @Now)
				
			INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
			VALUES (237, 236, 10, N'YieldStarProductAccessCheckboxUIId', NULL, N'isAssigned', 1, @UserId, @Now)
				
			INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
			VALUES (238, 236, 5, N'YieldStarProductAccessRoleLabelUIId', N'Role', N'name', 2, @UserId, @Now)

			--PropertyGroup
			INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
			VALUES (239, 234, 9, N'YieldStarProductAccessPropertyGroupsTabUIId', N'Property Group', NULL, 2, @UserId, @Now)
			
			INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
			VALUES (240, 239, 3, N'YieldStarProductAccessPropertyGroupsMultiSelectGridUIId', NULL, NULL, 1, @UserId, @Now)
				
			INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate]) 
			VALUES (241, 240, 10, N'YieldStarProductAccessCheckboxUIId', NULL, N'isAssigned', 1, @UserId, @Now)
				
			INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate]) 
			VALUES (242, 240, 5, N'YieldStarProductAccessPropertyGroupLabelUIId', N'Group', N'name', 2, @UserId, @Now)
			--PGSlide
			INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
			VALUES (243, 240, 11, N'YieldStarProductAccessIconUIId', NULL, N'InfoIcon', 4, @UserId, @Now)

			INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
			VALUES (244, 243, 5, N'YieldStarProductAccessPropertyGroupDetailsLabelUIId', N'Property Group Details', NULL, 1, @UserId, @Now)

			INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
			VALUES (245, 243, 12, N'YieldStarProductAccessPropertyGroupDetailsGridUIId', NULL, NULL, 2, @UserId, @Now)

			INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
			VALUES (246, 245, 5, N'YieldStarProductAccessPropertyGroupDetailsPropertyLabelUIId', N'Property', N'name', 2, @UserId, @Now)
			INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
			VALUES (247, 245, 5, N'YieldStarProductAccessPropertyGroupDetailsPropertyLabelUIId', N'State', N'state', 2, @UserId, @Now)

			
			--Properties
			INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate]) 
			VALUES (248, 234, 9, N'YieldStarProductAccessPropertiesTabUIId', N'Properties', NULL, 3, @UserId, @Now)
				
			INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
			VALUES (249, 248, 3, N'YieldStarProductAccessPropertiesMultiSelectGridUIId', NULL, NULL, 1, @UserId, @Now)
				
			INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
			VALUES (250, 249, 10, N'YieldStarProductAccessCheckboxUIId', NULL, N'isAssigned', 1, @UserId, @Now)
				
			INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
			VALUES (251, 249, 5, N'YieldStarProductAccessPropertyLabelUIId', N'Property', N'name', 2, @UserId, @Now)
				
			INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate]) 
			VALUES (252, 249, 5, N'YieldStarProductAccessStateLabelUIId', N'State', N'state', 3, @UserId, @Now)	
				
			SET IDENTITY_INSERT [UserManagement].[Control] OFF
              
            SET IDENTITY_INSERT [UserManagement].[ControlAttribute] ON 
            INSERT [UserManagement].[ControlAttribute] ([ControlAttributeId], [ControlId], [Key], [Value], [CreatedBy], [CreatedDate])
			VALUES (42, 235, N'Default', N'True', @UserId, @Now)

			--INSERT [UserManagement].[ControlAttribute] ([ControlAttributeId], [ControlId], [Key], [Value], [CreatedBy], [CreatedDate]) 
			--VALUES (43, 236, N'ShowSelectAll', N'True', @UserId, @Now)
    
			INSERT [UserManagement].[ControlAttribute] ([ControlAttributeId], [ControlId], [Key], [Value], [CreatedBy], [CreatedDate]) 
			VALUES (44, 243, N'InfoIcon', N'Slide', @UserId, @Now)

			INSERT [UserManagement].[ControlAttribute] ([ControlAttributeId], [ControlId], [Key], [Value], [CreatedBy], [CreatedDate]) 
			VALUES (45, 240, N'ShowSelectAll', N'True', @UserId, @Now)

			INSERT [UserManagement].[ControlAttribute] ([ControlAttributeId], [ControlId], [Key], [Value], [CreatedBy], [CreatedDate])
			VALUES (46, 249, N'ShowSelectAll', N'True', @UserId, @Now)

            SET IDENTITY_INSERT [UserManagement].[ControlAttribute] OFF

            SET IDENTITY_INSERT [UserManagement].[ProductPage] ON 
              
			INSERT [UserManagement].[ProductPage] ([ProductPageId], [ProductId], [DisplayName], [CreatedBy], [CreatedDate], [IsActive])
			VALUES (17, 32, N'Revenue Management Product Access', @UserId, @Now, 1)
  
            SET IDENTITY_INSERT [UserManagement].[ProductPage] OFF
              
            SET IDENTITY_INSERT [UserManagement].[ProductPageControl] ON 
            
			INSERT [UserManagement].[ProductPageControl] ([ProductPageControlId], [ProductPageId], [ControlId], [CreatedBy], [CreatedDate]) 
			VALUES (26, 17, 234, @UserId, @Now)
          
            SET IDENTITY_INSERT [UserManagement].[ProductPageControl] OFF

END

SELECT @ProductId = 29

IF NOT EXISTS (SELECT TOP 1 1 FROM[UserManagement].[ProductPage] WHERE ProductId = @ProductId)
BEGIN
            SET IDENTITY_INSERT [UserManagement].[Control] ON 
			--Parent
			INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
			VALUES (253, NULL, 8, N'BusinessIntelligenceProductAccessTabGroupUIId', NULL, NULL, 1, @UserId, @Now)

			--Roles
			INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
			VALUES (254, 253, 9, N'BusinessIntelligenceProductAccessRolesTabUIId', N'Roles', NULL, 1, @UserId, @Now)

			INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate]) 
			VALUES (255, 254, 3, N'BusinessIntelligenceProductAccessRolesMultiSelectGridUIId', NULL, NULL, 1, @UserId, @Now)
				
			INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
			VALUES (256, 255, 10, N'BusinessIntelligenceProductAccessCheckboxUIId', NULL, N'isAssigned', 1, @UserId, @Now)
				
			INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
			VALUES (257, 255, 5, N'BusinessIntelligenceProductAccessRoleLabelUIId', N'Role', N'name', 2, @UserId, @Now)

			--PropertyGroup
			INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
			VALUES (258, 253, 9, N'BusinessIntelligenceProductAccessPropertyGroupsTabUIId', N'Property Group', NULL, 2, @UserId, @Now)
			
			INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
			VALUES (259, 258, 3, N'BusinessIntelligenceProductAccessPropertyGroupsMultiSelectGridUIId', NULL, NULL, 1, @UserId, @Now)
				
			INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate]) 
			VALUES (260, 259, 10, N'BusinessIntelligenceProductAccessCheckboxUIId', NULL, N'isAssigned', 1, @UserId, @Now)
				
			INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate]) 
			VALUES (261, 259, 5, N'BusinessIntelligenceProductAccessPropertyGroupLabelUIId', N'Group', N'name', 2, @UserId, @Now)
			--PGSlide
			INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
			VALUES (262, 259, 11, N'BusinessIntelligenceProductAccessIconUIId', NULL, N'InfoIcon', 4, @UserId, @Now)

			INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
			VALUES (263, 262, 5, N'BusinessIntelligenceProductAccessPropertyGroupDetailsLabelUIId', N'Property Group Details', NULL, 1, @UserId, @Now)

			INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
			VALUES (264, 262, 12, N'BusinessIntelligenceProductAccessPropertyGroupDetailsGridUIId', NULL, NULL, 2, @UserId, @Now)

			INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
			VALUES (265, 264, 5, N'BusinessIntelligenceProductAccessPropertyGroupDetailsPropertyLabelUIId', N'Property', N'name', 2, @UserId, @Now)

			INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
			VALUES (266, 264, 5, N'BusinessIntelligenceProductAccessPropertyGroupDetailsPropertyLabelUIId', N'State', N'state', 2, @UserId, @Now)

			
			--Properties
			INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate]) 
			VALUES (267, 253, 9, N'BusinessIntelligenceProductAccessPropertiesTabUIId', N'Properties', NULL, 3, @UserId, @Now)
				
			INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
			VALUES (268, 267, 3, N'BusinessIntelligenceProductAccessPropertiesMultiSelectGridUIId', NULL, NULL, 1, @UserId, @Now)
				
			INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
			VALUES (269, 268, 10, N'BusinessIntelligenceProductAccessCheckboxUIId', NULL, N'isAssigned', 1, @UserId, @Now)
				
			INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
			VALUES (270, 268, 5, N'BusinessIntelligenceProductAccessPropertyLabelUIId', N'Property', N'name', 2, @UserId, @Now)
				
			INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate]) 
			VALUES (271, 268, 5, N'BusinessIntelligenceProductAccessStateLabelUIId', N'State', N'state', 3, @UserId, @Now)	
				
			SET IDENTITY_INSERT [UserManagement].[Control] OFF
              
            SET IDENTITY_INSERT [UserManagement].[ControlAttribute] ON 
            INSERT [UserManagement].[ControlAttribute] ([ControlAttributeId], [ControlId], [Key], [Value], [CreatedBy], [CreatedDate])
			VALUES (47, 254, N'Default', N'True', @UserId, @Now)

			--INSERT [UserManagement].[ControlAttribute] ([ControlAttributeId], [ControlId], [Key], [Value], [CreatedBy], [CreatedDate]) 
			--VALUES (48, 255, N'ShowSelectAll', N'True', @UserId, @Now)
    
			INSERT [UserManagement].[ControlAttribute] ([ControlAttributeId], [ControlId], [Key], [Value], [CreatedBy], [CreatedDate]) 
			VALUES (49, 262, N'InfoIcon', N'Slide', @UserId, @Now)

			INSERT [UserManagement].[ControlAttribute] ([ControlAttributeId], [ControlId], [Key], [Value], [CreatedBy], [CreatedDate]) 
			VALUES (50, 259, N'ShowSelectAll', N'True', @UserId, @Now)

			INSERT [UserManagement].[ControlAttribute] ([ControlAttributeId], [ControlId], [Key], [Value], [CreatedBy], [CreatedDate])
			VALUES (51, 268, N'ShowSelectAll', N'True', @UserId, @Now)

            SET IDENTITY_INSERT [UserManagement].[ControlAttribute] OFF

            SET IDENTITY_INSERT [UserManagement].[ProductPage] ON 
              
			INSERT [UserManagement].[ProductPage] ([ProductPageId], [ProductId], [DisplayName], [CreatedBy], [CreatedDate], [IsActive])
			VALUES (18, 29, N'Business Intelligence Product Access', @UserId, @Now, 1)
  
            SET IDENTITY_INSERT [UserManagement].[ProductPage] OFF
              
            SET IDENTITY_INSERT [UserManagement].[ProductPageControl] ON 
            
			INSERT [UserManagement].[ProductPageControl] ([ProductPageControlId], [ProductPageId], [ControlId], [CreatedBy], [CreatedDate]) 
			VALUES (27, 18, 253, @UserId, @Now)
          
            SET IDENTITY_INSERT [UserManagement].[ProductPageControl] OFF

END

SELECT @ProductId = 31

IF NOT EXISTS (SELECT TOP 1 1 FROM[UserManagement].[ProductPage] WHERE ProductId = @ProductId)
BEGIN
            SET IDENTITY_INSERT [UserManagement].[Control] ON 
			--Parent
			INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
			VALUES (272, NULL, 8, N'InvestmentAnalyticsProductAccessTabGroupUIId', NULL, NULL, 1, @UserId, @Now)

			--Roles
			INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
			VALUES (273, 272, 9, N'InvestmentAnalyticsProductAccessRolesTabUIId', N'Roles', NULL, 1, @UserId, @Now)

			INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate]) 
			VALUES (274, 273, 3, N'InvestmentAnalyticsProductAccessRolesMultiSelectGridUIId', NULL, NULL, 1, @UserId, @Now)
				
			INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
			VALUES (275, 274, 10, N'InvestmentAnalyticsProductAccessCheckboxUIId', NULL, N'isAssigned', 1, @UserId, @Now)
				
			INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
			VALUES (276, 274, 5, N'InvestmentAnalyticsProductAccessRoleLabelUIId', N'Role', N'name', 2, @UserId, @Now)

			--PropertyGroup
			INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
			VALUES (277, 272, 9, N'InvestmentAnalyticsProductAccessPropertyGroupsTabUIId', N'Markets', NULL, 2, @UserId, @Now)
			
			INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
			VALUES (278, 277, 3, N'InvestmentAnalyticsProductAccessPropertyGroupsMultiSelectGridUIId', NULL, NULL, 1, @UserId, @Now)
				
			INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate]) 
			VALUES (279, 278, 10, N'InvestmentAnalyticsProductAccessCheckboxUIId', NULL, N'isAssigned', 1, @UserId, @Now)
				
			INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate]) 
			VALUES (280, 278, 5, N'InvestmentAnalyticsProductAccessPropertyGroupLabelUIId', N'Market', N'name', 2, @UserId, @Now)
			
				
			SET IDENTITY_INSERT [UserManagement].[Control] OFF
              
            SET IDENTITY_INSERT [UserManagement].[ControlAttribute] ON 
            INSERT [UserManagement].[ControlAttribute] ([ControlAttributeId], [ControlId], [Key], [Value], [CreatedBy], [CreatedDate])
			VALUES (52, 273, N'Default', N'True', @UserId, @Now)

			--INSERT [UserManagement].[ControlAttribute] ([ControlAttributeId], [ControlId], [Key], [Value], [CreatedBy], [CreatedDate]) 
			--VALUES (53, 274, N'ShowSelectAll', N'True', @UserId, @Now)
    
			INSERT [UserManagement].[ControlAttribute] ([ControlAttributeId], [ControlId], [Key], [Value], [CreatedBy], [CreatedDate]) 
			VALUES (54, 278, N'ShowSelectAll', N'True', @UserId, @Now)

            SET IDENTITY_INSERT [UserManagement].[ControlAttribute] OFF

            SET IDENTITY_INSERT [UserManagement].[ProductPage] ON 
              
			INSERT [UserManagement].[ProductPage] ([ProductPageId], [ProductId], [DisplayName], [CreatedBy], [CreatedDate], [IsActive])
			VALUES (19, 31, N'Investment Analytics Product Access', @UserId, @Now, 1)
  
            SET IDENTITY_INSERT [UserManagement].[ProductPage] OFF
              
            SET IDENTITY_INSERT [UserManagement].[ProductPageControl] ON 
            
			INSERT [UserManagement].[ProductPageControl] ([ProductPageControlId], [ProductPageId], [ControlId], [CreatedBy], [CreatedDate]) 
			VALUES (28, 19, 272, @UserId, @Now)
          
            SET IDENTITY_INSERT [UserManagement].[ProductPageControl] OFF

END
SELECT @ProductId = 51

IF NOT EXISTS (SELECT TOP 1 1 FROM[UserManagement].[ProductPage] WHERE ProductId = @ProductId)
BEGIN
            SET IDENTITY_INSERT [UserManagement].[Control] ON 
			--Parent
			INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
			VALUES (281, NULL, 8, N'LROProductAccessTabGroupUIId', NULL, NULL, 1, @UserId, @Now)

			--Roles
			INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
			VALUES (282, 281, 9, N'LROProductAccessRolesTabUIId', N'Roles', NULL, 1, @UserId, @Now)

			INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate]) 
			VALUES (283, 282, 3, N'LROProductAccessRolesMultiSelectGridUIId', NULL, NULL, 1, @UserId, @Now)
				
			INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
			VALUES (284, 283, 10, N'LROProductAccessCheckboxUIId', NULL, N'isAssigned', 1, @UserId, @Now)
				
			INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
			VALUES (285, 283, 5, N'LROProductAccessRoleLabelUIId', N'Role', N'name', 2, @UserId, @Now)

			--PropertyGroup
			INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
			VALUES (286, 281, 9, N'LROProductAccessPropertyGroupsTabUIId', N'Property Group', NULL, 2, @UserId, @Now)
			
			INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
			VALUES (287, 286, 3, N'LROProductAccessPropertyGroupsMultiSelectGridUIId', NULL, NULL, 1, @UserId, @Now)
				
			INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate]) 
			VALUES (288, 287, 10, N'LROProductAccessCheckboxUIId', NULL, N'isAssigned', 1, @UserId, @Now)
				
			INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate]) 
			VALUES (289, 287, 5, N'LROProductAccessPropertyGroupLabelUIId', N'Group', N'name', 2, @UserId, @Now)
			--PGSlide
			INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
			VALUES (290, 287, 11, N'LROProductAccessIconUIId', NULL, N'InfoIcon', 4, @UserId, @Now)

			INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
			VALUES (291, 290, 5, N'LROProductAccessPropertyGroupDetailsLabelUIId', N'Property Group Details', NULL, 1, @UserId, @Now)

			INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
			VALUES (292, 290, 12, N'LROProductAccessPropertyGroupDetailsGridUIId', NULL, NULL, 2, @UserId, @Now)

			INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
			VALUES (293, 292, 5, N'LROProductAccessPropertyGroupDetailsPropertyLabelUIId', N'Property', N'name', 2, @UserId, @Now)
			
			INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
			VALUES (294, 292, 5, N'LROProductAccessPropertyGroupDetailsPropertyLabelUIId', N'State', N'state', 2, @UserId, @Now)

			
			--Properties
			INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate]) 
			VALUES (295, 281, 9, N'LROProductAccessPropertiesTabUIId', N'Properties', NULL, 3, @UserId, @Now)
				
			INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
			VALUES (296, 295, 3, N'LROProductAccessPropertiesMultiSelectGridUIId', NULL, NULL, 1, @UserId, @Now)
				
			INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
			VALUES (297, 296, 10, N'LROProductAccessCheckboxUIId', NULL, N'isAssigned', 1, @UserId, @Now)
				
			INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
			VALUES (298, 296, 5, N'LROProductAccessPropertyLabelUIId', N'Property', N'name', 2, @UserId, @Now)
				
			INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate]) 
			VALUES (299, 296, 5, N'LROProductAccessStateLabelUIId', N'State', N'state', 3, @UserId, @Now)	
				
			SET IDENTITY_INSERT [UserManagement].[Control] OFF
              
            SET IDENTITY_INSERT [UserManagement].[ControlAttribute] ON 
            INSERT [UserManagement].[ControlAttribute] ([ControlAttributeId], [ControlId], [Key], [Value], [CreatedBy], [CreatedDate])
			VALUES (55, 282, N'Default', N'True', @UserId, @Now)

			--INSERT [UserManagement].[ControlAttribute] ([ControlAttributeId], [ControlId], [Key], [Value], [CreatedBy], [CreatedDate]) 
			--VALUES (56, 283, N'ShowSelectAll', N'True', @UserId, @Now)
    
			INSERT [UserManagement].[ControlAttribute] ([ControlAttributeId], [ControlId], [Key], [Value], [CreatedBy], [CreatedDate]) 
			VALUES (57, 287, N'ShowSelectAll', N'True', @UserId, @Now)

			INSERT [UserManagement].[ControlAttribute] ([ControlAttributeId], [ControlId], [Key], [Value], [CreatedBy], [CreatedDate]) 
			VALUES (58, 290, N'InfoIcon', N'Slide', @UserId, @Now)			

			INSERT [UserManagement].[ControlAttribute] ([ControlAttributeId], [ControlId], [Key], [Value], [CreatedBy], [CreatedDate])
			VALUES (59, 296, N'ShowSelectAll', N'True', @UserId, @Now)

            SET IDENTITY_INSERT [UserManagement].[ControlAttribute] OFF

            SET IDENTITY_INSERT [UserManagement].[ProductPage] ON 
              
			INSERT [UserManagement].[ProductPage] ([ProductPageId], [ProductId], [DisplayName], [CreatedBy], [CreatedDate], [IsActive])
			VALUES (20, 51, N'LRO Product Access', @UserId, @Now, 1)
  
            SET IDENTITY_INSERT [UserManagement].[ProductPage] OFF
              
            SET IDENTITY_INSERT [UserManagement].[ProductPageControl] ON 
            
			INSERT [UserManagement].[ProductPageControl] ([ProductPageControlId], [ProductPageId], [ControlId], [CreatedBy], [CreatedDate]) 
			VALUES (29, 20, 281, @UserId, @Now)
          
            SET IDENTITY_INSERT [UserManagement].[ProductPageControl] OFF

END
SELECT @ProductId = 52

IF NOT EXISTS (SELECT TOP 1 1 FROM[UserManagement].[ProductPage] WHERE ProductId = @ProductId)
BEGIN
            SET IDENTITY_INSERT [UserManagement].[Control] ON 
			--Parent
			INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
			VALUES (300, NULL, 8, N'AmenityOptimizationProductAccessTabGroupUIId', NULL, NULL, 1, @UserId, @Now)

			--Roles
			INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
			VALUES (301, 300, 9, N'AmenityOptimizationProductAccessRolesTabUIId', N'Roles', NULL, 1, @UserId, @Now)

			INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate]) 
			VALUES (302, 301, 3, N'AmenityOptimizationProductAccessRolesMultiSelectGridUIId', NULL, NULL, 1, @UserId, @Now)
				
			INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
			VALUES (303, 302, 10, N'AmenityOptimizationProductAccessCheckboxUIId', NULL, N'isAssigned', 1, @UserId, @Now)
				
			INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
			VALUES (304, 302, 5, N'AmenityOptimizationProductAccessRoleLabelUIId', N'Role', N'name', 2, @UserId, @Now)

			--PropertyGroup
			INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
			VALUES (305, 300, 9, N'AmenityOptimizationProductAccessPropertyGroupsTabUIId', N'Property Group', NULL, 2, @UserId, @Now)
			
			INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
			VALUES (306, 305, 3, N'AmenityOptimizationProductAccessPropertyGroupsMultiSelectGridUIId', NULL, NULL, 1, @UserId, @Now)
				
			INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate]) 
			VALUES (307, 306, 10, N'AmenityOptimizationProductAccessCheckboxUIId', NULL, N'isAssigned', 1, @UserId, @Now)
				
			INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate]) 
			VALUES (308, 306, 5, N'AmenityOptimizationProductAccessPropertyGroupLabelUIId', N'Group', N'name', 2, @UserId, @Now)
			--PGSlide
			INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
			VALUES (309, 306, 11, N'AmenityOptimizationProductAccessIconUIId', NULL, N'InfoIcon', 4, @UserId, @Now)

			INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
			VALUES (310, 309, 5, N'AmenityOptimizationProductAccessPropertyGroupDetailsLabelUIId', N'Property Group Details', NULL, 1, @UserId, @Now)

			INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
			VALUES (311, 309, 12, N'AmenityOptimizationProductAccessPropertyGroupDetailsGridUIId', NULL, NULL, 2, @UserId, @Now)

			INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
			VALUES (312, 311, 5, N'AmenityOptimizationProductAccessPropertyGroupDetailsPropertyLabelUIId', N'Property', N'name', 2, @UserId, @Now)
			
			INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
			VALUES (313, 311, 5, N'AmenityOptimizationProductAccessPropertyGroupDetailsPropertyLabelUIId', N'State', N'state', 2, @UserId, @Now)

			
			--Properties
			INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate]) 
			VALUES (314, 300, 9, N'AmenityOptimizationProductAccessPropertiesTabUIId', N'Properties', NULL, 3, @UserId, @Now)
				
			INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
			VALUES (315, 314, 3, N'AmenityOptimizationProductAccessPropertiesMultiSelectGridUIId', NULL, NULL, 1, @UserId, @Now)
				
			INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
			VALUES (316, 315, 10, N'AmenityOptimizationProductAccessCheckboxUIId', NULL, N'isAssigned', 1, @UserId, @Now)
				
			INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
			VALUES (317, 315, 5, N'AmenityOptimizationProductAccessPropertyLabelUIId', N'Property', N'name', 2, @UserId, @Now)
				
			INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate]) 
			VALUES (318, 315, 5, N'AmenityOptimizationProductAccessStateLabelUIId', N'State', N'state', 3, @UserId, @Now)	
				
			SET IDENTITY_INSERT [UserManagement].[Control] OFF
              
            SET IDENTITY_INSERT [UserManagement].[ControlAttribute] ON 
            INSERT [UserManagement].[ControlAttribute] ([ControlAttributeId], [ControlId], [Key], [Value], [CreatedBy], [CreatedDate])
			VALUES (60, 301, N'Default', N'True', @UserId, @Now)

			--INSERT [UserManagement].[ControlAttribute] ([ControlAttributeId], [ControlId], [Key], [Value], [CreatedBy], [CreatedDate]) 
			--VALUES (61, 303, N'ShowSelectAll', N'True', @UserId, @Now)
    
			INSERT [UserManagement].[ControlAttribute] ([ControlAttributeId], [ControlId], [Key], [Value], [CreatedBy], [CreatedDate]) 
			VALUES (62, 306, N'ShowSelectAll', N'True', @UserId, @Now)

			INSERT [UserManagement].[ControlAttribute] ([ControlAttributeId], [ControlId], [Key], [Value], [CreatedBy], [CreatedDate]) 
			VALUES (63, 309, N'InfoIcon', N'Slide', @UserId, @Now)			

			INSERT [UserManagement].[ControlAttribute] ([ControlAttributeId], [ControlId], [Key], [Value], [CreatedBy], [CreatedDate])
			VALUES (64, 315, N'ShowSelectAll', N'True', @UserId, @Now)

            SET IDENTITY_INSERT [UserManagement].[ControlAttribute] OFF

            SET IDENTITY_INSERT [UserManagement].[ProductPage] ON 
              
			INSERT [UserManagement].[ProductPage] ([ProductPageId], [ProductId], [DisplayName], [CreatedBy], [CreatedDate], [IsActive])
			VALUES (21, 52, N'Amenity Optimization Product Access', @UserId, @Now, 1)
  
            SET IDENTITY_INSERT [UserManagement].[ProductPage] OFF
              
            SET IDENTITY_INSERT [UserManagement].[ProductPageControl] ON 
            
			INSERT [UserManagement].[ProductPageControl] ([ProductPageControlId], [ProductPageId], [ControlId], [CreatedBy], [CreatedDate]) 
			VALUES (30, 21, 300, @UserId, @Now)
          
            SET IDENTITY_INSERT [UserManagement].[ProductPageControl] OFF

END
SELECT @ProductId = 53

IF NOT EXISTS (SELECT TOP 1 1 FROM[UserManagement].[ProductPage] WHERE ProductId = @ProductId)
BEGIN
            SET IDENTITY_INSERT [UserManagement].[Control] ON 
			--Parent
			INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
			VALUES (319, NULL, 8, N'AIRevenueManagementProductAccessTabGroupUIId', NULL, NULL, 1, @UserId, @Now)

			--Roles
			INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
			VALUES (320, 319, 9, N'AIRevenueManagementProductAccessRolesTabUIId', N'Roles', NULL, 1, @UserId, @Now)

			INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate]) 
			VALUES (321, 320, 3, N'AIRevenueManagementProductAccessRolesMultiSelectGridUIId', NULL, NULL, 1, @UserId, @Now)
				
			INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
			VALUES (322, 321, 10, N'AIRevenueManagementProductAccessCheckboxUIId', NULL, N'isAssigned', 1, @UserId, @Now)
				
			INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
			VALUES (323, 321, 5, N'AIRevenueManagementProductAccessRoleLabelUIId', N'Role', N'name', 2, @UserId, @Now)

			--PropertyGroup
			INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
			VALUES (324, 319, 9, N'AIRevenueManagementProductAccessPropertyGroupsTabUIId', N'Property Group', NULL, 2, @UserId, @Now)
			
			INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
			VALUES (325, 324, 3, N'AIRevenueManagementProductAccessPropertyGroupsMultiSelectGridUIId', NULL, NULL, 1, @UserId, @Now)
				
			INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate]) 
			VALUES (326, 325, 10, N'AIRevenueManagementProductAccessCheckboxUIId', NULL, N'isAssigned', 1, @UserId, @Now)
				
			INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate]) 
			VALUES (327, 325, 5, N'AIRevenueManagementProductAccessPropertyGroupLabelUIId', N'Group', N'name', 2, @UserId, @Now)
			--PGSlide
			INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
			VALUES (328, 325, 11, N'AIRevenueManagementProductAccessIconUIId', NULL, N'InfoIcon', 4, @UserId, @Now)

			INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
			VALUES (329, 328, 5, N'AIRevenueManagementProductAccessPropertyGroupDetailsLabelUIId', N'Property Group Details', NULL, 1, @UserId, @Now)

			INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
			VALUES (330, 328, 12, N'AIRevenueManagementProductAccessPropertyGroupDetailsGridUIId', NULL, NULL, 2, @UserId, @Now)

			INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
			VALUES (331, 330, 5, N'AIRevenueManagementProductAccessPropertyGroupDetailsPropertyLabelUIId', N'Property', N'name', 2, @UserId, @Now)
			
			INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
			VALUES (332, 330, 5, N'AIRevenueManagementProductAccessPropertyGroupDetailsPropertyLabelUIId', N'State', N'state', 2, @UserId, @Now)

			
			--Properties
			INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate]) 
			VALUES (333, 319, 9, N'AIRevenueManagementProductAccessPropertiesTabUIId', N'Properties', NULL, 3, @UserId, @Now)
				
			INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
			VALUES (334, 333, 3, N'AIRevenueManagementProductAccessPropertiesMultiSelectGridUIId', NULL, NULL, 1, @UserId, @Now)
				
			INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
			VALUES (335, 334, 10, N'AIRevenueManagementProductAccessCheckboxUIId', NULL, N'isAssigned', 1, @UserId, @Now)
				
			INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
			VALUES (336, 334, 5, N'AIRevenueManagementProductAccessPropertyLabelUIId', N'Property', N'name', 2, @UserId, @Now)
				
			INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate]) 
			VALUES (337, 334, 5, N'AIRevenueManagementProductAccessStateLabelUIId', N'State', N'state', 3, @UserId, @Now)	
				
			SET IDENTITY_INSERT [UserManagement].[Control] OFF
              
            SET IDENTITY_INSERT [UserManagement].[ControlAttribute] ON 
            INSERT [UserManagement].[ControlAttribute] ([ControlAttributeId], [ControlId], [Key], [Value], [CreatedBy], [CreatedDate])
			VALUES (65, 320, N'Default', N'True', @UserId, @Now)

			--INSERT [UserManagement].[ControlAttribute] ([ControlAttributeId], [ControlId], [Key], [Value], [CreatedBy], [CreatedDate]) 
			--VALUES (66, 321, N'ShowSelectAll', N'True', @UserId, @Now)
    
			INSERT [UserManagement].[ControlAttribute] ([ControlAttributeId], [ControlId], [Key], [Value], [CreatedBy], [CreatedDate]) 
			VALUES (67, 325, N'ShowSelectAll', N'True', @UserId, @Now)

			INSERT [UserManagement].[ControlAttribute] ([ControlAttributeId], [ControlId], [Key], [Value], [CreatedBy], [CreatedDate]) 
			VALUES (68, 328, N'InfoIcon', N'Slide', @UserId, @Now)			

			INSERT [UserManagement].[ControlAttribute] ([ControlAttributeId], [ControlId], [Key], [Value], [CreatedBy], [CreatedDate])
			VALUES (69, 334, N'ShowSelectAll', N'True', @UserId, @Now)

            SET IDENTITY_INSERT [UserManagement].[ControlAttribute] OFF

            SET IDENTITY_INSERT [UserManagement].[ProductPage] ON 
              
			INSERT [UserManagement].[ProductPage] ([ProductPageId], [ProductId], [DisplayName], [CreatedBy], [CreatedDate], [IsActive])
			VALUES (22, 53, N'AI Revenue Management Product Access', @UserId, @Now, 1)
  
            SET IDENTITY_INSERT [UserManagement].[ProductPage] OFF
              
            SET IDENTITY_INSERT [UserManagement].[ProductPageControl] ON 
            
			INSERT [UserManagement].[ProductPageControl] ([ProductPageControlId], [ProductPageId], [ControlId], [CreatedBy], [CreatedDate]) 
			VALUES (31, 22, 319, @UserId, @Now)
          
            SET IDENTITY_INSERT [UserManagement].[ProductPageControl] OFF

END
SELECT @ProductId = 54

IF NOT EXISTS (SELECT TOP 1 1 FROM[UserManagement].[ProductPage] WHERE ProductId = @ProductId)
BEGIN
            SET IDENTITY_INSERT [UserManagement].[Control] ON 
			--Parent
			INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
			VALUES (338, NULL, 8, N'RentControlProductAccessTabGroupUIId', NULL, NULL, 1, @UserId, @Now)

			--Roles
			INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
			VALUES (339, 338, 9, N'RentControlProductAccessRolesTabUIId', N'Roles', NULL, 1, @UserId, @Now)

			INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate]) 
			VALUES (340, 339, 3, N'RentControlProductAccessRolesMultiSelectGridUIId', NULL, NULL, 1, @UserId, @Now)
				
			INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
			VALUES (341, 340, 10, N'RentControlProductAccessCheckboxUIId', NULL, N'isAssigned', 1, @UserId, @Now)
				
			INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
			VALUES (342, 340, 5, N'RentControlProductAccessRoleLabelUIId', N'Role', N'name', 2, @UserId, @Now)

			--PropertyGroup
			INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
			VALUES (343, 338, 9, N'RentControlProductAccessPropertyGroupsTabUIId', N'Property Group', NULL, 2, @UserId, @Now)
			
			INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
			VALUES (344, 343, 3, N'RentControlProductAccessPropertyGroupsMultiSelectGridUIId', NULL, NULL, 1, @UserId, @Now)
				
			INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate]) 
			VALUES (345, 344, 10, N'RentControlProductAccessCheckboxUIId', NULL, N'isAssigned', 1, @UserId, @Now)
				
			INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate]) 
			VALUES (346, 344, 5, N'RentControlProductAccessPropertyGroupLabelUIId', N'Group', N'name', 2, @UserId, @Now)
			--PGSlide
			INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
			VALUES (347, 344, 11, N'RentControlProductAccessIconUIId', NULL, N'InfoIcon', 4, @UserId, @Now)

			INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
			VALUES (348, 347, 5, N'RentControlProductAccessPropertyGroupDetailsLabelUIId', N'Property Group Details', NULL, 1, @UserId, @Now)

			INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
			VALUES (349, 347, 12, N'RentControlProductAccessPropertyGroupDetailsGridUIId', NULL, NULL, 2, @UserId, @Now)

			INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
			VALUES (350, 349, 5, N'RentControlProductAccessPropertyGroupDetailsPropertyLabelUIId', N'Property', N'name', 2, @UserId, @Now)
			
			INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
			VALUES (351, 349, 5, N'RentControlProductAccessPropertyGroupDetailsPropertyLabelUIId', N'State', N'state', 2, @UserId, @Now)

			
			--Properties
			INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate]) 
			VALUES (352, 338, 9, N'RentControlProductAccessPropertiesTabUIId', N'Properties', NULL, 3, @UserId, @Now)
				
			INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
			VALUES (353, 352, 3, N'RentControlProductAccessPropertiesMultiSelectGridUIId', NULL, NULL, 1, @UserId, @Now)
				
			INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
			VALUES (354, 353, 10, N'RentControlProductAccessCheckboxUIId', NULL, N'isAssigned', 1, @UserId, @Now)
				
			INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
			VALUES (355, 353, 5, N'RentControlProductAccessPropertyLabelUIId', N'Property', N'name', 2, @UserId, @Now)
				
			INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate]) 
			VALUES (356, 353, 5, N'RentControlProductAccessStateLabelUIId', N'State', N'state', 3, @UserId, @Now)	
				
			SET IDENTITY_INSERT [UserManagement].[Control] OFF
              
            SET IDENTITY_INSERT [UserManagement].[ControlAttribute] ON 
            INSERT [UserManagement].[ControlAttribute] ([ControlAttributeId], [ControlId], [Key], [Value], [CreatedBy], [CreatedDate])
			VALUES (70, 339, N'Default', N'True', @UserId, @Now)

			--INSERT [UserManagement].[ControlAttribute] ([ControlAttributeId], [ControlId], [Key], [Value], [CreatedBy], [CreatedDate]) 
			--VALUES (71, 340, N'ShowSelectAll', N'True', @UserId, @Now)
    
			INSERT [UserManagement].[ControlAttribute] ([ControlAttributeId], [ControlId], [Key], [Value], [CreatedBy], [CreatedDate]) 
			VALUES (72, 344, N'ShowSelectAll', N'True', @UserId, @Now)

			INSERT [UserManagement].[ControlAttribute] ([ControlAttributeId], [ControlId], [Key], [Value], [CreatedBy], [CreatedDate]) 
			VALUES (73, 347, N'InfoIcon', N'Slide', @UserId, @Now)			

			INSERT [UserManagement].[ControlAttribute] ([ControlAttributeId], [ControlId], [Key], [Value], [CreatedBy], [CreatedDate])
			VALUES (74, 353, N'ShowSelectAll', N'True', @UserId, @Now)

            SET IDENTITY_INSERT [UserManagement].[ControlAttribute] OFF

            SET IDENTITY_INSERT [UserManagement].[ProductPage] ON 
              
			INSERT [UserManagement].[ProductPage] ([ProductPageId], [ProductId], [DisplayName], [CreatedBy], [CreatedDate], [IsActive])
			VALUES (23, 54, N'Rent Control Product Access', @UserId, @Now, 1)
  
            SET IDENTITY_INSERT [UserManagement].[ProductPage] OFF
              
            SET IDENTITY_INSERT [UserManagement].[ProductPageControl] ON 
            
			INSERT [UserManagement].[ProductPageControl] ([ProductPageControlId], [ProductPageId], [ControlId], [CreatedBy], [CreatedDate]) 
			VALUES (32, 23, 338, @UserId, @Now)
          
            SET IDENTITY_INSERT [UserManagement].[ProductPageControl] OFF

END

SELECT @ProductId = 30


IF NOT EXISTS (SELECT TOP 1 1 FROM[UserManagement].[ProductPage] WHERE ProductId = @ProductId)
BEGIN
            SET IDENTITY_INSERT [UserManagement].[Control] ON 
			--Parent
			INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
			VALUES (357, NULL, 8, N'PerformanceAnalyticsProductAccessTabGroupUIId', NULL, NULL, 1, @UserId, @Now)

			--Roles
			INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
			VALUES (358, 357, 9, N'PerformanceAnalyticsProductAccessRolesTabUIId', N'Roles', NULL, 1, @UserId, @Now)

			INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate]) 
			VALUES (359, 358, 3, N'PerformanceAnalyticsProductAccessRolesMultiSelectGridUIId', NULL, NULL, 1, @UserId, @Now)
				
			INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
			VALUES (360, 359, 10, N'PerformanceAnalyticsProductAccessCheckboxUIId', NULL, N'isAssigned', 1, @UserId, @Now)
				
			INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
			VALUES (361, 359, 5, N'PerformanceAnalyticsProductAccessRoleLabelUIId', N'Role', N'name', 2, @UserId, @Now)

			--BM Roles
			INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
			VALUES (362, 357, 9, N'PerformanceAnalyticsProductAccessBMRolesTabUIId', N'Benchmarking Role', NULL, 2, @UserId, @Now)

			INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate]) 
			VALUES (363, 362, 3, N'PerformanceAnalyticsProductAccessBMRolesMultiSelectGridUIId', NULL, NULL, 1, @UserId, @Now)
				
			INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
			VALUES (364, 363, 10, N'PerformanceAnalyticsBMProductAccessCheckboxUIId', NULL, N'isAssigned', 1, @UserId, @Now)
				
			INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
			VALUES (365, 363, 5, N'PerformanceAnalyticsBMProductAccessRoleLabelUIId', N'Benchmarking Role', N'name', 2, @UserId, @Now)

			--PropertyGroup
			INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
			VALUES (366, 357, 9, N'PerformanceAnalyticsProductAccessPropertyGroupsTabUIId', N'Property Group', NULL, 2, @UserId, @Now)
			
			INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
			VALUES (367, 366, 3, N'PerformanceAnalyticsProductAccessPropertyGroupsMultiSelectGridUIId', NULL, NULL, 1, @UserId, @Now)
				
			INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate]) 
			VALUES (368, 367, 10, N'PerformanceAnalyticsProductAccessCheckboxUIId', NULL, N'isAssigned', 1, @UserId, @Now)
				
			INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate]) 
			VALUES (369, 367, 5, N'PerformanceAnalyticsProductAccessPropertyGroupLabelUIId', N'Group', N'name', 2, @UserId, @Now)
			--PGSlide
			INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
			VALUES (370, 367, 11, N'PerformanceAnalyticsProductAccessIconUIId', NULL, N'InfoIcon', 4, @UserId, @Now)

			INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
			VALUES (371, 370, 5, N'PerformanceAnalyticsProductAccessPropertyGroupDetailsLabelUIId', N'Property Group Details', NULL, 1, @UserId, @Now)

			INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
			VALUES (372, 370, 12, N'PerformanceAnalyticsProductAccessPropertyGroupDetailsGridUIId', NULL, NULL, 2, @UserId, @Now)

			INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
			VALUES (373, 372, 5, N'PerformanceAnalyticsProductAccessPropertyGroupDetailsPropertyLabelUIId', N'Property', N'name', 2, @UserId, @Now)
			
			INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
			VALUES (374, 372, 5, N'PerformanceAnalyticsProductAccessPropertyGroupDetailsPropertyLabelUIId', N'State', N'state', 2, @UserId, @Now)

			
			--Properties
			INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate]) 
			VALUES (375, 357, 9, N'PerformanceAnalyticsProductAccessPropertiesTabUIId', N'Properties', NULL, 3, @UserId, @Now)
				
			INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
			VALUES (376, 375, 3, N'PerformanceAnalyticsProductAccessPropertiesMultiSelectGridUIId', NULL, NULL, 1, @UserId, @Now)
				
			INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
			VALUES (377, 376, 10, N'PerformanceAnalyticsProductAccessCheckboxUIId', NULL, N'isAssigned', 1, @UserId, @Now)
				
			INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
			VALUES (378, 376, 5, N'PerformanceAnalyticsProductAccessPropertyLabelUIId', N'Property', N'name', 2, @UserId, @Now)
				
			INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate]) 
			VALUES (379, 376, 5, N'PerformanceAnalyticsProductAccessStateLabelUIId', N'State', N'state', 3, @UserId, @Now)	
				
			SET IDENTITY_INSERT [UserManagement].[Control] OFF
              
            SET IDENTITY_INSERT [UserManagement].[ControlAttribute] ON 
            INSERT [UserManagement].[ControlAttribute] ([ControlAttributeId], [ControlId], [Key], [Value], [CreatedBy], [CreatedDate])
			VALUES (75, 358, N'Default', N'True', @UserId, @Now)

			--INSERT [UserManagement].[ControlAttribute] ([ControlAttributeId], [ControlId], [Key], [Value], [CreatedBy], [CreatedDate]) 
			--VALUES (76, 359, N'ShowSelectAll', N'True', @UserId, @Now)

			--INSERT [UserManagement].[ControlAttribute] ([ControlAttributeId], [ControlId], [Key], [Value], [CreatedBy], [CreatedDate]) 
			--VALUES (77, 363, N'ShowSelectAll', N'True', @UserId, @Now)
    
			INSERT [UserManagement].[ControlAttribute] ([ControlAttributeId], [ControlId], [Key], [Value], [CreatedBy], [CreatedDate]) 
			VALUES (78, 367, N'ShowSelectAll', N'True', @UserId, @Now)

			INSERT [UserManagement].[ControlAttribute] ([ControlAttributeId], [ControlId], [Key], [Value], [CreatedBy], [CreatedDate]) 
			VALUES (79, 370, N'InfoIcon', N'Slide', @UserId, @Now)			

			INSERT [UserManagement].[ControlAttribute] ([ControlAttributeId], [ControlId], [Key], [Value], [CreatedBy], [CreatedDate])
			VALUES (80, 376, N'ShowSelectAll', N'True', @UserId, @Now)

            SET IDENTITY_INSERT [UserManagement].[ControlAttribute] OFF

            SET IDENTITY_INSERT [UserManagement].[ProductPage] ON 
              
			INSERT [UserManagement].[ProductPage] ([ProductPageId], [ProductId], [DisplayName], [CreatedBy], [CreatedDate], [IsActive])
			VALUES (24, 30, N'Performance Analytics Product Access', @UserId, @Now, 1)
  
            SET IDENTITY_INSERT [UserManagement].[ProductPage] OFF
              
            SET IDENTITY_INSERT [UserManagement].[ProductPageControl] ON 
            
			INSERT [UserManagement].[ProductPageControl] ([ProductPageControlId], [ProductPageId], [ControlId], [CreatedBy], [CreatedDate]) 
			VALUES (33, 24, 357, @UserId, @Now)
          
            SET IDENTITY_INSERT [UserManagement].[ProductPageControl] OFF

END
-- New API in MT for Resident Portals
 SELECT  @ProductId = 17;

IF @ServerName NOT IN ('RCPGBKDBSQL005A', 'RCPGBKDBSQL005B')
BEGIN
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
			WHERE Name = 'MTApiEndPoint'
		) IS NOT NULL
		BEGIN
			SELECT @ProductSettingTypeId=ProductSettingTypeId
			FROM Enterprise.ProductSettingType
			WHERE Name = 'MTApiEndPoint'
		END;

		IF @ProductSettingTypeId IS NOT NULL AND 
				EXISTS
			(
				SELECT TOP 1 1
				FROM Enterprise.ProductSetting
				WHERE ProductID = @ProductId AND 
					  ProductSettingTypeId = @ProductSettingTypeId AND 
					  ThruDate IS NULL
			)
			BEGIN
				-- Update the Value and assign it to the Product and ProductSettingType

				IF @ServerName IN ('RCTUSODBSQL001', 'RCDUSODBSQL001')
				BEGIN
					UPDATE Enterprise.ProductSetting SET Value='https://api.ocr.activebuilding.com/ulmt',
					@ProductSettingId= ProductSettingId
					WHERE ProductId=@ProductId AND ProductSettingTypeId= @ProductSettingTypeId
				END
				IF @ServerName IN ('RCQUSODBSQL001')
				BEGIN
					UPDATE Enterprise.ProductSetting SET Value='https://api.sat.activebuilding.com/ulmt',
					@ProductSettingId= ProductSettingId
					WHERE ProductId=@ProductId AND ProductSettingTypeId= @ProductSettingTypeId
				END
				IF @ServerName IN ('RCPGBKDBSQL005A', 'RCPGBKDBSQL005B')
				BEGIN
					UPDATE Enterprise.ProductSetting SET Value='https://api.activebuilding.com/ulmt',
					@ProductSettingId= ProductSettingId
					WHERE ProductId=@ProductId AND ProductSettingTypeId= @ProductSettingTypeId
				END
		
		
				-- Link the Product Setting to an actual configuration
				EXEC Enterprise.LinkProductSettingToConfiguration @ConfigurationId = @CurrentProductConfigurationID, -- int
				@ProductSettingId = @ProductSettingId, -- int
				@FromDate = @NOW, -- datetime
				@ThruDate = NULL;   -- datetime
			END;


		IF
		(
			SELECT 1
			FROM Enterprise.ProductSettingType
			WHERE Name = 'AppId'
		) IS NULL
		BEGIN
			EXEC Enterprise.CreateProductSettingType 'AppId', 'The APP Id provided by the Resident Portals for your account', @ProductSettingTypeId OUTPUT;
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
				@Value = 'd8f43b85', 
				@FromDate = @NOW, -- datetime
				@ThruDate = NULL, -- datetime
				@ProductSettingId = @ProductSettingId OUTPUT; -- int

				-- Link the Product Setting to an actual configuration
				EXEC Enterprise.LinkProductSettingToConfiguration @ConfigurationId = @CurrentProductConfigurationID, -- int
				@ProductSettingId = @ProductSettingId, -- int
				@FromDate = @NOW, -- datetime
				@ThruDate = NULL;   -- datetime
			END;

		IF
		(
			SELECT 1
			FROM Enterprise.ProductSettingType
			WHERE Name = 'AppKey'
		) IS NULL
		BEGIN
			EXEC Enterprise.CreateProductSettingType 'AppKey', 'The APP Key provided by the Resident Portals for your account', @ProductSettingTypeId OUTPUT;
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
				@Value = '50aa7342baf824716f87e6999cf4b472', 
				@FromDate = @NOW, -- datetime
				@ThruDate = NULL, -- datetime
				@ProductSettingId = @ProductSettingId OUTPUT; -- int

				-- Link the Product Setting to an actual configuration
				EXEC Enterprise.LinkProductSettingToConfiguration @ConfigurationId = @CurrentProductConfigurationID, -- int
				@ProductSettingId = @ProductSettingId, -- int
				@FromDate = @NOW, -- datetime
				@ThruDate = NULL;   -- datetime
			END;
END;

GO
----LRO Product is not Going in June


--/*This script is a sample script to create new prodcut in the system.*/

--DECLARE @ProductId INT, 
--		@LoginURI NVARCHAR(100), 
--		@SigningCertificateThumbprint NVARCHAR(50), 
--		@ParentProductTypeId INT, 
--		@ProductName NVARCHAR(100)= 'LRO Management',  -- Produact Name
--		@LoginURL NVARCHAR(500), 
--		@ProductUrl NVARCHAR(256), 
--		@apiendpoint NVARCHAR(1000), 
--		@ServerName SYSNAME = @@SERVERNAME;

--DECLARE @ProductConfiguration AS PRODUCTCONFIGURATIONTYPE;

--/*Validate what product type ths new product belongs to. 'Administration' in the following block 
--need to be chnanged to desired prodcut type. You can query Enterprise.ProductType table for more details.
--*/

--SELECT @ParentProductTypeId = ProductTypeId
--FROM Enterprise.ProductType
--WHERE Name = 'Asset Optimization'
--      AND ParentProductTypeId IS NULL;
--IF NOT EXISTS
--(
--    SELECT TOP 1 1
--    FROM enterprise.ProductType
--    WHERE Name = 'LRO Management'
--)
--    BEGIN
--        EXEC [Enterprise].[CreateProductType] 
--             @ProductTypeId = 407, -- Thsi value may change based on the root prodcut type
--             @ParentProductTypeId = @ParentProductTypeId, 
--             @Name = @ProductName, 
--             @Description = @ProductName, 
--             @ProductTypeGUID = 'BC77178D-3972-4236-A2CA-2E8DE104817E'; -- Use newid() to generate new uniqueidentifier.
--END;
--SET @ProductId = 51; -- Assign new product Id

----Following block will create the new prodcut in the database
--IF NOT EXISTS
--(
--    SELECT 1
--    FROM Enterprise.Product
--    WHERE Name = @ProductName
--)
--    BEGIN
--        EXEC Enterprise.CreateProduct 
--             @ProductId = @ProductId, 
--             @ProductGUID = '6EEC0148-D062-4CFA-907B-6FF82773E92E', -- Use newid() to generate new uniqueidentifier.
--             @Name = @ProductName, 
--             @Description = 'LRO', 
--             @ProductTypeId = 407;
        
--		UPDATE Enterprise.Product
--          SET 
--              BooksProductCode = 'LRO'
--        WHERE ProductId = @ProductId;
--END;

----The following block picks up all the detail frm Enterprise.ProductSettingType table
----To set up the product, bunch of these settings are required.
--SET @apiendpoint = '';
--IF @ServerName IN ('RCDUSODBSQL001')
--BEGIN
--	SET @apiendpoint = 'https://aodev.realpage.com/ysconfig/ws/';
--END
--IF @ServerName IN ('rctusodbsql001')
--BEGIN
--	SET @apiendpoint = 'https://aoqa.realpage.com/ysconfig/ws/';
--END
--IF @ServerName IN ('RCQUSODBSQL001')
--BEGIN
--	SET @apiendpoint = 'https://aosat.realpage.com/ysconfig/ws/';
--END
--IF @ServerName IN ('RCPGBKDBSQL005A', 'RCPGBKDBSQL005B')
--BEGIN
--	SET @apiendpoint = 'https://ao.realpage.com/ysconfig/ws/';
--END
--set nocount on
--INSERT INTO @ProductConfiguration
--(SettingName, 
-- SettingDescription, 
-- SettingValue
--)
--VALUES
--('ClientId','','1')
--,('ClassName','','lromanagement')
--,('ProductUrl','','/product/lromanagement')
--,('TitleId','','LRO Management')
--,('TitleUniqueId','','AA32B948-4F1E-4102-ADC9-2E041BF6E918')
--,('IsNewTab','','1')
--,('MetatagUniqueId','','Lease Rent Option Management')
--,('IsResource','','0')
--,('IsFavorite','','1')
--,('LearnMore','','https://www.realpage.com/asset-investment-management/portfolio-asset-management/')
--,('ApiEndPoint','', @apiendpoint)
--,('ApiUserName','','wsuser')
--,('ApiPassword','','cGdAIXcyM3Jn')
--,('ProductSuperUserLoginName','','amungale')
--,('ProductStatus','Show if the external application was configured for the dashboard user.','8')
--,('ProductStatus','Show if the external application was configured for the dashboard user.','10')
--,('ProductStatus','Show if the external application was configured for the dashboard user.','19')
--,('ShowInUserDetails','Should the product show in the New/Edit user pages','1')
--,('ShowInRolesAndRights','Should the product show in the Role/Rights page','0')
--,('ShowInAppSwitcher','Should the product show in the application switcher','1')
--,('ShowInUserListFilter','Should the product show in the user list product pick list','1')
--,('ProductAPIRequiresUser','Does the product require a user for api calls','0')
--,('LockOnProductAccess', '', '0')
--,('ProductNotAvailableForRegularUserNoEmail','Product Attribute for Product Not Available for Regular User No Email.','0')
--,('NotificationEmailRequiredForUserWithNoEmail', '', '1')
--,('AuthenticationType','Used to determine how to log into the product','Redirect')



--SELECT * FROM @ProductConfiguration

--SET @LoginURL = '';
--IF @ServerName IN ('RCDUSODBSQL001')
--BEGIN
--	SET @LoginURL = 'https://aodev.realpage.com/ysconfig/sso/oauth?Product=LRO';
--END
--IF @ServerName IN ('rctusodbsql001')
--BEGIN
--	SET @LoginURL = 'https://aoqa.realpage.com/ysconfig/sso/oauth?Product=LRO';
--END
--IF @ServerName IN ('RCQUSODBSQL001')
--BEGIN
--	SET @LoginURL = 'https://aosat.realpage.com/ysconfig/sso/oauth?Product=LRO';
--END
--IF @ServerName IN ('RCPGBKDBSQL005A', 'RCPGBKDBSQL005B')
--BEGIN
--	SET @LoginURL = 'https://ao.realpage.com/ysconfig/sso/oauth?Product=LRO';
--END

--SET @LoginURI = @LoginURL;
--SET @SigningCertificateThumbprint = NULL;

----Setup the product configurations.
--if not exists (select top 1 1 from Enterprise.ProductSetting where ProductId = @ProductId)
--begin

--	EXEC Enterprise.ProductConfigurationSetup 
--		 @ProductId, 
--		 @LoginURI, 
--		 @SigningCertificateThumbprint, 
--		 @ProductConfiguration;
--end;

--IF NOT EXISTS
--(
--    SELECT 1
--    FROM ident.SamlProductSettings
--    WHERE ProductId = @ProductId
--          AND LoginUri = @LoginURL
--)
--    BEGIN
--        INSERT INTO ident.SamlProductSettings
--        (
--        --SamlProductSettingsId - column value is auto-generated
--        ProductId, 
--        LoginUri, 
--        SigningCertificateThumbprint, 
--        SubjectIdSamlAttribute
--        )
--        VALUES
--        (
--        -- SamlProductSettingsId - int
--        @ProductId, -- ProductId - int
--        @LoginURL, -- LoginUri - nvarchar
--        N'NA', -- SigningCertificateThumbprint - nvarchar
--        N'productUserName' -- SubjectIdSamlAttribute - nvarchar
--        );
--END;
--GO

--AA Product


/*This script is a sample script to create new prodcut in the system.*/

DECLARE @ProductId INT, 
		@LoginURI NVARCHAR(100), 
		@SigningCertificateThumbprint NVARCHAR(50), 
		@ParentProductTypeId INT, 
		@ProductName NVARCHAR(100)= 'Amenity Optimization',  -- Produact Name
		@LoginURL NVARCHAR(500), 
		@ProductUrl NVARCHAR(256),
		@apiendpoint NVARCHAR(1000), 
		@ServerName SYSNAME = @@SERVERNAME;

DECLARE @ProductConfiguration AS PRODUCTCONFIGURATIONTYPE;

/*Validate what product type ths new product belongs to. 'Administration' in the following block 
need to be chnanged to desired prodcut type. You can query Enterprise.ProductType table for more details.
*/

SELECT @ParentProductTypeId = ProductTypeId
FROM Enterprise.ProductType
WHERE Name = 'Asset Optimization'
      AND ParentProductTypeId IS NULL;
IF NOT EXISTS
(
    SELECT TOP 1 1
    FROM enterprise.ProductType
    WHERE Name = 'Amenity Optimization'
)
    BEGIN
        EXEC [Enterprise].[CreateProductType] 
             @ProductTypeId = 408, -- Thsi value may change based on the root prodcut type
             @ParentProductTypeId = @ParentProductTypeId, 
             @Name = @ProductName, 
             @Description = @ProductName, 
             @ProductTypeGUID = '0B33FCAE-EB13-4927-9512-895B554646BE'; -- Use newid() to generate new uniqueidentifier.
END;
SET @ProductId = 52; -- Assign new product Id

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
             @ProductGUID = '0B1BD4AD-D52B-444A-859B-CBADBF3CCA97', -- Use newid() to generate new uniqueidentifier.
             @Name = @ProductName, 
             @Description = 'Amenity Optimization', 
             @ProductTypeId = 408;
        
		UPDATE Enterprise.Product
          SET 
              BooksProductCode = 'AA'
        WHERE ProductId = @ProductId;
END;

--The following block picks up all the detail frm Enterprise.ProductSettingType table
--To set up the product, bunch of these settings are required.
SET @apiendpoint = '';
IF @ServerName IN ('RCDUSODBSQL001')
BEGIN
	SET @apiendpoint = 'https://aodev.realpage.com/ysconfig/ws/';
END
IF @ServerName IN ('rctusodbsql001')
BEGIN
	SET @apiendpoint = 'https://aoqa.realpage.com/ysconfig/ws/';
END
IF @ServerName IN ('RCQUSODBSQL001')
BEGIN
	SET @apiendpoint = 'https://aosat.realpage.com/ysconfig/ws/';
END
IF @ServerName IN ('RCPGBKDBSQL005A', 'RCPGBKDBSQL005B')
BEGIN
	SET @apiendpoint = 'https://ao.realpage.com/ysconfig/ws/';
END
set nocount on
INSERT INTO @ProductConfiguration
(SettingName, 
 SettingDescription, 
 SettingValue
)
VALUES
('ClientId','','1')
,('ClassName','','amenityoptimization')
,('ProductUrl','','/product/amenityoptimization')
,('TitleId','','Amenity Optimization')
,('TitleUniqueId','','A2A04207-CAA7-4E91-B0CD-31D0003D73EC')
,('IsNewTab','','1')
,('MetatagUniqueId','','Amenity Optimization')
,('IsResource','','0')
,('IsFavorite','','1')
,('LearnMore','','https://www.realpage.com/asset-investment-management/portfolio-asset-management/')
,('ApiEndPoint','',@apiendpoint)
,('ApiUserName','','wsuser')
,('ApiPassword','','cGdAIXcyM3Jn')
,('ProductSuperUserLoginName','','amungale')
,('ProductStatus','Show if the external application was configured for the dashboard user.','8')
,('ProductStatus','Show if the external application was configured for the dashboard user.','10')
,('ProductStatus','Show if the external application was configured for the dashboard user.','19')
,('ShowInUserDetails','Should the product show in the New/Edit user pages','1')
,('ShowInRolesAndRights','Should the product show in the Role/Rights page','0')
,('ShowInAppSwitcher','Should the product show in the application switcher','1')
,('ShowInUserListFilter','Should the product show in the user list product pick list','1')
,('ProductAPIRequiresUser','Does the product require a user for api calls','0')
,('LockOnProductAccess', '', '0')
,('ProductNotAvailableForRegularUserNoEmail','Product Attribute for Product Not Available for Regular User No Email.','0')
,('NotificationEmailRequiredForUserWithNoEmail', '', '1')
,('AuthenticationType','Used to determine how to log into the product','Redirect')



SELECT * FROM @ProductConfiguration

SET @LoginURL = '';
IF @ServerName IN ('RCDUSODBSQL001')
BEGIN
	SET @LoginURL = 'https://aodev.realpage.com/ysconfig/sso/oauth?Product=AA';
END
IF @ServerName IN ('rctusodbsql001')
BEGIN
	SET @LoginURL = 'https://aoqa.realpage.com/ysconfig/sso/oauth?Product=AA';
END
IF @ServerName IN ('RCQUSODBSQL001')
BEGIN
	SET @LoginURL = 'https://aosat.realpage.com/ysconfig/sso/oauth?Product=AA';
END
IF @ServerName IN ('RCPGBKDBSQL005A', 'RCPGBKDBSQL005B')
BEGIN
	SET @LoginURL = 'https://ao.realpage.com/ysconfig/sso/oauth?Product=AA';
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

--AIRM Product 


/*This script is a sample script to create new prodcut in the system.*/

DECLARE @ProductId INT, 
		@LoginURI NVARCHAR(100), 
		@SigningCertificateThumbprint NVARCHAR(50), 
		@ParentProductTypeId INT, 
		@ProductName NVARCHAR(100)= 'AI Revenue Management',  -- Produact Name
		@LoginURL NVARCHAR(500), 
		@ProductUrl NVARCHAR(256), 
		@apiendpoint NVARCHAR(1000), 
		@ServerName SYSNAME = @@SERVERNAME;

DECLARE @ProductConfiguration AS PRODUCTCONFIGURATIONTYPE;

/*Validate what product type ths new product belongs to. 'Administration' in the following block 
need to be chnanged to desired prodcut type. You can query Enterprise.ProductType table for more details.
*/

SELECT @ParentProductTypeId = ProductTypeId
FROM Enterprise.ProductType
WHERE Name = 'Asset Optimization'
      AND ParentProductTypeId IS NULL;
IF NOT EXISTS
(
    SELECT TOP 1 1
    FROM enterprise.ProductType
    WHERE Name = 'AI Revenue Management'
)
    BEGIN
        EXEC [Enterprise].[CreateProductType] 
             @ProductTypeId = 409, -- Thsi value may change based on the root prodcut type
             @ParentProductTypeId = @ParentProductTypeId, 
             @Name = @ProductName, 
             @Description = @ProductName, 
             @ProductTypeGUID = '7210B899-0D0A-4EB7-BB19-71900E2AD2A5'; -- Use newid() to generate new uniqueidentifier.
END;
SET @ProductId = 53; -- Assign new product Id

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
             @ProductGUID = 'C8DE2E25-FEB6-4CF4-AF1E-9E654FA0FBEF', -- Use newid() to generate new uniqueidentifier.
             @Name = @ProductName, 
             @Description = 'AI Revenue Management', 
             @ProductTypeId = 409;
        
		UPDATE Enterprise.Product
          SET 
              BooksProductCode = 'AIRM'
        WHERE ProductId = @ProductId;
END;

--The following block picks up all the detail frm Enterprise.ProductSettingType table
--To set up the product, bunch of these settings are required.
SET @apiendpoint = '';
IF @ServerName IN ('RCDUSODBSQL001')
BEGIN
	SET @apiendpoint = 'https://aodev.realpage.com/ysconfig/ws/';
END
IF @ServerName IN ('rctusodbsql001')
BEGIN
	SET @apiendpoint = 'https://aoqa.realpage.com/ysconfig/ws/';
END
IF @ServerName IN ('RCQUSODBSQL001')
BEGIN
	SET @apiendpoint = 'https://aosat.realpage.com/ysconfig/ws/';
END
IF @ServerName IN ('RCPGBKDBSQL005A', 'RCPGBKDBSQL005B')
BEGIN
	SET @apiendpoint = 'https://ao.realpage.com/ysconfig/ws/';
END
set nocount on
INSERT INTO @ProductConfiguration
(SettingName, 
 SettingDescription, 
 SettingValue
)
VALUES
('ClientId','','1')
,('ClassName','','airevenuemanagement')
,('ProductUrl','','/product/airevenuemanagement')
,('TitleId','','Amenity Optimization')
,('TitleUniqueId','','48DA3DC0-7EC7-403A-88A0-341CF1CC9D92')
,('IsNewTab','','1')
,('MetatagUniqueId','','AI Revenue Management')
,('IsResource','','0')
,('IsFavorite','','1')
,('LearnMore','','https://www.realpage.com/asset-investment-management/portfolio-asset-management/')
,('ApiEndPoint','',@apiendpoint)
,('ApiUserName','','wsuser')
,('ApiPassword','','cGdAIXcyM3Jn')
,('ProductSuperUserLoginName','','amungale')
,('ProductStatus','Show if the external application was configured for the dashboard user.','8')
,('ProductStatus','Show if the external application was configured for the dashboard user.','10')
,('ProductStatus','Show if the external application was configured for the dashboard user.','19')
,('ShowInUserDetails','Should the product show in the New/Edit user pages','1')
,('ShowInRolesAndRights','Should the product show in the Role/Rights page','0')
,('ShowInAppSwitcher','Should the product show in the application switcher','1')
,('ShowInUserListFilter','Should the product show in the user list product pick list','1')
,('ProductAPIRequiresUser','Does the product require a user for api calls','0')
,('LockOnProductAccess', '', '0')
,('ProductNotAvailableForRegularUserNoEmail','Product Attribute for Product Not Available for Regular User No Email.','0')
,('NotificationEmailRequiredForUserWithNoEmail', '', '1')
,('AuthenticationType','Used to determine how to log into the product','Redirect')



SELECT * FROM @ProductConfiguration

SET @LoginURL = '';
IF @ServerName IN ('RCDUSODBSQL001')
BEGIN
	SET @LoginURL = 'https://aodev.realpage.com/ysconfig/sso/oauth?Product=AIRM';
END
IF @ServerName IN ('rctusodbsql001')
BEGIN
	SET @LoginURL = 'https://aoqa.realpage.com/ysconfig/sso/oauth?Product=AIRM';
END
IF @ServerName IN ('RCQUSODBSQL001')
BEGIN
	SET @LoginURL = 'https://aosat.realpage.com/ysconfig/sso/oauth?Product=AIRM';
END
IF @ServerName IN ('RCPGBKDBSQL005A', 'RCPGBKDBSQL005B')
BEGIN
	SET @LoginURL = 'https://ao.realpage.com/ysconfig/sso/oauth?Product=AIRM';
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



-- RC Product



/*This script is a sample script to create new prodcut in the system.*/

DECLARE @ProductId INT, 
		@LoginURI NVARCHAR(100), 
		@SigningCertificateThumbprint NVARCHAR(50), 
		@ParentProductTypeId INT, 
		@ProductName NVARCHAR(100)= 'Rent Control',  -- Produact Name
		@LoginURL NVARCHAR(500), 
		@ProductUrl NVARCHAR(256), 
		@apiendpoint NVARCHAR(1000), 
		@ServerName SYSNAME = @@SERVERNAME;

DECLARE @ProductConfiguration AS PRODUCTCONFIGURATIONTYPE;

/*Validate what product type ths new product belongs to. 'Administration' in the following block 
need to be chnanged to desired prodcut type. You can query Enterprise.ProductType table for more details.
*/

SELECT @ParentProductTypeId = ProductTypeId
FROM Enterprise.ProductType
WHERE Name = 'Asset Optimization'
      AND ParentProductTypeId IS NULL;
IF NOT EXISTS
(
    SELECT TOP 1 1
    FROM enterprise.ProductType
    WHERE Name = 'Rent Control'
)
    BEGIN
        EXEC [Enterprise].[CreateProductType] 
             @ProductTypeId = 410, -- Thsi value may change based on the root prodcut type
             @ParentProductTypeId = @ParentProductTypeId, 
             @Name = @ProductName, 
             @Description = @ProductName, 
             @ProductTypeGUID = 'FA9E0072-7139-478D-A49D-C2E172D790E7'; -- Use newid() to generate new uniqueidentifier.
END;
SET @ProductId = 54; -- Assign new product Id

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
             @ProductGUID = '56FEC3BC-F189-4255-9CD4-679FB9A172FB', -- Use newid() to generate new uniqueidentifier.
             @Name = @ProductName, 
             @Description = 'Rent Control', 
             @ProductTypeId = 410;
        
		UPDATE Enterprise.Product
          SET 
              BooksProductCode = 'RC'
        WHERE ProductId = @ProductId;
END;

--The following block picks up all the detail frm Enterprise.ProductSettingType table
--To set up the product, bunch of these settings are required.

SET @apiendpoint = '';
IF @ServerName IN ('RCDUSODBSQL001')
BEGIN
	SET @apiendpoint = 'https://aodev.realpage.com/ysconfig/ws/';
END
IF @ServerName IN ('rctusodbsql001')
BEGIN
	SET @apiendpoint = 'https://aoqa.realpage.com/ysconfig/ws/';
END
IF @ServerName IN ('RCQUSODBSQL001')
BEGIN
	SET @apiendpoint = 'https://aosat.realpage.com/ysconfig/ws/';
END
IF @ServerName IN ('RCPGBKDBSQL005A', 'RCPGBKDBSQL005B')
BEGIN
	SET @apiendpoint = 'https://ao.realpage.com/ysconfig/ws/';
END

set nocount on
INSERT INTO @ProductConfiguration
(SettingName, 
 SettingDescription, 
 SettingValue
)
VALUES
('ClientId','','1')
,('ClassName','','rentcontrol')
,('ProductUrl','','/product/rentcontrol')
,('TitleId','','Rent Control')
,('TitleUniqueId','','B4D6F1BD-933E-4061-A4D3-E64A6F89609A')
,('IsNewTab','','1')
,('MetatagUniqueId','','Rent Control')
,('IsResource','','0')
,('IsFavorite','','1')
,('LearnMore','','https://www.realpage.com/asset-investment-management/portfolio-asset-management/')
,('ApiEndPoint','', @apiendpoint)
,('ApiUserName','','wsuser')
,('ApiPassword','','cGdAIXcyM3Jn')
,('ProductSuperUserLoginName','','amungale')
,('ProductStatus','Show if the external application was configured for the dashboard user.','8')
,('ProductStatus','Show if the external application was configured for the dashboard user.','10')
,('ProductStatus','Show if the external application was configured for the dashboard user.','19')
,('ShowInUserDetails','Should the product show in the New/Edit user pages','1')
,('ShowInRolesAndRights','Should the product show in the Role/Rights page','0')
,('ShowInAppSwitcher','Should the product show in the application switcher','1')
,('ShowInUserListFilter','Should the product show in the user list product pick list','1')
,('ProductAPIRequiresUser','Does the product require a user for api calls','0')
,('LockOnProductAccess', '', '0')
,('ProductNotAvailableForRegularUserNoEmail','Product Attribute for Product Not Available for Regular User No Email.','0')
,('NotificationEmailRequiredForUserWithNoEmail', '', '1')
,('AuthenticationType','Used to determine how to log into the product','Redirect')



SELECT * FROM @ProductConfiguration

SET @LoginURL = '';
IF @ServerName IN ('RCDUSODBSQL001')
BEGIN
	SET @LoginURL = 'https://aodev.realpage.com/ysconfig/sso/oauth?Product=RC';
END
IF @ServerName IN ('rctusodbsql001')
BEGIN
	SET @LoginURL = 'https://aoqa.realpage.com/ysconfig/sso/oauth?Product=RC';
END
IF @ServerName IN ('RCQUSODBSQL001')
BEGIN
	SET @LoginURL = 'https://aosat.realpage.com/ysconfig/sso/oauth?Product=RC';
END
IF @ServerName IN ('RCPGBKDBSQL005A', 'RCPGBKDBSQL005B')
BEGIN
	SET @LoginURL = 'https://ao.realpage.com/ysconfig/sso/oauth?Product=RC';
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

if not exists(Select top 1 1 from Enterprise.ProductSettingType where Name = 'UPFMOrderIgnoreEnvironment')
Begin
	Insert into Enterprise.ProductSettingType (Name, Description) Values ('UPFMOrderIgnoreEnvironment', 'Ignore environment for new company creation from UPFM Order create Tibco events')
End
GO

---- 99688 [GB-6736] Unified Login integration with Senior Lead Management
---- 207273 Add text for the Senior Lead Management tile text and "Learn More" URL

/*This script is a sample script to create new prodcut in the system.*/

DECLARE @ProductId INT, 
		@LoginURI NVARCHAR(100), 
		@SigningCertificateThumbprint NVARCHAR(50), 
		@ParentProductTypeId INT, 
		@ProductName NVARCHAR(100)= 'Senior Lead Management',  -- Produact Name
		@LoginURL NVARCHAR(500), 
		@ProductUrl NVARCHAR(256), 
		@ServerName SYSNAME = @@SERVERNAME;

DECLARE @ProductConfiguration AS PRODUCTCONFIGURATIONTYPE;

/*Validate what product type ths new product belongs to. 'Administration' in the following block 
need to be chnanged to desired prodcut type. You can query Enterprise.ProductType table for more details.
*/

SELECT @ParentProductTypeId = ProductTypeId
FROM Enterprise.ProductType
WHERE Name = 'Lease Management'
      AND ParentProductTypeId IS NULL;
IF NOT EXISTS
(
    SELECT TOP 1 1
    FROM enterprise.ProductType
    WHERE Name = 'Senior Lead Management'
)
    BEGIN
        EXEC [Enterprise].[CreateProductType] 
             @ProductTypeId = 311, -- Thsi value may change based on the root prodcut type
             @ParentProductTypeId = @ParentProductTypeId, 
             @Name = @ProductName, 
             @Description = @ProductName, 
             @ProductTypeGUID = '39BECAD7-19C7-4B07-86A6-EDD424BE12BF'; -- Use newid() to generate new uniqueidentifier.
END;
SET @ProductId = 50; -- Assign new product Id

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
             @ProductGUID = '97EED28D-93DC-4435-9A26-07B33A162839', -- Use newid() to generate new uniqueidentifier.
             @Name = @ProductName, 
             @Description = 'Senior Lead Manager is a lead and referrer management platform designed to help sales agents and leasing staff to manage leads from a variety of sources. With Senior Lead Manager, users can do lead capture, lead and prospect nurturing, follow-up tasks, reminders, reports and referrer management in one convenient location. This is an internal tool designed for the on-site staff.', 
             @ProductTypeId = 311;
        UPDATE Enterprise.Product
          SET 
              BooksProductCode = 'SLM'
        WHERE ProductId = @ProductId;
END;

--The following block picks up all the detail frm Enterprise.ProductSettingType table
--To set up the product, bunch of these settings are required.
set nocount on
INSERT INTO @ProductConfiguration
(SettingName, 
 SettingDescription, 
 SettingValue
)
VALUES
('ClientId','','1')
,('ClassName','','seniorleadmanagement')
,('ProductUrl','','/product/seniorleadmanagement')
,('TitleId','','Senior Lead Management')
,('TitleUniqueId','','EDFB27F1-6335-4297-B44F-F265204B0538')
,('IsNewTab','','1')
,('MetatagUniqueId','','Senior Lead Management')
,('IsResource','','0')
,('IsFavorite','','1')
,('LearnMore','','https://www.realpage.com/senior/')
,('ApiEndPoint','','https://dev-spmadminbff.realpage.com/api/v1')
,('ProductStatus','Show if the external application was configured for the dashboard user.','8')
,('ShowInUserDetails','Should the product show in the New/Edit user pages','1')
,('ShowInRolesAndRights','Should the product show in the Role/Rights page','0')
,('ShowInAppSwitcher','Should the product show in the application switcher','1')
,('ShowInUserListFilter','Should the product show in the user list product pick list','1')
,('ProductAPIRequiresUser','Does the product require a user for api calls','0')
,('LockOnProductAccess', '', '0')
,('ProductNotAvailableForRegularUserNoEmail','Product Attribute for Product Not Available for Regular User No Email.','0')

--,('ApiUserName','','test+rfqarpunity@novelpay.com')
--,('ApiPassword','','7mgp43EIvc8c!@')
,('ApiKey','','53448358-FC1C-4B30-8C45-1171B06D84D1') -- For DEV Environment

,('GetRoleEndpoint','Role End point for product API','/{0}/Roles?isIncludeRights={1}')
,('GetRightEndpoint','Right End point for product API','/roleRights/{0}')

,('GetPropertyEndpoint','Property End point for product API','/properties/{0}')
,('GetUserEndpoint','GET User Endpoint for product API','/users?loginName={0}')
,('GetListUsersEndpoint','','/users/{0}?filter={1}&pageNumber={2}&PageSize={3}')
,('PostUserEndpoint','POST User Endpoint for product API','/users')
,('PutUserEndpoint','PUT User Endpoint for product API','/users')
,('DeleteUserEndpoint','DELETE User Endpoint for product API','/users?loginName={0}') 
,('PatchMigrateUsersEndpoint','Patch Migrate Users Endpoint', '/users/{0}/migrate')
,('PatchProfileEndpoint','PATCH Profile Endpoint for product API','/userprofile')
,('GetUserExistEndpoint','Get User Exist Endpoint for product API','/userexists?loginName={0}') -- Made New Setting
,('AuthenticationType','Used to determine how to log into the product','Redirect')
--- Not sure about below ones -------
--,('GetCompanyEndpoint','GET Company Endpoint for API','/orgs/{0}')
--,('GetParentCompanyEndpoint','GET Company Endpoint for API','/orgs?parentOrgId={0}')
--,('GetProfileEndpoint','GET User Profile Endpoint for product API','/users/{0}/profile')


SELECT * FROM @ProductConfiguration

SET @LoginURL = 'http://dev-spm.realpage.com/oidcHanlder';

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
SET @RightName = 'Manage Senior Lead Management Product Access'; -- Name of the right 
SET @RightDescription = 'Manage Senior Lead Management Product Access'; --Description of the right as stated in story.
SET @RightShortName = 'ManageSeniorLeadManagement'; --Short name of the right that is being used by the application
SET @ActionName = 'Manage Senior Lead Management'; -- This specifically pertains to actions used for routing purposes. 
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
WHERE Name = 'Senior Lead Management';

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


