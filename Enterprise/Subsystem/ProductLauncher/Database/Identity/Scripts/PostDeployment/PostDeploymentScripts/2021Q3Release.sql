-- Navigation Menu data

SET IDENTITY_INSERT Enterprise.NavigationMenu ON;

MERGE INTO Enterprise.NavigationMenu t
	USING 
	(
		VALUES
			(8, N'Enterprise Roles', N'enterpriseRoles', NULL, N'/home/roles-rights/enterprise-roles', 80, 6, 'unified-login')
	) 
	AS 
	s (Id, Title, PageId, Icon, [URL], OrderIndex, ParentId, Origin) on t.Id = s.Id
	WHEN MATCHED THEN
		UPDATE SET Title = s.Title,
			PageId = s.PageId,
			Icon = s.Icon,
			[URL] = s.[URL],
			OrderIndex = s.OrderIndex,
			ParentId = s.ParentId,
			Origin = s.Origin
	WHEN NOT MATCHED BY TARGET THEN
		INSERT(Id, Title, PageId, Icon, [URL], OrderIndex, ParentId, Origin) VALUES (s.Id, s.Title, s.PageId, s.Icon, s.[URL], s.OrderIndex, s.ParentId, s.Origin)
;

SET IDENTITY_INSERT Enterprise.NavigationMenu OFF;
DECLARE @maxId int = (SELECT MAX(Id) FROM Enterprise.NavigationMenu);
DBCC CHECKIDENT ('Enterprise.NavigationMenu', RESEED, @maxId);

MERGE INTO Enterprise.NavigationMenuRights t
	USING 
	(
		SELECT 8 NavigationMenuId, RightId FROM [Security].[Right] WHERE RightName = 'ViewRoleRight'
	) 
	AS 
	s (NavigationMenuId, RightId) on t.NavigationMenuId = s.NavigationMenuId
		AND t.RightId = s.RightId
	WHEN NOT MATCHED BY TARGET THEN
		INSERT(NavigationMenuId, RightId) VALUES (s.NavigationMenuId, s.RightId);

GO

--Rename UsePrimaryProperties to 
UPDATE Enterprise.MasterSettingType  SET NAME='EnablePrimaryPropertiesAndEnterpriseRoles' WHERE name ='UsePrimaryProperties'
GO

-- Add user sync integration URL
IF NOT EXISTS (SELECT TOP 1 1 FROM Enterprise.ProductSettingType WHERE [Name] = 'UserSyncIntegrationURL')
BEGIN
	INSERT INTO Enterprise.ProductSettingType ([Name], [Description], SensitiveData)
	VALUES ('UserSyncIntegrationURL', 'The URL to fetch user data from when the UserSyncIntegrationMethod setting is set to Pull', 0);
END

-- Add user sync integration method setting
IF NOT EXISTS (SELECT TOP 1 1 FROM Enterprise.ProductSettingType WHERE [Name] = 'UserSyncIntegrationMethod')
BEGIN
	INSERT INTO Enterprise.ProductSettingType ([Name], [Description], SensitiveData)
	VALUES ('UserSyncIntegrationMethod', 'The method to use for syncing user data', 0);
END

GO

---Insert records to Enterpirse.ProductRule as part of Userstroy:840776

IF NOT EXISTS (SELECT 1 FROM Enterprise.ProductRuleType WHERE ProductRuleType = 'Product')
BEGIN
	INSERT INTO Enterprise.ProductRuleType(ProductRuleType, Description)
	VALUES( 'Product','Product')
END
IF NOT EXISTS (SELECT 1 FROM Enterprise.ProductRuleType WHERE ProductRuleType = 'Role')
BEGIN
	INSERT INTO Enterprise.ProductRuleType(ProductRuleType, Description)
	VALUES( 'Role','Role')
END
IF NOT EXISTS (SELECT 1 FROM Enterprise.ProductRuleType WHERE ProductRuleType = 'AccessType')
BEGIN
	INSERT INTO Enterprise.ProductRuleType(ProductRuleType, Description)
	VALUES( 'AccessType','AccessType')
END

DECLARE @ProductRuleAccessTypeId int, @UserId bigint,	
	@Now datetime = GETUTCDATE()	
select @ProductRuleAccessTypeId = productRuleTypeId from Enterprise.ProductRuleType where productRuleType = 'AccessType'

DECLARE @ProductRuleRoleTypeId int
select @ProductRuleRoleTypeId = productRuleTypeId from Enterprise.ProductRuleType where productRuleType = 'Role'

DECLARE @ProductRuleProductTypeId int
select @ProductRuleProductTypeId = productRuleTypeId from Enterprise.ProductRuleType where productRuleType = 'Product'

SELECT	@UserId = UserId
FROM	Ident.UserLogin
WHERE	LoginName LIKE 'realpagead@%'

IF NOT EXISTS(SELECT 1 FROM Enterprise.ProductValidationRule WHERE ProductId = 34 AND ProductRuleTypeId = @ProductRuleProductTypeId)
BEGIN
INSERT INTO Enterprise.ProductValidationRule(ProductId,ProductRuleTypeId,RuleValue,ValidationMessage,CreatedBy,CreatedDate)
SELECT 34, @ProductRuleProductTypeId, 30, 'Performance Analytics Role Required', @UserId, @Now
END
IF NOT EXISTS(SELECT 1 FROM Enterprise.ProductValidationRule WHERE ProductId = 3 AND ProductRuleTypeId = @ProductRuleRoleTypeId)
BEGIN
INSERT INTO Enterprise.ProductValidationRule(ProductId,ProductRuleTypeId,RuleValue,ValidationMessage,CreatedBy,CreatedDate)
SELECT 3, @ProductRuleRoleTypeId, 1, 'At least one role is required for Unified Platform', @UserId, @Now
END
IF NOT EXISTS(SELECT 1 FROM Enterprise.ProductValidationRule WHERE ProductId = 16 AND ProductRuleTypeId = @ProductRuleAccessTypeId)
BEGIN
INSERT INTO Enterprise.ProductValidationRule(ProductId,ProductRuleTypeId,RuleValue,ValidationMessage,CreatedBy,CreatedDate)
SELECT 16, @ProductRuleAccessTypeId, 1, 'Access type is required for Vendor Credentialing', @UserId, @Now
END
IF NOT EXISTS(SELECT 1 FROM Enterprise.ProductValidationRule WHERE ProductId = 18 AND ProductRuleTypeId = @ProductRuleAccessTypeId)
BEGIN
INSERT INTO Enterprise.ProductValidationRule(ProductId,ProductRuleTypeId,RuleValue,ValidationMessage,CreatedBy,CreatedDate)
SELECT 18, @ProductRuleAccessTypeId, 1, 'Access type is required for Utility Management', @UserId, @Now
END
IF NOT EXISTS(SELECT 1 FROM Enterprise.ProductValidationRule WHERE ProductId = 44 AND ProductRuleTypeId = @ProductRuleRoleTypeId)
BEGIN
INSERT INTO Enterprise.ProductValidationRule(ProductId,ProductRuleTypeId,RuleValue,ValidationMessage,CreatedBy,CreatedDate)
SELECT 44, @ProductRuleRoleTypeId, 1, 'At least one Entity role is required for Portfolio Management', @UserId, @Now
END