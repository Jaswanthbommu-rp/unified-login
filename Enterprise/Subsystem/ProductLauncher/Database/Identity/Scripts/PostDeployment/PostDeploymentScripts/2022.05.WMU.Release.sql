--User Story 1072599: DB/API: AD Group Implementation for products with a UPFM integration type
IF NOT EXISTS(SELECT * FROM Enterprise.ProductSettingType WHERE Name = 'UPFMProductsHasProperties')
BEGIN
	INSERT INTO Enterprise.ProductSettingType([Name], [Description],SensitiveData)
	VALUES('UPFMProductsHasProperties','Can UPFM Products can have properties. Eg: For HOTS values is 0',0)
END

GO


-- User Story 1088575 Sustainbility Services Product Integration Role's and Right
Declare @UserId bigint;
SELECT @UserId = UserId
FROM   Ident.UserLogin
WHERE  LoginName LIKE 'realpagead@%';
-- Adding default roles to Sustainbility Services Product 
IF NOT EXISTS(SELECT 1 FROM Security.Role WHERE ProductId = 84)
BEGIN
	INSERT INTO Security.Role(RoleName, ShortName, Description, RoleTypeID, OrgPartyID, ProductId, CreatedBy, CreatedDate )
	VALUES('Portfolio Manager','PortfolioManager','Portfolio Manager', 1, NULL, 84, @UserId, GETDATE())	
	INSERT INTO Security.Role(RoleName, ShortName, Description, RoleTypeID, OrgPartyID, ProductId, CreatedBy, CreatedDate )
	VALUES('Property Manager','PropertyManager','Property Manager', 1, NULL, 84, @UserId,GETDATE())	
END
IF NOT EXISTS(SELECT 1 FROM Security.[Right] WHERE ProductId = 84)
BEGIN
	INSERT INTO Security.[Right](RightName, Description, Value, StatusTypeId, VisibilityStatusId, ProductId, TargetProductId, CreatedBy, CreatedDate)
	VALUES('ReadOnly', NULL, 'Read Only Access',13, 9, 84, 84, @UserId, GETDATE())	
END
DECLARE @PortfolioManagerRoleId INT;
DECLARE @PropertyManagerRoleId INT;
DECLARE @RightId INT;
SELECT @PortfolioManagerRoleId = RoleId FROM Security.Role WHERE RoleName = 'Portfolio Manager' AND ProductId = 84
SELECT @PropertyManagerRoleId = RoleId FROM Security.Role WHERE RoleName = 'Property Manager' AND ProductId = 84
SELECT @RightId = RightId FROm Security.[Right] WHERE RightName = 'ReadOnly' AND ProductId = 84 AND TargetProductId = 84
IF NOT EXISTS(SELECT 1 FROM Security.RoleRight WHERE RightId  = @RightId AND RoleId = @PortfolioManagerRoleId)
BEGIN
	INSERT INTO Security.RoleRight(RoleId, RightId, CreatedBy, CreatedDate)
	VALUES(@PortfolioManagerRoleId, @RightId, @UserId, GETDATE())
END
IF NOT EXISTS(SELECT 1 FROM Security.RoleRight WHERE RightId  = @RightId AND RoleId = @PropertyManagerRoleId)
BEGIN
	INSERT INTO Security.RoleRight(RoleId, RightId, CreatedBy, CreatedDate)
	VALUES(@PropertyManagerRoleId, @RightId, @UserId, GETDATE())
END
GO