DECLARE @UserId bigint,
	@ProductId int = 44,
	@productSettingTypeId INT,
	@productGroupSettingTypeId INT,
	@ParentControlID INT = 496,
	@MaxControlId INT,
	@MaxControlAttributeId INT,
	@Now datetime = GETDATE();

SELECT	@UserId = UserId
FROM	Ident.UserLogin
WHERE	LoginName LIKE 'realpagead@%'

IF EXISTS (SELECT TOP 1 1 FROM[UserManagement].[ProductPage] WHERE ProductId = @ProductId)
BEGIN

SELECT @MaxControlId = max(ControlId) from UserManagement.Control

SET IDENTITY_INSERT [UserManagement].[Control] ON 

INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate]) 
VALUES (@MaxControlId + 1, @ParentControlID, 14, N'PortfolioManagementProductAccessAssignedGroupsLinkLabelUIId', N'Assigned Groups', N'assignedGroups', 3, @UserId, @Now)

INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate]) 
VALUES (@MaxControlId + 2, @MaxControlId + 1, 5, N'PortfolioManagementProductAccessAssignedGroupsLabelUIId', N'Groups', NULL, 1, @UserId, @Now)

INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate]) 
VALUES (@MaxControlId + 3, @MaxControlId + 1, 12, N'PortfolioManagementProductAccessAssignedGroupsMultiSelectGridUIId', NULL, NULL, 2, @UserId, @Now)

INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate]) 
VALUES (@MaxControlId + 4, @MaxControlId + 3, 10, N'PortfolioManagementProductAccessCheckboxUIId', NULL, N'isAssigned', 1, @UserId, @Now)

INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate]) 
VALUES (@MaxControlId + 5, @MaxControlId + 3, 5, N'PortfolioManagementProductAccessGroupLabelUIId', N'Group', N'name', 2, @UserId, @Now)

INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate]) 
VALUES (@MaxControlId + 6, @MaxControlId + 3, 11, N'PortfolioManagementProductAccessIconUIId', NULL, N'InfoIcon', 3, @UserId, @Now)

INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate]) 
VALUES (@MaxControlId + 7, @MaxControlId + 6, 5, N'PortfolioManagementProductAccessEntityDetailsLabelUIId', N'Entity Group Details', NULL, 1, @UserId, @Now)

INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate]) 
VALUES (@MaxControlId + 8, @MaxControlId + 6, 12, N'PortfolioManagementProductAccessGridUIId', NULL, NULL, 2, @UserId, @Now)

INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate]) 
VALUES (@MaxControlId + 9, @MaxControlId + 8, 5, N'PortfolioManagementProductAccessEntityLabelUIId', N'Entity', N'name', 1, @UserId, @Now)

INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate]) 
VALUES (@MaxControlId + 10, @MaxControlId + 8, 5, N'PortfolioManagementProductAccessEntityTypeLabelUIId', N'Type', N'propertyType', 2, @UserId, @Now)

SET IDENTITY_INSERT [UserManagement].[Control] OFF 


SELECT @MaxControlAttributeId = max(ControlAttributeId) from [UserManagement].[ControlAttribute]
SET IDENTITY_INSERT [UserManagement].[ControlAttribute] ON

INSERT [UserManagement].[ControlAttribute] ([ControlAttributeId], [ControlId], [Key], [Value], [CreatedBy], [CreatedDate]) 
VALUES (@MaxControlAttributeId + 1, @MaxControlId + 1, N'AssignedGroups', N'Slide', @UserId, @Now)

INSERT [UserManagement].[ControlAttribute] ([ControlAttributeId], [ControlId], [Key], [Value], [CreatedBy], [CreatedDate]) 
VALUES (@MaxControlAttributeId + 2, @MaxControlId + 6, N'InfoIcon', N'Slide', @UserId, @Now)

SET IDENTITY_INSERT [UserManagement].[ControlAttribute] OFF
END

select @productGroupSettingTypeId = ProductSettingTypeId from Enterprise.ProductSettingType where [Name] = 'GetPropertyGroupsEndpoint'

IF(@productGroupSettingTypeId IS NOT NULL)
BEGIN
	INSERT INTO Enterprise.ProductSetting(ProductId,ProductSettingTypeId,Value,FromDate,ThruDate)
	VALUES(@ProductId, @productGroupSettingTypeId, '/api/{0}/UserPropertyGroups', @Now, NULL)
END

IF NOT EXISTS (select top 1 1 from Enterprise.ProductSettingType where [Name] = 'GetPropertyByGroupEndpoint')
BEGIN
	INSERT INTO Enterprise.ProductSettingType([Name],[Description])
	VALUES('GetPropertyByGroupEndpoint','GET Properties By Group Endpoint for product API') 
END

select @productSettingTypeId = ProductSettingTypeId from Enterprise.ProductSettingType where [Name] = 'GetPropertyByGroupEndpoint'

IF(@productSettingTypeId IS NOT NULL)
BEGIN
	INSERT INTO Enterprise.ProductSetting(ProductId,ProductSettingTypeId,Value,FromDate,ThruDate)
	VALUES(@ProductId, @productSettingTypeId, '/api/{0}/UserPropertyGroupsById?propertyGroupId={1}', @Now, NULL)
END