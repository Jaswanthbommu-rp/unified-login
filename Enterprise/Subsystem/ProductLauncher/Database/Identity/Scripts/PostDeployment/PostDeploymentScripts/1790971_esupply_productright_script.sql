Go
Declare @createdate datetime, @CreatedBy bigint, @RightId int;
select @createdate = GETUTCDATE()
SELECT	@CreatedBy = UserId
FROM	Ident.UserLogin
WHERE	LoginName LIKE 'realpagead@%'


IF NOT EXISTS(SELECT TOP 1 1 FROM security.[Right] where RightName ='AccessESupply')
BEGIN
	INSERT INTO Security.[Right] (RightName,[Description],	[Value],StatusTypeId,VisibilityStatusId,ProductId,TargetProductId,CreatedBy,CreatedDate,PersistRight,IsExcludeRightFromImpersonation)
	VALUES('AccessESupply','Access to eSupply product','Access to eSupply product',13,9,3,3,@CreatedBy,@createdate,0,0)
END

select @RightId = RightID from Security.[Right] where Rightname = 'AccessESupply'

IF NOT EXISTS(SELECT TOP 1 1 FROM security.[RoleRight] where RoleId = 1 AND RightID = @RightId)
BEGIN
  INSERT INTO Security.RoleRight(RoleId, RightId, CreatedBy, CreatedDate)
  VALUES(1,@RightID, @CreatedBy, @createdate)
END

IF NOT EXISTS(SELECT TOP 1 1 FROM Enterprise.ProductRight where ProductId = 96 AND RightShortName = 'AccessESupply')
BEGIN
  INSERT INTO Enterprise.ProductRight(ProductId, RightShortName,DependantProductId)
  VALUES(96,'AccessESupply',NULL)
END

Go