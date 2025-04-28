--User Story 2252341: Leverage new product setting to enable Contract Management product integration

IF NOT EXISTS (SELECT TOP (1) 1 FROM Enterprise.ProductSettingType WHERE [Name] = 'ProductUsernameDataSharedWithOtherProduct')
BEGIN
	INSERT INTO Enterprise.ProductSettingType ([Name], [Description], SensitiveData) 
	VALUES('ProductUsernameDataSharedWithOtherProduct', 'Product username data shared with other product', 0)
END


GO

IF NOT EXISTS (SELECT TOP (1) 1 FROM Enterprise.ProductSettingType WHERE [Name] = 'PreventEnablingThisProductID')
BEGIN
	INSERT INTO Enterprise.ProductSettingType ([Name], [Description], SensitiveData) 
	VALUES('PreventEnablingThisProductID', 'Prevent enabling from other shared ProductID', 0)
END


GO


DECLARE @UserId bigint, @RightId bigint

SELECT	@UserId = UserId
FROM	Ident.UserLogin
WHERE	LoginName LIKE 'realpagead@%'

IF NOT EXISTS (SELECT TOP (1) 1 FROM security.[Right] where Rightname = 'AccessToContractManagement')
BEGIN
	INSERT INTO security.[Right] (RightName	,[Description],	[Value],	StatusTypeId,	VisibilityStatusId,	ProductId,TargetProductId,CreatedBy,CreatedDate,PersistRight,IsExcludeRightFromImpersonation)
	VALUES('AccessToContractManagement','Access to Contract Management',	'Access to Contract Management',11,10,3,100,@UserId,GETUTCDATE(),0,0)
END


SELECT @RightId = RightID from security.[Right] where Rightname = 'AccessToContractManagement'

IF NOT EXISTS (SELECT TOP (1) 1 FROM security.RoleRight where RightId = @RightId AND RoleId = 1)
BEGIN
	INSERT INTO [Security].RoleRight(RoleId,RightId,CreatedBy,CreatedDate)
	VALUES(1, @RightId, @UserId,GETUTCDATE())
END

GO