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
IF NOT EXISTS (SELECT 1 FROM [UserManagement].[ControlType] WHERE Name = 'Group Grid')
BEGIN
	Insert into [UserManagement].[ControlType](name,Description,CreatedBy,CreatedDate)
	Select 'Group Grid','Read Only Group Grid',@UserId,@Now
END

IF NOT EXISTS (SELECT 1 FROM [UserManagement].[ControlType] WHERE Name = 'Multi Select Group Grid')
BEGIN
	Insert into [UserManagement].[ControlType](name,Description,CreatedBy,CreatedDate)
	Select 'Multi Select Group Grid','Multiple Selection Group Grid',@UserId,@Now
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

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
	VALUES (@ControlId+23, @ControlId+20, 5, N'UnifiedPlatformRolesAndRightsRightTypeLabelUIId', N'Type', N'roletype', 2, @UserId, @Now)

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
GO
DECLARE @UserId bigint,
	@ProductId int = 44,
	@productSettingId INT,
	@productSettingTypeId INT,
	@productGroupSettingTypeId INT,
	@ConfigurationId INT,
	@ParentControlID INT,
	@ControlID INT,
	@MaxControlId INT,
	@MaxControlAttributeId INT,
	@Now datetime = GETDATE();

SELECT	@UserId = UserId
FROM	Ident.UserLogin
WHERE	LoginName LIKE 'realpagead@%'

IF NOT EXISTS (SELECT TOP 1 1 FROM[UserManagement].[Control] WHERE UIID = 'PortfolioManagementProductAccessAssignedGroupsLinkLabelUIId')
BEGIN

SELECT @MaxControlId = max(ControlId) from UserManagement.Control
select @ParentControlID = ControlId from UserManagement.COntrol where UIID = 'PortfolioManagementProductAccessEntityRolesMultiSelectGridUIId'

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

IF EXISTS (SELECT TOP 1 1 FROM[UserManagement].[Control] WHERE UIID = 'PortfolioManagementProductAccessAssignedEntitiesLinkLabelUIId' AND [Sequence] = 3)
BEGIN
	SELECT @ControlID = ControlID from [UserManagement].[Control] where UIID = 'PortfolioManagementProductAccessAssignedEntitiesLinkLabelUIId'
	UPDATE [UserManagement].[Control] SET [Sequence] = 4 WHERE ControlID = @ControlID
END

SELECT @productGroupSettingTypeId = ProductSettingTypeId from Enterprise.ProductSettingType where [Name] = 'GetPropertyGroupsEndpoint'
SELECT TOP 1 @ConfigurationId = ConfigurationId from Enterprise.ProductConfiguration where ProductSettingId in (select ProductSettingId from Enterprise.ProductSetting where ProductId = @ProductId)

IF NOT EXISTS(SELECT TOP 1 1 FROM Enterprise.ProductSetting where ProductId = @ProductId and [Value] = '/api/{0}/UserPropertyGroups')
BEGIN
 
	INSERT INTO Enterprise.ProductSetting(ProductId,ProductSettingTypeId,Value,FromDate,ThruDate)
	VALUES(@ProductId, @productGroupSettingTypeId, '/api/{0}/UserPropertyGroups', @Now, NULL)
	
	SELECT @productSettingId = SCOPE_IDENTITY()
	
	INSERT INTO Enterprise.ProductConfiguration(ConfigurationId, ProductSettingId, FromDate, ThruDate)
	VALUES(@ConfigurationId, @productSettingId, @Now, NULL)
END

IF NOT EXISTS (select top 1 1 from Enterprise.ProductSettingType where [Name] = 'GetPropertyByGroupEndpoint')
BEGIN
	INSERT INTO Enterprise.ProductSettingType([Name],[Description])
	VALUES('GetPropertyByGroupEndpoint','GET Properties By Group Endpoint for product API') 
END

select @productSettingTypeId = ProductSettingTypeId from Enterprise.ProductSettingType where [Name] = 'GetPropertyByGroupEndpoint'

IF NOT EXISTS(SELECT TOP 1 1 FROM Enterprise.ProductSetting where ProductId = @ProductId and [Value] = '/api/{0}/UserPropertyGroupsById?propertyGroupId={1}')
BEGIN
	INSERT INTO Enterprise.ProductSetting(ProductId,ProductSettingTypeId,Value,FromDate,ThruDate)
	VALUES(@ProductId, @productSettingTypeId, '/api/{0}/UserPropertyGroupsById?propertyGroupId={1}', @Now, NULL)

	SELECT @productSettingId = SCOPE_IDENTITY()
	
	INSERT INTO Enterprise.ProductConfiguration(ConfigurationId, ProductSettingId, FromDate, ThruDate)
	VALUES(@ConfigurationId, @productSettingId, @Now, NULL)
END

GO

if not exists ( select top (1) 1 from Enterprise.ProductSettingType where name = 'NotificationsApiEndPoint' )
begin
	INSERT INTO Enterprise.ProductSettingType ( name, Description, SensitiveData ) values ( 'NotificationsApiEndPoint', 'The api endpoint for Unified Notifications', 0 )
end
GO

If not exists ( select top (1) 1 from Enterprise.ProductSettingType where name = 'NotificationsEventsEndPoint' )
begin
	INSERT INTO Enterprise.ProductSettingType ( name, Description, SensitiveData ) values ( 'NotificationsEventsEndPoint', 'The api endpoint for Unified Notification events', 0 )
end
GO

If not exists ( select top (1) 1 from Enterprise.ProductSettingType where name = 'NotificationsEventChangeCompany' )
begin
	INSERT INTO Enterprise.ProductSettingType ( name, Description, SensitiveData ) values ( 'NotificationsEventChangeCompany', 'The event name used to trigger the Change Company event from Notifications', 0 )
end
GO

If not exists ( select top (1) 1 from Enterprise.ProductSettingType where name = 'UnifiedLoginServerClientName' )
begin
	INSERT INTO Enterprise.ProductSettingType ( name, Description, SensitiveData ) values ( 'UnifiedLoginServerClientName', 'The client name for Unified Login server side client', 0 )
end
GO

If not exists ( select TOP (1) 1 from Enterprise.ProductSettingType where name = 'UnifiedLoginServerClientSecret' )
begin
	INSERT INTO Enterprise.ProductSettingType ( name, Description, SensitiveData ) values ( 'UnifiedLoginServerClientSecret', 'The client secret for Unified Login server side client', 1 )
end
GO



if not exists(Select top (1) 1 from Enterprise.ProductSetting ps 
				inner join Enterprise.ProductSettingType pst
				on ps.ProductSettingTypeId = pst.ProductSettingTypeId
				where pst.Name = 'NotificationsApiEndPoint' and ps.ProductId= 3)
Begin
	Insert into Enterprise.ProductSetting (ProductId, ProductSettingTypeId, Value, FromDate)
	Select 3, ProductSettingTypeId, 'https://notifications-api-dev.realpage.com', GETUTCDATE()
	from Enterprise.ProductSettingType
	where Name = 'NotificationsApiEndPoint'

	declare @productsettingid int
	select @productsettingid = productsettingid from Enterprise.ProductSetting ps 
				inner join Enterprise.ProductSettingType pst
				on ps.ProductSettingTypeId = pst.ProductSettingTypeId
				where pst.Name = 'NotificationsApiEndPoint' and ps.ProductId= 3

	insert into enterprise.ProductConfiguration ( ConfigurationId, ProductSettingId, FromDate )
				select TOP (1) ConfigurationId, @productsettingid, GETUTCDATE() from enterprise.GlobalProductConfiguration where productid = 3 and thrudate is NULL ORDER BY GlobalProductConfigurationId DESC
end
GO

if not exists(Select top (1) 1 from Enterprise.ProductSetting ps 
				inner join Enterprise.ProductSettingType pst
				on ps.ProductSettingTypeId = pst.ProductSettingTypeId
				where pst.Name = 'NotificationsEventsEndPoint' and ps.ProductId= 3)
Begin
	Insert into Enterprise.ProductSetting (ProductId, ProductSettingTypeId, Value, FromDate)
	Select 3, ProductSettingTypeId, 'v1/events', GETUTCDATE()
	from Enterprise.ProductSettingType
	where Name = 'NotificationsEventsEndPoint'

	declare @productsettingid int
	select @productsettingid = productsettingid from Enterprise.ProductSetting ps 
				inner join Enterprise.ProductSettingType pst
				on ps.ProductSettingTypeId = pst.ProductSettingTypeId
				where pst.Name = 'NotificationsEventsEndPoint' and ps.ProductId= 3

	insert into enterprise.ProductConfiguration ( ConfigurationId, ProductSettingId, FromDate )
				select TOP (1) ConfigurationId, @productsettingid, GETUTCDATE() from enterprise.GlobalProductConfiguration where productid = 3 and thrudate is NULL ORDER BY GlobalProductConfigurationId DESC
end
GO

if not exists(Select top 1 1 from Enterprise.ProductSetting ps 
				inner join Enterprise.ProductSettingType pst
				on ps.ProductSettingTypeId = pst.ProductSettingTypeId
				where pst.Name = 'UnifiedLoginServerClientName' and ps.ProductId= 3)
Begin
	Insert into Enterprise.ProductSetting (ProductId, ProductSettingTypeId, Value, FromDate)
	Select 3, ProductSettingTypeId, 'unifiedlogin-server', GETUTCDATE()
	from Enterprise.ProductSettingType
	where Name = 'UnifiedLoginServerClientName'

	declare @productsettingid int
	select @productsettingid = productsettingid from Enterprise.ProductSetting ps 
				inner join Enterprise.ProductSettingType pst
				on ps.ProductSettingTypeId = pst.ProductSettingTypeId
				where pst.Name = 'UnifiedLoginServerClientName' and ps.ProductId= 3

	insert into enterprise.ProductConfiguration ( ConfigurationId, ProductSettingId, FromDate )
		select TOP (1) ConfigurationId, @productsettingid, GETUTCDATE() from enterprise.GlobalProductConfiguration where productid = 3 and thrudate is NULL ORDER BY GlobalProductConfigurationId DESC
end
GO


if not exists(Select top 1 1 from Enterprise.ProductSetting ps 
				inner join Enterprise.ProductSettingType pst
				on ps.ProductSettingTypeId = pst.ProductSettingTypeId
				where pst.Name = 'NotificationsEventChangeCompany' and ps.ProductId= 3)
Begin
	Insert into Enterprise.ProductSetting (ProductId, ProductSettingTypeId, Value, FromDate)
	Select 3, ProductSettingTypeId, 'changeCompanyMessage', GETUTCDATE()
	from Enterprise.ProductSettingType
	where Name = 'NotificationsEventChangeCompany'

	declare @productsettingid int
	select @productsettingid = productsettingid from Enterprise.ProductSetting ps 
				inner join Enterprise.ProductSettingType pst
				on ps.ProductSettingTypeId = pst.ProductSettingTypeId
				where pst.Name = 'NotificationsEventChangeCompany' and ps.ProductId= 3

	insert into enterprise.ProductConfiguration ( ConfigurationId, ProductSettingId, FromDate )
		select TOP (1) ConfigurationId, @productsettingid, GETUTCDATE() from enterprise.GlobalProductConfiguration where productid = 3 and thrudate is NULL ORDER BY GlobalProductConfigurationId DESC
end
GO

if not exists(Select top 1 1 from Enterprise.ProductSetting ps 
				inner join Enterprise.ProductSettingType pst
				on ps.ProductSettingTypeId = pst.ProductSettingTypeId
				where pst.Name = 'UnifiedLoginServerClientSecret' and ps.ProductId= 3)
Begin
	Insert into Enterprise.ProductSetting (ProductId, ProductSettingTypeId, Value, FromDate)
	Select 3, ProductSettingTypeId, 'QUVFNjNDNTQtREMzMS00MDZDLUJCODEtQzU1ODk4RkI5M0Ix', GETUTCDATE()
	from Enterprise.ProductSettingType
	where Name = 'UnifiedLoginServerClientSecret'

	declare @productsettingid int
	select @productsettingid = productsettingid from Enterprise.ProductSetting ps 
				inner join Enterprise.ProductSettingType pst
				on ps.ProductSettingTypeId = pst.ProductSettingTypeId
				where pst.Name = 'UnifiedLoginServerClientSecret' and ps.ProductId= 3

	insert into enterprise.ProductConfiguration ( ConfigurationId, ProductSettingId, FromDate )
        select TOP (1) ConfigurationId, @productsettingid, GETUTCDATE() from enterprise.GlobalProductConfiguration where productid = 3 and thrudate is NULL ORDER BY GlobalProductConfigurationId DESC
end
GO

--Vendor Credential product panel changes
IF NOT EXISTS(SELECT TOP 1 1 FROM UserManagement.ProductPage WHERE ProductId=16 AND IsActive = 1)
BEGIN
	UPDATE UserManagement.ProductPage SET IsActive = 1 WHERE ProductId=16;
END

IF NOT EXISTS (SELECT TOP 1 1 FROM [UserManagement].[Control] where UIId = 'VendorCredentialiProductAccessTypeTabUIId')
BEGIN
	DECLARE @UserId bigint, @MaxControlId INT;
	
	SELECT	@UserId = UserId
	FROM	Ident.UserLogin
	WHERE	LoginName LIKE 'realpagead@%';

	SELECT @MaxControlId = ControlId + 1
	FROM [UserManagement].[Control];

	select @MaxControlId;

	SET IDENTITY_INSERT [UserManagement].[Control] ON 
	INSERT INTO UserManagement.[Control](ControlId,ParentControlId,ControlTypeId,UIId,DisplayName,DataSource,[Sequence],CreatedBy,CreatedDate)
	VALUES(@MaxControlId,427,9,'VendorCredentialiProductAccessTypeTabUIId','Access Type',null,2,@UserId,GETUTCDATE())
	SET IDENTITY_INSERT [UserManagement].[Control] OFF

	--Updating the parent for the access type radio buttons
	UPDATE UserManagement.[Control] SET ParentControlId = @MaxControlId WHERE ControlId in (429,430,431,432);

	--Updating the sequence for the tabs as Access Type tab should 2(after roles tab)
	UPDATE UserManagement.[Control] SET [Sequence] = 3 WHERE ControlId = 437;
	UPDATE UserManagement.[Control] SET [Sequence] = 4 WHERE ControlId = 442;
	UPDATE UserManagement.[Control] SET [Sequence] = 5 WHERE ControlId = 447;
END

GO
DECLARE @UserId bigint,
	@ProductId int ,
	@ControlId int,
	@Now datetime = GETDATE()

Declare @ControlAttributeId int
Declare @ProductPageId int
Declare @ProductPageControlId int

SELECT	@UserId = UserId
FROM	Ident.UserLogin
WHERE	LoginName LIKE 'realpagead@%'
SET @ProductId  = 1
IF NOT EXISTS (SELECT TOP 1 1 FROM[UserManagement].[ProductPage] WHERE ProductId = @ProductId and ProductPageTypeId = 2)
BEGIN
	SET IDENTITY_INSERT [UserManagement].[Control] ON 
	Select @ControlId = MAX(controlid) +1 from [UserManagement].[Control]

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
	VALUES (@ControlId, NULL, 8, N'OnesiteRolesAndRightsTabGroupUIId', NULL, NULL, 1, @UserId, @Now)

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
	VALUES (@ControlId+1, @ControlId, 9, N'OnesiteRolesAndRightsRolesTabUIId', N'Roles', NULL, 1, @UserId, @Now)

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
	VALUES (@ControlId+2, @ControlId+1, 15, N'OnesiteRolesAndRightsRolesTabUIId', N'New Role', NULL, 1, @UserId, @Now)

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
	VALUES (@ControlId+3, @ControlId+1, 2, N'OnesiteRolesAndRightsRolesSelectGridUIId', NULL, NULL, 1, @UserId, @Now)

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
	VALUES (@ControlId+4, @ControlId+3, 5, N'OnesiteRolesAndRightsRoleLabelUIId', N'Role', N'name', 1, @UserId, @Now)

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
	VALUES (@ControlId+5, @ControlId+3, 14, N'OnesiteRolesAndRightsRadioUIId', N'Right', N'rightsAssigned', 2, @UserId, @Now)

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
	VALUES (@ControlId+6, @ControlId+3, 5, N'OnesiteRolesAndRightsRoleTypeLabelUIId', N'Role Type', N'roletype', 3, @UserId, @Now)

	--INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
	--VALUES (@ControlId+7, @ControlId+3, 5, N'OnesiteRolesAndRightsRoleTypeLabelUIId', NULL, N'defaultRole', 4, @UserId, @Now)

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
	VALUES (@ControlId+7, @ControlId+3, 16, N'OnesiteRolesAndRightsIconUIId', NULL, N'more', 5, @UserId, @Now)

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
	VALUES (@ControlId+8, @ControlId+5, 5, N'OnesiteRolesAndRightsRoleDetailsLabelUIId', N'Edit Custom Role', NULL, 1, @UserId, @Now)

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
	VALUES (@ControlId+9, @ControlId+8, 17, N'OnesiteRolesAndRightsRoleDetailsLabelUIId', N'Role Name', N'name', 2, @UserId, @Now)

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
	VALUES (@ControlId+10, @ControlId+8, 3, N'OnesiteRolesAndRightsGridUIId', NULL, NULL, 3, @UserId, @Now)

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
	VALUES (@ControlId+11, @ControlId+10, 10, N'OnesiteRolesAndRightsCheckboxUIId', NULL, N'isAssigned', 1, @UserId, @Now)	

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
	VALUES (@ControlId+12, @ControlId+10, 5, N'OnesiteRolesAndRightsRightLabelUIId',  N'Product Center', N'centerName', 2, @UserId, @Now)

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
	VALUES (@ControlId+13, @ControlId+10, 5, N'OnesiteRolesAndRightsRightLabelUIId', N'Right', N'description', 3, @UserId, @Now)

	--tab 2

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
	VALUES (@ControlId+14, @ControlId, 9, N'OnesiteRolesAndRightsPropertiesTabUIId', N'Rights', NULL, 2, @UserId, @Now)

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
	VALUES (@ControlId+15, @ControlId+14, 12, N'OnesiteRolesAndRightsPropertiesMultiSelectGridUIId', NULL, NULL, 1, @UserId, @Now)

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
	VALUES (@ControlId+16, @ControlId+15, 5, N'OnesiteRolesAndRightsPropertyLabelUIId', N'Product Center', N'centerName', 1, @UserId, @Now)

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
	VALUES (@ControlId+17, @ControlId+15, 5, N'OnesiteRolesAndRightsPropertyLabelUIId', N'Right', N'description', 1, @UserId, @Now)

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
	VALUES (@ControlId+18, @ControlId+15, 14, N'OnesiteRolesAndRightsCityLabelUIId', N'Role', N'rolesAssigned', 2, @UserId, @Now)

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
	VALUES (@ControlId+19, @ControlId+15, 16, N'OnesiteRolesAndRightsStateLabelUIId', NULL, N'more', 3, @UserId, @Now)

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
	VALUES (@ControlId+20, @ControlId+18, 5, N'OnesiteRolesAndRightsRoleDetailsLabelUIId', N'Assign Roles', NULL, 1, @UserId, @Now)

	--INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
	--VALUES (@ControlId+10, @ControlId+19, 17, N'OnesiteRolesAndRightsRoleDetailsLabelUIId', N'Role Name', N'name', 2, @UserId, @Now)

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
	VALUES (@ControlId+21, @ControlId+20, 3, N'OnesiteRolesAndRightsGridUIId', NULL, NULL, 3, @UserId, @Now)

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
	VALUES (@ControlId+22, @ControlId+21, 10, N'OnesiteRolesAndRightsCheckboxUIId', NULL, N'isAssigned', 1, @UserId, @Now)

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
	VALUES (@ControlId+23, @ControlId+21, 5, N'OnesiteRolesAndRightsRightLabelUIId', N'Role', N'description', 2, @UserId, @Now)

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
	VALUES (@ControlId+24, @ControlId+21, 5, N'OnesiteRolesAndRightsRightTypeLabelUIId', N'Type', N'roletype', 2, @UserId, @Now)


	SET IDENTITY_INSERT [UserManagement].[Control] OFF

	SET IDENTITY_INSERT [UserManagement].[ControlAttribute] ON 
	
	Select @ControlAttributeId = MAX(ControlAttributeId) +1 from [UserManagement].[ControlAttribute]

	INSERT [UserManagement].[ControlAttribute] ([ControlAttributeId], [ControlId], [Key], [Value], [CreatedBy], [CreatedDate])
	VALUES (@ControlAttributeId, @ControlId+1, N'Default', N'True', @UserId, @Now)

	INSERT [UserManagement].[ControlAttribute] ([ControlAttributeId], [ControlId], [Key], [Value], [CreatedBy], [CreatedDate])
	VALUES (@ControlAttributeId+1, @ControlId+1, N'Hide', N'False', @UserId, @Now)

	INSERT [UserManagement].[ControlAttribute] ([ControlAttributeId], [ControlId], [Key], [Value], [CreatedBy], [CreatedDate])
	VALUES (@ControlAttributeId+2, @ControlId+5, N'Link', N'Slide', @UserId, @Now)

	INSERT [UserManagement].[ControlAttribute] ([ControlAttributeId], [ControlId], [Key], [Value], [CreatedBy], [CreatedDate])
	VALUES (@ControlAttributeId+3, @ControlId+7, N'Menu', N'ActionMenu', @UserId, @Now)

	INSERT [UserManagement].[ControlAttribute] ([ControlAttributeId], [ControlId], [Key], [Value], [CreatedBy], [CreatedDate])
	VALUES (@ControlAttributeId+4, @ControlId+10, N'ShowSelectAll', N'True', @UserId, @Now)
	
	INSERT [UserManagement].[ControlAttribute] ([ControlAttributeId], [ControlId], [Key], [Value], [CreatedBy], [CreatedDate])
	VALUES (@ControlAttributeId+5, @ControlId+18, N'Link', N'Slide', @UserId, @Now)

	INSERT [UserManagement].[ControlAttribute] ([ControlAttributeId], [ControlId], [Key], [Value], [CreatedBy], [CreatedDate])
	VALUES (@ControlAttributeId+6, @ControlId+19, N'Menu', N'ActionMenu', @UserId, @Now)

	INSERT [UserManagement].[ControlAttribute] ([ControlAttributeId], [ControlId], [Key], [Value], [CreatedBy], [CreatedDate])
	VALUES (@ControlAttributeId+7, @ControlId+21, N'ShowSelectAll', N'True', @UserId, @Now)

	SET IDENTITY_INSERT [UserManagement].[ControlAttribute] OFF

	SET IDENTITY_INSERT [UserManagement].[ProductPage] ON 
	
	Select @ProductPageId = MAX(ProductPageId) +1 from [UserManagement].[ProductPage]

	INSERT [UserManagement].[ProductPage] ([ProductPageId], [ProductId], [DisplayName],[ProductPageTypeId], [CreatedBy], [CreatedDate], [IsActive])
	VALUES (@ProductPageId, 1, N'OneSite Roles And Rights Access',2, @UserId, @Now, 1)

	SET IDENTITY_INSERT [UserManagement].[ProductPage] OFF

	SET IDENTITY_INSERT [UserManagement].[ProductPageControl] ON 
	
	Select @ProductPageControlId = MAX(ProductPageControlId) +1 from [UserManagement].[ProductPageControl]

	INSERT [UserManagement].[ProductPageControl] ([ProductPageControlId], [ProductPageId], [ControlId], [CreatedBy], [CreatedDate])
	VALUES (@ProductPageControlId, @ProductPageId, @ControlId, @UserId, @Now)

	SET IDENTITY_INSERT [UserManagement].[ProductPageControl] OFF
END

SET @ProductId  = 8
IF NOT EXISTS (SELECT TOP 1 1 FROM[UserManagement].[ProductPage] WHERE ProductId = @ProductId and ProductPageTypeId = 2)
BEGIN
	SET IDENTITY_INSERT [UserManagement].[Control] ON 
	Select @ControlId = MAX(controlid) +1 from [UserManagement].[Control]

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
	VALUES (@ControlId, NULL, 8, N'FinancialSuiteRolesAndRightsTabGroupUIId', NULL, NULL, 1, @UserId, @Now)

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
	VALUES (@ControlId+1, @ControlId, 9, N'FinancialSuiteRolesAndRightsRolesTabUIId', N'Roles', NULL, 1, @UserId, @Now)

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
	VALUES (@ControlId+2, @ControlId+1, 15, N'FinancialSuiteRolesAndRightsRolesTabUIId', N'New Role', NULL, 1, @UserId, @Now)

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
	VALUES (@ControlId+3, @ControlId+1, 2, N'FinancialSuiteRolesAndRightsRolesSelectGridUIId', NULL, NULL, 1, @UserId, @Now)

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
	VALUES (@ControlId+4, @ControlId+3, 5, N'FinancialSuiteRolesAndRightsRoleLabelUIId', N'Role', N'name', 1, @UserId, @Now)

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
	VALUES (@ControlId+5, @ControlId+3, 14, N'FinancialSuiteRolesAndRightsRadioUIId', N'Right', N'rightsAssigned', 2, @UserId, @Now)

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
	VALUES (@ControlId+6, @ControlId+3, 5, N'FinancialSuiteRolesAndRightsRoleTypeLabelUIId', N'Type', N'roletype', 3, @UserId, @Now)

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
	VALUES (@ControlId+7, @ControlId+3, 16, N'FinancialSuiteRolesAndRightsIconUIId', NULL, N'more', 5, @UserId, @Now)

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
	VALUES (@ControlId+8, @ControlId+5, 5, N'FinancialSuiteRolesAndRightsRoleDetailsLabelUIId', N'Edit Custom Role', NULL, 1, @UserId, @Now)

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
	VALUES (@ControlId+9, @ControlId+8, 17, N'FinancialSuiteRolesAndRightsRoleDetailsLabelUIId', N'Role Name', N'name', 2, @UserId, @Now)

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
	VALUES (@ControlId+10, @ControlId+8, 3, N'FinancialSuiteRolesAndRightsGridUIId', NULL, NULL, 3, @UserId, @Now)

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
	VALUES (@ControlId+11, @ControlId+10, 10, N'FinancialSuiteRolesAndRightsCheckboxUIId', NULL, N'isAssigned', 1, @UserId, @Now)

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
	VALUES (@ControlId+12, @ControlId+10, 5, N'FinancialSuiteRolesAndRightsRightLabelUIId', N'Product', N'centerName', 2, @UserId, @Now)

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
	VALUES (@ControlId+13, @ControlId+10, 5, N'FinancialSuiteRolesAndRightsRightLabelUIId', N'Right', N'description', 2, @UserId, @Now)

	--tab 2

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
	VALUES (@ControlId+14, @ControlId, 9, N'FinancialSuiteRolesAndRightsPropertiesTabUIId', N'Rights', NULL, 2, @UserId, @Now)

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
	VALUES (@ControlId+15, @ControlId+14, 12, N'FinancialSuiteRolesAndRightsPropertiesMultiSelectGridUIId', NULL, NULL, 1, @UserId, @Now)

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
	VALUES (@ControlId+16, @ControlId+15, 5, N'FinancialSuiteRolesAndRightsPropertyLabelUIId', N'Area', N'centerName', 1, @UserId, @Now)

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
	VALUES (@ControlId+17, @ControlId+15, 5, N'FinancialSuiteRolesAndRightsPropertyLabelUIId', N'Right', N'right', 1, @UserId, @Now)

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
	VALUES (@ControlId+18, @ControlId+15, 5, N'FinancialSuiteRolesAndRightsPropertyLabelUIId', N'Action', N'actionLabel', 1, @UserId, @Now)

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
	VALUES (@ControlId+19, @ControlId+15, 14, N'FinancialSuiteRolesAndRightsCityLabelUIId', N'Role', N'rolesAssigned', 2, @UserId, @Now)

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
	VALUES (@ControlId+20, @ControlId+15, 16, N'FinancialSuiteRolesAndRightsStateLabelUIId', NULL, N'more', 3, @UserId, @Now)

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
	VALUES (@ControlId+21, @ControlId+20, 5, N'FinancialSuiteRolesAndRightsRoleDetailsLabelUIId', N'Assign Roles', NULL, 1, @UserId, @Now)

	--INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
	--VALUES (@ControlId+10, @ControlId+19, 17, N'FinancialSuiteRolesAndRightsRoleDetailsLabelUIId', N'Role Name', N'name', 2, @UserId, @Now)

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
	VALUES (@ControlId+22, @ControlId+21, 3, N'FinancialSuiteRolesAndRightsGridUIId', NULL, NULL, 3, @UserId, @Now)

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
	VALUES (@ControlId+23, @ControlId+22, 10, N'FinancialSuiteRolesAndRightsCheckboxUIId', NULL, N'isAssigned', 1, @UserId, @Now)

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
	VALUES (@ControlId+24, @ControlId+22, 5, N'FinancialSuiteRolesAndRightsRightLabelUIId', N'Role', N'name', 2, @UserId, @Now)

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
	VALUES (@ControlId+25, @ControlId+22, 5, N'FinancialSuiteRolesAndRightsRightLabelUIId', N'Type', N'roletype', 2, @UserId, @Now)

	SET IDENTITY_INSERT [UserManagement].[Control] OFF

	SET IDENTITY_INSERT [UserManagement].[ControlAttribute] ON 
	
	Select @ControlAttributeId = MAX(ControlAttributeId) +1 from [UserManagement].[ControlAttribute]

	INSERT [UserManagement].[ControlAttribute] ([ControlAttributeId], [ControlId], [Key], [Value], [CreatedBy], [CreatedDate])
	VALUES (@ControlAttributeId, @ControlId+1, N'Default', N'True', @UserId, @Now)

	INSERT [UserManagement].[ControlAttribute] ([ControlAttributeId], [ControlId], [Key], [Value], [CreatedBy], [CreatedDate])
	VALUES (@ControlAttributeId+1, @ControlId+1, N'Hide', N'False', @UserId, @Now)

	INSERT [UserManagement].[ControlAttribute] ([ControlAttributeId], [ControlId], [Key], [Value], [CreatedBy], [CreatedDate])
	VALUES (@ControlAttributeId+2, @ControlId+5, N'Link', N'Slide', @UserId, @Now)

	INSERT [UserManagement].[ControlAttribute] ([ControlAttributeId], [ControlId], [Key], [Value], [CreatedBy], [CreatedDate])
	VALUES (@ControlAttributeId+3, @ControlId+7, N'Menu', N'ActionMenu', @UserId, @Now)

	INSERT [UserManagement].[ControlAttribute] ([ControlAttributeId], [ControlId], [Key], [Value], [CreatedBy], [CreatedDate])
	VALUES (@ControlAttributeId+4, @ControlId+11, N'ShowSelectAll', N'True', @UserId, @Now)
	
	INSERT [UserManagement].[ControlAttribute] ([ControlAttributeId], [ControlId], [Key], [Value], [CreatedBy], [CreatedDate])
	VALUES (@ControlAttributeId+5, @ControlId+19, N'Link', N'Slide', @UserId, @Now)

	INSERT [UserManagement].[ControlAttribute] ([ControlAttributeId], [ControlId], [Key], [Value], [CreatedBy], [CreatedDate])
	VALUES (@ControlAttributeId+6, @ControlId+20, N'Menu', N'ActionMenu', @UserId, @Now)

	INSERT [UserManagement].[ControlAttribute] ([ControlAttributeId], [ControlId], [Key], [Value], [CreatedBy], [CreatedDate])
	VALUES (@ControlAttributeId+7, @ControlId+23, N'ShowSelectAll', N'True', @UserId, @Now)

	SET IDENTITY_INSERT [UserManagement].[ControlAttribute] OFF

	SET IDENTITY_INSERT [UserManagement].[ProductPage] ON 

	Select @ProductPageId = MAX(ProductPageId) +1 from [UserManagement].[ProductPage]

	INSERT [UserManagement].[ProductPage] ([ProductPageId], [ProductId], [DisplayName],[ProductPageTypeId], [CreatedBy], [CreatedDate], [IsActive])
	VALUES (@ProductPageId,8, N'Financial Suite Roles And Rights Access',2, @UserId, @Now, 1)

	SET IDENTITY_INSERT [UserManagement].[ProductPage] OFF

	SET IDENTITY_INSERT [UserManagement].[ProductPageControl] ON 

	Select @ProductPageControlId = MAX(ProductPageControlId) +1 from [UserManagement].[ProductPageControl]

	INSERT [UserManagement].[ProductPageControl] ([ProductPageControlId], [ProductPageId], [ControlId], [CreatedBy], [CreatedDate])
	VALUES (@ProductPageControlId, @ProductPageId, @ControlId, @UserId, @Now)

	SET IDENTITY_INSERT [UserManagement].[ProductPageControl] OFF
END

SET @ProductId  = 13
IF NOT EXISTS (SELECT TOP 1 1 FROM[UserManagement].[ProductPage] WHERE ProductId = @ProductId and ProductPageTypeId = 2)
BEGIN
	SET IDENTITY_INSERT [UserManagement].[Control] ON 
	Select @ControlId = MAX(controlid) +1 from [UserManagement].[Control]

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
	VALUES (@ControlId, NULL, 8, N'SpendManagementRolesAndRightsTabGroupUIId', NULL, NULL, 1, @UserId, @Now)

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
	VALUES (@ControlId+1, @ControlId, 9, N'SpendManagementRolesAndRightsRolesTabUIId', N'Roles', NULL, 1, @UserId, @Now)

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
	VALUES (@ControlId+2, @ControlId+1, 15, N'SpendManagementRolesAndRightsRolesTabUIId', N'New Role', NULL, 1, @UserId, @Now)

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
	VALUES (@ControlId+3, @ControlId+1, 2, N'SpendManagementRolesAndRightsRolesSelectGridUIId', NULL, NULL, 1, @UserId, @Now)

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
	VALUES (@ControlId+4, @ControlId+3, 5, N'SpendManagementRolesAndRightsRoleLabelUIId', N'Role', N'name', 1, @UserId, @Now)

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
	VALUES (@ControlId+5, @ControlId+3, 16, N'SpendManagementRolesAndRightsIconUIId', NULL, N'more', 2, @UserId, @Now)

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
	VALUES (@ControlId+6, @ControlId+5, 5, N'SpendManagementRolesAndRightsRoleDetailsLabelUIId', N'Assign Role', NULL, 1, @UserId, @Now)

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
	VALUES (@ControlId+7, @ControlId+6, 17, N'SpendManagementRolesAndRightsRoleDetailsLabelUIId', N'Role Name', N'name', 2, @UserId, @Now)

	--assign role Tab group
	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
	VALUES (@ControlId+8, @ControlId+6, 8, N'SpendManagementassignRolesAndRightsTabGroupUIId', NULL, NULL, 1, @UserId, @Now)
	--rights tab
	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
	VALUES (@ControlId+9, @ControlId+8, 9, N'SpendManagementassignRolesAndRightsRolesTabUIId', N'Rights', NULL, 1, @UserId, @Now)

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
	VALUES (@ControlId+10, @ControlId+9, 19, N'SpendManagementassignRolesAndRightsGridUIId', NULL, NULL, 3, @UserId, @Now)

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
	VALUES (@ControlId+11, @ControlId+10, 5, N'SpendManagementassignRolesAndRightsRightLabelUIId',  N'Right', N'mainName', 1, @UserId, @Now)

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
	VALUES (@ControlId+12, @ControlId+10, 5, N'SpendManagementassignRolesAndRightsRightLabelUIId', N'Description', N'description', 2, @UserId, @Now)

	--workflow tab
	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
	VALUES (@ControlId+13, @ControlId+8, 9, N'SpendManagementRolesAndRightswrokflowTabUIId', N'WorkFlow', NULL, 1, @UserId, @Now)

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
	VALUES (@ControlId+14, @ControlId+13, 5, N'SpendManagementWorkflowLabel1UIId',  N'Workflow Timeout for Orders', N'', 1, @UserId, @Now)

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
	VALUES (@ControlId+15, @ControlId+13, 17, N'SpendManagementWorkflowtxt1UIId',  N'', N'orderTimeOut', 2, @UserId, @Now)

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
	VALUES (@ControlId+16, @ControlId+13, 5, N'SpendManagementWorkflowLabel2UIId',  N'Workflow Timeout for Invoices', N'', 3, @UserId, @Now)

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
	VALUES (@ControlId+17, @ControlId+13, 17, N'SpendManagementWorkflowtxt2UIId',  N'', N'invoiceTimeOut', 4, @UserId, @Now)

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
	VALUES (@ControlId+18, @ControlId+13, 10, N'SpendManagementWorkflowchk1UIId',  N'Reminder Email Only', N'isOrderReminder', 5, @UserId, @Now)

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
	VALUES (@ControlId+19, @ControlId+13, 10, N'SpendManagementWorkflowchk2UIId',  N'Reminder Email Only', N'isInvoiceReminder', 5, @UserId, @Now)

	--tab 2

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
	VALUES (@ControlId+20, @ControlId, 9, N'SpendManagementRolesAndRightsPropertiesTabUIId', N'Rights', NULL, 2, @UserId, @Now)

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
	VALUES (@ControlId+21, @ControlId+20, 18, N'SpendManagementassignRolesAndRightsGridUIId', NULL, NULL, 3, @UserId, @Now)

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
	VALUES (@ControlId+22, @ControlId+21, 5, N'SpendManagementassignRolesAndRightsRightLabelUIId',  N'Right', N'mainName', 1, @UserId, @Now)

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
	VALUES (@ControlId+23, @ControlId+21, 5, N'SpendManagementassignRolesAndRightsRightLabelUIId', N'Description', N'description', 2, @UserId, @Now)

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
	VALUES (@ControlId+24, @ControlId+10, 10, N'SpendManagementassignRolesAndRightsRightLabelUIId', N'Warn', N'isWarnAssigned', 3, @UserId, @Now)

	SET IDENTITY_INSERT [UserManagement].[Control] OFF

	SET IDENTITY_INSERT [UserManagement].[ControlAttribute] ON 
	
	Select @ControlAttributeId = MAX(ControlAttributeId) +1 from [UserManagement].[ControlAttribute]

	INSERT [UserManagement].[ControlAttribute] ([ControlAttributeId], [ControlId], [Key], [Value], [CreatedBy], [CreatedDate])
	VALUES (@ControlAttributeId, @ControlId+1, N'Default', N'True', @UserId, @Now)

	INSERT [UserManagement].[ControlAttribute] ([ControlAttributeId], [ControlId], [Key], [Value], [CreatedBy], [CreatedDate])
	VALUES (@ControlAttributeId+1, @ControlId+1, N'Hide', N'False', @UserId, @Now)

	INSERT [UserManagement].[ControlAttribute] ([ControlAttributeId], [ControlId], [Key], [Value], [CreatedBy], [CreatedDate])
	VALUES (@ControlAttributeId+3, @ControlId+5, N'Menu', N'ActionMenu', @UserId, @Now)	

	SET IDENTITY_INSERT [UserManagement].[ControlAttribute] OFF

	SET IDENTITY_INSERT [UserManagement].[ProductPage] ON 
	
	Select @ProductPageId = MAX(ProductPageId) +1 from [UserManagement].[ProductPage]

	INSERT [UserManagement].[ProductPage] ([ProductPageId], [ProductId], [DisplayName],[ProductPageTypeId], [CreatedBy], [CreatedDate], [IsActive])
	VALUES (@ProductPageId, @ProductId, N'Spend Management Roles And Rights Access',2, @UserId, @Now, 1)

	SET IDENTITY_INSERT [UserManagement].[ProductPage] OFF

	SET IDENTITY_INSERT [UserManagement].[ProductPageControl] ON 
	
	Select @ProductPageControlId = MAX(ProductPageControlId) +1 from [UserManagement].[ProductPageControl]

	INSERT [UserManagement].[ProductPageControl] ([ProductPageControlId], [ProductPageId], [ControlId], [CreatedBy], [CreatedDate])
	VALUES (@ProductPageControlId, @ProductPageId, @ControlId, @UserId, @Now)

	SET IDENTITY_INSERT [UserManagement].[ProductPageControl] OFF
END
GO
DECLARE @UserId bigint,
    @Now datetime = GETDATE();

SELECT	@UserId = UserId
FROM	Ident.UserLogin
WHERE	LoginName LIKE 'realpagead@%' 
	
--Onesite
IF EXISTS (select TOP 1 1 from UserManagement.Control where ControlId = 109 and ParentControlId = 102 and [Sequence] = 2)
BEGIN
	UPDATE UserManagement.Control  SET [Sequence] = 1  where ControlID = 109
	UPDATE UserManagement.Control  SET [Sequence] = 2  where ControlID = 103
END
IF EXISTS (select TOP 1 1 from UserManagement.ControlAttribute where ControlId = 103 and ControlAttributeId = 6)
BEGIN
  UPDATE UserManagement.ControlAttribute SET ControlId = 109 WHERE ControlId = 103 and ControlAttributeId = 6
END

--Financial Suite 
IF EXISTS (select TOP 1 1 from UserManagement.Control where ControlId = 533 and ParentControlId = 516 and [Sequence] = 7)
BEGIN
	UPDATE UserManagement.Control  SET [Sequence] = 5  where ControlID = 533
	UPDATE UserManagement.Control  SET [Sequence] = 6  where ControlID = 521
	UPDATE UserManagement.Control  SET [Sequence] = 7  where ControlID = 526
END
IF EXISTS (select TOP 1 1 from UserManagement.ControlAttribute where ControlId = 521 and ControlAttributeId = 131)
BEGIN
  UPDATE UserManagement.ControlAttribute SET ControlId = 534 WHERE ControlId = 521 and ControlAttributeId = 131
END

--Unified Amenities

IF EXISTS (select TOP 1 1 from UserManagement.Control where ControlId = 196 and ParentControlId = 190 and [Sequence] = 2)
BEGIN
	UPDATE UserManagement.Control  SET [Sequence] = 1  where ControlID = 196
	UPDATE UserManagement.Control  SET [Sequence] = 2  where ControlID = 191
END

-- Resident Portal
IF EXISTS (select TOP 1 1 from UserManagement.Control where ControlId = 405 and ParentControlId = 399 and [Sequence] = 2)
BEGIN
	UPDATE UserManagement.Control  SET [Sequence] = 1  where ControlID = 405
	UPDATE UserManagement.Control  SET [Sequence] = 2  where ControlID = 400
END

-- On-ste

IF EXISTS (select TOP 1 1 from UserManagement.Control where ControlId = 176 and ParentControlId = 164 and [Sequence] = 3)
BEGIN
	UPDATE UserManagement.Control  SET [Sequence] = 1  where ControlID = 176
	UPDATE UserManagement.Control  SET [Sequence] = 2  where ControlID = 165
	UPDATE UserManagement.Control  SET [Sequence] = 3  where ControlID = 169
END

IF EXISTS (select TOP 1 1 from UserManagement.ControlAttribute where ControlId = 169 and ControlAttributeId = 28)
BEGIN
  UPDATE UserManagement.ControlAttribute SET ControlId = 176 WHERE ControlId = 169 and ControlAttributeId = 28
END

--Marketing Center

IF EXISTS (select TOP 1 1 from UserManagement.Control where ControlId = 130 and ParentControlId = 119 and [Sequence] = 3)
BEGIN
	UPDATE UserManagement.Control  SET [Sequence] = 1  where ControlID = 130
	UPDATE UserManagement.Control  SET [Sequence] = 2  where ControlID = 124
END

IF EXISTS (select TOP 1 1 from UserManagement.ControlAttribute where ControlId = 124 and ControlAttributeId = 8)
BEGIN
  UPDATE UserManagement.ControlAttribute SET ControlId = 130 WHERE ControlId = 124 and ControlAttributeId = 8
END

--ILM-Lead Management

IF EXISTS (select TOP 1 1 from UserManagement.Control where ControlId = 225 and ParentControlId = 219 and [Sequence] = 2)
BEGIN
	UPDATE UserManagement.Control  SET [Sequence] = 1  where ControlID = 225
	UPDATE UserManagement.Control  SET [Sequence] = 2  where ControlID = 220
END

IF EXISTS (select TOP 1 1 from UserManagement.ControlAttribute where ControlId = 220 and ControlAttributeId = 40)
BEGIN
	UPDATE UserManagement.ControlAttribute SET ControlId = 225 WHERE ControlId = 220 and ControlAttributeId = 40
END

IF NOT EXISTS(select TOP 1 1 from UserManagement.ControlAttribute where ControlId in (220,225))
BEGIN
	INSERT [UserManagement].[ControlAttribute] ([ControlId], [Key], [Value], [CreatedBy], [CreatedDate]) 
	VALUES (225, N'Default', N'True', @UserId, @Now)
END

--ILM-Leasing Analytics

IF EXISTS (select TOP 1 1 from UserManagement.Control where ControlId = 215 and ParentControlId = 205 and [Sequence] = 3)
BEGIN
	UPDATE UserManagement.Control  SET [Sequence] = 1  where ControlID = 215
	UPDATE UserManagement.Control  SET [Sequence] = 2  where ControlID = 206
	UPDATE UserManagement.Control  SET [Sequence] = 3  where ControlID = 210
END

IF EXISTS (select TOP 1 1 from UserManagement.ControlAttribute where ControlId = 206 and ControlAttributeId = 38)
BEGIN
	UPDATE UserManagement.ControlAttribute SET ControlId = 215  where ControlId = 206 and ControlAttributeId = 38
END

IF NOT EXISTS(select TOP 1 1 from UserManagement.ControlAttribute where ControlId in (206,215))
BEGIN
	INSERT [UserManagement].[ControlAttribute] ([ControlId], [Key], [Value], [CreatedBy], [CreatedDate]) 
	VALUES (215, N'Default', N'True', @UserId, @Now)
END

--Client Portal

IF EXISTS (select TOP 1 1 from UserManagement.Control where ControlId = 68 and ParentControlId = 66 and [Sequence] = 2)
BEGIN
	UPDATE UserManagement.Control  SET [Sequence] = 1  where ControlID = 68
	UPDATE UserManagement.Control  SET [Sequence] = 2  where ControlID = 67
END

IF EXISTS (select TOP 1 1 from UserManagement.ControlAttribute where ControlId = 67 and ControlAttributeId = 2)
BEGIN
	UPDATE UserManagement.ControlAttribute SET ControlId = 68 WHERE ControlId = 67 and ControlAttributeId = 2
END
