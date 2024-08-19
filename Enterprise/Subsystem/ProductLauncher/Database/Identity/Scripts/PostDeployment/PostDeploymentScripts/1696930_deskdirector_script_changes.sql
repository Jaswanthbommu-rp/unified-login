Go
Declare @createdate datetime, @CreatedBy bigint, @RightId int, @ProductsettingTypeid int;
select @createdate = GETUTCDATE()
SELECT	@CreatedBy = UserId
FROM	Ident.UserLogin
WHERE	LoginName LIKE 'realpagead@%'
IF NOT EXISTS(SELECT TOP 1 1 FROM security.[Right] where RightName ='AccessManagedServices')
BEGIN
	INSERT INTO Security.[Right] (RightName,[Description],	[Value],StatusTypeId,VisibilityStatusId,ProductId,TargetProductId,CreatedBy,CreatedDate,PersistRight,IsExcludeRightFromImpersonation)
	VALUES('AccessManagedServices','Access to Managed Services product','Access to ManagedServices product',13,9,3,3,@CreatedBy,@createdate,0,0)
END
select @RightId = RightID from Security.[Right] where Rightname = 'AccessManagedServices'
IF NOT EXISTS(SELECT TOP 1 1 FROM security.[RoleRight] where RoleId = 1 AND RightID = @RightId)
BEGIN
  INSERT INTO Security.RoleRight(RoleId, RightId, CreatedBy, CreatedDate)
  VALUES(1,@RightID, @CreatedBy, @createdate)
END
IF NOT EXISTS(SELECT TOP 1 1 FROM Enterprise.ProductRight where ProductId = 93 AND RightShortName = 'AccessManagedServices')
BEGIN
  INSERT INTO Enterprise.ProductRight(ProductId, RightShortName,DependantProductId)
  VALUES(93,'AccessManagedServices',NULL)
END

IF NOT EXISTS (SELECT TOP 1 1 FROM Enterprise.ProductSettingType WHERE [Name] = 'isUserExistsInProductCheckRequired')
BEGIN
	INSERT INTO Enterprise.ProductSettingType ([Name], [Description], SensitiveData)
	VALUES ('isUserExistsInProductCheckRequired', 'User check in product required', 0);
	SELECT @ProductsettingTypeid = ProductSettingTypeId FROM Enterprise.ProductSettingType WHERE [Name] = 'isUserExistsInProductCheckRequired'
    exec [Enterprise].[SetProductSetting] 0,93,@ProductsettingTypeid,'1'
END
Go