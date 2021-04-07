GO
--Updating GetUserEndpoint for ILMLA and ILMLA products
DECLARE @ProductSettingTypeId INT
SELECT @ProductSettingTypeId = ProductSettingTypeId FROM Enterprise.ProductSettingType WHERE Name='GetUserEndpoint'
IF EXISTS(SELECT 1 FROM Enterprise.ProductSetting WHERE ProductId IN (40, 41) AND ProductSettingTypeId = @ProductSettingTypeId)
BEGIN
UPDATE Enterprise.ProductSetting SET value = '/{0}/users?loginName={1}'
WHERE ProductId IN (40, 41) AND ProductSettingTypeId = @ProductSettingTypeId
END
GO

-- Adding Rights to HomeSharing Product	
DECLARE @UserId INT
SELECT @UserId = UserId FROM Ident.UserLogin WHERE LoginName like 'realpagead@%'
IF NOT EXISTS(SELECT 1 FROM Security.[Right] WHERE RightName = 'PropertyAdmin')
BEGIN
	INSERT INTO Security.[Right](RightName, Description, Value, StatusTypeId, VisibilityStatusId, ProductId, TargetProductId, CreatedBy, CreatedDate)
	VALUES('PropertyAdmin', 'Property Admin', 'Property Admin', 13, 9, 60, 60, @UserId, GETDATE())
END
IF NOT EXISTS(SELECT 1 FROM Security.[Right] WHERE RightName = 'PropertyUser')
BEGIN
	INSERT INTO Security.[Right](RightName, Description, Value, StatusTypeId, VisibilityStatusId, ProductId, TargetProductId, CreatedBy, CreatedDate)
	VALUES('PropertyUser', 'Property User', 'Property User', 13, 9, 60, 60, @UserId, GETDATE())
END
DECLARE @PropertyAdminRoleId INT
DECLARE @PropertyUserRoleId INT		
DECLARE @PropertyAdminRightId INT
DECLARE @PropertyUserRightId INT		
SELECT @PropertyAdminRightId  = RightId FROM Security.[Right] WHERE RightName = 'PropertyAdmin' AND ProductId = 60;
SELECT @PropertyUserRightId = RightId FROM Security.[Right] WHERE RightName='PropertyUser' AND ProductId = 60;
IF NOT EXISTS(SELECT 1 FROM Security.Role WHERE RoleName = 'Property Admin' AND OrgPartyID IS NULL AND ProductId = 60)
BEGIN
	INSERT INTO Security.Role(RoleName, ShortName, Description, RoleTypeID, OrgPartyID, ProductId, CreatedBy, CreatedDate)
	VALUES('Property Admin', 'Property Admin', 'Property Admin', 3, NULL, 60, @UserId, GETDATE())	
END	
SELECT @PropertyAdminRoleId = RoleId FROM Security.Role WHERE RoleName = 'Property Admin' AND OrgPartyID IS NULL AND ProductId = 60
IF NOT EXISTS(SELECT 1 FROM Security.RoleRight WHERE RoleId = @PropertyAdminRoleId AND RightId = @PropertyAdminRightId)
BEGIN
	INSERT INTO Security.RoleRight(RoleId, RightId, CreatedBy, CreatedDate)
	VALUES(@PropertyAdminRoleId, @PropertyAdminRightId, @UserId, GETDATE())
END
IF NOT EXISTS(SELECT 1 FROM Security.Role WHERE RoleName = 'Property User' AND OrgPartyID IS NULL AND ProductId = 60)
BEGIN
	INSERT INTO Security.Role(RoleName, ShortName, Description, RoleTypeID, OrgPartyID, ProductId, CreatedBy, CreatedDate)
	VALUES('Property User', 'Property User', 'Property User', 3, NULL, 60, @UserId, GETDATE())	
END
SELECT @PropertyUserRoleId = RoleId FROM Security.Role WHERE RoleName = 'Property User' AND OrgPartyID IS NULL AND ProductId = 60
	
IF NOT EXISTS(SELECT 1 FROM Security.RoleRight WHERE RoleId = @PropertyUserRoleId AND RightId = @PropertyUserRightId)
BEGIN
	INSERT INTO Security.RoleRight(RoleId, RightId, CreatedBy, CreatedDate)
	VALUES(@PropertyUserRoleId, @PropertyUserRightId, @UserId, GETDATE())
END
GO


--Accounting Location Group
Declare @FSMasterControlId int,@FSLocationGroupControlId int,@MaxControlId int,@MaxControlAttributeId int
DECLARE @UserId bigint,
	@ProductId int ,
	@Now datetime = GETDATE()

SELECT	@UserId = UserId
FROM	Ident.UserLogin
WHERE	LoginName LIKE 'realpagead@%'

Select @FSMasterControlId = ControlId From UserManagement.Control 
Where UIId = 'FinancialSuiteProductAccessTabGroupUIId' AND ControlTypeId = 8

Select @FSLocationGroupControlId = ControlId From UserManagement.Control 
Where UIId = 'FinancialSuiteProductAccessLocationGroupTabUIId' AND ControlTypeId = 9

update [UserManagement].[Control] set Sequence = 8
where uiid = 'FinancialSuiteProductAccessEntitiesTabUIId' and ControlTypeId = 9

IF NOT EXISTS (SELECT TOP 1 1 FROM[UserManagement].[Control] WHERE ControlId = @FSLocationGroupControlId)
BEGIN
	
	SET IDENTITY_INSERT [UserManagement].[Control] ON 
	SELECT @MaxControlId = max(ControlId) from UserManagement.Control

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
	VALUES (@MaxControlId +1, @FSMasterControlId, 9, N'FinancialSuiteProductAccessLocationGroupTabUIId', N'Location Group', NULL, 7, @UserId, @Now)

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
	VALUES (@MaxControlId +2, @MaxControlId +1, 3, N'FinancialSuiteProductAccessLocationGroupMultiSelectGridUIId', NULL, NULL, 1, @UserId, @Now)

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
	VALUES (@MaxControlId +3, @MaxControlId +2, 10, N'FinancialSuiteProductAccessLocationGroupCheckboxUIId', NULL, N'isAssigned', 2, @UserId, @Now)

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
	VALUES (@MaxControlId +4, @MaxControlId +2, 5, N'FinancialSuiteProductAccessLocationGroupLabelUIId', N'Location Group', N'name', 3, @UserId, @Now)

	--PGSlide
			INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
			VALUES (@MaxControlId +5, @MaxControlId +2, 11, N'FinancialSuiteProductAccessLocationGroupIconUIId', NULL, N'InfoIcon', 4, @UserId, @Now)

			INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
			VALUES (@MaxControlId +6, @MaxControlId +5, 5, N'FinancialSuiteProductAccessLocationGroupDetailsLabelUIId', N'Location Group Details', NULL, 1, @UserId, @Now)

			INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
			VALUES (@MaxControlId +7, @MaxControlId +5, 12, N'FinancialSuiteProductAccessLocationGroupDetailsGridUIId', NULL, NULL, 2, @UserId, @Now)

			INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
			VALUES (@MaxControlId +8, @MaxControlId +7, 5, N'FinancialSuiteProductAccessLocationGroupDetailsPropertyLabelUIId', N'Entity', N'name', 2, @UserId, @Now)
			
			SET IDENTITY_INSERT [UserManagement].[Control] OFF
END

		SET IDENTITY_INSERT [UserManagement].[ControlAttribute] ON 
            
			SELECT @MaxControlAttributeId = max(ControlAttributeId) from UserManagement.ControlAttribute
			Declare @FSLGSelallID int,@FSLGinfoiconID int

			Select @FSLGSelallID = ControlId From UserManagement.Control 
            Where UIId = 'FinancialSuiteProductAccessLocationGroupMultiSelectGridUIId' AND ControlTypeId = 3

            Select @FSLGinfoiconID = ControlId From UserManagement.Control 
            Where UIId = 'FinancialSuiteProductAccessLocationGroupIconUIId' AND ControlTypeId = 11

			IF NOT EXISTS ( SELECT TOP 1 1 FROM [UserManagement].[ControlAttribute] WHERE ControlId = @FSLGSelallID)
			BEGIN
				INSERT [UserManagement].[ControlAttribute] ([ControlAttributeId], [ControlId], [Key], [Value], [CreatedBy], [CreatedDate]) 
				VALUES (@MaxControlAttributeId + 1, @FSLGSelallID, N'ShowSelectAll', N'False', @UserId, @Now)
			END

			--SELECT @MaxControlAttributeId = max(ControlAttributeId) from UserManagement.ControlAttribute
			IF NOT EXISTS ( SELECT TOP 1 1 FROM [UserManagement].[ControlAttribute] WHERE ControlId = @FSLGinfoiconID)
			BEGIN
				INSERT [UserManagement].[ControlAttribute] ([ControlAttributeId], [ControlId], [Key], [Value], [CreatedBy], [CreatedDate]) 
				VALUES (@MaxControlAttributeId + 2, @FSLGinfoiconID, N'InfoIcon', N'Slide', @UserId, @Now)		
			END
			
            SET IDENTITY_INSERT [UserManagement].[ControlAttribute] OFF

GO